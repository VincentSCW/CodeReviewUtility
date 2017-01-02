using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Library.BusinessObjects;
using Utility.Library.Commands;
using System.Windows.Input;
using CodeReviewUtility.Properties;

namespace CodeReviewUtility.ViewModels
{
    public class ReviewOrCommitViewModel : ViewModelBase
    {
        public ICompletionAwareCommand DoOkCmd { get; protected set; }
        public ICommand HelpCmd { get; private set; }

        public bool? DialogResult
        {
            get { return this.Get<bool?>("DialogResult"); }
            set { this.Set<bool?>("DialogResult", value); }
        }

        public string Header { get; private set; }

        public bool CommitChanges
        {
            get { return this.Get<bool>("CommitChanges"); }
            set { this.Set<bool>("CommitChanges", value); }
        }

        public ReviewOrCommitViewModel(string taskName)
        {
            this.Header = string.Format(Resources.Label_SelectOpeningAction, taskName);
            this.CommitChanges = false;
            this.DoOkCmd = new SimpleCommand<object, object>(
                  (x) => { return true; },
                  (x) =>
                  {
                      this.DialogResult = true;
                  });

            this.HelpCmd = new SimpleCommand<object, object>(
              (x) => { return false; },
              (x) =>
              {
                  // TODO = Define help..
              });
        }
    }
}
