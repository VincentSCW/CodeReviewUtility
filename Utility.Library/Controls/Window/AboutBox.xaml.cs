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
using System.Windows.Shapes;
using System.Reflection;
using System.Xml;
using Utility.Library.Internal.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using Utility.Library.Misc;

namespace Utility.Library.Controls
{
    public sealed class AboutBoxDataViewModel
    {
        private static string propertyNameTitle = "Title";
        private static string propertyNameDescription = "Description";
        private static string propertyNameProduct = "Product";
        private static string propertyNameCopyright = "Copyright";
        private static string propertyNameCompany = "Company";
        private static string xPathRoot = "ApplicationInfo/";
        private static string xPathTitle = xPathRoot + propertyNameTitle;
        private static string xPathVersion = xPathRoot + "Version";
        private static string xPathDescription = xPathRoot + propertyNameDescription;
        private static string xPathProduct = xPathRoot + propertyNameProduct;
        private static string xPathCopyright = xPathRoot + propertyNameCopyright;
        private static string xPathCompany = xPathRoot + propertyNameCompany;
        private static string xPathLink = xPathRoot + "Link";
        private static string xPathLinkUri = xPathRoot + "Link/@Uri";

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;

        private XmlDocument xmlDoc;
        private Assembly caller;

        public ImageSource Icon  { get; set; }
        public ImageSource Logo { get; set; }
        /// <summary>
        /// Gets the title property, which is display in the About dialogs window title.
        /// </summary>
        public string ProductTitle
        {
            get
            {
                string result = CalculatePropertyValue(AssemblyHelper.GetProduct(this.caller), xPathTitle);
                if(string.IsNullOrEmpty(result))
                {    
                    // otherwise, just get the name of the assembly itself.
                    result = System.IO.Path.GetFileNameWithoutExtension(this.caller.CodeBase);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the application's version information to show.
        /// </summary>
        public string Version
        {
            get
            {
                string result;
                // first, try to get the version string from the assembly.
                Version ver = this.caller.GetName().Version;
                if (ver != null)
                    result = ver.ToString();
                else
                    // if that fails, try to get the version from a resource in the Application.
                    result = GetLogicalResourceString(xPathVersion);
            
                return result;
            }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public string Description { get { return CalculatePropertyValue(AssemblyHelper.GetDescription(this.caller), xPathDescription); } }
        /// <summary>
        /// Gets the product's full name.
        /// </summary>
        public string Product { get { return CalculatePropertyValue(AssemblyHelper.GetProduct(this.caller), xPathDescription); } }
        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public string Copyright { get { return CalculatePropertyValue(AssemblyHelper.GetCopyright(this.caller), xPathDescription); } }
        /// <summary>
        /// Gets the product's company name.
        /// </summary>
        public string Company { get { return CalculatePropertyValue(AssemblyHelper.GetCompany(this.caller), xPathDescription); } }
        /// <summary>
        /// Gets the link text to display in the About dialog.
        /// </summary>
        public string LinkText { get { return GetLogicalResourceString(xPathLink); } }
        /// <summary>
        /// Gets the link text to display in the About dialog.
        /// </summary>
        public string LinkUri { get { return GetLogicalResourceString(xPathLinkUri); } }

        public AboutBoxDataViewModel(Assembly caller, XmlDocument doc)
        {
            this.caller = caller;
            this.xmlDoc = doc;
        }

        /// <summary>
        /// Gets the specified property value either from a specific attribute, or from a resource dictionary.
        /// </summary>
        /// <typeparam name="T">Attribute type that we're trying to retrieve.</typeparam>
        /// <param name="propertyName">Property name to use on the attribute.</param>
        /// <param name="xpathQuery">XPath to the element in the XML data resource.</param>
        /// <returns>The resulting string to use for a property.
        /// Returns null if no data could be retrieved.</returns>
        private string CalculatePropertyValue(string assemblyValue, string xpathQuery)
        {
            // if the attribute wasn't found or it did not have a value, then look in an xml resource.
            if (string.IsNullOrEmpty(assemblyValue))
            {
                // if that fails, try to get it from a resource.
                assemblyValue = GetLogicalResourceString(xpathQuery);
            }

            return assemblyValue;
        }

        /// <summary>
        /// Gets the specified data element from the XmlDataProvider in the resource dictionary.
        /// </summary>
        /// <param name="xpathQuery">An XPath query to the XML element to retrieve.</param>
        ///<returns>The resulting string value for the specified XML element. 
        ///Returns empty string if resource element couldn't be found.</returns>
        private string GetLogicalResourceString(string xpathQuery)
        {
            string result = string.Empty;
            // get the About xml information from the resources.
            if (this.xmlDoc != null)
            {
                //if we found the XmlDocument, then look for the specified data. 
                XmlNode node = xmlDoc.SelectSingleNode(xpathQuery);
                if (node != null)
                {
                    if (node is XmlAttribute)
                    {
                        // only an XmlAttribute has a Value set.
                        result = node.Value;
                    }
                    else
                    {
                        // otherwise, need to just return the inner text.
                        result = node.InnerText;
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox(Assembly caller, ImageSource image) 
            : base()
        {
            InitializeComponent();

            XmlDocument xmlDoc = null;
            // if find the resource XmlDocument,
            XmlDataProvider provider = this.TryFindResource("aboutProvider") as XmlDataProvider;
            if (provider != null)
            {
                //  keep the XmlDocument
                xmlDoc = provider.Document;
            }

            this.DataContext = new AboutBoxDataViewModel(caller, xmlDoc) { Icon = image };
        }


        // <summary>
        // Handles click navigation on the hyperlink in the About dialog.
        // </summary>
        // <param name="sender">Object the sent the event.</param>
        // <param name="e">Navigation events arguments.</param>
        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null && !string.IsNullOrEmpty(e.Uri.OriginalString))
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
        }
    }
}
