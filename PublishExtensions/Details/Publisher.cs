using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.Details
{
    internal class Publisher
    {
        private class Statistics
        {
            public Statistics()
            {
                Clear();
            }

            public override string ToString()
            {
                return "{0} published, {1} up-to-date, {2} skipped, {3} ignored, {4} failed".TryFormat(
                    AddCount + UpdateCount, UpToDateCount, SkipCount, IgnoreCount, ErrorCount);
            }

            public void Clear()
            {
                AddCount = 0;
                UpdateCount = 0;
                ErrorCount = 0;
                UpToDateCount = 0;
                SkipCount = 0;
                IgnoreCount = 0;
            }

            public void Update(PublishStatus status)
            {
                switch (status)
                {
                    case PublishStatus.Created:
                        AddCount++;
                        break;
                    case PublishStatus.Updated:
                        UpdateCount++;
                        break;
                    case PublishStatus.Error:
                        ErrorCount++;
                        break;
                    case PublishStatus.Unmodified:
                        UpToDateCount++;
                        break;
                    case PublishStatus.TargetNewer:
                        SkipCount++;
                        break;
                    case PublishStatus.Ignored:
                        IgnoreCount++;
                        break;
                    case PublishStatus.Folder:
                    case PublishStatus.Unknown:
                    default:
                        break;
                };
            }

            public int AddCount { get; private set; }

            public int UpdateCount { get; private set; }

            public int UpToDateCount { get; private set; }

            public int SkipCount { get; private set; }

            public int IgnoreCount { get; private set; }

            public int ErrorCount { get; private set; }
        }

        private class PublishOutput : IDisposable
        {
            private Statistics stats = null;

            public PublishOutput(string publishLocation, Statistics statistics)
            {
                this.stats = statistics;

                Logger.Log("");
                Logger.Log("--------- Publish started ({0}) ---------", DateTime.Now.ToString());
                Logger.Log("=> Publish Location: {0}", publishLocation);
            }

            public void Dispose()
            {
                Logger.Log("========= Publish done: {0} =========", stats.ToString());
            }
        }

        private static readonly object syncRoot = new object();

        private ProjectSettings project = null;
        private List<string> publishDirectories = null;
        private Regex excludeRegex = null;
        private string _projectConfig;

        public static bool IsPublishing { get; private set; }

        /// <summary>
        /// Determines whether the current user has access to the specified directory and can write to it.
        /// </summary>
        /// <param name="publishLocation">The publish location.</param>
        /// <returns>true, if the user can write to the specified directory; otherwise, false.</returns>
        public static bool CheckAccess(string publishLocation)
        {
            return CheckDirAccess(GetPublishDirectories(publishLocation));
        }

        /// <summary>
        /// Checks whether a publish is already running.
        /// </summary>
        /// <returns>true, if a publish is running; otherwise, false.</returns>
        public static bool CheckIsPublishing()
        {
            if (!IsPublishing)
                return false;

            MessageBoxResult result = MessageBox.Show(
                "A publish is already running. Please wait until it is finished.",
                "Already publishing",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Publisher"/> class.
        /// </summary>
        /// <param name="settings">The settings to use for publishing.</param>
        public Publisher(ProjectSettings settings)
        {
            settings.ThrowIfNull("settings");

            // set variables
            project = settings;
            publishDirectories = GetPublishDirectories(project.GetPublishLocation());

            // build exclude regex
            HashSet<string> filterParts = project.IgnoreFilter.ToHashSet(';');
            if (filterParts.Count > 0)
            {
                string filter = "^(";
                foreach (string part in filterParts)
                    filter += Regex.Escape(part).Replace(@"\*", ".*") + "|";
                filter = filter.TrimEnd('|') + ")$";
                excludeRegex = new Regex(filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public Publisher(ProjectSettings settings, string projectConfig) : this(settings)
        {
            _projectConfig = projectConfig;
        }

        public void Clean(ProjectItem item)
        {
            if (item == null)
                return;

            List<string> allPaths = new List<string>();
            this.FillAllPaths(item, allPaths);

            this.StartPublishTask<IEnumerable<string>>(this.DoClean, allPaths);
        }

        /// <summary>
        /// Publishes the specified selection.
        /// </summary>
        /// <param name="selection">The selection to publish.</param>
        public void Publish(SelectedItems selectedItems)
        {
            selectedItems.ThrowIfNull("selectedItems");

            // check for write access
            // copy items
            List<ProjectItem> items = new List<ProjectItem>();
            foreach (SelectedItem item in selectedItems)
            {
                // only process project items
                if (item.ProjectItem is ProjectItem)
                {
                    items.Add(item.ProjectItem);
                }
                else if (item.Project is Project)
                {
                    foreach (ProjectItem projectItem in item.Project.ProjectItems)
                        items.Add(projectItem);
                }
            }

            // start publishing
            this.StartPublishTask<List<ProjectItem>>(this.DoPublish, items);
        }

        /// <summary>
        /// Publishes the specified items.
        /// </summary>
        /// <param name="projectItems">The items to publish.</param>
        public void Publish(params ProjectItem[] projectItems)
        {
            projectItems.ThrowIfNull("projectItems");

            // start publishing
            this.StartPublishTask<List<ProjectItem>>(this.DoPublish, projectItems.ToList());
        }

        public void Rename(ProjectItem item, string oldFullPath)
        {
            string oldRelativePath = oldFullPath.Replace(item.ContainingProject.GetDirectory(), string.Empty);

            List<string> allPaths = new List<string>();
            this.FillAllPaths(item, allPaths);

            string newRelativePath = allPaths.Last();

            this.StartPublishTask<object>(f =>
            {
                this.DoClean(allPaths.Select(p => p.Replace(newRelativePath, oldRelativePath)));
                this.DoPublish(new List<ProjectItem> { item });
            }, null);
        }

        private void StartPublishTask<T>(Action<T> action, T arg)
        {
            if (!CheckDirAccess(publishDirectories))
                return;

            // run in background
            Task.Factory.StartNew(() =>
            {
                lock (syncRoot)
                {
                    IsPublishing = true;

                    try
                    {
                        action(arg);
                    }
                    finally
                    {
                        IsPublishing = false;
                    }
                }
            });
        }

        private void DoPublish(List<ProjectItem> items)
        {
            this.publishDirectories.ForEach(pd => this.DoPublish(items, pd));
        }

        private void DoPublish(List<ProjectItem> items, string publishDir)
        {
            var stats = new Statistics();
            var processedItems = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            using (new PublishOutput(publishDir, stats))
            {
                // now publish each selected item
                foreach (ProjectItem item in items)
                {
                    // only process project items
                    if (item != null)
                        PublishItem(item, publishDir, processedItems, stats);
                }

                PublishAssemblies(publishDir, processedItems, stats);
            }
        }

        private void PublishAssemblies(string publishDir, HashSet<string> processedItems, Statistics stats)
        {
            string projectDir = Path.GetDirectoryName(Path.GetDirectoryName(project.FilePath));

            var files = project.Assemblies
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim().Replace("$(ConfigurationName)", _projectConfig ?? "$(ConfigurationName)"))
                .ToList();

            foreach (string sourceFile in files)
            {
                string sourceFilePath = Path.Combine(projectDir, sourceFile);
                string targetFilePath = Path.Combine(publishDir, project.AssemblyPublishFolder, Path.GetFileName(sourceFile));
                PublishFile(sourceFilePath, targetFilePath, processedItems, stats);
            }
        }

        private void PublishFile(string sourceFile, string targetFile, HashSet<string> processedItems, Statistics stats)
        {
            if (!File.Exists(sourceFile))
            {
                Logger.Log("> NOT FOUND {0}", sourceFile);
                return;
            }

            PublishStatus status;
            string message;
            CopyFile(sourceFile, targetFile, out status, out message);

            UpdateStatistics(stats, status, sourceFile, message);
        }

        /// <summary>
        /// Publishes the specified item.
        /// </summary>
        /// <param name="item">The item to publish.</param>
        /// <param name="publishDir">The publish directory.</param>
        /// <param name="level">The level for logging.</param>
        private void PublishItem(ProjectItem item, string publishDir, HashSet<string> processedItems, Statistics stats)
        {
            // item must be a physical file or folder to be published
            if (item.Kind != Constants.vsProjectItemKindPhysicalFile && item.Kind != Constants.vsProjectItemKindPhysicalFolder)
                return;

            bool hasSubItems = item.ProjectItems.Count > 0;
            string relativePath = item.GetRelativePath();

            // already processed?
            if (processedItems.Contains(relativePath))
                return;

            // get path to copy the file to
            PublishStatus status;
            string message;
            bool ignoreFile;
            string targetPath = GetMappedPath(relativePath, out ignoreFile, out message);
            if (ignoreFile)
            {
                status = PublishStatus.Ignored;

                // process nested items if it's a file
                hasSubItems = item.Kind == Constants.vsProjectItemKindPhysicalFile;
            }
            else if (!relativePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                // now that we are going to copy the file, save it first if needed
                if (item.IsOpen && item.Document != null && !item.Document.Saved)
                    item.Document.Save();

                // for now only files are copied to avoid empty folders
                CopyFile(item.GetFullPath(), publishDir + targetPath, out status, out message);
            }
            else
            {
                // folder
                status = PublishStatus.Folder;
            }

            UpdateStatistics(stats, status, relativePath, message);

            // add to processed items
            processedItems.Add(relativePath);

            // has subitems? publish them as well
            if (hasSubItems)
            {
                foreach (ProjectItem subItem in item.ProjectItems)
                    PublishItem(subItem, publishDir, processedItems, stats);
            }
        }

        private void UpdateStatistics(Statistics stats, PublishStatus status, string path, string message)
        {
            // log
            if (!status.IsIn(PublishStatus.Folder, PublishStatus.Unknown, PublishStatus.Unmodified))
            {
                if (message != null)
                    Logger.Log("> {0,-7}  {1} ({2})", status.GetDescription(), path, message);
                else
                    Logger.Log("> {0,-7}  {1}", status.GetDescription(), path);
            }

            // update statistics
            stats.Update(status);
        }

        /// <summary>
        /// Gets the mapped relative path for the specified relative path of the project.
        /// </summary>
        /// <param name="relativePath">The relative path to map.</param>
        /// <param name="ignoreFile">true, if the file should be ignored; otherwise, false.</param>
        /// <param name="ignoreReason">The ignore reason, if any.</param>
        /// <returns>The mapped relative path or a null reference if the file should be ignored.</returns>
        private string GetMappedPath(string relativePath, out bool ignoreFile, out string ignoreReason)
        {
            ignoreFile = false;
            ignoreReason = null;

            // split path into it's parts to check filter and ignore flags
            string targetPath = string.Empty;
            string realPath = string.Empty;
            foreach (string part in SplitIntoParts(relativePath))
            {
                // part matches regex?
                if (excludeRegex != null && excludeRegex.IsMatch(part))
                {
                    ignoreFile = true;
                    ignoreReason = "Ignored by filter";
                    return null;
                }

                // get settings
                realPath += part;
                string partPath = targetPath + part;
                ItemSettings partSettings = project.GetItemSettings(realPath);

                // ignored?
                if (partSettings.Ignore)
                {
                    ignoreFile = true;
                    if (realPath == relativePath)
                        ignoreReason = "Ignored by configuration";
                    else
                        ignoreReason = "Ignored by configuration of parent '{0}'".TryFormat(realPath);
                    return null;
                }

                // apply mapping to path if enabled
                string mapping = project.MappingEnabled ? partSettings.Mapping : null;
                if (mapping != null && part.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    mapping = mapping.EnsureEndingDirectorySeparator();
                targetPath = targetPath + mapping.OrDefault(part);
            }

            return targetPath;
        }

        /// <summary>
        /// Copies the file from the specified source path to the specified target path.
        /// </summary>
        /// <param name="sourcePath">The absolute source path.</param>
        /// <param name="targetPath">The relative target path.</param>
        /// <param name="status">The publish status.</param>
        /// <param name="copyMessage">The copy message, if any.</param>
        /// <returns>true, if the file was copied successfully; otherwise, false.</returns>
        private bool CopyFile(string sourcePath, string targetPath, out PublishStatus status, out string copyMessage)
        {
            copyMessage = null;
            string targetFolder = targetPath.SubstringBeforeLast(Path.DirectorySeparatorChar.ToString());

            // create folder
            try
            {
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                // check file date and size to avoid unnecessary copying!
                FileInfo sourceInfo = new FileInfo(sourcePath);
                FileInfo targetInfo = new FileInfo(targetPath);

                // target file exists?
                if (targetInfo.Exists)
                {
                    // same write time and same size?
                    if (sourceInfo.LastWriteTimeUtc == targetInfo.LastWriteTimeUtc &&
                        sourceInfo.Length == targetInfo.Length)
                    {
                        status = PublishStatus.Unmodified;
                        copyMessage = null;
                        return false;
                    }

                    // target newer than source?
                    if (targetInfo.LastWriteTimeUtc >= sourceInfo.LastWriteTimeUtc)
                    {
                        status = PublishStatus.TargetNewer;
                        copyMessage = "Target file is newer";
                        return false;
                    }

                    status = PublishStatus.Updated;
                }
                else
                {
                    status = PublishStatus.Created;
                }

                // now we copy the file
                File.Copy(sourcePath, targetPath, true);
                return true;
            }
            catch (Exception ex)
            {
                status = PublishStatus.Error;
                copyMessage = ex.ToString();
                return false;
            }
        }

        private void DoClean(IEnumerable<string> paths)
        {
            this.publishDirectories.ForEach(pd => this.DoClean(paths, pd));
        }

        private void DoClean(IEnumerable<string> paths, string publishDir)
        {
            foreach (string path in paths)
            {
                string message;
                bool ignoreFile;
                string targetPath = publishDir + GetMappedPath(path, out ignoreFile, out message);

                if (ignoreFile)
                    continue;

                if (Directory.Exists(targetPath) && !Directory.EnumerateFileSystemEntries(targetPath).Any())
                    Directory.Delete(targetPath, true);

                if (File.Exists(targetPath))
                    File.Delete(targetPath);

                Logger.Log("Clean: {0}", path);
            }
        }

        /// <summary>
        /// Splits the specified relative path into its parts.
        /// </summary>
        /// <param name="relativePath">The relative path to split.</param>
        /// <returns>A list containing all parts of the path.</returns>
        private List<string> SplitIntoParts(string relativePath)
        {
            var parts = new List<string>();

            int index = 0;
            int start = 0;
            while (start < relativePath.Length && index > -1)
            {
                index = relativePath.IndexOf(Path.DirectorySeparatorChar, start);
                if (index == -1)
                    parts.Add(relativePath.Substring(start));
                else
                    parts.Add(relativePath.Substring(start, index - start + 1));

                start = index + 1;
            }

            return parts;
        }

        private void FillAllPaths(ProjectItem item, List<string> allPaths)
        {
            foreach (ProjectItem child in item.ProjectItems)
            {
                this.FillAllPaths(child, allPaths);
            }

            allPaths.Add(item.GetRelativePath());
        }

        private static List<string> GetPublishDirectories(string publishLocation)
        {
            return publishLocation
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().EnsureEndingDirectorySeparator())
                .ToList();
        }

        public static bool CheckDirAccess(List<string> directories)
        {
            return directories.All(CheckDirAccess);
        }

        private static bool CheckDirAccess(string directory)
        {
            try
            {
                var ac = Directory.GetAccessControl(directory);
                foreach (System.Security.AccessControl.FileSystemAccessRule rule in
                    ac.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    if (rule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow)
                        return true;
                }
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The specified publish location is not a valid directory." +
                    Environment.NewLine + Environment.NewLine +
                    "Location: " + directory,
                    "Invalid publish location",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBoxResult result = MessageBox.Show(
                    "You do not have access to the specified publish location. Please make sure you have write access to that directory." +
                    Environment.NewLine + Environment.NewLine +
                    "Location: " + directory,
                    "Invalid publish location",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error checking access for directory '{0}'", directory);
                return false;
            }
        }
    }
}
