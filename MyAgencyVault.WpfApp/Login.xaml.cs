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
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        VMLoginUser objNewLogin;
        public Login(VMLoginUser objLogin)
        {
            InitializeComponent();
            objNewLogin = objLogin;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            objNewLogin.OnDirectLogin();
            this.Title = this.Title + "                      Version: " + System.Configuration.ConfigurationSettings.AppSettings["DisplayVersion"];
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            objNewLogin.ForgotPassStatus = string.Empty;
            objNewLogin.PHintA = string.Empty;
            objNewLogin.PHintQ = string.Empty;

            ForgotPassword forgetPasswordDialog = new ForgotPassword(objNewLogin);
            forgetPasswordDialog.ShowDialog();
        }

    }
}
