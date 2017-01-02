using System.Windows;
using System.Windows.Input;

namespace Utility.Library.Controls
{
    public class SystemMenuItem : Freezable
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(SystemMenuItem), new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(SystemMenuItem));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(SystemMenuItem));

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id", typeof(int), typeof(SystemMenuItem));

        public static readonly DependencyProperty IsSeparatorProperty = DependencyProperty.Register(
           "IsSeparator", typeof(bool), typeof(SystemMenuItem));

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get  { return GetValue(CommandParameterProperty); }
            set  { SetValue(CommandParameterProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public bool IsSeparator
        {
            get { return (bool)GetValue(IsSeparatorProperty); }
            set { SetValue(IsSeparatorProperty, value); }
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SystemMenuItem();
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemMenuItem item = d as SystemMenuItem;

            if (item != null)
            {
                if (e.NewValue != null)
                {
                    item.Command = e.NewValue as ICommand;
                }
            }
        }
    }
}
