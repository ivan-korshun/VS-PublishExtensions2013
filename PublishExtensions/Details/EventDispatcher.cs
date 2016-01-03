using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions.Details
{
    /// <summary>
    /// Dispatches events that occur in the solution and on project items.
    /// </summary>
    internal class EventDispatcher
    {
        private SolutionEvents solutionEvents = null;
        private BuildEvents buildEvents = null;
        private DocumentEvents documentEvents = null;
        private ProjectItemsEvents projectItemsEvents = null;
        private ProjectDocumentsEventListener documentListener = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
        /// </summary>
        public EventDispatcher()
        {
        }

        /// <summary>
        /// Starts listening for the events.
        /// </summary>
        public void Start()
        {
            DTE2 dte = Globals.DTE;

            // solution events
            solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.Opened += SettingsStore.UpdateCache;
            solutionEvents.AfterClosing += SettingsStore.UpdateCache;

            // document events
            documentEvents = dte.Events.DocumentEvents;
            documentEvents.DocumentSaved += SettingsStore.DocumentSaved;
            documentEvents.DocumentSaved += EventStore.DocumentSaved;

            Events2 events2 = dte.Events as Events2;
            if (events2 == null)
                return;

            // project item events
            projectItemsEvents = events2.ProjectItemsEvents;
            projectItemsEvents.ItemRemoved += SettingsStore.ItemRemoved;
            projectItemsEvents.ItemRemoved += EventStore.ItemRemoved;
            projectItemsEvents.ItemAdded += EventStore.ItemAdded;

            // document listener
            documentListener = new ProjectDocumentsEventListener();
            documentListener.ItemRenamed += SettingsStore.ItemRenamed;
            documentListener.ItemRenamed += EventStore.ItemRenamed;
            documentListener.StartListening();

            this.buildEvents = events2.BuildEvents;
            buildEvents.OnBuildProjConfigDone += EventStore.OnBuildProjConfigDone;
        }

        /// <summary>
        /// Stops listening for the events.
        /// </summary>
        public void Stop()
        {
            if (solutionEvents != null)
            {
                solutionEvents.Opened -= SettingsStore.UpdateCache;
                solutionEvents.AfterClosing -= SettingsStore.UpdateCache;
                solutionEvents = null;
            }

            if (documentEvents != null)
            {
                documentEvents.DocumentSaved -= EventStore.DocumentSaved;
                documentEvents.DocumentSaved -= SettingsStore.DocumentSaved;
                documentEvents = null;
            }

            if (projectItemsEvents != null)
            {
                projectItemsEvents.ItemRemoved -= EventStore.ItemRemoved;
                projectItemsEvents.ItemRemoved -= SettingsStore.ItemRemoved;
                projectItemsEvents.ItemAdded -= EventStore.ItemAdded;
                projectItemsEvents = null;
            }

            if (documentListener != null)
            {
                documentListener.StopListening();
                documentListener.ItemRenamed -= EventStore.ItemRenamed;
                documentListener.ItemRenamed -= SettingsStore.ItemRenamed;
                documentListener = null;
            }

            if (buildEvents != null)
            {
                buildEvents.OnBuildProjConfigDone -= EventStore.OnBuildProjConfigDone;
                buildEvents = null;
            }
        }
    }
}
