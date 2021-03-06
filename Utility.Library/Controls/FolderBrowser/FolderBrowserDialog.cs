﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using System.Security.Permissions;

using Utility.Library.Internal.Win32;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Specifies the type of the root folder in a <see cref="FolderBrowserDialog"/>.
    /// </summary>
    public enum RootType
    {
        /// <summary>
        /// Use <see cref="System.Environment.SpecialFolder"/>.
        /// </summary>
        SpecialFolder,
        /// <summary>
        /// Use a path.
        /// </summary>
        Path,
    }

    /// <summary>
    /// Prompts the user to select a folder.
    /// </summary>
    public sealed class FolderBrowserDialog : Microsoft.Win32.CommonDialog
    {
        public const int BFFM_INITIALIZED = 1;
        public const int BFFM_SELCHANGED = 2;

        #region Fields

		[SecurityCritical, SecuritySafeCritical]
        NativeMethods.FolderBrowserOptions _dialogOptions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderBrowserDialog"/> class.
        /// </summary>
        [SecurityCritical]
        public FolderBrowserDialog()
        {
            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the properties of a common dialog to their default values.
        /// </summary>
        [SecurityCritical]
        public override void Reset()
        {
            new System.Security.Permissions.FileIOPermission(PermissionState.Unrestricted).Demand();

            Initialize();
        }

        /// <summary>
        /// Displays the folder browser dialog.
        /// </summary>
        /// <param name="hwndOwner">Handle to the window that owns the dialog box.</param>
        /// <returns>
        /// If the user clicks the OK button of the dialog that is displayed, true is returned; otherwise, false.
        /// </returns>
        [SecurityCritical]
        protected override bool RunDialog(IntPtr hwndOwner)
        {
            bool result = false;

            IntPtr pidlRoot = IntPtr.Zero;
            IntPtr pszPath = IntPtr.Zero;
            IntPtr pidlSelected = IntPtr.Zero;

          //  SelectedPath = string.Empty;

            try
            {

                if (RootType == RootType.SpecialFolder)
                {
                    NativeMethods.SHGetFolderLocation(hwndOwner, (int)RootSpecialFolder, IntPtr.Zero, 0, out pidlRoot);
                }
                else // RootType == Path
                {
                    uint iAttribute;
                    NativeMethods.SHParseDisplayName(RootPath, IntPtr.Zero, out pidlRoot, 0, out iAttribute);
                }

                NativeMethods.BROWSEINFO browseInfo = new NativeMethods.BROWSEINFO
                {
                    HwndOwner = hwndOwner,
                    Root = pidlRoot,
                    DisplayName = new string(' ', 256),
                    Title = Title,
                    Flags = (uint)_dialogOptions,
                    LParam = 0,
                    Callback = new NativeMethods.WndProc(HookProc)
                };

                // Show dialog
                pidlSelected = NativeMethods.SHBrowseForFolder(ref browseInfo);

                if (pidlSelected != IntPtr.Zero)
                {
                    result = true;

                    pszPath = Marshal.AllocHGlobal((int)(260 * Marshal.SystemDefaultCharSize));
                    NativeMethods.SHGetPathFromIDList(pidlSelected, pszPath);

                    SelectedPath = Marshal.PtrToStringAuto(pszPath);
                }
            }
            finally // release all unmanaged resources
            {
                NativeMethods.IMalloc malloc = NativeMethods.GetSHMalloc();

                if (pidlRoot != IntPtr.Zero)
                {
                    malloc.Free(pidlRoot);
                }

                if (pidlSelected != IntPtr.Zero)
                {
                    malloc.Free(pidlSelected);
                }

                if (pszPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszPath);
                }

                Marshal.ReleaseComObject(malloc);
            }

            return result;
        }

        protected override IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case BFFM_INITIALIZED:
                    if (this.SelectedPath != string.Empty)
                    {
                        return NativeMethods.SendMessage(new HandleRef(null, hwnd), 0x467, new IntPtr(1), this.SelectedPath);
                    }
                    break;

            //    case BFFM_SELCHANGED: //Selction Changed
            //        {
            //            IntPtr ptr1 = lParam;
            //            if (ptr1 != IntPtr.Zero)
            //            {
            //                IntPtr ptr2 = Marshal.AllocHGlobal((int)(260 * Marshal.SystemDefaultCharSize));
            //                bool flag1 = NativeMethods.SHGetPathFromIDList(ptr1, ptr2);
            //                this.SelectedPath = Marshal.PtrToStringAuto(ptr2);

            //                Marshal.FreeHGlobal(ptr2);
            //                NativeMethods.SendMessage2(new HandleRef(null, hwnd), 0x465, 0, flag1 ? 1 : 0);
            //            }
            //            break;
            //        }
            }

            return base.HookProc(hwnd, msg, wParam, lParam);
        }

        [SecurityCritical]
        private void Initialize()
        {
            RootType = RootType.SpecialFolder;
            RootSpecialFolder = Environment.SpecialFolder.Desktop;
            RootPath = string.Empty;
            Title = string.Empty;

            // default options
            _dialogOptions = NativeMethods.FolderBrowserOptions.BrowseFiles
                | NativeMethods.FolderBrowserOptions.ShowEditBox
                | NativeMethods.FolderBrowserOptions.UseNewStyle
                | NativeMethods.FolderBrowserOptions.BrowseShares
                | NativeMethods.FolderBrowserOptions.ShowStatusText
                | NativeMethods.FolderBrowserOptions.ValidateResult;
        }

        private bool GetOption(NativeMethods.FolderBrowserOptions option)
        {
            return ((_dialogOptions & option) != NativeMethods.FolderBrowserOptions.None);
        }

        [SecurityCritical]
        private void SetOption(NativeMethods.FolderBrowserOptions option, bool value)
        {
            if (value)
            {
                _dialogOptions |= option;
            }
            else
            {
                _dialogOptions &= ~option;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the root.
        /// </summary>
        /// <value>The type of the root.</value>
        public RootType RootType { get; set; }

        /// <summary>
        /// Gets or sets the root path.
        /// <remarks>Valid only if RootType is set to Path.</remarks>
        /// </summary>
        /// <value>The root path.</value>
        public string RootPath { get; set; }

        /// <summary>
        /// Gets or sets the root special folder.
        /// <remarks>Valid only if RootType is set to SpecialFolder.</remarks>
        /// </summary>
        /// <value>The root special folder.</value>
        public Environment.SpecialFolder RootSpecialFolder { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string SelectedPath { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether browsing files is allowed.
        /// </summary>
        /// <value></value>
        public bool BrowseFiles
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.BrowseFiles); }
            [SecurityCritical]
            set { SetOption(NativeMethods.FolderBrowserOptions.BrowseFiles, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show an edit box.
        /// </summary>
        /// <value></value>
        public bool ShowEditBox
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ShowEditBox); }
            [SecurityCritical]
            set { SetOption(NativeMethods.FolderBrowserOptions.ShowEditBox, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether browsing shares is allowed.
        /// </summary>
        /// <value></value>
        public bool BrowseShares
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.BrowseShares); }
            [SecurityCritical]
            set { SetOption(NativeMethods.FolderBrowserOptions.BrowseShares, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show status text.
        /// </summary>
        /// <value></value>
        public bool ShowStatusText
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ShowStatusText); }
            [SecurityCritical]
            set { SetOption(NativeMethods.FolderBrowserOptions.ShowStatusText, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to validate the result.
        /// </summary>
        /// <value></value>
        public bool ValidateResult
        {
            get { return GetOption(NativeMethods.FolderBrowserOptions.ValidateResult); }
            [SecurityCritical]
            set { SetOption(NativeMethods.FolderBrowserOptions.ValidateResult, value); }
        }

        #endregion
    }
}
