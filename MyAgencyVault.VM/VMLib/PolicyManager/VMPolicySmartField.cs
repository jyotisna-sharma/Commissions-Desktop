using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM;
using System.Windows.Input;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;
using MyAgencyVault.ViewModel.VMLib;
using System.Threading;

namespace MyAgencyVault.VMLib.PolicyManager
{
    public class VMPolicySmartField : BaseViewModel
    {
        #region Constructor

        Guid licenseID = new Guid();

        //public delegate void SelectedLearnedChangedEventHandler(string modifiedText);
        //public event SelectedLearnedChangedEventHandler SelectedLearnedChanged;

        public VMPolicySmartField(VMOptimizePolicyManager OptimizedPolicyManager, PolicyDetailMasterData MasterData, Guid SelectedLicensee)
        {
            //added to selected client change
            OptimizedPolicyManager.SelectedClientChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);

            PropertyChanged += new PropertyChangedEventHandler(VMPolicySmartField_PropertyChanged);

            serviceClients.CoverageClient.GetCoverageNickNameCompleted += new EventHandler<GetCoverageNickNameCompletedEventArgs>(CoverageClient_GetCoverageNickNameCompleted);
            OptimizedPolicyManager.SelectedLicenseeChanged += new VMOptimizePolicyManager.SelectedLicenseeChangedEventHandler(OptimizedPolicyManager_SelectedLicenseeChanged);
            OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedPolicyChangedEventHandler(OptimizedPolicyManager_SelectedPolicyChanged);
            OptimizedPolicyManager.SelectedPolicySaved += new VMOptimizePolicyManager.SelectedPolicySavedEventHandler(OptimizedPolicyManager_SelectedPolicySaved);
            // OptimizedPolicyManager.SelectedPolicyDetailSmartIssueChanged += new VMOptimizePolicyManager.SelectedPolicyDetailSmartIssueHandler(OptimizedPolicyManager_SelectedPolicyDetailSmartIssueChanged);
            OptimizedPolicyManager.SelectedPayorChanged += new VMOptimizePolicyManager.SelectedPayorChangedEventHandler(OptimizedPolicyManager_SelectedPayorChanged);
            OptimizedPolicyManager.SelectedCarrierChanged += new VMOptimizePolicyManager.SelectedCarrierChangedEventHandler(OptimizedPolicyManager_SelectedCarrierChanged);
            OptimizedPolicyManager.SelectedCoverageChanged += new VMOptimizePolicyManager.SelectedCoverageChangedEventHandler(OptimizedPolicyManager_SelectedCoverageChanged);
            serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseCompleted += new EventHandler<GetPolicyLearnedFieldsPolicyWiseCompletedEventArgs>(PolicyLearnedFieldClient_GetPolicyLearnedFieldsPolicyWiseCompleted);

            //OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);

            //serviceClients.ClientClient.GetAllClientListCompleted += new EventHandler<GetAllClientListCompletedEventArgs>(AllClientClient_GetAllClientListCompleted);

            LearnedMasterIncomingPaymentTypeLst = MasterData.LearnedMasterIncomingPaymentTypes;
            LrndMasterPaymentsModeData = MasterData.LearnedMasterPaymentsModes;

            licenseID = SelectedLicensee;

            //Acme added - to enable/disable new policyiD txt field - April 10, 2017
            if (RoleManager.Role == UserRole.SuperAdmin)
            {
                PolicyIDEnabled = true;
            }

        }

        void CoverageClient_GetCoverageNickNameCompleted(object sender, GetCoverageNickNameCompletedEventArgs e)
        {
            if (e.Error == null)
                SelecetdPolicyLearnedField.CoverageNickName = e.Result;
            else
                SelecetdPolicyLearnedField.CoverageNickName = string.Empty;
        }


        void OptimizedPolicyManager_SelectedCoverageChanged(DisplayedCoverage SelectedCoverage)
        {
            this.SelectedCoverage = SelectedCoverage;
        }

        void OptimizedPolicyManager_SelectedCarrierChanged(Carrier SelectedCarrier)
        {
            this.SelectedCarrier = SelectedCarrier;
        }

        void OptimizedPolicyManager_SelectedPayorChanged(DisplayedPayor SelectedPayor)
        {
            this.SelectedPayor = SelectedPayor;
        }

        //Add this to get selected client
        private Client SelectedClient;
        void OptimizedPolicyManager_SelectedClientChanged(Client SelectedClient)
        {
            this.SelectedClient = SelectedClient;
            if (SelectedClient != null && SelectedClient.Name != null)
                SelectedClientLrnd = DisplayedClientsLists.Where(p => p.Name.ToLower() == SelectedClient.Name.ToString().ToLower()).FirstOrDefault();
        }

        //void OptimizedPolicyManager_SelectedPolicyDetailSmartIssueChanged(Guid PolicyId)
        //{
        //    VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(PolicyId);
        //}
        string lastModifiedDetail = "";

        public string LastModifiedDetail
        {
            get { return lastModifiedDetail; }
            set
            {
                lastModifiedDetail = value;
                OnPropertyChanged("LastModifiedDetail");
            }
        }
        private string _AutotermDate;
        public string AutoTermDate
        {
            get
            {
                return _AutotermDate;
            }
            set
            {
                _AutotermDate = value;
                OnPropertyChanged("AutoTermDate");
            }
        }

        private bool _policyIDEnabled = false;
        public bool PolicyIDEnabled
        {
            get
            {
                return _policyIDEnabled;
            }
            set
            {
                _policyIDEnabled = value;
            }
        }

        void VMPolicySmartField_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    //SelectedPolicyLearnField.CompScheduleType
                    case "SelectedCoverage":
                    case "SelectedCarrier":
                    case "SelectedPayor":

