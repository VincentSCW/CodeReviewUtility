using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Utility.Library.Configuration
{
    internal class ManagerCollection : ConfigurationElementCollection
    {
        public ManagerCollection()
        {
        }

        public ManagerItem this[int index]
        {
            get { return (ManagerItem)base.BaseGet(index); }
        }

        public new ManagerItem this[string key]
        {
            get { return (ManagerItem)base.BaseGet(key); }
        }

        public bool ContainsKey(string key)
        {
            return base.BaseGet(key) != null;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ManagerItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ManagerItem)element).MgrName;
        }
    }
}
