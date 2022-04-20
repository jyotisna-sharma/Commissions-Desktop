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
using System.Threading;
using System.IO;
using MyAgencyVault.ViewModel.CommonBatch;
using MyAgencyVault.VM.VMLib;

namespace MyAgencyVault.VMLib
{
    public class VMStatementManager : BaseViewModel, IDataRefresh
    {
        static MastersClient objLog = new MastersClient();
        #region Foreground color of batch filter link

        private string _AvailableBatchLinkColor = "Blue";
        public string AvailableBatchLinkColor
        {
            get { return _AvailableBatchLinkColor; }
            set { _AvailableBatchLinkColor = value; OnPropertyChanged("AvailableBatchLinkColor"); }
        }

        private string _InProgressBatchLinkColor = "Blue";
        public string InProgressBatchLinkColor
        {
            get { return _InProgressBatchLinkColor; }
            set { _InProgressBatchLinkColor = value; OnPropertyChanged("InProgressBatchLinkColor"); }
        }

        private string _OpenBatchLinkColor = "Blue";
        public string OpenBatchLinkColor
        {
            get { return _OpenBatchLinkColor; }
            set { _OpenBatchLinkColor = value; OnPropertyChanged("OpenBatchLinkColor"); }
        }

        private string _ClosedBatchLinkColor = "Blue";
        public string ClosedBatchLinkColor
        {
            get { return _ClosedBatchLinkColor; }
            set { _ClosedBatchLinkColor = value; OnPropertyChanged("ClosedBatchLinkColor"); }
        }

        private string _AllBatchLinkColor = "Blue";
        public string AllBatchLinkColor
        {
            get { return _AllBatchLinkColor; }
            set { _AllBatchLinkColor = value; OnPropertyChanged("AllBatchLinkColor"); }
        }

        #endregion

        public delegate void PopUpLaunchWebSite();
        public event PopUpLaunchWebSite OpenPopUpLaunchWebSite;

        private bool _RefreshRequired = false;
        public bool RefreshRequired
        {
            get { return _RefreshRequired; }
            set { _RefreshRequired = value; }
        }

        private BaseBatch baseBatch;
        //private List<string> WebSiteUris;
        private Dictionary<int, DownloadBatchVM> DictionaryDownloadBatch;

        private bool _IsLaunchWebSiteEnabled;
        public bool IsLaunchWebSiteEnabled
        {
            get { return _IsLaunchWebSiteEnabled; }
            set
            {
                _IsLaunchWebSiteEnabled = value;
                OnPropertyChanged("IsLaunchWebSiteEnabled");
            }
        }

