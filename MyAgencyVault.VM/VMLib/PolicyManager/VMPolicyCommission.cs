using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using System.Windows.Input;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows;
using MyAgencyVault.VM;
using System.Collections.ObjectModel;
using MyAgencyVault.ViewModel;
using System.ComponentModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.CommonItems;
using System.Threading;
using System.IO;
using System.Data;

namespace MyAgencyVault.VMLib.PolicyManager
{
    public class VMPolicyCommission : BaseViewModel
    {
        static MastersClient objLog = new MastersClient();
        #region Constructor
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
        public VMPolicyCommission(VMOptimizePolicyManager OptimizedPolicyManager, PolicyDetailMasterData MasterData)
        {
            ParentVM = OptimizedPolicyManager;
            PropertyChanged += new PropertyChangedEventHandler(VMPolicyCommission_PropertyChanged);
            OptimizedPolicyManager.SelectedLicenseeChanged += new VMOptimizePolicyManager.SelectedLicenseeChangedEventHandler(OptimizedPolicyManager_SelectedLicenseeChanged);
            OptimizedPolicyManager.SelectedClientChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);
            OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedPolicyChangedEventHandler(OptimizedPolicyManager_SelectedPolicyChanged);
            OptimizedPolicyManager.SelectedPayorChanged += new VMOptimizePolicyManager.SelectedPayorChangedEventHandler(OptimizedPolicyManager_SelectedPayorChanged);
            OptimizedPolicyManager.SelectedCarrierChanged += new VMOptimizePolicyManager.SelectedCarrierChangedEventHandler(OptimizedPolicyManager_SelectedCarrierChanged);
            OptimizedPolicyManager.SelectedCoverageChanged += new VMOptimizePolicyManager.SelectedCoverageChangedEventHandler(OptimizedPolicyManager_SelectedCoverageChanged);
            OptimizedPolicyManager.SelectedPolicySaved += new VMOptimizePolicyManager.SelectedPolicySavedEventHandler(OptimizedPolicyManager_SelectedPolicySaved);

            serviceClients.PostUtilClient.GetPolicyPaymentEntryForCommissionDashboardCompleted += new EventHandler<GetPolicyPaymentEntryForCommissionDashboardCompletedEventArgs>(PostUtilClient_GetPolicyPaymentEntryForCommissionDashboardCompleted);
            //serviceClients.FollowupIssueClient.GetIssuesCompleted += new EventHandler<GetIssuesCompletedEventArgs>(FollowupIssueClient_GetIssuesCompleted);
            serviceClients.FollowupIssueClient.GetFewIssueForCommissionDashBoardCompleted += new EventHandler<GetFewIssueForCommissionDashBoardCompletedEventArgs>(FollowupIssueClient_GetFewIssueForCommissionDashBoardCompleted);

            IssueCategoryCollection = MasterData.IssueCategories;
            IssueStatusCollection = MasterData.IssueStatuses;
            IssueReasonCollection = MasterData.IssueReasons;
            IssueResultCollection = MasterData.IssueResults;
            //Default Status is open
            isOpenStatus = true;

        }

        void OptimizedPolicyManager_SelectedPolicySaved()
        {
            if (SelectedPolicy != null)
            {
                if (SelectedPolicy.IsSavedPolicy == true)
                {
                    FillIncomingPaymentCommissionDashBoard();
                    FillFollowUpIssue();
                }
            }
        }

        void VMPolicyCommission_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "PolicySelectedIncomingPaymentCommissionDashBoard":
                    //LoadFollowupDate();
                    CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst == null ? null : CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.Distinct().FirstOrDefault();
                    break;

                case "SelectedCommDashPayee":
                    NewPayeeToPay = "";
                    NewPayeeToPay = "Pay to ";
                    NewPayeeToPay += SelectedCommDashPayee.NickName;

                    break;

                case "isOpenStatus":

