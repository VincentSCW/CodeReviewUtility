using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Utility.Library.Commands;

namespace Utility.Library.Controls
{
    public enum SearchMode
    {
        Instant,
        Delayed,
    }

    public class SearchTextBox : TextBox
    {
        public static DependencyProperty LabelTextProperty = DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(SearchTextBox));

        public static DependencyProperty LabelTextColorProperty = DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(SearchTextBox));

        public static DependencyProperty SearchModeProperty = DependencyProperty.Register(
                "SearchMode",
                typeof(SearchMode),
                typeof(SearchTextBox),
                new PropertyMetadata(SearchMode.Instant));

        private static DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(SearchTextBox),
                new PropertyMetadata());
        public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        private static DependencyPropertyKey IsMouseLeftButtonDownPropertyKey = DependencyProperty.RegisterReadOnly(
                "IsMouseLeftButtonDown",
                typeof(bool),
                typeof(SearchTextBox),
                new PropertyMetadata());
        public static DependencyProperty IsMouseLeftButtonDownProperty = IsMouseLeftButtonDownPropertyKey.DependencyProperty;

        public static DependencyProperty SearchEventTimeDelayProperty = DependencyProperty.Register(
                "SearchEventTimeDelay",
                typeof(Duration),
                typeof(SearchTextBox),
                new FrameworkPropertyMetadata(
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    new PropertyChangedCallback(OnSearchEventTimeDelayChanged)));

        public static readonly RoutedEvent SearchEvent = EventManager.RegisterRoutedEvent(
                "Search",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SearchTextBox));

        public static readonly DependencyProperty SearchCmdProperty = DependencyProperty.Register("SearchCmd",
           typeof(ICommand),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnSearchCmdChanged));

        public static readonly DependencyProperty SearchClearedCmdProperty = DependencyProperty.Register("SearchClearedCmd",
           typeof(ICommand),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnSearchClearedCmdChanged));


        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(IEnumerable),
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        public static DependencyProperty ItemsSourceTagProperty = DependencyProperty.Register("ItemsSourceTag",
                typeof(string),
                typeof(SearchTextBox));

        public ICommand SearchCmd
        {
            get { return (ICommand)this.GetValue(SearchCmdProperty); }
            set { this.SetValue(SearchCmdProperty, value); }
        }

        public ICommand SearchClearedCmd
        {
            get { return (ICommand)this.GetValue(SearchClearedCmdProperty); }
            set { this.SetValue(SearchClearedCmdProperty, value); }
        }

        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        private DispatcherTimer searchEventDelayTimer;

        public SearchTextBox()
            : base()
        {
            searchEventDelayTimer = new DispatcherTimer();
            searchEventDelayTimer.Interval = SearchEventTimeDelay.TimeSpan;
            searchEventDelayTimer.Tick += new EventHandler(OnSeachEventDelayTimerTick);
        }


        private void OnSeachEventDelayTimerTick(object o, EventArgs e)
        {
            RaiseSearchEvent();
        }

        static void OnSearchEventTimeDelayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SearchTextBox stb = o as SearchTextBox;
            if (stb != null)
            {
                stb.searchEventDelayTimer.Interval = ((Duration)e.NewValue).TimeSpan;
                stb.searchEventDelayTimer.Stop();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            this.HasText = !string.IsNullOrEmpty(Text);

            if (this.HasText && SearchMode == SearchMode.Instant)
            {
                searchEventDelayTimer.Stop();
                searchEventDelayTimer.Start();
            }

            if (!this.HasText && this.SearchClearedCmd != null)
                this.SearchClearedCmd.Execute(null);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Border iconBorder = GetTemplateChild("PART_SearchIconBorder") as Border;
            if (iconBorder != null)
            {
                iconBorder.MouseLeftButtonDown += new MouseButtonEventHandler(IconBorder_MouseLeftButtonDown);
                iconBorder.MouseLeftButtonUp += new MouseButtonEventHandler(IconBorder_MouseLeftButtonUp);
                iconBorder.MouseLeave += new MouseEventHandler(IconBorder_MouseLeave);
            }
        }

        private void IconBorder_MouseLeftButtonDown(object obj, MouseButtonEventArgs e)
        {
            IsMouseLeftButtonDown = true;
        }

        private void IconBorder_MouseLeftButtonUp(object obj, MouseButtonEventArgs e)
        {
            if (!IsMouseLeftButtonDown)
                return;

            if (this.HasText && SearchMode == SearchMode.Instant)
            {
                this.Text = string.Empty;
            }

            if (this.HasText && SearchMode == SearchMode.Delayed)
            {
                RaiseSearchEvent();
            }

            IsMouseLeftButtonDown = false;
        }

        private void IconBorder_MouseLeave(object obj, MouseEventArgs e)
        {
            IsMouseLeftButtonDown = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && SearchMode == SearchMode.Instant)
            {
                this.Text = string.Empty;
            }
            else if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                RaiseSearchEvent();
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private void RaiseSearchEvent()
        {
            searchEventDelayTimer.Stop();

            if (this.HasText)
            {
                if (this.SearchCmd != null && this.SearchCmd.CanExecute(this.Text))
                {
                    this.SearchCmd.Execute(this.Text);
                }
                else
                {
                    RoutedEventArgs args = new RoutedEventArgs(SearchEvent);
                    RaiseEvent(args);
                }
            }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public Brush LabelTextColor
        {
            get { return (Brush)GetValue(LabelTextColorProperty); }
            set { SetValue(LabelTextColorProperty, value); }
        }

        public SearchMode SearchMode
        {
            get { return (SearchMode)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        public bool HasText
        {
            get { return (bool)GetValue(HasTextProperty); }
            private set { SetValue(HasTextPropertyKey, value); }
        }

        public Duration SearchEventTimeDelay
        {
            get { return (Duration)GetValue(SearchEventTimeDelayProperty); }
            set { SetValue(SearchEventTimeDelayProperty, value); }
        }

        public bool IsMouseLeftButtonDown
        {
            get { return (bool)GetValue(IsMouseLeftButtonDownProperty); }
            private set { SetValue(IsMouseLeftButtonDownPropertyKey, value); }
        }

        public event RoutedEventHandler Search
        {
            add { AddHandler(SearchEvent, value); }
            remove { RemoveHandler(SearchEvent, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string ItemsSourceTag
        {
            get { return (string)GetValue(ItemsSourceTagProperty); }
            set { SetValue(ItemsSourceTagProperty, value); }
        }

        private static void OnSearchCmdChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = dependencyObject as SearchTextBox;
            ctrl.SearchCmd = e.NewValue as ICommand;
        }

        private static void OnSearchClearedCmdChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = dependencyObject as SearchTextBox;
            ctrl.SearchClearedCmd = e.NewValue as ICommand;
        }

        private ICollectionView view;

        private static void OnItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = dependencyObject as SearchTextBox;

            if (e.NewValue != null)
            {
                ctrl.view = CollectionViewSource.GetDefaultView(e.NewValue);
                ctrl.SearchCmd = new SimpleCommand<object, object>(
                    (x) => { return true; },
                    (x) =>
                    {
                        ctrl.view.Filter += (m) =>
                        {
                            var s = ctrl.Text;
                            var mm = m.GetType().GetProperty(ctrl.ItemsSourceTag);
                            if (mm != null)
                                return mm.GetValue(m, null).ToString().ToLower().Contains(ctrl.Text.ToLower());
                            return true;
                        };
                    });

                ctrl.SearchClearedCmd = new SimpleCommand<object, object>(
                    (x) => { return true; },
                    (x) =>
                    {
                        ctrl.view.Filter = null;
                    });
            }
        }
    }
}
