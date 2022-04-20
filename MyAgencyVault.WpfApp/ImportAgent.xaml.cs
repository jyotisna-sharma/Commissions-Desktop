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
    /// Interaction logic for ImportAgent.xaml
    /// </summary>
    public partial class ImportAgent : Window
    {
        public ImportAgent()
        {
            InitializeComponent();
            DgImport.ItemsSource = VMInstances.PeopleManager.TempTable.DefaultView;
        }

        private void btImport_Click(object sender, RoutedEventArgs e)
        {
            VMInstances.PeopleManager.ImportAgent(VMInstances.PeopleManager.TempTable);
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        
    }
}
