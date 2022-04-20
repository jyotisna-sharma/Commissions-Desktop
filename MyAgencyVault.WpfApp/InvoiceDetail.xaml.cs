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
    /// Interaction logic for InvoiceDetail.xaml
    /// </summary>
    public partial class InvoiceDetail : Window
    {
        public InvoiceDetail()
        {
            InitializeComponent();
        }

        public InvoiceDetail(VmBillingManager billManager)
        {
            InitializeComponent();
            billManager.setInvoiceDetail();
            invoiceDetailGrid.DataContext = billManager.InvoiceLine;
            if (billManager.InvoiceLine.InvoiceId == 0)
                this.Close();
        }
    }
}
