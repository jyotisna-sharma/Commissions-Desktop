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
using System.IO;
using System.Windows.Markup;
using MyAgencyVault.WinApp.UserControls;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.WinApp.Common;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml //ARG
    /// Gaurav
    /// </summary>
    public partial class Window1 : Window
    {
        ////CreateClient _client = new CreateClient();
        ////AddEditFormula _objFormula;
        ////TestFormula objtest;
        ////private Mode _mode = Mode.Preview;
        ////LoginVM objLoginVm;
        VMHelpUpdate objHelpUpdate;
        ////FormulaWindowVM _Windowclient;
        ////PayorToolVM viewModelPayor;
        HelpAndUpdatesManager objhelp;
        NewsDetailViewer objNewsDetailViewer;
        ////public Window1()
        ////{
        ////    InitializeComponent();

        ////    myContent.Content = new PolicyManager();
        ////    lblScreenName.Content = "Policy Manager";
        ////    DynamicImageSourse("Images/Icons/cabinet_open.png");
        ////    ////MainWindowVM MainVM = new MainWindowVM();
        ////    ////this.DataContext = MainVM; 
        ////    var PolicyMgr = new PolicyManager();
        ////    var PolicymgrVm = new PolicyManagerVm();
        ////    PolicymgrVm.Dialogue += new PolicyManagerVm.PopUpWindosForClient(PolicymgrVm_Dialogue);
        ////    PolicyMgr.DataContext = PolicymgrVm;
        ////    myContent.Content = PolicyMgr;
        ////}

        
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            //ComboBoxItem cb = (ComboBoxItem)comboBox1.SelectedItem;
            //string uri = Application.Current.StartupUri.ToString().Replace("Window1.xaml", cb.Content.ToString().Trim() + ".xaml");
            ////ResourceDictionary obj = new ResourceDictionary();
            ////obj.Clear();
            ////obj.Source = new Uri(uri,UriKind.RelativeOrAbsolute);
            //string bgColor = "";
            //switch (cb.Content.ToString())
            //{
            //    case "BureauBlack":
            //        bgColor = "#D5D8DD";
            //        break;
            //    case "BureauBlue":
            //        bgColor = "#E0EAF6";
            //        break;
            //    case "ExpressionDark":
            //        bgColor = "#434343";
            //        break;
            //    case "ExpressionLight":
            //        bgColor = "#9F9F9F";
            //        break;
            //    case "ShinyBlue":
            //        bgColor = "#0361B8";
            //        break;
            //    case "ShinyRed":
            //        bgColor = "#801D1B";
            //        break;
            //    case "WhistlerBlue":
            //        bgColor = "#FFFFFF";
            //        break;

            //}

            //DynamicLoadStyles(cb.Content.ToString(), bgColor);

        }

        ///// <summary>
        /////  property to set the login View Model
        ///// </summary>
        //public vml SetLoginVM
        //{
        //    get
        //    {
        //        return objLoginVm;
        //    }

        //    set
        //    {
        //        objLoginVm = value;
        //    }

        //}



        private void DynamicLoadStyles(string uri, string bgColor)
        {
            string fileName;

            fileName = Environment.CurrentDirectory.Replace("\\bin\\Debug", "") + @"\" + uri + ".xaml";

            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    // Read in ResourceDictionary File
                    ResourceDictionary dic =
                       (ResourceDictionary)XamlReader.Load(fs);
                    // Clear any previous dictionaries loaded
                    Resources.MergedDictionaries.Clear();
                    // Add in newly loaded Resource Dictionary
                    Resources.MergedDictionaries.Add(dic);
                    BrushConverter b = new BrushConverter();
                    this.Background = (Brush)b.ConvertFromString(bgColor);
                }
            }
            else
                MessageBox.Show("File: " + uri);
        }
        private void btnPolicyManager_Click(object sender, RoutedEventArgs e)
        {


            var popup = (Action<string>)(msg => MessageBox.Show(msg));
            //    // confirm box  
            var confirm = (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK) == MessageBoxResult.OK);
            Button btnTabClicked = (Button)sender;

            switch (btnTabClicked.Content.ToString().Trim())
            {
                case "Help/Update":
                    DynamicImageSourse("Images/Icons/help2.png");
                    lblScreenName.Content = "Help/Update";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");
                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("backGroundButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                    objHelpUpdate = new VMHelpUpdate();
                    objhelp = new HelpAndUpdatesManager();
                    //objHelpUpdate.OpenDialog += new VMHelpUpdate.OnPopUpHelpUpdate(objHelpUpdate_OpenDialog);
                   
                    objhelp.DataContext = objHelpUpdate;
                    myContent.Content = objhelp;


                    break;
                case "Policy Manager":

                    DynamicImageSourse("Images/Icons/cabinet_open.png");
                    lblScreenName.Content = "Policy Manager";
                    btnPolicyManager.Style = (Style)FindResource("backGroundButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                    ////var PolicyMgr = new PolicyManager();
                    ////var PolicymgrVm = new PolicyManagerVm();
                    ////PolicymgrVm.Dialogue += new PolicyManagerVm.PopUpWindosForClient(PolicymgrVm_Dialogue);

                    ////PolicyMgr.DataContext = PolicymgrVm;

                    ////myContent.Content = PolicyMgr;
                    break;
                case "Configuration":
                    DynamicImageSourse("Images/Icons/ConfigurMgr.png");
                    lblScreenName.Content = "Configuration Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("backGroundButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");
                    VMConfigrationManager ConVm = new VMConfigrationManager(popup, null);
                    //ConfigrationManagerVM ConVm = new ConfigrationManagerVM(popup, confirm);
                    ConfigurationManager Configmgr = new ConfigurationManager();
                    Configmgr.DataContext = ConVm;
                    myContent.Content = Configmgr;

                    break;
                case "Settings":
                    DynamicImageSourse("Images/Icons/Settings.png");
                    lblScreenName.Content = "Settings Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("backGroundButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                    ////var _viewModel = new SettingsManagerVM();
                    ////Settings _objCon = new Settings();
                    ////_objCon.DataContext = _viewModel;
                    ////myContent.Content = _objCon;

                    break;
                case "Statement Manager":
                    DynamicImageSourse("Images/Icons/download.png");
                    lblScreenName.Content = "Statement Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("backGroundButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                    myContent.Content = new DownloadManager();
                    break;
                case "People Manager":
                    DynamicImageSourse("Images/Icons/businesspeople2.png");
                    lblScreenName.Content = "People Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("backGroundButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");
                    ////var viewModel = new PeopleManagerVM();
                    ////PeopleManager objCon = new PeopleManager();
                    ////objCon.DataContext = viewModel;
                    ////myContent.Content = objCon;
                    break;
                case "Billing Manager":
                    DynamicImageSourse("Images/Icons/BillingMgr.png");
                    lblScreenName.Content = "Billing Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("backGroundButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");
                    ////BillingManagerVM Vm = new BillingManagerVM(popup, confirm);
                    ////BillingManager BillingMgr = new BillingManager();
                    ////BillingMgr.DataContext = Vm;
                    ////myContent.Content = BillingMgr;
                    break;
                case "Comp Manager":
                    DynamicImageSourse("Images/Icons/CompMgr.png");
                    lblScreenName.Content = "Comp Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");
                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("backGroundButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");
                    ////CompVM objcomm = new CompVM();
                    ////ucCompManager objcomp = new ucCompManager();
                    ////objcomp.DataContext = objcomm;
                    ////myContent.Content = objcomp;

                    break;
                case "Data Entry Unit":
                    DynamicImageSourse("Images/Icons/Deu.png");
                    lblScreenName.Content = "Comp Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("backGroundButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                    myContent.Content = new DataEntryUnit();
                    break;
                case "Follow Up Manager":
                    DynamicImageSourse("Images/Icons/FollowUpMgr.png");
                    lblScreenName.Content = "Comp Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");

                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("backGroundButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");

                  //  myContent.Content = new FollowUpManager();
                    break;
                case "Payor Tool":
                    DynamicImageSourse("Images/Icons/toolbox.png");
                    lblScreenName.Content = "Comp Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");
                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("backGroundButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("ImageButton");
                    ////viewModelPayor = new PayorToolVM();
                    ////PayorToolManager objConPayor = new PayorToolManager(viewModelPayor);
                    ////viewModelPayor.OpenFormulaWindow += new PayorToolVM.OnOpenFormulaWindow(viewModelPayor_OpenFormulaWindow);
                    ////objConPayor.DataContext = viewModelPayor;
                    ////myContent.Content = objConPayor;
                    break;
                case "Report Manager":
                    DynamicImageSourse("Images/Icons/ReportMgr.png");
                    lblScreenName.Content = "Report Manager";
                    btnPolicyManager.Style = (Style)FindResource("ImageButton");
                    btnBillingManager.Style = (Style)FindResource("ImageButton");
                    btnCompManager.Style = (Style)FindResource("ImageButton");
                    btnConfiguration.Style = (Style)FindResource("ImageButton");
                    btnDEU.Style = (Style)FindResource("ImageButton");
                    btnDownloadManager.Style = (Style)FindResource("ImageButton");
                    btnFollowUp.Style = (Style)FindResource("ImageButton");
                    btnHelpUpdate.Style = (Style)FindResource("ImageButton");
                    btnLogOut.Style = (Style)FindResource("ImageButton");
                    btnPayorTool.Style = (Style)FindResource("ImageButton");
                    btnPeopleManager.Style = (Style)FindResource("ImageButton");
                    btnSettings.Style = (Style)FindResource("ImageButton");
                    btnReportManager.Style = (Style)FindResource("backGroundButton"); ;
                    myContent.Content = new ReportManager();
                    break;
                case "Log Out":

                    //this.DataContext = SetLoginVM;
                    break;
            }




        }

    
        void objHelpUpdate_OpenDialog()
        {
            //objNewsDetailViewer = new NewsDetailViewer(Mode.Preview);
            //VMHelpUpdate objHelpUpdate = new VMHelpUpdate();

            //objHelpUpdate.OpenDialog += new VMHelpUpdate.OnPopUpHelpUpdate(objHelpUpdate_OpenDialog);
            //objHelpUpdate.CloseDialog += new VMHelpUpdate.OnClosePopUpHelpUpdate(obj_CloseDialog);

            //objNewsDetailViewer.DataContext = objHelpUpdate;
            //objhelp.DataContext = objHelpUpdate;
            //objNewsDetailViewer.ShowDialog();
        }


        void obj_CloseDialog()
        {
            ////obj.Hide();
        }

        void viewModelPayor_OpenFormulaWindow()
        {
            ////_Windowclient = new FormulaWindowVM(viewModelPayor);
            ////_objFormula = new AddEditFormula(_Windowclient);
            ////_Windowclient.CloseWindow += new FormulaWindowVM.OnCloseWindow(_Windowclient_CloseWindow);
            ////_Windowclient.TestFormula += new FormulaWindowVM.OnTestFormula(_Windowclient_TestFormula);
            ////_Windowclient.SCheckItem += new FormulaWindowVM.OnCheckItem(_Windowclient_SCheckItem);
            ////_Windowclient.SClickOk += new FormulaWindowVM.OnClickOk(_Windowclient_SClickOk);
            ////_Windowclient.SClickCancel += new FormulaWindowVM.OnClickCancel(_Windowclient_SClickCancel);
            ////_objFormula.DataContext = _Windowclient;
            ////_objFormula.ShowDialog();

        }

        ////void PolicymgrVm_Dialogue(IDialogue Dialogue)
        ////{

        ////    PolicyClientVm _policyclient = new PolicyClientVm(Dialogue);
        ////    _policyclient.CloseEvent += new PolicyClientVm.CloseClient(_policyclient_CloseEvent);
        ////    _client.DataContext = _policyclient;
        ////    _client.ShowDialog();

        ////}

        ////void _Windowclient_SClickOk()
        ////{
        ////    if (_Windowclient.FieldForTestFormula.Contains("+") == true)
        ////    {
        ////        _Windowclient.totalValue += _Windowclient.EnterValue;


        ////    }
        ////    else if (_Windowclient.FieldForTestFormula.Contains("-") == true)
        ////    {

        ////        _Windowclient.totalValue -= _Windowclient.EnterValue;
        ////    }
        ////    else if (_Windowclient.FieldForTestFormula.Contains("*") == true)
        ////    {

        ////        _Windowclient.totalValue *= _Windowclient.EnterValue;
        ////    }
        ////    else if (_Windowclient.FieldForTestFormula.Contains("/") == true)
        ////    {

        ////        _Windowclient.totalValue /= _Windowclient.EnterValue;
        ////    }
        ////    else
        ////    {
        ////        _Windowclient.totalValue = _Windowclient.EnterValue;
        ////    }


        ////    objtest.Hide();

        ////}

        ////void _Windowclient_SClickCancel()
        ////{
        ////    objtest.Close();

        ////}

        void _Windowclient_SCheckItem()
        {

            // _Windowclient.AddExpression( , _Windowclient.FormulaChar);

        }
        ////void _Windowclient_TestFormula()
        ////{
        ////    objtest = new TestFormula(_Windowclient);
        ////    _Windowclient.EnterValue = 0;
        ////    if (_Windowclient.NewSelecetedItem.Count == 0) return;

        ////    foreach (var item in _Windowclient.NewSelecetedItem)
        ////    {

        ////        _Windowclient.FieldForTestFormula = item;
        ////        objtest.ShowDialog();
        ////    }

        ////    _Windowclient.FieldForTestFormula = "Result";
        ////    _Windowclient.EnterValue = _Windowclient.totalValue;
        ////    objtest.ShowDialog();

        ////}

        ////void _Windowclient_CloseWindow()
        ////{
        ////    if (_objFormula != null)
        ////        _objFormula.Hide();
           

        ////}
        ////void _policyclient_CloseEvent(string StrVal)
        ////{
        ////    if (_client != null)
        ////        _client.Close();
        ////    if (StrVal == "New")
        ////        policyManagerDetail();
        ////}

        ////void policyManagerDetail()
        ////{
        ////    var PolicyMgr = new PolicyManager();
        ////    var PolicymgrVm = new PolicyManagerVm();
        ////    PolicymgrVm.Dialogue += new PolicyManagerVm.PopUpWindosForClient(PolicymgrVm_Dialogue);
        ////    PolicyMgr.DataContext = PolicymgrVm;
        ////    myContent.Content = PolicyMgr;
        ////}

        private void DynamicImageSourse(string imgUri)
        {

            BitmapImage Btimg = new BitmapImage();
            Btimg.BeginInit();
            Btimg.UriSource = new Uri(imgUri, UriKind.Relative);
            Btimg.EndInit();
            ImgDynamic.Source = Btimg;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window w = new PolicList();
            w.ShowDialog();
        }



    }

}
