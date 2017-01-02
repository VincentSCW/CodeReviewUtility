using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeReviewUtility.Properties;
using System.IO;
using System.Reflection;
using Utility.Library.BusinessObjects;
using Utility.Library.UpdateManager;
using System.Windows.Input;
using Utility.Library.Commands;

namespace CodeReviewUtility.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public CodeReviewViewModel CodeReviewModel
        {
            get { return base.Get<CodeReviewViewModel>("CodeReviewModel"); }
            set { base.Set<CodeReviewViewModel>("CodeReviewModel", value); }
        }

        public Action FinishAction { get; set; }
        public UpdateManager UpdateManager { get; set; }
        public ICommand CheckForUpdatesCommand { get; private set; }

        public MainWindowViewModel()
        {
            this.CheckForUpdatesCommand = new SimpleCommand<object, object>(
              (x) => { return this.UpdateManager == null || !this.UpdateManager.InProgress; },
              (x) =>
              {
                  this.CheckForUpdates(false);
              });
        }

        public void CheckForUpdates(bool silent)
        {
            this.UpdateManager.Check(silent);
        }
    }
}
