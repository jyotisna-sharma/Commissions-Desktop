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
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for EditAndDisplay.xaml
    /// </summary>
    public partial class EditAndDisplay : Window
    {
      
        public EditAndDisplay()
        {
            InitializeComponent();
        }

        private ServiceClients _ServiceClients;
        private ServiceClients serviceClients
        {
            get
            {
                if (_ServiceClients == null)
                {
                    _ServiceClients = new ServiceClients();
                }
                return _ServiceClients;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you to delete selected phrase", "Compdept", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    int intID = VMInstances.BillingManager.SelectedobjImportToolPhrase.ID;
                    serviceClients.PayorTemplateClient.DeletePhrase(intID);
                    VMInstances.BillingManager.tempobjImportToolPhrase = VMInstances.BillingManager.objImportToolPhrase = new List<ImportToolPayorPhrase>(serviceClients.PayorTemplateClient.GetAllTemplatePhraseOnTemplate());
                }
            }
            catch (Exception)
            {
            }

           
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int intID = VMInstances.BillingManager.SelectedobjImportToolPhrase.ID;
                string strPhrase = VMInstances.BillingManager.SelectedobjImportToolPhrase.PayorPhrases;
                serviceClients.PayorTemplateClient.UpdatePhrase(intID, strPhrase);
            }
            catch (Exception)
            {
            }
           
        }
    }
}
