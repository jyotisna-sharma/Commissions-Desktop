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
    /// Interaction logic for ForgotPassword.xaml
    /// </summary>
    public partial class ForgotPassword : Window
    {
        private VMLoginUser _vmLogin;

        public ForgotPassword(VMLoginUser vmLogin)
        {
            InitializeComponent();
            _vmLogin = vmLogin;
            this.DataContext = vmLogin;
            txtUserName.Focus();
        }

        private void sendMail_Click(object sender, RoutedEventArgs e)
        {
            _vmLogin.SendPassMail();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _vmLogin.LoadUserData();
            txtAns.Focus();
        }
    }
}
