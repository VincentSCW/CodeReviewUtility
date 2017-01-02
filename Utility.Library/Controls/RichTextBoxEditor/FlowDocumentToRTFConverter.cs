using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Convert a flowdocument contents to and from rtf
    /// </summary>
    [ValueConversion(typeof(string), typeof(FlowDocument))]
    public class FlowDocumentToRTFConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts from XAML markup to a WPF FlowDocument.
        /// </summary>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var document = new FlowDocument();
            if (value != null)
            {
                var text = (string)value;

                //if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
                //to the TextRange.Load method an exception would occur.
                if (string.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                }
                else
                {
                    TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                    {
                        tr.Load(ms, DataFormats.Rtf);
                    }
                }
            }

            // Set return value
            return document;
        }

        /// <summary>
        /// Converts from a WPF FlowDocument to a XAML markup string.
        /// </summary>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) 
                return string.Empty;

            // Get flow document from value passed in
            var document = (FlowDocument)value;

            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                return System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        #endregion
    }
}
