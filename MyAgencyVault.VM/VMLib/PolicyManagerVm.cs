using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.VMLib;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using MyAgencyVault.ViewModel.VMLib;
using System.Transactions;
using System.Collections.Specialized;
using MyAgencyVault.ViewModel.Converters;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VMLib
{
    public class PolicyManagerVm : BaseViewModel, IDataRefresh
    {
        static MastersClient objLog = new MastersClient();
        private bool FollowUpcalled;
        // private bool IsSavePolicyBtnCmd = false;
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
        private bool replaceRbStatus = true;
        // private bool replaceRbStatus = false;
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

        public static ScheduleGridData ScheduleGrdData;
        public static ScheduleGridData OutScheduleGrdData;

        /// <summary>
        /// It is a constructor for the class for default data loading in the policy manager tab.
        /// </summary>
        public PolicyManagerVm(PolicyClientVm objClientVm)
        {
           
            IsActiceChecked = true;
            ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();
            ShowHidecheckScheduleBtn = DesignerSerializationVisibility.Visible.ToString();
            PropertyChanged += new PropertyChangedEventHandler(PolicyManagerVm_PropertyChanged);
            policyStatus = _PolicyStatus.Active;
            try
            {
                PolicyScreenControl();
                MasterPolicyStatus = serviceClients.MasterClient.GetPolicyStatusList();
                PolicyTerminationtReasonLst = serviceClients.MasterClient.GetTerminationReasonListWithBlankAdded();
                MasterIncomingPaymentTypeLst = serviceClients.MasterClient.GetPolicyIncomingPaymentTypeList();
                LearnedMasterIncomingPaymentTypeLst = serviceClients.MasterClient.GetPolicyIncomingPaymentTypeList();

                MasterPaymentsModeData = serviceClients.MasterClient.GetPolicyModeListWithBlankAdded();
                LrndMasterPaymentsModeData = serviceClients.MasterClient.GetPolicyModeListWithBlankAdded();

                IncomingAdvanceScheduleTypes = serviceClients.MasterClient.GetPolicyIncomingScheduleTypeList();
                SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault();
                OutgoingAdvanceScheduleTypes = serviceClients.MasterClient.GetPolicyOutgoingScheduleTypeList();
                SelecetdOutgoingAdvanceScheduleTypes = OutgoingAdvanceScheduleTypes.FirstOrDefault();

                _SelecetdPolicyClientVm = objClientVm;

                if (_NewSheduleEntry == null)
                    _NewSheduleEntry = new IncomingScheduleEntry();

                if (_NewOutSheduleEntry == null)
                    _NewOutSheduleEntry = new OutgoingScheduleEntry();
            }
            catch
            {
            }
        }


        #region ControlLevelProerty
        #region PolicyScreen

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

        private bool addpayee = false;
        public bool AddPaeeButton
        {
            get { return addpayee; }
            set { addpayee = value; OnPropertyChanged("AddPaeeButton"); }
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

        #endregion
        #endregion

        void PolicyManagerVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //#step2
                case "SelectedLicensee":
                    break;
                case "SelectedDisplayClient":
                    try
                    {
                        PolicyList = FillPolicyGrid();

                        LastViewPolicy m_LastViewPolicy=new LastViewPolicy();// = LastViewPolicyClientCollection.getLastViewedPolicyForClient(SelectedDisplayClient.ClientId);

                        if (m_LastViewPolicy != null)
                        {
                            for (int i = 0; i < policyList.Count; i++)
                            {
                                if (policyList[i].PolicyId == m_LastViewPolicy.PolicyId)
                                {
                                    SelectedPolicy = policyList[i];
                                    break;
                                }
                            }
                        }
                        if (PolicyList.Count() != 0 && SelectedPolicy.PolicyId == Guid.Empty)
                        {
                            SelectedPolicy = PolicyList.FirstOrDefault();
                        }
                    }
                    catch
                    {
                    }

                    break;

                case "SelectedPolicy":

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
                    string SubThr = SelectedPolicy.SubmittedThrough;
                    SelectedPayor = PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault();
                    SubmittedThrough = SubmittedThroughLst.Where(p => p.PayorName == SubThr).FirstOrDefault();
                    SelectedTermReason = PolicyTerminationtReasonLst.Where(p => p.TerminationReasonId == SelectedPolicy.TerminationReasonId).FirstOrDefault();
                    SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelectedPolicy.IncomingPaymentTypeId).FirstOrDefault();


                    SelectedPolicyToolIncommingShedule = FillIncomingBasicSchedule();

                    OutGoingField = FillBasicOutGoingSchedule();
                    if (SelectedPolicy.IsOutGoingBasicSchedule == true)
                        SelectedOutGoingField = OutGoingField.FirstOrDefault();
                    //---------
                    if (SelectedOutGoingField.OutgoingScheduleId == Guid.Empty && (SelectedPolicy.IsOutGoingBasicSchedule ?? false))
                    {
                        OutPercentOfCommission = true;

                    }
                    //-----------------
                    SelecetdPolicyLearnedField = FillLrndField();

                    PolicyNote = FillPolicyNotes();
                    SelectNote = PolicyNote.FirstOrDefault();

                    FillIncomingAdvanceSchedule();
                    SelectedIncomingAdvanceSchedule = IncomingAdvanceScheduleLst.IncomingScheduleList.FirstOrDefault();

                    OutgoingAdvanceScheduleLst = FillOutgoingAdvanceSchedule();
                    SelectedOutgoingAdvanceSchedule = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault();


                    OutgoingScheduleEntry entry = OutgoingAdvanceScheduleLst.OutgoingScheduleList.FirstOrDefault(s => s.IsPrimaryAgent = true);
                    SelectedPrimaryAgent = null;
                    if (entry != null)
                        SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault(s => s.UserCredentialID == entry.PayeeUserCredentialId);

                    PolicyIncomingPaymentCommissionDashBoard = FillIncomingPaymentCommissionDashBoard();
                    PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();

                    PolicyFollowUpCommissionDashBoardLst = FillFollowUpIssue();
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst == null ? null : PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();



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
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("PolicyId", SelectedPolicy.ReplacedBy ?? Guid.Empty);
                        PolicyDetailsData _Policy = serviceClients.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                        ReplaceBtntooltip = "Carr : " + _Policy.CarrierName + ", # : " + _Policy.PolicyNumber;
                        ReplaceBtntooltip = ReplaceBtntooltip.Trim();

                    }
                    if (SelectedPolicy.IsOutGoingBasicSchedule.HasValue)
                    {
                        if (SelectedPolicy.IsOutGoingBasicSchedule.Value)
                        {
                            OutAdvance = false;
                        }
                        else
                        {
                            OutAdvance = true;

                        }
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
                        {
                            IncAdvance = false;
                        }
                        else
                        {
                            IncAdvance = true;
                        }
                    }
                    else
                    {
                        IncPercentOfPremium = false;
                        IncPerHead = false;
                        IncAdvance = false;
                    }

                    AddSelectedClientToLastTenViewed();

                    break;
                case "ReplaceRbStatus":
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();
                    break;
                case "NewRbStatus":
                    ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();
                    break;

                case "SelectedStatus":
                    SelectedPolicy.PolicyStatusId = SelectedStatus.StatusId;
                    if (SelectedPolicy.PolicyStatusName == "Deleted") return;
                    SelectedPolicy.PolicyStatusName = GetPolicayStatusName(SelectedPolicy.PolicyStatusId);
                    break;
                case "SelectedClient":
                    if (SelectedPolicy == null) return;
                    SelectedPolicy.ClientId = SelectedClient.ClientId;
                    break;
                case "SelectedPaymentMode":
                    SelectedPolicy.PolicyModeId = SelectedPaymentMode.ModeId;
                    break;
                case "SelectedPayor":
                    SelectedPolicy.PayorId = (SelectedPayor == null || SelectedPayor.PayorID == Guid.Empty) ? Guid.Empty : SelectedPayor.PayorID;
                    SelectedPolicy.PayorName = SelectedPolicy.PayorId == Guid.Empty || SelectedPolicy.PayorId == null ? "" : PayorsLst.Where(p => p.PayorID == SelectedPolicy.PayorId).FirstOrDefault().PayorName;
                    SubmittedThroughLst = FillSubmitedThrough();


                    CarriersLst = FillCarrierLst();

                    if (CarriersLst != null)
                        SelectedCarrier = CarriersLst.Where(p => p.CarrierId == SelectedPolicy.CarrierID).FirstOrDefault();

                    break;
                case "SelectedCarrier":
                    if ((SelectedCarrier != null && SelectedCarrier.CarrierId == Guid.Empty))
                    {
                        SelectedPolicy.CarrierID = null;
                        SelectedPolicy.CarrierName = null;
                    }
                    else if (SelectedCarrier != null && SelectedCarrier.CarrierId != Guid.Empty)
                    {
                        SelectedPolicy.CarrierID = CarriersLst.Where(p => p.CarrierId == SelectedCarrier.CarrierId).FirstOrDefault().CarrierId;
                        SelectedPolicy.CarrierName = SelectedPolicy.CarrierID == Guid.Empty || SelectedPolicy.CarrierID == null ? "" : CarriersLst.Where(p => p.CarrierId == SelectedCarrier.CarrierId).FirstOrDefault().CarrierName;
                    }
                    ProductsLst = FillCoverageLst();
                    if (ProductsLst != null)
                        SelectedProduct = ProductsLst.Where(p => p.CoverageID == SelectedPolicy.CoverageId).FirstOrDefault();
                    break;
                case "SelectedProduct":
                    if ((SelectedProduct != null && SelectedProduct.CoverageID == Guid.Empty))
                    {
                        SelectedPolicy.CoverageId = null;
                        SelectedPolicy.CoverageName = null;

                    }
                    else if (SelectedProduct != null && SelectedProduct.CoverageID != Guid.Empty)
                    {
                        SelectedPolicy.CoverageId = (SelectedProduct == null || SelectedProduct.CoverageID == Guid.Empty) ? Guid.Empty : SelectedProduct.CoverageID;
                        SelectedPolicy.CoverageName = SelectedPolicy.CoverageId == Guid.Empty || SelectedPolicy.CoverageId == null ? "" : ProductsLst.Where(p => p.CoverageID == SelectedProduct.CoverageID).FirstOrDefault().Name;
                    }
                    break;
                case "SubmittedThrough":
                    SelectedPolicy.SubmittedThrough = SubmittedThrough.PayorName;
                    break;
                case "SelectedTermReason":
                    SelectedPolicy.TerminationReasonId = SelectedTermReason.TerminationReasonId;
                    break;
                case "SelectedMasterIncomingPaymentType":
                    SelectedPolicy.IncomingPaymentTypeId = SelectedMasterIncomingPaymentType.PaymentTypeId;
                    break;
                case "SelectedPolicyToolIncommingShedule":
                    if (SelectedPolicyToolIncommingShedule == null)
                    {
                        IncPercentOfPremium = true;
                        IncPerHead = false;
                        IncAdvance = false;
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
                        SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                        SelectedPolicy.IsIncomingBasicSchedule = true;
                    }
                    break;
                case "IncPercentOfPremium":
                    if (IncPercentOfPremium == true)
                    {
                        FirstYearText = "First Year";
                        Renewaltext = "Renewal";
                        if (SelectedPolicyToolIncommingShedule == null) SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule()
                            {
                                FirstYearPercentage = 0,
                                RenewalPercentage = 0,
                            };
                        SelectedPolicyToolIncommingShedule.PolicyId = SelectedPolicy.PolicyId;
                        SelectedPolicyToolIncommingShedule.ScheduleTypeId = 1;
                        SelectedPolicy.IsIncomingBasicSchedule = true;
                    }

                    break;
                case "IncAdvance":
                    if (IncAdvance == true)
                    {
                        SelectedPolicy.IsIncomingBasicSchedule = false;
                        IncPercentOfPremium = false;
                        IncPerHead = false;
                    }

                    break;

                case "SelectedOutGoingField":
                    SelectedOutGoingField.PropertyChanged += new PropertyChangedEventHandler(SelectedOutGoingField_PropertyChanged);
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
                    if (OutPercentOfCommission)
                    {
                        SelectedOutGoingField.ScheduleTypeId = 2;
                        if (OutGoingField != null)
                            OutGoingField.ToList().ForEach(p => p.ScheduleTypeId = 2);
                        SelectedPolicy.IsOutGoingBasicSchedule = true;
                    }
                    break;
                case "OutPercentOfPremium":
                    if (OutPercentOfPremium)
                    {
                        SelectedOutGoingField.ScheduleTypeId = 1;
                        if (OutGoingField != null)
                            OutGoingField.ToList().ForEach(p => p.ScheduleTypeId = 1);
                        SelectedPolicy.IsOutGoingBasicSchedule = true;
                    }


                    break;
                case "OutAdvance":
                    if (OutAdvance)
                    {
                        SelectedPolicy.IsOutGoingBasicSchedule = false;
                    }

                    break;

                case "SelectNote":
                    RTFNotecontent = SelectNote.Content;
                    break;
                case "SelecetdPolicyLearnedField":
                    if (SelecetdPolicyLearnedField == null || SelecetdPolicyLearnedField.PolicyId == Guid.Empty)
                    {
                        SelectedClientLrnd = null;
                        SelectedMasterIncomingPaymentTypeLrnd = null;
                        SelectedPaymentModeLrnd = null;
                        return;
                    }

                    SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == SelecetdPolicyLearnedField.ClientID).FirstOrDefault();
                    SelectedMasterIncomingPaymentTypeLrnd = LearnedMasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelecetdPolicyLearnedField.CompTypeId).FirstOrDefault();
                    SelectedPaymentModeLrnd = LrndMasterPaymentsModeData.Where(p => p.ModeId == SelecetdPolicyLearnedField.PolicyModeId).FirstOrDefault();

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

                    OutgoingSelecetdPayee = OutgoingPayeeList.Where(p => p.UserCredentialID == SelectedOutgoingAdvanceSchedule.PayeeUserCredentialId).FirstOrDefault();

                    break;
                case "PolicySelectedIncomingPaymentCommissionDashBoard":

                    CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                    CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst == null ? null : CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();

                    PolicyFollowUpCommissionDashBoardLst = FillFollowUpIssue();
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    break;

                case "SelectedCommDashPayee":
                    NewPayeeToPay = "";
                    NewPayeeToPay = SelectedCommDashPayee.NickName;
                    NewPayeeToPay += " to pay";
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
                        SelectedReplacePolicyTermReason = PolicyReplacePolicyTerminationtReasonLst
                                    .Where(p => p.TerminationReasonId == 0).FirstOrDefault();
                    }

                    break;
                default:
                    break;
            }
        }

        private void AddSelectedClientToLastTenViewed()
        {
            Guid? PolicyId = null;
            if (SelectedPolicy != null || SelectedPolicy.PolicyId == Guid.Empty)
                PolicyId = SelectedPolicy.PolicyId;

           // LastViewPolicyClientCollection.LastViewedClients.ClientOrPolicyChanged(SelectedDisplayClient.ClientId, SelectedDisplayClient.Name, PolicyId, false);
        }

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
            return PolicyStatusdata;
        }
        

        #region PolicySettingUpdate

        private ICommand _ClickPrimaryAgent;
        public ICommand ClickPrimaryAgent
        {
            get
            {
                if (_ClickPrimaryAgent == null)
                {
                    _ClickPrimaryAgent = new BaseCommand(x => BeforeClickPrimaryAgent(), x => ClickPrimaryAgentCmd());
                }
                return _ClickPrimaryAgent;
            }
        }

        private bool BeforeClickPrimaryAgent()
        {
            throw new NotImplementedException();
        }

        private object ClickPrimaryAgentCmd()
        {
            throw new NotImplementedException();
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }


        public void UpdatePolicySetting()
        {

            if (SelectedPolicy == null) return;
            serviceClients.PolicyClient.UpdatePolicySetting(SelectedPolicy);

        }
        #endregion
        
        #region ClientVM
        PolicyClientVm _SelecetdPolicyClientVm;

        #endregion

        #region PolicyNotes
        private ObservableCollection<PolicyNotes> FillPolicyNotes()
        {
            ObservableCollection<PolicyNotes> PolicyNotesLst = serviceClients.NoteClient.GetNotesPolicyWise(SelectedPolicy.PolicyId);
            return PolicyNotesLst;
        }

        private ObservableCollection<PolicyNotes> _policyNote;
        public ObservableCollection<PolicyNotes> PolicyNote
        {
            get
            {
                return _policyNote;
            }

            set
            {
                _policyNote = value;
                OnPropertyChanged("PolicyNote");
            }

        }

        private PolicyNotes _selectNote;
        public PolicyNotes SelectNote
        {
            get
            {
                return _selectNote == null ? new PolicyNotes() : _selectNote;
            }

            set
            {
                _selectNote = value;
                OnPropertyChanged("SelectNote");
            }
        }




        private ICommand _newNote;
        public ICommand NewNote
        {
            get
            {
                if (_newNote == null)
                {
                    _newNote = new BaseCommand(X => BeforeCreateNewNote(), X => CreateNewNote());
                }
                return _newNote;
            }

        }

        private bool BeforeCreateNewNote()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            // if (string.IsNullOrEmpty(RTFNotecontent))
            //return false;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;

        }

        private string _notecontent;
        public string RTFNotecontent
        {
            get { return _notecontent; }
            set { _notecontent = value; OnPropertyChanged("RTFNotecontent"); }
        }

        private void CreateNewNote()
        {
            //  if (string.IsNullOrEmpty(Notecontent)) return;
            RTFNotecontent = "";
            PolicyNotes _PolicyNote = new PolicyNotes()
            {
                NoteID = Guid.NewGuid(),
                PolicyID = SelectedPolicy.PolicyId,
                //Content = string.Empty,
                Content = RTFNotecontent,
                CreatedDate = DateTime.Today,
                LastModifiedDate = DateTime.Today,
            };
            SelectNote = _PolicyNote;
            // PolicyNote.Add(_PolicyNote);
            //PolicyNote = new ObservableCollection<PolicyNotes>(PolicyNote.OrderByDescending(p => p.CreatedDate));
            //SelectNote = PolicyNote.Where(p => p.NoteID == _PolicyNote.NoteID).FirstOrDefault();
            //  serviceClients.NoteClient.AddUpdateNote(SelectNote);
            //  SelectNote = _PolicyNote;
        }

        private ICommand _AddNote;
        public ICommand AddNote
        {
            get
            {
                if (_AddNote == null)
                {
                    _AddNote = new BaseCommand(X => BeforeAddNewNote(), X => AddNewNote());
                }
                return _AddNote;
            }

        }

        private bool BeforeAddNewNote()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            if (string.IsNullOrEmpty(RTFNotecontent))
                return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void AddNewNote()
        {

            if (RTFNotecontent == null || RTFNotecontent == string.Empty) return;

            if (SelectedPolicy.PolicyId == Guid.Empty) return;
            SelectNote.Content = RTFNotecontent;

            RTFToTextConverter converter = new RTFToTextConverter();
            SelectNote.SimpleTextContent = converter.Convert(SelectNote.Content);

            SelectNote.LastModifiedDate = DateTime.Today;
            serviceClients.NoteClient.AddUpdateNote(SelectNote);
            //PolicyNote.Add(SelectNote);
            if (!PolicyNote.Contains(SelectNote))
            {
                PolicyNote.Add(SelectNote);
                PolicyNote = new ObservableCollection<PolicyNotes>(PolicyNote.OrderByDescending(p => p.CreatedDate));

            }
            // SelectNote = PolicyNote.Where(p => p.NoteID == _PolicyNote.NoteID).FirstOrDefault();


        }
        private ICommand _deleteNote;
        public ICommand DeleteNote
        {
            get
            {
                if (_deleteNote == null)
                {
                    _deleteNote = new BaseCommand(X => BeforeDODeleteNote(), X => DODeleteNote());
                }
                return _deleteNote;
            }

        }

        private bool BeforeDODeleteNote()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            if (string.IsNullOrEmpty(RTFNotecontent))
                return false;

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void DODeleteNote()
        {
            if (SelectNote.NoteID == Guid.Empty) return;
            if (MessageBox.Show("Do you want to delete Selecetd Note", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }
            serviceClients.NoteClient.DeleteNote(SelectNote);
            if (PolicyNote.Contains(SelectNote))
            {
                PolicyNote.Remove(SelectNote);
                SelectNote = PolicyNote.FirstOrDefault();
            }



        }

        private string _showHideTypeOfPolicy;
        public string ShowHideTypeOfPolicy
        {
            get { return _showHideTypeOfPolicy; }
            set { _showHideTypeOfPolicy = value; OnPropertyChanged("ShowHideTypeOfPolicy"); }
        }
        private string _showHidecheckScheduleBtn;
        public string ShowHidecheckScheduleBtn
        {
            get { return _showHidecheckScheduleBtn; }
            set { _showHidecheckScheduleBtn = value; OnPropertyChanged("ShowHidecheckScheduleBtn"); }
        }
        
        #endregion

        #region PolicyDetail

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
        ////-----

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
        private void OnStatusChanged(object param)
        {
            string clickedLink = param as string;
            switch (clickedLink)
            {
                case "Active":
                    policyStatus = _PolicyStatus.Active;

                    break;
                case "Pending":
                    policyStatus = _PolicyStatus.Pending;
                    break;
                case "Terminated":
                    policyStatus = _PolicyStatus.Terminated;

                    break;
                case "Deleted":
                    policyStatus = _PolicyStatus.Delete;

                    break;
                case "All":
                    policyStatus = _PolicyStatus.Any;

                    break;
                default:
                    break;
            }
            PolicyList = FillPolicyGrid();
            SelectedPolicy = PolicyList.FirstOrDefault();
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

        private bool BeforeSavePoliciesData()
        {
            PolicyDetailSaveToolTip = null;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
            {
                PolicyDetailSaveToolTip = "Security Violation";
                return false;
            }
            else if (OutGoingField != null && OutGoingField.Count != 0)
            {
                if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2))
                {
                    if ((OutGoingField.ToList().Sum(p => p.FirstYearPercentage) != 100) || (OutGoingField.ToList().Sum(p => p.RenewalPercentage) != 100))
                    {
                        PolicyDetailSaveToolTip = "FirstYear/Renewal not equal to 100";
                        return false;
                    }

                }
            }

            if (SelectedClient.ClientId == Guid.Empty)
            {
                PolicyDetailSaveToolTip = "No Client Selected";
                return false;
            }
            if (OutGoingField != null && OutGoingField.Count != 0 && OutGoingField.Where(p => p.IsPrimaryAgent).Count() != 1)
            {
                PolicyDetailSaveToolTip = "No Primary Agent";
                return false;
            }

            if (OutPercentOfPremium && (OutGoingField != null && OutGoingField.Count != 0))
            {
                if ((OutGoingField.Sum(p => p.FirstYearPercentage) != SelectedPolicyToolIncommingShedule.FirstYearPercentage) || (OutGoingField.Sum(p => p.RenewalPercentage) != SelectedPolicyToolIncommingShedule.RenewalPercentage))
                {
                    PolicyDetailSaveToolTip = "Outgoing FirstYear/Renewal not equal to Outgoing FirstYear/Renewal";
                    return false;
                }
            }
            if (SelectedPolicy.TrackFromDate != null && SelectedPolicy.OriginalEffectiveDate != null)
            {
                if (SelectedPolicy.TrackFromDate < SelectedPolicy.OriginalEffectiveDate)
                {
                    PolicyDetailSaveToolTip = "Track from date should less than Original effective date";
                    return false;
                }
            }

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
            {
                PolicyDetailSaveToolTip = "no selected policy";
                return false;
            }
            if (SelectedPolicy.IsSavedPolicy && SelectedPolicy.PolicyStatusId == 2)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("PolicyId", SelectedPolicy.PolicyId);
                PolicyDetailsData _Policy = serviceClients.PolicyClient.GetPolicydata(parameters).FirstOrDefault();
                if (_Policy != null)
                {
                    if (_Policy.PolicyStatusId == 0)
                    {
                        PolicyDetailSaveToolTip = "Cannot change Active to Pending";
                        return false;
                    }

                }
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void CancelPoliciesData()
        {
            if (SelectedPolicy.PolicyId == Guid.Empty) return;
            SelectedDisplayClient = DisplayedClientsLists.Where(p => p.ClientId == SelectedDisplayClient.ClientId).FirstOrDefault();
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
        //------------------------------------------------------------------------------------------------------------------------
        #region PolicyCommandMethod


        private void DoDuplicatePolicy()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;
            if (SelectedPolicy.IsDeleted == true) return;
            PolicyDetailsData policy = new PolicyDetailsData();

            policy.PolicyId = Guid.NewGuid();
            policy.PolicyStatusId = SelectedPolicy.PolicyStatusId;
            policy.PolicyNumber = string.Empty;
            policy.PolicyType = SelectedPolicy.PolicyType;
            policy.ClientId = SelectedPolicy.ClientId;
            policy.PolicyLicenseeId = SelectedPolicy.PolicyLicenseeId;
            policy.Insured = string.Empty;
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

            SelectedPolicy = new PolicyDetailsData()
            {
                PolicyId = policy.PolicyId,
                PolicyStatusId = policy.PolicyStatusId,
                PolicyNumber = policy.PolicyNumber,
                PolicyType = policy.PolicyType,
                ClientId = policy.ClientId,
                PolicyLicenseeId = policy.PolicyLicenseeId,
                Insured = policy.Insured,
                OriginalEffectiveDate = policy.OriginalEffectiveDate,
                TrackFromDate = policy.TrackFromDate,
                PolicyModeId = policy.PolicyModeId,
                ModeAvgPremium = policy.ModeAvgPremium,
                CoverageId = policy.CoverageId,
                SubmittedThrough = policy.SubmittedThrough,
                Enrolled = policy.Enrolled,
                Eligible = policy.Eligible,
                PolicyTerminationDate = policy.PolicyTerminationDate,
                TerminationReasonId = policy.TerminationReasonId,
                IsTrackMissingMonth = policy.IsTrackMissingMonth,
                IsTrackIncomingPercentage = policy.IsTrackIncomingPercentage,
                IsTrackPayment = policy.IsTrackPayment,
                IsDeleted = policy.IsDeleted,
                ReplacedBy = policy.ReplacedBy,
                PayorId = policy.PayorId,
                DuplicateFrom = policy.DuplicateFrom,
                CreatedBy = policy.CreatedBy,
                CreatedOn = policy.CreatedOn,
                IsIncomingBasicSchedule = policy.IsIncomingBasicSchedule,
                CarrierID = policy.CarrierID,
                SplitPercentage = policy.SplitPercentage,
                IncomingPaymentTypeId = policy.IncomingPaymentTypeId,

            };
            UpdateUIByAddNewPolicy(SelectedPolicy);
        }
        public void UpdatePolicyPreviousData()
        {

        }
        private void SavePoliciesData()
        {
            bool ReplaceOccur = false;
            bool? ChangeInModeToRunFollowUp = null;
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;
            //IsSavePolicyBtnCmd = true;
            if (SelectedPolicy.PolicyType == "Replace")
            {
                MakeReplacePolicyToSave();
                if (SelecetdPolicylstForReplace != null)
                {
                    serviceClients.PolicyClient.AddUpdatePolicy(SelecetdPolicylstForReplace);
                    ReplaceOccur = true;

                    SelecetdPolicylstForReplace.PolicyPreviousData.OriginalEffectiveDate = SelecetdPolicylstForReplace.OriginalEffectiveDate;
                    SelecetdPolicylstForReplace.PolicyPreviousData.PolicyModeId = SelecetdPolicylstForReplace.PolicyModeId;
                    SelecetdPolicylstForReplace.PolicyPreviousData.TrackFromDate = SelecetdPolicylstForReplace.TrackFromDate;
                    SelecetdPolicylstForReplace.IsSavedPolicy = true;
                    //--Policy History Stuff
                    serviceClients.PolicyClient.AddUpdatePolicyHistory(SelecetdPolicylstForReplace.PolicyId);
                    serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearned(SelecetdPolicylstForReplace.PolicyId);
                    //-------------------------------
                }
            }
            else
            {
                ReplaceRbStatus = true;
            }

            if (SelectedPolicy != null)
            {
                //SelectedPolicy.CreatedBy=
                serviceClients.PolicyClient.AddUpdatePolicy(SelectedPolicy);
                if (SelectedPolicy.PolicyPreviousData == null)
                {
                    SelectedPolicy.PolicyPreviousData = new PolicyDetailPreviousData();
                }
                if (SelectedPolicy.PolicyPreviousData.OriginalEffectiveDate != SelectedPolicy.OriginalEffectiveDate)
                {
                    ChangeInModeToRunFollowUp = false;
                }
                else if (SelectedPolicy.PolicyPreviousData.PolicyModeId != SelectedPolicy.PolicyModeId)
                {
                    ChangeInModeToRunFollowUp = true;
                }
                else if (SelectedPolicy.PolicyPreviousData.TrackFromDate != SelectedPolicy.TrackFromDate)
                {
                    ChangeInModeToRunFollowUp = false;
                }

                SelectedPolicy.PolicyPreviousData.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                SelectedPolicy.PolicyPreviousData.PolicyModeId = SelectedPolicy.PolicyModeId;
                SelectedPolicy.PolicyPreviousData.TrackFromDate = SelectedPolicy.TrackFromDate;
                SelectedPolicy.IsSavedPolicy = true;

                if ((policyList.Where(p => p.PolicyId == SelectedPolicy.PolicyId).Count() == 0))
                    policyList.Add(SelectedPolicy);
            }
            else
            {
                return;
            }


            serviceClients.PolicyToLearnPostClient.AddUpdatPolicyToLearn(SelectedPolicy.PolicyId);
            //--Policy History Stuff
            serviceClients.PolicyClient.AddUpdatePolicyHistory(SelectedPolicy.PolicyId);
            serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearned(SelectedPolicy.PolicyId);
            //-------------------------------
            SelecetdPolicyLearnedField = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);
            ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

            if ((_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0)
                &&

                  ChangeInModeToRunFollowUp.HasValue

                )
            {
                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId
                    , SelectedPolicy.IsTrackPayment, true, RoleManager.Role, ChangeInModeToRunFollowUp);
                FollowUpcalled = true;
            }
            else
            {
                FollowUpcalled = false;
            }

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true) return;

            SaveIncomingSchedule();

            if (OutGoingField != null && OutGoingField.Count != 0)
            {
                if ((OutGoingField.FirstOrDefault().ScheduleTypeId == 2) && (OutGoingField.ToList().Sum(p => p.FirstYearPercentage) == 100) && (OutGoingField.ToList().Sum(p => p.RenewalPercentage) == 100))
                {
                    //serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField);
                }
                else if (OutGoingField.FirstOrDefault().ScheduleTypeId == 1)
                {
                    //serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(OutGoingField);
                }

            }
            if (_deleteseletedOutGoingField.Count != 0)
            {
                foreach (OutGoingPayment paymen in _deleteseletedOutGoingField)
                {
                    serviceClients.OutGoingPaymentClient.DeleteOutgoingPayment(paymen);

                }
                _deleteseletedOutGoingField.Clear();
            }
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", SelectedPolicy.PolicyId);
            SelectedPolicy = serviceClients.PolicyClient.GetPolicydata(parameters).FirstOrDefault();

            if (ReplaceOccur)
            {
                PolicyList.Remove(SelecetdPolicylstForReplace);
            }
        }

        void SaveIncomingSchedule()
        {
            if (IncPercentOfPremium == true || IncPerHead == true)
            {
                if (SelectedPolicyToolIncommingShedule == null)
                {
                    SelectedPolicyToolIncommingShedule = new PolicyToolIncommingShedule();
                }

            }

            if (SelectedPolicyToolIncommingShedule != null)
            {
                if (SelectedPolicyToolIncommingShedule.FirstYearPercentage != null || SelectedPolicyToolIncommingShedule.RenewalPercentage != null)
                {
                    //if (SelectedPolicyToolIncommingShedule.IncomingScheduleId == Guid.Empty)
                    //{
                    //    SelectedPolicyToolIncommingShedule.IncomingScheduleId = Guid.NewGuid();
                    //}
                    serviceClients.PolicyIncomingScheduleClient.AddUpdatePolicyToolIncommingShedule(SelectedPolicyToolIncommingShedule);
                    if ((SelectedPolicy.IsIncomingBasicSchedule ?? false))
                        if (!FollowUpcalled)
                        {
                            serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.IncomingScheduleChange, null,
                                SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, null);
                            FollowUpcalled = false;
                        }
                }
            }
        }
        private void AddPolicyWithDefaultSetting()
        {
            policyStatus = _PolicyStatus.Active;
            PolicyDetailsData policy = new PolicyDetailsData()
            {
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
                TrackFromDate = serviceClients.LicenseeClient.GetLicenseeByID(SharedVMData.SelectedLicensee.LicenseeId).TrackDateDefault,// DateTime.Now,
                IsTrackIncomingPercentage = false,
                IsTrackMissingMonth = false,
                CreatedOn = DateTime.Today,

                IsIncomingBasicSchedule = true,
                IsOutGoingBasicSchedule = true,
                IsSavedPolicy = false,

            };

            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
            {
                policy.CreatedBy = RoleManager.userCredentialID;
            }
            else
            {
                policy.CreatedBy =
                 serviceClients.PostUtilClient.GetPolicyHouseOwner(SharedVMData.SelectedLicensee.LicenseeId);
            }

            if (PolicyList == null) PolicyList = new ObservableCollection<PolicyDetailsData>();
            PolicyList.Add(policy);
            SelectedPolicy = policy;
            UpdateUIByAddNewPolicy(SelectedPolicy);
            ReplaceRbStatus = true; NewRbStatus = true;
        }

        private void UpdateUIByAddNewPolicy(PolicyDetailsData _selectedPolicy)
        {
            SelectedStatus = MasterPolicyStatus.Where(p => p.StatusId == SelectedPolicy.PolicyStatusId).FirstOrDefault();
            SelectedClient = DisplayedClientsLists.Where(p => p.ClientId == (SelectedPolicy.ClientId ?? Guid.Empty)).FirstOrDefault();
            SelectedClientLrnd = DisplayedClientsLists.Where(p => p.ClientId == (SelectedPolicy.ClientId ?? Guid.Empty)).FirstOrDefault();
            SelectedPaymentMode = MasterPaymentsModeData.Where(p => p.ModeId == SelectedPolicy.PolicyModeId).FirstOrDefault();

            SelectedPayor = PayorsLst.Where(p => p.PayorID == (SelectedPolicy.PayorId ?? Guid.Empty)).FirstOrDefault();// PayorsLst == null ? new Payor() : PayorsLst.FirstOrDefault();
            SelectedCarrier = CarriersLst == null ? new Carrier() : CarriersLst.FirstOrDefault();
            SelectedProduct = ProductsLst == null ? new Coverage() : ProductsLst.FirstOrDefault();
            SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(p => p.PaymentTypeId == SelectedPolicy.IncomingPaymentTypeId).FirstOrDefault();
            SelectedTermReason = PolicyTerminationtReasonLst.Where(p => p.TerminationReasonId == SelectedPolicy.TerminationReasonId).FirstOrDefault();//FirstOrDefault();
            SubmittedThrough = SubmittedThroughLst.Where(p => p.PayorName == SelectedPolicy.SubmittedThrough).FirstOrDefault();

            //------
            if (PolicyIncomingPaymentCommissionDashBoard != null)
                PolicyIncomingPaymentCommissionDashBoard.Clear();
            if (CommissionDashBoardOutGoingPaymentLst != null)
                CommissionDashBoardOutGoingPaymentLst.Clear();
            if (PolicyFollowUpCommissionDashBoardLst != null)
                PolicyFollowUpCommissionDashBoardLst.Clear();
            if (PolicyNote != null)
                PolicyNote.Clear();

            SelecetdPolicyLearnedField = null;

            //--------------
            IncPerHead = false;
            IncAdvance = false;
            IncPercentOfPremium = false;

            SelectedPolicyToolIncommingShedule = null;

            OutPercentOfCommission = true;
            OutPercentOfPremium = false;
            OutAdvance = false;
            OutGoingField = null;


        }

        private void DeletePolicy()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;

            if (SelectedPolicy.IsDeleted == true) return;
            if (serviceClients.PolicyClient.CheckForPolicyPaymentExists(SelectedPolicy.PolicyId))
            {
                MessageBox.Show("Policy can not be deleted",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Do you want to remove Selected Policy",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;
            serviceClients.PolicyClient.DeletePolicy(SelectedPolicy);
            //History---
            serviceClients.PolicyClient.DeletePolicyHistory(SelectedPolicy);

            PolicyList.Remove(SelectedPolicy);
            SelectedPolicy = PolicyList.FirstOrDefault();
        }

        private void CreateClient()
        {
            SharedVMData.UpdateMode = UpdateMode.Add;
            Client _Client = _SelecetdPolicyClientVm.Show();
            if (_Client.ClientId != Guid.Empty)
            {
                DisplayedClientsLists.Insert(DisplayedClientsLists.Count - 1, _Client);
                SelectedDisplayClient = DisplayedClientsLists.Where(p => p.ClientId == _Client.ClientId).FirstOrDefault();
                SelectedClient = DisplayedClientsLists.Where(p => p.ClientId == Guid.Empty).FirstOrDefault();
            }
            SharedVMData.UpdateMode = UpdateMode.None;
        }

        private void EditClientDetail()
        {
            SharedVMData.UpdateMode = UpdateMode.Edit;
            Client _Client = _SelecetdPolicyClientVm.Show();
            SharedVMData.UpdateMode = UpdateMode.None;
        }

        private void DeleteClient()
        {
            if (SelectedDisplayClient.ClientId == Guid.Empty) return;
            if (SelectedDisplayClient == null) return;
            if (serviceClients.ClientClient.CheckClientPolicyIssueExists(SelectedDisplayClient.ClientId))
            {
                MessageBox.Show("Client can not be deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show("Do you want to delete Client " + SelectedClient.Name + " ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            serviceClients.ClientClient.DeleteClients(SelectedDisplayClient);
            DisplayedClientsLists.Remove(SelectedDisplayClient);
            SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
            SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
        }

        #endregion
        //------------------------------------------------------------------------------------------

        #region Licensees

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

        public void OnSelectedLicenseeChanged()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                //GlobalData.PolicyManagerSelectedLicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                DisplayedClientsLists = UpdateClientListLicenseeWise();
                //ClientsLists = UpdateClientListLicenseeWise();
                //LearnedClientsLists = UpdateClientListLicenseeWise();
                PayorsLst = FillPayorLst();
                SubmittedThroughLst = FillSubmitedThrough();
                OutgoingPayeeList = FillOutgoingPayeeUser();
                PrimaryAgents = FillOutgoingPayeeUser();

                SelectedPrimaryAgent = PrimaryAgents.FirstOrDefault();
                SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();

                AddPaeeButton = serviceClients.BillingLineDetailClient.IsAgencyVersionLicense(SharedVMData.SelectedLicensee.LicenseeId);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion
        #region Clients

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

        private ObservableCollection<Client> displayedClientsLists;
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

        private Client selectedClient;
        public Client SelectedClient
        {
            get { return selectedClient == null ? new Client() : selectedClient; }
            set { selectedClient = value; OnPropertyChanged("SelectedClient"); }
        }

        #endregion
        #region Policy

        private bool isacticechecked;
        public bool IsActiceChecked
        {
            get { return isacticechecked; }
            set { isacticechecked = value; OnPropertyChanged("IsActiceChecked"); }
        }


        private PolicyDetailsData selectedPolicy;
        public PolicyDetailsData SelectedPolicy
        {
            get
            {
                return selectedPolicy == null ? new PolicyDetailsData() : selectedPolicy;
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

        private Payor submittedThrough;
        public Payor SubmittedThrough
        {
            get { return submittedThrough == null ? new Payor() : submittedThrough; }
            set { submittedThrough = value; OnPropertyChanged("SubmittedThrough"); }
        }

        private ObservableCollection<Payor> submittedThroughLst;
        public ObservableCollection<Payor> SubmittedThroughLst
        {
            get { return submittedThroughLst; }
            set { submittedThroughLst = value; OnPropertyChanged("SubmittedThroughLst"); }
        }
        #endregion
        #region PolicyStatus
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

        #endregion
        #region MasterData
        /// <summary> 
        /// 
        /// </summary>
        private PolicyStatus selectedStatus;
        public PolicyStatus SelectedStatus
        {
            get { return selectedStatus == null ? new PolicyStatus() : selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); }
        }
        /// <summary>
        /// 
        /// </summary>
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
        private ObservableCollection<Payor> payorslst;
        public ObservableCollection<Payor> PayorsLst
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

        private Payor selectedPayor;
        public Payor SelectedPayor
        {
            get { return selectedPayor == null ? new Payor() : selectedPayor; }
            set
            {
                selectedPayor = value;
                OnPropertyChanged("SelectedPayor");
            }
        }


        private ObservableCollection<Carrier> carriersLst;
        public ObservableCollection<Carrier> CarriersLst
        {
            get { return carriersLst; }
            set { carriersLst = value; OnPropertyChanged("CarriersLst"); }
        }

        private Carrier selectedCarrier;
        public Carrier SelectedCarrier
        {
            get { return selectedCarrier; }
            set
            {
                selectedCarrier = value;
                OnPropertyChanged("SelectedCarrier");
            }
        }


        private ObservableCollection<Coverage> productlst;
        public ObservableCollection<Coverage> ProductsLst
        {
            get { return productlst; }
            set { productlst = value; OnPropertyChanged("ProductsLst"); }
        }
        private Coverage selectedProduct;
        public Coverage SelectedProduct
        {
            get { return selectedProduct; }
            set { selectedProduct = value; OnPropertyChanged("SelectedProduct"); }
        }

        #endregion

        #region methods
        private ObservableCollection<Client> UpdateClientListLicenseeWise()
        {
            Guid? LicenseeId = null;
            if (SharedVMData.SelectedLicensee.LicenseeId == null)
            {
                LicenseeId = Guid.Empty;
            }
            else
            {
                LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
            }
            
            ObservableCollection<Client> clientlst = serviceClients.ClientClient.GetClientList(LicenseeId);

            Client blankClient = new Client();
            blankClient.ClientId = Guid.Empty;
            blankClient.LicenseeId = SelectedPolicy.PolicyLicenseeId;
            clientlst.Add(blankClient);

            return clientlst;
        }

        private ObservableCollection<PolicyDetailsData> FillPolicyGrid()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyLicenseeId", SharedVMData.SelectedLicensee.LicenseeId);
            parameters.Add("PolicyStatusId", (int)policyStatus);
            List<PolicyDetailsData> _policylst = serviceClients.PolicyClient.GetPolicydata(parameters).ToList();
            _policylst = _policylst.Where(p => p.ClientId == SelectedDisplayClient.ClientId).ToList();

            ObservableCollection<PolicyDetailsData> policylst = new ObservableCollection<PolicyDetailsData>(_policylst);
            if (policyStatus == _PolicyStatus.Delete)
            {
                for (int idx = 0; idx < policylst.Count; idx++)
                {
                    policylst[idx].PolicyStatusName = "Deleted";
                }

            }
            return policylst;
        }

        private ObservableCollection<Payor> FillPayorLst()
        {
            ObservableCollection<Payor> payorlst = null;
            PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.All };
            payorlst = new ObservableCollection<Payor>(serviceClients.PayorClient.GetPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo).OrderBy(p => p.PayorName));
            payorlst.Add(

               new Payor()
               {
                   PayorName = "",
                   PayorID = Guid.Empty,
               }
            );
            payorlst.Reverse();
            return payorlst;

        }
        private ObservableCollection<Carrier> FillCarrierLst()
        {
            ObservableCollection<Carrier> carrlst = null;
            carrlst = (SelectedPayor == null || SelectedPayor.PayorID == Guid.Empty) ? null : new ObservableCollection<Carrier>(serviceClients.CarrierClient.GetPayorCarriersOnly(SelectedPayor.PayorID).OrderBy(p => p.CarrierName));

            if (carrlst != null)
            {
                carrlst.Add(

                       new Carrier()
                       {
                           CarrierName = "",
                           CarrierId = Guid.Empty,
                           LicenseeId = SharedVMData.SelectedLicensee.LicenseeId,
                           PayerId = SelectedPayor.PayorID,
                           IsDeleted = false,


                       });
            }
            return carrlst;
        }
        private ObservableCollection<Coverage> FillCoverageLst()
        {
            ObservableCollection<Coverage> coveragelst = null;
            coveragelst = (SelectedPayor == null || SelectedCarrier == null || SelectedCarrier.CarrierId == Guid.Empty || SelectedPayor.PayorID == Guid.Empty) ? null :
               new ObservableCollection<Coverage>(serviceClients.CoverageClient.GetPayorCarrierCoverages(SelectedPayor.PayorID, SelectedCarrier.CarrierId).OrderBy(p => p.Name));
            if (coveragelst != null)
            {
                coveragelst.Add(


                     new Coverage()
                     {
                         Name = "",
                         CoverageID = Guid.Empty,
                     }
                 );
            }
            return coveragelst;
        }
        private ObservableCollection<Payor> FillSubmitedThrough()
        {
            ObservableCollection<Payor> payorlst = new ObservableCollection<Payor>();
            if (SelectedPayor != null)
            {
                if (payorlst.Where(p => p.PayorName != SelectedPayor.PayorName).Count() == 0)
                    payorlst.Add(new Payor() { PayorID = Guid.NewGuid(), PayorName = SelectedPayor.PayorName });
            }
            payorlst.AddRange(serviceClients.PayorClient.GetPayors(SharedVMData.SelectedLicensee.LicenseeId, null).Where(p => p.PayorTypeID == 1));

            if (SelectedPolicy != null && string.IsNullOrEmpty(SelectedPolicy.SubmittedThrough))
            {
                if (payorlst.Where(p => p.PayorName != SelectedPolicy.SubmittedThrough).Count() == 0)
                    payorlst.Add(new Payor() { PayorID = Guid.NewGuid(), PayorName = SelectedPolicy.SubmittedThrough });
            }
            List<string> payorname = (from d in payorlst select d.PayorName).Distinct().ToList();
            payorlst = new ObservableCollection<Payor>();

            foreach (string p in payorname)
            {
                payorlst.Add(new Payor()

                {
                    PayorName = p,
                }
                                );
            }
            return payorlst;
        }

        #endregion
        #endregion                

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
                PolicyToolIncommingShedule _PolicyToolIncommingShedule = serviceClients.PolicyIncomingScheduleClient.GetPolicyToolIncommingSheduleListPolicyWise(SelectedPolicy.PolicyId);
                return _PolicyToolIncommingShedule;
            }
            catch 
            {
                return null;
            }


        }
        #endregion
        
        #region PolicyBasicOutGoing
        private bool ValidateOutGoingSchedule()
        {
            bool flag = false;
            // string errMSG="";
            if (OutGoingField.Count == 0)
                return true;
            int numPrimaryAgent = OutGoingField.Where(p => p.IsPrimaryAgent == true).Count();
            if (numPrimaryAgent == 1)
            {
                flag = true;
            }
            else
            {
                flag = false;
                //  errMSG = "Identify the primary Agent. "; 
                return flag;

            }

            if (OutPercentOfCommission)
            {
                double sumFirstYr = OutGoingField.Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
                double sumRenewalYr = OutGoingField.Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
                if (sumFirstYr == 100)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                    //  errMSG += "First Year shoule be 100. ";
                    return flag;
                }

                if (sumRenewalYr == 100)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                    // errMSG += "Renewal Year shoule be 100. ";
                    return flag;

                }
            }
            else if (OutPercentOfPremium)
            {

            }
            return flag;

        }
        private ICommand _cmdOk;
        public ICommand CmdOk
        {
            get
            {
                if (_cmdOk == null)
                {
                    _cmdOk = new BaseCommand(x => AddPayeeInShedule());

                }
                return _cmdOk;
            }

        }
        private void AddPayeeInShedule()
        {
            ObservableCollection<AddPayee> tempAddPayee = new ObservableCollection<AddPayee>();
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == null || SelectedPolicy.PolicyId == Guid.Empty)
            //    if (SelectedPolicy.ClientId == Guid.Empty)
            {
                System.Windows.Forms.MessageBox.Show("No Policy is Selected", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }
            if (AddPayee.Count == 0) return;

            foreach (AddPayee i in AddPayee.ToList())
            {
                if (!tempAddPayee.Contains(i) && i.IsSelect == true)

                    tempAddPayee.Add(i);
            }
            foreach (AddPayee i in tempAddPayee.ToList())
            {
                ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));

                User _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.UserCredentialID == i.UserCiD).FirstOrDefault();
                if (i.IsSelect != true) continue;

                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                bool IsThrprimaryAgent = (OutGoingField.Where(p => p.IsPrimaryAgent == true).Count() > 0);
                if (tempAddPayee.Count == 1 && !IsThrprimaryAgent) OutgoingRecord.IsPrimaryAgent = i.IsSelect;
                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                OutgoingRecord.FirstYearPercentage = _user.FirstYearDefault;
                OutgoingRecord.RenewalPercentage = _user.RenewalDefault;
                OutgoingRecord.Payor = i.NickName;
                // OutgoingRecord.Payor = i.LastName + " " + i.FirstName;
                //OutgoingRecord.Payor = i.FirstName;
                OutgoingRecord.PayeeUserCredentialId = i.UserCiD;
                OutgoingRecord.PolicyId = SelectedPolicy.PolicyId;
                OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                OutGoingField.Add(OutgoingRecord);

                AddPayee.Remove(i);
                tempAddPayee.Remove(i);

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
            PolicyAddPayeeToolTip = null;
            if (!serviceClients.BillingLineDetailClient.IsAgencyVersionLicense(SharedVMData.SelectedLicensee.LicenseeId))
            {
                PolicyAddPayeeToolTip = "Contact commission department to activate Agency Version.";
            }
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
            {
                PolicyAddPayeeToolTip = "Permission Violation";
                return false;
            }
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
            string selectItem = string.Empty;
            if (!OutGoingField.Contains(SelectedOutGoingField)) return;
            if ((OutGoingField != null && OutGoingField.Count() != 0) && OutGoingField.Count() != 1)
            {
                if (SelectedOutGoingField.IsPrimaryAgent)
                {
                    MessageBoxResult mres1 = MessageBox.Show("Cannot delete a Primary Agent", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            MessageBoxResult mres = MessageBox.Show("Do you want to delete the seleted record", "Confirm", MessageBoxButton.YesNo);
            if (MessageBoxResult.Yes == mres)
            {
                _deleteseletedOutGoingField.Add(SelectedOutGoingField);
                OutGoingField.Remove(SelectedOutGoingField);

            }


        }
        private void AddPayeeInGrid()
        {
            ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));

            AddPayee = new ObservableCollection<AddPayee>();

            List<User> _user = AgentList.Where(p => p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && p.IsHouseAccount == false).ToList<User>();
            if (OutGoingField == null)
                OutGoingField = new ObservableCollection<OutGoingPayment>();
            foreach (User Ap in _user)
            {
                int num = OutGoingField.Where(p => p.PayeeUserCredentialId == Ap.UserCredentialID).Count();
                if (num == 1) continue;
                AddPayee Payee = new AddPayee();
                //SelectedPayee.IsSelect=
                Payee.FirstName = Ap.FirstName;
                Payee.LastName = Ap.LastName;
                Payee.NickName = Ap.NickName;
                Payee.Company = Ap.Company;
                Payee.UserCiD = Ap.UserCredentialID;
                Payee.IsHouse = false;//this is not the House Owner
                AddPayee.Add(Payee);
            }
            SelectedPayee = AddPayee.FirstOrDefault();

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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else if (OutGoingField == null || OutGoingField.Where(p => p.IsPrimaryAgent == true).Count() != 1)
            {
                return false;
            }
            else
                return true;
        }

        private void DoSplitEvently()
        {


            //AddPayeeInShedule();
            if (OutGoingField.Count == 0) return;
            double totalpayee = 0;
            if (OutPercentOfCommission)
            {
                //  CalcOutGoingShedule(CommisionPer, OutGoingField.Count());
                CalcOutGoingShedule(OutGoingField.Count());

            }
            if (OutPercentOfPremium)
            {
                CalcOutGoingSheduleOnPremium(OutGoingField.Count());

            }
            totalpayee = OutGoingField.Count();
            //CalcOutGoingShedule(CommisionPer, OutGoingField.Count());
            //ObservableCollection<OutGoingPayment> TempOutGoingField = new ObservableCollection<OutGoingPayment>();
            //TempOutGoingField = OutGoingField;
            //  OutGoingField = new ObservableCollection<OutGoingPayment>();

            //foreach (OutGoingPayment i in TempOutGoingField)
            //foreach (OutGoingPayment i in OutGoingField)
            //{
            //    SelectedOutGoingField = new OutGoingPayment();

            //    SelectedOutGoingField.IsPrimaryAgent = i.IsPrimaryAgent;
            //    SelectedOutGoingField.FirstYearPercentage = FirstYear;
            //    SelectedOutGoingField.RenewalPercentage = Renewal;
            //    SelectedOutGoingField.Payor = i.Payor;
            //    SelectedOutGoingField.OutgoingScheduleId = i.OutgoingScheduleId;
            //    SelectedOutGoingField.PayeeUserCredentialId = i.PayeeUserCredentialId;
            //    SelectedOutGoingField.PolicyId = i.PolicyId;

            //    OutGoingField.Add(SelectedOutGoingField);
            //}

            //FirstYear = Math.Round(FirstYear ?? 0, 2);
            //Renewal = Math.Round(Renewal ?? 0, 2);
            FirstYear = Math.Truncate(FirstYear.Value * 100) / 100;
            Renewal = Math.Truncate(Renewal.Value * 100) / 100;

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


            PrimaryAgentFirstYrAmount = Firstyrtotalsum - (FirstYear * totalpayee);
            PrimaryAgentRenewYrAmount = secondyrtotalsum - (Renewal * totalpayee);
            OutGoingField.Where(p => p.IsPrimaryAgent).FirstOrDefault().FirstYearPercentage += PrimaryAgentFirstYrAmount;
            OutGoingField.Where(p => p.IsPrimaryAgent).FirstOrDefault().RenewalPercentage += PrimaryAgentRenewYrAmount;

            //for (int idx = 0; idx < OutGoingField.Count; idx++)
            //{
            //    OutGoingField[idx].FirstYearPercentage = FirstYear;
            //    OutGoingField[idx].RenewalPercentage = Renewal;
            //}

        }
        // public void CalcOutGoingShedule(double? totPercent, int totalPayee)
        public void CalcOutGoingShedule(int totalPayee)
        {
            if (totalPayee == 0) return;

            // if (OutPercentOfCommission) FirstYear = (OutGoingField.Sum(p => p.FirstYearPercentage) / totalPayee);// (AddPayee.Where(x => x.IsSelect == true).Count()));
            if (OutPercentOfCommission) FirstYear = ((double)100.0 / totalPayee);// (AddPayee.Where(x => x.IsSelect == true).Count()));

            //if (OutPercentOfPremium) FirstYear = (OutGoingField.Sum(p => p.FirstYearPercentage) / totalPayee);//(AddPayee.Where(x => x.IsSelect == true).Count()));
            // if (OutPercentOfCommission) Renewal = (OutGoingField.Sum(p => p.RenewalPercentage) / totalPayee);//(AddPayee.Where(x => x.IsSelect == true).Count()));
            if (OutPercentOfCommission) Renewal = ((double)100.0 / totalPayee);//(AddPayee.Where(x => x.IsSelect == true).Count()));

            //  if (OutPercentOfPremium) Renewal = (OutGoingField.Sum(p => p.RenewalPercentage) / totalPayee); //(AddPayee.Where(x => x.IsSelect == true).Count()));
        }
        public void CalcOutGoingSheduleOnPremium(int totalPayee)
        {
            //double? totalPremium = 0;
            // if (SelectedPolicyToolIncommingShedule.FirstYearPercentage == null || SelectedPolicyToolIncommingShedule.RenewalPercentage == null) return;
            //if (SelectedPolicyToolIncommingShedule.FirstYearPercentage != null)
            //    totalPremium = ((SelectedPolicyToolIncommingShedule.FirstYearPercentage * SelectedPolicy.SplitPercentage) / 100);

            //CalcOutGoingShedule(totalPremium, totalPayee);
            //double? Firstyrtotalsum = OutGoingField.Sum(p => p.FirstYearPercentage);
            //double? secondyrtotalsum = OutGoingField.Sum(p => p.RenewalPercentage); 
            double? Firstyrtotalsum = SelectedPolicyToolIncommingShedule.FirstYearPercentage;
            double? secondyrtotalsum = SelectedPolicyToolIncommingShedule.RenewalPercentage;

            // FirstYear = Convert.ToDouble(Math.Round((Convert.ToDecimal(Firstyrtotalsum) / totalPayee), 4));
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
        private void AddHouseInShedule()
        {

            ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent));

            User _user = AgentList.Where(p => p.IsHouseAccount == true && p.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).FirstOrDefault();
            if (_user == null) return;
            ///
            if (OutGoingField == null)
            {
                OutGoingField = new ObservableCollection<OutGoingPayment>();
            }
            if (OutGoingField.Where(p => p.PayeeUserCredentialId == _user.UserCredentialID).Count() >= 1)
            {
                return;
            }

            double firstyear = 0.0;
            double renewal = 0.0;
            firstyear = _user.FirstYearDefault;
            renewal = _user.RenewalDefault;
            //if (OutGoingField.Count != 0)
            //{
            //    double fyr = OutGoingField.Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
            //    double ryn = OutGoingField.Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
            //    fyr = 100 - fyr;
            //    ryn = 100 - ryn;
            //    if (fyr < firstyear)
            //    {
            //        firstyear = fyr;
            //    }
            //    if (ryn < renewal)
            //    {
            //        renewal = ryn;
            //    }
            //}
            if (OutPercentOfCommission)
            {
                if (OutGoingField.Count == 0)
                {
                    SelectedOutGoingField = new OutGoingPayment();
                    SelectedOutGoingField.IsPrimaryAgent = false;
                    SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                    //SelectedOutGoingField.FirstYearPercentage = firstyear;
                    //SelectedOutGoingField.RenewalPercentage = renewal;
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
                    double fyr = OutGoingField.Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
                    double ryn = OutGoingField.Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
                    fyr = 100 - fyr;
                    ryn = 100 - ryn;
                    SelectedOutGoingField = new OutGoingPayment();
                    SelectedOutGoingField.IsPrimaryAgent = false;
                    SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                    SelectedOutGoingField.FirstYearPercentage = fyr;
                    SelectedOutGoingField.RenewalPercentage = ryn;
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
                    SelectedOutGoingField.IsPrimaryAgent = false;
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
                    double fyr = OutGoingField.Sum<OutGoingPayment>(p => p.FirstYearPercentage.Value);
                    double ryn = OutGoingField.Sum<OutGoingPayment>(p => p.RenewalPercentage.Value);
                    fyr = (SelectedPolicyToolIncommingShedule.FirstYearPercentage ?? 0) - fyr;
                    ryn = (SelectedPolicyToolIncommingShedule.RenewalPercentage ?? 0) - ryn;
                    SelectedOutGoingField = new OutGoingPayment();
                    SelectedOutGoingField.IsPrimaryAgent = false;
                    SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
                    SelectedOutGoingField.FirstYearPercentage = fyr;
                    SelectedOutGoingField.RenewalPercentage = ryn;
                    SelectedOutGoingField.Payor = _user.NickName;
                    SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
                    SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
                    SelectedOutGoingField.ScheduleTypeId = 1;
                    OutGoingField.Add(SelectedOutGoingField);
                }
            }
            ///


            //if (OutGoingField.Where(p => p.PayeeUserCredentialId == _user.UserCredentialID).Count() == 0)
            //{
            //    SelectedOutGoingField = new OutGoingPayment();
            //    SelectedOutGoingField.IsPrimaryAgent = false;
            //    SelectedOutGoingField.OutgoingScheduleId = Guid.NewGuid();
            //    SelectedOutGoingField.FirstYearPercentage = firstyear;
            //    SelectedOutGoingField.RenewalPercentage = renewal;
            //    SelectedOutGoingField.Payor = _user.FirstName;
            //    SelectedOutGoingField.PayeeUserCredentialId = _user.UserCredentialID;
            //    SelectedOutGoingField.PolicyId = SelectedPolicy.PolicyId;
            //    SelectedOutGoingField.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
            //    OutGoingField.Add(SelectedOutGoingField);
            //}

            if (OutGoingField.Count == 1)
            {
                OutGoingField.FirstOrDefault().IsPrimaryAgent = true;
            }

            //foreach
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
        private ObservableCollection<OutGoingPayment> FillBasicOutGoingSchedule()
        {
            ObservableCollection<OutGoingPayment> OutGoingPaymentLst = new ObservableCollection<OutGoingPayment>(serviceClients.OutGoingPaymentClient.GetOutgoingPayments().Where(u => u.PolicyId == SelectedPolicy.PolicyId).ToList());
            return OutGoingPaymentLst;

        }

        #endregion
        
        #region PolicyManagerLerned

        private Client selectedclientlrnd;
        public Client SelectedClientLrnd
        {
            get { return selectedclientlrnd == null ? new Client() : selectedclientlrnd; }
            set { selectedclientlrnd = value; OnPropertyChanged("SelectedClientLrnd"); }
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

        private PolicyLearnedFieldData FillLrndField()
        {
            try
            {
                PolicyLearnedFieldData _PolicyLearnedField = null;
                _PolicyLearnedField = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);
                return _PolicyLearnedField;
            }
            catch 
            {
                return new PolicyLearnedFieldData();
            }
        }
        //private void SaveLearndField()
        //{

        //    if (SelecetdPolicyLearnedField == null)
        //        return;


        //    serviceClients.LearnedToPolicyPostClient.AddLearnedToPolicy(SelecetdPolicyLearnedField.PolicyId);
        //    serviceClients.PolicyLearnedFieldClient.AddUpdatePolicyLearnedField(SelecetdPolicyLearnedField,SelecetdPolicyLearnedField.ProductType);
        //    //--Policy History
        //    serviceClients.PolicyClient.AddUpdatePolicyHistory(SelectedPolicy.PolicyId);
        //    serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearned(SelectedPolicy.PolicyId);

        //    ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost =  serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);
        //    if ( //(_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0)   //&&
        //     (
        //     SelecetdPolicyLearnedField.PreviousEffectiveDate != SelecetdPolicyLearnedField.Effective
        //     ||
        //     SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId
        //     ||
        //     SelecetdPolicyLearnedField.PrevoiusTrackFromDate != SelecetdPolicyLearnedField.TrackFrom
        //     )
        //     )
        //    {
        //        serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange,
        //            null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role,
        //            SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId);
        //    }

        //    //-----------
        //    SelecetdPolicyLearnedField.PreviousEffectiveDate = SelecetdPolicyLearnedField.Effective;
        //    SelecetdPolicyLearnedField.PreviousPolicyModeid = SelecetdPolicyLearnedField.PolicyModeId;
        //    SelecetdPolicyLearnedField.PrevoiusTrackFromDate = SelecetdPolicyLearnedField.TrackFrom;
        //    //--------------

        //}

        private void SaveLearndField()
        {
            if (SelecetdPolicyLearnedField == null)
                return;
            
            serviceClients.LearnedToPolicyPostClient.AddLearnedToPolicy(SelecetdPolicyLearnedField.PolicyId);
            serviceClients.PolicyLearnedFieldClient.AddUpdatePolicyLearnedField(SelecetdPolicyLearnedField, SelecetdPolicyLearnedField.ProductType);
            //--Policy History
            serviceClients.PolicyClient.AddUpdatePolicyHistoryAsync(SelectedPolicy.PolicyId);
            serviceClients.PolicyLearnedFieldClient.AddUpdateHistoryLearnedAsync(SelectedPolicy.PolicyId);

            ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryPolicyIDWise(SelectedPolicy.PolicyId);

            if (( SelecetdPolicyLearnedField.PreviousEffectiveDate != SelecetdPolicyLearnedField.Effective  || SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId || SelecetdPolicyLearnedField.PrevoiusTrackFromDate != SelecetdPolicyLearnedField.TrackFrom ))
            {
                serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.PolicyDetailChange, null, SelectedPolicy.PolicyId, SelectedPolicy.IsTrackPayment, true, RoleManager.Role, SelecetdPolicyLearnedField.PreviousPolicyModeid != SelecetdPolicyLearnedField.PolicyModeId);
            }
           
            SelecetdPolicyLearnedField.PreviousEffectiveDate = SelecetdPolicyLearnedField.Effective;
            SelecetdPolicyLearnedField.PreviousPolicyModeid = SelecetdPolicyLearnedField.PolicyModeId;
            SelecetdPolicyLearnedField.PrevoiusTrackFromDate = SelecetdPolicyLearnedField.TrackFrom;
        }

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

            if (RoleManager.Role != UserRole.SuperAdmin)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void OnAddIncomingAdvanceSchedule()
        {
            if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount != true) return;

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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void OnRemoveIncomingAdvanceSchedule()
        {
            if (SelectedIncomingAdvanceSchedule == null || SelectedIncomingAdvanceSchedule.CoveragesScheduleId == Guid.Empty)
            {

                MessageBox.Show("Invalid Operation-No Record Selected", "Warning", MessageBoxButton.OK);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Do you want to remove Incoimg Schedule Record",
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void OnSaveIncomingAdvanceSchedule()
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

            if (SelectedCarrier == null || SelectedProduct == null || SelectedCarrier.CarrierId == Guid.Empty || SelectedProduct.CoverageID == Guid.Empty)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;


        }

        private void OnDefaultIncomingAdvanceSchedule()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty) return;
            if (SelectedCarrier == null || SelectedProduct == null || SelectedCarrier.CarrierId == Guid.Empty || SelectedProduct.CoverageID == Guid.Empty) return;
            GlobalIncomingSchedule GlobalIncomingScheduleRecord = serviceClients.GlobalIncomingScheduleClient.GetGlobalIncomingSchedule(SelectedCarrier.CarrierId, SelectedProduct.CoverageID);
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


        private PolicyIncomingSchedule FillIncomingAdvanceSchedule()
        {
            if (SelectedPolicy.PolicyId == null) return null;
            IncomingAdvanceScheduleLst = serviceClients.IncomingScheduleClient.GetPolicyIncomingSchedule(SelectedPolicy.PolicyId);
            if (IncomingAdvanceScheduleLst.IncomingScheduleList == null)
                IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();

            IncomingAdvanceScheduleLst.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(IncomingAdvanceScheduleLst.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());

            if (IncomingAdvanceScheduleLst != null && IncomingAdvanceScheduleLst.ScheduleTypeId != 0)
                SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault(s => s.ScheduleTypeId == IncomingAdvanceScheduleLst.ScheduleTypeId);
            else
                SelecetdIncomingAdvanceScheduleTypes = IncomingAdvanceScheduleTypes.FirstOrDefault();

            return IncomingAdvanceScheduleLst;
        }
        #endregion
        
        #region OutgoingAdvanceSchedlue


        private ICommand _NewOutgoingAdvanceSchedule;
        public ICommand NewOutgoingAdvanceSchedule
        {
            get
            {
                if (_NewOutgoingAdvanceSchedule == null)
                    _NewOutgoingAdvanceSchedule = new BaseCommand(param => OnNewOutgoingAdvanceSchedule());
                return _NewOutgoingAdvanceSchedule;
            }
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
                    addOutgoingAdvanceSchedule = new BaseCommand(param => OnAddOutgoingAdvanceSchedule());
                return addOutgoingAdvanceSchedule;
            }
        }

        private void OnAddOutgoingAdvanceSchedule()
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
                    removeOutgoingAdvanceSchedule = new BaseCommand(param => OnRemoveOutgoingAdvanceSchedule());
                return removeOutgoingAdvanceSchedule;

            }
        }

        private void OnRemoveOutgoingAdvanceSchedule()
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

        private ICommand saveOutgoingAdvanceSchedule;
        public ICommand SaveOutgoingAdvanceSchedule
        {
            get
            {
                if (saveOutgoingAdvanceSchedule == null)
                    saveOutgoingAdvanceSchedule = new BaseCommand(param => OnSaveOutgoingAdvanceSchedule());
                return saveOutgoingAdvanceSchedule;

            }
        }

        private void OnSaveOutgoingAdvanceSchedule()
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

            //if (OutgoingAdvanceScheduleLst.OutgoingScheduleList.Where(p => p.IsPrimaryAgent == true).Count() > 1)
            //{
            //    MessageBox.Show("There should be only one Primary Agent", "Warning", MessageBoxButton.OK);
            //    return;
            //}

            //if (OutgoingAdvanceScheduleLst.OutgoingScheduleList.Count > 1)
            //{
            //    if (OutgoingAdvanceScheduleLst.OutgoingScheduleList.Where(p => p.IsPrimaryAgent == true).Count() == 0)
            //    {
            //        MessageBox.Show("No Primary Agent is There", "Warning", MessageBoxButton.OK);
            //        return;
            //    }
            //}

            OutgoingAdvanceScheduleLst.OutgoingScheduleList.ToList().ForEach(s => s.IsPrimaryAgent = false);
            OutgoingAdvanceScheduleLst.OutgoingScheduleList.Where(s => s.PayeeUserCredentialId == SelectedPrimaryAgent.UserCredentialID).ToList().ForEach(s => s.IsPrimaryAgent = true);
            OutgoingAdvanceScheduleLst.ScheduleTypeId = SelecetdOutgoingAdvanceScheduleTypes.ScheduleTypeId;
            serviceClients.OutgoingScheduleClient.AddUpdateOutgoingShedule(OutgoingAdvanceScheduleLst);
            _SavedOutShedule = null;
        }


        //private ICommand defaultOutgoingAdvanceShedule;
        //public ICommand DefaultOutgoingAdvanceShedule
        //{
        //    get
        //    {
        //        if (defaultOutgoingAdvanceShedule == null)
        //            defaultOutgoingAdvanceShedule = new BaseCommand(param => OnDefaultOutgoingAdvanceSchedule());
        //        return defaultOutgoingAdvanceShedule;

        //    }
        //}

        //private void OnDefaultOutgoingAdvanceSchedule()
        //{
        //    if (SelectedCarrier.CarrierId == Guid.Empty || SelectedProduct.CoverageID == Guid.Empty) return;
        //    GlobalIncomingSchedule GlobalIncomingScheduleRecord = serviceClients.GlobalIncomingScheduleClient.GetGlobalIncomingSchedule(SelectedCarrier.CarrierId, SelectedProduct.CoverageID);
        //    IncomingAdvanceScheduleLst = new ObservableCollection<IncomingSchedule>();
        //    for (int idx = 0; idx < GlobalIncomingScheduleRecord.IncomingScheduleList.Count(); idx++)
        //    {
        //        IncomingSchedule _IncomingSchedule = new IncomingSchedule();
        //        _IncomingSchedule.IncomingAdvancedScheduleId = Guid.NewGuid();
        //        _IncomingSchedule.PolicyId = SelectedPolicy.PolicyId;
        //        _IncomingSchedule.ScheduleTypeId = GlobalIncomingScheduleRecord.ScheduleTypeId;
        //        _IncomingSchedule.FromRange = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].FromRange;
        //        _IncomingSchedule.ToRange = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].ToRange;
        //        _IncomingSchedule.EffectiveFromDate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].EffectiveFromDate;
        //        _IncomingSchedule.EffectiveToDate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].EffectiveToDate;
        //        _IncomingSchedule.Rate = GlobalIncomingScheduleRecord.IncomingScheduleList[idx].Rate;
        //        IncomingAdvanceScheduleLst.Add(_IncomingSchedule);
        //    }
        //    SelectedIncomingAdvanceSchedule = IncomingAdvanceScheduleLst.FirstOrDefault();
        //}

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


        private PolicyOutgoingSchedule FillOutgoingAdvanceSchedule()
        {
            if (SelectedPolicy.PolicyId == null) return null;
            PolicyOutgoingSchedule _OutgoingAdvanceSchedulelst = serviceClients.OutgoingScheduleClient.GetOutgoingSheduleBy(SelectedPolicy.PolicyId);

            if (_OutgoingAdvanceSchedulelst.OutgoingScheduleList == null)
                _OutgoingAdvanceSchedulelst.OutgoingScheduleList = new ObservableCollection<OutgoingScheduleEntry>();

            return _OutgoingAdvanceSchedulelst;
        }

        private ObservableCollection<User> FillOutgoingPayeeUser()
        {
            ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent).OrderBy(p => p.NickName));
            //foreach (User user in AgentList)
            //{
            //    user.FullName = user.FirstName + " " + user.LastName;
            //}
            return AgentList;
        }
        #endregion
       
        #region CommissionDashBoard

        #region CommissionDashBoardVM
        public CommissionDashBoardVM _CommissionDashBoardVM;
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
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
            return true;
        }

        private void CommissionDashOKAction()
        {
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty) return;
            if (Convert.ToDouble(PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment) != CommissionDashBoardOutGoingPaymentLst.Sum(p => p.PaidAmount ?? 0))
            {
                MessageBox.Show("Payment Mismatch", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutGoingPaymentEntries(CommissionDashBoardOutGoingPaymentLst);

            if (CloseEditOutGoingCommissionDashBoardEvent != null)
                CloseEditOutGoingCommissionDashBoardEvent();
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
            Guid OutgoingPaymentId = (Guid)x;
            MessageBoxResult _MessageBoxResult = MessageBox.Show("Do you want to delete record", "Information", MessageBoxButton.OKCancel);
            if (_MessageBoxResult == MessageBoxResult.OK)
            {
                serviceClients.PolicyOutgoingDistributionClient.DeleteOutGoingPaymentViaOutgoingPaymentId(OutgoingPaymentId);
                CommissionDashBoardOutGoingPaymentLst.Remove(CommissionDashBoardOutGoingPaymentLst.Where(p => p.OutgoingPaymentId == OutgoingPaymentId).FirstOrDefault());
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
            if (OpenEditOutGoingPaymentCommissionDashBoardEvent != null)
            {
                CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                OpenEditOutGoingPaymentCommissionDashBoardEvent();
                CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
            }
        }

        private ObservableCollection<PolicyOutgoingDistribution> FillCommissionDashBoardOutGoingPaymentLst()
        {
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null) return new ObservableCollection<PolicyOutgoingDistribution>();
            ObservableCollection<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = new ObservableCollection<PolicyOutgoingDistribution>(serviceClients.PolicyOutgoingDistributionClient.GetOutgoingPaymentByPoicyPaymentEntryId(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID));
            return _PolicyOutgoingDistribution;
        }
        #endregion
        /// <summary>
        /// 
        /// //////////////////////////////////////////////////
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
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
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty || SelectedPolicy.IsDeleted == true) return;
            if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty
                //|| SelectedPolicy.CarrierID == null || SelectedPolicy.CarrierID == Guid.Empty ||
                //SelectedProduct.CoverageID == null || SelectedProduct.CoverageID == Guid.Empty
                )
            {
                MessageBox.Show("Payor is not assign to Policy", "Information", MessageBoxButton.OK);
                return;

            }

            SharedVMData.UpdateMode = UpdateMode.Add;
            PolicyPaymentEntriesPost _tPolicyPaymentEntriesPost = _CommissionDashBoardVM.Show();

            SharedVMData.UpdateMode = UpdateMode.None;
            if (_tPolicyPaymentEntriesPost.PaymentEntryID == Guid.Empty) return;
            if (PolicyIncomingPaymentCommissionDashBoard == null)
            {
                PolicyIncomingPaymentCommissionDashBoard = new ObservableCollection<PolicyPaymentEntriesPost>();
            }
            //if (!PolicyIncomingPaymentCommissionDashBoard.Contains(_tPolicyPaymentEntriesPost))
            //{
            //    PolicyIncomingPaymentCommissionDashBoard.Add(_tPolicyPaymentEntriesPost);
            //    PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
            //}
            PolicyIncomingPaymentCommissionDashBoard = FillIncomingPaymentCommissionDashBoard();
            PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void EditCommissionDashBoardIncomingpaymentOpen()
        {
            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty || SelectedPolicy.IsDeleted == true) return;
            if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty || SelectedPolicy.CarrierID == null || SelectedPolicy.CarrierID == Guid.Empty ||
                SelectedProduct.CoverageID == null || SelectedProduct.CoverageID == Guid.Empty)
            {
                MessageBox.Show("Payor/Carrier/Coverage is not assign to Policy", "Information", MessageBoxButton.OK);
                return;

            }
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null)
            {
                MessageBox.Show("No Payment Selected", "Information", MessageBoxButton.OK);
                return;
            }
            SharedVMData.UpdateMode = UpdateMode.Edit;
            PolicyPaymentEntriesPost _tPolicyPaymentEntriesPost = _CommissionDashBoardVM.Show(PolicySelectedIncomingPaymentCommissionDashBoard);
            if (_tPolicyPaymentEntriesPost == null || _tPolicyPaymentEntriesPost.PaymentEntryID == Guid.Empty) return;
            PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
            PolicyIncomingPaymentCommissionDashBoard.Add(_tPolicyPaymentEntriesPost);
            PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();

            //CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
            //CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst == null ? null : CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
            PolicyIncomingPaymentCommissionDashBoard = FillIncomingPaymentCommissionDashBoard();
            PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
            SharedVMData.UpdateMode = UpdateMode.None;
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
            if (PolicyIncomingPaymentCommissionDashBoard == null || PolicyIncomingPaymentCommissionDashBoard.Count == 0)
            {
                return false;
            }
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void RemoveCommIncomingPayment()
        {

            bool flag = false;
            MessageBoxResult _result = MessageBox.Show("Do You Want to Delete", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            if (_result == MessageBoxResult.Cancel)
            {
                return;
            }

            if (objLog == null) objLog = new MastersClient();
            MyAgencyVault.VM.MyAgencyVaultSvc.PostProcessReturnStatus _PostProcessReturnStatus = serviceClients.PostUtilClient.
                RemoveCommissiondashBoardIncomingPayment(PolicySelectedIncomingPaymentCommissionDashBoard, RoleManager.Role);
            flag = _PostProcessReturnStatus.IsComplete;
            if (flag)
            {
                string logMsg = "Manual deletion of incoming payment from comm dashboard, PaymentEntryID : " + PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID + ",  loggedInUser: " + RoleManager.LoggedInUser;
                objLog.AddLog(logMsg);

                PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
                PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
                if (_PostProcessReturnStatus.IsClientDeleted)
                {
                    DisplayedClientsLists.Remove(SelectedDisplayClient);
                    SelectedDisplayClient = DisplayedClientsLists.FirstOrDefault();
                }
                objLog.AddLog("Payment entry deleted successfully");
                MessageBox.Show("Remove Successful", "Infomation", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Payment Cannot Remove", "Warning", MessageBoxButton.OK);

            }
            //bool flag=  serviceClients.PolicyOutgoingDistributionClient.IsEntryMarkPaid(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID);
            //if (flag == true) return;
            //serviceClients.PolicyOutgoingDistributionClient.DeleteByPolicyIncomingPaymentId(PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID);
            //ServiceClients
            //  //RemovePolicyInComingGoingPayment          
            //  //RemoveDEUEntry
            //          ///If no DEUEntryForThat ID GetSomeWhereElse
            //  //GetLAtesetDEUEntryForPoicyID
            //  //MakeDEURecord From it
            //  //RunDEUTOLeare
            //  //RunLeartopolicy
            //  //run FollowUp
            //  serviceClients.DeuClient.

            //  //throw new NotImplementedException();
            //SelectedPolicy = serviceClients.PolicyClient.GetPolicyIdWise(SelectedPolicy.PolicyId);
            Guid SelectedPolicyId = SelectedPolicy.PolicyId;
            PolicyList = FillPolicyGrid();
            SelectedPolicy = PolicyList.Where(p => p.PolicyId == SelectedPolicyId).FirstOrDefault();
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
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId == Guid.Empty)
                return false;
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void UnlinkCommIncomingPayment()
        {
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId == Guid.Empty) return;
            if (serviceClients.DeuClient.GetDeuEntryidWise(PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId.Value).IsEntrybyCommissiondashBoard == true) return;
            if (PolicySelectedIncomingPaymentCommissionDashBoard == null || PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty) return;
            bool flag = serviceClients.PostUtilClient.UnlinkCommissiondashBoardIncomingPayment(PolicySelectedIncomingPaymentCommissionDashBoard, RoleManager.Role).IsComplete;
            if (flag)
            {
                PolicyIncomingPaymentCommissionDashBoard.Remove(PolicySelectedIncomingPaymentCommissionDashBoard);
                PolicySelectedIncomingPaymentCommissionDashBoard = PolicyIncomingPaymentCommissionDashBoard.FirstOrDefault();
                MessageBox.Show("Unlink Successful", "Infomation", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Payment Cannot be Unlink", "Warning", MessageBoxButton.OK);

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


        private ObservableCollection<PolicyPaymentEntriesPost> FillIncomingPaymentCommissionDashBoard()
        {

            ObservableCollection<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = serviceClients.PostUtilClient.GetPolicyPaymentEntryForCommissionDashboard(SelectedPolicy.PolicyId);
            return _PolicyPaymentEntriesPost;

        }
        private ObservableCollection<PolicyOutgoingDistribution> FillOutgoingPaymentCommissionDashBoard()
        {
            if (PolicyIncomingPaymentCommissionDashBoard.Count == 0) return null;
            try
            {
                ObservableCollection<PolicyOutgoingDistribution> _PolicyOutgoingDistribution =
                    serviceClients.PostUtilClient.
                    GetPolicyOutgoingPaymentForCommissionDashboard(PolicyIncomingPaymentCommissionDashBoard.First().PaymentEntryID);
                return _PolicyOutgoingDistribution;
            }
            catch
            {
                return null;
            }
        }
        private ObservableCollection<DisplayFollowupIssue> FillFollowUpIssueCommissiondashBoard()
        {
            ObservableCollection<DisplayFollowupIssue> _FollowupIssue =
                serviceClients.PostUtilClient.
                GetPolicyCommissionIssuesForCommissionDashboard(SelectedPolicy.PolicyId);
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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
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
            try
            {
                if (AmountToReverse == 0) return;
                if (SelectedCommDashPayee == null || SelectedCommDashPayee.UserCredentialID == Guid.Empty)
                {
                    return;
                }
                // double remain = CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount ?? 0 - (CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount ?? 0 * AmountToReverse / 100);
                double remain = (CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount * AmountToReverse / 100) ?? 0;
                PolicyOutgoingDistribution policyoutgoingdis = new PolicyOutgoingDistribution();
                policyoutgoingdis.CreatedOn = DateTime.Today;

                policyoutgoingdis.IsPaid = false;
                policyoutgoingdis.OutgoingPaymentId = Guid.NewGuid();
                policyoutgoingdis.PaidAmount = remain;
                policyoutgoingdis.PaymentEntryId = CommissionDashBoardSelecetdOutGoingPaymentLst.PaymentEntryId;
                policyoutgoingdis.RecipientUserCredentialId = SelectedCommDashPayee.UserCredentialID;
                //Acme 
              //  policyoutgoingdis.ReverseOutgoingPaymentId = CommissionDashBoardSelecetdOutGoingPaymentLst.OutgoingPaymentId;
                //policyoutgoingdis.ReferencedOutgoingAdvancedScheduleId = CommissionDashBoardSelecetdOutGoingPaymentLst.ReferencedOutgoingAdvancedScheduleId;
                // policyoutgoingdis.ReferencedOutgoingScheduleId = CommissionDashBoardSelecetdOutGoingPaymentLst.ReferencedOutgoingScheduleId;
                serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntry(policyoutgoingdis);
                ///////////////
                //CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount = CommissionDashBoardSelecetdOutGoingPaymentLst.PaidAmount - remain;
                policyoutgoingdis = new PolicyOutgoingDistribution();
                policyoutgoingdis.CreatedOn = DateTime.Today;
                policyoutgoingdis.IsPaid = false;
                policyoutgoingdis.OutgoingPaymentId = Guid.NewGuid();
                policyoutgoingdis.PaidAmount = (-1) * remain;
                policyoutgoingdis.PaymentEntryId = CommissionDashBoardSelecetdOutGoingPaymentLst.PaymentEntryId;
                policyoutgoingdis.RecipientUserCredentialId = CommissionDashBoardSelecetdOutGoingPaymentLst.RecipientUserCredentialId;

                //Acme 
               // policyoutgoingdis.ReverseOutgoingPaymentId = CommissionDashBoardSelecetdOutGoingPaymentLst.OutgoingPaymentId;
                //policyoutgoingdis.ReferencedOutgoingAdvancedScheduleId = CommissionDashBoardSelecetdOutGoingPaymentLst.ReferencedOutgoingAdvancedScheduleId;
                // policyoutgoingdis.ReferencedOutgoingScheduleId = CommissionDashBoardSelecetdOutGoingPaymentLst.ReferencedOutgoingScheduleId;
                serviceClients.PolicyOutgoingDistributionClient.AddUpdateOutgoingPaymentEntry(policyoutgoingdis);                
                if (CloseReverseCommissionDashBoardEvent != null)
                {
                    CloseReverseCommissionDashBoardEvent();
                }
                CommissionDashBoardOutGoingPaymentLst = FillCommissionDashBoardOutGoingPaymentLst();
                CommissionDashBoardSelecetdOutGoingPaymentLst = CommissionDashBoardOutGoingPaymentLst.FirstOrDefault();
                
            }
            catch
            {
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
        //private PolicyOutgoingDistribution _commissionDashBoardSelectedOutgoingPayment;
        //public PolicyOutgoingDistribution CommissionDashBoardSelectedOutgoingPayment
        //{
        //    get
        //    {
        //        return _commissionDashBoardSelectedOutgoingPayment == null ? new PolicyOutgoingDistribution() : _commissionDashBoardSelectedOutgoingPayment;
        //    }
        //    set
        //    {
        //        _commissionDashBoardSelectedOutgoingPayment = value;
        //        OnPropertyChanged("CommissionDashBoardSelectedOutgoingPayment");
        //    }
        //}

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
            ObservableCollection<User> AgentList = new ObservableCollection<User>(serviceClients.UserClient.UsersWithLicenseeId(SharedVMData.SelectedLicensee.LicenseeId, UserRole.Agent).OrderBy(p=>p.NickName));

            //foreach (User user in AgentList)
            //{
            //    user.FullName = user.FirstName + " " + user.LastName;
            //}
            return AgentList;
        }


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
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void CommiossionIssueResolvedBtnAction()
        {
            if (PolicySelectedFollowUpCommissionDashBoard == null || PolicySelectedFollowUpCommissionDashBoard.IssueId == Guid.Empty) return;
            ShowCommDashFollowUpWindow();
            //  throw new NotImplementedException();
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
        private ObservableCollection<DisplayFollowupIssue> FillFollowUpIssue()
        {
            ObservableCollection<DisplayFollowupIssue> _FollowupIssue = new ObservableCollection<DisplayFollowupIssue>();
            _FollowupIssue = serviceClients.FollowupIssueClient.GetIssues(SelectedPolicy.PolicyId);
            foreach (DisplayFollowupIssue fip in _FollowupIssue)
            {
                if (fip.IssueCategoryID.HasValue)
                    fip.Category = serviceClients.IssueCategoryClient.GetCategory(fip.IssueCategoryID.Value);
                if (fip.IssueReasonId.HasValue)
                    fip.Reason = serviceClients.IssueReasonClient.GetReasons(fip.IssueReasonId.Value);
                if (fip.IssueResultId.HasValue)
                    fip.Results = serviceClients.IssueResultClient.GetResults(fip.IssueResultId.Value);
                if (fip.IssueStatusId.HasValue)
                    fip.Status = serviceClients.IssueStatusClient.GetStatus(fip.IssueStatusId.Value);

            }
            return _FollowupIssue;
        }
        private void CommiossionIssuePaymentReceivedAction(object param)
        {
            try
            {
                string str = param as string;
                if (str == "PaymentPeceived")
                {
                    if (RoleManager.Role == UserRole.SuperAdmin)
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 3;
                    else
                        //if (RoleManager.Role == UserRole.Agent)
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;

                    PolicySelectedFollowUpCommissionDashBoard.IssueStatusId = 2;
                    serviceClients.FollowupIssueClient.AddUpdateIssue(PolicySelectedFollowUpCommissionDashBoard);
                    PolicyFollowUpCommissionDashBoardLst = FillFollowUpIssue();
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
                    CloseCommdashFollowUpWindow();
                }
                else if (str == "ResolvedInvoice")
                {
                    if (RoleManager.Role == UserRole.SuperAdmin)
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 3;
                    else if (RoleManager.Role == UserRole.Agent)
                        PolicySelectedFollowUpCommissionDashBoard.IssueResultId = 1;
                    PolicySelectedFollowUpCommissionDashBoard.IssueStatusId = 2;
                    serviceClients.FollowupIssueClient.AddUpdateIssue(PolicySelectedFollowUpCommissionDashBoard);
                    PolicyFollowUpCommissionDashBoardLst = FillFollowUpIssue();
                    PolicySelectedFollowUpCommissionDashBoard = PolicyFollowUpCommissionDashBoardLst.FirstOrDefault();
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
        #endregion

        public void Refresh()
        {
            try
            {
                OnSelectedLicenseeChanged();
                OutgoingPayeeList = FillOutgoingPayeeUser();
            }
            catch 
            {
            }
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

        #region ReplacePolicy
        public delegate void OpenReplacedPolicy();
        public event OpenReplacedPolicy OpenReplacedPolicyEvent;

        public delegate void CloseReplacedPolicy();
        public event CloseReplacedPolicy CloseReplacedPolicyEvent;
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
            string str = param as string;
            if (str == "New")
            {
                ShowHideTypeOfPolicy = DesignerSerializationVisibility.Hidden.ToString();

            }
            else if (str == "Replace")
            {
                ShowHideTypeOfPolicy = DesignerSerializationVisibility.Visible.ToString();

            }
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

        private bool BeforeShowReplacePolicy()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
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
                //ReplacePolicyTermDate
                OpenReplacedPolicyEvent();
            }
            return SelecetdPolicylstForReplace;
        }

        private ObservableCollection<PolicyDetailsData> FillReplacePolicyGrid()
        {
            List<PolicyDetailsData> _policylst = serviceClients.PolicyClient.GetPoliciesLicenseeWise(SharedVMData.SelectedLicensee.LicenseeId, _PolicyStatus.Any, null)
                .Where(p => p.IsDeleted == false)
                .Where(p => p.PolicyStatusId != 1)
                .ToList();
            if (SelectedPolicy.ReplacedBy.HasValue)
            {
                if (SelectedPolicy.ReplacedBy != Guid.Empty)
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("PolicyId", SelectedPolicy.ReplacedBy ?? Guid.Empty);
                    _policylst.Add(serviceClients.PolicyClient.GetPolicydata(parameters).FirstOrDefault());
                }
            }
            ObservableCollection<PolicyDetailsData> policylst = new ObservableCollection<PolicyDetailsData>(_policylst);
            return policylst;

        }
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
            if (SelecetdPolicylstForReplace.PolicyId == Guid.Empty)
            {
                MessageBox.Show("No Policy is selecetd", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SelecetdPolicylstForReplace.TerminationReasonId = SelectedReplacePolicyTermReason.TerminationReasonId;
            SelecetdPolicylstForReplace.PolicyTerminationDate = ReplacePolicyTermDate;
            SelecetdPolicylstForReplace.PolicyStatusId = (int)_PolicyStatus.Terminated;

            //serviceClients.PolicyClient.AddUpdatePolicy(SelecetdPolicylstForReplace);
            CloseReplacePolicy();
            //MakeReplacePolicyToSave();
        }

        private void MakeReplacePolicyToSave()
        {
            if (SelecetdPolicylstForReplace != null)
            {
                //serviceClients.PolicyClient.AddUpdatePolicy(SelecetdPolicylstForReplace);

                SelectedPolicy.Insured = SelecetdPolicylstForReplace.Insured;
                SelectedPolicy.Enrolled = SelecetdPolicylstForReplace.Enrolled;
                SelectedPolicy.Eligible = SelecetdPolicylstForReplace.Eligible;
                SelectedPolicy.IsOutGoingBasicSchedule = SelecetdPolicylstForReplace.IsOutGoingBasicSchedule;
                SelectedPolicy.ReplacedBy = SelecetdPolicylstForReplace.PolicyId;

                // if (SelecetdPolicylstForReplace.IsOutGoingBasicSchedule.HasValue)
                {
                    // if (SelectedPolicy.IsOutGoingBasicSchedule.Value)
                    {
                        ObservableCollection<OutGoingPayment> _OutGoingPayment = serviceClients.OutGoingPaymentClient.GetOutgoingSheduleForPolicy(SelecetdPolicylstForReplace.PolicyId);
                        if (_OutGoingPayment != null)
                        {
                            _OutGoingPayment.ToList().ForEach(p => p.PolicyId = SelectedPolicy.PolicyId);
                            _OutGoingPayment.ToList().ForEach(p => p.OutgoingScheduleId = Guid.NewGuid());
                        }
                        //serviceClients.OutGoingPaymentClient.AddUpdateOutgoingPayment(_OutGoingPayment);
                    }
                    //  else
                    {
                        PolicyOutgoingSchedule _OutgoingShedule = serviceClients.OutgoingScheduleClient.GetOutgoingSheduleBy(SelecetdPolicylstForReplace.PolicyId);
                        if (_OutgoingShedule != null)
                        {
                            _OutgoingShedule.PolicyId = SelectedPolicy.PolicyId;
                            if (_OutgoingShedule != null && _OutgoingShedule.OutgoingScheduleList != null)
                                _OutgoingShedule.OutgoingScheduleList.ToList().ForEach(p => p.CoveragesScheduleId = Guid.NewGuid());
                        }
                        // serviceClients.OutgoingScheduleClient.AddUpdateOutgoingShedule(_OutgoingShedule);
                    }
                }
            }
            else if (SelecetdPolicylstForReplace != null)
            {
                SelectedPolicy.ReplacedBy = SelecetdPolicylstForReplace.PolicyId;
            }

            else
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

        private string replaceBtntooltip = "";

        public string ReplaceBtntooltip
        {
            get { return replaceBtntooltip; }
            set { replaceBtntooltip = value; OnPropertyChanged("ReplaceBtntooltip"); }
        }
        #endregion
        
    }

    public class PrimaryAgent
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}
