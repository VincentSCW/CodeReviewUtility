using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utility.Library.BusinessObjects;
using Utility.Model;

namespace CodeReviewUtility.ViewModels
{
    /// <summary>
    /// Class to display a SVN file in a View
    /// </summary>
    internal class FileViewModel : ViewModelBase
    {
        public string FileName
        {
            get { return this.Get<string>("FileName"); }
            set { this.Set<string>("FileName", value); }
        }

        public FileModificationState State
        {
            get { return this.Get<FileModificationState>("State"); }
            set { this.Set<FileModificationState>("State", value); }
        }

        public bool Checked
        {
            get { return this.Get<bool>("Checked"); }
            set { this.Set<bool>("Checked", value); }
        }

        public bool Enabled
        {
            get { return this.Get<bool>("Enabled"); }
            set { this.Set<bool>("Enabled", value); }
        }

        public ImageSource Image { get; set; }

        public FileViewModel(FileModificationState state, string fileName)
        {
            this.Enabled = true;
            this.Checked = state != FileModificationState.NotVersioned;

            this.FileName = fileName;
            this.State = state;
        }
    }

    internal class DiffFileViewModel : FileViewModel
    {      
        public DiffFileViewModel(FileModificationState state, string fileName)
            : base(state, fileName)
        {
        }
    }
}