                        //check null then call the service
                        if (SelectedPayor != null && SelectedCarrier != null && SelectedCoverage != null)
                        {
                            serviceClients.CoverageClient.GetCoverageNickNameAsync(SelectedPayor.PayorID, SelectedCarrier.CarrierId, SelectedCoverage.CoverageID);
                        }

                        break;

                    case "SelecetdPolicyLearnedField":
                        if (SelecetdPolicyLearnedField == null || SelecetdPolicyLearnedField.PolicyId == Guid.Empty)
                        {
                            SelectedClientLrnd = null;
                            SelectedMasterIncomingPaymentTypeLrnd = null;
                            SelectedPaymentModeLrnd = null;
                            return;
                        }

                        LastModifiedDetail = SelecetdPolicyLearnedField.LastModifiedDetail;
                        //string msg = SelecetdPolicyLearnedField.LastModifiedOn == null ? "" : " on " + Convert.ToDateTime(SelecetdPolicyLearnedField.LastModifiedOn).ToString("MM/dd/yyyy");

                        //if (!string.IsNullOrEmpty(SelecetdPolicyLearnedField.LastModifiedUserName))
                        //{
                        //    LastModifiedDetail = "Last modified by " + SelecetdPolicyLearnedField.LastModifiedUserName + msg;
                        //}
                        //else if(SelecetdPolicyLearnedField.LastModifiedOn != null)
                        //{
                        //    LastModifiedDetail = "Last modified"+ msg;
                        //}
                        //else
                        //{
                        //    LastModifiedDetail = "Last modified - No information";
                        //}

                        #region"Client pull down and comp type is displayed in smart fields "

