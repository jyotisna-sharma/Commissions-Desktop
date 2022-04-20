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
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;
using System.Diagnostics;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Reflection;
using MyAgencyVault.VMLib.PolicyManager;
using System.Threading;
using System.Security.Permissions;
using System.Security;
using System.ComponentModel;
using MyAgencyVault.EmailFax;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;  


namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    


    public partial class MainWindow : Window
    {

        IView viewValidation = new ViewValidation();
        LinkPolicyToCommissionDashboad _Linkpolicy;
        AgentPayee _agent;
        frmTermDate _objfrmTermDate;
        PolicyManager PolicyMgr = null;
        PolicyManagerOptimized OptPolicyMgr = null;
        PolicyManagerVm PolicymgrVm = null;
        VMOptimizePolicyManager OptPolicyManagerVM = null;
        AddEditFormula _objFormula;
        VMLoginUser objLoginVm;
        VMHelpUpdate objHelpUpdate = null;
        SettingsManagerVM settingManagerVMobj = null;
        Settings settingCon = null;
        VMStatementManager _ViewDownloadModel;
        DownloadManager _downloadMG;
        PeopleManagerVM peoplemanagerviewmodel;
        PeopleManager peoplemanagerobjcon;
        ConfigurationManager Configmgr;
        BillingManager BillingMgr;
        VmBillingManager billingmanagervm;
        VMDeu vmdeu;
        DataEntryUnit dataentryunit;
        FollowUpManager Follmgr;
        VMFollowUpManager follo;
        PayorToolManager objConPayor;

        VMConfigrationManager ConVm;
        PayorToolVM viewModelPayor;
        HelpAndUpdatesManager objhelp;
        NewsDetailViewer objNewsDetailViewer;
        VMCompManager objcomm;
        ucCompManager objcomp;

        public VMLinkPaymentClientChangeDialog _VMLinkPaymentClientChangeDialog;
        public LinkPaymentClientChangeDialog _LinkPaymentClientChangeDialog = null;
        PolicyClientVm policyclientdialogvm = null;
        CreateClient CreateClientDialog = null;
        CommissionDashBoardVM commissiondashboardvm = null;
        CommissionDashAdjustment commissiondashadjustment = null;
        ReplacePolicy replacePolicy = null;
        CheckedNamedSchedule checkNamedSchedule = null;
        CommissionDashBoardReverse _CommissionDashBoardReverse = null;
        CustomSchedule customSchedule = null;
        CommissionDashIssue _CommissionDashIssue = null;

        AddTemplate _objAddTemplate;

        CommissionDashBoardEditOutGoingPayment _CommissionDashBoardEditOutGoingPayment = null;
        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
                if (_SharedVMData == null)
                {
                    _SharedVMData = VMSharedData.getInstance();
                }

                return _SharedVMData;
            }
        }


        private void GetStreamForWelcometext(string stream, RichTextBox rt)
        {
            Stream st = new MemoryStream(ASCIIEncoding.Default.GetBytes(stream));

            TextRange rtbtext = new TextRange(rt.Document.ContentStart, rt.Document.ContentEnd);
            rtbtext.Load(st, DataFormats.Rtf);

        }

        private static RoutedCommand RefreshCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            MyAgencyVault.VM.CommonItems.DispatchService.DispatchObject = Dispatcher;
            this.CommandBindings.Add(new CommandBinding(RefreshCommand, RefreshCommandHandler, CanRefresh));
            RefreshCommand.InputGestures.Add(new KeyGesture(Key.F5));

            if (VMInstances.vmMainWindow == null) VMInstances.vmMainWindow = new MainWindowVM();
            string displayedModleName = DisplayedModuleName();
            VMInstances.CurrentScreenName = displayedModleName;

            if (!string.IsNullOrEmpty(displayedModleName))
                ModuleChanged(displayedModleName);

            this.Title = "User: " + RoleManager.LoggedInUser + "     Role: " + RoleManager.Role.ToString() + "                                                                       Version: " + System.Configuration.ConfigurationSettings.AppSettings["DisplayVersion"];
           
            // _mainWindowVM.onOpenPolicySearchedWindow += new MainWindowVM.OnPolicySearchWindow(_mainWindowVM_onOpenPolicySearchedWindow);
            VMInstances.vmMainWindow.onOpenPolicySearchedWindow += new MainWindowVM.OnPolicySearchWindow(_mainWindowVM_onOpenPolicySearchedWindow);
            this.DataContext = VMInstances.vmMainWindow;// _mainWindowVM;

        }

        string CurrentScreenName { get; set; }
        void RefreshHanndle(string CurrentScreenName)
        {

            switch (CurrentScreenName)
            {
                case "People Manager":
                    if (VMInstances.PeopleManager != null)
                        VMInstances.PeopleManager.SelectedLicenseeChanged();
                    break;
                case "Settings":
                    if (VMInstances.SettingManager != null)
                        VMInstances.SettingManager.SelectedLicenseeChanged();
                    break;
                case "Report Manager":
                    if (VMInstances.RepManager != null)
                        VMInstances.RepManager.SelectedLicenseeChanged();
                    break;
                case "Policy Manager":
                    if (VMInstances.OptimizedPolicyManager != null)
                    {
                        VMInstances.OptimizedPolicyManager.OnSelectedLicenseeChanged(null, false);
                        //VMInstances.OptimizedPolicyManager.FullRefresh();
                    }
                    break;
            }

        }
        private void RefreshCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            string CurrentScreenName = VMInstances.CurrentScreenName;
            RefreshHanndle(CurrentScreenName);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            string CurrentScreenName = VMInstances.CurrentScreenName;
            switch (CurrentScreenName)
            {
                case "People Manager":
                case "Settings":
                case "Comp Manager":
                case "Report Manager":
                    e.CanExecute = false;
                    break;
                case "Policy Manager":
                    e.CanExecute = true;
                    break;
            }
        }

        private string DisplayedModuleName()
        {
            string displayedModeuleName = string.Empty;

            if (RoleManager.IsNewsToFlash)
            {
                displayedModeuleName = "Help/Update";

                if (VMInstances.vmMainWindow != null)
                    VMInstances.vmMainWindow.TurnOffNewsFlashBit(RoleManager.userCredentialID);
            }
            else
            {
                if (RoleManager.Role == UserRole.DEP)
                {
                    displayedModeuleName = "Data Entry Unit";
                }
                else if (RoleManager.UserPermissions == null)
                {
                    displayedModeuleName = "Policy Manager";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.PolicyManager - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.PolicyManager - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Policy Manager";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.PeopleManager - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.PeopleManager - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "People Manager";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Settings";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.FollowUpManger - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.FollowUpManger - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Follow Up Manager";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.HelpUpdate - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.HelpUpdate - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Help/Update";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.CompManager - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.CompManager - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Comp Manager";
                }
                else if (RoleManager.UserPermissions[(int)MasterModule.ReportManager - 1].Permission == ModuleAccessRight.Read
                    || RoleManager.UserPermissions[(int)MasterModule.ReportManager - 1].Permission == ModuleAccessRight.Write)
                {
                    displayedModeuleName = "Report Manager";
                }
            }

            return displayedModeuleName;
        }

        void _mainWindowVM_onOpenPolicySearchedWindow()
        {

            Window w = new PolicySearched();
            w.DataContext = VMInstances.vmMainWindow;
            w.ShowDialog();


        }

        /// <summary>
        ///  property to set the login View Model
        /// </summary>
        public VMLoginUser SetLoginVM
        {
            get
            {
                return objLoginVm;
            }

            set
            {
                objLoginVm = value;
            }

        }

        private void GetStreamForScreentext(string stream)
        {
            //richTextBox1.Document.Blocks.Clear();
            ////We initialize paragraphs 
            //Paragraph textTitle = new Paragraph();
            ////We will add the strings to the paragraphs as well as bolding the first and justifying the 3rd 

            //textTitle.Inlines.Add(stream);             
            //textTitle.TextAlignment = TextAlignment.Justify;
            //textTitle.FontWeight = FontWeights.ExtraLight;
            ////And here we add all paragraphs to the rich text box 
            //richTextBox1.Document.Blocks.Add(textTitle);


            Stream st = new MemoryStream(ASCIIEncoding.Default.GetBytes(stream));
            TextRange rtbtext = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
            //rtbtext.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            rtbtext.Load(st, DataFormats.Rtf);
        }

        private void btnManager_Click(object sender, RoutedEventArgs e)
        {
            Button btnTabClicked = sender as Button;

            if (btnTabClicked != null)
            {
                string ModuleName = btnTabClicked.Content.ToString().Trim();
                VMInstances.CurrentScreenName = ModuleName;
                ModuleChanged(ModuleName);
                RefreshHanndle(VMInstances.CurrentScreenName);
            }
        }

        private void ModuleChanged(string ModuleName)
        {
            var popup = (Action<string>)(msg => MessageBox.Show(msg));
            var confirm = (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK) == MessageBoxResult.OK);

            switch (ModuleName)
            {
                case "Help/Update":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 View news and updates as well as links to user guides.\line}");

                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;

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
                        if (objHelpUpdate == null && objhelp == null)
                        {
                            objHelpUpdate = new VMHelpUpdate();
                            objhelp = new HelpAndUpdatesManager();
                            objHelpUpdate.OpenDialog += new VMHelpUpdate.OnPopUpHelpUpdate(objHelpUpdate_OpenDialog);

                        }
                        objhelp.DataContext = objHelpUpdate;
                        myContent.Content = objhelp;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Policy Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        Gstartuptext.Visibility = Visibility.Hidden;
                        Gpolicysearch.Visibility = Visibility.Visible;

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
                        //IViewDialog dialogPOlicy = new WaitDialog();

                        //if (VMInstances.OptimizedPolicyManager == null)
                        //{
                          
                        //}
                        OptPolicyMgr = VMUserControl.getOptimizedPolicyManagerUserControl();
                        VMPolicySchedule.ScheduleGrdData = OptPolicyMgr.FindResource("schData") as ScheduleGridData;
                        VMPolicySchedule.OutScheduleGrdData = OptPolicyMgr.FindResource("outSchData") as ScheduleGridData;
                        if (VMInstances.OptimizedPolicyManager == null)
                        {
                            if (policyclientdialogvm == null)
                            {
                                policyclientdialogvm = new PolicyClientVm(viewValidation);
                                policyclientdialogvm.OpenClientEvent += new PolicyClientVm.OpenClient(policyclientdialogvm_OpenClientEvent);
                                policyclientdialogvm.CloseEvent += new PolicyClientVm.CloseClient(policyclientdialogvm_CloseEvent);

                            }

                            if (commissiondashboardvm == null)
                            {
                                commissiondashboardvm = new CommissionDashBoardVM();
                                commissiondashboardvm.OpenCommissionDashBoardEvent += new CommissionDashBoardVM.OpenCommissionDashBoard(commissiondashboardvm_OpenCommissionDashBoardEvent);
                                commissiondashboardvm.CloseCommissionDashBoardEvent += new CommissionDashBoardVM.CloseCommissionDashBoard(commissiondashboardvm_CloseCommissionDashBoardEvent);
                            }
                             //IViewDialog dialogPOlicy = new WaitDialog();
                            VMInstances.OptimizedPolicyManager = new VMOptimizePolicyManager(policyclientdialogvm);
                            //VMInstances.OptimizedPolicyManager.WaitDialog = dialogPOlicy;

                            if (VMInstances.PolicyCommissionVM == null)
                            {
                                return;
                            }
                            VMInstances.PolicyCommissionVM._CommissionDashBoardVM = commissiondashboardvm;
                            VMInstances.OptimizedPolicyManager.OpenAgentWindow += new VMOptimizePolicyManager.popUpAgentWindow(PolicymgrVm_OpenAgentWindow);
                            VMInstances.OptimizedPolicyManager.CloseAgentWindow += new VMOptimizePolicyManager.CloseUpAgentWindow(PolicymgrVm_CloseAgentWindow);
                            VMInstances.OptimizedPolicyManager.OpenReplacedPolicyEvent += new VMOptimizePolicyManager.OpenReplacedPolicy(PolicyManager_OpenReplacedPolicyEvent);
                            VMInstances.OptimizedPolicyManager.CloseReplacedPolicyEvent += new VMOptimizePolicyManager.CloseReplacedPolicy(PolicyManager_CloseReplacedPolicyEvent);
                            VMInstances.OptimizedPolicyManager.OpenCheckNamedScheduleEvent += new VMOptimizePolicyManager.OpenCheckNamedSchedule(PolicyManager_OpenCheckNamedScheduleEvent);
                            VMInstances.OptimizedPolicyManager.CloseCheckNamedScheduleEvent += new VMOptimizePolicyManager.CloseCheckNamedSchedule(PolicyManager_CloseCheckNamedScheduleEvent);
                            VMInstances.PolicyCommissionVM.OpenReverseCommissionDashBoardEvent += new VMPolicyCommission.OpenReverseCommissionDashBoard(PolicyManager_OpenReverseCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.CloseReverseCommissionDashBoardEvent += new VMPolicyCommission.CloseReverseCommissionDashBoard(PolicyManager_CloseReverseCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.OpenIssueCommissionDashBoardEvent += new VMPolicyCommission.OpenIssueCommissionDashBoard(PolicyManager_OpenIssueCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.CloseIssueCommissionDashBoardEvent += new VMPolicyCommission.CloseissueCommissionDashBoard(PolicyManager_CloseIssueCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.OpenEditOutGoingPaymentCommissionDashBoardEvent += new VMPolicyCommission.OpenEditOutGoingPaymentCommissionDashBoard(PolicyManager_OpenEditOutGoingPaymentCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.CloseEditOutGoingCommissionDashBoardEvent += new VMPolicyCommission.CloseEditOutGoingCommissionDashBoard(PolicyManager_CloseEditOutGoingCommissionDashBoardEvent);
                            VMInstances.PolicyCommissionVM.LinkPolicyToCommissionDashboadWindow += new VMPolicyCommission.LinkPolicyToCommissionDashboad(PolicyCommissionVM_LinkPolicyToCommissionDashboadWindow);
                            VMInstances.OptimizedPolicyManager.OpenImportPolicy += new VMOptimizePolicyManager.delgateOpenImportPolicy(OptimizedPolicyManager_OpenImportPolicy);
                            VMInstances.OptimizedPolicyManager.OpenCustomScheduleEvent += OptimizedPolicyManager_OpenCustomScheduleEvent;
                            VMInstances.OptimizedPolicyManager.CloseCustomScheduleEvent += new VMOptimizePolicyManager.CloseCustomSchedule(PolicyManager_CloseCustomScheduleEvent);

                        }
                        else
                        {
                            //VMInstances.OptimizedPolicyManager.Refresh();
                        }

                        OptPolicyMgr.DataContext = VMInstances.OptimizedPolicyManager;
                        myContent.Content = OptPolicyMgr;
                        OptPolicyMgr.btnNew.Focus();

                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Configuration":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Manage payors, contacts,carriers and products.\line}");

                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;

                        DynamicImageSourse("Images/Icons/ConfigurMgr.png");
                        //lblScreenName.FontWeight = FontWeights.Normal;
                        //lblScreenName.FontSize = 12;
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

                        Configmgr = VMUserControl.getConfigurationManagerUserControl();
                        ScheduleGridData schData1 = Configmgr.FindResource("schGridData") as ScheduleGridData;

                        if (VMInstances.ConfigurationManager == null)
                        {
                            //ConVm = VMInstances.ConfigurationManager = new VMConfigrationManager(popup, confirm, schData1);
                            ConVm = VMInstances.ConfigurationManager = new VMConfigrationManager(popup, schData1);

                            VMInstances.ConfigurationManager.OnPopupStatementDate += new VMConfigrationManager.PopupStatementDate(ConVm_OnPopupStatementDate);
                            VMInstances.ConfigurationManager.OnClosePopusStatementDate += new VMConfigrationManager.ClosePopupStatmentDate(ConVm_OnClosePopusStatementDate);
                        }

                        ConVm.Refresh();

                        Configmgr.DataContext = ConVm;
                        myContent.Content = Configmgr;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Settings":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {

                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Manage custom payors,carriers and products.Add specific notes for each payor including broker numbers.System payors are managed by CommissionsDept.\line}");

                        //GetStreamForScreentext("Manage custom payors,carriers and products.Add specific notes for each payor including broker numbers.System payors are managed by CommissionsDept.");

                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;


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

                        if (VMInstances.SettingManager == null)
                            VMInstances.SettingManager = new SettingsManagerVM();

                        settingManagerVMobj = VMInstances.SettingManager;
                        //settingManagerVMobj.Refresh();

                        if (settingManagerVMobj.RefreshRequired)
                        {
                            settingManagerVMobj.RefreshRequired = false;
                            settingManagerVMobj.Refresh();
                        }

                        settingCon = VMUserControl.getSettingManagerUserControl();
                        settingCon.DataContext = settingManagerVMobj;
                        myContent.Content = settingCon;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Statement Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Download online statements from payors\rquote websites.\line}");
                        //GetStreamForScreentext("Download online statements from payors\rquote websites.");
                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;


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

                        if (VMInstances.DownloadManagerVM == null)
                        {
                            VMInstances.DownloadManagerVM = new VMStatementManager();
                            VMInstances.DownloadManagerVM.OpenPopUpLaunchWebSite += delegate
                            {
                                LaunchWebSite objLchWebSite = new LaunchWebSite(_ViewDownloadModel);
                                if (objLchWebSite.IsActive)
                                {
                                    objLchWebSite.ShowDialog();
                                }

                            };
                        }

                        _ViewDownloadModel = VMInstances.DownloadManagerVM;

                        _ViewDownloadModel.Refresh();

                        _downloadMG = VMUserControl.getDownloadUserControl();

                        _downloadMG.DataContext = _ViewDownloadModel;
                        myContent.Content = _downloadMG;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "People Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {

                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Manage users, contact information, payees, default settings,passwords and permissions.\line}");

                        Gstartuptext.Visibility = Visibility.Visible;

                        Gpolicysearch.Visibility = Visibility.Hidden;

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

                        if (VMInstances.PeopleManager == null)
                        {
                            VMInstances.PeopleManager = new PeopleManagerVM(viewValidation);
                        }

                        peoplemanagerviewmodel = VMInstances.PeopleManager;
                        VMInstances.PeopleManager.OpenImportPolicy += new PeopleManagerVM.delgateOpenImportPolicy(PeopleManager_OpenImportPolicy);

                        if (peoplemanagerviewmodel.RefreshRequired)
                        {
                            peoplemanagerviewmodel.RefreshRequired = false;
                            peoplemanagerviewmodel.Refresh();
                        }


                        peoplemanagerobjcon = VMUserControl.getPeopleManagerUserControl();
                        peoplemanagerobjcon.DataContext = peoplemanagerviewmodel;
                        myContent.Content = peoplemanagerobjcon;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;

                case "Billing Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Manage billing functions.\line}");

                        Gstartuptext.Visibility = Visibility.Visible;

                        Gpolicysearch.Visibility = Visibility.Hidden;

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

                        BillingMgr = VMUserControl.getBillingManagerUserControl();
                        if (VMInstances.BillingManager == null)
                        {
                            ObjectDataProvider objectData = BillingMgr.FindResource("serCharges") as ObjectDataProvider;
                            ServiceChargesType chargeTypes = objectData.Data as ServiceChargesType;
                            Products prods = BillingMgr.FindResource("pds") as Products;

                            VMInstances.BillingManager = new VmBillingManager(popup, confirm, chargeTypes, prods, viewValidation);

                            VMInstances.BillingManager.OpenFormulaWindow += new VmBillingManager.OnOpenFormulaWindow(BillingManager_OpenFormulaWindow);

                        }

                        // viewModelPayor.OpenFormulaWindow += new PayorToolVM.OnOpenFormulaWindow(viewModelPayor_OpenFormulaWindow);

                        billingmanagervm = VMInstances.BillingManager;

                        billingmanagervm.OpenAddTemplate += new VmBillingManager.OnOpenAddTemplate(billingmanagervm_OpenAddTemplate);
                        billingmanagervm.onOpenPhraseSearchedWindow += new VmBillingManager.OnPolicySearchPhraseWindow(billingmanagervm_onOpenPhraseSearchedWindow);

                        billingmanagervm.OnEditAndDisplayWindow += new VmBillingManager.delEditAndDisplayWindow(billingmanagervm_OnEditAndDisplayWindow);

                        BillingMgr.DataContext = billingmanagervm;
                        myContent.Content = BillingMgr;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Comp Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Batch manager allows you to upload new batches and view the status/details of a batch.Link Payment to Policies allows you to assign payments to an existing policy or create new policies from payments.\line}");

                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;
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

                        if (VMInstances.CompManager == null)
                        {
                            if (_VMLinkPaymentClientChangeDialog == null)
                                _VMLinkPaymentClientChangeDialog = new VMLinkPaymentClientChangeDialog();

                            VMInstances.CompManager = new VMCompManager(_VMLinkPaymentClientChangeDialog);
                            _VMLinkPaymentClientChangeDialog._VMCompManager = VMInstances.CompManager;

                            _VMLinkPaymentClientChangeDialog.PopupLinkedClientOpen += new VMLinkPaymentClientChangeDialog.PopUpWindosForLinkedPolicyClient(_VMLinkPaymentClientChangeDialog_PopupLinkedClientOpen);
                            _VMLinkPaymentClientChangeDialog.PopupLinkedClientClose += new VMLinkPaymentClientChangeDialog.PopUpWindosForLinkedPolicyClient(_VMLinkPaymentClientChangeDialog_PopupLinkedClientClose);
                        }

                        objcomm = VMInstances.CompManager;

                        objcomm.Refresh();

                        objcomp = VMUserControl.getCompManagerUserControl();

                        objcomp.DataContext = objcomm;
                        myContent.Content = objcomp;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Data Entry Unit":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Enter commission statement data.\line}");
                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;
                        DynamicImageSourse("Images/Icons/Deu.png");
                        lblScreenName.Content = "Data Entry Unit";
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
                        dataentryunit = VMUserControl.getDEUUserControl();
                        if (VMInstances.DeuVM == null)
                        {
                            vmdeu = new VMDeu(dataentryunit.getCanvas());
                            VMInstances.DeuVM = vmdeu;
                        }
                        else
                        {
                            vmdeu = VMInstances.DeuVM;
                            vmdeu.Refresh();
                        }
                        dataentryunit.DataContext = vmdeu;
                        myContent.Content = dataentryunit;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Follow Up Manager":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 View policies requiring follow up with the payors for missing and incorrect payments.\line}");
                        //GetStreamForScreentext("View policies requiring follow up with the payors for missing and incorrect payments.");
                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;
                        DynamicImageSourse("Images/Icons/FollowUpMgr.png");
                        lblScreenName.Content = "Follow Up Manager";
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
                        Follmgr = VMUserControl.getFollowUpUserControl();

                        ObjectDataProvider objectData1 = Follmgr.FindResource("staticmastercategory") as ObjectDataProvider;
                        StaticMasterCategory smca = objectData1.Data as StaticMasterCategory;

                        objectData1 = Follmgr.FindResource("staticmasterstatus") as ObjectDataProvider;
                        StaticMasterStatus smst = objectData1.Data as StaticMasterStatus;

                        objectData1 = Follmgr.FindResource("staticmasterresult") as ObjectDataProvider;
                        StaticMasterResult smre = objectData1.Data as StaticMasterResult;

                        objectData1 = Follmgr.FindResource("staticmasterreason") as ObjectDataProvider;
                        StaticMasterReason smrea = objectData1.Data as StaticMasterReason;

                        if (VMInstances.FollowUpVM == null)
                        {
                            follo = new VMFollowUpManager(smca, smst, smre, smrea);
                            VMInstances.FollowUpVM = follo;
                        }
                        else
                            follo = VMInstances.FollowUpVM;

                        follo.Refresh();
                        Follmgr.DataContext = follo;
                        myContent.Content = Follmgr;

                        if (VMInstances.FollowUpVM != null)
                        {
                            VMInstances.FollowUpVM.openTermDateEvent += new VMFollowUpManager.openTermDate(FollowUpVM_openTermDateEvent);
                            VMInstances.FollowUpVM.CloseEvent += new VMFollowUpManager.ClosetermWindows(FollowUpVM_CloseEvent);
                        }
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Payor Tool":
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Create data entry template for a payor.\line}");
                        //GetStreamForScreentext("Create data entry template for a payor.");

                        Gstartuptext.Visibility = Visibility.Visible;
                        Gpolicysearch.Visibility = Visibility.Hidden;

                        DynamicImageSourse("Images/Icons/toolbox.png");
                        lblScreenName.Content = "Payor Tool";
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

                        if (VMInstances.PayorToolVM == null)
                        {
                            viewModelPayor = new PayorToolVM();
                            VMInstances.PayorToolVM = viewModelPayor;
                        }
                        else
                            viewModelPayor = VMInstances.PayorToolVM;


                        viewModelPayor.Refresh();

                        objConPayor = VMUserControl.getPayorToolUserControl();

                        viewModelPayor.OpenFormulaWindow += new PayorToolVM.OnOpenFormulaWindow(viewModelPayor_OpenFormulaWindow);

                        viewModelPayor.OpenAddTemplate += new PayorToolVM.OnOpenAddTemplate(viewModelPayor_OpenAddTemplate);

                        objConPayor.DataContext = viewModelPayor;
                        myContent.Content = objConPayor;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case "Report Manager":

                    GetStreamForScreentext(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Tahoma;}}\fs18 Create, view and print reports including payee statements, management reports and payment audit reports.Scroll over the report to read a description.\line}");
                    Gstartuptext.Visibility = Visibility.Visible;
                    Gpolicysearch.Visibility = Visibility.Hidden;
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
                    btnReportManager.Style = (Style)FindResource("backGroundButton");

                    IViewDialog dialog = new WaitDialog();

                    if (VMInstances.RepManager == null)
                    {
                        VMInstances.RepManager = new VMRepManager(dialog);
                    }
                    else
                    {
                        //VMInstances.RepManager.Refresh();
                    }

                    RepManager repManager = VMUserControl.getRepManagerUserControl();
                    repManager.DataContext = VMInstances.RepManager;
                    myContent.Content = repManager;
                    break;
                case "Log Out":

                    VMStatementManager downloadManager = VMInstances.DownloadManagerVM;
                    int InProgressBatchCount = 0;
                    bool Logout = true;

                    if (downloadManager != null && downloadManager.DownloadBatches != null)
                        InProgressBatchCount = downloadManager.DownloadBatches.Where(s => s.DownloadBatch.UploadStatus == UploadStatus.InProgress).ToList().Count;

                    if (InProgressBatchCount != 0)
                    {
                        MessageBoxResult result = MessageBox.Show(@"Some batches are currentlly Downloading/Uploading.\nAre you sure you want to exit?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                            Logout = true;
                        else
                            Logout = false;
                    }

                    if (Logout)
                    {
                        FileIOPermission fp = new FileIOPermission(PermissionState.Unrestricted);
                        fp.Assert();
                        var dirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\MyAgencyVault");
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        File.Create(string.Concat(dirPath, @"\Application.logout"));
                        // Revert the assert when operation is completed
                        CodeAccessPermission.RevertAssert();
                        ((App)Application.Current).IsLogOut = true;
                        SetLoginVM.DoCloseAplication();

                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += new System.ComponentModel.DoWorkEventHandler(sendLogoutMail);
                        //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker1_RunWorkerCompleted);
                        worker.RunWorkerAsync();

                    }

                    break;
            }
        }

      

        private void OptimizedPolicyManager_OpenCustomScheduleEvent(string headerText)
        {
            customSchedule = new CustomSchedule(headerText);
            customSchedule.DataContext = VMInstances.OptimizedPolicyManager;
            customSchedule.ShowDialog();
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

        void sendLogoutMail(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                string strUserRole = Convert.ToString(VMInstances.Login.UserRole);
                string strUserName = Convert.ToString(VMInstances.Login.UserName);
                string strCompany = Convert.ToString(VMInstances.Login.UserInfo.Company);

                if (string.IsNullOrEmpty(strCompany))
                {
                    SendMailToLoginLogout(strUserName, "Access Log: " + strUserName + " ", strCompany);
                }
                else
                {
                    SendMailToLoginLogout(strUserName, "Access Log: " + strUserName + " Of " + strCompany + " ", strCompany);
                }
            }
            catch
            {
            }
        }

        private void SendMailToLoginLogout(string strUserName, string strType, string strLicenseName)
        {
            MailData _MailData = new MailData();
            try
            {
                _MailData.ToMail = "service@commissionsdept.com";
                _MailData.FromMail = "service@commissionsdept.com";
                //serviceClients.FollowupIssueClient.SendLoginLogoutMail(_MailData, strType + "  " + System.DateTime.Now, MailContent(strUserName, strLicenseName));
                serviceClients.FollowupIssueClient.SendNotificationMailAsync(_MailData, strType + "  " + System.DateTime.Now, MailContent(strUserName, strLicenseName));
            }
            catch
            {
            }
        }

        private string MailContent(string strUserName, string strLicenseName)
        {
            string MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                       "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>User Name: " +
                   strUserName + "</td></tr><tr><td colspan='2'>Log out:" +
                   System.DateTime.Now +
                   "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                   + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>"
                   + "</td></tr></table>";

            return MailBody;

        }

        void PolicyManager_CloseEditOutGoingCommissionDashBoardEvent()
        {
            _CommissionDashBoardEditOutGoingPayment.Close();
        }

        void PolicyManager_OpenEditOutGoingPaymentCommissionDashBoardEvent()
        {
            _CommissionDashBoardEditOutGoingPayment = new CommissionDashBoardEditOutGoingPayment();
            _CommissionDashBoardEditOutGoingPayment.DataContext = VMInstances.PolicyCommissionVM;
            _CommissionDashBoardEditOutGoingPayment.ShowDialog();
        }

        void PolicyManager_CloseIssueCommissionDashBoardEvent()
        {
            _CommissionDashIssue.Close();
        }

        void PolicyManager_OpenIssueCommissionDashBoardEvent()
        {

            _CommissionDashIssue = new CommissionDashIssue();
            _CommissionDashIssue.DataContext = VMInstances.PolicyCommissionVM;
            _CommissionDashIssue.ShowDialog();
        }

        void PolicyManager_CloseReverseCommissionDashBoardEvent()
        {
            _CommissionDashBoardReverse.Close();
        }

        void PolicyManager_OpenReverseCommissionDashBoardEvent()
        {

            _CommissionDashBoardReverse = new CommissionDashBoardReverse();
            _CommissionDashBoardReverse.DataContext = VMInstances.PolicyCommissionVM;
            _CommissionDashBoardReverse.ShowDialog();
        }

        void PolicyManager_CloseReplacedPolicyEvent()
        {
            replacePolicy.Close();
        }
        void PolicyManager_CloseCheckNamedScheduleEvent()
        {
            checkNamedSchedule.Close();
        }

        void PolicyManager_OpenReplacedPolicyEvent()
        {
            replacePolicy = new ReplacePolicy();
            replacePolicy.DataContext = VMInstances.OptimizedPolicyManager;
            replacePolicy.ShowDialog();

        }
        void PolicyManager_OpenCheckNamedScheduleEvent()
        {
            checkNamedSchedule = new CheckedNamedSchedule();
            checkNamedSchedule.DataContext = VMInstances.OptimizedPolicyManager;
            checkNamedSchedule.ShowDialog();

        }
        

        void commissiondashboardvm_CloseCommissionDashBoardEvent()
        {
            commissiondashadjustment.Close();
        }


        //void PolicyManager_OpenCustomScheduleEvent()
        //{
        //    //customSchedule = new CustomSchedule();
        //    //customSchedule.DataContext = VMInstances.OptimizedPolicyManager;
        //    //customSchedule.ShowDialog();

        //}


        void PolicyManager_CloseCustomScheduleEvent()
        {
            customSchedule.Close();
        }



        void commissiondashboardvm_OpenCommissionDashBoardEvent()
        {
            if (SharedVMData.UpdateMode == UpdateMode.Edit)
            {
                commissiondashboardvm.PolicySelectedIncomingPaymentCommissionDashBoard = VMInstances.PolicyCommissionVM.PolicySelectedIncomingPaymentCommissionDashBoard;
                commissiondashboardvm.SelectedPolicy = VMInstances.OptimizedPolicyManager.SelectedPolicy;
                
            }
            else if (SharedVMData.UpdateMode == UpdateMode.Add)
            {
                commissiondashboardvm.PolicySelectedIncomingPaymentCommissionDashBoard = new PolicyPaymentEntriesPost();
                commissiondashboardvm.PolicySelectedIncomingPaymentCommissionDashBoard.InvoiceDate = DateTime.Today;
                commissiondashboardvm.SelectedPolicy = VMInstances.OptimizedPolicyManager.SelectedPolicy;
            }
            else if (SharedVMData.UpdateMode == UpdateMode.Delete)
            {
                commissiondashboardvm.PolicySelectedIncomingPaymentCommissionDashBoard = VMInstances.PolicyCommissionVM.PolicySelectedIncomingPaymentCommissionDashBoard;
            }
            // if(commissiondashadjustment==null)
            commissiondashadjustment = new CommissionDashAdjustment();
            commissiondashadjustment.DataContext = commissiondashboardvm;
            commissiondashadjustment.ShowDialog();
        }

        void policyclientdialogvm_CloseEvent()
        {
            if (CreateClientDialog != null)
            {
                CreateClientDialog.Close();
                VMUserControl.CreateClient = null;
            }
        }

        void policyclientdialogvm_OpenClientEvent()
        {
            if (SharedVMData.UpdateMode == UpdateMode.Edit)
            {
                policyclientdialogvm.SelectedClient = VMInstances.OptimizedPolicyManager.SelectedDisplayClient;
            }
            else if (SharedVMData.UpdateMode == UpdateMode.Add)
            {
                policyclientdialogvm.SelectedClient = new Client();
            }

            CreateClientDialog = new CreateClient(policyclientdialogvm);
            VMUserControl.CreateClient = CreateClientDialog;

            CreateClientDialog.DataContext = policyclientdialogvm;
            CreateClientDialog.ShowDialog();
        }

        void _VMLinkPaymentClientChangeDialog_PopupLinkedClientClose()
        {
            _LinkPaymentClientChangeDialog.Close();
        }

        void _VMLinkPaymentClientChangeDialog_PopupLinkedClientOpen()
        {
            _LinkPaymentClientChangeDialog = new LinkPaymentClientChangeDialog(_VMLinkPaymentClientChangeDialog);

            _LinkPaymentClientChangeDialog.DataContext = _VMLinkPaymentClientChangeDialog;
            _LinkPaymentClientChangeDialog.ShowDialog();
        }

        void ConVm_OnClosePopusStatementDate()
        {
            stmtdateobj.Close();
            stmtdateobj = null;
        }

        void PolicyCommissionVM_LinkPolicyToCommissionDashboadWindow()
        {
            _Linkpolicy = new LinkPolicyToCommissionDashboad();
            _Linkpolicy.DataContext = VMInstances.PolicyCommissionVM;
            _Linkpolicy.ShowDialog();
        }

        public static StatementDate stmtdateobj;

        void ConVm_OnPopupStatementDate()
        {
            stmtdateobj = null;
            if (stmtdateobj == null)
            {
                stmtdateobj = new StatementDate(ConVm);
                stmtdateobj.DataContext = ConVm;
                stmtdateobj.ShowDialog();
            }
        }

        void objHelpUpdate_OpenDialog(bool isEditable)
        {
            objNewsDetailViewer = new NewsDetailViewer(isEditable);
            objHelpUpdate.CloseDialog += new VMHelpUpdate.OnClosePopUpHelpUpdate(obj_CloseDialog);
            objNewsDetailViewer.DataContext = objHelpUpdate;
            objhelp.DataContext = objHelpUpdate;
            objNewsDetailViewer.ShowDialog();
        }

        void obj_CloseDialog()
        {
            objNewsDetailViewer.Hide();
        }

        void viewModelPayor_OpenAddTemplate(string strCommandType)
        {
            _objAddTemplate = new AddTemplate("PayorTool", strCommandType);
            _objAddTemplate.DataContext = VMInstances.PayorToolVM;

            if (_objAddTemplate != null)
            {
                //_objAddTemplate.Owner = this;
                _objAddTemplate.ShowDialog();
            }
        }

        void billingmanagervm_OpenAddTemplate(string strCommandType)
        {
            _objAddTemplate = new AddTemplate("ImportTool", strCommandType);
            _objAddTemplate.DataContext = VMInstances.BillingManager;

            if (_objAddTemplate != null)
            {
                //_objAddTemplate.Owner = this;
                _objAddTemplate.ShowDialog();
            }
        }

        void FollowUpVM_openTermDateEvent()
        {
            _objfrmTermDate = new frmTermDate();
            _objfrmTermDate.DataContext = VMInstances.FollowUpVM;
            if (_objfrmTermDate != null)
            {
                _objfrmTermDate.ShowDialog();
            }

        }

        void FollowUpVM_CloseEvent()
        {
            _objfrmTermDate.Hide();
            _objfrmTermDate.Close();
            _objfrmTermDate = null;

        }

        void viewModelPayor_OpenFormulaWindow()
        {
            PayorToolVM payorToolvm = VMInstances.PayorToolVM;
            Formula formula = null;
            if (payorToolvm != null && payorToolvm.CurrentPayorFieldProperty != null)
                formula = payorToolvm.CurrentPayorFieldProperty.CalculationFormula;

            _objFormula = new AddEditFormula(formula, "PayorTool");
            _objFormula.ShowDialog();

        }

        #region for import tool
        void BillingManager_OpenFormulaWindow()
        {
            VmBillingManager objBillingManager = VMInstances.BillingManager;           
            Formula formula = null;
            if (objBillingManager != null && objBillingManager.CurrentPayorToolInfo != null)
                //formula=objBillingManager.CurrentPayorToolInfo.
                _objFormula = new AddEditFormula(formula, "BillingManager");

            _objFormula.ShowDialog();
            if (_objFormula.ImportTool == 1)
            {

                objBillingManager.strformulaExpression = _objFormula.expTextBlock.Text.Trim();

            }



        }
        #endregion

        void PolicymgrVm_OpenAgentWindow()
        {
            _agent = new AgentPayee();
            //VMInstances.OptimizedPolicyManager.CloseAgentWindow += new VMOptimizePolicyManager.CloseUpAgentWindow(PolicymgrVm_CloseAgentWindow);
            //OptPolicyManagerVM.CloseAgentWindow += new VMOptimizePolicyManager.CloseUpAgentWindow(PolicymgrVm_CloseAgentWindow);
            //new PolicyManagerVm.CloseUpAgentWindow(PolicymgrVm_CloseAgentWindow);
            _agent.WindowStyle = WindowStyle.ToolWindow; 
            _agent.DataContext = VMInstances.OptimizedPolicyManager;
            _agent.Owner = Application.Current.MainWindow;
            _agent.ShowDialog();
        }

        void PolicymgrVm_CloseAgentWindow()
        {
            _agent.Close();
        }

        void PolicymgrVm_Dialogue(Client client)
        {
            //var popup = (Action<string>)(msg => MessageBox.Show(msg));
            ////    // confirm box  
            //var confirm = (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK) == MessageBoxResult.OK);
            //PolicyClientVm _policyclient = new PolicyClientVm(Dialogue, popup, confirm, objMasterClient);
            //_policyclient.CloseEvent += new PolicyClientVm.CloseClient(_policyclient_CloseEvent);
            //_client.DataContext = _policyclient;
            //_client.ShowDialog();
        }

        //void _policyclient_CloseEvent(string StrVal)
        //{
        //    if (CreateClientDialog != null)
        //        CreateClientDialog.Close();
        //    if (StrVal == "New")
        //        policyManagerDetail();
        //}

        //void policyManagerDetail()
        //{
        //    var PolicyMgr = new PolicyManager();
        //    ScheduleGridData schData = PolicyMgr.FindResource("schData") as ScheduleGridData;
        //    ScheduleGridData outSchData = PolicyMgr.FindResource("outSchData") as ScheduleGridData;
        //    var PolicymgrVm = new PolicyManagerVm(schData, outSchData);
        //   // PolicymgrVm.Dialogue += new PolicyManagerVm.PopUpWindosForClient(PolicymgrVm_Dialogue);
        //  //  PolicymgrVm.Dialogue += new PolicyManagerVm.PopUpWindosForClient(PolicymgrVm_Dialogue);
        //    PolicyMgr.DataContext = PolicymgrVm;
        //    myContent.Content = PolicyMgr;
        //}

        private void DynamicImageSourse(string imgUri)
        {

            BitmapImage Btimg = new BitmapImage();
            Btimg.BeginInit();
            Btimg.UriSource = new Uri(imgUri, UriKind.Relative);
            Btimg.EndInit();
            ImgDynamic.Source = Btimg;

        }

        void billingmanagervm_onOpenPhraseSearchedWindow()
        {

            Window w = new PhraseSearch();
            w.DataContext = VMInstances.BillingManager;
            w.ShowDialog();
        }

        void billingmanagervm_OnEditAndDisplayWindow()
        {
            Window w = new EditAndDisplay();
            w.DataContext = VMInstances.BillingManager;
            w.ShowDialog();
        }

        public ExecutedRoutedEventHandler OnRefresh { get; set; }

        void OptimizedPolicyManager_OpenImportPolicy()
        {
            ImportPolicy objPolicyImport = new ImportPolicy();
            objPolicyImport.DataContext = VMInstances.OptimizedPolicyManager;
            objPolicyImport.ShowDialog();
        }

        void PeopleManager_OpenImportPolicy()
        {
            ImportAgent objImportAgent = new ImportAgent();
            objImportAgent.DataContext = VMInstances.PeopleManager;
            objImportAgent.ShowDialog();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

    }
}
