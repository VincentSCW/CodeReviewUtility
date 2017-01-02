using System;
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
using Utility.Library.Controls;
using Microsoft.Win32;
using CodeReviewUtility.Properties;
using CodeReviewUtility.Views;
using CodeReviewUtility.ViewModels;
using System.IO;
using CodeReviewUtility.Controls;
using System.Threading.Tasks;

namespace CodeReviewUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SystemMenuWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var commandText = ((System.Windows.Input.RoutedUICommand)(e.Command)).Text;
            
            switch(commandText)
            {
                case "New":
                    Clear();
                    ShowCreate(string.Empty);
                    break;
                case "Open":
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.DefaultExt = "review";
                    dialog.Filter = "Review Package (*.review)|*.review";
                    
                    if (dialog.ShowDialog(this) ?? false)
                    {
                        ShowReviewOrCommit(dialog.FileName);
                    }
                    break;
                case "Close":
                    Clear();
                    break;
                case "Help":
                    break;
            }
            e.Handled = true;
        }

        private void Clear()
        {
            var model = ((MainWindowViewModel)this.DataContext).CodeReviewModel;
            if (model != null)
                model.CleanUp();
            this.contentCtrl.Content = null;
        }

        public void ShowCreate(string path)
        {
            UserControl view = ContentPresenter.CreateControl(ControlType.Create, path);
            ((MainWindowViewModel)this.DataContext).CodeReviewModel = (CodeReviewViewModel)view.DataContext;
            this.contentCtrl.Content = view;
        }

        public void ShowReviewOrCommit(string path)
        {
            UserControl view = ContentPresenter.CreateControl(ControlType.Review, path);  // ControlType.Review and ControlType.Commit are same

            ((MainWindowViewModel)this.DataContext).CodeReviewModel = (CodeReviewViewModel)view.DataContext;
            this.contentCtrl.Content = view;
        }
    }
}