                        if (DisplayedClientsLists.Count > 0)
                        {
                            SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
                        }
                        else
                        {
                            if (licenseID != null)
                            {
                                DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(licenseID).OrderBy(o => o.Name).ToList());
                                //Selected client name
                                if (SelecetdPolicyLearnedField != null)
                                {
                                    SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();

                                }
                            }
                        }

                        SelectedMasterIncomingPaymentTypeLrnd = LearnedMasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelecetdPolicyLearnedField.CompTypeId).FirstOrDefault();
                        SelectedPaymentModeLrnd = LrndMasterPaymentsModeData.Where(p => p.ModeId == SelecetdPolicyLearnedField.PolicyModeId).FirstOrDefault();

                        AutoTermDate = SelecetdPolicyLearnedField.AutoTerminationDate.ToString();
                        SelecetdPolicyLearnedField.PMC = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePMC(SelecetdPolicyLearnedField.PolicyId));
                        SelecetdPolicyLearnedField.PAC = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelecetdPolicyLearnedField.PolicyId));



                        #endregion

                        break;
                    case "SelectedClientLrnd":
                        if (SelectedClientLrnd == null) return;
                        SelecetdPolicyLearnedField.ClientID = SelectedClientLrnd.ClientId;

                        break;
                    case "SelectedMasterIncomingPaymentTypeLrnd":
                        if (SelectedMasterIncomingPaymentTypeLrnd == null) return;
                        SelecetdPolicyLearnedField.CompTypeId = SelectedMasterIncomingPaymentTypeLrnd.PaymentTypeId;

                        break;
                    case "SelectedPaymentModeLrnd":
                        if (SelectedPaymentModeLrnd == null) return;
                        SelecetdPolicyLearnedField.PolicyModeId = SelectedPaymentModeLrnd.ModeId;

                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Selected Object

        private PolicyDetailsData SelectedPolicy;

        public void OptimizedPolicyManager_SelectedPolicyChanged(PolicyDetailsData SelectedPolicy)
        {
            this.SelectedPolicy = SelectedPolicy;

            if (this.SelectedPolicy == null || this.SelectedPolicy.PolicyId == Guid.Empty)
                SelecetdPolicyLearnedField = null;
           // LastModifiedOn-27-12-2019 : For refresh a policy details when changed the policy 
            //else
            //    if (this.SelectedPolicy.LearnedFields == null)
            //    {
            serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseAsync(SelectedPolicy.PolicyId);
            //}
            //else
            //{
            //    SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields;
            //}


        }

        void OptimizedPolicyManager_SelectedPolicySaved()
        {
            //cHECK NULL 
            if (SelectedPolicy != null)
            {
                if (this.SelectedPolicy.PolicyId != null)
                {
                    if (this.SelectedPolicy.PolicyId != Guid.Empty)
                    {
                        if (this.SelectedPolicy.LearnedFields != null)
                        {
                            SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields;
                        }
                        else
                        {
                            serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseAsync(SelectedPolicy.PolicyId);
                        }
                    }
                }
            }
        }

        void PolicyLearnedFieldClient_GetPolicyLearnedFieldsPolicyWiseCompleted(object sender, GetPolicyLearnedFieldsPolicyWiseCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (SelectedPolicy != null)
                        SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields = e.Result;
                }
            }
            catch
            {
            }
        }

        void OptimizedPolicyManager_SelectedLicenseeChanged(LicenseeDisplayData SelectedLicensee, ObservableCollection<Client> Clients)
        {
            DisplayedClientsLists = Clients;
        }


        #endregion

        #region Collection

        private ObservableCollection<Client> displayedClientsLists = new ObservableCollection<Client>();
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

        private Client selectedclientlrnd;
        public Client SelectedClientLrnd
        {
            get { return selectedclientlrnd == null ? new Client() : selectedclientlrnd; }
            set { selectedclientlrnd = value; OnPropertyChanged("SelectedClientLrnd"); }
        }


        private PolicyLearnedFieldData _selectedPolicyLearnedField;
        public PolicyLearnedFieldData SelecetdPolicyLearnedField
        {
            get
            {
                return _selectedPolicyLearnedField == null ? new PolicyLearnedFieldData() : _selectedPolicyLearnedField;
            }
            set
            {
                _selectedPolicyLearnedField = value;
                OnPropertyChanged("SelecetdPolicyLearnedField");
            }
        }

        private PolicyIncomingPaymentType selectedMasterIncomingPaymentTypelrnd;
        public PolicyIncomingPaymentType SelectedMasterIncomingPaymentTypeLrnd
        {
            get { return selectedMasterIncomingPaymentTypelrnd == null ? new PolicyIncomingPaymentType() : selectedMasterIncomingPaymentTypelrnd; }
            set { selectedMasterIncomingPaymentTypelrnd = value; OnPropertyChanged("SelectedMasterIncomingPaymentTypeLrnd"); }
        }

        private ObservableCollection<PolicyIncomingPaymentType> learnedmasterIncomingPaymentTypeLst;
        public ObservableCollection<PolicyIncomingPaymentType> LearnedMasterIncomingPaymentTypeLst
        {
            get
            {
                return learnedmasterIncomingPaymentTypeLst;
            }
            set
            {
                learnedmasterIncomingPaymentTypeLst = value;
                OnPropertyChanged("LearnedMasterIncomingPaymentTypeLst");
            }
        }

        private PolicyMode selectedPaymentModelrnd;
        public PolicyMode SelectedPaymentModeLrnd
        {
            get { return selectedPaymentModelrnd == null ? new PolicyMode() : selectedPaymentModelrnd; }
            set { selectedPaymentModelrnd = value; OnPropertyChanged("SelectedPaymentModeLrnd"); }
        }

        private ObservableCollection<PolicyMode> lrndmasterPaymentsModeData;
        public ObservableCollection<PolicyMode> LrndMasterPaymentsModeData
        {
            get
            {
                return lrndmasterPaymentsModeData;
            }
            set
            {
                lrndmasterPaymentsModeData = value;
                OnPropertyChanged("LrndMasterPaymentsModeData");
            }
        }

        private DisplayedPayor _SelectedPayor;
        public DisplayedPayor SelectedPayor
        {
            get
            {
                return _SelectedPayor;
            }
            set
            {
                _SelectedPayor = value;
                OnPropertyChanged("SelectedPayor");
            }
        }

        private Carrier _SelectedCarrier;
        public Carrier SelectedCarrier
        {
            get
            {
                return _SelectedCarrier;
            }
            set
            {
                _SelectedCarrier = value;
                OnPropertyChanged("SelectedCarrier");
            }
        }
        private DisplayedCoverage _SelectedCoverage;
        public DisplayedCoverage SelectedCoverage
        {
            get
            {
                return _SelectedCoverage;
            }
            set
            {
                _SelectedCoverage = value;
                OnPropertyChanged("SelectedCoverage");
            }
        }

        private string _Compscheduletype;
        public string Compscheduletype
        {
            get
            {
                return _Compscheduletype;
            }
            set
            {
                _Compscheduletype = value;
                OnPropertyChanged("_Compscheduletype");
            }
        }

        private decimal? calculatePMC(PolicyLearnedFieldData SelecetdPolicyLearnedField)
        {
            //SET @ResultVar = (@SplitPercentage * @ModalPremium * @Commission) / 10000
            //                                                  / ( CASE
            //                                                  WHEN @PolicyMode = 0
            //                                                  THEN 1
            //                                                  WHEN @PolicyMode = 1
            //                                                  THEN 3
            //                                                  WHEN @PolicyMode = 2
            //                                                  THEN 6
            //                                                  ELSE 12
            //                                                  END 


            decimal? splitPercetange = SelecetdPolicyLearnedField.Link2;
            decimal? ModelPremium = SelecetdPolicyLearnedField.ModalAvgPremium;
            decimal? commision = 100;

            decimal? db = splitPercetange * ModelPremium * commision / 10000;

            if (SelecetdPolicyLearnedField.PolicyModeId == 0)
            {
                return db;
            }
            else if (SelecetdPolicyLearnedField.PolicyModeId == 1)
            {
                return db / 3;
            }
            else if (SelecetdPolicyLearnedField.PolicyModeId == 2)
            {
                return db / 6;
            }
            else
            {
                return db / 12;
            }
        }


        #endregion
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
        #region Save Learned Field Command

        private ICommand savePolicyLearnedFieldCmd;
        public ICommand SavePolicyLearnedFieldCmd
        {
            get
            {
                if (savePolicyLearnedFieldCmd == null)
                {
                    savePolicyLearnedFieldCmd = new BaseCommand(x => BeforeSaveLearndField(), x => SaveLearndField());
                }
                return savePolicyLearnedFieldCmd;
            }
        }

        private bool BeforeSaveLearndField()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //if selected policy is not saved then disable save 
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role != UserRole.SuperAdmin)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void SaveLearndField()
        {
            try
            {
                if (SelecetdPolicyLearnedField == null)
                    return;

                //Acme - to track user
                SelecetdPolicyLearnedField.LastModifiedUserCredentialId = RoleManager.userCredentialID;

                serviceClients.PolicyLearnedFieldClient.AddUpdatePolicyLearnedField(SelecetdPolicyLearnedField, SelecetdPolicyLearnedField.ProductType);
                //Acme fixed the following by calling after above statement, else data never updated 
                serviceClients.LearnedToPolicyPostClient.AddLearnedToPolicy(SelecetdPolicyLearnedField.PolicyId);
                //Acme - added to save last modified detail
                UpdateLastModifiedTime();
                serviceClients.PolicyClient.SavePolicyLastUpdated(SelecetdPolicyLearnedField.PolicyId, RoleManager.userCredentialID);
                //--Policy History
                serviceClients.PolicyClient.AddUpdatePolicyHistory(SelectedPolicy.PolicyId);
                serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearned(SelectedPolicy.PolicyId);

                //ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

                //if ((SelecetdPolicyLearnedField.PreviousEffectiveDate != SelecetdPolicyLearnedField.Effective || SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId || SelecetdPolicyLearnedField.PrevoiusTrackFromDate != SelecetdPolicyLearnedField.TrackFrom))
                //{
                //    //Thread ThreadSmart = new Thread(() =>
                //    //{
                //    //    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId);
                //    //});
                //    //ThreadSmart.IsBackground = true;
                //    //ThreadSmart.Start();

                //    //Thread newThread = new Thread(new ThreadStart(ThreadFollowUpStartingPoint));
                //    //newThread.SetApartmentState(ApartmentState.STA);
                //    //newThread.IsBackground = true;
                //    //newThread.Start();   

                //    BackgroundWorker worker = new BackgroundWorker();
                //    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(FollowUP_DoWork);
                //    worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(FollowUP_RunWorkerCompleted);
                //    worker.RunWorkerAsync();
                //}

                SelecetdPolicyLearnedField.PreviousEffectiveDate = SelecetdPolicyLearnedField.Effective;
                SelecetdPolicyLearnedField.PreviousPolicyModeid = SelecetdPolicyLearnedField.PolicyModeId;
                SelecetdPolicyLearnedField.PrevoiusTrackFromDate = SelecetdPolicyLearnedField.TrackFrom;



            }
            catch (Exception)
            {
            }
        }
        public void UpdateLastModifiedTime()
        {
            SelecetdPolicyLearnedField.LastModifiedOn = DateTime.Now;
            SelecetdPolicyLearnedField.LastModifiedUserName = RoleManager.LoggedInUser;
            string msg = SelecetdPolicyLearnedField.LastModifiedOn == null ? "" : " on " + Convert.ToDateTime(SelecetdPolicyLearnedField.LastModifiedOn).ToString("MM/dd/yyyy hh:mm tt");

            if (!string.IsNullOrEmpty(SelecetdPolicyLearnedField.LastModifiedUserName))
            {
                LastModifiedDetail = "Last modified by " + SelecetdPolicyLearnedField.LastModifiedUserName + msg;
            }
            else if (SelecetdPolicyLearnedField.LastModifiedOn != null)
            {
                LastModifiedDetail = "Last modified" + msg;
            }
            else
            {
                LastModifiedDetail = "Last modified - No information";
            }
            SelecetdPolicyLearnedField.LastModifiedDetail = LastModifiedDetail;
        }

        private void ThreadFollowUpStartingPoint()
        {
            VMInstances.PolicyCommissionVM.IsBusy = true;
            serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
            VMInstances.PolicyCommissionVM.FillFollowUpIssue();
            System.Windows.Threading.Dispatcher.Run();
        }

        void FollowUP_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                VMInstances.PolicyCommissionVM.IsBusy = true;
                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
            }
            catch
            {
            }
        }

        void FollowUP_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            VMInstances.PolicyCommissionVM.FillFollowUpIssue();
        }

        #endregion



    }
}






























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MyAgencyVault.VM.MyAgencyVaultSvc;
//using System.Collections.ObjectModel;
//using MyAgencyVault.VM.BaseVM;
//using MyAgencyVault.VM;
//using System.Windows.Input;
//using MyAgencyVault.ViewModel.CommonItems;
//using System.ComponentModel;
//using MyAgencyVault.ViewModel.VMLib;
//using System.Threading;
//using MyAgencyVault.ViewModel;

