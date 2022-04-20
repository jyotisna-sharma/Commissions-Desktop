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
using MyAgencyVault.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for LinkPaymentClientChangeDialog.xaml
    /// </summary>
    public partial class LinkPaymentClientChangeDialog : Window
    {
        private readonly VMLinkPaymentClientChangeDialog viewModel;
        public LinkPaymentClientChangeDialog(VMLinkPaymentClientChangeDialog viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel; //Do this if you need access to the VM from inside your View. Or you could just use this.Datacontext to access the VM.
            this.DataContext = viewModel;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

    }
}
