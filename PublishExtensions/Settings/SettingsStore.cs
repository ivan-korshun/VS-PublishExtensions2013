using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using PHZH.PublishExtensions.Details;
using PHZH.PublishExtensions.Xml;

namespace PHZH.PublishExtensions.Settings
{
    internal static class SettingsStore
    {
        // constants
        public const string SettingsFolder = "Properties";
        public const string SettingsFileName = "PublishSettings.xml";

        // variables
        private static Dictionary<string, ProjectSettings> settings = new Dictionary<string, ProjectSettings>();

        public static void UpdateCache()
        {
            settings.Clear();

            if (Globals.Solution != null)
            {
                foreach (Project project in Globals.Solution.Projects)
                    UpdateCache(project);
            }
        }

        private static void UpdateCache(Project project)
        {
            if (!project.IsSupported())
                return;

            // check if the settings file exists
            string filePath = project.GetSettingsFilePath();
            try
            {
                if (File.Exists(filePath))
                {
                    string projectId = project.GetIdentifier();
                    ProjectSettings projectSettings = ProjectSettings.Load(filePath);
                    settings.Remove(projectId);
                    settings.Add(projectId, projectSettings);

                    Logger.Log(" -> Settings file loaded for project '{0}'", project.Name);
                }
                else
                {
                    Logger.Debug(" -> Project '{0}': No settings file exists for publishing", project.Name);
                }
            }
            catch (XmlSerializationException ex)
            {
                Logger.Log(Logger.Divider);
                Logger.Log(ex, "Error in publish settings file '{0}'", Path.Combine(SettingsStore.SettingsFolder, SettingsStore.SettingsFileName));
                Logger.Log(Logger.Divider);

                MessageBox.Show(
                    "The publish settings file contains errors, please correct the errors manually and save the file." +
                    Environment.NewLine + Environment.NewLine + 
                    "See the output window for more information.",
                    "Invalid Settings File",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Error in publish settings file '{0}'", Path.Combine(SettingsStore.SettingsFolder, SettingsStore.SettingsFileName));
            }
        }

        public static ProjectSettings Get(Project project)
        {
            if (!project.IsSupported())
                return null;
            
            string key = project.GetIdentifier();
            return settings.ValueOrNull(key);
        }

        public static ProjectSettings Create(Project project)
        {
            if (!project.IsSupported())
                return null;

            string key = project.GetIdentifier();
            if (!settings.ContainsKey(key))
            {
                string fullPath = project.GetSettingsFilePath();
                ProjectSettings projectSettings = ProjectSettings.Load(fullPath);
                settings.Add(key, projectSettings);
            }
            return settings[key];
        }

        public static void DocumentSaved(Document document)
        {
            ProjectItem item = document.ProjectItem;
            if (item == null || item.ContainingProject == null)
                return;

            // check file name
            if (!SettingsFileName.EqualsIgnoreCase(document.Name))
                return;

            // check full path
            string settingsPath = item.ContainingProject.GetSettingsFilePath();
            if (document.FullName.EqualsIgnoreCase(settingsPath))
                UpdateCache(item.ContainingProject);
        }

        public static void ItemRemoved(ProjectItem item)
        {
            ProjectSettings s = item.GetProjectSettings();
            if (s != null && s.ItemRemoved(item))
                s.Save();
        }

        public static void ItemRenamed(ProjectItem item, string oldFullPath)
        {
            ProjectSettings s = item.GetProjectSettings();
            if (s != null && s.ItemRenamed(item, oldFullPath))
                s.Save();
        }
    }
}
