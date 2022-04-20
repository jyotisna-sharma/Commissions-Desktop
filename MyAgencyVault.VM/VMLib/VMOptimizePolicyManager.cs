using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VMLib.PolicyManager;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel;
using System.ComponentModel;
using MyAgencyVault.ViewModel.VMLib;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using MyAgencyVault.VM.ClientProxy;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using MyAgencyVault.VM.CommonItems;
using Excel;



//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;

namespace MyAgencyVault.VMLib
{
    public class VMOptimizePolicyManager : BaseViewModel, IDataRefresh, INotifyPropertyChanged
    {
        private bool FirstTimeLoading = false;
        //private bool isBlankClientAdded = false;
        DateTime? policyLearnFields = null;
        private ObservableCollection<User> UsrLst;
        private List<LinkedUser> objList = new List<LinkedUser>();
        private ServiceClients _ServiceClients;
        static MastersClient objLog = new MastersClient();
        static string importStatusMsg = string.Empty;
        string previousPolicyType = "";
        string newPolicyType = "";
        bool isFirstTime = false;

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

        #region VMInstance

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }

        bool isCustomDateSelected = true;

        public bool IsCustomDateSelected
        {
            get { return isCustomDateSelected; }
            set
            {
                isCustomDateSelected = value;
                OnPropertyChanged("IsCustomDateSelected");
            }
        }

        bool isTiered = false;

        public bool IsTiered
        {
            get { return isTiered; }
            set
            {
                isTiered = value;
                OnPropertyChanged("IsTiered");
            }
        }

        bool _isTrackPayment = true;
        public bool IsTrackPayment
        {
            get { return _isTrackPayment; }
            set
            {
                _isTrackPayment = value;
                OnPropertyChanged("IsTrackPayment");
            }
        }

        private VMPolicySchedule _VMPolicySchedule;
        public VMPolicySchedule VMPolicySchedule
        {
            get
            {
                return _VMPolicySchedule;
            }
            set
            {
                _VMPolicySchedule = value;
                OnPropertyChanged("VMPolicySchedule");
            }
        }

        private VMPolicySmartField _VMPolicySmartField;
        public VMPolicySmartField VMPolicySmartField
        {
            get
            {
                return _VMPolicySmartField;
            }
            set
            {
                _VMPolicySmartField = value;
                OnPropertyChanged("VMPolicySmartField");
            }
        }

        private VMPolicyNote _VMPolicyNote;
        public VMPolicyNote VMPolicyNote
        {
            get
            {
                return _VMPolicyNote;
            }
            set
            {
                _VMPolicyNote = value;
                OnPropertyChanged("VMPolicyNote");
            }
        }

        private VMPolicyCommission _VMPolicyCommission;
        public VMPolicyCommission VMPolicyCommission
        {
            get
            {
                return _VMPolicyCommission;
            }
            set
            {
                _VMPolicyCommission = value;
                OnPropertyChanged("VMPolicyCommission");
            }
        }

        public PolicyClientVm VMPolicyClient { get; set; }

        #endregion

        #region Client

        private Client selectedDisplayClient;
        public Client SelectedDisplayClient
        {
            get
            {
                return selectedDisplayClient == null ? new Client() : selectedDisplayClient;
            }
            set
            {
                selectedDisplayClient = value;
                OnPropertyChanged("SelectedDisplayClient");
            }
        }

        private Client selectedDisplayClientTemp;
        public Client SelectedDisplayClientTemp
        {
            get
            {
                return selectedDisplayClientTemp == null ? new Client() : selectedDisplayClientTemp;
            }
            set
            {
                selectedDisplayClientTemp = value;
                OnPropertyChanged("SelectedDisplayClientTemp");
            }
        }


        public ObservableCollection<Client> displayedClientsLists;
        public ObservableCollection<Client> DisplayedClientsLists
        {
            get
            {
                return displayedClientsLists;
            }
            set
            {
                displayedClientsLists = value;
                OnPropertyChanged("DisplayedClientsLists");

            }
        }

        public ObservableCollection<Client> displayedClientsListsTemp;
        public ObservableCollection<Client> DisplayedClientsListsTemp
        {
            get
            {
                return displayedClientsListsTemp;
            }
            set
            {
                displayedClientsListsTemp = value;
                OnPropertyChanged("DisplayedClientsListsTemp");

            }
        }


        private Client selectedClient;
        public Client SelectedClient
        {
            get { return selectedClient == null ? new Client() : selectedClient; }
            set { selectedClient = value; OnPropertyChanged("SelectedClient"); }
        }

        private bool _SelectedPolicyIsNotNull;
        public bool SelectedPolicyIsNotNull
        {
            get { return _SelectedPolicyIsNotNull; }
            set
            {
                _SelectedPolicyIsNotNull = value;
                OnPropertyChanged("SelectedPolicyIsNotNull");
            }
        }
        //"Enable and disable policy term date according change status of policy"
        //enable And disble combobox
        private bool _EnabledPolicyTermDate;
        public bool EnabledPolicyTermDate
        {
            get
            {
                return _EnabledPolicyTermDate;
            }
            set
            {
                _EnabledPolicyTermDate = value;
                OnPropertyChanged("EnabledPolicyTermDate");
            }
        }

        private ICommand _StatusSelectionChanged;
        public ICommand StatusSelectionChanged
        {
            get
            {
                if (_StatusSelectionChanged == null)
                {
                    _StatusSelectionChanged = new BaseCommand(x => ChangeStatus());
                }
                return _StatusSelectionChanged;
            }
        }

        public void ChangeStatus()
        {
            if (SelectedStatus != null && SelectedStatus.Status != null)
            {

                if (SelectedStatus.Status.ToLower() == "terminated")
                {
                    if (SelectedPolicy != null)
                    {
                        if (SelectedPolicy.TerminationReasonId != null)
                        {
                            SelectedTermReason = PolicyTerminationtReasonLst.Where(p => p.TerminationReasonId == SelectedPolicy.TerminationReasonId).FirstOrDefault();
                        }
                        else
                        {
                            //SelectedTermReason = PolicyTerminationtReasonLst.FirstOrDefault();
                            //Update code
                            SelectedTermReason = PolicyTerminationtReasonLst.LastOrDefault();
                        }
                    }
                    EnabledPolicyTermDate = true;
                }
                else
                {
                    SelectedTermReason = PolicyTerminationtReasonLst.LastOrDefault();
                    EnabledPolicyTermDate = false;
                }
            }
        }
        #endregion

        #region Delegate & Events

        public delegate void SelectedPolicyChangedEventHandler(PolicyDetailsData SelectedPolicy);
        public event SelectedPolicyChangedEventHandler SelectedPolicyChanged;

        public delegate void SelectedLicenseeChangedEventHandler(LicenseeDisplayData SelectedLicensee, ObservableCollection<Client> Clients);
        public event SelectedLicenseeChangedEventHandler SelectedLicenseeChanged;

        public delegate void SelectedClientChangedEventHandler(Client SelectedClient);
        public event SelectedClientChangedEventHandler SelectedClientChanged;

        public delegate void SelectedPayorChangedEventHandler(DisplayedPayor SelectedPayor);
        public event SelectedPayorChangedEventHandler SelectedPayorChanged;

        public delegate void SelectedCarrierChangedEventHandler(Carrier SelectedCarrier);
        public event SelectedCarrierChangedEventHandler SelectedCarrierChanged;

        public delegate void SelectedCoverageChangedEventHandler(DisplayedCoverage SelectedCoverage);
        public event SelectedCoverageChangedEventHandler SelectedCoverageChanged;

        public delegate void SelectedPolicySavedEventHandler();
        public event SelectedPolicySavedEventHandler SelectedPolicySaved;

        //public delegate void SelectedPolicyDetailSmartIssueHandler(Guid PolicyId);
        //public event SelectedPolicyDetailSmartIssueHandler SelectedPolicyDetailSmartIssueChanged;

        #endregion

        #region Licensees

        private int _SelectedPageIndex;
        public int SelectedPageIndex
        {
            get { return _SelectedPageIndex; }
            set { _SelectedPageIndex = value; OnPropertyChanged("SelectedPageIndex"); }
        }

        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
                if (_SharedVMData == null)
                {
                    _SharedVMData = VMSharedData.getInstance();

                    if (_SharedVMData.SelectedLicensee != null)
                    {
                        OnSelectedLicenseeChanged(null, false);
                    }
                }

