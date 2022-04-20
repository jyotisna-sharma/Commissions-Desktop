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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using MyAgencyVault.WinApp.Common;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;
using System.Threading;

////using Microsoft.Windows.Controls;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for BillingManager.xaml
    /// </summary>
    public partial class BillingManager : UserControl
    {
        public BillingManager()
        {
            InitializeComponent();
            radExp.IsEnabled = true;
            radExp.IsChecked = true;
        }
       
        private void btnBillNo_Click(object sender, RoutedEventArgs e)
        {
            PopUpBlngMgr popUpMgr = new PopUpBlngMgr();
            popUpMgr.ShowDialog();  
              
        }
    
        private void grdBilling_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (dg.SelectedItem != null)
            {
                Window w = new NewsDetailViewer(false);
                w.ShowDialog();
            }
        }

        private void btnViewFile_Click(object sender, RoutedEventArgs e)
        {
            //string s = batchFileGrid.SelectedItem.ToString();
        }

        private void btnCreateCardPayee_Click(object sender, RoutedEventArgs e)
        {
            VmBillingManager billManager = this.DataContext as VmBillingManager;
            InvoiceMonthSelection invoiceMonthSel = new InvoiceMonthSelection("Card", billManager);
            //Fill the value of combobox that are prresent in the dialog...
            invoiceMonthSel.DataContext = this.DataContext;

            int startMonth = 0;
            int endMonth = 0;

            List<string> DateCollection = new List<string>();
            DateTime? CardPayeeDate = billManager.getCardPayeeExportDate();

            if (CardPayeeDate.HasValue)
            {
                for (int year = CardPayeeDate.Value.Year; year <= DateTime.Today.Year; year++)
                {
                    endMonth = 12;
                    startMonth = 1;

                    if (year == DateTime.Today.Year)
                        endMonth = DateTime.Today.Month;

                    if (year == CardPayeeDate.Value.Year)
                        startMonth = CardPayeeDate.Value.Month;

                    for (int m = startMonth; m < endMonth; m++)
                    {
                        DateCollection.Add(new DateTime(year,m,1).ToString("MMM-yyyy"));
                    }
                }
            }
            else
            {
                DateTime today = DateTime.Today;
                today = today.AddMonths(-7);
                for (int m = 0; m < 8; m++)
                {
                    DateCollection.Add(today.ToString("MMM-yyyy"));
                    today = today.AddMonths(1);
                }
            }

            if (DateCollection.Count > 0)
                DateCollection.RemoveAt(DateCollection.Count - 1);

            invoiceMonthSel.FillMonthYearCombo(DateCollection);
            invoiceMonthSel.ShowDialog();
        }

        private void btnCreateCheckPayee_Click(object sender, RoutedEventArgs e)
        {
            VmBillingManager billManager = this.DataContext as VmBillingManager;
            InvoiceMonthSelection invoiceMonthSel = new InvoiceMonthSelection("Check", billManager);
   
            invoiceMonthSel.DataContext = this.DataContext;

            int startMonth = 1;
            int endMonth = 0;

            List<string> DateCollection = new List<string>();
            DateTime? CheckPayeeDate = billManager.getCheckPayeeExportDate();

            if (CheckPayeeDate.HasValue)
            {
                for (int year = CheckPayeeDate.Value.Year; year <= DateTime.Today.Year; year++)
                {
                    endMonth = 12;
                    startMonth = 1;

                    if (year == DateTime.Today.Year)
                        endMonth = DateTime.Today.Month;

                    if (year == CheckPayeeDate.Value.Year)
                        startMonth = CheckPayeeDate.Value.Month;

                    for (int m = startMonth; m < endMonth; m++)
                    {
                        DateCollection.Add(new DateTime(year, m, 1).ToString("MMM-yyyy"));
                    }
                }
            }
            else
            {
                DateTime today = DateTime.Today;
                today = today.AddMonths(-7);
                for (int m = 0; m < 8; m++)
                {
                    DateCollection.Add(today.ToString("MMM-yyyy"));
                    today = today.AddMonths(1);
                }
            }

            if(DateCollection.Count > 0)
                DateCollection.RemoveAt(DateCollection.Count - 1);

            invoiceMonthSel.FillMonthYearCombo(DateCollection);
            invoiceMonthSel.ShowDialog();

        }

        private void btnJournalAdd_Click(object sender, RoutedEventArgs e)
        {
            VmBillingManager billManager = this.DataContext as VmBillingManager;
            JournalEntry journalEntry = new JournalEntry("Add", billManager);
            journalEntry.ShowDialog();
        }

        private void btnJournalEdit_Click(object sender, RoutedEventArgs e)
        {
            VmBillingManager billManager = this.DataContext as VmBillingManager;
            billManager.setJournalDetail();
            if (billManager.JnlData != null)
            {
                JournalEntry journalEntry = new JournalEntry("Update", billManager);
                journalEntry.ShowDialog();
            }
        }

        private void btnInvoiceView_Click(object sender, RoutedEventArgs e)
        {
            VmBillingManager billManager = this.DataContext as VmBillingManager;
            if (billManager.SelectedLicenseeInvoice != null && billManager.SelectedLicenseeInvoice.InvoiceId != 0)
            {
                InvoiceDetail journalEntry = new InvoiceDetail(billManager);
                journalEntry.ShowDialog();
            }
        }

        private void txtCutofDay1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int intValue = Convert.ToInt32(e.Text);
                e.Handled = false;
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void btnApplyRule_Click(object sender, RoutedEventArgs e)
        {
                 
            
        }

    }
}
