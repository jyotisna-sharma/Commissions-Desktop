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
using System.Windows.Navigation;
using System.Windows.Shapes;
 using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;


namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for PayorToolManager.xaml
    /// </summary>
    public partial class PayorToolManager : UserControl
    {
        //PayorToolVM objviewModel;
        //public PayorToolManager(PayorToolVM objview)
        //{
        //    InitializeComponent();
        //    //this.DataContext = objview;
        //    //objviewModel = objview;
        //}
        public PayorToolManager()
        {
            InitializeComponent();
            //this.DataContext = objview;
            //objviewModel = objview;
            if (VMInstances.PayorToolVM.SelectedPayortempalate == null)
            {
                Hypechang.IsEnabled = false;
                rdStatementUpload.IsEnabled = false;
                rdAmountUpload.IsEnabled = false;
            }
            else
            {
                Hypechang.IsEnabled = true;
                rdStatementUpload.IsEnabled = true;
                rdAmountUpload.IsEnabled = true;
            }
        }
        //add by neha
        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ////BindMasterPayors();

            //lbltext.Content = "Mode legend \nMO = Monthly \nA    = Annual \n map the payors \n" +  "mode" + "\ncodes to \n our systems code";
            //lbltext1.Content = "Coverage legend \n L = Group Life \n Dent = Dental \n map the payors in \n" + " coverage " + "codes to \n our  systems code";

        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            //dropCanvas.Focus();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }
        ////private void BindMasterPayors()
        ////{
        ////    ConfigurationsClient objPayorTool = new ConfigurationsClient();
        ////    List<Payor> lstPayors = objPayorTool.GetPayorList().ToList<Payor>();
        ////    cmbPayors.ItemsSource = lstPayors.ToArray();
        ////    cmbPayors.SelectedIndex = 0;  
        ////    //cmbPayors.DisplayMemberPath  = "PayorName";

        ////}

        private void hyperDulplicate_Click(object sender, RoutedEventArgs e)
        {
            //PopUpPayor PopUpPayor = new PopUpPayor();
            //PopUpPayor.ShowDialog();  
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Window w = new AddEditFormula();
            //w.ShowDialog();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VMInstances.PayorToolVM.SelectedPayortempalate == null)
            {
                Hypechang.IsEnabled = false;
                rdStatementUpload.IsEnabled = false;
                rdAmountUpload.IsEnabled = false;
            }
            else
            {
                Hypechang.IsEnabled = true;
                rdStatementUpload.IsEnabled = true;
                rdAmountUpload.IsEnabled = true;
            }
        }

    }
}
