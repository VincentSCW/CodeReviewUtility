using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utility.Library.Misc
{
    public static class FileSystem
    {
        public static bool IsDirectory(string name)
        {
            DirectoryInfo info = new DirectoryInfo(name);
            return info.Exists && (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public static void CreateDirectory(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public static void DeleteFolder(string folder)
        {
            // Clean up
            if (Directory.Exists(folder))
            {
                try
                {

                    // Delete any file in the root
                    var contained = Directory.Exists(folder) ? Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly).ToList() : new List<string>();
                    foreach (string file in contained)
                    {
                        DeleteFile(file);
                    }
                    // Delete the folder
                    DeleteDirectory(folder);

                }
                catch { }
            }
        }

        public static bool DeleteFile(string file)
        {
            // Remove the ReadOnly flag if any..
            if (File.Exists(file))
            {
                // Clean up
                try
                {
                    if ((File.GetAttributes(file) & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(file, FileAttributes.Normal);

                    File.Delete(file);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }


        public static string GetInTemp(string filename)
        {
            return Path.Combine(Path.GetTempPath(), filename);
        }

        private static bool DeleteDirectory(string Path)
        {
            if (Directory.Exists(Path))
            {
                try
                {
                    ClearAttributes(Path);
                    Directory.Delete(Path, true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return true;
        }


        private static void ClearAttributes(string currentDir)
        {
           if (Directory.Exists(currentDir))
           {
               foreach (string dir in Directory.GetDirectories(currentDir))
               {
                   ClearAttributes(dir);
               }

               foreach (string file in Directory.GetFiles(currentDir))
               {
                   File.SetAttributes(file, FileAttributes.Normal);
               }

            }
        }
    }
}
