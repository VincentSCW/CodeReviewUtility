using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Model
{
    public class FileModel
    {
        public FileModel(FileModificationState state, string name)
        {
            this.State = state;
            this.FileName = name;
        }

        public string FileName { get; set; }
        public FileModificationState State { get; set; }
    }
}
