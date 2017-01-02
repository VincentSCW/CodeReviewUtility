using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Utility.Library.Attributes
{
    /// <summary>
    /// Attribute for localization.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        private readonly Type resourcesType;
        private bool isLocalized;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="resourcesType">Type of the resources.</param>
        public LocalizableDescriptionAttribute(string description, Type resourcesType)
            : base(description)
        {
            this.resourcesType = resourcesType;
        }

        /// <summary>
        /// Get the string value from the resources.
        /// </summary>
        /// <value></value>
        /// <returns>The description stored in this attribute.</returns>
        public override string Description
        {
            get
            {
                if (!this.isLocalized)
                {
                    ResourceManager resMan = this.resourcesType.InvokeMember(@"ResourceManager", 
                        BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as ResourceManager;

                    CultureInfo culture = resourcesType.InvokeMember(
                         @"Culture",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as CultureInfo;

                    this.isLocalized = true;

                    if (resMan != null)
                    {
                        DescriptionValue = resMan.GetString(DescriptionValue, culture);
                    }
                }

                return DescriptionValue;
            }
        }

    }
}
