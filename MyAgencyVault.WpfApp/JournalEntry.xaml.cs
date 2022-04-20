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
    /// Interaction logic for JournalEntry.xaml
    /// </summary>
    public partial class JournalEntry : Window
    {
        private string m_sOperationType;
        private VmBillingManager m_obillingManager;
        private ManualJournalData m_journalData;

        public JournalEntry()
        {
            InitializeComponent();
        }

        public JournalEntry(string operation, VmBillingManager billingManager)
        {
            InitializeComponent();

            if (operation == "Update")
            {
                billingManager.setJournalDetail();
            }
            else
            {
                billingManager.JnlData = new ManualJournalData();
            }

            grid.DataContext = billingManager.JnlData;

            m_sOperationType = operation;
            m_obillingManager = billingManager;
        }

        //Ok
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_sOperationType == "Update")
            {
                m_obillingManager.UpdateJournalEntry();
            }
            else
            {
                m_obillingManager.AddJournalEntry();
            }
            this.DialogResult = true;
        }

        //Cancel
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private bool ValidateContent()
        {
            return false;
        }
    }
}
