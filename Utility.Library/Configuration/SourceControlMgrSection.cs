using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Utility.Library.Configuration
{
    internal class SourceControlMgrSection : ConfigurationSection
    {
        [ConfigurationCollection(typeof(ManagerItem), AddItemName="add")]
        [ConfigurationProperty("managers")]
        public ManagerCollection Managers
        {
            get
            {
                return (ManagerCollection)base["managers"];
            }
            set
            {
                base["managers"] = value;
            }
        }
    }
}