        private int _currentProgress;
        public int CurrentProgress
        {
            get { return this._currentProgress; }
            private set
            {
                if (this._currentProgress != value)
                {
                    this._currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }

        private void EnableDisableControl()
        {
            IsLaunchWebSiteEnabled = false;
            if (CurrentDownloadBatch != null)
            {
                if (CurrentDownloadBatch.DownloadBatch.UploadStatus != UploadStatus.Completed)
                    IsLaunchWebSiteEnabled = true;
            }
        }

        private ObservableCollection<DownloadBatchVM> _downloadBatches;
        public ObservableCollection<DownloadBatchVM> DownloadBatches
        {
            get { return _downloadBatches; }
            set
            {
                _downloadBatches = value;
                OnPropertyChanged("DownloadBatches");
            }
        }

        private ObservableCollection<DownloadBatchVM> _displayedDownloadBatches;
        public ObservableCollection<DownloadBatchVM> DisplayedDownloadBatches
        {
            get { return _displayedDownloadBatches; }
            set
            {
                _displayedDownloadBatches = value;
                OnPropertyChanged("DisplayedDownloadBatches");
            }
        }

        private DownloadBatchVM _currentDownloadBatch;
        public DownloadBatchVM CurrentDownloadBatch
        {
            get { return _currentDownloadBatch; }
            set
            {
                _currentDownloadBatch = value;
                EnableDisableControl();
                OnPropertyChanged("CurrentDownloadBatch");
            }
        }

        private ObservableCollection<PayorDefaults> payorDefaults;
        private ObservableCollection<PayorSiteLoginInfo> payorSiteLoginInfo;

        public VMStatementManager()
        {
            //StmtScreenControl();

            baseBatch = new BaseBatch();
            DictionaryDownloadBatch = new Dictionary<int, DownloadBatchVM>();
            //WebSiteUris = new List<string>();
            LoadData(false);
        }

        private void LoadData(bool maintainState)
        {
            try
            {
              using (ServiceClients serviceClients = new ServiceClients())
              {
                payorDefaults = serviceClients.PayorDefaultsClient.GetPayorDefaults();
                payorSiteLoginInfo = serviceClients.PayorUserWebSiteClient.GetLicenseeUsers(Guid.Empty);
                GenerateBatchRecordsForPayors();

                //Sorting Uploaded batch by batch Number
                ObservableCollection<DownloadBatch> downloadBatches = new ObservableCollection<DownloadBatch>(serviceClients.DownloadBatchClient.GetDownloadBatchList().OrderBy(b => b.BatchNumber).ToList());

                if (downloadBatches != null && downloadBatches.Count != 0)
                {
                  DownloadBatches = new ObservableCollection<DownloadBatchVM>();
                  foreach (DownloadBatch downloadBatch in downloadBatches)
                  {
                    DownloadBatchVM downloadBatchVm = new DownloadBatchVM(downloadBatch, payorDefaults, payorSiteLoginInfo);
                    DownloadBatches.Add(downloadBatchVm);
                  }
                }

                DisplayedDownloadBatches = DownloadBatches;
                if (DownloadBatches != null)
                  CurrentDownloadBatch = DownloadBatches[0];
              }
            }
            catch
            {
            }
        }

        private void GenerateBatchRecordsForPayors()
        {
            try
            {
              using (ServiceClients serviceClients = new ServiceClients())
              {
                ObservableCollection<StatementDates> payorStatementDates = serviceClients.StatementDatesClient.GetActiveStatementDates();
                if (payorStatementDates != null && payorStatementDates.Count != 0)
                {
                  if (payorSiteLoginInfo != null && payorSiteLoginInfo.Count != 0)
                  {
                    ObservableCollection<FileType> fileTypes = serviceClients.MasterClient.GetSupportedFileTypeList();

                    List<PayorSiteLoginInfo> loginInfo = null;
                    List<StatementDates> statementDates = null;

                    List<Guid> payorIds = payorStatementDates.Select(s => s.PayorID).Distinct().ToList();

                    if (payorIds != null && payorIds.Count != 0)
                    {
                      foreach (Guid payorId in payorIds)
                      {
                        loginInfo = payorSiteLoginInfo.Where(s => s.PayorID == payorId).ToList();
                        statementDates = payorStatementDates.Where(s => s.PayorID == payorId).ToList();

                        if (loginInfo != null && statementDates != null && loginInfo.Count != 0 && statementDates.Count != 0)
                        {
                          foreach (PayorSiteLoginInfo logInfo in loginInfo)
                          {
                            foreach (StatementDates date in statementDates)
                            {
                              Batch batch = new Batch();
                              batch.BatchId = Guid.NewGuid();
                              batch.LicenseeId = logInfo.LicenseID.Value;
                              batch.UploadStatus = UploadStatus.Available;
                              batch.EntryStatus = EntryStatus.Unassigned;
                              batch.SiteId = logInfo.SiteID;
                              batch.IsManuallyUploaded = false;
                              int FileType = payorDefaults.FirstOrDefault(s => s.GlobalPayorId == payorId).FileTypeId;
                              batch.FileType = fileTypes.FirstOrDefault(p => p.FileTypeId == FileType).Name;
                              batch.PayorId = date.PayorID;

                              BatchAddOutput batchoutput = serviceClients.BatchClient.AddUpdateBatchWithBatchOutput(batch);
                              batch.FileName = batchoutput.LicenseeName + "_" + batchoutput.BatchNumber.ToString() + "." + batch.FileType;
                            }
                          }

                          serviceClients.StatementDatesClient.MarkAsBatchGenerated(new ObservableCollection<StatementDates>(statementDates));
                        }
                      }
                    }
                  }
                }
              }
            }
            catch (Exception)
            {
            }
        }

        #region ControlLevelProerty
        #region PeopleScreen

        #endregion
        #endregion

        private ICommand _LaunchWebSite;
        public ICommand LaunchWebSite
        {
            get
            {
                // System.Threading.Thread sf = new System.Threading.Thread(new System.Threading.ThreadStart(DoLaunchWebSite));
                if (_LaunchWebSite == null)
                {
                    if (_LaunchWebSite == null)
                    {
                        _LaunchWebSite = new BaseCommand(param => BeforeDoLaunchWebSite(), param => DoLaunchWebSite());

                    }

                }
                return _LaunchWebSite;

            }

        }

        private bool BeforeDoLaunchWebSite()
        {
            try
            {
                if (CurrentDownloadBatch == null)
                    return false;

                if (CurrentDownloadBatch.Url == null)
                    return false;

                if (CurrentDownloadBatch.DownloadBatch.UploadStatus == UploadStatus.Available)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private ICommand _FilterBatches;
        public ICommand FilterBatches
        {
            get
            {
                if (_FilterBatches == null)
                {
                    _FilterBatches = new BaseCommand(p => OnFilterBatches(p));
                }
                return _FilterBatches;
            }
        }

        private void OnFilterBatches(object value)
        {
            string filterString = value as string;
            AvailableBatchLinkColor = "Blue";
            InProgressBatchLinkColor = "Blue";
            OpenBatchLinkColor = "Blue";
            ClosedBatchLinkColor = "Blue";
            AllBatchLinkColor = "Blue";

            try
            {
                switch (filterString)
                {
                    case "Available":
                        DisplayedDownloadBatches = new ObservableCollection<DownloadBatchVM>(DownloadBatches.Where(s => s.DownloadBatch.UploadStatus == UploadStatus.Available).ToList());
                        AvailableBatchLinkColor = "Purple";
                        break;
                    case "InProgress":
                        DisplayedDownloadBatches = new ObservableCollection<DownloadBatchVM>(DownloadBatches.Where(s => s.DownloadBatch.UploadStatus == UploadStatus.InProgress).ToList());
                        InProgressBatchLinkColor = "Purple";
                        break;
                    case "Open":
                        DisplayedDownloadBatches = new ObservableCollection<DownloadBatchVM>(DownloadBatches.Where(s => s.DownloadBatch.UploadStatus == UploadStatus.Available || s.DownloadBatch.UploadStatus == UploadStatus.InProgress).ToList());
                        OpenBatchLinkColor = "Purple";
                        break;
                    case "Closed":
                        DisplayedDownloadBatches = new ObservableCollection<DownloadBatchVM>(DownloadBatches.Where(s => s.DownloadBatch.UploadStatus == UploadStatus.Completed).ToList());
                        ClosedBatchLinkColor = "Purple";
                        break;
                    case "ShowAll":
                        DisplayedDownloadBatches = DownloadBatches;
                        AllBatchLinkColor = "Purple";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private ICommand _DeleteRecord;
        public ICommand DeleteRecord
        {
            get
            {
                if (_DeleteRecord == null)
                {
                    _DeleteRecord = new BaseCommand(p => BeforeOnDeleteRecord(), p => OnDeleteRecord(p));
                }
                return _DeleteRecord;
            }
        }

        private bool BeforeOnDeleteRecord()
        {

            if (CurrentDownloadBatch != null && CurrentDownloadBatch.DownloadBatch.EntryStatus != EntryStatus.Paid)
                return true;
            else
                return false;
        }

        private void OnDeleteRecord(object value)
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.ImportUnsuccessfull || CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.BatchCompleted || CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.PartialUnpaid)
                        {
                            objLog.AddLog(" Statement manager OnDeleteRecord request : " + CurrentDownloadBatch.DownloadBatch.BatchId + ", User: " + RoleManager.userCredentialID);
                            serviceClients.DownloadBatchClient.DeleteDownloadBatch(CurrentDownloadBatch.DownloadBatch, RoleManager.Role);
                        }

                        DisplayedDownloadBatches.Remove(CurrentDownloadBatch);
                        DownloadBatches.Remove(CurrentDownloadBatch);
                    }
                }
            }
            catch (Exception)
            {
                objLog.AddLog(" Statement manager OnDeleteRecord : " + CurrentDownloadBatch.DownloadBatch.BatchId);
            }
        }


        private ICommand _ClearDownloadStatus;
        public ICommand ClearDownloadStatus
        {
            get
            {
                if (_ClearDownloadStatus == null)
                {
                    _ClearDownloadStatus = new BaseCommand(p => OnClearDownloadStatus(), p => OnClearDownloadStatus(p));
                }
                return _ClearDownloadStatus;
            }
        }

        private bool OnClearDownloadStatus()
        {
            if (CurrentDownloadBatch == null)
                return false;

            if (CurrentDownloadBatch.DownloadBatch.UploadStatus == UploadStatus.Completed && CurrentDownloadBatch.DownloadBatch.EntryStatus != EntryStatus.Unassigned && CurrentDownloadBatch.DownloadBatch.EntryStatus != EntryStatus.Paid)
                return true;
            else
                return false;
        }

        private void OnClearDownloadStatus(object value)
        {
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              if (CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.Paid)
                return;

              MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
              if (result == MessageBoxResult.Yes)
              {

                  if (CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.ImportUnsuccessfull || CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.BatchCompleted || CurrentDownloadBatch.DownloadBatch.EntryStatus == EntryStatus.PartialUnpaid)
                {
                  CurrentDownloadBatch.DownloadBatch.UploadStatus = UploadStatus.Available;
                  OnUploadStatusChanged(CurrentDownloadBatch);
                  CurrentDownloadBatch.DownloadBatch.EntryStatus = EntryStatus.Unassigned;

                  serviceClients.DownloadBatchClient.ClearDownloadBatch(CurrentDownloadBatch.DownloadBatch, RoleManager.Role);
                }
                else
                {
                  CurrentDownloadBatch.DownloadBatch.UploadStatus = UploadStatus.Available;
                  OnUploadStatusChanged(CurrentDownloadBatch);
                  CurrentDownloadBatch.DownloadBatch.EntryStatus = EntryStatus.Unassigned;

                  serviceClients.DownloadBatchClient.UpdateEntryStatus(CurrentDownloadBatch.DownloadBatch);

                }
              }
            }
          }
          catch (Exception)
          {
          }
        }


