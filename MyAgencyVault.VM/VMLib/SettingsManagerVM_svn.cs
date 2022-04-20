using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.EmailFax;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM.ClientProxy;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VM.VMLib.Setting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MyAgencyVault.ViewModel
{
    enum PayorType
    {
        Local,
        Global,
        All
    }

    public class SettingsManagerVM : BaseViewModel, IDataRefresh
    {
        #region New code
        static MastersClient objLog = new MastersClient();

        private List<PayorIncomingSchedule> _scheduleList;
        public List<PayorIncomingSchedule> ScheduleList
        {
            get { return _scheduleList; }
            set
            {
                _scheduleList = value;
                OnPropertyChanged("ScheduleList");
            }
        }

        private List<PayorIncomingSchedule> _namedScheduleList;
        public List<PayorIncomingSchedule> NamedScheduleList
        {
            get { return _namedScheduleList; }
            set
            {
                _namedScheduleList = value;
                OnPropertyChanged("NamedScheduleList");
            }
        }


        private PayorIncomingSchedule _selectedSchedule;
        public PayorIncomingSchedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;


                OnPropertyChanged("SelectedSchedule");
            }
        }
        private PayorIncomingSchedule _selectedNamedSchedule;
        public PayorIncomingSchedule SelectedNamedSchedule
        {
            get { return _selectedNamedSchedule; }
            set
            {
                _selectedNamedSchedule = value;


                OnPropertyChanged("SelectedNamedSchedule");
            }
        }

        private PayorIncomingSchedule _savedSelectedSchedule;
        public PayorIncomingSchedule SavedSelectedSchedule
        {
            get { return _savedSelectedSchedule; }
            set
            {
                _savedSelectedSchedule = value;
                OnPropertyChanged("SavedSelectedSchedule");
            }
        }

        private PayorIncomingSchedule _savedNamedSelectedSchedule;
        public PayorIncomingSchedule SavedNamedSelectedSchedule
        {
            get { return _savedNamedSelectedSchedule; }
            set
            {
                _savedNamedSelectedSchedule = value;
                OnPropertyChanged("SavedNamedSelectedSchedule");
            }
        }
        private double? _SplitPercentage;
        public double? SplitPercentage
        {
            get { return _SplitPercentage; }
            set
            {
                _SplitPercentage = value;


                OnPropertyChanged("SplitPercentage");
            }
        }
        private string _StringFirstYearPercentage;
        public string StringFirstYearPercentage
        {
            get { return _StringFirstYearPercentage; }
            set
            {
                _StringFirstYearPercentage = value;


                OnPropertyChanged("SplitPercentage");
            }
        }

        private bool _IncPercentOfPremium;
        public bool IncPercentOfPremium
        {
            get { return _IncPercentOfPremium; }
            set
            {
                _IncPercentOfPremium = value;


                OnPropertyChanged("IncPercentOfPremium");
            }
        }
        private bool _namedScheduleIncPercentOfPremium;
        public bool NamedScheduleIncPercentOfPremium
        {
            get { return _namedScheduleIncPercentOfPremium; }
            set
            {
                _namedScheduleIncPercentOfPremium = value;


                OnPropertyChanged("NamedScheduleIncPercentOfPremium");
            }
        }
        private bool _IncPerHead;
        public bool IncPerHead
        {
            get { return _IncPerHead; }
            set
            {
                _IncPerHead = value;


                OnPropertyChanged("IncPerHead");
            }
        }
        private bool _namedScheduleIncPerHead;
        public bool NamedScheduleIncPerHead
        {
            get { return _namedScheduleIncPerHead; }
            set
            {
                _namedScheduleIncPerHead = value;


                OnPropertyChanged("NamedScheduleIncPerHead");
            }
        }
        //Load screen
        /// <summary>
        /// set from test 
        /// </summary>
        private string _renewaltext;
        public string Renewaltext
        {
            get
            {
                return _renewaltext;
            }
            set
            {
                _renewaltext = value;
                OnPropertyChanged("Renewaltext");

            }
        }
        /// <summary>
        /// set commisition test 
        /// </summary>
        private string _firstYearText;
        public string FirstYearText
        {
            get
            {
                return _firstYearText;
            }
            set
            {
                _firstYearText = value;
                OnPropertyChanged("FirstYearText");

            }
        }
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private double? _firstYearPer;
        public double? FirstYearPer
        {
            get
            {
                return _firstYearPer;
            }
            set
            {
                _firstYearPer = value;
                OnPropertyChanged("FirstYearPer");

            }
        }
       
       
        
        private double? _namedScheduleFirstYearPer;
        public double? NamedScheduleFirstYearPer
        {
            get
            {
                return _namedScheduleFirstYearPer;
            }
            set
            {
                _namedScheduleFirstYearPer = value;
                OnPropertyChanged("NamedScheduleFirstYearPer");

            }
        }
        private int _advance;
        public int Advance
        {
            get
            {
                return _advance;
            }
            set
            {
                _advance = value;
                OnPropertyChanged("Advance");

            }
        }
        private int _namedScheduleAdvance;
        public int NamedScheduleAdvance
        {
            get
            {
                return _namedScheduleAdvance;
            }
            set
            {
                _namedScheduleAdvance = value;
                OnPropertyChanged("NamedScheduleAdvance");

            }
        }

        private double? _renewalPer;
        public double? RenewalPer
        {
            get
            {
                return _renewalPer;
            }
            set
            {
                _renewalPer = value;
                OnPropertyChanged("RenewalPer");

            }
        }
        private double? _namedScheduleRenewalPer;
        public double? NamedScheduleRenewalPer
        {
            get
            {
                return _namedScheduleRenewalPer;
            }
            set
            {
                _namedScheduleRenewalPer = value;
                OnPropertyChanged("NamedScheduleRenewalPer");

            }
        }

        private double? _splitPer;
        public double? SplitPer
        {
            get
            {
                return _splitPer;
            }
            set
            {
                _splitPer = value;
                OnPropertyChanged("SplitPer");

            }
        }
        private double? _namedScheduleSplitPer;
        public double? NamedScheduleSplitPer
        {
            get
            {
                return _namedScheduleSplitPer;
            }
            set
            {
                _namedScheduleSplitPer = value;
                OnPropertyChanged("NamedScheduleSplitPer");

            }
        }

        private PolicyIncomingPaymentType selectedMasterIncomingPaymentType;
        public PolicyIncomingPaymentType SelectedMasterIncomingPaymentType
        {
            get { return selectedMasterIncomingPaymentType == null ? new PolicyIncomingPaymentType() : selectedMasterIncomingPaymentType; }
            set { selectedMasterIncomingPaymentType = value; OnPropertyChanged("SelectedMasterIncomingPaymentType"); }
        }


        #endregion


        #region Old code
        #region VMInstances

        private SettingCarrierCoveragesVM _CarrierCoveragesVM;
        public SettingCarrierCoveragesVM CarrierCoveragesVM
        {
            get
            {
                return _CarrierCoveragesVM;
            }
            set
            {
                _CarrierCoveragesVM = value;
                OnPropertyChanged("CarrierCoveragesVM");
            }
        }

        private SettingPayorCarriersVM _PayorCarriersVM;
        public SettingPayorCarriersVM PayorCarriersVM
        {
            get
            {
                return _PayorCarriersVM;
            }
            set
            {
                _PayorCarriersVM = value;
                OnPropertyChanged("PayorCarriersVM");
            }
        }

        private SettingSharedDataVM _SettingSharedDataVM;
        public SettingSharedDataVM SettingSharedDataVM
        {
            get
            {
                return _SettingSharedDataVM;
            }
            set
            {
                _SettingSharedDataVM = value;
                OnPropertyChanged("SettingSharedDataVM");
            }
        }


        private ObservableCollection<PolicyIncomingPaymentType> masterIncomingPaymentTypeLst;
        public ObservableCollection<PolicyIncomingPaymentType> MasterIncomingPaymentTypeLst
        {
            get
            {
                return masterIncomingPaymentTypeLst;
            }
            set
            {
                masterIncomingPaymentTypeLst = value;
                OnPropertyChanged("MasterIncomingPaymentTypeLst");
            }

        }

        #endregion

        #region Private Fields

        private PayorType _payorType;
        private ObservableCollection<Region> _regions = null;
        private ObservableCollection<Region> _objRegion = new ObservableCollection<Region>();
        private Region _selectedRegion;
        private Region _selectedUserRegion;


        //By Acme- for report settings on this page.
        static Guid PSReportID = new Guid("1D38ACD0-BE49-4CD1-820C-B118EAE882DC");
        string selectedProductType;
        DisplayedCoverage selectedCoverage;
        DisplayedPayor selectedPayor;
        Carrier selectedCarrier;

        private Coverage _selectedProduct = new Coverage();

        private ICommand _savePayor = null;
        private ICommand _newSchedule = null;
        private ICommand _newNamedSchedule = null;
        private ICommand _deletePayor = null;
        private ICommand _deleteSchedule = null;
        private ICommand _deleteNamedSchedule = null;

        static bool isNewClicked = false;
        private bool isDeleteDisabled = false;
        static bool IsNamedSchedule = false;
        private ObservableCollection<PayorSiteLoginInfo> displayedLogins;
        public ObservableCollection<PayorSiteLoginInfo> DisplayedLogins
        {
            get { return displayedLogins; }
            set
            {
                displayedLogins = value;
                OnPropertyChanged("DisplayedLogins");
            }
        }

        private bool _IsSingleCarrierEnabled = true;
        public bool IsSingleCarrierEnabled
        {
            get { return _IsSingleCarrierEnabled; }
            set { _IsSingleCarrierEnabled = value; OnPropertyChanged("IsSingleCarrierEnabled"); }
        }

        int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; OnPropertyChanged("SelectedTabIndex"); }
        }

        string errorTooltip;
        public string ErrorTooltip
        {
            get
            {
                return errorTooltip;

            }
            set
            {
                errorTooltip = value;
                OnPropertyChanged("ErrorTooltip");
            }
        }
        string errorTooltip1;
        public string ErrorTooltip1
        {
            get
            {
                return errorTooltip1;

            }
            set
            {
                errorTooltip1 = value;
                OnPropertyChanged("ErrorTooltip1");
            }
        }
        string errorTooltipForReport;
        public string ErrorTooltipForReport
        {
            get
            {
                return errorTooltipForReport;

            }
            set
            {
                errorTooltipForReport = value;
                OnPropertyChanged("ErrorTooltipForReport");
            }
        }
        
        private string _imagePath;
        public string imagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;
                OnPropertyChanged("imagePath");
            }
        }

        #endregion

        #region Delegate & Events

        public delegate void SelectedPayorChangedEventHandler(Payor SelectedPayor);
        public event SelectedPayorChangedEventHandler SelectedPayorChanged;

        public delegate void SaveScheduleEventHandler();
        public event SaveScheduleEventHandler SaveScheduleEvent;

        #endregion

        #region Constructor

        private SettingServiceClient _SettingServiceClient;

        public SettingsManagerVM()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsManagerVM_PropertyChanged);
            _SettingServiceClient = new SettingServiceClient(this);
            _payorType = PayorType.All;
            SettingScreenControl();
            LoadReportSettings();
            PayorCarriersVM = new SettingPayorCarriersVM(this, AllowEdit);
            CarrierCoveragesVM = new SettingCarrierCoveragesVM(this, AllowEdit);
            SettingSharedDataVM = new SettingSharedDataVM(this, PayorCarriersVM);
            PayorCarriersVM.SelectedCarrierChanged += PayorCarriersVM_SelectedCarrierChanged;
            CarrierCoveragesVM.SelectedProductTypeChanged += CarrierCoveragesVM_SelectedProductTypeChanged;
            CarrierCoveragesVM.SelectedCoverageChanged += CarrierCoveragesVM_SelectedCoverageChanged;

            string strLocalPath = AppDomain.CurrentDomain.BaseDirectory;
            string strSavePath = @"\Images\Icons\floppy_disk_blue.png";

            imagePath = strLocalPath + strSavePath;
            //LoadRegions();
            //LoadData();
            //Hide web site panel
            //ShowWebSitePanel = "Hidden";

            //btnAddNew="New";
            //btnSaveUpdate = "Update";

        }

        private void CarrierCoveragesVM_SelectedCoverageChanged(DisplayedCoverage SelectedCoverage)
        {
            if (SelectedCoverage != null)
            {
                //                SelectedSchedule.CoverageID
                selectedCoverage = SelectedCoverage;
                selectedProductType = (CarrierCoveragesVM.CoveragesNickName.Count > 0) ? CarrierCoveragesVM.CoveragesNickName[0].NickName : "";
            }
        }

        private void CarrierCoveragesVM_SelectedProductTypeChanged(CoverageNickName SelectedProductType)
        {
            if (SelectedProductType != null)
            {
                selectedProductType = SelectedProductType.NickName;
            }
        }

        bool BeforeOnNewSchedule()
        {


            return true;
        }
        bool BeforeOnNewNamedSchedule()
        {
            return true;
        }
        void OnNewSchedule()
        {

            PayorIncomingSchedule newSchedule = new PayorIncomingSchedule();
            newSchedule.IncomingScheduleID = Guid.NewGuid();
            newSchedule.LicenseeID = SharedVMData.SelectedLicensee.LicenseeId;
            newSchedule.PayorID = SelectedPayor.PayorID;
            newSchedule.CarrierID = PayorCarriersVM.SelectedCarrier.CarrierId;
            newSchedule.CoverageID = (CarrierCoveragesVM.SelectedProduct != null) ? CarrierCoveragesVM.SelectedProduct.CoverageID : Guid.Empty;
            newSchedule.ProductType = (CarrierCoveragesVM.SelectedProductType != null) ? CarrierCoveragesVM.SelectedProductType.NickName : "";

            newSchedule.IncomingPaymentTypeID = 1;
            SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == 1).FirstOrDefault();
            newSchedule.ScheduleTypeId = 1;
            IncPercentOfPremium = true;
            newSchedule.FirstYearPercentage = newSchedule.RenewalPercentage = 0;
            newSchedule.SplitPercentage = 100;
            newSchedule.CreatedBy = RoleManager.userCredentialID;
            newSchedule.ModifiedBy = RoleManager.userCredentialID;
            SelectedSchedule = newSchedule;
            isNewClicked = true;
            IsNamedSchedule = false;

        }
        void OnNewNamedSchedule()
        {
            PayorIncomingSchedule newNamedSchedule = new PayorIncomingSchedule();
            newNamedSchedule.IncomingScheduleID = Guid.NewGuid();
            newNamedSchedule.LicenseeID = SharedVMData.SelectedLicensee.LicenseeId;
            newNamedSchedule.ScheduleTypeId = 1;
            NamedScheduleIncPercentOfPremium = true;
            newNamedSchedule.FirstYearPercentage = newNamedSchedule.RenewalPercentage = 0;
            newNamedSchedule.SplitPercentage = 100;
            newNamedSchedule.CreatedBy = RoleManager.userCredentialID;
            newNamedSchedule.ModifiedBy = RoleManager.userCredentialID;
            SelectedNamedSchedule = newNamedSchedule;
            isNewClicked = true;
            IsNamedSchedule = true;
        }
        private void PayorCarriersVM_SelectedCarrierChanged(Carrier SelectedCarrier)
        {
            selectedCarrier = SelectedCarrier;
            CarrierCoveragesVM.FillProductTypesOnCarrierChange(SelectedCarrier);
            selectedCoverage = CarrierCoveragesVM.SelectedAllProduct;
            //Set first entry as schedule product type
            //   if(SelectedSchedule!= null)
            {
                // SelectedSchedule.ProductType 
                selectedProductType = (CarrierCoveragesVM.CoveragesNickName.Count > 0) ? CarrierCoveragesVM.CoveragesNickName[0].NickName : "";
            }
        }

        void ReloadReportSettings()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                if (SharedVMData.SelectedLicensee != null)
                    ReportFields = serviceClients.SettingsClient.GetReportCustomFieldSettings(SharedVMData.SelectedLicensee.LicenseeId, PSReportID);
            }
        }

        public void LoadReportSettings()
        {

            using (ServiceClients serviceClients = new ServiceClients())
            {
                //if (SelectedTabIndex == 0)
                {
                    if (SharedVMData.SelectedLicensee != null)
                        ReportFields = serviceClients.SettingsClient.GetReportCustomFieldSettings(SharedVMData.SelectedLicensee.LicenseeId, PSReportID);
                    //}
                    //else
                    //{
                    PolicyDetailMasterData masterData = serviceClients.MasterClient.GetPolicyDetailMasterData();
                    MasterIncomingPaymentTypeLst = masterData.IncomingPaymentTypes;
                    SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == 1).FirstOrDefault();


                    //load schedules
                    ScheduleList = serviceClients.GlobalIncomingScheduleClient.GetAllSchedules(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                    NamedScheduleList = serviceClients.SettingsClient.GetNamedScheduleList(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                    IncPercentOfPremium = true;
                    SelectedSchedule = null;
                    NamedScheduleIncPercentOfPremium = true;
                    SelectedNamedSchedule = null;
                }
            }

        }

        public void LoadData()
        {
            Guid LicenseeId = Guid.Empty;
            Guid payorId = Guid.Empty;

            if (SharedVMData.SelectedLicensee != null)
            {
                LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                SelectedLicenseeChanged();
            }
        }

        bool changeCurrentPayor = true;
        bool selectionChangedPayor = false;

        void SettingsManagerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                switch (e.PropertyName)
                {
                    case "Advance":
                        break;
                    //case "IsSelectAll":
                    //    //IsSelectAll = !IsSelectAll;
                    //    OnSelectAll();
                    //    break;
                    ////case "SelectedField":
                    //    SelectedField.PropertyChanged += SelectedField_PropertyChanged;
                    //    break;
                    case "SelectedRegion":
                        _payorType = PayorType.All;
                        FilterDisplayedPayors(true);
                        break;

                    case "Payors":
                        FilterDisplayedPayors(true);
                        break;

                    case "SelectedLicense":
                        if (SelectedDisplayPayor != null && SelectedDisplayPayor.PayorID != Guid.Empty)
                        {
                            LoadPayors();
                            PayorCarriersVM.LoadCarrierLicenseeWise();
                            CarrierCoveragesVM.loadProductDataLicenseeWise();
                        }
                        break;
                    case "SelectedPayor":
                        if (SelectedPayor != null && SelectedPayor.PayorID != Guid.Empty)
                        {
                            selectionChangedPayor = true;
                            //SelectedPayor.Region = SelectedDisplayPayor.Region;

                            //EnableDisableWebsite();

                            //if (SharedVMData.SelectedLicensee != null && SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                            //{
                            //    PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = SelectedDisplayPayor.PayorID });
                            //    bool IsWebsite = _PayorSource.IsWebsite;
                            //    RTFNotecontent = _PayorSource.Notes;
                            //    if (IsWebsite)
                            //    {
                            //        SelectedPayor.SourceType = SelectedDisplayPayor.SourceType = 1;
                            //        ShowWebSitePanel = "Visible";
                            //    }
                            //    else
                            //    {
                            //        SelectedPayor.SourceType = SelectedDisplayPayor.SourceType = 0;
                            //        ShowWebSitePanel = "Hidden";
                            //    }
                            //}

                            //if (SelectedPayor.PayorTypeID == 1)
                            //    IsSingleCarrierEnabled = false;
                            //else
                            //    IsSingleCarrierEnabled = true;

                            //if (SelectedPayor.ISGlobal && RoleManager.Role != UserRole.SuperAdmin)
                            //    ReadOnly = true;
                            //else
                            //    ReadOnly = false;

                            //IsEnable = !ReadOnly;
                            //PayorRegionIndex = SelectedPayor.PayorRegionID;
                        }
                        else
                        {
                            ReadOnly = false;
                            IsEnable = true;
                        }

                        if (SelectedPayorChanged != null)
                            SelectedPayorChanged(SelectedPayor);

                        selectionChangedPayor = false;
                        ////Load Broker code when change the agency
                        //LoadBrokerCode();
                        break;

                    case "SelectedDisplayPayor":
                        if (SelectedDisplayPayor == null)
                            return;
                        //else
                        //{
                        //    if (SelectedDisplayPayor.IsGlobal)
                        //    {
                        //        RTFNotecontentReadOnly = true;
                        //        Showrtfnote = "Visible";
                        //    }
                        //    else
                        //    {
                        //        RTFNotecontentReadOnly = false;
                        //        Showrtfnote = "Collapsed";
                        //    }

                        if (SelectedDisplayPayor.PayorID != Guid.Empty)
                        {
                            SelectedPayor = serviceClients.PayorClient.GetPayorByID(SelectedDisplayPayor.PayorID);
                            if (SelectedSchedule != null)
                            {
                                SelectedSchedule.PayorID = SelectedPayor.PayorID;
                            }
                            //   DisplayedCoverages = serviceClients.CoverageClient.GetPayorCoverages(SelectedPayor.PayorID);

                        }
                        //else
                        //    SelectedPayor = SelectedDisplayPayor.CreatePayor();

                        SelectedDisplayPayor.PropertyChanged += new PropertyChangedEventHandler(SelectedPayor_PropertyChanged);
                        //    SelectedDisplayPayor.Region = UserRegions.FirstOrDefault(s => s.RegionId == SelectedDisplayPayor.RegionID);
                        //}

                        break;
                    case "PayorRegionIndex":
                        SelectedDisplayPayor.RegionID = PayorRegionIndex;
                        SelectedDisplayPayor.Region = UserRegions.FirstOrDefault(s => s.RegionId == SelectedDisplayPayor.RegionID);
                        if (!selectionChangedPayor)
                            PayorRegionChanged();
                        break;
                    case "IncPerHead":
                        //if (SelectedPolicy != null)
                        //{
                        if (IncPerHead == true)
                        {
                            FirstYearText = "First Year PerHead";
                            Renewaltext = "Renewal PerHead";
                            if (SelectedSchedule != null)
                            {
                                SelectedSchedule.ScheduleTypeId = 2;
                            }

                            //if (SelectedPolicyToolIncommingShedule == null) SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule()
                            //{
                            //    FirstYearPercentage = 0,
                            //    RenewalPercentage = 0,
                            //};
                            //SelectedPolicyToolIncommingShedule.ScheduleTypeId = 2;
                            //SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                            //SelectedPolicy.IsIncomingBasicSchedule = true;
                        }
                        //}
                        break;
                    case "IncPercentOfPremium":

                        //if (SelectedPolicy != null)
                        //{
                        if (IncPercentOfPremium == true)
                        {
                            FirstYearText = "First Year";
                            Renewaltext = "Renewal";
                            if (SelectedSchedule != null)
                            {
                                SelectedSchedule.ScheduleTypeId = 1;
                            }
                            //if (SelectedPolicyToolIncommingShedule == null) SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule()
                            //{
                            //    FirstYearPercentage = 0,
                            //    RenewalPercentage = 0,
                            //};
                            //SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                            //SelectedPolicyToolIncommingShedule.ScheduleTypeId = 1;
                            //SelectedPolicy.IsIncomingBasicSchedule = true;
                        }

                        //}
                        break;                        
                    case "NamedScheduleIncPerHead":
                       
                        if (NamedScheduleIncPerHead == true)
                        {
                            FirstYearText = "First Year PerHead";
                            Renewaltext = "Renewal PerHead";
                            if (SelectedNamedSchedule != null)
                            {
                                SelectedNamedSchedule.ScheduleTypeId = 2;
                            }

                        }
                        break;
                    case "NamedScheduleIncPercentOfPremium":
                        if (NamedScheduleIncPercentOfPremium == true)
                        {
                            FirstYearText = "First Year";
                            Renewaltext = "Renewal";
                            if (SelectedNamedSchedule != null)
                            {
                                SelectedNamedSchedule.ScheduleTypeId = 1;
                            }
                        }
                        break;
                    case "Title":
                        break;
                    case "NamedScheduleFirstYearPer":
                        break;
                    case "NamedScheduleRenewalPer":
                        break;
                    case "NamedScheduleSplitPer":
                        break;
                    case "SelectedNamedSchedule":
                        if (SelectedNamedSchedule == null)
                        {
                            NamedScheduleFirstYearPer = 0;
                            NamedScheduleRenewalPer = 0;
                            NamedScheduleSplitPer = 100;
                            NamedScheduleAdvance = 0;
                            Title = "";
                            return;
                        }
                            NamedScheduleFirstYearPer = SelectedNamedSchedule.FirstYearPercentage;
                            NamedScheduleRenewalPer = SelectedNamedSchedule.RenewalPercentage;
                            NamedScheduleSplitPer = SelectedNamedSchedule.SplitPercentage;
                            NamedScheduleAdvance = SelectedNamedSchedule.Advance;
                        if (SelectedNamedSchedule.StringFirstYearPercentage != null)
                        {
                            NamedScheduleFirstYearPer = Convert.ToDouble(SelectedNamedSchedule.ScheduleTypeId == 1 ? SelectedNamedSchedule.StringFirstYearPercentage.Replace("%", "") : SelectedNamedSchedule.StringFirstYearPercentage.Replace("$", ""));
                        }
                        if (SelectedNamedSchedule.StringFirstYearPercentage != null)
                        {
                            NamedScheduleRenewalPer = Convert.ToDouble(SelectedNamedSchedule.ScheduleTypeId == 1 ? SelectedNamedSchedule.StringRenewalPercentage.Replace("%", "") : SelectedNamedSchedule.StringRenewalPercentage.Replace("$", ""));
                        }
                        if (SelectedNamedSchedule.StringFirstYearPercentage != null)
                        {
                            NamedScheduleSplitPer = Convert.ToDouble(SelectedNamedSchedule.StringSplitPercentage.Replace("%", ""));
                        }
                        Title = SelectedNamedSchedule.Title;
                        NamedScheduleAdvance = SelectedNamedSchedule.Advance;
                        if (SelectedNamedSchedule.ScheduleTypeId == 1)//PercentageOfPremium
                        {
                            NamedScheduleIncPercentOfPremium = true;
                            NamedScheduleIncPerHead = false;
                        }
                        else if (SelectedNamedSchedule.ScheduleTypeId == 2)//PerHead
                        {
                            NamedScheduleIncPercentOfPremium = false;
                            NamedScheduleIncPerHead = true;
                           
                        }
                        isNewClicked = false;
                        SavedNamedSelectedSchedule = SelectedNamedSchedule.Clone() as PayorIncomingSchedule;
                        break;

                    case "SelectedSchedule":
                        if (SelectedSchedule == null)
                        {
                            FirstYearPer = 0;
                            RenewalPer = 0;
                            SplitPer = 100;
                            Advance = 0;
                            return;
                        }

                        FirstYearPer = SelectedSchedule.FirstYearPercentage;
                        RenewalPer = SelectedSchedule.RenewalPercentage;
                        SplitPer = SelectedSchedule.SplitPercentage;
                        Advance = SelectedSchedule.Advance;
                        if (SelectedSchedule.ScheduleTypeId == 1)//PercentageOfPremium
                        {
                            IncPercentOfPremium = true;
                            IncPerHead = false;
                            //IncAdvance = false;
                        }
                        else if (SelectedSchedule.ScheduleTypeId == 2)//PerHead
                        {
                            IncPercentOfPremium = false;
                            IncPerHead = true;
                            // IncAdvance = false;
                        }
                        SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == SelectedSchedule.IncomingPaymentTypeID).FirstOrDefault();
                        //if (!isNewClicked)
                        //{
                        SelectedDisplayPayor = DisplayedPayors.Where(x => x.PayorID == SelectedSchedule.PayorID).FirstOrDefault();
                        if (SelectedSchedule.CarrierID != null)
                        {
                            PayorCarriersVM.SelectedCarrier = PayorCarriersVM.Carriers.Where(x => x.CarrierId == SelectedSchedule.CarrierID).FirstOrDefault();
                        }
                        if (SelectedSchedule.CoverageID != null)
                        {
                            CarrierCoveragesVM.SelectedAllProduct = CarrierCoveragesVM.AllProducts.Where(x => x.CoverageID == SelectedSchedule.CoverageID).FirstOrDefault();
                        }
                        if (!string.IsNullOrEmpty(SelectedSchedule.ProductType))
                        {
                            CarrierCoveragesVM.SelectedProductType = CarrierCoveragesVM.CoveragesNickName.Where(x => x.NickName == SelectedSchedule.ProductType).FirstOrDefault();
                        }

                        SavedSelectedSchedule = SelectedSchedule.Clone() as PayorIncomingSchedule;
                        //}

                        isNewClicked = false;
                        break;

                    default:
                        break;
                }
            }
        }

        //private void SelectedField_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "IsSelected":

        //            if (ReportFields.Where(x => x.IsSelected).ToList().Count == ReportFields.Count())
        //                IsSelectAll = true; 
        //            break;
        //        default:
        //            break;
        //    }
        //}

        void SelectedPayor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Region":
                    if (SelectedDisplayPayor != null && SelectedDisplayPayor.Region != null)
                    {
                        SelectedPayor.PayorRegionID = SelectedDisplayPayor.RegionID = SelectedDisplayPayor.Region.RegionId;
                        SelectedPayor.Region = SelectedDisplayPayor.Region;
                    }
                    break;
            }
        }

        private int _payorRegionIndex = 0;

        public int PayorRegionIndex
        {
            get
            {
                return _payorRegionIndex;
            }
            set
            {
                _payorRegionIndex = value;
                OnPropertyChanged("PayorRegionIndex");
            }
        }

        private void PayorRegionChanged()
        {
            if (SelectedRegion == null || SelectedDisplayPayor == null || SelectedDisplayPayor.Region == null)
                return;

            if (SelectedRegion.RegionId == 7 || PayorRegionIndex == 5)
                return;

            changeCurrentPayor = false;
            SelectedDisplayPayor.Region = UserRegions[PayorRegionIndex];
            SelectedRegion = UserRegions[PayorRegionIndex];
            changeCurrentPayor = true;
        }

        private void LoadRegions()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {

                Regions = serviceClients.MasterClient.GetRegionList();
                UserRegions = new ObservableCollection<Region>(Regions);
                int _idx = UserRegions.IndexOf((from p in UserRegions where p.RegionId == 7 select p).FirstOrDefault());
                if (_idx != -1)
                    UserRegions.RemoveAt(_idx);
                SelectedRegion = Regions.Where(s => s.RegionId == 7).FirstOrDefault();

            }
        }

        private bool AllowEdit = false;

        private void SettingScreenControl()
        {
            if (RoleManager.Role == UserRole.Agent)
            {
                LicenseePayorComboEnable = false;
                WebsitePayorRadioButtonEnable = true;
                AllowEdit = true;

                if (RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Read)
                {
                    WebsitePayorRadioButtonEnable = false;
                    AllowEdit = false;
                }
            }
            else if (RoleManager.Role == UserRole.Administrator)
            {
                LicenseePayorComboEnable = false;
                WebsitePayorRadioButtonEnable = false;
                AllowEdit = true;
            }
            else if (RoleManager.Role == UserRole.DEP)
            {
                LicenseePayorComboEnable = false;
                WebsitePayorRadioButtonEnable = false;
                AllowEdit = false;
            }
            else if (RoleManager.Role == UserRole.HO)
            {
                LicenseePayorComboEnable = false;
                WebsitePayorRadioButtonEnable = true;
                AllowEdit = true;
            }
            else if (RoleManager.Role == UserRole.SuperAdmin)
            {
                LicenseePayorComboEnable = true;
                WebsitePayorRadioButtonEnable = true;
                AllowEdit = true;
            }

        }

        private void LoadPayors()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                PayorFillInfo payorFillInfo = new PayorFillInfo { PayorStatus = PayorStatus.All };
                if (SharedVMData.SelectedLicensee != null)
                    Payors = serviceClients.SettingDisplayedPayorClient.GetSettingDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, payorFillInfo);
            }
        }

        #endregion

        #region PayorSource

        private ICommand _SourceChanged;
        public ICommand SourceChanged
        {
            get
            {
                if (_SourceChanged == null)
                {
                    _SourceChanged = new BaseCommand(p => OnSourceChanged(p));
                }
                return _SourceChanged;
            }

        }

        private void OnSourceChanged(object arg)
        {
            string value = arg as string;
            using (ServiceClients serviceClients = new ServiceClients())
            {
                if (SharedVMData.SelectedLicensee != null && SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty && SelectedDisplayPayor != null)
                {
                    PayorSource source = new PayorSource();
                    source.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                    source.PayorId = SelectedDisplayPayor.PayorID;

                    //check if payor id is null
                    if (source.PayorId == new Guid())
                        return;

                    if (value == "Paper")
                    {
                        source.IsWebsite = false;
                        SelectedDisplayPayor.SourceType = 0;
                        ShowWebSitePanel = "Hidden";

                    }
                    else
                    {
                        source.IsWebsite = true;
                        SelectedDisplayPayor.SourceType = 1;
                        ShowWebSitePanel = "Visible";
                    }
                    source.Notes = RTFNotecontent;

                    if (source.PayorId != new Guid())
                        serviceClients.PayorSourceClient.AddUpdatePayorSource(source);
                }
            }
        }

        #endregion

        #region ControlLevelProerty

        #region SettingScreen

        private bool websitepayorradio = false;
        public bool WebsitePayorRadioButtonEnable
        {
            get
            {
                return websitepayorradio;
            }
            set
            {
                websitepayorradio = value;
                OnPropertyChanged("WebsitePayorRadioButtonEnable");
            }
        }

        private bool licenseepayorcomboenable = false;
        public bool LicenseePayorComboEnable
        {
            get
            {
                return licenseepayorcomboenable;
            }
            set
            {
                licenseepayorcomboenable = value;
                OnPropertyChanged("LicenseePayorComboEnable");

            }
        }

        #endregion
        #endregion

        #region Payor
        #region Public Properties

        private bool _RTFNotecontentReadOnly;
        public bool RTFNotecontentReadOnly
        {
            get
            {
                return _RTFNotecontentReadOnly;
            }
            set
            {
                _RTFNotecontentReadOnly = value;
                OnPropertyChanged("RTFNotecontentReadOnly");
            }
        }

        private ICommand _btnSaveNotes;
        public ICommand btnSaveNotes
        {
            get
            {

                if (_btnSaveNotes == null)
                    _btnSaveNotes = new BaseCommand(Param => BeforeSaveNotes(), Param => OnSaveNotes());
                return _btnSaveNotes;
            }
        }

        private ICommand _RTFNotesLostFocus;
        public ICommand RTFNotesLostFocus
        {
            get
            {
                if (_RTFNotesLostFocus == null)
                    _RTFNotesLostFocus = new BaseCommand(param => BeforeOnRTFNotesLostFocus(), param => OnRTFNotesLostFocus());
                return _RTFNotesLostFocus;
            }
        }

        private void OnRTFNotesLostFocus()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = SelectedDisplayPayor.PayorID });
                _PayorSource.Notes = RTFNotecontent;
                serviceClients.PayorSourceClient.AddUpdatePayorSource(_PayorSource);
            }
        }

        private void OnSaveNotes()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = SelectedDisplayPayor.PayorID });
                _PayorSource.Notes = RTFNotecontent;
                serviceClients.PayorSourceClient.AddUpdatePayorSource(_PayorSource);
            }
        }

        private bool BeforeSaveNotes()
        {
            if (string.IsNullOrEmpty(RTFNotecontent))
                return false;
            else
            {

                return true;
            }
        }

        private bool BeforeOnRTFNotesLostFocus()
        {
            if (SelectedDisplayPayor.IsGlobal)
            {
                return false;
            }
            else
            {
                return true;

            }
        }

        private string _RTFNotecontent;
        public string RTFNotecontent
        {
            get
            {
                return _RTFNotecontent;
            }
            set
            {
                _RTFNotecontent = value;
                OnPropertyChanged("RTFNotecontent");
            }
        }

        private ObservableCollection<SettingDisplayedPayor> _payors = null;
        public ObservableCollection<SettingDisplayedPayor> Payors
        {
            get
            {
                return _payors;
            }
            set
            {
                _payors = value;
                OnPropertyChanged("Payors");
            }
        }

        private void FilterDisplayedPayors(bool changeCurrentPayor)
        {
            if (Payors == null || Payors.Count == 0)
                return;

            //if ((SelectedRegion != null))
            //{
            //    if (SelectedRegion.RegionId != 7)
            //    {
            //        SettingDisplayedPayor cPayor = SelectedDisplayPayor;

            //        if (_payorType == PayorType.All)
            //            DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.Where(s => s.RegionID == SelectedRegion.RegionId || s.RegionID == 5).OrderBy(s => s.PayorName).ToList());
            //        else if (_payorType == PayorType.Global)
            //            DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.Where(s => (s.RegionID == SelectedRegion.RegionId || s.RegionID == 5) && s.IsGlobal).OrderBy(s => s.PayorName).ToList());
            //        else
            //            DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.Where(s => (s.RegionID == SelectedRegion.RegionId || s.RegionID == 5) && (!s.IsGlobal)).OrderBy(s => s.PayorName).ToList());

            //        if (DisplayedPayors != null && DisplayedPayors.Count > 0)
            //        {
            //            if (changeCurrentPayor)
            //            {
            //                SelectedDisplayPayor = DisplayedPayors.FirstOrDefault();
            //                //SelectedDisplayPayor = DisplayedPayors.First().Clone() as SettingDisplayedPayor;
            //            }
            //            else
            //                SelectedDisplayPayor = cPayor;
            //        }
            //    }
            //    else
            //    {
            SettingDisplayedPayor cPayor = SelectedDisplayPayor;

            if (_payorType == PayorType.All)
                DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.OrderBy(s => s.PayorName).ToList());
            else if (_payorType == PayorType.Global)
                DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.Where(s => s.IsGlobal).OrderBy(s => s.PayorName).ToList());
            else
                DisplayedPayors = new ObservableCollection<SettingDisplayedPayor>(Payors.Where(s => !s.IsGlobal).OrderBy(s => s.PayorName).ToList());

            if (DisplayedPayors != null && DisplayedPayors.Count > 0)
            {
                if (changeCurrentPayor)
                {
                    SelectedDisplayPayor = DisplayedPayors.FirstOrDefault();
                    //SelectedDisplayPayor = DisplayedPayors.First().Clone() as SettingDisplayedPayor;
                }
                else
                    SelectedDisplayPayor = cPayor;

            }
            //    }
            //}
        }

        private ObservableCollection<SettingDisplayedPayor> _displayedPayors;
        public ObservableCollection<SettingDisplayedPayor> DisplayedPayors
        {
            get
            {
                return _displayedPayors;
            }
            set
            {
                _displayedPayors = value;
                OnPropertyChanged("DisplayedPayors");
            }
        }

        private ObservableCollection<SettingCarrierCoveragesVM> _displayedCoverages;
        public ObservableCollection<SettingCarrierCoveragesVM> DisplayedCoverages
        {
            get
            {
                return _displayedCoverages;
            }
            set
            {
                _displayedCoverages = value;
                OnPropertyChanged("DisplayedCoverages");
            }
        }

        public ObservableCollection<Region> UserRegions
        {
            get
            {
                if (_objRegion != null)
                    return _objRegion;
                return null;
            }
            set
            {
                _objRegion = value;
                OnPropertyChanged("UserRegions");
            }
        }

        public ObservableCollection<Region> Regions
        {
            get
            {
                return _regions;
            }
            set
            {
                _regions = value;
                OnPropertyChanged("Regions");
            }
        }

        public Region SelectedRegion
        {
            get
            {
                if (_selectedRegion != null)
                    return _selectedRegion;

                return null;
            }
            set
            {
                if (_selectedRegion != null && value != null)
                    if (_selectedRegion.RegionId == value.RegionId)
                        return;

                _selectedRegion = value;
                OnPropertyChanged("SelectedRegion");
            }

        }

        public Region SelectedUserRegion
        {
            get
            {
                if (_selectedUserRegion != null)
                    return _selectedUserRegion;

                return null;
            }
            set
            {
                if (_selectedUserRegion != null && value != null)
                    if (_selectedUserRegion.RegionId == value.RegionId)
                        return;

                _selectedUserRegion = value;
                OnPropertyChanged("SelectedUserRegion");
            }
        }

        private bool readOnly;
        public bool ReadOnly
        {
            get { return readOnly; }
            set
            {
                readOnly = value;
                OnPropertyChanged("ReadOnly");
            }
        }

        private bool isEnable;
        public bool IsEnable
        {
            get { return isEnable; }
            set
            {
                isEnable = value;
                OnPropertyChanged("IsEnable");
            }
        }

        private Payor _SelectedPayor;
        public Payor SelectedPayor
        {
            get
            {
                return _SelectedPayor;
            }
            set
            {
                if (_SelectedPayor != null)
                    _SelectedPayor.PropertyChanged -= new PropertyChangedEventHandler(SelectedPayor_PropertyChanged);

                _SelectedPayor = value;
                OnPropertyChanged("SelectedPayor");
            }
        }

        private SettingDisplayedPayor _SelectedDisplayPayor;
        public SettingDisplayedPayor SelectedDisplayPayor
        {
            get
            {
                return _SelectedDisplayPayor;
            }
            set
            {
                _SelectedDisplayPayor = value;
                OnPropertyChanged("SelectedDisplayPayor");
            }
        }

        private string _Showrtfnote;
        public string Showrtfnote
        {
            get
            {
                return _Showrtfnote;
            }
            set
            {
                _Showrtfnote = value;
                OnPropertyChanged("Showrtfnote");
            }
        }
        
        private string _WebSitePanel = string.Empty;
        public string ShowWebSitePanel
        {
            get
            {
                if (_WebSitePanel != string.Empty)
                    return _WebSitePanel;

                return string.Empty;
            }
            set
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    _WebSitePanel = value;

                    if (_WebSitePanel == "Visible")
                    {
                        ShowWebSiteBorder = "Visible";
                        if (SelectedDisplayPayor != null && SelectedDisplayPayor.PayorID != Guid.Empty)
                        {
                            if (SharedVMData.SelectedLicensee != null && SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                            {
                                DisplayedLogins = serviceClients.PayorUserWebSiteClient.GetPayorUsers(SharedVMData.SelectedLicensee.LicenseeId, SelectedDisplayPayor.PayorID);

                                if (displayedLogins != null && displayedLogins.Count != 0)
                                    CurrentSite = displayedLogins[0];
                                else
                                {
                                    CurrentSite = new PayorSiteLoginInfo();
                                    CurrentSite.SiteID = Guid.NewGuid();
                                }
                            }
                        }
                    }
                    else
                    {
                        ShowWebSiteBorder = "Hidden";
                    }
                    OnPropertyChanged("ShowWebSitePanel");
                }
            }
        }

        private string _Border = string.Empty;
        public string ShowWebSiteBorder
        {
            get
            {
                if (_Border != string.Empty)
                    return _Border;
                else
                    return _Border;
            }
            set
            {
                _Border = value;
                OnPropertyChanged("ShowWebSiteBorder");
            }
        }

        private bool _isSelectAll;
        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set
            {
                _isSelectAll = value;
                OnPropertyChanged("IsSelectAll");
            }
        }

        private ReportCustomFieldSettings _selectedField;
        public ReportCustomFieldSettings SelectedField
        {
            get
            {
                return _selectedField;
            }
            set
            {
                _selectedField = value;
                OnPropertyChanged("SelectedField");
            }
        }



        private ObservableCollection<ReportCustomFieldSettings> _reportFields = null;
        public ObservableCollection<ReportCustomFieldSettings> ReportFields
        {
            get
            {
                return _reportFields;
            }
            set
            {
                _reportFields = value;
                OnPropertyChanged("ReportFields");
            }
        }

        public void SelectedLicenseeChanged()
        {
            //  if (SelectedTabIndex == 1)
            {
                LoadPayors();
                PayorCarriersVM.LoadCarrierLicenseeWise();
                CarrierCoveragesVM.loadProductDataLicenseeWise();
            }
            LoadReportSettings();
        }

        public VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
                if (_SharedVMData == null)
                    _SharedVMData = VMSharedData.getInstance();

                return _SharedVMData;
            }
        }

        #endregion

        #region Icommands
        public ICommand SavePayor
        {
            get
            {
                if (_savePayor == null)
                    _savePayor = new BaseCommand(param => BeforeOnSavePayor(), param => OnSavePayor());
                return _savePayor;
            }

        }

        ICommand _cancelReportSetting;
        public ICommand CancelReportSetting
        {
            get
            {
                if (_cancelReportSetting == null)
                    _cancelReportSetting = new BaseCommand(param => BeforeOnCancelReportSetting(), param => OnCancelReportSetting());
                return _cancelReportSetting;
            }


        }

        public ICommand DeletePayor
        {
            get
            {
                if (_deletePayor == null)
                    _deletePayor = new BaseCommand(param => BeforeOnDeletePayor(), param => OnDeletePayor());
                return _deletePayor;
            }

        }

        public ICommand DeleteSchedule
        {
            get
            {
                if (_deleteSchedule == null)
                    _deleteSchedule = new BaseCommand(param => BeforeOnDeleteSchedule(), param => OnDeleteSchedule());
                return _deleteSchedule;
            }

        }
        public ICommand DeleteNamedSchedule
        {
            get
            {
                if (_deleteNamedSchedule == null)
                    _deleteNamedSchedule = new BaseCommand(param => BeforeOnDeleteNamedSchedule(), param => OnDeleteNamedSchedule());
                return _deleteNamedSchedule;
            }

        }
        public ICommand NewSchedule
        {
            get
            {
                if (_newSchedule == null)
                    _newSchedule = new BaseCommand(param => BeforeOnNewSchedule(), param => OnNewSchedule());
                return _newSchedule;
            }

        }

        public ICommand NewNamedSchedule
        {
            get
            {
                if (_newNamedSchedule == null)
                    _newNamedSchedule = new BaseCommand(param => BeforeOnNewNamedSchedule(), param => OnNewNamedSchedule());
                return _newNamedSchedule;
            }

        }
        public ICommand _Global;
        public ICommand Global
        {
            get
            {
                if (_Global == null)
                    _Global = new BaseCommand(param => FilterPayor(0));
                return _Global;
            }

        }

        private ICommand _Local;
        public ICommand Local
        {
            get
            {
                if (_Local == null)
                    _Local = new BaseCommand(param => FilterPayor(1));
                return _Local;
            }

        }

        private ICommand _all;
        public ICommand All
        {
            get
            {
                if (_all == null)
                    _all = new BaseCommand(param => FilterPayor(2));
                return _all;
            }

        }


        //public ICommand _childCheckboxChecked;

        //public ICommand ChildCheckboxChecked
        //{
        //    get
        //    {
        //        if (_childCheckboxChecked == null)
        //            _childCheckboxChecked = new BaseCommand(param => OnCheckedChange());
        //        return _childCheckboxChecked;
        //    }
        //}
        private ICommand _onselectAll;
        public ICommand OnSelectAll
        {
            get
            {
                if (_onselectAll == null)
                    _onselectAll = new BaseCommand(param => OnSelectChange(param));
                return _onselectAll;

            }
        }

        public ICommand _saveReportFields;
        public ICommand SaveReportFields
        {
            get
            {
                if (_saveReportFields == null)
                    _saveReportFields = new BaseCommand(param => BeforeOnSaveReportFields(), param => OnSaveReportFields());
                return _saveReportFields;
            }

        }
        public ICommand _saveSchedule;
        public ICommand SaveSchedule
        {
            get
            {
                if (_saveSchedule == null)
                    _saveSchedule = new BaseCommand(param => BeforeOnSaveSchedule(), param => OnSaveSchedule());
                return _saveSchedule;
            }

        }
        public ICommand _saveNamedSchedule;
        public ICommand SaveNamedSchedule
        {
            get
            {
                if (_saveNamedSchedule == null)
                    _saveNamedSchedule = new BaseCommand(param => BeforeOnSaveNamedSchedule(), param => OnSaveNamedSchedule());
                return _saveNamedSchedule;
            }

        }


        public ICommand _cancelSchedule;
        public ICommand CancelSchedule
        {
            get
            {
                if (_cancelSchedule == null)
                    _cancelSchedule = new BaseCommand(param => BeforeOnCancelSchedule(), param => OnCancelSchedule());
                return _cancelSchedule;
            }

        }
        public ICommand _cancelNamedSchedule;
        public ICommand CancelNamedSchedule
        {
            get
            {
                if (_cancelNamedSchedule == null)
                    _cancelNamedSchedule = new BaseCommand(param => BeforeOnCancelNamedSchedule(), param => OnCancelNamedSchedule());
                return _cancelNamedSchedule;
            }

        }
        bool BeforeOnCancelNamedSchedule()
        {
            if (SelectedNamedSchedule == null)
                return false;

            return true;
        }

        #endregion
        #region Private Methods
        bool BeforeOnDeleteSchedule()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
            {
                return false;
            }
            if (SelectedSchedule == null || isNewClicked)
                return false;

            return true;
        }
        bool BeforeOnDeleteNamedSchedule()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
            {
                return false;
            }
            if (SelectedNamedSchedule == null || isNewClicked)
                return false;

            return true;
        }
        void OnDeleteSchedule()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this schedule?", "Delete Schedule", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    serviceClients.GlobalIncomingScheduleClient.DeleteSchedule(SelectedSchedule.IncomingScheduleID);
                    ScheduleList = serviceClients.GlobalIncomingScheduleClient.GetAllSchedules(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                    SelectedSchedule = null;
                }
            }
        }
        void OnDeleteNamedSchedule()
        {

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this schedule?", "Delete Schedule", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    serviceClients.GlobalIncomingScheduleClient.DeleteSchedule(SelectedNamedSchedule.IncomingScheduleID);
                    NamedScheduleList = serviceClients.SettingsClient.GetNamedScheduleList(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                    SelectedNamedSchedule = null;
                }
            }
        }
        void OnSelectChange(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (ReportCustomFieldSettings rpt in ReportFields)
                    {
                        if (rpt.IsModifiable)
                            rpt.IsSelected = true;
                    }

                    break;
                case "None":
                    foreach (ReportCustomFieldSettings rpt in ReportFields)
                    {
                        if (rpt.IsModifiable)
                            rpt.IsSelected = false;
                    }
                    break;
            }
        }

        bool BeforeOnSaveReportFields()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
            {
                ErrorTooltipForReport = "Security Violation";
                return false;
            }
            return true;
        }

        bool BeforeOnCancelSchedule()
        {
            if (SelectedSchedule == null)
                return false;

            return true;
        }

        bool BeforeOnSaveNamedSchedule()
        {
            ErrorTooltip1 = null;
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
            {
                ErrorTooltip1 = "Security Violation";
                return false;
            }
            if (SelectedNamedSchedule == null)
            {
                ErrorTooltip1 = "Please select a schedule to edit or add a new one.";
                return false;
            }
            else
            {
                if ((Title == null || Title == "") && (IsNamedSchedule || (SelectedNamedSchedule != null && SelectedNamedSchedule.IsNamedSchedule == true)))
                {
                    ErrorTooltip1 = "Please enter title.";
                    return false;
                }
                if (NamedScheduleSplitPer == null || NamedScheduleSplitPer == 0)
                {
                    ErrorTooltip1 = "Split percentage cannot have 0 or blank.";
                    return false;

                }
                if ((NamedScheduleRenewalPer == null || NamedScheduleRenewalPer == 0) && (NamedScheduleFirstYearPer == null || NamedScheduleFirstYearPer == 0))
                {
                    ErrorTooltip1 = "First year and renewal cannot be blank at the same time in payor schedule.";
                    return false;
                }
                if (Title != null || Title != "")
                {
                    PayorIncomingSchedule existingSchedule = new PayorIncomingSchedule();
                    using (ServiceClients serviceClients = new ServiceClients())
                    {
                        var incomingScheduleId = Guid.Empty;
                        if (SelectedNamedSchedule != null)
                        {
                            incomingScheduleId = SelectedNamedSchedule.IncomingScheduleID;
                        }
                        existingSchedule = serviceClients.SettingsClient.IsNamedScheduleExist(out bool isRecordExist, Title, incomingScheduleId,SharedVMData.SelectedLicensee.LicenseeId);
                        var isScheduleExist = isRecordExist;
                        if (existingSchedule.IncomingScheduleID != incomingScheduleId && isScheduleExist && SharedVMData.SelectedLicensee.LicenseeId == existingSchedule.LicenseeID)
                        {
                            ErrorTooltip1 = "Title already Exist with same name.";
                            return false;
                        }
                        else if(Title.Length>50)
                        {
                            ErrorTooltip1 = "Title length can't be more than 50 characters.";
                            return false;
                        }
                    }


                }
            }
            return true;
        }
        void OnCancelSchedule()
        {
            if (isNewClicked)
            {
                OnNewSchedule(); //reset all values to original 
            }
            else
            {
                SelectedSchedule.Copy(SavedSelectedSchedule);
                OnPropertyChanged("SelectedSchedule");
            }
        }
        void OnCancelNamedSchedule()
        {
            if (isNewClicked)
            {
                OnNewNamedSchedule(); //reset all values to original 
            }
            else
            {
                SelectedNamedSchedule.Copy(SavedNamedSelectedSchedule);
                OnPropertyChanged("SelectedNamedSchedule");
            }
        }
        
        bool BeforeOnSaveSchedule()
        {
            ErrorTooltip = null;
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
            {
                ErrorTooltip1 = "Security Violation";
                return false;
            }
            if (SelectedSchedule == null)
            {
                ErrorTooltip = "Please select a schedule to edit or add a new one.";
                return false;
            }

            else
            {
                if (selectedCarrier == null || selectedCarrier.CarrierId == Guid.Empty)
                {
                    ErrorTooltip = "Please select suitable carrier.";
                    return false;
                }

                if (selectedProductType == string.Empty)
                {
                    ErrorTooltip = "Please select product type.";
                    return false;
                }
                if (selectedCoverage == null || selectedCoverage.CoverageID == Guid.Empty)
                {
                    ErrorTooltip = "Please select product.";
                    return false;
                }
                if ((FirstYearPer == null || FirstYearPer == 0) &&
                     (RenewalPer == null || RenewalPer == 0) &&
                      (SplitPer == null || SplitPer == 0) &&
                     Advance == 0
                    )
                {
                    ErrorTooltip = "Please make sure that schedule parameters have valid non-zero values.";
                    return false;
                }
            }

            return true;
        }


        private bool BeforeOnCancelReportSetting()
        {
            return true;
        }

        private void OnCancelReportSetting()
        {
            ReloadReportSettings();
        }

        void ResetSchedule()
        {
            IncPercentOfPremium = true;
            SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == 1).FirstOrDefault();
            FirstYearPer = RenewalPer = Advance = 0;
            SplitPer = 100;
            SelectedSchedule = null;
        }

        public bool IsExistingScheduleConfiguration(out Guid? ExistingScheduleID)
        {
            bool result = false;
            ExistingScheduleID = null;
            using (ServiceClients serviceClients = new ServiceClients())
            {
                SelectedSchedule.PayorID = SelectedDisplayPayor.PayorID;
                SelectedSchedule.CarrierID = selectedCarrier.CarrierId;
                SelectedSchedule.CoverageID = selectedCoverage.CoverageID;
                SelectedSchedule.ProductType = selectedProductType;
                SelectedSchedule.IncomingPaymentTypeID = SelectedMasterIncomingPaymentType.PaymentTypeId;
                SelectedSchedule.FirstYearPercentage = FirstYearPer;
                SelectedSchedule.RenewalPercentage = RenewalPer;
                SelectedSchedule.SplitPercentage = SplitPer;
                SelectedSchedule.Advance = Advance;
                SelectedSchedule.LicenseeID = SharedVMData.SelectedLicensee.LicenseeId;

                PayorIncomingSchedule schedule = serviceClients.GlobalIncomingScheduleClient.GetPayorScheduleDetails(SelectedSchedule.PayorID, SelectedSchedule.CarrierID, SelectedSchedule.CoverageID, SharedVMData.SelectedLicensee.LicenseeId, SelectedSchedule.ProductType, SelectedSchedule.IncomingPaymentTypeID);
                if (schedule != null && schedule.IncomingScheduleID != Guid.Empty && schedule.IncomingScheduleID != SelectedSchedule.IncomingScheduleID)
                {
                    result = true;
                    ExistingScheduleID = schedule.IncomingScheduleID;
                }
            }
            return result;
        }

        public void SaveIncomingSchedule(string option)
        {
            int overwriteOption = 0;
            switch (option)
            {
                case "All":
                    overwriteOption = 2;
                    break;
                case "OnlyNew":
                    overwriteOption = 1;
                    break;
                default:
                    overwriteOption = 0;
                    break;
            }
            using (ServiceClients serviceClients = new ServiceClients())
            {
                serviceClients.GlobalIncomingScheduleClient.SavePayorSchedules(SelectedSchedule, overwriteOption);
                ScheduleList = serviceClients.GlobalIncomingScheduleClient.GetAllSchedules(SharedVMData.SelectedLicensee.LicenseeId).ToList();
            }
            ResetSchedule();
        }

        void OnSaveNamedSchedule()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                var incomingScheduleId = Guid.Empty;
                if (SelectedNamedSchedule != null)
                {
                    incomingScheduleId = SelectedNamedSchedule.IncomingScheduleID;
                }

                PayorIncomingSchedule schedules = new PayorIncomingSchedule();
                schedules.IncomingScheduleID = incomingScheduleId == Guid.Empty ? Guid.NewGuid() : incomingScheduleId;
                schedules.Title = Title;
                schedules.FirstYearPercentage = NamedScheduleFirstYearPer;
                schedules.RenewalPercentage = NamedScheduleRenewalPer;
                schedules.SplitPercentage = NamedScheduleSplitPer;
                schedules.Advance = NamedScheduleAdvance;
                schedules.LicenseeID = SharedVMData.SelectedLicensee.LicenseeId;
                if (NamedScheduleIncPercentOfPremium == true)
                {
                    schedules.ScheduleTypeId = 1;
                }
                else
                {
                    schedules.ScheduleTypeId = 2;
                }
                serviceClients.SettingsClient.AddUpdateNamedSchedule(schedules);
                NamedScheduleList = serviceClients.SettingsClient.GetNamedScheduleList(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                SelectedNamedSchedule = null;
            }






        }


        void OnSaveSchedule()
        {
            Guid? existingScheduleID = null;
            if (IsExistingScheduleConfiguration(out existingScheduleID))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Schedule already exists for the given Payor-Product configuration. Are you sure you want to overwrite schedule settings for this configuration?", "Overwrite Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    SelectedSchedule.IncomingScheduleID = (Guid)existingScheduleID; //Make sure that existing schedule is updated with new configuration
                    if (SaveScheduleEvent != null)
                    {
                        SaveScheduleEvent();
                    }
                    /* CustomPrompt dialog = new CustomPrompt();
                     if (dialog.ShowDialog() == true)
                     {
                         VMInstances.SettingManager.SaveIncomingSchedule(dialog.SelectedOption);
                     }*/

                }
            }
            else
            {
                if (SaveScheduleEvent != null)
                {
                    SaveScheduleEvent();
                }
                /* CustomPrompt dialog = new CustomPrompt();
                 if (dialog.ShowDialog() == true)
                 {
                     VMInstances.SettingManager.SaveIncomingSchedule(dialog.SelectedOption);
                 }*/
            }
        }


        /*  void OnSaveSchedule()
          {

              using (ServiceClients serviceClients = new ServiceClients())
              {
                  SelectedSchedule.PayorID = SelectedDisplayPayor.PayorID;
                  SelectedSchedule.CarrierID = selectedCarrier.CarrierId;
                  SelectedSchedule.CoverageID = selectedCoverage.CoverageID;
                  SelectedSchedule.ProductType = selectedProductType;
                  SelectedSchedule.IncomingPaymentTypeID = SelectedMasterIncomingPaymentType.PaymentTypeId;
                  SelectedSchedule.FirstYearPercentage = FirstYearPer;
                  SelectedSchedule.RenewalPercentage = RenewalPer;
                  SelectedSchedule.SplitPercentage = SplitPer;
                  SelectedSchedule.Advance = Advance;
                  SelectedSchedule.LicenseeID = SharedVMData.SelectedLicensee.LicenseeId;

                  PayorIncomingSchedule schedule = serviceClients.GlobalIncomingScheduleClient.GetPayorScheduleDetails(SelectedSchedule.PayorID, SelectedSchedule.CarrierID, SelectedSchedule.CoverageID, SharedVMData.SelectedLicensee.LicenseeId, SelectedSchedule.ProductType, SelectedSchedule.IncomingPaymentTypeID);
                  if (schedule != null && schedule.IncomingScheduleID != Guid.Empty && schedule.IncomingScheduleID != SelectedSchedule.IncomingScheduleID)
                  {
                      MessageBoxResult result = System.Windows.MessageBox.Show("Schedule already exists for the given Payor-Product configuration. Are you sure you want to overwrite?", "Overwrite Warning", MessageBoxButton.YesNo);
                      if (result == MessageBoxResult.Yes)
                      {
                          result = System.Windows.MessageBox.Show("Do you want to apply this schedule to all the policies matching the selected payor-product configuration?" + System.Environment.NewLine  + "Please select 'Yes' to apply and overwrite existing ones too OR 'No' to apply to new ones only without overwriting.", "Overwrite Warning", MessageBoxButton.YesNoCancel);
                          if (result == MessageBoxResult.Cancel)
                              return;

                          bool overwrite = result == MessageBoxResult.Yes;
                          SelectedSchedule.IncomingScheduleID = schedule.IncomingScheduleID; //make sure existing schedule is overwritten
                          serviceClients.GlobalIncomingScheduleClient.SavePayorSchedules(SelectedSchedule, overwrite);
                          ScheduleList = serviceClients.GlobalIncomingScheduleClient.GetAllSchedules(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                          ResetSchedule();
                      }
                  }
                  else
                  {
                      MessageBoxResult result = System.Windows.MessageBox.Show("This schedule will be applied to all the policies matching the payor configuration. Please select 'Yes' to overwrite or 'No' to save without overwriting.", "Overwrite Warning", MessageBoxButton.YesNoCancel);
                      if (result == MessageBoxResult.Cancel)
                          return;

                      bool overwrite = result == MessageBoxResult.Yes;
                      serviceClients.GlobalIncomingScheduleClient.SavePayorSchedules(SelectedSchedule, overwrite);
                      ScheduleList = serviceClients.GlobalIncomingScheduleClient.GetAllSchedules(SharedVMData.SelectedLicensee.LicenseeId).ToList();
                      ResetSchedule();

                  }
              }
          }*/


        void OnSaveReportFields()
        {
            try
            {


                //Get all fields value
                string fields = "";
                foreach (ReportCustomFieldSettings rpt in ReportFields)
                {
                    if (rpt.IsSelected)
                    {
                        fields += rpt.FieldName + ",";
                    }
                }
                if (!string.IsNullOrEmpty(fields))
                {
                    fields = fields.Substring(0, fields.Length - 1);
                }
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    serviceClients.SettingsClient.AddUpdate(SharedVMData.SelectedLicensee.LicenseeId, PSReportID, fields);
                }
                MessageBox.Show("Report Settings are successfully saved to the system.", "Report Settings Saved!", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                if (objLog == null) objLog = new MastersClient();
                objLog.AddLog("Report fields save exeption: Agency " + SharedVMData.SelectedLicensee.LicenseeId + ", ex: " + ex.Message);
                if (ex.InnerException != null)
                {
                    objLog.AddLog("Report fields save inner exeption: Agency " + SharedVMData.SelectedLicensee.LicenseeId + ", ex: " + ex.InnerException.Message);
                }
                MessageBox.Show("An error occurred while saving report settings to the system. Please try again.", "Error!", MessageBoxButton.OK);
            }
        }

        private bool BeforeOnSavePayor()
        {
            bool isAllowedToSavePayor = false;

            if (SelectedPayor == null)
                return isAllowedToSavePayor;

            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return isAllowedToSavePayor;

            if (string.IsNullOrWhiteSpace(SelectedPayor.PayorName) || string.IsNullOrWhiteSpace(SelectedPayor.NickName))
                return isAllowedToSavePayor;

            //if (CurrentPayor.Region == null)
            //    return isAllowedToSavePayor;

            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return isAllowedToSavePayor;

            if (AllowEdit)
            {
                if (SelectedPayor != null && SelectedPayor.ISGlobal == false)
                    isAllowedToSavePayor = true;
            }
            return isAllowedToSavePayor;
        }


        private void FilterPayor(int _Type)
        {
            if (_Type == 0)
            {
                _payorType = PayorType.Global;
            }
            else if (_Type == 1)
            {
                _payorType = PayorType.Local;
            }
            else
            {
                _payorType = PayorType.All;
            }
            FilterDisplayedPayors(true);
        }

        bool flagPayorViewChage = false;

        private void OnSavePayor()
        {
            if (SelectedPayor.ISGlobal == true || SelectedRegion == null)
                return;
            using (ServiceClients serviceClients = new ServiceClients())
            {
                PayorClient Client = serviceClients.PayorClient;
                SelectedPayor.UserID = RoleManager.userCredentialID;
                SelectedPayor.PayorLicensee = SharedVMData.SelectedLicensee.LicenseeId;
                SelectedPayor.PayorName = SelectedPayor.PayorName.Trim();
                SelectedPayor.NickName = SelectedPayor.NickName.Trim();

                ReturnStatus status = null;
                bool IsAddCase = false;

                if (SelectedPayor.PayorID == Guid.Empty)
                {
                    SelectedPayor.PayorID = Guid.NewGuid();
                    status = Client.AddUpdateDeletePayor(SelectedPayor, Operation.Add);
                    IsAddCase = true;
                }
                else
                {
                    status = Client.AddUpdateDeletePayor(SelectedPayor, Operation.Upadte);
                    IsAddCase = false;
                }


                if (!status.IsError)
                {
                    SettingDisplayedPayor selectedPayor = null;
                    if (IsAddCase)
                    {
                        selectedPayor = SelectedPayor.CreateSettingDisplayPayor();
                        Payors.Add(selectedPayor);
                        SelectedDisplayPayor = selectedPayor;
                    }
                    else
                    {
                        selectedPayor = Payors.FirstOrDefault(s => s.PayorID == SelectedPayor.PayorID);
                        selectedPayor.Copy(SelectedPayor);
                    }

                    FilterDisplayedPayors(false);

                    if (SelectedDisplayPayor != null && SelectedDisplayPayor.Region != null && SelectedRegion != null)
                        if (SelectedDisplayPayor.Region.RegionId != SelectedRegion.RegionId)
                            PayorRegionChanged();
                }
                else
                {
                    if (IsAddCase)
                        SelectedPayor.PayorID = Guid.Empty;

                    MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool BeforeOnDeletePayor()
        {
            bool IsAllowedToDeletePayor = false;

            if (SelectedDisplayPayor == null)
                return IsAllowedToDeletePayor;

            if (DisplayedPayors == null || DisplayedPayors.Count == 0)
                return IsAllowedToDeletePayor;

            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return IsAllowedToDeletePayor;

            if (AllowEdit)
            {
                if (SelectedDisplayPayor != null && SelectedDisplayPayor.IsGlobal == false)
                    IsAllowedToDeletePayor = true;
            }
            return IsAllowedToDeletePayor;
        }

        private void OnDeletePayor()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                if (SelectedDisplayPayor != null)
                {
                    if (SelectedDisplayPayor.IsGlobal == true)
                        return;

                    if (SelectedDisplayPayor.PayorID != Guid.Empty)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            ReturnStatus status = serviceClients.PayorClient.DeletePayor(SelectedDisplayPayor.PayorID);

                            if (status.IsError)
                                MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            Payors.Remove(Payors.FirstOrDefault(s => s.PayorID == SelectedDisplayPayor.PayorID));
                            FilterDisplayedPayors(true);
                        }
                    }
                    else
                    {
                        Payors.Remove(Payors.FirstOrDefault(s => s.PayorID == SelectedDisplayPayor.PayorID));
                        FilterDisplayedPayors(true);
                    }
                }
            }
        }

        private bool BeforeOnNewPayor()
        {
            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return false;

            return true;
        }

        private void OnNewPayor()
        {
            if (SelectedRegion.RegionId != 7)
            {
                SelectedDisplayPayor = new SettingDisplayedPayor { PayorID = Guid.Empty, RegionID = SelectedRegion.RegionId, IsGlobal = false, SourceType = 0 };
            }
            else
            {
                if (SelectedPayor != null && SelectedDisplayPayor != null)
                {
                    SelectedDisplayPayor = new SettingDisplayedPayor { PayorID = Guid.Empty, RegionID = SelectedDisplayPayor.RegionID, IsGlobal = false, SourceType = 0 };
                }
                else
                {
                    SelectedDisplayPayor = new SettingDisplayedPayor { PayorID = Guid.Empty, RegionID = 0, IsGlobal = false, SourceType = 0 };
                }
            }

            IsSingleCarrierEnabled = true;
            //Clear Notes fields when add a new payor
            RTFNotecontent = string.Empty;

        }

        private void EnableDisableWebsite()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                bool retVal = false;
                if (SelectedDisplayPayor != null)
                {
                    PayorDefaults payorDef = null;
                    if (SelectedDisplayPayor.IsGlobal)
                    {
                        payorDef = serviceClients.PayorDefaultsClient.GetPayorDefaultBy(SelectedDisplayPayor.PayorID);
                        if (payorDef != null && payorDef.WebSiteUrl != null && payorDef.WebSiteUrl.Trim().Length != 0)
                            retVal = true;
                    }
                }
                WebsitePayorRadioButtonEnable = retVal;
            }
        }
        #endregion
        #endregion

        #region WebSite

        private PayorSiteLoginInfo _currentSite;
        public PayorSiteLoginInfo CurrentSite
        {
            get
            {

                if (_currentSite != null)
                    return _currentSite;
                return null;
            }
            set
            {
                if (value == null)
                {
                    _currentSite = new PayorSiteLoginInfo();
                    _currentSite.SiteID = Guid.NewGuid();
                }
                else
                {

                    _currentSite = value;
                }
                OnPropertyChanged("CurrentSite");
            }
        }

        private ICommand _saveWebsite;
        public ICommand SaveWebSite
        {
            get
            {

                if (_saveWebsite == null)
                    _saveWebsite = new BaseCommand(Param => BeforeInsertOrUpdatepayorWebSite(), Param => InsertOrUpdatepayorWebSite());
                return _saveWebsite;
            }
        }

        private bool BeforeInsertOrUpdatepayorWebSite()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand _RemoveWebSite;
        public ICommand RemoveWebSite
        {
            get
            {
                if (_RemoveWebSite == null)
                    _RemoveWebSite = new BaseCommand(param => BeforeRemovePayorWebSite(), param => RemovePayorWebSite());
                return _RemoveWebSite;
            }
        }

        private bool BeforeRemovePayorWebSite()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand _NewWebSite;
        public ICommand NewWebSite
        {
            get
            {
                if (_NewWebSite == null)
                    _NewWebSite = new BaseCommand(param => BeforeNewPayorWebSite(), param => NewPayorWebSite());
                return _NewWebSite;

            }
        }

        private bool BeforeNewPayorWebSite()
        {
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void InsertOrUpdatepayorWebSite()
        {
            try
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    if (CurrentSite.LogInName == null || CurrentSite.Password == null)
                        return;

                    CurrentSite.PayorID = SelectedDisplayPayor.PayorID;
                    CurrentSite.LicenseID = SharedVMData.SelectedLicensee.LicenseeId;
                    if (CurrentSite.PayorID == Guid.Empty || CurrentSite.LicenseID == Guid.Empty)
                    {
                        throw new Exception();
                    }
                    {
                        serviceClients.PayorUserWebSiteClient.AddUpdatePayorUserWebSite(CurrentSite);
                    }
                    if (displayedLogins == null)
                        DisplayedLogins = new ObservableCollection<PayorSiteLoginInfo>();

                    DisplayedLogins.Add(CurrentSite);
                }
            }
            catch
            {
                MessageBoxResult _MessageBoxResult = MessageBox.Show("Licensee/Payor does not exist in the system", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemovePayorWebSite()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                PayorUserWebSiteClient Client = serviceClients.PayorUserWebSiteClient;
                Client.DeletePayorUserWebSite(CurrentSite);
                if (CurrentSite != null)
                {
                    DisplayedLogins.Remove(CurrentSite);
                }
            }
        }

        private void NewPayorWebSite()
        {
            CurrentSite = new PayorSiteLoginInfo();
            CurrentSite.SiteID = Guid.NewGuid();
            OnPropertyChanged("PayorWebsites");
        }

        #endregion

        #region"Broker functionality"

        private string _txtbrokerCode;
        public string txtbrokerCode
        {
            get
            {
                return _txtbrokerCode;
            }
            set
            {
                _txtbrokerCode = value;
                OnPropertyChanged("txtbrokerCode");
            }
        }

        private string _btnAddNew;
        public string btnAddNew
        {
            get
            {
                return _btnAddNew;
            }
            set
            {
                _btnAddNew = value;
                OnPropertyChanged("btnAddNew");
            }
        }

        private string _btnSaveUpdate;
        public string btnSaveUpdate
        {
            get
            {
                return _btnSaveUpdate;
            }
            set
            {
                _btnSaveUpdate = value;
                OnPropertyChanged("btnSaveUpdate");
            }
        }

        private ICommand _AddNewBrokerCode;
        public ICommand AddNewBrokerCode
        {
            get
            {
                if (_AddNewBrokerCode == null)
                {
                    _AddNewBrokerCode = new BaseCommand(param => BeforeAddBrokerCode(), param => OnAddBrokerCode());
                }
                return _AddNewBrokerCode;
            }

        }

        private bool BeforeAddBrokerCode()
        {
            bool bValue = true;
            //if (!string.IsNullOrEmpty(txtbrokerCode))
            //{
            //    bValue = true;
            //}
            //else
            //{
            //    bValue = false;
            //}
            return bValue;

        }
        private void OnAddBrokerCode()
        {
            try
            {

                //if (ValidationObjectCode())
                //{
                //    if (!string.IsNullOrEmpty(CurrentDisplayBrokerCode.Code))
                //    {
                //        BrokercodeDisplayBrokerCode objDisplayBrokerCode = new BrokercodeDisplayBrokerCode();
                //        objDisplayBrokerCode.Code = CurrentDisplayBrokerCode.Code;
                //        //licensee ID
                //        objDisplayBrokerCode.licenseeID = SharedVMData.SelectedLicensee.LicenseeId;
                //        //Payor Id
                //        objDisplayBrokerCode.payorID = SelectedDisplayPayor.PayorID;

                //        using (ServiceClients serviceClients = new ServiceClients())
                //        {
                //            serviceClients.BrokerCodeClient.AddBrokerCode(objDisplayBrokerCode);
                //        }

                //        LoadBrokerCode();
                //    }

                //}
                //else
                //{
                //    MessageBox.Show("Broker code already Present in another agency with same name");
                //}

                if (btnAddNew == "New")
                {
                    btnAddNew = "Cancel";
                    btnSaveUpdate = "Save";

                    CurrentDisplayBrokerCode = new DisplayBrokerCode();
                    CurrentDisplayBrokerCode.Code = string.Empty;
                    CurrentDisplayBrokerCode.licenseeID = SharedVMData.SelectedLicensee.LicenseeId;
                    CurrentDisplayBrokerCode.payorID = SelectedDisplayPayor.PayorID;

                    BrokercodeDisplayBrokerCode.Insert(0, CurrentDisplayBrokerCode);

                    CurrentDisplayBrokerCode = BrokercodeDisplayBrokerCode.FirstOrDefault();
                }
                else
                {
                    if (btnAddNew == "Cancel")
                    {
                        btnAddNew = "New";
                        btnSaveUpdate = "Update";
                        LoadBrokerCode();
                    }
                }
            }
            catch
            {
            }
        }

        private ICommand _UpdateBrokerCode;
        public ICommand UpdateBrokerCode
        {
            get
            {
                if (_UpdateBrokerCode == null)
                {
                    _UpdateBrokerCode = new BaseCommand(p => BeforeUpdatebrokerCode(), p => OnSaveUpdateBrokerCode());
                }
                return _UpdateBrokerCode;
            }
        }

        private bool BeforeUpdatebrokerCode()
        {
            bool bValue = false;
            if (CurrentDisplayBrokerCode != null)
            {
                if (CurrentDisplayBrokerCode.Code != null)
                {
                    bValue = true;
                }
            }
            else
            {
                bValue = false;
            }
            return bValue;
        }
        private void OnSaveUpdateBrokerCode()
        {
            try
            {
                if (ValidationObjectCode())
                {
                    SaveBrokerCode();
                }
                else
                {
                    using (ServiceClients serviceClients = new ServiceClients())
                    {
                        MailData objMailData = new MailData();
                        objMailData.FromMail = "service@commissionsdept.com";
                        objMailData.ToMail = "service@commissionsdept.com";
                        string strSubject = "ALERT:Multiple Broker Codes Entered";
                        string strMailBody = MailBody(Convert.ToString(SelectedDisplayPayor.PayorName), Convert.ToString(CurrentDisplayBrokerCode.Code), Convert.ToString(SharedVMData.SelectedLicensee.Company));
                        serviceClients.BrokerCodeClient.NotifyMailAsync(objMailData, strSubject, strMailBody);
                    }
                }
                SaveBrokerCode();
            }
            catch
            {
            }
        }

        private void SaveBrokerCode()
        {
            if (!string.IsNullOrEmpty(CurrentDisplayBrokerCode.Code))
            {
                DisplayBrokerCode objDisplayBrokerCode = new DisplayBrokerCode();
                objDisplayBrokerCode.Id = CurrentDisplayBrokerCode.Id;
                objDisplayBrokerCode.Code = CurrentDisplayBrokerCode.Code;
                //licensee ID
                objDisplayBrokerCode.licenseeID = SharedVMData.SelectedLicensee.LicenseeId;
                //Payor Id
                objDisplayBrokerCode.payorID = SelectedDisplayPayor.PayorID;

                using (ServiceClients serviceClients = new ServiceClients())
                {
                    if (btnSaveUpdate == "Update")
                    {
                        serviceClients.BrokerCodeClient.UpdateBrokerCode(CurrentDisplayBrokerCode);
                    }
                    else
                    {
                        serviceClients.BrokerCodeClient.AddBrokerCode(CurrentDisplayBrokerCode);
                        btnAddNew = "New";
                        btnSaveUpdate = "Update";
                    }
                }

                LoadBrokerCode();
            }
        }

        private ICommand _DeleteBrokerCode;
        public ICommand DeleteBrokerCode
        {
            get
            {
                if (_DeleteBrokerCode == null)
                {
                    _DeleteBrokerCode = new BaseCommand(p => BeforeDeleteBrokerCode(), p => OnDeleteBrokerCode());
                }
                return _DeleteBrokerCode;
            }
        }

        private bool BeforeDeleteBrokerCode()
        {
            bool bValue = false;
            if (CurrentDisplayBrokerCode != null)
            {
                if (CurrentDisplayBrokerCode.Code != null)
                {
                    bValue = true;
                }
            }
            else
            {
                bValue = false;
            }
            return bValue;

        }

        private void OnDeleteBrokerCode()
        {
            bool bValue = false;
            string strcode = string.Empty;
            using (ServiceClients serviceClients = new ServiceClients())
            {
                if (CurrentDisplayBrokerCode != null)
                {
                    strcode = CurrentDisplayBrokerCode.Code;
                    bValue = serviceClients.BrokerCodeClient.DeleteBrokerCode(CurrentDisplayBrokerCode);
                }
            }

            if (bValue)
            {
                MessageBox.Show("Broker code: " + strcode + " deleted successfully", "MyAgencyVault", MessageBoxButton.OK);
                LoadBrokerCode();
            }

        }

        private ObservableCollection<DisplayBrokerCode> _BrokercodeDisplayBrokerCode;
        public ObservableCollection<DisplayBrokerCode> BrokercodeDisplayBrokerCode
        {
            get
            {
                return _BrokercodeDisplayBrokerCode;
            }
            set
            {
                _BrokercodeDisplayBrokerCode = value;
                OnPropertyChanged("BrokercodeDisplayBrokerCode");
            }
        }

        private DisplayBrokerCode _currentDisplayBrokerCode;
        public DisplayBrokerCode CurrentDisplayBrokerCode
        {
            get
            {

                if (_currentDisplayBrokerCode != null)
                    return _currentDisplayBrokerCode;
                return null;
            }
            set
            {
                _currentDisplayBrokerCode = value;
                OnPropertyChanged("CurrentDisplayBrokerCode");
            }
        }


        private void LoadBrokerCode()
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                BrokercodeDisplayBrokerCode = new ObservableCollection<DisplayBrokerCode>(serviceClients.BrokerCodeClient.LoadBrokerCode(SharedVMData.SelectedLicensee.LicenseeId));
            }

            if (BrokercodeDisplayBrokerCode.Count > 1)
            {
                CurrentDisplayBrokerCode = BrokercodeDisplayBrokerCode.FirstOrDefault();
            }
        }

        private bool ValidationObjectCode()
        {
            bool bValue = false;
            try
            {
                if (CurrentDisplayBrokerCode != null)
                {
                    if (CurrentDisplayBrokerCode.Code != null)
                    {
                        using (ServiceClients serviceClients = new ServiceClients())
                        {
                            bValue = serviceClients.BrokerCodeClient.ValidateBrokerCode(CurrentDisplayBrokerCode.Code);
                        }
                    }
                }
            }
            catch
            {
            }
            return bValue;

        }

        private ICommand _BrokerCodeFocus;
        public ICommand BrokerCodeFocus
        {

            get
            {
                if (_BrokerCodeFocus == null)
                {
                    _BrokerCodeFocus = new BaseCommand(x => CorrectBrokerCode());
                }
                return _BrokerCodeFocus;
            }
        }

        private void CorrectBrokerCode()
        {
            if (CurrentDisplayBrokerCode == null || CurrentDisplayBrokerCode.Code == null) return;
            CurrentDisplayBrokerCode.Code = MyAgencyVault.ViewModel.VMHelper.CorrectBrokerCode(CurrentDisplayBrokerCode.Code);
        }

        private string MailBody(string strPayorName, string strCode, string strAgency)
        {
            string MailBody = string.Empty;
            try
            {
                MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                           "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'>" +
                           "<tr><td colspan='2'>" +
                           "</td></tr><tr><td colspan='2'></td></tr><tr><td colspan='2'>" +
                           "</td></tr><tr><td colspan='2'>Payor: " +
                           strPayorName +
                           "</td></tr><tr><td colspan='2'>Code: " +
                           strCode +
                           "</td></tr><tr><td colspan='2'>Agency: " +
                           strAgency +
                           "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'></td></tr><tr><td colspan='2'>&nbsp;</td></tr>" + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>" +
                           "</span></td></tr><tr><td colspan='2'>" + "</span></td></tr><tr>" + "<td colspan='2'>" +
                           "</span></td></tr><tr><td colspan='2'" + "</span></td></tr><tr><td colspan='2'>" +
                           "<br/><tr><td colspan='2'>" + "</td></tr><tr><td colspan='2'>" +
                           "</td></tr></table>";
            }
            catch
            {
            }

            return MailBody;
        }

        #endregion

        public void Refresh()
        {
            LoadData();
        }

        private bool _RefreshRequired;
        public bool RefreshRequired
        {
            get
            {
                return _RefreshRequired;
            }
            set
            {
                _RefreshRequired = value;
            }
        }


        #endregion
    }

    #region Validations

    public class UserRegion : Region
    {
        public UserRegion()
        {
        }

        public UserRegion(Region reg)
        {
            RegionId = reg.RegionId;
            RegionName = reg.RegionName;
        }

        public static UserRegion getUserRegion(Region reg)
        {
            UserRegion userRegion = new UserRegion();
            userRegion.RegionId = reg.RegionId;
            userRegion.RegionName = reg.RegionName;
            return userRegion;
        }

        public static ObservableCollection<UserRegion> getUserRegions(ObservableCollection<Region> regions)
        {
            ObservableCollection<UserRegion> userRegions = null;
            if (regions != null && regions.Count != 0)
            {
                userRegions = new ObservableCollection<UserRegion>();
                foreach (Region reg in regions)
                    userRegions.Add(getUserRegion(reg));
            }
            return userRegions;
        }

        public override string ToString()
        {
            return this.RegionName;
        }
    }

    public class PayorSiteLoginInfoValidate : PayorSiteLoginInfo, IDataErrorInfo
    {
        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();
                ValidationResults results = Validation.ValidateFromConfiguration<PayorSiteLoginInfoValidate>(this, "PayorSiteLoginInfoRule");
                foreach (ValidationResult result in results)
                {
                    error.AppendLine(result.Message);
                }

                return error.ToString();
            }
        }
        public string this[string columnName]
        {
            get
            {
                ValidationResults results = Validation.ValidateFromConfiguration<PayorSiteLoginInfoValidate>(this, "PayorSiteLoginInfoRule");
                foreach (ValidationResult result in results)
                {
                    if (result.Key == columnName)
                    {
                        return result.Message;
                    }
                }

                return string.Empty;
            }
        }

        #endregion
    }

    #endregion

}