//namespace MyAgencyVault.VMLib.PolicyManager
//{
//    public class VMPolicySmartField : BaseViewModel
//    {
//        #region Constructor

//        Guid licenseID = new Guid();

//        public VMPolicySmartField(VMOptimizePolicyManager OptimizedPolicyManager, PolicyDetailMasterData MasterData, Guid SelectedLicensee)
//        {
//            //added to selected client change
//            OptimizedPolicyManager.SelectedClientChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);

//            PropertyChanged += new PropertyChangedEventHandler(VMPolicySmartField_PropertyChanged);

//            serviceClients.CoverageClient.GetCoverageNickNameCompleted += new EventHandler<GetCoverageNickNameCompletedEventArgs>(CoverageClient_GetCoverageNickNameCompleted);
//            OptimizedPolicyManager.SelectedLicenseeChanged += new VMOptimizePolicyManager.SelectedLicenseeChangedEventHandler(OptimizedPolicyManager_SelectedLicenseeChanged);
//            OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedPolicyChangedEventHandler(OptimizedPolicyManager_SelectedPolicyChanged);
//            OptimizedPolicyManager.SelectedPolicySaved += new VMOptimizePolicyManager.SelectedPolicySavedEventHandler(OptimizedPolicyManager_SelectedPolicySaved);
//            // OptimizedPolicyManager.SelectedPolicyDetailSmartIssueChanged += new VMOptimizePolicyManager.SelectedPolicyDetailSmartIssueHandler(OptimizedPolicyManager_SelectedPolicyDetailSmartIssueChanged);
//            OptimizedPolicyManager.SelectedPayorChanged += new VMOptimizePolicyManager.SelectedPayorChangedEventHandler(OptimizedPolicyManager_SelectedPayorChanged);
//            OptimizedPolicyManager.SelectedCarrierChanged += new VMOptimizePolicyManager.SelectedCarrierChangedEventHandler(OptimizedPolicyManager_SelectedCarrierChanged);
//            OptimizedPolicyManager.SelectedCoverageChanged += new VMOptimizePolicyManager.SelectedCoverageChangedEventHandler(OptimizedPolicyManager_SelectedCoverageChanged);
//            serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseCompleted += new EventHandler<GetPolicyLearnedFieldsPolicyWiseCompletedEventArgs>(PolicyLearnedFieldClient_GetPolicyLearnedFieldsPolicyWiseCompleted);

