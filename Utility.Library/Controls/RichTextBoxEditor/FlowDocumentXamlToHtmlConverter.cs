using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows;
using Utility.Library.Controls.XamlToHtmlParser;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Convert a flowdocument contents to and from xaml to html
    /// </summary>
    [ValueConversion(typeof(string), typeof(FlowDocument))]
    public class FlowDocumentXamlToHtmlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
                    text = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);

                    TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                    {
                        tr.Load(ms, DataFormats.Xaml);
                    }
                }
            }

            // Set return value
            return document;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            // Get flow document from value passed in
            var document = (FlowDocument)value;

            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return HtmlFromXamlConverter.ConvertXamlToHtml(System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray()), false);
            }
        }
    }
}
