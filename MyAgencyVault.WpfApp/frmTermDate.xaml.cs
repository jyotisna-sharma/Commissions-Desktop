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
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.EmailFax;
using MyAgencyVault.VM.CommonItems;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for frmTermDate.xaml
    /// </summary>
    public partial class frmTermDate : Window
    {
        VM.VMLib.VMFollowUpManager obj = VMInstances.FollowUpVM;
        private WebDevPath ObjWebDevPath;
        public frmTermDate()
        {

            InitializeComponent();
            SharedVMData.isClosedWindow = false;
            //load selected value
            LoadForm();
        }

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

        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
                if (_SharedVMData == null)
                {
                    _SharedVMData = VMSharedData.getInstance();
                   
                }

                return _SharedVMData;
            }
        }

        private void LoadForm()
        {
            txtPayor.Text = Convert.ToString(obj.SelectFollowUpIssue.Payor);
            txtCarrier.Text = Convert.ToString(obj.SelectedFollowupIssueDetail.Carrier);
            txtClientName.Text = Convert.ToString(obj.SelectedFollowupIssueDetail.Client);
            txtInsured.Text = Convert.ToString(obj.SelectedFollowupIssueDetail.Insured);
            txtPolicyNumber.Text = Convert.ToString(obj.SelectFollowUpIssue.PolicyNumber);
            txtproduct.Text = Convert.ToString(obj.SelectedFollowupIssueDetail.Product);
            txtTermDate.Text = Convert.ToString(obj.SelectedFollowupIssueDetail.PolicyTermDate);


            txtPayor.IsReadOnly = true;
            txtCarrier.IsReadOnly = true;
            txtClientName.IsReadOnly = true;
            txtInsured.IsReadOnly = true;
            txtPolicyNumber.IsReadOnly = true;
            txtproduct.IsReadOnly = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SharedVMData.TermanationDate = obj.SelectedFollowupIssueDetail.PolicyTermDate;

            obj = null;
            SharedVMData.isClosedWindow = true;
            this.Hide();
            e.Handled = false;          
            this.Close();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SharedVMData.TermanationDate = string.Empty;

            if (!string.IsNullOrEmpty(txtTermDate.Text))
            {
                DateTime dtTermDate = Convert.ToDateTime(txtTermDate.Text);
                string strEffective = obj.SelectedFollowupIssueDetail.Effective;
                string strTrackFrom = obj.SelectedFollowupIssueDetail.TrackFrom;

                DateTime? dtEffectiveDate = null;
                DateTime? dtTrack = null;

                if (!string.IsNullOrEmpty(strEffective))
                {
                    dtEffectiveDate = Convert.ToDateTime(obj.SelectedFollowupIssueDetail.Effective);
                }

                if (!string.IsNullOrEmpty(strTrackFrom))
                {
                    dtTrack = Convert.ToDateTime(obj.SelectedFollowupIssueDetail.TrackFrom);
                }

                if (dtEffectiveDate != null)
                {
                    if (dtEffectiveDate > dtTermDate)
                    {
                        MessageBox.Show("Effective date cannot be greater than Term date.", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }                   

                }
                else if (dtTrack != null)
                {
                    if (dtTrack > dtTermDate)
                    {
                        MessageBox.Show("Track from date cannot be greater than Term date.", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; 
                    }                    
                }
                else
                {
                    serviceClients.PolicyClient.UpdatePolicyTermDateAsync((Guid)obj.SelectFollowUpIssue.PayorId, dtTermDate);

                    SharedVMData.TermanationDate = txtTermDate.Text;
                    SharedVMData.isClosedWindow = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Please enter term date", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        private void btnSaveAndNotify_Click(object sender, RoutedEventArgs e)
        {
            SharedVMData.TermanationDate = string.Empty;
            if (!string.IsNullOrEmpty(txtTermDate.Text))
            {
                DateTime dtTermDate = Convert.ToDateTime(txtTermDate.Text);
                string strEffective = obj.SelectedFollowupIssueDetail.Effective;
                string strTrackFrom = obj.SelectedFollowupIssueDetail.TrackFrom;

                DateTime? dtEffectiveDate = null;
                DateTime? dtTrack = null;

                if (!string.IsNullOrEmpty(strEffective))
                {
                    dtEffectiveDate = Convert.ToDateTime(obj.SelectedFollowupIssueDetail.Effective);
                }

                if (!string.IsNullOrEmpty(strTrackFrom))
                {
                    dtTrack = Convert.ToDateTime(obj.SelectedFollowupIssueDetail.TrackFrom);
                }

                if (dtEffectiveDate != null)
                {
                    if (dtEffectiveDate > dtTermDate)
                    {
                        MessageBox.Show("Effective date cannot be greater than Term date.", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
                else if (dtTrack != null)
                {
                    if (dtTrack > dtTermDate)
                    {
                        MessageBox.Show("Track from date cannot be greater than Term date.", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    serviceClients.PolicyClient.UpdatePolicyTermDateAsync((Guid)obj.SelectFollowUpIssue.PolicyId, dtTermDate);
                }


                string strSubject = subject();
                string strMailBody = MailBody();
                MailData mail = new MailData();
                List<User> UserList = serviceClients.UserClient.GetUsersByLicensee((Guid)obj.SelectFollowUpIssue.LicenseeId).ToList();
                List<User> AdminUser = new List<User>(UserList.Where(p => p.Role == UserRole.Administrator).ToList());

                string strEmail = string.Empty;
                foreach (var item in AdminUser)
                {
                    strEmail = item.Email;
                }

                if (string.IsNullOrEmpty(strEmail))
                {
                    mail.ToMail = "service@commissionsdept.com";
                }
                else
                {
                    mail.ToMail = strEmail;
                }

                mail.FromMail = "service@commissionsdept.com";

                //serviceClients.FollowupIssueClient.SaveNotifyMailAsync(mail, strSubject, strMailBody);
                serviceClients.FollowupIssueClient.SendNotificationMailAsync(mail, strSubject, strMailBody);

                SharedVMData.TermanationDate = txtTermDate.Text;
                SharedVMData.isClosedWindow = true;
                this.Hide();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter term date", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string subject()
        {
            string strSubject = string.Empty;
            strSubject = txtClientName.Text + " -" + txtproduct.Text + " " + txtPolicyNumber.Text + " With";
            List<Carrier> carrierLst = serviceClients.CarrierClient.GetPayorCarriersOnly((Guid)obj.SelectFollowUpIssue.PayorId).ToList();
            if (carrierLst.Count > 1)
            {
                strSubject = " " + strSubject + " " + txtPayor.Text;

                if (!string.IsNullOrEmpty(txtCarrier.Text))
                {
                    strSubject = " " + strSubject + " " + "/" + txtCarrier.Text;
                }
            }
            else
            {
                strSubject = strSubject + " " + txtPayor.Text;
            }
            strSubject = strSubject + " has been terminated as of " + txtTermDate.Text.ToString();

            return strSubject;
        }

        private string MailBody()
        {

            string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
            ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);
            string MailBody = string.Empty;

            //get logo path from dev server
            try
            {
                string strLogoPath = ObjWebDevPath.URL + "/Images/Logo.png";

                string strClientAndInsured = string.Empty;

                if (string.IsNullOrEmpty(txtInsured.Text))
                {
                    strClientAndInsured = txtClientName.Text;
                }
                else
                {
                    if (txtClientName.Text.Equals(txtClientName.Text))
                    {
                        strClientAndInsured = txtClientName.Text;
                    }
                    else
                    {
                        strClientAndInsured = txtClientName.Text + " / " + txtInsured.Text;
                    }
                }

                string strHeader = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                                  "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'>";

                string strReason = "<tr><td colspan='2'>Reason: Per Carrier " + "</td></tr>";

                string strPayor = "<tr><td colspan='2'>Payor:" + txtPayor.Text + "</td></tr>";

                string strCarrier = "<tr><td colspan='2'>Carrier:" + txtCarrier.Text + "</td></tr>";

                string strProduct = "<tr><td colspan='2'>Product:" + txtproduct.Text + "</td></tr>";

                string strClient = "<tr><td colspan='2'>Client:" + strClientAndInsured + "</td></tr>";

                string strPolicy = "<tr><td colspan='2'>Policy#:" + txtPolicyNumber.Text + "</td></tr>";

                string strLives = string.Empty;

                string strLivesValue = obj.SelectedFollowupIssueDetail.Enroll_Eligible;

                if (!string.IsNullOrEmpty(strLivesValue))
                {
                    string[] strVaue = null;

                    if (strLivesValue.Contains("/"))
                    {
                        strVaue = strLivesValue.Split('/');
                    }

                    if (!string.IsNullOrEmpty(strVaue[0]))
                    {
                        strLives = "<tr><td colspan='2'>Lives:" + strVaue[0].ToString() + "</td></tr>";
                    }
                }
                string strPremium = string.Empty;

                if (obj.SelectedFollowupIssueDetail.ModalPremium > 0)
                {
                    strPremium = "<tr><td colspan='2'>Premium:" + obj.SelectedFollowupIssueDetail.ModalPremium + "</td></tr>";
                }

                string strMode = "<tr><td colspan='2'>Mode:" + obj.SelectedFollowupIssueDetail.Mode + "</td></tr>";

                string strTermDate = "<tr><td colspan='2'>Term Date:" + txtTermDate.Text + "</td></tr>";

                string strRevenue = string.Empty;
                //Call to calculate PAC
                string strPAC = Convert.ToString(serviceClients.PolicyClient.CalculatePAC((Guid) obj.SelectFollowUpIssue.PolicyId));

                if (!string.IsNullOrEmpty(strPAC))
                {
                    if (strPAC == "0.00")
                    {

                    }
                    else
                    {
                        strRevenue = "<tr><td colspan='2'>AnualRev:" + "$" + strPAC + "</td></tr>";
                    }
                }

                string strFooter = "<tr><td colspan='2'><img src=" + strLogoPath + "  alt='CommissionsDept' />" +
                                  "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                                  + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>"
                                  + "</td></tr></table>";

                MailBody = strHeader + strReason + strPayor + strCarrier + strProduct + strClient + strPolicy + strLives + strPremium + strMode + strTermDate + strRevenue + strFooter;

               
            }
            catch
            {
            }

            return MailBody;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

    }
}
