using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MyAgencyVault.WinApp.Validation;
using MyAgencyVault.ViewModel.Behaviour;
using System.Windows.Input;
using Microsoft.Windows.Controls;

namespace MyAgencyVault.WinApp.Styles
{
    partial class BureuSelected : ResourceDictionary
    {
       public BureuSelected()
       {
          InitializeComponent();
       }

       private void Element_LostFocus(object sender, RoutedEventArgs e)
       {
           Control control = sender as Control;
           if (control is TextBox || control is PasswordBox || control is MaskedTextBox)
           {
               ControlValidation.OnValidation(control);
           }
       }

       private void Element_GotFocus(object sender, RoutedEventArgs e)
       {
           if (sender is TextBox)
           {
               TextBox textbox = sender as TextBox;
               bool isNumeric = (bool)(textbox.GetValue(FieldType.IsNumericProperty));
               if (isNumeric)
               {
                   textbox.SelectAll();
               }
           }
       }

       private void Element_MouseUp(object sender, MouseButtonEventArgs e)
       {
           if (sender is TextBox)
           {
               TextBox textbox = sender as TextBox;
               bool isNumeric = (bool)(textbox.GetValue(FieldType.IsNumericProperty));
               if (isNumeric)
               {
                   textbox.SelectAll();
               }
           }
       }
    }
}
