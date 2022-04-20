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
using System.Diagnostics;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel.Behaviour;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for DataEntryUnit.xaml
    /// </summary>
    public partial class DataEntryUnit : UserControl
    {
        public DataEntryUnit()
        {
            InitializeComponent();

            //Binding binding = new Binding();
            //binding.ElementName = "TextBox_0";
            //btnPost.SetBinding(EventFocusAttachment.ElementToFocusProperty, binding);

            Binding binding = new Binding();
            binding.ElementName = "TextBox_0";
            btnPost.SetBinding(FocusExtension.IsFocusedProperty, binding);
        }

        public Canvas getCanvas()
        {
            return cancon;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink thisLink = (Hyperlink)sender;
            string navigateUri = thisLink.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;

        }

        private void cancon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (!VMInstances.DeuVM.DeuFormBuilder.ValidateCompSchuduleForEmpty())
                {
                    btnPost.IsEnabled = false;

                }

                else
                {
                    btnPost.IsEnabled = true;
                }
            }
            if (!VMInstances.DeuVM.DeuFormBuilder.ValidateCompSchuduleForEmpty())
            {
                btnPost.IsEnabled = false;

            }

            else
            {
                btnPost.IsEnabled = true;
            }
        }


        /// <summary>
        /// To Provided the back color on the mouse focus on the textbox
        /// </summary>
        /// <param name="t1"></param>
        private void changeTextBoxColor_OnFocus(TextBox t1)
        {

            if (t1.Text.Trim() == "" || t1.Text.Trim() == "0" || t1.Text.Trim() == "$0.00")
            {
                t1.Background = Brushes.LightGray;
            }
        }

        /// <summary>
        /// To make  background transparent of the textbox on lost focus
        /// </summary>
        /// <param name="t1"></param>
        private void changeTextBoxColor_OnFocusLost(TextBox t1)
        {
            //if (t1.Text.Trim() != "" && t1.Text.Trim() != "0" && t1.Text.Trim() != "$0.00")
            //{
            //    t1.Background = Brushes.White;
            //}
            t1.Background = Brushes.White;

        }

        private void txtBatch_GotFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocus(txtBatch);
        }

        private void txtBatch_LostFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocusLost(txtBatch);
        }

        private void txtStatementId_GotFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocus(txtStatementId);
        }

        private void txtStatementId_LostFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocusLost(txtStatementId);
        }

        private ServiceClients _ServiceClients;
        private ServiceClients serviceClients
        {
            get
            {
                if (_ServiceClients == null)
                {
                    _ServiceClients = new ServiceClients();
                }
                return _ServiceClients;
            }
        }

        private ObservableCollection<Statement> _Statementlist;
        public ObservableCollection<Statement> Statementlist
        {
            get
            {
                return _Statementlist;
            }
            set
            {
                _Statementlist = value;

            }
        }

        private void txtCheckAmt_LostFocus(object sender, RoutedEventArgs e)
        {
            bool bValue = false;
            try
            {
                changeTextBoxColor_OnFocusLost(txtCheckAmt);
                string strCheckAmontValue = string.Empty;
                strCheckAmontValue = txtCheckAmt.Text;
                strCheckAmontValue = strCheckAmontValue.Replace("$", "");
                if (Convert.ToDouble(strCheckAmontValue) == 0.00)
                {
                    return;
                }
                else
                {
                    Guid guidId = VMInstances.DeuVM.CurrentPayor.PayorID;
                    Guid guidBatch = VMInstances.DeuVM.CurrentBatch.BatchId;
                    Guid guidStatement = VMInstances.DeuVM.CurrentStatement.StatementID;
                    Statementlist = serviceClients.BatchClient.GetCheckAmount(guidBatch, guidId);
                    Statementlist = new ObservableCollection<Statement>(Statementlist.Where(s => s.StatementID != guidStatement));

                    foreach (var value in Statementlist)
                    {
                        if (Convert.ToDouble(value.CheckAmount) == Convert.ToDouble(strCheckAmontValue))
                        {
                            bValue = true;
                            break;
                        }
                    }
                    if (bValue)
                    {
                        MessageBox.Show("The check amount already appears in this batch for the same payor." + Environment.NewLine + "Please confirm that it is not a duplicate check amount.", "Data Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch
            {
            }
        }

        private void txtCheckAmt_GotFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocus(txtCheckAmt);
        }

        private void txtBalFor_GotFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocus(txtBalFor);
        }

        private void txtBalFor_LostFocus(object sender, RoutedEventArgs e)
        {
            changeTextBoxColor_OnFocusLost(txtBalFor);
        }

        private void txtStatementImage_MouseEnter(object sender, MouseEventArgs e)
        {
            //Newely added code show statement image 
            try
            {
                if (VMInstances.DeuVM == null)
                    return;

                if (VMInstances.DeuVM.PayorTool == null)
                    return;

                if (VMInstances.DeuVM.PayorTool.StatementImageFilePath == null)
                    return;

                VMInstances.DeuVM.ImagePath = VMInstances.DeuVM.PayorTool.StatementImageFilePath;
                VMInstances.DeuVM.DeuFormBuilder.ShowFields();
            }
            catch
            {
            }
        }

        private void txtCheckAmountImage_MouseEnter(object sender, MouseEventArgs e)
        {
            //Newely added code show check image 
            try
            {
                if (VMInstances.DeuVM == null)
                    return;

                if (VMInstances.DeuVM.PayorTool == null)
                    return;

                if (VMInstances.DeuVM.PayorTool.ChequeImageFilePath == null)
                    return;

                VMInstances.DeuVM.ImagePath = VMInstances.DeuVM.PayorTool.ChequeImageFilePath;
                VMInstances.DeuVM.DeuFormBuilder.HideFields();
            }
            catch
            {
            }
        }

        private void cancon_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!VMInstances.DeuVM.DeuFormBuilder.ValidateCompSchuduleForEmpty())
            {
                btnPost.IsEnabled = false;
            }

            else
            {
                btnPost.IsEnabled = true;
            }
        }
        //add by neha
        private void btnPost_GotFocus(object sender, RoutedEventArgs e)
        {
            VMInstances.DeuVM.bindHelpText = string.Empty;
        }
        //add by neha
        private void btnReset_GotFocus(object sender, RoutedEventArgs e)
        {
            VMInstances.DeuVM.bindHelpText = string.Empty;
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void txtPages_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                string strValue = string.Empty;

                if (e.Text != "-")
                    Convert.ToInt32(e.Text);

                bool isNumber = IsTextAllowed(txtPages.Text);

                if (isNumber)
               
                    e.Handled = false;
                
                else
                {
                    e.Handled = true;
                }

                strValue = txtPages.Text;

                if (strValue.Contains("-"))
                {
                    List<string> myList = new List<string>(strValue.Split('-'));
                    if (!string.IsNullOrEmpty(Convert.ToString(myList[0])))
                    {
                        try
                        {
                            Convert.ToInt32(e.Text);
                            int intValue = Convert.ToInt32(Convert.ToString(myList[0]));
                            if (intValue > 0 && intValue < 10000)
                                e.Handled = false;

                            else
                                e.Handled = true;

                        }
                        catch
                        {
                            e.Handled = true;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(Convert.ToString(myList[1])))
                    {
                        try
                        {
                            Convert.ToInt32(e.Text);
                            if (Convert.ToString(myList[1]).Length > 3)
                                e.Handled = true;

                            else
                                e.Handled = false;

                        }
                        catch
                        {
                            e.Handled = true;
                        }
                    }
                   
                }
                else
                {
                    if (e.Text != "-")
                    {
                        if (strValue.Length > 3)                       
                            e.Handled = true;
                        
                        else                        
                            e.Handled = false;
                        
                    }

                }

            }
            catch
            {
                e.Handled = true;
            }
        }

        private void DtPicker_StmtDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
           // DatePicker control = ((e.Source) as DatePicker);
            if (VMInstances.DeuVM.StatementDate != null)
            {
                DateTime? dtOriginal = VMInstances.DeuVM.StatementDate;
                

                if (e.Source != null && e.Source.GetType().Name == "DatePicker")
                {
                    string strDate = e.Source.ToString();
                    DateTime dtNew = DateTime.MinValue;
                    DateTime.TryParse(strDate, out dtNew);

                    if (dtPicker_StmtDate.IsDropDownOpen || (dtOriginal!= null && dtNew != DateTime.MinValue && dtNew.ToShortDateString() != ((DateTime)dtOriginal).ToShortDateString()))
                    {
                        if (MessageBox.Show("Are you sure you want to change the statement date?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            VMInstances.DeuVM.SaveStatementDate(dtPicker_StmtDate.SelectedDate);
                        }
                        else
                        {
                            VMInstances.DeuVM.ResetStatementDate(dtOriginal);
                        }
                    }
                }
            }
        }

        private void DtPicker_StmtDate_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DtPicker_StmtDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            
        }

        private void DtPicker_StmtDate_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
