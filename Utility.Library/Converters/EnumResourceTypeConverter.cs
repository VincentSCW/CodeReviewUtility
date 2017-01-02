using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Reflection;
using Utility.Library.Attributes;
using System.ComponentModel;
using Utility.Library.Misc;
using System.Resources;

namespace Utility.Library.Converters
{
    public abstract class EnumResourceTypeConverter : EnumConverter,
        IValueConverter
    {
        public EnumResourceTypeConverter()
            : base(null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumResourceTypeConverter{TResourceManager}"/> class.
        /// </summary>
        /// <param name="type">A <see cref="T:System.Type"/> that represents the type of enumeration to associate with this enumeration converter.</param>
        public EnumResourceTypeConverter(Type type)
            : base(type)
        {
        }

        protected virtual string Seportator
        {
            get { return "."; }
        }

        protected abstract string GetString(string key, CultureInfo culture);


        #region Conversion Methods

        /// <summary>
        /// Converts the given value object to the specified destination type.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo"/>. If not supplied, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="T:System.Type"/> to convert the value to.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted <paramref name="value"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="destinationType"/> is null. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="value"/> is not a valid value for the enumeration. </exception>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                return string.Join(", ",
                    // split using commas
                    value.ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    // get resource string
                    .Select(s => this.GetString(string.Concat(EnumType.Name, this.Seportator, s), culture) ?? s).ToArray());
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Converts the specified value object to an enumeration object.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo"/>. If not supplied, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted <paramref name="value"/>.
        /// </returns>
        /// <exception cref="T:System.FormatException">
        /// 	<paramref name="value"/> is not a valid value for the target type. </exception>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                // we split and rejoin to trim any spaces between entries
                stringValue = string.Concat(',', string.Join(",", stringValue.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)), ',');

                // get values
                ulong[] values = Enum.GetValues(EnumType).Cast<object>().Select(o => EnumHelper.ToUInt64(o)).ToArray();

                ulong enumValue = Enum.GetNames(EnumType)
                    // try to get string from resource manager
                    .Select((s, i) => new KeyValuePair<int, string>(i, this.GetString(string.Concat(EnumType.Name, this.Seportator, s), culture) ?? s))
                    // filter to those strings who appear in the value parameter
                    .Where(si => stringValue.Contains(string.Concat(',', si.Value.Trim(), ',')))
                    // aggregate values using bit 'and' operation
                    .Aggregate(0UL, (i, si) => i & values[si.Key]);

                return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(EnumType), culture);
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }

        #endregion

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return ConvertTo(value, value.GetType(), culture);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
     
            return ConvertBack(value.ToString(), targetType, culture);
        }

        #endregion

        private string ConvertTo(object value, Type enumType, CultureInfo culture)
        {
            return string.Join(", ",
                // split using commas
                value.ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                // get resource string
                .Select(s => this.GetString(string.Concat(enumType.Name, this.Seportator, s), culture) ?? s).ToArray());
        }

        private object ConvertBack(string value, Type enumType, CultureInfo culture)
        {
            // we split and rejoin to trim any spaces between entries
            string stringValue = string.Concat(',', string.Join(",", value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)), ',');

            // get values
            ulong[] values = Enum.GetValues(enumType).Cast<object>().Select(o => EnumHelper.ToUInt64(o)).ToArray();

            ulong enumValue = Enum.GetNames(enumType)
                // try to get string from resource manager
                .Select((s, i) => new KeyValuePair<int, string>(i, this.GetString(string.Concat(enumType.Name, this.Seportator, s), culture) ?? s))
                // filter to those strings who appear in the value parameter
                .Where(si => stringValue.Contains(string.Concat(',', si.Value.Trim(), ',')))
                // aggregate values using bit 'and' operation
                .Aggregate(0UL, (i, si) => i & values[si.Key]);

            return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(enumType), culture);

        }
    }
    /// <summary>
    /// Provides a <see cref="TypeConverter"/> that converts enum values to and from resource strings.
    /// </summary>
    /// <typeparam name="TResourceManager">The type of the static class that contains a reference to a <see cref="ResourceManager"/>.</typeparam>
    public class EnumResourceTypeConverter<TResourceManager> : EnumResourceTypeConverter
    {
        #region Fields

        private static readonly ResourceManager _resourceManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="EnumResourceTypeConverter{TResourceManager}"/> class.
        /// </summary>
        static EnumResourceTypeConverter()
        {
            PropertyInfo property = typeof(TResourceManager).GetProperty("ResourceManager",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
                typeof(ResourceManager), Type.EmptyTypes, null);

            if (property != null)
            {
                _resourceManager = (ResourceManager)property.GetValue(null, null);
            }

            if (_resourceManager == null)
            {
                throw new InvalidOperationException("EnumResourceTypeConverter InvalidResourceManager");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumResourceTypeConverter{TResourceManager}"/> class.
        /// </summary>
        /// <param name="type">A <see cref="T:System.Type"/> that represents the type of enumeration to associate with this enumeration converter.</param>
        public EnumResourceTypeConverter(Type type)
            : base(type)
        {
        }

        #endregion

        protected override string GetString(string key, CultureInfo culture)
        {
            return _resourceManager.GetString(key, culture);
        }

        protected override string Seportator
        {
            get { return "_"; }
        }
    }
}