//            //OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);

//            //serviceClients.ClientClient.GetAllClientListCompleted += new EventHandler<GetAllClientListCompletedEventArgs>(AllClientClient_GetAllClientListCompleted);

//            LearnedMasterIncomingPaymentTypeLst = MasterData.LearnedMasterIncomingPaymentTypes;
//            LrndMasterPaymentsModeData = MasterData.LearnedMasterPaymentsModes;
//            licenseID = SelectedLicensee;

//            //this.SelectedPolicy.LearnedFields = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);

//        }

//        void CoverageClient_GetCoverageNickNameCompleted(object sender, GetCoverageNickNameCompletedEventArgs e)
//        {
//            if (e.Error == null)
//                SelecetdPolicyLearnedField.CoverageNickName = e.Result;
//            else
//                SelecetdPolicyLearnedField.CoverageNickName = string.Empty;
//        }


//        void OptimizedPolicyManager_SelectedCoverageChanged(DisplayedCoverage SelectedCoverage)
//        {
//            this.SelectedCoverage = SelectedCoverage;
//        }

//        void OptimizedPolicyManager_SelectedCarrierChanged(Carrier SelectedCarrier)
//        {
//            this.SelectedCarrier = SelectedCarrier;
//        }

//        void OptimizedPolicyManager_SelectedPayorChanged(DisplayedPayor SelectedPayor)
//        {
//            this.SelectedPayor = SelectedPayor;
//        }

//        //Add this to get selected client
//        private Client SelectedClient;
//        void OptimizedPolicyManager_SelectedClientChanged(Client SelectedClient)
//        {
//            this.SelectedClient = SelectedClient;
//            if (SelectedClient != null && SelectedClient.Name != null)
//                SelectedClientLrnd = DisplayedClientsLists.Where(p => p.Name.ToLower() == SelectedClient.Name.ToString().ToLower()).FirstOrDefault();
//        }

//        //void OptimizedPolicyManager_SelectedPolicyDetailSmartIssueChanged(Guid PolicyId)
//        //{
//        //    VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(PolicyId);
//        //}

//        private string _AutotermDate;
//        public string AutoTermDate
//        {
//            get
//            {
//                return _AutotermDate;
//            }
//            set
//            {
//                _AutotermDate = value;
//                OnPropertyChanged("AutoTermDate");
//            }
//        }

//        void VMPolicySmartField_PropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            try
//            {
//                switch (e.PropertyName)
//                {
//                    //SelectedPolicyLearnField.CompScheduleType
//                    case "SelectedCoverage":
//                    case "SelectedCarrier":
//                    case "SelectedPayor":

//                        //check null then call the service
//                        if (SelectedPayor != null && SelectedCarrier != null && SelectedCoverage != null)
//                        {
//                            serviceClients.CoverageClient.GetCoverageNickNameAsync(SelectedPayor.PayorID, SelectedCarrier.CarrierId, SelectedCoverage.CoverageID);
//                        }

//                        break;

//                    case "SelecetdPolicyLearnedField":
//                        if (SelecetdPolicyLearnedField == null || SelecetdPolicyLearnedField.PolicyId == Guid.Empty)
//                        {
//                            SelectedClientLrnd = null;
//                            SelectedMasterIncomingPaymentTypeLrnd = null;
//                            SelectedPaymentModeLrnd = null;
//                            return;
//                        }

//                        #region"Client pull down and comp type is displayed in smart fields "


//                        //if (DisplayedClientsLists.Count > 0)
//                        //{
//                        //    if (SelecetdPolicyLearnedField != null)
//                        //    {
//                        //        if (SelecetdPolicyLearnedField.ClientID == Guid.Empty)
//                        //        {
//                        //            SelecetdPolicyLearnedField.ClientID = DisplayedClientsLists.FirstOrDefault().ClientId;
//                        //            SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                        //        }
//                        //        else
//                        //        {
//                        //            SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                        //        }

//                        //    }
//                        //}
//                        //else
//                        //{
//                            if (SharedVMData.SelectedLicensee.LicenseeId != null)
//                            {
//                                DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
//                                //Selected client name
//                                if (SelecetdPolicyLearnedField != null)
//                                {
//                                    if (SelecetdPolicyLearnedField.ClientID == Guid.Empty)
//                                    {
//                                        SelecetdPolicyLearnedField.ClientID = DisplayedClientsLists.FirstOrDefault().ClientId;
//                                        SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                                    }
//                                    else
//                                    {
//                                        SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                                    }

//                                }
//                            }
//                        //}


//                        //if (SharedVMData.CachedClientList.ContainsKey(licenseID))
//                        //{
//                        //    try
//                        //    {
//                        //        DisplayedClientsLists = new ObservableCollection<Client>(SharedVMData.CachedClientList[licenseID]);
//                        //    }
//                        //    catch
//                        //    {
//                        //    }
//                        //}

//                        //if (DisplayedClientsLists.Count > 0)
//                        //{
//                        //    SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();

