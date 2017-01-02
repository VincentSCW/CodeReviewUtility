using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using CodeReviewUtility.Misc;
using CodeReviewUtility.Properties;
using Utility.Interface;
using Utility.Library.Commands;
using Utility.Library.Controls.XamlToHtmlParser;
using Utility.Library.Misc;
using Utility.Model;
using Utility.Library.Configuration;

namespace CodeReviewUtility.ViewModels
{ 
    internal class CreateCodeReviewViewModel : CodeReviewViewModel
    {
        private PropertyCollectionObserver<FileViewModel> observer;
        private bool updating;
        private FileWatcher FileWatcher;

        public PackageInfoModel PackageInfo { get; private set; }
       
        public ICommand CompareSelection { get; private set; }
        public ICommand ShowHistorySelection { get; private set; }
        public ICommand AddUnversionSelection { get; private set; }
        public ICommand RevertSelection { get; private set; }

        public bool ShowUnversionedFiles
        {
            get { return this.Get<bool>("ShowUnversionedFiles"); }
            set
            { 
                this.Set<bool>("ShowUnversionedFiles", value);
                if (this.StartCmd.CanExecute(null))
                    this.StartCmd.Execute(null);
            }
        }

        public List<string> SourceControlTools
        {
            get
            {
                return SourceControlMgrContainer.Instance.NameOfTools;
            }
        }

        public string SelectedTool
        {
            get
            {
                return this.Get<string>("SelectedTool");
            }
            set
            {
                this.Set<string>("SelectedTool", value);
                base.SourceControlMgr = SourceControlMgrContainer.Instance.GetSouceControlManager(value);
                if (this.StartCmd.CanExecute(null))
                    this.StartCmd.Execute(null);
            }
        }

        public bool IsCheckedAll
        {
            get
            {
                return this.Files == null || this.Files.Count == 0 ? false :
                    this.Files.All(f =>
                    {
                        if (!f.Enabled) return true;
                        return f.Checked;
                    });
            }
            set
            {
                if (this.Files != null)
                {
                    this.Files.ToList().ForEach(f =>
                    {
                        if (f.Enabled && f.Checked != value)
                            f.Checked = value;
                    });
                }
            }
        }

        public string Comments
        {
            get { return this.Get<string>("Comments"); }
            set { this.Set<string>("Comments", value); }
        }

        public string SelectionDetails
        {
            get { return this.Get<string>("SelectionDetails"); }
            set { this.Set<string>("SelectionDetails", value); }
        }

