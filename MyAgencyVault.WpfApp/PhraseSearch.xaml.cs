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

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for PhraseSearch.xaml
    /// </summary>
    public partial class PhraseSearch : Window
    {
        public PhraseSearch()
        {
            InitializeComponent();
        }

        private void btnOK1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
