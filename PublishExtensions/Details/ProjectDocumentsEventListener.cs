using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.Details
{
    /// <summary>
    /// Listens for document events within the projects.
    /// </summary>
    internal class ProjectDocumentsEventListener : IVsTrackProjectDocumentsEvents2
    {
        private readonly IVsTrackProjectDocuments2 tracker = null;
        private uint cookie = VSConstants.VSCOOKIE_NIL;

        public event Action<ProjectItem, string> ItemRenamed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDocumentsEventListener"/> class.
        /// </summary>
        public ProjectDocumentsEventListener()
        {
            tracker = Globals.GetService<IVsTrackProjectDocuments2>(typeof(SVsTrackProjectDocuments));
        }

        /// <summary>
        /// Starts listening for the project document events.
        /// </summary>
        public void StartListening()
        {
            if (tracker != null)
            {
                int hr = tracker.AdviseTrackProjectDocumentsEvents(this, out cookie);
                ErrorHandler.ThrowOnFailure(hr);
            }
        }

        /// <summary>
        /// Stops listening for the project document events.
        /// </summary>
        public void StopListening()
        {
            if (tracker != null)
            {
                int hr = tracker.UnadviseTrackProjectDocumentsEvents(cookie);
                ErrorHandler.Succeeded(hr); // do nothing if this fails
                cookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        /// <summary>
        /// This method notifies the client when files have been renamed in the project.
        /// </summary>
        /// <param name="projectCount">[in] Number of projects in which files have been renamed.</param>
        /// <param name="fileCount">[in] Number of files renamed.</param>
        /// <param name="projects">[in] Array of projects in which files were renamed.</param>
        /// <param name="firstIndices">[in] Array of first indices identifying which project each file belongs to. For more information, see <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocumentsEvents2" />.</param>
        /// <param name="oldNames">[in] Array of paths for the old file names.</param>
        /// <param name="newNames">[in] Array of paths for the new file names.</param>
        /// <param name="flags">[in] Array of flags. For a list of <paramref name="rgFlags" /> values, see <see cref="T:Microsoft.VisualStudio.Shell.Interop.VSRENAMEFILEFLAGS" />.</param>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK" />. If it fails, it returns an error code.
        /// </returns>
        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(
            int projectCount, 
            int fileCount, 
            IVsProject[] projects, 
            int[] firstIndices, 
            string[] oldNames, 
            string[] newNames, 
            VSRENAMEFILEFLAGS[] flags)
        {
            // files available?
            if (projectCount > 0 && fileCount > 0)
            {
                // The way these parameters work is:
                // rgFirstIndices contains a list of the starting index into the file name arrays for each project 
                // listed in the rgpProjects list
                // Example: if you get two projects, then firstIndices should have two elements, the first element 
                // is probably zero since firstIndices would start at zero. Then item two in the firstIndices 
                // array is where in the changeProjectItems list that the second project's changed items reside.
                int itemIndex = 0;
                for (int projectIndex = 0; projectIndex < projectCount; ++projectIndex)
                {
                    // get index of last item in the current project
                    int endProjectIndex = ((projectIndex + 1) == projectCount) ? fileCount : firstIndices[projectIndex + 1];
                    string projectPath = projects[projectIndex].GetFullPath();
                    if (projectPath == null)
                        continue;

                    // process all items of that project
                    for (; itemIndex < endProjectIndex; itemIndex++)
                    {
                        ProjectItem item = Globals.FindProjectItem(projectPath, newNames[itemIndex]);
                        if (item != null)
                            OnItemRenamed(item, oldNames[itemIndex]);
                    }
                }
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when an item was renamed.
        /// </summary>
        /// <param name="item">The item that was renamed.</param>
        /// <param name="oldName">The old name of the item.</param>
        protected virtual void OnItemRenamed(ProjectItem item, string oldName)
        {
            if (ItemRenamed != null)
                ItemRenamed(item, oldName);
        }

        #region Unused

        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        #endregion Unused
    }
}
