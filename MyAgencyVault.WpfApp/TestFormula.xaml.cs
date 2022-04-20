using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for TestFormula.xaml
    /// </summary>
    public partial class TestFormula : Window
    {
        public double Value;

        public TestFormula()
        {
            InitializeComponent();
        }

        public TestFormula(string labelText,double value)
        {
            InitializeComponent();

            //show focus on text box
            txtValue.Focus();

            if (labelText == "m_ResultedValue")
            {
                fieldLabel.Content = "Result";
                txtValue.Text = value.ToString();
                txtValue.IsReadOnly = true;
            }
            else
            {
                fieldLabel.Content = labelText;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double val = 0.0;
            if (double.TryParse(txtValue.Text, out val))
            {
                Value = val;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please Enter Numeric Value.");
                this.DialogResult = false;
            }
        }
    }
}
