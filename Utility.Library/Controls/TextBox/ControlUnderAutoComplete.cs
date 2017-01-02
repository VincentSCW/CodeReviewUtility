using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

using TBox = System.Windows.Controls.TextBox;

namespace Utility.Library.Controls
{
   internal abstract class ControlUnderAutoComplete
   {
      private readonly Control control;

      internal static ControlUnderAutoComplete Create(Control control)
      {
          if (control is TBox || control.GetType().IsSubclassOf(typeof(TBox)))
            return new TextBoxUnderAutoComplete(control);
         return null;
      }

      protected ControlUnderAutoComplete(Control control)
      {
         this.control = control;
      }

      public abstract DependencyProperty TextDependencyProperty { get; }

      public string Text
      {
         get { return (string)control.GetValue(TextDependencyProperty); }
         set { control.SetValue(TextDependencyProperty, value); }
      }

      public Control Control
      {
         get { return control; }
      }

      public abstract string StyleKey {get;}

      public abstract bool Focus();
      public abstract void SelectAll();
      public abstract void Select(int start, int lenght);
      public abstract CollectionViewSource GetViewSource(Style style);
      
   }

   internal class TextBoxUnderAutoComplete : ControlUnderAutoComplete
   {

      public TextBoxUnderAutoComplete(Control control)
         : base(control)
      {
      }

      public override DependencyProperty TextDependencyProperty
      {
          get { return TBox.TextProperty; }
      }

      public override string StyleKey
      {
         get { return "autoCompleteTextBoxStyle"; }
      }

      public override bool Focus()
      {
          return ((TBox)Control).Focus();
      }
      public override void SelectAll()
      {
          ((TBox)Control).SelectAll();
      }

      public override void Select(int start, int length)
      {
          ((TBox)Control).Select(start, length);
      }

      public override CollectionViewSource GetViewSource(Style style)
      {
         return (CollectionViewSource)style.BasedOn.Resources["viewSource"];
      }
   }
}