using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using PHZH.PublishExtensions;
using PHZH.PublishExtensions.Settings;

namespace EnvDTE
{
    internal static class EnvDTEExtensions
    {
        public static string GetFullPath(this Project project)
        {
            project.ThrowIfNull("project");

            Property prop = project.Properties.Item("FullPath");
            if (prop != null)
                return prop.Value.ToString();
            else
                return null;
        }

        public static string GetFullPath(this ProjectItem item)
        {
            item.ThrowIfNull("item");

            Property prop = item.Properties.Item("FullPath");
            if (prop != null)
                return prop.Value.ToString();
            else
                return null;
        }

        public static string GetRelativePath(this ProjectItem item)
        {
            item.ThrowIfNull("item");

            string fullPath = item.GetFullPath();
            if (fullPath != null)
                return fullPath.Replace(item.ContainingProject.GetDirectory(), "");
            else
                return null;
        }

        public static string GetDirectory(this Project project)
        {
            project.ThrowIfNull("project");

            string path = project.GetFullPath();
            if (path != null)
                return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
            else
                return null;
        }

        public static bool IsSupported(this Project project)
        {
            if (project == null)
                return false;

            return !project.IsSolutionFolder();
        }

        public static string GetIdentifier(this Project project)
        {
            project.ThrowIfNull("project");
            return project.UniqueName;
        }

        public static bool IsSolutionFolder(this Project project)
        {
            project.ThrowIfNull("project");
            return project.FullName.IsNullOrEmpty();
        }

        public static ProjectSettings GetProjectSettings(this ProjectItem item)
        {
            return SettingsStore.Get(item.ContainingProject);
        }

        public static Project GetProject(this SelectedItem item)
        {
            if (item == null)
                return null;

            if (item.Project is Project)
                return item.Project;
            else if (item.ProjectItem is ProjectItem)
                return item.ProjectItem.ContainingProject;
            else
                return null;
        }

        public static string GetSettingsFilePath(this Project project)
        {
            if (!project.IsSupported())
                return null;

            return Path.Combine(
                project.GetDirectory(), 
                SettingsStore.SettingsFolder, 
                SettingsStore.SettingsFileName);
        }
    }
}
