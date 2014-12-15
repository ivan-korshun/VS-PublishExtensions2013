using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using PHZH.PublishExtensions;
using PHZH.PublishExtensions.Settings;

namespace Microsoft.VisualStudio.Shell.Interop
{
    internal static class VSIPExtensions
    {
        public static string GetFullPath(this IVsProject project)
        {
            project.ThrowIfNull("project");

            string fullPath;
            if (project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath) == VSConstants.S_OK)
                return fullPath;
            else
                return null;
        }

        /*public static string GetFullPath(this ProjectItem item)
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
        }*/

        public static string GetDirectory(this IVsProject project)
        {
            project.ThrowIfNull("project");

            string path = project.GetFullPath();
            if (path != null)
                return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
            else
                return null;
        }

        /*public static bool IsSupported(this Project project)
        {
            if (project == null)
                return false;

            return !project.IsSolutionFolder();
        }*/

        public static string GetIdentifier(this IVsProject project)
        {
            project.ThrowIfNull("project");

            IVsSolution3 solution = Globals.GetService<IVsSolution3>(typeof(SVsSolution));
            string name;
            if (solution.GetUniqueUINameOfProject((IVsHierarchy)project, out name) == VSConstants.S_OK)
                return name;
            else
                return null;
        }

        /*public static bool IsSolutionFolder(this Project project)
        {
            project.ThrowIfNull("project");
            return project.FullName.IsNullOrEmpty();
        }

        public static ProjectSettings GetProjectSettings(this ProjectItem item)
        {
            return SettingsStore.Get(item.ContainingProject);
        }*/
    }
}
