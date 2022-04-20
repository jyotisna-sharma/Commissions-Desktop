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
using Microsoft.Windows.Controls;
 
namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for PolicyManager.xaml
    /// </summary>
    public partial class PolicyManager : UserControl
    {

        public PolicyManager()
        {
            InitializeComponent();
        }


    
        private void btnSearchClient_Click(object sender, RoutedEventArgs e)
        {
            
            //try
            //{
            //    CreateClient frmCClient = new CreateClient();
            //    frmCClient.ShowDialog();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //}
        }
        private void txtOrigEffectiveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void tbPolicyDetail5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //PolicyManagerClient objPolicyClient = new PolicyManagerClient();
            //List<Note> lstNotes = objPolicyClient.GetPolicyNotes(67).ToList<Note>();
            //var notes = from u in lstNotes
            //            select new { u.Note1, u.DateCreated };
            ////dataGrid2.ItemsSource = notes.ToArray();

        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //AgentPayee frmAgentList = new AgentPayee();
            //frmAgentList.ShowDialog();

        }
        private void btnAdd4_Click(object sender, RoutedEventArgs e)
        {
            //ReplacePolicy frmReplace = new ReplacePolicy();
            //frmReplace.ShowDialog();
        }
        private void button10_Click(object sender, RoutedEventArgs e)
        {
            //PolicList frmPolicyList = new PolicList();
            //frmPolicyList.ShowDialog();
        }
        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            //////bool ischecked = radioButton2.IsChecked??false;
            //////if (ischecked)
            //////{
            //////    ////button10.Visibility = Visibility.Visible;
            //////}
            //////else
            //////{
            //////    ////button10.Visibility = Visibility.Hidden;
            //////}
        }
        private void MyCalendaerBytton_Click(object sender, RoutedEventArgs e)
        {
            //myPopUp.IsOpen = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //CreateClient frmClient = new CreateClient();
            //frmClient.ShowDialog();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {               
                //at the top..
                //=============
                //bind client combo box.            
                //
                //////this.cmbClients.ItemsSource = this.ClientNameList();
                ////this.cmbClnt.ItemsSource = this.ClientNameList();
                //
                //bind the gridPolicies. i.e. all the policies for the logged in agent ... 
                //incase it is a commission department user .. show all policies of all agnets.
                //
                ////this.dgPolicies.ItemsSource = this.DtPolicies.DefaultView;
                //
                //for the policy Detail tab
                //=========================
                //
                //fill the payor / product / carrier combos
                //            
                //List<string> lstPayors = new List<string>();
                //foreach (DataRow dr in this.Payors.Rows)
                //{
                //    lstPayors.Add(dr["Payor"].ToString());
                //}
                ////this.cmbPayor.ItemsSource = lstPayors;
                ////this.cmbPayor.SelectedIndex = 1;
                //List<string> lstCarrier = new List<string>();
                //foreach (DataRow dr in this.CarrieerList.Rows)
                //{
                //    lstCarrier.Add(dr["Carrier"].ToString());
                //}
                ////////this.cmbCarrier.ItemsSource = lstCarrier;                
                //List<string> lstProduct = new List<string>();
                //foreach (DataRow dr in this.ProductList.Select("Carrier = '" + lstCarrier[1] + "'"))
                //{
                //    lstProduct.Add(dr["Product"].ToString());
                //}
                //////////this.cmbProduct.ItemsSource = lstProduct;
                ////
                ////Other data feeding to controls on Policy Detail of selected policy in the top grid.
                //////////
                ////////fillPolicyDetailsToControl(this.DtPolicies.Select("Policy# = '12348'")[0]);
                ////
                ////for the Advanced schedule tab.
                ////==============================
                ////
                ////bind the product and carrier combo.
                ////bind the schedule type combo on two tab i.e. incoming schedule and outgoing schedule.
                ////
                ////this.cmbAdvancedScheduleCarrier.ItemsSource = lstCarrier;
                ////this.cmbAdvancedScheduleCarrier.SelectedIndex = 1;
                ////this.cmbAdvancedScheduleProduct.ItemsSource = lstProduct;
                //////
                ////fill the schedule grid.dgIScheduleDetails
                ////
                //this.dgIScheduleDetails.ItemsSource = this.Schedules.DefaultView;
                ////
                ////for the commission dashboard tab.
                ////=================================
                ////bind dgCommissions grid and dgCommissionsIssues
                ////
                //this.dgCommissions.ItemsSource = this.Commissions.DefaultView;
                //this.dgCommissionIssues.ItemsSource = this.CommissionIssues.DefaultView;
                //this.dgOutgoingIssues.ItemsSource = this.getOutCommission.DefaultView;
                //dataGrid1.ItemsSource = this.GrdOutGoing().DefaultView;  
                ////
                ////for the policy settings tab.
                ////==============================
                ////fill the controls on this tab.
                ////
                //fillPolicySettings();
                ////for the Learned Fields tab.
                ////===========================                            
                ////bind the monthly premium combo, payor , carrier, and product and Type combo
                ////
                //////////this.cmbPayorLearned.ItemsSource = lstPayors;
                ////////////this.cmbPayorLearned.SelectedIndex = 1;
                //////////this.cmbCarrierLearned.ItemsSource = lstCarrier;
                ////////////this.cmbCarrierLearned.SelectedIndex = 1;
                //////////this.cmbProductLearned.ItemsSource = lstProduct;
                ////////////bind the type combo related to incoming schedule Type. 
                ////////////                
                ////////////bind the source combo
                //////////this.cmbSource.ItemsSource = this.ClientNameList();
                ////fill the policy number combo..
                ////
                //List<string> lstPolicyNumber = new List<string>();
                //foreach (DataRow dr in this.DtPolicies.Rows)
                //{
                //    lstPolicyNumber.Add(dr["Policy#"].ToString());
                //}
                //////this.cmbPolicyManagerLearned.ItemsSource = lstPolicyNumber;
                ////fill other controls on there on the tab.
                //////////fillPolicyLearnedFields(this.DtPolicies.Select("Policy# = '12348'")[0]);
                ////
                ////For the Notes tab
                ////==================
                ////fill the grid of Notes.
                ////
                //this.dgNotes.ItemsSource = this.DtNotes.DefaultView;
                ////========
                ////END
                //========
            }
            catch
            {

            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //Window w = new CreateClient();            
            //w.ShowDialog();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //DataTable dt = new DataTable();
            //DataRow dr = dt.NewRow();
            //CreateClient w = new CreateClient(dr);
            //w.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //CommitionDashAdjushment ComDashAdj = new CommitionDashAdjushment();
            //ComDashAdj.ShowDialog();  
        }

        private void btnCommissionReverse_Click(object sender, RoutedEventArgs e)
        {
            //CommissionDashEdit ComDashEdit = new CommissionDashEdit();
            //ComDashEdit.ShowDialog();  
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //CommissionDashIssue ComDashIssue = new CommissionDashIssue();
            //ComDashIssue.ShowDialog();  
        }

        /// <summary>
        /// Incoming Schedule Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType schType = e.AddedItems[0] as MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType;
                Binding binding = null;
                
                BindingOperations.ClearBinding(txtBoxFromRange,TextBox.TextProperty);
                BindingOperations.ClearBinding(txtBoxToRange, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtBoxToRate, TextBox.TextProperty);

                switch (schType.ScheduleTypeId)
                {
                    case 1:
                    case 2:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.FromRange");
                        binding.StringFormat = "c";
                        txtBoxFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.ToRange");
                        binding.StringFormat = "c";
                        txtBoxToRange.SetBinding(TextBox.TextProperty, binding);
                        
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "p";
                        binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        
                        
                        break;

                    case 3:
                    case 4:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.FromRange");
                        txtBoxFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.ToRange");
                        txtBoxToRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                    case 5:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                }
            }
        }

        private void rdbIncomingScheduleType1_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.FirstYearPercentage");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.RenewalPercentage");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtRenewal.SetBinding(TextBox.TextProperty, binding);
        }

        private void rdbIncomingScheduleType3_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.FirstYearPercentage");
            binding.StringFormat = "c";
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.RenewalPercentage");
            binding.StringFormat = "c";
            txtRenewal.SetBinding(TextBox.TextProperty, binding);
        }

        /// <summary>
        /// Outgoing Schedule Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                MyAgencyVault.VM.MyAgencyVaultSvc.PolicyOutgoingScheduleType schType = e.AddedItems[0] as MyAgencyVault.VM.MyAgencyVaultSvc.PolicyOutgoingScheduleType;
                Binding binding = null;

                BindingOperations.ClearBinding(fromOutgoing, TextBox.TextProperty);
                BindingOperations.ClearBinding(toOutgoing, TextBox.TextProperty);
                BindingOperations.ClearBinding(rateOutgoing, TextBox.TextProperty);

                switch (schType.ScheduleTypeId)
                {
                    case 1:
                    case 2:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.FromRange");
                        binding.StringFormat = "c";
                        fromOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.ToRange");
                        binding.StringFormat = "c";
                        toOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "p";
                        binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);


                        break;

                    case 3:
                    case 4:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.FromRange");
                        fromOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.ToRange");
                        toOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);
                        break;
                    case 5:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);
                        break;
                }
            }
        }

        private void PolicyDetailEffDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolicyDefaultTrackFromDate.SelectedDate == null&&PolicyDetailEffDate.SelectedDate!=null)
            {
                PolicyDefaultTrackFromDate.SelectedDate = PolicyDetailEffDate.SelectedDate;
            }
        }
    }
}
