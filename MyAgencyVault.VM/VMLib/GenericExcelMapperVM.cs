using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM;
using MyAgencyVault.VM.CommonItems;
using System.Threading;
using System.Windows;
using MyAgencyVault.VM.VMLib.PayorForm;
using System.IO;
using MyAgencyVault.VM.VMLib;
using System.Windows.Forms;

namespace MyAgencyVault.ViewModel.VMLib
{

    public enum DeuMapperField
    {
        PolicyNumber,
        Insured,
        Carrier,
        Product,
        ModelAvgPremium,
        PolicyMode,
        Enrolled,
        SplitPercentage,
        Client,
        CompType,
        PayorSysId,
        Renewal,
        CompScheduleType,
        InvoiceDate,
        PaymentReceived,
        CommissionPercentage,
        NumberOfUnits,
        DollerPerUnit,
        Fee,
        Bonus,
        CommissionTotal
    }

    public class IssueFile
    {
        public string Name { get; set; }

        public IssueFile(string Name)
        {
            this.Name = Name;
        }
    }

    public class DateFormatCollection : List<string>
    {
        public DateFormatCollection()
        {
            this.Add("");
            this.Add("MMM-yyyy");
            this.Add("MMM-dd-yyyy");
            this.Add("MMM-dd-yy");
            this.Add("dd-MMM-yyyy");
            this.Add("dd-MM-yy");
            this.Add("dd-yy-MM");
            this.Add("MM-yy-dd");
            this.Add("MM-dd-yy");
            this.Add("yy-dd-MM");
            this.Add("yy-MM-dd");
            this.Add("dd-MM-yyyy");
            this.Add("dd-yyyy-MM");
            this.Add("MM-yyyy-dd");
            this.Add("MM-dd-yyyy");
            this.Add("yyyy-dd-MM");
            this.Add("yyyy-MM-dd");
            this.Add("MM-yyyy");
            this.Add("MM-dd-yyyy");
            this.Add("MM-dd-yy");
            this.Add("dd-MM-yyyy");
            this.Add("dd-MM-yy");
            this.Add("M-d-yyyy");
            this.Add("d-M-yyyy");
            this.Add("M/d/yyyy");
            this.Add("d/M/yyyy");
        }
    }

    public class ExtendedDataEntryField : INotifyPropertyChanged
    {
        public DataEntryField EntryField { get; set; }

        private string _ExcelColumnNo;
        public string ExcelColumnNo
        {
            get { return _ExcelColumnNo; }
            set { _ExcelColumnNo = value; OnPropertyChanged("ExcelColumnNo"); }
        }

        private string _ExcelColumnName;
        public string ExcelColumnName
        {
            get { return _ExcelColumnName; }
            set { _ExcelColumnName = value; OnPropertyChanged("ExcelColumnName"); }

        }

        private string _ExcelFormat;
        public string ExcelFormat
        {
            get { return _ExcelFormat; }
            set { _ExcelFormat = value; OnPropertyChanged("ExcelFormat"); }

        }

