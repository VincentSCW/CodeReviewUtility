using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Interface
{
    public interface ISourceControlManager
    {
        string Name { get; }

        string SourceFolder { get; set; }

        bool IsAvaliable { get; }

        void ApplyPatch(string fileName);

        void Commit(IEnumerable<string> fileNames, string task);

        void CreatePatch(string patchFile, string file);

        void Diff(string newFile, string oldFile);

        void Add(IEnumerable<string> sources);

        void Add(string filename);

        void DiffWithBase(string fileName);

        void GetFile(string revision, string source, string desination);

        IList<Model.FileModel> GetFiles();

        IDictionary<Model.FileDetailInfo, string> GetFileInfoDetails(string fileName);

        string GetUrl();

        void ShowLog(string fileName);

        void RevertChanges(IEnumerable<string> sources);

        void RevertChanges(string filename);

        bool CanApplyPath();

        bool CanCommit();

        bool CanRevertChanges();
    }
}
