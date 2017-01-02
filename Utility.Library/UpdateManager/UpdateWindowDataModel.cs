using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Utility.Library.BusinessObjects;
using Utility.Library.Commands;
using System.Windows.Media;
using Utility.Library.Misc;

namespace Utility.Library.UpdateManager
{
    public class UpdateWindowDataModel : ViewModelBase
    {
        public ICommand UpdateCmd { get; private set; }
        public ICommand UpdateNotNowCmd { get; private set; }

        private Action<bool> UpdateAction { get; set; }
        public string AppName { get; private set; }

        public string Title { get; private set; }
        public string Info { get; private set; }
        public ImageSource Icon { get; private set; }
        public Version Version { get; private set; }

        public bool InProgress
        {
            get { return this.Get<bool>("InProgress"); }
            set { this.Set<bool>("InProgress", value); }
        }

        public string Progress
        {
            get { return this.Get<string>("Progress"); }
            set { this.Set<string>("Progress", value); }
        }

        public UpdateWindowDataModel(Assembly caller, Version newVersion, string updates, ImageSource icon, Action<bool> updateAction)
        {
            this.AppName = AssemblyHelper.GetTitle(caller);
            this.Icon = icon;
            this.InProgress = false;
            this.Progress = string.Empty;

            this.Version = newVersion; 
            this.Title = string.Format("{0} Update", this.AppName);
            this.Info = string.Format("An update for {0} is available.\nYour version: {1}. New version: {2}.\n\nUpdates:\n{3}.\n\nIt is recommended that you install it as soon as possible.\nThe application will be restarted after the upgrade.",
                this.AppName,
                caller.GetName().Version,
                newVersion,
                updates);

            this.UpdateAction = updateAction;

            this.UpdateCmd = new SimpleCommand<object, object>(
                (x) => { return this.UpdateAction != null && !this.InProgress; },
                (x) =>
                {
                    this.InProgress = true;
                    Task.Factory.StartNew(() => this.UpdateAction(true));
                });
            this.UpdateNotNowCmd = new SimpleCommand<object, object>(
                (x) => { return this.UpdateAction != null && !this.InProgress; },
                (x) =>
                {
                    Task.Factory.StartNew(() => this.UpdateAction(false));
                });
        }
    }
}
