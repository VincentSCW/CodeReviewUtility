using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Utility.Library.Misc;
using System.Reflection;
using System.Windows.Media;

namespace Utility.Library.UpdateManager
{
    public interface IUpdateManagerHost
    {
       void Invoke(Action a);

        void ShowInformationMessage(string message, string title);
        void ShowErrorMessage(string message, string title);
        void Shutdown();
        ImageSource Icon { get;}

        string FtpCredentials { get; }
    }

    /// <summary>
    /// Update Manager class. this will
    /// - Check for the new version (downloading the xml file to get the latest version that is available
    /// - Download the installer
    /// - Close the current running App, Start the installer, and Restart the App
    /// 
    /// 
    /// the Update,xml file will look like
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <Updates>
    ///     <CodeReviewUtility>
	///         <Version>1.0.1.0</Version>
	///         <Url></Url>
	///         <Installer>ftp://129.221.31.86/ftp/Code Review Utility Installer/CodeReviewTask.msi</Installer>
	///         <Date>07/06/2012</Date>
    ///     </CodeReviewUtility>
    /// </Updates>
    /// 
    /// The file can contain multiple Update sections for different Application. The appKey on the constructer is used to identify which section will be read
    /// In Installer path can be a ftp server or html server.
    /// </summary>
    public class UpdateManager
    {
        enum UpdateState
        {
            Aborted,
            Error,
            Updated,
            NoUpdateNeeded
        }

        // this class will contain the info from the xml file
        class DownloadedVersionInfo
        {
            public Version LatestVersion { get; set; }
            public Uri InstallerUrl { get; set; }
            public string HomeUrl { get; set; }
            public string Description { get; set; } 
        }

        private string location;
        private string updatefile; 
        private string appKey;
        private Assembly caller;

        // events used to stop worker thread
        private readonly ManualResetEvent EventStop;
        private readonly ManualResetEvent EventStopped;
        private Task updateTask;
        private IUpdateManagerHost host;

        public UpdateManager(IUpdateManagerHost host, string location, string updatefile, string appKey, Assembly caller)
        {
            this.host = host;
            this.location = location;
            this.updatefile = updatefile;
            this.caller = caller;
            this.appKey = appKey;

            this.EventStop = new ManualResetEvent(false);
            this.EventStopped = new ManualResetEvent(false);
        }

        /// <summary>
        /// Answer is we in progress of checking of or.
        /// </summary>
        public bool InProgress { get; private set;}

        /// <summary>
        /// Check for available updates. Will run in a thread so caller is not blocked.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="silent"></param>
        public void Check(bool silent = true)
        {
            if ((this.updateTask == null) || (this.updateTask.IsCompleted))
            {
                this.EventStop.Reset();
                this.EventStopped.Reset();

                this.updateTask = Task.Factory.StartNew(() =>
                    {
                        this.InProgress = true;
                        var result = this.CheckForUpdates();
                        if (!silent)
                        {
                            if (result == UpdateState.NoUpdateNeeded)
                            {
                                this.host.ShowInformationMessage("Your version is up to date", AssemblyHelper.GetTitle(caller));
                            }
                        }
                        this.InProgress = false;
                    });
            }
        }

