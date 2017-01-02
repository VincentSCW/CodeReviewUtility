using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Utility.Library.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                return Visibility.Hidden;
            }

            bool parameterValue = true;
            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out parameterValue);
            }

            bool boolValue = (bool)value;
            boolValue = (parameterValue == boolValue);

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
