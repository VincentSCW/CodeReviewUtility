using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Interface;
using Utility.Model;
using System.IO;

namespace Utility.Git
{
    public class GitManager : ISourceControlManager
    {
        public string Name
        {
            get { return "Git"; }
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

        public bool IsAvaliable
        {
            get
            {
                string path = System.Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                return path.Split(';').Any((p) => File.Exists(Path.Combine(p, "git.exe")));
            }
        }

        public void ApplyPatch(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Commit(IEnumerable<string> fileNames, string task)
        {
            throw new NotImplementedException();
        }

        public void CreatePatch(string patchFile, string file)
        {
            throw new NotImplementedException();
        }

        public void Diff(string newFile, string oldFile)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<string> sources)
        {
            throw new NotImplementedException();
        }

        public void Add(string filename)
        {
            throw new NotImplementedException();
        }

        public void DiffWithBase(string fileName)
        {
            throw new NotImplementedException();
        }

        public void GetFile(string revision, string source, string desination)
        {
            throw new NotImplementedException();
        }

        public IList<Model.FileModel> GetFiles()
        {
            if (files == null)
            {
                var lines = GitHelper.ExecuteCmd("status -s --ignored", this.SourceFolder);
                List<Model.FileModel> tmp = new List<Model.FileModel>();
                lines.ToList().ForEach(l =>
                    {
                        if (l.Length > 2)
                        {
                            tmp.Add(ToFileMode(l.Substring(0, 2), l.Substring(2).Trim()));
                        }
                    });
                files = tmp
                    .OrderBy(s => s.State).ThenBy(s => s.FileName)
                    .ToList();
            }

            return files;
        }

        public IDictionary<Model.FileDetailInfo, string> GetFileInfoDetails(string fileName)
        {
            throw new NotImplementedException();
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }

        public void ShowLog(string fileName)
        {
            throw new NotImplementedException();
        }

        public void RevertChanges(IEnumerable<string> sources)
        {
            throw new NotImplementedException();
        }

        public void RevertChanges(string filename)
        {
            throw new NotImplementedException();
        }

        public bool CanApplyPath()
        {
            throw new NotImplementedException();
        }

        public bool CanCommit()
        {
            throw new NotImplementedException();
        }

        public bool CanRevertChanges()
        {
            throw new NotImplementedException();
        }

        private FileModel ToFileMode(string status, string file)
        {
            switch (status)
            {
                case "??":
                    return new FileModel(FileModificationState.NotVersioned, file);
                case "!!":
                    return new FileModel(FileModificationState.Ignored, file);
                default:
                    return new FileModel(FileModificationState.NotModified, file);
            }
        }
    }
}
