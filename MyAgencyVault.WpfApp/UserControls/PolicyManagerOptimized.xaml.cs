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
using System.Data;
using MyAgencyVault.WinApp.Common;
using Microsoft.Windows.Controls;
using MyAgencyVault.ViewModel.VMLib;
using System.Text.RegularExpressions;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;

using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for PolicyManager.xaml
    /// </summary>
    public partial class PolicyManagerOptimized : UserControl
    {
      //  WaitDialog w = new WaitDialog(); 
        WaitDialog w;
        public PolicyManagerOptimized()
        {
            InitializeComponent();
             //w = new WaitDialog(); 
             //w.Visibility = System.Windows.Visibility.Collapsed;
           
        }

        private void btnSearchClient_Click(object sender, RoutedEventArgs e)
        {
        }
        private void txtOrigEffectiveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void tbPolicyDetail5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnAdd4_Click(object sender, RoutedEventArgs e)
        {
        }
        private void button10_Click(object sender, RoutedEventArgs e)
        {
        }
        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
        }
        private void MyCalendaerBytton_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }       

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
             
        }

        private void btnCommissionReverse_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
              
        }

        /// <summary>
        /// Incoming Schedule Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType schType = e.AddedItems[0] as MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType;
                Binding binding = null;
                
                BindingOperations.ClearBinding(txtBoxFromRange,TextBox.TextProperty);
                BindingOperations.ClearBinding(txtBoxToRange, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtBoxToRate, TextBox.TextProperty);

                switch (schType.ScheduleTypeId)
                {
                    case 1:
                    case 2:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.FromRange");
                        binding.StringFormat = "c";
                        txtBoxFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.ToRange");
                        binding.StringFormat = "c";
                        txtBoxToRange.SetBinding(TextBox.TextProperty, binding);
                        
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "p";
                        binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        
                        
                        break;

                    case 3:
                    case 4:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.FromRange");
                        txtBoxFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.ToRange");
                        txtBoxToRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                    case 5:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtBoxToRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                }
            }
        }

        private void rdbIncomingScheduleType1_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.FirstYearPercentage");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.RenewalPercentage");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtRenewal.SetBinding(TextBox.TextProperty, binding);

            if (rdbIncomingScheduleType3.IsChecked == true && rdbOutgoingPaymentType2.IsChecked == true)
            {
                txtRenewalOutgoing1.Visibility = Visibility.Visible;
                txtFirstYearOutgoing1.Visibility = Visibility.Visible;
                txtSplitOutgoing1.Visibility = Visibility.Visible;
                txtRenewalOutgoing.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing.Visibility = Visibility.Hidden;
                txtSplitOutgoing.Visibility = Visibility.Hidden;
            }
            else
            {
                txtRenewalOutgoing1.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing1.Visibility = Visibility.Hidden;
                txtSplitOutgoing1.Visibility = Visibility.Hidden;
                txtRenewalOutgoing.Visibility = Visibility.Visible;
                txtFirstYearOutgoing.Visibility = Visibility.Visible;
                txtSplitOutgoing.Visibility = Visibility.Visible;
            }
        }

        private void rdbIncomingScheduleType3_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.FirstYearPercentage");
            binding.StringFormat = "c";
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("SelectedPolicyToolIncommingShedule.RenewalPercentage");
            binding.StringFormat = "c";
            txtRenewal.SetBinding(TextBox.TextProperty, binding);

            if (rdbIncomingScheduleType3.IsChecked == true && rdbOutgoingPaymentType2.IsChecked == true)
            {
                txtRenewalOutgoing1.Visibility = Visibility.Visible;
                txtFirstYearOutgoing1.Visibility = Visibility.Visible;
                txtSplitOutgoing1.Visibility = Visibility.Visible;
                txtRenewalOutgoing.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing.Visibility = Visibility.Hidden;
                txtSplitOutgoing.Visibility = Visibility.Hidden;
            }
            else
            {
                txtRenewalOutgoing1.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing1.Visibility = Visibility.Hidden;
                txtSplitOutgoing1.Visibility = Visibility.Hidden;
                txtRenewalOutgoing.Visibility = Visibility.Visible;
                txtFirstYearOutgoing.Visibility = Visibility.Visible;
                txtSplitOutgoing.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Outgoing Schedule Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                MyAgencyVault.VM.MyAgencyVaultSvc.PolicyOutgoingScheduleType schType = e.AddedItems[0] as MyAgencyVault.VM.MyAgencyVaultSvc.PolicyOutgoingScheduleType;
                Binding binding = null;

                BindingOperations.ClearBinding(fromOutgoing, TextBox.TextProperty);
                BindingOperations.ClearBinding(toOutgoing, TextBox.TextProperty);
                BindingOperations.ClearBinding(rateOutgoing, TextBox.TextProperty);

                switch (schType.ScheduleTypeId)
                {
                    case 1:
                    case 2:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.FromRange");
                        binding.StringFormat = "c";
                        fromOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.ToRange");
                        binding.StringFormat = "c";
                        toOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "p";
                        binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);


                        break;

                    case 3:
                    case 4:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.FromRange");
                        fromOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.ToRange");
                        toOutgoing.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);
                        break;
                    case 5:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewOutSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        rateOutgoing.SetBinding(TextBox.TextProperty, binding);
                        break;
                }
            }
        }

        #region for remove and reolve button hide and show
       
        private void rdClose_Checked(object sender, RoutedEventArgs e)
        {
            if (rdClose.IsChecked == true)
            {
                bnresolve.Visibility = Visibility.Hidden;
                btnremove.Visibility = Visibility.Visible;
            }
            else
            {
                bnresolve.Visibility = Visibility.Visible;
                btnremove.Visibility = Visibility.Hidden;
            }

        }

        private void rdAll_Checked(object sender, RoutedEventArgs e)
        {
            if (rdAll.IsChecked == true)
            {
                bnresolve.Visibility = Visibility.Visible;
                btnremove.Visibility = Visibility.Hidden;
            }
        }
        private void rdOpen_Checked(object sender, RoutedEventArgs e)
        {
            if (rdOpen.IsChecked == true)
            {
                bnresolve.Visibility = Visibility.Visible;
                btnremove.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        private void PolicyDetailEffDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolicyDefaultTrackFromDate.SelectedDate == null&&PolicyDetailEffDate.SelectedDate!=null)
            {
                PolicyDefaultTrackFromDate.SelectedDate = PolicyDetailEffDate.SelectedDate;
            }
        }

        private void txtPremium_TextChanged(object sender, TextChangedEventArgs e)
        {
            string str = txtPremium.Text;
            if (string.IsNullOrWhiteSpace(str) || string.IsNullOrEmpty(str))
            {
                txtPremium.Text = "0";
            }
           
        }

        private void lblTermDate_MouseEnter(object sender, MouseEventArgs e)
        {
            //call the method to update auto term date
            VMInstances.OptimizedPolicyManager.OnShowAutoTermDate(); 
            this.Pops.IsOpen = true;
            
        }

        private void lblTermDate_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Pops.IsOpen = false;
        }

        private void txtFirstYear_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtFirstYear.Text =="0.00 %")
            {
                txtFirstYear.SelectAll();
            }
        }

        private void txtRenewal_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtRenewal.Text == "0.00 %")
            {
                txtRenewal.SelectAll();
            }
        }

        private void txtPremium_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtPremium.Text == "$0.00"||txtPremium.Text=="0")
            {
                txtPremium.SelectAll();
            }
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void txtAdvances_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text != "-")
                {
                    Convert.ToInt32(e.Text);
                }

                bool isNumber = IsTextAllowed(txtAdvances.Text);

                if (isNumber)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tbPolicyImportDetail5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void txtFirstYear_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnImportPolicy_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog objOpenFileDialog = new Microsoft.Win32.OpenFileDialog();
            objOpenFileDialog.DefaultExt = ".xlsx";
            objOpenFileDialog.Filter = "Excel sheet (.xlsx)|*.xlsx| All files (*)|*.*";

            if (objOpenFileDialog.ShowDialog() == true)
            {
                Dispatcher.BeginInvoke((Action)(() => run(objOpenFileDialog.FileName)), null);
            }
        }

        void run(string f)
        {
            w = new WaitDialog();
            if (w.Visibility != System.Windows.Visibility.Visible)
            {
                // w.txtDialog.Text = "Please wait..";
                w.ResizeMode = System.Windows.ResizeMode.NoResize;
                Dispatcher.BeginInvoke((Action)(() => w.ShowDialog()), null);
            }
            System.ComponentModel.BackgroundWorker bg = new System.ComponentModel.BackgroundWorker();
            bg.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate(object s, DoWorkEventArgs ea)
            {
                ea.Result = VMInstances.OptimizedPolicyManager.DoImportPolicy(f);
            });
            bg.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bg.RunWorkerAsync();
        }

        void bg_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show(e.Result.ToString());
            //Application.Current.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(Refresh),null);
            w.Visibility = System.Windows.Visibility.Hidden;
            w.Close(); w = null; 
            Mouse.OverrideCursor = Cursors.Arrow;
            Dispatcher.BeginInvoke((Action)(() => VMInstances.OptimizedPolicyManager.AfterImport(e.Result as string)), null);
            this.UpdateLayout();
        }
        

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveDialog.FilterIndex = 1;

            if (saveDialog.ShowDialog() == true)
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                
                //Generating columns to datatable:
                foreach (Microsoft.Windows.Controls.DataGridColumn column in dgCommissions.Columns)
                    dt.Columns.Add(column.Header.ToString(), typeof(string));

                int i = 0; 
                foreach (var item in dgCommissions.ItemsSource)
                {
                    int j  = 0;
                    VM.MyAgencyVaultSvc.PolicyPaymentEntriesPost obj = item as VM.MyAgencyVaultSvc.PolicyPaymentEntriesPost;
                    dt.Rows.Add();
                    dt.Rows[i][j++] = Convert.ToDateTime(obj.InvoiceDate).ToShortDateString();
                    dt.Rows[i][j++] = String.Format("{0:C}", obj.PaymentRecived);
                    dt.Rows[i][j++] = String.Format("{0:0.00}" + "%", obj.CommissionPercentage);
                    dt.Rows[i][j++] = obj.NumberOfUnits;
                    dt.Rows[i][j++] = String.Format("{0:C}", obj.DollerPerUnit);
                    dt.Rows[i][j++] = String.Format("{0:C}", obj.Fee);
                    dt.Rows[i][j++] = String.Format("{0:0.00}" + "%", obj.SplitPer);
                    dt.Rows[i][j++] = String.Format("{0:C}", obj.TotalPayment);
                    dt.Rows[i][j++] = obj.BatchNumber;
                    dt.Rows[i][j++] = Convert.ToDateTime(obj.EntryDate).ToShortDateString();
                    dt.Rows[i][j++] = obj.Pageno;

                    i++;
                }
                
                ds.Tables.Add(dt);
                GenerateExcel2007(saveDialog.FileName, ds);
                System.Windows.MessageBox.Show("Payments data is successfully imported to '" + saveDialog.FileName + "'");
            }

        }

        void GenerateExcel2007(string p_strPath, DataSet p_dsSrc)
        {
            using (ExcelPackage objExcelPackage = new ExcelPackage())
            {
                foreach (DataTable dtSrc in p_dsSrc.Tables)
                {
                    //Create the worksheet    
                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(dtSrc.TableName);
                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1    
                    objWorksheet.Cells["A1"].LoadFromDataTable(dtSrc, true);
                    //objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
                    objWorksheet.Cells.AutoFitColumns();
                    //Format the header    
                    using (ExcelRange objRange = objWorksheet.Cells["A1:XFD1"])
                    {
                        objRange.Style.Font.Bold = true;
                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //objRange.Style.Fill.BackgroundColor.SetColor(Color.FromA#eaeaea);    
                    }
                }

                //Write it back to the client    
                if (File.Exists(p_strPath))
                    File.Delete(p_strPath);

                //Create excel file on physical disk    
                FileStream objFileStrm = File.Create(p_strPath);
                objFileStrm.Close();

                //Write content to excel file    
                File.WriteAllBytes(p_strPath, objExcelPackage.GetAsByteArray());
            }
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RdbOutgoingPaymentType1_Checked(object sender, RoutedEventArgs e)
        {
            if (rdbIncomingScheduleType3.IsChecked == true && rdbOutgoingPaymentType2.IsChecked == true)
            {
                txtRenewalOutgoing1.Visibility = Visibility.Visible;
                txtFirstYearOutgoing1.Visibility = Visibility.Visible;
                txtSplitOutgoing1.Visibility = Visibility.Visible;
                txtRenewalOutgoing.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing.Visibility = Visibility.Hidden;
                txtSplitOutgoing.Visibility = Visibility.Hidden;
            }
            else
            {
                txtRenewalOutgoing1.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing1.Visibility = Visibility.Hidden;
                txtSplitOutgoing1.Visibility = Visibility.Hidden;
                txtRenewalOutgoing.Visibility = Visibility.Visible;
                txtFirstYearOutgoing.Visibility = Visibility.Visible;
                txtSplitOutgoing.Visibility = Visibility.Visible;
            }
        }

        private void RdbOutgoingPaymentType2_Checked(object sender, RoutedEventArgs e)
        {
            if (rdbIncomingScheduleType3.IsChecked == true && rdbOutgoingPaymentType2.IsChecked == true)
            {
                txtRenewalOutgoing1.Visibility = Visibility.Visible;
                txtFirstYearOutgoing1.Visibility = Visibility.Visible;
                txtSplitOutgoing1.Visibility = Visibility.Visible;
                txtRenewalOutgoing.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing.Visibility = Visibility.Hidden;
                txtSplitOutgoing.Visibility = Visibility.Hidden;
            }
            else
            {
                txtRenewalOutgoing1.Visibility = Visibility.Hidden;
                txtFirstYearOutgoing1.Visibility = Visibility.Hidden;
                txtSplitOutgoing1.Visibility = Visibility.Hidden;
                txtRenewalOutgoing.Visibility = Visibility.Visible;
                txtFirstYearOutgoing.Visibility = Visibility.Visible;
                txtSplitOutgoing.Visibility = Visibility.Visible;
            }
        }
    }
}
