using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VMLib.PolicyManager
{
    public class VMPolicySchedule : BaseViewModel
    {
        #region Schedule Grid Data

        public static ScheduleGridData ScheduleGrdData;
        public static ScheduleGridData OutScheduleGrdData;

        #endregion

        #region Constructor

        public VMPolicySchedule(VMOptimizePolicyManager OptimizedPolicyManager,PolicyDetailMasterData MasterData)
        {
            PropertyChanged += new PropertyChangedEventHandler(VMPolicySchedule_PropertyChanged);
            OptimizedPolicyManager.SelectedLicenseeChanged += new VMOptimizePolicyManager.SelectedLicenseeChangedEventHandler(OptimizedPolicyManager_SelectedLicenseeChanged);
            OptimizedPolicyManager.SelectedClientChanged += new VMOptimizePolicyManager.SelectedClientChangedEventHandler(OptimizedPolicyManager_SelectedClientChanged);
            OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedPolicyChangedEventHandler(OptimizedPolicyManager_SelectedPolicyChanged);
            OptimizedPolicyManager.SelectedPayorChanged += new VMOptimizePolicyManager.SelectedPayorChangedEventHandler(OptimizedPolicyManager_SelectedPayorChanged);
            OptimizedPolicyManager.SelectedCarrierChanged += new VMOptimizePolicyManager.SelectedCarrierChangedEventHandler(OptimizedPolicyManager_SelectedCarrierChanged);
            OptimizedPolicyManager.SelectedCoverageChanged += new VMOptimizePolicyManager.SelectedCoverageChangedEventHandler(OptimizedPolicyManager_SelectedCoverageChanged);

            IncomingAdvanceScheduleTypes = MasterData.IncomingAdvanceScheduleTypes;
            SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault();
            OutgoingAdvanceScheduleTypes = MasterData.OutgoingAdvanceScheduleTypes;
            SelecetdOutgoingAdvanceScheduleTypes = OutgoingAdvanceScheduleTypes.FirstOrDefault();

            serviceClients.IncomingScheduleClient.GetPolicyIncomingScheduleCompleted += new EventHandler<GetPolicyIncomingScheduleCompletedEventArgs>(IncomingScheduleClient_GetPolicyIncomingScheduleCompleted);
            serviceClients.OutgoingScheduleClient.GetOutgoingSheduleByCompleted += new EventHandler<GetOutgoingSheduleByCompletedEventArgs>(OutgoingScheduleClient_GetOutgoingSheduleByCompleted);
            if (_NewSheduleEntry == null)
                _NewSheduleEntry = new IncomingScheduleEntry();

            if (_NewOutSheduleEntry == null)
                _NewOutSheduleEntry = new OutgoingScheduleEntry();
        }

        void VMPolicySchedule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelecetdIncomingAdvanceScheduleTypes":
                    if (SelecetdIncomingAdvanceScheduleTypes.Name == "Percentage of Premium")
                    {
                        FromText = "From($)";
                        ToText = "To($)";
                        CommTText = "Commission(%)";
                        FromRangeVisibility = Visibility.Visible.ToString();
                        ToRangeVisibility = Visibility.Visible.ToString();

                        ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.RateColumnName = "Commission %";

                    }
                    else if (SelecetdIncomingAdvanceScheduleTypes.Name == "Percentage of Target")
                    {
                        FromText = "From($)";
                        ToText = "To($)";
                        CommTText = "Commission(%)";
                        FromRangeVisibility = Visibility.Visible.ToString();
                        ToRangeVisibility = Visibility.Visible.ToString();

                        ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.RateColumnName = "Commission %";

                    }
                    else if (SelecetdIncomingAdvanceScheduleTypes.Name == "Per Head Fee Scale")
                    {
                        FromText = "From";
                        ToText = "To";
                        CommTText = "Fee $";
                        FromRangeVisibility = Visibility.Visible.ToString();
                        ToRangeVisibility = Visibility.Visible.ToString();

                        ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.RateColumnName = "Fee($) / Head";

                    }
                    else if (SelecetdIncomingAdvanceScheduleTypes.Name == "Per Head Fee Target")
                    {
                        FromText = "From";
                        ToText = "To";
                        CommTText = "Fee $";
                        FromRangeVisibility = Visibility.Visible.ToString();
                        ToRangeVisibility = Visibility.Visible.ToString();

                        ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        ScheduleGrdData.RateColumnName = "Fee($) / Head";

                    }
                    else if (SelecetdIncomingAdvanceScheduleTypes.Name == "Flat $")
                    {
                        FromText = "";
                        ToText = "";
                        CommTText = "Fee $";
                        FromRangeVisibility = Visibility.Collapsed.ToString();
                        ToRangeVisibility = Visibility.Collapsed.ToString();

                        ScheduleGrdData.FromRangeVisibility = Visibility.Collapsed;
                        ScheduleGrdData.ToRangeVisibility = Visibility.Collapsed;
                        ScheduleGrdData.RateColumnName = "Fee($)";

                    }
                    break;
                case "SelectedIncomingAdvanceSchedule":
                    if (SelectedIncomingAdvanceSchedule != null)
                        NewSheduleEntry = SelectedIncomingAdvanceSchedule.Clone() as IncomingScheduleEntry;
                    else
                        NewSheduleEntry = new IncomingScheduleEntry();


                    break;

                case "SelecetdOutgoingAdvanceScheduleTypes":
                    if (SelecetdOutgoingAdvanceScheduleTypes.Name == "Percentage of Premium")
                    {
                        FromOutgoingText = "From($)";
                        ToOutgoingText = "To($)";
                        CommOutgoingTText = "Commission(%)";

                        FromOutgoingRangeVisibility = Visibility.Visible.ToString();
                        ToOutgoingRangeVisibility = Visibility.Visible.ToString();

                        OutScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.RateColumnName = "Commission %";

                    }
                    else if (SelecetdOutgoingAdvanceScheduleTypes.Name == "Percentage of Target")
                    {
                        FromOutgoingText = "From($)";
                        ToOutgoingText = "To($)";
                        CommOutgoingTText = "Commission(%)";
                        FromOutgoingRangeVisibility = Visibility.Visible.ToString();
                        ToOutgoingRangeVisibility = Visibility.Visible.ToString();

                        OutScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.RateColumnName = "Commission %";

                    }
                    else if (SelecetdOutgoingAdvanceScheduleTypes.Name == "Per Head Fee Scale")
                    {
                        FromOutgoingText = "From";
                        ToOutgoingText = "To";
                        CommOutgoingTText = "Fee $";
                        FromOutgoingRangeVisibility = Visibility.Visible.ToString();
                        ToOutgoingRangeVisibility = Visibility.Visible.ToString();

                        OutScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.RateColumnName = "Fee($) / Head";

                    }
                    else if (SelecetdOutgoingAdvanceScheduleTypes.Name == "Per Head Fee Target")
                    {
                        FromOutgoingText = "From";
                        ToOutgoingText = "To";
                        CommOutgoingTText = "Fee $";
                        FromOutgoingRangeVisibility = Visibility.Visible.ToString();
                        ToOutgoingRangeVisibility = Visibility.Visible.ToString();

                        OutScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                        OutScheduleGrdData.RateColumnName = "Fee($) / Head";

                    }
                    else if (SelecetdOutgoingAdvanceScheduleTypes.Name == "Flat $")
                    {
                        FromOutgoingText = "";
                        ToOutgoingText = "";
                        CommOutgoingTText = "Fee $";
                        FromOutgoingRangeVisibility = Visibility.Hidden.ToString();
                        ToOutgoingRangeVisibility = Visibility.Collapsed.ToString();

                        OutScheduleGrdData.FromRangeVisibility = Visibility.Collapsed;
                        OutScheduleGrdData.ToRangeVisibility = Visibility.Collapsed;
                        OutScheduleGrdData.RateColumnName = "Fee($)";

                    }
                    break;
                case "SelectedOutgoingAdvanceSchedule":
                    if (SelectedOutgoingAdvanceSchedule != null)
                        NewOutSheduleEntry = SelectedOutgoingAdvanceSchedule.Clone() as OutgoingScheduleEntry;
                    else
                        NewOutSheduleEntry = new OutgoingScheduleEntry();

                    if(OutgoingPayeeList != null)
                        OutgoingSelecetdPayee = OutgoingPayeeList.Where(p => p.UserCredentialID == SelectedOutgoingAdvanceSchedule.PayeeUserCredentialId).FirstOrDefault();

                    break;
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

        #region Selected Object

        private DisplayedPayor SelectedPayor;
        private Carrier SelectedCarrier;
        private DisplayedCoverage SelectedCoverage;

        private PolicyDetailsData SelectedPolicy;
        private Client SelectedClient;
        private LicenseeDisplayData SelectedLicensee;

        void OptimizedPolicyManager_SelectedLicenseeChanged(LicenseeDisplayData SelectedLicensee, ObservableCollection<Client> Clients)
        {
            this.SelectedLicensee = SelectedLicensee;
            PrimaryAgents = OutgoingPayeeList = FillOutgoingPayeeUser();

            //PrimaryAgents = FillOutgoingPayeeUser();
            SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault();
        }

        void OptimizedPolicyManager_SelectedClientChanged(Client SelectedClient)
        {
            this.SelectedClient = SelectedClient;
        }

        void OptimizedPolicyManager_SelectedPolicyChanged(PolicyDetailsData SelectedPolicy)
        {
            try
            {
                this.SelectedPolicy = SelectedPolicy;
                FillIncomingAdvanceSchedule();
                FillOutgoingAdvanceSchedule();
            }
            catch (Exception)
            {
            }
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
            this.SelectedCoverage = SelectedCoverage;
        }

        #endregion

        #region IncomingAdvanceSchedule

        private ICommand _NewIncomingAdvanceSchedule;
        public ICommand NewIncomingAdvanceSchedule
        {
            get
            {
                if (_NewIncomingAdvanceSchedule == null)
                    _NewIncomingAdvanceSchedule = new BaseCommand(param => BeforeOnNewIncomingAdvanceSchedule(), param => OnNewIncomingAdvanceSchedule());
                return _NewIncomingAdvanceSchedule;
            }
        }

        private bool BeforeOnNewIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then add button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
         
                return true;
        }

        private void OnNewIncomingAdvanceSchedule()
        {
            NewSheduleEntry = new IncomingScheduleEntry();
        }

        private ICommand addIncomingAdvanceSchedule;
        public ICommand AddIncomingAdvanceSchedule
        {
            get
            {
                if (addIncomingAdvanceSchedule == null)
                    addIncomingAdvanceSchedule = new BaseCommand(param => BeforeOnAddIncomingAdvanceSchedule(), param => OnAddIncomingAdvanceSchedule());
                return addIncomingAdvanceSchedule;
            }
        }

        private bool BeforeOnAddIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then add button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
           
          
                return true;
        }

        private void OnAddIncomingAdvanceSchedule()
        {
           // if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true) return;

            try
            {
                if (IncomingAdvanceScheduleLst == null)
                {
                    IncomingAdvanceScheduleLst = new PolicyIncomingSchedule();
                    return;
                }

                if (SelecetdIncomingAdvanceScheduleTypes.ScheduleTypeId != 5)
                {
                    if (NewSheduleEntry.FromRange == null) return;
                    if (NewSheduleEntry.ToRange == null) return;
                    if (NewSheduleEntry.FromRange > NewSheduleEntry.ToRange) return;
                }

                if (!IncomingAdvanceScheduleLst.IsModified)
                {
                    _SavedShedule = IncomingAdvanceScheduleLst.Clone() as PolicyIncomingSchedule;
                    IncomingAdvanceScheduleLst.IsModified = true;
                }

                if (IncomingAdvanceScheduleLst.IncomingScheduleList == null)
                    IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();

                if (NewSheduleEntry.CoveragesScheduleId == Guid.Empty)
                {
                    NewSheduleEntry.CoveragesScheduleId = Guid.NewGuid();
                    IncomingScheduleEntry entry = NewSheduleEntry.Clone() as IncomingScheduleEntry;
                    IncomingAdvanceScheduleLst.IncomingScheduleList.Add(entry);
                }
                else
                {
                    IncomingScheduleEntry entry = IncomingAdvanceScheduleLst.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewSheduleEntry.CoveragesScheduleId);
                    if (entry != null)
                    {
                        entry.EffectiveFromDate = NewSheduleEntry.EffectiveFromDate;
                        entry.EffectiveToDate = NewSheduleEntry.EffectiveToDate;
                        entry.FromRange = NewSheduleEntry.FromRange;
                        entry.ToRange = NewSheduleEntry.ToRange;
                        entry.Rate = NewSheduleEntry.Rate;
                    }
                }
                IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(IncomingAdvanceScheduleLst.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());

            }
            catch (Exception)
            {
            }
        }

        private ICommand removeIncomingAdvanceSchedule;

        public ICommand RemoveIncomingAdvanceSchedule
        {
            get
            {
                if (removeIncomingAdvanceSchedule == null)
                    removeIncomingAdvanceSchedule = new BaseCommand(param => BeforeOnRemoveIncomingAdvanceSchedule(), param => OnRemoveIncomingAdvanceSchedule());
                return removeIncomingAdvanceSchedule;

            }
        }

        private bool BeforeOnRemoveIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then remove button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
           
                return true;
        }

        private void OnRemoveIncomingAdvanceSchedule()
        {
            if (SelectedIncomingAdvanceSchedule == null || SelectedIncomingAdvanceSchedule.CoveragesScheduleId == Guid.Empty)
            {

                MessageBox.Show("Invalid Operation-No Record Selected", "Warning", MessageBoxButton.OK);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Do you want to remove Incoming Schedule Record",
                   "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            if (IncomingAdvanceScheduleLst != null && IncomingAdvanceScheduleLst.IncomingScheduleList != null)
            {
                IncomingScheduleEntry entry = IncomingAdvanceScheduleLst.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewSheduleEntry.CoveragesScheduleId);
                if (entry != null)
                {
                    if (!IncomingAdvanceScheduleLst.IsModified)
                    {
                        _SavedShedule = IncomingAdvanceScheduleLst.Clone() as PolicyIncomingSchedule;
                        IncomingAdvanceScheduleLst.IsModified = true;
                    }

                    IncomingAdvanceScheduleLst.IncomingScheduleList.Remove(entry);

                    if (IncomingAdvanceScheduleLst.IncomingScheduleList.Count != 0)
                    {
                        NewSheduleEntry = IncomingAdvanceScheduleLst.IncomingScheduleList[0].Clone() as IncomingScheduleEntry;
                    }
                    else
                    {
                        NewSheduleEntry = new IncomingScheduleEntry();
                        return;
                    }

                    IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(IncomingAdvanceScheduleLst.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());
                }
            }
        }

        private ICommand saveIncomingAdvanceSchedule;
        public ICommand SaveIncomingAdvanceSchedule
        {
            get
            {
                if (saveIncomingAdvanceSchedule == null)
                    saveIncomingAdvanceSchedule = new BaseCommand(param => BeforeOnSaveIncomingAdvanceSchedule(), param => OnSaveIncomingAdvanceSchedule());
                return saveIncomingAdvanceSchedule;

            }
        }

        private bool BeforeOnSaveIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            //check policy is saved or not 
            //if not save button will disble
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;
        }

        private void OnSaveIncomingAdvanceSchedule()
        {
            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                {
                    MessageBox.Show("Invalid Operation-No Policy Selected", "Warning", MessageBoxButton.OK);
                    return;

                }
                if (SelectedPolicy != null)
                    IncomingAdvanceScheduleLst.PolicyId = SelectedPolicy.PolicyId;
                IncomingAdvanceScheduleLst.ScheduleTypeId = SelecetdIncomingAdvanceScheduleTypes.ScheduleTypeId;
                serviceClients.IncomingScheduleClient.AddUpdatePolicyIncomingSchedule(IncomingAdvanceScheduleLst);
                IncomingAdvanceScheduleLst.IsModified = false;
                _SavedShedule = null;
                if (!(SelectedPolicy.IsIncomingBasicSchedule ?? false))
                {
                    //if (!FollowUpcalled)
                    {
                        serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.IncomingScheduleChange,
                            null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, null);
                        // FollowUpcalled = false;
                    }
                }
            }
            catch (Exception)
            {
            }
        }


        private ICommand defaultIncomingAdvanceShedule;
        public ICommand DefaultIncomingAdvanceShedule
        {
            get
            {
                if (defaultIncomingAdvanceShedule == null)
                    defaultIncomingAdvanceShedule = new BaseCommand(param => BeforeOnDefaultIncomingAdvanceSchedule(), param => OnDefaultIncomingAdvanceSchedule());
                return defaultIncomingAdvanceShedule;

            }
        }

        private bool BeforeOnDefaultIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;


            if (SelectedCarrier == null || SelectedCoverage == null || SelectedCarrier.CarrierId == Guid.Empty || SelectedCoverage.CoverageID == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then default button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;


        }

        private void OnDefaultIncomingAdvanceSchedule()
        {
            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;
                if (SelectedCarrier == null || SelectedCoverage == null || SelectedCarrier.CarrierId == Guid.Empty || SelectedCoverage.CoverageID == Guid.Empty) return;
                GlobalIncomingSchedule GlobalIncomingScheduleRecord = serviceClients.GlobalIncomingScheduleClient.GetGlobalIncomingSchedule(SelectedCarrier.CarrierId, SelectedCoverage.CoverageID);
                if (GlobalIncomingScheduleRecord == null || GlobalIncomingScheduleRecord.IncomingScheduleList == null)
                {
                    MessageBox.Show("No Default Schedule avilable", "Information", MessageBoxButton.OK);
                    return;
                }

                IncomingAdvanceScheduleLst = new PolicyIncomingSchedule();
                IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();
                IncomingAdvanceScheduleLst.PolicyId = SelectedPolicy.PolicyId;
                IncomingAdvanceScheduleLst.ScheduleTypeId = GlobalIncomingScheduleRecord.ScheduleTypeId;
                IncomingAdvanceScheduleLst.ScheduleTypeName = GlobalIncomingScheduleRecord.ScheduleTypeName;

                for (int idx = 0; idx < GlobalIncomingScheduleRecord.IncomingScheduleList.Count(); idx++)
                {
                    IncomingScheduleEntry _IncomingSchedule = new IncomingScheduleEntry();
                    _IncomingSchedule.CoveragesScheduleId = Guid.NewGuid();
                    _IncomingSchedule.FromRange = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].FromRange;
                    _IncomingSchedule.ToRange = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].ToRange;
                    _IncomingSchedule.EffectiveFromDate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].EffectiveFromDate;
                    _IncomingSchedule.EffectiveToDate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].EffectiveToDate;
                    _IncomingSchedule.Rate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].Rate;
                    IncomingAdvanceScheduleLst.IncomingScheduleList.Add(_IncomingSchedule);
                }
                IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(IncomingAdvanceScheduleLst.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());
                SelectedIncomingAdvanceSchedule = IncomingAdvanceScheduleLst.IncomingScheduleList.FirstOrDefault();

                if (IncomingAdvanceScheduleLst != null && IncomingAdvanceScheduleLst.ScheduleTypeId != 0)
                    SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault(s => s.ScheduleTypeId == IncomingAdvanceScheduleLst.ScheduleTypeId);
                else
                    SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault();

            }
            catch (Exception)
            {
            }
        }

        private PolicyIncomingSchedule incomingAdvanceScheduleLst;
        public PolicyIncomingSchedule IncomingAdvanceScheduleLst
        {
            get { return incomingAdvanceScheduleLst; }
            set { incomingAdvanceScheduleLst = value; OnPropertyChanged("IncomingAdvanceScheduleLst"); }
        }

        private PolicyIncomingSchedule _SavedShedule;

        private IncomingScheduleEntry _NewSheduleEntry;
        public IncomingScheduleEntry NewSheduleEntry
        {
            get
            {
                return _NewSheduleEntry == null ? new IncomingScheduleEntry() : _NewSheduleEntry;
            }
            set
            {
                _NewSheduleEntry = value;
                OnPropertyChanged("NewSheduleEntry");
            }
        }

        private IncomingScheduleEntry selectedIncomingAdvanceSchedule;
        public IncomingScheduleEntry SelectedIncomingAdvanceSchedule
        {
            get { return selectedIncomingAdvanceSchedule == null ? new IncomingScheduleEntry() : selectedIncomingAdvanceSchedule; }
            set { selectedIncomingAdvanceSchedule = value; OnPropertyChanged("SelectedIncomingAdvanceSchedule"); }
        }

        private string rateColumnName;
        public string RateColumnName
        {
            get { return rateColumnName; }
            set { rateColumnName = value; OnPropertyChanged("RateColumnName"); }
        }


        private string _FromRangeVisibility = System.Windows.Visibility.Visible.ToString();
        public string FromRangeVisibility
        {
            get { return _FromRangeVisibility; }
            set
            {
                _FromRangeVisibility = value;
                OnPropertyChanged("FromRangeVisibility");
            }
        }

        private string _ToRangeVisibility = System.Windows.Visibility.Visible.ToString();
        public string ToRangeVisibility
        {
            get { return _ToRangeVisibility; }
            set
            {
                _ToRangeVisibility = value;
                OnPropertyChanged("ToRangeVisibility");
            }
        }

        private string _CommissionVisibility = System.Windows.Visibility.Visible.ToString();
        public string CommissionVisibility
        {
            get { return _CommissionVisibility; }
            set
            {
                _CommissionVisibility = value;
                OnPropertyChanged("CommissionVisibility");
            }
        }
        private string _FromText;
        public string FromText
        {
            get
            {
                return _FromText;
            }
            set
            {
                _FromText = value;
                OnPropertyChanged("FromText");

            }
        }

        /// <summary>
        /// set commisition test 
        /// </summary>
        private string _CommText;
        public string CommTText
        {
            get
            {
                return _CommText;
            }
            set
            {
                _CommText = value;
                OnPropertyChanged("CommTText");

            }
        }

        /// <summary>
        /// Set To test
        /// </summary>
        private string _ToText;
        public string ToText
        {
            get
            {
                return _ToText;
            }
            set
            {
                _ToText = value;
                OnPropertyChanged("ToText");

            }
        }
        private PolicyIncomingScheduleType selectedIncomingAdvanceScheduleTypes;
        public PolicyIncomingScheduleType SelecetdIncomingAdvanceScheduleTypes
        {
            get
            {
                return selectedIncomingAdvanceScheduleTypes == null ? new PolicyIncomingScheduleType() : selectedIncomingAdvanceScheduleTypes;
            }
            set
            {
                selectedIncomingAdvanceScheduleTypes = value;
                OnPropertyChanged("SelecetdIncomingAdvanceScheduleTypes");
            }
        }
        private ObservableCollection<PolicyIncomingScheduleType> incomingAdvanceScheduleTypes;
        public ObservableCollection<PolicyIncomingScheduleType> IncomingAdvanceScheduleTypes
        {
            get
            {
                return incomingAdvanceScheduleTypes;
            }
            set
            {
                incomingAdvanceScheduleTypes = value;
                OnPropertyChanged("IncomingAdvanceScheduleTypes");
            }
        }


        private void FillIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                //IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();
                IncomingAdvanceScheduleLst = new PolicyIncomingSchedule();
                SelectedIncomingAdvanceSchedule = null;
                return;
            }

            serviceClients.IncomingScheduleClient.GetPolicyIncomingScheduleAsync(SelectedPolicy.PolicyId);
        }

        void IncomingScheduleClient_GetPolicyIncomingScheduleCompleted(object sender, GetPolicyIncomingScheduleCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IncomingAdvanceScheduleLst = e.Result;

                if (IncomingAdvanceScheduleLst.IncomingScheduleList == null)
                    IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();

                IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(IncomingAdvanceScheduleLst.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());

                if (IncomingAdvanceScheduleLst != null && IncomingAdvanceScheduleLst.ScheduleTypeId != 0)
                    SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault(s => s.ScheduleTypeId == IncomingAdvanceScheduleLst.ScheduleTypeId);
                else
                    SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault();

                SelectedIncomingAdvanceSchedule = IncomingAdvanceScheduleLst.IncomingScheduleList.FirstOrDefault();
            }
        }

        #endregion

        #region OutgoingAdvanceSchedlue

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
        private ICommand _NewOutgoingAdvanceSchedule;
        public ICommand NewOutgoingAdvanceSchedule
        {
            get
            {
                if (_NewOutgoingAdvanceSchedule == null)
                    _NewOutgoingAdvanceSchedule = new BaseCommand(param => BeforeOnNewOutgoingAdvanceSchedule(),param => OnNewOutgoingAdvanceSchedule());
                return _NewOutgoingAdvanceSchedule;
            }
        }

        private bool BeforeOnNewOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then New button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;
        }

        private void OnNewOutgoingAdvanceSchedule()
        {
            NewOutSheduleEntry = new OutgoingScheduleEntry();
        }

        private ICommand addOutgoingAdvanceSchedule;
        public ICommand AddOutgoingAdvanceSchedule
        {
            get
            {
                if (addOutgoingAdvanceSchedule == null)
                    addOutgoingAdvanceSchedule = new BaseCommand(param => BeforeOnAddOutgoingAdvanceSchedule(),param => OnAddOutgoingAdvanceSchedule());
                return addOutgoingAdvanceSchedule;
            }
        }

        private bool BeforeOnAddOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then save outgoing button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;
        }

        private void OnAddOutgoingAdvanceSchedule()
        {
            try
            {
                if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true) return;

                if (OutgoingAdvanceScheduleLst == null)
                    OutgoingAdvanceScheduleLst = new PolicyOutgoingSchedule();

                if (SelectedOutgoingAdvanceSchedule == null) return;
                if (OutgoingSelecetdPayee == null || OutgoingSelecetdPayee.UserCredentialID == null || OutgoingSelecetdPayee.UserCredentialID == Guid.Empty) return;

                if (SelecetdOutgoingAdvanceScheduleTypes.ScheduleTypeId != 5)
                {

                    if (NewOutSheduleEntry.FromRange == null) return;
                    if (NewOutSheduleEntry.ToRange == null) return;
                    if (NewOutSheduleEntry.FromRange > NewOutSheduleEntry.ToRange) return;
                }


                if (OutgoingAdvanceScheduleLst.OutgoingScheduleList == null)
                    OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>();

                if (NewOutSheduleEntry.CoveragesScheduleId == Guid.Empty)
                {
                    NewOutSheduleEntry.CoveragesScheduleId = Guid.NewGuid();
                    OutgoingScheduleEntry entry = NewOutSheduleEntry.Clone() as OutgoingScheduleEntry;
                    entry.PayeeUserCredentialId = OutgoingSelecetdPayee.UserCredentialID;
                    OutgoingAdvanceScheduleLst.OutgoingScheduleList.Add(entry);
                }
                else
                {
                    OutgoingScheduleEntry entry = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewOutSheduleEntry.CoveragesScheduleId);
                    if (entry != null)
                    {
                        entry.PayeeUserCredentialId = OutgoingSelecetdPayee.UserCredentialID;
                        entry.PayeeName = OutgoingSelecetdPayee.NickName;
                        entry.EffectiveFromDate = NewOutSheduleEntry.EffectiveFromDate;
                        entry.EffectiveToDate = NewOutSheduleEntry.EffectiveToDate;
                        entry.FromRange = NewOutSheduleEntry.FromRange;
                        entry.ToRange = NewOutSheduleEntry.ToRange;
                        entry.Rate = NewOutSheduleEntry.Rate;
                    }
                }

                if (OutgoingAdvanceScheduleLst.OutgoingScheduleList != null && OutgoingAdvanceScheduleLst.OutgoingScheduleList.Count == 1)
                    SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault(s => s.UserCredentialID == OutgoingAdvanceScheduleLst.OutgoingScheduleList[0].PayeeUserCredentialId);

                OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>(OutgoingAdvanceScheduleLst.OutgoingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).OrderBy(s => s.PayeeUserCredentialId).ToList());

            }
            catch (Exception)
            {
            }
        }

        private void ArrangePayeeRecordForDate()
        {
            throw new NotImplementedException();
        }

        private ICommand removeOutgoingAdvanceSchedule;
        public ICommand RemoveOutgoingAdvanceSchedule
        {
            get
            {
                if (removeOutgoingAdvanceSchedule == null)
                    removeOutgoingAdvanceSchedule = new BaseCommand(param => BeforeOnRemoveOutgoingAdvanceSchedule(),param => OnRemoveOutgoingAdvanceSchedule());
                return removeOutgoingAdvanceSchedule;

            }
        }

        private bool BeforeOnRemoveOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then remove out going shedule button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;
           
            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;
        }

        private void OnRemoveOutgoingAdvanceSchedule()
        {
            try
            {
                if (SelectedOutgoingAdvanceSchedule == null || SelectedOutgoingAdvanceSchedule.CoveragesScheduleId == Guid.Empty)
                {
                    MessageBox.Show("Invalid Operation-No Record Selected", "Warning", MessageBoxButton.OK);
                    return;
                }

                if (SelectedOutgoingAdvanceSchedule == null) return;
                MessageBoxResult result = MessageBox.Show("Do you want to remove Outgoing Schedule Record",
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;

                if (NewOutSheduleEntry != null && OutgoingAdvanceScheduleLst.OutgoingScheduleList != null)
                {
                    OutgoingScheduleEntry entry = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewOutSheduleEntry.CoveragesScheduleId);
                    if (entry != null)
                    {
                        OutgoingAdvanceScheduleLst.OutgoingScheduleList.Remove(entry);

                        if (OutgoingAdvanceScheduleLst.OutgoingScheduleList.Count != 0)
                        {
                            NewOutSheduleEntry = OutgoingAdvanceScheduleLst.OutgoingScheduleList[0].Clone() as OutgoingScheduleEntry;
                        }
                        else
                        {
                            NewOutSheduleEntry = new OutgoingScheduleEntry();
                            return;
                        }

                        if (OutgoingAdvanceScheduleLst.OutgoingScheduleList != null && OutgoingAdvanceScheduleLst.OutgoingScheduleList.Count == 1)
                        {
                            SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault(s => s.UserCredentialID == OutgoingAdvanceScheduleLst.OutgoingScheduleList[0].PayeeUserCredentialId);
                        }

                        OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>(OutgoingAdvanceScheduleLst.OutgoingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).OrderBy(s => s.PayeeUserCredentialId).ToList());
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private ICommand saveOutgoingAdvanceSchedule;
        public ICommand SaveOutgoingAdvanceSchedule
        {
            get
            {
                if (saveOutgoingAdvanceSchedule == null)
                    saveOutgoingAdvanceSchedule = new BaseCommand(param => BeforeOnSaveOutgoingAdvanceSchedule(),param => OnSaveOutgoingAdvanceSchedule());
                return saveOutgoingAdvanceSchedule;

            }
        }

        private bool BeforeOnSaveOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check policy is saved or not 
            //if not then add button will disable
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            
                return true;
        }

        private void OnSaveOutgoingAdvanceSchedule()
        {

            try
            {
                if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                {
                    MessageBox.Show("Invalid Operation-No Policy Selected", "Warning", MessageBoxButton.OK);
                    return;
                }

                if (OutgoingAdvanceScheduleLst == null || OutgoingAdvanceScheduleLst.OutgoingScheduleList == null)
                    return;

                if (SelectedPrimaryAgent == null)
                {
                    MessageBox.Show("No Primary Agent is There", "Error", MessageBoxButton.OK);
                    return;
                }

                if (OutgoingAdvanceScheduleLst.OutgoingScheduleList.Count != 0)
                {
                    OutgoingScheduleEntry entry = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault(s => s.PayeeUserCredentialId == SelectedPrimaryAgent.UserCredentialID);
                    if (entry == null)
                    {
                        MessageBox.Show("Primary Agent is not available in outgoing schedule. Please check.", "Error", MessageBoxButton.OK);
                        return;
                    }
                }

                OutgoingAdvanceScheduleLst.OutgoingScheduleList.ToList().ForEach(s => s.IsPrimaryAgent = false);
                OutgoingAdvanceScheduleLst.OutgoingScheduleList.Where(s => s.PayeeUserCredentialId == SelectedPrimaryAgent.UserCredentialID).ToList().ForEach(s => s.IsPrimaryAgent = true);
                OutgoingAdvanceScheduleLst.ScheduleTypeId = SelecetdOutgoingAdvanceScheduleTypes.ScheduleTypeId;
                serviceClients.OutgoingScheduleClient.AddUpdateOutgoingShedule(OutgoingAdvanceScheduleLst);
                _SavedOutShedule = null;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///  This schedule is used for saving actual schedule before any modification so that we can restore
        ///  original schedule if the opetration is cancelled.
        /// </summary>
        private GlobalIncomingSchedule _SavedOutShedule;

        private OutgoingScheduleEntry selectedOutgoingAdvanceSchedule;
        public OutgoingScheduleEntry SelectedOutgoingAdvanceSchedule
        {
            get { return selectedOutgoingAdvanceSchedule == null ? new OutgoingScheduleEntry() : selectedOutgoingAdvanceSchedule; }
            set { selectedOutgoingAdvanceSchedule = value; OnPropertyChanged("SelectedOutgoingAdvanceSchedule"); }
        }

        private OutgoingScheduleEntry _NewOutSheduleEntry;
        public OutgoingScheduleEntry NewOutSheduleEntry
        {
            get
            {
                return _NewOutSheduleEntry == null ? new OutgoingScheduleEntry() : _NewOutSheduleEntry;
            }
            set
            {
                _NewOutSheduleEntry = value;
                OnPropertyChanged("NewOutSheduleEntry");
            }
        }

        private PolicyOutgoingSchedule outgoingAdvanceScheduleLst;
        public PolicyOutgoingSchedule OutgoingAdvanceScheduleLst
        {
            get { return outgoingAdvanceScheduleLst; }
            set { outgoingAdvanceScheduleLst = value; OnPropertyChanged("OutgoingAdvanceScheduleLst"); }
        }


        private string rateOutgoingColumnName;
        public string RateOutgoingColumnName
        {
            get { return rateOutgoingColumnName; }
            set { rateOutgoingColumnName = value; OnPropertyChanged("RateOutgoingColumnName"); }
        }


        private string _fromOutgoingRangeVisibility = System.Windows.Visibility.Visible.ToString();
        public string FromOutgoingRangeVisibility
        {
            get { return _fromOutgoingRangeVisibility; }
            set
            {
                _fromOutgoingRangeVisibility = value;
                OnPropertyChanged("FromOutgoingRangeVisibility");
            }
        }

        private string _toOutgoingRangeVisibility = System.Windows.Visibility.Visible.ToString();
        public string ToOutgoingRangeVisibility
        {
            get { return _toOutgoingRangeVisibility; }
            set
            {
                _toOutgoingRangeVisibility = value;
                OnPropertyChanged("ToOutgoingRangeVisibility");
            }
        }

        private string _commissionOutgoingVisibility = System.Windows.Visibility.Visible.ToString();
        public string CommissionOutgoingVisibility
        {
            get { return _commissionOutgoingVisibility; }
            set
            {
                _commissionOutgoingVisibility = value;
                OnPropertyChanged("CommissionOutgoingVisibility");
            }
        }

        private string _fromOutgoingText;
        public string FromOutgoingText
        {
            get
            {
                return _fromOutgoingText;
            }
            set
            {
                _fromOutgoingText = value;
                OnPropertyChanged("FromOutgoingText");

            }
        }

        /// <summary>
        /// set commisition test 
        /// </summary>
        private string _commOutgoingText;
        public string CommOutgoingTText
        {
            get
            {
                return _commOutgoingText;
            }
            set
            {
                _commOutgoingText = value;
                OnPropertyChanged("CommOutgoingTText");

            }
        }

        /// <summary>
        /// Set To test
        /// </summary>
        private string _toOutgoingText;
        public string ToOutgoingText
        {
            get
            {
                return _toOutgoingText;
            }
            set
            {
                _toOutgoingText = value;
                OnPropertyChanged("ToOutgoingText");

            }
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

        private ObservableCollection<User> _PrimaryAgents;
        public ObservableCollection<User> PrimaryAgents
        {
            get { return _PrimaryAgents; }
            set { _PrimaryAgents = value; OnPropertyChanged("PrimaryAgents"); }
        }

        private PolicyOutgoingScheduleType selectedOutgoingAdvanceScheduleTypes;
        public PolicyOutgoingScheduleType SelecetdOutgoingAdvanceScheduleTypes
        {
            get
            {
                return selectedOutgoingAdvanceScheduleTypes == null ? new PolicyOutgoingScheduleType() : selectedOutgoingAdvanceScheduleTypes;
            }
            set
            {
                selectedOutgoingAdvanceScheduleTypes = value;
                OnPropertyChanged("SelecetdOutgoingAdvanceScheduleTypes");
            }
        }
        private ObservableCollection<PolicyOutgoingScheduleType> outgoingAdvanceScheduleTypes;
        public ObservableCollection<PolicyOutgoingScheduleType> OutgoingAdvanceScheduleTypes
        {
            get
            {
                return outgoingAdvanceScheduleTypes;
            }
            set
            {
                outgoingAdvanceScheduleTypes = value;
                OnPropertyChanged("OutgoingAdvanceScheduleTypes");
            }
        }

        private void FillOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                //OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>();
                OutgoingAdvanceScheduleLst = new PolicyOutgoingSchedule();
                SelectedOutgoingAdvanceSchedule = null;
                SelectedPrimaryAgent = null;
                return;
            }

            serviceClients.OutgoingScheduleClient.GetOutgoingSheduleByAsync(SelectedPolicy.PolicyId);
        }

        private void FillOutgoingAdvanceSchedule(Guid PolicyId)
        {
            if (PolicyId == Guid.Empty)
            {
                //OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>();
                OutgoingAdvanceScheduleLst = new PolicyOutgoingSchedule();
                SelectedOutgoingAdvanceSchedule = null;
                SelectedPrimaryAgent = null;
                return;
            }

            serviceClients.OutgoingScheduleClient.GetOutgoingSheduleByAsync(PolicyId);
        }

        void OutgoingScheduleClient_GetOutgoingSheduleByCompleted(object sender, GetOutgoingSheduleByCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OutgoingAdvanceScheduleLst = e.Result;

                if (OutgoingAdvanceScheduleLst == null)
                    OutgoingAdvanceScheduleLst = new PolicyOutgoingSchedule();

                if (OutgoingAdvanceScheduleLst.OutgoingScheduleList == null)
                    OutgoingAdvanceScheduleLst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>();

                SelectedOutgoingAdvanceSchedule = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault();
                OutgoingScheduleEntry entry = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault(s => s.IsPrimaryAgent = true);

                SelectedPrimaryAgent = null;
                if (entry != null && PrimaryAgents!= null)
                    SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault(s => s.UserCredentialID == entry.PayeeUserCredentialId);
            }
        }

        private ObservableCollection<User> FillOutgoingPayeeUser()
        {
            ObservableCollection<User> AgentList= null;
            try
            {
                if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                {
                    AgentList = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);
                }
                else
                {
                    AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SelectedLicensee.LicenseeId, UserRole.Agent).OrderBy(p => p.NickName));
                    SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);
                }
            }
            catch
            {
            }
            return AgentList;
        }

        public void CopyOutgoingScheduleFrom(Guid policyId)
        {
            try
            {
                FillOutgoingAdvanceSchedule(policyId);
            }
            catch
            {
            }
        }

        #endregion
    }
}
