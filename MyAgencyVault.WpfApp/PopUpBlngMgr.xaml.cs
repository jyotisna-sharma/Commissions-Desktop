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
    /// Interaction logic for PopUpBlngMgr.xaml
    /// </summary>
    public partial class PopUpBlngMgr : Window
    {
        public PopUpBlngMgr()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        //  BindAgent();

        }
        //private void BindAgent()
        //{
        //    PolicyManagerClient objService = new PolicyManagerClient();
        //    try
        //    {

        //        ////List<UserDetail> lstUserDetail = objService.GetSubAgentList(MyAgencyVault.Common.UserDetail.objUserdetail.LoggedInUserProductId).ToList<UserDetail>();
        //        List<UserDetail> lstUserDetail = objService.GetSubAgentList(1).ToList<UserDetail>();
        //        DataTable dt = new DataTable();

        //        grdAgent.ItemsSource = lstUserDetail.ToArray();
        //        grdAgent.Columns[0].Visibility  = Visibility.Hidden  ;
        //        grdAgent.Columns[1].Visibility = Visibility.Hidden;
                
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
