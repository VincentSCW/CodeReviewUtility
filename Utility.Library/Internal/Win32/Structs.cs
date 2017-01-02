using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Utility.Library.Internal.Win32
{
    #region HDITEM
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct HDITEM
    {
        public HeaderItemFlags mask;
        public int cxy;
        public IntPtr pszText;
        public IntPtr hbm;
        public int cchTextMax;
        public HeaderItemFormatFlags fmt;
        public int lParam;
        public int iImage;
        public int iOrder;
        public uint type;
        public IntPtr pvFilter;
    }
    #endregion

    /// <summary>
    /// Header filter text data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct HDTEXTFILTER
    {
        public String pszText;
        public int cchTextMax;
    }
}