                return _SharedVMData;
            }
        }

        public void OnSelectedLicenseeChanged(Guid? ClientId, bool isDataRefreshing, bool isFromImportProcess = false)
        {
            bool isScheduleExist = false;
            isScheduleExist = serviceClients.SettingsClient.CheckNamedscheduleExist(SharedVMData.SelectedLicensee.LicenseeId);
            ShowHideCheckSchedulebtn = DesignerSerializationVisibility.Hidden.ToString();
            if (isScheduleExist)
            {
                ShowHideCheckSchedulebtn = DesignerSerializationVisibility.Visible.ToString();
            }
            if (!isFromImportProcess)
            {
                Mouse.OverrideCursor = Cursors.Wait;
            }
            try
            {
                //GlobalData.PolicyManagerSelectedLicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                //Load All product 
                //vinod
                //Added new code
                if (SharedVMData.SelectedLicensee.IsClientEnable == null)
                {
                    IsClientEnable = false;
                    SharedVMData.SelectedLicensee.IsClientEnable = false;
                }
                if (SharedVMData.SelectedLicensee.IsClientEnable == false)
                {
                    IsClientEnable = true;
                }
                else
                {
                    IsClientEnable = false;
                }

                //AllProducts = new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetDisplayedCarrierCoverages(Guid.Empty).ToList());
                //if (SharedVMData.SelectedLicensee == null)
                //    return;

                if (ClientId == null || ClientId == new Guid())
                    PolicyList = null;


                if (SharedVMData.SelectedLicensee == null)
                    return;

                if (SharedVMData.SelectedLicensee.LicenseeId == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                {
                    AllProducts = new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetDisplayedCarrierCoverages(Guid.Empty).ToList());
                }
                else
                {
                    AllProducts = new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetDisplayedCarrierCoverages((Guid)SharedVMData.SelectedLicensee.LicenseeId).ToList());
                }

                FillPayorLst();

                UpdateClientListLicenseeWise(true, ClientId, isDataRefreshing);

                OutgoingPayeeList = FillOutgoingPayeeUser();
                PrimaryAgents = serviceClients.UserClient.GetAllPayeeByLicencessID((Guid)SharedVMData.SelectedLicensee.LicenseeId);

                //  if (PrimaryAgents != null) ACME - remove default selection and set for policy
                //   SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault();
                if (PrimaryAgents != null)
                {
                    PrimaryAgents = new ObservableCollection<User>(PrimaryAgents.Where(x => x.Role == UserRole.Agent));

                    if (SelectedPolicy != null && SelectedPolicy.PrimaryAgent != null)
                        SelectedPrimaryAgent = PrimaryAgents.Where(d => d.UserCredentialID == SelectedPolicy.PrimaryAgent).FirstOrDefault();
                }


                if (SelectedLicenseeChanged != null)
                    SelectedLicenseeChanged(SharedVMData.SelectedLicensee, DisplayedClientsLists);

            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private bool _isNotSaveLastViewedClientPolicy;

        private void UpdateClientListLicenseeWise(bool isLicenseeChanging, Guid? ClientId, bool isDataRefreshing)
        {
            try
            {
                Guid? LicenseeId = null;
                _isNotSaveLastViewedClientPolicy = isLicenseeChanging;

                if (SharedVMData.SelectedLicensee.LicenseeId == null)
                {
                    LicenseeId = Guid.Empty;
                }
                else
                {
                    LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                }
                if (SharedVMData.SelectedLicensee.IsClientEnable == null)
                {
                    SharedVMData.SelectedLicensee.IsClientEnable = false;
                }

                if (SharedVMData.SelectedLicensee.IsClientEnable == false)
                {
                    PolicyAddPayeeToolTip = null;
                    bool isAgencyVersionLicense = serviceClients.BillingLineDetailClient.IsAgencyVersionLicense(SharedVMData.SelectedLicensee.LicenseeId);
                    if (!isAgencyVersionLicense)
                    {
                        AddPaeeButton = false;
                        PolicyAddPayeeToolTip = "Contact commission department to activate Agency Version.";
                    }
                    else
                    {
                        AddPaeeButton = true;
                    }

                    if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                    {
                        PolicyAddPayeeToolTip = "Permission Violation";
                    }
                    if (SharedVMData.CachedClientList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        DisplayedClientsLists = new ObservableCollection<Client>(SharedVMData.CachedClientList[SharedVMData.SelectedLicensee.LicenseeId].OrderBy(or => or.Name));
                    }
                    else
                    {
                        DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(LicenseeId).OrderBy(o => o.Name).ToList());
                        SharedVMData.CachedClientList.Add(SharedVMData.SelectedLicensee.LicenseeId, DisplayedClientsLists);
                    }
                    //Null  client is not avilable
                    DisplayedClientsLists = new ObservableCollection<Client>(DisplayedClientsLists.Where(p => !string.IsNullOrEmpty(p.Name)));

                    //Add Blank client 
                    //vinod               
                    //Client objBlank = new Client();
                    //objBlank.ClientId = Guid.Empty;
                    //objBlank.Name = "-Select client-";                
                    //DisplayedClientsLists.Insert(0, objBlank);
                    //vinod
                    Guid? SelectedDisplaytempClientId = null;
                    Client tempclient = DisplayedClientsLists.FirstOrDefault();
                    if (tempclient != null)
                    {
                        SelectedDisplaytempClientId = tempclient.ClientId;
                    }
                    //SelectedClientPolicyList.ClientId = SelectedDisplayClient.ClientId;

                    if (LicenseeId != Guid.Empty)
                    {
                        try
                        {

                            AccountExecLst = serviceClients.UserClient.GetAccountExecByLicencessID((Guid)LicenseeId);
                            if (AccountExecLst != null)
                            {
                                if (AccountExecLst.Count > 0)
                                {
                                    User objBalnk = new User();
                                    int intAxec = AccountExecLst.Count;
                                    AccountExecLst.Insert(intAxec, objBalnk);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    if (FirstTimeLoading)
                    {
                        FirstTimeLoading = false;
                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                    }
                    else
                    {
                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                    }
                }
                else
                {
                    BackgroundWorker workerClient = new BackgroundWorker();
                    workerClient.DoWork += new System.ComponentModel.DoWorkEventHandler(workerClient_DoWork);
                    workerClient.RunWorkerAsync();

                    DisplayedClientsLists = new ObservableCollection<Client>();
                }
            }
            catch
            {
            }
        }

        void workerClient_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                DisplayedClientsListsTemp = new ObservableCollection<Client>();
                int intCount = serviceClients.ClientClient.GetAllClientCount();
                int takeRecordCount = 100;
                int asyncCallingCount = Convert.ToInt32(Math.Ceiling(intCount / Convert.ToDouble(takeRecordCount)));
                int remainingRecords = intCount;
                int skipRecordCount = 0;

                for (int count = 0; count < asyncCallingCount; count++)
                {
                    ObservableCollection<Client> tempAgentList = new ObservableCollection<Client>();
                    if (remainingRecords < takeRecordCount)
                    {
                        tempAgentList = new ObservableCollection<Client>(serviceClients.ClientClient.GetAllClientByLicChunck(SharedVMData.SelectedLicensee.LicenseeId, skipRecordCount, takeRecordCount).ToList());
                        DisplayedClientsListsTemp.AddRange(tempAgentList);
                    }
                    else
                    {
                        tempAgentList = new ObservableCollection<Client>(serviceClients.ClientClient.GetAllClientByLicChunck(SharedVMData.SelectedLicensee.LicenseeId, skipRecordCount, takeRecordCount).ToList());
                        DisplayedClientsListsTemp.AddRange(tempAgentList);
                    }

                    remainingRecords -= takeRecordCount;
                    skipRecordCount += takeRecordCount;

                }
                //SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllUsers());
            }
            catch
            {
            }
        }

        public void PolicyClient_GetPolicydataCompleted(object sender, GetPolicydataCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                    SelectedClientPolicyList.PolicyList = e.Result;
                    SelectedClientPolicyList.ClientId = SelectedDisplayClient.ClientId;
                    FirstTimeLoading = false;
                }
            }
            catch
            {
            }
        }

        private void FillPayorLst()
        {
            try
            {
                string str = null;
                if (SelectedPolicy != null)
                {
                    str = SelectedPolicy.SubmittedThrough;
                }
                if (SharedVMData.CachedPayorLists.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                {
                    if (_SharedVMData.IsLoadPayor)
                    {
                        PayorFillInfo PayorFillInfo = new VM.MyAgencyVaultSvc.PayorFillInfo { IsCarriersRequired = false, IsContactsRequired = false, IsWebsiteLoginsRequired = false, IsCoveragesRequired = false, PayorStatus = PayorStatus.Active };
                        int payorCount = _PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorCount(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo);
                        int takeRecordCount = 100;
                        int asyncCallingCount = Convert.ToInt32(Math.Ceiling(payorCount / Convert.ToDouble(takeRecordCount)));
                        int remainingRecords = payorCount;
                        int skipRecordCount = 0;

                        if (PayorsLst != null)
                        {
                            PayorsLst.Clear();

                        }
                        else
                            PayorsLst = new ObservableCollection<DisplayedPayor>();

                        for (int count = 0; count < asyncCallingCount; count++)
                        {

                            if (remainingRecords < takeRecordCount)
                                PayorsLst.AddRange(_PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorsInChunk(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo, skipRecordCount, remainingRecords));
                            else
                                PayorsLst.AddRange(_PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorsInChunk(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo, skipRecordCount, takeRecordCount));

                            remainingRecords -= takeRecordCount;
                            skipRecordCount += takeRecordCount;
                        }
                        //to sort Payor in asc order
                        PayorsLst = new ObservableCollection<DisplayedPayor>(PayorsLst.OrderBy(s => s.PayorName).ToList());

                        SharedVMData.CachedPayorLists.Add(SharedVMData.SelectedLicensee.LicenseeId, payorslst);
                        if (PayorsLst != null && SelectedPolicy != null)
                        {
                            PayorsLst.Add(new DisplayedPayor()
                            {
                                PayorName = "",
                                PayorID = Guid.Empty,
                            });
                            FillSubmitedThrough(PayorsLst);

                            DisplayedPayor policyPayor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                            if (policyPayor != null && policyPayor.PayorID != Guid.Empty)
                            {
                                if (SelectedPolicy.Payor == null || SelectedPolicy.Payor.PayorID != policyPayor.PayorID)
                                    SelectedPolicy.Payor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                            }
                        }

                        if (SelectedPolicy != null)
                        {
                            SelectedPolicy.SubmittedThrough = str;
                        }

                        _SharedVMData.IsLoadPayor = false;
                    }
                    else
                    {
                        payorslst = new ObservableCollection<DisplayedPayor>(SharedVMData.CachedPayorLists[SharedVMData.SelectedLicensee.LicenseeId]);
                        PayorsLst = new ObservableCollection<DisplayedPayor>(PayorsLst.OrderBy(s => s.PayorName).ToList());
                        foreach (DisplayedPayor disItem in PayorsLst)
                        {
                            if (disItem.PayorName == "--All payor--")
                            {
                                PayorsLst.Remove(disItem);
                            }
                        }
                    }
                }
                else
                {
                    PayorFillInfo PayorFillInfo = new VM.MyAgencyVaultSvc.PayorFillInfo { IsCarriersRequired = false, IsContactsRequired = false, IsWebsiteLoginsRequired = false, IsCoveragesRequired = false, PayorStatus = PayorStatus.Active };
                    int payorCount = _PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorCount(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo);
                    int takeRecordCount = 100;
                    int asyncCallingCount = Convert.ToInt32(Math.Ceiling(payorCount / Convert.ToDouble(takeRecordCount)));
                    int remainingRecords = payorCount;
                    int skipRecordCount = 0;

                    if (PayorsLst != null)
                    {
                        PayorsLst.Clear();

                    }
                    else
                        PayorsLst = new ObservableCollection<DisplayedPayor>();

                    for (int count = 0; count < asyncCallingCount; count++)
                    {

                        if (remainingRecords < takeRecordCount)
                            PayorsLst.AddRange(_PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorsInChunk(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo, skipRecordCount, remainingRecords));
                        else
                            PayorsLst.AddRange(_PolicyServiceClient.DisplayedPayorClient.GetDisplayPayorsInChunk(SharedVMData.SelectedLicensee.LicenseeId, PayorFillInfo, skipRecordCount, takeRecordCount));


                        remainingRecords -= takeRecordCount;
                        skipRecordCount += takeRecordCount;
                    }
                    //to sort Payor in asc order
                    PayorsLst = new ObservableCollection<DisplayedPayor>(PayorsLst.OrderBy(s => s.PayorName).ToList());
                    SharedVMData.CachedPayorLists.Add(SharedVMData.SelectedLicensee.LicenseeId, payorslst);
                }
                if (PayorsLst != null && SelectedPolicy != null)
                {
                    PayorsLst.Add(new DisplayedPayor()
                    {
                        PayorName = "",
                        PayorID = Guid.Empty,
                    });
                    FillSubmitedThrough(PayorsLst);

                    DisplayedPayor policyPayor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                    if (policyPayor != null && policyPayor.PayorID != Guid.Empty)
                    {
                        if (SelectedPolicy.Payor == null || SelectedPolicy.Payor.PayorID != policyPayor.PayorID)
                            SelectedPolicy.Payor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                    }

                }

                if (SelectedPolicy != null)
                {
                    SelectedPolicy.SubmittedThrough = str;
                }
            }
            catch
            {
            }

        }

        public void PayorClient_GetPayorsCompleted(object sender, GetDisplayPayorsInChunkCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        PayorsLst.AddRange(e.Result);
                        PayorsLst.Add(new DisplayedPayor()
                        {
                            PayorName = "",
                            PayorID = Guid.Empty,
                        });
                        FillSubmitedThrough(e.Result);

                        if (PayorsLst != null && SelectedPolicy != null)
                        {
                            DisplayedPayor policyPayor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                            if (policyPayor != null && policyPayor.PayorID != Guid.Empty)
                            {
                                if (SelectedPolicy.Payor == null || SelectedPolicy.Payor.PayorID != policyPayor.PayorID)
                                    SelectedPolicy.Payor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                            }

                        }
                    }));
                    SharedVMData.CachedPayorLists.Add(SharedVMData.SelectedLicensee.LicenseeId, PayorsLst);
                }
            }
            catch
            {
            }

        }

        private void FillSubmitedThrough(ObservableCollection<DisplayedPayor> PayorList)
        {
            try
            {
                string str = null;
                if (SelectedPolicy != null)
                {
                    str = SelectedPolicy.SubmittedThrough;
                }
                SubmittedThroughLst = new ObservableCollection<string>();

                ObservableCollection<Guid> PayorIds = new ObservableCollection<Guid>();

                foreach (var payor in PayorList)
                {
                    if (payor != null)
                    {
                        PayorIds.Add(payor.PayorID);
                    }
                }

                ObservableCollection<Guid> obCarrier = serviceClients.CarrierClient.PayorCarrierGlobal(PayorIds);
                foreach (Guid payorID in obCarrier)
                {
                    SubmittedThroughLst.Add(PayorList.Where(p => p.PayorID == payorID).FirstOrDefault().PayorName);
                }
                if (SelectedPolicy != null && SelectedPolicy.Payor != null)
                {
                    //if (SubmittedThroughLst.Where(p => p != SelectedPayor.PayorName).Count() == 0)
                    if (!submittedThroughLst.Contains(SelectedPolicy.Payor.PayorName))
                        SubmittedThroughLst.Add(SelectedPolicy.Payor.PayorName);
                }

                if (SelectedPolicy != null && string.IsNullOrEmpty(SelectedPolicy.SubmittedThrough))
                {
                    if (SubmittedThroughLst.Where(p => p != SelectedPolicy.SubmittedThrough).Count() == 0)
                        SubmittedThroughLst.Add(SelectedPolicy.SubmittedThrough);

                }

                SubmittedThroughLst = new ObservableCollection<string>(SubmittedThroughLst.Distinct().ToList());

                if (SelectedPolicy != null)
                    SelectedPolicy.SubmittedThrough = SubmittedThroughLst.Where(p => p == str).FirstOrDefault();

            }
            catch
            {
            }

        }

        //private ObservableCollection<User> FillOutgoingPayeeUser()
        //{
        //    ObservableCollection<User> AgentList = null;

        //    try
        //    {
        //        if (SharedVMData.SelectedLicensee != null)
        //            if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
        //            {
        //                AgentList = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);
        //            }
        //            else
        //            {
        //                AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent).OrderBy(p => p.NickName));
        //                SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);
        //            }
        //    }
        //    catch
        //    {
        //    }

        //    return AgentList;

        //}

        private ObservableCollection<User> FillOutgoingPayeeUser()
        {
            ObservableCollection<User> AgentList = null;

            try
            {
                if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                {
                    AgentList = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);
                }
                else
                {
                    AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent).OrderBy(p => p.NickName));

                    if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                    }
                    else
                    {
                        SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);
                    }
                }
            }
            catch
            {
            }

            return AgentList;

        }

        private bool addpayee = false;
        public bool AddPaeeButton
        {
            get { return addpayee; }
            set { addpayee = value; OnPropertyChanged("AddPaeeButton"); }
        }

        #endregion

        #region Constructor

        private string _showHideTypeOfPolicy;
        public string ShowHideTypeOfPolicy
        {
            get { return _showHideTypeOfPolicy; }
            set { _showHideTypeOfPolicy = value; OnPropertyChanged("ShowHideTypeOfPolicy"); }
        }

        //PAC calculation
        private string _strLableContent = string.Empty;
        public string strLableContent
        {
            get { return _strLableContent; }
            set { _strLableContent = value; OnPropertyChanged("strLableContent"); }
        }

        private bool isacticechecked;
        public bool IsActiceChecked
        {
            get { return isacticechecked; }
            set
            {
                isacticechecked = value;
                OnPropertyChanged("IsActiceChecked");

                if (value)
                {
                    OnPolicyStatusChanged("Active", value);
                }
            }
        }

        private bool isPendingChecked;
        public bool IsPendingChecked
        {
            get { return isPendingChecked; }
            set
            {
                isPendingChecked = value;
                OnPropertyChanged("IsPendingChecked");

                if (value)
                {
                    OnPolicyStatusChanged("Pending", value);
                }
            }
        }

        private bool isTerminatedCheck;
        public bool IsTerminatedCheck
        {
            get { return isTerminatedCheck; }
            set
            {
                isTerminatedCheck = value;
                OnPropertyChanged("IsTerminatedCheck");

                if (value)
                {
                    OnPolicyStatusChanged("Terminated", value);
                }
            }
        }

        private bool isAllCheck;
        public bool IsAllCheck
        {
            get { return isAllCheck; }
            set
            {
                isAllCheck = value;
                OnPropertyChanged("IsAllCheck");

                if (value)
                {
                    OnPolicyStatusChanged("All", value);
                }
            }
        }

        private void OnPolicyStatusChanged(string status, bool value)
        {
            try
            {
                switch (status)
                {
                    case "Active":
                        if (value)
                        {
                            IsPendingChecked = false;
                            IsTerminatedCheck = false;
                            isAllCheck = false;
                        }
                        break;
                    case "Pending":
                        if (value)
                        {
                            IsActiceChecked = false;
                            IsTerminatedCheck = false;
                            isAllCheck = false;
                        }
                        break;
                    case "Terminated":
                        if (value)
                        {
                            IsActiceChecked = false;
                            IsPendingChecked = false;
                            isAllCheck = false;
                        }
                        break;
                    case "All":
                        if (value)
                        {
                            IsActiceChecked = false;
                            IsPendingChecked = false;
                            IsTerminatedCheck = false;
                        }
                        break;
                }
            }
            catch
            {
            }
        }

        private _PolicyStatus _policystatus;
        public _PolicyStatus policyStatus
        {
            get
            {
                return _policystatus;
            }
            set
            {
                _policystatus = value;
                OnPropertyChanged("policyStatus");
            }
        }
        #region Ankit added - Custom Mode Variable intalization
        bool isCustomOptionSelected = true;

        public bool IsCustomOptionSelected
        {
            get { return isCustomOptionSelected; }
            set
            {
                isCustomOptionSelected = value;
                OnPropertyChanged("IsCustomOptionSelected");
            }
        }

        private ObservableCollection<string> incomingModes;
        public ObservableCollection<string> IncomingModes
        {
            get
            {
                return incomingModes;
            }
            set
            {
                incomingModes = value;
                OnPropertyChanged("IncomingModes");
            }

        }
        ICommand _addGradedRow;
        public ICommand AddGradedRow
        {
            get
            {

                if (_addGradedRow == null)
                    _addGradedRow = new BaseCommand(Param => AddNewCustomRow());
                return _addGradedRow;
            }
        }

        ICommand _saveCustomSchedule;
        public ICommand SaveCustomSchedule
        {
            get
            {

                if (_saveCustomSchedule == null)
                    _saveCustomSchedule = new BaseCommand(param => SaveCustomSchedules());
                return _saveCustomSchedule;
            }
        }


        ICommand _removeCustomRow;
        public ICommand RemoveCustomRow
        {
            get
            {

                if (_removeCustomRow == null)
                    _removeCustomRow = new BaseCommand(Param => BeforeDeleteCustomRow(), Param => DeleteCustomRow());
                return _removeCustomRow;
            }
        }

        private string selectedIncomingMode;
        public string SelectedIncomingMode
        {
            get
            {
                return selectedIncomingMode;
            }
            set
            {
                selectedIncomingMode = value;
                OnPropertyChanged("SelectedIncomingMode");
            }

        }
        private bool isGraded;
        public bool IsGraded
        {
            get { return isGraded; }
            set
            {
                isGraded = value;
                OnPropertyChanged("IsGraded");
            }
        }
        private bool isPercentOfPremiumEnabled;
        public bool IsPercentOfPremiumEnabled
        {
            get { return isPercentOfPremiumEnabled; }
            set
            {
                isPercentOfPremiumEnabled = value;
                OnPropertyChanged("IsPercentOfPremiumEnabled");
            }
        }
        private bool isNonGraded;
        public bool IsNonGraded
        {
            get { return isNonGraded; }
            set
            {
                isNonGraded = value;
                OnPropertyChanged("IsNonGraded");
            }
        }
        private string gradedScheduleHeader;
        public string GradedScheduleHeader
        {
            get
            {
                return gradedScheduleHeader;
            }
            set
            {
                gradedScheduleHeader = value;
                OnPropertyChanged("GradedScheduleHeader");
            }

        }
        private string validationMessage;
        public string ValidationMessage
        {
            get
            {
                return validationMessage;
            }
            set
            {
                validationMessage = value;
                OnPropertyChanged("ValidationMessage");
            }

        }
        private bool isValidationShown;
        public bool IsValidationShown
        {
            get { return isValidationShown; }
            set
            {
                isValidationShown = value;
                OnPropertyChanged("IsValidationShown");
            }
        }
        ObservableCollection<Graded> gradedList;
        public ObservableCollection<Graded> GradedList
        {
            get
            {
                return gradedList;
            }
            set
            {
                gradedList = value;
                OnPropertyChanged("GradedList");
            }
        }


        Graded _selectedGraded;
        public Graded SelectedGraded
        {
            get
            {
                return _selectedGraded ?? new Graded();
            }
            set
            {
                _selectedGraded = value;
                OnPropertyChanged("SelectedGraded");
            }
        }

        NonGraded _selectedNonGraded;
        public NonGraded SelectedNonGraded
        {
            get
            {
                return _selectedNonGraded ?? new NonGraded();
            }
            set
            {
                _selectedNonGraded = value;
                OnPropertyChanged("SelectedNonGraded");
            }
        }

        ObservableCollection<NonGraded> nonGradedList;
        public ObservableCollection<NonGraded> NonGradedList
        {
            get
            {
                return nonGradedList;
            }
            set
            {
                nonGradedList = value;
                OnPropertyChanged("NonGradedList");
            }
        }

        private PolicyToolIncommingShedule _savedCustomSelectedSchedule;
        public PolicyToolIncommingShedule SavedCustomSelectedSchedule
        {
            get { return _savedCustomSelectedSchedule; }
            set
            {
                _savedCustomSelectedSchedule = value;
                OnPropertyChanged("SavedCustomSelectedSchedule");
            }
        }
        #endregion
        private bool _IsClientEnable;
        public bool IsClientEnable
        {
            get
            {
                return _IsClientEnable;
            }
            set
            {
                _IsClientEnable = value;
                OnPropertyChanged("IsClientEnable");
            }
        }



        readonly Dispatcher dispatcher;
        private PolicyServiceClient _PolicyServiceClient;

        //public IViewDialog WaitDialog = null;
        public VMOptimizePolicyManager(PolicyClientVm objClientVm)
        {
            try
            {

                //------------------------------------------------------Ankit Added this code for show hide the check schedule button-----------------------------------------------------------------------------------------------
                //bool isScheduleExist = serviceClients.SettingsClient.CheckNamedscheduleExist(SharedVMData.SelectedLicensee.LicenseeId);
                //ShowHideCheckSchedulebtn = isScheduleExist ? DesignerSerializationVisibility.Visible.ToString() : DesignerSerializationVisibility.Hidden.ToString();
                //  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                ReferseEnable = true;
                FirstTimeLoading = true;
                dispatcher = Dispatcher.CurrentDispatcher;
                ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();
                _PolicyServiceClient = new PolicyServiceClient(this);
                IncomingModes = new ObservableCollection<string>();
                IncomingModes.Add("Standard");
                IncomingModes.Add("Custom");
                SelectedIncomingMode = IncomingModes[0];
                IsGraded = true;
                ValidationMessage = "";
                IsValidationShown = false;
                IsCustomOptionSelected = false;
                PropertyChanged += new PropertyChangedEventHandler(VMOptimizePolicyManager_PropertyChanged);

                PolicyScreenControl();
                IsActiceChecked = true;
                policyStatus = _PolicyStatus.Active;

                VMPolicyClient = objClientVm;
                GradedScheduleHeader = "% of Premium";
                PolicyDetailMasterData masterData = serviceClients.MasterClient.GetPolicyDetailMasterData();
                //var chanel = serviceClients.MasterClient.ChannelFactory.CreateChannel(); 
                //PolicyDetailMasterData masterData = chanel.GetPolicyDetailMasterData();

                MasterPolicyStatus = masterData.Statuses;
                PolicyTerminationtReasonLst = masterData.TerminationReasons;
                MasterIncomingPaymentTypeLst = masterData.IncomingPaymentTypes;
                MasterPaymentsModeData = masterData.Modes;

                if (SharedVMData.SelectedLicensee != null)
                {
                    if (AllProducts == null)
                        AllProducts = new ObservableCollection<DisplayedCoverage>();

                    DisplayedCoverage emptyCoverage = new DisplayedCoverage { CoverageID = Guid.Empty, Name = string.Empty };
                    AllProducts.Add(emptyCoverage);
                }

                //if (SharedVMData != null)
                //{
                //    BackgroundWorker worker2 = new BackgroundWorker();
                //    worker2.DoWork += new System.ComponentModel.DoWorkEventHandler(worker2_DoWork);
                //    worker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker2_RunWorkerCompleted);
                //    worker2.RunWorkerAsync();
                //}


                VMPolicySchedule = new VMPolicySchedule(this, masterData);
                VMInstances.PolicyScheduleVM = VMPolicySchedule;

                if (SharedVMData.SelectedLicensee != null)
                {
                    VMPolicySmartField = new VMPolicySmartField(this, masterData, SharedVMData.SelectedLicensee.LicenseeId);
                    VMInstances.PolicySmartFieldVM = VMPolicySmartField;
                }

                //  VMPolicySmartField.SelectedLearnedChanged += VMPolicySmartField_SelectedLearnedChanged;

                VMPolicyNote = new VMPolicyNote(this);
                VMInstances.PolicyNoteVM = VMPolicyNote;

                VMPolicyCommission = new VMPolicyCommission(this, masterData);
                VMInstances.PolicyCommissionVM = VMPolicyCommission;

                serviceClients.PolicyClient.GetPolicydataCompleted += new EventHandler<GetPolicydataCompletedEventArgs>(PolicyClient_GetPolicydataCompleted);
                LastViewPolicyClientCollection.LastViewedClients.LastViewedClientChanged += new LastViewPolicyClientCollection.LastViewedClientChangedEventHandler(LastViewedClients_LastViewedClientChanged);

                if (RoleManager.Role != UserRole.SuperAdmin)
                {
                    try
                    {
                        //DisplayedClientsListsTemp = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                        //Need to analysis of using
                        //vinod
                        //UsrLst = serviceClients.UserClient.GetUsersByLicensee(RoleManager.LicenseeId.Value);
                        //UsrLst = new ObservableCollection<User>(UsrLst.Where(p => p.Role == UserRole.Agent && p.IsHouseAccount == false).ToList());
                        //objList = new List<LinkedUser>(serviceClients.UserClient.GetLinkedUser(RoleManager.userCredentialID, RoleManager.Role, RoleManager.IsHouseAccount));

                        ////Enabled and disabled outgoing schudule
                        //if (RoleManager.IsEditDisable == null || RoleManager.IsEditDisable == false)
                        //    IsEditDisable = false;
                        //else
                        //    IsEditDisable = true;
                        //vinod
                    }
                    catch
                    {
                    }
                }
                else
                {

                }
            }
            catch
            {
            }

        }

        //private void VMPolicySmartField_SelectedLearnedChanged(string modifiedText)
        //{
        //    LastModifiedDetail = modifiedText;
        //    FirstYearText = "1111";
        //}

        void worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SharedVMData.isRefeshAgentList = false;
        }

        void worker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                if (SharedVMData != null)
                {
                    if (SharedVMData.TestMasterFollowupIssueList != null)
                    {
                        SharedVMData.TestMasterFollowupIssueList.Clear();
                    }
                }
                bool FollowUp = true;
                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {
                    SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp, 180);
                }
                else
                {
                    Guid guidID = (Guid)RoleManager.LicenseeId;
                    SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp, 180);
                }
            }
            catch
            {
            }
        }

        void worker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {

                    SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllPayee());

                }
                else
                {
                    Guid guidID = (Guid)RoleManager.LicenseeId;
                    SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllPayeeByLicencessID(guidID));

                }
            }
            catch
            {
            }
        }

        private void LoadAllproduct()
        {
            PolicyDetailMasterData masterData = serviceClients.MasterClient.GetPolicyDetailMasterData();
            MasterPolicyStatus = masterData.Statuses;
            PolicyTerminationtReasonLst = masterData.TerminationReasons;
            MasterIncomingPaymentTypeLst = masterData.IncomingPaymentTypes;
            MasterPaymentsModeData = masterData.Modes;

            AllProducts = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);

            if (AllProducts == null)
                AllProducts = new ObservableCollection<DisplayedCoverage>();
            AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
            DisplayedCoverage emptyCoverage = new DisplayedCoverage { CoverageID = Guid.Empty, Name = string.Empty };
            AllProducts.Add(emptyCoverage);

            VMPolicySchedule = new VMPolicySchedule(this, masterData);
            VMInstances.PolicyScheduleVM = VMPolicySchedule;

            if (SharedVMData.SelectedLicensee != null)
            {
                VMPolicySmartField = new VMPolicySmartField(this, masterData, SharedVMData.SelectedLicensee.LicenseeId);
                VMInstances.PolicySmartFieldVM = VMPolicySmartField;
            }

            VMPolicyNote = new VMPolicyNote(this);
            VMInstances.PolicyNoteVM = VMPolicyNote;

            VMPolicyCommission = new VMPolicyCommission(this, masterData);
            VMInstances.PolicyCommissionVM = VMPolicyCommission;

            serviceClients.PolicyClient.GetPolicydataCompleted += new EventHandler<GetPolicydataCompletedEventArgs>(PolicyClient_GetPolicydataCompleted);
            LastViewPolicyClientCollection.LastViewedClients.LastViewedClientChanged += new LastViewPolicyClientCollection.LastViewedClientChangedEventHandler(LastViewedClients_LastViewedClientChanged);

            if (RoleManager.Role != UserRole.SuperAdmin)
            {
                try
                {
                    UsrLst = serviceClients.UserClient.GetUsersByLicensee(RoleManager.LicenseeId.Value);
                    UsrLst = new ObservableCollection<User>(UsrLst.Where(p => p.Role == UserRole.Agent && p.IsHouseAccount == false).ToList());
                    objList = new List<LinkedUser>(serviceClients.UserClient.GetLinkedUser(RoleManager.userCredentialID, RoleManager.Role, RoleManager.IsHouseAccount));
                    //Enabled and disabled outgoing schudule
                    if (RoleManager.IsEditDisable == null || RoleManager.IsEditDisable == false)
                        IsEditDisable = false;
                    else
                        IsEditDisable = true;
                }
                catch
                {
                }
            }
        }

        #region"Show pop up of auto term date"

        private ICommand _ShowAutoTermDate;
        public ICommand ShowAutoTermDate
        {
            get
            {
                if (_ShowAutoTermDate == null)
                {
                    _ShowAutoTermDate = new BaseCommand(p => OnShowAutoTermDate());
                }
                return _ShowAutoTermDate;
            }
        }

        private ICommand _cmdUpdateAutotermDate;
        public ICommand cmdUpdateAutotermDate
        {
            get
            {
                if (_cmdUpdateAutotermDate == null)
                {
                    _cmdUpdateAutotermDate = new BaseCommand(p => OnUpdateAutoTermDate());
                }
                return _cmdUpdateAutotermDate;
            }
        }

        private string _WaterText;
        public string WaterText
        {
            get
            {
                return _WaterText;
            }
            set
            {
                _WaterText = value;
                OnPropertyChanged("WaterText");
            }
        }

        private bool _IsEditDisable;
        public bool IsEditDisable
        {
            get { return _IsEditDisable; }
            set
            {
                _IsEditDisable = value;
                OnPropertyChanged("IsEditDisable");
            }
        }

        private void UserClient_GetAllUsersCompleted()
        {
        }

        public void CalculatePolicyType()
        {
            string PolicyType = "";
            try
            {
                PolicyType = serviceClients.PolicyClient.CalculatePolicyType(SelectedPolicy.OriginalEffectiveDate, SelectedPolicy.ClientId, (Guid)SelectedPolicy.PolicyLicenseeId, SelectedPolicy.PolicyId, SelectedPolicy.CoverageId);

                //see6 to check all values passed is comng correct in edit and new

                //PolicyType = "Replace";//for testing

                SelectedPolicy.PolicyType = null;//seeA
                SelectedPolicy.PolicyType = PolicyType;//seeA

                if (PolicyType == "New")//seeA
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();//seeA
                }
                else//seeA
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();//seeA
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            //return PolicyType;
        }

        //public void setPolicyType(string PolicyType)
        //{
        //    try
        //    {
        //        SelectedPolicy.PolicyType = PolicyType;
        //    }
        //    catch
        //    {
        //    }
        //}

        public void OnShowAutoTermDate()
        {
            try
            {
                if (SelectedPolicy != null)
                    policyLearnFields = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldAutoTerminationDate(SelectedPolicy.PolicyId);

                if (policyLearnFields != null)
                {
                    DateTime? dtSmartFieldAutoTerm = policyLearnFields;
                    if (SelectedPolicy != null)
                    {
                        DateTime? dtPolicyDetailsAutoTerm = SelectedPolicy.PolicyTerminationDate;
                    }

                    if (dtSmartFieldAutoTerm == null)
                        WaterText = "No AutoTerm";
                    else
                    {
                        if (SelectedPolicy != null)
                        {
                            if (SelectedPolicy.PolicyTerminationDate == null)
                            {
                                DateTime dt = new DateTime();
                                if (dtSmartFieldAutoTerm != null)
                                {
                                    dt = Convert.ToDateTime(dtSmartFieldAutoTerm);
                                    WaterText = "AutoTerm: " + dt.ToShortDateString();
                                }
                                else
                                    WaterText = "AutoTerm: " + dtSmartFieldAutoTerm.Value.ToString();

                            }
                            else
                            {
                                DateTime dt = new DateTime();
                                if (dtSmartFieldAutoTerm != null)
                                {
                                    dt = Convert.ToDateTime(dtSmartFieldAutoTerm);
                                    WaterText = "AutoTerm: " + dt.ToShortDateString();
                                }
                                else
                                    WaterText = "AutoTerm: " + dtSmartFieldAutoTerm.Value.ToString();

                            }
                        }
                        else
                            WaterText = "No AutoTerm";
                    }

                }
                else
                {
                    if (SelectedPolicy != null)
                    {
                        SelectedPolicy.PolicyTerminationDate = null;
                        WaterText = "No AutoTerm";
                    }
                    return;
                }
            }
            catch
            {
            }
        }

        private void OnUpdateAutoTermDate()
        {
            try
            {

                DateTime? dtSmartFieldAutoTerm = null;
                DateTime? dtPolicyDetailsAutoTerm = null;
                if (policyLearnFields != null)
                {
                    dtSmartFieldAutoTerm = policyLearnFields;
                    if (SelectedPolicy != null)
                    {
                        dtPolicyDetailsAutoTerm = SelectedPolicy.PolicyTerminationDate;
                    }
                }

                if (dtSmartFieldAutoTerm == null)
                    return;
                else
                {
                    if (SelectedPolicy == null)
                        return;

                    if (SelectedPolicy.PolicyTerminationDate == null)
                    {
                        SelectedPolicy.PolicyTerminationDate = dtSmartFieldAutoTerm;
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to replace the Policy Term Date with the AutoTerm Date?", "MyAgencyVault", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            SelectedPolicy.PolicyTerminationDate = dtSmartFieldAutoTerm;
                        }
                        else
                        {
                            SelectedPolicy.PolicyTerminationDate = dtPolicyDetailsAutoTerm;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        void LastViewedClients_LastViewedClientChanged(Guid? ClinetId)
        {
            if (objLog == null) objLog = new MastersClient();
            try
            {

                if ((SelectedDisplayClient == null || SelectedDisplayClient.ClientId != ClinetId) && DisplayedClientsLists != null)

                    if (SharedVMData.SelectedLicensee.IsClientEnable == true)
                    {
                        if (SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                        {
                            try
                            {
                                int intCount = serviceClients.UserClient.GetAccountExecCount((Guid)SharedVMData.SelectedLicensee.LicenseeId);

                                if (intCount != AccountExecLst.Count() - 1)
                                {
                                    AccountExecLst = serviceClients.UserClient.GetAccountExecByLicencessID((Guid)SharedVMData.SelectedLicensee.LicenseeId);
                                }

                                if (AccountExecLst != null)
                                {
                                    if (AccountExecLst.Count > 0)
                                    {
                                        User objBalnk = new User();
                                        int intAxec = AccountExecLst.Count;
                                        AccountExecLst.Insert(intAxec, objBalnk);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ActionLogger.Logger.WriteLog(" Exception : " + ex.Message, true);
                            }
                        }
                        // DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                        if (DisplayedClientsListsTemp != null)
                        {
                            if (DisplayedClientsListsTemp.Count > 0)
                            {
                                // DisplayedClientsListsTemp = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                                SelectedDisplayClientTemp = DisplayedClientsListsTemp.FirstOrDefault(s => s.ClientId == (ClinetId ?? Guid.Empty));
                                DisplayedClientsLists = new ObservableCollection<Client>();
                                DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                                SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();

                            }
                        }
                        else
                        {
                            if (DisplayedClientsListsTemp != null)
                            {
                                SelectedDisplayClientTemp = DisplayedClientsListsTemp.FirstOrDefault(s => s.ClientId == (ClinetId ?? Guid.Empty));
                            }

                            if (SelectedDisplayClientTemp != null)
                            {
                                if (SelectedDisplayClientTemp.ClientId != Guid.Empty)
                                {
                                    if (SelectedDisplayClient.ClientId != ClinetId)
                                    {
                                        SelectedDisplayClientTemp = serviceClients.ClientClient.GetClientByClientID((Guid)ClinetId, SharedVMData.SelectedLicensee.LicenseeId);
                                        DisplayedClientsLists = new ObservableCollection<Client>();
                                        DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                                    }
                                    else
                                    {
                                        DisplayedClientsLists = new ObservableCollection<Client>();
                                        DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                                    }
                                }
                                else
                                {
                                    SelectedDisplayClientTemp = serviceClients.ClientClient.GetClientByClientID((Guid)ClinetId, SharedVMData.SelectedLicensee.LicenseeId);
                                    DisplayedClientsLists = new ObservableCollection<Client>();
                                    DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                                    SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                                }

                            }
                            else
                            {
                                SelectedDisplayClientTemp = serviceClients.ClientClient.GetClientByClientID((Guid)ClinetId, SharedVMData.SelectedLicensee.LicenseeId);
                                DisplayedClientsLists = new ObservableCollection<Client>();
                                DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                                SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                                //DisplayedClientsListsTemp = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());

                            }

                            //SelectedDisplayClientTemp = DisplayedClientsListsTemp.FirstOrDefault(s => s.ClientId == (ClinetId ?? Guid.Empty));
                            //DisplayedClientsLists = new ObservableCollection<Client>();
                            //DisplayedClientsLists.Add(SelectedDisplayClientTemp);
                            //SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                        }
                    }
                    else
                    {
                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault(s => s.ClientId == (ClinetId ?? Guid.Empty));
                    }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" Exception : " + ex.Message);
            }
        }

        public bool agencycomboBoxenable = false;
        public bool AgencyComboBoxEnable
        {
            get
            {
                return agencycomboBoxenable;
            }
            set
            {
                agencycomboBoxenable = value;
                OnPropertyChanged("AgencyComboBoxEnable");
            }

        }

        public void PolicyScreenControl()
        {
            if (RoleManager.Role == UserRole.SuperAdmin)
            {
                AgencyComboBoxEnable = false;
            }
            else if (RoleManager.Role == UserRole.Administrator)
            {
                AgencyComboBoxEnable = false;
            }
            else if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
            {
                AgencyComboBoxEnable = false;
            }
            else if (RoleManager.Role == UserRole.HO || ((RoleManager.Role == UserRole.Agent) && (RoleManager.IsHouseAccount == true)))
            {
                AgencyComboBoxEnable = true;
            }
        }

        #endregion

        #region

        private PolicyDetailsData selectedPolicy;
        public PolicyDetailsData SelectedPolicy
        {
            get
            {
                return selectedPolicy;
            }
            set
            {
                selectedPolicy = value;
                OnPropertyChanged("SelectedPolicy");
            }
        }

        private ObservableCollection<PolicyDetailsData> policyList;
        public ObservableCollection<PolicyDetailsData> PolicyList
        {
            get
            {
                return policyList;
            }
            set
            {
                policyList = value;
                OnPropertyChanged("PolicyList");
            }
        }

        private ObservableCollection<PolicyDetailsData> _TemppolicyList;
        public ObservableCollection<PolicyDetailsData> TemppolicyList
        {
            get
            {
                return _TemppolicyList;
            }
            set
            {
                _TemppolicyList = value;
                OnPropertyChanged("TemppolicyList");
            }
        }

        private ObservableCollection<PolicyDetailsData> _FTemppolicyList;
        public ObservableCollection<PolicyDetailsData> FTemppolicyList
        {
            get
            {
                return _FTemppolicyList;
            }
            set
            {
                _FTemppolicyList = value;
                OnPropertyChanged("FTemppolicyList");
            }
        }

        private ClientWisePolicyCollection _SelectedClientPolicyList;
        public ClientWisePolicyCollection SelectedClientPolicyList
        {
            get
            {
                return _SelectedClientPolicyList;
            }
            set
            {
                _SelectedClientPolicyList = value;
                OnPropertyChanged("SelectedClientPolicyList");
            }
        }

        private ObservableCollection<ClientWisePolicyCollection> _ClientWisePolicyList = null;
        public ObservableCollection<ClientWisePolicyCollection> ClientWisePolicyList
        {
            get
            {
                return _ClientWisePolicyList;
            }
            set
            {
                _ClientWisePolicyList = value;
                OnPropertyChanged("ClientWisePolicyList");
            }
        }

        private string replaceBtntooltip = "";
        public string ReplaceBtntooltip
        {
            get { return replaceBtntooltip; }
            set { replaceBtntooltip = value; OnPropertyChanged("ReplaceBtntooltip"); }
        }

        #endregion

        #region Property Changed

        bool IsMissMonth = true;
        bool istrackPayment = true;

        void VMOptimizePolicyManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "SelectedDisplayClient":
                        if (SelectedDisplayClient != null)
                        {
                            //Refresh the incomming and outgoing schudule bug id :2895
                            Mouse.OverrideCursor = Cursors.Wait;
                            try
                            {
                                if (ClientWisePolicyList != null)
                                    SelectedClientPolicyList = ClientWisePolicyList.FirstOrDefault(s => s.ClientId == SelectedDisplayClient.ClientId);
                                else
                                    SelectedClientPolicyList = null;

                                LastViewPolicy lastViewedPolicy = LastViewPolicyClientCollection.getLastViewedPolicyForClient(SelectedDisplayClient.ClientId);

                                SelectedClientPolicyList = new ClientWisePolicyCollection();
                                //call service from database for get the record for pending policy payment 
                                string strClientName = string.Empty;
                                if (SelectedDisplayClient.LicenseeId != null)
                                {
                                    strClientName = SelectedDisplayClient.Name;
                                    Guid guidLincenseID = SelectedDisplayClient.LicenseeId.Value;
                                    Guid? ClientId = selectedDisplayClient.ClientId;
                                    if (!string.IsNullOrEmpty(strClientName.Trim()) && guidLincenseID != null)
                                    {
                                        if (SelectedClientPolicyList.ClientId != ClientId)
                                        {
                                            SelectedClientPolicyList.PolicyList = new ObservableCollection<PolicyDetailsData>(serviceClients.PolicyClient.GetPolicyClientWise((Guid)guidLincenseID, (Guid)ClientId));
                                            SelectedClientPolicyList.ClientId = ClientId;
                                        }
                                    }

                                    PolicyList = SelectedClientPolicyList.PolicyList;

                                    if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                                    {
                                        //TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.CreatedBy == RoleManager.userCredentialID));
                                        //FTemppolicyList = new ObservableCollection<PolicyDetailsData>(TemppolicyList);
                                        bool isBreak = false;
                                        TemppolicyList = new ObservableCollection<PolicyDetailsData>();
                                        FTemppolicyList = new ObservableCollection<PolicyDetailsData>(TemppolicyList);
                                        List<LinkedUser> objList = new List<LinkedUser>(serviceClients.UserClient.GetLinkedUser(RoleManager.userCredentialID, RoleManager.Role, RoleManager.IsHouseAccount));
                                        if (objList.Count > 0)
                                        {
                                            foreach (var item in PolicyList)
                                            {
                                                OutGoingField = FillBasicOutGoingSchedule(item.PolicyId);
                                                if (OutGoingField.Count > 0)
                                                {
                                                    for (int i = 0; i < OutGoingField.Count; i++)
                                                    {
                                                        for (int j = 0; j < objList.Count; j++)
                                                        {
                                                            if ((OutGoingField[i].PayeeUserCredentialId == objList[j].UserId) || (RoleManager.userCredentialID == OutGoingField[i].PayeeUserCredentialId))
                                                            {
                                                                TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.PolicyId == OutGoingField[i].PolicyId));
                                                                if (TemppolicyList.Count > 0)
                                                                {
                                                                    FTemppolicyList.AddRange(TemppolicyList);
                                                                    isBreak = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        if (isBreak)
                                                        {
                                                            isBreak = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                //acme - Sep 11, 2017, By Kevin - When no outgoing schedule, then show policy iff logged in user has permission to view house
                                                else
                                                {
                                                    try
                                                    {
                                                        foreach (var i in objList) //Check if house account is present in linked agents
                                                        {
                                                            var usr = serviceClients.UserClient.GetUserIdWise(i.UserId);
                                                            if (usr != null && usr.IsHouseAccount)
                                                            {
                                                                // add policy to the list
                                                                TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.PolicyId == item.PolicyId));
                                                                FTemppolicyList.AddRange(TemppolicyList);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        objLog.AddLog("Exception adding policy with no outgoing schedule : " + ex.Message);
                                                    }
                                                }
                                            }
                                        }
                                        else //No linked user found then
                                        {
                                            foreach (var item in PolicyList)
                                            {
                                                OutGoingField = FillBasicOutGoingSchedule(item.PolicyId);

                                                if (OutGoingField.Count > 0)
                                                {
                                                    for (int i = 0; i < OutGoingField.Count; i++)
                                                    {
                                                        if (OutGoingField[i].PayeeUserCredentialId == RoleManager.userCredentialID)
                                                        {
                                                            TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.PolicyId == OutGoingField[i].PolicyId));
                                                            if (TemppolicyList.Count > 0)
                                                            {
                                                                FTemppolicyList.AddRange(TemppolicyList);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.PolicyId == item.PolicyId && p.CreatedBy == RoleManager.userCredentialID));
                                                            if (TemppolicyList != null && TemppolicyList.Count > 0)
                                                                FTemppolicyList.AddRange(TemppolicyList);
                                                        }

                                                    }
                                                }
                                                //check the policy which is created by agent
                                                else //When no out going schudule
                                                {
                                                    TemppolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.PolicyId == item.PolicyId && p.CreatedBy == RoleManager.userCredentialID));
                                                    FTemppolicyList.AddRange(TemppolicyList);
                                                }

                                            }
                                        }

                                        PolicyList = new ObservableCollection<PolicyDetailsData>(FTemppolicyList);
                                    }


                                    //Vinod
                                    bool isPendingPolicyExist = false;
                                    bool isActivePolicyExist = false;
                                    bool isTerminatedPolicyExist = false;

                                    if (PolicyList != null)
                                    {
                                        PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.IsDeleted == false));
                                        isPendingPolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 2);
                                        isActivePolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 0);
                                        isTerminatedPolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 1);
                                    }

                                    if (isPendingPolicyExist && isActivePolicyExist)
                                        OnStatusChanged("All");

                                    else if (isActivePolicyExist && isTerminatedPolicyExist)
                                        OnStatusChanged("All");

                                    else if (isPendingPolicyExist && isTerminatedPolicyExist)
                                        OnStatusChanged("All");

                                    else if (isActivePolicyExist && isPendingPolicyExist && isTerminatedPolicyExist)
                                        OnStatusChanged("All");

                                    else
                                    {
                                        if (isActivePolicyExist)
                                        {
                                            OnStatusChanged("Active");
                                        }
                                        else if (isPendingPolicyExist)
                                        {
                                            OnStatusChanged("Pending");
                                        }
                                        else if (isTerminatedPolicyExist)
                                        {
                                            OnStatusChanged("Terminated");
                                        }
                                        else
                                        {
                                            OnStatusChanged("All");
                                        }
                                    }
                                }

                                if (PolicyList == null || PolicyList.Count == 0)
                                {
                                    SelectedPolicy = null;
                                    OutGoingField = null;
                                    //Tfs bug id:2895
                                    SelectedPolicyToolIncommingShedule = null;
                                    //Added 7 may (07/05/2010)
                                    //If you have 2 clients each with a policy:ABC Company Inc.ABC Company Select ABC Company and its policy.  Chance the “Client” field to ABC Company Inc.The policy properly moves but the ABC Company name is still in the Client pull down.  Needs to be removed if there are no policies associated.(DONE)
                                    if (string.IsNullOrEmpty(strClientName))
                                    {
                                        //Load refresh added client
                                        SelectedClient = DisplayedClientsLists.FirstOrDefault();
                                    }
                                    else
                                    { //assign the selected client name 
                                        SelectedClient = DisplayedClientsLists.Where(p => p.Name == strClientName).FirstOrDefault();
                                    }
                                }
                                //Save view client when chenge 
                                //Vinod
                                //if (!_isNotSaveLastViewedClientPolicy)
                                //    SaveLastViewedClientPolicyData();

                            }
                            catch
                            {
                            }
                            finally
                            {
                                Mouse.OverrideCursor = null;
                            }
                        }
                        //vinod
                        //  SelectedClientChanged(SelectedDisplayClient);


                        break;
                    case "SelectedCoverageNickName": //added by acme to get incomign schedules list
                        //Modified By Ankit Khandelwal changes  for adding custom Schedule
                        if (SelectedPolicy != null
                            && SelectedCoverageNickName != null && !string.IsNullOrEmpty(SelectedCoverageNickName.NickName)
                             && SelectedCoverageNickName.NickName != SelectedPolicy.ProductType
                            )
                        {
                            PayorIncomingSchedule schedule = serviceClients.GlobalIncomingScheduleClient.GetPayorScheduleDetails((Guid)SelectedPolicy.PayorId, (Guid)SelectedPolicy.CarrierID, (Guid)SelectedPolicy.CoverageId, SharedVMData.SelectedLicensee.LicenseeId, SelectedCoverageNickName.NickName, (int)SelectedPolicy.IncomingPaymentTypeId);
                            if (schedule != null && schedule.IncomingScheduleID != Guid.Empty)
                            {
                                //if (SelectedPolicyToolIncommingShedule != null) //Show alert if schedule exists
                                //{
                                MessageBoxResult _result = MessageBox.Show("Please note that there exists 'Incoming Schedule' configuration for selected payor. Do you want to use these settings?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                                if (_result == MessageBoxResult.Yes)
                                {

                                    AssignPayorIncomingSchedule(schedule);
                                }
                                //}
                                //else //silently assign if no schedule 
                                //{
                                //    AssignPayorIncomingSchedule(schedule);
                                //}
                            }
                        }
                        //    else
                        //    {
                        //        SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == 1).FirstOrDefault();
                        //        if (SelectedPolicyToolIncommingShedule == null)
                        //        {
                        //            SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
                        //        }
                        //        SelectedPolicyToolIncommingShedule.FirstYearPercentage = 0;
                        //        SelectedPolicyToolIncommingShedule.RenewalPercentage = 0;
                        //        SelectedPolicyToolIncommingShedule.ScheduleTypeId = 1;
                        //        SelectedPolicy.SplitPercentage = 100;
                        //        SelectedPolicy.Advance = 0;
                        //        IncPercentOfPremium = true;
                        //        IncPerHead = false;
                        //    }
                        //}
                        //else
                        //{
                        //    SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == 1).FirstOrDefault();
                        //    if (SelectedPolicyToolIncommingShedule == null)
                        //    {
                        //        SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
                        //    }
                        //    SelectedPolicyToolIncommingShedule.FirstYearPercentage = 0;
                        //    SelectedPolicyToolIncommingShedule.RenewalPercentage = 0;
                        //    SelectedPolicyToolIncommingShedule.ScheduleTypeId = 1;
                        //    SelectedPolicy.SplitPercentage =100;
                        //    SelectedPolicy.Advance = 0;
                        //    IncPercentOfPremium = true;
                        //    IncPerHead = false;
                        //}

                        break;
                    case "SelectedPolicy":
                        DateTime StartTime = DateTime.Now;
                        Mouse.OverrideCursor = Cursors.Wait;
                        IsValidationShown = false;
                        validationMessage = "";
                        try
                        {

                            if (SelectedPolicy != null)
                            {
                                //[Modified By-Ankit]
                                //[Aug06,2018-We should clear the list of outgoing Schedules when switching to another policy without saving the policy which we have deleted outgoing schedules.]
                                if (_deleteseletedOutGoingField != null && _deleteseletedOutGoingField.Count > 0)
                                {
                                    _deleteseletedOutGoingField.Clear();
                                }

                                IsMissMonth = SelectedPolicy.IsTrackMissingMonth;
                                istrackPayment = SelectedPolicy.IsTrackIncomingPercentage;
                                IsTrackPayment = SelectedPolicy.IsTrackPayment;
                                try
                                {
                                    IsCustomDateSelected = (SelectedPolicy.IsCustomBasicSchedule == null) ? false : (bool)SelectedPolicy.IsCustomBasicSchedule;
                                    IsOutScheduleEntered = (IsCustomDateSelected && (!string.IsNullOrEmpty(SelectedPolicy.CustomDateType)) && SelectedPolicy.CustomDateType.ToLower() == "entered") ? true : false;
                                    IsOutScheduleInvoice = (IsCustomDateSelected && (!string.IsNullOrEmpty(SelectedPolicy.CustomDateType)) && SelectedPolicy.CustomDateType.ToLower() == "invoice") ? true : false;
                                    //Tiered Schedule 
                                    IsTiered = (SelectedPolicy.IsTieredSchedule == null) ? false : (bool)SelectedPolicy.IsTieredSchedule;

                                    //Case when no option selected, select invoice date by default 
                                    //if (IsCustomDateSelected && !IsOutScheduleEntered && !IsOutScheduleInvoice)
                                    //    IsOutScheduleInvoice = true;
                                }
                                catch (Exception ex)
                                {
                                    objLog.AddLog("exception in policy settings: " + ex.Message);
                                }


                                try
                                {
                                    if (SelectedPolicy.UserCredentialId != null)
                                    {
                                        if (AccountExecLst != null)
                                        {
                                            if (SelectedPolicy.UserCredentialId != Guid.Empty)
                                            {
                                                SelectedAccountExecLst = AccountExecLst.Where(d => d.UserCredentialID == SelectedPolicy.UserCredentialId).FirstOrDefault();
                                            }
                                        }

                                    }
                                    else
                                    {
                                        SelectedAccountExecLst = null;
                                    }
                                }
                                catch
                                {
                                }

                                //Acme
                                if (SelectedPolicy.PrimaryAgent != null && PrimaryAgents != null)
                                {
                                    SelectedPrimaryAgent = PrimaryAgents.Where(d => d.UserCredentialID == SelectedPolicy.PrimaryAgent).FirstOrDefault();
                                }
                                else
                                {
                                    SelectedPrimaryAgent = null;
                                }


                                if (!SelectedPolicy.IsSelectedPolicyChangeAttach)
                                {
                                    SelectedPolicy.PropertyChanged += new PropertyChangedEventHandler(SelectedPolicy_PropertyChanged);
                                    SelectedPolicy.IsSelectedPolicyChangeAttach = true;
                                }

                                SelectedPolicyIsNotNull = true;
                                if (SelectedPolicy.PolicyId == Guid.Empty)
                                {
                                    UpdateUIByAddNewPolicy(SelectedPolicy);
                                    return;
                                }
                                if (PolicyList.Count() != 0 && SelectedPolicy.PolicyId == Guid.Empty)
                                {
                                    SelectedPolicy = PolicyList.FirstOrDefault();
                                }

                                SelectedStatus = MasterPolicyStatus.Where(p => p.StatusId == SelectedPolicy.PolicyStatusId).FirstOrDefault();
                                SelectedClient = DisplayedClientsLists.Where(p => p.ClientId == SelectedPolicy.ClientId).FirstOrDefault();
                                SelectedPaymentMode = MasterPaymentsModeData.Where(p => p.ModeId == SelectedPolicy.PolicyModeId).FirstOrDefault();

                                if (PayorsLst != null && PayorsLst.Count != 0)
                                {
                                    SelectedPolicy.Payor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                                }
                                else
                                {
                                    FillSubmitedThrough(PayorsLst);
                                }

                                SelectedTermReason = PolicyTerminationtReasonLst.Where(p => p.TerminationReasonId == SelectedPolicy.TerminationReasonId).FirstOrDefault();
                                SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelectedPolicy.IncomingPaymentTypeId).FirstOrDefault();

                                SelectedPolicyToolIncommingShedule = FillIncomingBasicSchedule();
                                SavedCustomSelectedSchedule = SelectedPolicyToolIncommingShedule.Clone() as PolicyToolIncommingShedule;
                                // Added by Ankit for set mode value in mode dropdown

                                SelectedIncomingMode = SelectedPolicyToolIncommingShedule.Mode == Mode.Custom ? "Custom" : "Standard";
                                OutGoingField = FillBasicOutGoingSchedule();

                                if (SelectedPolicy.IsOutGoingBasicSchedule == true)
                                    SelectedOutGoingField = OutGoingField.FirstOrDefault();

                                if (SelectedOutGoingField.OutgoingScheduleId == Guid.Empty && (SelectedPolicy.IsOutGoingBasicSchedule ?? false))
                                {
                                    OutPercentOfCommission = true;
                                }

                                if (SelectedPolicy.PolicyType == "New")
                                {
                                    ReplaceRbStatus = true;
                                    NewRbStatus = true;
                                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();
                                }
                                else if (SelectedPolicy.PolicyType == "Replace")
                                {
                                    ReplaceRbStatus = true;
                                    NewRbStatus = true;
                                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();
                                    PolicyDetailsData _Policy = SelectedClientPolicyList.PolicyList.FirstOrDefault(s => s != null && s.PolicyId == (SelectedPolicy.ReplacedBy ?? Guid.Empty));
                                    if (_Policy != null)
                                    {
                                        ReplaceBtntooltip = "Carr : " + _Policy.CarrierName + ", # : " + _Policy.PolicyNumber;
                                        ReplaceBtntooltip = ReplaceBtntooltip.Trim();
                                    }
                                }
                                if (SelectedPolicy.IsOutGoingBasicSchedule.HasValue)
                                {
                                    if (SelectedPolicy.IsOutGoingBasicSchedule.Value)
                                        OutAdvance = false;
                                    else
                                        OutAdvance = true;
                                }
                                else
                                {
                                    OutPercentOfCommission = false;
                                    OutPercentOfPremium = false;
                                    OutAdvance = false;
                                }

                                if (SelectedPolicy.IsIncomingBasicSchedule.HasValue)
                                {
                                    if (SelectedPolicy.IsIncomingBasicSchedule.Value)
                                        IncAdvance = false;
                                    else
                                        IncAdvance = true;
                                }
                                else
                                {
                                    IncPercentOfPremium = false;
                                    IncPerHead = false;
                                    IncAdvance = false;
                                }
                            }
                            else
                            {
                                IncPercentOfPremium = false;
                                IncPerHead = false;
                                IncAdvance = false;
                                SelectedPolicyIsNotNull = false;
                                SelectedPolicyToolIncommingShedule = null;
                                OutGoingField = null;
                                SelectedOutGoingField = null;
                                //Clear the previous value
                                ClearCollection();

                            }
                            if (SelectedPolicy != null)
                            {
                                SelectedPolicy.SubmittedThrough = SubmittedThroughLst.Where(p => p == SelectedPolicy.SubmittedThrough).FirstOrDefault();
                                //Change and update the code of policy insured
                                if (string.IsNullOrEmpty(SelectedPolicy.Insured))
                                    SelectedPolicy.Insured = SelectedPolicy.ClientName;

                            }

                            if (SelectedPolicy != null)
                            {
                                SelectedPolicyChanged(SelectedPolicy);
                            }
                            //BackgroundWorker worker3 = new BackgroundWorker();
                            //worker3.DoWork += new System.ComponentModel.DoWorkEventHandler(worker3_DoWork);                               
                            //worker3.RunWorkerAsync();

                        }
                        finally
                        {
                            Mouse.OverrideCursor = null;
                        }

                        if (SelectedPolicy != null)
                        {
                            BackgroundWorker worker3 = new BackgroundWorker();
                            worker3.DoWork += new System.ComponentModel.DoWorkEventHandler(worker3_DoWork);
                            worker3.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker3_RunWorkerCompleted);
                            worker3.RunWorkerAsync();
                            //strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));
                            //strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));
                            ////Get updated policy setting
                            PolicyDetailsData policySetiing = serviceClients.PolicyClient.GetPolicyStting(SelectedPolicy.PolicyId);
                            SelectedPolicy.IsTrackMissingMonth = policySetiing.IsTrackMissingMonth;
                            SelectedPolicy.IsTrackIncomingPercentage = policySetiing.IsTrackIncomingPercentage;
                        }

                        break;
                    case "ReplaceRbStatus":
                        ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();
                        break;

                    case "NewRbStatus":
                        ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();
                        break;

                    case "SelectedStatus":
                        if (SelectedPolicy == null) return;

                        SelectedPolicy.PolicyStatusId = SelectedStatus.StatusId;
                        //if (SelectedPolicy.PolicyStatusName == "Deleted") return;
                        SelectedPolicy.PolicyStatusName = GetPolicayStatusName(SelectedPolicy.PolicyStatusId);
                        break;

                    case "SelectedClient":
                        if (SelectedPolicy == null || SelectedClient.ClientId == Guid.Empty) return;
                        SelectedPolicy.ClientId = SelectedClient.ClientId;
                        break;

                    case "SelectedPaymentMode":
                        if (SelectedPolicy == null) return;

                        SelectedPolicy.PolicyModeId = SelectedPaymentMode.ModeId;
                        break;

                    case "SelectedPayor":
                        break;
                    case "SelectedCarrier":
                        break;

                    case "SelectedProduct":
                        break;
                    case "SubmittedThrough":

                        break;
                    case "SelectedTermReason":
                        if (SelectedPolicy == null) return;
                        SelectedPolicy.TerminationReasonId = SelectedTermReason.TerminationReasonId;
                        break;
                    case "SelectedMasterIncomingPaymentType":
                        if (SelectedPolicy == null)
                            return;
                        SelectedPolicy.IncomingPaymentTypeId = SelectedMasterIncomingPaymentType.PaymentTypeId;
                        SelectedPolicy.PolicyIncomingPayType = SelectedMasterIncomingPaymentType.PaymenProcedureName;
                        break;
                    case "SelectedPolicyToolIncommingShedule":

                        if (SelectedPolicyToolIncommingShedule == null)
                        {
                            IncPercentOfPremium = true;
                            IncPerHead = false;
                            IncAdvance = false;
                            return;
                        }
                        if (SelectedPolicy.IsIncomingBasicSchedule == false)
                        {
                            IncAdvance = true;
                            IncPercentOfPremium = false;
                            IncPerHead = false;
                            return;
                        }
                        if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 1)//PercentageOfPremium
                        {
                            IncPercentOfPremium = true;
                            IncPerHead = false;
                            IncAdvance = false;
                        }
                        else if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 2)//PerHead
                        {
                            IncPercentOfPremium = false;
                            IncPerHead = true;
                            IncAdvance = false;
                        }
                        break;
                    case "IncPerHead":
                        if (SelectedPolicy != null)
                        {
                            if (IncPerHead == true)
                            {
                                FirstYearText = "First Year PerHead";
                                Renewaltext = "Renewal PerHead";
                                if (SelectedPolicyToolIncommingShedule == null) SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule()
                                {
                                    FirstYearPercentage = 0,
                                    RenewalPercentage = 0,
                                };

                                SelectedPolicyToolIncommingShedule.ScheduleTypeId = 2;
                                GradedScheduleHeader = "Per Head";
                                SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                                SelectedPolicy.IsIncomingBasicSchedule = true;
                            }
                        }
                        break;
                    case "IncPercentOfPremium":

                        if (SelectedPolicy != null)
                        {
                            if (IncPercentOfPremium == true)
                            {
                                FirstYearText = "First Year";
                                Renewaltext = "Renewal";
                                if (SelectedPolicyToolIncommingShedule == null) SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule()
                                {
                                    FirstYearPercentage = 0,
                                    RenewalPercentage = 0,
                                };
                                //SelectedPolicyToolIncommingShedule.CustomType = CustomMode.Graded;
                                SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                                SelectedPolicyToolIncommingShedule.ScheduleTypeId = 1;
                                GradedScheduleHeader = "% of Premium";
                                SelectedPolicy.IsIncomingBasicSchedule = true;
                            }

                        }
                        break;
                    case "IncAdvance":
                        if (SelectedPolicy != null)
                        {
                            if (IncAdvance == true)
                            {
                                SelectedPolicy.IsIncomingBasicSchedule = false;
                                IncPercentOfPremium = false;
                                IncPerHead = false;
                            }
                        }
                        break;

                    case "SelectedOutGoingField":
                        //SelectedOutGoingField.PropertyChanged += new PropertyChangedEventHandler(SelectedOutGoingField_PropertyChanged);
                        if (SelectedOutGoingField.ScheduleTypeId == 1)
                        {
                            OutPercentOfPremium = true;
                            OutPercentOfCommission = false;
                            OutAdvance = false;

                        }
                        else if (SelectedOutGoingField.ScheduleTypeId == 2)
                        {
                            OutPercentOfCommission = true;
                            OutPercentOfPremium = false;
                            OutAdvance = false;

                        }
                        break;

                    case "OutPercentOfCommission":
                        if (SelectedPolicy != null)
                        {
                            if (OutPercentOfCommission)
                            {
                                SelectedOutGoingField.ScheduleTypeId = 2;
                                if (OutGoingField != null)
                                    OutGoingField.ToList().ForEach(p => p.ScheduleTypeId = 2);
                                SelectedPolicy.IsOutGoingBasicSchedule = true;
                            }
                        }
                        break;

                    case "OutPercentOfPremium":
                        if (SelectedPolicy != null)
                        {
                            if (OutPercentOfPremium)
                            {
                                SelectedOutGoingField.ScheduleTypeId = 1;
                                if (OutGoingField != null)
                                    OutGoingField.ToList().ForEach(p => p.ScheduleTypeId = 1);
                                SelectedPolicy.IsOutGoingBasicSchedule = true;
                            }
                        }
                        break;
                    case "IsOutScheduleInvoice":
                        if (SelectedPolicy != null)
                        {
                            if (IsOutScheduleInvoice)
                            {
                                SelectedPolicy.CustomDateType = "Invoice";
                            }
                        }
                        break;
                    case "IsOutScheduleEntered":
                        if (SelectedPolicy != null)
                        {
                            if (IsOutScheduleEntered)
                            {
                                SelectedPolicy.CustomDateType = "Entered";
                            }
                        }
                        break;
                    case "IsCustomDateSelected":

                        if (IsCustomDateSelected && !IsOutScheduleEntered && !IsOutScheduleInvoice)
                        {
                            IsOutScheduleInvoice = true;
                        }
                        break;
                    case "OutAdvance":
                        if (SelectedPolicy != null)
                        {
                            if (OutAdvance)
                            {
                                SelectedPolicy.IsOutGoingBasicSchedule = false;
                            }
                        }
                        break;
                    case "SelecetdPolicylstForReplace":
                        if (SelecetdPolicylstForReplace == null) return;
                        if (SelecetdPolicylstForReplace.PolicyTerminationDate.HasValue)
                        {
                            ReplacePolicyTermDate = SelecetdPolicylstForReplace.PolicyTerminationDate.Value;
                        }
                        else
                        {
                            ReplacePolicyTermDate = FirstDate(DateTime.Today).Value;

                        }
                        if (SelecetdPolicylstForReplace.TerminationReasonId.HasValue)
                        {
                            SelectedReplacePolicyTermReason = PolicyReplacePolicyTerminationtReasonLst
                                        .Where(p => p.TerminationReasonId == SelecetdPolicylstForReplace.TerminationReasonId).FirstOrDefault();
                        }
                        else
                        {
                            SelectedReplacePolicyTermReason = PolicyReplacePolicyTerminationtReasonLst.Where(p => p.TerminationReasonId == 0).FirstOrDefault();
                        }
                        break;
                    case "SelectedIncomingMode":
                        GradedList = null;
                        NonGradedList = null;
                        IsCustomOptionSelected = (SelectedIncomingMode == "Custom");
                        if (!IsCustomOptionSelected)
                        {

                            IsPercentOfPremiumEnabled = true;
                        }

                        if (SelectedPolicyToolIncommingShedule != null)

                        {
                            SelectedPolicyToolIncommingShedule.Mode = (isCustomOptionSelected) ? Mode.Custom : Mode.Standard;
                            //SelectedPolicyToolIncommingShedule.CustomType = SelectedPolicyToolIncommingShedule.Mode == Mode.Standard ? CustomMode.Graded : CustomMode.NonGraded;
                            IsGraded = (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded);
                        }
                        else
                        {
                            IsGraded = true;
                        }
                        NewCustomRecord();
                        break;
                    case "IsGraded":
                        IsNonGraded = !IsGraded;
                        if (IsCustomOptionSelected && IsGraded)
                        {
                            OutPercentOfCommission = true;
                            IsPercentOfPremiumEnabled = false;
                        }
                        else if (IsCustomOptionSelected && !IsGraded)
                        {
                            IsPercentOfPremiumEnabled = true;
                        }

                        if (SelectedPolicyToolIncommingShedule != null)
                        {
                            SelectedPolicyToolIncommingShedule.CustomType = (IsGraded) ? CustomMode.Graded : CustomMode.NonGraded;
                        }
                        if (IsGraded && (SelectedPolicyToolIncommingShedule.GradedSchedule != null && SelectedPolicyToolIncommingShedule.GradedSchedule.Count > 0))
                        {
                            SelectedGraded = SelectedPolicyToolIncommingShedule.GradedSchedule[0];
                            GradedList = SelectedPolicyToolIncommingShedule.GradedSchedule;
                        }
                        else if (!IsGraded && (SelectedPolicyToolIncommingShedule.NonGradedSchedule != null && SelectedPolicyToolIncommingShedule.NonGradedSchedule.Count > 0))
                        {
                            SelectedNonGraded = SelectedPolicyToolIncommingShedule.NonGradedSchedule[0];
                            NonGradedList = SelectedPolicyToolIncommingShedule.NonGradedSchedule;
                        }
                        if (isCustomOptionSelected && ((IsGraded && GradedList == null) || (GradedList != null && GradedList.Count == 0)) ||
                                                                 ((IsNonGraded && NonGradedList == null) || (NonGradedList != null && NonGradedList.Count == 0)))
                        {
                            AddNewCustomRow();

                        }

                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Modified By:Ankit Kahndelwal
        /// Modified On:27-11-2019  
        /// Purpose: Modified for apply custom schedule  from matched payor Schedule  
        /// </summary>
        /// <param name="schedule"></param>

        void AssignPayorIncomingSchedule(PayorIncomingSchedule schedule)
        {
            SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(x => x.PaymentTypeId == schedule.IncomingPaymentTypeID).FirstOrDefault();
            if (SelectedPolicyToolIncommingShedule == null)
            {
                SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
            }
            SelectedPolicyToolIncommingShedule.CopyFromPayor(schedule);
            //SavedCustomSelectedSchedule = SelectedPolicyToolIncommingShedule;
            SavedCustomSelectedSchedule.Copy(SelectedPolicyToolIncommingShedule);

            //SelectedPolicyToolIncommingShedule.Mode = schedule.Mode;
            //SelectedPolicyToolIncommingShedule.IncomingScheduleID = schedule.IncomingScheduleID;
            //SelectedPolicyToolIncommingShedule.LicenseeID = schedule.LicenseeID;
            //SelectedPolicyToolIncommingShedule.PayorID = schedule.PayorID;
            //SelectedPolicyToolIncommingShedule.ScheduleTypeId = schedule.ScheduleTypeId;
            //SelectedPolicyToolIncommingShedule.CarrierID = schedule.CarrierID;
            //SelectedPolicyToolIncommingShedule.CoverageID = schedule.CoverageID;
            //SelectedPolicyToolIncommingShedule.ProductType = schedule.ProductType;
            //SelectedPolicyToolIncommingShedule.FirstYearPercentage = schedule.FirstYearPercentage;
            //SelectedPolicyToolIncommingShedule.RenewalPercentage = schedule.RenewalPercentage;
            //SelectedPolicyToolIncommingShedule.SplitPercentage = schedule.SplitPercentage;
            //SelectedPolicyToolIncommingShedule.Advance = schedule.Advance;
            //SelectedPolicyToolIncommingShedule.CreatedBy = schedule.CreatedBy;
            //SelectedPolicyToolIncommingShedule.CreatedOn = schedule.CreatedOn;
            //SelectedPolicyToolIncommingShedule.ModifiedBy = schedule.ModifiedBy;
            //SelectedPolicyToolIncommingShedule.ModifiedOn = schedule.ModifiedOn;
            //SelectedPolicyToolIncommingShedule.Mode = schedule.Mode;
            //SelectedPolicyToolIncommingShedule.CustomType = schedule.CustomType;
            SelectedPolicy.SplitPercentage = schedule.SplitPercentage;
            SelectedPolicy.Advance = schedule.Advance;
            //SelectedPolicyToolIncommingShedule.GradedSchedule = schedule.GradedSchedule;
            //SelectedPolicyToolIncommingShedule.NonGradedSchedule = schedule.NonGradedSchedule;
            SelectedIncomingMode = SelectedPolicyToolIncommingShedule.Mode == Mode.Custom ? "Custom" : "Standard";
            if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 1)//PercentageOfPremium
            {
                IncPercentOfPremium = true;
                IncPerHead = false;
            }
            else if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 2)//PerHead
            {
                IncPercentOfPremium = false;
                IncPerHead = true;
            }
        }
        void worker3_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));
            }
            catch
            {
            }
        }

        void worker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //SharedVMData.isRefeshAgentList = false;
        }


        private void ClearCollection()
        {
            try
            {
                VMPolicyCommission.PolicyIncomingPaymentCommissionDashBoard = null;
                VMPolicyCommission.CommissionDashBoardOutGoingPaymentLst = null;
                VMPolicyCommission.PolicyFollowUpCommissionDashBoardLst = null;
            }
            catch { }
            //VMPolicySmartField = null;
        }

        void SelectedPolicy_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //GradedList = null;
            //NonGradedList = null;
            if (objLog == null) objLog = new MastersClient();
            PolicyDetailsData _policytemp = sender as PolicyDetailsData;
            try
            {
                switch (e.PropertyName)
                {
                    case "OriginalEffectiveDate":
                        if (SelectedPolicy.IsManuallyChanged == false)//seeA
                        {
                            CalculatePolicyType();
                        }
                        break;
                    case "Payor":
                        //SelectedPolicy.PayorId =_policytemp.Payor!=null ?_policytemp.Payor.PayorID:Guid.Empty;
                        if (_policytemp.Payor == null)
                        {
                            FillSubmitedThrough(PayorsLst);
                            return;
                        }

                        SelectedPolicy.PayorId = _policytemp.Payor.PayorID;
                        SelectedPolicy.PayorNickName = _policytemp.Payor.NickName;
                        SelectedPolicy.PayorName = _policytemp.Payor.PayorName;
                        FillSubmitedThrough(PayorsLst);
                        FillCarrierLst();
                        #region "sort"
                        //sort by ASC
                        CarriersLst = new ObservableCollection<Carrier>(carriersLst.OrderBy(c => c.CarrierName).ToList());
                        //sort submitted through
                        List<string> submitthrow = new List<string>(SubmittedThroughLst);
                        submitthrow.Sort();
                        string strPayor = Convert.ToString(selectedPolicy.Payor.PayorName);
                        if (!string.IsNullOrEmpty(strPayor))
                        {
                            submitthrow.Remove(strPayor);
                            submitthrow.Insert(0, strPayor);
                        }
                        SubmittedThroughLst = new ObservableCollection<string>(submitthrow);
                        #endregion

                        break;
                    case "Carrier":
                        if (_policytemp.Carrier == null || _policytemp.Carrier.CarrierId == Guid.Empty)
                        {
                            SelectedPolicy.CarrierID = null;

                            SetEmptyProductType();
                            return;
                        }
                        SelectedPolicy.CarrierID = _policytemp.Carrier.CarrierId;
                        SelectedPolicy.CarrierName = _policytemp.Carrier.CarrierName;

                        SelectedPolicy.IsTrackIncomingPercentage = SelectedPolicy.Carrier.IsTrackIncomingPercentage;
                        SelectedPolicy.IsTrackMissingMonth = SelectedPolicy.Carrier.IsTrackMissingMonth;

                        //FillCoverageLst();
                        SelectedPolicy.CoverageId = _policytemp.CoverageId;
                        if (AllProducts != null)
                        {
                            SelectedPolicy.Coverage = AllProducts.Where(p => p.CoverageID == SelectedPolicy.CoverageId).FirstOrDefault();
                        }

                        if (SelectedCarrierChanged != null)
                            SelectedCarrierChanged(SelectedPolicy.Carrier);

                        break;
                    case "Coverage":

                        //if (isFirstTime == false)
                        //{
                        //    previousPolicyType = SelectedPolicy.PolicyType;
                        //    newPolicyType = SelectedPolicy.PolicyType;
                        //}

                        bool? IsManuallyChanged = SelectedPolicy.IsManuallyChanged;

                        //if (isFirstTime == false)//it also represents that PolicyType change event is not fired and page is loading
                        //{
                            if (IsManuallyChanged == false)
                            {
                                //if (SelectedPolicy.PolicyPreviousData != null)
                                //{
                                //    if (SelectedPolicy.PolicyPreviousData.PolicyType != SelectedPolicy.PolicyType)
                                //    {
                                CalculatePolicyType();
                                //    }
                                //}

                            }
                        //}
                        //else
                        //{
                        //    if (IsManuallyChanged == false && (previousPolicyType == newPolicyType))//(previousPolicyType == newPolicyType) this equal to stcIsPolicyTypeFunctionCalled
                        //    {
                        //        CalculatePolicyType();
                        //        //previousPolicyType = newPolicyType;
                        //    }
                        //}


                        if (_policytemp.Coverage == null || _policytemp.Coverage.CoverageID == Guid.Empty)
                        {
                            if (_policytemp.CoverageId == Guid.Empty || _policytemp.CoverageId == null)
                            {
                                SelectedPolicy.CoverageId = null;
                                //Set Produt type empty when no coverage is null
                                SetEmptyProductType();
                                return;
                            }
                            else
                            {
                                _policytemp.Coverage = serviceClients.CoverageClient.GetCoverageForPolicy((Guid)_policytemp.CoverageId);
                            }

                        }
                        SelectedPolicy.CoverageId = _policytemp.Coverage.CoverageID;
                        SelectedPolicy.CoverageName = _policytemp.Coverage.Name;
                        if (SelectedCoverageChanged != null)
                            SelectedCoverageChanged(_policytemp.Coverage);
                        //Load product type
                        FillProductType((Guid)SelectedPolicy.PayorId, (Guid)SelectedPolicy.CarrierID, (Guid)SelectedPolicy.CoverageId);

                        break;
                    case "SubmittedThrough":

                        break;
                    case "IsTrackIncomingPercentage":
                        bool ccc = SelectedPolicy.IsTrackIncomingPercentage;
                        break;
                    case "IsTrackMissingMonth":
                        bool ccc1 = SelectedPolicy.IsTrackMissingMonth;

                        break;
                    case "IncomingPaymentTypeId":
                        PayorIncomingSchedule schedule = serviceClients.GlobalIncomingScheduleClient.GetPayorScheduleDetails((Guid)SelectedPolicy.PayorId, (Guid)SelectedPolicy.CarrierID, (Guid)SelectedPolicy.CoverageId, SharedVMData.SelectedLicensee.LicenseeId, SelectedCoverageNickName.NickName, (int)SelectedPolicy.IncomingPaymentTypeId);
                        if (schedule != null && schedule.IncomingScheduleID != Guid.Empty)
                        {
                            MessageBoxResult _result = MessageBox.Show("Please note that there exists 'Incoming Schedule' configuration for selected payor. Do you want to use these settings?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                            if (_result == MessageBoxResult.Yes)
                            {
                                AssignPayorIncomingSchedule(schedule);
                            }
                        }
                        break;
                    case "SelectedIncomingMode":
                        //IsCustomOptionSelected = (SelectedIncomingMode == "Custom");
                        //if (SelectedPolicyToolIncommingShedule != null)

                        //{
                        //    SelectedPolicyToolIncommingShedule.Mode = (isCustomOptionSelected) ? Mode.Custom : Mode.Standard;
                        //    SelectedPolicyToolIncommingShedule.CustomType = SelectedPolicyToolIncommingShedule.Mode == Mode.Standard ? CustomMode.Graded : CustomMode.NonGraded;
                        //    IsGraded = (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded);
                        //}
                        //else
                        //{
                        //    IsGraded = true;
                        //}
                        //NewCustomRecord();
                        //break;
                        IsCustomOptionSelected = (SelectedIncomingMode == "Custom");
                        if (SelectedPolicyToolIncommingShedule != null)

                        {
                            SelectedPolicyToolIncommingShedule.Mode = (isCustomOptionSelected) ? Mode.Custom : Mode.Standard;
                            //SelectedPolicyToolIncommingShedule.CustomType = SelectedPolicyToolIncommingShedule.Mode == Mode.Standard ? CustomMode.Graded : CustomMode.NonGraded;
                            IsGraded = (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded);
                        }
                        else
                        {
                            IsGraded = true;
                        }
                        NewCustomRecord();
                        break;
                    case "IsGraded":
                        IsNonGraded = !IsGraded;
                        if (SelectedPolicyToolIncommingShedule != null)
                        {
                            SelectedPolicyToolIncommingShedule.CustomType = (IsGraded) ? CustomMode.Graded : CustomMode.NonGraded;
                        }
                        if (IsGraded && SelectedPolicyToolIncommingShedule.GradedSchedule != null)
                        {
                            SelectedGraded = SelectedPolicyToolIncommingShedule.GradedSchedule[0];
                            GradedList = SelectedPolicyToolIncommingShedule.GradedSchedule;
                        }
                        else if (!IsGraded && SelectedPolicyToolIncommingShedule.NonGradedSchedule != null)
                        {
                            SelectedNonGraded = SelectedPolicyToolIncommingShedule.NonGradedSchedule[0];
                            NonGradedList = SelectedPolicyToolIncommingShedule.NonGradedSchedule;
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("Selected policy prop changed exception: " + ex.Message);
            }
        }

        public void SaveLastViewedClientPolicyData()
        {
            try
            {
                Guid? PolicyId = null;
                bool IsSaved = true;

                if (SelectedPolicy != null)
                {
                    PolicyId = SelectedPolicy.PolicyId;
                    IsSaved = SelectedPolicy.IsSavedPolicy;
                }

                if (SelectedDisplayClient.ClientId != Guid.Empty && IsSaved)
                    LastViewPolicyClientCollection.LastViewedClients.ClientOrPolicyChanged(SelectedDisplayClient.ClientId, SelectedDisplayClient.Name, PolicyId, false);
            }
            catch
            {
            }
        }

        #endregion

        void SelectedOutGoingField_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OutGoingField.Where(p => p.IsPrimaryAgent).Count() != 1)
            {
                OutGoingField.ToList().ForEach(p => p.IsPrimaryAgent = false);
                SelectedOutGoingField.IsPrimaryAgent = true;

            }

            SelectedOutGoingField.PropertyChanged -= new PropertyChangedEventHandler(SelectedOutGoingField_PropertyChanged);
        }



        private string GetPolicayStatusName(int? StatusId)
        {
            string PolicyStatusdata = "";
            if (objLog == null) objLog = new MastersClient();
            try
            {
                switch (StatusId)
                {
                    case 0:
                        PolicyStatusdata = _PolicyStatus.Active.ToString();
                        break;
                    case 1:
                        PolicyStatusdata = _PolicyStatus.Terminated.ToString();
                        break;
                    case 2:
                        PolicyStatusdata = _PolicyStatus.Pending.ToString();
                        break;
                    case 3:
                        PolicyStatusdata = _PolicyStatus.Terminated.ToString();
                        break;
                    case 4:
                        PolicyStatusdata = _PolicyStatus.Delete.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" Exception : " + ex.Message);
            }
            return PolicyStatusdata;
        }

        private void FillCarrierLst()
        {
            if (objLog == null) objLog = new MastersClient();
            try
            {
                // if (SelectedPayor == null || SelectedPayor.PayorID == Guid.Empty)
                if (SelectedPolicy.Payor == null || SelectedPolicy.Payor.PayorID == null || SelectedPolicy.Payor.PayorID == Guid.Empty)
                    CarriersLst = new ObservableCollection<Carrier>();

                else
                // _PolicyServiceClient.CarrierClient.GetPayorCarriersOnlyAsync(SelectedPolicy.Payor.PayorID);
                {
                    PolicyDetailsData _ppolicy = new PolicyDetailsData();

                    CarriersLst = _PolicyServiceClient.CarrierClient.GetPayorCarriersOnly(SelectedPolicy.Payor.PayorID);
                    CarriersLst.Add(
                         new Carrier()
                         {
                             CarrierName = "",
                             CarrierId = Guid.Empty,
                             LicenseeId = SharedVMData.SelectedLicensee.LicenseeId,
                             //PayerId = SelectedPayor.PayorID,
                             PayerId = SelectedPolicy.Payor.PayorID,
                             IsDeleted = false,
                         });
                    //SelectedPolicy.CarrierID = _ppolicy.CarrierID;
                    //SelectedPolicy.CoverageId = _ppolicy.CoverageId;
                    if (CarriersLst != null)
                    {
                        if (CarriersLst.Count > 1)
                        {
                            //Set Default carrier if only one carrier is available
                            if (CarriersLst.Count == 2)
                            //SelectedPolicy.Carrier = CarriersLst.FirstOrDefault();  
                            {
                                if (SelectedPolicy.CarrierID != null)
                                {
                                    SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierId == SelectedPolicy.CarrierID).FirstOrDefault();
                                }
                                else
                                {
                                    if (SelectedPolicy.IsSavedPolicy)
                                    {
                                        SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierId == SelectedPolicy.CarrierID).FirstOrDefault();
                                    }
                                    else
                                    {
                                        SelectedPolicy.Carrier = CarriersLst.FirstOrDefault();
                                    }
                                }
                            }
                            else
                            {
                                SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierId == SelectedPolicy.CarrierID).FirstOrDefault();
                                //Select empty 
                                if (SelectedPolicy.Carrier == null)
                                {    //CarrierName = "",
                                    SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierName == "").FirstOrDefault();
                                    if (SelectedPolicy.Carrier != null)
                                    {
                                        SelectedPolicy.CarrierName = SelectedPolicy.Carrier.CarrierName;
                                    }
                                }
                            }
                        }
                        else
                            SelectedPolicy.Carrier = CarriersLst.FirstOrDefault();

                    }
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" Exception : " + ex.Message);
            }
        }

        //Get nick name of selected product
        private void FillProductType(Guid payorID, Guid carrierID, Guid coverageID)
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                AllCoverageNickName = new ObservableCollection<CoverageNickName>();
                string strNickName = string.Empty;
                if (SelectedPolicy != null)
                {
                    if (!string.IsNullOrEmpty(SelectedPolicy.CoverageName.Trim()))
                    {
                        AllCoverageNickName = new ObservableCollection<CoverageNickName>(serviceClients.CoverageClient.GetAllNickNames(payorID, carrierID, coverageID).ToList());
                        AllCoverageNickName = new ObservableCollection<CoverageNickName>(AllCoverageNickName.Where(p => p.CoverageID == coverageID).ToList());
                        //Inser empty value into product Type
                        CoverageNickName emptyCoverageNickName = new CoverageNickName
                        {
                            CoverageID = Guid.Empty,
                            PayorID = Guid.Empty,
                            CarrierID = Guid.Empty,
                            NickName = string.Empty,
                            IsDeleted = false
                        };
                        AllCoverageNickName.Insert(0, emptyCoverageNickName);
                        CoverageNickName covNickName = null;// Acme to handle pop up for incoming schedule change
                        //If Value found with null value
                        if (AllCoverageNickName.Count > 1)
                        {
                            //GET nick name from smart fields
                            //If found then match nick name with smart feilds nick names
                            //strNickName = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedCoverageNickName(SelectedPolicy.PolicyId, payorID, carrierID, coverageID);
                            //Get nick name from policy 
                            strNickName = SelectedPolicy.ProductType;
                            if (string.IsNullOrEmpty(strNickName))
                                strNickName = serviceClients.PolicyClient.GetPolicyProductType(SelectedPolicy.PolicyId, payorID, carrierID, coverageID);
                            //if nick name does not found into policy thenGet nick name from entry by deu 
                            if (string.IsNullOrEmpty(strNickName))
                                strNickName = serviceClients.DeuClient.GetProductTypeNickName(SelectedPolicy.PolicyId, payorID, carrierID, coverageID);


                            if (!string.IsNullOrEmpty(strNickName))

                                covNickName = AllCoverageNickName.Where(p => p.NickName.Trim().ToLower() == strNickName.Trim().ToLower()).FirstOrDefault();
                            else
                                covNickName = AllCoverageNickName.FirstOrDefault();
                        }
                        else
                        {
                            covNickName = AllCoverageNickName.FirstOrDefault();
                        }
                        SelectedPolicy.ProductType = covNickName.NickName;
                        SelectedCoverageNickName = covNickName;
                    }
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" Exception fillproductType: " + ex.Message);
            }
        }

        private void SetEmptyProductType()
        {
            try
            {
                AllCoverageNickName = new ObservableCollection<CoverageNickName>();
                //Inser empty value into product Type
                CoverageNickName emptyCoverageNickName = new CoverageNickName
                {
                    CoverageID = Guid.Empty,
                    PayorID = Guid.Empty,
                    CarrierID = Guid.Empty,
                    NickName = string.Empty,
                    IsDeleted = false
                };
                AllCoverageNickName.Insert(0, emptyCoverageNickName);
                SelectedCoverageNickName = AllCoverageNickName.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private ObservableCollection<CoverageNickName> _AllCoverageNickName;
        public ObservableCollection<CoverageNickName> AllCoverageNickName
        {
            get
            {
                return _AllCoverageNickName;
            }
            set
            {
                _AllCoverageNickName = value;
                OnPropertyChanged("AllCoverageNickName");
            }
        }

        private CoverageNickName _SelectedCoverageNickName;
        public CoverageNickName SelectedCoverageNickName
        {
            get
            {
                return _SelectedCoverageNickName;
            }
            set
            {
                _SelectedCoverageNickName = value;
                OnPropertyChanged("SelectedCoverageNickName");
            }
        }

        private string _strNickName;
        public string strNickName
        {
            get
            {
                return _strNickName;
            }
            set
            {
                _strNickName = value;
                OnPropertyChanged("strNickName");
            }
        }

        public void CarrierClient_GetPayorCarriersOnlyCompleted(object sender, GetPayorCarriersOnlyCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    // if (SelectedPolicy == null ) return;
                    // if (SelectedPolicy.Payor == null) return;
                    PolicyDetailsData _ppolicy = new PolicyDetailsData();

                    _ppolicy.CarrierID = SelectedPolicy.CarrierID;
                    _ppolicy.CoverageId = SelectedPolicy.CoverageId;

                    CarriersLst = e.Result;

                    SelectedPolicy.CarrierID = _ppolicy.CarrierID;
                    SelectedPolicy.CoverageId = _ppolicy.CoverageId;

                    if (CarriersLst != null)
                    {
                        if (CarriersLst.Count > 1)
                        {
                            SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierId == SelectedPolicy.CarrierID).FirstOrDefault();
                        }
                        else
                        {
                            SelectedPolicy.Carrier = CarriersLst.FirstOrDefault();
                        }

                    }
                    CarriersLst.Add(
                      new Carrier()
                      {
                          CarrierName = "",
                          CarrierId = Guid.Empty,
                          LicenseeId = SharedVMData.SelectedLicensee.LicenseeId,
                          //PayerId = SelectedPayor.PayorID,
                          PayerId = SelectedPolicy.Payor.PayorID,
                          IsDeleted = false,
                      });
                }
            }
            catch
            {
            }

        }



        private void UpdateUIByAddNewPolicy(PolicyDetailsData _selectedPolicy)
        {
            try
            {
                SelectedStatus = MasterPolicyStatus.Where(p => p.StatusId == SelectedPolicy.PolicyStatusId).FirstOrDefault();
                SelectedClient = DisplayedClientsLists.Where(p => p.ClientId == (SelectedPolicy.ClientId ?? Guid.Empty)).FirstOrDefault();
                SelectedPaymentMode = MasterPaymentsModeData.Where(p => p.ModeId == SelectedPolicy.PolicyModeId).FirstOrDefault();
                SelectedPolicy.Payor = PayorsLst.Where(p => p.PayorID == (SelectedPolicy.PayorId ?? Guid.Empty)).FirstOrDefault();

                if (SelectedPolicy.Payor != null)
                    SelectedPolicy.Carrier = CarriersLst.Where(p => p.CarrierId == (SelectedPolicy.CarrierID ?? Guid.Empty)).FirstOrDefault();

                SelectedPolicy.Coverage = AllProducts.Where(p => p.CoverageID == (SelectedPolicy.CoverageId ?? Guid.Empty)).FirstOrDefault();

                SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.FirstOrDefault(p => p.PaymentTypeId == SelectedPolicy.IncomingPaymentTypeId);
                SelectedTermReason = PolicyTerminationtReasonLst.FirstOrDefault(p => p.TerminationReasonId == SelectedPolicy.TerminationReasonId);

                IncPerHead = false;
                IncAdvance = false;
                IncPercentOfPremium = false;

                //acme 
                this.IsOutScheduleInvoice = true;

                SelectedPolicyToolIncommingShedule = null;

                OutPercentOfCommission = true;
                OutPercentOfPremium = false;
                OutAdvance = false;
                OutGoingField = null;
            }
            catch
            {
            }
        }

        #region Refresh

        public ICommand _FullRefreshCommand;
        public ICommand FullRefreshCommand
        {
            get
            {
                if (_FullRefreshCommand == null)
                {
                    _FullRefreshCommand = new BaseCommand(x => BeforeRefresh(), x => FullRefresh());
                }
                return _FullRefreshCommand;
            }
        }

        private bool BeforeRefresh()
        {
            if (ReferseEnable == false)
                return false;

            return true;

        }

        private bool _ReferseEnable;
        public bool ReferseEnable
        {
            get
            {
                return _ReferseEnable;
            }
            set
            {
                _ReferseEnable = value;
            }

        }

        public void FullRefresh()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {

                ReferseEnable = false;
                Guid? ClientId = null;
                if (SharedVMData.SelectedLicensee != null)
                {
                    var tempAllProducts = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);

                    DisplayedCoverage emptyCoverage = new DisplayedCoverage { CoverageID = Guid.Empty, Name = string.Empty };


                    if (AllProducts != null)
                    {
                        var productNames = new HashSet<string>(AllProducts.Select(x => x.Name));
                        var DiffProducts = tempAllProducts.Where(x => !productNames.Contains(x.Name));

                        if (SelectedPolicy != null)
                            SelectedPolicy.Coverage = AllProducts.Where(p => p.CoverageID == SelectedPolicy.CoverageId).FirstOrDefault();

                        AllProducts.AddRange(DiffProducts);
                        AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
                    }
                    //Call to updated client 

                    DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                    //  AllProducts.Add(emptyCoverage);
                    if (SelectedDisplayClient != null && SelectedDisplayClient.ClientId != Guid.Empty)
                    {
                        ClientId = SelectedDisplayClient.ClientId;
                        //Refresh smart fields
                        if (DisplayedClientsLists.Count > 0)
                        {
                            if (VMPolicySmartField != null)
                            {
                                VMPolicySmartField.DisplayedClientsLists = DisplayedClientsLists;
                                VMPolicySmartField.SelectedClientLrnd = selectedClient;
                            }
                        }
                    }
                    else
                    {
                        if (DisplayedClientsLists.Count > 0)
                        {
                            SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                            SelectedClient = SelectedDisplayClient;
                            VMPolicySmartField.DisplayedClientsLists = DisplayedClientsLists;
                            VMPolicySmartField.SelectedClientLrnd = selectedClient;
                        }
                    }
                    OnSelectedLicenseeChanged(ClientId, true);
                }

            }
            finally
            {
                //Thread.Sleep(30000);
                ReferseEnable = true;
                Mouse.OverrideCursor = null;
            }
        }

        public void Refresh()
        {
            try
            {
                LastViewPolicyClientCollection.LastViewedClients.Refresh();
                SharedVMData.RefreshLicensees();
                if (SelectedPolicyChanged != null && SelectedPolicy != null)
                    //SelectedPolicyChanged(SelectedPolicy);
                    OutgoingPayeeList = FillOutgoingPayeeUser();
            }
            catch
            {
            }
        }

        public bool RefreshRequired
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region "Policy import functionality"

        //private ICommand _CmdCheckImportFormat;
        //public ICommand CmdCheckImportFormat
        //{
        //    get
        //    {
        //        if (_CmdCheckImportFormat == null)
        //        {

        //            _CmdCheckImportFormat = new BaseCommand(X => CheckImportFormat());
        //        }
        //        return _CmdCheckImportFormat;
        //    }

        //}

        //private ICommand _CmdImportPolicy;
        //public ICommand CmdImportPolicy
        //{
        //    get
        //    {
        //        if (_CmdImportPolicy == null)
        //        {

        //            _CmdImportPolicy = new BaseCommand(X => DoImportPolicy());
        //        }
        //        return _CmdImportPolicy;
        //    }

        //}

        private void ShowHideImportPolicyButton()
        {
            if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
            {
                IspolicyImportVisible = Visibility.Visible;
            }
            else
            {
                IspolicyImportVisible = Visibility.Hidden;
            }
        }

        private Visibility _IspolicyImportVisible;
        public Visibility IspolicyImportVisible
        {
            get
            {
                return _IspolicyImportVisible;
            }
            set
            {
                _IspolicyImportVisible = value;
                OnPropertyChanged("IspolicyImportVisible");
            }
        }


        public delegate void delgateOpenImportPolicy();
        public event delgateOpenImportPolicy OpenImportPolicy;

        public delegate void delgateCloseImportPolicy();

        /*    string GetValue(SpreadsheetDocument doc, Cell cell)
            {
                string value = cell.CellValue.InnerText;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
                }
                return value;
            } 
            DataTable ImportXLS()
            {
                DataTable dt = new DataTable();
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                {
                    //Read the first Sheet from Excel file.
                    Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                    //Get the Worksheet instance.
                    Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

                    //Fetch all the rows present in the Worksheet.
                    IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                    //Create a new DataTable.


                    //Loop through the Worksheet rows.
                    foreach (Row row in rows)
                    {
                        //Use the first row to add columns to DataTable.
                        if (row.RowIndex.Value == 1)
                        {
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                dt.Columns.Add(GetValue(doc, cell));
                            }
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                                i++;
                            }
                        }
                    }
                }
                return dt;
            }*/

        #endregion

        #region Copy Section

        private bool FollowUpcalled;

        private bool replaceRbStatus = true;

        public bool ReplaceRbStatus
        {
            get { return replaceRbStatus; }
            set { replaceRbStatus = value; OnPropertyChanged("ReplaceRbStatus"); }
        }

        private bool newRbStatus = true;
        public bool NewRbStatus
        {
            get { return newRbStatus; }
            set { newRbStatus = value; OnPropertyChanged("NewRbStatus"); }
        }

        public delegate void popUpAgentWindow();
        public event popUpAgentWindow OpenAgentWindow;

        public delegate void CloseUpAgentWindow();
        public event CloseUpAgentWindow CloseAgentWindow;

        #endregion

        #region MasterData

        private PolicyStatus selectedStatus;
        public PolicyStatus SelectedStatus
        {
            get { return selectedStatus == null ? new PolicyStatus() : selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); }
        }

        private ObservableCollection<PolicyStatus> masterPolicyStatus;
        public ObservableCollection<PolicyStatus> MasterPolicyStatus
        {
            get
            {
                return masterPolicyStatus;
            }
            set
            {
                masterPolicyStatus = value;
                OnPropertyChanged("MasterPolicyStatus");
            }
        }

        private PolicyMode selectedPaymentMode;
        public PolicyMode SelectedPaymentMode
        {
            get { return selectedPaymentMode == null ? new PolicyMode() : selectedPaymentMode; }
            set { selectedPaymentMode = value; OnPropertyChanged("SelectedPaymentMode"); }
        }

        private ObservableCollection<PolicyMode> masterPaymentsModeData;
        public ObservableCollection<PolicyMode> MasterPaymentsModeData
        {
            get
            {
                return masterPaymentsModeData;
            }
            set
            {
                masterPaymentsModeData = value;
                OnPropertyChanged("MasterPaymentsModeData");
            }
        }

        private PolicyTerminationReason selectedTermReason;
        public PolicyTerminationReason SelectedTermReason
        {
            get { return selectedTermReason == null ? new PolicyTerminationReason() : selectedTermReason; }
            set { selectedTermReason = value; OnPropertyChanged("SelectedTermReason"); }
        }

        private ObservableCollection<PolicyTerminationReason> policyTerminationtReasonLst;
        public ObservableCollection<PolicyTerminationReason> PolicyTerminationtReasonLst
        {
            get
            {
                return policyTerminationtReasonLst;
            }
            set
            {
                policyTerminationtReasonLst = value;
                OnPropertyChanged("PolicyTerminationtReasonLst");
            }

        }

        private PolicyIncomingPaymentType selectedMasterIncomingPaymentType;
        public PolicyIncomingPaymentType SelectedMasterIncomingPaymentType
        {
            get { return selectedMasterIncomingPaymentType == null ? new PolicyIncomingPaymentType() : selectedMasterIncomingPaymentType; }
            set { selectedMasterIncomingPaymentType = value; OnPropertyChanged("SelectedMasterIncomingPaymentType"); }
        }

        private PolicyIncomingPaymentType importMasterIncomingPaymentType;
        public PolicyIncomingPaymentType ImportMasterIncomingPaymentType
        {
            get { return importMasterIncomingPaymentType == null ? new PolicyIncomingPaymentType() : importMasterIncomingPaymentType; }
            set { importMasterIncomingPaymentType = value; OnPropertyChanged("ImportMasterIncomingPaymentType"); }
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

        #region Payor/Carrier/Product
        private ObservableCollection<DisplayedPayor> payorslst;
        public ObservableCollection<DisplayedPayor> PayorsLst
        {
            get
            {
                return payorslst;
            }
            set
            {
                payorslst = value;
                OnPropertyChanged("PayorsLst");
            }
        }


        private ObservableCollection<Carrier> carriersLst;
        public ObservableCollection<Carrier> CarriersLst
        {
            get { return carriersLst; }
            set { carriersLst = value; OnPropertyChanged("CarriersLst"); }
        }


        private ObservableCollection<DisplayedCoverage> _AllProducts;
        public ObservableCollection<DisplayedCoverage> AllProducts
        {
            get
            {
                return _AllProducts;
            }
            set
            {
                _AllProducts = value;
                OnPropertyChanged("AllProducts");
            }
        }




        private ObservableCollection<string> submittedThroughLst;
        public ObservableCollection<string> SubmittedThroughLst
        {
            get { return submittedThroughLst; }
            set { submittedThroughLst = value; OnPropertyChanged("SubmittedThroughLst"); }
        }

        private User outgoingselectedPayee;
        public User OutgoingSelecetdPayee
        {

            get { return outgoingselectedPayee ?? new User(); }
            set { outgoingselectedPayee = value; OnPropertyChanged("OutgoingSelecetdPayee"); }
        }

        private ObservableCollection<User> outgoingPayeeList;
        public ObservableCollection<User> OutgoingPayeeList
        {
            get { return outgoingPayeeList; }
            set { outgoingPayeeList = value; OnPropertyChanged("OutgoingPayeeList"); }
        }

        private User _SelectedPrimaryAgent;
        public User SelectedPrimaryAgent
        {

            get { return _SelectedPrimaryAgent; }
            set { _SelectedPrimaryAgent = value; OnPropertyChanged("SelectedPrimaryAgent"); }
        }

        private ObservableCollection<User> _AccountExecLst;
        public ObservableCollection<User> AccountExecLst
        {
            get { return _AccountExecLst; }
            set { _AccountExecLst = value; OnPropertyChanged("AccountExecLst"); }
        }

        private User _SelectedAccountExecLst;
        public User SelectedAccountExecLst
        {

            get { return _SelectedAccountExecLst; }
            set { _SelectedAccountExecLst = value; OnPropertyChanged("SelectedAccountExecLst"); }
        }

        private ObservableCollection<User> _PrimaryAgents;
        public ObservableCollection<User> PrimaryAgents
        {
            get { return _PrimaryAgents; }
            set { _PrimaryAgents = value; OnPropertyChanged("PrimaryAgents"); }
        }

        #endregion

        public void DeleteSelectedClient()
        {
            int x = DisplayedClientsLists.IndexOf(SelectedDisplayClient);
            int i = PolicyList.Count(s => s.ClientId == selectedDisplayClient.ClientId);

            if (i <= 1)
            {
                MessageBoxResult _result = MessageBox.Show("Do you want to Delete the Client?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (_result == MessageBoxResult.OK)
                {
                    DisplayedClientsLists.Remove(SelectedDisplayClient);

                    SelectedDisplayClient = DisplayedClientsLists.ElementAtOrDefault(x);
                    if (SelectedDisplayClient.ClientId == Guid.Empty)
                    {
                        SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                    }
                    SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                }
            }
            PolicyList.Remove(SelectedPolicy);
        }

        public void OnChangeLastViewdClient()
        {

        }

        #region PolicyIncomingBasicSchedule

        private PolicyToolIncommingShedule selectedpolicyincomingschedule;
        public PolicyToolIncommingShedule SelectedPolicyToolIncommingShedule
        {
            get
            {
                return selectedpolicyincomingschedule;
            }
            set
            {
                selectedpolicyincomingschedule = value;
                OnPropertyChanged("SelectedPolicyToolIncommingShedule");
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

        //string lastModifiedDetail = "";

        //public string LastModifiedDetail
        //{
        //    get { return lastModifiedDetail; }
        //    set
        //    {
        //        lastModifiedDetail = value;
        //        OnPropertyChanged("LastModifiedDetail");
        //    }
        //}

        private bool _IncAdvance;
        public bool IncAdvance
        {
            get { return _IncAdvance; }
            set { _IncAdvance = value; OnPropertyChanged("IncAdvance"); }
        }
        private PolicyToolIncommingShedule FillIncomingBasicSchedule()
        {
            try
            {
                if (SelectedPolicy == null) return null;
                PolicyToolIncommingShedule _PolicyToolIncommingShedule = serviceClients.PolicyIncomingScheduleClient.GettingPolicyIncomingSchedule(SelectedPolicy.PolicyId);
                return _PolicyToolIncommingShedule;
            }
            catch
            {
                return null;
            }


        }
        #endregion

        #region PolicyBasicOutGoing

        private ICommand _cmdOk;
        public ICommand CmdOk
        {
            get
            {
                if (_cmdOk == null)
                {
                    _cmdOk = new BaseCommand(x => BeforeAddPayeeInShedule(), x => AddPayeeInShedule());

                }
                return _cmdOk;
            }

        }

        private bool BeforeAddPayeeInShedule()
        {
            if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            if (SelectedPolicy == null)
                return false;

            return true;
        }

        private void AddPayeeInShedule()
        {
            ObservableCollection<AddPayee> tempAddPayee = new ObservableCollection<AddPayee>();
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                System.Windows.Forms.MessageBox.Show("No Policy is Selected", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }
            if (AddPayee.Count == 0)
            {
                if (CloseAgentWindow != null) CloseAgentWindow();
                return;
            }

            foreach (AddPayee i in AddPayee.ToList())
            {
                if (!tempAddPayee.Contains(i) && i.IsSelect == true)

                    tempAddPayee.Add(i);
            }
            foreach (AddPayee i in tempAddPayee.ToList())
            {
                ObservableCollection<User> AgentList = new ObservableCollection<User>();
                User _user = new User();
                if (SharedVMData.GlobalAgentList != null)
                {
                    if (SharedVMData.GlobalAgentList.Count > 0)
                    {
                        AgentList = SharedVMData.GlobalAgentList;
                    }
                    else
                    {
                        AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    }
                }
                else
                {
                    AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                }

                _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.UserCredentialID == i.UserCiD).FirstOrDefault();

                if (_user == null)
                {
                    AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.UserCredentialID == i.UserCiD).FirstOrDefault();
                }

                if (i.IsSelect != true) continue;

                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                //bool IsThrprimaryAgent = (OutGoingField.Where(p => p.IsPrimaryAgent == true).Count() > 0);
                //if (tempAddPayee.Count == 1 && !IsThrprimaryAgent) OutgoingRecord.IsPrimaryAgent = i.IsSelect;
                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                OutgoingRecord.FirstYearPercentage = _user.FirstYearDefault;
                OutgoingRecord.RenewalPercentage = _user.RenewalDefault;
                OutgoingRecord.Payor = i.NickName;
                OutgoingRecord.PayeeUserCredentialId = i.UserCiD;
                OutgoingRecord.PolicyId = SelectedPolicy.PolicyId;
                OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                OutGoingField.Add(OutgoingRecord);

                if (!IsCustomDateSelected) //Acme - check to apply this only for default case 
                {
                    AddPayee.Remove(i);
                    tempAddPayee.Remove(i);
                }


            }
            SelectedOutGoingField = OutGoingField.FirstOrDefault();
            if (CloseAgentWindow != null) CloseAgentWindow();

        }
        private OutGoingPayment _SelectedOutGoingField;
        public OutGoingPayment SelectedOutGoingField
        {
            get
            {
                return _SelectedOutGoingField == null ? new OutGoingPayment() : _SelectedOutGoingField;

            }
            set
            {
                _SelectedOutGoingField = value;
                OnPropertyChanged("SelectedOutGoingField");
            }
        }
        private ObservableCollection<OutGoingPayment> _OutGoingField;
        public ObservableCollection<OutGoingPayment> OutGoingField
        {
            get
            {
                return _OutGoingField;

            }
            set
            {
                _OutGoingField = value;
                OnPropertyChanged("OutGoingField");
            }
        }

        private string _PolicyAddPayeeToolTip;
        public string PolicyAddPayeeToolTip
        {
            get
            {
                return _PolicyAddPayeeToolTip;

            }
            set
            {
                _PolicyAddPayeeToolTip = value;
                OnPropertyChanged("PolicyAddPayeeToolTip");
            }
        }

        private string _policyDetailSaveToolTip;
        public string PolicyDetailSaveToolTip
        {
            get
            {
                return _policyDetailSaveToolTip;

            }
            set
            {
                _policyDetailSaveToolTip = value;
                OnPropertyChanged("PolicyDetailSaveToolTip");
            }
        }

        private ICommand _OpenCAgentWindow;
        public ICommand OpenCAgentWindow
        {
            get
            {
                if (_OpenCAgentWindow == null)
                {
                    _OpenCAgentWindow = new BaseCommand(x => BeforeOpenWindowAgent(), x => OpenWindowAgent());

                }
                return _OpenCAgentWindow;

            }

        }

        private bool BeforeOpenWindowAgent()
        {
            if (PolicyAddPayeeToolTip != null)
                return false;

            return true;
        }

        private void OpenWindowAgent()
        {
            if (OpenAgentWindow != null)
            {
                AddPayeeInGrid();
                OpenAgentWindow();
            }
        }

        private ObservableCollection<AddPayee> _addPayee;
        public ObservableCollection<AddPayee> AddPayee
        {
            get
            {
                return _addPayee == null ? new ObservableCollection<AddPayee>() : _addPayee;
            }
            set
            {
                _addPayee = value;
                OnPropertyChanged("AddPayee");
            }
        }

        private ICommand _DeletePayee;
        public ICommand DeletePayee
        {
            get
            {
                if (_DeletePayee == null)
                {
                    _DeletePayee = new BaseCommand(X => BeforeDeleteSelectPayee(), X => DeleteSelectPayee());
                }
                return _DeletePayee;
            }

        }

        private bool BeforeDeleteSelectPayee()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else if (OutGoingField == null || OutGoingField.Count == 0)
            {
                return false;
            }
            else
                return true;
        }

        ObservableCollection<OutGoingPayment> _deleteseletedOutGoingField = new ObservableCollection<OutGoingPayment>();
        private void DeleteSelectPayee()
        {
            //MastersClient c = new MastersClient();
            if (objLog == null)
                objLog = new MastersClient();
            string selectItem = string.Empty;
            try
            {
                if (!OutGoingField.Contains(SelectedOutGoingField)) return;
                //if ((OutGoingField != null && OutGoingField.Count() != 0) && OutGoingField.Count() != 1)
                //{
                //    if (SelectedOutGoingField.IsPrimaryAgent)
                //    {
                //        MessageBoxResult mres1 = MessageBox.Show("Cannot delete a Primary Agent", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                //        return;
                //    }
                //}

                MessageBoxResult mres = MessageBox.Show("Do you want to delete the seleted record", "Confirm", MessageBoxButton.YesNo);
                if (MessageBoxResult.Yes == mres)
                {
                    objLog.AddLog(" DeleteSelectPayee request: " + SelectedOutGoingField.OutgoingScheduleId);
                    _deleteseletedOutGoingField.Add(SelectedOutGoingField);
                    OutGoingField.Remove(SelectedOutGoingField);

                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" DeleteSelectPayee exception : " + ex.Message);
            }
        }


        private void AddPayeeInGrid()
        {
            try
            {

                ObservableCollection<User> AgentList = new ObservableCollection<User>();
                AddPayee = new ObservableCollection<AddPayee>();
                List<User> _user = new List<User>();
                if (OutGoingField == null)
                    OutGoingField = new ObservableCollection<OutGoingPayment>();
                //List<User> _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.IsHouseAccount == false).ToList<User>();
                if (SharedVMData.isRefeshAgentList)
                {
                    AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.IsHouseAccount == false).ToList<User>();
                    SharedVMData.isRefeshAgentList = false;
                }
                else
                {
                    if (SharedVMData.GlobalAgentList.Count > 0)
                    {
                        _user = SharedVMData.GlobalAgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.IsHouseAccount == false).ToList<User>();
                    }
                    else
                    {
                        AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                        _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.IsHouseAccount == false).ToList<User>();
                    }
                }

                foreach (User Ap in _user)
                {
                    //Acme added following check to avoid removing payees from list
                    if (!IsCustomDateSelected)
                    {
                        int num = OutGoingField.Where(p => p.PayeeUserCredentialId == Ap.UserCredentialID).Count();
                        if (num == 1) continue;
                    }
                    AddPayee Payee = new AddPayee();
                    Payee.FirstName = Ap.FirstName;
                    Payee.LastName = Ap.LastName;
                    Payee.NickName = Ap.NickName;
                    Payee.Company = Ap.Company;
                    Payee.UserCiD = Ap.UserCredentialID;
                    Payee.IsHouse = false;
                    AddPayee.Add(Payee);
                }

                AddPayee = new ObservableCollection<AddPayee>(AddPayee.OrderBy(p => p.NickName));
                SelectedPayee = AddPayee.FirstOrDefault();
            }
            catch
            {
            }

        }
        private AddPayee _SelectedPayee;
        public AddPayee SelectedPayee
        {
            get
            {
                return _SelectedPayee == null ? new AddPayee() : _SelectedPayee;

            }

            set
            {
                _SelectedPayee = value;
                OnPropertyChanged("SelectedPayee");
            }
        }
        private ICommand _SplitEvently;
        public ICommand SplitEvently
        {
            get
            {
                if (_SplitEvently == null)
                {
                    _SplitEvently = new BaseCommand(x => BeforeDoSplitEvently(), x => DoSplitEvently());
                }
                return _SplitEvently;
            }
        }

        private bool BeforeDoSplitEvently()
        {

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read || IsCustomDateSelected) //Acme - Added 2nd condition
                return false;

            /* Acme - condition removed after primary agent moved to policy level 
             * else if (OutGoingField == null || OutGoingField.Where(p => p.IsPrimaryAgent == true).Count() != 1)
            {
                return false;
            }
             * */

            else
                return true;
        }

        private void DoSplitEvently()
        {
            try
            {
                if (OutGoingField.Count == 0) return;
                int totalpayee = OutGoingField.Count();

                //Added March 13, 2019 - for IsTiered schedule implementation
                //Check if tier 2 present in outgoing schedule or not
                bool isTier2Present = OutGoingField.Where(x => x.TierNumber == 2).ToList().Count > 0;

                if (IsTiered)
                {
                    totalpayee = OutGoingField.Where(x => ((isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1))).ToList().Count();
                }

                if (OutPercentOfCommission)
                {
                    CalcOutGoingShedule(totalpayee);
                }
                if (OutPercentOfPremium)
                {
                    CalcOutGoingSheduleOnPremium(totalpayee);

                }

                FirstYear = Math.Round((FirstYear.Value * 100) / 100, 2);
                Renewal = Math.Round((Renewal.Value * 100) / 100, 2);

                //double? Firstyrtotalsum = OutGoingField.Sum(p => p.FirstYearPercentage);
                //double? secondyrtotalsum = OutGoingField.Sum(p => p.RenewalPercentage);
                //if (OutPercentOfCommission)
                //{
                //    Firstyrtotalsum = 100;
                //    secondyrtotalsum = 100;
                //}
                //else if (OutPercentOfPremium)
                //{
                //    Firstyrtotalsum = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
                //    secondyrtotalsum = SelectedPolicyToolIncommingShedule.RenewalPercentage;
                //}
                OutGoingField.Where(x => (!IsTiered || (IsTiered &&
                (isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1))
                )).ToList().ForEach(p => p.FirstYearPercentage = FirstYear);

                OutGoingField.Where(x => (!IsTiered || (IsTiered &&
                (isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1))
                )).ToList().ForEach(p => p.RenewalPercentage = Renewal);


                /*if (OutGoingField.Count == 0) return;
                double totalpayee = 0;
                if (OutPercentOfCommission)
                {
                    CalcOutGoingShedule(OutGoingField.Count());
                }
                if (OutPercentOfPremium)
                {
                    CalcOutGoingSheduleOnPremium(OutGoingField.Count());

                }
                totalpayee = OutGoingField.Count();

                FirstYear = Math.Round((FirstYear.Value * 100) / 100, 2);
                Renewal = Math.Round((Renewal.Value * 100) / 100, 2);

                double? Firstyrtotalsum = OutGoingField.Sum(p => p.FirstYearPercentage);
                double? secondyrtotalsum = OutGoingField.Sum(p => p.RenewalPercentage);
                if (OutPercentOfCommission)
                {
                    Firstyrtotalsum = 100;
                    secondyrtotalsum = 100;
                }
                else if (OutPercentOfPremium)
                {
                    Firstyrtotalsum = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
                    secondyrtotalsum = SelectedPolicyToolIncommingShedule.RenewalPercentage;
                }
                OutGoingField.ToList().ForEach(p => p.FirstYearPercentage = FirstYear);
                OutGoingField.ToList().ForEach(p => p.RenewalPercentage = Renewal);


                //Acme- commented as primary agent isat policy level now 
                ////PrimaryAgentFirstYrAmount = Firstyrtotalsum - (FirstYear * totalpayee);
                ////PrimaryAgentRenewYrAmount = secondyrtotalsum - (Renewal * totalpayee);
                ////OutGoingField.Where(p => p.IsPrimaryAgent).FirstOrDefault().FirstYearPercentage += PrimaryAgentFirstYrAmount;
                ////OutGoingField.Where(p => p.IsPrimaryAgent).FirstOrDefault().RenewalPercentage += PrimaryAgentRenewYrAmount;
                ///
                /// */

            }
            catch (Exception ex)
            {
            }
        }

        #region Custom outgoing schedule
        bool IsUseFirstYear(DateTime date)
        {

            if (SelectedPolicy.OriginalEffectiveDate != null)
            {
                //Changed by Acme on March 21 - 2014. as per Eric's feedback to correctthe logic
                //if ((_invoicedate >= EffDate.Value) && (_invoicedate <= EffDate.Value.AddYears(1)))
                if ((date >= SelectedPolicy.OriginalEffectiveDate.Value) && (date < SelectedPolicy.OriginalEffectiveDate.Value.AddYears(1)))
                {
                    return true;
                }
                else //if (date >= EffDate.Value.AddYears(1))
                {
                    return false;
                    //Flag = FirstYrRenewalYr.Renewal;
                }
            }
            else
                return true;
        }
        void AddHouseInCustomSchedule(User _user)
        {
            try
            {
                if (OutGoingField == null || (OutGoingField != null && OutGoingField.Count == 0))
                {
                    OutGoingField = new ObservableCollection<OutGoingPayment>();
                    if (OutPercentOfCommission)
                    {
                        SelectedOutGoingField = new OutGoingPayment();
                        //SelectedOutGoingField.IsPrimaryAgent = false;
                        SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                        SelectedOutGoingField.SplitPercent = 100;
                        SelectedOutGoingField.Payor = _user.NickName;
                        SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                        SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                        SelectedOutGoingField.ScheduleTypeId = 2;
                        OutGoingField.Add(SelectedOutGoingField);
                    }
                    else if (OutPercentOfPremium)
                    {
                        SelectedOutGoingField = new OutGoingPayment();
                        SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();

                        //To confirm from kevin - when this is first entry, assuming first year %
                        SelectedOutGoingField.SplitPercent = SelectedPolicyToolIncommingShedule.FirstYearPercentage;

                        SelectedOutGoingField.Payor = _user.NickName;
                        SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                        SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                        SelectedOutGoingField.ScheduleTypeId = 1;
                        OutGoingField.Add(SelectedOutGoingField);
                    }
                    return;
                }

                //Get distinct dates list
                var sdates = (from s in OutGoingField
                              select new { s.CustomStartDate }).Distinct().OrderBy(x => x.CustomStartDate).ToList();

                //when no dates present i.e. start date not specified with any of payees 
                foreach (var i in sdates)
                {
                    //Changes to add house in tiered custom schedule
                    if (IsTiered)
                    {
                        //check if house already present in any tiered schedule more than twice
                        var allAgents = (from s in OutGoingField where s.CustomStartDate == i.CustomStartDate && s.PayeeUserCredentialId == _user.UserCredentialID select s).ToList();
                        //if so, continue
                        if (allAgents.Count == 2)
                            continue;
                    }
                    else
                    {
                        //check if house already present in any schedule
                        var allAgents = (from s in OutGoingField where s.CustomStartDate == i.CustomStartDate && s.PayeeUserCredentialId == _user.UserCredentialID select s).ToList();
                        //if so, continue
                        if (allAgents.Count > 0)
                            continue;
                    }

                    //Else - go with logic 
                    if (OutPercentOfCommission)
                    {
                        double split = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                        split = 100 - split;
                        SelectedOutGoingField = new OutGoingPayment();
                        SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                        SelectedOutGoingField.Payor = _user.NickName;
                        SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                        SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                        SelectedOutGoingField.SplitPercent = split;
                        SelectedOutGoingField.CustomStartDate = i.CustomStartDate;
                        SelectedOutGoingField.ScheduleTypeId = 2;
                        OutGoingField.Add(SelectedOutGoingField);
                    }
                    else if (OutPercentOfPremium)
                    {
                        double split = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                        //to confirm from kevin
                        bool isFirstYear = (i.CustomStartDate == null) ? true : IsUseFirstYear(i.CustomStartDate.Value);
                        split = (isFirstYear) ? (SelectedPolicyToolIncommingShedule.FirstYearPercentage ?? 0) - split : (SelectedPolicyToolIncommingShedule.RenewalPercentage ?? 0) - split;

                        SelectedOutGoingField = new OutGoingPayment();
                        SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                        SelectedOutGoingField.SplitPercent = split;
                        SelectedOutGoingField.Payor = _user.NickName;
                        SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                        SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                        SelectedOutGoingField.ScheduleTypeId = 1;
                        SelectedOutGoingField.CustomStartDate = i.CustomStartDate;
                        OutGoingField.Add(SelectedOutGoingField);
                    }
                }
                List<OutGoingPayment> lst = new List<OutGoingPayment>(OutGoingField);
                OutGoingField = new ObservableCollection<OutGoingPayment>(lst.OrderBy(x => x.CustomStartDate));
                //OutGoingField.OrderBy(x => x.CustomStartDate);

            }
            catch (Exception ex)
            {
                objLog.AddLog("Exception adding house in custom schedule: " + ex.Message);
            }
        }


        #endregion
        public void CalcOutGoingShedule(int totalPayee)
        {
            if (totalPayee == 0) return;

            if (OutPercentOfCommission) FirstYear = ((double)100.0 / totalPayee);

            if (OutPercentOfCommission) Renewal = ((double)100.0 / totalPayee);
        }
        public void CalcOutGoingSheduleOnPremium(int totalPayee)
        {
            double? Firstyrtotalsum = SelectedPolicyToolIncommingShedule.FirstYearPercentage ?? 0;
            double? secondyrtotalsum = SelectedPolicyToolIncommingShedule.RenewalPercentage ?? 0;

            if (SelectedPolicyToolIncommingShedule.Mode == Mode.Custom && SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded)
            {
                if (SelectedPolicyToolIncommingShedule.NonGradedSchedule == null || (SelectedPolicyToolIncommingShedule.NonGradedSchedule != null && SelectedPolicyToolIncommingShedule.NonGradedSchedule.Count == 0))
                {
                    Firstyrtotalsum = 0;
                    secondyrtotalsum = 0;
                    return;
                }

                int year = getPolicyAgeFromEffective(DateTime.Now);
                Firstyrtotalsum = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == 1).FirstOrDefault().Percent;

                int maxYear = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderByDescending(x => x.Year).FirstOrDefault().Year;
                year = (year > maxYear) ? maxYear : year;

                secondyrtotalsum = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderBy(x => x.Year).Where(x => x.Year == year).FirstOrDefault().Percent;

                //if (secondyrtotalsum == null || secondyrtotalsum == 0)
                //{
                //    secondyrtotalsum = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == maxYear).FirstOrDefault().Percent;
                //}

            }

            FirstYear = Firstyrtotalsum / totalPayee;
            Renewal = secondyrtotalsum / totalPayee;
            PrimaryAgentFirstYrAmount = Firstyrtotalsum - (FirstYear * Convert.ToDouble(totalPayee));
            PrimaryAgentRenewYrAmount = secondyrtotalsum - (Renewal * Convert.ToDouble(totalPayee));
        }
        private ICommand _AddHouse;
        public ICommand AddHouse
        {
            get
            {
                if (_AddHouse == null)
                {
                    _AddHouse = new BaseCommand(x => BeforeAddHouseInShedule(), x => AddHouseInShedule());
                }
                return _AddHouse;
            }
        }

        private bool BeforeAddHouseInShedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Method to return house account for selected licensee 
        /// </summary>
        /// <returns></returns>
        User GetHouseAccount()
        {
            User _user = new User();
            if (SharedVMData.GlobalAgentList.Count > 0)
            {
                _user = SharedVMData.GlobalAgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                if (_user == null)
                {
                    ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                }
            }
            else
            {
                ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
            }
            return _user;
        }

        private void AddHouseInShedule()
        {
            try
            {

                User _user = new User();

                if (SharedVMData.isRefeshAgentList)
                {
                    ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                    SharedVMData.isRefeshAgentList = false;
                }
                else
                {
                    _user = GetHouseAccount();
                    //Commented by Acme - while tiered schedule implementation 

                    //if (SharedVMData.GlobalAgentList.Count > 0)
                    //{
                    //    _user = SharedVMData.GlobalAgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                    //    if (_user == null)
                    //    {
                    //        ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    //        _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                    //    }
                    //}
                    //else
                    //{
                    //    ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
                    //    _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
                    //}
                }

                if (_user == null) return;

                if (IsCustomDateSelected)
                {
                    AddHouseInCustomSchedule(_user);
                }
                else
                {

                    if (OutGoingField == null)
                    {
                        OutGoingField = new ObservableCollection<OutGoingPayment>();
                    }
                    //Acme - modified to include isTiered condition on house account, not to be more than twice for tiered 
                    if (OutGoingField.Where(p => (!IsTiered && p.PayeeUserCredentialId == _user.UserCredentialID)).Count() >= 1 ||
                        OutGoingField.Where(p => (IsTiered && p.PayeeUserCredentialId == _user.UserCredentialID)).Count() == 2)
                    {
                        return;
                    }


                    double firstyear = 0.0;
                    double renewal = 0.0;
                    firstyear = _user.FirstYearDefault;
                    renewal = _user.RenewalDefault;

                    if (OutPercentOfCommission)
                    {
                        if (OutGoingField.Count == 0)
                        {
                            SelectedOutGoingField = new OutGoingPayment();
                            // SelectedOutGoingField.IsPrimaryAgent = false;
                            SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                            SelectedOutGoingField.FirstYearPercentage = 100;
                            SelectedOutGoingField.RenewalPercentage = 100;
                            SelectedOutGoingField.Payor = _user.NickName;
                            SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                            SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                            SelectedOutGoingField.ScheduleTypeId = 2;
                            OutGoingField.Add(SelectedOutGoingField);
                        }
                        else
                        {
                            double fyr = OutGoingField.Where(p => p.FirstYearPercentage != null).Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
                            double ryn = OutGoingField.Where(p => p.RenewalPercentage != null).Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
                            fyr = 100 - fyr;
                            ryn = 100 - ryn;
                            SelectedOutGoingField = new OutGoingPayment();
                            //           SelectedOutGoingField.IsPrimaryAgent = false;
                            SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                            SelectedOutGoingField.FirstYearPercentage = (IsTiered) ? 0 : fyr;
                            SelectedOutGoingField.RenewalPercentage = (IsTiered) ? 0 : ryn;
                            SelectedOutGoingField.Payor = _user.NickName;
                            SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                            SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                            SelectedOutGoingField.ScheduleTypeId = 2;
                            OutGoingField.Add(SelectedOutGoingField);
                        }
                    }
                    else if (OutPercentOfPremium)
                    {
                        if (OutGoingField.Count == 0)
                        {
                            SelectedOutGoingField = new OutGoingPayment();
                            //   SelectedOutGoingField.IsPrimaryAgent = false;
                            SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                            SelectedOutGoingField.FirstYearPercentage = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
                            SelectedOutGoingField.RenewalPercentage = SelectedPolicyToolIncommingShedule.RenewalPercentage;
                            SelectedOutGoingField.Payor = _user.NickName;
                            SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                            SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                            SelectedOutGoingField.ScheduleTypeId = 1;
                            OutGoingField.Add(SelectedOutGoingField);
                        }
                        else
                        {
                            double fyr = OutGoingField.Where(p => p.FirstYearPercentage != null).Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
                            double ryn = OutGoingField.Where(p => p.RenewalPercentage != null).Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
                            fyr = (SelectedPolicyToolIncommingShedule.FirstYearPercentage ?? 0) - fyr;
                            ryn = (SelectedPolicyToolIncommingShedule.RenewalPercentage ?? 0) - ryn;
                            SelectedOutGoingField = new OutGoingPayment();
                            //   SelectedOutGoingField.IsPrimaryAgent = false;
                            SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                            SelectedOutGoingField.FirstYearPercentage = (IsTiered) ? 0 : fyr;
                            SelectedOutGoingField.RenewalPercentage = (IsTiered) ? 0 : ryn;
                            SelectedOutGoingField.Payor = _user.NickName;
                            SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                            SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                            SelectedOutGoingField.ScheduleTypeId = 1;
                            OutGoingField.Add(SelectedOutGoingField);
                        }
                    }
                }
                // Acme- commented after primary agent movd to policy 
                //if (OutGoingField.Count == 1)
                //{
                //    OutGoingField.FirstOrDefault().IsPrimaryAgent = true;
                //}
            }
            catch (Exception ex)
            {

            }

        }


        private bool _OutPercentOfPremium;
        public bool OutPercentOfPremium
        {
            get { return _OutPercentOfPremium; }
            set { _OutPercentOfPremium = value; OnPropertyChanged("OutPercentOfPremium"); }
        }

        private bool _OutAdvance;
        public bool OutAdvance
        {
            get { return _OutAdvance; }
            set { _OutAdvance = value; OnPropertyChanged("OutAdvance"); }
        }

        private bool _OutPercentOfCommission;
        public bool OutPercentOfCommission
        {
            get { return _OutPercentOfCommission; }
            set { _OutPercentOfCommission = value; OnPropertyChanged("OutPercentOfCommission"); }
        }
        private double? _firstYear;
        public double? FirstYear
        {
            get { return _firstYear; }
            set { _firstYear = value; OnPropertyChanged("FirstYear"); }
        }

        private double? _renewal;
        public double? Renewal
        {
            get { return _renewal; }
            set { _renewal = value; OnPropertyChanged("Renewal"); }
        }
        private double? _primaryagentFirstYramount;
        public double? PrimaryAgentFirstYrAmount
        {
            get { return _primaryagentFirstYramount; }
            set { _primaryagentFirstYramount = value; OnPropertyChanged("PrimaryAgentFirstYrAmount"); }
        }
        private double? _primaryagentRenewYramount;
        public double? PrimaryAgentRenewYrAmount
        {
            get { return _primaryagentRenewYramount; }
            set { _primaryagentRenewYramount = value; OnPropertyChanged("PrimaryAgentRenewYrAmount"); }
        }
        private bool _IsOutEnteredDate;
        public bool IsOutScheduleEntered
        {
            get { return _IsOutEnteredDate; }
            set { _IsOutEnteredDate = value; OnPropertyChanged("IsOutScheduleEntered"); }
        }
        private bool _IsOutScheduleInvoice;
        public bool IsOutScheduleInvoice
        {
            get { return _IsOutScheduleInvoice; }
            set { _IsOutScheduleInvoice = value; OnPropertyChanged("IsOutScheduleInvoice"); }
        }
        private ObservableCollection<OutGoingPayment> FillBasicOutGoingSchedule()
        {

            ObservableCollection<OutGoingPayment> OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>();
            try
            {
                OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>(serviceClients.OutGoingPaymentClient.GetOutgoingSheduleForPolicy(SelectedPolicy.PolicyId).ToList());
                if (SelectedPolicy.IsCustomBasicSchedule == true && OutGoingPaymentLst != null && OutGoingPaymentLst.Count > 0)
                {
                    OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>(OutGoingPaymentLst.OrderBy(x => x.CustomStartDate));
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("FillBasicOutGoingSchedule exception: " + ex.Message);

            }

            return OutGoingPaymentLst;

        }

        private ObservableCollection<OutGoingPayment> FillBasicOutGoingSchedule(Guid policyId)
        {

            ObservableCollection<OutGoingPayment> OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>();
            try
            {
                OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>(serviceClients.OutGoingPaymentClient.GetOutgoingSheduleForPolicy(policyId).ToList());

            }
            catch
            {
            }

            return OutGoingPaymentLst;

        }

        #endregion

        #region ReplacePolicy

        public delegate void OpenReplacedPolicy();
        public event OpenReplacedPolicy OpenReplacedPolicyEvent;

        public delegate void CloseReplacedPolicy();
        public event CloseReplacedPolicy CloseReplacedPolicyEvent;


        public delegate void OpenCheckNamedSchedule();
        public event OpenCheckNamedSchedule OpenCheckNamedScheduleEvent;

        public delegate void CloseCheckNamedSchedule();
        public event CloseCheckNamedSchedule CloseCheckNamedScheduleEvent;

        public delegate void OpenCustomSchedule(string headerText);
        public event OpenCustomSchedule OpenCustomScheduleEvent;

        public delegate void CloseCustomSchedule();
        public event CloseCustomSchedule CloseCustomScheduleEvent;

        private ICommand _newrepalcepoliceCmd;
        public ICommand NewRepalcePoliceCmd
        {
            get
            {
                if (_newrepalcepoliceCmd == null)
                {
                    _newrepalcepoliceCmd = new BaseCommand(param => NewRepalcePolice(param));
                }
                return _newrepalcepoliceCmd;
            }

        }

        private void NewRepalcePolice(object param)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to change Policy Type?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            string str = param as string;
            if (str == "New")
            {
                if (result == MessageBoxResult.Yes)
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();//seeA
                }
                else
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();//seeA
                }


            }
            else if (str == "Replace")
            {
                if (result == MessageBoxResult.Yes)
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();//seeA
                }
                else
                {
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();//seeA
                }
            }


            if (result == MessageBoxResult.Yes)
            {
                //newPolicyType = str;
                SelectedPolicy.PolicyType = null;//seeA
                SelectedPolicy.PolicyType = str;//seeA
                SelectedPolicy.IsManuallyChanged = true;
                //serviceClients.PolicyClient.setIsManuallyChangedPolicyTypeFunctionCalled(true, true);//seeA //this is for OriginalEffectiveDate

                //for Products PolicyType function called or not is check done using previousvalue == newValue or previousvalue != newValue
                //previousvalue == newValue this means function not called //previousvalue != newValue this means function called
            }
            else
            {
                //str = str == "New" ? "Replace" : "New";//seeA

                //SelectedPolicy.PolicyType = str;//seeA // in this code it always making SelectedPolicy.PolicyType = "Replace" when we click No after clicking Replacement

                if (str == "New")//so from above line do this way
                {
                    SelectedPolicy.PolicyType = null;//seeA
                    SelectedPolicy.PolicyType = "Replace";
                }
                else//so from above line do this way
                {
                    SelectedPolicy.PolicyType = null;//seeA
                    SelectedPolicy.PolicyType = "New";
                }

                //bool? getIsManuallyChanged = serviceClients.PolicyClient.getValueOfGetIsManuallyChanged();//seeA //this is for checking value of GetIsManuallyChanged

                //if (getIsManuallyChanged == false)
                //{
                //    SelectedPolicy.IsManuallyChanged = false;
                //    serviceClients.PolicyClient.setIsManuallyChangedPolicyTypeFunctionCalled(false, false);//seeA //this is for OriginalEffectiveDate
                //}

                if (SelectedPolicy.IsManuallyChanged == false)
                {
                    SelectedPolicy.IsManuallyChanged = false;
                    //serviceClients.PolicyClient.setIsManuallyChangedPolicyTypeFunctionCalled(false, false);//seeA //this is for OriginalEffectiveDate
                }

                //for Products PolicyType function called or not is check done using previousvalue == newValue or previousvalue != newValue
                //previousvalue == newValue this means function not called //previousvalue != newValue this means function called

            }

            isFirstTime = true;
        }
        private ICommand _ReplacePolicy;

        public ICommand ReplacePolicy
        {
            get
            {
                if (_ReplacePolicy == null)
                {
                    _ReplacePolicy = new BaseCommand(param => BeforeShowReplacePolicy(), param => ShowReplacePolicy());
                }
                return _ReplacePolicy;
            }
        }

        private ICommand _CheckedNamedSchedule;
        //------------------------------------------------------Ankit Added this code-----------------------------------------------------------------------------------------------
        public ICommand CheckedNamedSchedule
        {
            get
            {
                if (_CheckedNamedSchedule == null)
                {
                    _CheckedNamedSchedule = new BaseCommand(param => BeforeShowReplacePolicy(), param => ShowCheckScheduleList());
                }
                return _CheckedNamedSchedule;
            }
        }
        //  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------Ankit Added this code-----------------------------------------------------------------------------------------------
        private ICommand _CustomSchedule;
        public ICommand CustomSchedule
        {
            get
            {
                if (_CustomSchedule == null)
                {
                    _CustomSchedule = new BaseCommand(param => BeforeShowCustomSchedule(), param => ShowCustomScheduleData());
                }
                return _CustomSchedule;
            }
        }
        private ICommand _CancelCustomSchedule;
        public ICommand CancelCustomSchedule
        {
            get
            {
                if (_CancelCustomSchedule == null)
                {
                    _CancelCustomSchedule = new BaseCommand(param => CloseCustomScheduleDialog());
                }
                return _CancelCustomSchedule;
            }
        }



        private bool BeforeShowReplacePolicy()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private bool BeforeShowCustomSchedule()
        {
            return true;
        }

        public PolicyDetailsData ShowReplacePolicy()
        {
            if (OpenReplacedPolicyEvent != null)
            {
                PolicyReplacePolicyTerminationtReasonLst = serviceClients.MasterClient.GetTerminationReasonListWithBlankAdded();
                SelectedReplacePolicyTermReason = PolicyReplacePolicyTerminationtReasonLst.FirstOrDefault();
                PolicyLstForReplace = FillReplacePolicyGrid();
                SelecetdPolicylstForReplace = PolicyLstForReplace.ToList().Find(p => p.PolicyStatusId == 1) ?? PolicyLstForReplace.FirstOrDefault();
                OpenReplacedPolicyEvent();
            }
            return SelecetdPolicylstForReplace;
        }

        private ObservableCollection<PolicyDetailsData> FillReplacePolicyGrid()
        {
            List<PolicyDetailsData> _policylst = new List<PolicyDetailsData>();
            ObservableCollection<PolicyDetailsData> policylst = new ObservableCollection<PolicyDetailsData>();
            try
            {
                _policylst = SelectedClientPolicyList.PolicyList.Where(s => s.PolicyStatusId != 1 && s.IsSavedPolicy == true).ToList();
                if (SelectedPolicy.ReplacedBy.HasValue)
                {
                    if (SelectedPolicy.ReplacedBy != Guid.Empty)
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();

                        if (!parameters.ContainsKey("PolicyId"))
                            parameters.Add("PolicyId", SelectedPolicy.ReplacedBy ?? Guid.Empty);

                        if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                        {
                            if (!parameters.ContainsKey("CreatedBy"))
                                parameters.Add("CreatedBy", RoleManager.userCredentialID);
                        }

                        _policylst.Add(_PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault());
                    }
                }
                policylst = new ObservableCollection<PolicyDetailsData>(_policylst.Where(p => p.IsDeleted == false));

            }
            catch
            {
            }
            return policylst;
        }
        //------------------------------------------------------Ankit Added this code-----------------------------------------------------------------------------------------------
        private ObservableCollection<PayorIncomingSchedule> checkNamedScheduleList;
        public ObservableCollection<PayorIncomingSchedule> CheckNamedScheduleList
        {
            get
            {
                return checkNamedScheduleList;
            }
            set
            {
                checkNamedScheduleList = value;
                OnPropertyChanged("CheckNamedScheduleList");
            }

        }
        private ICommand _CloseCheckNameScheduleListCmd;
        public ICommand CloseCheckNameScheduleListCmd
        {
            get
            {
                if (_CloseCheckNameScheduleListCmd == null)
                {
                    _CloseCheckNameScheduleListCmd = new BaseCommand(x => CloseCheckNameScheduleList());
                }
                return _CloseCheckNameScheduleListCmd;
            }
        }
        private string _showHideCheckSchedulebtn;
        public string ShowHideCheckSchedulebtn
        {
            get { return _showHideCheckSchedulebtn; }
            set { _showHideCheckSchedulebtn = value; OnPropertyChanged("ShowHideCheckSchedulebtn"); }
        }
        private ObservableCollection<PayorIncomingSchedule> _checkedNamedSchedulelst;
        public ObservableCollection<PayorIncomingSchedule> CheckedNamedSchedulelst
        {
            get { return _checkedNamedSchedulelst; }
            set { _checkedNamedSchedulelst = value; OnPropertyChanged("CheckedNamedSchedulelst"); }
        }



        private ICommand _SaveNamedSchedule;
        public ICommand SaveNamedSchedule
        {
            get
            {
                if (_SaveNamedSchedule == null)
                {
                    _SaveNamedSchedule = new BaseCommand(x => BeforeSaveNamedSchedule(), x => OnSaveNamedSchedule());
                }
                return _SaveNamedSchedule;
            }
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
        private PayorIncomingSchedule selecetdNamedSchedule;
        public PayorIncomingSchedule SelecetdNamedSchedule
        {
            get { return selecetdNamedSchedule; }
            set { selecetdNamedSchedule = value; OnPropertyChanged("SelecetdNamedSchedule"); }
        }
        public PolicyDetailsData ShowCheckScheduleList()
        {
            if (OpenCheckNamedScheduleEvent != null)
            {
                CheckNamedScheduleList = serviceClients.SettingsClient.GetNamedScheduleList(SharedVMData.SelectedLicensee.LicenseeId);
                CheckedNamedSchedulelst = FillCheckNamedScheduleGrid();
                //SelecetdPolicylstForReplace = PolicyLstForReplace.ToList().Find(p => p.PolicyStatusId == 1) ?? PolicyLstForReplace.FirstOrDefault();
                OpenCheckNamedScheduleEvent();
            }
            return SelecetdPolicylstForReplace;
        }

        public IncomingSchedule ShowCustomScheduleData()
        {
            IncomingSchedule data = new IncomingSchedule();
            OpenCustomScheduleEvent(GradedScheduleHeader);
            return data;
        }
        public void CloseCustomScheduleDialog()
        {
            SelectedPolicyToolIncommingShedule.CopyFromPayor(SavedCustomSelectedSchedule);
            SelectedIncomingMode = SelectedPolicyToolIncommingShedule.Mode == Mode.Custom ? "Custom" : "Standard";
            ValidationMessage = "";
            IsValidationShown = false;
            CloseCustomScheduleEvent();
        }
        void OnSaveNamedSchedule()
        {
            if (SelectedPolicyToolIncommingShedule == null)
            {
                SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
            }
            if (selecetdNamedSchedule != null)
            {

                // SelectedPolicyToolIncommingShedule.ScheduleTypeId = selecetdNamedSchedule.ScheduleTypeId;
                if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 1)//PercentageOfPremium
                {
                    IncPercentOfPremium = true;
                    IncPerHead = false;
                }
                else if (SelectedPolicyToolIncommingShedule.ScheduleTypeId == 2)//PerHead
                {
                    IncPercentOfPremium = false;
                    IncPerHead = true;
                }
                if (SelectedPolicy == null)
                {
                    SelectedPolicy = new PolicyDetailsData();
                }
                SelectedPolicyToolIncommingShedule.CopyFromPayor(selecetdNamedSchedule);
                if (selecetdNamedSchedule.StringFirstYearPercentage != null)
                {
                    SelectedPolicyToolIncommingShedule.FirstYearPercentage = Convert.ToDouble(selecetdNamedSchedule.ScheduleTypeId == 1 ? selecetdNamedSchedule.StringFirstYearPercentage.Replace("%", "") : selecetdNamedSchedule.StringFirstYearPercentage.Replace("$", ""));
                }
                if (selecetdNamedSchedule.StringFirstYearPercentage != null)
                {
                    SelectedPolicyToolIncommingShedule.RenewalPercentage = Convert.ToDouble(selecetdNamedSchedule.ScheduleTypeId == 1 ? selecetdNamedSchedule.StringRenewalPercentage.Replace("%", "") : selecetdNamedSchedule.StringRenewalPercentage.Replace("$", ""));
                }
                SelectedPolicy.Advance = selecetdNamedSchedule.Advance;
                if (selecetdNamedSchedule.StringSplitPercentage != null)
                {
                    SelectedPolicy.SplitPercentage = Convert.ToDouble(selecetdNamedSchedule.StringSplitPercentage.Replace("%", ""));


                }
                // SavedCustomSelectedSchedule = SelectedPolicyToolIncommingShedule;
                SavedCustomSelectedSchedule.Copy(SelectedPolicyToolIncommingShedule);
                SelectedIncomingMode = SelectedPolicyToolIncommingShedule.Mode == Mode.Custom ? "Custom" : "Standard";


            }
            CloseCheckNameScheduleList();
        }
        private void CloseCheckNameScheduleList()
        {
            if (CloseCheckNamedScheduleEvent != null)
            {
                CloseCheckNamedScheduleEvent();
            }

        }

        private ObservableCollection<PayorIncomingSchedule> FillCheckNamedScheduleGrid()
        {
            List<PayorIncomingSchedule> _schedulelst = new List<PayorIncomingSchedule>();
            ObservableCollection<PayorIncomingSchedule> schedulelst = new ObservableCollection<PayorIncomingSchedule>();
            try
            {
                schedulelst = new ObservableCollection<PayorIncomingSchedule>(serviceClients.SettingsClient.GetNamedScheduleList(SharedVMData.SelectedLicensee.LicenseeId));
            }
            catch
            {
            }
            return schedulelst;
        }

        bool BeforeSaveNamedSchedule()
        {
            ErrorTooltip = null;
            if (selecetdNamedSchedule == null)
            {
                ErrorTooltip = "Please select a Schedule.";
                return false;
            }
            else
            {

                return true;
            }

        }
        //  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private ICommand _SaveReplacePolicyCmd;
        public ICommand SaveReplacePolicyCmd
        {
            get
            {
                if (_SaveReplacePolicyCmd == null)
                {
                    _SaveReplacePolicyCmd = new BaseCommand(x => SaveReplacePolicy());
                }
                return _SaveReplacePolicyCmd;
            }
        }

        private void SaveReplacePolicy()
        {
            try
            {
                if (SelecetdPolicylstForReplace == null || SelecetdPolicylstForReplace.PolicyId == Guid.Empty)
                {
                    MessageBox.Show("No Policy is selecetd", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Guid PolicyId = SelectedPolicy.PolicyId;
                OnStatusChanged(CurrentPolicyStatus);
                SelectedPolicy = PolicyList.FirstOrDefault(s => s.PolicyId == PolicyId);

                SelecetdPolicylstForReplace.TerminationReasonId = SelectedReplacePolicyTermReason.TerminationReasonId;
                SelecetdPolicylstForReplace.PolicyTerminationDate = ReplacePolicyTermDate;
                SelecetdPolicylstForReplace.PolicyStatusId = (int)_PolicyStatus.Terminated;

                if (SelecetdPolicylstForReplace != null)
                {
                    if (SelectedPolicy.IsSavedPolicy == false)
                    {
                        SelectedPolicy.Insured = SelecetdPolicylstForReplace.Insured;
                        SelectedPolicy.Enrolled = SelecetdPolicylstForReplace.Enrolled;
                        SelectedPolicy.Eligible = SelecetdPolicylstForReplace.Eligible;
                        SelectedPolicy.IsOutGoingBasicSchedule = SelecetdPolicylstForReplace.IsOutGoingBasicSchedule;

                        OutGoingField = serviceClients.OutGoingPaymentClient.GetOutgoingSheduleForPolicy(SelecetdPolicylstForReplace.PolicyId);
                        if (OutGoingField != null && OutGoingField.Count > 0)
                        {
                            OutGoingField.ToList().ForEach(p => p.PolicyId = SelectedPolicy.PolicyId);
                            OutGoingField.ToList().ForEach(p => p.OutgoingScheduleId = Guid.NewGuid());
                        }
                        IsTiered = SelecetdPolicylstForReplace.IsTieredSchedule != null ? Convert.ToBoolean(SelecetdPolicylstForReplace.IsTieredSchedule) : false;
                        IsCustomDateSelected = SelecetdPolicylstForReplace.IsCustomBasicSchedule != null ? Convert.ToBoolean(SelecetdPolicylstForReplace.IsCustomBasicSchedule) : false;
                        VMPolicySchedule.CopyOutgoingScheduleFrom(SelecetdPolicylstForReplace.PolicyId);
                    }
                    SelectedPolicy.PolicyType = "Replace";
                    SelectedPolicy.ReplacedBy = SelecetdPolicylstForReplace.PolicyId;

                    if (SelectedPolicy.OriginalEffectiveDate == null)
                        SelectedPolicy.OriginalEffectiveDate = SelecetdPolicylstForReplace.PolicyTerminationDate;
                }

                //Guid PolicyId = SelectedPolicy.PolicyId;
                //OnStatusChanged(CurrentPolicyStatus);
                //SelectedPolicy = PolicyList.FirstOrDefault(s => s.PolicyId == PolicyId);

                CloseReplacePolicy();
            }
            catch
            {
            }
        }


        private ICommand _CloseReplacePolicyCmd;
        public ICommand CloseReplacePolicyCmd
        {
            get
            {
                if (_CloseReplacePolicyCmd == null)
                {
                    _CloseReplacePolicyCmd = new BaseCommand(x => CloseReplacePolicy());
                }
                return _CloseReplacePolicyCmd;
            }
        }

        private void CloseReplacePolicy()
        {
            if (CloseReplacedPolicyEvent != null)
            {
                CloseReplacedPolicyEvent();
            }

        }

        private ObservableCollection<PolicyDetailsData> _policylstForReplace;
        public ObservableCollection<PolicyDetailsData> PolicyLstForReplace
        {
            get { return _policylstForReplace; }
            set { _policylstForReplace = value; OnPropertyChanged("PolicyLstForReplace"); }
        }

        private PolicyDetailsData selecetdpolicylstForReplace;

        public PolicyDetailsData SelecetdPolicylstForReplace
        {
            get { return selecetdpolicylstForReplace; }
            set { selecetdpolicylstForReplace = value; OnPropertyChanged("SelecetdPolicylstForReplace"); }
        }

        private PolicyTerminationReason selectedReplacePolicyTermReason;
        public PolicyTerminationReason SelectedReplacePolicyTermReason
        {
            get { return selectedReplacePolicyTermReason == null ? new PolicyTerminationReason() : selectedReplacePolicyTermReason; }
            set { selectedReplacePolicyTermReason = value; OnPropertyChanged("SelectedReplacePolicyTermReason"); }
        }

        private ObservableCollection<PolicyTerminationReason> policyReplacePolicyTerminationtReasonLst;
        public ObservableCollection<PolicyTerminationReason> PolicyReplacePolicyTerminationtReasonLst
        {
            get
            {
                return policyReplacePolicyTerminationtReasonLst;
            }
            set
            {
                policyReplacePolicyTerminationtReasonLst = value;
                OnPropertyChanged("PolicyReplacePolicyTerminationtReasonLst");
            }

        }

        private DateTime replacePolicyTermDate = DateTime.Today;
        public DateTime ReplacePolicyTermDate
        {
            get { return replacePolicyTermDate; }
            set { replacePolicyTermDate = value; OnPropertyChanged("ReplacePolicyTermDate"); }
        }

        #endregion

        #region PolicyCommand

        private ICommand _DefaultTrackFormDate;
        public ICommand DefaultTrackFormDate
        {
            get
            {
                if (_DefaultTrackFormDate == null)
                {
                    _DefaultTrackFormDate = new BaseCommand(x => DefaultTrackFormDateCmd());
                }
                return _DefaultTrackFormDate;
            }
        }

        private void DefaultTrackFormDateCmd()
        {
            if (SelectedPolicy == null) return;
            if (SelectedPolicy.TrackFromDate == null)
            {
                SelectedPolicy.TrackFromDate = FirstDate(DateTime.Today.AddMonths(1));
            }
            else
            {
                if (SelectedPolicy.OriginalEffectiveDate != null)
                {
                    if (SelectedPolicy.OriginalEffectiveDate > SelectedPolicy.TrackFromDate)
                    {
                        SelectedPolicy.TrackFromDate = SelectedPolicy.OriginalEffectiveDate;
                    }
                }
            }

        }

        private ICommand _DefaultEffDate;
        public ICommand DefaultEffDate
        {
            get
            {
                if (_DefaultEffDate == null)
                {
                    _DefaultEffDate = new BaseCommand(x => DefaultEffDateCmd());
                }
                return _DefaultEffDate;
            }
        }

        private void DefaultEffDateCmd()
        {
            if (SelectedPolicy == null) return;
            if (SelectedPolicy.OriginalEffectiveDate == null)
            {
                SelectedPolicy.OriginalEffectiveDate = FirstDate(DateTime.Today.AddMonths(1));
            }
        }
        public static DateTime? FirstDate(DateTime? dt)
        {
            if (dt == null) return dt;
            return new DateTime(dt.Value.Year, dt.Value.Month, 1);
        }

        private ICommand _policyNumberFocus;
        public ICommand PolicyNumberFocus
        {

            get
            {
                if (_policyNumberFocus == null)
                {
                    _policyNumberFocus = new BaseCommand(x => CorrectPolicyNumber());
                }
                return _policyNumberFocus;
            }
        }

        private void CorrectPolicyNumber()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyNumber == null) return;
            SelectedPolicy.PolicyNumber = MyAgencyVault.ViewModel.VMHelper.CorrectPolicyNo(SelectedPolicy.PolicyNumber);
        }

        private ICommand _ClickStatusChanged;
        public ICommand ClickStatusChanged
        {
            get
            {
                if (_ClickStatusChanged == null)
                {


                    _ClickStatusChanged = new BaseCommand(param => OnStatusChanged(param));
                }
                return _ClickStatusChanged;
            }
        }

        private string CurrentPolicyStatus = "Active";
        private void OnStatusChanged(object param)
        {
            try
            {
                if (SelectedClientPolicyList == null || SelectedClientPolicyList.PolicyList.Count == 0)
                    return;


                string clickedLink = param as string;
                CurrentPolicyStatus = clickedLink;
                _isNotSaveLastViewedClientPolicy = true;
                switch (clickedLink)
                {
                    case "Active":
                        IsActiceChecked = true;
                        policyStatus = _PolicyStatus.Active;
                        //PolicyList = new ObservableCollection<PolicyDetailsData>(SelectedClientPolicyList.PolicyList.Where(s => s != null && s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(s => s != null && s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        break;
                    case "Pending":
                        IsPendingChecked = true;
                        policyStatus = _PolicyStatus.Pending;
                        //PolicyList = new ObservableCollection<PolicyDetailsData>(SelectedClientPolicyList.PolicyList.Where(s => s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(s => s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        break;
                    case "Terminated":
                        IsTerminatedCheck = true;
                        policyStatus = _PolicyStatus.Terminated;
                        //PolicyList = new ObservableCollection<PolicyDetailsData>(SelectedClientPolicyList.PolicyList.Where(s => s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(s => s.IsDeleted == false && s.PolicyStatusId == (int)policyStatus).ToList());
                        break;

                    case "All":
                        IsAllCheck = true;
                        policyStatus = _PolicyStatus.Any;
                        //PolicyList = new ObservableCollection<PolicyDetailsData>(SelectedClientPolicyList.PolicyList.Where(s => s.IsDeleted == false).ToList());
                        PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(s => s.IsDeleted == false).ToList());
                        break;
                    default:
                        break;
                }
                LastViewPolicy lastViewedPolicy = LastViewPolicyClientCollection.getLastViewedPolicyForClient(SelectedDisplayClient.ClientId);
                Guid? PolicyId = null;
                if (lastViewedPolicy != null)
                {
                    PolicyId = lastViewedPolicy.PolicyId;
                    //vinod
                    SelectedPolicy = PolicyList.FirstOrDefault(p => p.PolicyId == PolicyId);//anshshikha10
                }

                if (SelectedPolicy == null)
                {   //vinod
                    SelectedPolicy = PolicyList.FirstOrDefault();//anshshikha10
                }
                _isNotSaveLastViewedClientPolicy = false;
            }
            catch
            {
            }
        }

        private ICommand duplicatePolicyCmd;
        public ICommand DuplicatePolicyCmd
        {
            get
            {
                if (duplicatePolicyCmd == null)
                {

                    duplicatePolicyCmd = new BaseCommand(X => BeforeDoDuplicatePolicy(), X => DoDuplicatePolicy());
                }
                return duplicatePolicyCmd;
            }

        }

        private bool BeforeDoDuplicatePolicy()
        {
            if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            if (SelectedPolicy.IsSavedPolicy == false)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand savePoliciesCmd;
        public ICommand SavePoliciesCmd
        {
            get
            {
                if (savePoliciesCmd == null)
                {
                    savePoliciesCmd = new BaseCommand(x => BeforeSavePoliciesData(), x => SavePoliciesData());
                }
                return savePoliciesCmd;
            }

        }

        private bool _Changedcolor;
        public bool Changedcolor
        {
            get
            {
                return _Changedcolor;
            }
            set
            {
                _Changedcolor = value;
                OnPropertyChanged("Changedcolor");
            }
        }

        private string _Savecontent;
        public string Savecontent
        {
            get
            {
                return _Savecontent;
            }
            set
            {
                _Savecontent = value;
                OnPropertyChanged("Savecontent");
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

        private bool _IsAdvanceEnabled;
        public bool IsAdvanceEnabled
        {
            get
            {
                return _IsAdvanceEnabled;
            }
            set
            {
                _IsAdvanceEnabled = value;
                OnPropertyChanged("IsAdvanceEnabled");
            }
        }

        private bool BeforeSavePoliciesData()
        {
            PolicyDetailSaveToolTip = null;

            Changedcolor = false;
            Savecontent = " Save";
            string strLocalPath = AppDomain.CurrentDomain.BaseDirectory;
            string strSavePath = @"\Images\Icons\floppy_disk_blue.png";
            try
            {
                //string strExeclamation = @"\Images\Icons\small.png";
                imagePath = strLocalPath + strSavePath;
                if (SelectedPolicy == null)
                {
                    PolicyDetailSaveToolTip = "No policy selected.";
                    Changedcolor = true;
                    Savecontent = "! Save";
                    imagePath = null;
                    return false;
                }

                if (SelectedPolicy.OriginalEffectiveDate != null)
                {
                    IsAdvanceEnabled = true;
                }
                else
                {
                    // SelectedPolicy.Advance = null;
                    IsAdvanceEnabled = false;

                }

                if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                {
                    PolicyDetailSaveToolTip = "Security Violation.";
                    Changedcolor = true;
                    Savecontent = "! Save";
                    imagePath = null;
                    return false;
                }
                else if (!isCustomDateSelected && !isTiered && OutGoingField != null && OutGoingField.Count != 0)
                {
                    if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2))
                    {
                        double dbFirstYear = 0.0;
                        double dbRenewal = 0.0;

                        foreach (var item in OutGoingField)
                        {
                            dbFirstYear = dbFirstYear + Convert.ToDouble(item.FirstYearPercentage);
                            dbRenewal = dbRenewal + Convert.ToDouble(item.RenewalPercentage);
                        }

                        dbFirstYear = Convert.ToDouble(dbFirstYear.ToString("n2"));
                        dbRenewal = Convert.ToDouble(dbRenewal.ToString("n2"));

                        //if ((OutGoingField.ToList().Sum(p => p.FirstYearPercentage) != 100) || (OutGoingField.ToList().Sum(p => p.RenewalPercentage) != 100))
                        if ((dbFirstYear != 100) || (dbRenewal != 100))
                        {
                            PolicyDetailSaveToolTip = "FirstYear/Renewal not equal to 100.";
                            Changedcolor = true;
                            Savecontent = "! Save";
                            imagePath = null;
                            return false;
                        }
                    }
                }

                if (SelectedClient.ClientId == Guid.Empty)
                {
                    PolicyDetailSaveToolTip = "No client selected.";
                    Changedcolor = true;
                    Savecontent = "! Save";
                    imagePath = null;
                    return false;
                }
                //if (OutGoingField != null && OutGoingField.Count != 0 && OutGoingField.Where(p => p.IsPrimaryAgent).Count() != 1)
                //{
                //    PolicyDetailSaveToolTip = "No Primary Agent.";
                //    Changedcolor = true;
                //    Savecontent = "! Save";
                //    imagePath = null;
                //    return false;
                //}

                /* Commented after custom schedules - Nov 28, 2019
                 * if (!isCustomDateSelected && !isTiered && OutPercentOfPremium && (OutGoingField != null && OutGoingField.Count != 0))
                  {
                      if ((OutGoingField.Sum(p => p.FirstYearPercentage) != SelectedPolicyToolIncommingShedule.FirstYearPercentage) || (OutGoingField.Sum(p => p.RenewalPercentage) != SelectedPolicyToolIncommingShedule.RenewalPercentage))
                      {
                          PolicyDetailSaveToolTip = "Incoming FirstYear/Renewal not equal to Outgoing FirstYear/Renewal.";
                          Changedcolor = true;
                          Savecontent = "! Save";
                          imagePath = null;
                          return false;
                      }
                  }
                  */
                if (SelectedPolicy == null)
                    return false;

                if (SelectedPolicy.PolicyStatusId == 1)
                {
                    //if (SelectedPolicy.PolicyTerminationDate == null || SelectedPolicy.TerminationReasonId == null)
                    //{
                    if (SelectedPolicy.PolicyTerminationDate == null)
                    {
                        PolicyDetailSaveToolTip = "A terminated policy must have a termination date, termination reason and the status must be Terminated.";
                        Changedcolor = true;
                        Savecontent = "! Save";
                        imagePath = null;
                        return false;
                    }
                }
                if (SelectedPolicy.PolicyTerminationDate != null)
                {
                    //if (SelectedPolicy.PolicyStatusId == null || SelectedPolicy.TerminationReasonId == null)
                    //{
                    if (SelectedPolicy.PolicyStatusId == null)
                    {
                        PolicyDetailSaveToolTip = "A terminated policy must have a termination date, termination reason and the status must be Terminated.";
                        Changedcolor = true;
                        Savecontent = "! Save";
                        imagePath = null;
                        return false;
                    }
                }


                if (SelectedPolicy.OriginalEffectiveDate != null)
                {
                    if (SelectedPolicy.PolicyTerminationDate < SelectedPolicy.OriginalEffectiveDate)
                    {
                        PolicyDetailSaveToolTip = "Termination date cannot be prior to the effective date.";
                        Changedcolor = true;
                        Savecontent = "! Save";
                        imagePath = null;
                        return false;
                    }
                }

                if (SelectedPolicy.TrackFromDate != null && SelectedPolicy.OriginalEffectiveDate != null)
                {
                    if (SelectedPolicy.TrackFromDate < SelectedPolicy.OriginalEffectiveDate)
                    {
                        PolicyDetailSaveToolTip = "Track from date should greater than or equal to Original effective date.";
                        Changedcolor = true;
                        Savecontent = "! Save";
                        imagePath = null;
                        return false;
                    }
                }

                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                {
                    PolicyDetailSaveToolTip = "No selected policy.";
                    Changedcolor = true;
                    Savecontent = "! Save";
                    imagePath = null;
                    return false;
                }

                //If there is at least one agent in the outgoing splits and there is no effective date.  Please do not save and tell the user:
                try
                {
                    if (OutGoingField.Count > 0)
                    {
                        if (SelectedPolicy.OriginalEffectiveDate == null)
                        {
                            PolicyDetailSaveToolTip = "Effective date is required with outgoing commission splits.";
                            Changedcolor = true;
                            Savecontent = "! Save";
                            imagePath = null;
                            return false;
                        }
                    }
                }
                catch
                {
                }

                //if (SelectedPolicy.IsSavedPolicy && SelectedPolicy.PolicyStatusId == 2)
                //{
                //    int? PolicyStatusId = _PolicyServiceClient.PolicyClient.GetPolicyStatusID(SelectedPolicy.PolicyId);
                //    if (PolicyStatusId != null)
                //    {
                //        if (PolicyStatusId == 0)
                //        {
                //            //Change Active policy to pending policy (Eric reqquirement-10 may 2012)
                //            //PolicyDetailSaveToolTip = "Cannot change Active to Pending.";
                //            //return false;
                //        }

                //    }
                //}
            }
            catch
            {
                imagePath = strLocalPath + strSavePath;
            }
            return true;
        }

        private ICommand cancelPoliciesCmd;
        public ICommand CancelPoliciesCmd
        {
            get
            {
                if (cancelPoliciesCmd == null)
                {
                    cancelPoliciesCmd = new BaseCommand(x => BeforeCancelPoliciesData(), x => CancelPoliciesData());
                }
                return cancelPoliciesCmd;
            }
        }

        private bool BeforeCancelPoliciesData()
        {
            if (SelectedPolicy == null)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void CancelPoliciesData()
        {
            isClickOnAddPolicy = false;
            if (SelectedPolicy.PolicyId == Guid.Empty) return;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (SelectedPolicy.PolicyType == "Replace")
                {
                    PolicyDetailsData policy = null;
                    if (!SelectedPolicy.IsSavedPolicy)
                    {
                        Guid? policyId = SelectedPolicy.PolicyId;
                        Dictionary<string, object> parameters = new Dictionary<string, object>();

                        if (!parameters.ContainsKey("PolicyId"))
                            parameters.Add("PolicyId", SelectedPolicy.ReplacedBy ?? Guid.Empty);

                        if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                        {
                            if (!parameters.ContainsKey("CreatedBy"))
                                parameters.Add("CreatedBy", RoleManager.userCredentialID);
                        }

                        policy = _PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                        //check policy is null                      
                        RemovePolicyFromList(PolicyList.FirstOrDefault(s => s.PolicyId == policyId));
                        AddPolicyToList(policy);
                    }
                    else
                    {
                        Guid? replacedPolicyId = SelectedPolicy.ReplacedBy;
                        Guid? policyId = SelectedPolicy.PolicyId;
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        if (!parameters.ContainsKey("PolicyId"))
                            parameters.Add("PolicyId", policyId ?? Guid.Empty);

                        if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                        {
                            //Get linked user list
                            if (!parameters.ContainsKey("CreatedBy"))
                                parameters.Add("CreatedBy", RoleManager.userCredentialID);
                        }

                        policy = _PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                        //check policy is null
                        if (policy != null)
                        {
                            RemovePolicyFromList(PolicyList.FirstOrDefault(s => s.PolicyId == policy.PolicyId));
                            AddPolicyToList(policy);
                        }
                        parameters = new Dictionary<string, object>();
                        if (!parameters.ContainsKey("PolicyId"))
                            parameters.Add("PolicyId", replacedPolicyId ?? Guid.Empty);
                        policy = _PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                        //check policy is null
                        if (policy != null)
                        {
                            RemovePolicyFromList(PolicyList.FirstOrDefault(s => s.PolicyId == policy.PolicyId));
                            AddPolicyToList(policy);
                        }
                    }
                    OnStatusChanged(CurrentPolicyStatus);
                    SelectedPolicy = policy;
                }
                else
                {
                    if (!SelectedPolicy.IsSavedPolicy)
                    {
                        RemovePolicyFromList(SelectedPolicy);
                        SelectedPolicy = PolicyList.FirstOrDefault();
                    }
                    else
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        if (!parameters.ContainsKey("PolicyId"))
                            parameters.Add("PolicyId", SelectedPolicy.PolicyId);

                        if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                        {
                            if (!parameters.ContainsKey("CreatedBy"))
                                parameters.Add("CreatedBy", RoleManager.userCredentialID);
                        }
                        _deleteseletedOutGoingField.Clear(); //Acme Sep26, 2017 - to clear the deleted payees list, as operation cancelled.

                        PolicyDetailsData policy = _PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                        RemovePolicyFromList(SelectedPolicy);
                        AddPolicyToList(policy);
                        SelectedPolicy = policy;
                    }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private ICommand addpolicy;
        public ICommand AddPolicy
        {
            get
            {
                if (addpolicy == null)
                {
                    addpolicy = new BaseCommand(x => BeforeAddPolicyWithDefaultSetting(), x => AddPolicyWithDefaultSetting());
                }
                return addpolicy;
            }

        }

        private bool BeforeAddPolicyWithDefaultSetting()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            return true;
        }

        private ICommand deletePolicyCmd;
        public ICommand DeletePolicyCmd
        {
            get
            {
                if (deletePolicyCmd == null)
                {
                    deletePolicyCmd = new BaseCommand(x => BeforeDeletePolicy(), x => DeletePolicy());
                }
                return deletePolicyCmd;

            }

        }

        private bool BeforeDeletePolicy()
        {
            if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            if (SelectedPolicy.IsSavedPolicy == false)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand createNewClient;
        public ICommand CreateNewClient
        {
            get
            {
                if (createNewClient == null)
                {
                    createNewClient = new BaseCommand(x => BeforeCreateClient(), x => CreateClient());
                }
                return createNewClient;
            }

        }

        private bool BeforeCreateClient()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand _EditClient;
        public ICommand EditClient
        {
            get
            {
                if (_EditClient == null)
                {
                    _EditClient = new BaseCommand(x => BeforeEditClientDetail(), x => EditClientDetail());
                }
                return _EditClient;
            }

        }

        private bool BeforeEditClientDetail()
        {
            if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private ICommand _DeleteClient;
        public ICommand DeleteClientCmd
        {
            get
            {
                if (_DeleteClient == null)
                {
                    _DeleteClient = new BaseCommand(x => BeforeDeleteClient(), x => DeleteClient());
                }
                return _DeleteClient;
            }

        }

        private bool BeforeDeleteClient()
        {
            if (SelectedDisplayClient == null || SelectedDisplayClient.ClientId == Guid.Empty)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        #endregion

        #region PolicyCommandMethod

        private void DoDuplicatePolicy()
        {
            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty || SelectedPolicy.IsDeleted == true)
                    return;

                PolicyDetailsData policy = new PolicyDetailsData();
                policy.PolicyId = Guid.NewGuid();
                policy.PolicyStatusId = SelectedPolicy.PolicyStatusId;
                policy.PolicyNumber = string.Empty;
                policy.PolicyType = SelectedPolicy.PolicyType;
                policy.ClientId = SelectedPolicy.ClientId;
                policy.PolicyLicenseeId = SelectedPolicy.PolicyLicenseeId;
                policy.Insured = SelectedPolicy.Insured;
                policy.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                policy.TrackFromDate = SelectedPolicy.TrackFromDate;
                policy.PolicyModeId = SelectedPolicy.PolicyModeId;
                policy.ModeAvgPremium = SelectedPolicy.ModeAvgPremium;
                policy.CoverageId = SelectedPolicy.CoverageId;
                policy.SubmittedThrough = SelectedPolicy.SubmittedThrough;
                policy.Enrolled = SelectedPolicy.Enrolled;
                policy.Eligible = SelectedPolicy.Eligible;
                policy.PolicyTerminationDate = SelectedPolicy.PolicyTerminationDate;
                policy.TerminationReasonId = SelectedPolicy.TerminationReasonId;
                policy.IsTrackMissingMonth = SelectedPolicy.IsTrackMissingMonth;
                policy.IsTrackIncomingPercentage = SelectedPolicy.IsTrackIncomingPercentage;
                policy.IsTrackPayment = SelectedPolicy.IsTrackPayment;
                policy.IsDeleted = SelectedPolicy.IsDeleted;
                policy.ReplacedBy = SelectedPolicy.ReplacedBy;
                policy.PayorId = SelectedPolicy.PayorId;
                policy.DuplicateFrom = SelectedPolicy.PolicyId;
                policy.CreatedBy = SelectedPolicy.CreatedBy;
                policy.CreatedOn = DateTime.Now;
                policy.IsIncomingBasicSchedule = SelectedPolicy.IsIncomingBasicSchedule;
                policy.CarrierID = SelectedPolicy.CarrierID;
                policy.SplitPercentage = SelectedPolicy.SplitPercentage;
                policy.IncomingPaymentTypeId = 1;

                Guid savedPolicyId = SelectedPolicy.PolicyId;
                AddPolicyToList(policy);
                SelectedPolicy = policy;

                UpdateUIByAddNewPolicy(SelectedPolicy);

                ObservableCollection<OutGoingPayment> _OutPayment = new ObservableCollection<OutGoingPayment>(serviceClients.OutGoingPaymentClient.GetOutgoingSheduleForPolicy(savedPolicyId));
                if (_OutPayment.Count != 0 && _OutPayment.FirstOrDefault().ScheduleTypeId == 2)
                {
                    OutGoingField = _OutPayment;
                    if (OutGoingField != null)
                    {
                        OutGoingField.ToList().ForEach(p => p.PolicyId = policy.PolicyId);
                        OutGoingField.ToList().ForEach(p => p.OutgoingScheduleId = Guid.NewGuid());
                    }
                    SelectedOutGoingField = OutGoingField.FirstOrDefault();
                }
                //VMPolicySchedule.CopyOutgoingScheduleFrom(savedPolicyId);
                SelectedPageIndex = 0;
            }
            catch
            {
            }

        }

        private void SavePoliciesDataLock()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;
            //bool ReplaceOccur = false;
            //bool? RunFollowUp = null;
        }

        public static void CheckImportFormat()
        {
            try
            {
                // IViewDialog _ViewDialog;
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                FileUtility ObjDownload = FileUtility.CreateClient(WebDevPath.GetWebDevPath(RoleManager.WebDavPath).URL, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).UserName, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).Password, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).DomainName);
                ObjDownload.DownloadComplete += (obj1, obj2) =>
                {
                    autoResetEvent.Set();
                };

                string localPath = Path.Combine(System.IO.Path.GetTempPath(), "ImportTemplate_" + DateTime.Now.Ticks.ToString() + ".xlsx");
                string RemotePath = @"/ImportTemplate/Template.xlsx";

                ObjDownload.Download(RemotePath, localPath);
                autoResetEvent.WaitOne();
                //   _ViewDialog.Close();

                MessageBox.Show("Template is downloaded successfully and opened in excel. Please check!");
                System.Diagnostics.Process.Start(localPath);
            }
            catch (Exception ex)
            {
                objLog.AddLog("ReportClient_PrintReportCompleted exception :" + ex.Message);
                MessageBox.Show("Error downloading template! " + ex.Message);
            }
        }

        public DataTable TempTable = null;

        public string DoImportPolicy(string fileName)
        {
            try
            {
                TempTable = ConvretExcelToDataTable(fileName);

                //if (OpenImportPolicy != null)
                //{
                //    OpenImportPolicy();
                //}

                #region"New to select filter type"
                /* Microsoft.Win32.OpenFileDialog objOpenFileDialog = new Microsoft.Win32.OpenFileDialog();
                objOpenFileDialog.DefaultExt = ".xlsx";
                objOpenFileDialog.Filter = "Excel sheet (.xlsx)|*.xlsx| All files (*)|*.*";

                if (objOpenFileDialog.ShowDialog() == true)
                {
                    
                    TempTable = ConvretExcelToDataTable(objOpenFileDialog.FileName);

                    if (OpenImportPolicy != null)
                    {
                        OpenImportPolicy();
                    }
                }
            //    MessageBox.Show("Policies imported successfully!");*/
                #endregion
                return string.Empty;
            }
            catch (Exception ex)
            {
                objLog.AddLog("Do import Policy exception: " + ex.Message);
                return ex.Message + "\n\nWould you like to check the required import format?\nClick 'Ok' to download the import format or 'Cancel' to return.";
                /* if (MessageBox.Show(ex.Message + "\nWould you like to check the required import format?\nClick 'Ok' to download the import format or 'Cancel' to return.", "Incorrect Import Format", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                 {
                     CheckImportFormat();
                 }*/
            }
        }

        public void AfterImport(string result)
        {
            if (result == string.Empty)
            {
                if (OpenImportPolicy != null)
                {
                    OpenImportPolicy();
                }
            }
            else
            {
                if (MessageBox.Show(result, "Incorrect Import Format", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CheckImportFormat();
                }
            }
        }

        DataTable GetDataTableFromXls(string _path)
        {
            // // to get started. This is how we avoid dependencies on ACE or Interop:
            // FileInfo fi = new FileInfo(_path);
            // StreamWriter stream = fi.CreateText();

            // //StreamWriter w = new StreamWriter(stream );
            // //w.Write("Hello World");
            // //w.Close();

            //// stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            // StreamReader r = new StreamReader(stream);
            // string t;
            // while ((t = r.ReadLine()) != null)
            // {
            //     Console.WriteLine(t);
            // }
            // w.Close();
            // stream.Close();
            // fi.Delete();
            //byte[] data = File.ReadAllBytes(_path);
            //MemoryStream stream = new MemoryStream(data);
            DataTable dt = new DataTable();
            using (FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //MemoryStream stream = new MemoryStream();
            //using (Stream input = File.OpenRead(_path))
            //{
            //    input.CopyTo(stream);
            //}
            //stream.Position = 0;
            {

                // We return the interface, so that
                IExcelDataReader reader = null;
                try
                {
                    if (_path.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream, true);
                    }
                    if (_path.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    if (reader != null)
                    {

                        reader.IsFirstRowAsColumnNames = true;
                        var workbook = reader.AsDataSet(true);
                        // var tables = workbook.Tables
                        //.Cast<DataTable>()
                        //.Select(t => new
                        //{
                        //    TableName = t.TableName,
                        //    Columns = t.Columns
                        //               .Cast<DataColumn>()
                        //               .Select(x => x.ColumnName)
                        //               .ToList()
                        //});

                        //var sheetName = from DataTable sheet in workbook.Tables select sheet.TableName;
                        //DataTable workSheet = workbook.Tables[sheetName.ElementAt(0)];
                        //IEnumerable<DataRow> rows = from DataRow row in workSheet.Rows
                        //                           select row; //rows = from DataRow a in workSheet.Rows select a;

                        dt = workbook.Tables[0];
                        dt.TableName = "temp";
                        //int count = rows.Count();
                        string s = serviceClients.PolicyClient.CompareExcel(dt);
                        if (!string.IsNullOrEmpty(s))
                        {
                            Exception ex = new Exception(s, new Exception("ExcelFormatException"));
                            throw ex;
                        }
                        /*
                        DataTable tbl = new DataTable("Temp");
                        foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                        {
                            tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                        }*/
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return dt;
        }

        public static DataTable GetDataTableFromExcel(string path, bool hasHeader = true)
        {
            if (objLog == null) objLog = new MastersClient();
            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))// File.OpenRead(path))
                                                                                                                  //   FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    {
                        pck.Load(stream);
                    }
                    var ws = pck.Workbook.Worksheets.First();
                    DataTable tbl = new DataTable("Temp");
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                    {
                        tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                    }
                    var startRow = hasHeader ? 2 : 1;
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = tbl.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            try
                            {
                                row[cell.Start.Column - 1] = cell.Text;
                            }
                            catch (Exception ex)
                            {
                                objLog.AddLog("Exception GetDataTableFromExcel: " + ex.Message);
                                throw ex;
                            }
                        }
                    }
                    return tbl;
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("Exception GetDataTableFromExcel: " + ex.Message);
                throw ex;
            }
        }

        public DataTable ConvretExcelToDataTable(string FilePath)
        {

            if (objLog == null) objLog = new MastersClient();
            string strConn = string.Empty;
            DataTable dt = new DataTable("Temp");
            DataTable dtCloned = null;
            //if (FilePath.Trim().EndsWith(".xlsx"))
            {
                try
                {
                    dt = (FilePath.Trim().EndsWith(".xlsx")) ? GetDataTableFromExcel(FilePath, true) : GetDataTableFromXls(FilePath);
                    //convert datetime vales to string

                    dtCloned = dt.Clone();
                    try
                    {
                        dtCloned.Columns["Original Plan Start Date"].DataType = typeof(string);
                    }
                    catch (Exception ex)
                    {
                        objLog.AddLog("ConvretExcelToDataTable exception - Original Plan Start Date not found: " + ex.Message);
                    }

                    try
                    {
                        dtCloned.Columns["Track From"].DataType = typeof(string);
                    }
                    catch (Exception ex)
                    {
                        objLog.AddLog("ConvretExcelToDataTable exception - Track From not found: " + ex.Message);
                    }
                    try
                    {
                        dtCloned.Columns["Plan End Date"].DataType = typeof(string);
                    }
                    catch (Exception ex)
                    {
                        objLog.AddLog("ConvretExcelToDataTable exception - Plan End Date not found: " + ex.Message);
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        dtCloned.ImportRow(dr);
                    }

                    objLog.AddLog("ConvretExcelToDataTable success with EPPlus");
                    try
                    {
                        string s = serviceClients.PolicyClient.CompareExcel(dt);
                        if (!string.IsNullOrEmpty(s))
                        {
                            Exception ex = new Exception(s, new Exception("ExcelFormatException"));
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message == "ExcelFormatException")
                        throw ex;

                    /* objLog.AddLog("ConvretExcelToDataTable exception with EPPlus: " +ex.Message + ", importing with oledb");
                     strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", FilePath);
                     OleDbConnection conn = null;
                     OleDbCommand cmd = null;
                     OleDbDataAdapter da = null;
                     string pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                     string fileName = System.IO.Path.GetFileName(FilePath);
                   //  dt = new DataTable("Temp");
                     try
                     {
                         conn = new OleDbConnection(strConn);
                         conn.Open();
                         string strSheetName = getSheetName(conn, fileName);
                         string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                         cmd = new OleDbCommand(strQuery, conn);
                         cmd.CommandType = CommandType.Text;
                         da = new OleDbDataAdapter(cmd);
                         da.Fill(dt);

                     }
                     catch (Exception e)
                     {
                         objLog.AddLog("ConvretExcelToDataTable exception: " + ex.Message + ", importing with oledb");
                         //MessageBox.Show(e.Message.ToString());
                         throw e;
                     }
                     finally
                     {
                         if (conn.State == ConnectionState.Open)
                             conn.Close();
                         conn.Dispose();
                         cmd.Dispose();
                         da.Dispose();
                     }*/
                }

            }
            return dtCloned;
        }
        //Function to get sheet name from excel files 
        private string getSheetName(OleDbConnection ObjConn, string fileName)
        {
            string strSheetName = String.Empty;
            try
            {
                System.Data.DataTable dtSheetNames = ObjConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dtSheetNames.Rows.Count > 0)
                {
                    strSheetName = dtSheetNames.Rows[0]["TABLE_NAME"].ToString();
                }

            }
            catch (Exception)
            {
            }

            return strSheetName;
        }

        //      IViewDialog dg;
        public string ImportPolicy(DataTable TempTable)
        {
            PolicyImportStatus impStatus = null;
            //dg = wait;
            //dg.Show(".....");
            int totalEntry = TempTable.Rows.Count;
            try
            {
                if (objLog == null) objLog = new MastersClient();

                objLog.AddLog("**************" + DateTime.Now.ToString() + " Import Policy Started******************");

                CompType objCompType = new CompType();
                CompTypeTypeLst = serviceClients.CompTypeClient.GetAllComptype();
                ActionLogger.Logger.WriteLog(" Import Policy: Comptype list done : ", true);
                impStatus = serviceClients.PolicyClient.ImportPolicy(TempTable, null, SharedVMData.SelectedLicensee.LicenseeId, CompTypeTypeLst);
                objLog.AddLog("**************" + DateTime.Now.ToString() + " Import Policy Completed, count : " + TempTable.Rows.Count + " ******************");


                importStatusMsg = "Policies imported successfully!\n Total policies count: " + TempTable.Rows.Count;
                if (impStatus != null)
                {
                    importStatusMsg = "Import process is complete!\n\nNew policies: " + impStatus.ImportCount.ToString() + "\n\nUpdated policies: " + impStatus.UpdateCount + "\n\nPolicies with error: " + impStatus.ErrorCount;
                    if (impStatus.ErrorCount > 0 && impStatus.ErrorList != null)
                    {
                        importStatusMsg += "\n\nRecords not imported due to error:";
                        foreach (string s in impStatus.ErrorList)
                        {
                            importStatusMsg += "\n" + s;
                        }
                    }
                }

                //      if (MessageBox.Show(mesg) == MessageBoxResult.OK)
                //{
                //OnPropertyChanged("SelectedPolicy");
                //  if (PolicyId.Value == SelectedPolicy.PolicyId)
                //     OnSelectedLicenseeChanged(null, true, true);

                var dispatchEvnt = Application.Current.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(RefreshImport), null);
                dispatchEvnt.Completed += new EventHandler(dispatchEvnt_Completed);
                // FullRefresh();
                // MessageBox.Show(mesg);
                // }
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
                objLog.AddLog(DateTime.Now.ToString() + " Exception importing policies: " + ex.Message);
            }
            return totalEntry.ToString() + " Policies imported successfully!";
            #region Old code 
            /*
                      int totalEntry = 0;
                      //UsrLst = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId));
                      //Acme added - April 13, 2017

                      string policyIDKey = serviceClients.PolicyClient.GetPolicyUniqueKeyName();
                      objLog.AddLog("Unique Key name: " + policyIDKey);
                      try
                      {
                          if (TempTable != null)
                          {
                              int intColIndex = TempTable.Columns.Count - 1;

                              for (int i = 0; i < TempTable.Rows.Count; i++)
                              {
                                  //Get unique policyID of the  row in iteration 
                                  #region Check new PolicyID for existing or new
                                  string importedPolicyID = Convert.ToString(TempTable.Rows[i][policyIDKey]);
                                  Guid policyID=Guid.Empty;
                                  if (!string.IsNullOrEmpty(importedPolicyID))
                                  {
                                      //check if existing in DB
                                     // policyID = serviceClients.PolicyClient.IsPolicyExistingWithImportID(importedPolicyID);
                                  }
                                  else
                                  {
                                      objLog.AddLog("No unique policy ID found, so skipping the record");
                                      continue;
                                  }
                                  #endregion

                                  #region Variables init
                                  OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                  PolicyToolIncommingShedule ImportIncomingSchedule = new PolicyToolIncommingShedule();
                                  PolicyDetailsData policy = new PolicyDetailsData();

                                  OutGoingField = new ObservableCollection<OutGoingPayment>();
                                  //Acme - use policyID if existing 
                                  policy.PolicyId = (policyID == Guid.Empty) ? Guid.NewGuid() : policyID;

                                  ImportIncomingSchedule.PolicyId = policy.PolicyId;
                                  OutgoingRecord.PolicyId = policy.PolicyId;
                                  policy.IsTrackPayment = true;
                                  policy.PolicyLicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                                  policy.TerminationReasonId = null;
                                  policy.IsTrackMissingMonth = true;
                                  policy.CreatedOn = DateTime.Today;
                                  policy.IsIncomingBasicSchedule = true;
                                  policy.IsOutGoingBasicSchedule = true;
                                  policy.IsSavedPolicy = false;
                                  policy.CreatedBy = new Guid("AA38DF84-2E30-43CA-AED3-7276224D1B7E");

                                  policy.PolicyIncomingPayType = Convert.ToString(SelectedMasterIncomingPaymentType.PaymenProcedureName);

                                  //policy.IncomingPaymentTypeId = 1;

                                  string strStatus = string.Empty;
                                  string strTypeOfPolicy = string.Empty;
                                  string strClient = string.Empty;
                                  string strInsured = string.Empty;
                                  string strPolicy = string.Empty;
                                  string strEffectiveDate = string.Empty;
                                  string strTrackDate = string.Empty;
                                  string strMode = string.Empty;
                                  string strPremiuum = string.Empty;
                                  string strPayor = string.Empty;
                                  string strCarrier = string.Empty;
                                  string strProduct = string.Empty;
                                  string strProdType = string.Empty;
                                  string strSubmitted = string.Empty;
                                  string strEnroll = string.Empty;
                                  string strEligible = string.Empty;
                                  string strTermdate = string.Empty;
                                  string strTermReson = string.Empty;
                                  string strCommisionType = string.Empty;
                                  string strSchdule = string.Empty;
                                  string strFirstYear = string.Empty;
                                  string strRenewal = string.Empty;
                                  string strSplit = string.Empty;
                                  string strAdvancePayment = string.Empty;
                                  string strAccountExec = string.Empty;
                                  string strPrimaryBroker = string.Empty;
                                  string strPrimarySplit = string.Empty;
                                  string strBroker1 = string.Empty;
                                  string strsplit1 = string.Empty;
                                  string strBroker2 = string.Empty;
                                  string strsplit2 = string.Empty;
                                  string strBroker3 = string.Empty;
                                  string strsplit3 = string.Empty;
                                  string strBroker4 = string.Empty;
                                  string strsplit4 = string.Empty;
                                  string strBroker5 = string.Empty;
                                  string strsplit5 = string.Empty;
                                  string strBroker6 = string.Empty;
                                  string strsplit6 = string.Empty;
                                  string strBroker7 = string.Empty;
                                  string strsplit7 = string.Empty;
                                  string strBroker8 = string.Empty;
                                  string strsplit8 = string.Empty;
                                  string strBroker9 = string.Empty;
                                  string strsplit9 = string.Empty;
                                  string strBroker10 = string.Empty;
                                  string strsplit10 = string.Empty;
                                  #endregion



                                  #region Old implementation
                                  if (intColIndex >= 0)
                                  {
                                      try
                                      {
                                          strStatus = Convert.ToString(TempTable.Rows[i][0]);
                                          if (!string.IsNullOrEmpty(strStatus))
                                          {
                                              policy.PolicyStatusId = StatusID(strStatus);
                                          }
                                          else
                                          {
                                              policy.PolicyStatusId = 0;
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 1)
                                  {
                                      try
                                      {
                                          strTypeOfPolicy = Convert.ToString(TempTable.Rows[i][1]);
                                          if (!string.IsNullOrEmpty(strTypeOfPolicy))
                                          {
                                              policy.PolicyType = strTypeOfPolicy.ToLower() == "new" ? "New" : "Replace";
                                          }
                                          else
                                          {
                                              policy.PolicyType = "New";
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 2)
                                  {
                                      //strClient = Convert.ToString(TempTable.Rows[i][2]);
                                      try
                                      {
                                          policy.ClientName = Convert.ToString(TempTable.Rows[i][2]);
                                          Client objClient = displayedClientsLists.Where(c => c.Name.ToLower() == policy.ClientName.ToLower() && c.LicenseeId == policy.PolicyLicenseeId).FirstOrDefault();

                                          if (objClient == null)
                                          {
                                              objClient = serviceClients.ClientClient.GetClientByClientName(policy.ClientName, (Guid)policy.PolicyLicenseeId);

                                          }
                                          //Get Client ID by Get Client name
                                          if (objClient != null)
                                          {
                                              policy.ClientId = objClient.ClientId;
                                          }
                                          else
                                          {
                                              //Create new client
                                              Client objClnt = new Client();
                                              objClnt.ClientId = Guid.NewGuid();
                                              string strClientValue = string.Empty;
                                              if (policy.ClientName != null)
                                              {
                                                  if (policy.ClientName.Length > 49)
                                                  {
                                                      strClientValue = policy.ClientName.Substring(0, 49);
                                                  }
                                                  else
                                                  {
                                                      strClientValue = policy.ClientName;
                                                  }
                                              }
                                              //objClnt.Name = policy.ClientName;
                                              objClnt.Name = strClientValue;
                                              objClnt.LicenseeId = policy.PolicyLicenseeId;
                                              objClnt.IsDeleted = false;
                                              serviceClients.ClientClient.AddUpdateClient(objClnt);
                                              policy.ClientId = objClnt.ClientId;
                                          }
                                      }
                                      catch (Exception ex)
                                      {
                                          MessageBox.Show(ex.ToString());
                                      }

                                  }
                                  if (intColIndex >= 3)
                                  {
                                      try
                                      {
                                          policy.Insured = Convert.ToString(TempTable.Rows[i][3]);
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 4)
                                  {
                                      try
                                      {
                                          policy.PolicyNumber = Convert.ToString(TempTable.Rows[i][4]);
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 5)
                                  {
                                      try
                                      {
                                          strEffectiveDate = Convert.ToString(TempTable.Rows[i][5]);
                                          if (!string.IsNullOrEmpty(strEffectiveDate))
                                          {
                                              policy.OriginalEffectiveDate = Convert.ToDateTime(strEffectiveDate);
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 6)
                                  {
                                      try
                                      {
                                          strTrackDate = Convert.ToString(TempTable.Rows[i][6]);
                                          if (!string.IsNullOrEmpty(strTrackDate))
                                          {
                                              policy.TrackFromDate = Convert.ToDateTime(strTrackDate);
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 7)
                                  {
                                      try
                                      {
                                          strMode = Convert.ToString(TempTable.Rows[i][7]);
                                          if (!string.IsNullOrEmpty(strMode))
                                          {
                                              policy.PolicyModeId = PolicyModeID(strMode);
                                          }
                                          else
                                          {
                                              policy.PolicyModeId = 0;
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 8)
                                  {
                                      try
                                      {
                                          strPremiuum = Convert.ToString(TempTable.Rows[i][8]);
                                          if (!string.IsNullOrEmpty(strPremiuum))
                                          {
                                              policy.ModeAvgPremium = Convert.ToDecimal(strPremiuum);
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 9)
                                  {
                                      try
                                      {
                                          strPayor = Convert.ToString(TempTable.Rows[i][9]);
                                          policy.PayorName = strPayor;
                                          DisplayedPayor importPayor = PayorsLst.Where(p => p.PayorName.ToLower() == strPayor.ToLower()).FirstOrDefault();
                                          if (importPayor != null)
                                          {
                                              policy.PayorId = importPayor.PayorID;
                                          }

                                          else
                                          {
                                              importPayor = PayorsLst.Where(p => p.NickName.ToLower() == strPayor.ToLower()).FirstOrDefault();
                                              if (importPayor != null)
                                              {
                                                  policy.PayorId = importPayor.PayorID;
                                              }
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }
                                  if (intColIndex >= 10)
                                  {
                                      try
                                      {
                                          strCarrier = Convert.ToString(TempTable.Rows[i][10]);
                                          policy.CarrierName = strCarrier;
                                          Carrier importCarrier = CarriersLst.Where(c => c.CarrierName.ToLower() == strCarrier.ToLower()).FirstOrDefault();
                                          if (importCarrier != null)
                                          {
                                              policy.CarrierID = importCarrier.CarrierId;
                                          }
                                          else
                                          {
                                              //importCarrier = CarriersLst.Where(c => c.NickName.ToLower() == strCarrier.ToLower()).FirstOrDefault();
                                              //if (importCarrier != null)
                                              //{
                                              //    policy.CarrierID = importCarrier.CarrierId;
                                              //}
                                              if (importCarrier == null)
                                              {
                                                  List<Carrier> objList = new List<Carrier>(serviceClients.CarrierClient.GetPayorCarriers((Guid)policy.PayorId));

                                                  importCarrier = objList.Where(c => c.CarrierName.ToLower() == strCarrier.ToLower()).FirstOrDefault();
                                                  if (importCarrier != null)
                                                  {
                                                      policy.CarrierID = importCarrier.CarrierId;
                                                  }
                                                  else
                                                  {
                                                      importCarrier = objList.Where(c => c.NickName.ToLower() == strCarrier.ToLower()).FirstOrDefault();
                                                      if (importCarrier != null)
                                                      {
                                                          policy.CarrierID = importCarrier.CarrierId;
                                                      }
                                                  }

                                              }
                                          }
                                      }
                                      catch (Exception)
                                      {

                                      }

                                  }
                                  if (intColIndex >= 11)
                                  {
                                      try
                                      {
                                          strProduct = Convert.ToString(TempTable.Rows[i][11]);
                                          policy.CoverageName = strProduct;
                                          DisplayedCoverage importCoverage = AllProducts.Where(p => p.Name.ToLower() == strProduct.ToLower()).FirstOrDefault();
                                          if (importCoverage != null)
                                          {
                                              policy.CoverageId = importCoverage.CoverageID;
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }
                                  if (intColIndex >= 12)
                                  {
                                      try
                                      {
                                          strProdType = Convert.ToString(TempTable.Rows[i][12]);
                                          CoverageNickName importCoverageNickName = AllCoverageNickName.Where(p => p.NickName.ToLower() == strProdType.ToLower()).FirstOrDefault();
                                          if (importCoverageNickName != null)
                                          {
                                              policy.ProductType = importCoverageNickName.NickName;
                                          }
                                          else
                                          {
                                              policy.ProductType = strProdType;
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 13)
                                  {
                                      policy.SubmittedThrough = Convert.ToString(TempTable.Rows[i][13]);
                                  }
                                  if (intColIndex >= 14)
                                  {
                                      policy.Enrolled = Convert.ToString(TempTable.Rows[i][14]);
                                  }
                                  if (intColIndex >= 15)
                                  {
                                      policy.Eligible = Convert.ToString(TempTable.Rows[i][15]); ;
                                  }
                                  if (intColIndex >= 16)
                                  {
                                      try
                                      {
                                          strTermdate = Convert.ToString(TempTable.Rows[i][16]);
                                          if (!string.IsNullOrEmpty(strTermdate))
                                          {
                                              policy.PolicyTerminationDate = Convert.ToDateTime(strTermdate);
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }
                                  if (intColIndex >= 17)
                                  {
                                      try
                                      {
                                          strTermReson = Convert.ToString(TempTable.Rows[i][17]);
                                          if (!string.IsNullOrEmpty(strTermReson))
                                          {
                                              policy.TerminationReasonId = PolicTermisionID(strTermReson);
                                          }
                                      }
                                      catch
                                      {
                                      }
                                  }
                                  if (intColIndex >= 18)
                                  {
                                      try
                                      {
                                          strCommisionType = Convert.ToString(TempTable.Rows[i][18]);
                                          if (!string.IsNullOrEmpty(strCommisionType))
                                          {
                                              policy.CompType = PolicCompType(strCommisionType);
                                              policy.IncomingPaymentTypeId = policy.CompType;
                                          }

                                          importMasterIncomingPaymentType = masterIncomingPaymentTypeLst.Where(s => s.PaymenProcedureName.ToLower().Contains(strCommisionType.ToLower())).FirstOrDefault();
                                          policy.PolicyIncomingPayType = Convert.ToString(importMasterIncomingPaymentType.PaymenProcedureName);

                                      }
                                      catch
                                      {
                                      }

                                  }
                                  if (intColIndex >= 19)
                                  {
                                      try
                                      {
                                          strSchdule = Convert.ToString(TempTable.Rows[i][19]);
                                          if (!string.IsNullOrEmpty(strSchdule))
                                          {
                                              ImportIncomingSchedule.ScheduleTypeId = PolicyIncomingSchuduleId(strSchdule.ToLower());
                                          }
                                          else
                                          {
                                              ImportIncomingSchedule.ScheduleTypeId = 1;
                                          }
                                          try
                                          {
                                              strFirstYear = Convert.ToString(TempTable.Rows[i][20]);
                                              strFirstYear = strFirstYear.Replace("%", "");
                                              ImportIncomingSchedule.FirstYearPercentage = Convert.ToDouble(strFirstYear);
                                          }
                                          catch
                                          {
                                          }

                                          try
                                          {
                                              strRenewal = Convert.ToString(TempTable.Rows[i][21]);
                                              strRenewal = strRenewal.Replace("%", "");
                                              policy.RenewalPercentage = strRenewal;
                                              ImportIncomingSchedule.RenewalPercentage = Convert.ToDouble(strRenewal);
                                          }
                                          catch
                                          {
                                          }

                                          try
                                          {
                                              strSplit = Convert.ToString(TempTable.Rows[i][22]);
                                              strSplit = strSplit.Replace("%", "");
                                              if (!string.IsNullOrEmpty(strSplit))
                                              {
                                                  policy.SplitPercentage = Convert.ToDouble(strSplit);
                                              }
                                          }
                                          catch
                                          {
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }

                                  if (intColIndex >= 23)
                                  {
                                      try
                                      {
                                          strAdvancePayment = Convert.ToString(TempTable.Rows[i][23]);
                                          if (!string.IsNullOrEmpty(strAdvancePayment))
                                          {
                                              policy.Advance = Convert.ToInt32(strAdvancePayment);
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }

                                  if (intColIndex >= 24)
                                  {
                                      try
                                      {
                                          strAccountExec = Convert.ToString(TempTable.Rows[i][24]);
                                          if (!string.IsNullOrEmpty(strAccountExec))
                                          {
                                              Guid tempGuid = new Guid(strAccountExec);
                                              User objUser = SharedVMData.GlobalAgentList.Where(d => d.UserCredentialID == tempGuid).FirstOrDefault();
                                              //Need to get nick name
                                              if (string.IsNullOrEmpty(objUser.NickName))
                                              {
                                                  policy.AccoutExec = objUser.NickName;
                                                  policy.UserCredentialId = tempGuid;
                                              }
                                              else
                                              {
                                                  policy.AccoutExec = objUser.UserName;
                                                  policy.UserCredentialId = tempGuid;
                                              }
                                              serviceClients.UserClient.CheckAccoutExecAsync((Guid)policy.UserCredentialId);
                                          }
                                      }
                                      catch
                                      {
                                      }

                                  }

                                  if (intColIndex >= 25)
                                  {
                                      if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][25])))
                                      {
                                          strPrimaryBroker = Convert.ToString(TempTable.Rows[i][25]);

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 25)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][25])))
                                                  {

                                                      try
                                                      {
                                                          //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                                          Guid guidBrokerID = new Guid(strPrimaryBroker);
                                                          User objUser = new User();
                                                          objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guidBrokerID).FirstOrDefault();
                                                          if (objUser != null)
                                                          {
                                                              string strFirstPer = Convert.ToString(TempTable.Rows[i][26]);
                                                              string strRenewalPer = Convert.ToString(TempTable.Rows[i][27]);
                                                              OutgoingRecord = new OutGoingPayment();
                                                              OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                              OutgoingRecord.PolicyId = policy.PolicyId;
                                                              OutgoingRecord.IsPrimaryAgent = true;
                                                              if (string.IsNullOrEmpty(strFirstPer))
                                                              {
                                                                  strFirstPer = "0";
                                                              }
                                                              if (string.IsNullOrEmpty(strRenewalPer))
                                                              {
                                                                  strRenewalPer = "0";
                                                              }
                                                              OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                              OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                              OutgoingRecord.Payor = policy.PayorNickName;
                                                              OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                              OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                              OutGoingField.Add(OutgoingRecord);

                                                          }
                                                          else
                                                          {
                                                          }
                                                      }
                                                      catch
                                                      {
                                                      }

                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 28)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][28])))
                                                  {
                                                      strBroker1 = Convert.ToString(TempTable.Rows[i][28]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 28)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][28])))
                                                          {

                                                              try
                                                              {
                                                                  Guid guidBrokerID = new Guid(strBroker1);
                                                                  User objUser = new User();
                                                                  objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guidBrokerID).FirstOrDefault();
                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstYearPer = Convert.ToString(TempTable.Rows[i][29]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][30]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstYearPer))
                                                                      {
                                                                          strFirstYearPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }

                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstYearPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);


                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 31)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][31])))
                                                  {
                                                      strBroker2 = Convert.ToString(TempTable.Rows[i][31]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 29)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][31])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guidBrokerID = new Guid(strBroker2);
                                                                  User objUser = new User();
                                                                  objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guidBrokerID).FirstOrDefault();
                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstYearPer = Convert.ToString(TempTable.Rows[i][32]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][33]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      OutgoingRecord.IsPrimaryAgent = false;

                                                                      if (string.IsNullOrEmpty(strFirstYearPer))
                                                                      {
                                                                          strFirstYearPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstYearPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 34)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][34])))
                                                  {
                                                      strBroker3 = Convert.ToString(TempTable.Rows[i][34]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 34)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][34])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker3);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstYearPer = Convert.ToString(TempTable.Rows[i][35]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][36]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;

                                                                      if (string.IsNullOrEmpty(strFirstYearPer))
                                                                      {
                                                                          strFirstYearPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstYearPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);
                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 37)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][37])))
                                                  {
                                                      strBroker4 = Convert.ToString(TempTable.Rows[i][37]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 37)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][37])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker4);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstPer = Convert.ToString(TempTable.Rows[i][38]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][39]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;

                                                                      if (string.IsNullOrEmpty(strFirstPer))
                                                                      {
                                                                          strFirstPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }

                                                          }
                                                      }
                                                  }
                                              }

                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 40)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][40])))
                                                  {
                                                      strBroker5 = Convert.ToString(TempTable.Rows[i][40]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 39)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][40])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker5);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstPer = Convert.ToString(TempTable.Rows[i][41]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][42]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstPer))
                                                                      {
                                                                          strFirstPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 43)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][43])))
                                                  {
                                                      strBroker6 = Convert.ToString(TempTable.Rows[i][43]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 43)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][43])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker6);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstPer = Convert.ToString(TempTable.Rows[i][44]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][45]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstPer))
                                                                      {
                                                                          strFirstPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 46)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][46])))
                                                  {
                                                      strBroker7 = Convert.ToString(TempTable.Rows[i][46]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 46)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][46])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker7);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstYearPer = Convert.ToString(TempTable.Rows[i][47]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][48]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstYearPer))
                                                                      {
                                                                          strFirstYearPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstYearPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 49)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][49])))
                                                  {
                                                      strBroker8 = Convert.ToString(TempTable.Rows[i][49]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 48)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][49])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker8);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstPer = Convert.ToString(TempTable.Rows[i][50]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][51]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstPer))
                                                                      {
                                                                          strFirstPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);


                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 52)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][52])))
                                                  {
                                                      strBroker9 = Convert.ToString(TempTable.Rows[i][52]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 52)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][52])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker9);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstPer = Convert.ToString(TempTable.Rows[i][52]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][53]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstPer))
                                                                      {
                                                                          strFirstPer = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstPer);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);


                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }

                                          if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                          {
                                              if (intColIndex >= 54)
                                              {
                                                  if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][54])))
                                                  {
                                                      strBroker10 = Convert.ToString(TempTable.Rows[i][54]);
                                                      //SelectedPolicy.Payor = strPayor;
                                                      if (intColIndex >= 52)
                                                      {
                                                          if (!string.IsNullOrWhiteSpace(Convert.ToString(TempTable.Rows[i][54])))
                                                          {
                                                              try
                                                              {
                                                                  Guid guid = new Guid(strBroker10);
                                                                  User objUser = SharedVMData.GlobalAgentList.Where(u => u.UserCredentialID == guid).FirstOrDefault();

                                                                  if (objUser != null)
                                                                  {
                                                                      string strFirstper = Convert.ToString(TempTable.Rows[i][55]);
                                                                      string strRenewalPer = Convert.ToString(TempTable.Rows[i][56]);

                                                                      OutgoingRecord = new OutGoingPayment();
                                                                      OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                                      OutgoingRecord.IsPrimaryAgent = false;
                                                                      OutgoingRecord.PolicyId = policy.PolicyId;
                                                                      if (string.IsNullOrEmpty(strFirstper))
                                                                      {
                                                                          strFirstper = "0";
                                                                      }
                                                                      if (string.IsNullOrEmpty(strRenewalPer))
                                                                      {
                                                                          strRenewalPer = "0";
                                                                      }
                                                                      OutgoingRecord.FirstYearPercentage = Convert.ToDouble(strFirstper);
                                                                      OutgoingRecord.RenewalPercentage = Convert.ToDouble(strRenewalPer);
                                                                      OutgoingRecord.Payor = policy.PayorNickName;
                                                                      OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialID;
                                                                      OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                                      OutGoingField.Add(OutgoingRecord);

                                                                  }
                                                              }
                                                              catch
                                                              {
                                                              }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }

                                  }

                                  #endregion
                                  _PolicyServiceClient.PolicyClient.SavePolicy(policy, SelecetdPolicylstForReplace, strRenewal, strProdType);

                                  if (ImportIncomingSchedule != null)
                                  {
                                      ImportIncomingSchedule.PolicyId = policy.PolicyId;
                                      ImportIncomingSchedule.IncomingScheduleId = Guid.NewGuid();
                                      serviceClients.PolicyIncomingScheduleClient.AddUpdatePolicyToolIncommingShedule(ImportIncomingSchedule);
                                  }

                                  if (OutGoingField != null)
                                  {
                                      serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField);
                                  }

                                  totalEntry = totalEntry + 1;

                              }
                          }
                          objLog.AddLog("**************" + DateTime.Now.ToString() + " Import Policy Completed, count : " + totalEntry + " ******************");
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show(ex.ToString());
                          objLog.AddLog( DateTime.Now.ToString() + " Exception importing policies: " + ex.Message);
                      }
                      MessageBox.Show("Total imported policy is: " + totalEntry.ToString());*/
            #endregion
        }

        void dispatchEvnt_Completed(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show(Application.Current.MainWindow, importStatusMsg);
        }

        public void RefreshImport()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ReferseEnable = false;
                Guid? ClientId = null;
                if (SharedVMData.SelectedLicensee != null)
                {
                    var tempAllProducts = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);

                    DisplayedCoverage emptyCoverage = new DisplayedCoverage { CoverageID = Guid.Empty, Name = string.Empty };


                    if (AllProducts != null)
                    {
                        var productNames = new HashSet<string>(AllProducts.Select(x => x.Name));
                        var DiffProducts = tempAllProducts.Where(x => !productNames.Contains(x.Name));

                        if (SelectedPolicy != null)
                            SelectedPolicy.Coverage = AllProducts.Where(p => p.CoverageID == SelectedPolicy.CoverageId).FirstOrDefault();

                        AllProducts.AddRange(DiffProducts);
                        AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
                    }
                    //Call to updated client 

                    DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                    //  AllProducts.Add(emptyCoverage);
                    if (SelectedDisplayClient != null && SelectedDisplayClient.ClientId != Guid.Empty)
                    {
                        ClientId = SelectedDisplayClient.ClientId;
                        //Refresh smart fields
                        if (DisplayedClientsLists.Count > 0)
                        {
                            if (VMPolicySmartField != null)
                            {
                                VMPolicySmartField.DisplayedClientsLists = DisplayedClientsLists;
                                VMPolicySmartField.SelectedClientLrnd = selectedClient;
                            }
                        }
                    }
                    else
                    {
                        if (DisplayedClientsLists.Count > 0)
                        {
                            SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                            SelectedClient = SelectedDisplayClient;
                            VMPolicySmartField.DisplayedClientsLists = DisplayedClientsLists;
                            VMPolicySmartField.SelectedClientLrnd = selectedClient;
                        }
                    }
                    //   OnSelectedLicenseeChanged(ClientId, true);
                }

            }
            finally
            {
                //Thread.Sleep(30000);
                ReferseEnable = true;
                Mouse.OverrideCursor = null;
            }
        }


        private int PolicyModeID(string strModeType)
        {
            int intStatus = 0;

            switch (strModeType.ToLower())
            {
                case "monthly":
                    intStatus = 0;
                    break;

                case "quarterly":
                    intStatus = 1;
                    break;

                case "semi-annually":
                    intStatus = 2;
                    break;

                case "annually":
                    intStatus = 3;
                    break;

                case "one time":
                    intStatus = 4;
                    break;

                case "random":
                    intStatus = 5;
                    break;

                default:
                    intStatus = 0;
                    break;
            }
            return intStatus;
        }

        private int StatusID(string strStatus)
        {
            int intStatus = 0;

            switch (strStatus.ToLower())
            {
                case "Active":
                    intStatus = 0;
                    break;

                case "Terminated":
                    intStatus = 1;
                    break;

                case "Pending":
                    intStatus = 2;
                    break;

                default:
                    intStatus = 0;
                    break;
            }
            return intStatus;
        }

        private int PolicTermisionID(string strTermReason)
        {
            int intStatus = 0;

            switch (strTermReason.ToLower())
            {
                case "replaced by new policy":
                    intStatus = 0;
                    break;

                case "lost to competitor":
                    intStatus = 1;
                    break;

                case "voluntary":
                    intStatus = 2;
                    break;

                case "out of business":
                    intStatus = 3;
                    break;

                case "non-payment":
                    intStatus = 4;
                    break;

                case "per carrier":
                    intStatus = 5;
                    break;

                default:
                    intStatus = 0;
                    break;
            }
            return intStatus;
        }

        private ObservableCollection<CompType> _compTypeTypeLst;
        public ObservableCollection<CompType> CompTypeTypeLst
        {
            get
            {
                return _compTypeTypeLst;
            }
            set
            {
                _compTypeTypeLst = value;
                OnPropertyChanged("CompTypeTypeLst");
            }
        }

        private int PolicCompType(string strCompType)
        {
            int intStatus = 5;

            try
            {
                if (string.IsNullOrEmpty(strCompType))
                {
                    //Default Pending
                    return intStatus = 5;
                }
                CompType objComp = CompTypeTypeLst.Where(p => p.Names.ToLower() == strCompType.ToLower()).FirstOrDefault();
                if (objComp != null)
                {
                    if (objComp.IncomingPaymentTypeID != null)
                    {
                        intStatus = Convert.ToInt32(objComp.IncomingPaymentTypeID);
                    }
                }
                else
                {
                    objComp = CompTypeTypeLst.Where(p => p.PaymentTypeName.ToLower() == strCompType.ToLower()).FirstOrDefault();

                    if (objComp != null)
                    {
                        if (objComp.IncomingPaymentTypeID != null)
                        {
                            intStatus = Convert.ToInt32(objComp.IncomingPaymentTypeID);
                        }
                    }
                }
            }
            catch
            {
            }

            return intStatus;
        }

        private int PolicyIncomingSchuduleId(string strCompType)
        {
            int intStatus = 1;

            //1:PercentageOfPremium
            //2:PerHead
            if (strCompType.ToLower().Contains("head"))
            {
                intStatus = 2;
            }
            else
            {
                intStatus = 1;
            }

            return intStatus;
        }

        //private int PolicyIncomingSchuduleId(string strCompType)
        //{
        //    int intStatus = 1;

        //    switch (strCompType.ToLower())
        //    {
        //        case "percentage of premium":
        //            intStatus = 1;
        //            break;

        //        case "percentage of target":
        //            intStatus = 2;
        //            break;

        //        case "per head fee scale":
        //            intStatus = 3;
        //            break;

        //        case "per head fee target":
        //            intStatus = 4;
        //            break;

        //        case "Flat $":
        //            intStatus = 5;
        //            break;

        //        default:
        //            intStatus = 1;
        //            break;
        //    }
        //    return intStatus;
        //}

        public bool isAvoideTocall = false;


        private void SavePoliciesData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;

                if (SelectedPolicy.OriginalEffectiveDate == null)
                {
                    if (SelectedPolicy.Advance != null)
                    {
                        MessageBox.Show("Advance payment will not be saved.because effective is empty", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                        SelectedPolicy.Advance = null;
                    }
                }

                try
                {
                    //Make sure custom incoming is not blank 
                    if (SelectedPolicyToolIncommingShedule.Mode == Mode.Custom)
                    {
                        if (SelectedPolicy.OriginalEffectiveDate == null)
                        {
                            MessageBox.Show("Effective date cannot be blank with incoming schedule in 'Custom' mode", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        if ((SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded && (GradedList == null || (GradedList != null && GradedList.Count == 0)))
                          ||
                          (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded && (NonGradedList == null || (NonGradedList != null && NonGradedList.Count == 0)))
                        )

                        {
                            MessageBox.Show("Incoming Schedule cannot be blank for the policy in 'Custom' mode", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        else
                        {
                            if (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded && GradedList != null && GradedList.Count == 1)
                            {
                                if (GradedList[0].From == 0 || GradedList[0].To == 0 || GradedList[0].Percent == 0)
                                {
                                    MessageBox.Show("Incoming Schedule cannot have blank or 0 values in 'Custom' mode.", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                            }
                            if (SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded && NonGradedList != null && NonGradedList.Count == 1)
                            {
                                if (NonGradedList[0].Year == 0 || NonGradedList[0].Percent == 0)
                                {
                                    MessageBox.Show("Incoming Schedule cannot have blank or 0 values in 'Custom' mode.", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                            }
                        }
                    }

                    if (OutGoingField.Count > 0)
                    {
                        if (SelectedPolicy.OriginalEffectiveDate == null)
                        {
                            MessageBox.Show("Effective date is required with outgoing commission splits.", "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        string strValid = ValidateOutgoingScheduleForSave();
                        if (strValid != string.Empty)
                        {
                            MessageBox.Show(strValid, "Compdept", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    objLog.AddLog(DateTime.Now.ToString() + ": Exception in SavePoliciesData:" + SelectedPolicy.PolicyId + ", exception: " + ex.Message);
                }

                //bool lockObtained = false;
                //lockObtained = PolicyLockingClient.LockPolicy(SelectedPolicy.PolicyId);
                //bool ReplaceOccur = false;
                bool? RunFollowUp = null;

                if (SelectedPolicy.PolicyType == "Replace")
                {
                    if (SelecetdPolicylstForReplace != null)
                    {
                        //ReplaceOccur = true;
                        SelecetdPolicylstForReplace.PolicyPreviousData.OriginalEffectiveDate = SelecetdPolicylstForReplace.OriginalEffectiveDate;
                        SelecetdPolicylstForReplace.PolicyPreviousData.PolicyModeId = SelecetdPolicylstForReplace.PolicyModeId;
                        SelecetdPolicylstForReplace.PolicyPreviousData.TrackFromDate = SelecetdPolicylstForReplace.TrackFromDate;
                        SelecetdPolicylstForReplace.IsSavedPolicy = true;
                        SelecetdPolicylstForReplace.PolicyStatusId = (int)_PolicyStatus.Terminated;
                        SelecetdPolicylstForReplace.PolicyStatusName = _PolicyStatus.Terminated.ToString();
                        //Last modified info
                        SelecetdPolicylstForReplace.LastModifiedBy = RoleManager.userCredentialID;
                    }
                }
                else
                {
                    ReplaceRbStatus = true;
                    SelecetdPolicylstForReplace = null;
                }

                if (SelectedPolicy != null)
                {
                    if (SelectedPolicy.PolicyPreviousData == null)
                    {
                        SelectedPolicy.PolicyPreviousData = new PolicyDetailPreviousData();

                    }
                    if (SelectedPolicy.PolicyPreviousData.OriginalEffectiveDate != SelectedPolicy.OriginalEffectiveDate)
                    {
                        RunFollowUp = false;
                    }
                    else if (SelectedPolicy.PolicyPreviousData.PolicyModeId != SelectedPolicy.PolicyModeId)
                    {
                        RunFollowUp = true;
                    }
                    else if (SelectedPolicy.PolicyPreviousData.TrackFromDate != SelectedPolicy.TrackFromDate)
                    {
                        RunFollowUp = false;
                    }

                    else if (SelectedPolicy.PolicyPreviousData.PolicyTermdateDate != SelectedPolicy.PolicyTerminationDate)
                    {
                        if (SelectedPolicy.LearnedFields == null)
                        {
                            SelectedPolicy.LearnedFields = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);
                        }

                        PolicyLearnedFieldData _PolicyLearnedField = SelectedPolicy.LearnedFields;
                        _PolicyLearnedField.ProductType = SelectedPolicy.ProductType;

                        if (_PolicyLearnedField != null)
                        {
                            DateTime? LrndTrmDate = _PolicyLearnedField != null ? _PolicyLearnedField.AutoTerminationDate : null;
                            DateTime? GreaterDate = GetGreaterDate(LrndTrmDate, SelectedPolicy.PolicyTerminationDate);
                            _PolicyLearnedField.AutoTerminationDate = GreaterDate;
                            serviceClients.PolicyLearnedFieldClient.AddUpdatePolicyLearnedField(_PolicyLearnedField, _PolicyLearnedField.ProductType);
                        }
                        RunFollowUp = false;
                    }

                    SelectedPolicy.PolicyPreviousData.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                    SelectedPolicy.PolicyPreviousData.PolicyModeId = SelectedPolicy.PolicyModeId;
                    SelectedPolicy.PolicyPreviousData.TrackFromDate = SelectedPolicy.TrackFromDate;
                    SelectedPolicy.IsSavedPolicy = true;

                    if ((policyList.Where(p => p.PolicyId == SelectedPolicy.PolicyId).Count() == 0))
                    {
                        AddPolicyToList(SelectedPolicy);
                    }
                }

                //Change for Renewal fields to update smart fields

                if (SelectedAccountExecLst != null)
                {
                    if (!string.IsNullOrEmpty(SelectedAccountExecLst.NickName))
                    {
                        SelectedPolicy.AccoutExec = SelectedAccountExecLst.NickName;
                        SelectedPolicy.UserCredentialId = SelectedAccountExecLst.UserCredentialID;
                    }
                    else
                    {
                        SelectedPolicy.AccoutExec = null;
                        SelectedPolicy.UserCredentialId = null;
                    }
                }
                //acme - with new outgoing spli
                SelectedPolicy.IsCustomBasicSchedule = IsCustomDateSelected;
                SelectedPolicy.IsTrackPayment = IsTrackPayment;
                if (isCustomDateSelected)
                {
                    SelectedPolicy.CustomDateType = (IsOutScheduleEntered) ? "Entered" : ((IsOutScheduleInvoice) ? "Invoice" : "");
                }

                if (SelectedPrimaryAgent != null && SelectedPrimaryAgent.UserCredentialID != null && SelectedPrimaryAgent.UserCredentialID != Guid.Empty)
                {
                    SelectedPolicy.PrimaryAgent = SelectedPrimaryAgent.UserCredentialID;
                }



                SelectedPolicy.RenewalPercentage = Convert.ToString(SelectedPolicyToolIncommingShedule.RenewalPercentage);

                string strRenewal = Convert.ToString(SelectedPolicyToolIncommingShedule.RenewalPercentage);

                string strSelectedCoverageNickName = string.Empty;

                if (SelectedCoverageNickName != null)
                {
                    strSelectedCoverageNickName = SelectedCoverageNickName.NickName;
                    SelectedPolicy.ProductType = SelectedCoverageNickName.NickName;
                }

                //Set and update comp type when change SelectedMasterIncomingPaymentType
                SelectedPolicy.IncomingPaymentTypeId = SelectedMasterIncomingPaymentType.PaymentTypeId;
                selectedPolicy.CompType = SelectedMasterIncomingPaymentType.PaymentTypeId;

                // New parameter in outgoing
                SelectedPolicy.IsTieredSchedule = IsTiered;

                //Last modified information here 
                SelectedPolicy.LastModifiedBy = RoleManager.userCredentialID;


                if (isClickOnAddPolicy)
                {
                    var varOutExecption = _PolicyServiceClient.PolicyClient.SavePolicy(SelectedPolicy, SelecetdPolicylstForReplace, strRenewal, strSelectedCoverageNickName, RoleManager.userCredentialID);

                    ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

                    if ((_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0) || RunFollowUp.HasValue)
                    {
                        FollowUpcalled = true;
                        Thread newThread = new Thread(new ThreadStart(ThreadFollowUpStartingPoint));
                        newThread.SetApartmentState(ApartmentState.STA);
                        newThread.IsBackground = true;
                        newThread.Start();
                    }
                    else
                    {
                        FollowUpcalled = false;
                    }
                    //Commented on 
                    //if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                    //{
                    //    if (SelectedPolicySaved != null)
                    //        SelectedPolicySaved();
                    //}
                    //else
                    //{
                    #region"Save incoming schedule"
                    SaveIncomingSchedule();
                    #endregion

                    if (OutGoingField != null && OutGoingField.Count != 0)
                    {

                        // Ankit - Add a new check for Save customStartDate for scheduleTypeId='2'
                        if (isCustomDateSelected)
                        {
                            AddEndDatesInOutgoingSchedule();
                            //serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected, IsTiered);
                        }
                        //else
                        //{
                        //    if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2) && (OutGoingField.ToList().Sum(p => p.FirstYearPercentage) == 100) && (OutGoingField.ToList().Sum(p => p.RenewalPercentage) == 100))
                        //    {
                        serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected, IsTiered);

                        //    }
                        //    else if (OutGoingField.FirstOrDefault().ScheduleTypeId == 1)
                        //    {
                        //        serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected,IsTiered);
                        //    }
                        //}


                    }

                    if (_deleteseletedOutGoingField.Count != 0)
                    {

                        foreach (OutGoingPayment paymen in _deleteseletedOutGoingField)
                        {
                            //[Added By Ankit]
                            //[Aug06,2018-We can add this check for checking the policyId of selected policy   is equal to deleteSelectedOutgoingField policyId  if not then don't deleted the outgoingschedules]
                            if (SelectedPolicy.PolicyId == paymen.PolicyId)
                            {
                                serviceClients.OutGoingPaymentClient.DeleteOutgoingPayment(paymen);
                            }
                            else
                            {
                                ActionLogger.Logger.WriteLog(DateTime.Now + " -SavePolicyData, Selected policyId is not equals to OutgoingScheduledpolicyId " + "SelectedPolicyId:" + SelectedPolicy.PolicyId + "" + "OutgoingScheduledpolicyId:" + paymen.PolicyId + "" + "OutgoingScheduledId:" + paymen.OutgoingScheduleId, true);
                            }
                        }
                        _deleteseletedOutGoingField.Clear();
                    }

                    //}

                    if (SelectedPolicySaved != null)
                        SelectedPolicySaved();

                    UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.PolicyDetail);
                    strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));
                    filterPolicy();
                }
                else
                {
                    //_PolicyServiceClient.PolicyClient.SavePolicyAsync(SelectedPolicy, SelecetdPolicylstForReplace, strRenewal, strSelectedCoverageNickName);

                    _PolicyServiceClient.PolicyClient.SavePolicy(SelectedPolicy, SelecetdPolicylstForReplace, strRenewal, strSelectedCoverageNickName, RoleManager.userCredentialID);

                    ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

                    if ((_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0) || RunFollowUp.HasValue)
                    {
                        FollowUpcalled = true;

                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += new System.ComponentModel.DoWorkEventHandler(FollowUP_DoWork);
                        worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(FollowUP_RunWorkerCompleted);
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        FollowUpcalled = false;
                    }
                    //if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                    //{
                    //if (SelectedPolicySaved != null)
                    //    SelectedPolicySaved();
                    //}
                    //else
                    //{
                    Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
                    newWindowThread.SetApartmentState(ApartmentState.STA);
                    newWindowThread.IsBackground = true;
                    newWindowThread.Start();

                    //    #region"Save incoming schedule"
                    //    SaveIncomingSchedule();
                    //    #endregion

                    //    if (OutGoingField != null && OutGoingField.Count != 0)
                    //    {
                    //        if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2) && (OutGoingField.ToList().Sum(p => p.FirstYearPercentage) == 100) && (OutGoingField.ToList().Sum(p => p.RenewalPercentage) == 100))
                    //        {
                    //            serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPaymentAsync(OutGoingField);

                    //        }
                    //        else if (OutGoingField.FirstOrDefault().ScheduleTypeId == 1)
                    //        {
                    //            serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPaymentAsync(OutGoingField);
                    //        }
                    //    }

                    //    if (_deleteseletedOutGoingField.Count != 0)
                    //    {
                    //        foreach (OutGoingPayment paymen in _deleteseletedOutGoingField)
                    //        {
                    //            serviceClients.OutGoingPaymentClient.DeleteOutgoingPayment(paymen);
                    //        }
                    //        _deleteseletedOutGoingField.Clear();
                    //    }
                    //}

                    //if (SelectedPolicySaved != null)
                    //{
                    //    isAvoideTocall = true;
                    //    SelectedPolicySaved();
                    //    isAvoideTocall = false;
                    //}

                    //UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.PolicyDetail);
                    //isAvoideTocall = false;
                    //strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));
                    // }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            isClickOnAddPolicy = false;
        }

        /// <summary>
        /// Method tovalidate tier numbers in the schedules.
        /// </summary>
        /// <param name="tierList"></param>
        /// <returns></returns>
        string ValidateTierNumbers(List<int?> tierList)
        {
            string result = "";
            //If anyone tier missing, return error
            if (!tierList.Contains(1))
            {
                result = "Please configure Tier 1 for the schedule.";
                return result;
            }
            //if (!tierList.Contains(2))
            //{
            //    result = "Please configure Tier 2 for the schedule.";
            //    return result;
            //}
            //If any number other that 1&2 exist in list, return error
            foreach (int? tier in tierList)
            {
                if (tier == null)
                {
                    result = "Please note that Tier number cannot be blank and must be mentioned as 1 or 2 for the schedule.";
                    return result;
                }
                if (tier != 1 && tier != 2)
                {
                    result = "Please note that 'Tiered' schedule cannot have tier numbers other than 1 and 2 for the schedule.";
                    return result;
                }
            }
            return result;
        }

        public string ValidateOutgoingScheduleForSave()
        {
            string result = "";
            User house = GetHouseAccount();
            //Check if tier 2 present in outgoing schedule or not
            bool isTier2Present = OutGoingField.Where(x => x.TierNumber == 2).ToList().Count > 0;

            if (!IsCustomDateSelected && OutGoingField != null && OutGoingField.Count != 0)
            {

                //Check that there for tiered schedule, there cannot be more than 2 tiers in schedule
                if (IsTiered && OutGoingField != null && OutGoingField.Count > 0)
                {
                    //Validate tier numbers
                    List<int?> tierList = OutGoingField.Select(x => x.TierNumber).Distinct().ToList();
                    result = ValidateTierNumbers(tierList);
                    if (!string.IsNullOrEmpty(result))
                        return result;

                    //Validate house account to exist only once in one tier
                    //-check no two house accounts in one schedule of Tier 1
                    int houseCount = OutGoingField.Where(x => (x.PayeeUserCredentialId == house.UserCredentialID) && x.TierNumber == 1).Count();
                    if (houseCount >= 2)
                    {
                        result = "House account is added more than once in Tier 1 in the outgoing schedule. Please make sure that house account appears only once per tier in schedule.";
                        return result;
                    }

                    //-check no two house accounts in one schedule of Tier 2
                    houseCount = OutGoingField.Where(x => (x.PayeeUserCredentialId == house.UserCredentialID) && x.TierNumber == 2).Count();
                    if (houseCount >= 2)
                    {
                        result = "House account is added more than once in Tier 2 in the outgoing schedule. Please make sure that house account appears only once per tier in schedule.";
                        return result;
                    }
                }

                //tiered schedule validation - House account should not be added more than once in basic scehdule
                if (!IsTiered && house.UserCredentialID != null && house.UserCredentialID != Guid.Empty && OutGoingField.Where(p => p.PayeeUserCredentialId == house.UserCredentialID).Count() > 1)
                {
                    result = "House account is added more than once in the outgoing schedule. Please make sure that house account appears only once in schedule.";
                    return result;
                }

                if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2))
                {
                    double dbFirstYear = 0.0;
                    double dbRenewal = 0.0;


                    foreach (var item in OutGoingField)
                    {
                        //Acme added March 13, 2019 - to cover tiered schedule scenario
                        if (!IsTiered ||
                            (IsTiered && isTier2Present && (item.TierNumber == 2)) ||
                            (IsTiered && !isTier2Present && (item.TierNumber == 1))
                            )
                        {
                            dbFirstYear = dbFirstYear + Convert.ToDouble(item.FirstYearPercentage);
                            dbRenewal = dbRenewal + Convert.ToDouble(item.RenewalPercentage);
                        }
                    }

                    dbFirstYear = Convert.ToDouble(dbFirstYear.ToString("n2"));
                    dbRenewal = Convert.ToDouble(dbRenewal.ToString("n2"));

                    //if ((OutGoingField.ToList().Sum(p => p.FirstYearPercentage) != 100) || (OutGoingField.ToList().Sum(p => p.RenewalPercentage) != 100))
                    if ((dbFirstYear != 100) || (dbRenewal != 100))
                    {
                        result = "FirstYear/Renewal not equal to 100%.";
                        return result;
                        //PolicyDetailSaveToolTip = "FirstYear/Renewal not equal to 100.";
                        //Changedcolor = true;
                        //Savecontent = "! Save";
                        //imagePath = null;
                        //return ;
                    }
                }
                else
                {
                    double? expectedfirst = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
                    double? expectedRenewal = SelectedPolicyToolIncommingShedule.RenewalPercentage;

                    //Check if custom mode, then 
                    if (SelectedPolicyToolIncommingShedule.Mode == Mode.Custom && SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded)
                    {
                        int year = getPolicyAgeFromEffective(DateTime.Now);
                        expectedfirst = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == 1).FirstOrDefault().Percent;

                        int maxYear = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderByDescending(x => x.Year).FirstOrDefault().Year;
                        year = (year > maxYear) ? maxYear : year;
                        expectedRenewal = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderBy(x => x.Year).Where(x => x.Year == year).FirstOrDefault().Percent;

                        //added - july 27, 2020 after analysis during BG/caravus integration
                        if (year == 1) // if policy is in year 1 , then use next higher year's % as renewal%
                        {
                            var year2Record = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderBy(x => x.Year).Where(x => x.Year > 1).FirstOrDefault();
                            if (year2Record != null) //This also handles the case where only 1 year is defined in non-graded schedule .
                            {
                                expectedRenewal = year2Record.Percent;
                            }
                        }
                        //if (expectedRenewal == null || expectedRenewal == 0)
                        //{
                        //    expectedRenewal = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == maxYear).FirstOrDefault().Percent;
                        //}

                    }
                    if (IsTiered)
                    {
                        if ((OutGoingField.Where(x => (isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1)).Sum(p => p.FirstYearPercentage) != expectedfirst) ||
                            (OutGoingField.Where(x => (isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1)).Sum(p => p.RenewalPercentage) != expectedRenewal))
                        {
                            result = "Incoming FirstYear/Renewal not equal to Outgoing FirstYear/Renewal.";
                            return result;
                        }
                    }
                    else
                    {

                        if ((OutGoingField.Sum(p => p.FirstYearPercentage) != expectedfirst) ||
                                (OutGoingField.Sum(p => p.RenewalPercentage) != expectedRenewal))
                        {
                            result = "Incoming FirstYear/Renewal not equal to Outgoing FirstYear/Renewal.";
                            return result;
                            //PolicyDetailSaveToolTip = "Incoming FirstYear/Renewal not equal to Outgoing FirstYear/Renewal.";
                            //Changedcolor = true;
                            //Savecontent = "! Save";
                            //imagePath = null;
                            // return false;
                        }

                    }
                }
            } //When custom date selected 
            else if (IsCustomDateSelected && (OutGoingField != null && OutGoingField.Count != 0))
            {
                try
                {
                    var sdates = (from s in OutGoingField
                                  select new { s.CustomStartDate }).Distinct().OrderBy(x => x.CustomStartDate).ToList();

                    //No date to be null
                    if (sdates.Where(x => x.CustomStartDate == null).ToList().Count > 0)
                    {
                        result = "Missing start date in outgoing schedule.";
                        return result;
                        //PolicyDetailSaveToolTip = "Missing start date in outgoing schedule.";
                        //Changedcolor = true;
                        //Savecontent = "! Save";
                        //imagePath = null;
                        //return false;
                    }

                    //Tiered Schedule validations
                    if (IsTiered && sdates.Count > 0)
                    {
                        foreach (var i in sdates)
                        {
                            //Check tiers to be proper for each date
                            List<int?> tierList = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate).Select(x => x.TierNumber).Distinct().ToList();
                            result = ValidateTierNumbers(tierList);

                            if (!string.IsNullOrEmpty(result))
                            {
                                return result.Replace(".", " starting " + i.CustomStartDate.Value.ToShortDateString() + ".");
                            }

                            //-check no two house accounts in one schedule of Tier 1
                            int houseCount = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate) && x.TierNumber == 1).Count(x => x.PayeeUserCredentialId == house.UserCredentialID);
                            if (houseCount >= 2)
                            {
                                result = "House account is added more than once in Tier 1 in the outgoing schedule of " + i.CustomStartDate.Value.ToShortDateString() + ". Please make sure that house account appears only once per tier in schedule.";
                                return result;
                            }

                            //-check no two house accounts in one schedule of Tier 2
                            houseCount = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate) && x.TierNumber == 2).Count(x => x.PayeeUserCredentialId == house.UserCredentialID);
                            if (houseCount >= 2)
                            {
                                result = "House account is added more than once in Tier 2 in the outgoing schedule of " + i.CustomStartDate.Value.ToShortDateString() + ". Please make sure that house account appears only once per tier in schedule.";
                                return result;
                            }
                        }
                    }

                    //-check no two house accounts in one schedule
                    if (!IsTiered && sdates.Count > 0)
                    {
                        foreach (var i in sdates)
                        {
                            int houseCount = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate)).Count(x => x.PayeeUserCredentialId == house.UserCredentialID);
                            if (houseCount >= 2)
                            {
                                result = "House account is added more than once in the outgoing schedule of " + i.CustomStartDate.Value.ToShortDateString() + ". Please make sure that house account appears only once in schedule.";
                                return result;
                            }
                        }
                    }

                    //Check that schedule must include origin effective date
                    if (SelectedPolicy.OriginalEffectiveDate != null)
                    {
                        bool isEffectiveDateIncluded = false;
                        foreach (var i in sdates)
                        {
                            if (i.CustomStartDate < SelectedPolicy.OriginalEffectiveDate) // if any date is less or equal eff date, then eff date included in schedule
                            {
                                //isEffectiveDateIncluded = true;
                                //break;
                                result = "Outgoing schedule cannot start before effective date of the policy. Please check " + Convert.ToDateTime(i.CustomStartDate).ToShortDateString();
                                return result;
                            }
                            else if (i.CustomStartDate == SelectedPolicy.OriginalEffectiveDate)
                            {
                                isEffectiveDateIncluded = true;
                            }
                        }

                        if (!isEffectiveDateIncluded)
                        {
                            result = "Outgoing schedule must start from effective date of the policy.";
                            return result;
                        }
                    }


                    //100% when % of commission
                    if (OutPercentOfCommission)
                    {
                        foreach (var i in sdates)
                        {
                            double split = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                            isTier2Present = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate && x.TierNumber == 2)).ToList().Count > 0;
                            if (IsTiered)
                            {
                                split = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate) &&
                                ((isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1))).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                            }
                            if (split != 100)
                            {
                                result = "Split percentage not equal to 100 for the schedule starting from " + i.CustomStartDate.Value.ToShortDateString();
                                //PolicyDetailSaveToolTip = "FirstYear/Renewal not equal to 100.";
                                ////Changedcolor = true;
                                ////Savecontent = "! Save";
                                ////imagePath = null;
                                return result;
                            }
                        }
                    }
                    else if (OutPercentOfPremium) //equal incoming when % of premium
                    {
                        foreach (var i in sdates)
                        {
                            double split = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                            isTier2Present = OutGoingField.Where(x => (x.CustomStartDate == i.CustomStartDate && x.TierNumber == 2)).ToList().Count > 0;
                            if (IsTiered)
                            {
                                split = OutGoingField.Where(x => x.CustomStartDate == i.CustomStartDate &&
                                ((isTier2Present && x.TierNumber == 2) || (!isTier2Present && x.TierNumber == 1))).Sum<OutGoingPayment>(p => p.SplitPercent ?? 0);
                            }

                            //logic to include custom incoming schedule
                            double? expectedfirst = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
                            double? expectedRenewal = SelectedPolicyToolIncommingShedule.RenewalPercentage;

                            //Check if custom mode, then 
                            if (SelectedPolicyToolIncommingShedule.Mode == Mode.Custom && SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded)
                            {

                                expectedfirst = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == 1).FirstOrDefault().Percent;
                                int year = getPolicyAgeFromEffective(i.CustomStartDate.Value);

                                int maxYear = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderByDescending(x => x.Year).FirstOrDefault().Year;
                                year = (year > maxYear) ? maxYear : year;
                                expectedRenewal = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderBy(x => x.Year).Where(x => x.Year == year).FirstOrDefault().Percent;

                                //added - july 27, 2020 after analysis during BG/caravus integration
                                if (year == 1) // if policy is in year 1 , then use next higher year's % as renewal%
                                {
                                    var year2Record = SelectedPolicyToolIncommingShedule.NonGradedSchedule.OrderBy(x => x.Year).Where(x => x.Year > 1).FirstOrDefault();
                                    if (year2Record != null) //This also handles the case where only 1 year is defined in non-graded schedule .
                                    {
                                        expectedRenewal = year2Record.Percent;
                                    }
                                }

                                //if (expectedRenewal == null || expectedRenewal == 0)
                                //{
                                //    expectedRenewal = SelectedPolicyToolIncommingShedule.NonGradedSchedule.Where(x => x.Year == maxYear).FirstOrDefault().Percent;
                                //}

                            }

                            //to confirm from kevin
                            bool isFirstYear = (i.CustomStartDate == null) ? true : IsUseFirstYear(i.CustomStartDate.Value);
                            if ((isFirstYear && split != expectedfirst) ||
                                 (!isFirstYear && split != expectedRenewal))
                            {
                                result = "Incoming FirstYear/Renewal not equal to Outgoing Split percentage for the schedule starting from " + i.CustomStartDate.Value.ToShortDateString();
                                //Changedcolor = true;
                                //Savecontent = "! Save";
                                //imagePath = null;
                                return result;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    objLog.AddLog("exception validating save policy: " + ex.Message);
                }
            }


            return result;
        }


        int getPolicyAgeFromEffective(DateTime secondDate)
        {
            int year = 1;
            try
            {
                if (SelectedPolicy.OriginalEffectiveDate != null) //if null, always assumed first year.
                {
                    year = (int)Math.Ceiling((secondDate - Convert.ToDateTime(SelectedPolicy.OriginalEffectiveDate)).TotalDays / 365.25D);
                    year = (year <= 0) ? 1 : year;
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("getPolicyAgeFromEffective Exception: " + ex.Message);
            }
            return year;

        }

        /// <summary>
        /// Acme - to find end date 
        /// </summary>
        void AddEndDatesInOutgoingSchedule()
        {
            if (IsCustomDateSelected)
            {
                //OutGoingField.Count
                var sdates = (from s in OutGoingField
                              select new { s.CustomStartDate }).Distinct().OrderBy(x => x.CustomStartDate).ToList();

                for (int i = 0; i < sdates.Count - 1; i++)
                {
                    DateTime sDate = Convert.ToDateTime(sdates[i].CustomStartDate);
                    DateTime eDate = Convert.ToDateTime(sdates[i + 1].CustomStartDate).AddDays(-1);
                    var allEntries = OutGoingField.Where(c => c.CustomStartDate == sDate).ToList();
                    if (allEntries != null && allEntries.Count > 0)
                    {
                        allEntries.ForEach(cc => cc.CustomEndDate = eDate);
                    }
                }
            }
        }


        private void filterPolicy()
        {
            bool isPendingPolicyExist = false;
            bool isActivePolicyExist = false;
            bool isTerminatedPolicyExist = false;
            if (PolicyList != null)
            {
                PolicyList = new ObservableCollection<PolicyDetailsData>(PolicyList.Where(p => p.IsDeleted == false));
                isPendingPolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 2);
                isActivePolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 0);
                isTerminatedPolicyExist = PolicyList.ToList().Exists(s => s.PolicyStatusId == 1);
            }

            if (isPendingPolicyExist && isActivePolicyExist)
                IsAllCheck = true;

            else if (isActivePolicyExist && isTerminatedPolicyExist)
                IsAllCheck = true;

            else if (isPendingPolicyExist && isTerminatedPolicyExist)
                IsAllCheck = true;

            else if (isActivePolicyExist && isPendingPolicyExist && isTerminatedPolicyExist)
                IsAllCheck = true;

            else
            {
                if (isActivePolicyExist)
                {
                    IsActiceChecked = true;
                }
                else if (isPendingPolicyExist)
                {
                    IsPendingChecked = true;
                }
                else if (isTerminatedPolicyExist)
                {
                    IsTerminatedCheck = true;
                }
                else
                {
                    IsAllCheck = true;
                }
            }
        }

        private void ThreadStartingPoint()
        {
            // WPF automatically creates a new Dispatcher to manage our new thread. All we have to 
            // do to make our window functional is ask this Dispatcher to start running
            SaveIncomingSchedule();

            if (OutGoingField != null && OutGoingField.Count != 0)
            {
                {
                    if (IsCustomDateSelected)
                    {
                        AddEndDatesInOutgoingSchedule();
                        //serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected,IsTiered);
                    }
                    //else if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2) && (OutGoingField.ToList().Sum(p => p.FirstYearPercentage) == 100) && (OutGoingField.ToList().Sum(p => p.RenewalPercentage) == 100) ||
                    //             OutGoingField.FirstOrDefault().ScheduleTypeId == 1
                    //            )
                    //else
                    //{
                    serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected, IsTiered);
                    //}
                    /* else if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2) && (OutGoingField.ToList().Sum(p => p.FirstYearPercentage) == 100) && (OutGoingField.ToList().Sum(p => p.RenewalPercentage) == 100))
                     {
                         serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected);
                     }
                     else if (OutGoingField.FirstOrDefault().ScheduleTypeId == 1)
                     {
                         serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField, IsCustomDateSelected);
                     }*/
                }
            }
            /// 12-17-2019 :Comment because when we update the schedule and change the policy suddenly then outgoingSchedule not deleted
            //if (_deleteseletedOutGoingField.Count != 0)
            //{
            //    foreach (OutGoingPayment paymen in _deleteseletedOutGoingField)
            //    {
            //        //[Added By Ankit]
            //        //[Aug06,2018-We can add this check for checking the policyId of selected policy   is equal to deleteSelectedOutgoingField policyId  if not then don't deleted the outgoingschedules]
            //        if (SelectedPolicy.PolicyId == paymen.PolicyId)
            //        {
            //            serviceClients.OutGoingPaymentClient.DeleteOutgoingPayment(paymen);
            //        }
            //        else
            //        {
            //            ActionLogger.Logger.WriteLog(DateTime.Now + "-ThreadStartingPoint, Selected policyId is not equals to OutgoingScheduledpolicyId " + "SelectedPolicyId:" + SelectedPolicy.PolicyId + "" + "OutgoingScheduledpolicyId:" + paymen.PolicyId + "" + "OutgoingScheduledId:" + paymen.OutgoingScheduleId, true);
            //        }
            //    }
            //    _deleteseletedOutGoingField.Clear();
            //}

            if (SelectedPolicySaved != null)
            {
                isAvoideTocall = true;
                SelectedPolicySaved();
                isAvoideTocall = false;
            }

            UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.PolicyDetail);
            isAvoideTocall = false;
            strLableContent = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelectedPolicy.PolicyId));

            filterPolicy();

            System.Windows.Threading.Dispatcher.Run();
        }

        void FollowUP_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //IsBusy = false;
            //MessageBox.Show("Completed");
            VMInstances.PolicyCommissionVM.FillFollowUpIssue();
        }

        private void ThreadFollowUpStartingPoint()
        {
            //VMInstances.PolicyCommissionVM.IsBusy = true;
            //Acme    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
            ////VMInstances.PolicyCommissionVM.FillFollowUpIssue();
            System.Windows.Threading.Dispatcher.Run();
        }

        void FollowUP_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                VMInstances.PolicyCommissionVM.IsBusy = true;
                //acme commenet                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
            }
            catch
            {
            }
        }

        private DateTime? GetGreaterDate(DateTime? date1, DateTime? date2)
        {
            if (date1 != null && date2 != null)
            {
                return DateTime.Compare(date1.Value, date2.Value) >= 0 ? date1 : date2;
            }
            else if (date1 == null)
            {
                return date2;
            }
            else if (date2 == null)
            {
                return date1;
            }
            return null;
        }

        //void SaveIncomingSchedule()
        //{
        //    try
        //    {
        //        if (IncPercentOfPremium == true || IncPerHead == true)
        //        {
        //            if (SelectedPolicyToolIncommingShedule == null)
        //            {
        //                SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
        //            }
        //        }

        //        if (SelectedPolicyToolIncommingShedule != null)
        //        {
        //            if (SelectedPolicyToolIncommingShedule.FirstYearPercentage != null || SelectedPolicyToolIncommingShedule.RenewalPercentage != null)
        //            {
        //                if (SelectedPolicyToolIncommingShedule.IncomingScheduleID == Guid.Empty)
        //                {
        //                    SelectedPolicyToolIncommingShedule.IncomingScheduleID = Guid.NewGuid();
        //                }
        //                serviceClients.PolicyIncomingScheduleClient.AddUpdatePolicyToolIncommingShedule(SelectedPolicyToolIncommingShedule);
        //                //serviceClients.PolicyIncomingScheduleClient.AddUpdatePolicyToolIncommingSheduleAsync(SelectedPolicyToolIncommingShedule);
        //                if (!FollowUpcalled)
        //                {
        //                    //Thread thIncominng = new Thread(() =>
        //                    //{
        //                    //    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.IncomingScheduleChange, null,
        //                    //        SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, null);
        //                    //    FollowUpcalled = false;
        //                    //});

        //                    //thIncominng.Start();

        //                    //acme commented      serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.IncomingScheduleChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, null);
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}
        void SaveIncomingSchedule()
        {
            if (SelectedPolicyToolIncommingShedule.Mode == Mode.Custom)
            {
                SelectedPolicyToolIncommingShedule.FirstYearPercentage = 0;
                SelectedPolicyToolIncommingShedule.RenewalPercentage = 0;
            }
            if (SelectedPolicyToolIncommingShedule.IncomingScheduleID == Guid.Empty)
            {
                SelectedPolicyToolIncommingShedule.IncomingScheduleID = Guid.NewGuid();
            }
            serviceClients.PolicyIncomingScheduleClient.SavePolicyIncomingSchedule(SelectedPolicyToolIncommingShedule);
        }

        public bool isClickOnAddPolicy = false;

        private void AddPolicyWithDefaultSetting()
        {
            try
            {
                isClickOnAddPolicy = true;
                IsActiceChecked = true;
                policyStatus = _PolicyStatus.Active;
                PolicyDetailsData policy = new PolicyDetailsData()
                {
                    IsManuallyChanged = false,
                    PolicyId = Guid.NewGuid(),
                    PolicyStatusId = 0,
                    PolicyType = "New",
                    ClientId = SelectedDisplayClient.ClientId,
                    Insured = SelectedDisplayClient.Name,
                    IsTrackPayment = true,
                    PolicyModeId = 0,
                    PolicyLicenseeId = SharedVMData.SelectedLicensee.LicenseeId,
                    IncomingPaymentTypeId = 1,
                    TerminationReasonId = null,
                    SplitPercentage = 100,
                    ModeAvgPremium = 0,
                    IsTrackIncomingPercentage = true,
                    IsTrackMissingMonth = true,
                    CreatedOn = DateTime.Today,
                    IsIncomingBasicSchedule = true,
                    IsOutGoingBasicSchedule = true,
                    IsSavedPolicy = false,
                    RenewalPercentage = "0",
                    PolicyIncomingPayType = Convert.ToString(SelectedMasterIncomingPaymentType.PaymenProcedureName),
                    IsCustomBasicSchedule = false,
                    CustomDateType = "Invoice",  //to keep default selection,  if custom schedule selected 

                };

                if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
                {
                    policy.CreatedBy = RoleManager.userCredentialID;
                }
                else
                {
                    policy.CreatedBy = serviceClients.PostUtilClient.GetPolicyHouseOwner(SharedVMData.SelectedLicensee.LicenseeId);
                }

                if (PolicyList == null) PolicyList = new ObservableCollection<PolicyDetailsData>();

                AddPolicyToList(policy);
                //OnStatusChanged(CurrentPolicyStatus);
                SelectedPolicy = policy;
                UpdateUIByAddNewPolicy(SelectedPolicy);
                ReplaceRbStatus = true; NewRbStatus = true;
                SelectedPageIndex = 0;
                //if add new policy then not any selected product
                if (SelectedPolicy != null)
                {
                    if (AllProducts.Count > 0)
                    {
                        //SelectedPolicy.Coverage = AllProducts.LastOrDefault();
                        //Adding Avoid to crash when seleted policy not having any coverage
                        if (SelectedPolicy.Coverage != null)
                        {
                            if (SelectedPolicy.Coverage.Name != "")
                            {
                                //Set product name
                                DisplayedCoverage emptyCoverage = new DisplayedCoverage { CoverageID = Guid.Empty, Name = string.Empty };
                                AllProducts.Add(emptyCoverage);
                                SelectedPolicy.Coverage = AllProducts.LastOrDefault();
                                //Set empty Product Type
                                AllCoverageNickName = new ObservableCollection<CoverageNickName>();
                                CoverageNickName emptyCoverageNickName = new CoverageNickName { CoverageID = Guid.Empty, PayorID = Guid.Empty, CarrierID = Guid.Empty, NickName = string.Empty, IsDeleted = false };
                                SelectedCoverageNickName = AllCoverageNickName.FirstOrDefault();
                            }
                        }
                        else
                        {    //When product is empty then
                            //Set empty Product Type
                            AllCoverageNickName = new ObservableCollection<CoverageNickName>();
                            CoverageNickName emptyCoverageNickName = new CoverageNickName { CoverageID = Guid.Empty, PayorID = Guid.Empty, CarrierID = Guid.Empty, NickName = string.Empty, IsDeleted = false };
                            SelectedCoverageNickName = AllCoverageNickName.FirstOrDefault();
                        }
                    }
                    //Clear fields
                    ClearCollection();
                }


            }
            catch
            {
            }
        }

        private void AddPolicyToList(PolicyDetailsData policy)
        {
            try
            {
                if (PolicyList != SelectedClientPolicyList.PolicyList)
                {
                    PolicyList.Add(policy);
                    SelectedClientPolicyList.PolicyList.Add(policy);
                }
                else
                {
                    PolicyList.Add(policy);
                }
            }
            catch
            {
            }
        }

        private void RemovePolicyFromList(PolicyDetailsData policy)
        {
            try
            {
                if (PolicyList != SelectedClientPolicyList.PolicyList)
                {
                    PolicyList.Remove(policy);
                    SelectedClientPolicyList.PolicyList.Remove(policy);
                }
                else
                {
                    PolicyList.Remove(policy);
                }
            }
            catch
            {
            }
        }

        private void DeletePolicy()
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;

                if (SelectedPolicy.IsDeleted == true) return;
                if (_PolicyServiceClient.PolicyClient.CheckForPolicyPaymentExists(SelectedPolicy.PolicyId))
                {
                    MessageBox.Show("Policy can not be deleted(payment is attach with policy)", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBoxResult result = MessageBox.Show("Do you want to remove selected policy?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    //Count the policy if there single policy into the client then ask Question to delete the client

                    int intCountPolicy = PolicyList.Count(s => s.ClientId == selectedClient.ClientId);

                    if (intCountPolicy == 1)
                    {
                        string logMsg = "Manual Delete Policy request , policyCount is 1 ,SelectedPolicy : " + SelectedPolicy.PolicyId + ",  loggedInUser: " + RoleManager.LoggedInUser;
                        //ActionLogger.Logger.WriteLog(logMsg, true);

                        objLog.AddLog(logMsg);


                        #region"delete policy"
                        //delete policy
                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)_PolicyServiceClient.PolicyClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            _PolicyServiceClient.PolicyClient.DeletePolicy(SelectedPolicy);
                        }
                        //delete from History---
                        _PolicyServiceClient.PolicyClient.DeletePolicyHistory(SelectedPolicy);

                        //Set first year and renewal pecetange;
                        selectedpolicyincomingschedule.FirstYearPercentage = null;
                        selectedpolicyincomingschedule.RenewalPercentage = null;

                        //Clear the Collection
                        VMInstances.PolicyCommissionVM.PolicyIncomingPaymentCommissionDashBoard.Clear();
                        VMInstances.PolicyCommissionVM.CommissionDashBoardOutGoingPaymentLst.Clear();
                        VMInstances.PolicyCommissionVM.PolicyFollowUpCommissionDashBoardLst.Clear();

                        //Clear the PAC 
                        strLableContent = string.Empty;
                        OutGoingField.Clear();

                        //remove from the collection
                        RemovePolicyFromList(SelectedPolicy);

                        #endregion

                        MessageBoxResult dialogresult = MessageBox.Show("Policy has been deleted. " + Environment.NewLine + "Do you want to delete client?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (dialogresult == MessageBoxResult.Yes)
                        {
                            #region"delete Client"
                            //Delete the selected client  
                            if (SelectedDisplayClient.ClientId == Guid.Empty) return;
                            if (SelectedDisplayClient == null) return;
                            //for get the selected index  

                            logMsg = "Client delete chosen after policy delete, SelectedDisplayClient : " + SelectedDisplayClient.Name + ",  loggedInUser: " + RoleManager.LoggedInUser;
                            objLog.AddLog(logMsg);

                            int x = DisplayedClientsLists.IndexOf(selectedDisplayClient);

                            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.ClientClient.InnerChannel))
                            {
                                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                                serviceClients.ClientClient.DeleteClients(SelectedDisplayClient);
                            }
                            DisplayedClientsLists.Remove(SelectedDisplayClient);

                            SelectedDisplayClient = DisplayedClientsLists.ElementAtOrDefault(x);
                            if (SelectedDisplayClient.ClientId == Guid.Empty)
                            {
                                SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                            }
                            SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                            #endregion
                        }
                        //Only delete the policy not to client
                        else
                        {  //delete policy
                            //_PolicyServiceClient.PolicyClient.DeletePolicy(SelectedPolicy);
                            ////delete from History---
                            //_PolicyServiceClient.PolicyClient.DeletePolicyHistory(SelectedPolicy);
                            ////remove from the collection
                            //RemovePolicyFromList(SelectedPolicy);
                            SelectedPolicy = PolicyList.FirstOrDefault();
                            //Filter the policy 
                            filterPolicy();
                        }
                    }
                    else //More then one policy so not requred to delete the policy
                    {
                        string logMsg = "Manual Delete Policy request , policyCount > 1 ,SelectedPolicy : " + SelectedPolicy.PolicyId + ",  loggedInUser: " + RoleManager.LoggedInUser;
                        objLog.AddLog(logMsg);
                        //delete policy
                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)_PolicyServiceClient.PolicyClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            _PolicyServiceClient.PolicyClient.DeletePolicy(SelectedPolicy);
                        }
                        // _PolicyServiceClient.PolicyClient.DeletePolicy(SelectedPolicy);
                        //delete from History---
                        _PolicyServiceClient.PolicyClient.DeletePolicyHistory(SelectedPolicy);
                        //remove from the collection
                        RemovePolicyFromList(SelectedPolicy);
                        SelectedPolicy = PolicyList.FirstOrDefault();
                        //Filter the policy 
                        filterPolicy();
                    }

                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("delete policy exception: " + ex.Message);
            }
        }

        public Client PreviousClientSelected { get; set; }

        private void CreateClient()
        {
            try
            {
                //Add code for validation
                PreviousClientSelected = SelectedDisplayClient;
                SelectedClient = new Client();
                SharedVMData.UpdateMode = UpdateMode.Add;

                Client _Client = VMPolicyClient.Show();

                if (_Client.ClientId != Guid.Empty)
                {
                    ClientWisePolicyCollection clientPolicyList = new ClientWisePolicyCollection();
                    clientPolicyList.ClientId = _Client.ClientId;
                    if (SharedVMData.CachedClientList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        SharedVMData.CachedClientList[SharedVMData.SelectedLicensee.LicenseeId].Add(_Client);
                    }
                    if (ClientWisePolicyList == null)
                    {
                        ClientWisePolicyList = new ObservableCollection<ClientWisePolicyCollection>();
                    }
                    ClientWisePolicyList.Add(clientPolicyList);
                    SelectedClientPolicyList = clientPolicyList;

                    PolicyList = SelectedClientPolicyList.PolicyList;

                    if (DisplayedClientsLists.Count == 0)
                        DisplayedClientsLists.Insert(0, _Client);


                    SelectedDisplayClient = DisplayedClientsLists.Where(p => p.ClientId == _Client.ClientId).FirstOrDefault();
                    SelectedClient = SelectedDisplayClient;

                    //Load refresh added client
                    DisplayedClientsLists = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);
                    if (DisplayedClientsLists.Count > 0)
                    {
                        SelectedDisplayClient = DisplayedClientsLists.Where(p => p.ClientId == _Client.ClientId).FirstOrDefault();
                        SelectedClient = SelectedDisplayClient;

                        //Update smart fields
                        VMPolicySmartField.DisplayedClientsLists = DisplayedClientsLists;
                        VMPolicySmartField.SelectedClientLrnd = SelectedClient;
                    }
                }
                else
                {
                    SelectedClient.ClientId = PreviousClientSelected.ClientId;
                }
                SharedVMData.UpdateMode = UpdateMode.None;

            }
            catch
            {
            }
        }

        private void EditClientDetail()
        {
            try
            {
                SharedVMData.UpdateMode = UpdateMode.Edit;
                VMPolicyClient.PreviousClientDataBeforeEdit = new Client();

                VMPolicyClient.PreviousClientDataBeforeEdit.ClientId = SelectedDisplayClient.ClientId;
                VMPolicyClient.PreviousClientDataBeforeEdit.LicenseeId = SelectedDisplayClient.LicenseeId;
                VMPolicyClient.PreviousClientDataBeforeEdit.Name = SelectedDisplayClient.Name;
                VMPolicyClient.PreviousClientDataBeforeEdit.InsuredName = SelectedDisplayClient.InsuredName;
                VMPolicyClient.PreviousClientDataBeforeEdit.Address = SelectedDisplayClient.Address;
                VMPolicyClient.PreviousClientDataBeforeEdit.Zip = SelectedDisplayClient.Zip;
                VMPolicyClient.PreviousClientDataBeforeEdit.City = SelectedDisplayClient.City;
                VMPolicyClient.PreviousClientDataBeforeEdit.State = SelectedDisplayClient.State;
                VMPolicyClient.PreviousClientDataBeforeEdit.Email = SelectedDisplayClient.Email;
                VMPolicyClient.PreviousClientDataBeforeEdit.IsDeleted = SelectedDisplayClient.IsDeleted;
                VMPolicyClient.PreviousClientDataBeforeEdit.LogInUserId = SelectedDisplayClient.LogInUserId;

                Client _Client = VMPolicyClient.Show();
                //add by neha
                DisplayedClientsLists.Remove(SelectedDisplayClient);
                DisplayedClientsLists = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);
                SelectedDisplayClient = DisplayedClientsLists.Where(p => p.ClientId == _Client.ClientId).FirstOrDefault();
                //close
                VMPolicyClient.PreviousClientDataBeforeEdit = null;
                SharedVMData.UpdateMode = UpdateMode.None;
            }
            catch
            {
            }
        }

        private void DeleteClient()
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                if (SelectedDisplayClient.ClientId == Guid.Empty) return;
                if (SelectedDisplayClient == null) return;
                //for get the selected index
                //add by neha
                int x = DisplayedClientsLists.IndexOf(selectedDisplayClient);
                if (serviceClients.ClientClient.CheckClientPolicyIssueExists(SelectedDisplayClient.ClientId))
                {
                    MessageBox.Show("Client can not be deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (MessageBox.Show("Do you want to delete Client " + SelectedDisplayClient.Name + " ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
                string logMsg = "Manually Deleted Client request - SelectedDisplayClient : " + SelectedDisplayClient.Name + ",  loggedInUser: " + RoleManager.LoggedInUser;
                objLog.AddLog(logMsg);

                using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.ClientClient.InnerChannel))
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                    serviceClients.ClientClient.DeleteClients(SelectedDisplayClient);
                }

                DisplayedClientsLists.Remove(SelectedDisplayClient);
                //add by neha
                SelectedDisplayClient = DisplayedClientsLists.ElementAtOrDefault(x);
                if (SelectedDisplayClient.ClientId == Guid.Empty)
                {
                    SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                }
                SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
            }
            catch (Exception ex)
            {
                objLog.AddLog("Exception DeleteClient: " + ex.Message);
            }
        }

        #endregion

        #region PolicySettingUpdate

        private ICommand _ClickPrimaryAgent;
        public ICommand ClickPrimaryAgent
        {
            get
            {
                if (_ClickPrimaryAgent == null)
                {
                    _ClickPrimaryAgent = new BaseCommand(x => ClickPrimaryAgentCmd());
                }
                return _ClickPrimaryAgent;
            }
        }

        private void ClickPrimaryAgentCmd()
        {

        }

        private ICommand _updatepolicySetting;
        public ICommand AddPolicySetting
        {
            get
            {
                if (_updatepolicySetting == null)
                {
                    _updatepolicySetting = new BaseCommand(x => BeforeUpdatePolicySetting(), x => UpdatePolicySetting());
                }
                return _updatepolicySetting;
            }
        }

        private bool BeforeUpdatePolicySetting()
        {
            if (SelectedPolicy == null)
                return false;
            //check if selected policy saved or not
            if (SelectedPolicy.IsSavedPolicy == false)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        public void UpdatePolicySetting()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return;

            _PolicyServiceClient.PolicyClient.UpdatePolicySetting(SelectedPolicy);
        }
        #endregion

        public void UpdateClient(Guid ClientId)
        {
            Refresh();
        }

        public void UpDatePolicyDetailSmartFiled(Guid? PolicyId, UpdatedModule _UpdatedModule)
        {
            try
            {
                if (PolicyId == null) return;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                if (!parameters.ContainsKey("PolicyId"))
                    parameters.Add("PolicyId", PolicyId);

                if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                {
                    if (!parameters.ContainsKey("CreatedBy"))
                        parameters.Add("CreatedBy", RoleManager.userCredentialID);
                }

                PolicyDetailsData _policy = _PolicyServiceClient.PolicyClient.GetPolicydata(parameters).FirstOrDefault();

                if (UpdatedModule.PolicyDetail != _UpdatedModule && SelectedPolicy != null && PolicyId.Value == SelectedPolicy.PolicyId)
                {
                    if (SelectedPolicy.PolicyPreviousData != null) //Acme -added null check to avoid crash
                    {
                        SelectedPolicy.PolicyPreviousData.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                        SelectedPolicy.PolicyPreviousData.PolicyModeId = SelectedPolicy.PolicyModeId;
                        SelectedPolicy.PolicyPreviousData.TrackFromDate = SelectedPolicy.TrackFromDate;
                        SelectedPolicy.PolicyPreviousData.PolicyTermdateDate = SelectedPolicy.PolicyTerminationDate;
                        
                    }

                    SelectedPolicy.PolicyId = _policy.PolicyId;
                    SelectedPolicy.PolicyNumber = _policy.PolicyNumber;
                    SelectedPolicy.PolicyStatusId = _policy.PolicyStatusId;
                    SelectedPolicy.PolicyStatusName = _policy.PolicyStatusName;
                    SelectedPolicy.PolicyType = _policy.PolicyType;
                    SelectedPolicy.PolicyLicenseeId = _policy.PolicyLicenseeId;
                    SelectedPolicy.Insured = _policy.Insured;
                    SelectedPolicy.OriginalEffectiveDate = _policy.OriginalEffectiveDate;
                    SelectedPolicy.TrackFromDate = _policy.TrackFromDate;
                    SelectedPolicy.PolicyModeId = _policy.PolicyModeId;
                    SelectedPolicy.ModeAvgPremium = _policy.ModeAvgPremium;
                    SelectedPolicy.SubmittedThrough = _policy.SubmittedThrough;
                    SelectedPolicy.Enrolled = _policy.Enrolled;
                    SelectedPolicy.Eligible = _policy.Eligible;
                    SelectedPolicy.PolicyTerminationDate = _policy.PolicyTerminationDate;
                    SelectedPolicy.TerminationReasonId = _policy.TerminationReasonId;
                    SelectedPolicy.IsTrackMissingMonth = _policy.IsTrackMissingMonth;
                    SelectedPolicy.IsTrackIncomingPercentage = _policy.IsTrackIncomingPercentage;
                    SelectedPolicy.IsTrackPayment = _policy.IsTrackPayment;
                    SelectedPolicy.IsDeleted = _policy.IsDeleted;
                    SelectedPolicy.CarrierID = _policy.CarrierID;
                    SelectedPolicy.CarrierName = _policy.CarrierName;
                    SelectedPolicy.CoverageId = _policy.CoverageId;
                    SelectedPolicy.CoverageName = _policy.CoverageName;
                    SelectedPolicy.ClientId = _policy.ClientId;
                    SelectedPolicy.ClientName = _policy.ClientName;
                    SelectedPolicy.ReplacedBy = _policy.ReplacedBy;
                    SelectedPolicy.DuplicateFrom = _policy.DuplicateFrom;
                    SelectedPolicy.IsIncomingBasicSchedule = _policy.IsIncomingBasicSchedule;
                    SelectedPolicy.IsOutGoingBasicSchedule = _policy.IsOutGoingBasicSchedule;
                    SelectedPolicy.PayorId = _policy.PayorId;
                    SelectedPolicy.PayorName = _policy.PayorName;
                    SelectedPolicy.SplitPercentage = _policy.SplitPercentage;

                    SelectedPolicy.IncomingPaymentTypeId = _policy.IncomingPaymentTypeId;
                    SelectedPolicy.CreatedOn = _policy.CreatedOn;
                    SelectedPolicy.CreatedBy = _policy.CreatedBy;//--always check it will never null
                    SelectedPolicy.IsSavedPolicy = _policy.IsSavedPolicy;
                    SelectedPolicy.LastModifiedBy = _policy.LastModifiedBy;
                    SelectedPolicy.LastModifiedOn = _policy.LastModifiedOn;

                }
                if (UpdatedModule.SmartField != _UpdatedModule)
                {
                    if (PolicyId.Value == SelectedPolicy.PolicyId)
                        VMInstances.PolicySmartFieldVM.OptimizedPolicyManager_SelectedPolicyChanged(_policy);
                }
                if (UpdatedModule.CommissionDashBoard != _UpdatedModule)
                {
                    if (PolicyId.Value == SelectedPolicy.PolicyId)
                        VMInstances.PolicyCommissionVM.OptimizedPolicyManager_SelectedPolicyChanged(_policy);
                }
            }
            catch
            {
            }
        }


        #region  Custom Schedule variable intalization and methods
        void DeleteCustomRow()
        {
            MessageBoxResult _result = MessageBox.Show("Do you want to delete selected record?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            if (_result == MessageBoxResult.OK)
            {
                if (IsGraded)
                {
                    GradedList.Remove(SelectedGraded);

                    SelectedGraded = GradedList[0];
                }
                else
                {
                    NonGradedList.Remove(SelectedNonGraded);
                    SelectedNonGraded = NonGradedList[0];
                }
            }
        }

        bool BeforeDeleteCustomRow()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else if ((IsGraded && (GradedList == null || GradedList.Count == 1)) ||
                    (IsNonGraded && (NonGradedList == null || NonGradedList.Count == 1)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        void NewCustomRecord()
        {
            if (GradedList == null && SelectedPolicyToolIncommingShedule.CustomType == CustomMode.Graded)
            {
                GradedList = new ObservableCollection<Graded>();
                Graded obj = new Graded();
                obj.From = 0;
                obj.To = 0;
                obj.Percent = 0;
                GradedList.Add(obj);
                if (SelectedPolicyToolIncommingShedule != null)
                {
                    SelectedPolicyToolIncommingShedule.GradedSchedule = new ObservableCollection<Graded>(GradedList);
                }
            }

            if (NonGradedList == null && SelectedPolicyToolIncommingShedule.CustomType == CustomMode.NonGraded)
            {
                NonGradedList = new ObservableCollection<NonGraded>();
                NonGraded objNonGraded = new NonGraded();
                objNonGraded.Year = 0;
                objNonGraded.Percent = 0;
                NonGradedList.Add(objNonGraded);
                if (SelectedPolicyToolIncommingShedule != null)
                {
                    SelectedPolicyToolIncommingShedule.NonGradedSchedule = new ObservableCollection<NonGraded>(NonGradedList);
                }
            }

        }
        void AddNewCustomRow()
        {
            if (IsGraded)
            {
                if (GradedList == null)
                {
                    GradedList = new ObservableCollection<Graded>();
                }
                Graded obj = new Graded();
                obj.From = 0;
                obj.To = 0;
                obj.Percent = 0;
                GradedList.Add(obj);
                if (SelectedPolicyToolIncommingShedule != null)
                {
                    SelectedPolicyToolIncommingShedule.GradedSchedule = new ObservableCollection<Graded>(GradedList);
                }
            }
            else
            {
                if (NonGradedList == null)
                {
                    NonGradedList = new ObservableCollection<NonGraded>();
                }
                NonGraded objNonGraded = new NonGraded();
                objNonGraded.Year = 0;
                objNonGraded.Percent = 0;
                NonGradedList.Add(objNonGraded);
                if (SelectedPolicyToolIncommingShedule != null)
                {
                    SelectedPolicyToolIncommingShedule.NonGradedSchedule = new ObservableCollection<NonGraded>(NonGradedList);
                }
            }

        }

        public void SaveCustomSchedules()
        {

            ValidationMessage = Common.ValidateIncomingSchedule(GradedList, NonGradedList, IsGraded, selectedIncomingMode, SelectedPolicyToolIncommingShedule.ScheduleTypeId);

            if (ValidationMessage == "")
            {
                SavedCustomSelectedSchedule.Copy(SelectedPolicyToolIncommingShedule);
                IsValidationShown = false;
                CloseCustomScheduleEvent();

            }
            else
            {
                IsValidationShown = true;
            }

        }
        #endregion
    }

    public class PolicyMetaData
    {
        public Guid? LicenseeId { get; set; }
        public List<Guid> ClientIds { get; set; }
        public List<int> ClientsPolicyCount { get; set; }
        public _PolicyStatus PolicyStatus { get; set; }
        public Guid? SelectedDisplayedClientId { get; set; }
    }

    public class ClientWisePolicyCollection
    {
        public ClientWisePolicyCollection()
        {
            this.PolicyList = new ObservableCollection<PolicyDetailsData>();
        }

        public Guid? ClientId { get; set; }
        public ObservableCollection<PolicyDetailsData> PolicyList { get; set; }
    }

    public class LastViewPolicyClientCollection : INotifyPropertyChanged
    {
        private const int TotalViewCount = 10;
        private bool ChangeDisplayedClient = true;
        private static LastViewPolicyClientCollection FirstLastViewedClients = null;

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
        public delegate void LastViewedClientChangedEventHandler(Guid? ClinetId);
        public event LastViewedClientChangedEventHandler LastViewedClientChanged;

        private ObservableCollection<LastViewPolicy> _LastViewClients;
        public ObservableCollection<LastViewPolicy> LastViewClients
        {
            get
            {
                return _LastViewClients;
            }
            set
            {
                _LastViewClients = value;
                OnPropertyChanged("LastViewClients");
            }
        }

        private LastViewPolicy _SelectedLastViewClient;
        public LastViewPolicy SelectedLastViewClient
        {
            get
            {
                return _SelectedLastViewClient;
            }
            set
            {
                if (value != null && _SelectedLastViewClient != value)
                {
                    _SelectedLastViewClient = value;
                    OnPropertyChanged("SelectedLastViewClient");

                    if (ChangeDisplayedClient && LastViewedClientChanged != null && _SelectedLastViewClient != null)
                        LastViewedClientChanged(_SelectedLastViewClient.Clientid);
                }
            }
        }

        public static LastViewPolicyClientCollection LastViewedClients
        {
            get
            {
                return FirstLastViewedClients;
            }
        }

        public static LastViewPolicy getLastViewedPolicyForClient(Guid ClientId)
        {
            if (FirstLastViewedClients != null && FirstLastViewedClients.LastViewClients != null)
                return FirstLastViewedClients.LastViewClients.FirstOrDefault(s => s.Clientid == ClientId);
            else
                return null;
        }

        public LastViewPolicyClientCollection()
        {

            if (FirstLastViewedClients == null)
            {
                ChangeDisplayedClient = false;
                LastViewClients = serviceClients.LastViewPolicyClient.GetLastViewedClients(RoleManager.userCredentialID);

                if (LastViewClients != null)
                    SelectedLastViewClient = LastViewClients.FirstOrDefault();
                else
                    LastViewClients = new ObservableCollection<LastViewPolicy>();

                FirstLastViewedClients = this;
                ChangeDisplayedClient = true;
            }

        }
        public void Refresh()
        {
            try
            {

                if (LastViewedClients != null)
                {
                    ChangeDisplayedClient = false;

                    LastViewClients = serviceClients.LastViewPolicyClient.GetLastViewedClients(RoleManager.userCredentialID);

                    if (LastViewClients != null)
                        SelectedLastViewClient = LastViewClients.FirstOrDefault();
                    else
                        LastViewClients = new ObservableCollection<LastViewPolicy>();

                    ChangeDisplayedClient = true;


                }
            }
            catch
            {
            }

        }

        public void ClientOrPolicyChanged(Guid ClientId, string ClientName, Guid? PolicyId, bool UpdatePolicyDetail)
        {
            try
            {

                ChangeDisplayedClient = UpdatePolicyDetail;
                LastViewPolicy lastViewPolicy = LastViewClients.FirstOrDefault(s => s.Clientid == ClientId);
                if (lastViewPolicy != null)
                {
                    LastViewClients.Remove(lastViewPolicy);
                    lastViewPolicy.PolicyId = PolicyId;
                    LastViewClients.Insert(0, lastViewPolicy);
                }
                else
                {
                    lastViewPolicy = new LastViewPolicy { Clientid = ClientId, ClientName = ClientName, PolicyId = PolicyId };
                    LastViewClients.Insert(0, lastViewPolicy);

                    if (LastViewClients.Count > TotalViewCount)
                        LastViewClients = new ObservableCollection<LastViewPolicy>(LastViewClients.Take(TotalViewCount));
                }

                SelectedLastViewClient = LastViewClients.Where(p => p.Clientid == ClientId).FirstOrDefault();

                if (SelectedLastViewClient == null)
                    SelectedLastViewClient = LastViewClients.FirstOrDefault();

                SelectedLastViewClient = LastViewClients.FirstOrDefault();
                serviceClients.LastViewPolicyClient.SaveLastViewedClientsAsync(LastViewClients, RoleManager.userCredentialID);
                ChangeDisplayedClient = true;

            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }
    }




}

























