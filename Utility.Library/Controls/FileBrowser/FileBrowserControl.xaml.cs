using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Interaction logic for FileBrowserControl.xaml
    /// </summary>
    [ContentProperty("FileNameContent")]
    public partial class FileBrowserControl : UserControl
    {
        public enum BrowseType
        {
            OpenFileDialog,
            SaveFileDialog
        }

        /// <summary>
        /// Identifies the <see cref="ItemContainerType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FileNameContentProperty =
            DependencyProperty.Register("FileNameContent", typeof(string),
            typeof(FileBrowserControl),
            new FrameworkPropertyMetadata(string.Empty,
                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                        new PropertyChangedCallback(OnFileNameContentPropertyChanged)));


        private static void OnFileNameContentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FileBrowserControl control = (FileBrowserControl)obj;
			control.textBox1.Text = args.NewValue as string;
        }

        public string FileNameContent
        {
            get { return (string)this.GetValue(FileNameContentProperty); }
            set { this.SetValue(FileNameContentProperty, value); }
        }

        public BrowseType Usage { get; set; }  

        public string FileExtension { get; set; }

        public string FileFilter { get; set; }

        public bool Multiselect { get; set; }

        public FileBrowserControl()
        {
            InitializeComponent();		
        }

		private void OnLoseTextBoxFocus(object sender, RoutedEventArgs e)
        {			
			this.FileNameContent = this.textBox1.Text;			 
        }   

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.FileDialog dlg = null;
            switch (this.Usage)
            {
                case BrowseType.OpenFileDialog:
                    {
                        dlg = new Microsoft.Win32.OpenFileDialog()
                            {
                                InitialDirectory = string.IsNullOrEmpty(this.FileNameContent) ? null :  System.IO.Path.GetDirectoryName(this.FileNameContent),
                                FileName = this.FileNameContent,
                                DefaultExt = this.FileExtension,
                                Filter = FileFilter
                            };                            
                    }
                    break;

                case BrowseType.SaveFileDialog:
                    {
                        dlg = new Microsoft.Win32.SaveFileDialog()
                        {
                            InitialDirectory = System.IO.Path.GetDirectoryName(this.FileNameContent),
                            FileName = this.FileNameContent,
                            DefaultExt = this.FileExtension,
                            Filter = FileFilter
                        };           
                    }
                    break;
            }

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.FileNameContent = dlg.FileName;
                this.textBox1.Focus();
            }       
        }
    }
}
