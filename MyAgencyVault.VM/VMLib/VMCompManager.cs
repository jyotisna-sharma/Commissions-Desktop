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
using System.Diagnostics;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;
using System.Transactions;
using iTextSharp.text.pdf;
using System.Data;
using MyAgencyVault.EmailFax;
using System.Threading.Tasks;
using System.Net;

namespace MyAgencyVault.ViewModel.VMLib
{
    public class VMCompManager : BaseViewModel, IDataRefresh
    {

        #region "Local Variable"
        Guid? temp = null;
        Guid? tempPolicy = null;
        WebDevPath webDevPath = null;
        Batch _CurrBatchInfo = null;
        ClientAndPayment _CurrInsuredInfo = null;
        private bool ShowPaymentsClicked = false;
        LicenseeDisplayData _CurrLicenseInfo = null;
        private Dictionary<Guid, bool> _DataPopulationforLicenceeInprogress;
        static MastersClient objLog = new MastersClient();
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


        private ICommand _UploadBatch = null;
        private AutoResetEvent autoResetEvent;
        ObservableCollection<Batch> _BatchInfo = null;
        ObservableCollection<ClientAndPayment> _InsuredInfo = null;

        SystemConstant objMasterSystemConstants = null;
        private string serverPath;

        #endregion

        #region Public - FillDataFunction

        public void FillLinkPaymentClients()
        {
            try
            {
                if (SharedVMData.CachedClientList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                {
                    LinkPaymentClientLst = new ObservableCollection<Client>(SharedVMData.CachedClientList[SharedVMData.SelectedLicensee.LicenseeId]);
                }
                else
                {
                    LinkPaymentClientLst = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId));
                }
                if (LinkPaymentClientLst != null)
                    LinkPaymentClientLst = new ObservableCollection<Client>(LinkPaymentClientLst.OrderBy(p => p.Name));
                Client cl = new Client()
                {
                    ClientId = Guid.Empty,
                    LicenseeId = Guid.Empty,
                    Name = "--All Client--",
                };
                LinkPaymentClientLst.Add(cl);
                LinkPaymentSelectedClient = LinkPaymentClientLst.FirstOrDefault();

            }
            catch 
            {
            }

        }

        #endregion

        VMLinkPaymentClientChangeDialog _VMLinkPaymentClientChangeDialog;

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

        public VMCompManager(VMLinkPaymentClientChangeDialog _obj)
        {
            try
            {
                string key = "WebDevPath";
                string KeyValue = string.Empty;
                if (SharedVMData.MasterSystemConstants.ContainsKey(key))
                {
                    KeyValue = SharedVMData.MasterSystemConstants[key];
                }
                {
                    KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                    SharedVMData.MasterSystemConstants.Add(key, KeyValue);
                }
                webDevPath = WebDevPath.GetWebDevPath(KeyValue);
                serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicyCompleted += new EventHandler<GetAllPoliciesForLinkedPolicyCompletedEventArgs>(LinkPaymentPoliciesClient_GetAllPoliciesForLinkedPolicyCompleted);
                serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicyCompleted += new EventHandler<GetPendingPoliciesForLinkedPolicyCompletedEventArgs>(LinkPaymentPoliciesClient_GetPendingPoliciesForLinkedPolicyCompleted);
                _VMLinkPaymentClientChangeDialog = _obj;
                BatchLst = new ObservableCollection<Batch>();
                FillBatchDate();
                PropertyChanged += new PropertyChangedEventHandler(VMCompManager_PropertyChanged);
                CompupScreenControl();
                //btnSatementEnable enable true by default
                btnSatementEnable = true;                

            }
            catch
            {
            }
        }
        
        private void FillBatchDate()
        {
            try
            {
                DateTime _TodayDate = DateTime.Today;
                BatchDate = new ObservableCollection<DateTime>();
                for (int idx = 0; idx < 5; idx++)
                {
                    BatchDate.Add(_TodayDate.AddYears(-idx));
                }
                SelectedBatchDate = BatchDate.FirstOrDefault();
            }
            catch
            {
            }
        }

        #region ControlLevelProerty
        #region SettingScreen
        private void CompupScreenControl()
        {
            try
            {
                if (RoleManager.Role == UserRole.Agent)
                {
                    // AgencyBatchComboEnable = false;
                    LinkPaymentToPolicyTab = System.Windows.Visibility.Visible.ToString();
                    AgencyLinkPaymentToPolicyComboEnable = true;

                    if (RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Read)
                    {
                        //AgencyBatchComboEnable = false;
                        LinkPaymentToPolicyTab = System.Windows.Visibility.Visible.ToString();
                        AgencyLinkPaymentToPolicyComboEnable = false;
                    }
                }
                else if (RoleManager.Role == UserRole.Administrator)
                {
                    //AgencyBatchComboEnable = false;
                    LinkPaymentToPolicyTab = System.Windows.Visibility.Visible.ToString();
                    AgencyLinkPaymentToPolicyComboEnable = false;
                }
                else if (RoleManager.Role == UserRole.DEP)
                {
                    // AgencyBatchComboEnable = true;
                    LinkPaymentToPolicyTab = System.Windows.Visibility.Collapsed.ToString();
                    AgencyLinkPaymentToPolicyComboEnable = false;
                }
                else if (RoleManager.Role == UserRole.HO)
                {
                    // AgencyBatchComboEnable = true;
                    LinkPaymentToPolicyTab = System.Windows.Visibility.Visible.ToString();
                    AgencyLinkPaymentToPolicyComboEnable = false;
                }
                else if (RoleManager.Role == UserRole.SuperAdmin)
                {
                    //AgencyBatchComboEnable = true;
                    LinkPaymentToPolicyTab = System.Windows.Visibility.Visible.ToString();
                    AgencyLinkPaymentToPolicyComboEnable = true;
                }
            }
            catch (Exception)
            {
            }

        }
        #region for batch note
       
        private string _notecontent;
        public string batchNotecontent
        {
            get { return _notecontent; }
            set
            {
                _notecontent = value;
                OnPropertyChanged("batchNotecontent");
            }
        }

        private ICommand _SaveBatch = null;        
        public ICommand SaveBatch
        {
            get
            {
                if (_SaveBatch == null)
                {
                    _SaveBatch = new BaseCommand(x => BeforeSaveBatchNote(), x => SaveBatchNote());
                }
                return _SaveBatch;
            }
        }

        private bool BeforeSaveBatchNote()
        {
            if (SelectedBatch != null)
                return true;
            else
                return false;
        }

        private void SaveBatchNote()
        {
            if (SelectedBatch != null)
            {
                if (SelectedBatch.BatchId != Guid.Empty)
                {
                    serviceClients.BatchClient.AddUpdateBatchNote(SelectedBatch.BatchNumber, batchNotecontent);
                    SelectedBatch.BatchNote = batchNotecontent;                   
                }
            }
            
        }
      #endregion 

        private string linkpaymenttopolicytab = System.Windows.Visibility.Visible.ToString();
        public string LinkPaymentToPolicyTab
        {
            get
            {
                return linkpaymenttopolicytab;
            }
            set
            {
                linkpaymenttopolicytab = value;
                OnPropertyChanged("linkpaymenttopolicytab");
            }
        }

        private bool agencylinkpaymenttopolicycomboenable = false;
        public bool AgencyLinkPaymentToPolicyComboEnable
        {
            get
            {
                return agencylinkpaymenttopolicycomboenable;
            }
            set
            {
                agencylinkpaymenttopolicycomboenable = value;
                OnPropertyChanged("AgencyLinkPaymentToPolicyComboEnable");
            }
        }

        #endregion
        #endregion

        #region "Public Properties "


        private Client changedClient;

        public Client ChangedClient
        {
            get { return changedClient; }
            set { changedClient = value; OnPropertyChanged("ChangedClient"); }
        }

        void VMCompManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "BatchLst":
                    ShowPaymentsClicked = false;                    
                    break;

