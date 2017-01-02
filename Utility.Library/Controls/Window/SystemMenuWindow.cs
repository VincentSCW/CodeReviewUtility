using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Utility.Library.Controls
{
    public class SystemMenuWindow : Window
    {
        private const uint WM_SYSCOMMAND = 0x112;
        private const uint WM_INITMENUPOPUP = 0x0117;
        private const uint MF_SEPARATOR = 0x800;
        private const uint MF_BYCOMMAND = 0x0;
        private const uint MF_BYPOSITION = 0x400;
        private const uint MF_STRING = 0x0;
        private const uint MF_ENABLED = 0x0;
        private const uint MF_DISABLED = 0x2;
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InsertMenu(IntPtr hmenu, int position, uint flags, uint item_id, [MarshalAs(UnmanagedType.LPTStr)]string item_text);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register("MenuItems", 
            typeof(FreezableCollection<SystemMenuItem>), 
            typeof(SystemMenuWindow), 
            new PropertyMetadata(new PropertyChangedCallback(OnMenuItemsChanged)));

        private IntPtr systemMenu;

        public FreezableCollection<SystemMenuItem> MenuItems
        {
            get { return (FreezableCollection<SystemMenuItem>)this.GetValue(MenuItemsProperty); }
            set { this.SetValue(MenuItemsProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the SystemMenuWindow class.
        /// </summary>
        public SystemMenuWindow()
        {
            this.Loaded += (o, e) =>
                {
                    WindowInteropHelper helper = new WindowInteropHelper(this);
                    this.systemMenu = GetSystemMenu(helper.Handle, false);

                    if (this.MenuItems.Count > 0)
                    {
                        InsertMenu(this.systemMenu, -1, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
                    }

                    foreach (SystemMenuItem item in this.MenuItems)
                    {
                        if(item.IsSeparator)
                            InsertMenu(this.systemMenu, -1, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
                        else
                            InsertMenu(this.systemMenu, (int)item.Id, MF_BYCOMMAND | MF_STRING, (uint)item.Id, item.Header);
                    }

                    HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
                    hwndSource.AddHook(this.WndProc);
                };

            this.MenuItems = new FreezableCollection<SystemMenuItem>();
        }

        private static void OnMenuItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemMenuWindow obj = d as SystemMenuWindow;

            if (obj != null)
            {
                if (e.NewValue != null)
                {
                    obj.MenuItems = e.NewValue as FreezableCollection<SystemMenuItem>;
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case WM_SYSCOMMAND:
                    var menuItem = this.MenuItems.Where(mi => mi.Id == wParam.ToInt32()).FirstOrDefault();
                    if (menuItem != null)
                    {
                        menuItem.Command.Execute(menuItem.CommandParameter);
                        handled = true;
                    }

                    break;

                case WM_INITMENUPOPUP:
                    if (this.systemMenu == wParam)
                    {
                        foreach (SystemMenuItem item in this.MenuItems.Where(m => !m.IsSeparator))
                        {
                           EnableMenuItem(this.systemMenu, (uint)item.Id, item.Command != null && item.Command.CanExecute(item.CommandParameter) ? MF_ENABLED : MF_DISABLED);
                        }
                        handled = true;
                    }

                    break;
            }

            return IntPtr.Zero;
        }        
    }
}
