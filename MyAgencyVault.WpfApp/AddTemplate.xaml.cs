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
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for AddTemplate.xaml
    /// </summary>
    public partial class AddTemplate : Window
    {
        private string strVMValus = string.Empty;
        private string strCommandType = string.Empty;
        VmBillingManager objVmBillingManager ;
        bool bMessage = false;
        private ServiceClients _serviceclients;
        private ServiceClients serviceClients
        {
            get
            {
                if (_serviceclients == null)
                {
                    _serviceclients = new ServiceClients();
                }
                return _serviceclients;
            }
        }

        public AddTemplate(string strVM,string strCommand)
        {
            InitializeComponent();
            objVmBillingManager = VMInstances.BillingManager;
            strVMValus = strVM;
            strCommandType = strCommand;
            if (strVMValus == "ImportTool")
            {
                if (objVmBillingManager != null)
                {
                    if (objVmBillingManager.SelectedPayortempalate != null)
                    {
                        if (!string.IsNullOrEmpty(objVmBillingManager.SelectedPayortempalate.TemplateName))
                        {
                            txtTemplateName.Text = objVmBillingManager.SelectedPayortempalate.TemplateName;
                            chkForceImport.IsChecked = false;
                        }
                    }
                    chkForceImport.Visibility = Visibility.Visible;
                }

            }
            else
            {
                chkForceImport.Visibility = Visibility.Hidden;
            }
            

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtTemplateName.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //close the windows
            this.Hide();
            e.Handled = false;
            this.Close();
           
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTemplateName.Text))
            {
                MessageBox.Show("Please enter template Name");
                return;
            }
            else if (txtTemplateName.Text.Trim().ToLower() == "default")
            {
                MessageBox.Show("You can not save default template.");
                return;
            }
            else if (txtTemplateName.Text.Length < 3)
            {
                MessageBox.Show("Template name minimum length will be 3.");
                return;
            }
            else if (txtTemplateName.Text.Length > 30)
            {
                MessageBox.Show("Template maximum length will be 30.");
                return;
            }
            else
            {
                bool bValue = false;
                if (strVMValus == "PayorTool")
                {
                    PayorToolVM payorToolvm = VMInstances.PayorToolVM;
                    bValue = serviceClients.PayorToolClient.AddUpdatePayorToolTemplate(new Guid(), txtTemplateName.Text.Trim(), false, payorToolvm.SelectedPayor.PayorID);
                }
                else if (strVMValus == "ImportTool")
                {
                    //VmBillingManager objVmBillingManager = VMInstances.BillingManager;
                    Guid guidTempID = Guid.Empty;

                    if (objVmBillingManager != null)
                    {
                        if (objVmBillingManager.SelectedPayortempalate != null)
                        {
                            if (objVmBillingManager.SelectedPayortempalate.TemplateID != null)
                            {
                                guidTempID = (Guid)objVmBillingManager.SelectedPayortempalate.TemplateID;
                            }
                            else
                            {
                                guidTempID = new Guid();
                            }
                        }
                        else
                        {
                            guidTempID = new Guid();
                        }

                        if (serviceClients.PayorTemplateClient.ValidateTemplateName(objVmBillingManager.SelectedPayor.PayorID, guidTempID, txtTemplateName.Text.Trim(), false, (bool)chkForceImport.IsChecked))
                        {
                            MessageBoxResult result = MessageBox.Show("Template name already present in selected payor." + Environment.NewLine + "Do you want to continue ", "MyAgencyVault", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                bValue = serviceClients.PayorTemplateClient.AddUpdateImportToolPayorTemplate(objVmBillingManager.SelectedPayor.PayorID, guidTempID, txtTemplateName.Text.Trim(), false, (bool)chkForceImport.IsChecked, strCommandType);
                            }
                            else
                            {
                                bMessage = true;
                            }
                        }
                        else
                        {
                            //Save If New Item
                            bValue = serviceClients.PayorTemplateClient.AddUpdateImportToolPayorTemplate(objVmBillingManager.SelectedPayor.PayorID, guidTempID, txtTemplateName.Text.Trim(), false, (bool)chkForceImport.IsChecked, strCommandType);
                        }
                    }
                }

                if (bValue)
                {
                    if (!bMessage)
                    {
                        MessageBox.Show("Template save successfully.");
                    }
                   
                    this.Hide();
                    e.Handled = false;
                    this.Close();

                }
                else
                {
                    if (!bMessage)
                    {
                        MessageBox.Show("Issue while adding template.");
                    }
                    return;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = false;
        }
    }
}
