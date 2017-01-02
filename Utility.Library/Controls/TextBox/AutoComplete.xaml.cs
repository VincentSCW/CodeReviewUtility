using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Utility.Library.Controls
{
    /// <summary>
    /// Auto Complete behaviour for a text box.
    /// 
    /// can be use as (1)
    /// 
    ///     xmlns:ctrl="clr-namespace:Utility.Library.Controls"
    ///     
    ///     <Grid.Resources>
    ///            <ctrl:FileFolderSelectSource x:Key="FileFolderSrc"></ctrl:FileFolderSelectSource>
    ///     </Grid.Resources>
    ///     <TextBox ctrl:AutoComplete.SourceType="{StaticResource FileFolderSrc}"></TextBox>
    ///     
    /// or (2)
    /// 
    ///     <TextBox ctrl:AutoComplete.FilterPath="LastName,Email" ctrl:AutoComplete.Source="{Binding Path=People}"></TextBox>
    ///     
    /// In (1) the SourceType must be a class the implements the ISelectAutoCompleteSource interface.
    /// 
    /// In (2) the Source is a collection of objects. 
    /// 
    /// In both cases the FilterPath can use use hold the property that are available on the object display in the dropdown of
    /// the auto complete
    /// </summary>
    public sealed partial class AutoComplete
    {
        internal event TextChangedEventHandler TextChanged;
        internal event KeyEventHandler PreviewKeyDown;

        private ControlUnderAutoComplete control;
        internal CollectionViewSource ViewSource { get; private set; }
        private ListBox ListBox { get; set; }		

        private bool iteratingListItems;
        private string rememberedText;
        private Popup autoCompletePopup;

        #region Attached Properties
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source",
               typeof(object),
               typeof(AutoComplete),
               new FrameworkPropertyMetadata(null, OnSourcePropertyChanged));

        public static readonly DependencyProperty SourceTypeProperty = DependencyProperty.RegisterAttached("SourceType",
                typeof(ISelectAutoCompleteSource),
                typeof(AutoComplete),
                new FrameworkPropertyMetadata(null, OnSourceTypePropertyChanged));

        public static readonly DependencyProperty FilterPathProperty = DependencyProperty.RegisterAttached("FilterPath",
               typeof(AutoCompleteFilterPathCollection),
               typeof(AutoComplete),
               new FrameworkPropertyMetadata(null));

        private static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.RegisterAttached("ItemTemplate",
              typeof(DataTemplate),
              typeof(AutoComplete),
              new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemTemplatePropertyChanged)));
        #endregion

        private static readonly DependencyPropertyKey AutoCompleteInstancePropertyKey;
        private static readonly DependencyProperty AutoCompleteInstance;

        static AutoComplete()
        {
            AutoCompleteInstancePropertyKey = DependencyProperty.RegisterAttachedReadOnly("AutoCompleteInstance",
                 typeof(AutoComplete),
                 typeof(AutoComplete),
                 new FrameworkPropertyMetadata(null));
 
           AutoCompleteInstance = AutoCompleteInstancePropertyKey.DependencyProperty;
        }

        public AutoComplete()
        {
            InitializeComponent();
			
        }
		

        #region Dependency Property Getters/Setters

        public static object GetSource(DependencyObject d)
        {
            return d.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject d, object value)
        {
            d.SetValue(SourceProperty, value);
        }

        public static ISelectAutoCompleteSource GetSourceType(DependencyObject d)
        {
            return d.GetValue(SourceTypeProperty) as ISelectAutoCompleteSource;
        }

        public static void SetSourceType(DependencyObject d, ISelectAutoCompleteSource value)
        {
            d.SetValue(SourceTypeProperty, value);
        }

        public static AutoCompleteFilterPathCollection GetFilterPath(DependencyObject d)
        {
            return (AutoCompleteFilterPathCollection)d.GetValue(FilterPathProperty);
        }

        public static void SetFilterPath(DependencyObject d, AutoCompleteFilterPathCollection value)
        {
            d.SetValue(FilterPathProperty, value);
        }

        public static DataTemplate GetItemTemplate(DependencyObject d)
        {
            return (DataTemplate)d.GetValue(ItemTemplateProperty);
        }

        public static void SetItemTemplate(DependencyObject d, object value)
        {
            d.SetValue(ItemTemplateProperty, value);
        }

        #endregion

        #region Dependency Property Callbacks

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoComplete ac = EnsureInstance(d);
            ac.ViewSource.Source = e.NewValue;
        }

        private static void OnSourceTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                AutoComplete ac = EnsureInstance(d);
                new SelectSource(ac, e.NewValue as ISelectAutoCompleteSource);
            }
        }

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoComplete ac = EnsureInstance(d);
            ac.ListBox.ItemTemplate = (DataTemplate)e.NewValue;
        }

        private static AutoComplete EnsureInstance(DependencyObject d)
        {
            AutoComplete ac = GetAutoCompleteInstance(d);
            if (ac == null)
            {
                ac = new AutoComplete();
                ac.TextBox = (Control)d;
                d.SetValue(AutoCompleteInstancePropertyKey, ac);
            }
            return ac;
        }

        internal static AutoComplete GetAutoCompleteInstance(DependencyObject o)
        {
            return (AutoComplete)o.GetValue(AutoCompleteInstance);
        }

        #endregion

        internal Control TextBox
        {
            set
            {
                control = ControlUnderAutoComplete.Create(value);
                System.Diagnostics.Debug.Assert(control != null, "AutoComplete non supported control set");

                Style s = (Style)this[control.StyleKey];
                this.ViewSource = control.GetViewSource(s);
                this.ViewSource.Filter += OnCollectionViewSourceFilter;
				
                value.SetValue(Control.StyleProperty, this[control.StyleKey]);
                value.ApplyTemplate();
                autoCompletePopup = (Popup)value.Template.FindName("autoCompletePopup", value);			
				this.ListBox = (ListBox)value.Template.FindName("autoCompleteListBox", value);
                this.ListBox.MouseDoubleClick += new MouseButtonEventHandler(ListBox_MouseDown);
                value.AddHandler(System.Windows.Controls.TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextBoxTextChanged));
                value.LostFocus += OnTextBoxLostFocus;                
                value.PreviewKeyUp += OnTextBoxPreviewKeyUp;
                value.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            }
        }

        void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            iteratingListItems = true;
            autoCompletePopup.IsOpen = false;
            ICollectionView view = this.ViewSource.View;
            if (view.CurrentItem == null)
                this.control.Text = rememberedText;
            else
                this.control.Text = view.CurrentItem.ToString();

            control.Select(this.control.Text.Length, 1);
			control.Focus();
            e.Handled = true;
        }

        private void OnCollectionViewSourceFilter(object sender, FilterEventArgs e)
        {
            AutoCompleteFilterPathCollection filterPaths = GetAutoCompleteFilterProperty();
            if (filterPaths != null && filterPaths.Count > 0)
            {
                Type t = e.Item.GetType();
                foreach (string autoCompleteProperty in filterPaths)
                {
                    PropertyInfo info = t.GetProperty(autoCompleteProperty);
                    object value = info.GetValue(e.Item, null);
                    if (this.TextBoxStartsWith(value))
                    {
                        e.Accepted = true;
                        return;
                    }
                }
                e.Accepted = false;
            }
            else
            {
                e.Accepted = this.TextBoxStartsWith(e.Item);
            }
        }

        private bool TextBoxStartsWith(object value)
        {
            return value != null && value.ToString().StartsWith(control.Text, StringComparison.CurrentCultureIgnoreCase);
        }

        private AutoCompleteFilterPathCollection GetAutoCompleteFilterProperty()
        {
            if (GetFilterPath(control.Control) != null)
                return GetFilterPath(control.Control);
            return null;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (control.Text == "")
            {
                autoCompletePopup.IsOpen = false;
                return;
            }

            if(this.TextChanged != null)
                this.TextChanged(this, e);

            if (!iteratingListItems && this.control.Text != "")
            {
                ICollectionView v = this.ViewSource.View;
                if (v != null)
                {
                    v.Refresh();
                    v.MoveCurrentToFirst();
                    autoCompletePopup.IsOpen = !v.IsEmpty;
                }
            }
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.PreviewKeyDown != null)
                this.PreviewKeyDown(this, e);

            if (autoCompletePopup.IsOpen && (e.Key == Key.Enter || e.Key == Key.Tab))
            {
                iteratingListItems = true;
                autoCompletePopup.IsOpen = false;
                ICollectionView view = this.ViewSource.View;
                if (view.CurrentItem == null)
                {
                    if (rememberedText != null)
                        this.control.Text = rememberedText;
                }
                else
                    this.control.Text = view.CurrentItem.ToString();

                control.Select(this.control.Text.Length, 1);
                e.Handled = true;
            }
            
        }

        private void OnTextBoxPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (rememberedText == null)
                    rememberedText = control.Text;
                iteratingListItems = true;
                ICollectionView view = this.ViewSource.View;

                if (e.Key == Key.Up)
                {
                    if (view.CurrentItem == null)
                        view.MoveCurrentToLast();
                    else
                        view.MoveCurrentToPrevious();
                }
                else
                {
                    if (view.CurrentItem == null)
                        view.MoveCurrentToFirst();
                    else
                        view.MoveCurrentToNext();
                }
                if (view.CurrentItem == null)
                    this.control.Text = rememberedText;
                else
                    this.control.Text = view.CurrentItem.ToString();

                control.Select(this.control.Text.Length, 1);
            }
            else
            {
                iteratingListItems = false;
                rememberedText = null;
                if (autoCompletePopup.IsOpen && (e.Key == Key.Escape || e.Key == Key.Enter))
                {
                    autoCompletePopup.IsOpen = false;
                    if (e.Key == Key.Enter)
                    {
                        control.Select(this.control.Text.Length, 1);
                    }
                }
            }
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
           // autoCompletePopup.IsOpen = false;
        }
    }

}