        /// <summary>
        /// Inform that we want to stop checking
        /// When the worker thread is running - let it know it should stop
        /// </summary>
        public void Stop()
        {
            if ((this.updateTask != null) && !this.updateTask.IsCompleted)
            {
                this.EventStop.Set();

                while (!this.updateTask.IsCompleted)
                {
                    if (WaitHandle.WaitAll(
                              (new ManualResetEvent[] { this.EventStopped }),
                               100,
                               true))
                    {
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// this is run in a thread. do the whole updating process:
        /// 
        /// - check for the new version (downloading the xml file)
        /// - download the installer, run the installer, close and restart the application
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private UpdateState CheckForUpdates()
        {
            DownloadedVersionInfo versionInfo = null;
            try
            {
                versionInfo = this.GetDownLoadVersionInfo();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            if (StopWorkerThread())
                return UpdateState.Aborted;

            if (versionInfo == null || versionInfo.InstallerUrl == null || versionInfo.LatestVersion == null)
                return UpdateState.Error;

            // compare the current version with the downloaded version number
            if (this.caller.GetName().Version.CompareTo(versionInfo.LatestVersion) >= 0)
            {
                // no new version
                return UpdateState.NoUpdateNeeded;
            }

            UpdateState result = UpdateState.Aborted;

            // new version found, ask the user if they want to download/install the new package
            this.host.Invoke(() =>
            {
                UpdateWindow up = null;
                UpdateWindowDataModel context = null;

                context = new UpdateWindowDataModel(this.caller, versionInfo.LatestVersion, versionInfo.Description.Trim(), host.Icon.Clone(), (upgrade) =>
                    {
                        if (upgrade)
                        {
                            FileDownLoader.DownLoad(null, Path.GetTempPath(), true, versionInfo.InstallerUrl);
                            string filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(versionInfo.InstallerUrl.LocalPath));
 
                              if (File.Exists(filePath))
                            {
                                context.Progress = "Installing...";
                                if (!StopWorkerThread())
                                {
                                    this.RunInstaller(filePath);
                                    Thread.Sleep(500);
                                }
                            }

                            result = UpdateState.Updated;
                        }
                        up.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate()
                        {
                            up.Close();
                        });
                    });

                up = new UpdateWindow(context) { ShowInTaskbar = false };
                up.Show();
            });

            return result;
        }

        /// <summary>
        /// Get the information that contain the down load information, like version number and location of the new package 
        /// </summary>
        /// <returns></returns>
        private DownloadedVersionInfo GetDownLoadVersionInfo()
        {
            DownloadedVersionInfo versionInfo = new DownloadedVersionInfo();

            FileDownLoader.DownLoad(this.host.FtpCredentials, Path.GetTempPath(), true, new Uri(Path.Combine(this.location, this.updatefile)));

            string filePath = Path.Combine(Path.GetTempPath(), this.updatefile);
            if (File.Exists(filePath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode node = doc.SelectSingleNode(string.Format("/Updates/{0}/Version", this.appKey));
                if (node != null)
                    versionInfo.LatestVersion = new Version(node.InnerText);

                node = doc.SelectSingleNode(string.Format("/Updates/{0}/Url", this.appKey));
                if (node != null)
                    versionInfo.HomeUrl = node.InnerText;

                node = doc.SelectSingleNode(string.Format("/Updates/{0}/Installer", this.appKey));
                if (node != null)
                    versionInfo.InstallerUrl = new Uri(node.InnerText);

                node = doc.SelectSingleNode(string.Format("/Updates/{0}/Description", this.appKey));
                if (node != null)
                    versionInfo.Description = node.InnerText;
            }

            return versionInfo;
        }

        // internal method - return true when the thread is supposed to stop
        private bool StopWorkerThread()
        {
            if (this.EventStopped.WaitOne(0, true))
            {
                this.EventStopped.Set();
                return true;
            }
            return false;
        }


        // called after the checkForUpdate object downloaded the installer
        private void RunInstaller(string installer)
        {
            if (string.IsNullOrEmpty(installer))
            {
                this.host.ShowErrorMessage("Error while downloading the installer", "Check for updates");
                return;
            }

            ManualResetEvent done = new ManualResetEvent(false);
            // run the installer and exit the app
            try
            {
                var startInfo = Process.GetCurrentProcess().StartInfo;
                var cmd = this.caller.Location;

                // Create a batch file so we can restart  the app...
                string cmdFile = FileSystem.GetInTemp("updater.cmd");

                using(StreamWriter r = new StreamWriter(cmdFile, false))
                {
                    r.WriteLine("@echo off");
                    r.WriteLine("cd {0}", Path.GetDirectoryName(installer));
                    r.WriteLine("start /wait {0}", Path.GetFileName(installer));
                    r.WriteLine("del /q  {0}", Path.GetFileName(installer));

                    r.WriteLine("cd {0}", Path.GetDirectoryName(cmd));
                    r.WriteLine("start {0} {1}", Path.GetFileName(cmd), startInfo.Arguments);

                    r.WriteLine("cd {0}", Path.GetTempPath());
                    r.WriteLine("del /q {0}", Path.GetFileName(cmdFile));
                }

                var p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = cmdFile;
                p.Start();

                this.host.Shutdown();

            }
            catch (Exception)
            {
                this.host.ShowErrorMessage("Error while running the installer.", "Check for updates");
                FileSystem.DeleteFile(installer);
            }
        }
    }
}
