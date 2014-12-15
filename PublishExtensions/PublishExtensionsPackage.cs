using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using PHZH.PublishExtensions.Details;
using PHZH.PublishExtensions.MenuItems;
using PHZH.PublishExtensions.Settings;

namespace PHZH.PublishExtensions
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(Guids.PublishExtensionsPkgString)]
    public sealed class PublishExtensionsPackage : Package
    {
        private EventDispatcher eventDispatcher = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishExtensionsPackage"/> class.
        /// </summary>
        public PublishExtensionsPackage()
        {
        }

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to return.</typeparam>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>A service of the specified type.</returns>
        public T GetService<T>(Type serviceType)
            where T: class
        {
            serviceType.ThrowIfNull("serviceType");
            return GetService(serviceType) as T;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            DTE2 dte = GetService<DTE2>(typeof(DTE));
            if (dte == null)
                return;

            Globals.Initialize(this, dte);

            // add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService<OleMenuCommandService>(typeof(IMenuCommandService));
            if (mcs != null)
            {
                PublishMenu publishMenu = new PublishMenu(dte, mcs);
                publishMenu.SetupCommands();
            }

            // attach events
            eventDispatcher = new EventDispatcher();
            eventDispatcher.Start();

            Logger.Log("Command Bars: {0}", dte.CommandBars);

            Logger.Log("Publish Extensions initialized");
        }

        /// <summary>
        /// Called to ask the package if the shell can be closed.
        /// </summary>
        /// <param name="canClose">[out] Returns true if the shell can be closed, otherwise false.</param>
        /// <returns>
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK" /> if the method succeeded, otherwise an error code.
        /// </returns>
        protected override int QueryClose(out bool canClose)
        {
            int result = base.QueryClose(out canClose);
            if (!canClose)
                return result;

            // remove events
            if (eventDispatcher != null)
            {
                eventDispatcher.Stop();
                eventDispatcher = null;
            }

            return result;
        }
    }
}