        /// <summary>
        /// to get selected Detes 
        /// </summary>
        private StatementDates _selectedStatementDates;
        public StatementDates CurrentStatementDate
        {
            get
            {
                return _selectedStatementDates == null ? new StatementDates() : _selectedStatementDates;
            }
            set
            {
                _selectedStatementDates = value;
                OnPropertyChanged("CurrentStatementDate");

            }
        }

        /// <summary>
        /// collection to get statement date details
        /// </summary>
        private ObservableCollection<StatementDates> _payorStatementDate;
        public ObservableCollection<StatementDates> payorStatementDate
        {
            get
            {
                return _payorStatementDate;
            }
            set
            {
                _payorStatementDate = value;
            }
        }

        private void DoLaunchWebSite()
        {
            if (OpenPopUpLaunchWebSite != null)
            {
                OpenPopUpLaunchWebSite();
            }
        }

        Batch _CurrBatchInfo = null;

        public Batch BatchInfoData
        {
            get
            {
                return _CurrBatchInfo == null ? new Batch() : _CurrBatchInfo;
            }
            set
            {
                _CurrBatchInfo = value;
                OnPropertyChanged("BatchInfoData");
            }

        }

        private void SaveBatchData()
        {
          using (ServiceClients serviceClients = new ServiceClients())
          {
            if (BatchInfoData == null) return;
            serviceClients.BatchClient.AddUpdateIBatch(BatchInfoData);
          }
        }