//                        //    if (SelectedClientLrnd != null)
//                        //    {
//                        //        if (SelectedClientLrnd.ClientId == Guid.Empty)
//                        //        {
//                        //            if (licenseID != null)
//                        //            {
//                        //                //Selected client name
//                        //                if (SelecetdPolicyLearnedField != null)
//                        //                {
//                        //                    SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                        //                }
//                        //            }
//                        //        }
//                        //    }
//                        //}
//                        //else
//                        //{
//                            //if (licenseID != null)
//                            //{
//                            //    DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(licenseID).OrderBy(o => o.Name).ToList());
//                            //    //Selected client name
//                            //    if (SelecetdPolicyLearnedField != null)
//                            //    {
//                            //        SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                            //    }
//                            //}

//                            //CachedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(licenseID).ToList());
//                            //if (licenseID != null)
//                            //{
//                            //    DisplayedClientsLists = new ObservableCollection<Client>(CachedClientsLists.Where(c => c.LicenseeId == licenseID).ToList());
//                            //    //Selected client name
//                            //    if (SelecetdPolicyLearnedField != null)
//                            //    {
//                            //        SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
//                            //    }
//                            //}

//                        //}

//                        SelectedMasterIncomingPaymentTypeLrnd = LearnedMasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelecetdPolicyLearnedField.CompTypeId).FirstOrDefault();
//                        SelectedPaymentModeLrnd = LrndMasterPaymentsModeData.Where(p => p.ModeId == SelecetdPolicyLearnedField.PolicyModeId).FirstOrDefault();

//                        AutoTermDate = SelecetdPolicyLearnedField.AutoTerminationDate.ToString();
//                        SelecetdPolicyLearnedField.PMC = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePMC(SelecetdPolicyLearnedField.PolicyId));
//                        SelecetdPolicyLearnedField.PAC = "$" + Convert.ToString(serviceClients.PolicyClient.CalculatePAC(SelecetdPolicyLearnedField.PolicyId));

//                        //Guid clitniID = Guid.Empty;
//                        //if (SelectedClientLrnd.ClientId == Guid.Empty)
//                        //{
//                        //    clitniID = SelecetdPolicyLearnedField.ClientID;
//                        //    SelectedClientLrnd.ClientId = clitniID;
//                        //    SelecetdPolicyLearnedField.ClientID = clitniID;
//                        //}
//                        //else
//                        //{
//                        //    clitniID = SelecetdPolicyLearnedField.ClientID;
//                        //    SelecetdPolicyLearnedField.ClientID = clitniID;
//                        //}

//                        SelectedClientLrnd = DisplayedClientsLists.FirstOrDefault();
//                        SelecetdPolicyLearnedField.ClientID = SelectedClientLrnd.ClientId;

//                        #endregion

//                        break;
//                    case "SelectedClientLrnd":
//                        //if (SelectedClientLrnd != null)
//                        //{
//                        //    if (SelectedClientLrnd.ClientId != null)
//                        //    {
//                        //        if (SelectedClientLrnd.ClientId != Guid.Empty)
//                        //        {
//                        //            SelecetdPolicyLearnedField.ClientID = SelectedClientLrnd.ClientId;
//                        //        }
//                        //        else
//                        //        {
//                        //            SelecetdPolicyLearnedField.ClientID = SelecetdPolicyLearnedField.ClientID;
//                        //        }
//                        //    }
//                        //}                       

//                        //if (SelectedClientLrnd == null)
//                        //{
//                        //    return;
//                        //}
//                        //else if (SelectedClientLrnd.ClientId == Guid.Empty)
//                        //{
//                        //    return;
//                        //}

//                        //SelecetdPolicyLearnedField.ClientID = SelectedClientLrnd.ClientId;


//                        break;
//                    case "SelectedMasterIncomingPaymentTypeLrnd":
//                        //if (SelectedMasterIncomingPaymentTypeLrnd == null) return;
//                        //SelecetdPolicyLearnedField.CompTypeId = SelectedMasterIncomingPaymentTypeLrnd.PaymentTypeId;
//                        if (SelectedMasterIncomingPaymentTypeLrnd != null)
//                        {
//                            SelecetdPolicyLearnedField.CompTypeId = SelectedMasterIncomingPaymentTypeLrnd.PaymentTypeId;
//                        }


//                        break;
//                    case "SelectedPaymentModeLrnd":
//                        //if (SelectedPaymentModeLrnd == null) return;
//                        //SelecetdPolicyLearnedField.PolicyModeId = SelectedPaymentModeLrnd.ModeId;
//                        if (SelectedPaymentModeLrnd != null)
//                        {
//                            SelecetdPolicyLearnedField.PolicyModeId = SelectedPaymentModeLrnd.ModeId;
//                        }



//                        break;
//                }
//            }
//            catch
//            {
//            }
//        }

//        #endregion

//        #region Selected Object

//        private PolicyDetailsData SelectedPolicy;

//        public void OptimizedPolicyManager_SelectedPolicyChanged(PolicyDetailsData SelectedPolicy)
//        {
//            this.SelectedPolicy = SelectedPolicy;

//            if (this.SelectedPolicy == null || this.SelectedPolicy.PolicyId == Guid.Empty)
//                SelecetdPolicyLearnedField = null;
//            else
//            {
//                try
//                {
//                    this.SelectedPolicy.LearnedFields = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);
//                    //Assign policy ID
//                    //this.SelectedPolicy.LearnedFields.PolicyId = SelectedPolicy.PolicyId;
//                    SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields;

//                    licenseID = (Guid)SelectedPolicy.PolicyLicenseeId;
//                }
//                catch
//                {
//                }

//                //if (this.SelectedPolicy.LearnedFields == null)
//                //{
//                //    // serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseAsync(SelectedPolicy.PolicyId);
//                //     this.SelectedPolicy.LearnedFields = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);

