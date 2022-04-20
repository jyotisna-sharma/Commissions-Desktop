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

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for FollowUpManager.xaml
    /// </summary>
    public partial class FollowUpManager : UserControl
    {
        DataTable dtPolicy;
        DataTable dtStatement;
        string oldStyle = string.Empty;
        public FollowUpManager()
        {
            InitializeComponent();
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //Followup();
            //BindCmbo();
            //this.dgCommissions.ItemsSource = this.Commissions.DefaultView;
            //BindPolicyInfo();
            //BindPayorInfo();
          //  BindIssue();
            //BindPolicy();
            
        }

        private void btnSearchFllup_Click(object sender, RoutedEventArgs e)
        {
            ////BindPolicy();
            ////BindStatement();
            
            //BindPolicyInfo();
            //BindPayorInfo();
         //   BindIssue();
            //BindPolicy();
        }
        public void Followup()
        {
            

            dtPolicy = new DataTable();
            dtPolicy.Columns.Add("Payor");
            dtPolicy.Columns.Add("Agency");
            dtPolicy.Columns.Add("Insured");
            //dtPolicy.Columns.Add("Carrier");
            dtPolicy.Columns.Add("Policy#");
            //dtPolicy.Columns.Add("Product");
            dtPolicy.Columns.Add("Inv Date");
            dtPolicy.Columns.Add("Category");
            dtPolicy.Columns.Add("Status");
            dtPolicy.Columns.Add("Result");
            dtPolicy.Columns.Add("Reason");
            dtPolicy.Columns.Add("Follow Up");
            //dtPolicy.Columns.Add("Date Resolved");

            dtStatement = new DataTable();
            dtStatement.Columns.Add("Policy#");
            dtStatement.Columns.Add("Invoice date");
            dtStatement.Columns.Add("Premium");
            dtStatement.Columns.Add("Percentage");
            dtStatement.Columns.Add("Head/Lives");
            dtStatement.Columns.Add("Fee Per Head/Live");
            dtStatement.Columns.Add("Payment");
            dtStatement.Columns.Add("Date Received");
            dtStatement.Columns.Add("Batch#");
            dtStatement.Columns.Add("Statement#");
            dtStatement.Columns.Add("Check Amount");
        }
        private void BindPolicyInfo()
        {
            DataTable dtPolicyInfo = new DataTable();
            dtPolicyInfo.Columns.Add("Type of Payment");
            dtPolicyInfo.Columns.Add("Mode");
            dtPolicyInfo.Columns.Add("Submitted Thru");

            DataRow dr = dtPolicyInfo.NewRow();
            dr["Type of Payment"] = "Commission";
            dr["Mode"] = "Monthly";
            dr["Submitted Thru"] = "ABC General Agent";

            dtPolicyInfo.Rows.Add(dr);

            //////grdPolicyInfo.ItemsSource  = dtPolicyInfo.DefaultView;

        }

        private void BindPayorInfo()
        {
           // DataTable dtPayorInfo = new DataTable();
           // dtPayorInfo.Columns.Add("First Name");
           // dtPayorInfo.Columns.Add("Last Name");
           // dtPayorInfo.Columns.Add("Contact By");
           // dtPayorInfo.Columns.Add("Phone");
           // dtPayorInfo.Columns.Add("Email");
           // dtPayorInfo.Columns.Add("Fax");
           // dtPayorInfo.Columns.Add("Priority");
           // dtPayorInfo.Columns.Add("City");
           //dtPayorInfo.Columns.Add("State");
           // dtPayorInfo.Columns.Add("Zip");


           // DataRow dr = dtPayorInfo.NewRow();
           // dr["First Name"] = "Hoshiyar";
           // dr["Last Name"] = "singh";
           // dr["Contact By"] = "Phone";
           // dr["Phone"] = "452-546-4521";
           // dr["Email"] = "Hoshi@mail.com";
           // dr["Fax"] = "452-546-4521";
           // dr["Priority"] = "1";
           // dr["City"] = "Gurgaon";
           // dr["State"] = "Haryana";
           // dr["Zip"] = "123";
           // dtPayorInfo.Rows.Add(dr);
           //dr = dtPayorInfo.NewRow();
           // dr["First Name"] = "Eric";
           // dr["Last Name"] = "Wood";
           // dr["Contact By"] = "Phone";
           // dr["Phone"] = "452-546-4521";
           // dr["Email"] = "Ewood19@mail.com";
           // dr["Fax"] = "452-546-4521";
           // dr["Priority"] = "1";
           // dr["City"] = "Melville";
           // dr["State"] = "Ny";
           // dr["Zip"] = "11735";
           // dtPayorInfo.Rows.Add(dr);

           // grdPayor.ItemsSource = dtPayorInfo.DefaultView ;


        }

        private void BindIssue()
        {
            DataTable dtIssue = new DataTable();
            dtIssue.Columns.Add("Date");
            dtIssue.Columns.Add("User");
            dtIssue.Columns.Add("Notes");

            DataRow dr = dtIssue.NewRow();
            dr["Date"] = "11/23/2009";
            dr["User"] = "Abdul Razzaq";
            dr["Notes"] = "Issue in commission";
            dtIssue.Rows.Add(dr);


            //////grdIssue.ItemsSource = dtIssue.DefaultView;


        }
        private void BindCmbo()
        {
            //cmbByItems.Items.Add("By Payor");
            //cmbByItems.Items.Add("By Broker");
            //cmbByItems.SelectedIndex = 0;  
        }
        private void BindPolicy()
        {
            DataRow dr = dtPolicy.NewRow();
            dr["Payor"] = "ABC General Agent";
            dr["Agency"] = "John Smith Brokerage";
            dr["Insured"] = "ABC Group";
            //dr["Carrier"] = "HealthNet";
            dr["Policy#"] = "AFS56763";
            //dr["Product"] = "Medical";
            dr["Inv Date"] = "3/1/2009";
            dr["Category"] = "Missing";
            dr["Status"] = "Open";
            dr["Result"] = "Payment In";
            dr["Reason"] = "No Premium";
            dr["Follow Up"] = "12/31/2010";
            //dr["Date Resolved"] = "";
            dtPolicy.Rows.Add(dr);

            dr = dtPolicy.NewRow();
            dr["Payor"] = "ABC General Agent";
            dr["Agency"] = "John Smith Brokerage";
            dr["Insured"] = "Jay's Birds";
            //dr["Carrier"] = "Oxford";
            dr["Policy#"] = "GDGD666";
           // dr["Product"] = "Dental";
            dr["Inv Date"] = "4/1/2009";
            dr["Category"] = "Missing First (Never Paid)";
            dr["Status"] = "Closed";
            dr["Result"] = "Resolved(Brk)";
            dr["Reason"] = "Policy Credit";
            dr["Follow Up"] = "1/1/2010";
            //dr["Date Resolved"] = "";
            dtPolicy.Rows.Add(dr);

            dr = dtPolicy.NewRow();
            dr["Payor"] = "Cigna of Northeat";
            dr["Agency"] = "John Smith Brokerage";
            dr["Insured"] = "Blue Marble LLC";
            //dr["Carrier"] = "Cigna";
            dr["Policy#"] = "145242";
           // dr["Product"] = "Dental";
            dr["Inv Date"] = "3/1/2009";
            dr["Category"] = "Stopped  (2/1/2009)";
            dr["Status"] = "Open";
            dr["Result"] = "Pending";
            dr["Reason"] = "Not Commiss";
            dr["Follow Up"] = "1/2/2010";
            //dr["Date Resolved"] = "";
            dtPolicy.Rows.Add(dr);

            dr = dtPolicy.NewRow();
            dr["Payor"] = "HealthPass";
            dr["Agency"] = "John Smith Brokerage";
            dr["Insured"] = "Pink Elephant Inc";
            //dr["Carrier"] = "HealthPass";
            dr["Policy#"] = "981771";
           // dr["Product"] = "Life";
            dr["Inv Date"] = "4/1/2009";
            dr["Category"] = "Percentage";
            dr["Status"] = "Open";
            dr["Result"] = "Pending";
            dr["Reason"] = "Brk Lic Exp";
            dr["Follow Up"] = "1/12/2010";
            //dr["Date Resolved"] = "";
            dtPolicy.Rows.Add(dr);

            dr = dtPolicy.NewRow();
            dr["Payor"] = "United HealthCare";
            dr["Agency"] = "BP Brokers";
            dr["Insured"] = "Kennels R Us";
            //dr["Carrier"] = "UHC";
            dr["Policy#"] = "GFSR42425365";
          //  dr["Product"] = "Medical";
            dr["Inv Date"] = "3/1/2009";
            dr["Category"] = "Variance";
            dr["Status"] = "Closed";
            dr["Result"] = "Pending";
            dr["Reason"] = "Pay not Due";
            dr["Follow Up"] = "6/6/2010";
            //dr["Date Resolved"] = "";
            dtPolicy.Rows.Add(dr);

            dr = dtStatement.NewRow();
            dr["Policy#"] = "GDGD666";
            dr["Invoice date"] = "3/1/2009";
            dr["Premium"] = "$2,334.72";
            dr["Percentage"] = "4%";
            dr["Head/Lives"] = "n/a";
            dr["Fee Per Head/Live"] = "n/a";
            dr["Payment"] = "$93.39";
            dr["Date Received"] = "4/22/2009";
            dr["Batch#"] = "62653";
            dr["Statement#"] = "6363552";
            dr["Check Amount"] = "$5,353.77";
            dtStatement.Rows.Add(dr);

            dr = dtStatement.NewRow();
            dr["Policy#"] = "GDGD666";
            dr["Invoice date"] = "4/1/2009";
            dr["Premium"] = "$3,323.88";
            dr["Percentage"] = "Scale";
            dr["Head/Lives"] = "n/a";
            dr["Fee Per Head/Live"] = "n/a";
            dr["Payment"] = "$132.96";
            dr["Date Received"] = "5/31/2009";
            dr["Batch#"] = "78782";
            dr["Statement#"] = "992882";
            dr["Check Amount"] = "$8,726.88";
            dtStatement.Rows.Add(dr);
            DataSet dsData = new DataSet();
            dsData.Tables.Add(dtPolicy);
            dsData.Tables.Add(dtStatement);
            grdPolicyFllUp.ItemsSource = dtPolicy.DefaultView;

            dsData.Relations.Add(new DataRelation("rel", dsData.Tables[0].Columns["Policy#"], dsData.Tables[1].Columns["Policy#"]));

            //grdPolicy.DataSource = dsData.Tables[0];
            //oldStyle = this.grdPolicy.NestedTableGroupOptions.CaptionText;
            //grdPolicy.TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
            //grdPolicy.ChildGroupOptions.ShowAddNewRecordBeforeDetails = false;

        }

        DataTable getCommissions()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Invoice");
            dt.Columns.Add("Amount");
            dt.Columns.Add("Incoming");
            dt.Columns.Add("Number");
            dt.Columns.Add("$ Per");
            dt.Columns.Add("Fee");
            dt.Columns.Add("Share %");
            dt.Columns.Add("Total Payment");
            dt.Columns.Add("Statement");
            dt.Columns.Add("Batch");

            DataRow dr;

            dr = dt.NewRow();
            dr["Invoice"] = "5/2/2010";
            dr["Amount"] = "$ 2000";
            dr["Incoming"] = "10%";
            dr["Number"] = "3";
            dr["$ Per"] = "$ 42.00";
            dr["Fee"] = "100";
            dr["Share %"] = "100 %";
            dr["Total Payment"] = "$ 350.00";
            dr["Statement"] = "1234";
            dr["Batch"] = "456";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Invoice"] = "5/2/2010";
            dr["Amount"] = "$ 3345";
            dr["Incoming"] = "0.6%";
            dr["Number"] = "2";
            dr["$ Per"] = "$ 52.00";
            dr["Fee"] = "$ 200.00";
            dr["Share %"] = "50 %";
            dr["Total Payment"] = "$ 3550.00";
            dr["Statement"] = "345";
            dr["Batch"] = "6565";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Invoice"] = "5/2/2010";
            dr["Amount"] = "$ 1500";
            dr["Incoming"] = "0.70%";
            dr["Number"] = "1";
            dr["$ Per"] = "$ 42.00";
            dr["Fee"] = "100";
            dr["Share %"] = "100 %";
            dr["Total Payment"] = "$ 388.00";
            dr["Statement"] = "7566";
            dr["Batch"] = "8676";
            dt.Rows.Add(dr);


            return dt;
        }
        DataTable _Commissions;
        DataTable Commissions
        {
            get
            {
                if (_Commissions == null)
                    _Commissions = getCommissions();
                return _Commissions;
            }
        }

        private void EventTrigger_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void EventTrigger_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtdefaultFilter_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Convert.ToInt32(e.Text);
                if (txtdefaultFilter.Text.Length > 3)
                {
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;

                }
            }
            catch
            {
                e.Handled = true;
            }
        }


       
    }
}
