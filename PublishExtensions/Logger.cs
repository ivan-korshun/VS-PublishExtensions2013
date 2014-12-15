using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace PHZH.PublishExtensions
{
    public static class Logger
    {
        public static readonly string Divider = new string('-', 100);
        public static readonly string ErrorDivider = new string('*', 100);
        
        private static IVsOutputWindowPane pane;
        private static object _syncRoot = new object();

        public static void Log(string message, params object[] args)
        {
            try
            {
                message = message.TryFormat(args).OrEmpty();
                if (EnsurePane())
                    pane.OutputString(message + Environment.NewLine);

                System.Diagnostics.Debug.Print(message);
            }
            catch
            {
            }
        }

        public static void Log(Exception ex, string message, params object[] args)
        {
            if (ex != null)
                message += Environment.NewLine + ex.ToString();

            Log(ErrorDivider);
            Log(message, args);
            Log(ErrorDivider);
        }

        public static void Debug(string message, params object[] args)
        {
            try
            {
                message = message.TryFormat(args).OrEmpty();
                System.Diagnostics.Debug.Print(message);
            }
            catch
            {
            }
        }

        public static void Debug(Exception ex, string message, params object[] args)
        {
            if (ex != null)
                message += ":" + Environment.NewLine + ex.ToString();

            Debug(ErrorDivider);
            Debug(message, args);
            Debug(ErrorDivider);
        }

        private static bool EnsurePane()
        {
            if (pane == null)
            {
                lock (_syncRoot)
                {
                    if (pane == null)
                    {
                        pane = Globals.Package.GetOutputPane(Guids.OutputWindowPane, "Publishing");
                    }
                }
            }

            return pane != null;
        }
    }
}
