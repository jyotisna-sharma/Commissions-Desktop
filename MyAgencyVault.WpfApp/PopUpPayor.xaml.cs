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
using System.Data;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for PopUpPayor.xaml
    /// </summary>
    public partial class PopUpPayor : Window
    {
        public PopUpPayor()
        {
            InitializeComponent();
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //BindAgent();
        }
        //private void BindAgent()
        //{
        //    PolicyManagerClient objService = new PolicyManagerClient();
        //    try
        //    {

        //        ////List<UserDetail> lstUserDetail = objService.GetSubAgentList(MyAgencyVault.Common.UserDetail.objUserdetail.LoggedInUserProductId).ToList<UserDetail>();
        //        List<UserDetail> lstUserDetail = objService.GetSubAgentList(1).ToList<UserDetail>();
        //        DataTable dt = new DataTable();

        //        grdpayors.ItemsSource = lstUserDetail.ToArray();
        //        grdpayors.Columns[0].Visibility = Visibility.Hidden;
        //        grdpayors.Columns[1].Visibility = Visibility.Hidden;
        //        grdpayors.Columns[5].Visibility = Visibility.Hidden;
        //        grdpayors.Columns[6].Visibility = Visibility.Hidden;
              
                
                
        //        //grdAgent.Model.HideCols[1] = true;
        //        //grdAgent.Model.HideCols["IsEnable"] = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        objService.Close();
        //        objService = null;
        //    }
        //}
    }
}
