using System;
using System.IO;

namespace Utility.Library.Misc
{
    /// <summary>
    /// Helper class to zip/unzip a folder
    /// 
    /// see http://msdn.microsoft.com/en-us/library/windows/desktop/bb787866(v=vs.85).aspx
    /// for details on Shell class
    /// 
    /// we using dynamic and CreateInstance because Shell32 is difference on Different OS's.
    /// </summary>
    public class ZipUtil
    {
        private static object GetShell32NameSpaceFolder(Object folder)
        {
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");

            Object shell = Activator.CreateInstance(shellAppType);
            return shellAppType.InvokeMember(
                    "NameSpace",
                System.Reflection.BindingFlags.InvokeMethod, 
                null, shell, new object[] { folder });
        } 

        public static void CompressFolder(string compressedFileName, string folderToCompress)
        {
            if (File.Exists(compressedFileName))
                File.Delete(compressedFileName);

            byte[] B = new byte[22];
            B[0] = 80;
            B[1] = 75;
            B[2] = 5;
            B[3] = 6;

            File.WriteAllBytes(compressedFileName, B); //Make an empty PKZip file.

            dynamic source = GetShell32NameSpaceFolder(compressedFileName); // shell.NameSpace(compressedFileName);
            dynamic dest = GetShell32NameSpaceFolder(folderToCompress);//  shell.NameSpace(folderToCompress);

            int instanceCount = 0;
            int initialCount = source.Items().Count;

            source.CopyHere(dest, 4 | 20);    // Do not display a progress dialog box && Respond with "Yes to All" for any dialog box that is displayed.

            System.Threading.Thread.Sleep(500);

            // The CopyHere method works asynchronously when called on a ZIP file. That is to say, even you call the method 
            // directly without using BeginInvoke, it returns immediately if there is no error.
            // So to wait until done  can be accomplished by comparing Item counts and sleeping the foreground thread until the
            // item count in the zip archive is incremented. Its dumb, but it works and its relatively straight forward. 
            while (instanceCount <= initialCount)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for zip to complete..");
                System.Threading.Thread.Sleep(500);
                instanceCount = source.Items().Count;
            }
            System.Diagnostics.Debug.WriteLine("Zip completed..");
        }

        public static void UnCompressFolder(string compressedFileName, string folderToUnCompress)
        {
            string target = Path.Combine(folderToUnCompress, Path.GetFileNameWithoutExtension(compressedFileName));
            if (Directory.Exists(target))
                Directory.Delete(target, true);

            dynamic source = GetShell32NameSpaceFolder(compressedFileName); // shell.NameSpace(compressedFileName);
            dynamic dest = GetShell32NameSpaceFolder(folderToUnCompress);// shell.NameSpace(folderToUnCompress);
            dynamic items = source.Items();

            dest.CopyHere(items, 4 | 20);    // Do not display a progress dialog box && Respond with "Yes to All" for any dialog box that is displayed.

            System.Threading.Thread.Sleep(500);

            int instanceCount = 0;
            int initialCount = dest.Items().Count;

            // The CopyHere method works asynchronously when called on a ZIP file. That is to say, even you call the method 
            // directly without using BeginInvoke, it returns immediately if there is no error.
            // So to wait until done  can be accomplished by comparing Item counts and sleeping the foreground thread until the
            // item count in the zip archive is incremented. Its dumb, but it works and its relatively straight forward. 
            while (instanceCount < initialCount)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for unzip to complete..");
                System.Threading.Thread.Sleep(500);
                instanceCount = dest.Items().Count;
            }

            System.Diagnostics.Debug.WriteLine("Unzip completed..");
        }
    }
}
