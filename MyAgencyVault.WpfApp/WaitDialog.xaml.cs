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
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for WaitDialog.xaml
    /// </summary>
    public partial class WaitDialog : Window,IViewDialog
    {
       
        public WaitDialog()
        {
            InitializeComponent();
        }

        void IViewDialog.Show(string StrFormat)
        {
            try
            {
                txtDialog.Text = "Your request has been accepted. Please wait while your reports are generated in " + StrFormat + " format.";
                this.ShowDialog();
            }
            catch
            {
            }
           
            
        }

        void IViewDialog.ShowCustomMsg(string StrFormat)
        {
            try
            {
                txtDialog.Text = StrFormat;
                this.ShowDialog();
            }
            catch
            {
            }
        }


        void IViewDialog.Close()
        {
            this.Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        internal void ShowCustomMsg(string p)
        {
            try
            {
                txtDialog.Text = p;
                this.ShowDialog();
            }
            catch
            {
            }
        }
    }
}
