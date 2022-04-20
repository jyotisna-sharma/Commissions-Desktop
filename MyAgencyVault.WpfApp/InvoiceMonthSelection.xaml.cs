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
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for InvoiceMonthSelection.xaml
    /// </summary>
    public partial class InvoiceMonthSelection : Window
    {
        public InvoiceMonthSelection()
        {
            InitializeComponent();
        }

        public InvoiceMonthSelection(string payeeType,VmBillingManager bill)
        {
            InitializeComponent();

            if (payeeType == "Card")
            {
                Binding bind = new Binding();
                bind.Source = bill.ExportCardPayee;
                Okbtn.SetBinding(Button.CommandProperty, bind);
            }
            else
            {
                Binding bind = new Binding();
                bind.Source = bill.ExportCheckPayee;
                Okbtn.SetBinding(Button.CommandProperty, bind);
            }
        }

        public void FillMonthYearCombo(List<string> dateList)
        {
            invoiceMonths.Items.Clear();
            invoiceMonths.ItemsSource = dateList;
        }

        private void Okbtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
