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
    /// Interaction logic for DownloadManager.xaml
    /// </summary>
    public partial class DownloadManager : UserControl
    {
        public DownloadManager()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
         //   BindDownloadDetail();
        }
        private void BindDownloadDetail()
        {
            try
            {
            //    DataTable dtDownloadDetail = new DataTable();
            //    dtDownloadDetail.Columns.Add("Tracking#");
            //    dtDownloadDetail.Columns.Add("Download Status");
            //    dtDownloadDetail.Columns.Add("Payor");
            //    dtDownloadDetail.Columns.Add("Available");
            //    dtDownloadDetail.Columns.Add("Agency");
            //    dtDownloadDetail.Columns.Add("File Type");
            //    dtDownloadDetail.Columns.Add("Batch#");
            //    dtDownloadDetail.Columns.Add("Statement#");
            //    dtDownloadDetail.Columns.Add("Batch Status");



            //    DataRow dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "102";
            //    dr["Download Status"] = "Available";
            //    dr["Payor"] = "Oxford";
            //    dr["Available"] = "6/4/2009";
            //    dr["Agency"] = "ABC Agency";
            //    dr["File Type"] = "XLS";
            //    dr["Batch#"] = "";
            //    dr["Statement#"] = "";
            //    dr["Batch Status"] = "Unassigned";
            //    dtDownloadDetail.Rows.Add(dr);

            //    dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "1726";
            //    dr["Download Status"] = "Available";
            //    dr["Payor"] = "Agencya";
            //    dr["Available"] = "6/4/2009";
            //    dr["Agency"] = "ABC Agency";
            //    dr["File Type"] = "PDF";
            //    dr["Batch#"] = "";
            //    dr["Statement#"] = "";
            //    dr["Batch Status"] = "Unassigned";
            //    dtDownloadDetail.Rows.Add(dr);

            //    dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "2625";
            //    dr["Download Status"] = "In Process";
            //    dr["Payor"] = "Cigna";
            //    dr["Available"] = "6/3/2009";
            //    dr["Agency"] = "ABC Agency";
            //    dr["File Type"] = "PDF";
            //    dr["Batch#"] = "";
            //    dr["Statement#"] = "";
            //    dr["Batch Status"] = "Unassigned";
            //    dtDownloadDetail.Rows.Add(dr);

            //    dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "2662";
            //    dr["Download Status"] = "Complete";
            //    dr["Payor"] = "Cigna";
            //    dr["Available"] = "6/3/2009";
            //    dr["Agency"] = "FEW Agency";
            //    dr["File Type"] = "PDF";
            //    dr["Batch#"] = "32453";
            //    dr["Statement#"] = "987665";
            //    dr["Batch Status"] = "Pending Data Entry";
            //    dtDownloadDetail.Rows.Add(dr);

            //    dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "Red, Sam";
            //    dr["Download Status"] = "C-14, Viash puri";
            //    dr["Payor"] = "Delhi";
            //    dr["Available"] = "6/3/2009";
            //    dr["Agency"] = "FEW Agency";
            //    dr["File Type"] = "XLS";
            //    dr["Batch#"] = "65223";
            //    dr["Statement#"] = "1093773";
            //    dr["Batch Status"] = "Closed";
            //    dtDownloadDetail.Rows.Add(dr);

            //    dr = dtDownloadDetail.NewRow();
            //    dr["Tracking#"] = "Red, Sam";
            //    dr["Download Status"] = "C-14, Viash puri";
            //    dr["Payor"] = "Delhi";
            //    dr["Available"] = "6/3/2009";
            //    dr["Agency"] = "FEW Agency";
            //    dr["File Type"] = "PDF";
            //    dr["Batch#"] = "53534";
            //    dr["Statement#"] = "7253654";
            //    dr["Batch Status"] = "In Data Entry";
            //    dtDownloadDetail.Rows.Add(dr);


            //grdDownloadDatail.ItemsSource  = dtDownloadDetail.DefaultView ;



            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
