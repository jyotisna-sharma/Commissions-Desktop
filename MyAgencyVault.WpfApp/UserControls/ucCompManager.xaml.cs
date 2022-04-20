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
using MyAgencyVault.ViewModel.VMLib;
namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for ucCompManager.xaml
    /// </summary>
    public partial class ucCompManager : UserControl
    {
        public ucCompManager()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //DataTable Dt=new DataTable();
            //Dt.Columns.Add(new DataColumn("Created"));            
            //Dt.Columns.Add(new DataColumn("Batch#"));
            //Dt.Columns.Add(new DataColumn("Agency")); 
            //Dt.Columns.Add(new DataColumn("Status"));            
            ////
            //DataRow dr = Dt.NewRow();
            //dr[0]= "2009-04-24";
            //dr[1] = "#1284";
            //dr[2] = "Abc Broker";
            //dr[3] = "Closed";

            //Dt.Rows.Add(dr);
            ////
            //dr = Dt.NewRow();
            //dr[0] = "2009-04-24";
            //dr[1] = "#1285";
            //dr[2] = "XYZ Agency";
            //dr[3] = "Closed";
            //Dt.Rows.Add(dr);
            ////
            //dr = Dt.NewRow();
            //dr[0] = "2009-04-24";
            //dr[1] = "#1286";
            //dr[2] = "lpr Broker";
            //dr[3] = "Open";
            //Dt.Rows.Add(dr);
            ////
            //dgPolicies.ItemsSource = Dt.DefaultView;
            ////
            //DataTable dt = CreatePolicyDataTable();
            //dt = FillPolicyDataTable(dt);
            //dgPolicies1.ItemsSource = dt.DefaultView;
            ////
            //GetAssociatedPayment();
            //PaymentsDataTab();
            //InsuredPayment();


        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void txtExpender_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("test1");
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Untest1");

        }

        private void chkAll_Click(object sender, RoutedEventArgs e)
        {

        }


        #region for child grid header checkbox
        //add by neha
        private void ChkAll_Click_1(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                foreach (var item in VMInstances.CompManager.LinkPaymentSelectedPendingPolicies.Entries)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in VMInstances.CompManager.LinkPaymentSelectedPendingPolicies.Entries)
                {
                    item.IsSelected = false;
                }
            }


        }
        #endregion

        #region for expand
        //add by neha
        int CurrentRowIndex;
        int PreviousRowIndex;
        Button CurrentSender = null;
        Button PreviousSender = null;

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            PreviousSender = CurrentSender;
            Button expandCollapseButton = (Button)sender;
            CurrentSender = expandCollapseButton;
            //object ID = ((((Button)sender))).GetType().GUID;
            //DataGridRow selectedRow = DataGridRow.GetRowContainingElement(expandCollapseButton);

            if (dg != null)
            {
                if (null != expandCollapseButton && "+" == expandCollapseButton.Content.ToString())
                {
                    dg.RowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                    expandCollapseButton.Content = "-";

                    if (PreviousSender == CurrentSender)
                    {
                        expandCollapseButton.Content = "-";
                    }

                    else if (PreviousSender != null)
                    {
                        PreviousSender.Content = "+";
                    }
                }
                else
                {
                    dg.RowDetailsVisibilityMode = Microsoft.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
                    expandCollapseButton.Content = "+";
                    if (PreviousSender == CurrentSender)
                    {
                        expandCollapseButton.Content = "+";
                    }

                    else if (PreviousSender != null)
                    {
                        PreviousSender.Content = "-";
                    }
                }
            }

        }

        Microsoft.Windows.Controls.DataGrid dg;
        private void ParentLink_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dg = sender as Microsoft.Windows.Controls.DataGrid;
        }

        #endregion
        //private DataTable CreatePolicyDataTable()
        //{
        //    DataTable PolicyDataTable = new DataTable("Policy");
        //    DataColumn dcPayor = new DataColumn("Payor");
        //    DataColumn dcStatement = new DataColumn("Statement#");
        //    DataColumn dcchkAmount = new DataColumn("Check Amt");
        //    DataColumn dcHouse = new DataColumn("House");
        //    DataColumn dcUnposted = new DataColumn("Remaining");
        //    DataColumn dcNet = new DataColumn("Done");
        //    DataColumn dcEntries = new DataColumn("Entries");
        //    DataColumn dcId = new DataColumn("Status");

        //    PolicyDataTable.Columns.Add(dcPayor);
        //    PolicyDataTable.Columns.Add(dcStatement) ;
        //    PolicyDataTable.Columns.Add(dcchkAmount);
        //    PolicyDataTable.Columns.Add(dcHouse);   
        //    PolicyDataTable.Columns.Add(dcUnposted);
        //    PolicyDataTable.Columns.Add(dcNet);
        //    PolicyDataTable.Columns.Add(dcEntries);
        //    PolicyDataTable.Columns.Add(dcId);
            

        //    //PolicyDataTable.Columns["Entries"].Unique = true;
        //    //DataColumn[] temp2 = { PolicyDataTable.Columns["Entries"] };
        //    //PolicyDataTable.PrimaryKey = temp2;

        //    return PolicyDataTable;

        //}
        //private DataTable FillPolicyDataTable(DataTable dtTemp)
        //{
        //    DataRow DrRow;
        //    DrRow = dtTemp.NewRow();

        //    DrRow["Payor"] = "Oxford";
        //    DrRow["Statement#"] = "43434";
        //    DrRow["Check Amt"] = "$12,231.36";
        //    DrRow["House"] = "$12,231.36";
        //    DrRow["Remaining"] = "$124.22";
        //    DrRow["Done"] = "100%";
        //    DrRow["Entries"] = "46";
        //    DrRow["Status"] = "Closed";

        //    dtTemp.Rows.InsertAt(DrRow, 0);

        //    DrRow = dtTemp.NewRow();

        //    DrRow["Payor"] = "ICICI";
        //    DrRow["Statement#"] = "676734";
        //    DrRow["Check Amt"] = "$2,231.36";
        //    DrRow["House"] = "$2,231.36";
        //    DrRow["Remaining"] = "$24.22";
        //    DrRow["Done"] = "100%";
        //    DrRow["Entries"] = "461";
        //    DrRow["Status"] = "Closed";

        //    dtTemp.Rows.InsertAt(DrRow, 1);

        //    DrRow = dtTemp.NewRow();

        //    DrRow["Payor"] = "Oxfoard1";
        //    DrRow["Statement#"] = "3634";
        //    DrRow["Check Amt"] = "$34,231.36";
        //    DrRow["House"] = "$87,231.36";
        //    DrRow["Remaining"] = "$424.22";
        //    DrRow["Done"] = "90%";
        //    DrRow["Entries"] = "462";
        //    DrRow["Status"] = "Closed";

        //    dtTemp.Rows.InsertAt(DrRow, 2);

        //    return dtTemp;

        //}
        //private DataTable CreateAssociatedPayment()
        //{

        //    DataTable AssociatedPaymentDataTable = new DataTable();

        //    DataColumn dcPolicyName = new DataColumn("PolicyName");
        //    DataColumn dcPayorName = new DataColumn("PayorName");
        //    DataColumn dcPayment = new DataColumn("Payment");
        //    DataColumn dcNet = new DataColumn("Net Amount");
        //    DataColumn dcEntries = new DataColumn("Entries");

        //    AssociatedPaymentDataTable.Columns.Add(dcPolicyName);
        //    AssociatedPaymentDataTable.Columns.Add(dcPayorName);
        //    AssociatedPaymentDataTable.Columns.Add(dcPayment);
        //    AssociatedPaymentDataTable.Columns.Add(dcNet);
        //    AssociatedPaymentDataTable.Columns.Add(dcEntries);

        //    return AssociatedPaymentDataTable;
        //}
        //private DataTable FillAssociatedPayment(DataTable dtTemp)
        //{

        //    DataRow DrRow;
        //    DrRow = dtTemp.NewRow();
        //    DrRow["PolicyName"] = "ICICI Life Long Plan";
        //    DrRow["PayorName"] = "ICICI";
        //    DrRow["Payment"] = "$14,423";
        //    DrRow["Net Amount"] = "$15.000";
        //    DrRow["Entries"] = "51";
        //    dtTemp.Rows.Add(DrRow);

        //    DrRow = dtTemp.NewRow();
        //    DrRow["PolicyName"] = "ICICI Life Long Plan";
        //    DrRow["PayorName"] = "Oxford";
        //    DrRow["Payment"] = "$14,423";
        //    DrRow["Net Amount"] = "$15.000";
        //    DrRow["Entries"] = "51";
        //    dtTemp.Rows.Add(DrRow);

        //    DrRow = dtTemp.NewRow();
        //    DrRow["PolicyName"] = "SBI Life Long Plan";
        //    DrRow["PayorName"] = "SBI";
        //    DrRow["Payment"] = "$15,233";
        //    DrRow["Net Amount"] = "$20.000";
        //    DrRow["Entries"] = "63";
        //    dtTemp.Rows.Add(DrRow);

        //    DrRow = dtTemp.NewRow();
        //    DrRow["PolicyName"] = "SBI Life Long Plan";
        //    DrRow["PayorName"] = "SBI";
        //    DrRow["Payment"] = "$15,423";
        //    DrRow["Net Amount"] = "$15.000";
        //    DrRow["Entries"] = "21";
        //    dtTemp.Rows.Add(DrRow);

        //    DrRow = dtTemp.NewRow();
        //    DrRow["PolicyName"] = "Jeevan Jyotee";
        //    DrRow["PayorName"] = "SBI";
        //    DrRow["Payment"] = "$21,545";
        //    DrRow["Net Amount"] = "$21.000";
        //    DrRow["Entries"] = "3";
        //    dtTemp.Rows.Add(DrRow);

        //    return dtTemp;
        //}
        //private void  GetAssociatedPayment()
        //{
        //    DataTable dt = CreateAssociatedPayment();
        //    dt = FillAssociatedPayment(dt);

        //    //var dataProvider = (CollectionViewSource)FindResource("PlayerData");
        //    //dataProvider.Source = dt;
        //    //DataContext = new TeamModel() { PlayerAges = playerAges };
        //}
        //private void PaymentsDataTab()
        //{
        //    DataTable PaymentInfo = new DataTable();
        //    try
        //    {                
            
        //    PaymentInfo.Columns.Add("Policy Name");
        //    PaymentInfo.Columns.Add("Policy Number");
        //    PaymentInfo.Columns.Add("Payor");
        //    PaymentInfo.Columns.Add("Broker");
        //    PaymentInfo.Columns.Add("Client");
        //    PaymentInfo.Columns.Add("Payment");
        //    PaymentInfo.Columns.Add("Net Amount");
        //    PaymentInfo.Columns.Add("Entries");
        //    DataRow DrRow = PaymentInfo.NewRow();
        //    DrRow[0] = "Dental Policy";
        //    DrRow[1] = "123";
        //    DrRow[2] = "ABC Acadmiya";
        //    DrRow[3] = "Csd Sys";
        //    DrRow[4] = "Ram Manohar";
        //    DrRow[5] = "5003";
        //    DrRow[6] = "5000";
        //    DrRow[7] = "4";
        //    PaymentInfo.Rows.Add(DrRow);
        //    DrRow = PaymentInfo.NewRow();
        //    DrRow[0] = "Health Policy";
        //    DrRow[1] = "124";
        //    DrRow[2] = "Jenaral Viewer";
        //    DrRow[3] = "Corpo ferm";
        //    DrRow[4] = "rajesh singh";
        //    DrRow[5] = "2004";
        //    DrRow[6] = "1989";
        //    DrRow[7] = "5";
        //    PaymentInfo.Rows.Add(DrRow);

        //    DrRow = PaymentInfo.NewRow();
        //    DrRow[0] = "Dream Plan";
        //    DrRow[1] = "124";
        //    DrRow[2] = "bajaj";
        //    DrRow[3] = "Corpo ferm";
        //    DrRow[4] = "Peyton";
        //    DrRow[5] = "3000";
        //    DrRow[6] = "2850";
        //    DrRow[7] = "4";
        //    PaymentInfo.Rows.Add(DrRow);
        //    //grdPolicyDetail.ItemsSource = PaymentInfo.DefaultView;
            

        //    }
        //    catch (Exception ex)
        //    {
        //    }
            

        //}

        //private void InsuredPayment()
        //{
        //    DataTable DtPaymnet = new DataTable();
        //    DtPaymnet.Columns.Add("Insured");
        //    DtPaymnet.Columns.Add("Payment");
        //    DataRow DtRow = DtPaymnet.NewRow();
        //    DtRow[0] = "ABC Grp";
        //    DtRow[1] = "$ 2,123";
        //    DtPaymnet.Rows.Add(DtRow);
        //    DtRow = DtPaymnet.NewRow();
        //    DtRow[0] = "Xyz Grp";
        //    DtRow[1] = "$ 5,123";
        //    DtPaymnet.Rows.Add(DtRow);
        //    dgPolicies2.ItemsSource = DtPaymnet.DefaultView;  
        //}

    }
}
