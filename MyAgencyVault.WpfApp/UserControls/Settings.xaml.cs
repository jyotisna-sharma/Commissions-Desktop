using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Sql;
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
//using MyAgencyVault.WinApp.AppServices;
using System.Data;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Text.RegularExpressions;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        //#region "For Carrier Tab"
        //DataTable _carrierList;
        //DataTable getCarrierList()
        //{
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("Carrier Name");
        //    dt.Columns.Add("NickName");
        //    //dt.Columns.Add("CreatedBy");
        //    ////dt.Columns.Add("IsTrackMissingMonth");
        //    ////dt.Columns.Add("IsTrackPremiumVariance");
        //    ////dt.Columns.Add("IsTrackPercentageReceive");
        //    ////dt.Columns.Add("IsTrackPaymentStop");
        //    ////dt.Columns.Add("MovingAverageType");
        //    DataRow dr;

        //    dr = dt.NewRow();
        //    dr["Carrier Name"] = "ICICI";
        //    dr["NickName"] = "ici";
        //    //dr["CreatedBy"] = "Rajesh singh";
        //    ////dr["IsTrackMissingMonth"] = "1";
        //    ////dr["IsTrackPremiumVariance"] = "1";
        //    ////dr["IsTrackPercentageReceive"] = "1";
        //    ////dr["IsTrackPaymentStop"] = "1";
        //    ////dr["MovingAverageType"] = "None";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Carrier Name"] = "KODAK";
        //    dr["NickName"] = "k";
        //    //dr["CreatedBy"] = "rajesh singh";
        //    ////dr["IsTrackMissingMonth"] = "1";
        //    ////dr["IsTrackPremiumVariance"] = "1";
        //    ////dr["IsTrackPercentageReceive"] = "1";
        //    ////dr["IsTrackPaymentStop"] = "0";
        //    ////dr["MovingAverageType"] = "Flat";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Carrier Name"] = "Oxford";
        //    dr["NickName"] = "ox";
        //    //dr["CreatedBy"] = "rajesh singh";
        //    ////dr["IsTrackMissingMonth"] = "0";
        //    ////dr["IsTrackPremiumVariance"] = "0";
        //    ////dr["IsTrackPercentageReceive"] = "0";
        //    ////dr["IsTrackPaymentStop"] = "0";
        //    ////dr["MovingAverageType"] = "Commission";
        //    dt.Rows.Add(dr);

        //    return dt;
        //}
        //DataTable CarrieerList
        //{
        //    get
        //    {
        //        if (_carrierList == null)
        //            _carrierList = getCarrierList();
        //        return _carrierList;
        //    }
        //}        
        //#endregion 
        //#region "For Product Tab"
        //DataTable productList;
        //DataTable getProductList()
        //{
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("Product");
        //    dt.Columns.Add("Carrier");
        //    dt.Columns.Add("NickName");
        //    dt.Columns.Add("CreatedBy");
        //    DataRow dr;

        //    dr = dt.NewRow();
        //    dr["Product"] = "Oxford Health Plan";
        //    dr["Carrier"] = "Oxford";
        //    dr["NickName"] = "OHP";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Product"] = "ICICI prudential";
        //    dr["Carrier"] = "ICICI";
        //    dr["NickName"] = "ICI PRU";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Product"] = "ICICI Life Insurance";
        //    dr["Carrier"] = "ICICI";
        //    dr["NickName"] = "ICI LIFE";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Product"] = "ICICI Health Plan";
        //    dr["Carrier"] = "ICICI";
        //    dr["NickName"] = "IHP";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Product"] = "Kodak Child insurance";
        //    dr["Carrier"] = "Kodak";
        //    dr["NickName"] = "Kchild";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["Product"] = "Kodak life insurance";
        //    dr["Carrier"] = "Kodak";
        //    dr["NickName"] = "K life";
        //    dr["CreatedBy"] = "Creater name";
        //    dt.Rows.Add(dr);

        //    return dt;
        //}
        //DataTable ProductList
        //{
        //    get
        //    {
        //        if (productList == null)
        //            productList = getProductList();
        //        return productList;
        //    }
        //}
        //void fillProductInfoToControl(DataRow dr)
        //{
        //    this.txtProductName.Text = dr["Product"].ToString();
        //    this.txtProductNickName.Text = dr["NickName"].ToString();
        //}
        //#endregion 
        public Settings()
        {
            InitializeComponent();
            VMInstances.SettingManager.SaveScheduleEvent += SettingManager_SaveScheduleEvent;

        }

        private void SettingManager_SaveScheduleEvent()
        {
            CustomPrompt dialog = new CustomPrompt();
            if (dialog.ShowDialog() == true)
            {
                VMInstances.SettingManager.SaveIncomingSchedule(dialog.SelectedOption);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
                listBox.Items.Refresh();
        }

        private void TbSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void rdbIncomingScheduleType1_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);
          //  BindingOperations.ClearBinding(txtFirstYearSchedule, TextBox.TextProperty);
          //  BindingOperations.ClearBinding(txtRenewalYearSchedule, TextBox.TextProperty);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("FirstYearPer");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);
          //  txtFirstYearSchedule.SetBinding(TextBox.TextProperty, binding);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("RenewalPer");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtRenewal.SetBinding(TextBox.TextProperty, binding);
            // txtRenewalYearSchedule.SetBinding(TextBox.TextProperty, binding);

            //Bind grid headers to selected mode
            grdGraded.Columns[2].Header = "% of Premium";
            grdNonGraded.Columns[1].Header = "% of Premium";
        }
        private void NamedIncomingScheduleTypepercent_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;
            BindingOperations.ClearBinding(txtFirstYearSchedule, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewalYearSchedule, TextBox.TextProperty);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("NamedScheduleFirstYearPer");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
            txtFirstYearSchedule.SetBinding(TextBox.TextProperty, binding);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("NamedScheduleRenewalPer");
            binding.StringFormat = "p";
            binding.Converter = this.TryFindResource("percentageConv") as MyAgencyVault.ViewModel.Converters.PercentageConverter;
           // txtRenewal.SetBinding(TextBox.TextProperty, binding);
            txtRenewalYearSchedule.SetBinding(TextBox.TextProperty, binding);

            //Bind grid headers to selected mode
            grdNamedGraded.Columns[2].Header = "% of Premium";
            grdNamedNonGraded.Columns[1].Header = "% of Premium";
        }
        private void NamedIncomingScheduleTypeperHead_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;
            BindingOperations.ClearBinding(txtFirstYearSchedule, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewalYearSchedule, TextBox.TextProperty);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("NamedScheduleFirstYearPer");
            binding.StringFormat = "c";
            txtFirstYearSchedule.SetBinding(TextBox.TextProperty, binding);
            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("NamedScheduleRenewalPer");
            binding.StringFormat = "c";
            txtRenewalYearSchedule.SetBinding(TextBox.TextProperty, binding);

            //Bind grid headers to selected mode
            grdNamedGraded.Columns[2].Header = "Per Head";
            grdNamedNonGraded.Columns[1].Header = "Per Head";
        }
        
        private void rdbIncomingScheduleType3_Checked(object sender, RoutedEventArgs e)
        {
            Binding binding = null;

            BindingOperations.ClearBinding(txtFirstYear, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtRenewal, TextBox.TextProperty);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("FirstYearPer");
            binding.StringFormat = "c";
            txtFirstYear.SetBinding(TextBox.TextProperty, binding);

            binding = new Binding();
            binding.ValidatesOnDataErrors = true;
            binding.Path = new PropertyPath("RenewalPer");
            binding.StringFormat = "c";
            txtRenewal.SetBinding(TextBox.TextProperty, binding);

            grdGraded.Columns[2].Header = "Per Head";
            grdNonGraded.Columns[1].Header = "Per Head";
        }
        private void txtFirstYear_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtFirstYear.Text == "0.00 %")
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
        private void txtFirstYearSchedule_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtFirstYear.Text == "0.00 %")
            {
                txtFirstYear.SelectAll();
            }
        }

        private void txtRenewalYearSchedule_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtRenewal.Text == "0.00 %")
            {
                txtRenewal.SelectAll();
            }
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
        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            grdList.UnselectAll();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RdbCustomMode2_Checked(object sender, RoutedEventArgs e)
        {
            grdGraded.Visibility = Visibility.Collapsed;
            grdNonGraded.Visibility = Visibility.Visible;
        }

        private void RdbCustomMode1_Checked(object sender, RoutedEventArgs e)
        {
            grdGraded.Visibility = Visibility.Visible;
            grdNonGraded.Visibility = Visibility.Collapsed;
        }

        private void RdbCustomModeNamed2_Checked(object sender, RoutedEventArgs e)
        {
            grdNamedGraded.Visibility = Visibility.Collapsed;
            grdNamedNonGraded.Visibility = Visibility.Visible;
        }

        private void RdbCustomModeNamed1_Checked(object sender, RoutedEventArgs e)
        {
            grdNamedGraded.Visibility = Visibility.Visible;
            grdNamedNonGraded.Visibility = Visibility.Collapsed;
        }

        private void GrdGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value = (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" || value == null)
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }

        private void GrdNamedGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value = (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" || value == null)
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }

        private void GrdNamedNonGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value = (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" || value == null)
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }

        private void GrdNonGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value = (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" || value == null)
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }




        //private void Hyperlink_Click(object sender, RoutedEventArgs e)
        //{
        //    DataGridRow row = new DataGridRow();
        //    row.IsEnabled = true;

        //    grdGraded.Items.Add(row);
        //    grdGraded.UpdateLayout();
        //}
        ////private void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    if (VMInstances.SettingManager.IsExistingScheduleConfiguration())
        //    {
        //        MessageBoxResult result = System.Windows.MessageBox.Show("Schedule already exists for the given Payor-Product configuration. Are you sure you want to overwrite schedule settings for this configuration?", "Overwrite Warning", MessageBoxButton.YesNo);
        //        if (result == MessageBoxResult.Yes)
        //        {
        //            CustomPrompt dialog = new CustomPrompt();
        //            if (dialog.ShowDialog() == true)
        //            {
        //                VMInstances.SettingManager.SaveIncomingSchedule(dialog.SelectedOption);
        //            }

        //        }
        //    }
        //    else
        //    {
        //        CustomPrompt dialog = new CustomPrompt();
        //        if (dialog.ShowDialog() == true)
        //        {
        //            VMInstances.SettingManager.SaveIncomingSchedule(dialog.SelectedOption);
        //        }
        //    }

        //}
        //private void bindPayors()
        //{
        //    //ListBoxItem li = new ListBoxItem();
        //    //li.Content = "LIC";

        //    //ListBoxItem lic = new ListBoxItem();
        //    //lic.Content = "ICICI";
        //    //listBox1.Items.Add(li);
        //    //listBox1.Items.Add(lic);
        //    //PayorsClient objPayorsClient = new PayorsClient();
        //    //comboBox3.Items.Clear();
        //    //cmbGrpRegion.Items.Clear();
        //    //List<Region> lstRegion = objPayorsClient.GetRegions().ToList<Region>();
        //    //foreach (Region r in lstRegion)
        //    //{
        //    //    ListBoxItem liRegions = new ListBoxItem();
        //    //    ListBoxItem liRegions1 = new ListBoxItem();
        //    //    liRegions.Content = r.RegionName;
        //    //    liRegions1.Content = r.RegionName;
        //    //    comboBox3.Items.Add(liRegions);
        //    //    cmbGrpRegion.Items.Add(liRegions1);
        //    //}

        //}
        //private void Button_Click(object sender, RoutedEventArgs e)
        //{

        //}
        //private void bindProductData()
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //    }
        //}
        //private void btnMore_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //string btnText = btnMore.Content.ToString();
        //        //switch (btnText)
        //        //{
        //        //    case "More":
        //        //        //grdAddProducts.Visibility = Visibility.Visible;
        //        //        //btnAddtolist.Visibility = Visibility.Visible;
        //        //        //btnMore.Content = "Hide Products";
        //        //        break;
        //        //    case "Hide Products":
        //        //        grdAddProducts.Visibility = Visibility.Hidden;
        //        //        btnAddtolist.Visibility = Visibility.Hidden;
        //        //        btnMore.Content = "More";
        //        //        break;
        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //    }
        //}

        ////////private void TabItem_Loaded(object sender, RoutedEventArgs e)
        ////////{

        ////////    MyAgencyVault.WinApp.AppServices.ConfigurationsClient objService = new MyAgencyVault.WinApp.AppServices.ConfigurationsClient();



        ////////    var    arrMasterCarriers = (from c in objService.GetCarrierListByPayor(187,159,true) 
        ////////                           select c).ToList<MasterCarrier>();


        ////////    var vCarrierOptions = from c in arrMasterCarriers
        ////////                          select new { c.CarrierId, c.CarrierName, c.NickName};


        ////////    ////grdCarrAgentsSett.ItemsSource = vCarrierOptions.ToArray();


        ////////     MyAgencyVault.WinApp.AppServices.ConfigurationsClient objService1 = new MyAgencyVault.WinApp.AppServices.ConfigurationsClient();

        ////////   var     arrMasterProducts = (from c in objService1.GetProductListByPayorCarrier(187, 153, 159,  true )

        ////////                             select c).ToList<MasterProduct>();


        ////////    var vProductOptions = from c in arrMasterProducts
        ////////                          select new { c.ProductId, c.ProductName, c.NickName, MasterProductId = c.ProductId };


        ////////    grdProducts.ItemsSource = vProductOptions.ToArray();




        ////////}



        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    ////bindPayors();
        //    //////grdCarrAgentsSett.ItemsSource = CarrieerList.DefaultView;   
        //    ////////
        //    ////////bind carrier.
        //    ////////
        //    //////List<string> lstc = new List<string>();
        //    //////foreach (DataRow dr in this.CarrieerList.Rows)
        //    //////{
        //    //////    lstc.Add(dr["Carrier Name"].ToString());
        //    //////}
        //    ////////this.cmbCarriers.ItemsSource = lstc;

        //    ////////bind product;
        //    //////this.grdProducts.ItemsSource = this.ProductList.DefaultView;
        //    //////fillProductInfoToControl(this.ProductList.Rows[0]);
        //    ////////
        //    ////////bind agent product.
        //    ////////
        //}

        //private void rdbWebsite_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (rdbWebsite.IsChecked == true)
        //    {
        //        stkPnlWebSite.Visibility = Visibility.Visible;
        //        bdrWebSite.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        stkPnlWebSite.Visibility = Visibility.Hidden;
        //        bdrWebSite.Visibility = Visibility.Hidden;
        //    }

        //}

        //private void rdbPaper_Checked(object sender, RoutedEventArgs e)
        //{
        //    stkPnlWebSite.Visibility = Visibility.Hidden;
        //    bdrWebSite.Visibility = Visibility.Hidden;

        //}

        //private void rdbEDI_Checked(object sender, RoutedEventArgs e)
        //{
        //    stkPnlWebSite.Visibility = Visibility.Hidden;
        //    bdrWebSite.Visibility = Visibility.Hidden;
        //}
    }
}
