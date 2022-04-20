using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.BaseVM;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM;
using MyAgencyVault.VM.VMLib;
using System.Data;
using MyAgencyVault.VMLib;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VM.VMLib
{
    public class MainWindowVM : BaseViewModel
    {

        #region Event&Delegates
        public delegate void OnPolicySearchWindow();
        public event OnPolicySearchWindow onOpenPolicySearchedWindow;

    //    public static PolicyManagerVm _PolicyManager;

       public static bool IsSuperUser;

        #endregion
        #region Constructor
        public MainWindowVM()
        {
            PropertyChanged += new PropertyChangedEventHandler(MainWindowVM_PropertyChanged);
            IsSuperUser = false;
            if (RoleManager.Role == UserRole.SuperAdmin)
            {
                IsSuperUser = true;
            }
            if (_LastViewedClients == null)
                _LastViewedClients = new LastViewPolicyClientCollection();

        }

        void MainWindowVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //case "MLastViewPolicyLst":

                //    break;
                //case "MSelectedMLastViewPolicyLocal":
                //    try
                //    {
                //        if (onChangedSelectedClient != null && GlobalData.IsClientChangeFromPolicySearch!=null)
                //        {
                //            GlobalData.IsClientChangeFromPolicySearch = true;
                //            onChangedSelectedClient(MSelectedMLastViewPolicyLocal.Clientid ?? Guid.Empty, MSelectedMLastViewPolicyLocal.PolicyId ?? Guid.Empty);
                //            GlobalData.IsClientChangeFromPolicySearch = false;
                //        }
                //    }
                //    catch
                //    {

                //    }
                //    break;
            }
        }
        #endregion

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
        #region ClientCombo
        private Client _mselectedClient;
        public Client MSelectedClient
        {
            get
            {
                return _mselectedClient;

            }
            set
            {
                _mselectedClient = value;
                OnPropertyChanged("MSelectedClient");
            }
        }

        private ObservableCollection<Client> _mclientLst;
        public ObservableCollection<Client> MClientLst
        {
            get
            {
                if (_mclientLst == null)
                {
                    _mclientLst = serviceClients.ClientClient.GetClientList(RoleManager.LicenseeId);
                }
                MSelectedClient = _mclientLst.Count == 0 ? new Client() : _mclientLst.FirstOrDefault();

                return _mclientLst;
            }
            set
            {
                _mclientLst = value;
                OnPropertyChanged("MClientLst");

            }
        }
        public ObservableCollection<Client> MGetClientList()
        {
            return serviceClients.ClientClient.GetClientList(RoleManager.LicenseeId);
        }

        #endregion

        #region PayorLocal
        private Payor _mselectedPayor;
        public Payor MSelectedPayor
        {
            get
            {
                return _mselectedPayor;
            }
            set
            {
                _mselectedPayor = value;
                OnPropertyChanged("MSelectedPayor");
            }
        }

        private ObservableCollection<Payor> _mpayorLst;
        public ObservableCollection<Payor> MPayorLst
        {
            get
            {
                MSelectedPayor = _mpayorLst.Count == 0 ? new Payor() : _mpayorLst.FirstOrDefault();
                return _mpayorLst;
            }
            set
            {
                _mpayorLst = value;
                OnPropertyChanged("MPayorLst");
            }
        }

        private ObservableCollection<string> _allclient;
        public ObservableCollection<string> allclient
        {
            get
            {

                return _allclient;
            }
            set
            {
                _allclient = value;
                OnPropertyChanged("allclient");
            }
        }

        

        #endregion

        private ICommand _mpolicyNumberFocus;
        public ICommand MPolicyNumberFocus
        {

            get
            {
                if (_mpolicyNumberFocus == null)
                {
                    _mpolicyNumberFocus = new BaseCommand(x => CorrectSearchPolicyNumber());
                }
                return _mpolicyNumberFocus;
            }
        }
        private void CorrectSearchPolicyNumber()
        {
            if (string.IsNullOrEmpty(PolicyNumberSearchText)) return;
            PolicyNumberSearchText = MyAgencyVault.ViewModel.VMHelper.CorrectPolicyNo(PolicyNumberSearchText);
        }
        private string _PolicyNumbersearchText;
        public string PolicyNumberSearchText
        {
            get
            {
                return _PolicyNumbersearchText;
            }
            set
            {
                _PolicyNumbersearchText = value;
                OnPropertyChanged("PolicyNumberSearchText");
            }
        }

        private string _carrierSearchText;
        public string CarrierSearchText
        {
            get
            {
                return _carrierSearchText;
            }
            set
            {
                _carrierSearchText = value;
                OnPropertyChanged("CarrierSearchText");
            }

        }


        private string _payorrSearchText;
        public string PayorSearchText
        {
            get
            {
                return _payorrSearchText;
            }
            set
            {
                _payorrSearchText = value;
                OnPropertyChanged("PayorSearchText");

            }
        }

        private string _txtClientSearch;
        public string txtClientSearch
        {
            get
            {
                return _txtClientSearch;
            }
            set
            {
                _txtClientSearch = value;
                OnPropertyChanged("txtClientSearch");

            }
        }

        private string _txtInsuredSearch;
        public string txtInsuredSearch
        {
            get
            {
                return _txtInsuredSearch;
            }
            set
            {
                _txtInsuredSearch = value;
                OnPropertyChanged("txtInsuredSearch");

            }
        }

        #region LastViewPolicyCombo

        private LastViewPolicyClientCollection _LastViewedClients;
        public LastViewPolicyClientCollection LastViewedClients
        {
            get
            {
                return _LastViewedClients;
            }
        }

        #endregion

        #region ICommand

        private ICommand _openPSWindow;
        public ICommand OpenPolicySearchedWindow
        {

            get
            {
                if (_openPSWindow == null)
                {
                    _openPSWindow = new BaseCommand(param => OpenPSWindow());
                }
                return _openPSWindow;
            }
        }

        private ICommand _ClearPolicySearchFields;
        public ICommand ClearPolicySearchFields
        {
            get
            {
                if (_ClearPolicySearchFields == null)
                {
                    _ClearPolicySearchFields = new BaseCommand(param => ClearSearchFields());
                }
                return _ClearPolicySearchFields;
            }
        }

        private void ClearSearchFields()
        {
            txtInsuredSearch = null;
            txtClientSearch = null;
            PolicyNumberSearchText = null;
            PayorSearchText = null;
            CarrierSearchText = null;
        }

        private ICommand _setClient_Policy;
        public ICommand SetClientPolicy
        {
            get
            {
                if (_setClient_Policy == null)
                {
                    _setClient_Policy = new BaseCommand(param => ChangedSelectedClient());
                }
                return _setClient_Policy;
            }
        }

        private void OpenPSWindow()
        {
            if (onOpenPolicySearchedWindow != null)
            {
                onOpenPolicySearchedWindow();
            }
        }

        private void ChangedSelectedClient()
        {
            if(MSelectedPolicySearched==null)return;
            if (MSelectedPolicySearched.ClientName == null && MSelectedPolicySearched.PolicyNumber == null)
                return;

            LastViewedClients.ClientOrPolicyChanged(MSelectedPolicySearched.ClienID,MSelectedPolicySearched.ClientName,MSelectedPolicySearched.PolicyId,true);

        }


        #endregion

        #region PolicySearched
        private PolicySearched _mselectedPolicySearched;
        public PolicySearched MSelectedPolicySearched
        {
            get
            {
                return _mselectedPolicySearched;

            }
            set
            {
                _mselectedPolicySearched = value;
                OnPropertyChanged("MSelectedPolicySearched");
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z')
                    // || c == '.' || c == '_'
                    )
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private ObservableCollection<PolicySearched> _mpolicySearchedLst;
        public ObservableCollection<PolicySearched> MPolicySearchedLst
        {

            get
            {
                string policysearchtextf = PolicyNumberSearchText;

                if ((!String.IsNullOrEmpty(PolicyNumberSearchText)))
                {
                    string sPolicyNumber = RemoveSpecialCharacters(PolicyNumberSearchText);
                    policysearchtextf = sPolicyNumber.TrimStart('0');
                }


                using (serviceClients.PolicySearchedClient)
                    if (RoleManager.IsHouseAccount == true)
                    {
                        _mpolicySearchedLst = serviceClients.PolicySearchedClient.GetAllSearchedPolicies(txtClientSearch, txtInsuredSearch, policysearchtextf, CarrierSearchText, PayorSearchText, RoleManager.userCredentialID, UserRole.HO, SharedVMData.SelectedLicensee.LicenseeId);

                    }
                    else
                    {
                        _mpolicySearchedLst = serviceClients.PolicySearchedClient.GetAllSearchedPolicies(txtClientSearch, txtInsuredSearch, policysearchtextf, CarrierSearchText, PayorSearchText, RoleManager.userCredentialID, RoleManager.Role, SharedVMData.SelectedLicensee.LicenseeId);
                    }

                if (_mpolicySearchedLst != null)
                    MSelectedPolicySearched = _mpolicySearchedLst.Count == 0 ? new PolicySearched() : _mpolicySearchedLst.FirstOrDefault();

                return _mpolicySearchedLst;


            }
            set
            {
                _mpolicySearchedLst = value;
                OnPropertyChanged("MPolicySearchedLst");

            }
        }

        public void TurnOffNewsFlashBit(Guid userId)
        {
            serviceClients.UserClient.TurnOffNewsToFlashBit(userId);
        }

        #endregion
 
        #region ModuleLevelSecurity
        private string _ispeoplevisible;
        public string IsPeopleVisible 
        {
            get
            {
                bool UserLevelSecCheck = false;

                if((RoleManager.Role==UserRole.Agent))
                {
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.PeopleManager);
                }
                _ispeoplevisible =
                 (RoleManager.Role == UserRole.Administrator) ||
                 (IsSuperUser)|| (UserLevelSecCheck)&&(
                       (RoleManager.Role == UserRole.Agent)
                       || (RoleManager.Role == UserRole.HO))
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _ispeoplevisible;
            }
            set
            {
                _ispeoplevisible = value;
                OnPropertyChanged("IsPeopleVisible");
            }
        }
        
        private string _ispolicyvisiblel;
        public string IsPolicyVisible
        {
            get
            {
                bool UserLevelSecCheck = false;
                if ((RoleManager.Role == UserRole.Agent))
                {
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.PolicyManager);
                }
                
                _ispolicyvisiblel = 
                 (RoleManager.Role == UserRole.Administrator)
                    ||   (IsSuperUser) || (UserLevelSecCheck)&&  (  
                 (RoleManager.Role==UserRole.Agent)
                    || (RoleManager.Role == UserRole.HO))
                    ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
               
                return _ispolicyvisiblel;
            }
            set
            {
                _ispolicyvisiblel = value;
                OnPropertyChanged("IsPolicyVisible");
            }
        }

        private string _ispayorvisiblel;
        public string IsPayorVisible
        {
            get
            {
                _ispayorvisiblel = IsSuperUser 
                  ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _ispayorvisiblel;
            }
            set
            {
                _ispayorvisiblel = value;
                OnPropertyChanged("IsPayorVisible");
            }
        }

        private string _isstmtvisiblel;
        public string IsStmtVisible
        {
            get
            {            
                _isstmtvisiblel = IsSuperUser
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
                
                return _isstmtvisiblel;
            }
            set
            {
                _isstmtvisiblel = value;
                OnPropertyChanged("IsStmtVisible");
            }
        }

        private string _issettingvisiblel;
        public string IsSettingVisible
        {
            get
            {
                bool UserLevelSecCheck = false;
                if ((RoleManager.Role == UserRole.Agent))
                {
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.Settings);
                }
                _issettingvisiblel =
              (RoleManager.Role == UserRole.Administrator)
                       || (IsSuperUser) || (UserLevelSecCheck) && (
                    // || 
                      (RoleManager.Role == UserRole.Agent)
                       || (RoleManager.Role == UserRole.HO))
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _issettingvisiblel;
            }
            set
            {
                _issettingvisiblel = value;
                OnPropertyChanged("IsSettingVisible");
            }
        }

        private string _isfollowupvisiblel;
        public string IsFollowUpVisible
        {
            get
            {
                bool UserLevelSecCheck = false;
              
                if ((RoleManager.Role == UserRole.Agent))
                {
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.FollowUpManger);
                }
                _isfollowupvisiblel =
              (RoleManager.Role == UserRole.Administrator)
                       ||   (IsSuperUser) || (UserLevelSecCheck) && (
                       (RoleManager.Role == UserRole.Agent)
                       || (RoleManager.Role == UserRole.HO))
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _isfollowupvisiblel;
            }
            set
            {
                _isfollowupvisiblel = value;
                OnPropertyChanged("IsFollowUpVisible");
            }
        }

        private string _ishelpupdatevisiblel;
        public string IsHelpUpdateVisible
        {
            get
            {
               bool UserLevelSecCheck = false;
              
                if ((RoleManager.Role == UserRole.Agent))
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.HelpUpdate);
              
                    _ishelpupdatevisiblel =
              (RoleManager.Role==UserRole.DEP)
                       ||   (RoleManager.Role == UserRole.Administrator)
                       || (IsSuperUser) || (UserLevelSecCheck) && (
                       (RoleManager.Role == UserRole.Agent)
                       ||(RoleManager.Role == UserRole.HO))
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _ishelpupdatevisiblel;
            }
            set
            {
                _ishelpupdatevisiblel = value;
                OnPropertyChanged("IsHelpUpdateVisible");
            }
        }

        private string _iscompmanagervisiblel;
        public string IsCompManagerVisible
        {
            get
            {
                bool UserLevelSecCheck = false;
                if ((RoleManager.Role == UserRole.Agent))
                UserLevelSecCheck = SecurityModuleWise((int)MasterModule.CompManager);
                _iscompmanagervisiblel =
         (RoleManager.Role == UserRole.DEP)
                  ||  (RoleManager.Role == UserRole.Administrator)
                  || (IsSuperUser) || (UserLevelSecCheck) && (
                  (RoleManager.Role == UserRole.Agent)
                  ||  (RoleManager.Role == UserRole.HO))
                  ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
                
                return _iscompmanagervisiblel;
            }
            set
            {
                _iscompmanagervisiblel = value;
                OnPropertyChanged("IsCompManagerVisible");
            }
        }

        private string _isbillingvisiblel;
        public string IsBillingVisible
        {
            get
            {
                _isbillingvisiblel =
                 (IsSuperUser)                   
                  ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _isbillingvisiblel;
            }
            set
            {
                _iscompmanagervisiblel = value;
                OnPropertyChanged("IsBillingVisible");
            }
        }

        private string _isdeuvisiblel;
        public string IsDEUVisible
        {
            get
            {              
                _isdeuvisiblel = (IsSuperUser) || (RoleManager.Role == UserRole.DEP)
                   ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
                
                return _isdeuvisiblel;
            }
            set
            {
                _isdeuvisiblel = value;
                OnPropertyChanged("IsDEUVisible");
            }
        }

        private string _isreportvisiblel;
        public string IsReportVisible
        {
            get
            {
              //  _isreportvisiblel = SecurityModuleWise((int)MasterModule.ReportManager) || (IsSuperUser) ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                bool UserLevelSecCheck = false;
               // if ((RoleManager.Role != UserRole.DEP))
                if ((RoleManager.Role == UserRole.Agent))
                {
                    UserLevelSecCheck = SecurityModuleWise((int)MasterModule.ReportManager);
                }
                _isreportvisiblel =
                (RoleManager.Role == UserRole.Administrator)
                       || (IsSuperUser) || (UserLevelSecCheck) && (
                    // || 
                       (RoleManager.Role == UserRole.Agent)
                       || (RoleManager.Role == UserRole.HO))
                       ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
                return _isreportvisiblel;
            }
            set
            {
                _isreportvisiblel = value;
                OnPropertyChanged("IsReportVisible");
            }
        }

        private string _isconfigurationvisiblel;
        public string IsConfigurationVisible
        {
            get
            {
             //   _isconfigurationvisiblel = SecurityModuleWise((int)MasterModule.Configuration) || (IsSuperUser) ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();


                _isconfigurationvisiblel =
                 (IsSuperUser)
                  ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();

                return _isconfigurationvisiblel;
            }
            set
            {
                _isconfigurationvisiblel = value;
                OnPropertyChanged("IsConfigurationVisible");
            }
        }     
        /// <summary>
        /// People Module
        /// </summary>
        private bool _ispeopleenable;
        public bool IsPeopleEnable
        {
            get
            {
                _ispeopleenable = SecurityModuleWise((int)MasterModule.PeopleManager);
                IsPeopleVisible = _ispeopleenable ? System.Windows.Visibility.Visible.ToString() : System.Windows.Visibility.Collapsed.ToString();
                return _ispeopleenable;
            }
            set
            {
                _ispeopleenable = value;
                OnPropertyChanged("IsPeopleEnable");
              
            }
        }

        /// <summary>
        /// Policy Module
        /// </summary>
        private bool _ispolicyenable;
        public bool IsPolicyEnable
        {
            get
            {
                _ispolicyenable = SecurityModuleWise((int)MasterModule.PolicyManager);
                return _ispolicyenable;
            }
            set
            {
                _ispolicyenable = value;
                OnPropertyChanged("IsPolicyEnable");
            }
        }

        /// <summary>
        /// Setting Module
        /// </summary>
        private bool _issettingenable;
        public bool IsSettingEnable
        {
            get
            {
                _issettingenable = SecurityModuleWise((int)MasterModule.Settings);
                return _issettingenable;
            }
            set
            {
                _issettingenable = value;
                OnPropertyChanged("IsSettingEnable");
            }
        }

        /// <summary>
        /// FollowUp Module
        /// </summary>
        private bool _isfollowupenable;
        public bool IsFollowUpEnable
        {
            get
            {
                _isfollowupenable = SecurityModuleWise((int)MasterModule.FollowUpManger);
                return _isfollowupenable;
            }
            set
            {
                _isfollowupenable = value;
                OnPropertyChanged("IsFollowUpEnable");
            }
        }


        /// <summary>
        /// HelpUpdate Module
        /// </summary>
        private bool _ishelpupdateenable;
        public bool IsHelpUpdateEnable
        {
            get
            {
                _ishelpupdateenable = SecurityModuleWise((int)MasterModule.HelpUpdate);
                return _ishelpupdateenable;
            }
            set
            {
                _ishelpupdateenable = value;
                OnPropertyChanged("IsHelpUpdateEnable");
            }
        }

        /// <summary>
        /// CompManager Module
        /// </summary>
        private bool _iscompmanagerenable;
        public bool IsCompManagerEnable
        {
            get
            {
                _iscompmanagerenable = SecurityModuleWise((int)MasterModule.CompManager);
                return _iscompmanagerenable;
            }
            set
            {
                _iscompmanagerenable = value;
                OnPropertyChanged("IsCompManagerEnable");
            }
        }

        /// <summary>
        /// ReportManager Module
        /// </summary>
        private bool _isreportmanagerenable;
        public bool IsReportManagerEnable
        {
            get
            {
                _isreportmanagerenable = SecurityModuleWise((int)MasterModule.ReportManager);
                return _isreportmanagerenable;
            }
            set
            {
                _isreportmanagerenable = value;
                OnPropertyChanged("IsReportManagerEnable");
            }
        }

        #endregion

        private bool SecurityModuleWise(int _module)
        {            
            bool flag = true;
            try
            {
                if (RoleManager.UserPermissions == null)
                    return flag;
                try
                {
                    if (RoleManager.UserPermissions[_module - 1].Permission == ModuleAccessRight.NoAccess)
                    {
                        flag = false;
                    }
                }
                catch
                {
                    flag = false;
                }
            }
            catch(Exception)
            {
                
            }
            return flag;
        }
    }
}
