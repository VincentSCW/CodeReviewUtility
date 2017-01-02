using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using SysWindow = System.Windows.Window;

namespace Utility.Library.Controls.Behaviours
{
    /// <summary>
    /// Helper class to set the Dialog Result via a MVVM model
    /// 
    /// Usage
    /// <Window ...
    /// 
    ///     xmlns:ui_bav="clr-namespace:SM.UtilityLibrary.Controls.Behaviours;assembly=SM.UtilityLibrary"
    ///     ui_bav:DialogCloser.DialogResult="{Binding DialogResult}">
    ///     
    /// Your ViewModel should expose a property of type bool? (Nullable<bool>), and should implement INotifyPropertyChanged so it 
    /// can tell the view when its value has changed.
    /// </summary>
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as SysWindow;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }

        public static void SetDialogResult(SysWindow target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }

}
