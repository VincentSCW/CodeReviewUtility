using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Library.Internal.Win32
{
    #region Header Control Styles
    /// <summary>
    /// Header control styles
    /// </summary>
    public enum HeaderControlStyles : int
    {
        HDS_HORZ = 0x0000,
        HDS_BUTTONS = 0x0002,
        HDS_HOTTRACK = 0x0004,
        HDS_HIDDEN = 0x0008,
        HDS_DRAGDROP = 0x0040,
        HDS_FULLDRAG = 0x0080,
        HDS_FILTERBAR = 0x0100
    }
    #endregion

    #region Header Control Messages
    public enum HeaderControlMessages : int
    {
        HDM_FIRST = 0x1200,
        HDM_GETITEMCOUNT = (HDM_FIRST + 0),
        HDM_INSERTITEMA = (HDM_FIRST + 1),
        HDM_DELETEITEM = (HDM_FIRST + 2),
        HDM_GETITEMA = (HDM_FIRST + 3),
        HDM_SETITEMA = (HDM_FIRST + 4),
        HDM_LAYOUT = (HDM_FIRST + 5),
        HDM_HITTEST = (HDM_FIRST + 6),
        HDM_GETITEMRECT = (HDM_FIRST + 7),
        HDM_SETIMAGELIST = (HDM_FIRST + 8),
        HDM_GETIMAGELIST = (HDM_FIRST + 9),
        HDM_INSERTITEMW = (HDM_FIRST + 10),
        HDM_GETITEMW = (HDM_FIRST + 11),
        HDM_SETITEMW = (HDM_FIRST + 12),
        HDM_ORDERTOINDEX = (HDM_FIRST + 15),
        HDM_CREATEDRAGIMAGE = (HDM_FIRST + 16),
        HDM_GETORDERARRAY = (HDM_FIRST + 17),
        HDM_SETORDERARRAY = (HDM_FIRST + 18),
        HDM_SETHOTDIVIDER = (HDM_FIRST + 19),
        HDM_SETBITMAPMARGIN = (HDM_FIRST + 20),
        HDM_GETBITMAPMARGIN = (HDM_FIRST + 21),
        HDM_SETFILTERCHANGETIMEOUT = (HDM_FIRST + 22),
        HDM_EDITFILTER = (HDM_FIRST + 23),
        HDM_CLEARFILTER = (HDM_FIRST + 24)
    }
    #endregion

    #region HeaderItem flags
    public enum HeaderItemFlags
    {
        HDI_WIDTH = 0x0001,
        HDI_HEIGHT = HDI_WIDTH,
        HDI_TEXT = 0x0002,
        HDI_FORMAT = 0x0004,
        HDI_LPARAM = 0x0008,
        HDI_BITMAP = 0x0010,
        HDI_IMAGE = 0x0020,
        HDI_DI_SETITEM = 0x0040,
        HDI_ORDER = 0x0080,
        HDI_FILTER = 0x0100
    }
    #endregion

    /// <summary>
    /// Header control item format
    /// </summary>
    public enum HeaderItemFormatFlags : int
    {
        HDF_LEFT = 0x0000,
        HDF_RIGHT = 0x0001,
        HDF_CENTER = 0x0002,
        HDF_JUSTIFYMASK = 0x0003,
        HDF_NOJUSTIFY = 0xFFFC,
        HDF_RTLREADING = 0x0004,
        HDF_SORTDOWN = 0x0200,
        HDF_SORTUP = 0x0400,
        HDF_SORTED = 0x0600,
        HDF_NOSORT = 0xF1FF,
        HDF_IMAGE = 0x0800,
        HDF_BITMAP_ON_RIGHT = 0x1000,
        HDF_BITMAP = 0x2000,
        HDF_STRING = 0x4000,
        HDF_OWNERDRAW = 0x8000
    }
}
