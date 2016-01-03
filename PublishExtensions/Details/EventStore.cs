using System;
using System.Linq;
using EnvDTE;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.Details
{
    internal static class EventStore
    {
        public static void OnBuildProjConfigDone(string projectName, string projectConfig, string platform, string solutionConfig, bool success)
        {
            foreach (Project project in Globals.Solution.Projects)
            {
                if (!project.FullName.EndsWith(projectName, StringComparison.OrdinalIgnoreCase) || !project.IsSupported())
                {
                    continue;
                }

                ProjectSettings settings = GetSettings(project);
                if (settings != null)
                {
                    var items = project.ProjectItems.Cast<ProjectItem>().Where(i => i.Name == "bin").ToArray();
                    new Publisher(settings).Publish(items);
                }

                return;
            }
        }

        public static void DocumentSaved(Document document)
        {
            ProjectItem item = document.ProjectItem;
            Publisher publisher = GetPublisher(item);
            publisher?.Publish(item);
        }

        public static void ItemAdded(ProjectItem item)
        {
            Publisher publisher = GetPublisher(item);
            publisher?.Publish(item);
        }

        public static void ItemRemoved(ProjectItem item)
        {
            Publisher publisher = GetPublisher(item);
            publisher?.Clean(item);
        }

        public static void ItemRenamed(ProjectItem item, string oldFullPath)
        {
            Publisher publisher = GetPublisher(item);
            publisher?.Rename(item, oldFullPath);
        }

        private static Publisher GetPublisher(ProjectItem item)
        {
            if (item == null || item.ContainingProject == null)
                return null;

            ProjectSettings settings = GetSettings(item.ContainingProject);
            if (settings == null)
                return null;

            return new Publisher(settings);
        }

        private static ProjectSettings GetSettings(Project project)
        {
            ProjectSettings settings = SettingsStore.Get(project);
            return (settings != null && !settings.GetPublishLocation().IsNullOrWhiteSpace()) ? settings : null;
        }
    }
}