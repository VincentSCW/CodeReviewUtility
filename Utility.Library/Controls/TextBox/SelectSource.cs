using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Linq;

using TBox = System.Windows.Controls.TextBox;


namespace Utility.Library.Controls
{
    public interface ISelectAutoCompleteSource
    {
        IEnumerable GetItems(Key key, string text);
    }

    class SelectSource
    {
        private AutoComplete autocomplete { get; set; }
        protected string LastPath { get; private set; }
        private ISelectAutoCompleteSource source;

        public ObservableCollection<object> Items { get; private set; }

        public SelectSource(AutoComplete autocomplete, ISelectAutoCompleteSource source)
        {
            this.Items = new ObservableCollection<object>();

            this.source = source;
            this.Autocomplete = autocomplete;
        }

        private AutoComplete Autocomplete
        {
            get { return this.autocomplete; }
            set
            {
                this.autocomplete = value;

                value = autocomplete;
                value.ViewSource.Source = this.Items;

                value.PreviewKeyDown += new KeyEventHandler(OnTextBoxPreviewKeyDown);
                value.TextChanged += new System.Windows.Controls.TextChangedEventHandler(OnAutocompleteTextChanged);
            }
        }

       private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
       {
           if (this.source != null)
           {
               IEnumerable items = this.source.GetItems(e.Key, this.LastPath);
               if(items != null)
               {
                   this.Items.Clear();
                   foreach(var i in items)
                        this.Items.Add(i);

                    this.Autocomplete.ViewSource.View.MoveCurrentToFirst();
               }
           }
       }

        private void OnAutocompleteTextChanged(object sender, TextChangedEventArgs e)
        {
            TBox textbox = e.Source as TBox;
            this.LastPath = textbox.Text;
        }
    }
}
