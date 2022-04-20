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
using System.Data;
using System.ComponentModel;


namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for ImportPolicy.xaml
    /// </summary>
    public partial class ImportPolicy : Window
    {
        WaitDialog w = new WaitDialog(); 

     //   public delegate void onImportComplete();
        //public event onImportComplete ImportCompleted;
        public ImportPolicy()
        {
            InitializeComponent();
            DgImport.ItemsSource = VMInstances.OptimizedPolicyManager.TempTable.DefaultView;
            //this.ImportCompleted +=new onImportComplete(ImportPolicy_ImportCompleted);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }

        private void btImport_Click(object sender, RoutedEventArgs e)
        {
            //VMInstances.OptimizedPolicyManager.ImportPolicy(VMInstances.OptimizedPolicyManager.TempTable); this.Close();
            if (w.Visibility != System.Windows.Visibility.Visible)
            {
                w.txtDialog.Text = "Please wait while we import policies into the system...";
                w.ResizeMode = System.Windows.ResizeMode.NoResize;
                Dispatcher.BeginInvoke((Action)(() => w.ShowDialog()), null);
            }
            System.ComponentModel.BackgroundWorker bg = new System.ComponentModel.BackgroundWorker();
            bg.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate(object s, DoWorkEventArgs ea)
            {
                ea.Result = VMInstances.OptimizedPolicyManager.ImportPolicy(VMInstances.OptimizedPolicyManager.TempTable.DefaultView.ToTable());
            });
            bg.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bg.RunWorkerAsync();
        }

        void bg_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show(e.Result.ToString());
            //Application.Current.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(Refresh),null);
            w.Close();
         //  
            this.Close();
        }

        void Refresh()
        {
            VMInstances.OptimizedPolicyManager.OnSelectedLicenseeChanged(null, true, true);
        }

    }
}
