using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Utility.Library.Controls
{
	/// <summary>
	/// Interaction logic for WindowButtonsGroupCtrl.xaml
	/// </summary>
	public partial class WindowButtonsGroupCtrl : UserControl
	{
        public static readonly DependencyProperty OkCommandProperty = DependencyProperty.Register("OkCommand",
                  typeof(ICommand),
                   typeof(WindowButtonsGroupCtrl),
                   new FrameworkPropertyMetadata(null, OnOkCommandChanged));

        public ICommand OkCommand
        {
            get { return (ICommand)this.GetValue(OkCommandProperty); }
            set
            { 
                this.SetValue(OkCommandProperty, value);
                this.okButton.Command = value;
            }
        }

        public static readonly DependencyProperty HelpCommandProperty = DependencyProperty.Register("HelpCommand",
                  typeof(ICommand),
                   typeof(WindowButtonsGroupCtrl),
                   new FrameworkPropertyMetadata(null, OnHelpCommandChanged));

        public ICommand HelpCommand
        {
            get { return (ICommand)this.GetValue(HelpCommandProperty); }
            set
            {
                this.SetValue(HelpCommandProperty, value);
                this.helpButton.Command = value;
            }
        }

        public static readonly DependencyProperty OkCommandTextProperty = DependencyProperty.Register("OkCommandText",
                 typeof(string),
                  typeof(WindowButtonsGroupCtrl));

        public string OkCommandText
        {
            get { return (string)this.GetValue(OkCommandTextProperty); }
            set { this.SetValue(OkCommandTextProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandTextProperty = DependencyProperty.Register("CancelCommandText",
              typeof(string),
               typeof(WindowButtonsGroupCtrl));

        public string CancelCommandText
        {
            get { return (string)this.GetValue(CancelCommandTextProperty); }
            set { this.SetValue(CancelCommandTextProperty, value); }
        }

		public WindowButtonsGroupCtrl()
		{
			InitializeComponent();

            this.OkCommandText = "Ok";
            this.CancelCommandText = "Cancel";
		}

		private void OnCancel(object sender, RoutedEventArgs e)
		{
            if (!e.Handled)
            {
                var w = Window.GetWindow(this);
                if (w != null)
                    w.Close();
                else
                    Debug.Fail("WindowButtonsGroupCtrl not owned by a Window object");
            }
		}

        private static void OnOkCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WindowButtonsGroupCtrl ctrl = dependencyObject as WindowButtonsGroupCtrl;
            ctrl.OkCommand = e.NewValue as ICommand;
        }
        private static void OnHelpCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WindowButtonsGroupCtrl ctrl = dependencyObject as WindowButtonsGroupCtrl;
            ctrl.HelpCommand = e.NewValue as ICommand;
        }
	}
}
