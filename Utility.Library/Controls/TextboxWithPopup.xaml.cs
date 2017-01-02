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

namespace Utility.Library.Controls
{
    /// <summary>
    /// Interaction logic for TextboxWithPopup.xaml
    /// </summary>
    public partial class TextboxWithPopup : UserControl
    {
        public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register("TextValue",
               typeof(string),
                typeof(TextboxWithPopup));

        public string TextValue
        {
            get { return (string)this.GetValue(TextValueProperty); }
            set { this.SetValue(TextValueProperty, value); }
        }

        public static readonly DependencyProperty CueProperty = DependencyProperty.Register("Cue",
               typeof(string),
                typeof(TextboxWithPopup));

        public string Cue
        {
            get { return (string)this.GetValue(CueProperty); }
            set { this.SetValue(CueProperty, value); }
        }

        public TextboxWithPopup()
        {
            InitializeComponent();

            this.popupText.LostKeyboardFocus += (o, e) =>
                {
                    this.popup.IsOpen = false;
                };
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            this.popup.IsOpen = true;
            this.popupText.Focus();
            this.popupText.SelectionStart = this.popupText.Text.Length;
            this.popupText.SelectionLength = 1;
        }
    }
}
