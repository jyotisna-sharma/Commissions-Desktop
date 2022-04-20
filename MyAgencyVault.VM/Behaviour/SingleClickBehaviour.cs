using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class SingleClickBehaviour
    {
        public static DependencyProperty SingleClickCommandProperty = DependencyProperty.RegisterAttached("SingleClick",
                    typeof(ICommand),
                    typeof(SingleClickBehaviour),
                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(SingleClickBehaviour.OnSingleClick)));


        private static void OnSingleClick(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Control element = target as Control;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.MouseDown += element_MouseSingleClick;
                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.MouseDown -= element_MouseSingleClick;
            }
        }

        static void element_MouseSingleClick(object sender, RoutedEventArgs e)
        {
            Control element = (Control)sender;
            ICommand command = (ICommand)element.GetValue(SingleClickBehaviour.SingleClickCommandProperty);
            command.Execute(null);
        }

        public static void SetSingleClick(DependencyObject target, ICommand value)
        {
            target.SetValue(SingleClickBehaviour.SingleClickCommandProperty, value);
        }

        public static ICommand GetSingleClick(DependencyObject target)
        {
            return (ICommand)target.GetValue(SingleClickBehaviour.SingleClickCommandProperty);
        }
    }

    public static class ComboBoxAutoFilter
    {
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(ComboBoxAutoFilter),
                new UIPropertyMetadata(false, Enabled_Changed)
            );

        private static void Enabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo != null)
            {
                if (combo.Template != null)
                    SetTextChangedHandler(combo);
                else
                    combo.Loaded += new RoutedEventHandler(combo_Loaded);
            }
        }

        static void combo_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            combo.Loaded -= combo_Loaded;
            if (combo.Template != null)
                SetTextChangedHandler(combo);
        }

        private static void SetTextChangedHandler(ComboBox combo)
        {
            TextBox textBox = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            if (textBox != null)
            {
                bool enabled = GetEnabled(combo);
                if (enabled)
                    textBox.TextChanged += textBox_TextChanged;
                else
                    textBox.TextChanged -= textBox_TextChanged;
            }
        }

        private static void textBox_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            ComboBox combo = textBox.TemplatedParent as ComboBox;
            combo.IsDropDownOpen = true;
            string text = textBox.Text.Substring(0, textBox.SelectionStart);
            combo.Items.Filter = value => value.ToString().StartsWith(text);
        }

    }


}
