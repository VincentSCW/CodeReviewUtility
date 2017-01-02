using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Utility.Library.Misc;
using Utility.Model;
using Utility.Interface;

namespace Utility.SVN
{
    /// <summary>
    /// SVN Manager, handle all commands/queries for SVN
    /// 
    /// 
    /// See http://tortoisesvn.net/docs/release/TortoiseSVN_en/tsvn-automation.html for usage of TortoiseProc
    /// </summary>
    public class SVNManager : ISourceControlManager
    {
        public string Name
        {
            get { return "SVN"; }
        }

        private string sourceFolder;

        /// <summary>
        /// Source folder we working on.
        /// </summary>
        public string SourceFolder
        {
            get { return this.sourceFolder; }
            set
            {
                this.sourceFolder = value;
                files = null;
            }
        }
        /// <summary>
        /// The initialize set of files we can work on.
        /// </summary>
        private IList<FileModel> files;

        public SVNManager()
        {
        }

        /// <summary>
        /// Get all the files know to SVN under the source folder.
        /// We filter out the  tmp files like xyz.~cs
        /// </summary>
        /// <returns>Sorted list of FileViewModel element</returns>
        public IList<FileModel> GetFiles()
        {
            if (files == null)
            {
                var source = this.SourceFolder;
                var details = this.ExecuteSvnCmd("status");
                var tmp = details
                    .Where(s =>
                    {
                        if (string.IsNullOrEmpty(s) || s.Length < 2)
                            return false;

                        int start = s.Contains("+") ? s.IndexOf("+") + 1 : 2;
                        var fileName = s.Substring(start).Trim();

                        if (FileSystem.IsDirectory(Path.Combine(source, fileName)))
                        {
                            return IncludeFolder(fileName);
                        }

                        string ext = Path.GetExtension(fileName);
                        return !ext.StartsWith(".~");
                    })
                    .Select(s =>
                    {
                        int start = s.Contains("+") ? s.IndexOf("+") + 1 : 2;
                        var fileName = s.Substring(start).Trim();
                        if (fileName.EndsWith("\\r"))
                            fileName = fileName.Substring(0, fileName.Length - 1);

                        return new FileModel(GetStateFrom(s.Substring(0, 1).Trim()), fileName);
                    })
                    .ToList();

                var folders = tmp.Where(f => FileSystem.IsDirectory(Path.Combine(source, f.FileName))).ToList();
                foreach (var f in folders)
                {
                    if (IncludeFolder(f.FileName))
                    {
                        var tf = Path.Combine(source, f.FileName);
                        if (Directory.Exists(tf))
                        {
                            var o = Directory.EnumerateFiles(tf, "*", SearchOption.AllDirectories);
                            foreach (var fi in o)
                            {
                                try
                                {
                                    if (IncludeFolder(fi))
                                    {
                                        var file = fi.Substring(source.Length);
                                        if (file.StartsWith("\\"))
                                            file = file.Substring(1);

                                        tmp.Add(new FileModel(FileModificationState.NotVersioned, file));
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }

                files = tmp
                    .OrderBy(s => s.State).ThenBy(s => s.FileName)
                    .ToList();
            }

            return files;
        }

        private bool IncludeFolder(string folder)
        {
            return !(folder.Equals("obj", StringComparison.CurrentCultureIgnoreCase)
                || folder.Equals("bin", StringComparison.CurrentCultureIgnoreCase)
                || folder.EndsWith("obj", StringComparison.CurrentCultureIgnoreCase)
                || folder.EndsWith("bin", StringComparison.CurrentCultureIgnoreCase)
                || folder.ToLower().Contains("\\debug\\")
                || folder.ToLower().Contains("\\release\\"));
        }

        /// <summary>
        /// Answer if SNV is installed
        /// </summary>
        public bool IsAvaliable
        {
            get
            {
                string path = System.Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                return path.Split(';').Any((p) => File.Exists(Path.Combine(p, "svn.exe")))
                    && path.Split(';').Any((p) => File.Exists(Path.Combine(p, "TortoiseProc.exe")));
            }
        }

        /// <summary>
        /// Get the SVN details for a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IDictionary<FileDetailInfo, string> GetFileInfoDetails(string fileName)
        {
            var details = this.ExecuteSvnCmd(string.Format("info \"{0}\"", fileName));

            IDictionary<FileDetailInfo, string> map = new Dictionary<FileDetailInfo, string>();

            foreach (string s in details)
            {
                var seporatorIndex = s.IndexOf(": ");
                if (seporatorIndex != -1)
                {
                    var keyName = s.Substring(0, seporatorIndex).Trim();
                    var key = GetDetailFrom(keyName);
                    if (key != FileDetailInfo.Unknown)
                        map.Add(key, s.Substring(seporatorIndex + 2).Trim());
                }
            }

            return map;
        }

        public string GetUrl()
        {
            var details = this.ExecuteSvnCmd("info");

            foreach (string s in details)
            {
                var seporatorIndex = s.IndexOf(": ");
                if (seporatorIndex != -1)
                {
                    var key = s.Substring(0, seporatorIndex).Trim();
                    if (GetDetailFrom(key) == FileDetailInfo.URL)
                        return s.Substring(seporatorIndex + 2).Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the file by 'revision' from SVN and store it in 'destination'
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="source"></param>
        /// <param name="desination"></param>
        public void GetFile(string revision, string source, string desination)
        {
            this.EnsureExists(desination);
            this.ExecuteSvnCmd(string.Format("export -r {0} \"{1}\" \"{2}\"", revision, source, desination));
        }

        public void Add(IEnumerable<string> sources)
        {
            foreach (string source in sources)
                this.Add(source);
        }

        public void Add(string source)
        {
            this.ExecuteSvnCmd(string.Format("add \"{0}\"", source));
        }

        public void RevertChanges(IEnumerable<string> sources)
        {
            foreach (string source in sources)
                this.RevertChanges(source);
        }

        public void RevertChanges(string source)
        {
            this.ExecuteSvnCmd(string.Format("revert \"{0}\"", source));
        }
        /// <summary>
        /// Start the SVN diff tool. This will compare it with the BASE version in SVN
        /// </summary>
        /// <param name="fileName"></param>
        public void DiffWithBase(string fileName)
        {
            string tmp;
            ProcessHelper.Execute("TortoiseProc",
                string.Format("/command:diff /path:{0} /notempfile", ProcessHelper.IncludeQoutes(fileName)), this.SourceFolder, false, out tmp);
        }

        /// <summary>
        /// Start the SVN diff tool. This will compare the two file using the compare used by SVN
        /// </summary>
        /// <param name="newFile"></param>
        /// <param name="oldFile"></param>
        public void Diff(string newFile, string oldFile)
        {
            string tmp;
            ProcessHelper.Execute("TortoiseProc",
                    string.Format("/command:diff /path:{0} /path2:{1} /notempfile", ProcessHelper.IncludeQoutes(newFile), ProcessHelper.IncludeQoutes(Path.Combine(oldFile))),
                    string.Empty, false, out tmp);
        }

        /// <summary>
        /// Start the SVN diff tool. This will launch the Commit dialog of SVN
        /// </summary>
        /// <param name="fileName"></param>
        public void Commit(IEnumerable<string> fileNames, string task)
        {
            string tmp;
            ProcessHelper.Execute("TortoiseProc", string.Format("/command:commit /path:{0} /logmsg:\"{1}\"", string.Join("*", fileNames), task), this.SourceFolder, false, out tmp);
        }

        /// <summary>
        /// Apply the path file. This will launch the Merge/Apply Patch dialog of SVN
        /// </summary>
        /// <param name="fileName"></param>
        public void ApplyPatch(string fileName)
        {
            ProcessHelper.Execute("TortoiseMerge", string.Format("/diff:\"{0}\"", fileName));
        }

        /// <summary>
        /// Show the Log (history) of the file
        /// </summary>
        /// <param name="fileName"></param>
        public void ShowLog(string fileName)
        {
            string tmp;
            ProcessHelper.Execute("TortoiseProc", string.Format("/command:log /path:{0} /notempfile", ProcessHelper.IncludeQoutes(fileName)), this.SourceFolder, false, out tmp);
        }

        /// <summary>
        /// This will create a patch file
        /// </summary>
        /// <param name="patchFile"></param>
        /// <param name="file"></param>
        public void CreatePatch(string patchFile, string file)
        {
            string output;
            ProcessHelper.Execute("svn", string.Format("diff \"{0}\"", file), this.SourceFolder, true, out output);

            using (StreamWriter w = new StreamWriter(patchFile, true))
            {
                w.Write(output);
            }
        }

        /// <summary>
        /// Ensure the target folder exists
        /// </summary>
        /// <param name="folder"></param>
        private void EnsureExists(string folder)
        {
            var d = Path.GetDirectoryName(folder);
            if (!Directory.Exists(d))
                Directory.CreateDirectory(d);
        }

        /// <summary>
        /// Exscute a SVN command, and parse the output in to single lines. (split by '\n')
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private IList<string> ExecuteSvnCmd(string arg)
        {
            string output;
            ProcessHelper.Execute("svn", arg, this.SourceFolder, true, out output);

            return output.Split(new char[] { '\n' });
        }

        /// <summary>
        /// Convert the state to enum FileModificationState
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private FileModificationState GetStateFrom(string value)
        {
            switch (value.ToUpper())
            {
                case "M":
                    return FileModificationState.Modified;
                case "A":
                    return FileModificationState.Added;
                case "D":
                    return FileModificationState.Deleted;
                case "C":
                    return FileModificationState.Conflicted;
                case "?":
                    return FileModificationState.NotVersioned;
                case "":
                    return FileModificationState.NotModified;
            }

            return FileModificationState.Ignored;
        }

        /// <summary>
        /// Conver the details string value to a enum FileDetailInfo
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private FileDetailInfo GetDetailFrom(string value)
        {
            switch (value.ToUpper())
            {
                case "PATH":
                    return FileDetailInfo.Path;
                case "NAME":
                    return FileDetailInfo.Name;
                case "REPOSITORY UUID":
                    return FileDetailInfo.RepositoryUUID;
                case "REVISION":
                    return FileDetailInfo.Revision;
                case "NODE KIND":
                    return FileDetailInfo.NodeKind;
                case "SCHEDULE":
                    return FileDetailInfo.Schedule;
                case "LAST CHANGED AUTHOR":
                    return FileDetailInfo.LastChangedAuthor;
                case "LAST CHANGED REV":
                    return FileDetailInfo.LastChangedRev;
                case "CHECKSUM":
                    return FileDetailInfo.Checksum;
                case "URL":
                    return FileDetailInfo.URL;
            }

            return FileDetailInfo.Unknown;
        }


        public bool CanApplyPath()
        {
            return true;
        }

        public bool CanCommit()
        {
            return true;
        }

        public bool CanRevertChanges()
        {
            return true;
        }
    }
}
