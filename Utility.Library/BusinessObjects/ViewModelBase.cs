using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Utility.Library.BusinessObjects
{
    /// <summary>
    /// Base class for all view models (from the Model-View-ViewModel pattern).
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IDictionary<string, object> values = new Dictionary<string, object>();

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Notify using pre-made PropertyChangedEventArgs
        /// </summary>
        /// <param name="args"></param>
        protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, args);
            }
        }

        protected T Get<T>(string key)
        {
            object v;
            if (values.TryGetValue(key, out v))
                return (T)v;

            return default(T);
        }

        protected bool Set<T>(string key, T value)
        {
            bool changed = true;
            object v;
            if (value != null && this.values.TryGetValue(key, out v))
            {
                changed = !value.Equals(v);
            }
            this.values[key] = value;
            this.NotifyPropertyChanged(key);

            return changed;
        }
    }
}

