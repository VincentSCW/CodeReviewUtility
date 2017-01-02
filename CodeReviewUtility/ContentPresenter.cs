using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CodeReviewUtility.ViewModels;
using CodeReviewUtility.Controls;
using CodeReviewUtility.Properties;
using System.Windows.Input;

namespace CodeReviewUtility
{
    internal enum ControlType
    {
        Review,
        Create,
        Commit
    }

    internal class ContentPresenter
    {
        public static UserControl CreateControl(ControlType controlType, string directory)
        {
            CodeReviewViewModel model = null;
            UserControl ctrl = null;

            switch(controlType)
            {
                case ControlType.Create:
                    model = new CreateCodeReviewViewModel(Settings.Default);
                    ctrl = new CreateCodeReviewControl();
                    break;
                case ControlType.Review:
                case ControlType.Commit:
                    model = new ReviewAndCommitViewModel(Settings.Default);
                    ctrl = new ReviewAndCommitControl();
                    break;
            }

            ctrl.DataContext = model;

            AddIsBusyPropertyChanged(model, ctrl);

            model.SourceDirectory = directory;
            return ctrl;
        }

        private static void AddIsBusyPropertyChanged(CodeReviewViewModel model, UserControl control)
        {
            model.PropertyChanged += (mo, me) =>
            {
                if (me.PropertyName == "IsBusy")
                {
                    if (model.IsBusy)
                    {
                        control.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate()
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                        });
                    }
                    else
                    {
                        control.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate()
                        {
                            Mouse.OverrideCursor = Cursors.Arrow;
                        });
                    }
                }
            };
        }
    }
}
