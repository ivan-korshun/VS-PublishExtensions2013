using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using PHZH.PublishExtensions.UI;
using PHZH.PublishExtensions.Details;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.MenuItems
{
    internal class PublishMenu
    {
        private DTE2 dte;
        private OleMenuCommandService mcs;

        public PublishMenu(DTE2 dte, OleMenuCommandService mcs)
        {
            this.dte = dte;
            this.mcs = mcs;
        }

        public void SetupCommands()
        {
            AddCommand(CommandId.ConfigureProject, ConfigureProject);
            AddCommand(CommandId.PublishFiles, PublishFiles);
            AddCommand(CommandId.EditMapping, EditMapping);
            AddCommand(CommandId.IncludeFiles, IncludeFiles);
            AddCommand(CommandId.IgnoreFiles, IgnoreFiles);
            AddCommand(CommandId.PublishActiveFile, PublishActiveFile);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            bool enabled;
            bool visible;

            // NOTE: this event is never fired, when a project is selected with project items!
            OleMenuCommand cmd = sender as OleMenuCommand;
            CommandId cmdId = (CommandId)cmd.CommandID.ID;
            SelectedItems selectedItems = dte.SelectedItems;

            // various states to set
            bool isSelectionSupported = false;
            bool isProjectSelected = false;
            Project project = null;
            ProjectSettings settings = null;

            // one item selected?
            if (selectedItems.Count == 1)
            {
                project = selectedItems.Item(1).GetProject();
                
                // supported project?
                if (project.IsSupported())
                {
                    isSelectionSupported = true;
                    isProjectSelected = selectedItems.Item(1).Project is Project;
                    settings = SettingsStore.Get(project);
                }
            }
            else if (selectedItems.Count > 1)
            {
                project = null;

                // selection not supported over multiple projects
                foreach (SelectedItem item in selectedItems)
                {
                    Project nextProject = item.GetProject();
                    if (nextProject == null)
                    {
                        project = null;
                        break;
                    }
                    else if (project == null)
                    {
                        project = nextProject;
                    }
                    else if (nextProject.UniqueName != project.UniqueName)
                    {
                        // not the same project
                        project = null;
                        break;
                    }   
                }

                // project set?
                if (project.IsSupported())
                {
                    isSelectionSupported = true;
                    isProjectSelected = false;
                    settings = SettingsStore.Get(project);
                }
            }

            // only process if selection is supported
            if (isSelectionSupported)
            {
                // no settings yet?
                if (settings == null)
                {
                    // only configure is visible and enabled
                    enabled = visible = cmdId == CommandId.ConfigureProject;
                }
                else
                {
                    visible = true;
                    switch (cmdId)
                    {
                        case CommandId.PublishFiles:
                        case CommandId.ConfigureProject:
                            // always enabled
                            enabled = true;
                            break;

                        case CommandId.EditMapping:
                            // disable when multiple selection
                            // disabled on project
                            visible = settings.MappingEnabled;
                            enabled = selectedItems.Count == 1 && !isProjectSelected;
                            break;

                        case CommandId.IncludeFiles:
                            // disabled on project
                            // hidden if single selection and file is already included
                            if (selectedItems.Count == 1)
                                visible = IsItemIgnored(selectedItems.Item(1));
                            enabled = !isProjectSelected;
                            break;

                        case CommandId.IgnoreFiles:
                            // disabled on project
                            // hidden if single selection and file is already ignored
                            if (selectedItems.Count == 1)
                                visible = !IsItemIgnored(selectedItems.Item(1));
                            enabled = !isProjectSelected;
                            break;

                        case CommandId.PublishActiveFile:
                            visible = Globals.ActiveDocument != null;
                            enabled = true;
                            break;

                        default:
                            enabled = false;
                            break;
                    }

                    // adjust enabled if publishing
                    enabled &= !Publisher.IsPublishing;
                }
            }
            else
            {
                enabled = false;
                visible = false;
            }
            
            cmd.Enabled = enabled;
            cmd.Visible = visible;
        }

        /// <summary>
        /// Configures the project for publishing.
        /// </summary>
        private void ConfigureProject()
        {
            Project project = GetProjectForSelection();
            if (project != null)
                ConfigureProject(project, SettingsStore.Create(project));
        }

        /// <summary>
        /// Configures the project for publishing.
        /// </summary>
        /// <param name="project">The project to configure.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        /// true, if the configuration was modified; otherwise, false.
        /// </returns>
        private bool ConfigureProject(Project project, ProjectSettings settings)
        {
            if (settings != null)
            {
                var window = new ProjectSettingsWindow(project.Name, settings);

                // show dialog and modify settings
                bool? saveSettings = window.ShowDialog(Globals.GetMainWindowHandle());
                if (saveSettings.Value)
                {
                    settings.Save();

                    // file does NOT exist?
                    if (!settings.Exists())
                    {
                        // add to project
                        string projectDir = project.GetDirectory();
                        string filePath = settings.FilePath;
                        if (filePath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
                        {
                            ProjectItem item = project.ProjectItems.AddFromFile(filePath);
                            Logger.Log(" -> Settings file added to project '{0}'", project.Name);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Publishes the selected files.
        /// </summary>
        private void PublishFiles()
        {
            Project project = GetProjectForSelection();
            ProjectSettings settings = SettingsStore.Get(project);
            if (settings == null)
                return;

            // check if already publishing
            if (Publisher.CheckIsPublishing())
                return;

            // check config
            if (!CheckConfiguration(project, settings))
                return;

            Publisher publisher = new Publisher(settings);
            publisher.Publish(dte.SelectedItems);
        }

        /// <summary>
        /// Publishes the file that is active in the editor.
        /// </summary>
        private void PublishActiveFile()
        {
            Document doc = Globals.ActiveDocument;
            if (doc == null)
                return;

            // does the document have a project item assigned?
            if (doc.ProjectItem is ProjectItem)
            {
                Project project = doc.ProjectItem.ContainingProject;
                ProjectSettings settings = SettingsStore.Get(project);
                if (settings == null)
                    return;

                // check if already publishing
                if (Publisher.CheckIsPublishing())
                    return;

                // check config
                if (!CheckConfiguration(project, settings))
                    return;

                // ask if we should really publish
                MessageBoxResult result = MessageBox.Show(
                    "Do you want to publish the following file?" +
                    Environment.NewLine + Environment.NewLine + 
                    "File: " + doc.ProjectItem.GetRelativePath() + Environment.NewLine + 
                    "Location: " + settings.GetPublishLocation(),
                    "Confirm Publish",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (result != MessageBoxResult.Yes)
                    return;

                Publisher publisher = new Publisher(settings);
                publisher.Publish(doc.ProjectItem);
            }
        }

        /// <summary>
        /// Checks the configuration for the specified project.
        /// </summary>
        /// <param name="project">The project to check.</param>
        /// <param name="settings">The settings of the project.</param>
        /// <returns>true, if the configuration is valid; otherwise, false.</returns>
        private bool CheckConfiguration(Project project, ProjectSettings settings)
        {
            // check if configuration is valid
            string targetPath = settings.GetPublishLocation();
            if (targetPath.IsNullOrWhiteSpace())
            {
                MessageBoxResult result = MessageBox.Show(
                    "No publish location is specified! Do you want to specify it now?",
                    "Publish Location missing",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Yes);

                if (result == MessageBoxResult.No)
                    return false;

                // show the configuration dialog
                if (!ConfigureProject(project, settings))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Edits the mapping for a certain project item.
        /// </summary>
        private void EditMapping()
        {
            Project project = GetProjectForSelection();
            ProjectSettings settings = SettingsStore.Get(project);
            if (settings == null)
                return;

            // due the rules above, there can only be one item selected
            ProjectItem item = dte.SelectedItems.Item(1).ProjectItem;
            string relativePath = item.GetRelativePath();
            if (relativePath != null)
            {
                ItemSettings itemSettings = settings.GetItemSettings(relativePath);
                var window = new MappingWindow(settings, itemSettings);

                // show dialog and modify settings
                bool? saveSettings = window.ShowDialog(Globals.GetMainWindowHandle());
                if (saveSettings.Value)
                {
                    settings.AddItemSettings(itemSettings);                    
                    settings.Save();
                }
            }
        }

        /// <summary>
        /// Includes the selected files.
        /// </summary>
        private void IncludeFiles()
        {
            IgnoreFiles(false);
        }

        /// <summary>
        /// Ignores the selected files.
        /// </summary>
        private void IgnoreFiles()
        {
            IgnoreFiles(true);
        }

        /// <summary>
        /// Includes or ignores the selected files.
        /// </summary>
        /// <param name="ignore">true, to ignore the files; false, to include them.</param>
        private void IgnoreFiles(bool ignore)
        {
            Project project = GetProjectForSelection();
            ProjectSettings settings = SettingsStore.Get(project);
            if (settings == null)
                return;

            bool saveSettings = false;
                
            // set each item in the selection
            foreach (SelectedItem item in dte.SelectedItems)
            {
                // only process project items
                if (item.ProjectItem is ProjectItem)
                {
                    string path = item.ProjectItem.GetRelativePath();
                    settings.IgnoreItem(path, ignore);
                    saveSettings = true;
                }
            }

            // save settings?
            if (saveSettings)
                settings.Save();
        }

        /// <summary>
        /// Determines whether the specified item is ignored when publishing.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true, if the item is ignored on publish; otherwise, false.</returns>
        private bool IsItemIgnored(SelectedItem item)
        {
            if (item == null || !(item.ProjectItem is ProjectItem))
                return false;

            // get settings
            ProjectSettings settings = SettingsStore.Get(item.GetProject());
            if (settings == null)
                return false;
            
            // only process project items
            string path = item.ProjectItem.GetRelativePath();
            return settings.IsItemIgnored(path);
        }

        /// <summary>
        /// Determines whether the selected files should be included when publishing.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>true, to include the selected files; false, to ignore them.</returns>
        private bool ShouldIncludeSelectedFiles(Project project)
        {
            // get settings
            ProjectSettings settings = SettingsStore.Get(project);
            if (settings == null)
                return false;
            
            // no selection?
            SelectedItems selectedItems = dte.SelectedItems;
            if (selectedItems.Count < 1)
                return false;

            // check each item in the selection
            foreach (SelectedItem item in selectedItems)
            {
            }
            
            return true;
        }

        /// <summary>
        /// Adds the command with the specified id and callback.
        /// </summary>
        /// <param name="cmdId">The command unique identifier.</param>
        /// <param name="cmdCallback">The command callback.</param>
        private void AddCommand(CommandId cmdId, Action cmdCallback)
        {
            var cmd = new CommandID(Guids.PublishCmdSet, (int)cmdId);
            var menuCmd = new OleMenuCommand((s, e) => cmdCallback(), cmd);
            menuCmd.BeforeQueryStatus += BeforeQueryStatus;
            mcs.AddCommand(menuCmd);
        }

        /// <summary>
        /// Gets the project for current selection.
        /// </summary>
        /// <returns>The project for the selection.</returns>
        private Project GetProjectForSelection()
        {
            if (dte.SelectedItems.Count < 1)
                return null;

            Project project = dte.SelectedItems.Item(1).GetProject();
            return project.IsSupported() ? project : null;
        }
    }
}
