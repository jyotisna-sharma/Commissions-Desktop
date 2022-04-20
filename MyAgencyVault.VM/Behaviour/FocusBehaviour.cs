using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Windows;
using System.Windows.Input;

namespace MyAgencyVault.ViewModel.Behaviour
{
   public static class FocusGotBehaviour
    {
        public static DependencyProperty FocusGotCommandProperty = DependencyProperty.RegisterAttached("FocusGot",
                  typeof(ICommand),
                  typeof(FocusGotBehaviour),
                  new FrameworkPropertyMetadata(null, new PropertyChangedCallback(FocusGotBehaviour.OnFocusGot)));

        private static void OnFocusGot(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;

            if (element != null)
            {
                
                //if (element is System.Windows.Controls.TextBox)
                //{
                //    System.Windows.Controls.TextBox d = (System.Windows.Controls.TextBox)element;
                //    d.SelectAll();
                //}
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.GotFocus += element_GotFocus;
                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.LostFocus -= element_GotFocus;
            }
        }

        static void element_GotFocus(object sender, RoutedEventArgs e)
        {
            UIElement element = (UIElement)sender;
            ICommand command = (ICommand)element.GetValue(FocusGotBehaviour.FocusGotCommandProperty);
            command.Execute(null);
        }
        public static void SetFocusGot(DependencyObject target, ICommand value)
        {
            target.SetValue(FocusGotBehaviour. FocusGotCommandProperty, value);
            
        }

        public static ICommand GetFocusGot(DependencyObject target)
        {
            return (ICommand)target.GetValue(FocusGotBehaviour.FocusGotCommandProperty);
        }

    }
}
