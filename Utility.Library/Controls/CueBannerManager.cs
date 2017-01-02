using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace Utility.Library.Controls
{
    public class CueBannerManager
    {
        private static readonly Dictionary<object, ItemsControl> itemsControls = new Dictionary<object, ItemsControl>();

        public static readonly DependencyProperty CueProperty = DependencyProperty.RegisterAttached(
                "Cue", typeof(object), typeof(CueBannerManager),
                new FrameworkPropertyMetadata(string.Empty, CuePropertyChanged));

        public static object GetCue(Control control)
        {
            return control.GetValue(CueProperty);
        }

        public static void SetCue(Control control, object value)
        {
            control.SetValue(CueProperty, value);
        }

        public static readonly DependencyProperty ForeColorProperty = DependencyProperty.RegisterAttached(
            "ForeColor", typeof(Brush), typeof(CueBannerManager),
            new FrameworkPropertyMetadata(null, CuePropertyChanged));

        public static Brush GetForeColor(Control control)
        {
            return (Brush)control.GetValue(ForeColorProperty);
        }

        public static void SetForeColor(Control control, Brush value)
        {
            control.SetValue(ForeColorProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.RegisterAttached(
                "FontSize", typeof(double), typeof(CueBannerManager),
                new FrameworkPropertyMetadata(new double(), CuePropertyChanged));

        public static double GetFontSize(Control control)
        {
            return (double)control.GetValue(FontSizeProperty);
        }

        public static void SetFontSize(Control control, double value)
        {
            control.SetValue(FontSizeProperty, value);
        }


        public static readonly DependencyProperty PaddingProperty = DependencyProperty.RegisterAttached(
            "Padding", typeof(Thickness), typeof(CueBannerManager),
            new FrameworkPropertyMetadata(new Thickness(), CuePropertyChanged));

        public static Thickness GetPadding(Control control)
        {
            return (Thickness)control.GetValue(PaddingProperty);
        }

        public static void SetPadding(Control control, Thickness value)
        {
            control.SetValue(PaddingProperty, value);
        }

        private static void CuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control control = (Control)d;
            control.Loaded += control_Loaded;
            if (d is ComboBox || d is TextBox)
            {
                control.GotFocus += control_GotFocus;
                control.LostFocus += control_LostFocus;
                control.LostFocus += control_Loaded;

                TextBox t = control as TextBox;
                if(t != null)
                    t.TextChanged += control_TextChanged;
            }

            if (d is ItemsControl && !(d is ComboBox))
            {
                ItemsControl i = (ItemsControl)d;
                //for Items property
                i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                itemsControls.Add(i.ItemContainerGenerator, i);
                //for ItemsSource property
                DependencyPropertyDescriptor prop =
                   DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                prop.AddValueChanged(i, ItemsSourceChanged);
            }
        }

        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl control = (ItemsControl)sender;
            if (control.ItemsSource != null)
                RemoveCueBanner(control);
            else
                ShowCueBanner(control);
        }

        private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl control;
            if (itemsControls.TryGetValue(sender, out control))
            {
                if (e.ItemCount > 0)
                    RemoveCueBanner(control);
                else
                    ShowCueBanner(control);
            }
        }

        private static void control_GotFocus(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            if (ShouldShowCueBanner(control))
            {
                RemoveCueBanner(control);
            }
        }
        private static void control_LostFocus(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            if (ShouldShowCueBanner(control))
            {
                ShowCueBanner(control);
            }
            else
            {
                RemoveCueBanner(control);
            }
        }

        private static void control_TextChanged(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            if (!ShouldShowCueBanner(control))
            {
                 RemoveCueBanner(control);
            }
        }
        private static void control_Loaded(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            if (ShouldShowCueBanner(control) && !control.IsFocused)
            {
                ShowCueBanner(control);
            }
            else
            {
                RemoveCueBanner(control);
            }
        }

        private static void RemoveCueBanner(UIElement control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(control);
                if (adorners == null)
                    return;
                foreach (Adorner adorner in adorners)
                {
                    if (adorner is CueBannerAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        private static void ShowCueBanner(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);
            if (layer != null)
            {
                Adorner a = new CueBannerAdorner(control);
                layer.Add(a);
            }
        }

        private static bool ShouldShowCueBanner(Control c)
        {
            DependencyProperty dp = GetDependencyProperty(c);
            if (dp == null) 
                return true;
            var v = c.GetValue(dp);
            return v == null || v.Equals("");
        }

        private static DependencyProperty GetDependencyProperty(Control control)
        {
            if (control is ComboBox)
                return ComboBox.TextProperty;
            if (control is TextBoxBase)
                return TextBox.TextProperty;
            return null;
        }

        class CueBannerAdorner : Adorner
        {
            private readonly ContentPresenter presenter;

            public CueBannerAdorner(Control adornedElement) :
                base(adornedElement)
            {
                this.IsHitTestVisible = false;

                presenter = new ContentPresenter();

                FrameworkElementFactory tbFactory = new FrameworkElementFactory(typeof(TextBlock));

                if (Control is TextBox)
                    tbFactory.SetValue(TextBlock.TextAlignmentProperty, ((TextBox)adornedElement).TextAlignment);

                tbFactory.SetValue(TextBlock.TextProperty, GetCue(adornedElement));

                tbFactory.SetValue(TextBlock.FontFamilyProperty, adornedElement.FontFamily);

                Thickness t = GetPadding(adornedElement);
                if (t == null)
                    tbFactory.SetValue(TextBlock.PaddingProperty, adornedElement.Padding);
                else
                    tbFactory.SetValue(TextBlock.PaddingProperty, t);

                double fontSize = GetFontSize(adornedElement);
                if (fontSize == 0)
                    tbFactory.SetValue(TextBlock.FontSizeProperty, adornedElement.FontSize);
                else
                    tbFactory.SetValue(TextBlock.FontSizeProperty, fontSize);

                Brush foreColor = GetForeColor(adornedElement);
                if (foreColor != null)
                    tbFactory.SetValue(TextBlock.ForegroundProperty, foreColor);

                tbFactory.SetValue(TextBlock.FontStyleProperty, FontStyles.Oblique);
                tbFactory.SetValue(TextBlock.OpacityProperty, 0.4);

                DataTemplate template = new DataTemplate() { VisualTree = tbFactory };
                presenter.ContentTemplate = template;

            }

            private Control Control
            {
                get { return (Control)this.AdornedElement; }
            }

            protected override Visual GetVisualChild(int index)
            {
                return presenter;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            protected override Size MeasureOverride(Size constraint)
            {
                presenter.Measure(Control.RenderSize);
                return Control.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                presenter.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }
    }
}