//                //}
//                //else
//                //{
//                //    SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields;
//                //}
//            }
//        }

//        void OptimizedPolicyManager_SelectedPolicySaved()
//        {
//            //check null 
//            if (SelectedPolicy != null)
//            {
//                if (this.SelectedPolicy.PolicyId != null)
//                {
//                    if (this.SelectedPolicy.PolicyId != Guid.Empty)
//                    {
//                        if (this.SelectedPolicy.LearnedFields != null)
//                        {
//                            SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields;
//                        }
//                        else
//                        {
//                            serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWiseAsync(SelectedPolicy.PolicyId);
//                        }
//                    }
//                }
//            }
//        }

//        void PolicyLearnedFieldClient_GetPolicyLearnedFieldsPolicyWiseCompleted(object sender, GetPolicyLearnedFieldsPolicyWiseCompletedEventArgs e)
//        {
//            try
//            {
//                if (e.Error == null)
//                {
//                    if (SelectedPolicy != null)
//                        SelecetdPolicyLearnedField = this.SelectedPolicy.LearnedFields = e.Result;
//                }
//            }
//            catch
//            {
//            }
//        }

//        void OptimizedPolicyManager_SelectedLicenseeChanged(LicenseeDisplayData SelectedLicensee, ObservableCollection<Client> Clients)
//        {
//            DisplayedClientsLists = Clients;
//        }
//        #endregion

//        #region Collection

//        private VMSharedData _SharedVMData;
//        public VMSharedData SharedVMData
//        {
//            get
//            {
//                if (_SharedVMData == null)
//                {
//                    _SharedVMData = VMSharedData.getInstance();
//                }

//                return _SharedVMData;
//            }
//        }

//        public ObservableCollection<Client> cachedClientsLists;
//        public ObservableCollection<Client> CachedClientsLists
//        {
//            get
//            {
//                return cachedClientsLists;
//            }
//            set
//            {
//                cachedClientsLists = value;
//                OnPropertyChanged("CachedClientsLists");

//            }
//        }

//        private ObservableCollection<Client> displayedClientsLists = new ObservableCollection<Client>();
//        public ObservableCollection<Client> DisplayedClientsLists
//        {
//            get
//            {
//                return displayedClientsLists;
//            }
//            set
//            {
//                displayedClientsLists = value;
//                OnPropertyChanged("DisplayedClientsLists");

//            }
//        }

//        private Client selectedclientlrnd;
//        public Client SelectedClientLrnd
//        {
//            get { return selectedclientlrnd == null ? new Client() : selectedclientlrnd; }
//            set { selectedclientlrnd = value; OnPropertyChanged("SelectedClientLrnd"); }
//        }


//        private PolicyLearnedFieldData _selectedPolicyLearnedField;
//        public PolicyLearnedFieldData SelecetdPolicyLearnedField
//        {
//            get
//            {
//                return _selectedPolicyLearnedField == null ? new PolicyLearnedFieldData() : _selectedPolicyLearnedField;
//            }
//            set
//            {
//                _selectedPolicyLearnedField = value;
//                OnPropertyChanged("SelecetdPolicyLearnedField");
//            }
//        }

//        private PolicyIncomingPaymentType selectedMasterIncomingPaymentTypelrnd;
//        public PolicyIncomingPaymentType SelectedMasterIncomingPaymentTypeLrnd
//        {
//            get { return selectedMasterIncomingPaymentTypelrnd == null ? new PolicyIncomingPaymentType() : selectedMasterIncomingPaymentTypelrnd; }
//            set { selectedMasterIncomingPaymentTypelrnd = value; OnPropertyChanged("SelectedMasterIncomingPaymentTypeLrnd"); }
//        }

//        private ObservableCollection<PolicyIncomingPaymentType> learnedmasterIncomingPaymentTypeLst;
//        public ObservableCollection<PolicyIncomingPaymentType> LearnedMasterIncomingPaymentTypeLst
//        {
//            get
//            {
//                return learnedmasterIncomingPaymentTypeLst;
//            }
//            set
//            {
//                learnedmasterIncomingPaymentTypeLst = value;
//                OnPropertyChanged("LearnedMasterIncomingPaymentTypeLst");
//            }
//        }

//        private PolicyMode selectedPaymentModelrnd;
//        public PolicyMode SelectedPaymentModeLrnd
//        {
//            get { return selectedPaymentModelrnd == null ? new PolicyMode() : selectedPaymentModelrnd; }
//            set { selectedPaymentModelrnd = value; OnPropertyChanged("SelectedPaymentModeLrnd"); }
//        }

//        private ObservableCollection<PolicyMode> lrndmasterPaymentsModeData;
//        public ObservableCollection<PolicyMode> LrndMasterPaymentsModeData
//        {
//            get
//            {
//                return lrndmasterPaymentsModeData;
//            }
//            set
//            {
//                lrndmasterPaymentsModeData = value;
//                OnPropertyChanged("LrndMasterPaymentsModeData");
//            }
//        }

//        private DisplayedPayor _SelectedPayor;
//        public DisplayedPayor SelectedPayor
//        {
//            get
//            {
//                return _SelectedPayor;
//            }
//            set
//            {
//                _SelectedPayor = value;
//                OnPropertyChanged("SelectedPayor");
//            }
//        }

//        private Carrier _SelectedCarrier;
//        public Carrier SelectedCarrier
//        {
//            get
//            {
//                return _SelectedCarrier;
//            }
//            set
//            {
//                _SelectedCarrier = value;
//                OnPropertyChanged("SelectedCarrier");
//            }
//        }
//        private DisplayedCoverage _SelectedCoverage;
//        public DisplayedCoverage SelectedCoverage
//        {
//            get
//            {
//                return _SelectedCoverage;
//            }
//            set
//            {
//                _SelectedCoverage = value;
//                OnPropertyChanged("SelectedCoverage");
//            }
//        }

