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
using MyAgencyVault.ViewModel.VMLib;
//using GenericExcelDataMapper;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using Microsoft.Win32;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for ConfigurationManager.xaml
    /// </summary>
    public partial class ConfigurationManager : UserControl
    {
        public ConfigurationManager()
        {
            InitializeComponent();
        
        }

        private void btnStatementdate_Click(object sender, RoutedEventArgs e)
        {
            //StatementDate objStatement = new StatementDate();
            //objStatement.Show();
        }

        private void btnNewProduct_Click(object sender, RoutedEventArgs e)
        {
           ProName.Focus();
           
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VMInstances.ConfigurationManager != null)
            {
                VMInstances.ConfigurationManager.VmGenericDataMapper.OnFileDblClick();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
           // ExcelTemplate control = (ExcelTemplate)dataMapperHost.Child;
          //  string colNo = control.getColumnNumber();
           // string colName = control.getColumnName();
            //if (VMInstances.ConfigurationManager != null)
            //{
            //    VMInstances.ConfigurationManager.VmGenericDataMapper.OnPasteExcelField(colNo, colName);
            //}
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (VMInstances.ConfigurationManager != null)
            {
                VMInstances.ConfigurationManager.VmGenericDataMapper.OnClearExcelField();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (VMInstances.ConfigurationManager != null)
            //{
            //    ExcelTemplate control = (ExcelTemplate)dataMapperHost.Child;
            //    string xlColumnList = control.GetXlsFileColumnList(VMInstances.ConfigurationManager.VmGenericDataMapper.ColumnHeaderIndex);
            //    VMInstances.ConfigurationManager.VmGenericDataMapper.OnSaveTemplate(xlColumnList);
            //}
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog objOpenFile = new OpenFileDialog();
            objOpenFile.DefaultExt = ".xls";
            objOpenFile.Filter = "Excel documents (.xls)|*.xls";

            if (objOpenFile.ShowDialog() == true)
            {
                //ExcelTemplate control = (ExcelTemplate)dataMapperHost.Child;
                //control.ExcelMapper_FileDownloaded(objOpenFile.FileName, null);
            }
        }

        private void cmbScheduletype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType schType = e.AddedItems[0] as MyAgencyVault.VM.MyAgencyVaultSvc.PolicyIncomingScheduleType;
                Binding binding = null;

                BindingOperations.ClearBinding(txtFromRange, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtToRange, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtRate, TextBox.TextProperty);

                switch (schType.ScheduleTypeId)
                {
                    case 1:
                    case 2:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.FromRange");
                        binding.StringFormat = "c";
                        txtFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.ToRange");
                        binding.StringFormat = "c";
                        txtToRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.Rate");
                        binding.StringFormat = "p";
                        binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
                        txtRate.SetBinding(TextBox.TextProperty, binding);


                        break;

                    case 3:
                    case 4:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.FromRange");
                        txtFromRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.ToRange");
                        txtToRange.SetBinding(TextBox.TextProperty, binding);

                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                    case 5:
                        binding = new Binding();
                        binding.ValidatesOnDataErrors = true;
                        binding.Path = new PropertyPath("NewCovrageSheduleEntry.Rate");
                        binding.StringFormat = "c";
                        txtRate.SetBinding(TextBox.TextProperty, binding);
                        break;
                }
            }
        }

        private void txtDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == "0")
                {
                    if (txtDays.Text.Length == 0)
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    Convert.ToInt32(e.Text);
                }
            }
            catch
            {
                e.Handled = true;
            }            
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
                
    }
}
