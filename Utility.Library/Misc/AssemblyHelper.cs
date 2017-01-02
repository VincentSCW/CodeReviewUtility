using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Utility.Library.Misc
{
    /// <summary>
    /// Helper class to access meta data from an Assembly
    /// </summary>
    class AssemblyHelper
    {
        private static string PropertyNameTitle = "Title";
        private static string PropertyNameDescription = "Description";
        private static string PropertyNameProduct = "Product";
        private static string PropertyNameCopyright = "Copyright";
        private static string PropertyNameCompany = "Company";

        /// <summary>
        /// Get the value for AssemblyTitleAttribute
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetTitle(Assembly caller)
        {
            return CalculatePropertyValue<AssemblyTitleAttribute>(caller, PropertyNameTitle);
        }

        /// <summary>
        /// Get the value for AssemblyDescriptionAttribute
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetDescription(Assembly caller)
        {
            return CalculatePropertyValue<AssemblyDescriptionAttribute>(caller, PropertyNameDescription);
        }

        /// <summary>
        /// Get the value for AssemblyProductAttribute
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetProduct(Assembly caller)
        {
            return CalculatePropertyValue<AssemblyProductAttribute>(caller, PropertyNameProduct);
        }

        /// <summary>
        /// Get the value for AssemblyCopyrightAttribute
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetCopyright(Assembly caller)
        {
            return CalculatePropertyValue<AssemblyCopyrightAttribute>(caller, PropertyNameCopyright);
        }

        /// <summary>
        /// Get the value for AssemblyCompanyAttribute
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetCompany(Assembly caller)
        {
            return CalculatePropertyValue<AssemblyCompanyAttribute>(caller, PropertyNameCompany);
        }

        private static string CalculatePropertyValue<T>(Assembly caller, string propertyName)
        {
            string result = string.Empty;
            // try to get the property value from an attribute.
            var attributes = caller.GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                T attrib = (T)attributes[0];
                PropertyInfo property = attrib.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                    result = (string)property.GetValue(attributes[0], null);
            }

            return result;
        }
    }
}
