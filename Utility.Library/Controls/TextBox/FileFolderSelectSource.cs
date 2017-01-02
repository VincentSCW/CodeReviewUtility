using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Linq;


namespace Utility.Library.Controls
{
    public class FolderSelectSource :
        ISelectAutoCompleteSource
    {
        public FolderSelectSource()
        {
        }

        public IEnumerable GetItems(Key key, string text)
        {
            if (key == Key.Oem5)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    IList<string> items = new List<string>();
                    IEnumerable<string> paths = Lookup(text);

                    foreach (string path in paths)
                    {
                        if (!(string.Equals(path, text, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            items.Add(path);
                        }
                    }

                    return items;
                }
            }

            return null;
        }
        
        protected virtual IEnumerable<string> Lookup(string path)
        {
            IEnumerable<string> names = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if(!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        path += Path.DirectorySeparatorChar.ToString();

                    DirectoryInfo lookupFolder = new DirectoryInfo(path);
                    if (lookupFolder != null)
                    {
                        if (lookupFolder.Exists)
                        {
                            names = this.Lookup(lookupFolder, path);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return names;
        }


        protected virtual IEnumerable<string> Lookup(DirectoryInfo lookupFolder, string path)
        {
            return from di in lookupFolder.GetDirectories()
                                     where di.FullName.StartsWith(path, StringComparison.CurrentCultureIgnoreCase) && IsAccessible(di)
                                     select di.FullName;
        }

        protected static bool IsAccessible(FileSystemInfo info)
        {
            if ((info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden
                || (info.Attributes & FileAttributes.System) == FileAttributes.System)
            {
                return false;
            }
            return true;
        }
    }

    public class FileFolderSelectSource : FolderSelectSource
    {
        public string FileExtensionStr { get; set; }

        public FileFolderSelectSource()
        {
        }

        protected override IEnumerable<string> Lookup(DirectoryInfo lookupFolder, string path)
        {
            List<string> names = new List<string>(base.Lookup(lookupFolder, path));

            var items = from di in lookupFolder.GetFiles()
                     where di.FullName.StartsWith(path, StringComparison.CurrentCultureIgnoreCase) && IsAccessible(di)
                           && String.Compare(di.Extension, 
                                            string.IsNullOrEmpty(this.FileExtensionStr) ? di.Extension : this.FileExtensionStr, 
                                            true) == 0
                     select di.FullName;

            names.AddRange(items);

            return names;
        }
    }
}