        public ExtendedDataEntryField(DataEntryField field)
        {
            EntryField = field;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class GenericExcelMapperVM : BaseViewModel
    {
        private ObservableCollection<ExtendedDataEntryField> _ExcelFields;
        public ObservableCollection<ExtendedDataEntryField> ExcelFields
        {
            get { return _ExcelFields; }
            set { _ExcelFields = value; OnPropertyChanged("ExcelFields"); }
        }

        public int ColumnHeaderIndex
        {
            get
            {
                if (PayorTemplate != null)
                    return PayorTemplate.DataStartIndex;
                else
                    return 1;
            }
        }

        //private List<DateFormat> _DateTimeFormats;
        //public List<DateFormat> DateTimeFormats
        //{
        //    get { return _DateTimeFormats; }
        //    set { _DateTimeFormats = value; OnPropertyChanged("DateTimeFormats"); }
        //}

        private ExtendedDataEntryField _SelectedExcelField;
        public ExtendedDataEntryField SelectedExcelField
        {
            get { return _SelectedExcelField; }
            set
            {
                _SelectedExcelField = value;
                if (_SelectedExcelField != null && _SelectedExcelField.EntryField.DeuFieldType == 1)
                    IsDateFormatsEnable = false;
                else
                    IsDateFormatsEnable = true;
                OnPropertyChanged("SelectedExcelField");
            }
        }

        private List<IssueFile> _IssuedFiles;
        public List<IssueFile> IssuedFiles
        {
            get { return _IssuedFiles; }
            set { _IssuedFiles = value; OnPropertyChanged("IssuedFiles"); }
        }

        private IssueFile _SelectedIssuedFile;
        public IssueFile SelectedIssuedFile
        {
            get { return _SelectedIssuedFile; }
            set { _SelectedIssuedFile = value; OnPropertyChanged("SelectedIssuedFile"); }
        }

        private PayorTemplate _PayorTemplate;
        public PayorTemplate PayorTemplate
        {
            get { return _PayorTemplate; }
            set { _PayorTemplate = value; OnPropertyChanged("PayorTemplate"); }
        }

        public List<string> DeuFields
        {
            get
            {
                List<string> _DeuFields = new List<string>();
                foreach (var field in Enum.GetValues(typeof(DeuMapperField)))
                {
                    _DeuFields.Add(field.ToString());
                }
                return _DeuFields;
            }
        }

        private Payor _CurrentPayor;
        public Payor CurrentPayor
        {
            set
            {
                _CurrentPayor = value;
                OnPropertyChanged("CurrentPayor");
            }
            get
            {
                return _CurrentPayor;
            }
        }

        private ICommand _CancelTemplateCommand;
        public ICommand CancelTemplateCommand
        {
            get
            {
                if (_CancelTemplateCommand == null)
                    _CancelTemplateCommand = new BaseCommand(param => OnCancelTemplate());
                return _CancelTemplateCommand;
            }
        }

        public void OnCancelTemplate()
        {

            try
            {
                PayorTemplateClient payorTempClient = new PayorTemplateClient();
                PayorTemplate = payorTempClient.getPayorTemplate(CurrentPayor.PayorID);
                if (ReloadData != null)
                    ReloadData(null, null);
            }
            catch (Exception)
            {

            }
        }

        //private ICommand _SaveTemplateCommand;
        //public ICommand SaveTemplateCommand
        //{
        //    get
        //    {
        //        if (_SaveTemplateCommand == null)
        //            _SaveTemplateCommand = new BaseCommand(param => OnSaveTemplate());
        //        return _SaveTemplateCommand;
        //    }
        //}

        public void OnSaveTemplate(string ExcelFieldList)
        {
            try
            {
                _PayorTemplate.PayorId = CurrentPayor.PayorID;

                if (!string.IsNullOrEmpty(ExcelFieldList))
                    _PayorTemplate.XlsColumnList = ExcelFieldList;

                if (PayorTemplate.MappedFields == null)
                    PayorTemplate.MappedFields = new ObservableCollection<MappedField>();
                else
                    PayorTemplate.MappedFields.Clear();

                foreach (ExtendedDataEntryField entryField in ExcelFields)
                {
                    if (!string.IsNullOrEmpty(entryField.ExcelColumnName))
                    {
                        MappedField mapField = new MappedField();
                        mapField.ExcelField = entryField.ExcelColumnNo;
                        mapField.DBField = entryField.EntryField.DeuFieldName;
                        mapField.ExcelFieldName = entryField.ExcelColumnName;
                        mapField.Format = entryField.ExcelFormat;
                        PayorTemplate.MappedFields.Add(mapField);
                    }
                }
                using (ServiceClients serviceClients = new ServiceClients())
                {
                  serviceClients.PayorTemplateClient.AddUpdateTemplate(_PayorTemplate);
                }
            }
            catch(Exception)
            {
                
            }
        }

        private ICommand _FileDblClick;
        public ICommand FileDblClick
        {
            get
            {
                if (_FileDblClick == null)
                    _FileDblClick = new BaseCommand(param => OnFileDblClick());
                return _FileDblClick;
            }
        }

        public void OnFileDblClick()
        {
            if (SelectedIssuedFile != null)
            {
                string excelFile = DownloadExcelFile(SelectedIssuedFile.Name);
                FileDownloaded(excelFile, null);
            }
        }

        private bool _IsDateFormatsEnable = false;
        public bool IsDateFormatsEnable
        {
            get { return _IsDateFormatsEnable; }
            set { _IsDateFormatsEnable = value; OnPropertyChanged("IsDateFormatsEnable"); }
        }

        //private ICommand _PasteExcelFieldCommand;
        //public ICommand PasteExcelFieldCommand
        //{
        //    get
        //    {
        //        if (_PasteExcelFieldCommand == null)
        //            _PasteExcelFieldCommand = new BaseCommand(param => OnPasteExcelField());
        //        return _PasteExcelFieldCommand;
        //    }
        //}

        public void OnPasteExcelField(string ColumnNo, string ColumnName)
        {
            if (SelectedExcelField != null)
            {
                SelectedExcelField.ExcelColumnName = ColumnName;
                SelectedExcelField.ExcelColumnNo = ColumnNo;
            }
        }

        public void OnClearExcelField()
        {
            if (SelectedExcelField != null)
            {
                SelectedExcelField.ExcelColumnName = string.Empty;
            }
        }

        private bool errorInDownload;
        private AutoResetEvent autoResetEvent;

        public string DownloadExcelFile(string ExcelPath)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                FileUtility ObjDownload = FileUtility.CreateClient(
                    WebDevPath.GetWebDevPath(RoleManager.WebDavPath).URL,
                    WebDevPath.GetWebDevPath(RoleManager.WebDavPath).UserName,
                    WebDevPath.GetWebDevPath(RoleManager.WebDavPath).Password,
                    WebDevPath.GetWebDevPath(RoleManager.WebDavPath).DomainName);

                string localPath = Path.Combine(System.IO.Path.GetTempPath(), ExcelPath);
                string RemotePath = "/UploadBatch/" + ExcelPath;

                autoResetEvent = new AutoResetEvent(false);
                ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
                ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
                ObjDownload.Download(RemotePath, localPath);
                autoResetEvent.WaitOne(TimeSpan.FromMinutes(3));

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                if (errorInDownload)
                    return string.Empty;
                else
                    return localPath;
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        public void LoadData(VMConfigrationManager vmConfigData)
        {
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              PayorTemplateClient payorTempClient = new PayorTemplateClient();
              PayorTemplate = payorTempClient.getPayorTemplate(vmConfigData.SelectedDisplayPayor.PayorID);

              if (ExcelFields == null)
              {
                ObservableCollection<DataEntryField> fields = serviceClients.DeuClient.GetDeuFields(Guid.Empty);
                ExcelFields = new ObservableCollection<ExtendedDataEntryField>();
                fields.ToList().ForEach(s => ExcelFields.Add(new ExtendedDataEntryField(s)));
              }

              if (PayorTemplate != null && PayorTemplate.MappedFields != null)
              {
                foreach (MappedField field in PayorTemplate.MappedFields)
                {
                  ExtendedDataEntryField extField =
                      ExcelFields.FirstOrDefault(s => s.EntryField.DeuFieldName == field.DBField);
                  if (extField != null)
                  {
                    extField.ExcelColumnName = field.ExcelFieldName;
                    extField.ExcelColumnNo = field.ExcelField;
                    extField.ExcelFormat = field.Format;
                  }
                }
              }

              if (vmConfigData.SelectedPayor.IssuedFiles != null)
              {
                IssuedFiles = new List<IssueFile>();
                vmConfigData.SelectedPayor.IssuedFiles.ToList().ForEach(s => IssuedFiles.Add(new IssueFile(s)));

                if (IssuedFiles != null && IssuedFiles.Count != 0)
                  SelectedIssuedFile = IssuedFiles.FirstOrDefault();
                else
                  SelectedIssuedFile = null;

                CurrentPayor = vmConfigData.SelectedPayor;
              }

              if (ReloadData != null)
                ReloadData(null, null);
            }
          }
          catch (Exception)
          {

          }
        }

        void ObjDownload_ErrorOccured(Exception error)
        {
            try
            {
                errorInDownload = true;
                autoResetEvent.Set();
            }
            catch(Exception)
            {
                
            }
        }

        void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
        {
            try
            {
                if (statusCode.ToString().StartsWith("20"))
                {
                    errorInDownload = false;
                    autoResetEvent.Set();
                }
                else
                {
                    errorInDownload = true;
                    autoResetEvent.Set();
                }
            }
            catch(Exception)
            {
                
            }
        }

        public event EventHandler ReloadData;
        public event EventHandler FileDownloaded;
    }
}
