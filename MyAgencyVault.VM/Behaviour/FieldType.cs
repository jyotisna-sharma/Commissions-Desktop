using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MyAgencyVault.ViewModel.Behaviour
{   
    public static class FieldType
    {
        public static DependencyProperty IsNumericProperty = DependencyProperty.RegisterAttached("IsNumeric",
                    typeof(bool),
                    typeof(FieldType),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsNumeric(TextBox target, bool value)
        {
            target.SetValue(FieldType.IsNumericProperty, value);
        }

        public static bool GetIsNumeric(TextBox target)
        {
            return (bool)target.GetValue(FieldType.IsNumericProperty);
        }

        public static DependencyProperty IsAlphaNumericProperty = DependencyProperty.RegisterAttached("IsAlphaNumeric",
                    typeof(bool),
                    typeof(FieldType),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsAlphaNumeric(TextBox target, bool value)
        {
            target.SetValue(FieldType.IsAlphaNumericProperty, value);
        }

        public static bool GetIsAlphaNumeric(TextBox target)
        {
            return (bool)target.GetValue(FieldType.IsAlphaNumericProperty);
        }

        public static DependencyProperty IsAlphaProperty = DependencyProperty.RegisterAttached("IsAlpha",
                    typeof(bool),
                    typeof(FieldType),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsAlpha(TextBox target, bool value)
        {
            target.SetValue(FieldType.IsAlphaProperty, value);
        }

        public static bool GetIsAlpha(TextBox target)
        {
            return (bool)target.GetValue(FieldType.IsAlphaProperty);
        }

        public static DependencyProperty IsAnyProperty = DependencyProperty.RegisterAttached("IsAny",
                    typeof(bool),
                    typeof(FieldType),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsAny(TextBox target, bool value)
        {
            target.SetValue(FieldType.IsAnyProperty, value);
        }

        public static bool GetIsAny(TextBox target)
        {
            return (bool)target.GetValue(FieldType.IsAnyProperty);
        }
    }
}
