using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyAgencyVault.ViewModel.CommonItems
{
   public class ComboHelper
    {
        public static int GetMaxLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLengthProperty);
        }

        public static void SetMaxLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLengthProperty, value);
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(ComboHelper), new UIPropertyMetadata(OnMaxLenghtChanged));

        private static void OnMaxLenghtChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var comboBox = obj as ComboBox;
            if (comboBox == null) return;

           // TextBox txt  = (TextBox) comboBox.FindName("txtCarrierName");

            comboBox.Loaded +=
                (s, e) =>
                {
                    var textBox = comboBox.FindChild(typeof(TextBox), "txtCarrierName");
                    //TextBox txt = (TextBox)comboBox.FindChild("txtCarrierName");
                    if (textBox == null) return;

                    textBox.SetValue(TextBox.MaxLengthProperty, args.NewValue);
                };
        }

    }
    public static class UIChildFinder
    {
        static Boolean _flag = false;
        public static DependencyObject FindChild(this DependencyObject reference, Type childType, string childName)
        {
            DependencyObject foundChild = null;
            try
            {
                if (reference != null)
                {
                    int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
                    for (int i = 0; i < childrenCount; i++)
                    {
                        var child = VisualTreeHelper.GetChild(reference, i);
                        // If the child is not of the request child type child
                        if (child.GetType() != childType)
                        {

                            foundChild = FindChild(child, childType, childName);
                            if (_flag == true)
                                break;

                        }
                        else if (!string.IsNullOrEmpty(childName))
                        {
                            var frameworkElement = child as FrameworkElement;
                            // If the child's name is set for search
                            if (frameworkElement != null && frameworkElement.Name == childName)
                            {
                                // if the child's name is of the request name
                                foundChild = child;
                                _flag = true;
                                break;
                            }
                        }
                        else
                        {
                            // child element found.
                            foundChild = child;
                            break;
                        }
                    }
                }
            }
            catch { }
            return foundChild;
        }
    }
}
