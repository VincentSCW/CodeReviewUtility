using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;
using CodeReviewUtility.Properties;
using Utility.Library.Commands;
using Utility.Library.Controls.XamlToHtmlParser;
using Utility.Library.Misc;
using Utility.Model;

namespace CodeReviewUtility.ViewModels
{
    internal class ReviewAndCommitViewModel : CodeReviewViewModel
    {
        public ICommand CompareSelection { get; private set; }
        public ICommand CopyName { get; private set; }
        public ICommand ApplyPatchCmd { get; private set; }
     
        private string beforePath;
        private string afterPath;

        private dynamic replyTo;
        private string zipTargetFile;

        public string PackageDiscription
        {
            get { return this.Get<string>("PackageDiscription"); }
            set { this.Set<string>("PackageDiscription", value); }
        }

        public string CommitComments
        {
            get { return this.Get<string>("CommitComments"); }
            set { this.Set<string>("CommitComments", value); }
        }

        public string Description { get { return this.PackageInfo != null ? this.PackageInfo.Description : string.Empty; } }

        public PackageInfo PackageInfo { get; private set; }
        public PackageInfoModel Package { get; private set; }

        private static string ResourceName = "CommitTemplate.txt";

        public int SelectedTabIndex
        {
            get { return this.Get<int>("SelectedTabIndex"); }
            set { this.Set<int>("SelectedTabIndex", value); }
        }

        public ReviewAndCommitViewModel(Settings settings)
            : base(settings)
        {
            this.CompareSelection = new SimpleCommand<object, object>(
              (x) => { return this.DoDoubleClick.CanExecute(x); },
              (x) => { this.DoDoubleClick.Execute(x); });

            this.CopyName = new SimpleCommand<object, object>(
               (x) => { return this.SelectedItem != null; },
               (x) => { Clipboard.SetText(this.SelectedItem.FileName); });

            this.ApplyPatchCmd = new SimpleCommand<object, object>(
              (x) => { return this.SourceControlMgr.IsAvaliable && this.TargetDirectory != null && this.TaskName != null && File.Exists(Path.Combine(this.TargetDirectory, Path.ChangeExtension(this.TaskName, Constants.PatchExt))); },
              (x) => { this.SourceControlMgr.ApplyPatch(Path.Combine(this.TargetDirectory, Path.ChangeExtension(this.TaskName, Constants.PatchExt))); });

            base.SourceControlMgr = Utility.Library.Configuration.SourceControlMgrContainer.Instance.GetSouceControlManager("Git"); //TODO
            this.Package = new PackageInfoModel();
        }

        protected override bool CanDoDefaultDoubleClickAction
        {
            get { return this.HasSingleSelection; }
        }

        protected override void DoDefaultDoubleClickAction()
        {
            if (SelectedTabIndex == 0)
            {
                string tmp;

                if (Settings.UseSVNComparer && this.SourceControlMgr.IsAvaliable)
                {
                    this.SourceControlMgr.Diff(Path.Combine(this.afterPath, this.SelectedItem.FileName), Path.Combine(this.beforePath, this.SelectedItem.FileName));
                }
                else if (!string.IsNullOrEmpty(Settings.AlternateComparerPath))
                {
                    // Fire the Alternate comparer
                    ProcessHelper.Execute(ProcessHelper.IncludeQoutes(Settings.AlternateComparerPath),
                         string.Format("{0} {1}", ProcessHelper.IncludeQoutes(Path.Combine(this.beforePath, this.SelectedItem.FileName)), ProcessHelper.IncludeQoutes(Path.Combine(this.afterPath, this.SelectedItem.FileName))),
                            string.Empty, false, out tmp);
                }
                else
                {
                    MessageBox.Show(Resources.Label_Error_NoComparer, Resources.Label_Error);
                }
            }
            else
            {
                this.SourceControlMgr.DiffWithBase(this.SelectedItem.FileName);
            }
        }

        protected override bool CanFinish
        {
            get { return true; }
        }