        public void DownloadBatchFile(string Url, DownloadBatchVM batch)
        {
            try
            {
                bool isDownloadApp = ValidateForDownload(batch);

                if (isDownloadApp)
                {
                    //if (!WebSiteUris.Contains(Url))
                    //{
                    //WebSiteUris.Add(Url);

                    bool isBatchIsAlreadyDownload = false;
                    if (batch.DownloadBatch.UploadStatus == UploadStatus.InProgress)
                    {
                        if (File.Exists(Path.Combine(ApplicationAgencyVault.ApplicationDataDirectory(), batch.FileName)))
                            isBatchIsAlreadyDownload = true;
                    }

                    if (!isBatchIsAlreadyDownload)
                    {
                        System.Net.WebClient webClient = new System.Net.WebClient();
                        int clientHashCode = webClient.GetHashCode();
                        DictionaryDownloadBatch.Add(clientHashCode, batch);

                        webClient.DownloadDataCompleted += new System.Net.DownloadDataCompletedEventHandler(webClient_DownloadDataCompleted);
                        Uri uri = new Uri(Url);
                        webClient.DownloadDataAsync(uri);

                        batch.DownloadBatch.UploadStatus = UploadStatus.InProgress;
                        OnUploadStatusChanged(batch);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void OnUploadStatusChanged(DownloadBatchVM batch)
        {
            EnableDisableControl();
        }

        private bool ValidateForDownload(DownloadBatchVM batch)
        {
            if (batch.DownloadBatch.UploadStatus == UploadStatus.Completed)
                return false;
            else
                return true;
        }

        void webClient_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Net.WebClient webClient = sender as System.Net.WebClient;
                int clientHashCode = webClient.GetHashCode();
                if (DictionaryDownloadBatch.ContainsKey(clientHashCode))
                {
                    string waterMarkFile = string.Empty;
                    DownloadBatchVM batch = DictionaryDownloadBatch[clientHashCode];
                    try
                    {
                        byte[] filedata = e.Result;
                        FileStream fs;
                        string filePath = Path.Combine(Path.GetTempPath(), batch.FileName);
                        fs = new FileStream(filePath, FileMode.Create);
                        fs.Write(filedata, 0, filedata.Length);
                        fs.Close();
                        

                        if (batch.DownloadBatch.FileType == "pdf")
                        {
                            waterMarkFile = baseBatch.CreateWaterMarkFile(filePath, batch.DownloadBatch.LicenseeName);
                        }
                        else
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            waterMarkFile = Path.Combine(ApplicationAgencyVault.ApplicationDataDirectory(), batch.FileName);
                            if (File.Exists(waterMarkFile))
                                File.Delete(waterMarkFile);
                            fileInfo.MoveTo(waterMarkFile);
                        }
                    }
                    catch
                    {
                    }
                    try
                    {
                        UploadBatchFile(waterMarkFile, batch);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                MessageBox.Show("There is problem with downloading the file.Please try again.", "Error", MessageBoxButton.OK);
                System.Net.WebClient webClient = sender as System.Net.WebClient;
                int clientHashCode = webClient.GetHashCode();
                if (DictionaryDownloadBatch.ContainsKey(clientHashCode))
                {
                    DownloadBatchVM batch = DictionaryDownloadBatch[clientHashCode];
                    batch.DownloadBatch.EntryStatus = EntryStatus.Unassigned;
                }
            }
        }

        /// <summary>
        /// Upload the batch file to the server.
        /// </summary>
        /// <returns></returns>
        private void UploadBatchFile(string file, DownloadBatchVM batch)
        {
            if (objLog == null)
                objLog = new MastersClient();
            string dirPath = string.Empty;

            try
            {
                WebDevPath webPath = WebDevPath.GetWebDevPath(RoleManager.WebDavPath);

                FileUtility ObjUpload = FileUtility.CreateClient(webPath.URL, webPath.UserName, webPath.Password, webPath.DomainName);
                ObjUpload.UploadComplete += new UploadCompleteDel(UploadCompleted);
                ObjUpload.ErrorOccured += new ErrorOccuredDel(ObjUpload_ErrorOccured);
                ObjUpload.Upload(file, @"/UploadBatch/" + Path.GetFileName(file), batch);
            }
            catch (Exception ex)
            {
                objLog.AddLog("UploadBatchFile exception :" + ex.Message);
            }
        }

        void ObjUpload_ErrorOccured(Exception error)
        {

        }

        private void UploadCompleted(int size, object arg)
        {
            //Update the batch record if the upload is completed.
            if (objLog == null)
                objLog = new MastersClient();
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              DownloadBatchVM batch = arg as DownloadBatchVM;
              batch.DownloadBatch.UploadStatus = UploadStatus.Completed;
              batch.DownloadBatch.EntryStatus = EntryStatus.ImportPending;
              OnUploadStatusChanged(batch);

              DateTime? createdDate = serviceClients.DownloadBatchClient.UpdateEntryStatus(batch.DownloadBatch);
              batch.DownloadBatch.CreatedDate = createdDate;

              if (batch.DownloadBatch.FileType == "xls")
              {
                serviceClients.DownloadBatchClient.ImportBatchFileCompleted += new EventHandler<ImportBatchFileCompletedEventArgs>(DownloadBatchClient_ImportBatchFileCompleted);
                serviceClients.DownloadBatchClient.ImportBatchFileAsync(batch.DownloadBatch, RoleManager.Role);
              }
            }
          }
          catch (Exception ex)
          {
              objLog.AddLog("UploadCompleted exception :" + ex.Message);
          }
        }


        void DownloadBatchClient_ImportBatchFileCompleted(object sender, ImportBatchFileCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ImportFileData ImportData = e.Result;
                DownloadBatch batch = DownloadBatches.Select(s => s.DownloadBatch).ToList().FirstOrDefault(s => s.BatchId == ImportData.BatchId);

                if (batch != null)
                {
                    if (ImportData.IsImportedSuccessfully)
                    {
                        batch.EntryStatus = EntryStatus.BatchCompleted;
                    }
                    else
                    {
                        batch.EntryStatus = EntryStatus.InDataEntry;
                    }
                }
            }
        }