        public CreateCodeReviewViewModel(Settings settings)
            : base(settings)
        {
            this.PackageInfo = new PackageInfoModel();
            if (string.IsNullOrEmpty( this.Settings.StoreLocation))
            {
                SettingsViewModel m = new SettingsViewModel(this, settings);
                this.Settings.StoreLocation = m.StorePath;
            }
            this.TargetDirectory = this.Settings.StoreLocation;
            this.SourceDirectory = this.Settings.SourceLocation;

            if (string.IsNullOrEmpty(this.Settings.PreferredTool))
                this.SelectedTool = this.Settings.PreferredTool;
            else
                this.SelectedTool = SourceControlMgrContainer.Instance.NameOfTools.First();
          
            this.PropertyChanged += (o, e) =>
                {
                    // Set up a File/Directory Wachter so we can refresh the file when any changes happen
                    // while we are runing..
                    if (e.PropertyName == "SourceDirectory")
                    {
                        if (this.FileWatcher != null)
                            this.FileWatcher.Stop();

                        if (!string.IsNullOrEmpty(this.SourceDirectory))
                        {
                            this.FileWatcher = new FileWatcher(this.SourceDirectory);

                            this.FileWatcher.FileChanged += (wo, we) => this.Restart();
                            this.FileWatcher.FileRenamed += (wo, we) => this.Restart();
                        }
                    }
                };

            this.ShowHistorySelection = new SimpleCommand<object, object>(
                (x) => { return this.HasSingleSelection; },
                (x) => { this.SourceControlMgr.ShowLog(this.SelectedItem.FileName); });

            this.AddUnversionSelection = new SimpleCommand<object, object>(
                (x) => { return this.SelectedItem != null && this.SelectedItem.State == FileModificationState.NotVersioned; },
                (x) => { this.SourceControlMgr.Add(this.SelectedItem.FileName); });

            this.CompareSelection = new SimpleCommand<object, object>(
                (x) => { return this.DoDoubleClick.CanExecute(x);  },
                (x) => { this.DoDoubleClick.Execute(x); });

            this.RevertSelection = new SimpleCommand<object, object>(
              (x) => { return this.SelectedItems.Count > 0; },
              (x) => 
              {
                  try
                  {
                      this.FileWatcher.StopWatching();
                      this.SourceControlMgr.RevertChanges(this.SelectedItems.Where(s => s.State != FileModificationState.NotVersioned).Select(s => s.FileName));
                  }
                  finally
                  {
                      this.FileWatcher.StartWatching();
                      this.Restart();
                  }
              });

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CodeReviewUtility.Resources.ReviewCommentsOpeningPage.htm"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        this.Comments = reader.ReadToEnd();
                    }
                }
            });
        }

        protected override bool CanDoDefaultDoubleClickAction
        {
            get { return this.HasSingleSelection; }
        }

        protected override void DoDefaultDoubleClickAction()
        {
            this.SourceControlMgr.DiffWithBase(this.SelectedItem.FileName);
        }

        protected override bool CanFinish
        {
            get
            {
                return !string.IsNullOrEmpty(this.TargetDirectory)
                  && !string.IsNullOrEmpty(this.TaskName)
                  && this.TargetDirectory.IndexOfAny(Path.GetInvalidPathChars()) == -1;
            }
        }

        protected override void Finish()
        {
            if (this.TaskName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                // Make it valid
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    this.TaskName = this.TaskName.Replace(c, '_');
                }
            }

            this.CreateCodeReviewPackage(this.SourceDirectory, this.TargetDirectory, this.TaskName.Trim(), this.PackageInfo.Description);
            this.Settings.SourceLocation = this.SourceDirectory;
            this.Settings.StoreLocation = this.TargetDirectory;
            this.Settings.SendByEmail = this.SendByEmail;
        }

        protected override bool CanStart
        {
            get { return !string.IsNullOrEmpty(this.SourceDirectory) && Directory.Exists(this.SourceDirectory); }
        }

        protected override void Start()
        {           
            if(this.SourceDirectory != this.SourceControlMgr.SourceFolder)
                this.SourceControlMgr.SourceFolder = this.SourceDirectory;
            this.LocateModifications(this.SourceControlMgr);  
        }

        private void Restart()
        {
            this.SourceControlMgr.SourceFolder = null;
            Task.Factory.StartNew(() => this.StartNow());
        }

        private void CreateCodeReviewPackage(string source, string target, string taskname, string taskdesciption)
        {
            target = Path.Combine(target, taskname);

            FileSystem.DeleteFolder(target);
            FileSystem.CreateDirectory(target);
            
            string dest_after = Path.Combine(target, Constants.After);
            string dest_before = Path.Combine(target, Constants.Before);

            string patchFile = Path.Combine(target, Path.ChangeExtension(taskname, Constants.PatchExt));

            var t1 = Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(this.Files.Where(f => f.Checked && MatchState(f.State)), (file) =>
                    {
                        try
                        {
                            switch (file.State)
                            {
                                case FileModificationState.NotVersioned:
                                case FileModificationState.Added:
                                    {
                                        // file added. Copy to After
                                        this.Copy(Path.Combine(source, file.FileName), Path.Combine(dest_after, file.FileName));

                                    }
                                    break;

                                case FileModificationState.Deleted:
                                    {
                                        // file deleted. Copy to Before
                                        // Get the file details
                                        this.GetBeforeRevision(dest_before, file);
                                    }
                                    break;

                                case FileModificationState.Modified:
                                    {
                                        // Changed file.. 

                                        // Copy to the after
                                        // file added. Copy to After
                                        this.Copy(Path.Combine(source, file.FileName), Path.Combine(dest_after, file.FileName));
                                        this.GetBeforeRevision(dest_before, file);
                                    }
                                    break;
                            }
                        }
                        catch(Exception e)
                        {
                            var f = Path.Combine(Path.GetTempPath() + "CodeReviewUtilityError.txt");
                            using (StreamWriter w = new StreamWriter(f))
                            {
                                w.WriteLine(e.Message);
                                w.WriteLine(e.StackTrace);
                            }
                        }
                    });
                });

            var t2 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (var file in this.Files.Where(f => f.Checked && MatchState(f.State)))
                        {
                            if(file.State == FileModificationState.NotVersioned)
                                this.SourceControlMgr.Add(file.FileName);
                            this.SourceControlMgr.CreatePatch(patchFile, file.FileName);
                        };
                    }
                    catch (Exception e)
                    {
                        var f = Path.Combine(Path.GetTempPath() + "CodeReviewUtilityError.txt");
                        using (StreamWriter w = new StreamWriter(f))
                        {
                                w.WriteLine(e.Message);
                                w.WriteLine(e.StackTrace);
                         }
                    }
                });

            PackageInfo p = this.PackageInfo.ToModel();
            p.Author = this.OutlookHelper != null ? this.OutlookHelper.CurrentUserName : System.Environment.UserName;
            p.Description = taskdesciption;
            p.SourceFolder = this.SourceDirectory;
            p.SourceControlServerPath = this.SourceControlMgr.GetUrl();
            p.FileNames = this.Files.Select(f => f.FileName).ToList();

            XmlSerializer s = new XmlSerializer(typeof(PackageInfo));
            using (StreamWriter writter = new StreamWriter(Path.Combine(target, Constants.DesciptionFileName)))
            {
                using(XmlWriter w = new XmlTextWriter(writter))
                {
                    s.Serialize(w, p);
                }
            }

            // Zip it up..
            var zipTargetFile = Path.ChangeExtension(Path.Combine(this.TargetDirectory, taskname), Constants.ZipExt);
            var targetFile = Path.ChangeExtension(zipTargetFile, Constants.ReviewFileExt);

            FileSystem.DeleteFile(zipTargetFile);
            FileSystem.DeleteFile(targetFile);

            // wait until CreatePatch task are completed...
            Task.WaitAll(t1, t2);

            ZipUtil.CompressFolder(zipTargetFile, target);

            try
            {
                FileSystem.DeleteFile(targetFile);
                FileSystem.DeleteFile(Path.Combine(target, Constants.DesciptionFileName));
            }
            catch
            {
            }

            File.Move(zipTargetFile, targetFile);
            string body;
            using (Stream letterStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CodeReviewUtility.Resources.PleaseReviewPage.htm"))
            {
                using (StreamReader reader = new StreamReader(letterStream))
                {
                    body = reader.ReadToEnd()
                        .Replace("{_Task_Id_}", string.Format("{0} : {1}", this.PackageInfo.Synopsis, this.TaskName))
                        .Replace("{_Problem_Change_}", this.PackageInfo.Description)
                        .Replace("{_Cuase_}", this.PackageInfo.Cause)
                        .Replace("{_Solution_}", this.Comments)
                        .Replace("{_Impacts}", string.Format("Installation {0}, Documentation  {1}, ReadMe {2}",
                            ToYN(this.PackageInfo.InstallationImpacted), ToYN(this.PackageInfo.DocumentationImpacted), ToYN(this.PackageInfo.ReadMeImpacted)))
                        .Replace("{_New_Added_Tests}", this.PackageInfo.TestsAddedOrChanged)
                        .Replace("{_Tests_Run_}", this.PackageInfo.TestsRun)
                        .Replace("{_Tests_Pass_}", ToYN(this.PackageInfo.AllTestsPass));
                }
            }

            if (this.SendByEmail && this.OutlookHelper != null)
            {
                try
                {
                    body = body.Replace("{_Me_}", this.OutlookHelper.CurrentUserName);

                    this.OutlookHelper.CreateMessage(string.Format(Resources.Label_ActionCodeReview, taskname, taskdesciption), targetFile, body);
                }
                catch
                {
                    System.Windows.MessageBox.Show(Resources.Label_CannotCreateEmailMsg, Resources.Label_Error);
                }
            }
            else
            {
                body = body.Replace("{_Me_}", Environment.UserName);       

                // Convert to RTF an put on clipboard
                var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(body, false);

                var document = new FlowDocument();
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                {
                    tr.Load(ms, DataFormats.Xaml);
                }

                tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream())
                {
                    tr.Save(ms, DataFormats.Rtf);
                    body = System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray());
                }
                Clipboard.SetData(System.Windows.DataFormats.Rtf, body);

                ProcessHelper.Execute(Path.Combine(Environment.ExpandEnvironmentVariables("%SystemRoot%"), "Explorer.exe"), this.TargetDirectory);
            }

            // Clean up
            FileSystem.DeleteFolder(target);
        }

        private void GetBeforeRevision(string dest_before, FileViewModel file)
        {
            // Get the file details
            var info_details = this.SourceControlMgr.GetFileInfoDetails(file.FileName);
            System.Diagnostics.Debug.Assert(info_details != null);

            string revision = null;
            info_details.TryGetValue(FileDetailInfo.Revision, out revision);
            // Get the Before version
            this.SourceControlMgr.GetFile(revision, file.FileName, Path.Combine(dest_before, file.FileName));
        }

        public override void CleanUp()
        {
            // Nothing to do...
        }

        private void Copy(string source, string target)
        {
            FileSystem.CreateDirectory(Path.GetDirectoryName(target));

            if (!FileSystem.IsDirectory(source))
                File.Copy(source, target, true);
        }

        private bool MatchState(FileModificationState state)
        {            
            bool match = state == FileModificationState.Modified || state == FileModificationState.Added || state == FileModificationState.Deleted;
            if (!match && this.ShowUnversionedFiles)
                match = state == FileModificationState.NotVersioned;

            return match;
        }

        private void LocateModifications(ISourceControlManager mgr)
        {
            if (this.observer != null)
                this.observer.UnregisterHandler(n => n.Checked);

            this.Files = new ObservableCollection<FileViewModel>(mgr.GetFiles().Select(file => new FileViewModel(file.State, file.FileName)).Where(f => MatchState(f.State)));
            this.UpdateSelectionDetails(null);

            this.observer = new PropertyCollectionObserver<FileViewModel>(this.Files)
                 .RegisterHandler(n => n.Checked, this.UpdateSelectionDetails);
        }

        private void UpdateSelectionDetails(FileViewModel n)
        {
            this.NotifyPropertyChanged("IsCheckedAll");

            if (!this.updating)
            {
                try
                {
                    this.updating = true;
                    if (n != null)
                    {
                        foreach (var f in this.SelectedItems.Where(f => f != n && f.Checked != n.Checked))
                        {
                            f.Checked = n.Checked;
                        }
                    }
                }
                finally
                {
                    this.updating = false;
                }

                this.SelectionDetails = string.Format(Resources.Label_SelectionOverview, this.Files.Count((f) => { return f.Checked; }), this.Files.Count());
            }
        }
    }
   
}
