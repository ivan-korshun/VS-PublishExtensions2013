using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using PHZH.PublishExtensions.Details;

namespace PHZH.PublishExtensions
{
    internal static class Globals
    {
        public static void Initialize(PublishExtensionsPackage package, DTE2 dte)
        {
            Package = package;
            DTE = dte;
        }

        /// <summary>
        /// Gets the DTE instance.
        /// </summary>
        public static DTE2 DTE { get; private set; }

        /// <summary>
        /// Gets the package instance.
        /// </summary>
        public static PublishExtensionsPackage Package { get; private set; }

        /// <summary>
        /// Gets the current solution.
        /// </summary>
        public static Solution Solution 
        { 
            get { return DTE.Solution; } 
        }

        /// <summary>
        /// Gets the active document.
        /// </summary>
        public static Document ActiveDocument
        {
            get { return DTE.ActiveDocument; }
        }

        /// <summary>
        /// Gets the main window handle.
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetMainWindowHandle()
        {
            return new IntPtr(DTE.MainWindow.HWnd);
        }

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to return.</typeparam>
        /// <returns>A service of the specified type.</returns>
        public static T GetService<T>()
            where T : class
        {
            return GetService<T>(typeof(T));
        }

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to return.</typeparam>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>A service of the specified type.</returns>
        public static T GetService<T>(Type serviceType)
            where T : class
        {
            return Package.GetService<T>(serviceType);
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        public static string UserName
        {
            get { return Environment.UserName; }
        }

        /// <summary>
        /// Finds the project item with the specified path.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="fullPath">The full path.</param>
        /// <returns>
        /// The project item with the specified path or a null reference if no item was found.
        /// </returns>
        public static ProjectItem FindProjectItem(string projectPath, string fullPath)
        {
            // get the project
            Project project = FindProject(projectPath);
            if (project == null)
                return null;
            
            // find the item
            string relativePath = fullPath.Replace(project.GetDirectory(), "");

            ProjectItem item = null;
            ProjectItems items = project.ProjectItems;
            string[] parts = relativePath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                try
                {
                    item = items.Item(part);
                    items = item.ProjectItems;
                }
                catch
                {
                    item = null;
                    break;
                }
            }

            return item;
        }

        /// <summary>
        /// Finds the project item with the specified path.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="fullPath">The full path.</param>
        /// <returns>
        /// The project item with the specified path or a null reference if no item was found.
        /// </returns>
        public static Project FindProject(string projectPath)
        {
            // get the project
            foreach (Project project in ProjectRepository.GetProjects())
            {
                if (project.FullName.EqualsIgnoreCase(projectPath))
                    return project;
            }

            return null;
        }

        /// <summary>
        /// Checks the specified file out from source control.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <returns>true, if the file is checked out; otherwise, false.</returns>
        public static bool CheckOutFileFromSCC(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath) && Solution.FindProjectItem(fullPath) != null)
                {
                    // under source control and NOT checked out?
                    if (DTE.SourceControl.IsItemUnderSCC(fullPath) && 
                        !DTE.SourceControl.IsItemCheckedOut(fullPath))
                    {
                        DTE.SourceControl.CheckOutItem(fullPath);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed to check out file '{0}' from SCC", fullPath);
            }

            return false;
        }
    }
}
