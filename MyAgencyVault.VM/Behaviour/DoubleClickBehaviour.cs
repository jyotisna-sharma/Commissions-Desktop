using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class DoubleClickBehaviour
    {
        public static DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached("DoubleClick",
                    typeof(ICommand),
                    typeof(DoubleClickBehaviour),
                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(DoubleClickBehaviour.OnDoubleClick)));


        private static void OnDoubleClick(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Control element = target as Control;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.MouseDoubleClick += element_MouseDoubleClick;
                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.MouseDoubleClick -= element_MouseDoubleClick;
            }
        }

        static void element_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Control element = (Control)sender;
            ICommand command = (ICommand)element.GetValue(DoubleClickBehaviour.DoubleClickCommandProperty);
            command.Execute(null);
        }

        public static void SetDoubleClick(DependencyObject target, ICommand value)
        {
            target.SetValue(DoubleClickBehaviour.DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClick(DependencyObject target)
        {
            return (ICommand)target.GetValue(DoubleClickBehaviour.DoubleClickCommandProperty);
        }
    }
}
