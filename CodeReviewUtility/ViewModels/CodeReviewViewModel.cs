using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CodeReviewUtility.Misc;
using CodeReviewUtility.Properties;
using CodeReviewUtility.Views;
using Utility.Interface;
using Utility.Library.BusinessObjects;
using Utility.Library.Commands;
using Utility.Library.UpdateManager;

namespace CodeReviewUtility.ViewModels
{
    internal abstract class CodeReviewViewModel : ViewModelBase
    {
        public ICompletionAwareCommand DoOkCmd { get; protected set; }
        public ICommand StartCmd { get; private set; }
        public ICommand HelpCmd { get; private set; }
        public ICommand DoDoubleClick { get; private set; }
        public ICommand ViewSelection { get; private set; }
        public ICommand AboutCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        protected Settings Settings { get; private set; }
        public ISourceControlManager SourceControlMgr { get; set; }

        public string SourceDirectory
        {
            get { return this.Get<string>("SourceDirectory"); }
            set
            {

                if (this.Set<string>("SourceDirectory", value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (this.StartCmd.CanExecute(null))
                            this.StartCmd.Execute(null);
                    }
                }
            }
        }

        public string GetShortDescription(string desciption)
        {
            if (string.IsNullOrEmpty(desciption))
                return "<No Desciption>";

            if (desciption.Length > 30)
                return string.Concat(desciption.Substring(0, 30), "...");

            return desciption;
        }

        public string TaskName
        {
            get { return this.Get<string>("TaskName"); }
            set { this.Set<string>("TaskName", value.Replace(".", "_")); }
        }

        public bool IsBusy
        {
            get { return this.Get<bool>("IsBusy"); }
            set { this.Set<bool>("IsBusy", value); }
        }

        public bool SendByEmail
        {
            get { return this.Get<bool>("SendByEmail"); }

            set
            {
                this.Set<bool>("SendByEmail", value);

                if (value)
                    this.CloseButtonText = Resources.Label_Send;
                else
                    this.CloseButtonText = Resources.Label_Ok;
            }
        }

        public string CloseButtonText
        {
            get { return this.Get<string>("CloseButtonText"); }
            set { this.Set<string>("CloseButtonText", value); }
        }

        public string TargetDirectory
        {
            get { return this.Get<string>("TargetDirectory"); }
            set { this.Set<string>("TargetDirectory", value); }
        }

        public ObservableCollection<FileViewModel> Files
        {
            get { return this.Get<ObservableCollection<FileViewModel>>("Files"); }
            set { this.Set<ObservableCollection<FileViewModel>>("Files", value); }
        }

        public ObservableCollection<FileViewModel> SelectedItems { get; set; }
        public FileViewModel SelectedItem
        {
            get { return this.Get<FileViewModel>("SelectedItem"); }
            set { this.Set<FileViewModel>("SelectedItem", value); }
        }

        protected bool HasSingleSelection
        {
            get { return this.SelectedItem != null && this.SelectedItems.Count == 1; }
        }

        protected OutlookHelper OutlookHelper { get; private set; }

        protected CodeReviewViewModel(Settings settings)
        {
            this.Settings = settings;
            this.SendByEmail = this.Settings.SendByEmail;
            this.Files = new ObservableCollection<FileViewModel>();
            this.SelectedItems = new ObservableCollection<FileViewModel>();
            this.OutlookHelper = OutlookHelper.Create();

            this.AboutCommand = new SimpleCommand<Window, Window>(
              (x) => { return true; },
              (x) =>
              {
                  new Utility.Library.Controls.AboutBox(Assembly.GetExecutingAssembly(), x.Icon.Clone()).ShowDialog();
              });

            this.DoOkCmd = new SimpleCommand<object, object>(
              (x) => { return this.CanFinish; },
              (x) =>
              {
                  Mouse.OverrideCursor = Cursors.Wait;
                  Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            this.Finish();
                        }
                        catch (System.Exception e)
                        {
                            var file = Path.Combine(Path.GetTempPath() + "CodeReviewUtilityError.txt");
                            using (StreamWriter w = new StreamWriter(file))
                            {
                                w.WriteLine(e.Message);
                                w.WriteLine(e.StackTrace);
                            }
                        }
                        finally
                        {
                            Mouse.OverrideCursor = Cursors.Arrow;
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
              },
              true);

            this.HelpCmd = new SimpleCommand<object, object>(
              (x) => { return !string.IsNullOrEmpty(HelpProvider.HelpProvider.FileName) && File.Exists(HelpProvider.HelpProvider.FileName); },
              (x) =>
              {
                  HelpProvider.HelpProvider.ShowIndex();
              });

            this.StartCmd = new SimpleCommand<object, object>(
              (x) => { return this.CanStart; },
              (x) =>
              {
                  Task.Factory.StartNew(() =>
                  {
                      StartNow();
                  }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
              });

            this.DoDoubleClick = new SimpleCommand<object, object>(
              (x) => { return this.CanDoDefaultDoubleClickAction; },
              (x) =>
              {
                  Task.Factory.StartNew(() =>
                  {
                      this.DoDefaultDoubleClickAction();
                  }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
              });

            this.ViewSelection = new SimpleCommand<object, object>(
              (x) => { return this.HasSingleSelection; },
              (x) =>
              {
                  Task.Factory.StartNew(() =>
                  {
                      // Start the child process.
                      Process p = new Process();

                      // Redirect the output stream of the child process.
                      p.StartInfo.UseShellExecute = true;
                      p.StartInfo.CreateNoWindow = true;
                      p.StartInfo.FileName = this.SelectedItem.FileName;
                      p.StartInfo.WorkingDirectory = this.SourceDirectory;
                      p.Start();

                  }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
              });

            this.SettingsCommand = new SimpleCommand<Window, Window>(
               (x) => { return true; },
               (x) =>
               {
                   SettingsViewModel data = new SettingsViewModel(this, Settings);
                   var dlg = new CodeCommentSettingsWindow() { DataContext = data, Icon = x.Icon.Clone() };
                   dlg.ShowDialog();
               });
        }

        protected void StartNow()
        {
            try
            {
                this.IsBusy = true;
                this.Start();
            }
            finally
            {

                this.IsBusy = false;
            }
        }

        protected static string ToYN(bool value)
        {
            return value ? "Yes" : "No";
        }

        public abstract void CleanUp();

        protected abstract bool CanFinish { get; }
        protected abstract void Finish();

        protected abstract bool CanStart { get; }
        protected abstract void Start();

        protected abstract bool CanDoDefaultDoubleClickAction { get; }
        protected abstract void DoDefaultDoubleClickAction();
    }
}