        private bool ImportFile(string fileName)
        {
            //Call the method that takes file name as input and 
            //and import the file data into database table.
            return false;
        }

        public void Refresh()
        {
            LoadData(true);
        }
    }

    /// <summary>
    /// This is VM Download Manager.
    /// </summary>
    public class DownloadBatchVM
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserNameControl { get; set; }
        public string PasswordControl { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string NavigationInstructions { get; set; }
        public DownloadBatch DownloadBatch { get; set; }

        public DownloadBatchVM(DownloadBatch batch, ObservableCollection<PayorDefaults> payorDefaults, ObservableCollection<PayorSiteLoginInfo> payorSiteLoginInfo)
        {
            DownloadBatch = batch;

            FileName = batch.LicenseeName + "_" + batch.BatchNumber.ToString() + "." + batch.FileType;

            PayorDefaults payorDefault = payorDefaults.FirstOrDefault(s => s.GlobalPayorId == batch.PayorId);
            PayorSiteLoginInfo loginInfo = payorSiteLoginInfo.FirstOrDefault(s => s.SiteID == batch.SiteId);

            if (payorDefault != null)
            {
                NavigationInstructions = payorDefault.NavigationInstructions;
                Url = payorDefault.WebSiteUrl;
                UserNameControl = payorDefault.LoginControl;
                PasswordControl = payorDefault.PasswordControl;
            }

            if (loginInfo != null)
            {
                UserName = loginInfo.LogInName;
                Password = loginInfo.Password;
            }
        }
    }
}
