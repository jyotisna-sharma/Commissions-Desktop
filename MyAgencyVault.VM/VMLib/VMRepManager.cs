using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel;
using System.Windows;
using MyAgencyVault.VM.VMLib;
using System.Threading;
using MyAgencyVault.VM.CommonItems;
using System.IO;
using MyAgencyVault.VM;


namespace MyAgencyVault.ViewModel
{
    public interface IViewDialog
    {
        void Show(string strFormat);
        void ShowCustomMsg(string strFormat);
        void Close();
    }

    public class VMRepManager : BaseViewModel, IDataRefresh
    {
        static MastersClient objLog = new MastersClient();
        //public bool Checktype;
        public string EmailAddress;
        ManagementReport managementReport;
        #region Constructor

        private Guid PreLICID = Guid.Empty;
        private Guid SharedCacheLICID = Guid.Empty;

        private ServiceClients _serviceClients;
        private ServiceClients serviceClients
        {
            get
            {
                if (_serviceClients == null)
                {
                    _serviceClients = new ServiceClients();
                }
                return _serviceClients;
            }
        }

        private int _selected;
        public int Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        bool isECRExcluded = true;

        public bool IsECRExcluded
        {
            get { return isECRExcluded; }
            set
            {
                isECRExcluded = value;
                OnPropertyChanged("IsECRExcluded");
            }
        }

        bool isExclExcluded = true;

        public bool IsExclExcluded
        {
            get { return isExclExcluded; }
            set
            {
                isExclExcluded = value;
                OnPropertyChanged("IsExclExcluded");
            }
        }

        public delegate void delegateOpenEmailWindow();
        public event delegateOpenEmailWindow OpenEmailWindow;

        public VMRepManager(IViewDialog viewDialog)
        {
            try
            {
                _ViewDialog = viewDialog;
                serviceClients.ReportClient.PrintReportCompleted += new EventHandler<PrintReportCompletedEventArgs>(ReportClient_PrintReportCompleted);
                PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(VMRepManager_PropertyChanged);
                Reports = serviceClients.ReportClient.GetReports();
                PayeeReports = new ObservableCollection<Report>(Reports.Where(s => s.GroupName == "Payee").ToList());
                AuditReports = new ObservableCollection<Report>(Reports.Where(s => s.GroupName == "Audit").ToList());
                MgmtReports = new ObservableCollection<Report>(Reports.Where(s => s.GroupName == "Management").ToList());
                CreateBatchTypeList();
                IsZero = true;

                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {
                    //TempBatches = serviceClients.BatchClient.GetBatchForReportManagerByLicenssID(SharedVMData.SelectedLicensee.LicenseeId);
                    //TempBatches = serviceClients.BatchClient.GetAllBatchForReportManagerForAllLicensee();
                    //TempBatches = serviceClients.BatchClient.GetBatchesForReportManager();
                    //Batches = serviceClients.BatchClient.GetBatchesForReportManager();
                    //Batches = new ObservableCollection<Batch>(TempBatches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList());

                    //if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    //{
                    //    TempAgents = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);
                    //    //TempAgencyAgents = new ObservableCollection<User>(TempAgents.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList());
                    //}
                }
                else
                {
                    DateTime dt = new DateTime();
                    TempBatches = serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, dt);
                    Batches = new ObservableCollection<Batch>(TempBatches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList());
                    //Batches = serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, dt);
                    //if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    //{
                    //    TempAgents = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);

                    //}
                }

                Batches.Where(s => s.EntryStatus == EntryStatus.PartialUnpaid).ToList().ForEach(s => s.FileName += "(Partial Unpaid)");

                if (SharedVMData.SelectedLicensee != null)
                {
                    //LicenseeBatches = new ObservableCollection<Batch>(Batches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId));// && ((int)s.EntryStatus >= (int)EntryStatus.BatchCompleted)).ToList());
                    //LicenseeBatches = new ObservableCollection<Batch>(TempBatches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId));

                    SelectedLicenseeChanged();
                }

            }
            catch
            {
            }
        }

        void VMRepManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedBatchType":

                    if (LicenseeBatches == null)
                        return;

