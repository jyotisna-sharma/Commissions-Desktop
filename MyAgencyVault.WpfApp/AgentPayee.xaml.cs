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
using MyAgencyVault.WinApp.UserControls;
namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for AgentPayee.xaml
    /// </summary>
    public partial class AgentPayee : Window
    {
        public AgentPayee()
        {
            InitializeComponent();

        }
        private void BindAgent()
        {
            //PolicyManagerClient objService = new PolicyManagerClient();
            //try
            //{   
            //    List<UserDetail> lstUserDetail = objService.GetSubAgentList(3).ToList<UserDetail>();

            //    var bindAgentsList = from u in lstUserDetail
            //                         select new { u.FirstName, u.LastName, u.NickName, u.Company };
            //    dgAgents.ItemsSource = bindAgentsList.ToArray();


            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    objService.Close();
            //    objService = null;
            //}
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.SystemKey == Key.LeftAlt)
            {
                e.Handled = true;
                return;
            }

            if (e.SystemKey == Key.RightAlt)
            {
                e.Handled = true;
                return;
            }                      

            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }           

            if (e.Key == Key.LeftAlt)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.RightAlt)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                e.Handled = true;
                return;
            }

            if (e.SystemKey == Key.LeftAlt && e.SystemKey == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.SystemKey == Key.RightAlt && e.SystemKey == Key.Tab)
            {
                e.Handled = true;
                return;
            }          

            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Tab)
            {
                e.Handled = true;
                return;
            }
           
          
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }
    }


}
