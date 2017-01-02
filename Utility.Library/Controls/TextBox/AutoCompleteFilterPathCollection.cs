using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Utility.Library.Controls
{
    [TypeConverter(typeof(AutoCompleteFilterPathCollectionTypeConverter))]
    public class AutoCompleteFilterPathCollection : Collection<string>
    {
        public AutoCompleteFilterPathCollection(IList<string> list)
            : base(list)
        {
        }

        public AutoCompleteFilterPathCollection()
        {
        }

        internal string Join()
        {
            string[] array = new string[this.Count];
            this.CopyTo(array, 0);
            return string.Join(",", array);
        }
    }

    internal class AutoCompleteFilterPathCollectionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] paths = ((string)value).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return new AutoCompleteFilterPathCollection(paths);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                AutoCompleteFilterPathCollection c = (AutoCompleteFilterPathCollection)value;
                return c.Join();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}