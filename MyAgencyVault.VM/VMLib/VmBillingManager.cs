using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using System.Workflow.ComponentModel;
using System.Windows;
using MyAgencyVault.VM.CommonItems;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using MyAgencyVault.ViewModel.Converters;
using MyAgencyVault.EmailFax;
using MyAgencyVault.VM.VMLib.PayorForm;


namespace MyAgencyVault.ViewModel.VMLib
{
    public class VmBillingManager : BaseViewModel
    {
        static MastersClient objLog = new MastersClient();
        public delegate void OnPolicySearchPhraseWindow();
        public event OnPolicySearchPhraseWindow onOpenPhraseSearchedWindow;

        public delegate void delEditAndDisplayWindow();
        public event delEditAndDisplayWindow OnEditAndDisplayWindow;

        private Action<string> popup;
        private Func<string, string, bool> confirm;
        private LicenseeVariableOutputDetail output;
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
        private VariableCollection _SelectedVariableCollection;
        public VariableCollection SelectedVariableCollection
        {
            get
            {
                return _SelectedVariableCollection;
            }
            set
            {
                _SelectedVariableCollection = value;
                OnPropertyChanged("SelectedVariableCollection");
            }
        }

        public VMSharedData _SharedVMData;
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

        private static VmBillingManager vmBillingManager = null;

        private bool expCardPayeeBtnStatus = true;
        private bool expCheckPayeeBtnStatus = true;

        #region based on zipcode find city and state

        private ICommand _ZipLostFocus;
        public ICommand ZipLostFocus
        {
            get
            {
                if (_ZipLostFocus == null)
                    _ZipLostFocus = new BaseCommand(param => OnZipLostFocus());
                return _ZipLostFocus;
            }
        }

        private void OnZipLostFocus()
        {
            if (SelectedLicensee != null && SelectedLicensee.ZipCode != null && SelectedLicensee.ZipCode.ToString().Length == 5)
            {
                Zip zipData = serviceClients.MasterClient.GetZip(SelectedLicensee.ZipCode);
                if (zipData != null)
                {
                    SelectedLicensee.City = zipData.City;
                    SelectedLicensee.State = zipData.State;
                }
            }
        }

        #endregion

        public bool ExpCardPayeeBtnStatus
        {
            get { return expCardPayeeBtnStatus; }
            set
            {
                expCardPayeeBtnStatus = value;
                OnPropertyChanged("ExpCardPayeeBtnStatus");
            }
        }

        public bool ExpCheckPayeeBtnStatus
        {
            get { return expCheckPayeeBtnStatus; }
            set
            {
                expCheckPayeeBtnStatus = value;
                OnPropertyChanged("ExpCheckPayeeBtnStatus");
            }
        }

        private IView _ViewValidation;

        public VmBillingManager(Action<string> Popup, Func<string, string, bool> Confirm, ServiceChargesType chargeTypes, Products pds, IView viewValidation)
        {
            try
            {
                this.popup = Popup;
                this.confirm = Confirm;
                _ViewValidation = viewValidation;

                PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(VmBillingManager_PropertyChanged);

                Status = serviceClients.MasterClient.GetLicenseeStatusList().Select(s => s.Status).ToList();
                PaymentTypes = serviceClients.LicenseeClient.getPaymentTypes().ToList();

                _batchFiles = serviceClients.BatchFilesClient.fillBatchFilesData();
                _displayedBatchFiles = _batchFiles;

                if (RoleManager.Role == UserRole.SuperAdmin)
                    IsImportToolSettings = true;
                else
                    IsImportToolSettings = false;

                try
                {
                    Products = serviceClients.BillingLineDetailClient.GetAllServices();
                }
                catch { }
                try
                {
                    ServiceChargeTypes = serviceClients.BillingLineDetailClient.GetAllServiceCharge();
                }
                catch { }
                try
                {
                    ServiceLines = serviceClients.BillingLineDetailClient.GetBillingLineDetail();
                }
                catch { }

                try
                {
                    LicenseeInvoices = serviceClients.LicenseeInvoiceClient.getAllInvoice();
                }
                catch { }

                try
                {
                    Journals = serviceClients.JournalClient.getAllJournalEntries();
                }
                catch { }

                foreach (ServiceChargeType s in ServiceChargeTypes)
                    chargeTypes.Add(s);

                foreach (ServiceProduct s in Products)
                    pds.Add(s);

                try
                {
                    serviceClients.CalcVariableClient.StartCalculationCompleted += new EventHandler<StartCalculationCompletedEventArgs>(CalcVariableClient_StartCalculationCompleted);
                }
                catch { }

                vmBillingManager = this;

                #region "Import tool settings"
                IsVisibleDuplicateUI = "Hidden";
                //RowsAndCulumnValue();
                //LoadStatementDateSetting()
                LoadStatementDateSetting();
                //LoadPayor
                loadPayor();
                //Load file type
                LoadFileType();
                //Load file formate
                LoadFileFormate();
                //Load Available fields list
                loadAvailableFieldsList();
                //loadAvailableFieldsList1();
                LoadImportToolMasterStatementData();
                //Load Payment data
                LoadPaymentDataFields();
                //Load Mask Type
                LoadMaskType();
                //Load translator type
                LoadTranslator();
                //Load selectedPaymentDataOnPayor
                LoadSeletetedPaymentDataOnPayor();
                #endregion
            }
            catch
            {
            }
        }

        public static VmBillingManager getBillingManager()
        {
            if (vmBillingManager != null)
            {
                //vmBillingManager.RefreshData();
                return vmBillingManager;
            }
            return null;
        }

        private void RefreshData()
        {

        }

        void VmBillingManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {

                switch (e.PropertyName)
                {
                    case "LicenseDetailLst":
                        SelectedLicensee = Licensees.Count == 0 ? null : Licensees.FirstOrDefault();
                        break;
                    case "SelectedLicensee":
                        if (SelectedLicensee != null)
                        {
                            if (SelectedLicensee.IsClientEnable == null)
                            {
                                SelectedLicensee.IsClientEnable = false;
                            }
                            NotesD = SelectedLicensee.Notes == null ? new ObservableCollection<LicenseeNote>() : SelectedLicensee.Notes;
                            FilterServiceLines();
                            FilterVariableLine(SelectedLicensee.LicenseeId);
                            FilterInvoiceLines(SelectedLicensee.LicenseeId);
                            FilterJournalLines(SelectedLicensee.LicenseeId);
                            InvoiceJournalLines = serviceClients.InvoiceLineClient.getInvoiceLinesForJournal(SelectedLicensee.LicenseeId);
                            FilterInvoiceJournalLines(SelectedLicensee.LicenseeId);

                        }
                        else
                        {
                            FilterServiceLines();
                        }
                        break;
                    case "NotesD":
                        SelectedNote = NotesD.Count == 0 ? new LicenseeNote() : NotesD.FirstOrDefault();
                        break;
                    case "ServiceLines":
                        foreach (BillingLineDetail billDetail in ServiceLines)
                            ServiceLine = billDetail;

                        ServiceLine = ServiceLines.Count == 0 ? new BillingLineDetail() : ServiceLines.FirstOrDefault();

                        if (LicenseeInvoices != null)
                            SelectedLicenseeInvoice = LicenseeInvoices.Count == 0 ? new LicenseeInvoice() : LicenseeInvoices.FirstOrDefault();
                        else
                            SelectedLicenseeInvoice = null;

                        break;
                    case "ServiceLine":
                        if (ServiceLine != null && ServiceLine.ServiceChargeType != null)
                            SelectedChargeType = ServiceChargeTypes.Where(p => p.ServiceChargeTypeID == ServiceLine.ServiceChargeType.ServiceChargeTypeID).FirstOrDefault();

                        if (ServiceLine != null && ServiceLine.Service != null)
                        {
                            SelectedProduct = Products.Where(p => p.ServiceID == ServiceLine.Service.ServiceID).FirstOrDefault();
                            ServiceLine.ServiceName = ServiceLine.Service.ServiceName;
                        }
                        break;
                    case "SelectedChargeType":
                        ServiceLine.ServiceChargeType = SelectedChargeType;
                        break;
                    case "SelectedProduct":
                        ServiceLine.Service = SelectedProduct;
                        break;
                    case "LicenseeInvoices":
                        LicenseeInvoicesView.Refresh();
                        break;
                    case "Journals":
                        JournalView.Refresh();
                        break;
                    case "SelectedPayor":

                        if (SelectedPayortempalate == null)
                        {
                            PayorSettingFixedRows = string.Empty;
                            PayorSettingFixedCols = string.Empty;
                            PayorSettingRelativeSearch = string.Empty;
                            PayorSettingRelativeRows = string.Empty;
                            PayorSettingRelativeCols = string.Empty;
                            SelectedBlankFields = null;

                            selectedPaymentDataColValue = string.Empty;
                            selectedPaymentDataRowValue = string.Empty;

                            selectedPaymentDataStartColValue = string.Empty;
                            selectedPaymentDataStartRowValue = string.Empty;
                            selectedPaymentDataEndColValue = string.Empty;
                            selectedPaymentDataEndRowValue = string.Empty;

                            strHeaderSearch = string.Empty;
                            selectedHeaderRowsValue = string.Empty;
                            selectedHeaderColsValue = string.Empty;
                            isPartOfPrimaryKeyYes = false;
                            isCalculatedFieldsYes = false;
                            strformulaExpression = string.Empty;
                            strDefaultText = string.Empty;

                        }
                        else
                        {
                            SelectedFileTypeValue = SelectedPayortempalate.FileType;
                            SelectedFileFormate = SelectedPayortempalate.FormatType;
                            if (SelectedImportToolMasterStatementData != null)
                            {
                                LoadSelectedStamentDataSetting((Guid)SelectedPayor.PayorID, (Guid)SelectedPayortempalate.TemplateID, SelectedImportToolMasterStatementData.ID);
                            }
                        }
                        LoadSeletetedPaymentDataOnPayor();

                        break;

                    case "SelectedDuplicatePayors":

                        LoadDuplicatePayorTempalate(SelectedDuplicatePayors.PayorID);

                        break;

                    case "SelectedImportToolMasterStatementData":
                        LoadSelectedStamentDataSetting((Guid)SelectedPayor.PayorID, (Guid)SelectedPayortempalate.TemplateID, SelectedImportToolMasterStatementData.ID);
                        break;

                    case "SelectedFieldsList":
                        if (SelectedFieldsList != null)
                        {
                            FilterMaskType();
                            FillSelectedPaymentData();
                        }
                        break;

                    case "SelectedPayortempalate":

                        if (SelectedPayortempalate == null)
                        {
                            PayorSettingFixedRows = string.Empty;
                            PayorSettingFixedCols = string.Empty;
                            PayorSettingRelativeSearch = string.Empty;
                            PayorSettingRelativeRows = string.Empty;
                            PayorSettingRelativeCols = string.Empty;
                            SelectedBlankFields = null;

                            selectedPaymentDataColValue = string.Empty;
                            selectedPaymentDataRowValue = string.Empty;

                            selectedPaymentDataStartColValue = string.Empty;
                            selectedPaymentDataStartRowValue = string.Empty;
                            selectedPaymentDataEndColValue = string.Empty;
                            selectedPaymentDataEndRowValue = string.Empty;

                            strHeaderSearch = string.Empty;
                            selectedHeaderRowsValue = string.Empty;
                            selectedHeaderColsValue = string.Empty;
                            isPartOfPrimaryKeyYes = true;
                            isCalculatedFieldsYes = true;
                            strformulaExpression = string.Empty;

                        }
                        else
                        {
                            SelectedFileTypeValue = SelectedPayortempalate.FileType;
                            SelectedFileFormate = SelectedPayortempalate.FormatType;
                            if (SelectedImportToolMasterStatementData != null)
                            {
                                LoadSelectedStamentDataSetting((Guid)SelectedPayor.PayorID, (Guid)SelectedPayortempalate.TemplateID, SelectedImportToolMasterStatementData.ID);
                            }


                        }
                        LoadSeletetedPaymentDataOnPayor();

                        break;

                    case "SelectedPayorForEdit":

                        if (SelectedPayorForEdit.PayorID != null && SelectedPayorForEdit.PayorID != new Guid())
                        {
                            tempobjImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase.Where(p => p.PayorID == SelectedPayorForEdit.PayorID).ToList());
                        }
                        else
                        {
                            tempobjImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase).ToList();
                        }
                        //if (tempobjImportToolPhrase.Count > 0)
                        //{
                        //    SelectedobjImportToolPhrase = tempobjImportToolPhrase.FirstOrDefault();
                        //}

                        break;

                    case "SelectedPayortempalateForEdit":