//        private string _Compscheduletype;
//        public string Compscheduletype
//        {
//            get
//            {
//                return _Compscheduletype;
//            }
//            set
//            {
//                _Compscheduletype = value;
//                OnPropertyChanged("_Compscheduletype");
//            }
//        }

//        private decimal? calculatePMC(PolicyLearnedFieldData SelecetdPolicyLearnedField)
//        {
//            //SET @ResultVar = (@SplitPercentage * @ModalPremium * @Commission) / 10000
//            //                                                  / ( CASE
//            //                                                  WHEN @PolicyMode = 0
//            //                                                  THEN 1
//            //                                                  WHEN @PolicyMode = 1
//            //                                                  THEN 3
//            //                                                  WHEN @PolicyMode = 2
//            //                                                  THEN 6
//            //                                                  ELSE 12
//            //                                                  END 


//            decimal? splitPercetange = SelecetdPolicyLearnedField.Link2;
//            decimal? ModelPremium = SelecetdPolicyLearnedField.ModalAvgPremium;
//            decimal? commision = 100;

//            decimal? db = splitPercetange * ModelPremium * commision / 10000;

//            if (SelecetdPolicyLearnedField.PolicyModeId == 0)
//            {
//                return db;
//            }
//            else if (SelecetdPolicyLearnedField.PolicyModeId == 1)
//            {
//                return db / 3;
//            }
//            else if (SelecetdPolicyLearnedField.PolicyModeId == 2)
//            {
//                return db / 6;
//            }
//            else
//            {
//                return db / 12;
//            }
//        }


//        #endregion
//        private ServiceClients _ServiceClients;
//        private ServiceClients serviceClients
//        {
//            get
//            {
//                if (_ServiceClients == null)
//                {
//                    _ServiceClients = new ServiceClients();
//                }
//                return _ServiceClients;
//            }
//        }
//        #region Save Learned Field Command

//        private ICommand savePolicyLearnedFieldCmd;
//        public ICommand SavePolicyLearnedFieldCmd
//        {
//            get
//            {
//                if (savePolicyLearnedFieldCmd == null)
//                {
//                    savePolicyLearnedFieldCmd = new BaseCommand(x => BeforeSaveLearndField(), x => SaveLearndField());
//                }
//                return savePolicyLearnedFieldCmd;
//            }
//        }

//        private bool BeforeSaveLearndField()
//        {
//            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
//                return false;

//            //if selected policy is not saved then disable save 
//            if (SelectedPolicy.IsSavedPolicy == false) return false;

//            if (RoleManager.Role != UserRole.SuperAdmin)
//                return false;

//            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
//                return false;
//            else
//                return true;
//        }

//        private void SaveLearndField()
//        {
//            try
//            {
//                if (SelecetdPolicyLearnedField == null)
//                    return;

//                serviceClients.LearnedToPolicyPostClient.AddLearnedToPolicy(SelecetdPolicyLearnedField.PolicyId);
//                serviceClients.PolicyLearnedFieldClient.AddUpdatePolicyLearnedField(SelecetdPolicyLearnedField, SelecetdPolicyLearnedField.ProductType);
//                //--Policy History
//                serviceClients.PolicyClient.AddUpdatePolicyHistory(SelectedPolicy.PolicyId);
//                serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearned(SelectedPolicy.PolicyId);

//                ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

//                if ((SelecetdPolicyLearnedField.PreviousEffectiveDate != SelecetdPolicyLearnedField.Effective || SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId || SelecetdPolicyLearnedField.PrevoiusTrackFromDate != SelecetdPolicyLearnedField.TrackFrom))
//                {
//                    //Thread ThreadSmart = new Thread(() =>
//                    //{
//                    //    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId);
//                    //});
//                    //ThreadSmart.IsBackground = true;
//                    //ThreadSmart.Start();

//                    //Thread newThread = new Thread(new ThreadStart(ThreadFollowUpStartingPoint));
//                    //newThread.SetApartmentState(ApartmentState.STA);
//                    //newThread.IsBackground = true;
//                    //newThread.Start();   

//                    BackgroundWorker worker = new BackgroundWorker();
//                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(FollowUP_DoWork);
//                    worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(FollowUP_RunWorkerCompleted);
//                    worker.RunWorkerAsync();
//                }

//                SelecetdPolicyLearnedField.PreviousEffectiveDate = SelecetdPolicyLearnedField.Effective;
//                SelecetdPolicyLearnedField.PreviousPolicyModeid = SelecetdPolicyLearnedField.PolicyModeId;
//                SelecetdPolicyLearnedField.PrevoiusTrackFromDate = SelecetdPolicyLearnedField.TrackFrom;

//                VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(SelecetdPolicyLearnedField.PolicyId, UpdatedModule.SmartField);

//            }
//            catch
//            {
//            }
//        }

//        private void ThreadFollowUpStartingPoint()
//        {
//            VMInstances.PolicyCommissionVM.IsBusy = true;
//            serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
//            VMInstances.PolicyCommissionVM.FillFollowUpIssue();
//            System.Windows.Threading.Dispatcher.Run();
//        }

//        void FollowUP_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
//        {
//            try
//            {
//                VMInstances.PolicyCommissionVM.IsBusy = true;
//                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
//            }
//            catch
//            {
//            }
//        }

//        void FollowUP_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
//        {
//            VMInstances.PolicyCommissionVM.FillFollowUpIssue();
//        }

//        #endregion



//    }
//}
