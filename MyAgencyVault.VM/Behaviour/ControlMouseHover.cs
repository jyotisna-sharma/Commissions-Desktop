using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class MouseHoverAttachment
    {
        public static ICommand GetMouseHover(DependencyObject button)
        {
            return (ICommand)button.GetValue(MouseHoverProperty);
        }

        public static void SetMouseHover(DependencyObject button, ICommand value)
        {
            button.SetValue(MouseHoverProperty, value);
        }

        public static readonly DependencyProperty MouseHoverProperty =
            DependencyProperty.RegisterAttached("MouseHover", typeof(ICommand),
            typeof(MouseHoverAttachment), new FrameworkPropertyMetadata(null, MouseHoverPropertyChanged));

        public static void MouseHoverPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.MouseEnter += element_LostFocus;
                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.MouseEnter -= element_LostFocus;
            }
        }

        static void element_LostFocus(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;
            try
            {               
                Report rpt = ((System.Windows.FrameworkElement)(((System.Windows.Controls.ContentControl)(sender)).Content)).DataContext as Report;
                ICommand command = (ICommand)element.GetValue(MouseHoverAttachment.MouseHoverProperty);
                command.Execute(rpt);
            }
            catch
            { }
        }
    }
}
