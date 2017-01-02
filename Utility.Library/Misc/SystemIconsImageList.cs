using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Utility.Library.Internal.Win32;

namespace Utility.Library.Misc
{
    /// <summary>
    /// Helper class to get/load icons for know files
    /// </summary>
	public sealed class SystemIconsManager : IDisposable
	{
        public static SystemIconsManager Current = new SystemIconsManager();

        sealed class Shell32Helper
        {
            public const int MAX_PATH = 256;

            [StructLayout(LayoutKind.Sequential)]
            public struct SHFILEINFO
            {
                public const int NAMESIZE = 80;
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
                public string szTypeName;
            };

            public const uint SHGFI_ICON = 0x000000100;     // get icon
            public const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
            public const uint SHGFI_TYPENAME = 0x000000400;     // get type name
            public const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
            public const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
            public const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
            public const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
            public const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
            public const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
            public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
            public const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
            public const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
            public const uint SHGFI_OPENICON = 0x000000002;     // get open icon
            public const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
            public const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
            public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
            public const uint SHGFI_ADDOVERLAYS = 0x000000020;     // apply the appropriate overlays
            public const uint SHGFI_OVERLAYINDEX = 0x000000040;     // Get the index of the overlay

            public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
        }

        /// <summary>
        /// Structure that encapsulates basic information of icon embedded in a file.
        /// </summary>
        private struct EmbeddedIconInfo
        {
            public string FileName;
            public int IconIndex;
        }

        //private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        //private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

        [Flags]
        private enum SHGetFileInfoConstants : uint
        {
            SHGFI_ICON = 0x100,                // get icon 
            SHGFI_DISPLAYNAME = 0x200,         // get display name 
            SHGFI_TYPENAME = 0x400,            // get type name 
            SHGFI_ATTRIBUTES = 0x800,          // get attributes 
            SHGFI_ICONLOCATION = 0x1000,       // get icon location 
            SHGFI_EXETYPE = 0x2000,            // return exe type 
            SHGFI_SYSICONINDEX = 0x4000,       // get system icon index 
            SHGFI_LINKOVERLAY = 0x8000,        // put a link overlay on icon 
            SHGFI_SELECTED = 0x10000,          // show icon in selected state 
            SHGFI_ATTR_SPECIFIED = 0x20000,    // get only specified attributes 
            SHGFI_LARGEICON = 0x0,             // get large icon 
            SHGFI_SMALLICON = 0x1,             // get small icon 
            SHGFI_OPENICON = 0x2,              // get open icon 
            SHGFI_SHELLICONSIZE = 0x4,         // get shell size icon 
            //SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
            SHGFI_USEFILEATTRIBUTES = 0x10,     // use passed dwFileAttribute 
            SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040,     // Get the index of the overlay

            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_NORMAL     = 0x00000080  
        }
	
        IDictionary<string, string> fileTypeAndIconInformation = new Dictionary<string, string>();

        /// <summary>
		/// List with small icons
		/// </summary>
        public IDictionary<string, BitmapSource> SmallIconsImageList { get; private set;}
		
		/// <summary>
		/// List with large icons in
		/// </summary>
        public IDictionary<string, BitmapSource> LargeIconsImageList  { get; private set;}

		/// <summary>
		/// Default constructor
		/// </summary>
        private SystemIconsManager()
		{
            this.LoadFileTypeAndIconInformation();
            this.LargeIconsImageList = new Dictionary<string, BitmapSource>();
            this.SmallIconsImageList = new Dictionary<string, BitmapSource>();
		}		

