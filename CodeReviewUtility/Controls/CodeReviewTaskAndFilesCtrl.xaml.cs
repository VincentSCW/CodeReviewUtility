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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeReviewUtility.Controls
{
    /// <summary>
    /// Interaction logic for CodeReviewTaskAndFilesCtrl.xaml
    /// </summary>
    public partial class CodeReviewTaskAndFilesCtrl : UserControl
    {
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText",
                  typeof(string),
                   typeof(CodeReviewTaskAndFilesCtrl));

        public string TitleText
        {
            get { return (string)this.GetValue(TitleTextProperty); }
            set { this.SetValue(TitleTextProperty, value); }
        }

        public static readonly DependencyProperty SubTitleTextProperty = DependencyProperty.Register("SubTitleText",
                 typeof(string),
                  typeof(CodeReviewTaskAndFilesCtrl));

        public string SubTitleText
        {
            get { return (string)this.GetValue(SubTitleTextProperty); }
            set { this.SetValue(SubTitleTextProperty, value); }
        }

        public CodeReviewTaskAndFilesCtrl()
        {
            InitializeComponent();
        }
    }
}
