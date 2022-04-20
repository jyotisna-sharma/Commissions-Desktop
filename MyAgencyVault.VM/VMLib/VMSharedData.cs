using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.ViewModel
{
    public class VMSharedData : BaseViewModel
    {
        private ObservableCollection<LicenseeDisplayData> _Licensees;
        public ObservableCollection<LicenseeDisplayData> Licensees
        {
            get { return _Licensees; }
            set
            {
                _Licensees = value;
                OnPropertyChanged("Licensees");
            }
        }

        private Dictionary<Guid, ObservableCollection<Client>> _cachedClientList;
        public Dictionary<Guid, ObservableCollection<Client>> CachedClientList
        {
            get
            {
                if (_cachedClientList == null)
                {
                    _cachedClientList = new Dictionary<Guid, ObservableCollection<Client>>();
                }
                return _cachedClientList;
            }
            set
            {
                _cachedClientList = value;
            }
        }

        private Dictionary<Guid, ObservableCollection<PolicyDetailsData>> _cachedPolicyList;
        public Dictionary<Guid, ObservableCollection<PolicyDetailsData>> CachedPolicyList
        {
            get
            {
                if (_cachedPolicyList == null)
                {
                    _cachedPolicyList = new Dictionary<Guid, ObservableCollection<PolicyDetailsData>>();
                }
                return _cachedPolicyList;
            }
            set
            {
                _cachedPolicyList = value;
            }
        }


        private bool _IsLoadIncomingPayment = false;
        public bool isLoadIncomingPayment
        {
            get { return _IsLoadIncomingPayment; }
            set { _IsLoadIncomingPayment = value; }
        }

        private Dictionary<Guid, ObservableCollection<User>> _cachedAgentList;
        public Dictionary<Guid, ObservableCollection<User>> CachedAgentList
        {
            get
            {
                if (_cachedAgentList == null)
                {
                    _cachedAgentList = new Dictionary<Guid, ObservableCollection<User>>();
                }
                return _cachedAgentList;
            }
            set
            {
                _cachedAgentList = value;
            }
        }
        private Dictionary<string, string> _mastersystemConstants;
        public Dictionary<string, string> MasterSystemConstants
        {
            get
            {
                if (_mastersystemConstants == null)
                {
                    _mastersystemConstants = new Dictionary<string, string>();
                }
                return _mastersystemConstants;
            }
            set
            {
                _mastersystemConstants = value;
            }
        }
        private Dictionary<Guid, ObservableCollection<DisplayedPayor>> _cachedPayorLists;

        public Dictionary<Guid, ObservableCollection<DisplayedPayor>> CachedPayorLists
        {
            get
            {
                if (_cachedPayorLists == null)
                {
                    _cachedPayorLists = new Dictionary<Guid, ObservableCollection<DisplayedPayor>>();
                }
                return _cachedPayorLists;
            }
            set
            {
                _cachedPayorLists = value;
            }
        }

        private LicenseeDisplayData _SelectedLicensee;
        public LicenseeDisplayData SelectedLicensee
        {
            get { return _SelectedLicensee; }
            set
            {
                if (_SelectedLicensee != value)
                    _SelectedLicensee = value;

                OnPropertyChanged("SelectedLicensee");
                NotifyLicenseeChange();
            }
        }



        private bool _RefreshRequired = false;
        public bool RefreshRequired
        {
            get { return _RefreshRequired; }
            set { _RefreshRequired = value; }
        }

        private bool _IsRefreshpolicy = false;
        public bool IsRefreshpolicy
        {
            get { return _IsRefreshpolicy; }
            set { _IsRefreshpolicy = value; }
        }

        private bool _IsRefreshClient = false;
        public bool IsRefreshClient
        {
            get { return _IsRefreshClient; }
            set { _IsRefreshClient = value; }
        }

        private bool _IsLoadComminsionDashBoard = false;
        public bool IsLoadComminsionDashBoard
        {
            get { return _IsLoadComminsionDashBoard; }
            set { _IsLoadComminsionDashBoard = value; }
        }

        private bool _IsLoadPayor = false;
        public bool IsLoadPayor
        {
            get { return _IsLoadPayor; }
            set { _IsLoadPayor = value; }
        }

        private bool _isClosedWindow = false;
        public bool isClosedWindow
        {
            get { return _isClosedWindow; }
            set { _isClosedWindow = value; }
        }



        private string _TermanationDate = string.Empty;
        public string TermanationDate
        {
            get { return _TermanationDate; }
            set { _TermanationDate = value; }
        }

        private Guid _NewClientid = Guid.Empty;
        public Guid NewClientid
        {
            get { return _NewClientid; }
            set { _NewClientid = value; }
        }

        private void NotifyLicenseeChange()
        {
            if (SelectedLicensee == null)
                return;

            string CurrentScreenName = VMInstances.CurrentScreenName;
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
                case "Comp Manager":
                    if (VMInstances.CompManager != null)
                        VMInstances.CompManager.SelectedLicenseeChanged();
                    break;
                case "Report Manager":
                    if (VMInstances.RepManager != null)
                        VMInstances.RepManager.SelectedLicenseeChanged();
                    break;
                case "Policy Manager":
                    if (VMInstances.OptimizedPolicyManager != null)
                    {
                        //Guid? ClientId = null;
                        //Guid? PolicyId = null;

                        //if(VMInstances.OptimizedPolicyManager.SelectedDisplayClient != null)
                        //    ClientId = VMInstances.OptimizedPolicyManager.SelectedDisplayClient.ClientId;
                        //if(VMInstances.OptimizedPolicyManager.SelectedPolicy != null)
                        //    PolicyId = VMInstances.OptimizedPolicyManager.SelectedPolicy.PolicyId;

                        VMInstances.OptimizedPolicyManager.OnSelectedLicenseeChanged(null, false);
                    }
                    break;
                //case "Follow Up Manager":
                //    if (VMInstances.FollowUpVM != null)
                //        VMInstances.FollowUpVM.SelectedLicenseeChanged();
                //    break;
            }
        }

        private static VMSharedData _SharedVMData = new VMSharedData();

        //private VMSharedData()
        //{
        //  try
        //  {
        //      using (ServiceClients serviceClients = new ServiceClients())
        //      {
        //          LicenseeClient client = serviceClients.LicenseeClient;
        //          //cooment
        //         // _Licensees = client.GetDisplayedLicenseeList(RoleManager.LicenseeId ?? Guid.Empty);

        //          _Licensees = serviceClients.LicenseeClient.GetDisplayedLicenseeListPolicyManger(RoleManager.LicenseeId ?? Guid.Empty);

        //          if (_Licensees != null)
        //          {
        //              _SelectedLicensee = _Licensees.FirstOrDefault();
        //          }
        //      }
        //  }
        //  catch
        //  {
        //  }
        //}
        //Vinod
        private VMSharedData()
        {
            try
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    //LicenseeClient client = serviceClients.LicenseeClient;
                    _Licensees = serviceClients.LicenseeClient.GetDisplayedLicenseeListPolicyManger(RoleManager.LicenseeId ?? Guid.Empty);

                    if (_Licensees != null)
                    {
                        _SelectedLicensee = _Licensees.FirstOrDefault();
                        
                    }
                }
            }
            catch
            {
            }
        }

        public void RefreshLicensees()
        {
            try
            {
                if (RefreshRequired)
                {
                    using (ServiceClients serviceClients = new ServiceClients())
                    {

                        RefreshRequired = false;
                        Guid SelectedLicenseeId = Guid.Empty;

                        if (_SelectedLicensee != null)
                            SelectedLicenseeId = _SelectedLicensee.LicenseeId;

                        Licensees = serviceClients.LicenseeClient.GetDisplayedLicenseeListPolicyManger(RoleManager.LicenseeId ?? Guid.Empty);
                        if (Licensees != null)
                        {
                            if (SelectedLicenseeId != Guid.Empty && Licensees.Any(s => s.LicenseeId == SelectedLicenseeId))
                                SelectedLicensee = _Licensees.FirstOrDefault(s => s.LicenseeId == SelectedLicenseeId);
                            else
                                SelectedLicensee = _Licensees.FirstOrDefault();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static VMSharedData getInstance()
        {
            try
            {
                _SharedVMData.RefreshLicensees();
            }
            catch
            {
            }
            return _SharedVMData;
        }
        private ObservableCollection<DisplayFollowupIssue> _readonlyMasterFollowUpList;
        public ObservableCollection<DisplayFollowupIssue> ReadonlyMasterFollowUpList
        {
            get
            {
                if (_readonlyMasterFollowUpList == null)
                {
                    _readonlyMasterFollowUpList = new ObservableCollection<DisplayFollowupIssue>();
                }
                return _readonlyMasterFollowUpList;
            }
            set
            {
                _readonlyMasterFollowUpList = value;
            }
        }

        private ObservableCollection<DisplayFollowupIssue> _masterfollowupIssueList;
        public ObservableCollection<DisplayFollowupIssue> MasterFollowupIssueList
        {
            get
            {
                if (_masterfollowupIssueList == null)
                {
                    _masterfollowupIssueList = new ObservableCollection<DisplayFollowupIssue>();
                }
                return _masterfollowupIssueList;
            }
            set
            {
                _masterfollowupIssueList = value;
            }
        }

        private ObservableCollection<User> _GlobalAgentList;
        public ObservableCollection<User> GlobalAgentList
        {
            get
            {
                if (_GlobalAgentList == null)
                {
                    _GlobalAgentList = new ObservableCollection<User>();
                }
                return _GlobalAgentList;
            }
            set
            {
                _GlobalAgentList = value;
            }
        }

        private ObservableCollection<User> _GlobalReportAgentList;
        public ObservableCollection<User> GlobalReportAgentList
        {
            get
            {
                if (_GlobalReportAgentList == null)
                {
                    _GlobalReportAgentList = new ObservableCollection<User>();
                }
                return _GlobalReportAgentList;
            }
            set
            {
                _GlobalReportAgentList = value;
            }
        }

        private bool _isAgentLoded;
        public bool isAgentLoded
        {
            get
            {
                if (_isAgentLoded == null)
                {
                    _isAgentLoded = false;
                }
                return _isAgentLoded;
            }
            set
            {
                _isAgentLoded = value;
            }
        }

        private ObservableCollection<User> _GlobalAllUsers;
        public ObservableCollection<User> GlobalAllUsers
        {
            get
            {
                if (_GlobalAllUsers == null)
                {
                    _GlobalAllUsers = new ObservableCollection<User>();
                }
                return _GlobalAllUsers;
            }
            set
            {
                _GlobalAllUsers = value;
            }
        }

        private bool _isRefeshAgentList = false;
        public bool isRefeshAgentList
        {
            get { return _isRefeshAgentList; }
            set { _isRefeshAgentList = value; }
        }



        private ObservableCollection<DisplayFollowupIssue> _TestmasterfollowupIssueList;
        public ObservableCollection<DisplayFollowupIssue> TestMasterFollowupIssueList
        {
            get
            {
                if (_TestmasterfollowupIssueList == null)
                {
                    _TestmasterfollowupIssueList = new ObservableCollection<DisplayFollowupIssue>();
                }
                return _TestmasterfollowupIssueList;
            }
            set
            {
                _TestmasterfollowupIssueList = value;
            }
        }


        public UpdateMode UpdateMode { get; set; }
    }
}
