using System.Windows;

namespace Utility.Library.UpdateManager
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow(UpdateWindowDataModel context)
        {
            InitializeComponent();
            this.DataContext = context;
        }
    }
}