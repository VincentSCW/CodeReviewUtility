using System;
using System.IO;
using System.Windows.Input;
using CodeReviewUtility.Properties;
using Utility.Library.BusinessObjects;
using Utility.Library.Commands;

namespace CodeReviewUtility.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        public bool? DialogResult
        {
            get { return this.Get<bool?>("DialogResult"); }
            set { this.Set<bool?>("DialogResult", value); }
        }

        public ICompletionAwareCommand DoOkCmd { get; protected set; }
        public ICommand HelpCmd { get; private set; }

        public bool UseSVNComparer
        {
            get { return this.Get<bool>("UseSVNComparer"); }
            set { this.Set<bool>("UseSVNComparer", value); }
        }
        public string AlternateComparerPath
        {
            get { return this.Get<string>("AlternateComparerPath"); }
            set { this.Set<string>("AlternateComparerPath", value); }
        }

        public string StorePath
        {
            get { return this.Get<string>("StorePath"); }
            set { this.Set<string>("StorePath", value); }
        }

        public bool ShowCheckListAtStartup
        {
            get { return this.Get<bool>("ShowCheckListAtStartup"); }
            set { this.Set<bool>("ShowCheckListAtStartup", value); }
        }

        public SettingsViewModel(CodeReviewViewModel model, Settings settings)
        {
            this.UseSVNComparer = settings.UseSVNComparer;
            this.AlternateComparerPath = settings.AlternateComparerPath;
            this.StorePath = settings.StoreLocation;
         
            if (string.IsNullOrEmpty(this.StorePath))
            {
                this.StorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AVIAGE\\CodeReviewUtility Store");
            }

            this.DoOkCmd = new SimpleCommand<object, object>(
              (x) => { return this.UseSVNComparer || !string.IsNullOrEmpty(this.AlternateComparerPath); },
              (x) =>
              {
                  settings.UseSVNComparer = this.UseSVNComparer;
                  settings.AlternateComparerPath = this.AlternateComparerPath;
                  settings.StoreLocation = this.StorePath;

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
