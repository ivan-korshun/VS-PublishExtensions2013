using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace System.Windows
{
    public static class WpfExtensions
    {
        private class Wpf32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Wpf32Window(Window wpfWindow)
            {
                Handle = new WindowInteropHelper(wpfWindow).Handle;
            }
        }
        
        public static bool? ShowDialog(this Window window, Window owner)
        {
            window.ThrowIfNull("window");

            if (owner != null)
                window.Owner = owner;

            return window.ShowDialog();
        }

        public static bool? ShowDialog(this Window window, IntPtr owner)
        {
            window.ThrowIfNull("window");

            window.SetOwner(owner);
            return window.ShowDialog();
        }

        public static void SetOwner(this Window window, IntPtr owner)
        {
            if (window == null)
                return;

            if (owner == IntPtr.Zero)
                return;
            
            var wih = new WindowInteropHelper(window);
            wih.Owner = owner;
        }

        public static System.Windows.Forms.DialogResult ShowDialog(this System.Windows.Forms.CommonDialog dialog, Window parent)
        {
            return dialog.ShowDialog(new Wpf32Window(parent));
        }
    }
}
