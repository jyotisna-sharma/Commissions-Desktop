using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class FocusLostBehaviour
    {
        public static DependencyProperty FocusLostCommandProperty = DependencyProperty.RegisterAttached("FocusLost",
                    typeof(ICommand),
                    typeof(FocusLostBehaviour),
                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(FocusLostBehaviour.OnFocusLost)));

        //public static DependencyProperty ValidationFieldNameProperty = DependencyProperty.Register("ValidationFieldName",
        //            typeof(string),
        //            typeof(FocusLostBehaviour),
        //            new FrameworkPropertyMetadata(null));
        
        private static void OnFocusLost(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.LostFocus += element_LostFocus;
                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.LostFocus -= element_LostFocus;
            }
        }

        static void element_LostFocus(object sender, RoutedEventArgs e)
        {
            UIElement element = (UIElement)sender;
            ICommand command = (ICommand)element.GetValue(FocusLostBehaviour.FocusLostCommandProperty);
            command.Execute(null);

            //string ValidationFieldName = element.GetValue(FocusLostBehaviour.ValidationFieldNameProperty) as string;

        }

        //public static void SetValidationFieldName(DependencyObject target, string value)
        //{
        //    target.SetValue(FocusLostBehaviour.ValidationFieldNameProperty, value);
        //}

        //public static string GetValidationFieldName(DependencyObject target)
        //{
        //    return target.GetValue(FocusLostBehaviour.ValidationFieldNameProperty) as string;
        //}

        public static void SetFocusLost(DependencyObject target, ICommand value)
        {
            target.SetValue(FocusLostBehaviour.FocusLostCommandProperty, value);
        }

        public static ICommand GetFocusLost(DependencyObject target)
        {
            return (ICommand)target.GetValue(FocusLostBehaviour.FocusLostCommandProperty);
        }
    }
}
