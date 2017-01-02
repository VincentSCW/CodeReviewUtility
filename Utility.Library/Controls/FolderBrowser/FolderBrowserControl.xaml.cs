using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Interaction logic for FileBrowserControl.xaml
    /// </summary>
    [ContentProperty("FolderNameContent")]
    public partial class FolderBrowserControl : UserControl,
        IDataErrorInfo 
    {
        /// <summary>
        /// Identifies the <see cref="ItemContainerType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FolderNameContentProperty =
            DependencyProperty.Register("FolderNameContent", typeof(string),
            typeof(FolderBrowserControl),
            new FrameworkPropertyMetadata(string.Empty,
                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                        new PropertyChangedCallback(OnTextChanged)));


        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FolderBrowserControl control = (FolderBrowserControl)obj;
            control.UpdateTextBoxText();
        }

        public string FolderNameContent 
        {
            get { return (string)this.GetValue(FolderNameContentProperty); }
            set { this.SetValue(FolderNameContentProperty, value); }
        }

        public FolderBrowserControl()
        {
            InitializeComponent();
            this.SetupTextBindings();
            this.UpdateTextBoxText();
            this.textBox1.LostFocus += new RoutedEventHandler(OnLostFocus);
            this.textBox1.KeyDown += new KeyEventHandler(OnKeyDown);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return || e.Key == Key.Enter))
                this.button1.Focus();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
           this.FolderNameContent = this.textBox1.Text;
        }

        private void UpdateTextBoxText()
        {
            textBox1.Text = this.FolderNameContent;
        }

        private void SetupTextBindings()
        {
            Binding binding = new Binding("FolderNameContent");
            binding.Source = this;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
            binding.ValidatesOnDataErrors = true;
            binding.NotifyOnValidationError = true;
            binding.Mode = BindingMode.TwoWay;
            this.textBox1.SetBinding(FolderNameContentProperty, binding);
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog()
                {
                    BrowseFiles = false,
                    ShowEditBox = true,
                    SelectedPath = this.FolderNameContent,
                    Title = "Browser Folder"
                };

            if(dlg.ShowDialog() == true)
            {
                this.FolderNameContent = dlg.SelectedPath;
            }
        }

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == "FolderNameContent")
                {
                    if (!string.IsNullOrEmpty(this.FolderNameContent) && this.FolderNameContent.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1)
                    {
                        result = "Folder Contains invalid characters";
                    }
                }
                return result;
            }
        }

        #endregion
    }
}
