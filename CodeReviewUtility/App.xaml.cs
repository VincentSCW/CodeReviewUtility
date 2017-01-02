using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeReviewUtility.Controls;
using CodeReviewUtility.Properties;
using CodeReviewUtility.ViewModels;
using CodeReviewUtility.Views;
using Utility.Library.Misc;
using Utility.Library.UpdateManager;

namespace CodeReviewUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            //HelpProvider.HelpProvider.FileName = "Code Review Utility.chm";
            Application.Current.DispatcherUnhandledException += (o, ex) =>
            {
                var file = Path.Combine(Path.GetTempPath() + "CodeReviewUtilityError.txt");
                using (StreamWriter w = new StreamWriter(file))
                {
                    w.WriteLine(ex.Exception.Message);
                    w.WriteLine(ex.Exception.StackTrace);
                }

                MessageBox.Show(string.Format("UnhandledException occurred\nSee {0} for details.)", file), "Error");
            };

            MainWindowViewModel mainVM = new MainWindowViewModel();
            MainWindow mainWindow = new CodeReviewUtility.MainWindow();

            mainWindow.DataContext = mainVM;

            string downloadftp = Settings.Default.UpdatesLocation;
            if (e.Args.Length == 2 && string.Equals(e.Args[0], "-L", StringComparison.CurrentCultureIgnoreCase))
            {
                downloadftp = e.Args[1];
            }

            if (e.Args.Length == 1)
            {
                if (string.Equals(Path.GetExtension(e.Args[0]), Constants.ReviewFileExt, StringComparison.CurrentCultureIgnoreCase)
                    && File.Exists(e.Args[0]))
                {
                    mainWindow.ShowReviewOrCommit(e.Args[0]);
                }
                else
                {
                    mainWindow.ShowCreate(e.Args[0]);
                }
            }
            else
            {
                mainWindow.ShowCreate(string.Empty);
            }

            mainVM.UpdateManager = new UpdateManager(
                new UpdateManagerHost(mainWindow, e.Args.Length == 3 ? e.Args[2] : null),
#if !DEBUG
                downloadftp,
#else
                Directory.GetCurrentDirectory(),
#endif
                "Updates.xml",
                "CodeReviewUtility",
                System.Reflection.Assembly.GetExecutingAssembly());

            mainVM.FinishAction = () =>
            {
                if (mainVM.UpdateManager != null)
                    mainVM.UpdateManager.Stop();
            };

            mainWindow.Closing += (o, ee) =>
            {
                Settings.Default.Save();
                var model = ((MainWindowViewModel)MainWindow.DataContext).CodeReviewModel;
                if (model != null)
                    model.CleanUp();
                SystemIconsManager.Current.Dispose();

                this.Shutdown();
            };

            this.MainWindow.Tag = mainVM;
            mainWindow.Show();

#if !DEBUG
            if (e.Args.Length > 1 || Settings.Default.LastUpdateCheck == null || Settings.Default.LastUpdateCheck.Date < DateTime.Now.Date)
#endif
            {
                Settings.Default.LastUpdateCheck = DateTime.Now;
                Settings.Default.Save();

                mainVM.CheckForUpdates(true);
            }
        }
    }

    /// <summary>
    /// The host that uses the UpdateManager. This class/interface is use to allow the UpdateManager to make
    /// call backs;
    /// </summary>
    sealed class UpdateManagerHost : IUpdateManagerHost
    {
        private Window host;

        public string FtpCredentials { get; private set; }

        public UpdateManagerHost(Window host, string ftpCredentials)
        {
            this.host = host;
            this.FtpCredentials = ftpCredentials;
        }

        public void ShowInformationMessage(string message, string title)
        {
            this.Invoke(() => MessageBox.Show(this.host, message, title, MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public void ShowErrorMessage(string message, string title)
        {
            this.Invoke(() => MessageBox.Show(this.host, message, title, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        public void Invoke(Action action)
        {
            this.host.Dispatcher.Invoke((ThreadStart)delegate()
            {
                action();
            });
        }

        public void Shutdown()
        {
            this.Invoke(() => this.host.Close());
        }

        public ImageSource Icon
        {
            get
            {
                ImageSource s = null;
                this.Invoke(() =>
                {
                    s = this.host.Icon.Clone();
                });

                return s;
            }
        }
    }
}