                        if (SelectedPayortempalateForEdit.TemplateID != null)
                        {
                            tempobjImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase.Where(p => p.TemplateID == SelectedPayortempalateForEdit.TemplateID).ToList());
                        }
                        else
                        {
                            tempobjImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase).ToList();
                        }
                        //if (tempobjImportToolPhrase.Count > 0)
                        //{
                        //    SelectedobjImportToolPhrase = tempobjImportToolPhrase.FirstOrDefault();
                        //}

                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("Exception occurs while processing" + ex.Message);
              //  ActionLogger.Logger.WriteLog("Exception occurs while processing" + ex.Message, true);
               // throw ex;
            }
        }

        private string lblaccCode = "Account Code";
        public string lblAccCode
        {
            get
            {
                return lblaccCode;
            }
            set
            {
                lblaccCode = value;
                OnPropertyChanged("lblAccCode");
            }
        }

        private string lblcutOff = "Cut Off Day 2";
        public string lblCutOff
        {
            get
            {
                return lblcutOff;
            }
            set
            {
                lblcutOff = value;
                OnPropertyChanged("lblCutOff");
            }
        }

        private void FilterVariableLine(Guid guid)
        {
            if (output != null && output.LicenseesValueDictionary != null)
            {
                if (output.LicenseesValueDictionary.ContainsKey(guid))
                {
                    SelectedVariableCollection = output.LicenseesValueDictionary[guid];
                    if (SelectedVariableCollection.ServiceCharges != null)
                    {
                        foreach (ServiceCharge charge in SelectedVariableCollection.ServiceCharges)
                        {
                            BillingLineDetail serLine = ServiceLines.FirstOrDefault(s => s.Service.ServiceID == charge.ServiceId && s.LicenseeID == guid);
                            if (serLine != null)
                                serLine.EstimatedCharge = charge.Charge;
                        }
                    }
                }
                else
                    SelectedVariableCollection = null;
            }
            else
                SelectedVariableCollection = null;
        }

        #region Licensees Tab

        #region Licensee Detail

        #region Public Properties

        private ObservableCollection<LicenseeDisplayData> _AllLicensees;
        public ObservableCollection<LicenseeDisplayData> AllLicensees
        {
            get
            {
                if (_AllLicensees == null)
                {
                    _AllLicensees = serviceClients.LicenseeClient.GetLicenseeList(LicenseeStatusEnum.All, RoleManager.LicenseeId.Value);
                }
                return _AllLicensees;
            }
            set
            {
                _Licensees = value;
                OnPropertyChanged("AllLicensees");
            }
        }

        private ObservableCollection<LicenseeDisplayData> _Licensees;
        public ObservableCollection<LicenseeDisplayData> Licensees
        {
            get
            {
                if (_Licensees == null)
                {
                    _Licensees = new ObservableCollection<LicenseeDisplayData>(from s in AllLicensees where (s.LicenseeStatus == LicenseeStatusEnum.Active) select s);
                    SelectedLicensee = _Licensees.Count == 0 ? null : _Licensees.FirstOrDefault();
                }
                return _Licensees;
            }
            set
            {
                _Licensees = value;
                OnPropertyChanged("Licensees");
            }
        }

        public LicenseeDisplayData SelectedLicensee
        {
            get
            {
                return SharedVMData.SelectedLicensee;
            }
            set
            {
                SharedVMData.SelectedLicensee = value;
                OnPropertyChanged("SelectedLicensee");

            }
        }

        private int LicenseeRadioSelection = 0;

        #endregion

        #region Commands
        private ICommand _LicenseSaveCom;
        public ICommand LicenseSaveCommand
        {
            get
            {
                if (_LicenseSaveCom == null)
                {
                    _LicenseSaveCom = new BaseCommand(Parm => BeforeOnSaveLicensee(), Parm => OnSaveLicensee());
                }
                return _LicenseSaveCom;

            }
            set
            {
                _LicenseSaveCom = value;
                OnPropertyChanged("LicenseDetailLst");
            }
        }
        private ICommand _cancelCommand;
        public ICommand LicenseDetailCancel
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new BaseCommand(parm => BeforeOnCancelLicensee(), parm => OnCancelLicensee());
                }
                return _cancelCommand;
            }

        }

        private bool BeforeOnCancelLicensee()
        {
            return true;
        }

        private void OnCancelLicensee()
        {
            if (SelectedLicensee != null)
            {
                if (SelectedLicensee.LicenseeId != Guid.Empty)
                {
                    LicenseeDisplayData licensee = serviceClients.LicenseeClient.GetLicenseeByID(SelectedLicensee.LicenseeId);
                    SelectedLicensee.Copy(licensee);
                }
                else
                {
                    SelectedLicensee = Licensees.FirstOrDefault();
                }
            }
        }

        private ICommand _newCommand;
        public ICommand LicenseDetailNewCmd
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new BaseCommand(parm => BeforeOnNewLicensee(), parm => OnNewLicensee());
                }
                return _newCommand;
            }

        }

        ICommand _licensesDelete;
        public ICommand LicensesDelete
        {
            get
            {
                if (_licensesDelete == null)
                {
                    _licensesDelete = new BaseCommand(x => BeforeOnDeleteLicensee(), x => OnDeleteLicense());
                }
                return _licensesDelete;
            }
        }

        private ICommand _SendAletMail;
        public ICommand SendAletMail
        {
            get
            {
                if (_SendAletMail == null)
                {
                    _SendAletMail = new BaseCommand(x => BeforeOnSendMail(), x => OnSendMail());
                }
                return _SendAletMail;
            }
        }

        private bool BeforeOnSendMail()
        {
            if (SelectedLicensee != null)
                return true;
            else
                return false;

        }

        private void OnSendMail()
        {
            MailData _MailData = new MailData();
            _MailData.AgencyName = Convert.ToString(SelectedLicensee.Company);

            if (SelectedLicensee.CutOffDay1 != null)
            {
                DateTime dtCutOfDay = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, Convert.ToInt32(SelectedLicensee.CutOffDay1));

                if (string.IsNullOrEmpty(Convert.ToString(SelectedLicensee.Email)))
                {
                    MessageBox.Show("Mail Id is not available", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (SelectedLicensee != null)
                    {
                        if (!string.IsNullOrEmpty(SelectedLicensee.Email))
                        {
                            _MailData.FromMail = "service@commissionsdept.com";
                            _MailData.ToMail = Convert.ToString(SelectedLicensee.Email);
                            //Subject = "CommissionsDept Statement PDF Upload reminder - Monthly statements due by " + Convert.ToString(dtMailDate.DayOfWeek) + ", " + dtMailDate.ToShortDateString();
                            string strSubject = "CommissionsDept Statement PDF Upload reminder - Monthly statements due by " + Convert.ToString(dtCutOfDay.DayOfWeek) + ", " + dtCutOfDay.ToShortDateString();
                            serviceClients.FollowupIssueClient.SendNotificationMailAsync(_MailData, strSubject, MailBody(_MailData));
                        }
                    }

                }
            }
        }

        private string MailBody(MailData EmailContentdata)
        {

            string MailBody = string.Empty;

            try
            {

                MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                               "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>" +
                               "</td></tr><tr><td colspan='2'>" +
                               "&nbsp;</td></tr><tr><td colspan='2'>This is a reminder to upload, email or fax your statements for entry." +
                               "If you would like to email your statements, please send to upload@commissionsdept.com or fax to 1-800-343-4072. <br /><br />" +
                               "For best results: <br/> <br/> 1) Remember to scan both front and back of double-sided statements <br />" +
                               "2) Must be in PDF format <br /> 3) All pages should be upright <br /> 4) Scan black and white, 200 dpi and compact if possible <br />" +
                               "5) Remove staples <br /> 6) Please make sure your sent from email address is registered in our system. <br /> <br /> <br />" +
                               "Thank you,<br />CommissionsDept <br />P 516.708.4477 <br />F 571.323.6970 <br />Fax Statements to 1-800-343-4072 <br />" +
                               "Email Statements to upload@commissionsdept.com" +
                               "</td></tr></table>";
            }
            catch
            {
            }

            return MailBody;

        }

        private ICommand _activeCheck;
        public ICommand ActiveCheck
        {
            get
            {
                if (_activeCheck == null)
                    _activeCheck = new BaseCommand(x => GetLicenseData(0));
                return _activeCheck;
            }

        }
        private ICommand _InActiveCheck;
        public ICommand InActiveCheck
        {
            get
            {
                if (_InActiveCheck == null)
                    _InActiveCheck = new BaseCommand(x => GetLicenseData(1));
                return _InActiveCheck;
            }

        }
        private ICommand _Pending;
        public ICommand Pending
        {
            get
            {
                if (_Pending == null)
                    _Pending = new BaseCommand(x => GetLicenseData(2));
                return _Pending;
            }

        }

        private ICommand _Balance;
        public ICommand Balance
        {
            get
            {
                if (_Balance == null)
                    _Balance = new BaseCommand(x => GetLicenseData(3));
                return _Balance;
            }

        }

        private ICommand _All;
        public ICommand All
        {
            get
            {
                if (_All == null)
                    _All = new BaseCommand(x => GetLicenseData(4));
                return _All;
            }

        }

        private void GetLicenseData(int Status)
        {
            LicenseeRadioSelection = Status;
            switch (Status)
            {
                case 0:
                    Licensees = new ObservableCollection<LicenseeDisplayData>(AllLicensees.Where(u => u.LicenseeStatus == LicenseeStatusEnum.Active).OrderBy(s => s.Company).ThenBy(s => s.ContactLast).ThenBy(s => s.ContactFirst));
                    break;
                case 1:
                    Licensees = new ObservableCollection<LicenseeDisplayData>(AllLicensees.Where(u => u.LicenseeStatus == LicenseeStatusEnum.InActive).OrderBy(s => s.Company).ThenBy(s => s.ContactLast).ThenBy(s => s.ContactFirst));
                    break;
                case 2:
                    Licensees = new ObservableCollection<LicenseeDisplayData>(AllLicensees.Where(u => u.LicenseeStatus == LicenseeStatusEnum.Pending).OrderBy(s => s.Company).ThenBy(s => s.ContactLast).ThenBy(s => s.ContactFirst));
                    break;
                case 3:
                    Licensees = new ObservableCollection<LicenseeDisplayData>(AllLicensees.Where(u => u.DueBalance > 0).OrderBy(s => s.Company).ThenBy(s => s.ContactLast).ThenBy(s => s.ContactFirst));
                    break;
                default:
                    Licensees = AllLicensees;
                    break;
            }

            if (AllLicensees.Count > 0)
            {
                SelectedLicensee = AllLicensees.FirstOrDefault();
            }
        }

        #endregion

        #region Methods


        private bool BeforeOnDeleteLicensee()
        {
            if (Licensees.Count != 0)
                return true;
            else
                return false;
        }

        private void OnDeleteLicense()
        {
            if (objLog == null) objLog = new MastersClient();
            if (SelectedLicensee == null)
            {
                MessageBox.Show("Select an agency to delete");
                return;
            }
            Guid licID = SelectedLicensee.LicenseeId;
            if (licID != Guid.Empty)
            {
                if (MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    serviceClients.LicenseeClient.DeleteLicensee(SelectedLicensee);
            }
            objLog.AddLog("DeleteLicensee request: " + SelectedLicensee.LicenseeId + ", Name: " + SelectedLicensee.Company + ", User: " + RoleManager.userCredentialID);

            Licensees.Remove(SelectedLicensee);
            AllLicensees.Remove(AllLicensees.Where(s => s.LicenseeId == licID).FirstOrDefault());

            SelectedLicensee = ((Licensees == null) || (Licensees.Count == 0)) ? null : Licensees.FirstOrDefault();

            SharedVMData.RefreshRequired = true;
            if (VMInstances.PeopleManager != null)
                VMInstances.PeopleManager.RefreshRequired = true;
            if (VMInstances.SettingManager != null)
                VMInstances.SettingManager.RefreshRequired = true;
            if (VMInstances.CompManager != null)
                VMInstances.CompManager.RefreshRequired = true;
            if (VMInstances.PolicyManager != null)
                VMInstances.PolicyManager.RefreshRequired = true;
        }
        private bool BeforeOnNewLicensee()
        {
            if (SelectedLicensee == null)
                return true;
            else
            {
                if (SelectedLicensee.LicenseeId != Guid.Empty)
                    return true;
                else
                    return false;
            }
        }

        private void OnNewLicensee()
        {
            SelectedLicensee = new LicenseeDisplayData { LicenseeStatus = LicenseeStatusEnum.Pending, LicensePaymentModeId = 1, Commissionable = true };
        }

        void OnSaveLicensee()
        {
            if (!_ViewValidation.Validate("Billing Manager"))
            {
                MessageBox.Show("Validation failed.");
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            bool isAddCase = false;
            try
            {
                if (SelectedLicensee.LicenseeId == Guid.Empty)
                {
                    //When new licensee added...
                    isAddCase = true;
                    //int companyCount = AllLicensees.Where(s => s.Company == SelectedLicensee.Company).Count();
                    int companyCount = AllLicensees.Where(s => s != null && s.Company.Trim().ToUpper() == SelectedLicensee.Company.Trim().ToUpper()).Count();
                    if (companyCount > 0)
                    {
                        confirm("Company already exist", "Error");
                        return;
                    }

                    if (serviceClients.UserClient.IsUserNameExist(Guid.Empty, SelectedLicensee.UserName))
                    {
                        confirm("User already exist", "Error");
                        return;
                    }
                    SelectedLicensee.DueBalance = 0;
                    SelectedLicensee.IsDeleted = false;
                    if (SelectedLicensee.UserName != null)
                    {
                        SelectedLicensee.UserName = SelectedLicensee.UserName.Trim();
                        if (ValidateUserName(SelectedLicensee.UserName))
                        {
                            confirm("Validation failed.User name contain space", "Error");
                            return;
                        }
                    }

                    if (SelectedLicensee.CutOffDay1 != null)
                    {
                        try
                        {
                            string strCutOfDate = Convert.ToString(SelectedLicensee.CutOffDay1);
                            if (!string.IsNullOrEmpty(strCutOfDate))
                            {
                                int intValue = Convert.ToInt32(strCutOfDate);
                                if (intValue < 1 || intValue > 28)
                                {
                                    confirm("Validation failed.Cut off day should be in between 1 to 28", "Error");
                                    SelectedLicensee.CutOffDay1 = null;
                                    return;
                                }

                            }
                        }
                        catch
                        {
                            confirm("Please enter numeric number", "Error");
                            SelectedLicensee.CutOffDay1 = null;
                            return;
                        }
                    }
                }
                else
                {
                    //When existing licensee saved...
                    if (SelectedLicensee.CutOffDay1 != null)
                    {
                        string strCutOfDate = Convert.ToString(SelectedLicensee.CutOffDay1);
                        if (!string.IsNullOrEmpty(strCutOfDate))
                        {
                            int intValue = Convert.ToInt32(strCutOfDate);
                            if (intValue < 1 || intValue > 28)
                            {
                                confirm("Validation failed.Cut off day should between 1 to 28", "Error");
                                SelectedLicensee.CutOffDay1 = null;
                                return;
                            }

                        }

                    }

                    int companyCount = AllLicensees.Where(s => s.Company == SelectedLicensee.Company).Count();
                    if (companyCount > 1)
                    {
                        confirm("Company already exist", "Error");
                        return;
                    }

                }

                SelectedLicensee.LicenseeId = SelectedLicensee.LicenseeId == Guid.Empty ? Guid.NewGuid() : SelectedLicensee.LicenseeId;


                if (SelectedLicensee.Company != null)
                    SelectedLicensee.Company = SelectedLicensee.Company.Trim();

                if (SelectedLicensee.UserName != null)
                {
                    SelectedLicensee.UserName = SelectedLicensee.UserName.Trim();
                    SelectedLicensee.UserName = ConvertWhitespaceToSpaces(SelectedLicensee.UserName);
                }
                SelectedLicensee.IsClientEnable = SelectedLicensee.IsClientEnable;


                serviceClients.LicenseeClient.AddUpdateLicensee(SelectedLicensee);

                if (isAddCase)
                {
                    AllLicensees.Add(SelectedLicensee);
                    if (LicenseeRadioSelection == (int)SelectedLicensee.LicenseeStatus)
                        Licensees.Add(SelectedLicensee);
                }

                if (SelectedLicensee != null)
                {
                    var list = (from ser in ServiceLines
                                where ser.LicenseeID == SelectedLicensee.LicenseeId
                                select ser).ToList();
                    var obsList = new ObservableCollection<BillingLineDetail>(list);
                    serviceClients.BillingLineDetailClient.Add(obsList, SelectedLicensee.LicenseeId);

                    //Refresh the product after saving
                    Products = serviceClients.BillingLineDetailClient.GetAllServices();
                }
                SharedVMData.RefreshRequired = true;

                if (VMInstances.PeopleManager != null)
                    VMInstances.PeopleManager.RefreshRequired = true;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public string ConvertWhitespaceToSpaces(string value)
        {
            string strNewUserName = string.Empty;
            char[] arr = value.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != ' ')
                {
                    strNewUserName = strNewUserName + arr[i];
                }
            }
            return strNewUserName;
        }

        public bool ValidateUserName(string value)
        {
            bool bValue = false;
            char[] arr = value.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == ' ')
                {
                    bValue = true;
                }
            }
            return bValue;
        }

        bool BeforeOnSaveLicensee()
        {
            if (SelectedLicensee != null)
                return true;
            else
                return false;
        }

        #endregion

        #endregion

        #region Notes
        #region Public Properties
        // SelectedNotes

        private ObservableCollection<LicenseeNote> _notesD;
        public ObservableCollection<LicenseeNote> NotesD
        {
            get
            {
                return _notesD;
            }
            set
            {
                _notesD = value;
                OnPropertyChanged("NotesD");
            }
        }
        private LicenseeNote _selectedNote;
        public LicenseeNote SelectedNote
        {
            get
            {
                return _selectedNote == null ? new LicenseeNote() : _selectedNote;
            }
            set
            {
                _selectedNote = value;
                OnPropertyChanged("SelectedNote");
            }
        }

        #endregion

        #region Commands
        private ICommand _saveNotes;
        public ICommand SaveNotes
        {
            get
            {
                if (_saveNotes == null)
                {
                    _saveNotes = new BaseCommand(parm => SaveNotesData());
                }
                return _saveNotes;
            }

        }
        private ICommand _newNotes;
        public ICommand CmdNewNotes
        {
            get
            {
                if (_newNotes == null)
                {
                    _newNotes = new BaseCommand(Parm => GetNewNotes());
                }
                return _newNotes;
            }

        }
        private ICommand _CmdDeleteNotes;
        public ICommand CmdDeleteNotes
        {
            get
            {
                if (_CmdDeleteNotes == null)
                {
                    _CmdDeleteNotes = new BaseCommand(Parm => DeleteNote());
                }
                return _CmdDeleteNotes;
            }

        }

        #endregion

        #region Methods

        private void DeleteNote()
        {
            if (SelectedNote != null && SelectedNote.LicenseeId != Guid.Empty && SelectedNote.NoteID != Guid.Empty)
            {
                serviceClients.LicenseeNoteClient.DeleteLicenseeNote(SelectedNote);
                NotesD.Remove(SelectedNote);
                SelectedNote = NotesD == null ? new LicenseeNote() : NotesD.FirstOrDefault();
            }
        }
        private void GetNewNotes()
        {
            SelectedNote = new LicenseeNote { LicenseeId = SelectedLicensee.LicenseeId, NoteID = Guid.NewGuid() };
        }


        private void SaveNotesData()
        {
            SelectedNote.LicenseeId = SelectedLicensee.LicenseeId;
            SelectedNote.NoteID = SelectedNote.NoteID == Guid.Empty ? Guid.NewGuid() : SelectedNote.NoteID;
            if (SelectedNote.Content != null && SelectedNote.Content.Trim() != string.Empty)
            {
                LicenseeNote licNote = null;

                licNote = serviceClients.LicenseeNoteClient.AddUpdateLicenseeNote(SelectedNote);

                if (NotesD.Where(u => u.NoteID == SelectedNote.NoteID).Count() == 0)
                {
                    SelectedNote.Content = licNote.Content;

                    RTFToTextConverter converter = new RTFToTextConverter();
                    SelectedNote.SimpleTextContent = converter.Convert(SelectedNote.Content);

                    SelectedNote.CreatedDate = licNote.CreatedDate;
                    SelectedNote.LastModifiedDate = licNote.LastModifiedDate;

                    NotesD.Add(SelectedNote);
                }
                else
                {
                    SelectedNote.Content = licNote.Content;

                    RTFToTextConverter converter = new RTFToTextConverter();
                    SelectedNote.SimpleTextContent = converter.Convert(SelectedNote.Content);

                    SelectedNote.CreatedDate = licNote.CreatedDate;
                    SelectedNote.LastModifiedDate = licNote.LastModifiedDate;
                }
            }

        }

        #endregion

        #endregion

        #endregion

        #region Common Items
        private List<string> _status;
        public List<string> Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        #endregion

        private string RadioSelection = "All";

        private ObservableCollection<BatchFiles> _batchFiles;
        public ObservableCollection<BatchFiles> BatchFiles
        {
            get
            {
                if (_batchFiles != null)
                    return _batchFiles;
                return null;
            }
            set
            {
                _batchFiles = value;
                OnBatchFilesChanged(RadioSelection);
                OnPropertyChanged("BatchFiles");
            }

        }

        #region "Import tool settings"

        private bool isNewClick = false;

        private ObservableCollection<string> _RowsAndCols;
        public ObservableCollection<string> RowsAndCols
        {
            get
            {
                return _RowsAndCols;
            }
            set
            {
                _RowsAndCols = value;
                OnPropertyChanged("RowsAndCols");
            }
        }

        private ObservableCollection<ImportToolBrokerSetting> _AllImportToolBrokerSetting;
        public ObservableCollection<ImportToolBrokerSetting> AllImportToolBroker
        {
            get
            {
                return _AllImportToolBrokerSetting;
            }
            set
            {
                _AllImportToolBrokerSetting = value;
                OnPropertyChanged("AllImportToolBroker");
            }
        }

        private ImportToolBrokerSetting _selectedImportToolSetting;
        public ImportToolBrokerSetting selectedImportToolSetting
        {
            get
            {
                return _selectedImportToolSetting;
            }
            set
            {
                _selectedImportToolSetting = value;
                OnPropertyChanged("selectedImportToolSetting");
            }
        }

        private string _SelectedFixRows;
        public string SelectedFixRows
        {
            get
            {
                return _SelectedFixRows;
            }
            set
            {
                _SelectedFixRows = value;
                OnPropertyChanged("SelectedFixRows");
            }
        }

        private string _SelectedRelativeRows;
        public string SelectedRelativeRows
        {
            get
            {
                return _SelectedRelativeRows;
            }
            set
            {
                _SelectedRelativeRows = value;
                OnPropertyChanged("SelectedRelativeRows");
            }
        }

        private string _SelectedBrokerFixColomnRows;
        public string SelectedBrokerFixColomnRows
        {
            get
            {
                return _SelectedBrokerFixColomnRows;
            }
            set
            {
                _SelectedBrokerFixColomnRows = value;
                OnPropertyChanged("SelectedBrokerFixColomnRows");
            }
        }

        private string _SelectedBrkoerRelativeColomnRows;
        public string SelectedBrkoerRelativeColomnRows
        {
            get
            {
                return _SelectedBrkoerRelativeColomnRows;
            }
            set
            {
                _SelectedBrkoerRelativeColomnRows = value;
                OnPropertyChanged("SelectedBrkoerRelativeColomnRows");
            }
        }

        private bool _IsImportToolSettings;
        public bool IsImportToolSettings
        {
            get
            {
                return _IsImportToolSettings;
            }
            set
            {
                _IsImportToolSettings = value;
                OnPropertyChanged("IsImportToolSettings");
            }
        }

        private string _strRelativeSearch;
        public string strRelativeSearch
        {
            get
            {
                return _strRelativeSearch;
            }
            set
            {
                _strRelativeSearch = value;
                OnPropertyChanged("strRelativeSearch");
            }
        }

        private ObservableCollection<DisplayedPayor> _Payors;
        public ObservableCollection<DisplayedPayor> Payors
        {
            get
            {
                if (_Payors != null)
                    return _Payors;
                return null;
            }
            set
            {
                _Payors = value;
                OnPropertyChanged("Payors");
            }
        }


        //private DisplayedPayor _selectedDuplicatePayor = null;
        //public DisplayedPayor SelectedDuplicatePayor
        //{
        //    get
        //    {
        //        return _selectedDuplicatePayor;
        //    }
        //    set
        //    {
        //        _selectedDuplicatePayor = value;

        //        if (_selectedDuplicatePayor != null)
        //        {
        //            OnPropertyChanged("SelectedDuplicatePayor");
        //        }
        //    }
        //}

        //private DisplayedPayor _PreviousSelectedPayor;
        //public DisplayedPayor PreviousSelectedPayor
        //{
        //    get
        //    {
        //        return _PreviousSelectedPayor;
        //    }
        //    set
        //    {
        //        _PreviousSelectedPayor = value;
        //    }
        //}

        private DisplayedPayor _selectedPayor;
        public DisplayedPayor SelectedPayor
        {
            get
            {
                return _selectedPayor;
            }
            set
            {
                _selectedPayor = value;

                if (_selectedPayor != null)
                {
                    if (SelectedPayor != null)
                    {
                        LoadPayorTempalate(SelectedPayor.PayorID);
                    }

                    OnPropertyChanged("SelectedPayor");
                }
            }
        }

        private ObservableCollection<DisplayedPayor> _DuplicatePayors;
        public ObservableCollection<DisplayedPayor> DuplicatePayors
        {
            get
            {
                return _DuplicatePayors;
            }
            set
            {
                _DuplicatePayors = value;
                if (_DuplicatePayors != null)
                {
                    if (SelectedDuplicatePayors != null)
                    {
                        LoadDuplicatePayorTempalate(SelectedDuplicatePayors.PayorID);
                    }
                }
                OnPropertyChanged("DuplicatePayors");
            }
        }

        private DisplayedPayor _SelectedDuplicatePayors;
        public DisplayedPayor SelectedDuplicatePayors
        {
            get
            {
                return _SelectedDuplicatePayors;
            }
            set
            {
                _SelectedDuplicatePayors = value;
                OnPropertyChanged("SelectedDuplicatePayors");
            }
        }

        private List<Tempalate> _PayorTemplate;
        public List<Tempalate> PayorTemplate
        {
            get
            {
                return _PayorTemplate;
            }
            set
            {
                _PayorTemplate = value;
                OnPropertyChanged("PayorTemplate");
            }
        }

        private Tempalate _SelectedPayortempalate = null;
        public Tempalate SelectedPayortempalate
        {
            get
            {
                return _SelectedPayortempalate;
            }
            set
            {
                _SelectedPayortempalate = value;
                OnPropertyChanged("SelectedPayortempalate");
            }
        }

        private List<Tempalate> _DuplicatePayorTemplate;
        public List<Tempalate> DuplicatePayorTemplate
        {
            get
            {
                return _DuplicatePayorTemplate;
            }
            set
            {
                _DuplicatePayorTemplate = value;
                OnPropertyChanged("DuplicatePayorTemplate");
            }
        }

        private Tempalate _SelectedDuplicatePayortempalate = null;
        public Tempalate SelectedDuplicatePayortempalate
        {
            get
            {
                return _SelectedDuplicatePayortempalate;
            }
            set
            {
                _SelectedDuplicatePayortempalate = value;
                OnPropertyChanged("SelectedDuplicatePayortempalate");

            }
        }

        private ICommand _AddNewBrokerSetting;
        public ICommand AddNewBrokerSetting
        {
            get
            {
                if (_AddNewBrokerSetting == null)
                {
                    _AddNewBrokerSetting = new BaseCommand(parm => BeforeAddNewBrokerSetting(), parm => OnAddBrokerSetting());
                }
                return _AddNewBrokerSetting;
            }
        }

        private bool BeforeAddNewBrokerSetting()
        {
            return true;
        }

        private void OnAddBrokerSetting()
        {
            //AllImportToolBroker =new ObservableCollection<ImportToolBrokerSetting>(serviceClients.BrokerCodeClient.LoadImportToolBrokerSetting().ToList());

            strRelativeSearch = string.Empty;
            isNewClick = true;
        }

        private ICommand _SaveNewBrokerSetting;
        public ICommand SaveNewBrokerSetting
        {
            get
            {
                if (_SaveNewBrokerSetting == null)
                {
                    _SaveNewBrokerSetting = new BaseCommand(parm => BeforeSaveBrokerSetting(), parm => OnSaveBrokerSetting());
                }
                return _SaveNewBrokerSetting;
            }
        }

        private bool BeforeSaveBrokerSetting()
        {
            if (isNewClick)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnSaveBrokerSetting()
        {
            ImportToolBrokerSetting objImportToolBrokerSetting = new ImportToolBrokerSetting();
            objImportToolBrokerSetting.FixedRows = SelectedFixRows;
            objImportToolBrokerSetting.FixedColumns = SelectedBrokerFixColomnRows;
            objImportToolBrokerSetting.RelativeSearchtext = strRelativeSearch;
            objImportToolBrokerSetting.RelativeColumns = SelectedBrkoerRelativeColomnRows;
            objImportToolBrokerSetting.RelativeRows = SelectedRelativeRows;

            serviceClients.BrokerCodeClient.AddImportToolBrokerSettings(objImportToolBrokerSetting);

            isNewClick = false;
        }

        private void loadPayor()
        {
            try
            {
                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
                DuplicatePayors = Payors = new ObservableCollection<DisplayedPayor>(serviceClients.DisplayedPayorClient.GetDisplayPayors(Guid.Empty, fillInfo).OrderBy(p => p.PayorName));

                SelectedDuplicatePayors = SelectedPayor = Payors.FirstOrDefault();
                LoadPayorTempalate(SelectedPayor.PayorID);

                LoadDuplicatePayorTempalate(SelectedDuplicatePayors.PayorID);

                isEnabled = false;
            }
            catch { }
        }

        private void LoadPayorTempalate(Guid PayorID)
        {
            if (PayorID != null)
            {
                PayorTemplate = new List<Tempalate>(serviceClients.PayorTemplateClient.GetImportToolPayorTemplate(PayorID)).ToList();
            }

            if (PayorTemplate.Count > 0)
            {
                SelectedPayortempalate = PayorTemplate.FirstOrDefault();
                SelectedFileTypeValue = SelectedPayortempalate.FileType;
                SelectedFileFormate = SelectedPayortempalate.FormatType;
            }
            else
            {
                SelectedPayortempalate = null;
            }
        }

        private void LoadDuplicatePayor()
        {
            PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
            DuplicatePayors = new ObservableCollection<DisplayedPayor>(serviceClients.DisplayedPayorClient.GetDisplayPayors(Guid.Empty, fillInfo).OrderBy(p => p.PayorName));

            SelectedDuplicatePayors = DuplicatePayors.FirstOrDefault();
            LoadDuplicatePayorTempalate(SelectedDuplicatePayors.PayorID);
            isEnabled = false;
        }

        private void LoadDuplicatePayorTempalate(Guid PayorID)
        {
            if (PayorID != null)
            {
                DuplicatePayorTemplate = new List<Tempalate>(serviceClients.PayorTemplateClient.GetImportToolPayorTemplate(PayorID)).ToList();
            }

            if (PayorTemplate.Count > 0)
            {
                SelectedDuplicatePayortempalate = DuplicatePayorTemplate.FirstOrDefault();
            }
            else
            {
                SelectedDuplicatePayortempalate = null;
            }
        }

        private ObservableCollection<string> _FileTypeValue;
        public ObservableCollection<string> FileTypeValue
        {
            get
            {
                if (_FileTypeValue != null)
                    return _FileTypeValue;
                return null;
            }
            set
            {
                _FileTypeValue = value;
                OnPropertyChanged("FileTypeValue");
            }
        }

        private string _SelectedFileTypeValue;
        public string SelectedFileTypeValue
        {
            get
            {
                if (_SelectedFileTypeValue != null)
                    return _SelectedFileTypeValue;
                return null;
            }
            set
            {
                _SelectedFileTypeValue = value;
                OnPropertyChanged("SelectedFileTypeValue");
            }
        }

        private void LoadFileType()
        {
            try
            {
                FileTypeValue = new ObservableCollection<string>();
                FileTypeValue.Add("xlsx");
                FileTypeValue.Add("xls");
                FileTypeValue.Add("csv");
                FileTypeValue.Add("txt");
                if (String.IsNullOrEmpty(SelectedFileTypeValue))
                {
                    SelectedFileTypeValue = FileTypeValue.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                objLog.AddLog("LoadFileType:Exception occurs while processing" + ex.Message);
                //ActionLogger.Logger.WriteLog("LoadFileType:Exception occurs while processing" + ex.Message, true);
            }
        }

        private ObservableCollection<string> _FileFormate;
        public ObservableCollection<string> FileFormate
        {
            get
            {
                if (_FileFormate != null)
                    return _FileFormate;
                return null;
            }
            set
            {
                _FileFormate = value;
                OnPropertyChanged("FileFormate");
            }
        }

        private string _SelectedFileFormate;
        public string SelectedFileFormate
        {
            get
            {
                if (_SelectedFileFormate != null)
                    return _SelectedFileFormate;
                return null;
            }
            set
            {
                _SelectedFileFormate = value;
                OnPropertyChanged("SelectedFileFormate");
            }
        }

        private void LoadFileFormate()
        {
            try
            {
                FileFormate = new ObservableCollection<string>();
                FileFormate.Add("Tab");
                FileFormate.Add("Comma");
                if(String.IsNullOrEmpty(SelectedFileFormate))
                {
                    SelectedFileFormate = FileFormate.FirstOrDefault();
                }
                
            }
            catch
            {
            }
        }

        private void RowsAndCulumnValue()
        {
            try
            {
                RowsAndCols = new ObservableCollection<string>();

                for (int i = 0; i <= 500; i++)
                {
                    RowsAndCols.Add(i.ToString());
                }

                RowsAndCols.Insert(0, string.Empty);

                SelectedFixRows = RowsAndCols.FirstOrDefault();
                SelectedRelativeRows = RowsAndCols.FirstOrDefault();
                SelectedBrokerFixColomnRows = RowsAndCols.FirstOrDefault();
                SelectedBrkoerRelativeColomnRows = RowsAndCols.FirstOrDefault();
                //Payment selected data
                selectedPaymentDataColValue = RowsAndCols.FirstOrDefault();
                selectedPaymentDataRowValue = RowsAndCols.FirstOrDefault();

                selectedPaymentDataStartColValue = RowsAndCols.FirstOrDefault();
                selectedPaymentDataStartRowValue = RowsAndCols.FirstOrDefault();
                selectedPaymentDataEndColValue = RowsAndCols.FirstOrDefault();
                selectedPaymentDataEndRowValue = RowsAndCols.FirstOrDefault();

                selectedHeaderRowsValue = RowsAndCols.FirstOrDefault();
                selectedHeaderColsValue = RowsAndCols.FirstOrDefault();
            }
            catch
            {
            }
        }

        private ObservableCollection<PayorToolAvailablelFieldType> _AllAvailableFieldsList;
        public ObservableCollection<PayorToolAvailablelFieldType> AllAvailableFieldsList
        {
            get
            {
                if (_AllAvailableFieldsList != null)
                    return _AllAvailableFieldsList;

                return _AllAvailableFieldsList;
            }
            set
            {
                _AllAvailableFieldsList = value;
                OnPropertyChanged("AllAvailableFieldsList");
            }
        }

        private ObservableCollection<PayorToolAvailablelFieldType> _TempAllAvailableFieldsList;
        public ObservableCollection<PayorToolAvailablelFieldType> TempAllAvailableFieldsList
        {
            get
            {
                if (_TempAllAvailableFieldsList != null)
                    return _TempAllAvailableFieldsList;

                return _TempAllAvailableFieldsList;
            }
            set
            {
                _TempAllAvailableFieldsList = value;
                OnPropertyChanged("TempAllAvailableFieldsList");
            }
        }

        private PayorToolAvailablelFieldType _SelectedBlankFields;
        public PayorToolAvailablelFieldType SelectedBlankFields
        {
            get
            {
                return _SelectedBlankFields;
            }
            set
            {
                _SelectedBlankFields = value;
                OnPropertyChanged("SelectedBlankFields");
            }
        }

        private PayorToolAvailablelFieldType _SelectedAvailablrFieldsList;
        public PayorToolAvailablelFieldType SelectedAvailablrFieldsList
        {
            get
            {
                if (_SelectedAvailablrFieldsList != null)
                    return _SelectedAvailablrFieldsList;

                return _SelectedAvailablrFieldsList;
            }
            set
            {
                _SelectedAvailablrFieldsList = value;
                OnPropertyChanged("SelectedAvailablrFieldsList");
            }
        }

        private ObservableCollection<PayorToolAvailablelFieldType> _AllSelectedFieldsList;
        public ObservableCollection<PayorToolAvailablelFieldType> AllSelectedFieldsList
        {
            get
            {
                if (_AllSelectedFieldsList != null)
                    return _AllSelectedFieldsList;

                return _AllSelectedFieldsList;
            }
            set
            {
                _AllSelectedFieldsList = value;
                OnPropertyChanged("AllSelectedFieldsList");
            }
        }

        private PayorToolAvailablelFieldType _SelectedFieldsList;
        public PayorToolAvailablelFieldType SelectedFieldsList
        {
            get
            {
                if (_SelectedFieldsList != null)
                    return _SelectedFieldsList;

                return _SelectedFieldsList;
            }
            set
            {
                _SelectedFieldsList = value;
                OnPropertyChanged("SelectedFieldsList");
            }
        }

        private void loadAvailableFieldsList()
        {
            TempAllAvailableFieldsList = AllAvailableFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>(serviceClients.PayorToolAvailablelFieldTypeClient.GetImportToolList().OrderBy(s => s.FieldName).ToList());
            SelectedAvailablrFieldsList = AllAvailableFieldsList.FirstOrDefault();
            AllSelectedFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>();
        }

        private void loadAvailableFieldsList1()
        {
            AllSelectedFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>(serviceClients.PayorToolAvailablelFieldTypeClient.GetFieldList().OrderBy(s => s.FieldName).ToList());
            SelectedFieldsList = AllSelectedFieldsList.FirstOrDefault();
        }

        private ICommand _MoveToSelectedFields;
        public ICommand MoveToSelectedFields
        {
            get
            {
                if (_MoveToSelectedFields == null)
                {
                    _MoveToSelectedFields = new BaseCommand(param => BeforeMovetoSelectedFields(), param => OnMoveToSelectedFields());
                }
                return _MoveToSelectedFields;
            }
        }

        public bool BeforeMovetoSelectedFields()
        {
            if (AllAvailableFieldsList == null)
            {
                return false;
            }
            if (AllAvailableFieldsList.Count > 0)
                return true;
            else
                return false;
        }

        public void OnMoveToSelectedFields()
        {
            try
            {

                ImportToolSeletedPaymentData objImportToolSeletedPaymentData = new ImportToolSeletedPaymentData();
                objImportToolSeletedPaymentData.PayorID = SelectedPayor.PayorID;
                objImportToolSeletedPaymentData.TemplateID = (Guid)SelectedPayortempalate.TemplateID;
                objImportToolSeletedPaymentData.PayorToolAvailableFeildsID = SelectedAvailablrFieldsList.FieldID;
                objImportToolSeletedPaymentData.FieldID = SelectedAvailablrFieldsList.FieldID;
                objImportToolSeletedPaymentData.FieldName = SelectedAvailablrFieldsList.FieldName;

                serviceClients.PayorTemplateClient.AddUpdatePaymentData(objImportToolSeletedPaymentData);

                AllSelectedFieldsList.Add(SelectedAvailablrFieldsList);

                AllAvailableFieldsList.Remove(SelectedAvailablrFieldsList);
            }
            catch
            {
            }
        }

        private ICommand _MoveToAvailbaleFields;
        public ICommand MoveToAvailbaleFields
        {
            get
            {
                if (_MoveToAvailbaleFields == null)
                {
                    _MoveToAvailbaleFields = new BaseCommand(param => BeforeMoveToAvailableFields(), param => OnMoveToAvailableFields());
                }
                return _MoveToAvailbaleFields;
            }
        }

        public bool BeforeMoveToAvailableFields()
        {
            if (AllSelectedFieldsList == null)
                return false;
            if (AllSelectedFieldsList.Count > 0)
                return true;
            else
                return false;
        }

        public void OnMoveToAvailableFields()
        {
            try
            {
                AllAvailableFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>(AllAvailableFieldsList);

                ImportToolSeletedPaymentData objImportToolSeletedPaymentData = new ImportToolSeletedPaymentData();
                objImportToolSeletedPaymentData.PayorID = SelectedPayor.PayorID;
                objImportToolSeletedPaymentData.TemplateID = (Guid)SelectedPayortempalate.TemplateID;
                objImportToolSeletedPaymentData.PayorToolAvailableFeildsID = SelectedFieldsList.FieldID;
                objImportToolSeletedPaymentData.FieldID = SelectedFieldsList.FieldID;
                objImportToolSeletedPaymentData.FieldName = SelectedFieldsList.FieldName;

                serviceClients.PayorTemplateClient.DeletePaymentData(objImportToolSeletedPaymentData);

                AllAvailableFieldsList.Add(SelectedFieldsList);
                AllSelectedFieldsList.Remove(SelectedFieldsList);


            }
            catch
            {
            }
        }

        private ObservableCollection<ImportToolSeletedPaymentData> _AllImportToolSeletedPaymentData;
        public ObservableCollection<ImportToolSeletedPaymentData> AllImportToolSeletedPaymentData
        {
            get
            {
                if (_AllImportToolSeletedPaymentData != null)
                    return _AllImportToolSeletedPaymentData;

                return _AllImportToolSeletedPaymentData;
            }
            set
            {
                _AllImportToolSeletedPaymentData = value;
                OnPropertyChanged("AllImportToolSeletedPaymentData");
            }
        }

        private void LoadSeletetedPaymentDataOnPayor()
        {
            try
            {

                if (SelectedPayortempalate == null)
                {
                    loadAvailableFieldsList();
                    AllSelectedFieldsList.Clear();
                    return;
                }
                AllImportToolSeletedPaymentData = new ObservableCollection<ImportToolSeletedPaymentData>(serviceClients.PayorTemplateClient.LoadImportToolSeletedPaymentData(SelectedPayor.PayorID, (Guid)SelectedPayortempalate.TemplateID));

                if (AllImportToolSeletedPaymentData.Count > 0)
                {
                    AllSelectedFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>();
                    foreach (var item in AllImportToolSeletedPaymentData)
                    {
                        SelectedFieldsList = new PayorToolAvailablelFieldType();
                        SelectedFieldsList.FieldID = item.FieldID;
                        SelectedFieldsList.FieldName = item.FieldName;
                        AllSelectedFieldsList.Add(SelectedFieldsList);
                    }


                    //Get Common value
                    //ObservableCollection<PayorToolAvailablelFieldType> result = new ObservableCollection<PayorToolAvailablelFieldType>(AllAvailableFieldsList
                    //                        .Where(p => !AllSelectedFieldsList.Any(p2 => p2.FieldID == p.FieldID)));

                    ObservableCollection<PayorToolAvailablelFieldType> result = new ObservableCollection<PayorToolAvailablelFieldType>(AllAvailableFieldsList
                                            .Where(p => !AllSelectedFieldsList.Any(p2 => p2.FieldID == p.FieldID)));


                    AllAvailableFieldsList = new ObservableCollection<PayorToolAvailablelFieldType>(result);

                    SelectedFieldsList = AllSelectedFieldsList.FirstOrDefault();
                    SelectedAvailablrFieldsList = AllAvailableFieldsList.FirstOrDefault();

                }
                else
                {
                    loadAvailableFieldsList();
                    AllSelectedFieldsList.Clear();
                }

            }
            catch(Exception ex)
            {
                if(objLog != null)
                {
                    objLog.AddLog("Billihg manager : " + ex.Message);
                }
            }
        }

        public delegate void OnOpenFormulaWindow();

        public event OnOpenFormulaWindow OpenFormulaWindow;

        #region for import tool
        /// <summary>
        /// add by neha for imprt tool
        /// </summary>

        private ICommand _OpenWindowTOAddFormula;
        public ICommand OpenWindowTOAddFormula
        {
            get
            {
                if (_OpenWindowTOAddFormula == null)
                {
                    _OpenWindowTOAddFormula = new BaseCommand(param => OpenFormulaWindows());
                }
                return
                    _OpenWindowTOAddFormula;
            }

        }

        private void OpenFormulaWindows()
        {
            if (OpenFormulaWindow != null)
                OpenFormulaWindow();
        }

        private PayorTool _CurrentPayorToolInfo;
        public PayorTool CurrentPayorToolInfo
        {
            get
            {
                if (_CurrentPayorToolInfo == null)
                    return new PayorTool();
                return _CurrentPayorToolInfo;
            }
            set
            {

                _CurrentPayorToolInfo = value == null ? new PayorTool() : value;
                OnPropertyChanged("CurrentPayorToolInfo");
            }
        }

        private ObservableCollection<PayorToolField> _PayorToolFieldsPropperty = new ObservableCollection<PayorToolField>();
        public ObservableCollection<PayorToolField> PayorToolFieldsProperty
        {
            get
            {
                return _PayorToolFieldsPropperty;
            }
            set
            {
                _PayorToolFieldsPropperty = value;
                OnPropertyChanged("PayorToolFieldsPropperty");
            }
        }

        internal ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken> getVariableExprTokens()
        {
            ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken> Fields = new ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken>();
            try
            {
                Guid pp = new Guid("cab1bd6b-2560-42cf-a4f8-962b728fd519");
                Guid? pp1 = new Guid("dd5af63e-b36a-4cf2-9f4d-4f7ae64e7b8e");



                CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgrWithTemplate(pp, pp1);
                _PayorToolFieldsPropperty = CurrentPayorToolInfo.ToolFields;

                foreach (PayorToolField field in _PayorToolFieldsPropperty)
                {
                    VM.VMLib.PayorForm.Formula.ExpressionToken token = new VM.VMLib.PayorForm.Formula.ExpressionToken { TokenString = field.AvailableFieldName, TokenType = VM.VMLib.PayorForm.Formula.ExpressionTokenType.Variable };
                    Fields.Add(token);
                }
                VM.VMLib.PayorForm.Formula.ExpressionToken expToken = new VM.VMLib.PayorForm.Formula.ExpressionToken();

                expToken.TokenString = "100";
                expToken.TokenType = VM.VMLib.PayorForm.Formula.ExpressionTokenType.Value;

                Fields.Add(expToken);
            }
            catch
            {
            }
            return Fields;
        }

        #endregion

        private ObservableCollection<ImportToolMasterStatementData> _ImportToolMasterStatementData;
        public ObservableCollection<ImportToolMasterStatementData> ImportToolMasterStatementData
        {
            get
            {
                return _ImportToolMasterStatementData;
            }
            set
            {
                _ImportToolMasterStatementData = value;
                OnPropertyChanged("ImportToolMasterStatementData");
            }
        }

        private ImportToolMasterStatementData _SelectedImportToolMasterStatementData;
        public ImportToolMasterStatementData SelectedImportToolMasterStatementData
        {
            get
            {
                return _SelectedImportToolMasterStatementData;
            }
            set
            {
                _SelectedImportToolMasterStatementData = value;
                OnPropertyChanged("SelectedImportToolMasterStatementData");
            }
        }

        private void LoadImportToolMasterStatementData()
        {
            ImportToolMasterStatementData = new ObservableCollection<ImportToolMasterStatementData>(serviceClients.BrokerCodeClient.LoadImportToolMasterStatementData());
            SelectedImportToolMasterStatementData = ImportToolMasterStatementData.FirstOrDefault();
        }

        private ObservableCollection<ImportToolStatementDataSettings> _ImportToolStatementDataSetting;
        public ObservableCollection<ImportToolStatementDataSettings> ImportToolStatementDataSetting
        {
            get
            {
                return _ImportToolStatementDataSetting;
            }
            set
            {
                _ImportToolStatementDataSetting = value;
                OnPropertyChanged("ImportToolStatementDataSetting");
            }
        }

        private ObservableCollection<ImportToolStatementDataSettings> _SelectedImportToolStatementDataSetting;
        public ObservableCollection<ImportToolStatementDataSettings> SelectedImportToolStatementDataSetting
        {
            get
            {
                return _SelectedImportToolStatementDataSetting;
            }
            set
            {
                _SelectedImportToolStatementDataSetting = value;
                OnPropertyChanged("SelectedImportToolStatementDataSetting");
            }
        }

        private void LoadStatementDateSetting()
        {
            try
            {
                ImportToolStatementDataSetting = new ObservableCollection<ImportToolStatementDataSettings>(serviceClients.PayorTemplateClient.LoadImportToolStatementDataSetting().ToList());
            }
            catch (Exception ex)
            {
                objLog.AddLog(ex.Message);
            }
        }

        private void LoadSelectedStamentDataSetting(Guid payorId, Guid templateId, int statementDataID)
        {
            try
            {
                if (ImportToolStatementDataSetting == null)
                    return;

                if (SelectedImportToolMasterStatementData.ID == 6)//Statement data "End data" 
                    isEnableBlankFields = true;

                else
                    isEnableBlankFields = false;

                SelectedImportToolStatementDataSetting = new ObservableCollection<ImportToolStatementDataSettings>(ImportToolStatementDataSetting.Where(p => p.PayorID == payorId && p.TemplateID == templateId && p.MasterStatementDataID == statementDataID).ToList());
                if (SelectedImportToolStatementDataSetting.Count > 0)
                {
                    foreach (var item in SelectedImportToolStatementDataSetting)
                    {
                        PayorSettingFixedRows = item.FixedRowLocation;
                        PayorSettingFixedCols = item.FixedColLocation;
                        PayorSettingRelativeSearch = item.RelativeSearch;
                        PayorSettingRelativeRows = item.RelativeRowLocation;
                        PayorSettingRelativeCols = item.RelativeColLocation;

                        if (!string.IsNullOrEmpty(item.BlankFieldsIndicator))
                            SelectedBlankFields = TempAllAvailableFieldsList.Where(p => p.FieldName.ToLower() == item.BlankFieldsIndicator.ToLower()).FirstOrDefault();
                        else
                            SelectedBlankFields = null;

                    }
                }
                else
                {
                    PayorSettingFixedRows = string.Empty;
                    PayorSettingFixedCols = string.Empty;
                    PayorSettingRelativeSearch = string.Empty;
                    PayorSettingRelativeRows = string.Empty;
                    PayorSettingRelativeCols = string.Empty;
                    SelectedBlankFields = null;
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("Error occured in statement data :" + ex.Message);
               // ActionLogger.Logger.WriteLog("Error occured in statement data :" + ex.Message, true);

            }
        }

        public delegate void OnOpenAddTemplate(string strCommandType);
        public event OnOpenAddTemplate OpenAddTemplate;

        private ICommand _AddImportToolTemplate;
        public ICommand AddImportToolTemplate
        {
            get
            {
                if (_AddImportToolTemplate == null)
                {
                    _AddImportToolTemplate = new BaseCommand(param => OnAddImportToolTemplate(param));
                }
                return _AddImportToolTemplate;
            }
        }



        private void OnAddImportToolTemplate(object param)
        {
            if (OpenAddTemplate != null)
                OpenAddTemplate("Add");

            ////Call to Load Tempalate when added Tempalte
            if (SelectedPayor != null)
            {
                LoadPayorTempalate(SelectedPayor.PayorID);
            }
        }

        private ICommand _UpdateImportToolTemplate;
        public ICommand UpdateImportToolTemplate
        {
            get
            {
                if (_UpdateImportToolTemplate == null)
                {
                    _UpdateImportToolTemplate = new BaseCommand(param => OnUpdateImportToolTemplate(param));
                }
                return _UpdateImportToolTemplate;
            }
        }
        private void OnUpdateImportToolTemplate(object param)
        {
            if (OpenAddTemplate != null)
                OpenAddTemplate("Update");

            ////Call to Load Tempalate when added Tempalte
            if (SelectedPayor != null)
            {
                LoadPayorTempalate(SelectedPayor.PayorID);
            }
        }

        private ICommand _DeleteImportToolPayor;
        public ICommand DeleteImportToolPayor
        {
            get
            {
                if (_DeleteImportToolPayor == null)
                {
                    _DeleteImportToolPayor = new BaseCommand(param => OnDeleteImportPayortemplate());
                }
                return _DeleteImportToolPayor;
            }
        }

        private void OnDeleteImportPayortemplate()
        {
            if (objLog == null) objLog = new MastersClient();
            if (serviceClients.PayorTemplateClient.deleteImportToolPayorTemplate(SelectedPayortempalate))
            {
                objLog.AddLog("OnDeleteImportPayortemplate request: " + SelectedPayortempalate.TemplateID + ", TemplateName: " + SelectedPayortempalate.TemplateName + ", User: " + RoleManager.userCredentialID);
                if (SelectedPayor != null)
                {
                    LoadPayorTempalate(SelectedPayor.PayorID);
                }

            }
        }

        private ICommand _DuplicateTemplate;
        public ICommand DuplicateTemplate
        {
            get
            {
                if (_DuplicateTemplate == null)
                {
                    _DuplicateTemplate = new BaseCommand(param => OnDuplicate());
                }
                return _DuplicateTemplate;
            }
        }

        private ICommand _DuplicateSaveTemplate;
        public ICommand DuplicateSaveTemplate
        {
            get
            {
                if (_DuplicateSaveTemplate == null)
                {
                    _DuplicateSaveTemplate = new BaseCommand(param => OnDuplicateSaveTemplate());
                }
                return _DuplicateSaveTemplate;
            }
        }

        private ICommand _DuplicateCancelTemplate;
        public ICommand DuplicateCancelTemplate
        {
            get
            {
                if (_DuplicateCancelTemplate == null)
                {
                    _DuplicateCancelTemplate = new BaseCommand(param => OnDuplicateCancelTemplate());
                }
                return _DuplicateCancelTemplate;
            }
        }

        private string _IsVisibleDuplicateUI;
        public string IsVisibleDuplicateUI
        {
            get
            {
                return _IsVisibleDuplicateUI;
            }
            set
            {
                _IsVisibleDuplicateUI = value;
                OnPropertyChanged("IsVisibleDuplicateUI");
            }
        }

        private void OnDuplicate()
        {
            IsVisibleDuplicateUI = "Visible";
            LoadDuplicatePayor();
        }

        private ObservableCollection<ImportToolStatementDataSettings> _DuplicateImportToolStatementDataSetting;
        public ObservableCollection<ImportToolStatementDataSettings> DuplicateImportToolStatementDataSetting
        {
            get
            {
                return _DuplicateImportToolStatementDataSetting;
            }
            set
            {
                _DuplicateImportToolStatementDataSetting = value;
                OnPropertyChanged("DuplicateImportToolStatementDataSetting");
            }
        }

        private ObservableCollection<ImportToolPaymentDataFieldsSettings> _DuplicateAllListOfImportToolPaymentDataFieldsSettings;
        public ObservableCollection<ImportToolPaymentDataFieldsSettings> DuplicateAllListOfImportToolPaymentDataFieldsSettings
        {
            get
            {
                return _DuplicateAllListOfImportToolPaymentDataFieldsSettings;
            }
            set
            {
                _DuplicateAllListOfImportToolPaymentDataFieldsSettings = value;
                OnPropertyChanged("DuplicateAllListOfImportToolPaymentDataFieldsSettings");
            }
        }

        private ObservableCollection<ImportToolSeletedPaymentData> _DuplicateAllImportToolSeletedPaymentData;
        public ObservableCollection<ImportToolSeletedPaymentData> DuplicateAllImportToolSeletedPaymentData
        {
            get
            {
                if (_DuplicateAllImportToolSeletedPaymentData != null)
                    return _DuplicateAllImportToolSeletedPaymentData;

                return _DuplicateAllImportToolSeletedPaymentData;
            }
            set
            {
                _DuplicateAllImportToolSeletedPaymentData = value;
                OnPropertyChanged("DuplicateAllImportToolSeletedPaymentData");
            }
        }

        private void OnDuplicateSaveTemplate()
        {
            IsVisibleDuplicateUI = "Hidden";

            if (SelectedPayortempalate.TemplateID != SelectedDuplicatePayortempalate.TemplateID)
            {
                try
                {
                    ImportToolStatementDataSetting = new ObservableCollection<ImportToolStatementDataSettings>(ImportToolStatementDataSetting);
                    DuplicateImportToolStatementDataSetting = new ObservableCollection<ImportToolStatementDataSettings>(ImportToolStatementDataSetting.Where(p => p.PayorID == SelectedPayor.PayorID && p.TemplateID == SelectedPayortempalate.TemplateID));

                    foreach (var item in DuplicateImportToolStatementDataSetting)
                    {
                        StatementDataSettings = new ImportToolStatementDataSettings();
                        StatementDataSettings.FixedColLocation = item.FixedColLocation;
                        StatementDataSettings.FixedRowLocation = item.FixedRowLocation;
                        StatementDataSettings.RelativeSearch = item.RelativeSearch;
                        StatementDataSettings.RelativeRowLocation = item.RelativeRowLocation;
                        StatementDataSettings.RelativeColLocation = item.RelativeColLocation;
                        StatementDataSettings.MasterStatementDataID = item.MasterStatementDataID;
                        StatementDataSettings.PayorID = SelectedDuplicatePayors.PayorID;
                        StatementDataSettings.TemplateID = (Guid)SelectedDuplicatePayortempalate.TemplateID;
                        StatementDataSettings.BlankFieldsIndicator = item.BlankFieldsIndicator;
                        StatementDataSettings.IsBlankFieldsIndicatorAvailable = item.IsBlankFieldsIndicatorAvailable;

                        bool bValue = serviceClients.PayorTemplateClient.AddUpdateImportToolStatementDataSettings(StatementDataSettings);
                    }

                    if (DuplicateImportToolStatementDataSetting.Count > 0)
                    {
                        LoadStatementDateSetting();
                    }

                    DuplicateAllImportToolSeletedPaymentData = new ObservableCollection<ImportToolSeletedPaymentData>(AllImportToolSeletedPaymentData.Where(p => p.PayorID == SelectedPayor.PayorID && p.TemplateID == SelectedPayortempalate.TemplateID));

                    foreach (var itemSelectedPaymentData in DuplicateAllImportToolSeletedPaymentData)
                    {
                        ImportToolSeletedPaymentData objImportToolSeletedPaymentData = new ImportToolSeletedPaymentData();

                        objImportToolSeletedPaymentData.PayorID = SelectedDuplicatePayors.PayorID;
                        objImportToolSeletedPaymentData.TemplateID = (Guid)SelectedDuplicatePayortempalate.TemplateID;
                        objImportToolSeletedPaymentData.PayorToolAvailableFeildsID = itemSelectedPaymentData.PayorToolAvailableFeildsID;
                        objImportToolSeletedPaymentData.FieldID = itemSelectedPaymentData.FieldID;
                        objImportToolSeletedPaymentData.FieldName = itemSelectedPaymentData.FieldName;
                        //Add to database
                        serviceClients.PayorTemplateClient.AddUpdatePaymentData(objImportToolSeletedPaymentData);
                    }

                    DuplicateAllListOfImportToolPaymentDataFieldsSettings = new ObservableCollection<ImportToolPaymentDataFieldsSettings>(AllListOfImportToolPaymentDataFieldsSettings.Where(p => p.PayorID == SelectedPayor.PayorID && p.TemplateID == SelectedPayortempalate.TemplateID));

                    foreach (var itemPaymentData in DuplicateAllListOfImportToolPaymentDataFieldsSettings)
                    {
                        ObjImportToolPaymentDataFieldsSettings = new ImportToolPaymentDataFieldsSettings();
                        ObjImportToolPaymentDataFieldsSettings.PayorID = SelectedDuplicatePayors.PayorID;
                        ObjImportToolPaymentDataFieldsSettings.TemplateID = (Guid)SelectedDuplicatePayortempalate.TemplateID;
                        ObjImportToolPaymentDataFieldsSettings.PayorToolAvailableFeildsID = itemPaymentData.PayorToolAvailableFeildsID;
                        ObjImportToolPaymentDataFieldsSettings.FieldsID = itemPaymentData.FieldsID;
                        ObjImportToolPaymentDataFieldsSettings.FieldsName = itemPaymentData.FieldsName;
                        ObjImportToolPaymentDataFieldsSettings.FixedRowLocation = itemPaymentData.FixedRowLocation;
                        ObjImportToolPaymentDataFieldsSettings.FixedColLocation = itemPaymentData.FixedColLocation;
                        ObjImportToolPaymentDataFieldsSettings.HeaderSearch = itemPaymentData.HeaderSearch;
                        ObjImportToolPaymentDataFieldsSettings.RelativeRowLocation = itemPaymentData.RelativeRowLocation;
                        ObjImportToolPaymentDataFieldsSettings.RelativeColLocation = itemPaymentData.RelativeColLocation;
                        ObjImportToolPaymentDataFieldsSettings.PartOfPrimaryKey = itemPaymentData.PartOfPrimaryKey;
                        ObjImportToolPaymentDataFieldsSettings.CalculatedFields = itemPaymentData.CalculatedFields;
                        ObjImportToolPaymentDataFieldsSettings.FormulaExpression = itemPaymentData.FormulaExpression;
                        ObjImportToolPaymentDataFieldsSettings.PayorToolMaskFieldTypeId = itemPaymentData.PayorToolMaskFieldTypeId;

                        ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataStartColValue = itemPaymentData.selectedPaymentDataStartColValue;
                        ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataEndColValue = itemPaymentData.selectedPaymentDataEndColValue;
                        ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataStartRowValue = itemPaymentData.selectedPaymentDataStartRowValue;
                        ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataEndRowValue = itemPaymentData.selectedPaymentDataEndRowValue;

                        //Add to database
                        serviceClients.PayorTemplateClient.AddUpdatePaymentDataFieldsSetting(ObjImportToolPaymentDataFieldsSettings);

                    }

                    if (DuplicateAllListOfImportToolPaymentDataFieldsSettings.Count > 0)
                    {

                        //Load Latest Payment data
                        LoadPaymentDataFields();

                        LoadSeletetedPaymentDataOnPayor();
                    }
                }
                catch
                {
                }

            }

        }

        private void OnDuplicateCancelTemplate()
        {
            IsVisibleDuplicateUI = "Hidden";
        }

        private bool _isEnabled;
        public bool isEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("isEnabled");
            }
        }

        private ICommand _AddNewPayorPhrase;
        public ICommand AddNewPayorPhrase
        {
            get
            {
                if (_AddNewPayorPhrase == null)
                {
                    _AddNewPayorPhrase = new BaseCommand(param => OnAddNewPayorPhrase());
                }
                return _AddNewPayorPhrase;
            }
        }

        private bool OnAddNewPayorPhrase()
        {
            isEnabled = true;
            return true;
        }

        private ICommand _SaveNewPhrase;
        public ICommand SaveNewPhrase
        {
            get
            {
                if (_SaveNewPhrase == null)
                {
                    _SaveNewPhrase = new BaseCommand(param => BeforeSaveNewPayorPhrase(), param => OnSaveNewPayorPhrase());
                }
                return _SaveNewPhrase;
            }
        }

        private bool BeforeSaveNewPayorPhrase()
        {
            if (isEnabled)
                return true;
            else
                return false;
        }

        private ImportToolPayorPhrase _SelectedImportToolPayorPhrase;
        public ImportToolPayorPhrase SelectedImportToolPayorPhrase
        {
            get
            {
                return _SelectedImportToolPayorPhrase;
            }
            set
            {
                _SelectedImportToolPayorPhrase = value;
            }
        }

        private string _PayorSettingFixedRows;
        public string PayorSettingFixedRows
        {
            get
            {
                return _PayorSettingFixedRows;
            }
            set
            {
                _PayorSettingFixedRows = value;
                OnPropertyChanged("PayorSettingFixedRows");
            }
        }

        private string _PayorSettingFixedCols;
        public string PayorSettingFixedCols
        {
            get
            {
                return _PayorSettingFixedCols;
            }
            set
            {
                _PayorSettingFixedCols = value;
                OnPropertyChanged("PayorSettingFixedCols");
            }
        }

        private string _PayorSettingPhrase;
        public string PayorSettingPhrase
        {
            get
            {
                return _PayorSettingPhrase;
            }
            set
            {
                _PayorSettingPhrase = value;
                OnPropertyChanged("PayorSettingPhrase");
            }
        }

        private string _PayorSettingRelativeSearch;
        public string PayorSettingRelativeSearch
        {
            get
            {
                return _PayorSettingRelativeSearch;
            }
            set
            {
                _PayorSettingRelativeSearch = value;
                OnPropertyChanged("PayorSettingRelativeSearch");
            }
        }

        private string _PayorSettingRelativeRows;
        public string PayorSettingRelativeRows
        {
            get
            {
                return _PayorSettingRelativeRows;
            }
            set
            {
                _PayorSettingRelativeRows = value;
                OnPropertyChanged("PayorSettingRelativeRows");
            }
        }

        private string _PayorSettingRelativeCols;
        public string PayorSettingRelativeCols
        {
            get
            {
                return _PayorSettingRelativeCols;
            }
            set
            {
                _PayorSettingRelativeCols = value;
                OnPropertyChanged("PayorSettingRelativeCols");
            }
        }

        private void OnSaveNewPayorPhrase()
        {
            string strPayor = string.Empty;

            strPayor = serviceClients.PayorTemplateClient.ValidatePhraseAvailbility(PayorSettingPhrase);

            if (!string.IsNullOrEmpty(strPayor))
            {
                MessageBoxResult result = MessageBox.Show("'" + strPayor + "'" + " phrases is available into another template." + Environment.NewLine + " Do you want continue.", "CommisionDept", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    SaveNewPayorPhrase();
                }

            }
            else//If Phrase is not available into another phrase then save
            {
                SaveNewPayorPhrase();
            }

        }

        private void SaveNewPayorPhrase()
        {
            try
            {
                string[] Phrases = null;
                if (!string.IsNullOrEmpty(PayorSettingPhrase))
                {
                    if (PayorSettingPhrase.Contains(","))
                    {
                        Phrases = PayorSettingPhrase.Split(',');

                        foreach (var itemPhrase in Phrases)
                        {
                            SelectedImportToolPayorPhrase = new ImportToolPayorPhrase();
                            SelectedImportToolPayorPhrase.FileType = SelectedFileTypeValue;
                            SelectedImportToolPayorPhrase.FileFormat = SelectedFileFormate;
                            SelectedImportToolPayorPhrase.FixedColLocation = Convert.ToString(PayorSettingFixedCols);
                            SelectedImportToolPayorPhrase.FixedRowLocation = Convert.ToString(PayorSettingFixedRows);
                            SelectedImportToolPayorPhrase.PayorID = SelectedPayor.PayorID;
                            SelectedImportToolPayorPhrase.PayorName = SelectedPayor.PayorName;
                            SelectedImportToolPayorPhrase.TemplateID = (Guid)SelectedPayortempalate.TemplateID;
                            SelectedImportToolPayorPhrase.TemplateName = SelectedPayortempalate.TemplateName;
                            SelectedImportToolPayorPhrase.PayorPhrases = itemPhrase;
                            SelectedImportToolPayorPhrase.RelativeSearchText = PayorSettingRelativeSearch;
                            SelectedImportToolPayorPhrase.RelativeRowLocation = Convert.ToString(PayorSettingRelativeRows);
                            SelectedImportToolPayorPhrase.RelativeColLocation = Convert.ToString(PayorSettingRelativeCols);

                            serviceClients.PayorTemplateClient.AddUpdateImportToolPayorPhrase(SelectedImportToolPayorPhrase);
                        }
                    }
                    else
                    {

                        SelectedImportToolPayorPhrase = new ImportToolPayorPhrase();
                        SelectedImportToolPayorPhrase.FileType = SelectedFileTypeValue;
                        SelectedImportToolPayorPhrase.FileFormat = SelectedFileFormate;
                        SelectedImportToolPayorPhrase.FixedColLocation = Convert.ToString(PayorSettingFixedCols);
                        SelectedImportToolPayorPhrase.FixedRowLocation = Convert.ToString(PayorSettingFixedRows);
                        SelectedImportToolPayorPhrase.PayorID = SelectedPayor.PayorID;
                        SelectedImportToolPayorPhrase.PayorName = SelectedPayor.PayorName;
                        SelectedImportToolPayorPhrase.TemplateID = (Guid)SelectedPayortempalate.TemplateID;
                        SelectedImportToolPayorPhrase.TemplateName = SelectedPayortempalate.TemplateName;
                        SelectedImportToolPayorPhrase.PayorPhrases = PayorSettingPhrase;
                        SelectedImportToolPayorPhrase.RelativeSearchText = PayorSettingRelativeSearch;
                        SelectedImportToolPayorPhrase.RelativeRowLocation = Convert.ToString(PayorSettingRelativeRows);
                        SelectedImportToolPayorPhrase.RelativeColLocation = Convert.ToString(PayorSettingRelativeCols);

                        serviceClients.PayorTemplateClient.AddUpdateImportToolPayorPhrase(SelectedImportToolPayorPhrase);
                    }
                }


                isEnabled = false;
            }
            catch
            {
            }
        }

        private ICommand _CmdCheckPhrase;
        public ICommand CmdCheckPhrase
        {
            get
            {
                if (_CmdCheckPhrase == null)
                {
                    _CmdCheckPhrase = new BaseCommand(param => BeforeCheckPhares(), param => OnCheckPhrase());
                }
                return _CmdCheckPhrase;
            }
        }

        private bool BeforeCheckPhares()
        {
            bool bValue = true;
            return bValue;
        }

        private void OnCheckPhrase()
        {
            if (string.IsNullOrEmpty(PayorSettingPhrase))
            {
                return;
            }

            objImportToolPhrase = new List<ImportToolPayorPhrase>(serviceClients.PayorTemplateClient.CheckAvailability(Convert.ToString(PayorSettingPhrase))).OrderBy(p => p.PayorName).ToList();
            if (onOpenPhraseSearchedWindow != null)
            {
                onOpenPhraseSearchedWindow();
            }
        }

        private ICommand _CmdEditAndDispaly;
        public ICommand CmdEditAndDispaly
        {
            get
            {
                if (_CmdEditAndDispaly == null)
                {
                    _CmdEditAndDispaly = new BaseCommand(param => BeforeEditAndDispaly(), param => OnEditAndDispaly());
                }
                return _CmdEditAndDispaly;
            }
        }

        private bool BeforeEditAndDispaly()
        {
            bool bValue = true;
            return bValue;
        }

        private void OnEditAndDispaly()
        {
            FillPayorLst();
        }

        private ICommand _CmdImport;
        public ICommand CmdImport
        {
            get
            {
                if (_CmdImport == null)
                {
                    _CmdImport = new BaseCommand(param => BeforeImport(), param => OnImport());
                }
                return _CmdImport;
            }
        }

        private bool BeforeImport()
        {
            return true;
        }

        private void OnImport()
        {
            try
            {
                string supportedExtensions = "*.xls,*.xlsx,*.txt,*.csv";
                //Acme commented on server change  string strFilePath = "199.66.132.155/FileManager/Uploadbatch/Import/Processing";
                string strFilePath = "204.13.182.12/FileManager/Uploadbatch/Import/Processing";
                int intStartServiceAgain = Directory.GetFiles(strFilePath, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).Count();

            }
            catch
            {
            }


        }

        private DisplayedPayor _selectedPayorForEdit;
        public DisplayedPayor SelectedPayorForEdit
        {
            get
            {
                return _selectedPayorForEdit;
            }
            set
            {
                _selectedPayorForEdit = value;

                if (_selectedPayorForEdit != null)
                {
                    OnPropertyChanged("SelectedPayorForEdit");
                }
            }
        }

        private ObservableCollection<DisplayedPayor> _payorLstForEdit;
        public ObservableCollection<DisplayedPayor> PayorLstForEdit
        {
            get
            {
                return _payorLstForEdit;
            }
            set
            {
                _payorLstForEdit = value;
                OnPropertyChanged("PayorLstForEdit");
            }
        }

        private void FillPayorLst()
        {
            PayorLst = new ObservableCollection<DisplayedPayor>();
            try
            {
                try
                {
                    PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = PayorStatus.Active };
                    ObservableCollection<DisplayedPayor> _payorLst = new ObservableCollection<DisplayedPayor>(serviceClients.DisplayedPayorClient.GetDisplayPayors(Guid.Empty, fillInfo));
                    PayorLstForEdit = new ObservableCollection<DisplayedPayor>(_payorLst.OrderBy(p => p.PayorName));
                    PayorLstForEdit.Add(new DisplayedPayor { PayorName = "--All--", PayorID = Guid.Empty, });
                   // SelectedPayorForEdit = PayorLstForEdit.Where(p => p.PayorID == Guid.Empty).FirstOrDefault();

                    tempobjImportToolPhrase = objImportToolPhrase = new List<ImportToolPayorPhrase>(serviceClients.PayorTemplateClient.GetAllTemplatePhraseOnTemplate());
                    SelectedobjImportToolPhrase = tempobjImportToolPhrase.FirstOrDefault();

                    SelectedPayorForEdit = PayorLstForEdit.Where(p => p.PayorID == Guid.Empty).FirstOrDefault();

                    if (SelectedPayorForEdit.PayorName == "--All--")
                    {
                        Guid? guidNull = null;
                        LoadTempalate(guidNull);
                    }
                    else
                    {
                        LoadTempalate(SelectedobjImportToolPhrase.PayorID);
                    }
                    if (OnEditAndDisplayWindow != null)
                    {
                        OnEditAndDisplayWindow();
                    }
                }
                catch (Exception ex)
                {
                    if (objLog != null)
                    {
                        objLog.AddLog(ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                if(objLog != null)
                {
                    objLog.AddLog(ex.Message);
                }
            }
        }

        private List<Tempalate> _PayorTemplateForEdit;
        public List<Tempalate> PayorTemplateForEdit
        {
            get
            {
                return _PayorTemplateForEdit;
            }
            set
            {
                _PayorTemplateForEdit = value;
                OnPropertyChanged("PayorTemplateForEdit");
            }
        }

        private Tempalate _SelectedPayortempalateForEdit;
        public Tempalate SelectedPayortempalateForEdit
        {
            get
            {
                return _SelectedPayortempalateForEdit;
            }
            set
            {
                _SelectedPayortempalateForEdit = value;
                OnPropertyChanged("SelectedPayortempalateForEdit");
            }
        }



        private void LoadTempalate(Guid? PayorID)
        {

            PayorTemplateForEdit = new List<Tempalate>(serviceClients.PayorTemplateClient.GetAllPayorTemplate(PayorID)).ToList();

            if (PayorTemplateForEdit.Count > 0)
            {
                SelectedPayortempalateForEdit = PayorTemplateForEdit.FirstOrDefault();
            }
            else
            {
                SelectedPayortempalateForEdit = null;
            }
        }

        void PayorClient_GetPayorsOnlyCompleted(object sender, GetDisplayPayorsCompletedEventArgs e)
        {

        }

        private ObservableCollection<DisplayedPayor> _payorLst;
        public ObservableCollection<DisplayedPayor> PayorLst
        {
            get
            {
                return _payorLst;
            }
            set
            {
                _payorLst = value;
                OnPropertyChanged("PayorLst");
            }
        }

        private ImportToolPayorPhrase _SelectedobjImportToolPhrase;
        public ImportToolPayorPhrase SelectedobjImportToolPhrase
        {
            get
            {
                return _SelectedobjImportToolPhrase;
            }
            set
            {
                _SelectedobjImportToolPhrase = value;
                OnPropertyChanged("SelectedobjImportToolPhrase");
            }
        }

        private ImportToolStatementDataSettings _StatementDataSettings;
        public ImportToolStatementDataSettings StatementDataSettings
        {
            get
            {
                return _StatementDataSettings;
            }
            set
            {
                _StatementDataSettings = value;
                OnPropertyChanged("StatementDataSettings");
            }
        }

        private ICommand _CmdSaveStatementData;
        public ICommand CmdSaveStatementData
        {
            get
            {
                if (_CmdSaveStatementData == null)
                {
                    _CmdSaveStatementData = new BaseCommand(param => BeforeSaveStatementSettings(), param => OnSaveStatementSettings());
                }
                return _CmdSaveStatementData;
            }
        }

        private bool BeforeSaveStatementSettings()
        {
            return true;
        }

        private void OnSaveStatementSettings()
        {
            try
            {
                StatementDataSettings = new ImportToolStatementDataSettings();
                StatementDataSettings.FixedColLocation = PayorSettingFixedCols;
                StatementDataSettings.FixedRowLocation = PayorSettingFixedRows;
                StatementDataSettings.RelativeSearch = PayorSettingRelativeSearch;
                StatementDataSettings.RelativeRowLocation = PayorSettingRelativeRows;
                StatementDataSettings.RelativeColLocation = PayorSettingRelativeCols;
                StatementDataSettings.MasterStatementDataID = SelectedImportToolMasterStatementData.ID;
                StatementDataSettings.PayorID = SelectedPayor.PayorID;
                StatementDataSettings.TemplateID = (Guid)SelectedPayortempalate.TemplateID;

                if (SelectedBlankFields != null)
                {
                    if (!string.IsNullOrEmpty(SelectedBlankFields.FieldName))
                    {
                        StatementDataSettings.IsBlankFieldsIndicatorAvailable = true;
                        StatementDataSettings.BlankFieldsIndicator = SelectedBlankFields.FieldName;
                    }
                    else
                    {
                        StatementDataSettings.IsBlankFieldsIndicatorAvailable = false;
                    }
                }
                else
                {
                    StatementDataSettings.IsBlankFieldsIndicatorAvailable = false;
                }

                bool bValue = serviceClients.PayorTemplateClient.AddUpdateImportToolStatementDataSettings(StatementDataSettings);

                if (bValue)
                {
                    LoadStatementDateSetting();
                }
                else
                {
                    MessageBox.Show("An error occurred while saving statement settings in the system.", "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                if (objLog != null)
                {
                    objLog.AddLog("OnSaveStatementSettings ex: " + ex.Message);
                }
            }

        }

        private ImportToolPaymentDataFieldsSettings _ObjImportToolPaymentDataFieldsSettings;
        public ImportToolPaymentDataFieldsSettings ObjImportToolPaymentDataFieldsSettings
        {
            get
            {
                return _ObjImportToolPaymentDataFieldsSettings;
            }
            set
            {
                _ObjImportToolPaymentDataFieldsSettings = value;
                OnPropertyChanged("ObjImportToolPaymentDataFieldsSettings");
            }
        }

        public ICommand _btnSavePaymentData;
        public ICommand btnSavePaymentData
        {
            get
            {
                if (_btnSavePaymentData == null)
                {
                    _btnSavePaymentData = new BaseCommand(param => BeforeSavePaymentDataSettings(), param => OnSavePaymentDataSettings());
                }
                return _btnSavePaymentData;
            }
        }

        private bool BeforeSavePaymentDataSettings()
        {
            bool bValue = true;
            if (AllSelectedFieldsList == null)
            {
                return false;
            }

            if (AllSelectedFieldsList.Count > 0)
            {
                if (SelectedFieldsList != null)
                {
                    bValue = true;
                }
                else
                {
                    bValue = false;
                }

            }
            else
            {
                bValue = false;
            }

            return bValue;
        }

        private void OnSavePaymentDataSettings()
        {
            ObjImportToolPaymentDataFieldsSettings = new ImportToolPaymentDataFieldsSettings();
            ObjImportToolPaymentDataFieldsSettings.PayorID = SelectedPayor.PayorID;
            ObjImportToolPaymentDataFieldsSettings.TemplateID = (Guid)SelectedPayortempalate.TemplateID;
            ObjImportToolPaymentDataFieldsSettings.PayorToolAvailableFeildsID = SelectedFieldsList.FieldID;
            ObjImportToolPaymentDataFieldsSettings.FieldsID = SelectedFieldsList.FieldID;
            ObjImportToolPaymentDataFieldsSettings.FieldsName = SelectedFieldsList.FieldName;
            ObjImportToolPaymentDataFieldsSettings.FixedRowLocation = selectedPaymentDataRowValue;
            ObjImportToolPaymentDataFieldsSettings.FixedColLocation = selectedPaymentDataColValue;
            ObjImportToolPaymentDataFieldsSettings.HeaderSearch = strHeaderSearch;
            ObjImportToolPaymentDataFieldsSettings.RelativeRowLocation = selectedHeaderRowsValue;
            ObjImportToolPaymentDataFieldsSettings.RelativeColLocation = selectedHeaderColsValue;
            ObjImportToolPaymentDataFieldsSettings.PartOfPrimaryKey = isPartOfPrimaryKeyYes;
            ObjImportToolPaymentDataFieldsSettings.CalculatedFields = isCalculatedFieldsYes;
            ObjImportToolPaymentDataFieldsSettings.FormulaExpression = strformulaExpression;

            if (SelectedMaskFieldsTypes != null)
                ObjImportToolPaymentDataFieldsSettings.PayorToolMaskFieldTypeId = SelectedMaskFieldsTypes.PTMaskFieldTypeId;

            ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataStartColValue = selectedPaymentDataStartColValue;
            ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataEndColValue = selectedPaymentDataEndColValue;
            ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataStartRowValue = selectedPaymentDataStartRowValue;
            ObjImportToolPaymentDataFieldsSettings.selectedPaymentDataEndRowValue = selectedPaymentDataEndRowValue;
            //newly added 
            ObjImportToolPaymentDataFieldsSettings.TransID = SelectedTranslatorsTypes.TransID;
            ObjImportToolPaymentDataFieldsSettings.TransName = SelectedTranslatorsTypes.Name;
            //Added default text 
            //strDefaultText
            ObjImportToolPaymentDataFieldsSettings.strDefaultText = strDefaultText;
            serviceClients.PayorTemplateClient.AddUpdatePaymentDataFieldsSetting(ObjImportToolPaymentDataFieldsSettings);
            //Load Latest Payment data
            LoadPaymentDataFields();

        }

        private ObservableCollection<ImportToolPaymentDataFieldsSettings> _AllListOfImportToolPaymentDataFieldsSettings;
        public ObservableCollection<ImportToolPaymentDataFieldsSettings> AllListOfImportToolPaymentDataFieldsSettings
        {
            get
            {
                return _AllListOfImportToolPaymentDataFieldsSettings;
            }
            set
            {
                _AllListOfImportToolPaymentDataFieldsSettings = value;
                OnPropertyChanged("AllListOfImportToolPaymentDataFieldsSettings");
            }
        }

        private ObservableCollection<ImportToolPaymentDataFieldsSettings> _TempAllListOfImportToolPaymentDataFieldsSettings;
        public ObservableCollection<ImportToolPaymentDataFieldsSettings> TempAllListOfImportToolPaymentDataFieldsSettings
        {
            get
            {
                return _TempAllListOfImportToolPaymentDataFieldsSettings;
            }
            set
            {
                _TempAllListOfImportToolPaymentDataFieldsSettings = value;
                OnPropertyChanged("TempAllListOfImportToolPaymentDataFieldsSettings");
            }
        }

        private void LoadPaymentDataFields()
        {
            try
            {
                if (SelectedPayortempalate != null)
                {

                    AllListOfImportToolPaymentDataFieldsSettings = new ObservableCollection<ImportToolPaymentDataFieldsSettings>(serviceClients.PayorTemplateClient.LoadPaymentDataFieldsSetting((Guid)SelectedPayor.PayorID, (Guid)SelectedPayortempalate.TemplateID).ToList());
                }

            }
            catch
            {
            }
        }

        private void FillSelectedPaymentData()
        {
            try
            {
                LoadPaymentDataFields();

                if (AllListOfImportToolPaymentDataFieldsSettings == null)
                {
                    return;
                }

                TempAllListOfImportToolPaymentDataFieldsSettings = new ObservableCollection<ImportToolPaymentDataFieldsSettings>(AllListOfImportToolPaymentDataFieldsSettings.Where(p => p.PayorID == SelectedPayor.PayorID && p.TemplateID == SelectedPayortempalate.TemplateID && p.FieldsID == SelectedFieldsList.FieldID).ToList());

                if (TempAllListOfImportToolPaymentDataFieldsSettings.Count > 0)
                {
                    foreach (var item in TempAllListOfImportToolPaymentDataFieldsSettings)
                    {
                        selectedPaymentDataColValue = item.FixedColLocation;
                        selectedPaymentDataRowValue = item.FixedRowLocation;
                        strHeaderSearch = item.HeaderSearch;
                        selectedHeaderRowsValue = item.RelativeRowLocation;
                        selectedHeaderColsValue = item.RelativeColLocation;
                        selectedPaymentDataStartColValue = item.selectedPaymentDataStartColValue;
                        selectedPaymentDataStartRowValue = item.selectedPaymentDataStartRowValue;
                        selectedPaymentDataEndColValue = item.selectedPaymentDataEndColValue;
                        selectedPaymentDataEndRowValue = item.selectedPaymentDataEndRowValue;

                        if (item.PartOfPrimaryKey)
                            isPartOfPrimaryKeyYes = item.PartOfPrimaryKey;
                        else
                            isPartOfPrimaryKeyNo = true;

                        if (item.CalculatedFields)
                            isCalculatedFieldsYes = item.CalculatedFields;
                        else
                            isCalculatedFieldsNo = true;

                        strformulaExpression = item.FormulaExpression;
                        if (item.FieldsName.ToLower() == "commissionpercentage")
                        {
                            IsTransTypeEnabled = true;
                            SelectedTranslatorsTypes = ListTranslatorTypes.Where(p => p.TransID == item.TransID).FirstOrDefault();
                        }
                        else
                        {
                            IsTransTypeEnabled = false;
                        }

                        strDefaultText = item.strDefaultText;
                        SelectedMaskFieldsTypes = ListMaskFieldTypes.Where(p => p.PTMaskFieldTypeId == item.PayorToolMaskFieldTypeId).FirstOrDefault();

                    }

                }
                else
                {
                    selectedPaymentDataColValue = string.Empty;
                    selectedPaymentDataRowValue = string.Empty;

                    selectedPaymentDataStartColValue = string.Empty;
                    selectedPaymentDataStartRowValue = string.Empty;
                    selectedPaymentDataEndColValue = string.Empty;
                    selectedPaymentDataEndRowValue = string.Empty;

                    strHeaderSearch = string.Empty;
                    selectedHeaderRowsValue = string.Empty;
                    selectedHeaderColsValue = string.Empty;
                    isPartOfPrimaryKeyNo = true;
                    isCalculatedFieldsNo = true;
                    strformulaExpression = string.Empty;

                    SelectedTranslatorsTypes = ListTranslatorTypes.FirstOrDefault();
                    strDefaultText = string.Empty;
                }

            }
            catch
            {
            }
        }

        private ICommand _ClearFormula;
        public ICommand ClearFormula
        {
            get
            {
                if (_ClearFormula == null)
                {
                    _ClearFormula = new BaseCommand(param => OnClearFormula());
                }
                return _ClearFormula;
            }
        }

        private string _strformulaExpression;
        public string strformulaExpression
        {
            get
            {
                return _strformulaExpression;
            }
            set
            {
                _strformulaExpression = value;
                OnPropertyChanged("strformulaExpression");
            }
        }

        private void OnClearFormula()
        {
            strformulaExpression = string.Empty;
        }

        private ObservableCollection<MaskFieldTypes> _ListMaskFieldTypes;
        public ObservableCollection<MaskFieldTypes> ListMaskFieldTypes
        {
            get
            {
                return _ListMaskFieldTypes;
            }
            set
            {
                _ListMaskFieldTypes = value;
                OnPropertyChanged("ListMaskFieldTypes");
            }
        }

        private ObservableCollection<MaskFieldTypes> _tempListMaskFieldTypes;
        public ObservableCollection<MaskFieldTypes> tempListMaskFieldTypes
        {
            get
            {
                return _tempListMaskFieldTypes;
            }
            set
            {
                _tempListMaskFieldTypes = value;
                OnPropertyChanged("tempListMaskFieldTypes");
            }
        }


        private MaskFieldTypes _SelectedMaskFieldsTypes;
        public MaskFieldTypes SelectedMaskFieldsTypes
        {
            get
            {
                return _SelectedMaskFieldsTypes;
            }
            set
            {
                _SelectedMaskFieldsTypes = value;
                OnPropertyChanged("SelectedMaskFieldsTypes");
            }
        }

        private ObservableCollection<TranslatorTypes> _ListTranslatorTypes;
        public ObservableCollection<TranslatorTypes> ListTranslatorTypes
        {
            get
            {
                return _ListTranslatorTypes;
            }
            set
            {
                _ListTranslatorTypes = value;
                OnPropertyChanged("ListTranslatorTypes");
            }
        }

        private ObservableCollection<TranslatorTypes> _tempListTranslatorsTypes;
        public ObservableCollection<TranslatorTypes> tempListTranslatorsTypes
        {
            get
            {
                return _tempListTranslatorsTypes;
            }
            set
            {
                _tempListTranslatorsTypes = value;
                OnPropertyChanged("tempListTranslatorsTypes");
            }
        }

        private TranslatorTypes _SelectedTranslatorsTypes;
        public TranslatorTypes SelectedTranslatorsTypes
        {
            get
            {
                return _SelectedTranslatorsTypes;
            }
            set
            {
                _SelectedTranslatorsTypes = value;
                OnPropertyChanged("SelectedTranslatorsTypes");
            }
        }

        private string _strDefaultText = string.Empty;
        public string strDefaultText
        {
            get
            {
                return _strDefaultText;
            }
            set
            {
                _strDefaultText = value;
                OnPropertyChanged("strDefaultText");
            }
        }

        private bool _IsTransTypeEnabled;
        public bool IsTransTypeEnabled
        {
            get
            {
                return _IsTransTypeEnabled;
            }
            set
            {
                _IsTransTypeEnabled = value;
                OnPropertyChanged("IsTransTypeEnabled");
            }
        }

        private string _selectedPaymentDataColValue;
        public string selectedPaymentDataColValue
        {
            get
            {
                return _selectedPaymentDataColValue;
            }
            set
            {
                _selectedPaymentDataColValue = value;
                OnPropertyChanged("selectedPaymentDataColValue");
            }
        }

        private string _selectedPaymentDataRowValue;
        public string selectedPaymentDataRowValue
        {
            get
            {
                return _selectedPaymentDataRowValue;
            }
            set
            {
                _selectedPaymentDataRowValue = value;
                OnPropertyChanged("selectedPaymentDataRowValue");
            }
        }

        private string _selectedPaymentDataStartColValue;
        public string selectedPaymentDataStartColValue
        {
            get
            {
                return _selectedPaymentDataStartColValue;
            }
            set
            {
                _selectedPaymentDataStartColValue = value;
                OnPropertyChanged("selectedPaymentDataStartColValue");
            }
        }

        private string _selectedPaymentDataStartRowValue;
        public string selectedPaymentDataStartRowValue
        {
            get
            {
                return _selectedPaymentDataStartRowValue;
            }
            set
            {
                _selectedPaymentDataStartRowValue = value;
                OnPropertyChanged("selectedPaymentDataStartRowValue");
            }
        }

        private string _selectedPaymentDataEndColValue;
        public string selectedPaymentDataEndColValue
        {
            get
            {
                return _selectedPaymentDataEndColValue;
            }
            set
            {
                _selectedPaymentDataEndColValue = value;
                OnPropertyChanged("selectedPaymentDataEndColValue");
            }
        }

        private string _selectedPaymentDataEndRowValue;
        public string selectedPaymentDataEndRowValue
        {
            get
            {
                return _selectedPaymentDataEndRowValue;
            }
            set
            {
                _selectedPaymentDataEndRowValue = value;
                OnPropertyChanged("selectedPaymentDataEndRowValue");
            }
        }

        private string _strHeaderSearch;
        public string strHeaderSearch
        {
            get
            {
                return _strHeaderSearch;
            }
            set
            {
                _strHeaderSearch = value;
                OnPropertyChanged("strHeaderSearch");
            }
        }

        private string _selectedHeaderRowsValue;
        public string selectedHeaderRowsValue
        {
            get
            {
                return _selectedHeaderRowsValue;
            }
            set
            {
                _selectedHeaderRowsValue = value;
                OnPropertyChanged("selectedHeaderRowsValue");
            }
        }

        private string _selectedHeaderColsValue;
        public string selectedHeaderColsValue
        {
            get
            {
                return _selectedHeaderColsValue;
            }
            set
            {
                _selectedHeaderColsValue = value;
                OnPropertyChanged("selectedHeaderColsValue");
            }
        }

        private bool _isPartOfPrimaryKeyYes;
        public bool isPartOfPrimaryKeyYes
        {
            get
            {
                return _isPartOfPrimaryKeyYes;
            }
            set
            {
                _isPartOfPrimaryKeyYes = value;
                OnPropertyChanged("isPartOfPrimaryKeyYes");
            }
        }

        private bool _isPartOfPrimaryKeyNo;
        public bool isPartOfPrimaryKeyNo
        {
            get
            {
                return _isPartOfPrimaryKeyNo;
            }
            set
            {
                _isPartOfPrimaryKeyNo = value;
                OnPropertyChanged("isPartOfPrimaryKeyNo");
            }
        }

        private bool _isCalculatedFieldsYes;
        public bool isCalculatedFieldsYes
        {
            get
            {
                return _isCalculatedFieldsYes;
            }
            set
            {
                _isCalculatedFieldsYes = value;
                OnPropertyChanged("isCalculatedFieldsYes");
            }
        }

        private bool _isCalculatedFieldsNo;
        public bool isCalculatedFieldsNo
        {
            get
            {
                return _isCalculatedFieldsNo;
            }
            set
            {
                _isCalculatedFieldsNo = value;
                OnPropertyChanged("isCalculatedFieldsNo");
            }
        }

        private bool _isEnableBlankFields;
        public bool isEnableBlankFields
        {
            get
            {
                return _isEnableBlankFields;
            }
            set
            {
                _isEnableBlankFields = value;
                OnPropertyChanged("isEnableBlankFields");
            }
        }

        private void LoadMaskType()
        {
            try
            {
                tempListMaskFieldTypes = ListMaskFieldTypes = serviceClients.PayorTemplateClient.AllMaskType();
                SelectedMaskFieldsTypes = ListMaskFieldTypes.FirstOrDefault();
            }
            catch
            {
            }
        }

        private void LoadTranslator()
        {
            try
            {
                tempListTranslatorsTypes = ListTranslatorTypes = serviceClients.PayorTemplateClient.AllTranslatorType();
                //SelectedTranslatorsTypes = ListTranslatorTypes.FirstOrDefault();
            }
            catch
            {
            }
        }

        private void FilterMaskType()
        {
            if (ListMaskFieldTypes == null || (ListMaskFieldTypes != null && ListMaskFieldTypes.Count == 0))
                return;

            AvailableMaskFieldType objAvailableMaskFieldType = new AvailableMaskFieldType();
            int intType = objAvailableMaskFieldType.GetAvailbleFieldType(SelectedFieldsList.FieldName);

            if (intType > 0)
            {
                tempListMaskFieldTypes = new ObservableCollection<MaskFieldTypes>(ListMaskFieldTypes.Where(p => p.Type == intType));
            }
            else
            {
                tempListMaskFieldTypes = new ObservableCollection<MaskFieldTypes>(ListMaskFieldTypes);
            }

            SelectedMaskFieldsTypes = tempListMaskFieldTypes.FirstOrDefault();
        }

        #endregion

        private List<ImportToolPayorPhrase> _objImportToolPhrase;
        public List<ImportToolPayorPhrase> objImportToolPhrase
        {
            get
            {
                return _objImportToolPhrase;
            }
            set
            {
                _objImportToolPhrase = value;
                OnPropertyChanged("objImportToolPhrase");
            }
        }


        private List<ImportToolPayorPhrase> _tempobjImportToolPhrase;
        public List<ImportToolPayorPhrase> tempobjImportToolPhrase
        {
            get
            {
                return _tempobjImportToolPhrase;
            }
            set
            {
                _tempobjImportToolPhrase = value;
                OnPropertyChanged("tempobjImportToolPhrase");
            }
        }

        private ObservableCollection<BatchFiles> _displayedBatchFiles;
        public ObservableCollection<BatchFiles> DisplayedBatchFiles
        {
            get
            {
                return _displayedBatchFiles;
            }
            set
            {
                _displayedBatchFiles = value;
                OnPropertyChanged("DisplayedBatchFiles");
            }

        }

        private ObservableCollection<ExportCardPayeeInfo> _exportCardPayeeInfo;
        public ObservableCollection<ExportCardPayeeInfo> ExportCardPayeeInfo
        {
            get
            {
                return _exportCardPayeeInfo;
            }
            set
            {
                _exportCardPayeeInfo = value;
                OnPropertyChanged("ExportCardPayeeInfo");
            }

        }

        private void OnBatchFilesChanged(object param)
        {
            string batchFileType = param as string;
            RadioSelection = batchFileType;

            switch (batchFileType)
            {
                case "Import":
                    if (_date1 == null && _date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Imported" select li);
                    else if (_date1 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Imported" && li.CreatedOn <= _date2 select li);
                    else if (_date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Imported" && li.CreatedOn >= _date1 select li);
                    else
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Imported" && li.CreatedOn >= _date1 && li.CreatedOn <= _date2 select li);
                    break;
                case "Export":
                    if (_date1 == null && _date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Exported" select li);
                    else if (_date1 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Exported" && li.CreatedOn <= _date2 select li);
                    else if (_date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Exported" && li.CreatedOn >= _date1 select li);
                    else
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.FileType == "Exported" && li.CreatedOn >= _date1 && li.CreatedOn <= _date2 select li);
                    break;
                case "All":
                default:
                    if (_date1 == null && _date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles select li);
                    else if (_date1 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.CreatedOn <= _date2 select li);
                    else if (_date2 == null)
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.CreatedOn >= _date1 select li);
                    else
                        DisplayedBatchFiles = new ObservableCollection<BatchFiles>(from li in BatchFiles where li.CreatedOn >= _date1 && li.CreatedOn <= _date2 select li);
                    break;
            }
        }

        private ICommand _selectBatchFiles;
        public ICommand SelectBatchFiles
        {
            get
            {
                if (_selectBatchFiles == null)
                {

                    _selectBatchFiles = new BaseCommand(param => OnBatchFilesChanged(param));

                }
                return _selectBatchFiles;
            }
        }

        private DateTime? _date1;
        public DateTime? Date1
        {
            get { return _date1; }
            set
            {
                _date1 = value;
                OnBatchFilesChanged(RadioSelection);
            }
        }


        private DateTime? _date2;
        public DateTime? Date2
        {
            get { return _date2; }
            set
            {
                _date2 = value;
                OnBatchFilesChanged(RadioSelection);
            }
        }

        private DateTime? _journalDate1;
        public DateTime? JournalDate1
        {
            get { return _journalDate1; }
            set
            {
                _journalDate1 = value;
                FilterInvoiceJournalLines(SelectedLicensee.LicenseeId);
            }
        }

        private DateTime? _journalDate2;
        public DateTime? JournalDate2
        {
            get { return _journalDate2; }
            set
            {
                _journalDate2 = value;
                FilterInvoiceJournalLines(SelectedLicensee.LicenseeId);
            }
        }


        private ICommand _viewBatchFile;
        public ICommand ViewBatchFile
        {
            get
            {
                if (_viewBatchFile == null)
                {

                    _viewBatchFile = new BaseCommand(param => OnViewFileClicked(param));

                }
                return _viewBatchFile;
            }
        }

        private void OnViewFileClicked(object param)
        {
            if (param != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    string KeyValue = string.Empty;
                    if (SharedVMData.MasterSystemConstants.ContainsKey("WebDevPath"))
                    {
                        KeyValue = SharedVMData.MasterSystemConstants["WebDevPath"];
                    }
                    else
                    {
                        KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                    }


                    WebDevPath ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);

                    BatchFiles bathcFileDetail = param as BatchFiles;
                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                    FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);

                    ObjDownload.DownloadComplete += (obj1, obj2) =>
                    {
                        autoResetEvent.Set();
                    };

                    if (bathcFileDetail != null && !string.IsNullOrEmpty(bathcFileDetail.FileName))
                    {
                        string localPath = Path.GetTempPath();
                        string fileName = bathcFileDetail.FileName;

                        ObjDownload.Download(fileName, localPath + fileName);

                        if (File.Exists(localPath + fileName))
                            System.Diagnostics.Process.Start(localPath + fileName);
                        else
                            MessageBox.Show("File named " + fileName + " is not found on server.", "File not found", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private ICommand _deleteBatchFile;
        public ICommand DeleteBatchFile
        {
            get
            {
                if (_deleteBatchFile == null)
                {
                    _deleteBatchFile = new BaseCommand(param => OnDeleteBatchFileClicked(param));
                }
                return _deleteBatchFile;
            }
        }

        private void OnDeleteBatchFileClicked(object param)
        {
            if (objLog == null) objLog = new MastersClient();
            if (MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    //BatchFiles bathcFileDetail = param as BatchFiles;
                    //string fileType = bathcFileDetail.FileType;

                    BatchFiles bathcFileDetail = param as BatchFiles;
                    //if bathcFileDetail is not equal to null

                    if (bathcFileDetail == null) return;

                    string fileType = bathcFileDetail.FileType;

                    if (serviceClients.BatchFilesClient.DeleteBatchFile(bathcFileDetail))
                    {
                        objLog.AddLog("DeleteBatchFile request: " + bathcFileDetail.FileName + ", User: " + RoleManager.userCredentialID);
                        BatchFiles.Remove(bathcFileDetail);
                        OnBatchFilesChanged(RadioSelection);
                        UpdateLicenseeBalance(serviceClients);

                        LicenseeInvoices = serviceClients.LicenseeInvoiceClient.getAllInvoice();

                        if (SelectedLicensee.LicenseeId != null)
                        {
                            FilterInvoiceLines(SelectedLicensee.LicenseeId);
                        }

                        InvoiceJournalLines = serviceClients.InvoiceLineClient.getInvoiceLinesForJournal(SelectedLicensee.LicenseeId);
                        FilterInvoiceJournalLines(SelectedLicensee.LicenseeId);
                        objLog.AddLog("DeleteBatchFile success: " + bathcFileDetail.FileName);
                    }
                }
                catch (Exception ex)
                {
                    objLog.AddLog("DeleteBatchFile exception: " + ex.Message);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        public DateTime? getCardPayeeExportDate()
        {

            ExportDate expDate = serviceClients.exportDateClient.GetExportDate();
            return expDate.CardPayeeExportDate;

        }

        public DateTime? getCheckPayeeExportDate()
        {

            ExportDate expDate = serviceClients.exportDateClient.GetExportDate();
            return expDate.CheckPayeeExportDate;

        }

        private ICommand _exportCardPayee;
        public ICommand ExportCardPayee
        {
            get
            {
                if (_exportCardPayee == null)
                {
                    _exportCardPayee = new BaseCommand(param => OnExportCardPayee(param));
                }
                return _exportCardPayee;
            }
        }

        private void OnExportCardPayee(object param)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string expDate = param as string;
                LicenseeVariableInputDetail inputInfo = new LicenseeVariableInputDetail();
                inputInfo.BatchFileInfo = ExportBatchFile.ExportCardPayee;
                inputInfo.Licensees = null;
                inputInfo.selectedInvoiceMonth = expDate;
                serviceClients.CalcVariableClient.StartCalculationAsync(inputInfo);
                ExpCardPayeeBtnStatus = false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void CalcVariableClient_StartCalculationCompleted(object sender, StartCalculationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (output == null)
                    output = e.Result;

                if (e.Result.BatchFileType == ExportBatchFile.Refresh)
                {
                    if (output != e.Result)
                    {
                        if (SelectedLicensee != null && SelectedLicensee.LicenseeId != Guid.Empty && output.LicenseesValueDictionary.ContainsKey(SelectedLicensee.LicenseeId))
                        {
                            if (e.Result.LicenseesValueDictionary.ContainsKey(SelectedLicensee.LicenseeId))
                            {
                                VariableCollection variable = e.Result.LicenseesValueDictionary[SelectedLicensee.LicenseeId];
                                output.LicenseesValueDictionary[SelectedLicensee.LicenseeId] = variable;
                            }
                        }
                        else
                        {
                            if (e.Result.LicenseesValueDictionary.ContainsKey(SelectedLicensee.LicenseeId))
                            {
                                VariableCollection variable = e.Result.LicenseesValueDictionary[SelectedLicensee.LicenseeId];
                                output.LicenseesValueDictionary.Add(SelectedLicensee.LicenseeId, variable);
                            }
                        }
                    }
                    FilterVariableLine(SelectedLicensee.LicenseeId);
                }
                else
                {
                    RefreshAfterExport();
                }

                if (e.Result.BatchFileType != ExportBatchFile.Refresh)
                {
                    ExpCardPayeeBtnStatus = true;
                }
                if (e.Result.BatchFileType == ExportBatchFile.ExportCheckPayee)
                {
                    ExpCheckPayeeBtnStatus = true;
                }
            }
        }

        private ICommand _exportCheckPayee;
        public ICommand ExportCheckPayee
        {
            get
            {
                if (_exportCheckPayee == null)
                {
                    _exportCheckPayee = new BaseCommand(param => OnExportCheckPayee(param));
                }
                return _exportCheckPayee;
            }
        }

        private void OnExportCheckPayee(object param)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string expDate = param as string;
                LicenseeVariableInputDetail inputInfo = new LicenseeVariableInputDetail();
                inputInfo.BatchFileInfo = ExportBatchFile.ExportCheckPayee;
                inputInfo.Licensees = null;
                inputInfo.selectedInvoiceMonth = expDate;
                serviceClients.CalcVariableClient.StartCalculationAsync(inputInfo);
                ExpCheckPayeeBtnStatus = false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void RefreshAfterExport()
        {
            UpdateLicenseeBalance(serviceClients);
            BatchFiles = serviceClients.BatchFilesClient.fillBatchFilesData();
            LicenseeInvoices = serviceClients.LicenseeInvoiceClient.getAllInvoice();
            InvoiceJournalLines = serviceClients.InvoiceLineClient.getInvoiceLinesForJournal(SelectedLicensee.LicenseeId);
            FilterInvoiceLines(SelectedLicensee.LicenseeId);
            FilterJournalLines(SelectedLicensee.LicenseeId);
            FilterInvoiceJournalLines(SelectedLicensee.LicenseeId);

        }

        private void UpdateLicenseeBalance(ServiceClients serviceClients)
        {
            List<LicenseeBalance> licenseeBalance = serviceClients.LicenseeClient.getLicenseesBalance().ToList();
            foreach (LicenseeBalance licBalance in licenseeBalance)
            {
                LicenseeDisplayData licensee = AllLicensees.FirstOrDefault(s => s.LicenseeId == licBalance.LicenseeId);
                if (licensee != null && licensee.LicenseeId != Guid.Empty)
                    licensee.DueBalance = licBalance.DueBalance;
            }
        }

        private void OnRefreshVariableList(object param)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string invoiceMonth = param as string;
                LicenseeVariableInputDetail inputInfo = new LicenseeVariableInputDetail();
                inputInfo.BatchFileInfo = ExportBatchFile.Refresh;
                inputInfo.Licensees = new ObservableCollection<Guid>();

                if (SharedVMData.SelectedLicensee != null)
                {
                    if (SharedVMData.SelectedLicensee.LicenseeId != null)
                    {
                        inputInfo.Licensees.Add(SharedVMData.SelectedLicensee.LicenseeId);

                    }
                }

                serviceClients.CalcVariableClient.StartCalculationAsync(inputInfo);

            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private ICommand _importCardPayee;
        public ICommand ImportCardPayee
        {
            get
            {
                if (_importCardPayee == null)
                {
                    _importCardPayee = new BaseCommand(param => OnImportCardPayee(param));
                }
                return _importCardPayee;
            }
        }

        private void OnImportCardPayee(object param)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult dialogResult;

            dialog.Title = "Select Transaction File";
            dialog.Filter = "Text Files|*.txt|All Files|*.*";

            dialogResult = dialog.ShowDialog();

            bool invalidFormat = false;
            List<string> records = null;

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                //Validate the file
                records = File.ReadAllLines(dialog.FileName).ToList();

                if (records != null && records.Count != 0)
                {
                    foreach (string record in records)
                    {
                        int tabCount = record.Count(p => p == '\t');

                        if (tabCount != 38)
                        {
                            invalidFormat = true;
                            break;
                        }
                    }
                }
                else
                {
                    invalidFormat = true;
                }
            }

            if (invalidFormat)
            {
                MessageBox.Show("File format is incorrent!", "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //Send file to server for saving information...
                //change by vinod
                //check null
                if (!string.IsNullOrEmpty(dialog.SafeFileName.Trim()))
                    serviceClients.ImportTSFileClient.fillImportInfo(dialog.SafeFileName, new ObservableCollection<string>(records));
            }

        }
        private ICommand _viewInvoiceDetail;
        public ICommand ViewInvoiceDetail
        {
            get
            {
                if (_viewInvoiceDetail == null)
                {
                    _viewInvoiceDetail = new BaseCommand(param => OnViewInvoiceDetail(param));
                }
                return _viewInvoiceDetail;
            }
        }

        private void OnViewInvoiceDetail(object param)
        {
            LicenseeInvoice invoice = SelectedLicenseeInvoice;

        }

        private ObservableCollection<ServiceProduct> _products;
        public ObservableCollection<ServiceProduct> Products
        {
            get
            {
                if (_products != null)
                    return _products;
                return null;
            }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private ServiceProduct _selectedProduct;
        public ServiceProduct SelectedProduct
        {
            get
            {
                return _selectedProduct == null ? _selectedProduct = new ServiceProduct() : _selectedProduct;
            }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged("SelectedProduct");

            }
        }

        private ObservableCollection<ServiceChargeType> _chargeTypes;
        public ObservableCollection<ServiceChargeType> ServiceChargeTypes
        {
            get
            {
                if (_chargeTypes != null)
                    return _chargeTypes;
                return null;
            }
            set
            {
                _chargeTypes = value;
                OnPropertyChanged("ServiceChargeTypes");
            }
        }

        private ServiceChargeType _selectedChargeType;
        public ServiceChargeType SelectedChargeType
        {
            get
            {
                return _selectedChargeType == null ? _selectedChargeType = new ServiceChargeType() : _selectedChargeType;
            }
            set
            {
                _selectedChargeType = value;
                OnPropertyChanged("SelectedChargeType");

            }
        }

        private ObservableCollection<BillingLineDetail> _serviceLines;
        public ObservableCollection<BillingLineDetail> ServiceLines
        {
            get
            {
                if (_serviceLines != null)
                {
                    _serviceLine = _serviceLines.Count == 0 ? new BillingLineDetail() : _serviceLines.FirstOrDefault();
                    return _serviceLines;
                }
                return null;
            }
            set
            {
                _serviceLines = value;
                OnPropertyChanged("ServiceLines");
            }
        }

        public ICollectionView ServLines
        {
            get { return CollectionViewSource.GetDefaultView(ServiceLines); }
        }

        private void FilterServiceLines()
        {
            ICollectionView view = ServLines;
            view.Filter = delegate (object item)
            {
                if (SelectedLicensee == null || SelectedLicensee.LicenseeId == Guid.Empty)
                    return false;
                else
                    return ((BillingLineDetail)item).LicenseeID == SelectedLicensee.LicenseeId;
            };
        }

        private BillingLineDetail _serviceLine;
        public BillingLineDetail ServiceLine
        {
            get
            {
                return _serviceLine == null ? _serviceLine = new BillingLineDetail() : _serviceLine;
            }
            set
            {
                _serviceLine = value;
                OnPropertyChanged("ServiceLine");

            }
        }

        private ICommand _deleteServiceLine;
        public ICommand DeleteServiceLine
        {
            get
            {
                if (_deleteServiceLine == null)
                {
                    _deleteServiceLine = new BaseCommand(param => BeforeOnDeleteServiceLine(param), param => OnDeleteServiceLine(param));
                }
                return _deleteServiceLine;
            }
        }

        private bool BeforeOnDeleteServiceLine(object param)
        {
            if (SelectedLicensee != null)
                return true;
            else
                return false;
        }

        private void OnDeleteServiceLine(object param)
        {
            if (MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Guid LicenseeServiceID = ServiceLine.LicenseeServiceID;
                BillingLineDetail detail = ServiceLines.Where(p => p.LicenseeServiceID == LicenseeServiceID).FirstOrDefault();

                if (detail.ServiceName == "Follow Up")
                {
                    SharedVMData.MasterFollowupIssueList = new ObservableCollection<DisplayFollowupIssue>();
                    SharedVMData.ReadonlyMasterFollowUpList = new ObservableCollection<DisplayFollowupIssue>();
                }

                ServiceLines.Remove(detail);
                ServiceLine = ServiceLines.FirstOrDefault(p => p.LicenseeID == SelectedLicensee.LicenseeId);
            }
        }

        private ICommand _addServiceLine;
        public ICommand AddServiceLine
        {
            get
            {
                if (_addServiceLine == null)
                {
                    _addServiceLine = new BaseCommand(param => BeforeOnAddServiceLine(param), param => OnAddServiceLine(param));
                }
                return _addServiceLine;
            }
        }

        private bool BeforeOnAddServiceLine(object param)
        {
            if (SelectedLicensee != null)
                return true;
            else
                return false;
        }

        private void OnAddServiceLine(object param)
        {
            //Add the service line here...
            if (SelectedLicensee != null && SelectedLicensee.LicenseeId != Guid.Empty)
            {
                BillingLineDetail serviceLine = new BillingLineDetail { LicenseeID = SelectedLicensee.LicenseeId };
                serviceLine.ServiceChargeType = ServiceChargeTypes.FirstOrDefault();
                serviceLine.Service = Products.FirstOrDefault();
                serviceLine.ServiceName = (serviceLine.Service != null ? serviceLine.Service.ServiceName : string.Empty);
                ServiceLines.Add(serviceLine);
            }
        }

        private ICommand _refreshVariableList;
        public ICommand RefreshVariableList
        {
            get
            {
                if (_refreshVariableList == null)
                {
                    _refreshVariableList = new BaseCommand(param => OnRefreshVariableList(param));
                }
                return _refreshVariableList;
            }
        }

        #region Invoices

        private void FilterJournalLines(Guid LicenseeId)
        {
            ICollectionView view = JournalView;
            view.Filter = delegate (object item)
            {
                return (((LicenseeInvoiceJournal)item).LicenseeID == LicenseeId);
            };
        }

        public ICollectionView JournalView
        {
            get { return CollectionViewSource.GetDefaultView(_journals); }
        }

        private void FilterInvoiceLines(Guid LicenseeId)
        {
            ICollectionView view = LicenseeInvoicesView;
            view.Filter = delegate (object item)
            {
                return (((LicenseeInvoice)item).LicenseeId.Value == LicenseeId) && ((_startInvoiceDate != null) ? (_startInvoiceDate <= ((LicenseeInvoice)item).BillingDate) : true) && ((_endInvoiceDate != null) ? (((LicenseeInvoice)item).BillingDate <= _endInvoiceDate) : true);
            };
        }

        public ICollectionView LicenseeInvoicesView
        {
            get { return CollectionViewSource.GetDefaultView(LicenseeInvoices); }
        }

        private ObservableCollection<LicenseeInvoice> _icenseeInvoices;
        public ObservableCollection<LicenseeInvoice> LicenseeInvoices
        {
            get
            {
                if (_icenseeInvoices != null)
                {
                    _selectedLicenseeInvoice = _icenseeInvoices.Count == 0 ? new LicenseeInvoice() : _icenseeInvoices.FirstOrDefault();
                    return _icenseeInvoices;
                }
                return null;
            }
            set
            {
                _icenseeInvoices = value;
                OnPropertyChanged("LicenseeInvoices");
            }
        }

        private ObservableCollection<LicenseeInvoiceJournal> _journals;
        public ObservableCollection<LicenseeInvoiceJournal> Journals
        {
            get
            {
                if (_journals != null)
                {
                    _selectedJournal = _journals.Count == 0 ? new LicenseeInvoiceJournal() : _journals.FirstOrDefault();
                    return _journals;
                }
                return null;
            }
            set
            {
                _journals = value;
                OnPropertyChanged("Journals");
            }
        }

        private ObservableCollection<InvoiceLineJournalData> _invoiceJournalLines;
        public ObservableCollection<InvoiceLineJournalData> InvoiceJournalLines
        {
            get
            {
                return _invoiceJournalLines;
            }
            set
            {
                _invoiceJournalLines = value;
                OnPropertyChanged("InvoiceJournalLines");
            }
        }

        private ObservableCollection<InvoiceLineJournalData> _displayedInvoiceJournalLines;
        public ObservableCollection<InvoiceLineJournalData> DisplayedInvoiceJournalLines
        {
            get
            {
                return _displayedInvoiceJournalLines;
            }
            set
            {
                _displayedInvoiceJournalLines = value;
                OnPropertyChanged("DisplayedInvoiceJournalLines");
            }
        }

        private void FilterInvoiceJournalLines(Guid LicenseeId)
        {
            DisplayedInvoiceJournalLines = new ObservableCollection<InvoiceLineJournalData>(InvoiceJournalLines.Where(s => ((_journalDate1 != null) ? (_journalDate1 <= s.BillingDate) : true) && ((_journalDate2 != null) ? (s.BillingDate <= _journalDate2) : true)).ToList());
        }

        private LicenseeInvoice _selectedLicenseeInvoice;
        public LicenseeInvoice SelectedLicenseeInvoice
        {
            get
            {
                return _selectedLicenseeInvoice == null ? _selectedLicenseeInvoice = new LicenseeInvoice() : _selectedLicenseeInvoice;
            }
            set
            {
                _selectedLicenseeInvoice = value;
                OnPropertyChanged("SelectedLicenseeInvoice");

            }
        }

        private LicenseeInvoiceJournal _selectedJournal;
        public LicenseeInvoiceJournal SelectedJournal
        {
            get
            {
                return _selectedJournal == null ? _selectedJournal = new LicenseeInvoiceJournal() : _selectedJournal;
            }
            set
            {
                _selectedJournal = value;
                OnPropertyChanged("SelectedJournal");

            }
        }

        private DateTime? _startInvoiceDate;
        public DateTime? StartInvoiceDate
        {
            get { return _startInvoiceDate; }
            set
            {
                _startInvoiceDate = value;
                FilterInvoiceLines(SelectedLicensee.LicenseeId);
            }
        }


        private DateTime? _endInvoiceDate;
        public DateTime? EndInvoiceDate
        {
            get { return _endInvoiceDate; }
            set
            {
                _endInvoiceDate = value;
                FilterInvoiceLines(SelectedLicensee.LicenseeId);
            }
        }

        private InvoiceLineData _invoiceLine;
        public InvoiceLineData InvoiceLine
        {
            get { return _invoiceLine; }
            set
            {
                _invoiceLine = value;
            }
        }

        private ManualJournalData _JnlData;
        public ManualJournalData JnlData
        {
            get { return _JnlData; }
            set
            {
                _JnlData = value;
            }
        }

        public void setJournalDetail()
        {
            if (SelectedJournal != null && SelectedJournal.JournalId != 0)
                _JnlData = new ManualJournalData(SelectedJournal);
            else
                _JnlData = null;
        }

        public void setInvoiceDetail()
        {
            if (SelectedLicenseeInvoice != null && SelectedLicenseeInvoice.InvoiceId != 0)
                _invoiceLine = new InvoiceLineData(SelectedLicenseeInvoice);
            else
                _invoiceLine = null;
        }

        public void UpdateJournalEntry()
        {

            LicenseeInvoiceJournal jnl = Journals.First(s => s.JournalId == _JnlData.JournalId);
            SelectedLicensee.DueBalance -= (JnlData.Amount - jnl.JournalAmount);
            jnl.JournalId = _JnlData.JournalId;
            jnl.lnvoiceId = _JnlData.InvoiceId;
            jnl.LicenseeID = SelectedLicensee.LicenseeId;
            jnl.JournalAmount = JnlData.Amount;
            jnl.ReceivedDate = _JnlData.DateReceived;
            jnl.TransactionID = _JnlData.TransactionId;
            jnl.PaymentType = _JnlData.Paymentype;
            serviceClients.JournalClient.UpdateJournalEntry(jnl);

        }

        public void AddJournalEntry()
        {

            LicenseeInvoiceJournal jnl = new LicenseeInvoiceJournal();
            jnl.lnvoiceId = _JnlData.InvoiceId;
            jnl.LicenseeID = SelectedLicensee.LicenseeId;
            jnl.JournalAmount = JnlData.Amount;
            jnl.ReceivedDate = _JnlData.DateReceived;
            jnl.TransactionID = _JnlData.TransactionId;
            jnl.PaymentType = _JnlData.Paymentype;
            jnl.IsManualEntry = true;

            long IdValue = 0;
            if ((IdValue = serviceClients.JournalClient.InsertJournalEntry(jnl)) != 0)
            {
                jnl.JournalId = IdValue;
                Journals.Add(jnl);
                SelectedLicensee.DueBalance -= jnl.JournalAmount;
            }

        }
        private ICommand _deleteJournal;
        public ICommand DeleteJournal
        {
            get
            {
                if (_deleteJournal == null)
                {
                    _deleteJournal = new BaseCommand(param => DeleteJournalEntry(param));
                }
                return _deleteJournal;
            }
        }

        public void DeleteJournalEntry(object param)
        {
            if (objLog == null) objLog = new MastersClient();
            if (SelectedJournal != null && SelectedJournal.JournalId != 0)
            {
                objLog.AddLog("DeleteJournalEntry request: " + SelectedJournal.JournalId + ", USre: " + RoleManager.userCredentialID);
                LicenseeInvoiceJournal jnl = new LicenseeInvoiceJournal();
                jnl.JournalId = SelectedJournal.JournalId;
                if (serviceClients.JournalClient.DeleteJournalEntry(jnl))
                {
                    decimal? amount = SelectedJournal.JournalAmount;
                    Journals.Remove(Journals.Where(s => s.JournalId == jnl.JournalId).First());
                    SelectedLicensee.DueBalance += amount;
                }
            }

        }

        private List<string> _PaymentTypes;
        public List<string> PaymentTypes
        {
            get
            {
                return _PaymentTypes;
            }
            set
            {
                _PaymentTypes = value;
                OnPropertyChanged("PaymentTypes");
            }
        }

        #endregion
    }

    public class ServiceChargesType : List<ServiceChargeType>
    {
    }

    public class Products : List<ServiceProduct>
    {
    }

    public class ManualJournalData
    {
        public long? JournalId { get; set; }
        public long? InvoiceId { get; set; }
        public Decimal Amount { get; set; }
        public string Paymentype { get; set; }
        public DateTime DateReceived { get; set; }
        public string TransactionId { get; set; }
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
        public ManualJournalData()
        {
            DateReceived = new DateTime(2010, 1, 1);
        }

        public ManualJournalData(LicenseeInvoiceJournal jData)
        {
            try
            {
                JournalId = jData.JournalId;
                InvoiceId = jData.lnvoiceId;
                Amount = jData.JournalAmount.Value;
                Paymentype = jData.PaymentType;
                DateReceived = jData.ReceivedDate.Value;
                TransactionId = jData.TransactionID;
            }
            catch
            {
            }
        }
    }

    public class InvoiceLineData
    {
        public long InvoiceId { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string BatchName { get; set; }
        public string CompanyName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal TotalTax { get; set; }
        public List<InvoiceServiceData> Services { get; set; }
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
        public InvoiceLineData(LicenseeInvoice invoiceLine)
        {
            try
            {
                InvoiceLine incLine = serviceClients.InvoiceLineClient.getInvoiceLines(invoiceLine.InvoiceId, false);

                if (invoiceLine != null)
                {
                    InvoiceId = invoiceLine.InvoiceId;
                    BillingDate = invoiceLine.BillingDate.Value;
                    CreatedOn = invoiceLine.InvoiceGeneratedOn.Value;

                    LicenseeDisplayData lic = serviceClients.LicenseeClient.GetLicenseeByID(invoiceLine.LicenseeId ?? Guid.Empty);
                    CompanyName = lic.Company;

                    string batchName = serviceClients.LicenseeInvoiceClient.getExportBatchName(invoiceLine.ExportedBatchId);
                    BatchName = batchName;

                    Total = 0;
                    TotalTax = 0;
                    SubTotal = 0;
                    if (Services == null)
                        Services = new List<InvoiceServiceData>();

                    foreach (InvoiceLineServiceData serData in incLine.InvoiceServiceLineData)
                    {
                        Total += serData.Total;
                        SubTotal += serData.SubTotal;
                        TotalTax += serData.Tax;

                        InvoiceServiceData isData = new InvoiceServiceData(serData);
                        Services.Add(isData);
                    }
                }

            }
            catch
            {
            }
        }
    }

    public class InvoiceServiceData
    {
        public string ServiceName { get; set; }
        public string ServiceChargeType { get; set; }
        public int ConsumedUnit { get; set; }
        public double Rate { get; set; }
        public double Discount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
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
        public InvoiceServiceData(InvoiceLineServiceData serviceData)
        {
            try
            {
                ServiceName = serviceData.ServiceName;
                ServiceChargeType = serviceData.ServiceChargeType;
                ConsumedUnit = serviceData.ConsumedUnit;
                Rate = serviceData.Rate ?? 0.0;
                Discount = Convert.ToDouble(serviceData.Discount ?? (decimal)0.0);
                SubTotal = serviceData.SubTotal;
                Tax = serviceData.Tax;
                Total = serviceData.Total;
            }

            catch
            {
            }
        }
    }
}