                    switch (SelectedBatchType)
                    {
                        case "Paid":
                            DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.Paid).ToList());
                            DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i=>i.BatchNumber).ToList());
                            break;
                        case "Unpaid":
                            DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.BatchCompleted || s.EntryStatus == EntryStatus.PartialUnpaid));
                            DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                            break;
                        case "Partial unpaid":
                            DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.PartialUnpaid));
                            DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                            break;
                        case "All":
                            DisplayedBatches = LicenseeBatches;
                            DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                            break;
                        default:
                            int year = 0;
                            if (int.TryParse(SelectedBatchType, out year))
                            {
                                DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.CreatedDate != null && s.CreatedDate.Value.Year == year).ToList());
                                DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.BatchNumber).ToList());
                            }
                            break;

                    }
                    break;

                default:
                    break;
            }

        }

        #endregion

        #region Payee Selection Report

        private IViewDialog _ViewDialog;

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

        //public void SelectedLicenseeChanged()
        //{
        //    try
        //    {
        //        if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
        //            return;

        //        Agents = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).OrderBy(s => s.NickName).ToList());
        //        //Create new colloectin for management reports
        //        ManagementAgents = new ObservableCollection<User>(Agents); //new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).ToList());
        //        //Add  new blank fields
        //        var objUser = ManagementAgents.Where(s => s.NickName == "(Blank)").FirstOrDefault();
        //        if (objUser == null)
        //        {
        //            User objBlankuser = new User
        //            {
        //                FirstName = "(Blank)",
        //                NickName = "(Blank)"
        //            };
        //            ManagementAgents.Add(objBlankuser);
        //        }


        //        if (Batches != null)
        //        {
        //            LicenseeBatches = new ObservableCollection<Batch>(Batches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId));// && ((int)s.EntryStatus >= (int)EntryStatus.BatchCompleted)).ToList());
        //            //set false to previous selected batch
        //            foreach (var item in LicenseeBatches)
        //            {
        //                item.IsChecked = false;
        //            }
        //        }
        //        else
        //            LicenseeBatches = new ObservableCollection<Batch>();

        //        PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };

        //        Payors = serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo);

        //        //Create new colloection for management reports
        //        ManagementPayors = new ObservableCollection<DisplayedPayor>(Payors); //serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo);



        //        //Add blank feilds
        //        if (ManagementPayors != null)
        //        {
        //            var objPayor = ManagementPayors.Where(s => s.PayorName == "(Blank)").FirstOrDefault();
        //            if (objPayor == null)
        //            {
        //                DisplayedPayor objBlankPayor = new DisplayedPayor
        //                {
        //                    PayorName = "(Blank)",
        //                    NickName = "(Blank)"
        //                };
        //                ManagementPayors.Add(objBlankPayor);
        //            }
        //        }

        //        Carriers = serviceClients.CarrierClient.GetDispalyedCarriersWith(SharedVMData.SelectedLicensee.LicenseeId, false);
        //        //Create new colloection for management carrirr
        //        ManagementCarriers = new ObservableCollection<DisplayedCarrier>(Carriers);
        //        //Add blank carrier feilds into carrier colloection
        //        if (ManagementCarriers != null)
        //        {
        //            var objCarrier = ManagementCarriers.Where(s => s.CarrierName == "(Blank)").FirstOrDefault();
        //            if (objCarrier == null)
        //            {
        //                DisplayedCarrier objBlankCarrier = new DisplayedCarrier
        //                {
        //                    CarrierName = "(Blank)"
        //                };
        //                ManagementCarriers.Add(objBlankCarrier);
        //            }
        //        }

        //        Products = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);
        //        //Create new colloection for management product
        //        ManagementProducts = new ObservableCollection<DisplayedCoverage>(Products);
        //        //Add new blank product into management product collection
        //        var objProcuct = ManagementProducts.Where(s => s.Name == "(Blank)").FirstOrDefault();
        //        if (objProcuct == null)
        //        {
        //            DisplayedCoverage objBlankCoverage = new DisplayedCoverage
        //            {
        //                Name = "(Blank)"
        //            };
        //            ManagementProducts.Add(objBlankCoverage);
        //        }
        //        OnAgentSelection("All");
        //        OnPayorSelection("All");
        //        OnCarrierSelection("All");
        //        OnProductSelection("All");

        //        MgmtReports.Last().IsChecked = true;
        //        SelectedBatchType = BatchTypes.FirstOrDefault(s => s == "Unpaid");

        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        public void SelectedLicenseeChanged()
        {
            try
            {
                if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                    return;

                Agents = new ObservableCollection<User>();
                //clear batch
                LicenseeBatches = new ObservableCollection<Batch>();

                Batches = LicenseeBatches = serviceClients.BatchClient.GetBatchForReportManagerByLicenssID(SharedVMData.SelectedLicensee.LicenseeId);

                //set false to previous selected batch
                foreach (var item in LicenseeBatches)
                {
                    item.IsChecked = false;
                }


                TempAgents = new ObservableCollection<User>(SharedVMData.GlobalReportAgentList);
                TempAgencyAgents = new ObservableCollection<User>(TempAgents.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList());


                if (SharedVMData.isAgentLoded)
                {
                    TempAgencyAgents = new ObservableCollection<User>(TempAgents.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList());
                }
                else
                {
                    TempAgencyAgents = new ObservableCollection<User>(serviceClients.UserClient.GetUsersForReports(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).OrderBy(s => s.NickName).ToList());
                }


                Agents = new ObservableCollection<User>(TempAgencyAgents.Where(s => s.Role == UserRole.Agent).OrderBy(s => s.NickName).ToList());
                //Create new colloectin for management reports
                if (Agents != null)
                {
                    if (Agents.Count == 0)
                    {
                        Agents = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).ToList());
                    }
                }
                ManagementAgents = new ObservableCollection<User>(Agents); //new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).ToList());
                //Add  new blank fields


                var objUser = ManagementAgents.Where(s => s.NickName == "(Blank)").FirstOrDefault();
                if (objUser == null)
                {
                    User objBlankuser = new User
                    {
                        FirstName = "(Blank)",
                        NickName = "(Blank)"
                    };
                    ManagementAgents.Add(objBlankuser);
                }




                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };



                Payors = serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo);

                //Create new colloection for management reports
                ManagementPayors = new ObservableCollection<DisplayedPayor>(Payors); //serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo);

                //Add blank feilds
                if (ManagementPayors != null)
                {
                    var objPayor = ManagementPayors.Where(s => s.PayorName == "(Blank)").FirstOrDefault();
                    if (objPayor == null)
                    {
                        DisplayedPayor objBlankPayor = new DisplayedPayor
                        {
                            PayorName = "(Blank)",
                            NickName = "(Blank)"
                        };
                        ManagementPayors.Add(objBlankPayor);
                    }
                }

                Carriers = serviceClients.CarrierClient.GetDispalyedCarriersWith(SharedVMData.SelectedLicensee.LicenseeId, false);
                //Create new colloection for management carrirr
                ManagementCarriers = new ObservableCollection<DisplayedCarrier>(Carriers);
                //Add blank carrier feilds into carrier colloection
                if (ManagementCarriers != null)
                {
                    var objCarrier = ManagementCarriers.Where(s => s.CarrierName == "(Blank)").FirstOrDefault();
                    if (objCarrier == null)
                    {
                        DisplayedCarrier objBlankCarrier = new DisplayedCarrier
                        {
                            CarrierName = "(Blank)"
                        };
                        ManagementCarriers.Add(objBlankCarrier);
                    }
                }

                Products = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);
                //Create new colloection for management product
                ManagementProducts = new ObservableCollection<DisplayedCoverage>(Products);
                //Add new blank product into management product collection
                var objProcuct = ManagementProducts.Where(s => s.Name == "(Blank)").FirstOrDefault();
                if (objProcuct == null)
                {
                    DisplayedCoverage objBlankCoverage = new DisplayedCoverage
                    {
                        Name = "(Blank)"
                    };
                    ManagementProducts.Add(objBlankCoverage);
                }


                OnAgentSelection("All");


                OnPayorSelection("All");
                OnCarrierSelection("All");
                OnProductSelection("All");


                MgmtReports.Last().IsChecked = true;
                SelectedBatchType = BatchTypes.FirstOrDefault(s => s == "Unpaid");


            }
            catch (Exception)
            {

            }
        }

        private Report _FocusedReport;
        public Report FocusedReport
        {
            get { return _FocusedReport; }
            set
            {
                _FocusedReport = value;
                OnPropertyChanged("FocusedReport");
            }
        }

        private ObservableCollection<Report> _Reports;
        public ObservableCollection<Report> Reports
        {
            get
            {
                return _Reports;
            }
            set
            {
                _Reports = value;
                OnPropertyChanged("Reports");
            }
        }

        private ObservableCollection<Report> _PayeeReports;
        public ObservableCollection<Report> PayeeReports
        {
            get
            {
                return _PayeeReports;
            }
            set
            {
                _PayeeReports = value;
                OnPropertyChanged("PayeeReports");
            }
        }

        private ObservableCollection<Report> _AuditReports;
        public ObservableCollection<Report> AuditReports
        {
            get
            {
                return _AuditReports;
            }
            set
            {
                _AuditReports = value;
                OnPropertyChanged("AuditReports");
            }
        }

        private ObservableCollection<Report> _MgmtReports;
        public ObservableCollection<Report> MgmtReports
        {
            get
            {
                return _MgmtReports;
            }
            set
            {
                _MgmtReports = value;
                OnPropertyChanged("MgmtReports");
            }
        }

        private Report _SelectedReport;
        public Report SelectedReport
        {
            get
            {
                return _SelectedReport;
            }
            set
            {

                _SelectedReport = value;
                OnPropertyChanged("SelectedReport");
            }
        }

        private string _SelectedBatchType;
        public string SelectedBatchType
        {
            get
            {
                return _SelectedBatchType;
            }
            set
            {

                _SelectedBatchType = value;
                OnPropertyChanged("SelectedBatchType");
            }
        }

        private ObservableCollection<string> _BatchTypes;
        public ObservableCollection<string> BatchTypes
        {
            get
            {
                return _BatchTypes;
            }
            set
            {
                _BatchTypes = value;
                OnPropertyChanged("BatchTypes");
            }
        }

        private ObservableCollection<Batch> _DisplayedBatches;
        public ObservableCollection<Batch> DisplayedBatches
        {
            get
            {
                return _DisplayedBatches;
            }
            set
            {
                _DisplayedBatches = value;
                OnPropertyChanged("DisplayedBatches");
            }
        }

        private ObservableCollection<Batch> _FilterBatches;
        public ObservableCollection<Batch> FilterBatches
        {
            get
            {
                return _FilterBatches;
            }
            set
            {
                _FilterBatches = value;
                OnPropertyChanged("FilterBatches");
            }
        }

        private ObservableCollection<Batch> _Batches;
        public ObservableCollection<Batch> Batches
        {
            get
            {
                return _Batches;
            }
            set
            {
                _Batches = value;
                OnPropertyChanged("Batches");
            }
        }

        private ObservableCollection<Batch> _TempBatches;
        public ObservableCollection<Batch> TempBatches
        {
            get
            {
                return _TempBatches;
            }
            set
            {
                _TempBatches = value;
                OnPropertyChanged("TempBatches");
            }
        }

        private ObservableCollection<Batch> _LicenseeBatches;
        public ObservableCollection<Batch> LicenseeBatches
        {
            get
            {
                return _LicenseeBatches;
            }
            set
            {
                _LicenseeBatches = value;
                OnPropertyChanged("LicenseeBatches");
            }
        }

        private ObservableCollection<User> _Agents;
        public ObservableCollection<User> Agents
        {
            get
            {
                return _Agents;
            }
            set
            {
                _Agents = value;
                OnPropertyChanged("Agents");
            }
        }

        private ObservableCollection<User> _TempAgents;
        public ObservableCollection<User> TempAgents
        {
            get
            {
                return _TempAgents;
            }
            set
            {
                _TempAgents = value;
                OnPropertyChanged("TempAgents");
            }
        }

        private ObservableCollection<User> _TempAgencyAgents;
        public ObservableCollection<User> TempAgencyAgents
        {
            get
            {
                return _TempAgencyAgents;
            }
            set
            {
                _TempAgencyAgents = value;
                OnPropertyChanged("TempAgencyAgents");
            }
        }

        private ObservableCollection<User> _ManagementAgents;
        public ObservableCollection<User> ManagementAgents
        {
            get
            {
                return _ManagementAgents;
            }
            set
            {
                _ManagementAgents = value;
                OnPropertyChanged("ManagementAgents");
            }
        }

        private bool _IsEnabledReportType;
        public bool IsEnabledReportType
        {
            get
            {
                return _IsEnabledReportType;
            }
            set
            {
                _IsEnabledReportType = value;
                OnPropertyChanged("IsEnabledReportType");
            }
        }



        private bool _IsEnabledAlloption;
        public bool IsEnabledAlloption
        {
            get
            {
                return _IsEnabledAlloption;
            }
            set
            {
                _IsEnabledAlloption = value;
                OnPropertyChanged("IsEnabledAlloption");
            }
        }

        private bool _IsZero;
        public bool IsZero
        {
            get
            {
                return _IsZero;
            }
            set
            {
                _IsZero = value;
                OnPropertyChanged("IsZero");
            }
        }
        private bool _IsSubTotal;
        public bool IsSubTotal
        {
            get
            {
                return _IsSubTotal;
            }
            set
            {
                _IsSubTotal = value;
                OnPropertyChanged("IsSubTotal");
            }
        }


        private void CreateBatchTypeList()
        {
            BatchTypes = new ObservableCollection<string>();
            BatchTypes.Add("Paid");
            BatchTypes.Add("Unpaid");
            BatchTypes.Add("Partial unpaid");
            BatchTypes.Add("All");

            for (int count = 0; count < 5; count++)
                BatchTypes.Add((DateTime.Now.Year - count).ToString());

            SelectedBatchType = BatchTypes.FirstOrDefault();
        }

        private ICommand _PayeeReportSelection;
        public ICommand PayeeReportSelection
        {
            get
            {
                if (_PayeeReportSelection == null)
                    _PayeeReportSelection = new BaseCommand(param => OnPayeeReportSelection(param));
                return _PayeeReportSelection;

            }
        }

        private void OnPayeeReportSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (Report report in PayeeReports)
                        report.IsChecked = true;
                    break;
                case "None":
                    foreach (Report report in PayeeReports)
                        report.IsChecked = false;
                    break;
            }
        }

        private ICommand _AuditReportSelection;
        public ICommand AuditReportSelection
        {
            get
            {
                if (_AuditReportSelection == null)
                    _AuditReportSelection = new BaseCommand(param => OnAuditReportSelection(param));
                return _AuditReportSelection;

            }
        }

        private void OnAuditReportSelection(object value)
        {
            string val = value as string;
            if (val.ToString() == "All")
            {
                IsEnabledReportType = true;
            }
            else
            {
                IsEnabledReportType = false;
            }

            switch (val)
            {
                case "All":
                    foreach (Report report in AuditReports)
                        report.IsChecked = true;

                    break;
                case "None":
                    foreach (Report report in AuditReports)
                        report.IsChecked = false;

                    break;
            }
        }

        private ICommand _MgmtReportSelection;
        public ICommand MgmtReportSelection
        {
            get
            {
                if (_MgmtReportSelection == null)
                    _MgmtReportSelection = new BaseCommand(param => OnMgmtReportSelection(param));
                return _MgmtReportSelection;

            }
        }

        private void OnMgmtReportSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (Report report in MgmtReports)
                        report.IsChecked = true;
                    break;
                case "None":
                    foreach (Report report in MgmtReports)
                        report.IsChecked = false;
                    break;
            }
        }

        private ICommand _EmailSelection;
        public ICommand EmailSelection
        {
            get
            {
                if (_EmailSelection == null)
                    _EmailSelection = new BaseCommand(param => BeforeOnEmailSelection(param), param => OnEmailSelection(param));
                return _EmailSelection;

            }
        }

        private bool BeforeOnEmailSelection(object param)
        {
            try
            {
                if (RoleManager.UserAccessPermission(MasterModule.ReportManager) == ModuleAccessRight.Read)
                    return false;

                ObservableCollection<Report> reports = null;
                bool enablePrint = false;

                if (SelectedTab == "Payee")
                {
                    int agentCount = 0;
                    int batchCount = 0;

                    if (Agents != null && DisplayedBatches != null)
                    {
                        agentCount = Agents.Where(s => s.IsChecked == true).Count();
                        batchCount = DisplayedBatches.Where(s => s.IsChecked == true).Count();
                    }

                    if (batchCount != 0 && agentCount != 0)
                        enablePrint = true;

                    reports = PayeeReports;
                }
                else if (SelectedTab == "Audit")
                {
                    int payorCount = 0;
                    int agentCount = 0;

                    if (Payors != null && Agents != null)
                    {
                        payorCount = Payors.Where(s => s.IsChecked == true).Count();
                        agentCount = Agents.Where(s => s.IsChecked == true).Count();
                    }

                    if (payorCount != 0 && agentCount != 0)
                        enablePrint = true;

                    reports = AuditReports;
                }
                else
                {
                    int payorCount = 0;
                    int carrierCount = 0;
                    int productCount = 0;
                    int agentCount = 0;

                    if (ManagementPayors != null && ManagementCarriers != null && ManagementProducts != null && ManagementAgents != null)
                    {
                        payorCount = ManagementPayors.Where(s => s.IsChecked == true).Count();
                        carrierCount = ManagementCarriers.Where(s => s.IsChecked == true).Count();
                        productCount = ManagementProducts.Where(s => s.IsChecked == true).Count();
                        agentCount = ManagementAgents.Where(s => s.IsChecked == true).Count();
                    }

                    if (payorCount != 0 && carrierCount != 0 && productCount != 0 && agentCount != 0)
                        enablePrint = true;

                    reports = MgmtReports;
                }

                if (!enablePrint)
                    return false;

                List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();

                if (rpts.Count != 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }

        private void OnEmailSelection(object param)
        {
            bool Result = false;
            ObservableCollection<Report> reports = null;
            try
            {
                if (SelectedTab == "Payee")
                    reports = PayeeReports;
                else if (SelectedTab == "Audit")
                    reports = AuditReports;
                else
                    reports = MgmtReports;

                if (SelectedTab == "Payee")
                {
                    PayeeStatementReport payeeReport = new PayeeStatementReport();
                    payeeReport.ReportId = Guid.NewGuid();
                    List<User> agents = Agents.Where(s => s.IsChecked == true).ToList();
                    agents.ForEach(s => payeeReport.AgentIds += s.UserCredentialID + ",");
                    string[] ReportAgents = payeeReport.AgentIds.Split(',');
                    if ((ReportAgents.Count() - 1) == Agents.Count)
                    {
                        payeeReport.AgentIds += "All";
                    }

                    List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                    rpts.ForEach(s => payeeReport.ReportNames += s.Code + ",");
                    string[] ReportName = payeeReport.ReportNames.Split(',');

                    if ((ReportName.Count() - 1) == reports.Count)
                    {
                        payeeReport.ReportNames += "All";
                    }

                    List<Batch> bathces = Batches.Where(s => s.IsChecked == true).ToList();
                    bathces.ForEach(s => payeeReport.BatcheIds += s.BatchId + ",");

                    string[] BatchIds = payeeReport.BatcheIds.Split(',');

                    if ((BatchIds.Count() - 1) == Batches.Count)
                    {
                        payeeReport.BatcheIds += "All";
                    }
                    payeeReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

                    payeeReport.PaymentType = PaidFilterValue;

                    payeeReport.IsZero = IsZero;

                    serviceClients.ReportClient.SavePayeeStatementReport(payeeReport);

                    Result = serviceClients.ReportClient.PrintReportAndSendMail(payeeReport.ReportId, payeeReport.ReportNames, RoleManager.userCredentialID,"");
                }
                else if (SelectedTab == "Audit")
                {
                    AuditReport auditReport = new AuditReport();
                    auditReport.ReportId = Guid.NewGuid();
                    List<User> agents = Agents.Where(s => s.IsChecked == true).ToList();
                    agents.ForEach(s => auditReport.AgentIds += s.UserCredentialID + ",");
                    string[] AgentIds = auditReport.AgentIds.Split(',');
                    if ((AgentIds.Count() - 1) == Agents.Count)
                    {
                        auditReport.AgentIds += "All";
                    }
                    List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                    rpts.ForEach(s => auditReport.ReportNames += s.Code + ",");
                    string[] ReportNames = auditReport.ReportNames.Split(',');
                    if ((ReportNames.Count() - 1) == reports.Count)
                    {
                        auditReport.ReportNames += "All";
                    }
                    List<DisplayedPayor> payors = Payors.Where(s => s.IsChecked == true).ToList();
                    payors.ForEach(s => auditReport.PayorIds += s.PayorID + ",");
                    string[] payorIds = auditReport.PayorIds.Split(',');
                    if ((payorIds.Count() - 1) == Payors.Count)
                    {
                        auditReport.PayorIds += "All";
                    }
                    auditReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

                    auditReport.OrderBy = OrderByParameter;
                    auditReport.FilterBy = FilterByParameter;

                    DateTime? CalcFromInvoiceDate = CalculateFromDate(FromInvoiceDate, ToInvoiceDate);
                    DateTime? CalToInvoiceDate = CalculateToDate(FromInvoiceDate, ToInvoiceDate);

                    auditReport.FromInvoiceDate = CalcFromInvoiceDate;
                    auditReport.ToInvoiceDate = CalToInvoiceDate;

                    serviceClients.ReportClient.SaveAuditReport(auditReport);

                    Result = serviceClients.ReportClient.PrintReportAndSendMail(auditReport.ReportId, auditReport.ReportNames, RoleManager.userCredentialID,"");
                }
                else
                {
                    //if(OpenEmailWindow != null)
                    //{
                    //    OpenEmailWindow();
                    //}
                    managementReport = new ManagementReport();
                    managementReport.ReportId = Guid.NewGuid();

                    List<User> agents = ManagementAgents.Where(s => s.IsChecked == true).ToList();
                    agents.ForEach(s => managementReport.AgentIds += s.UserCredentialID + ",");
                    string[] AgentIds = managementReport.AgentIds.Split(',');
                    if ((AgentIds.Count() - 1) == ManagementAgents.Count)
                    {
                        managementReport.AgentIds += "All";
                    }

                    List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                    rpts.ForEach(s => managementReport.ReportNames += s.Code + ",");
                    string[] ReportNames = managementReport.ReportNames.Split(',');
                    if ((ReportNames.Count() - 1) == reports.Count)
                    {
                        managementReport.ReportNames += "All";
                    }

                    List<DisplayedPayor> payors = ManagementPayors.Where(s => s.IsChecked == true).ToList();
                    payors.ForEach(s => managementReport.PayorIds += s.PayorID + ",");
                    string[] PayorIds = managementReport.PayorIds.Split(',');
                    if ((PayorIds.Count() - 1) == Payors.Count)
                    {
                        managementReport.PayorIds += "All";
                    }

                    List<DisplayedCarrier> carriers = ManagementCarriers.Where(s => s.IsChecked == true).ToList();
                    carriers.ForEach(s => managementReport.CarrierIds += s.CarrierId + ",");
                    string[] CarrierIds = managementReport.CarrierIds.Split(',');
                    if ((CarrierIds.Count() - 1) == Carriers.Count)
                    {
                        managementReport.CarrierIds += "All";
                    }



                    List<DisplayedCoverage> products = ManagementProducts.Where(s => s.IsChecked == true).ToList();
                    products.ForEach(s => managementReport.ProductIds += s.CoverageID + ",");
                    string[] ProductIds = managementReport.ProductIds.Split(',');
                    if ((ProductIds.Count() - 1) == Products.Count)
                    {
                        managementReport.ProductIds += "All";
                    }
                    managementReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

                    managementReport.FromEffectiveDate = FromEffectiveDate;
                    managementReport.ToEffectiveDate = ToEffectiveDate;
                    managementReport.FromTrackDate = FromTrackDate;
                    managementReport.ToTrackDate = ToTrackDate;
                    managementReport.FromTermDate = FromTermDate;
                    managementReport.ToTermDate = ToTermDate;

                    managementReport.BeginPremium = PremiumStart;
                    managementReport.EndPremium = PremiumEnd;
                    managementReport.BeginEnrolled = EnrolledStart;
                    managementReport.EndEnrolled = EnrolledEnd;
                    managementReport.BeginEligible = EligibleStart;
                    managementReport.EndEligible = EligibleEnd;

                    managementReport.PolicyType = PolicyType;
                    managementReport.PolicyMode = PolicyMode;
                    managementReport.TrackPayment = TrackPaymentParameter;
                    managementReport.OrderBy = OrderByParamMgmtReport;
                    managementReport.PolicyTermReason = PolicyTermReason;
                    managementReport.EffectiveMonth = GetMonth(effectiveMonth);

                    managementReport.InvoiceFrom = ECRInvoiceFrom;
                    managementReport.InvoiceTo = ECRInvoiceTo;

                    serviceClients.ReportClient.SaveManagementReport(managementReport);

                    Result = serviceClients.ReportClient.PrintReportAndSendMail(managementReport.ReportId, managementReport.ReportNames, RoleManager.userCredentialID,"");
                }

                if (Result)
                {
                    MessageBox.Show("Mail is sent successfully.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Mail sending operation is failed.Please try again.", "Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch
            {
            }
        }


        public void Workerwork()
        {
            //if (Checktype == true)
            if(!string.IsNullOrEmpty(EmailAddress))
            {
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { }));
                serviceClients.ReportClient.SaveManagementReport(managementReport);
                objLog.AddLog("Management report success with ID: " + managementReport.ReportId);
                MessageBox.Show("Report creation is in progress and will be sent to your mentioned email address. You can continue using the application.", "Report In Progress!", MessageBoxButton.OK, MessageBoxImage.Information);

                serviceClients.ReportClient.PrintReportAndSendMailAsync(managementReport.ReportId, managementReport.ReportNames, RoleManager.userCredentialID, EmailAddress);
            }

            else
            {
                MessageBox.Show("Mail  sending failed.", "failed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private int? GetMonth(string effMonth)
        {
            int? intvalue = null;
            try
            {
                switch (effMonth)
                {
                    case "Any":
                        intvalue = 0;
                        break;
                    case "Jan":
                        intvalue = 1;
                        break;
                    case "Feb":
                        intvalue = 2;
                        break;
                    case "March":
                        intvalue = 3;
                        break;
                    case "April":
                        intvalue = 4;
                        break;
                    case "May":
                        intvalue = 5;
                        break;
                    case "Jun":
                        intvalue = 6;
                        break;
                    case "July":
                        intvalue = 7;
                        break;
                    case "Aug":
                        intvalue = 8;
                        break;
                    case "Sep":
                        intvalue = 9;
                        break;
                    case "Oct":
                        intvalue = 10;
                        break;
                    case "Nov":
                        intvalue = 11;
                        break;
                    case "Dec":
                        intvalue = 12;
                        break;
                    case "Blank":
                        intvalue = null;
                        break;
                }

            }
            catch
            {
            }
            return intvalue;
        }

        private ICommand _BatchSelection;
        public ICommand BatchSelection
        {
            get
            {
                if (_BatchSelection == null)
                    _BatchSelection = new BaseCommand(param => OnBatchSelection(param));
                return _BatchSelection;

            }
        }

        private void OnBatchSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (Batch batch in DisplayedBatches)
                        batch.IsChecked = true;
                    break;
                case "None":
                    foreach (Batch batch in DisplayedBatches)
                        batch.IsChecked = false;
                    break;
            }
        }

        private ICommand _AgentSelection;
        public ICommand AgentSelection
        {
            get
            {
                if (_AgentSelection == null)
                    _AgentSelection = new BaseCommand(param => OnAgentSelection(param));
                return _AgentSelection;

            }
        }

        private void OnAgentSelection(object value)
        {
            string val = value as string;
            if (Agents == null)
                return;

            switch (val)
            {
                case "All":
                    foreach (User agent in Agents)
                        agent.IsChecked = true;

                    foreach (User agent in ManagementAgents)
                        agent.IsChecked = true;

                    break;
                case "None":
                    foreach (User agent in Agents)
                        agent.IsChecked = false;

                    foreach (User agent in ManagementAgents)
                        agent.IsChecked = false;

                    break;
            }
        }

        private ICommand _PrintPayeeStatementReport;
        public ICommand PrintPayeeStatementReport
        {
            get
            {
                if (_PrintPayeeStatementReport == null)
                    _PrintPayeeStatementReport = new BaseCommand(param => BeforeOnPrintPayeeStatementReport(), param => OnPrintPayeeStatementReport());
                return _PrintPayeeStatementReport;

            }
        }

        private ICommand _PrintPayeeStatementReportOnExcel;
        public ICommand PrintPayeeStatementReportOnExcel
        {
            get
            {
                if (_PrintPayeeStatementReportOnExcel == null)
                    _PrintPayeeStatementReportOnExcel = new BaseCommand(param => BeforeOnPrintPayeeStatementReport(), param => OnExportReport());
                return _PrintPayeeStatementReportOnExcel;

            }
        }


        private ICommand _ExportReport;
        public ICommand ExportReport
        {
            get
            {
                if (_ExportReport == null)
                    _ExportReport = new BaseCommand(param => BeforeOnPrintPayeeStatementReport(), param => OnExportReport());
                return _ExportReport;

            }
        }

        private void OnPrintPayeeStatementReport()
        {
            PrintReport("PDF");
        }

        private void OnExportReport()
        {
            PrintReport("Excel");
        }

        private void PrintReport(string strFormat)
        {
            if (objLog == null) objLog = new MastersClient();
            ObservableCollection<Report> reports = null;

            try
            {

                if (SelectedTab == "Payee")
                    reports = PayeeReports;
                else if (SelectedTab == "Audit")
                    reports = AuditReports;
                else
                    reports = MgmtReports;

                if (SelectedTab == "Payee")
                {
                    try
                    {

                        PayeeStatementReport payeeReport = new PayeeStatementReport();
                        payeeReport.ReportId = Guid.NewGuid();
                        objLog.AddLog("Payee Statement report request with ID: " + payeeReport.ReportId);
                        List<User> agents = Agents.Where(s => s.IsChecked == true).ToList();
                        agents.ForEach(s => payeeReport.AgentIds += s.UserCredentialID + ",");
                        string[] ReportAgents = payeeReport.AgentIds.Split(',');
                        if ((ReportAgents.Count() - 1) == Agents.Count)
                        {
                            payeeReport.AgentIds += "All";
                        }

                        List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                        rpts.ForEach(s => payeeReport.ReportNames += s.Code + ",");
                        string[] ReportName = payeeReport.ReportNames.Split(',');

                        if ((ReportName.Count() - 1) == reports.Count)
                        {
                            payeeReport.ReportNames += "All";
                        }

                        //List<Batch> bathces = Batches.Where(s => s.IsChecked == true && s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList();

                        List<Batch> tempbathces = DisplayedBatches.Where(s => s.IsChecked == true && s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId).ToList();

                        //bathces.ForEach(s => payeeReport.BatcheIds += s.BatchId + ",");
                        tempbathces.ForEach(s => payeeReport.BatcheIds += s.BatchId + ",");

                        string[] BatchIds = payeeReport.BatcheIds.Split(',');

                        if ((BatchIds.Count() - 1) == Batches.Count)
                        {
                            payeeReport.BatcheIds += "All";
                        }
                        payeeReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

                        payeeReport.PaymentType = PaidFilterValue;

                        payeeReport.IsZero = IsZero;
                        //  IsEnabledReportType = true;

                        payeeReport.IsSubTotal = IsSubTotal;

                        serviceClients.ReportClient.SavePayeeStatementReport(payeeReport);

                        serviceClients.ReportClient.PrintReportAsync(payeeReport.ReportId, payeeReport.ReportNames, strFormat);
                        objLog.AddLog("Payee Statement report success with ID: " + payeeReport.ReportId);
                    }
                    catch (Exception ex)
                    {
                        objLog.AddLog("Payee Statement PrintReport exception: " + ex.Message);
                    }
                    _ViewDialog.Show(strFormat);

                }
                else if (SelectedTab == "Audit")
                {
                    AuditReport auditReport = new AuditReport();
                    auditReport.ReportId = Guid.NewGuid();
                    objLog.AddLog("Audit report request with ID: " + auditReport.ReportId);
                    List<User> agents = Agents.Where(s => s.IsChecked == true).ToList();
                    agents.ForEach(s => auditReport.AgentIds += s.UserCredentialID + ",");
                    string[] AgentIds = auditReport.AgentIds.Split(',');
                    if ((AgentIds.Count() - 1) == Agents.Count)
                    {
                        auditReport.AgentIds += "All";
                    }
                    List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                    //rpts.ForEach(s => auditReport.ReportNames += s.Code + ",");
                    //May 17, 2019 - Acme replaced above to include excel verion 
                    if (strFormat == "Excel")
                    {
                        //Acme - July 22, 2019 - Commneted after new report implementation 
                       // rpts.ForEach(s => auditReport.ReportNames += s.Code + "_EX" + "," );
                       foreach(Report s in rpts)
                        {
                            if (s.Code != "AUNP")
                            {
                                auditReport.ReportNames += s.Code + "_EX" + ",";
                            }
                            else
                            {
                                auditReport.ReportNames += s.Code + ",";
                            }
                        }
                    }
                    else
                    {
                        rpts.ForEach(s => auditReport.ReportNames += s.Code + ",");
                    }
                    string[] ReportNames = auditReport.ReportNames.Split(',');
                    if ((ReportNames.Count() - 1) == reports.Count)
                    {
                        auditReport.ReportNames += "All";
                    }
                    List<DisplayedPayor> payors = Payors.Where(s => s.IsChecked == true).ToList();
                    payors.ForEach(s => auditReport.PayorIds += s.PayorID + ",");
                    string[] payorIds = auditReport.PayorIds.Split(',');
                    if ((payorIds.Count() - 1) == Payors.Count)
                    {
                        auditReport.PayorIds += "All";
                    }
                    auditReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;

                    auditReport.OrderBy = OrderByParameter;
                    auditReport.FilterBy = FilterByParameter;

                    DateTime? CalcFromInvoiceDate = CalculateFromDate(FromInvoiceDate, ToInvoiceDate);
                    DateTime? CalToInvoiceDate = CalculateToDate(FromInvoiceDate, ToInvoiceDate);

                    auditReport.FromInvoiceDate = CalcFromInvoiceDate;
                    auditReport.ToInvoiceDate = CalToInvoiceDate;

                    serviceClients.ReportClient.SaveAuditReport(auditReport);
                    serviceClients.ReportClient.PrintReportAsync(auditReport.ReportId, auditReport.ReportNames, strFormat);
                    objLog.AddLog("Audit report success with ID: " + auditReport.ReportId);
                    _ViewDialog.Show(strFormat);

                }
                else
                {

                    //If ECR selected, make sure dates are entered correctly
                    if (reports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault() != null && reports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault().IsChecked)
                    {
                        if (ECRInvoiceFrom == null || ECRInvoiceTo == null)
                        {
                            MessageBox.Show("Please enter 'InvoiceFrom' and 'InvoiceTo' for Expected Commissions Report");
                            return;
                        }
                        else if (ECRInvoiceFrom != null && ECRInvoiceTo != null)
                        {
                            if (ECRInvoiceFrom > ECRInvoiceTo)
                            {
                                MessageBox.Show("Please check dates for Expected Commissions report, 'InvoiceFrom' cannot be greater than 'InvoiceTo'");
                                return;
                            }
                            else if (ECRInvoiceTo > ECRInvoiceFrom.Value.AddMonths(24))
                            {
                                MessageBox.Show("Interval between 'InvoiceFrom' and 'InvoiceTo' cannot be more than 24 months");
                                return;
                            }
                        }
                    }

                    managementReport = new ManagementReport();
                    managementReport.ReportId = Guid.NewGuid();
                    objLog.AddLog("Management report request with ID: " + managementReport.ReportId);

                    List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                    rpts.ForEach(s => managementReport.ReportNames += s.Code + ",");
                    string[] ReportNames = managementReport.ReportNames.Split(',');
                    if ((ReportNames.Count() - 1) == reports.Count)
                    {
                        managementReport.ReportNames += "All";
                    }

                    List<User> agents = ManagementAgents.Where(s => s.IsChecked == true).ToList();
                    agents.ForEach(s => managementReport.AgentIds += s.UserCredentialID + ",");
                    string[] AgentIds = managementReport.AgentIds.Split(',');

                    //All selected  then need to Query with Null value
                    if ((AgentIds.Count() - 1) == ManagementAgents.Count)
                    {
                        managementReport.AgentIds += "All";
                    }
                    else if (AgentIds.Length == 2)
                    {
                        Guid guidEmpty = new Guid(AgentIds[0]);

                        //Blank selected  then need to Query with Null value
                        if (string.IsNullOrEmpty(AgentIds[1]) && (guidEmpty == Guid.Empty))
                        {
                            managementReport.AgentIds += "OnlyBlank";
                        }
                        //Blank is not selected then need to Query with Ids
                        else
                        {
                            managementReport.AgentIds += "OnlyIDs";
                        }
                    }

                    else if (AgentIds.Length > 2)
                    {
                        bool isBlank = false;
                        foreach (var item in AgentIds)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Guid guidEmpty = new Guid(item);
                                if (guidEmpty == Guid.Empty)
                                {
                                    isBlank = true;
                                    break;
                                }
                            }
                        }

                        if (isBlank)
                        {
                            //If blank is available then need to query with null value and Ids
                            managementReport.AgentIds += "Both";
                        }
                        else
                        { //If blank is not available then need to query with onlyIds
                            managementReport.AgentIds += "OnlyIDs";
                        }
                    }

                    List<DisplayedPayor> payors = ManagementPayors.Where(s => s.IsChecked == true).ToList();
                    payors.ForEach(s => managementReport.PayorIds += s.PayorID + ",");
                    string[] PayorIds = managementReport.PayorIds.Split(',');

                    if ((PayorIds.Count() - 1) == ManagementPayors.Count)
                    {
                        managementReport.PayorIds += "All";
                    }
                    else if (PayorIds.Length == 2)
                    {
                        Guid guidEmpty = new Guid(PayorIds[0]);

                        //Blank selected  then need to Query with Null value
                        if (string.IsNullOrEmpty(PayorIds[1]) && (guidEmpty == Guid.Empty))
                        {
                            managementReport.PayorIds += "OnlyBlank";
                        }
                        //Blank is not selected then need to Query with Ids
                        else
                        {
                            managementReport.PayorIds += "OnlyIDs";
                        }
                    }

                    else if (PayorIds.Length > 2)
                    {
                        bool isBlank = false;
                        foreach (var item in PayorIds)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Guid guidEmpty = new Guid(item);
                                if (guidEmpty == Guid.Empty)
                                {
                                    isBlank = true;
                                    break;
                                }
                            }
                        }

                        if (isBlank)
                        {
                            //If blank is available then need to query with null value and Ids
                            managementReport.PayorIds += "Both";
                        }
                        else
                        { //If blank is not available then need to query with onlyIds
                            managementReport.PayorIds += "OnlyIDs";
                        }
                    }

                    List<DisplayedCarrier> carriers = ManagementCarriers.Where(s => s.IsChecked == true).ToList();
                    carriers.ForEach(s => managementReport.CarrierIds += s.CarrierId + ",");
                    string[] CarrierIds = managementReport.CarrierIds.Split(',');
                    if ((CarrierIds.Count() - 1) == ManagementCarriers.Count)
                    {
                        managementReport.CarrierIds += "All";
                    }

                    else if (CarrierIds.Length == 2)
                    {
                        Guid guidEmpty = new Guid(CarrierIds[0]);
                        //Blank selected  then need to Query with Null value
                        if (string.IsNullOrEmpty(CarrierIds[1]) && (guidEmpty == Guid.Empty))
                        {
                            managementReport.CarrierIds += "OnlyBlank";
                        }
                        //Blank is not selected then need to Query with Ids
                        else
                        {
                            managementReport.CarrierIds += "OnlyIDs";
                        }
                    }

                    else if (CarrierIds.Length > 2)
                    {
                        bool isBlank = false;
                        foreach (var item in CarrierIds)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Guid guidEmpty = new Guid(item);
                                if (guidEmpty == Guid.Empty)
                                {
                                    isBlank = true;
                                    break;
                                }
                            }
                        }

                        if (isBlank)
                        {
                            //If blank is available then need to query with null value and Ids
                            managementReport.CarrierIds += "Both";
                        }
                        else
                        { //If blank is not available then need to query with onlyIds
                            managementReport.CarrierIds += "OnlyIDs";
                        }
                    }

                    List<DisplayedCoverage> products = ManagementProducts.Where(s => s.IsChecked == true).ToList();
                    products.ForEach(s => managementReport.ProductIds += s.CoverageID + ",");
                    string[] ProductIds = managementReport.ProductIds.Split(',');
                    if ((ProductIds.Count() - 1) == ManagementProducts.Count)
                    {
                        managementReport.ProductIds += "All";
                    }

                    else if (ProductIds.Length == 2)
                    {
                        Guid guidEmpty = new Guid(ProductIds[0]);
                        //Blank selected  then need to Query with Null value
                        if (string.IsNullOrEmpty(ProductIds[1]) && (guidEmpty == Guid.Empty))
                        {
                            managementReport.ProductIds += "OnlyBlank";
                        }
                        //Blank is not selected then need to Query with Ids
                        else
                        {
                            managementReport.ProductIds += "OnlyIDs";
                        }
                    }

                    else if (CarrierIds.Length > 2)
                    {
                        bool isBlank = false;
                        foreach (var item in ProductIds)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Guid guidEmpty = new Guid(item);
                                if (guidEmpty == Guid.Empty)
                                {
                                    isBlank = true;
                                    break;
                                }
                            }
                        }

                        if (isBlank)
                        {
                            //If blank is available then need to query with null value and Ids
                            managementReport.ProductIds += "Both";
                        }
                        else
                        { //If blank is not available then need to query with onlyIds
                            managementReport.ProductIds += "OnlyIDs";
                        }
                    }

                    managementReport.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                    managementReport.FromEffectiveDate = FromEffectiveDate;
                    managementReport.ToEffectiveDate = ToEffectiveDate;
                    managementReport.FromTrackDate = FromTrackDate;
                    managementReport.ToTrackDate = ToTrackDate;
                    managementReport.FromTermDate = FromTermDate;
                    managementReport.ToTermDate = ToTermDate;

                    managementReport.BeginPremium = PremiumStart;
                    managementReport.EndPremium = PremiumEnd;
                    managementReport.BeginEnrolled = EnrolledStart;
                    managementReport.EndEnrolled = EnrolledEnd;
                    managementReport.BeginEligible = EligibleStart;
                    managementReport.EndEligible = EligibleEnd;

                    managementReport.PolicyType = PolicyType;
                    managementReport.PolicyMode = PolicyMode;
                    managementReport.TrackPayment = TrackPaymentParameter;
                    managementReport.OrderBy = OrderByParamMgmtReport;
                    managementReport.PolicyTermReason = PolicyTermReason;

                    managementReport.EffectiveMonth = GetMonth(effectiveMonth);
                    managementReport.InvoiceFrom = ECRInvoiceFrom;
                    managementReport.InvoiceTo = ECRInvoiceTo;

                    if (reports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault() != null && reports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault().IsChecked)
                    {
                        if (OpenEmailWindow != null)
                        {
                            OpenEmailWindow();
                        }
                    }
                    else
                    {
                        serviceClients.ReportClient.SaveManagementReport(managementReport);
                        serviceClients.ReportClient.PrintReportAsync(managementReport.ReportId, managementReport.ReportNames, strFormat);
                        objLog.AddLog("Management report success with ID: " + managementReport.ReportId);
                        _ViewDialog.Show(strFormat);
                    }
                    //serviceClients.ReportClient.SaveManagementReport(managementReport);
                    //serviceClients.ReportClient.PrintReportAsync(managementReport.ReportId, managementReport.ReportNames, strFormat);
                    //objLog.AddLog("Management report success with ID: " + managementReport.ReportId);

                }
               
            }
            catch (Exception ex)
            {
                objLog.AddLog("Print report exception: " + ex.Message + ", Trace: " + ex.StackTrace);

            }
        }

        private DateTime? CalculateFromDate(DateTime? FromInvoiceDate, DateTime? ToInvoiceDate)
        {
            if (FromInvoiceDate != null)
            {
                DateTime dtFromInvoiceDate = Convert.ToDateTime(FromInvoiceDate);
                FromInvoiceDate = FirstDayOfMonthFromDateTime(dtFromInvoiceDate);
            }

            if (FromInvoiceDate == null && ToInvoiceDate == null)
            {
                DateTime dtToInvoiceDate = System.DateTime.Now;
                //dtToInvoiceDate = dtToInvoiceDate.AddDays(-93);
                dtToInvoiceDate = dtToInvoiceDate.AddDays(-63);
                dtToInvoiceDate = dtToInvoiceDate.AddMonths(-1);
                dtToInvoiceDate = LastDayOfMonthFromDateTime(dtToInvoiceDate);
                DateTime dtFromInvoiceDate;
                dtFromInvoiceDate = dtToInvoiceDate.AddMonths(-12);
                FromInvoiceDate = dtFromInvoiceDate.AddDays(1);
            }

            if (FromInvoiceDate == null && ToInvoiceDate != null)
            {
                DateTime dtToInvoiceDate = Convert.ToDateTime(ToInvoiceDate);
                dtToInvoiceDate = LastDayOfMonthFromDateTime(dtToInvoiceDate);
                DateTime dtFromInvoiceDate;
                dtFromInvoiceDate = dtToInvoiceDate.AddDays(1);
                dtFromInvoiceDate = dtFromInvoiceDate.AddMonths(-12);
                FromInvoiceDate = FirstDayOfMonthFromDateTime(dtFromInvoiceDate);
            }


            return FromInvoiceDate;
        }

        private DateTime? CalculateToDate(DateTime? FromInvoiceDate, DateTime? ToInvoiceDate)
        {
            if (ToInvoiceDate != null)
            {
                DateTime dtToInvoiceDate = Convert.ToDateTime(ToInvoiceDate);
                ToInvoiceDate = LastDayOfMonthFromDateTime(dtToInvoiceDate);
            }

            if (FromInvoiceDate == null && ToInvoiceDate == null)
            {
                DateTime dtToInvoiceDate = System.DateTime.Now;
                //dtToInvoiceDate = dtToInvoiceDate.AddDays(-93);
                dtToInvoiceDate = dtToInvoiceDate.AddDays(-63);
                dtToInvoiceDate = dtToInvoiceDate.AddMonths(-1);
                ToInvoiceDate = LastDayOfMonthFromDateTime(dtToInvoiceDate);
            }

            if (FromInvoiceDate != null && ToInvoiceDate == null)
            {
                DateTime dtFromInvoiceDate = Convert.ToDateTime(FromInvoiceDate);
                dtFromInvoiceDate = FirstDayOfMonthFromDateTime(dtFromInvoiceDate);
                DateTime dtToInvoiceDate;
                dtToInvoiceDate = dtFromInvoiceDate.AddMonths(12);
                dtToInvoiceDate = dtToInvoiceDate.AddDays(-1);
                ToInvoiceDate = LastDayOfMonthFromDateTime(dtToInvoiceDate);
            }

            return ToInvoiceDate;
        }

        public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }

        private void RefreshBatch(string SelectedBatchType)
        {
            //Call 
            Batches = serviceClients.BatchClient.GetBatchesForReportManager();

            if (SharedVMData.SelectedLicensee != null)
            {
                LicenseeBatches = new ObservableCollection<Batch>(Batches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId));// && ((int)s.EntryStatus >= (int)EntryStatus.BatchCompleted)).ToList());
            }

            switch (SelectedBatchType)
            {
                case "Paid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.Paid).ToList());
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "Unpaid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.BatchCompleted || s.EntryStatus == EntryStatus.PartialUnpaid));
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "Partial unpaid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.PartialUnpaid));
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "All":
                    DisplayedBatches = LicenseeBatches;
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                default:
                    int year = 0;
                    if (int.TryParse(SelectedBatchType, out year))
                    {
                        DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.CreatedDate != null && s.CreatedDate.Value.Year == year).ToList());
                        DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    }
                    break;

            }
        }
        private void RefreshLicenseeBatch(string SelectedBatchType, Guid licenseeId)
        {
            //Call 
            Batches = serviceClients.BatchClient.GetBatchForReportManagerByLicenssID(licenseeId);

            if (SharedVMData.SelectedLicensee != null)
            {
                LicenseeBatches = new ObservableCollection<Batch>(Batches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId));// && ((int)s.EntryStatus >= (int)EntryStatus.BatchCompleted)).ToList());
            }

            switch (SelectedBatchType)
            {
                case "Paid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.Paid).ToList());
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "Unpaid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.BatchCompleted || s.EntryStatus == EntryStatus.PartialUnpaid));
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "Partial unpaid":
                    DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.EntryStatus == EntryStatus.PartialUnpaid));
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                case "All":
                    DisplayedBatches = LicenseeBatches;
                    DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    break;
                default:
                    int year = 0;
                    if (int.TryParse(SelectedBatchType, out year))
                    {
                        DisplayedBatches = new ObservableCollection<Batch>(LicenseeBatches.Where(s => s.CreatedDate != null && s.CreatedDate.Value.Year == year).ToList());
                        DisplayedBatches = new ObservableCollection<Batch>(DisplayedBatches.OrderByDescending(r => r.CreatedDate.Value.Date).ThenByDescending(i => i.BatchNumber).ToList());
                    }
                    break;

            }
        }

        private bool BeforeOnPrintPayeeStatementReport()
        {
            if (RoleManager.UserAccessPermission(MasterModule.ReportManager) == ModuleAccessRight.Read)
                return false;

            try
            {
                ObservableCollection<Report> reports = null;
                bool enablePrint = false;
                if (SelectedTab == "Payee")
                {
                    int agentCount = 0;
                    int batchCount = 0;

                    if (Agents != null && DisplayedBatches != null)
                    {
                        agentCount = Agents.Where(s => s.IsChecked == true).Count();
                        batchCount = DisplayedBatches.Where(s => s.IsChecked == true).Count();
                    }

                    if (batchCount != 0 && agentCount != 0)
                        enablePrint = true;

                    reports = PayeeReports;

                    if (reports[1].Code == "PS" && reports[1].IsChecked == true)
                    {
                        IsEnabledAlloption = true;
                    }
                    else
                    {
                        IsEnabledAlloption = false;
                    }

                    //if()
                }
                else if (SelectedTab == "Audit")
                {
                    int payorCount = 0;
                    int agentCount = 0;

                    if (Payors != null && Agents != null)
                    {
                        payorCount = Payors.Where(s => s.IsChecked == true).Count();
                        agentCount = Agents.Where(s => s.IsChecked == true).Count();
                    }

                    if (payorCount != 0 && agentCount != 0)
                        enablePrint = true;

                    reports = AuditReports;

                    foreach (var item in AuditReports)
                    {
                        if (item.IsChecked)
                        {
                            //filter option will be available only for these report like missing pament, follow up, variance payments and variance rate
                            if (item.Code.Contains("AUMIR") || item.Code.Contains("AUFU") || item.Code.Contains("AUVPA") || item.Code.Contains("AUVPR"))
                            {
                                IsEnabledReportType = true;

                                if (item.Code.Contains("AUFU"))
                                {
                                    IsEnabledAlloption = false;
                                    IsExclExcluded = false;
                                }
                                else
                                {
                                    IsEnabledAlloption = true;
                                    IsExclExcluded = true;
                                }
                                break;
                            }
                            else
                            {
                                IsEnabledReportType = false;
                            }
                        }
                        else
                        {
                            IsEnabledReportType = false;
                        }
                    }
                }
                else //management reports
                {
                    int payorCount = 0;
                    int carrierCount = 0;
                    int productCount = 0;
                    int agentCount = 0;

                    if (ManagementPayors != null && ManagementCarriers != null && ManagementProducts != null && ManagementAgents != null)
                    {
                        payorCount = ManagementPayors.Where(s => s.IsChecked == true).Count();
                        carrierCount = ManagementCarriers.Where(s => s.IsChecked == true).Count();
                        productCount = ManagementProducts.Where(s => s.IsChecked == true).Count();
                        agentCount = ManagementAgents.Where(s => s.IsChecked == true).Count();
                    }

                    if (payorCount != 0 && carrierCount != 0 && productCount != 0 && agentCount != 0)
                        enablePrint = true;

                    //New check to force invoice date for ECR
                    if (
                        (MgmtReports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault() != null &&
                        MgmtReports.Where(x => x.Code.ToLower() == "mrecr").FirstOrDefault().IsChecked) ||
                            ((MgmtReports.Where(x => x.Code.ToLower() == "mros").FirstOrDefault() != null &&
                        MgmtReports.Where(x => x.Code.ToLower() == "mros").FirstOrDefault().IsChecked))
                        )
                    {
                        IsECRExcluded = false;
                        IsExclExcluded = false;
                        //if ((ECRInvoiceFrom == null || ECRInvoiceTo == null) ||
                        //  (ECRInvoiceFrom != null && ECRInvoiceTo != null && (ECRInvoiceFrom > ECRInvoiceTo || ECRInvoiceTo > ECRInvoiceFrom.Value.AddMonths(24))))
                        //{
                        //    enablePrint = false;
                        //}
                    }
                    else
                    {
                        IsECRExcluded = true;
                        IsExclExcluded = true;
                    }

                    reports = MgmtReports;
                }

                if (!enablePrint)
                    return false;

                List<Report> rpts = reports.Where(s => s.IsChecked == true).ToList();
                if (rpts.Count != 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        void ReportClient_PrintReportCompleted(object sender, PrintReportCompletedEventArgs e)
        {
            if (objLog == null) objLog = new MastersClient();
            try
            {
                if (e.Error == null)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    bool IsException = false;
                    try
                    {
                        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                        FileUtility ObjDownload = FileUtility.CreateClient(WebDevPath.GetWebDevPath(RoleManager.WebDavPath).URL, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).UserName, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).Password, WebDevPath.GetWebDevPath(RoleManager.WebDavPath).DomainName);
                        ObjDownload.DownloadComplete += (obj1, obj2) =>
                        {
                            autoResetEvent.Set();
                        };

                        string localPath = Path.Combine(System.IO.Path.GetTempPath(), e.Result.FileName);
                        string RemotePath = @"/Reports/" + e.Result.FileName;

                        ObjDownload.Download(RemotePath, localPath);
                        autoResetEvent.WaitOne();
                        _ViewDialog.Close();
                        System.Diagnostics.Process.Start(localPath);
                    }
                    catch (Exception ex)
                    {
                        IsException = true;
                        objLog.AddLog("ReportClient_PrintReportCompleted exception :" + ex.Message);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }

                    if (!IsException && e.Result.ShowPaidPopup)
                    {
                        MessageBoxResult result = MessageBox.Show("Would you like to mark the UNPAID payments in the printed batch(es) as PAID ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            objLog.AddLog("ReportClient_PrintReportCompleted Marking paid  :");
                            bool isBatchSetasPaid = true;

                            FilterBatches = new ObservableCollection<Batch>(DisplayedBatches.Where(s => s.IsChecked == true));
                            ObservableCollection<Guid> lst = new ObservableCollection<Guid>();
                            Guid licenseeId = Guid.Empty;
                            if (FilterBatches.Count > 0)
                            {
                                licenseeId = FilterBatches[0].LicenseeId;
                            }
                            foreach (var item in FilterBatches)
                            {
                                lst.Add(item.BatchId);

                            }

                            List<User> agents = Agents.Where(s => s.IsChecked == true).ToList();
                            ObservableCollection<Guid> AgentIDs = new ObservableCollection<Guid>();
                            foreach (var item in agents)
                            {
                                AgentIDs.Add(item.UserCredentialID);
                            }

                            //Acme commented isBatchSetasPaid = serviceClients.BatchClient.SetBatchesToPaid(lst, PaidFilterValue, AgentIDs);
                            //Before code
                            //isBatchSetasPaid = serviceClients.BatchClient.SetBatchesAsPaid(e.Result.BatchIds);                          
                            string setBatchStatusMsg = serviceClients.BatchClient.SetBatchesToPaidInReports(lst, PaidFilterValue, AgentIDs);
                            objLog.AddLog("ReportClient_PrintReportCompleted all entries marking paid with msg  :" + setBatchStatusMsg);
                            if (licenseeId != Guid.Empty)
                            {
                                RefreshLicenseeBatch(SelectedBatchType, licenseeId);
                            }
                            else
                            {
                                RefreshBatch(SelectedBatchType);
                            }


                            string messageBoxText = setBatchStatusMsg;
                            string str = "Failed Batches";
                            if (messageBoxText.Contains(str))
                            {
                                string caption = "Batch failure status";
                                MessageBoxButton button = MessageBoxButton.OK;
                                MessageBoxImage icon = MessageBoxImage.Information;
                                MessageBox.Show(messageBoxText, caption, button, icon);
                            }
                            else
                            {
                                MessageBox.Show("Marking Batches as Paid operation Successfully Completed");
                            }
                            /*if (!isBatchSetasPaid)
                            {
                                MessageBox.Show("Marking Batches as Paid operation is failed due to some reason. Please try again.", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Marking Batches as Paid operation Successfully Completed.", "Operation Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                            }*/
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog("ReportClient_PrintReportCompleted exception  :" + ex.Message);
            }
        }

        private string PaidFilterValue = "Unpaid";

        private ICommand _PaidFilter;
        public ICommand PaidFilter
        {
            get
            {
                if (_PaidFilter == null)
                    _PaidFilter = new BaseCommand(param => OnPaidFilter(param));
                return _PaidFilter;

            }
        }

        private void OnPaidFilter(object value)
        {
            string str = value as string;
            PaidFilterValue = str;
        }

        public string SelectedTab = "Payee";

        private ICommand _ReportChanged;
        public ICommand ReportChanged
        {
            get
            {
                if (_ReportChanged == null)
                    _ReportChanged = new BaseCommand(param => OnReportChanged(param));
                return _ReportChanged;

            }
        }

        private void OnReportChanged(object param)
        {
            Report rpt = param as Report;
            FocusedReport = rpt;
        }

        #endregion

        #region Payor,Carrier and Product

        private ObservableCollection<DisplayedPayor> _Payors;
        public ObservableCollection<DisplayedPayor> Payors
        {
            get
            {
                return _Payors;
            }
            set
            {
                _Payors = value;
                OnPropertyChanged("Payors");
            }
        }


        private ObservableCollection<DisplayedPayor> _ManagementPayors;
        public ObservableCollection<DisplayedPayor> ManagementPayors
        {
            get
            {
                return _ManagementPayors;
            }
            set
            {
                _ManagementPayors = value;
                OnPropertyChanged("ManagementPayors");
            }
        }

        private ICommand _PayorSelection;
        public ICommand PayorSelection
        {
            get
            {
                if (_PayorSelection == null)
                    _PayorSelection = new BaseCommand(param => OnPayorSelection(param));
                return _PayorSelection;

            }
        }

        private void OnPayorSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (DisplayedPayor payor in Payors)
                        payor.IsChecked = true;

                    foreach (DisplayedPayor payor in ManagementPayors)
                        payor.IsChecked = true;

                    break;
                case "None":
                    foreach (DisplayedPayor payor in Payors)
                        payor.IsChecked = false;

                    foreach (DisplayedPayor payor in ManagementPayors)
                        payor.IsChecked = false;
                    break;
            }
        }

        private ObservableCollection<DisplayedCarrier> _Carriers;
        public ObservableCollection<DisplayedCarrier> Carriers
        {
            get
            {
                return _Carriers;
            }
            set
            {
                _Carriers = value;
                OnPropertyChanged("Carriers");
            }
        }

        private ObservableCollection<DisplayedCarrier> _ManagementCarriers;
        public ObservableCollection<DisplayedCarrier> ManagementCarriers
        {
            get
            {
                return _ManagementCarriers;
            }
            set
            {
                _ManagementCarriers = value;
                OnPropertyChanged("ManagementCarriers");
            }
        }

        private ICommand _CarrierSelection;
        public ICommand CarrierSelection
        {
            get
            {
                if (_CarrierSelection == null)
                    _CarrierSelection = new BaseCommand(param => OnCarrierSelection(param));
                return _CarrierSelection;

            }
        }

        private void OnCarrierSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (DisplayedCarrier carrier in Carriers)
                        carrier.IsChecked = true;

                    foreach (DisplayedCarrier carrier in ManagementCarriers)
                        carrier.IsChecked = true;

                    break;
                case "None":
                    foreach (DisplayedCarrier carrier in Carriers)
                        carrier.IsChecked = false;

                    foreach (DisplayedCarrier carrier in ManagementCarriers)
                        carrier.IsChecked = false;
                    break;
            }
        }

        private ObservableCollection<DisplayedCoverage> _Products;
        public ObservableCollection<DisplayedCoverage> Products
        {
            get
            {
                return _Products;
            }
            set
            {
                _Products = value;
                OnPropertyChanged("Products");
            }
        }

        //Management reports
        private ObservableCollection<DisplayedCoverage> _ManagementProducts;
        public ObservableCollection<DisplayedCoverage> ManagementProducts
        {
            get
            {
                return _ManagementProducts;
            }
            set
            {
                _ManagementProducts = value;
                OnPropertyChanged("ManagementProducts");
            }
        }

        private ICommand _ProductSelection;
        public ICommand ProductSelection
        {
            get
            {
                if (_ProductSelection == null)
                    _ProductSelection = new BaseCommand(param => OnProductSelection(param));
                return _ProductSelection;

            }
        }

        private void OnProductSelection(object value)
        {
            string val = value as string;
            switch (val)
            {
                case "All":
                    foreach (DisplayedCoverage cov in Products)
                        cov.IsChecked = true;

                    foreach (DisplayedCoverage cov in ManagementProducts)
                        cov.IsChecked = true;

                    break;
                case "None":
                    foreach (DisplayedCoverage cov in Products)
                        cov.IsChecked = false;

                    foreach (DisplayedCoverage cov in ManagementProducts)
                        cov.IsChecked = false;

                    break;
            }
        }

        #endregion

        #region Audit report parameter

        private DateTime? _FromInvoiceDate;
        public DateTime? FromInvoiceDate
        {
            get { return _FromInvoiceDate; }
            set
            {
                _FromInvoiceDate = value;
                OnPropertyChanged("FromInvoiceDate");
            }
        }

        private DateTime? _ToInvoiceDate;
        public DateTime? ToInvoiceDate
        {
            get { return _ToInvoiceDate; }
            set
            {
                _ToInvoiceDate = value;
                OnPropertyChanged("ToInvoiceDate");
            }
        }
        //  Dec 27,2018 - Ankit changed this for showing report based on order by client
        private string OrderByParameter = "Client";

        private ICommand _OrderBy;
        public ICommand OrderBy
        {
            get
            {
                if (_OrderBy == null)
                    _OrderBy = new BaseCommand(param => OnOrderBy(param));
                return _OrderBy;

            }
        }

        private void OnOrderBy(object value)
        {
            OrderByParameter = value as string;
        }

        private int FilterByParameter = 1;

        private ICommand _isFilterBy;
        public ICommand isFilterBy
        {
            get
            {
                if (_isFilterBy == null)
                    _isFilterBy = new BaseCommand(param => OnFilterBy(param));
                return _isFilterBy;

            }
        }

        private void OnFilterBy(object value)
        {
            FilterByParameter = Convert.ToInt32(value);
        }

        #endregion

        #region Management Report

        private string TrackPaymentParameter = "Both";

        private ICommand _TrackPayment;
        public ICommand TrackPayment
        {
            get
            {
                if (_TrackPayment == null)
                    _TrackPayment = new BaseCommand(param => OnTrackPayment(param));
                return _TrackPayment;

            }
        }

        private void OnTrackPayment(object value)
        {
            TrackPaymentParameter = value as string;
        }

        private string _PolicyType = "Active/Pending";
        public string PolicyType
        {
            get { return _PolicyType; }
            set { _PolicyType = value; OnPropertyChanged("PolicyType"); }
        }

        private string _PolicyMode = "All";
        public string PolicyMode
        {
            get { return _PolicyMode; }
            set { _PolicyMode = value; OnPropertyChanged("PolicyMode"); }
        }

        private string _PolicyTermReason = "All";
        public string PolicyTermReason
        {
            get { return _PolicyTermReason; }
            set { _PolicyTermReason = value; OnPropertyChanged("PolicyTermReason"); }
        }

        private string _effectiveMonth = "Any";
        public string effectiveMonth
        {
            get { return _effectiveMonth; }
            set { _effectiveMonth = value; OnPropertyChanged("effectiveMonth"); }
        }

        private string _OrderByParamMgmtReport = "None";
        public string OrderByParamMgmtReport
        {
            get { return _OrderByParamMgmtReport; }
            set { _OrderByParamMgmtReport = value; OnPropertyChanged("OrderByParamMgmtReport"); }
        }

        private DateTime? _FromEffectiveDate;
        public DateTime? FromEffectiveDate
        {
            get { return _FromEffectiveDate; }
            set { _FromEffectiveDate = value; OnPropertyChanged("FromEffectiveDate"); }
        }

        private DateTime? _ToEffectiveDate;
        public DateTime? ToEffectiveDate
        {
            get { return _ToEffectiveDate; }
            set { _ToEffectiveDate = value; OnPropertyChanged("ToEffectiveDate"); }
        }

        private DateTime? _FromTrackDate;
        public DateTime? FromTrackDate
        {
            get { return _FromTrackDate; }
            set { _FromTrackDate = value; OnPropertyChanged("FromTrackDate"); }
        }

        private DateTime? _ToTrackDate;
        public DateTime? ToTrackDate
        {
            get { return _ToTrackDate; }
            set { _ToTrackDate = value; OnPropertyChanged("ToTrackDate"); }
        }

        private DateTime? _FromTermDate;
        public DateTime? FromTermDate
        {
            get { return _FromTermDate; }
            set { _FromTermDate = value; OnPropertyChanged("FromTermDate"); }
        }

        private DateTime? _ToTermDate;
        public DateTime? ToTermDate
        {
            get { return _ToTermDate; }
            set { _ToTermDate = value; OnPropertyChanged("ToTermDate"); }
        }

        private decimal? _PremiumStart;
        public decimal? PremiumStart
        {
            get { return _PremiumStart; }
            set { _PremiumStart = value; OnPropertyChanged("PremiumStart"); }
        }

        private decimal? _PremiumEnd;
        public decimal? PremiumEnd
        {
            get { return _PremiumEnd; }
            set { _PremiumEnd = value; OnPropertyChanged("PremiumEnd"); }
        }

        private int? _EnrolledStart;
        public int? EnrolledStart
        {
            get { return _EnrolledStart; }
            set { _EnrolledStart = value; OnPropertyChanged("EnrolledStart"); }
        }

        private int? _EnrolledEnd;
        public int? EnrolledEnd
        {
            get { return _EnrolledEnd; }
            set { _EnrolledEnd = value; OnPropertyChanged("EnrolledEnd"); }
        }

        private int? _EligibleStart;
        public int? EligibleStart
        {
            get { return _EligibleStart; }
            set { _EligibleStart = value; OnPropertyChanged("EligibleStart"); }
        }

        private int? _EligibleEnd;
        public int? EligibleEnd
        {
            get { return _EligibleEnd; }
            set { _EligibleEnd = value; OnPropertyChanged("EligibleEnd"); }
        }

        private DateTime? _ECRInvoiceFrom;
        public DateTime? ECRInvoiceFrom
        {
            get { return _ECRInvoiceFrom; }
            set { _ECRInvoiceFrom = value; OnPropertyChanged("ECRInvoiceFrom"); }
        }

        private DateTime? _ECRInvoiceTo;
        public DateTime? ECRInvoiceTo
        {
            get { return _ECRInvoiceTo; }
            set { _ECRInvoiceTo = value; OnPropertyChanged("ECRInvoiceTo"); }
        }

        #endregion

        #region Implement IDataRefresh

        public void Refresh()
        {
            try
            {
                SelectedLicenseeChanged();
                //Batches = serviceClients.BatchClient.GetBatchesForReportManager();
                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {
                    Batches = serviceClients.BatchClient.GetBatchesForReportManager();
                }
                else
                {
                    DateTime dt = new DateTime();
                    Batches = serviceClients.BatchClient.GetCurrentBatch(SharedVMData.SelectedLicensee.LicenseeId, dt);
                }

                Batches.Where(s => s.EntryStatus == EntryStatus.PartialUnpaid).ToList().ForEach(s => s.FileName += "(Partial Unpaid)");
                LicenseeBatches = Batches;
                //LicenseeBatches = new ObservableCollection<Batch>(Batches.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && ((int)s.EntryStatus >= (int)EntryStatus.BatchCompleted)).ToList());

                if (SharedVMData.SelectedLicensee != null)
                {
                    //Agents = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).ToList());
                    if (SharedVMData.CachedAgentList.ContainsKey(SharedVMData.SelectedLicensee.LicenseeId))
                    {
                        Agents = new ObservableCollection<User>(SharedVMData.CachedAgentList[SharedVMData.SelectedLicensee.LicenseeId]);
                    }
                    else
                    {
                        Agents = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(SharedVMData.SelectedLicensee.LicenseeId).Where(s => s.Role == UserRole.Agent).OrderBy(s => s.NickName).ToList());
                    }
                    PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };

                    Payors = serviceClients.DisplayedPayorClient.GetDisplayPayors(SharedVMData.SelectedLicensee.LicenseeId, fillInfo);
                    ManagementPayors = new ObservableCollection<DisplayedPayor>(Payors);

                    Carriers = serviceClients.CarrierClient.GetDispalyedCarriersWith(SharedVMData.SelectedLicensee.LicenseeId, false);
                    ManagementCarriers = new ObservableCollection<DisplayedCarrier>(Carriers);

                    Products = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(SharedVMData.SelectedLicensee.LicenseeId);
                    ManagementProducts = new ObservableCollection<DisplayedCoverage>(Products);

                    OnAgentSelection("All");
                    OnPayorSelection("All");
                    OnCarrierSelection("All");
                    OnProductSelection("All");
                }
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

        #endregion
    }
}
