using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using Utility.Library.Internal.Win32;

namespace Utility.Library.Controls
{
    public class Window : System.Windows.Window
    {
        private WindowInteropHelper windowInteropHelper;

        public Window()
        {
            this.SetupOwner();
        }

        private void SetupOwner()
        {
            if (this.Owner == null)
            {
                windowInteropHelper = new WindowInteropHelper(this);
              

                if (windowInteropHelper.Owner == IntPtr.Zero)
                    windowInteropHelper.Owner = NativeMethods.GetActiveWindow();
            }
        }
    }
}