        protected override void Finish()
        {
            if (SelectedTabIndex == 0)
            {
                if (this.SendByEmail & this.OutlookHelper != null)
                {
                    this.OutlookHelper.CreateMessage(replyTo, string.Format(Resources.Label_ActionCodeReviewComments, this.TaskName, this.GetShortDescription(this.PackageInfo.Description)), null, this.PackageDiscription);
                }
                else
                {
                    // Convert to RTF an put on clipboard
                    var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(this.PackageDiscription, false);

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
                        this.PackageDiscription = System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray());
                    }
                    Clipboard.SetData(System.Windows.DataFormats.Rtf, this.PackageDiscription);
                }
            }
            else
            {
                this.SourceControlMgr.Commit(this.PackageInfo.FileNames, GetCheckInTextBody());
            }
        }

        public override void CleanUp()
        {
            // Clean up
            FileSystem.DeleteFolder(this.TargetDirectory);
            FileSystem.DeleteFile(this.zipTargetFile);
        }

        protected override bool CanStart
        {
            get { return !string.IsNullOrEmpty(this.SourceDirectory) && File.Exists(this.SourceDirectory); }
        }

        protected override void Start()
        {
            this.zipTargetFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetFileName(this.SourceDirectory)), Constants.ZipExt);

            this.TaskName = Path.GetFileNameWithoutExtension(this.zipTargetFile);
            this.TargetDirectory = Path.Combine(Path.GetDirectoryName(this.zipTargetFile), this.TaskName);

            FileSystem.DeleteFile(this.zipTargetFile);
            FileSystem.DeleteFolder(this.TargetDirectory);

            File.Copy(this.SourceDirectory, this.zipTargetFile, true);
            ZipUtil.UnCompressFolder(zipTargetFile, Path.GetDirectoryName(this.zipTargetFile));

            var desc = Path.Combine(this.TargetDirectory, Constants.DesciptionFileName);
            if (File.Exists(desc))
            {               
                XmlSerializer s = new XmlSerializer(typeof(PackageInfo));
                using (StreamReader writter = new StreamReader(desc))
                {
                    this.PackageInfo = s.Deserialize(writter) as PackageInfo;
                    this.NotifyPropertyChanged("Description");
                 }
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.beforePath = Path.Combine(this.TargetDirectory, Constants.Before);
                this.afterPath = Path.Combine(this.TargetDirectory, Constants.After);

                var beforeFilesNames = Directory.Exists(this.beforePath) ? Directory.EnumerateFiles(this.beforePath, "*", SearchOption.AllDirectories).ToList() : new List<string>();
                var afterFilesNames = Directory.Exists(this.afterPath) ? Directory.EnumerateFiles(this.afterPath, "*", SearchOption.AllDirectories).ToList() : new List<string>();

                var beforeNames = beforeFilesNames.Select(f =>
                        {
                            return f.Substring(this.beforePath.Length + 1);
                        }).ToList();

                var items = afterFilesNames.Select(f =>
                        {
                            var relitivepath = f.Substring(this.afterPath.Length + 1);

                            FileModificationState state = FileModificationState.Deleted;
                            if (beforeNames.Contains(relitivepath))
                                state = FileModificationState.Modified;
                            else
                                state = FileModificationState.Added;

                            return new DiffFileViewModel(state, relitivepath)
                                {
                                    Image = SystemIconsManager.Current.GetSmallIcon(Path.Combine(this.afterPath, relitivepath))
                                };

                        }).ToList();

                items.AddRange(
                    beforeFilesNames
                        .Where(f =>
                            {
                                var relitivepath = f.Substring(this.beforePath.Length + 1);
                                return !afterFilesNames.Contains(Path.Combine(this.afterPath, relitivepath));
                            })
                        .Select(f =>
                        {
                            var relitivepath = f.Substring(this.beforePath.Length + 1);
                            return new DiffFileViewModel(FileModificationState.Deleted, relitivepath)
                                {
                                    Image = SystemIconsManager.Current.GetSmallIcon(Path.Combine(this.beforePath, relitivepath))
                                };

                        })
                );

                this.Files = new ObservableCollection<FileViewModel>(items);
            });

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.PackageDiscription = GetCommentsBody();
            });

            // For commit
            this.Package.FromModel(this.PackageInfo);

            if (this.PackageInfo.SourceFolder != this.SourceControlMgr.SourceFolder)
                this.SourceControlMgr.SourceFolder = this.PackageInfo.SourceFolder;
        }

        protected string GetCommentsBody()
        {
            if (SelectedTabIndex == 0)
            {
                if (this.SendByEmail && this.OutlookHelper != null)
                {
                    try
                    {
                        this.replyTo = OutlookHelper.FindMessage(this.TaskName, this.PackageInfo == null ? string.Empty : this.GetShortDescription(this.PackageInfo.Description));
                    }
                    catch
                    {
                    }
                }

                string body;
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CodeReviewUtility.Resources.ReviewCommentsPage.htm"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        body = reader.ReadToEnd();
                    }
                }

                string to = replyTo != null ? replyTo.To : (this.PackageInfo == null ? Resources.Label_Unknow : this.PackageInfo.Author);
                if (string.IsNullOrEmpty(to))
                    to = Resources.Label_ToDo;

                if (to.Contains(","))
                {
                    to = to.Split(',')[1].Trim();
                }
                if (to.Contains(' '))
                {
                    to = to.Split(' ')[1].Trim();
                }

                return body.Replace("{0}", to);
            }
            else
            {
                return string.Format(Resources.Label_ReviewCommitBody, Environment.NewLine, this.PackageInfo.Description);
            }
        }

        public string GetCheckInTextBody()
        {
            string text;
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat("CodeReviewUtility.Resources.", ResourceName))))
            {
                text = reader.ReadToEnd()
                    .Replace("{Tab}", "\t")
                    .Replace("{Synopsis}", this.Package.Synopsis)
                    .Replace("{TaskInfo}", string.Format("{0} - {1}", this.TaskName, this.Package.Description))
                    .Replace("{Cause}", this.Package.Cause)
                    .Replace("{Reviewers}", this.Package.Reviewers)
                    .Replace("{SourceControlUrl}", this.Package.SourceControlUrl)
                    .Replace("{SolutionBuilds}", ToYN(this.Package.SolutionBuilds))
                    .Replace("{InstallationImpacted}", ToYN(this.Package.InstallationImpacted))
                    .Replace("{DocumentationImpacted}", ToYN(this.Package.DocumentationImpacted))
                    .Replace("{ReadMeImpacted}", ToYN(this.Package.ReadMeImpacted))
                    .Replace("{TestsAddedOrChanged}", this.Package.TestsAddedOrChanged)
                    .Replace("{TestsRun}", this.Package.TestsRun)
                    .Replace("{AllTestsPass}", ToYN(this.Package.AllTestsPass))
                    .Replace("{Comments}", this.CommitComments);
            }
            return text;
        }
    }
}