		/// <summary>
		/// Performs resource cleaning
		/// </summary>
		public void Dispose()
		{
            this.LargeIconsImageList.Clear();
            this.LargeIconsImageList.Clear();

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Returns index of an icon based on FileName. Note: File should exists!
		/// </summary>
		/// <param name="FileName">Name of an existing File or Directory</param>
		/// <returns>Index of an Icon</returns>
		public BitmapSource GetSmallIcon(string fileName)
		{
            var key = this.LoadIcon(fileName);

            BitmapSource s;
            this.SmallIconsImageList.TryGetValue(key, out s);
            return s;
        }

        public BitmapSource  GetLargeIcon(string fileName)
		{
            var key = this.LoadIcon(fileName);
            BitmapSource s;
            this.LargeIconsImageList.TryGetValue(key, out s);
            return s;
        }

        private string LoadIcon(string fileName)
        {
            
            FileInfo info = new FileInfo(fileName);
            string ext = info.Extension;

            try
            {
                if (string.IsNullOrEmpty(ext))
                {
                    if ((info.Attributes & FileAttributes.Directory) != 0)
                    {
                        ext = "5EEB255733234c4dBECF9A128E896A1E"; // for directories

                        if (!this.SmallIconsImageList.ContainsKey(ext))
                        {
                            this.SmallIconsImageList.Add(ext, this.GetFolderIcon(true));
                            this.LargeIconsImageList.Add(ext, this.GetFolderIcon(false));
                        }

                    }
                    else
                    {
                        ext = "F9EB930C78D2477c80A51945D505E9C4"; // for files without extension
                        this.GetIconFromSHGetFileInfo(ext, fileName);
                    }

                    return ext;
                }

                if (ext.Equals(".exe", StringComparison.InvariantCultureIgnoreCase) || ext.Equals(".lnk", StringComparison.InvariantCultureIgnoreCase))
                {
                    ext = info.Name;
                }

                if (!this.SmallIconsImageList.ContainsKey(ext))
                {
                    string fileAndParam;
                    if (this.fileTypeAndIconInformation.TryGetValue(ext.ToLower(), out fileAndParam))
                    {
                        this.SmallIconsImageList.Add(ext, this.GetIconFromFileExt(fileAndParam, false));
                        this.LargeIconsImageList.Add(ext, this.GetIconFromFileExt(fileAndParam, true));
                    }
                }
            }
            catch
            { 
            }
            return ext;
		}

        private BitmapSource GetFolderIcon(bool smallSize)
        {
            // Need to add size check, although errors generated at present!
            uint flags = Shell32Helper.SHGFI_ICON | Shell32Helper.SHGFI_USEFILEATTRIBUTES | Shell32Helper.SHGFI_OPENICON;

            if (smallSize)
            {
                flags += Shell32Helper.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32Helper.SHGFI_LARGEICON;
            }

            // Get the folder icon
            Shell32Helper.SHFILEINFO shfi = new Shell32Helper.SHFILEINFO();
            Shell32Helper.SHGetFileInfo(null,
                Shell32Helper.FILE_ATTRIBUTE_DIRECTORY,
                ref shfi,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                (uint)flags);

            return Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private BitmapSource GetFileIcon(string fileName, bool smallSize, bool linkOverlay)
        {
            // Need to add size check, although errors generated at present!
            uint flags = Shell32Helper.SHGFI_ICON | Shell32Helper.SHGFI_USEFILEATTRIBUTES;

            if (linkOverlay)
                flags += Shell32Helper.SHGFI_LINKOVERLAY;

            if (smallSize)
            {
                flags += Shell32Helper.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32Helper.SHGFI_LARGEICON;
            }

            // Get the folder icon
            Shell32Helper.SHFILEINFO shfi = new Shell32Helper.SHFILEINFO();
            Shell32Helper.SHGetFileInfo(null,
                Shell32Helper.FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                (uint)flags);

            return Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        private void GetIconFromSHGetFileInfo(string ext, string fileName)
        {
            if (!this.SmallIconsImageList.ContainsKey(ext) && (Directory.Exists(fileName) || File.Exists(fileName)))
            {              
                try
                {
                    this.SmallIconsImageList.Add(ext, GetFileIcon(fileName, true, false));
                    this.LargeIconsImageList.Add(ext, GetFileIcon(fileName, false, false));
                }
                catch
                {
                }
            }
        }



        /// <summary>
        /// Extract the icon from file.
        /// </summary>
        /// <param name="fileAndParam">The params string, 
        /// such as ex: "C:\\Program Files\\NetMeeting\\conf.exe,1".</param>
        /// <param name="isLarge">
        /// Determines the returned icon is a large (may be 32x32 px) 
        /// or small icon (16x16 px).</param>
        private BitmapSource GetIconFromFileExt(string fileAndParam, bool isLarge)
        {
            uint readIconCount = 0;
            IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
            IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };

            try
            {
                EmbeddedIconInfo embeddedIcon = GetEmbeddedIconInfo(fileAndParam);

                if (isLarge)
                    readIconCount = NativeMethods.ExtractIconEx(embeddedIcon.FileName, 0, hIconEx, hDummy, 1);
                else
                    readIconCount = NativeMethods.ExtractIconEx(embeddedIcon.FileName, 0, hDummy, hIconEx, 1);

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero)
                {
                    // Get first icon.
                    return Imaging.CreateBitmapSourceFromHIcon(hIconEx[0], Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }

                return null;
            }
            catch (Exception exc)
            {
                // Extract icon error.
                throw new ApplicationException("Could not extract icon", exc);
            }
            finally
            {
                // Release resources.
                foreach (IntPtr ptr in hIconEx)
                    if (ptr != IntPtr.Zero)
                        NativeMethods.DestroyIcon(ptr);

                foreach (IntPtr ptr in hDummy)
                    if (ptr != IntPtr.Zero)
                        NativeMethods.DestroyIcon(ptr);
            }
        }

        /// <summary>
        /// Parses the parameters string to the structure of EmbeddedIconInfo.
        /// </summary>
        /// <param name="fileAndParam">The params string, 
        /// such as ex: "C:\\Program Files\\NetMeeting\\conf.exe,1".</param>
        /// <returns></returns>
        private EmbeddedIconInfo GetEmbeddedIconInfo(string fileAndParam)
        {
            EmbeddedIconInfo embeddedIcon = new EmbeddedIconInfo();

            if (string.IsNullOrEmpty(fileAndParam))
                return embeddedIcon;

            //Use to store the file contains icon.
            string fileName = string.Empty;

            //The index of the icon in the file.
            int iconIndex = 0;
            string iconIndexString = string.Empty;

            int commaIndex = fileAndParam.IndexOf(",");
            //if fileAndParam is some thing likes that: "C:\\Program Files\\NetMeeting\\conf.exe,1".
            if (commaIndex > 0)
            {
                fileName = fileAndParam.Substring(0, commaIndex);
                iconIndexString = fileAndParam.Substring(commaIndex + 1);
            }
            else
                fileName = fileAndParam;

            if (!string.IsNullOrEmpty(iconIndexString))
            {
                //Get the index of icon.
                iconIndex = int.Parse(iconIndexString);
                if (iconIndex < 0)
                    iconIndex = 0;  //To avoid the invalid index.
            }

            embeddedIcon.FileName = fileName;
            embeddedIcon.IconIndex = iconIndex;

            return embeddedIcon;
        }
        /// <summary>
        /// Gets registered file types and their associated icon in the system.
        /// </summary>
        /// <returns>Returns a hash table which contains the file extension as keys, the icon file and param as values.</returns>
        private void LoadFileTypeAndIconInformation()
        {
            // Create a registry key object to represent the HKEY_CLASSES_ROOT registry section
            using (RegistryKey rkRoot = Registry.ClassesRoot)
            {
                //Gets all sub keys' names.
                //Find the file icon.
                foreach (string keyName in rkRoot.GetSubKeyNames())
                {
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        int indexOfPoint = keyName.IndexOf(".");
                        //If this key is not a file exttension(eg, .zip), skip it.
                        if (indexOfPoint == 0)
                        {
                            using(RegistryKey rkFileType = rkRoot.OpenSubKey(keyName))
                            {
                                if (rkFileType != null)
                                {
                                    //Gets the default value of this key that contains the information of file type.
                                    object defaultValue = rkFileType.GetValue("");
                                    if (defaultValue != null)
                                    {
                                        //Go to the key that specifies the default icon associates with this file type.
                                        string defaultIcon = defaultValue.ToString() + "\\DefaultIcon";
                                        using (RegistryKey rkFileIcon = rkRoot.OpenSubKey(defaultIcon))
                                        {
                                            if (rkFileIcon != null)
                                            {
                                                //Get the file contains the icon and the index of the icon in that file.
                                                object value = rkFileIcon.GetValue("");
                                                if (value != null)
                                                {
                                                    //Clear all unecessary " sign in the string to avoid error.
                                                    string fileParam = value.ToString().Replace("\"", "");
                                                    fileTypeAndIconInformation.Add(keyName.ToLower(), fileParam);
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