                    if (isOpenStatus)
                    {
                        FillFollowUpIssue();
                        PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId != 2).Distinct().ToList().OrderByDescending(p => p.InvoiceDate));
                    }
                    break;

                case "isCloseStatus":

                    if (isCloseStatus)
                    {   //Call to fill followup issue
                        FillFollowUpIssue();
                        // PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId == 2).Distinct().ToList().OrderByDescending(p => p.InvoiceDate));

                    }
                    break;

                case "isAllStatus":

                    if (isAllStatus)
                    {    //Call to fill followup issue
                        FillFollowUpIssue();
                        // PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>((PolicyFollowUpCommissionDashBoardLst).Distinct().ToList().OrderByDescending(p => p.InvoiceDate));
                    }
                    break;

                #region Link all policy
                case "LinkPaymentSelecetedPayor":
                    LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId));
                    if (LinkPaymentActivePoliciesLstCollection.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLstCollection[SharedVMData.SelectedLicensee.LicenseeId]);
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
                    }
                    else
                    {
                        if (!DataPopulationforLicenceeInprogress.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                        {
                            DataPopulationforLicenceeInprogress.Add(SharedVMData.SelectedLicensee.LicenseeId, false);
                        }
                        if (DataPopulationforLicenceeInprogress[SharedVMData.SelectedLicensee.LicenseeId] == false)
                        {
                            serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicyAsync(SharedVMData.SelectedLicensee.LicenseeId);

                            DataPopulationforLicenceeInprogress[SharedVMData.SelectedLicensee.LicenseeId] = true;
                        }
                    }

                    if (LinkPaymentSelectedClient.ClientId != Guid.Empty)
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.ClientId == LinkPaymentSelectedClient.ClientId)));
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));

                    }
                    if (LinkPaymentSelecetedPayor.PayorID != Guid.Empty)
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.PayorId == LinkPaymentSelecetedPayor.PayorID)));
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
                    }

                    LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.FirstOrDefault();

                    break;


                case "LinkPaymentSelectedClient":
                    LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId));
                    if (LinkPaymentActivePoliciesLstCollection.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLstCollection[SharedVMData.SelectedLicensee.LicenseeId].OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
                    }
                    else
                    {
                        if (!DataPopulationforLicenceeInprogress.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                        {
                            DataPopulationforLicenceeInprogress.Add(SharedVMData.SelectedLicensee.LicenseeId, false);
                        }
                        if (DataPopulationforLicenceeInprogress[SharedVMData.SelectedLicensee.LicenseeId] == false)
                        {
                            serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicyAsync(SharedVMData.SelectedLicensee.LicenseeId);
                            DataPopulationforLicenceeInprogress[SharedVMData.SelectedLicensee.LicenseeId] = true;
                        }
                    }

                    if (LinkPaymentSelectedClient.ClientId != Guid.Empty)
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.ClientId == LinkPaymentSelectedClient.ClientId)).OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));

                    }
                    if (LinkPaymentSelecetedPayor.PayorID != Guid.Empty)
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.PayorId == LinkPaymentSelecetedPayor.PayorID)).OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
                    }

                    LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.FirstOrDefault();

                    break;
                    #endregion
            }
        }

        #endregion

        public delegate void LinkPolicyToCommissionDashboad();
        public event LinkPolicyToCommissionDashboad LinkPolicyToCommissionDashboadWindow;//make a event to open link button popup

        private ICommand _linkCommissionDashBordToPolicy;//make a property for linkbutton page
        public ICommand LinkCommissiondashBoardToPolicy
        {
            get
            {
                if (_linkCommissionDashBordToPolicy == null)
                {
                    _linkCommissionDashBordToPolicy = new BaseCommand(x => BeforeLinkCommissiondashBoardToPolicyOpen(), x => NewLinkCommissiondashBoardToPolicyOpen());
                }
                return _linkCommissionDashBordToPolicy;
            }

        }

        private bool BeforeLinkCommissiondashBoardToPolicyOpen()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (policySelectedIncomingPaymentCommissionDashBoard == null) return false;

            if (policySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == null) return false;

            if (CommissionDashBoardOutGoingPaymentLst != null)
            {
                int intCount = CommissionDashBoardOutGoingPaymentLst.Where(p => p.IsPaid == true).Count();
                if (intCount > 0)
                {
                    return false;
                }
            }

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read ) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;

            return true;
        }


        private void NewLinkCommissiondashBoardToPolicyOpen()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == null || SelectedPolicy.IsDeleted == true) return;
            if (SelectedPolicy.IsSavedPolicy == false) return;

            if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty)
            {
                MessageBox.Show("Payor is not assign to Policy", "Information", MessageBoxButton.OK);
                return;
            }
            if (LinkPolicyToCommissionDashboadWindow != null)
            {
                AddPayor();
                AddClient();
                Getalllinkgriddata();
                LinkPolicyToCommissionDashboadWindow();
            }

        }

        private ObservableCollection<Payor> _Payor;
        public ObservableCollection<Payor> Payor
        {
            get
            {
                return _Payor == null ? new ObservableCollection<Payor>() : _Payor;
            }
            set
            {
                _Payor = value;
                OnPropertyChanged("Payor");
            }
        }

        //declare selected text variable
        private Payor _LinkPaymentSelecetedPayor;
        public Payor LinkPaymentSelecetedPayor
        {
            get
            {
                return _LinkPaymentSelecetedPayor == null ? new Payor() : _LinkPaymentSelecetedPayor;

            }

            set
            {
                _LinkPaymentSelecetedPayor = value;
                OnPropertyChanged("LinkPaymentSelecetedPayor");
            }
        }
        //add payor in listbox
        private void AddPayor()
        {
            try
            {
                Payor = new ObservableCollection<Payor>();
                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.All };
                Payor = new ObservableCollection<Payor>(serviceClients.PayorClient.GetPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo).OrderBy(p => p.PayorName));

                Payor.Add(

                           new Payor()
                           {
                               PayorName = "All",
                               PayorID = Guid.Empty,
                           }
                          );
                LinkPaymentSelecetedPayor = Payor.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();

            }
            catch
            {
            }

        }

        private ObservableCollection<Client> _LinkPaymentClientLst;
        public ObservableCollection<Client> LinkPaymentClientLst
        {
            get
            {
                return _LinkPaymentClientLst == null ? new ObservableCollection<Client>() : _LinkPaymentClientLst;
            }
            set
            {
                _LinkPaymentClientLst = value;
                OnPropertyChanged("LinkPaymentClientLst");
            }
        }

        private Client _LinkPaymentSelectedClient;
        public Client LinkPaymentSelectedClient
        {
            get
            {
                return _LinkPaymentSelectedClient == null ? new Client() : _LinkPaymentSelectedClient;
            }
            set
            {
                _LinkPaymentSelectedClient = value;
                OnPropertyChanged("LinkPaymentSelectedClient");
            }
        }

        private void AddClient()
        {
            LinkPaymentClientLst = new ObservableCollection<Client>();
            LinkPaymentClientLst = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId));
            LinkPaymentClientLst.Add(
                new Client()
                {
                    Name = "All",
                    ClientId = Guid.Empty,

                }

                );
            LinkPaymentSelectedClient = LinkPaymentClientLst.LastOrDefault();
        }

        private Dictionary<Guid, bool> _DataPopulationforLicenceeInprogress;
        private Dictionary<Guid, bool> DataPopulationforLicenceeInprogress
        {
            get
            {
                if (_DataPopulationforLicenceeInprogress == null)
                {
                    _DataPopulationforLicenceeInprogress = new Dictionary<Guid, bool>();
                }
                return _DataPopulationforLicenceeInprogress;
            }
            set
            {
                _DataPopulationforLicenceeInprogress = value;
            }
        }

        private Dictionary<Guid, ObservableCollection<LinkPaymentPolicies>> _LinkPaymentActivePoliciesLstCollection;
        private Dictionary<Guid, ObservableCollection<LinkPaymentPolicies>> LinkPaymentActivePoliciesLstCollection
        {
            get
            {
                if (_LinkPaymentActivePoliciesLstCollection == null)
                {
                    _LinkPaymentActivePoliciesLstCollection = new Dictionary<Guid, ObservableCollection<LinkPaymentPolicies>>();
                }
                return _LinkPaymentActivePoliciesLstCollection;
            }
            set
            {
                _LinkPaymentActivePoliciesLstCollection = value;
            }
        }

        private ObservableCollection<LinkPaymentPolicies> _LinkPaymentActivePoliciesLst;
        public ObservableCollection<LinkPaymentPolicies> LinkPaymentActivePoliciesLst
        {
            get
            {
                return _LinkPaymentActivePoliciesLst == null ? new ObservableCollection<LinkPaymentPolicies>() : _LinkPaymentActivePoliciesLst;
            }
            set
            {
                _LinkPaymentActivePoliciesLst = value;
                OnPropertyChanged("LinkPaymentActivePoliciesLst");
            }
        }


        private LinkPaymentPolicies _LinkPaymentSelectedActivePolicies;
        public LinkPaymentPolicies LinkPaymentSelectedActivePolicies
        {
            get
            {
                return _LinkPaymentSelectedActivePolicies == null ? new LinkPaymentPolicies() : _LinkPaymentSelectedActivePolicies;
            }
            set
            {
                _LinkPaymentSelectedActivePolicies = value;
                OnPropertyChanged("LinkPaymentSelectedActivePolicies");
            }
        }

        public void Getalllinkgriddata()
        {
            LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>();
            LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId));
            LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.PayorId == LinkPaymentSelecetedPayor.PayorID)));
            LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
            LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.FirstOrDefault();
        }
        #region Selected Object

        private DisplayedPayor SelectedPayor;
        private Carrier SelectedCarrier;
        //private Coverage SelectedCoverage;

        private PolicyDetailsData SelectedPolicy;
        private Client SelectedClient;
        private LicenseeDisplayData SelectedLicensee;
        private VMSharedData _SharedVMData;
        bool _hasPayments;
        public bool HasPayments
        {
            get
            {
                return _hasPayments;
            }
            set
            {
                _hasPayments = value;
                OnPropertyChanged("HasPayments");
            }
        }
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
        private VMOptimizePolicyManager ParentVM;

        void OptimizedPolicyManager_SelectedLicenseeChanged(LicenseeDisplayData SelectedLicensee, ObservableCollection<Client> Clients)
        {
            this.SelectedLicensee = SelectedLicensee;
        }

        void OptimizedPolicyManager_SelectedClientChanged(Client SelectedClient)
        {
            this.SelectedClient = SelectedClient;
        }

        public void OptimizedPolicyManager_SelectedPolicyChanged(PolicyDetailsData SelectedPolicy)
        {
            this.SelectedPolicy = SelectedPolicy;
            this.HasPayments = false;
            if (this.SelectedPolicy == null || this.SelectedPolicy.PolicyId == Guid.Empty)
            {
                PolicyIncomingPaymentCommissionDashBoard = null;
                CommissionDashBoardOutGoingPaymentLst = null;
                PolicyFollowUpCommissionDashBoardLst = null;
            }
            else
            {
                if (SelectedPolicy.IsSavedPolicy == true)
                {
                    FillIncomingPaymentCommissionDashBoard();
                    FillFollowUpIssue();
                }
            }
            //Added by Acme - Sep 13, 2018 when export option added on system
            HasPayments = (SelectedPolicy.policyPaymentEntries != null && SelectedPolicy.policyPaymentEntries.Count > 0) ? true : false;
        }

        void OptimizedPolicyManager_SelectedPayorChanged(DisplayedPayor SelectedPayor)
        {
            this.SelectedPayor = SelectedPayor;
        }

        void OptimizedPolicyManager_SelectedCarrierChanged(Carrier SelectedCarrier)
        {
            this.SelectedCarrier = SelectedCarrier;
        }

        void OptimizedPolicyManager_SelectedCoverageChanged(DisplayedCoverage SelectedCoverage)
        {
            //this.SelectedCoverage = SelectedCoverage;
        }

        #endregion

        #region Issue Collection

        private ObservableCollection<IssueCategory> _IssueCategoryCollection;
        public ObservableCollection<IssueCategory> IssueCategoryCollection
        {
            get { return _IssueCategoryCollection; }
            set
            {
                _IssueCategoryCollection = value;
                OnPropertyChanged("IssueCategoryCollection");
            }
        }

        private ObservableCollection<IssueStatus> _IssueStatusCollection;
        public ObservableCollection<IssueStatus> IssueStatusCollection
        {
            get { return _IssueStatusCollection; }
            set
            {
                _IssueStatusCollection = value;
                OnPropertyChanged("IssueStatusCollection");
            }
        }


        private ObservableCollection<IssueReasons> _IssueReasonCollection;
        public ObservableCollection<IssueReasons> IssueReasonCollection
        {
            get { return _IssueReasonCollection; }
            set
            {
                _IssueReasonCollection = value;
                OnPropertyChanged("IssueReasonCollection");
            }
        }

        private ObservableCollection<IssueResults> _IssueResultCollection;
        public ObservableCollection<IssueResults> IssueResultCollection
        {
            get { return _IssueResultCollection; }
            set
            {
                _IssueResultCollection = value;
                OnPropertyChanged("IssueResultCollection");
            }
        }

        #endregion

        #region CommissionDashBoard

        #region CommissionDashBoardVM
        public CommissionDashBoardVM _CommissionDashBoardVM;
        #endregion

        /// <summary>
        ///    
        /// </summary>
        private ICommand _newCommissionDashBoardIncomingpayment;
        public ICommand NewCommissionDashBoardIncomingpayment
        {
            get
            {
                if (_newCommissionDashBoardIncomingpayment == null)
                {
                    _newCommissionDashBoardIncomingpayment = new BaseCommand(x => BeforeNewCommissionDashBoardIncomingpaymentOpen(), x => NewCommissionDashBoardIncomingpaymentOpen());
                }
                return _newCommissionDashBoardIncomingpayment;
            }
        }

        private bool BeforeNewCommissionDashBoardIncomingpaymentOpen()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;

            return true;
        }
        private string _CompensationPaidToAgentA;
        public string CompensationPaidToAgentA
        {
            get
            {
                return _CompensationPaidToAgentA;
            }
            set
            {
                _CompensationPaidToAgentA = value;
                OnPropertyChanged("CompensationPaidToAgentA");
            }
        }
        private string _ReverseFromAgentA;
        public string ReverseFromAgentA
        {
            get
            {
                return _ReverseFromAgentA;
            }
            set
            {
                _ReverseFromAgentA = value;
                OnPropertyChanged("ReverseFromAgentA");
            }
        }

        private string _NewPayeeToPay;
        public string NewPayeeToPay
        {
            get
            {
                return _NewPayeeToPay;
            }
            set
            {
                _NewPayeeToPay = value;
                OnPropertyChanged("NewPayeeToPay");
            }
        }

        private void NewCommissionDashBoardIncomingpaymentOpen()
        {
            if (objLog == null) objLog = new MastersClient();
            objLog.AddLog(DateTime.Now.ToString() + " NewCommissionDashBoardIncomingpaymentOpen eneterd , user: " + RoleManager.LoggedInUser);

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty || SelectedPolicy.IsDeleted == true) return;
            if (SelectedPolicy.IsSavedPolicy == false) return;

            if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty)
            {
                objLog.AddLog(DateTime.Now.ToString() + " NewCommissionDashBoardIncomingpaymentOpen return, payor null , user: " + RoleManager.LoggedInUser);

                MessageBox.Show("Payor is not assign to Policy", "Information", MessageBoxButton.OK);
                return;

            }
            _CommissionDashBoardVM.IsEditEnable = true; //Added by acme 

            SharedVMData.UpdateMode = UpdateMode.Add;
            PolicyPaymentEntriesPost _tPolicyPaymentEntriesPost = _CommissionDashBoardVM.Show();

            SharedVMData.UpdateMode = UpdateMode.None;
            if (_tPolicyPaymentEntriesPost.PaymentEntryID == Guid.Empty) return;
            if (PolicyIncomingPaymentCommissionDashBoard == null)
            {
                PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>();
            }
            SharedVMData.isLoadIncomingPayment = true;

            FillIncomingPaymentCommissionDashBoard();

            //vinod
            // VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.CommissionDashBoard);

            FillFollowUpIssue();
            DistinctCalculation(PolicyFollowUpCommissionDashBoardLst);


            objLog.AddLog(DateTime.Now.ToString() + " NewCommissionDashBoardIncomingpaymentOpen success");

        }

        private ICommand _editCommissionDashBoardIncomingPayment;
        public ICommand EditCommissionDashBoardIncomingPayment
        {
            get
            {
                if (_editCommissionDashBoardIncomingPayment == null)
                {
                    _editCommissionDashBoardIncomingPayment = new BaseCommand(x => BeforeEditCommissionDashBoardIncomingpaymentOpen(), x => EditCommissionDashBoardIncomingpaymentOpen());
                }
                return _editCommissionDashBoardIncomingPayment;
            }
        }

        private bool BeforeEditCommissionDashBoardIncomingpaymentOpen()
        {

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (PolicySelectedIncomingPaymentCommissionDashBoard == null) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read ) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;
            return true;
        }

        private void EditCommissionDashBoardIncomingpaymentOpen()
        {
            try
            {
                _CommissionDashBoardVM.IsEditEnable = true; //always true by default
                if (PolicySelectedIncomingPaymentCommissionDashBoard != null)
                {
                    if (!serviceClients.DeuClient.IsPaymentFromCommissionDashBoardByPaymentEntryId(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID))
                    {
                        //MessageBox.Show("It is Deu entered payment");
                        // Acme - Oct 23, 2017 enhancement - Check if it is super, user then allow editing invoice date - 
                       // if (RoleManager.Role == UserRole.SuperAdmin)
                        if(RoleManager.Role == UserRole.SuperAdmin || RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Write)
                        {
                            _CommissionDashBoardVM.IsEditEnable = false;
                        }
                        //else
                        //{
                        //    MessageBox.Show("It is Deu entered payment");
                        //    return;
                        //}

                    }
                }

                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty || SelectedPolicy.IsDeleted == true) return;
                //if selected policy is not saved then edit button will disabled
                if (SelectedPolicy.IsSavedPolicy == false) return;

                if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty || SelectedPolicy.CarrierID == null || SelectedPolicy.CarrierID == Guid.Empty ||
                   SelectedPolicy.CoverageId == null || SelectedPolicy.CoverageId == Guid.Empty)
                {
                    MessageBox.Show("Payor/Carrier/Coverage is not assign to Policy", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;

                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard == null)
                {
                    MessageBox.Show("No payment selected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                SharedVMData.UpdateMode = UpdateMode.Edit;
                SharedVMData.isLoadIncomingPayment = true;
                PolicyPaymentEntriesPost _tPolicyPaymentEntriesPost = _CommissionDashBoardVM.Show(PolicySelectedIncomingPaymentCommissionDashBoard);
                if (_tPolicyPaymentEntriesPost == null || _tPolicyPaymentEntriesPost.PaymentEntryID == Guid.Empty) return;
                PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
                //PolicyIncomingPaymentCommissionDashBoard.Add(_tPolicyPaymentEntriesPost);
                SelectedPolicy.policyPaymentEntries.Add(_tPolicyPaymentEntriesPost);
                PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>(PolicyIncomingPaymentCommissionDashBoard.OrderByDescending(p => p.InvoiceDate));
                PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();

                FillIncomingPaymentCommissionDashBoard();
                FillFollowUpIssue();
                SharedVMData.UpdateMode = UpdateMode.None;

                if (SelectedPolicy != null)
                    VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.CommissionDashBoard);

            }
            catch
            {
            }

        }

        private ICommand _removeCommissionDashBoardIncomingPayment;
        public ICommand RemoveCommissionDashBoardIncomingPayment
        {
            get
            {
                if (_removeCommissionDashBoardIncomingPayment == null)
                {
                    _removeCommissionDashBoardIncomingPayment = new BaseCommand(x => BeforeRemoveCommIncomingPayment(), x => RemoveCommIncomingPayment());
                }
                return _removeCommissionDashBoardIncomingPayment;
            }
        }

        private bool BeforeRemoveCommIncomingPayment()
        {

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (PolicyIncomingPaymentCommissionDashBoard == null || PolicyIncomingPaymentCommissionDashBoard.Count == 0)
            {
                return false;
            }
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read ) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;

            return true;
        }

        private void RemoveCommIncomingPayment()
        {
            if (objLog == null) objLog = new MastersClient();
            if (PolicySelectedIncomingPaymentCommissionDashBoard != null)
            {
                if (!serviceClients.DeuClient.IsPaymentFromCommissionDashBoardByPaymentEntryId(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID))
                {
                    objLog.AddLog(DateTime.Now.ToString() + " RemoveCommIncomingPayment: Alert shown for Deu entered payment");
                    MessageBox.Show("It is Deu entered payment");
                    return;
                }
            }
            bool flag = false;
            try
            {
                objLog.AddLog(DateTime.Now.ToString() + " RemoveCommIncomingPayment: Allowed deletion, showing prompt");

                MessageBoxResult _result = MessageBox.Show("Do you want to delete the payment?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (_result == MessageBoxResult.Cancel)
                {
                    return;
                }
                string logMsg = "Manual deletion of incoming payment from comm dashboard, PaymentEntryID : " + PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID + ",  loggedInUser: " + RoleManager.LoggedInUser;
                objLog.AddLog(logMsg);
                MyAgencyVault.VM.MyAgencyVaultSvc.PostProcessReturnStatus _PostProcessReturnStatus = null;
                using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PostUtilClient.InnerChannel))
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                    _PostProcessReturnStatus = serviceClients.PostUtilClient.RemoveCommissiondashBoardIncomingPayment(PolicySelectedIncomingPaymentCommissionDashBoard, RoleManager.Role);
                }

                flag = _PostProcessReturnStatus.IsComplete;
                if (!flag && _PostProcessReturnStatus.ErrorMessage == MessageConst.LockErrorMessage)
                {
                    MessageBox.Show(_PostProcessReturnStatus.ErrorMessage + ", " + "Please try after some time", "Information !", MessageBoxButton.OK);
                    return;
                }
                if (flag)
                {
                    PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
                    PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>(PolicyIncomingPaymentCommissionDashBoard.OrderByDescending(p => p.InvoiceDate));
                    PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();

                    if (SelectedPolicy != null)
                        VMInstances.OptimizedPolicyManager.UpDatePolicyDetailSmartFiled(SelectedPolicy.PolicyId, UpdatedModule.CommissionDashBoard);
                    else
                    {
                        VMInstances.OptimizedPolicyManager.UpdateClient(Guid.Empty);
                    }

                    FillFollowUpIssue();
                    MessageBox.Show("Removed successfully.", "Infomation", MessageBoxButton.OK, MessageBoxImage.Information);
                    objLog.AddLog("Payment entry deleted successfully");

                }
                else
                {
                    MessageBox.Show("Payment cannot be removed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("Payment entry deletion Exception: " + ex.Message);
            }
        }


        private ICommand _unlinkCommissionDashBoardIncomingPayment;
        public ICommand UnlinkCommissionDashBoardIncomingPayment
        {
            get
            {
                if (_unlinkCommissionDashBoardIncomingPayment == null)
                {
                    _unlinkCommissionDashBoardIncomingPayment = new BaseCommand(x => BeforeUnlinkCommIncomingPayment(), x => UnlinkCommIncomingPayment());
                }
                return _unlinkCommissionDashBoardIncomingPayment;
            }
        }

        private bool BeforeUnlinkCommIncomingPayment()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId == Guid.Empty) return false;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)) 
                return false;

            return true;
        }

        private void UnlinkCommIncomingPayment()
        {
            if (objLog == null) objLog = new MastersClient();
            objLog.AddLog(DateTime.Now.ToString() + " Unlink entered" + ", user: " + RoleManager.LoggedInUser);

            try
            {

                if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId == Guid.Empty) return;
                //"Payment cannot be unlink.because it is entered by commision dashboard"
                //if (serviceClients.DeuClient.GetDeuEntryidWise(PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId.Value).IsEntrybyCommissiondashBoard == true) return;
                if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty) return;
                objLog.AddLog(DateTime.Now.ToString() + " Unlink request: Payment -" + PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID + ", Policy - " + PolicySelectedIncomingPaymentCommissionDashBoard.PolicyID + ", user: " + RoleManager.LoggedInUser);
                bool flag = false;
                // using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PostUtilClient.InnerChannel))
                //{

                //serviceClients.PostUtilClient.ChannelFactory.Endpoint.Behaviors.Add(new UsernameClientBehavior());
                //var chanel = serviceClients.PostUtilClient.ChannelFactory.CreateChannel();
                //   System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                flag = serviceClients.PostUtilClient.UnlinkCommissiondashBoardIncomingPayment(PolicySelectedIncomingPaymentCommissionDashBoard, RoleManager.Role).IsComplete;// chanel.UnlinkCommissiondashBoardIncomingPayment(PolicySelectedIncomingPaymentCommissionDashBoard, RoleManager.Role).IsComplete; //
                                                                                                                                                                             //    serviceClients.PostUtilClient.ChannelFactory.Endpoint.Behaviors.Clear();
                                                                                                                                                                             //}

                if (flag)
                {
                    PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
                    PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>(PolicyIncomingPaymentCommissionDashBoard.OrderByDescending(p => p.InvoiceDate));
                    PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();

                    Guid ClienId = SelectedPolicy.ClientId ?? Guid.Empty;
                    VMInstances.OptimizedPolicyManager.UpdateClient(ClienId);
                    objLog.AddLog(DateTime.Now.ToString() + " Unlink request success");
                    MessageBox.Show("Unlink successful", "Infomation", MessageBoxButton.OK, MessageBoxImage.Information);
                    //refresh client 
                    if (SelectedLicensee != null)
                    {
                        VMInstances.OptimizedPolicyManager.DisplayedClientsLists = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SelectedLicensee.LicenseeId).OrderBy(o => o.Name).ToList());
                        if (VMInstances.OptimizedPolicyManager.DisplayedClientsLists.Count > 0)
                        {
                            if (ClienId != null)
                            {
                                if (ClienId != new Guid())
                                {
                                    VMInstances.OptimizedPolicyManager.SelectedDisplayClient = VMInstances.OptimizedPolicyManager.DisplayedClientsLists.Where(p => p.ClientId == ClienId).FirstOrDefault();
                                }
                            }

                        }
                    }

                }
                else
                {
                    MessageBox.Show("Payment cannot be unlink", "warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(DateTime.Now.ToString() + " Unlink request exception: " + ex.Message);
            }
        }

        private ICommand _ViewBatchFile;
        public ICommand ViewBatchFile
        {
            get
            {
                if (_ViewBatchFile == null)
                {
                    _ViewBatchFile = new BaseCommand(param => BeforeInComingTabViewFiles(), param => OnInComingTabViewFiles());
                }
                return _ViewBatchFile;
            }

        }

        private ICommand _ExportPayments;
        public ICommand ExportPayments
        {
            get
            {
                if (_ExportPayments == null)
                {
                    _ExportPayments = new BaseCommand(param => OnExportPayments());
                }
                return _ExportPayments;
            }

        }

        private bool BeforeInComingTabViewFiles()
        {
            bool bValue = true;
            if (SelectedPolicy != null)
                bValue = true;
            else
                bValue = false;

            return bValue;
        }
        private AutoResetEvent autoResetEvent;

        #region export payments

        public static void OnExportPayments()
        {
        }
        public DataTable ConvertToDataTable<T>(IList<T> data)

        {

            PropertyDescriptorCollection properties =

            TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)

                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)

            {

                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)

                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);

            }

            return table;

        }

        #endregion

        #region"Show view file on incoming tab in follw up manager"
        private void OnInComingTabViewFiles()
        {
            try
            {
                WebDevPath webDevPath = null;
                string key = "WebDevPath";
                string KeyValue = string.Empty;
                string strFileName = string.Empty;
                string RemotePath = string.Empty;

                //get uploaded files name

                if (PolicySelectedIncomingPaymentCommissionDashBoard == null)
                {
                    return;
                }

                Statement statementvalue = serviceClients.StatementClient.GetFindStatement(PolicySelectedIncomingPaymentCommissionDashBoard.StmtNumber);
                if (statementvalue.BatchId != null)
                    strFileName = serviceClients.BatchClient.BatchNameById(statementvalue.BatchId);
                bool bvalue = true;

                if (!string.IsNullOrEmpty(strFileName))
                {
                    if (strFileName.ToLower().Contains(".pdf"))
                    {
                        //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return;
                        bvalue = false;
                    }
                    else if (strFileName.ToLower().Contains(".xlsx"))
                    {
                        //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return; 
                        bvalue = false;

                    }
                    else if (strFileName.ToLower().Contains(".xls"))
                    {
                        //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return;
                        bvalue = false;
                    }
                    else if (strFileName.ToLower().Contains(".csv"))
                    {
                        //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return;
                        bvalue = false;
                    }
                    else if (strFileName.ToLower().Contains(".txt"))
                    {
                        //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return;
                        bvalue = false;
                    }
                    if (bvalue)
                    {
                        MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                }
                else
                {
                    MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SharedVMData.MasterSystemConstants.ContainsKey(key))
                {
                    KeyValue = SharedVMData.MasterSystemConstants[key];
                }
                {
                    KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                    //SharedVMData.MasterSystemConstants.Add(key, KeyValue);
                }
                webDevPath = WebDevPath.GetWebDevPath(KeyValue);

                Mouse.OverrideCursor = Cursors.Wait;
                autoResetEvent = new AutoResetEvent(false);

                FileUtility ObjDownload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);

                string localPath = Path.Combine(System.IO.Path.GetTempPath(), Path.GetFileName(strFileName));

                string strFileExtension = System.IO.Path.GetExtension(strFileName);

                if (strFileExtension.ToLower().Contains("pdf"))
                {
                    RemotePath = "/UploadBatch/" + strFileName;
                }
                else
                {
                    RemotePath = "/UploadBatch/Import/Success/" + strFileName;
                }

                // string RemotePath = "/UploadBatch/" + strFileName;

                ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
                ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
                ObjDownload.Download(RemotePath, localPath);

                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch
            {
            }
        }
        #endregion

        void ObjDownload_ErrorOccured(Exception error)
        {
            MessageBox.Show("There is some problem in viewing the file.please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
        {
            if (statusCode.ToString().StartsWith("20"))
            {
                System.Diagnostics.Process.Start(localFilePath);
            }
            else
            {
                MessageBox.Show("There is some problem in viewing the file.please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PolicyPaymentEntriesPost policySelectedIncomingPaymentCommissionDashBoard;
        public PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard
        {
            get { return policySelectedIncomingPaymentCommissionDashBoard; }
            set
            {
                policySelectedIncomingPaymentCommissionDashBoard = value;
                OnPropertyChanged("PolicySelectedIncomingPaymentCommissionDashBoard");
            }
        }

        private ObservableCollection<PolicyPaymentEntriesPost> policyIncomingPaymentCommissionDashBoard;
        public ObservableCollection<PolicyPaymentEntriesPost> PolicyIncomingPaymentCommissionDashBoard
        {
            get { return policyIncomingPaymentCommissionDashBoard; }
            set
            {
                policyIncomingPaymentCommissionDashBoard = value;
                OnPropertyChanged("PolicyIncomingPaymentCommissionDashBoard");
            }
        }

        private PolicyOutgoingDistribution policySelecetdOutgoingPaymentCommissionDashBoard;
        public PolicyOutgoingDistribution PolicySelecetdOutgoingPaymentCommissionDashBoard
        {
            get { return policySelecetdOutgoingPaymentCommissionDashBoard; }
            set
            {
                policySelecetdOutgoingPaymentCommissionDashBoard = value;
                OnPropertyChanged("PolicySelecetdOutgoingPaymentCommissionDashBoard");
            }
        }


        private ObservableCollection<PolicyOutgoingDistribution> policyOutgoingPaymentCommissionDashBoard;
        public ObservableCollection<PolicyOutgoingDistribution> PolicyOutgoingPaymentCommissionDashBoard
        {
            get { return policyOutgoingPaymentCommissionDashBoard; }
            set
            {
                policyOutgoingPaymentCommissionDashBoard = value;
                OnPropertyChanged("PolicyOutgoingPaymentCommissionDashBoard");
            }
        }

        private DisplayFollowupIssue followUpSelectedIssuesCommissionDashBoard;
        public DisplayFollowupIssue FollowUpSelectedIssuesCommissionDashBoard
        {
            get { return followUpSelectedIssuesCommissionDashBoard; }
            set
            {
                followUpSelectedIssuesCommissionDashBoard = value;
                OnPropertyChanged("FollowUpSelectedIssuesCommissionDashBoard");
            }
        }

        private ObservableCollection<DisplayFollowupIssue> followUpIssuesCommissionDashBoard;
        public ObservableCollection<DisplayFollowupIssue> FollowUpIssuesCommissionDashBoard
        {
            get { return followUpIssuesCommissionDashBoard; }
            set
            {
                followUpIssuesCommissionDashBoard = value;
                OnPropertyChanged("FollowUpIssuesCommissionDashBoard");
            }
        }

        private void FillIncomingPaymentCommissionDashBoard()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                PolicyIncomingPaymentCommissionDashBoard = null;
                PolicySelectedIncomingPaymentCommissionDashBoard = null;
                return;
            }

            if (SelectedPolicy.policyPaymentEntries == null || SelectedPolicy.policyPaymentEntries.Count == 0)
            {
                serviceClients.PostUtilClient.GetPolicyPaymentEntryForCommissionDashboardAsync(SelectedPolicy.PolicyId);
            }
            else
            {
                if (SharedVMData.isLoadIncomingPayment)
                {
                    serviceClients.PostUtilClient.GetPolicyPaymentEntryForCommissionDashboardAsync(SelectedPolicy.PolicyId);
                    SharedVMData.isLoadIncomingPayment = false;
                }

                PolicyIncomingPaymentCommissionDashBoard = SelectedPolicy.policyPaymentEntries;
            }
            if (PolicyIncomingPaymentCommissionDashBoard != null)
            {
                PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>(PolicyIncomingPaymentCommissionDashBoard.OrderByDescending(p => p.InvoiceDate));
                PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
            }


        }

        void PostUtilClient_GetPolicyPaymentEntryForCommissionDashboardCompleted(object sender, GetPolicyPaymentEntryForCommissionDashboardCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (SelectedPolicy != null)
                    {
                        PolicyIncomingPaymentCommissionDashBoard = SelectedPolicy.policyPaymentEntries = e.Result;
                        PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>(PolicyIncomingPaymentCommissionDashBoard.OrderByDescending(p => p.InvoiceDate));
                        PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
                    }
                }
                //Added by Acme - Sep 13, 2018 when export option added on system
                HasPayments = (SelectedPolicy.policyPaymentEntries != null && SelectedPolicy.policyPaymentEntries.Count > 0) ? true : false;
            }
            catch
            {
            }
        }

        private ObservableCollection<PolicyOutgoingDistribution> FillOutgoingPaymentCommissionDashBoard()
        {
            if (PolicyIncomingPaymentCommissionDashBoard.Count == 0) return null;
            ObservableCollection<PolicyOutgoingDistribution> _PolicyOutgoingDistribution =
                serviceClients.PostUtilClient.
                GetPolicyOutgoingPaymentForCommissionDashboard(PolicyIncomingPaymentCommissionDashBoard.First().PaymentEntryID);
            return _PolicyOutgoingDistribution;
        }
        private ObservableCollection<DisplayFollowupIssue> FillFollowUpIssueCommissiondashBoard()
        {
            ObservableCollection<DisplayFollowupIssue> _FollowupIssue =
                serviceClients.PostUtilClient.GetPolicyCommissionIssuesForCommissionDashboard(SelectedPolicy.PolicyId);
            return _FollowupIssue;
        }
        public delegate void OpenReverseCommissionDashBoard();
        public event OpenReverseCommissionDashBoard OpenReverseCommissionDashBoardEvent;

        public delegate void CloseReverseCommissionDashBoard();
        public event CloseReverseCommissionDashBoard CloseReverseCommissionDashBoardEvent;

        private ICommand commissionDashBoardReverse;
        public ICommand CommissionDashBoardReverse
        {
            get
            {
                if (commissionDashBoardReverse == null)
                {
                    commissionDashBoardReverse = new BaseCommand(x => BeforeOpenReverseThePayment(), x => OpenReverseThePayment());
                }
                return commissionDashBoardReverse;
            }
        }

        private bool BeforeOpenReverseThePayment()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read ) // || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;

            return true;
        }

        private void OpenReverseThePayment()
        {
            ShowCommDashReverseWindow();
        }
        private void ShowCommDashReverseWindow()
        {
            try
            {
                if (OpenReverseCommissionDashBoardEvent != null)
                {
                    if (CommissionDashBoardSelecetdOutGoingPaymentLst == null || CommissionDashBoardSelecetdOutGoingPaymentLst.OutgoingPaymentId == Guid.Empty) return;
                    CompensationPaidToAgentA = "Compensation Paid to Agent " + CommissionDashBoardSelecetdOutGoingPaymentLst.NickName;
                    ReverseFromAgentA = "Reverse From Agent " + CommissionDashBoardSelecetdOutGoingPaymentLst.NickName;
                    CommDashPayeeLst = FillCommissionDashPayeeUser();
                    SelectedCommDashPayee = CommDashPayeeLst.FirstOrDefault();
                    OpenReverseCommissionDashBoardEvent();
                }
            }
            catch
            {
            }
        }
        private ICommand updateReverseCommDashPayee;
        public ICommand UpdateReverseCommDashPayee
        {
            get
            {
                if (updateReverseCommDashPayee == null)
                {
                    updateReverseCommDashPayee = new BaseCommand(x => UpdateActionReverseCommDashPayee());
                }
                return updateReverseCommDashPayee;
            }
        }

        private ICommand cancleCommDashPayee;
        public ICommand CancleCommDashPayee
        {
            get
            {
                if (cancleCommDashPayee == null)
                {
                    cancleCommDashPayee = new BaseCommand(x => CalcelActionReverseCommDashPayee());
                }
                return cancleCommDashPayee;
            }

        }

        private void CalcelActionReverseCommDashPayee()
        {
            if (CloseReverseCommissionDashBoardEvent != null)
            {
                CloseReverseCommissionDashBoardEvent();
            }
        }
        private void UpdateActionReverseCommDashPayee()
        {
            if (objLog == null) objLog = new MastersClient();
            objLog.AddLog(DateTime.Now.ToString() + " UpdateActionReverseCommDashPayee entered: user - " + RoleManager.LoggedInUser);
            try
            {
                if (AmountToReverse == 0) return;
                if (SelectedCommDashPayee == null || SelectedCommDashPayee.UserCredentialID == Guid.Empty)
                {
                    return;
                }
                objLog.AddLog("UpdateActionReverseCommDashPayee : PaymentID - " + CommissionDashBoardSelecetdOutGoingPaymentLst.PaymentEntryId + ", recipient - " + CommissionDashBoardSelecetdOutGoingPaymentLst.RecipientUserCredentialId + ", user - " + RoleManager.LoggedInUser);

                double remain = (CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount * AmountToReverse / 100) ?? 0;
                PolicyOutgoingDistribution policyoutgoingdis = new PolicyOutgoingDistribution();
                policyoutgoingdis.CreatedOn = DateTime.Today;

                policyoutgoingdis.IsPaid = false;
                policyoutgoingdis.OutgoingPaymentId = Guid.NewGuid();
                policyoutgoingdis.PaidAmount = remain;
                policyoutgoingdis.PaymentEntryId = CommissionDashBoardSelecetdOutGoingPaymentLst.PaymentEntryId;
                //  policyoutgoingdis.ReverseOutgoingPaymentId = CommissionDashBoardSelecetdOutGoingPaymentLst.OutgoingPaymentId;
                policyoutgoingdis.RecipientUserCredentialId = SelectedCommDashPayee.UserCredentialID;

                using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PolicyOutgoingDistributionClient.InnerChannel))
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                    serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntry(policyoutgoingdis);
                }

                //serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntryAsync(policyoutgoingdis);

                policyoutgoingdis = new PolicyOutgoingDistribution();
                policyoutgoingdis.CreatedOn = DateTime.Today;
                policyoutgoingdis.IsPaid = false;
                policyoutgoingdis.OutgoingPaymentId = Guid.NewGuid();
                policyoutgoingdis.PaidAmount = (-1) * remain;
                policyoutgoingdis.PaymentEntryId = CommissionDashBoardSelecetdOutGoingPaymentLst.PaymentEntryId;
                policyoutgoingdis.RecipientUserCredentialId = CommissionDashBoardSelecetdOutGoingPaymentLst.RecipientUserCredentialId;
                // policyoutgoingdis.ReverseOutgoingPaymentId = CommissionDashBoardSelecetdOutGoingPaymentLst.OutgoingPaymentId;
                using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PolicyOutgoingDistributionClient.InnerChannel))
                {
                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                    serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntry(policyoutgoingdis);
                }
                //serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntry(policyoutgoingdis);

                //serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntryAsync(policyoutgoingdis);

                if (CloseReverseCommissionDashBoardEvent != null)
                {
                    CloseReverseCommissionDashBoardEvent();
                }
                //Himla ankita
                //CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                //CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();

                try
                {
                    //Update batch after reverse the payment
                    string strBatchNumber = PolicySelectedIncomingPaymentCommissionDashBoard.BatchNumber;
                    if (!string.IsNullOrEmpty(strBatchNumber))
                    {
                        if (strBatchNumber.Contains('/'))
                        {
                            string[] arBatch = strBatchNumber.Split('/');
                            int intBatchNumber = Convert.ToInt32(arBatch[0]);
                            //serviceClients.BatchClient.SetBatchToUnPaidStatus(intBatchNumber);
                            serviceClients.BatchClient.SetBatchToUnPaidStatusAsync(intBatchNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    objLog.AddLog(DateTime.Now.ToString() + " UpdateActionreverse exception: " + ex.Message);
                }
                //Load data into grid
                CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                objLog.AddLog(DateTime.Now.ToString() + " UpdateActionreverse success ");

            }
            catch (Exception ex)
            {
                objLog.AddLog(DateTime.Now.ToString() + " UpdateActionreverse exception: " + ex.Message);
            }
        }

        private ObservableCollection<PolicyOutgoingDistribution> _commissionDashBoardSelectedOutgoingPaymentLst;
        public ObservableCollection<PolicyOutgoingDistribution> CommissionDashBoardSelectedOutgoingPaymentLst
        {
            get
            {
                return _commissionDashBoardSelectedOutgoingPaymentLst;
            }
            set
            {
                _commissionDashBoardSelectedOutgoingPaymentLst = value;
                OnPropertyChanged("CommissionDashBoardSelectedOutgoingPaymentLst");
            }
        }

        private double _amountToReverse;
        public double AmountToReverse
        {
            get
            {
                return _amountToReverse;
            }
            set
            {
                _amountToReverse = value;
                OnPropertyChanged("AmountToReverse");
            }
        }
        private User _selectedCommDashPayee;
        public User SelectedCommDashPayee
        {
            get
            {
                return _selectedCommDashPayee == null ? new User() : _selectedCommDashPayee;
            }
            set
            {
                _selectedCommDashPayee = value;
                OnPropertyChanged("SelectedCommDashPayee");
            }
        }
        private ObservableCollection<User> _commDashPayeeLst;
        public ObservableCollection<User> CommDashPayeeLst
        {
            get
            {
                return _commDashPayeeLst;
            }
            set
            {
                _commDashPayeeLst = value;
                OnPropertyChanged("CommDashPayeeLst");
            }
        }

        private ObservableCollection<User> FillCommissionDashPayeeUser()
        {
            ObservableCollection<User> AgentList = new ObservableCollection<User>();
            List<User> _userList = new List<User>();
            if (SharedVMData.GlobalAgentList != null)
            {
                if (SharedVMData.GlobalAgentList.Count > 0)
                {
                    _userList = SharedVMData.GlobalAgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList<User>();

                    if (_userList == null)
                    {
                        _userList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent)).ToList();
                    }
                }
                else
                {
                    _userList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent)).ToList();
                }
            }
            else
            {
                _userList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent)).ToList();
            }

            if (_userList.Count > 0)
            {
                AgentList = new ObservableCollection<User>(_userList.OrderBy(a => a.NickName));
            }
            return AgentList;
        }
        //private ObservableCollection<User> FillCommissionDashPayeeUser()
        //{
        //    ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));
        //    if (AgentList.Count > 0)
        //    {
        //        AgentList = new ObservableCollection<User>(AgentList.OrderBy(a => a.NickName));
        //    }
        //    return AgentList;
        //}


        public delegate void OpenIssueCommissionDashBoard();
        public event OpenIssueCommissionDashBoard OpenIssueCommissionDashBoardEvent;

        public delegate void CloseissueCommissionDashBoard();
        public event CloseissueCommissionDashBoard CloseIssueCommissionDashBoardEvent;

        private ICommand _commiossionIssueResolvedBtn;
        public ICommand CommiossionIssueResolvedBtn
        {
            get
            {
                if (_commiossionIssueResolvedBtn == null)
                {
                    _commiossionIssueResolvedBtn = new BaseCommand(x => BeforeCommiossionIssueResolvedBtnAction(), x => CommiossionIssueResolvedBtnAction());
                }
                return _commiossionIssueResolvedBtn;
            }
        }

        private bool BeforeCommiossionIssueResolvedBtnAction()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true; 
        }

        private void CommiossionIssueResolvedBtnAction()
        {
            if (PolicySelectedFollowUpCommissionDashBoard == null || PolicySelectedFollowUpCommissionDashBoard.IssueId == Guid.Empty) return;
            //add by neha  
            if (isCloseStatus == true)
            {
                if (PolicySelectedFollowUpCommissionDashBoard.IssueId != Guid.Empty)
                {
                    //Acme added - 01/10/2020
                    if (PolicySelectedFollowUpCommissionDashBoard.IsResolvedFromCommDashboard == true)
                    {
                        MessageBox.Show("Issue cannot be removed as it is manually resolved from Commission Dashboard.", "Information", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBoxResult _MessageBoxResult = MessageBox.Show("Are you sure you want to remove?", "Information", MessageBoxButton.OKCancel);
                        if (_MessageBoxResult == MessageBoxResult.OK)
                        {
                            //serviceClients.FollowupIssueClient.DeleteIssue(PolicySelectedFollowUpCommissionDashBoard.IssueId);
                            //update isdeleted =true.
                            //Don't delete recards from database
                            serviceClients.FollowupIssueClient.RemoveCommisionIssue(PolicySelectedFollowUpCommissionDashBoard.IssueId);
                            FillFollowUpIssue();
                            PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId == 2).Distinct().ToList());
                            DistinctCalculation(PolicyFollowUpCommissionDashBoardLst);
                            PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();

                        }
                    }
                }
            }
            else
            {
                ShowCommDashFollowUpWindow();
            }
        }

        private void ShowCommDashFollowUpWindow()
        {
            if (OpenIssueCommissionDashBoardEvent != null)
            {
                OpenIssueCommissionDashBoardEvent();
            }
        }
        private void CloseCommdashFollowUpWindow()
        {
            if (CloseIssueCommissionDashBoardEvent != null)
            {
                CloseIssueCommissionDashBoardEvent();
            }
        }
        private ICommand _commiossionIssuePaymentReceived;
        public ICommand CommiossionIssuePaymentReceived
        {
            get
            {
                if (_commiossionIssuePaymentReceived == null)
                {
                    _commiossionIssuePaymentReceived = new BaseCommand(x => CommiossionIssuePaymentReceivedAction(x));
                }
                return _commiossionIssuePaymentReceived;
            }
        }

        private ICommand _RefreshFollowUpIssue;
        public ICommand RefreshFollowUpIssue
        {
            get
            {
                if (_RefreshFollowUpIssue == null)
                {
                    _RefreshFollowUpIssue = new BaseCommand(x => UpdateFollowUpIssues());
                }
                return _RefreshFollowUpIssue;
            }
        }

        private DisplayFollowupIssue _policySelectedFollowUpCommissionDashBoard;
        public DisplayFollowupIssue PolicySelectedFollowUpCommissionDashBoard
        {
            get
            {
                return _policySelectedFollowUpCommissionDashBoard == null ? new DisplayFollowupIssue() : _policySelectedFollowUpCommissionDashBoard;
            }
            set
            {
                _policySelectedFollowUpCommissionDashBoard = value;
                OnPropertyChanged("PolicySelectedFollowUpCommissionDashBoard");
            }
        }

        private string StrLastFollowUpRuns;
        public string strLastFollowUpRuns
        {
            get { return StrLastFollowUpRuns; }
            set
            {
                StrLastFollowUpRuns = value;
                OnPropertyChanged("StrLastFollowUpRuns");
            }
        }


        private bool _IsBusy;
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                _IsBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        private bool _PopulatingRecords;
        public bool PopulatingRecords
        {
            get
            {
                return _PopulatingRecords;
            }
            set
            {
                _PopulatingRecords = value;
                OnPropertyChanged("PopulatingRecords");
            }
        }

        private bool _isOpenStatus;
        public bool isOpenStatus
        {
            get
            {
                return _isOpenStatus;
            }
            set
            {
                _isOpenStatus = value;
                OnPropertyChanged("isOpenStatus");
            }
        }

        private bool _isCloseStatus;
        public bool isCloseStatus
        {
            get
            {
                return _isCloseStatus;
            }
            set
            {
                _isCloseStatus = value;
                OnPropertyChanged("isCloseStatus");
            }
        }

        private bool _isAllStatus;
        public bool isAllStatus
        {
            get
            {
                return _isAllStatus;
            }
            set
            {
                _isAllStatus = value;
                OnPropertyChanged("isAllStatus");
            }
        }

        private ObservableCollection<DisplayFollowupIssue> _policyFollowUpCommissionDashBoardLst;
        public ObservableCollection<DisplayFollowupIssue> PolicyFollowUpCommissionDashBoardLst
        {
            get
            {
                return _policyFollowUpCommissionDashBoardLst == null ? new ObservableCollection<DisplayFollowupIssue>() : _policyFollowUpCommissionDashBoardLst;
            }
            set
            {
                _policyFollowUpCommissionDashBoardLst = value;
                OnPropertyChanged("PolicyFollowUpCommissionDashBoardLst");
            }
        }

        DateTime? dtRefresh = new DateTime();
        private void UpdateFollowUpIssues()
        {
            Thread Th = new Thread(() =>
            {
                IsBusy = true;

                if (SelectedPolicy == null)
                {
                    return;
                }
                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
                dtRefresh = serviceClients.PolicyClient.GetFollowUpDate(SelectedPolicy.PolicyId);
                FillFollowUpIssue();

                if (CommissionDashBoardOutGoingPaymentLst.Count > 0)
                    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
            });
            Th.Start();



            //IsBusy = true;

            //if (SelectedPolicy == null)
            //{
            //    return;
            //}
            //serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, true);
            //dtRefresh = serviceClients.PolicyClient.GetFollowUpDate(SelectedPolicy.PolicyId);
            //FillFollowUpIssue();

            //if (CommissionDashBoardOutGoingPaymentLst.Count > 0)
            //    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();


        }

        public void FillFollowUpIssue()
        {
            IsBusy = true;
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                PolicyFollowUpCommissionDashBoardLst = null;
                PolicySelectedFollowUpCommissionDashBoard = null;
                return;
            }

            // serviceClients.FollowupIssueClient.GetIssuesAsync(SelectedPolicy.PolicyId);
            // PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(serviceClients.FollowupIssueClient.GetFewIssueForCommissionDashBoard(SelectedPolicy.PolicyId).OrderBy(p=>p.InvoiceDate));
            if (!VMInstances.OptimizedPolicyManager.isAvoideTocall)
            {
                serviceClients.FollowupIssueClient.GetFewIssueForCommissionDashBoardAsync(SelectedPolicy.PolicyId);
            }
            //Vinod
            //PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(serviceClients.FollowupIssueClient.GetFewIssueForCommissionDashBoard(SelectedPolicy.PolicyId).OrderBy(p=>p.InvoiceDate));



            //PolicyFollowUpCommissionDashBoardLst =new ObservableCollection<DisplayFollowupIssue>(serviceClients.FollowupIssueClient.GetFewIssueAccordingtoMode(SelectedPolicy.PolicyId)).ToList();
        }

        void FollowupIssueClient_GetIssuesCompleted(object sender, GetIssuesCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    PolicyFollowUpCommissionDashBoardLst = e.Result;
                    foreach (DisplayFollowupIssue fip in PolicyFollowUpCommissionDashBoardLst.Distinct().ToList())
                    {
                        if (fip.IssueCategoryID.HasValue)
                            fip.Category = IssueCategoryCollection.FirstOrDefault(s => s.CategoryID == fip.IssueCategoryID.Value);
                        if (fip.IssueReasonId.HasValue)
                            fip.Reason = IssueReasonCollection.FirstOrDefault(s => s.ReasonsID == fip.IssueReasonId.Value);
                        if (fip.IssueResultId.HasValue)
                            fip.Results = IssueResultCollection.FirstOrDefault(s => s.ResultsID == fip.IssueResultId.Value);
                        if (fip.IssueStatusId.HasValue)
                            fip.Status = IssueStatusCollection.FirstOrDefault(s => s.StatusID == fip.IssueStatusId.Value);

                    }
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.Distinct().FirstOrDefault();

                }
            }
            catch
            {
            }
        }

        void FollowupIssueClient_GetFewIssueForCommissionDashBoardCompleted(object sender, GetFewIssueForCommissionDashBoardCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    PolicyFollowUpCommissionDashBoardLst = e.Result;

                    PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(p => p.isDeleted != true).ToList());

                    bool isforfollowup = false;
                    isforfollowup = serviceClients.BillingLineDetailClient.IsFollowUpLicensee(SharedVMData.SelectedLicensee.LicenseeId);

                    foreach (DisplayFollowupIssue fip in PolicyFollowUpCommissionDashBoardLst)
                    {
                        if (fip.IssueCategoryID.HasValue)
                            fip.Category = IssueCategoryCollection.FirstOrDefault(s => s.CategoryID == fip.IssueCategoryID.Value);
                        if (fip.IssueReasonId.HasValue)
                            fip.Reason = IssueReasonCollection.FirstOrDefault(s => s.ReasonsID == fip.IssueReasonId.Value);
                        if (fip.IssueResultId.HasValue)
                            fip.Results = IssueResultCollection.FirstOrDefault(s => s.ResultsID == fip.IssueResultId.Value);
                        if (fip.IssueStatusId.HasValue)
                            fip.Status = IssueStatusCollection.FirstOrDefault(s => s.StatusID == fip.IssueStatusId.Value);
                        if (isforfollowup)
                        {
                            if (SharedVMData.MasterFollowupIssueList.Count > 0)
                            {
                                SharedVMData.MasterFollowupIssueList.Remove((SharedVMData.MasterFollowupIssueList.Where(p => p.IssueId == fip.IssueId).FirstOrDefault()));
                                SharedVMData.MasterFollowupIssueList.Add(fip);
                            }
                        }
                        else
                        {
                            if (SharedVMData.ReadonlyMasterFollowUpList.Count > 0)
                            {
                                SharedVMData.ReadonlyMasterFollowUpList.Remove((SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.IssueId == fip.IssueId).FirstOrDefault()));
                                SharedVMData.ReadonlyMasterFollowUpList.Add(fip);
                            }
                        }
                    }

                    if (isAllStatus)
                    {
                        PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.ToList());
                        PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    }
                    if (isCloseStatus)
                    {
                        PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId == 2).Distinct().ToList());
                        PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    }
                    if (isOpenStatus)
                    {
                        PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId != 2).Distinct().ToList());
                        PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    }
                }

                //[Ankit-26-10-18:Add check for showing last follow up run if null then empty otherwise value]
                if (SelectedPolicy.LastFollowUpRuns == null)
                {    
                   String getLastFollowUpRuns = serviceClients.FollowupIssueClient.CheckrefershButton(SelectedPolicy.PolicyId);

                    if (string.IsNullOrEmpty(getLastFollowUpRuns) || getLastFollowUpRuns=="")
                    {
                        strLastFollowUpRuns = null;
                    }
                    else
                    {

                        strLastFollowUpRuns= getLastFollowUpRuns.ToString().Split(' ')[0];
                    }
                }
                if (SelectedPolicy.LastFollowUpRuns != null)
                {
                    if (dtRefresh >= SelectedPolicy.LastFollowUpRuns)
                    {
                        strLastFollowUpRuns = dtRefresh.Value.ToShortDateString();
                        SelectedPolicy.LastFollowUpRuns = dtRefresh;
                        dtRefresh = new DateTime();
                    }
                    else
                    {
                        strLastFollowUpRuns = SelectedPolicy.LastFollowUpRuns.Value.ToShortDateString();
                    }

                }


                if (CommissionDashBoardOutGoingPaymentLst != null)
                {
                    if (PolicyFollowUpCommissionDashBoardLst.Count > 0)
                    {
                        PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    }
                }

                if (CommissionDashBoardOutGoingPaymentLst != null)
                {
                    if (CommissionDashBoardOutGoingPaymentLst.Count > 0)
                    {
                        CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }

            IsBusy = false;
        }

        private void LoadFollowupDate()
        {
            if (SelectedPolicy != null)
            {
                if (SelectedPolicy.LastFollowUpRuns != null)
                {
                    if (dtRefresh > SelectedPolicy.LastFollowUpRuns)
                        strLastFollowUpRuns = dtRefresh.Value.ToShortDateString();
                    else
                        strLastFollowUpRuns = SelectedPolicy.LastFollowUpRuns.Value.ToShortDateString();
                }
                else
                {
                    strLastFollowUpRuns = string.Empty;
                }
            }
            else
            {
                strLastFollowUpRuns = "Not found";
            }
        }

        private void loadIssueWhenClickNew()
        {
            try
            {
                PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(serviceClients.FollowupIssueClient.GetFewIssueForCommissionDashBoard(SelectedPolicy.PolicyId));
                foreach (DisplayFollowupIssue fip in PolicyFollowUpCommissionDashBoardLst)
                {
                    if (fip.IssueCategoryID.HasValue)
                        fip.Category = IssueCategoryCollection.FirstOrDefault(s => s.CategoryID == fip.IssueCategoryID.Value);
                    if (fip.IssueReasonId.HasValue)
                        fip.Reason = IssueReasonCollection.FirstOrDefault(s => s.ReasonsID == fip.IssueReasonId.Value);
                    if (fip.IssueResultId.HasValue)
                        fip.Results = IssueResultCollection.FirstOrDefault(s => s.ResultsID == fip.IssueResultId.Value);
                    if (fip.IssueStatusId.HasValue)
                        fip.Status = IssueStatusCollection.FirstOrDefault(s => s.StatusID == fip.IssueStatusId.Value);
                }

                if (isAllStatus)
                {
                    PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.ToList());
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.Distinct().ToList().FirstOrDefault();
                }
                if (isCloseStatus)
                {
                    PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId == 2).Distinct().ToList());
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                }
                if (isOpenStatus)
                {
                    PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.Where(s => s.IssueStatusId != 2).Distinct().ToList());
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                }
            }

            catch
            {
            }
        }

        private void CommiossionIssuePaymentReceivedAction(object param)
        {
            try
            {
                string str = param as string;
                if (str == "PaymentPeceived")
                {
                    if (RoleManager.Role == UserRole.SuperAdmin)
                        //PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 3;
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;
                    else
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;

                    PolicySelectedFollowUpCommissionDashBoard.IssueStatusId = 2;
                    PolicySelectedFollowUpCommissionDashBoard.IsResolvedFromCommDashboard = true; //added by Acme - 01/10/2020
                    serviceClients.FollowupIssueClient.AddUpdateIssue(PolicySelectedFollowUpCommissionDashBoard);

                    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.ResolveIssue, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, false, RoleManager.Role, null);

                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(PolicySelectedFollowUpCommissionDashBoard.InvoiceDate);

                    /*
                     * Acme April 08, 2019 - following commented after found wrong as per kevin's commnets 
                     * PolicyPaymentEntriesPost objPolicyPaymentEntriesPost = new PolicyPaymentEntriesPost();
                    objPolicyPaymentEntriesPost.PolicyID = SelectedPolicy.PolicyId;
                    objPolicyPaymentEntriesPost.InvoiceDate = dt;
                    objPolicyPaymentEntriesPost.CreatedOn = System.DateTime.Now;
                    objPolicyPaymentEntriesPost.CreatedBy = RoleManager.userCredentialID;
                    objPolicyPaymentEntriesPost.FollowUpVarIssueId = PolicySelectedFollowUpCommissionDashBoard.IssueId;
                    objPolicyPaymentEntriesPost.FollowUpIssueResolveOrClosed = 1;

                    serviceClients.PostUtilClient.AddUpadateResolvedorClosed(objPolicyPaymentEntriesPost);*/

                    FillFollowUpIssue();
                    //DistinctCalculation(PolicyFollowUpCommissionDashBoardLst);
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.Distinct().FirstOrDefault();
                    CloseCommdashFollowUpWindow();
                }
                else if (str == "ResolvedInvoice")
                {
                    if (RoleManager.Role == UserRole.SuperAdmin)
                        //PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 3;
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;
                    else if (RoleManager.Role == UserRole.Agent)
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;
                    PolicySelectedFollowUpCommissionDashBoard.IssueStatusId = 2;
                    PolicySelectedFollowUpCommissionDashBoard.IsResolvedFromCommDashboard = true; //added by Acme - 01/10/2020
                    serviceClients.FollowupIssueClient.AddUpdateIssue(PolicySelectedFollowUpCommissionDashBoard);

                    serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.ResolveIssue, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, false, RoleManager.Role, null);

                    /* 
                     * Acme April 08, 2019 - following commented after found wrong as per kevin's commnets 
                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(PolicySelectedFollowUpCommissionDashBoard.InvoiceDate);

                    
                     * PolicyPaymentEntriesPost objPolicyPaymentEntriesPost = new PolicyPaymentEntriesPost();
                    objPolicyPaymentEntriesPost.PolicyID = SelectedPolicy.PolicyId;
                    objPolicyPaymentEntriesPost.InvoiceDate = dt;
                    objPolicyPaymentEntriesPost.CreatedOn = System.DateTime.Now;
                    objPolicyPaymentEntriesPost.CreatedBy = RoleManager.userCredentialID;
                    objPolicyPaymentEntriesPost.FollowUpVarIssueId = PolicySelectedFollowUpCommissionDashBoard.IssueId;
                    objPolicyPaymentEntriesPost.FollowUpIssueResolveOrClosed = 1;

                    serviceClients.PostUtilClient.AddUpadateResolvedorClosed(objPolicyPaymentEntriesPost);*/

                    FillFollowUpIssue();
                    //DistinctCalculation(PolicyFollowUpCommissionDashBoardLst);
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.Distinct().FirstOrDefault();
                    CloseCommdashFollowUpWindow();
                }
                else if (str == "Cancel")
                {
                    CloseCommdashFollowUpWindow();
                }
            }
            catch
            {
            }
        }

        private void DistinctCalculation(ObservableCollection<DisplayFollowupIssue> PolicyFollowUpCommissionDashBoardLst)
        {
            //List<DateTime?> UniqueDateTime = PolicyFollowUpCommissionDashBoardLst.Select(c => c.InvoiceDate).Distinct().ToList();

            //List<DisplayFollowupIssue> _TempDisplayFollowupIssue = new List<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst);

            //PolicyFollowUpCommissionDashBoardLst.Clear();

            //foreach (DateTime dt in UniqueDateTime)
            //{
            //    PolicyFollowUpCommissionDashBoardLst.Add(_TempDisplayFollowupIssue.Where(p => p.InvoiceDate == dt).FirstOrDefault());
            //}
            PolicyFollowUpCommissionDashBoardLst = new ObservableCollection<DisplayFollowupIssue>(PolicyFollowUpCommissionDashBoardLst.OrderBy(p => p.InvoiceDate).Distinct().ToList());

        }

        #endregion

        #region CommissionDashBoardEditOutGoingPayment
        public delegate void OpenEditOutGoingPaymentCommissionDashBoard();
        public event OpenEditOutGoingPaymentCommissionDashBoard OpenEditOutGoingPaymentCommissionDashBoardEvent;

        public delegate void CloseEditOutGoingCommissionDashBoard();
        public event CloseEditOutGoingCommissionDashBoard CloseEditOutGoingCommissionDashBoardEvent;


        private ICommand _commissionDashBoardEditOutgoingPayment;
        public ICommand CommissionDashBoardEditOutgoingPayment
        {
            get
            {
                if (_commissionDashBoardEditOutgoingPayment == null)
                {
                    _commissionDashBoardEditOutgoingPayment = new BaseCommand(x => BeforeCommissionDashBoardEditOutgoingPaymentOpen(), x => CommissionDashBoardEditOutgoingPaymentOpen());
                }
                return _commissionDashBoardEditOutgoingPayment;
            }
        }

        private bool BeforeCommissionDashBoardEditOutgoingPaymentOpen()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check if selected policy is not saved then new button is disabled
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)// || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true))
                return false;

            return true;
        }

        private ICommand commissionDashOK;
        public ICommand CommissionDashOK
        {
            get
            {
                if (commissionDashOK == null)
                {
                    commissionDashOK = new BaseCommand(x => BeforeCommissionDashOKAction(), x => CommissionDashOKAction());
                }
                return commissionDashOK;
            }
        }

        private bool BeforeCommissionDashOKAction()
        {
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty)
                return false;
            else if ((CommissionDashBoardOutGoingPaymentLst.Sum(p => p.Payment) != 100) && (CommissionDashBoardOutGoingPaymentLst.Sum(p => p.Payment) != 0))
                return false;
            else if (Convert.ToDouble(PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment) != CommissionDashBoardOutGoingPaymentLst.Sum(p => p.PaidAmount ?? 0))
                return false;
            return true;
        }

        private void CommissionDashOKAction()
        {
            try
            {
                if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty) return;
                if (Convert.ToDouble(PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment) != CommissionDashBoardOutGoingPaymentLst.Sum(p => p.PaidAmount ?? 0))
                {
                    MessageBox.Show("Payment mismatch", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutGoingPaymentEntries(CommissionDashBoardOutGoingPaymentLst);

                if (CloseEditOutGoingCommissionDashBoardEvent != null)
                    CloseEditOutGoingCommissionDashBoardEvent();
            }
            catch
            {
            }
        }

        private ICommand commissionDashCancel;

        public ICommand CommissionDashCancel
        {
            get
            {
                if (commissionDashCancel == null)
                {
                    commissionDashCancel = new BaseCommand(x => CommissionDashCancelAction());
                }
                return commissionDashCancel;
            }
        }

        private ICommand commdashOutGoingDelete;
        public ICommand CommDashOutGoingDelete
        {
            get
            {
                if (commdashOutGoingDelete == null)
                {
                    commdashOutGoingDelete = new BaseCommand(x => CommDashOutGoingDeleteAction(x));
                }
                return commdashOutGoingDelete;
            }
        }

        private void CommDashOutGoingDeleteAction(object x)
        {
            if (objLog == null) objLog = new MastersClient();
            try
            {
                Guid OutgoingPaymentId = (Guid)x;
                //Acme - added check to ignore deletion when entry from DEU - as discussed with Kevin Jan 17, 2017
                bool isDEUEntry = !serviceClients.OutGoingPaymentClient.CheckIsPaymentFromDEUForOutgoingPaymentID(OutgoingPaymentId);
                if (isDEUEntry)
                {
                    MessageBoxResult _MessageBoxResult = MessageBox.Show("This is DEU entry and cannot be deleted", "Information", MessageBoxButton.OK);
                }
                else
                {
                    MessageBoxResult _MessageBoxResult = MessageBox.Show("Do you want to delete record", "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (_MessageBoxResult == MessageBoxResult.OK)
                    {
                        //Acme changes to delete both entries 
                        //PolicyOutgoingDistribution obj = serviceClients.OutGoingPaymentClient.GetOutgoingPaymentByID(OutgoingPaymentId);
                        //Guid? reverseID = obj.ReverseOutgoingPaymentId;
                        objLog.AddLog(DateTime.Now.ToString() + " Manual deletion of outgoing entry requested: " + OutgoingPaymentId);

                        try
                        {
                            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PolicyOutgoingDistributionClient.InnerChannel))
                            {
                                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                                serviceClients.PolicyOutgoingDistributionClient.DeleteOutGoingPaymentViaOutgoingPaymentId(OutgoingPaymentId);
                            }
                        }
                        catch (Exception ex)
                        {
                            serviceClients.PolicyOutgoingDistributionClient.DeleteOutGoingPaymentViaOutgoingPaymentId(OutgoingPaymentId);
                        }

                        CommissionDashBoardOutGoingPaymentLst.Remove(CommissionDashBoardOutGoingPaymentLst.Where(p => p.OutgoingPaymentId == OutgoingPaymentId).FirstOrDefault());
                        objLog.AddLog(DateTime.Now.ToString() + " Manual deletion of outgoing entry success: " + OutgoingPaymentId);
                        //Acme - get reversePaymentID and remove if exists - two statements required as per the architecture
                        //CommissionDashBoardOutGoingPaymentLst.Remove(CommissionDashBoardOutGoingPaymentLst.Where(p => p.ReverseOutgoingPaymentId == reverseID).FirstOrDefault());
                        //CommissionDashBoardOutGoingPaymentLst.Remove(CommissionDashBoardOutGoingPaymentLst.Where(p => p.ReverseOutgoingPaymentId == reverseID).FirstOrDefault());
                    }
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(DateTime.Now.ToString() + " Manual deletion of outgoing entry exception: " + ex.Message);
            }

        }
        private ICommand commissiondashboardResetOutGoingPayment;
        public ICommand CommissionDashBoardResetOutGoingPayment
        {
            get
            {
                if (commissiondashboardResetOutGoingPayment == null)
                {
                    commissiondashboardResetOutGoingPayment = new BaseCommand(x => CommissionDashBoardResetOutGoingPaymentAction(x));
                }
                return commissiondashboardResetOutGoingPayment;
            }
        }

        private void CommissionDashBoardResetOutGoingPaymentAction(object x)
        {
            CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
            CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
        }

        private void CommissionDashCancelAction()
        {
            if (CloseEditOutGoingCommissionDashBoardEvent != null)
                CloseEditOutGoingCommissionDashBoardEvent();
        }


        private PolicyOutgoingDistribution _commissionDashBoardSelecetdOutGoingPaymentLst;
        public PolicyOutgoingDistribution CommissionDashBoardSelecetdOutGoingPaymentLst
        {
            get { return _commissionDashBoardSelecetdOutGoingPaymentLst == null ? new PolicyOutgoingDistribution() : _commissionDashBoardSelecetdOutGoingPaymentLst; }
            set { _commissionDashBoardSelecetdOutGoingPaymentLst = value; OnPropertyChanged("CommissionDashBoardSelecetdOutGoingPaymentLst"); }
        }

        private ObservableCollection<PolicyOutgoingDistribution> _commissionDashBoardOutGoingPaymentLst;
        public ObservableCollection<PolicyOutgoingDistribution> CommissionDashBoardOutGoingPaymentLst
        {
            get { return _commissionDashBoardOutGoingPaymentLst; }
            set { _commissionDashBoardOutGoingPaymentLst = value; OnPropertyChanged("CommissionDashBoardOutGoingPaymentLst"); }
        }

        private void CommissionDashBoardEditOutgoingPaymentOpen()
        {
            ShowOutGoingPaymentEdit();
        }
        private void ShowOutGoingPaymentEdit()
        {
            try
            {
                if (OpenEditOutGoingPaymentCommissionDashBoardEvent != null)
                {
                    CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                    OpenEditOutGoingPaymentCommissionDashBoardEvent();
                    CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                }
            }
            catch
            {
            }
        }

        private ObservableCollection<PolicyOutgoingDistribution> FillCommissionDashBoardOutGoingPaymentLst()
        {
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null)
            {
                return new ObservableCollection<PolicyOutgoingDistribution>();
            }
            ObservableCollection<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = new ObservableCollection<PolicyOutgoingDistribution>(serviceClients.PolicyOutgoingDistributionClient.GetOutgoingPaymentByPoicyPaymentEntryId(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID));
            return _PolicyOutgoingDistribution;
        }

        #endregion
    }
}