                case "LinkPaymentSelectedLicensee":
                    if (SharedVMData.CachedPayorLists.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        LinkPaymentPayorLst = new ObservableCollection<DisplayedPayor>(SharedVMData.CachedPayorLists[SharedVMData.SelectedLicensee.LicenseeId]);
                    }
                    else
                    {
                        PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
                        LinkPaymentPayorLst = new ObservableCollection<DisplayedPayor>(serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo));
                    }
                    DisplayedPayor patem = new DisplayedPayor() { PayorID = Guid.Empty, PayorName = "--All payor--", };
                    LinkPaymentPayorLst.Insert(0, patem);

                    LinkPaymentSelecetedPayor = LinkPaymentPayorLst.FirstOrDefault();
                    if (SharedVMData.CachedClientList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        LinkPaymentClientLst = new ObservableCollection<Client>(SharedVMData.CachedClientList[SharedVMData.SelectedLicensee.LicenseeId]);
                    }
                    else
                    {
                        LinkPaymentClientLst = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId));
                    }
                    Client cltemp = new Client() { ClientId = Guid.Empty, LicenseeId = Guid.Empty, Name = "--All Client--", };
                    LinkPaymentClientLst.Insert(0, cltemp);

                    LinkPaymentSelectedClient = LinkPaymentClientLst.FirstOrDefault();
                    try
                    {
                        //LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(p => p.CreatedOn));
                        LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
                        LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.FirstOrDefault();
                    }
                    catch (Exception)
                    {
                    }
                    break;
                case "LinkPaymentSelectedClient":
                    if (LinkPaymentActivePoliciesLstCollection.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLstCollection[SharedVMData.SelectedLicensee.LicenseeId].OrderBy(p => p.ClientName));
                        //LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientId!=Guid.Empty).ToList());
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));

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
                case "LinkPaymentSelecetedPayor":
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
                case "LinkPaymentSelectedPendingPolicies":
                   
                    LinkPaymentSelectedClient = LinkPaymentClientLst.Where(p => p.ClientId == Guid.Empty).FirstOrDefault();
                    LinkPaymentSelectedPendingPolicies.Entries = serviceClients.LinkPaymentReciptRecordsClient.GetLinkPaymentReciptRecordsByPolicyId(LinkPaymentSelectedPendingPolicies.PolicyId);
                    LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.Where(p => p.PolicyId == tempPolicy).FirstOrDefault();
                    if (temp == null)
                    {
                        LinkPaymentSelecetedPayor = LinkPaymentPayorLst.Where(p => p.PayorID == LinkPaymentSelectedPendingPolicies.PayorId).FirstOrDefault();
                    }
                    else
                    {
                        LinkPaymentSelecetedPayor = LinkPaymentPayorLst.Where(p => p.PayorID == temp).FirstOrDefault();
                    }

                    break;
                case "LinkPaymentSelectedActivePolicies": 
                    break;
                case "LinkPaymentReciptSelectedRecords":
                    break;
                case "BatchSelectedStatement":
                    if (BatchSelectedStatement.StatmentId != Guid.Empty)
                    {
                        if (ShowPaymentsClicked)
                        {
                            BatchInsuredRecoredLst = serviceClients.BatchInsuredRecordsClient.GetInsuredPayments(BatchSelectedStatement.StatmentId);
                            BatchSelectedInsuredRecored = BatchInsuredRecoredLst.FirstOrDefault();
                        }
                    }
                    else                   
                        BatchInsuredRecoredLst = null;
                   

                    break;
                case "SelectedBatch":
                    if (SelectedBatch != null && SelectedBatch.BatchId != Guid.Empty)
                    {
                        BatchStatementLst = serviceClients.BatchStatmentRecordsClient.GetBatchStatmentWithoutCalculation(SelectedBatch.BatchId);
                        BatchStatementLst = new ObservableCollection<BatchStatmentRecords>(BatchStatementLst.OrderBy(s => s.StatmentNumber));
                        ShowPaymentsClicked = false;
                        BatchSelectedStatement = BatchStatementLst.FirstOrDefault();
                        if (SelectedBatch.BatchNote != "" || SelectedBatch.BatchNote != string.Empty)
                        {
                            batchNotecontent =SelectedBatch.BatchNote ;
                        }
                    }
                    else
                    {
                        //SelectedBatch != null Or  SelectedBatch.BatchId != Guid.Empty then clear the collection
                        BatchStatementLst.Clear();
                    }
                    break;

                case "BatchSelectedLicensee":
                    if (SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                    {
                        ShowPaymentsClicked = false;
                        BatchLst = new ObservableCollection<Batch>(serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, SelectedBatchDate));
                        if (BatchLst != null && BatchLst.Count != 0)
                            SelectedBatch = BatchLst.FirstOrDefault();
                    }
                    break;

                case "SelectedBatchDate":
                    if (SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                    {
                        BatchLst = new ObservableCollection<Batch>(serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, SelectedBatchDate).OrderByDescending(date => date.CreatedDate.Value.Date).ThenByDescending(batch => batch.BatchNumber).ToList());
                        SelectedBatch = BatchLst.FirstOrDefault();
                    }
                    break;               

                case "SetClientByLinkedDialog":
                    if (LinkPaymentSelectedPendingPolicies == null)
                    {
                        MessageBox.Show("Illegal Operation");
                        return;
                    }
                    if (SetClientByLinkedDialog == null || SetClientByLinkedDialog.ClientId == Guid.Empty)
                    {
                        return;
                    }

                    serviceClients.LinkPaymentPoliciesClient.MakePolicyActive(LinkPaymentSelectedPendingPolicies.PolicyId, SetClientByLinkedDialog.ClientId, RoleManager.userCredentialID);
                    serviceClients.PolicyClient.SavePolicyLastUpdated(LinkPaymentSelectedPendingPolicies.PolicyId, RoleManager.userCredentialID);
                    try
                    {
                        SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetAllPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId));
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => ((p.ClientId == LinkPaymentSelectedClient.ClientId) && (p.PayorId == LinkPaymentSelecetedPayor.PayorID))));
                        LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
                        LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.FirstOrDefault();
                        LinkPaymentPendingPoliciesLst.Remove(LinkPaymentPendingPoliciesLst.Where(p => p.PolicyId == LinkPaymentSelectedPendingPolicies.PolicyId).FirstOrDefault());
                        LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.FirstOrDefault();
                    }
                    catch(Exception ex)
                    {
                        if(objLog != null)
                        {
                            objLog.AddLog("Make Policy aactive compl manager Exception: " + ex.Message);
                        }
                    }
                    break;

                default:
                    break;

            }
        }

        void LinkPaymentPoliciesClient_GetAllPoliciesForLinkedPolicyCompleted(object sender, GetAllPoliciesForLinkedPolicyCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(e.Result);
                LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
                if (LinkPaymentActivePoliciesLstCollection.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                {
                    LinkPaymentActivePoliciesLstCollection.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                }
                LinkPaymentActivePoliciesLstCollection.Add(SharedVMData.SelectedLicensee.LicenseeId, LinkPaymentActivePoliciesLst);
                // add new line for selection show in grid
                LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.Where(s => s.ClientId == SharedVMData.NewClientid).FirstOrDefault();

                DataPopulationforLicenceeInprogress[SharedVMData.SelectedLicensee.LicenseeId] = false;
                SharedVMData.NewClientid = Guid.Empty;
            }
        }

        private LinkPaymentReciptRecords _SelectedEntries;
        public LinkPaymentReciptRecords SelectedEntries
        {
            get { return _SelectedEntries; }
            set { _SelectedEntries = value; OnPropertyChanged("SelectedEntries"); }
        }
        private decimal _CheckAmt;
        public decimal CheckAmt
        {
            get { return _CheckAmt; }
            set { _CheckAmt = value; OnPropertyChanged("CheckAmt"); }
        }

        private decimal _HouseAmount;
        public decimal HouseAmount
        {
            get { return _HouseAmount; }
            set { _HouseAmount = value; OnPropertyChanged("HouseAmount"); }
        }

        private decimal _RemainingAmt;
        public decimal RemainingAmt
        {
            get
            {
                return _RemainingAmt;
            }
            set
            {
                _RemainingAmt = value;
                OnPropertyChanged("RemainingAmt");
            }
        }

        private bool _btnSatementEnable;
        public bool btnSatementEnable
        {
            get
            {
                return _btnSatementEnable;
            }
            set
            {
                _btnSatementEnable = value;
                OnPropertyChanged("btnSatementEnable");
            }
        }

        private double _DonePer;
        public double DonePer
        {
            get
            {
                return _DonePer;
            }
            set
            {
                _DonePer = value;
                OnPropertyChanged("DonePer");
            }
        }

        private int _TotalEntry;
        public int TotalEntry
        {
            get
            {
                return _TotalEntry;
            }
            set
            {
                _TotalEntry = value;
                OnPropertyChanged("TotalEntry");
            }
        }

        private Batch _SelectedBatch;
        public Batch SelectedBatch
        {
            get
            {
                return _SelectedBatch;
            }
            set
            {
                _SelectedBatch = value;
                OnPropertyChanged("SelectedBatch");
            }
        }

        private ObservableCollection<Batch> _BatchLst;
        public ObservableCollection<Batch> BatchLst
        {
            get
            {
                return _BatchLst;
            }
            set
            {
                _BatchLst = value;
                OnPropertyChanged("BatchLst");
            }
        }

        private InsuredPayment _BatchSelectedInsuredRecored;

        public InsuredPayment BatchSelectedInsuredRecored
        {
            get { return _BatchSelectedInsuredRecored; }
            set { _BatchSelectedInsuredRecored = value; OnPropertyChanged("BatchSelectedInsuredRecored"); }
        }

        private ObservableCollection<InsuredPayment> _BatchInsuredRecoredLst;

        public ObservableCollection<InsuredPayment> BatchInsuredRecoredLst
        {
            get { return _BatchInsuredRecoredLst; }
            set { _BatchInsuredRecoredLst = value; OnPropertyChanged("BatchInsuredRecoredLst"); }
        }

        private BatchStatmentRecords _BatchSelectedStatement;
        public BatchStatmentRecords BatchSelectedStatement
        {
            get
            {
                return _BatchSelectedStatement == null ? new BatchStatmentRecords() : _BatchSelectedStatement;

            }

            set
            {
                _BatchSelectedStatement = value;
                OnPropertyChanged("BatchSelectedStatement");

            }
        }

        private BatchStatmentRecords _TotalBatchStatement;
        public BatchStatmentRecords TotalBatchStatement
        {
            get
            {
                if (_TotalBatchStatement == null)
                    _TotalBatchStatement = new BatchStatmentRecords();
                return _TotalBatchStatement;
            }
        }

        private ObservableCollection<BatchStatmentRecords> _BatchStatementLst;
        public ObservableCollection<BatchStatmentRecords> BatchStatementLst
        {
            get
            {
                return _BatchStatementLst == null ? new ObservableCollection<BatchStatmentRecords>() : _BatchStatementLst;
            }
            set
            {
                _BatchStatementLst = value;
                OnPropertyChanged("BatchStatementLst");
            }
        }

        private bool _IsAllChecked;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }
            set
            {
                _IsAllChecked = value;
                OnPropertyChanged("IsAllChecked");

            }
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

        private ICommand _ShowPayments;
        public ICommand ShowPayments
        {
            get
            {
                if (_ShowPayments == null)
                {
                    _ShowPayments = new BaseCommand(x => BeforeOnShowPayments(), x => OnShowPayments());
                }
                return _ShowPayments;
            }

        }

        private bool BeforeOnShowPayments()
        {
            bool bValue = true;

            if (SelectedBatch == null)
                return false;
            else

                return bValue;
        }

        private void OnShowPayments()
        {
            ShowPaymentsClicked = true;
            ShowTotalPayment();
        }

        private void ShowTotalPayment()
        {
            foreach (var item in BatchLst)
            {
                if (item != null)
                {
                    decimal batchTotal = (serviceClients.BatchStatmentRecordsClient.GetBatchTotal(item.BatchId));

                    if (batchTotal > 0)
                        item.TotalStatementAmount = "$" + String.Format("{0:0,0.00}", batchTotal);
                    else
                        item.TotalStatementAmount = "$" + String.Format("{0:0.00}", batchTotal);
                }
            }
            
            BatchStatementLst = serviceClients.BatchStatmentRecordsClient.GetBatchStatment(SelectedBatch.BatchId);
            BatchStatementLst = new ObservableCollection<BatchStatmentRecords>(BatchStatementLst.OrderBy(s => s.StatmentNumber));
            BatchSelectedStatement = BatchStatementLst.FirstOrDefault();
            decimal checkamt = 0;
            decimal houseamt = 0;
            decimal remaning = 0;
            decimal enteramount = 0;
            decimal TotalDonePercent = 0;
            decimal NetCheck = 0;
            //DonePer = 0;
            int entry = 0;
            Parallel.ForEach(BatchStatementLst, bst =>
           //foreach (BatchStatmentRecords bst in BatchStatementLst)
           //{
           {
               if (bst.CheckAmount != null)
                   checkamt += bst.CheckAmount.Value;
               if (bst.House != null)
                   houseamt += bst.House.Value;
               if (bst.Remaining != null)
                   remaning += bst.Remaining.Value;
               if (bst.Entries != null)
                   entry += bst.Entries;
               if (bst.EnterAmount != null)
                   enteramount += bst.EnterAmount.Value;

               if (bst.CheckAmount != null)
               {
                   if (bst.BalAdj != null)
                   {
                       NetCheck += Convert.ToDecimal(bst.CheckAmount - bst.BalAdj);
                   }
                   else
                   {
                       NetCheck += Convert.ToDecimal(bst.CheckAmount);
                   }

               }

               if ((NetCheck != null) && (bst.Remaining != null))
               {
                   if (NetCheck != 0)
                   {
                       bst.DonePer = (double)(((NetCheck - bst.Remaining) / NetCheck));
                   }
                   else
                       bst.DonePer = 0;
               }

           });

            CheckAmt = checkamt;
            HouseAmount = houseamt;
            RemainingAmt = remaning;           

            //if (CheckAmt != 0)
            //    TotalDonePercent = (((CheckAmt - RemainingAmt) / CheckAmt));
            //else
            //    TotalDonePercent = 0;

            if (NetCheck != 0)
            {
                TotalDonePercent = (((NetCheck - RemainingAmt) / NetCheck));                
            }
            else
            {
                TotalDonePercent = 0;
            }

            TotalEntry = entry;
            TotalBatchStatement.PayorNickName = "Total:";
            TotalBatchStatement.CheckAmount = CheckAmt;
            TotalBatchStatement.House = HouseAmount;
            TotalBatchStatement.Remaining = RemainingAmt;
            TotalBatchStatement.Entries = TotalEntry;
            TotalBatchStatement.DonePer = Convert.ToDouble(TotalDonePercent);
            TotalBatchStatement.StatmentNumber = null;
            TotalBatchStatement.StmtStatus = null;

            if (BatchStatementLst.Count > 0 && !BatchStatementLst.Contains(TotalBatchStatement))
                BatchStatementLst.Add(TotalBatchStatement);
        }

        //need to modify to calculate fuction to and database hit when deleting statement
        private void CalculatedTotal()
        {
            BatchStatementLst = serviceClients.BatchStatmentRecordsClient.GetBatchStatment(SelectedBatch.BatchId);
            //sorting with statment StatmentNumber
            BatchStatementLst = new ObservableCollection<BatchStatmentRecords>(BatchStatementLst.OrderBy(s => s.StatmentNumber));
            BatchSelectedStatement = BatchStatementLst.FirstOrDefault();

            decimal checkamt = 0;
            decimal houseamt = 0;
            decimal remaning = 0;
            decimal enteramount = 0;
            decimal TotalDonePercent = 0;
            //DonePer = 0;
            int entry = 0;
            decimal NetCheck = 0;
            foreach (BatchStatmentRecords bst in BatchStatementLst)
            {
                if (bst.CheckAmount != null)
                    checkamt += bst.CheckAmount.Value;
                if (bst.House != null)
                    houseamt += bst.House.Value;
                if (bst.Remaining != null)
                    remaning += bst.Remaining.Value;
                if (bst.Entries != null)
                    entry += bst.Entries;
                if (bst.EnterAmount != null)
                    enteramount += bst.EnterAmount.Value;

                if (bst.BalAdj != null)
                    NetCheck += Convert.ToDecimal(bst.CheckAmount - bst.BalAdj);

                if ((NetCheck != null) && (bst.Remaining != null))
                {
                    if (NetCheck != 0)
                    {
                        bst.DonePer = (double)(((NetCheck - bst.Remaining) / NetCheck));
                        if (bst.DonePer > 0)
                        {
                            bst.DonePer = bst.DonePer * 100;
                        }
                    }
                    else
                    {
                        bst.DonePer = 0;
                    }
                }

                //if ((bst.CheckAmount != null) && (bst.Remaining != null))
                //{
                //    if (bst.CheckAmount > 0)
                //    {
                //        bst.DonePer = (double)(((bst.CheckAmount - bst.Remaining) / bst.CheckAmount));
                //    }

                //}
            }

            CheckAmt = checkamt;
            HouseAmount = houseamt;
            RemainingAmt = remaning;

            //if (CheckAmt != 0)
            //    TotalDonePercent = (((CheckAmt - RemainingAmt) / CheckAmt));

            if (NetCheck != 0)
                TotalDonePercent = (((NetCheck - RemainingAmt) / NetCheck));

            TotalEntry = entry;
            TotalBatchStatement.PayorNickName = "Total:";
            TotalBatchStatement.CheckAmount = CheckAmt;
            TotalBatchStatement.House = HouseAmount;
            TotalBatchStatement.Remaining = RemainingAmt;
            TotalBatchStatement.Entries = TotalEntry;
            TotalBatchStatement.DonePer = Convert.ToDouble(TotalDonePercent);
            TotalBatchStatement.StatmentNumber = null;
            TotalBatchStatement.StmtStatus = null;

            if (BatchStatementLst.Count > 0 && !BatchStatementLst.Contains(TotalBatchStatement))
                BatchStatementLst.Add(TotalBatchStatement);
            //User LicenseeName only show the balacee amount ...
            //need to change property balance insted of licenseeName
            if (enteramount > 0)
                SelectedBatch.TotalStatementAmount = "$" + String.Format("{0:0,0.00}", enteramount);
            else
                SelectedBatch.TotalStatementAmount = "$" + String.Format("{0:0.00}", enteramount);

        }

        public void SelectedLicenseeChanged()
        {
            //DateTime Time1; DateTime Time2; DateTime Time3; DateTime Time4;
            if (SharedVMData.SelectedLicensee == null)
                return;

            if (SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
            {
                //Sorting batch by batch Number

                BatchLst = new ObservableCollection<Batch>(serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, SelectedBatchDate).OrderByDescending(o => o.CreatedDate.Value.Date).ThenByDescending(o => o.BatchNumber).ToList());

                if (BatchLst != null && BatchLst.Count != 0)
                {
                    SelectedBatch = BatchLst.FirstOrDefault();
                }
            }
            if (SharedVMData.CachedPayorLists.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
            {
                LinkPaymentPayorLst = new ObservableCollection<DisplayedPayor>(SharedVMData.CachedPayorLists[SharedVMData.SelectedLicensee.LicenseeId]);
            }
            else
            {
                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
                LinkPaymentPayorLst = new ObservableCollection<DisplayedPayor>(serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo));
                SharedVMData.CachedPayorLists.Add(SharedVMData.SelectedLicensee.LicenseeId, LinkPaymentPayorLst);
            }
            DisplayedPayor patem = new DisplayedPayor() { PayorID = Guid.Empty, PayorName = "--All payor--", };
            //LinkPaymentPayorLst.Add(patem);
            LinkPaymentPayorLst.Insert(0, patem);

            LinkPaymentSelecetedPayor = LinkPaymentPayorLst.FirstOrDefault();
            if (SharedVMData.CachedClientList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
            {
                LinkPaymentClientLst = new ObservableCollection<Client>(SharedVMData.CachedClientList[SharedVMData.SelectedLicensee.LicenseeId]);
            }
            else
            {
                LinkPaymentClientLst = new ObservableCollection<Client>(serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId));
            }

            Client cltemp = new Client() { ClientId = Guid.Empty, LicenseeId = Guid.Empty, Name = "--All Client--", };
            //LinkPaymentClientLst.Add(cltemp);
            LinkPaymentClientLst.Insert(0, cltemp);
            LinkPaymentSelectedClient = LinkPaymentClientLst.FirstOrDefault();
            serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicyAsync(SharedVMData.SelectedLicensee.LicenseeId);
            LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.FirstOrDefault();
        }

        void LinkPaymentPoliciesClient_GetPendingPoliciesForLinkedPolicyCompleted(object sender, GetPendingPoliciesForLinkedPolicyCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(e.Result);
                //LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentPendingPoliciesLst.Where(p => p.Entries != null));
                LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentPendingPoliciesLst.OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
                LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.FirstOrDefault();

            }
        }

        private Visibility _dgPendingGridVisible;
        public Visibility DgPendingGridVisible
        {
            get { return _dgPendingGridVisible; }
            set { _dgPendingGridVisible = value; }
        }

        private DisplayedPayor _LinkPaymentSelecetedPayor;
        public DisplayedPayor LinkPaymentSelecetedPayor
        {
            get
            {
                return _LinkPaymentSelecetedPayor == null ? new DisplayedPayor() : _LinkPaymentSelecetedPayor;

            }
            set
            {
                _LinkPaymentSelecetedPayor = value;
                OnPropertyChanged("LinkPaymentSelecetedPayor");
            }
        }

        private ObservableCollection<DisplayedPayor> _LinkPaymentPayorLst;
        public ObservableCollection<DisplayedPayor> LinkPaymentPayorLst
        {
            get
            {
                return _LinkPaymentPayorLst == null ? new ObservableCollection<DisplayedPayor>() : _LinkPaymentPayorLst;

            }
            set
            {
                _LinkPaymentPayorLst = value;
                OnPropertyChanged("LinkPaymentPayorLst");
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

        private static LinkPaymentReciptRecords _LinkPaymentReciptSelectedRecords1;
        public static LinkPaymentReciptRecords LinkPaymentReciptSelectedRecords1
        {
            get { return _LinkPaymentReciptSelectedRecords1 == null ? new LinkPaymentReciptRecords() : _LinkPaymentReciptSelectedRecords1; }
            set { _LinkPaymentReciptSelectedRecords1 = value; }
        }

        private LinkPaymentReciptRecords _LinkPaymentReciptSelectedRecords;
        public LinkPaymentReciptRecords LinkPaymentReciptSelectedRecords
        {
            get { return _LinkPaymentReciptSelectedRecords == null ? new LinkPaymentReciptRecords() : _LinkPaymentReciptSelectedRecords; }
            set { _LinkPaymentReciptSelectedRecords = value; OnPropertyChanged("LinkPaymentReciptSelectedRecords"); }
        }

        private LinkPaymentPolicies _LinkPaymentSelectedPendingPolicies;
        public LinkPaymentPolicies LinkPaymentSelectedPendingPolicies
        {
            get
            {
                return _LinkPaymentSelectedPendingPolicies == null ? new LinkPaymentPolicies() : _LinkPaymentSelectedPendingPolicies;
            }
            set
            {
                _LinkPaymentSelectedPendingPolicies = value;
                OnPropertyChanged("LinkPaymentSelectedPendingPolicies");
            }
        }

        private ObservableCollection<LinkPaymentPolicies> _LinkPaymentPendingPoliciesLst;
        public ObservableCollection<LinkPaymentPolicies> LinkPaymentPendingPoliciesLst
        {
            get
            {
                return _LinkPaymentPendingPoliciesLst == null ? new ObservableCollection<LinkPaymentPolicies>() : _LinkPaymentPendingPoliciesLst;
            }
            set
            {
                _LinkPaymentPendingPoliciesLst = value;
                OnPropertyChanged("LinkPaymentPendingPoliciesLst");

            }
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
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

        public string ServerFilepath
        {
            get
            {
                return serverPath;
            }
            set
            {
                serverPath = value;
            }
        }

        private ObservableCollection<SystemConstant> _systemConstantsDetail = null;
        public ObservableCollection<SystemConstant> SystemConstantDetails
        {
            get
            {
                return _systemConstantsDetail == null ? new ObservableCollection<SystemConstant>() : _systemConstantsDetail;
            }

            set
            {
                _systemConstantsDetail = value;
                OnPropertyChanged("SystemConstantDetails");
            }

        }

        private DateTime _SelectedBatchDate;
        public DateTime SelectedBatchDate
        {
            get
            {
                return _SelectedBatchDate;
            }

            set
            {
                _SelectedBatchDate = value;
                OnPropertyChanged("SelectedBatchDate");

            }
        }

        private ObservableCollection<DateTime> _BatchDate;
        public ObservableCollection<DateTime> BatchDate
        {
            get
            {
                return _BatchDate;
            }

            set
            {
                _BatchDate = value;
                OnPropertyChanged("BatchDate");
            }

        }

        public ClientAndPayment InsuredInfoData
        {
            get
            {
                return _CurrInsuredInfo == null ? new ClientAndPayment() : _CurrInsuredInfo;
            }

            set
            {
                _CurrInsuredInfo = value;
                OnPropertyChanged("InsuredInfoData");

            }
        }

        public ObservableCollection<ClientAndPayment> InsuredInfoDetails
        {
            get
            {
                return _InsuredInfo;
            }

            set
            {
                _InsuredInfo = value;
                OnPropertyChanged("InsuredInfoDetails");

            }
        }
        
        private Client _SetClientByLinkedDialog;
        public Client SetClientByLinkedDialog
        {
            get { return _SetClientByLinkedDialog == null ? new Client() : _SetClientByLinkedDialog; }
            set { _SetClientByLinkedDialog = value; OnPropertyChanged("SetClientByLinkedDialog"); }
        }
        #endregion

        private ICommand _DeleteBatch;
        public ICommand DeleteBatch
        {
            get
            {
                if (_DeleteBatch == null)
                {
                    _DeleteBatch = new BaseCommand(x => BeforeOnDeleteBatch(), x => OnDeleteBatch());
                }
                return _DeleteBatch;
            }


        }

        private bool BeforeOnDeleteBatch()
        {
            if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                return false;

            if (SelectedBatch != null && SelectedBatch.EntryStatus != EntryStatus.Paid)
                return true;
            else
                return false;
        }

        private void OnDeleteBatch()
        {
             if(objLog==null) objLog = new MastersClient();
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (SelectedBatch != null)
                {
                   objLog.AddLog("DeleteBatch request : BatchId - " + SelectedBatch.BatchId + ", User: " + RoleManager.LoggedInUser + ", UserID: " + RoleManager.userCredentialID);
                    if (serviceClients.BatchClient.DeleteBatch(SelectedBatch.BatchId, RoleManager.Role))
                    {
                        BatchLst.Remove(SelectedBatch);
                       objLog.AddLog("DeleteBatch Success : BatchId - " + SelectedBatch.BatchId );
                    }
                    else
                    {
                        MessageBox.Show("Batch cannot be deleted before statement", "Batch deletion");
                    }

                }
            }
        }

        private ICommand _LinkPendingPolicyCmd;
        public ICommand LinkPendingPolicyCmd
        {
            get
            {
                if (_LinkPendingPolicyCmd == null)
                {
                    _LinkPendingPolicyCmd = new BaseCommand(x => BeforeAttachLinkPendingPolicy(), x => AttachLinkPendingPolicy());
                }
                return _LinkPendingPolicyCmd;
            }


        }

        private bool BeforeAttachLinkPendingPolicy()
        {
            try
            {
                if (LinkPaymentSelectedPendingPolicies == null) return false;
                if (LinkPaymentSelectedPendingPolicies.Entries == null) return false;
                if (LinkPaymentSelectedPendingPolicies.Entries.Count == 0) return false;
                if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private ICommand _OpenChangedClientDialog;
        public ICommand OpenChangedClientDialog
        {
            get
            {
                if (_OpenChangedClientDialog == null)
                {
                    _OpenChangedClientDialog = new BaseCommand(x => BeforeOpenClientDialog(), x => OpenClientDialog());
                }
                return _OpenChangedClientDialog;
            }
        }

        private bool BeforeOpenClientDialog()
        {
            try
            {
                if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                    return false;
                else if (LinkPaymentSelectedPendingPolicies == null) return false;
                else if (LinkPaymentSelectedPendingPolicies.PolicyId == Guid.Empty) return false;

                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private void OpenClientDialog()
        {
            #region"Set the previous value"

            string strPayor = LinkPaymentSelecetedPayor.PayorName;
            Guid guidPayaorId = LinkPaymentSelecetedPayor.PayorID;

            string str = LinkPaymentSelectedClient.Name;
            Guid guidClientID = LinkPaymentSelectedClient.ClientId;

            #endregion

            SetClientByLinkedDialog = _VMLinkPaymentClientChangeDialog.Show();

            LinkPaymentActivePoliciesLst.Clear();
            LinkPaymentActivePoliciesLstCollection.Clear();
            if (!LinkPaymentActivePoliciesLstCollection.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
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

            LinkPaymentSelecetedPayor.PayorID = guidPayaorId;
            LinkPaymentSelectedClient.ClientId = guidClientID;

            if (LinkPaymentSelecetedPayor.PayorID != Guid.Empty)
            {
                LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.PayorId == LinkPaymentSelecetedPayor.PayorID)));
            }

            LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName));
        }

        private void AttachLinkPendingPolicy()
        {
             if(objLog==null) objLog = new MastersClient();

            bool IsAgencyVersion = false;//Call for check Agency check
            bool IsPaidMarked = false;//Call for check Paid
            bool IsScheduleMatches = false;//Call for check Agency check
            Guid LicenseeId = Guid.Empty;
            bool IsReverse = false;
            bool IsLinkWithExithingPolicy = false;
            bool DoAskForLinkingOrDirect = false;
            string ConfirmationMgs = null;
            string strConfirmationMessage = string.Empty;

            bool isMsgShow = true;
            int x = LinkPaymentPendingPoliciesLst.IndexOf(LinkPaymentPendingPoliciesLst.Where(p => p.PolicyId == LinkPaymentSelectedPendingPolicies.PolicyId).FirstOrDefault());
            temp = LinkPaymentSelecetedPayor.PayorID;

            tempPolicy = LinkPaymentSelectedActivePolicies.PolicyId;

            if (tempPolicy == Guid.Empty)
            {
                LinkPaymentSelectedActivePolicies = LinkPaymentActivePoliciesLst.FirstOrDefault();
                tempPolicy = LinkPaymentSelectedActivePolicies.PolicyId;
            }

            if (tempPolicy == Guid.Empty)
            {
                MessageBox.Show("Please select the policy");
            }

            LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

            IsAgencyVersion = GetAgencyVersion(LicenseeId);

            //Check How to fill this entry
            List<LinkPaymentReciptRecords> _LinkPaymentReciptRecords = null;

            if (LinkPaymentSelectedPendingPolicies.Entries == null) return;

            if (LinkPaymentSelectedPendingPolicies.SelectedEntries == null)
            {
                _LinkPaymentReciptRecords = LinkPaymentSelectedPendingPolicies.Entries.ToList();
            }
            else
            {
                _LinkPaymentReciptRecords = new List<LinkPaymentReciptRecords>();
                _LinkPaymentReciptRecords.Add(LinkPaymentSelectedPendingPolicies.SelectedEntries);
                //Get the selected payment here
                _LinkPaymentReciptRecords = LinkPaymentSelectedPendingPolicies.Entries.Where(i => i.IsSelected == true).ToList();
            }

            foreach (LinkPaymentReciptRecords _PaymentRecipt in _LinkPaymentReciptRecords)
            {
                IsScheduleMatches = CheckForScheduleMatch(_PaymentRecipt.PaymentEntryID, LinkPaymentSelectedActivePolicies.PolicyId);
                if (objLog != null)
                {
                    objLog.AddLog("PaymentEntry: " + _PaymentRecipt.PaymentEntryID + ", Active PolicyID: " + LinkPaymentSelectedActivePolicies.PolicyId + ", IsScheduleMatch: " + IsScheduleMatches);
                }
                IsPaidMarked = CheckForMarkedPaid(_PaymentRecipt.PaymentEntryID);

                if (objLog != null)
                {
                    objLog.AddLog("PaymentEntry: " + _PaymentRecipt.PaymentEntryID + ", Active PolicyID: " + LinkPaymentSelectedActivePolicies.PolicyId + ", IsPaidMarked: " + IsPaidMarked);
                }

                if (IsAgencyVersion && IsPaidMarked && IsScheduleMatches)
                {
                    if (isMsgShow)
                    {
                        IsReverse = MessageShow(LinkedMessges.msg1, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        strConfirmationMessage = LinkedMessges.msg1;
                        isMsgShow = false;
                    }
                    DoAskForLinkingOrDirect = true;
                }
                else if (IsAgencyVersion && IsPaidMarked && !IsScheduleMatches)
                {
                    bool tempflag;
                    if (isMsgShow)
                    {
                        tempflag = MessageShow(LinkedMessges.msg3, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        strConfirmationMessage = LinkedMessges.msg3;
                        isMsgShow = false;
                        if (!tempflag) return;
                    }

                    DoAskForLinkingOrDirect = false;
                }
                else if (IsAgencyVersion && !IsPaidMarked && IsScheduleMatches)
                {
                    ConfirmationMgs = LinkedMessges.msg4;
                    strConfirmationMessage = LinkedMessges.msg4;
                }
                else if (IsAgencyVersion && !IsPaidMarked && !IsScheduleMatches)
                {
                    bool tempFlag;
                    if (isMsgShow)
                    {
                        tempFlag = MessageShow(LinkedMessges.meg5, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        isMsgShow = false;
                        strConfirmationMessage = LinkedMessges.meg5;
                        if (!tempFlag) return;
                    }
                    DoAskForLinkingOrDirect = false;
                }
                else if (!IsAgencyVersion && (IsPaidMarked || !IsPaidMarked))
                {
                    ConfirmationMgs = LinkedMessges.msg6;
                    strConfirmationMessage = LinkedMessges.msg6;
                }

                if (DoAskForLinkingOrDirect)
                {
                    IsLinkWithExithingPolicy = MessageShow(LinkedMessges.msg2, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    strConfirmationMessage = LinkedMessges.msg2;
                }
                else
                    IsLinkWithExithingPolicy = true;

                if (IsLinkWithExithingPolicy)
                {
                    try
                    {
                        objLog.AddLog("Call to link policy with payment: LicenseeID " + LicenseeId + ", Policy: " + _PaymentRecipt.PolicyId + ", User: " + RoleManager.userCredentialID + ", PaymentEntry: " + _PaymentRecipt.PaymentEntryID + ", ClientID: " + _LinkPaymentSelectedPendingPolicies.ClientId);
                        serviceClients.LinkPaymentPoliciesClient.DoLinkPolicy(LicenseeId, IsReverse, IsLinkWithExithingPolicy, _PaymentRecipt.PolicyId, LinkPaymentSelectedPendingPolicies.ClientId, LinkPaymentSelectedActivePolicies.PolicyId, _PaymentRecipt.PaymentEntryID,
                              RoleManager.userCredentialID, LinkPaymentSelectedPendingPolicies.PayorId, LinkPaymentSelectedActivePolicies.PayorId, IsAgencyVersion, IsPaidMarked
                              , IsScheduleMatches, RoleManager.Role);

                        SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                        objLog.AddLog("Link policy with payment success ");
                    }
                    catch(Exception ex)
                    {
                        objLog.AddLog("AttachLinkPendingPolicy excepion : " + ex.Message);
                    }

                }
            }

            if (ConfirmationMgs != null)
                MessageShow(ConfirmationMgs, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            if (LinkPaymentSelectedActivePolicies.Entries == null || LinkPaymentSelectedActivePolicies.Entries.Count == 0)
            {
                LinkPaymentPendingPoliciesLst.Remove(LinkPaymentSelectedPendingPolicies);
            }

            //Need to load Again for refresh data
            //LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(p => p.CreatedOn));
            LinkPaymentPendingPoliciesLst = new ObservableCollection<LinkPaymentPolicies>(serviceClients.LinkPaymentPoliciesClient.GetPendingPoliciesForLinkedPolicy(SharedVMData.SelectedLicensee.LicenseeId).OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
            LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.ElementAtOrDefault(x);

            if (LinkPaymentSelectedPendingPolicies.PolicyId == Guid.Empty)
            {
                LinkPaymentSelectedPendingPolicies = LinkPaymentPendingPoliciesLst.LastOrDefault();
            }


            //if (LinkPaymentSelectedPendingPolicies.PayorId != null)
            //{
            //    LinkPaymentSelecetedPayor = LinkPaymentPayorLst.Where(p => p.PayorID == LinkPaymentSelectedPendingPolicies.PayorId).FirstOrDefault();
            //    LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.Where(p => (p.PayorId == LinkPaymentSelecetedPayor.PayorID)));
            //    LinkPaymentActivePoliciesLst = new ObservableCollection<LinkPaymentPolicies>(LinkPaymentActivePoliciesLst.OrderBy(p => p.ClientName).ThenBy(p => p.PayorName).ThenBy(p => p.PolicyNumber));
            //}

            temp = null;
            tempPolicy = null;

        }

        private PolicyDetailsData GetPolicy(Guid PolicyId)
        {
            PolicyDetailsData _policy = serviceClients.PostUtilClient.GetPolicyPU(PolicyId);
            return _policy;
        }

        private bool CheckForScheduleMatch(Guid EntryId, Guid ActivePolicyId)
        {
            bool flag = serviceClients.LinkPaymentPoliciesClient.ScheduleMatches(EntryId, ActivePolicyId);
            return flag;
        }

        private bool CheckForMarkedPaid(Guid PaymentEntryId)
        {
           // bool flag = serviceClients.PolicyOutgoingDistributionClient.IsEntryMarkPaid(PaymentEntryId);
            //Changed by Jyotisna - to make sure "Reverse" dialog appears if a payment has even a single paid entry.
            bool flag = serviceClients.PolicyOutgoingDistributionClient.IfPaymentHasPaidEntry(PaymentEntryId);
            return flag;
        }

        private bool GetAgencyVersion(Guid LicenseeId)
        {
            return serviceClients.BillingLineDetailClient.IsAgencyVersionLicense(LicenseeId);
        }

        public bool MessageShow(string Message, string Caption, MessageBoxButton _MessageBoxButton, MessageBoxImage _MessageBoxImage)
        {
            bool flag = false;
            if (_MessageBoxButton == MessageBoxButton.YesNo)
            {
                MessageBoxResult _MessageBoxResult = System.Windows.MessageBox.Show(Message, Caption, _MessageBoxButton, _MessageBoxImage);
                flag = _MessageBoxResult == MessageBoxResult.Yes ? true : false;
            }
            else if (_MessageBoxButton == MessageBoxButton.OK)
            {
                MessageBoxResult _MessageBoxResult = System.Windows.MessageBox.Show(Message, Caption, _MessageBoxButton, _MessageBoxImage);

                flag = true;
            }
            return flag;
        }

        private ICommand _viewFile;
        public ICommand ViewFile
        {
            get
            {
                if (_viewFile == null)
                {
                    _viewFile = new BaseCommand(x => BeforeViewBatchFile(), x => ViewBatchFile());

                }
                return _viewFile;
            }
        }

        private bool BeforeViewBatchFile()
        {
            if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                return false;

            if (SelectedBatch != null)
                return true;
            else
                return false;
        }

        public ICommand UploadBatch
        {
            get
            {
                if (_UploadBatch == null)
                {
                    _UploadBatch = new BaseCommand(x => BeforeUploadBatchFile(), x => UploadBatchFile());
                }
                return _UploadBatch;
            }
        }

        private bool BeforeUploadBatchFile()
        {
            if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                return false;

            if (SharedVMData.SelectedLicensee == null)
                return false;

            if (SharedVMData.SelectedLicensee.LicenseeId == null)
                return false;
            else
                return true;
        }

        private ICommand _DeleteStatement;
        public ICommand DeleteStatement
        {
            get
            {
                if (_DeleteStatement == null)
                {
                    _DeleteStatement = new BaseCommand(x => BeforeOnDeleteStatement(), x => OnDeleteStatement());
                }
                return _DeleteStatement;
            }
        }

        private void OnDeleteStatement()
        {
            if (objLog == null)
                objLog = new MastersClient();
            btnSatementEnable = false;
            //set property to delete the statement from colloection
            bool isRemoveFromColloection = false;
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (BatchSelectedStatement != null)
                {   //serviceClients.StatementClient.DeleteStatement(BatchSelectedStatement.StatmentId, RoleManager.Role);
                    //BatchStatementLst.Remove(BatchSelectedStatement);
                    //Thread Threaddelete = new Thread(() =>
                    //{
                    //    if (serviceClients.StatementClient.DeleteStatement(BatchSelectedStatement.StatmentId, RoleManager.Role, "Delete"))
                    //    {
                    //        ////BatchStatementLst.Remove(BatchSelectedStatement);
                    //        //////add to calculate total when Delete the Statement
                    //        //////CalculatedTotal();
                    //    }
                    //});

                    //Threaddelete.IsBackground = true;
                    //isRemoveFromColloection = true;
                    //Threaddelete.Start();
                    objLog.AddLog("DeleteStatement request : StatementID - " + BatchSelectedStatement.StatmentId + ", user: " + RoleManager.LoggedInUser + ", UserID: " + RoleManager.userCredentialID);
                    serviceClients.StatementClient.DeleteStatement(BatchSelectedStatement.StatmentId, RoleManager.Role, "Delete");
                    objLog.AddLog("DeleteStatement Success : StatementID - " + BatchSelectedStatement.StatmentId);
                    //Remove from collection
                    if (isRemoveFromColloection)
                    {
                        BatchStatementLst.Remove(BatchSelectedStatement);
                    }
                }
            }
            btnSatementEnable = true;
        }

        private bool BeforeOnDeleteStatement()
        {
            if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                return false;

            if (BatchSelectedStatement != null && BatchSelectedStatement.CheckAmount == null)
                return false;

            if (BatchSelectedStatement != null && BatchSelectedStatement.PayorNickName != "Total:" && SelectedBatch != null && SelectedBatch.EntryStatus != EntryStatus.Paid)
                return true;
            else
                return false;
        }

        string _RfileName;
        string RemotefileName
        {
            get
            {
                return _RfileName;
            }
            set
            {
                _RfileName = value;
            }
        }

        string _fileName;
        string fileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        private void ViewBatchFile()
        {
            string RemotePath = string.Empty;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                autoResetEvent = new AutoResetEvent(false);

                FileUtility ObjDownload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);

                string localPath = Path.Combine(System.IO.Path.GetTempPath(), Path.GetFileName(SelectedBatch.FileName));

                string strFileExtension = System.IO.Path.GetExtension(SelectedBatch.FileName);

                if (strFileExtension.ToLower().Contains("pdf"))
                {
                    RemotePath = "/UploadBatch/" + SelectedBatch.FileName;
                }
                else
                {
                    RemotePath = "/UploadBatch/Import/Success/" + SelectedBatch.FileName;
                }

                //string RemotePath = "/UploadBatch/" + SelectedBatch.FileName;

                string strLocal = System.IO.Path.GetExtension(localPath);
                string strRemoteExt = System.IO.Path.GetExtension(RemotePath);
                if (string.IsNullOrEmpty(strRemoteExt))
                {
                    MessageBox.Show("There are no file to view","MyAgencyVault",MessageBoxButton.OK,MessageBoxImage.Information);                    
                }
                else
                {
                    ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
                    ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
                    ObjDownload.Download(RemotePath, localPath);
                }

                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch
            {
            }
        }

        void ObjDownload_ErrorOccured(Exception error)
        {
            MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
        {
            if (statusCode.ToString().StartsWith("20"))
            {
                System.Diagnostics.Process.Start(localFilePath);
            }
            else
            {
                MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<Statement> _Statementlist;
        public ObservableCollection<Statement> Statementlist
        {
            get
            {
                return _Statementlist;
            }
            set
            {
                _Statementlist = value;
               
            }
        }

        private Statement _CurrentStatement;
        public Statement CurrentStatement
        {
            get { return _CurrentStatement; }
            set
            {
                _CurrentStatement = value;
                OnPropertyChanged("CurrentStatement");
            }
        }

        private ICommand _exportFile;
        public ICommand ExportFile
        {
            get
            {
                if (_exportFile == null)
                {
                    _exportFile = new BaseCommand(x => BeforeOnExportFile(), x => OnExportFile());

                }
                return _exportFile;
            }
        }

        private bool BeforeOnExportFile()
        {
            if (RoleManager.UserAccessPermission(MasterModule.CompManager) == ModuleAccessRight.Read)
                return false;

            if (SelectedBatch != null)
                return true;
            else
                return false;
        }

        private void OnExportFile()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files|*.CSV";
                saveDialog.Title = "Save Batch to Csv File";
                saveDialog.InitialDirectory = @"C:\";
                saveDialog.DefaultExt = "csv";
                saveDialog.RestoreDirectory = true;

                if (saveDialog.ShowDialog() == true)
                {
                    WriteCSV(saveDialog.FileName.ToString());
                }
            }
            catch
            {
            }

        }

        public void WriteCSV(string strPath)
        {
            string location = strPath;
            StringBuilder strBuilder = new StringBuilder();
            if (System.IO.File.Exists(location))
            {
                System.IO.File.Delete(location);                
            }
            CreateCSVFile(location);            

            MessageBox.Show("Export file in CSV format completed..", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        public StringBuilder ExportCSV()
        {
            DataTable dt = new DataTable();
            dt = getTable();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName);
                sb.Append(i == dt.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append(row[i].ToString());
                    sb.Append(i == dt.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return sb;
        }

        public void CreateCSVFile(string strFilePath)
        {
            try
            {
                DataTable dtDataTablesList = new DataTable();
                dtDataTablesList = getTable();

                var lines = new List<string>();

                string[] columnNames = dtDataTablesList.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName).
                                                  ToArray();

                var header = string.Join(",", columnNames);
                lines.Add(header);

                var valueLines = dtDataTablesList.AsEnumerable()
                                   .Select(row => string.Join(",", row.ItemArray));
                lines.AddRange(valueLines);

                File.WriteAllLines(strFilePath, lines);
            }
            catch
            {
            }
        }

        private ObservableCollection<ExposedDEU> _CurrentDeuEntry;
        public ObservableCollection<ExposedDEU> CurrentDeuEntry
        {
            get { return _CurrentDeuEntry; }
            set
            {
                _CurrentDeuEntry = value;             
                OnPropertyChanged("CurrentDeuEntry");
            }
        }

        public DataTable getTable()
        {

            DataTable dt = new DataTable();
            DateTime? dtInvoice = null;
            try
            {
                dt.Columns.Add(new DataColumn("Batch", typeof(string)));
                dt.Columns.Add(new DataColumn("Stmnt#", typeof(string)));
                dt.Columns.Add(new DataColumn("Payor", typeof(string)));
                dt.Columns.Add(new DataColumn("Carrier", typeof(string)));
                dt.Columns.Add(new DataColumn("Product", typeof(string)));
                dt.Columns.Add(new DataColumn("Policy number", typeof(string)));
                dt.Columns.Add(new DataColumn("Client", typeof(string)));
                dt.Columns.Add(new DataColumn("Premium", typeof(string)));
                dt.Columns.Add(new DataColumn("Comm  %", typeof(string)));
                dt.Columns.Add(new DataColumn("Date", typeof(string)));
                dt.Columns.Add(new DataColumn("Units", typeof(string)));
                dt.Columns.Add(new DataColumn("Fee", typeof(string)));
                dt.Columns.Add(new DataColumn("Share%", typeof(string)));
                dt.Columns.Add(new DataColumn("Total payment", typeof(string)));
                dt.Columns.Add(new DataColumn("Entered date/time", typeof(string)));

                for (int j = 0; j < BatchStatementLst.Count; j++)
                {
                    CurrentDeuEntry = serviceClients.DeuClient.GetDeuEntriesForStatement(BatchStatementLst[j].StatmentId);
                    DataRow dr;

                    if (CurrentDeuEntry.Count > 0)
                    {

                        for (int i = 0; i < CurrentDeuEntry.Count; i++)
                        {
                            dr = dt.NewRow();
                            dtInvoice = null;

                            dr["Batch"] = Convert.ToString(SelectedBatch.BatchNumber);
                            dr["Stmnt#"] = Convert.ToString(BatchStatementLst[j].StatmentNumber);
                            dr["Payor"] = Convert.ToString(BatchStatementLst[j].PayorNickName);
                            dr["Carrier"] = Convert.ToString(CurrentDeuEntry[i].CarrierName);
                            dr["Product"] = Convert.ToString(CurrentDeuEntry[i].ProductName);

                            if (!string.IsNullOrEmpty(Convert.ToString(CurrentDeuEntry[i].PolicyNumber)))
                            {
                                if (CurrentDeuEntry[i].PolicyNumber.Contains(","))
                                {
                                    dr["Policy number"] = Convert.ToString(CurrentDeuEntry[i].PolicyNumber).Replace(",", "");
                                }
                                else
                                {
                                    dr["Policy number"] = Convert.ToString(CurrentDeuEntry[i].PolicyNumber);
                                }
                            }
                            else
                            {
                                dr["Policy number"] = Convert.ToString(CurrentDeuEntry[i].PolicyNumber);
                            }

                            if (!string.IsNullOrEmpty(Convert.ToString(CurrentDeuEntry[i].ClientName)))
                            {
                                if (CurrentDeuEntry[i].ClientName.Contains(","))
                                {
                                    dr["Client"] = Convert.ToString(CurrentDeuEntry[i].ClientName).Replace(",", "");
                                }
                                else
                                {
                                    dr["Client"] = Convert.ToString(CurrentDeuEntry[i].ClientName);
                                }
                            }
                            else
                            {
                                dr["Client"] = Convert.ToString(CurrentDeuEntry[i].ClientName);
                            }

                            dr["Premium"] = Convert.ToString(CurrentDeuEntry[i].PaymentRecived);
                            dr["Comm  %"] = Convert.ToString(CurrentDeuEntry[i].CommissionPercentage);

                            if (!string.IsNullOrEmpty(Convert.ToString(CurrentDeuEntry[i].InvoiceDate)))
                            {
                                dtInvoice = Convert.ToDateTime(Convert.ToString(CurrentDeuEntry[i].InvoiceDate));
                                dr["Date"] = Convert.ToDateTime(dtInvoice).ToShortDateString();

                            }
                            else
                                dr["Date"] = string.Empty;

                            dr["Units"] = Convert.ToString(CurrentDeuEntry[i].Units);
                            dr["Fee"] = Convert.ToString(CurrentDeuEntry[i].Fee);
                            dr["Share%"] = Convert.ToString(CurrentDeuEntry[i].SplitPercentage);
                            dr["Total payment"] = Convert.ToString(CurrentDeuEntry[i].CommissionTotal);
                            dr["Entered date/time"] = Convert.ToString(CurrentDeuEntry[i].CreatedDate);

                            dt.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        if (BatchStatementLst[j].PayorNickName != "Total:")
                        {
                            dr = dt.NewRow();

                            dr["Batch"] = Convert.ToString(SelectedBatch.BatchNumber);
                            dr["Stmnt#"] = Convert.ToString(BatchStatementLst[j].StatmentNumber);
                            dr["Payor"] = Convert.ToString(BatchStatementLst[j].PayorNickName);

                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return dt;
        }

        private string status(string strValue)
        {
            string strStatus = string.Empty;
            switch (strValue)
            {
                case "0":
                    strStatus = "Not Started";
                    break;

                case "1":
                    strStatus = "In Progress";
                    break;

                case "2":
                    strStatus = "Close";
                    break;

                default:
                    break;
            }

            return strStatus;
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

        private void UploadBatchFile()
        {
            try
            {

                #region"New to select filter type"
                OpenFileDialog objOpenFile = new OpenFileDialog();
                objOpenFile.DefaultExt = ".pdf";
                objOpenFile.Filter = "Pdf documents (.pdf)|*.pdf| All files |*.*";

                if (objOpenFile.ShowDialog() == true)
                {
                    IsBusy = true;
                    fileName = objOpenFile.FileName;
                    //Added By Jyotisna
                    //On Aug 06, 2018 to log upload process from comp manager, after an issue occurred regarding wrong file association with existing batch
                    if (objLog != null)
                    {
                        objLog.AddLog(DateTime.Now.ToString() + ": New Batch upload request from Comp Manager with file: " + fileName);
                    }
                    UploadBatchFile(fileName);
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
                else
                {
                    IsBusy = false;
                }
                #endregion

            }
            catch (Exception ex)
            {
                if (objLog != null)
                {
                    objLog.AddLog(DateTime.Now.ToString() + ": Exception in upload batch from Comp Manager with file: " + fileName + ", exception: " + ex.Message);
                }
            }
        }


        private void UploadBatchFile(string strFilePath)
        {
            string dirPath = string.Empty;
            try
            {
                strFilePath = strFilePath.ToLower();
                string strFileExtension = System.IO.Path.GetExtension(strFilePath);
                string strFileType = strFileExtension.Replace(".", "");
                if (strFileExtension.Contains(".pdf"))
                {
                    Uploadbatch(strFilePath, strFileType);
                    //Sorting with batch number decending
                    BatchLst = new ObservableCollection<Batch>(BatchLst.OrderByDescending(date => date.CreatedDate.Value.Date).ThenByDescending(batch => batch.BatchNumber).ToList());
                }
                else //.xls,.xlsx,pdf,.text
                {
                    if (strFileExtension.Contains(".xls") || strFileExtension.Contains(".xlsx") || strFileExtension.Contains(".csv") || strFileExtension.Contains(".txt"))
                    {
                        Uploadbatch(strFilePath, strFileType);
                        IsBusy = false;
                        MessageBox.Show("'" + System.IO.Path.GetFileName(strFilePath) + "'" + " successfully uploaded at server to process", "File Name: " + System.IO.Path.GetFileName(strFilePath), MessageBoxButton.OK);
                    }
                    else
                    {
                        IsBusy = false;
                        MessageBox.Show("File format is not supported");
                    }
                }

            }
            catch
            {
                IsBusy = false;
            }
        }

        private void Uploadbatch(string strFilePath, string strType)
        {
            try
            {
                Batch NewBatch = new Batch();
                NewBatch.BatchId = Guid.NewGuid();
                //NewBatch.UploadStatus = UploadStatus.Manual;
                NewBatch.CreatedDate = DateTime.Now.Date;
                
                //Add status
                if (strType.ToLower().Equals("pdf"))
                {
                    NewBatch.IsManuallyUploaded = true;
                    NewBatch.EntryStatus = EntryStatus.Unassigned;
                    NewBatch.UploadStatus = UploadStatus.Manual;
                }
                else
                {
                    NewBatch.IsManuallyUploaded = false;
                    NewBatch.EntryStatus = EntryStatus.Importedfiletype;
                    NewBatch.UploadStatus = UploadStatus.ImportXls;
                }
                NewBatch.FileType = strType;
                NewBatch.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                NewBatch.LicenseeName = SharedVMData.SelectedLicensee.Company;

                //add batch to database
                int batchNumber = serviceClients.BatchClient.AddUpdateIBatch(NewBatch);
                NewBatch.BatchNumber = batchNumber;
                NewBatch.FileName = SharedVMData.SelectedLicensee.Company + "_" + batchNumber.ToString() + Path.GetExtension(strFilePath);

                if (strType.ToLower().Equals("pdf"))
                    BatchLst.Add(NewBatch);
                string strName = Path.GetFileName(strFilePath);
                string strTempPath = System.IO.Path.GetTempPath();
                strTempPath = strTempPath + strName;
                string OutputFile = strTempPath;

                //FileUtility ObjUpload = FileUtility.CreateClientlocal(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);
                FileUtility ObjUpload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);
                //Create stamp on PdF file.
                if (strType.ToLower().Equals("pdf"))
                {
                    if (System.IO.File.Exists(OutputFile))
                    {
                        System.IO.File.Delete(OutputFile);
                    }
                    var pathforcopy = Path.Combine(Path.GetTempPath(), strName);
                    //[Ankit-08-06-2018: Removing  stamps from Pdf file]
                    // OutputFile = StampOnPDF(strFilePath, strName, batchNumber.ToString(), NewBatch.LicenseeName.ToString());
                    File.Copy(strFilePath, pathforcopy, true);
                    ObjUpload.UploadComplete += new UploadCompleteDel(UploadCompleted);
                    ObjUpload.Upload(OutputFile, @"/UploadBatch/" + NewBatch.FileName, NewBatch);
                }
                else
                {
                    string strNewTempPath = System.IO.Path.GetTempPath();
                    string strFileName = System.IO.Path.GetFileName(strFilePath);
                    // [changes for Removing Filenameso for matching parameter with import tool service we can change this]
                    var NewFileName = NewBatch.LicenseeName + "_" + NewBatch.BatchNumber + "_" + NewBatch.LicenseeId.ToString() + System.IO.Path.GetExtension(strFileName);
                    strNewTempPath = strNewTempPath + NewFileName;
                    if (System.IO.File.Exists(strNewTempPath))
                    {
                        System.IO.File.Delete(strNewTempPath);
                    }
                    var pathforcopy = Path.Combine(Path.GetTempPath(), NewFileName);
                    File.Copy(strFilePath,pathforcopy, true);
                   
                    ObjUpload.Upload(strNewTempPath, @"/Uploadbatch/Import/Processing/" + NewFileName);
                    //ObjUpload.Upload(strNewTempPath, @"/Uploadbatch/Import/Processing/" + NewBatch.LicenseeName + "_" + NewBatch.LicenseeId.ToString() + "_" + NewBatch.BatchNumber + "_" + strFileName, NewBatch);
                    // [Change by Ankit on 08-06-2018 for checking length of file not more than 200 for upload file ]

                    serviceClients.BatchClient.UpdateBatchFileName(batchNumber, NewFileName);
                }

            }
            catch(Exception ex)
            {
                if (objLog != null)
                {
                    objLog.AddLog("Error in upload batch: strFilePath -" + strFilePath + ", type: " + strType + ", error: " + ex.Message);
                }
            }
        }

        public string StampOnPDF(string strFileNameFullPath, string strfile, string strbatchNumber, string strAgencyName)
        {
            string InputFile = strFileNameFullPath;
            string strTempPath = System.IO.Path.GetTempPath();
            strTempPath = strTempPath + strfile;
            string OutputFile = strTempPath;
            PdfImportedPage page = null;

            iTextSharp.text.Color watermarkFontColor;
            watermarkFontColor = iTextSharp.text.Color.RED;

            PdfReader reader = new iTextSharp.text.pdf.PdfReader(InputFile);
            iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfCopy writer = new iTextSharp.text.pdf.PdfCopy(pdfDoc, new FileStream(OutputFile, FileMode.OpenOrCreate, FileAccess.Write));
            int pageCount = reader.NumberOfPages;
            int currentPage = 0;
            pdfDoc.Open();

            while (currentPage < pageCount)
            {
                currentPage += 1;
                pdfDoc.SetPageSize(reader.GetPageSizeWithRotation(currentPage));
                pdfDoc.NewPage();
                page = writer.GetImportedPage(reader, currentPage);
                var cb1 = writer.CreatePageStamp(page);
                var cb = cb1.GetOverContent();
                cb.BeginText();
                cb.SetColorFill(watermarkFontColor);
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, Encoding.ASCII.EncodingName, false);

                cb.SetFontAndSize(baseFont, 8);
                cb.SetTextMatrix(3, 10);
                cb.ShowText("                " + "Batch#" + strbatchNumber + "         ST# ______________" + "   " + strAgencyName + "        " + System.DateTime.Now + "    " + "DEU:_____________ Review: ______________ " + "Page " + currentPage.ToString() + " of " + pageCount.ToString() + "");
                cb.EndText();
                cb1.AlterContents();
               
                writer.AddPage(page);
            }

            pdfDoc.Close();

            return strTempPath;


        }

        private void AddWaterMark(String sourceFile, String outputFile, string strbatchNumber, string strAgencyName)
        {
            try
            {
                PdfReader reader = new PdfReader(sourceFile);
                int pageCount = reader.NumberOfPages;
                PdfStamper stamper = new PdfStamper(reader, new System.IO.FileStream(outputFile, FileMode.Create));
                PdfContentByte underContent = null;

                iTextSharp.text.Rectangle rect = reader.GetPageSizeWithRotation(1);

                Single watermarkFontSize = 7;
                Single watermarkFontOpacity = 0.1f;
                Single watermarkRotation = 0f;
                iTextSharp.text.pdf.BaseFont watermarkFont;
                iTextSharp.text.Color watermarkFontColor;

                watermarkFont = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.HELVETICA,
                                                             iTextSharp.text.pdf.BaseFont.CP1252,
                                                            iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
                watermarkFontColor = iTextSharp.text.Color.BLACK;
                iTextSharp.text.pdf.PdfGState gstate = new iTextSharp.text.pdf.PdfGState();
                
                gstate.StrokeOpacity = watermarkFontOpacity;
                string watermarkText = string.Empty;
                for (int intCount = 1; intCount <= pageCount; intCount++)
                {
                    watermarkText = "Batch# " + strbatchNumber + "     ST#  __________" + "  " + strAgencyName + "                           " + System.DateTime.Now + "    " + "DEU: ____________________ Review: ____________________ " + "Page " + intCount.ToString() + " of " + pageCount.ToString() + "";
                    underContent = stamper.GetUnderContent(intCount);
                    underContent.SaveState();
                    underContent.SetGState(gstate);
                    underContent.SetColorFill(watermarkFontColor);
                    underContent.BeginText();
                    underContent.SetFontAndSize(watermarkFont, watermarkFontSize);
                    underContent.SetTextMatrix(10, 10);
                    underContent.ShowTextAligned(iTextSharp.text.Element.ALIGN_CENTER, watermarkText, rect.Width / 2, 10, watermarkRotation);
                    underContent.EndText();
                    underContent.RestoreState();
                }

                stamper.Close();
                reader.Close();
            }
            catch
            {
            }
        }

        private void UploadCompleted(int statusCode, object arg)
        {
            try
            {
                IsBusy = false;
                Batch batch = arg as Batch;
                if (!statusCode.ToString().StartsWith("20"))
                {
                    IsBusy = false;
                    serviceClients.BatchClient.DeleteBatch(batch.BatchId, RoleManager.Role);
                    BatchLst.Remove(batch);
                    MessageBox.Show("There is a problem while Uploading the file.Please re-upload file.", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    serviceClients.LicenseeClient.SetLastUploadTime(batch.LicenseeId);
                    //Send mail for uploaded batch suessfully
                    //MyAgencyVault.VM.MyAgencyVaultSvc.MailData _MailData = new MyAgencyVault.VM.MyAgencyVaultSvc.MailData();
                    MailData _MailData = new MailData();
                    _MailData.AgencyName = batch.LicenseeName;

                    if (SharedVMData.SelectedLicensee != null)
                    {
                        if (SharedVMData.SelectedLicensee.Email != null)
                        {
                            _MailData.ToMail = Convert.ToString(SharedVMData.SelectedLicensee.Email);
                            _MailData.FromMail = "service@commisisonsdept.com";
                            //serviceClients.FollowupIssueClient.SendMailToUpload(_MailData, MailBody(_MailData));
                            //mailMessage.Subject = "Upload batch in " + mailData.AgencyName;
                            string strSubject = "Upload batch in " + batch.LicenseeName;
                            serviceClients.FollowupIssueClient.SendNotificationMailAsync(_MailData,strSubject, MailBody(_MailData));
                        }
                    }
                }
                //Sorting with batch number decending
                BatchLst = new ObservableCollection<Batch>(BatchLst.OrderByDescending(date => date.CreatedDate.Value.Date).ThenByDescending(batchs => batchs.BatchNumber).ToList());
                IsBusy = false;
            }

            catch
            {
                BatchLst = new ObservableCollection<Batch>(BatchLst.OrderByDescending(date => date.CreatedDate.Value.Date).ThenByDescending(batch => batch.BatchNumber).ToList());
                IsBusy = false;
            }

            IsBusy = false;
        }

        private string MailBody(MailData EmailContentdata)
        {
            string MailBody = string.Empty;
            try
            {
                MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                           "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'> Batch Uploaded sucessfully in " +
                            EmailContentdata.AgencyName +
                           "</td></tr></table>";
            }
            catch
            {
            }
            return MailBody;

        }

        private bool UploadImageToSever()
        {
            try
            {
                string dirPath = string.Empty;
                autoResetEvent = new AutoResetEvent(false);

                FileUtility ObjUpload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);
                ObjUpload.UploadComplete += (i, j) =>
                {
                    autoResetEvent.Set();
                };

                ObjUpload.Upload(RemotefileName, "/UploadBatch/" + SelectedBatch.BatchNumber + Path.GetExtension(RemotefileName));
                autoResetEvent.WaitOne();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Refresh()
        {
            try
            {
                SharedVMData.RefreshLicensees();
                SelectedLicenseeChanged();
                LinkPaymentActivePoliciesLstCollection = null;
            }
            catch (Exception)
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
    }
}

