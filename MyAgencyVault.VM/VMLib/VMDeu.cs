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
using System.Diagnostics;
using MyAgencyVault.VM.CommonItems;
using System.Threading;
using System.IO;
using MyAgencyVault.VM.VMLib;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Threading;
using System.Collections;
using System.Transactions;
using MyAgencyVault.VM.VMLib.DEU;
using System.Text.RegularExpressions;
using MyAgencyVault.EmailFax;
//using Microsoft.Office;
using System.Net.Mail;
using System.Net;

namespace MyAgencyVault.ViewModel
{
    public class VMDeu : BaseViewModel, IDataRefresh
    {
        WebDevPath webDevPath = null;

        private string AssignPaymentTextFormat = "Commissions Posted For Statement #{0} :";
        static MastersClient objLog = new MastersClient();
        private string strClientName = string.Empty;
        private string strinsuredName = string.Empty;

        private DateTime? _statementdate;
        public DateTime? StatementDate
        {
            get { return _statementdate; }
            set { _statementdate = value; }
        }

        private string _AssignPaymentText;
        public string AssignPaymentText
        {
            get { return _AssignPaymentText; }
            set { _AssignPaymentText = value; OnPropertyChanged("AssignPaymentText"); }
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
        private string _DeuEntryEditText = "Start Edit";
        public string DeuEntryEditText
        {
            get { return _DeuEntryEditText; }
            set { _DeuEntryEditText = value; OnPropertyChanged("DeuEntryEditText"); }
        }

        private bool _RefreshRequired = false;
        public bool RefreshRequired
        {
            get { return _RefreshRequired; }
            set { _RefreshRequired = value; }
        }

        private bool _IsFocusToFirstField;
        public bool IsFocusToFirstField
        {
            get { return _IsFocusToFirstField; }
            set
            {
                _IsFocusToFirstField = value;
                OnPropertyChanged("IsFocusToFirstField");
            }
        }

        private string _ToolTipError;
        public string ToolTipError
        {
            get { return _ToolTipError; }
            set
            {
                _ToolTipError = value;
                OnPropertyChanged("ToolTipError");
            }
        }

        private string _txtPagesNumber;
        public string txtPagesNumber
        {
            get { return _txtPagesNumber; }
            set
            {
                _txtPagesNumber = value;
                OnPropertyChanged("txtPagesNumber");
            }
        }

        private AutoResetEvent autoResetEvent;
        private WebDevPath ObjWebDevPath;
        private Canvas ImageCanvas;
        public DeuFormBuilder DeuFormBuilder;
        private string MailFormat = @"mailto:{0}";


        private bool InEditableMode = false;
        private bool InDeleteMode = false;
        private Guid DeuEntryId = Guid.Empty;

        private int? _StatementNumber;
        public int? StatementNumber
        {
            get { return _StatementNumber; }
            set { _StatementNumber = value; OnPropertyChanged("StatementNumber"); }
        }

        private decimal _AdjAmount;
        public decimal AdjAmount
        {
            get { return _AdjAmount; }
            set
            {
                _AdjAmount = value;
                if (CurrentStatement != null)
                    CurrentStatement.BalanceForOrAdjustment = _AdjAmount;
                NetAmount = CheckAmount - AdjAmount;
                OnPropertyChanged("AdjAmount");
            }
        }

        private decimal _NetAmount;
        public decimal NetAmount
        {
            get { return _NetAmount; }
            set
            {
                _NetAmount = value;
                if (CurrentStatement != null)
                    CurrentStatement.NetAmount = _NetAmount;
                OnPropertyChanged("NetAmount");
            }
        }

        private decimal _CheckAmount;
        public decimal CheckAmount
        {
            get { return _CheckAmount; }
            set
            {
                _CheckAmount = value;
                if (CurrentStatement != null)
                    CurrentStatement.CheckAmount = _CheckAmount;
                NetAmount = CheckAmount - AdjAmount;
                OnPropertyChanged("CheckAmount");
            }
        }

        private bool _EnableEditDelete;
        public bool EnableEditDelete
        {
            get { return _EnableEditDelete; }
            set
            {
                _EnableEditDelete = value;

                OnPropertyChanged("EnableEditDelete");
            }
        }

        private bool _isEnableCanvas;
        public bool isEnableCanvas
        {
            get { return _isEnableCanvas; }
            set
            {
                _isEnableCanvas = value;

                OnPropertyChanged("isEnableCanvas");
            }
        }

        private bool _isEnablePostWrap;
        public bool isEnablePostWrap
        {
            get { return _isEnablePostWrap; }
            set
            {
                _isEnablePostWrap = value;

                OnPropertyChanged("isEnablePostWrap");
            }
        }

        private int ReferenceNo = 1;
        public readonly Dispatcher dispatcher = null;

        public VMDeu(Canvas canvas)
        {
            EnableEditDelete = true;
            isEnableCanvas = true;
            isEnablePostWrap = true;

            ImageCanvas = canvas;
            dispatcher = Dispatcher.CurrentDispatcher;
            DeuFormBuilder = DeuFormBuilder.getDeuFormBuilder(this);
            DeuFormBuilder.ImageCanvas = ImageCanvas;
            string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
            ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);

            this.PropertyChanged += new PropertyChangedEventHandler(VMDeu_PropertyChanged);
            NavigationMailAddress = serviceClients.SendMailClient.GetAlertCommissionDepartmentMailId();

            serviceClients.PostUtilClient.DeuPostStartWrapperCompleted += new EventHandler<DeuPostStartWrapperCompletedEventArgs>(PostUtilClient_DeuPostStartWrapperCompleted);
            LoadData();

        }

        private void LoadData()
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                Batch PreviousBatch = CurrentBatch;
                Batches = serviceClients.BatchClient.GetBatchesForDeuEntries();
                Batches = new ObservableCollection<Batch>(Batches.OrderBy(s => (int)s.BatchNumber).ToList());

                if (Batches.Count != 0)
                {
                    if (PreviousBatch != null)
                    {
                        CurrentBatch = Batches.Where(bt => bt.BatchId == PreviousBatch.BatchId).FirstOrDefault() ?? Batches[0];
                    }
                    else
                    {
                        CurrentBatch = Batches[0];
                    }
                }

                if (PayorToolCollection != null)
                    PayorToolCollection.Clear();

                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.All };
                Payors = serviceClients.DisplayedPayorClient.GetDisplayPayors(Guid.Empty, fillInfo);

                if (Payors != null)
                    Payors = new ObservableCollection<DisplayedPayor>(Payors.OrderBy(p => p.PayorName));

                if (Payors.Count != 0)
                {
                    if (CurrentStatement != null)
                        CurrentPayor = Payors.Where(s => s.PayorID == CurrentStatement.PayorId).FirstOrDefault();// Payors.FirstOrDefault(s => s.PayorID == CurrentStatement.PayorId);
                    else
                        CurrentPayor = Payors.FirstOrDefault();
                }
                else
                    CurrentPayor = null;

                if (CurrentStatement != null)
                {
                    if (CurrentStatement.DeuEntries != null)
                    {
                        CurrentStatement.DeuEntries = new ObservableCollection<ExposedDEU>(CurrentStatement.DeuEntries.OrderBy(s => s.EntryDate));
                    }

                }

            }
            catch (Exception ex)
            {
                objLog.AddLog("LoadData exception: " + ex.Message);
            }
        }

        void VMDeu_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "CurrentPayor":
                        if (CurrentPayor != null)
                        {
                            if (PayorToolCollection == null)
                                PayorToolCollection = new List<VM.MyAgencyVaultSvc.PayorTool>();

                            //Load Payor Template
                            if (CurrentPayor != null)
                                LoadPayorTempalate(CurrentPayor.PayorID);

                            PayorTool TmpPayorTool = PayorToolCollection.Find(s => s.PayorID == CurrentPayor.PayorID);
                            if (TmpPayorTool != null)
                            {
                                PayorTool = TmpPayorTool;
                            }
                            else
                            {
                                PayorTool = serviceClients.PayorToolClient.GetPayorToolMgr(CurrentPayor.PayorID);
                                if (PayorTool != null)
                                {
                                    PayorToolCollection.Add(PayorTool);
                                    GetImageFromSever(PayorTool);
                                }
                            }

                            if (PayorTool != null)
                            {
                                DeuFormBuilder.PayorTool = PayorTool;
                                ImagePath = PayorTool.StatementImageFilePath;
                                CreatePrimaryKey(PayorTool.ToolFields);
                            }
                            else
                            {
                                DeuFormBuilder.PayorTool = PayorTool;
                                ImagePath = string.Empty;
                                PrimaryKey = string.Empty;
                            }

                            if (CurrentStatement != null && (CurrentStatement.DeuEntries == null || CurrentStatement.DeuEntries.Count == 0))
                                CurrentStatement.PayorId = CurrentPayor.PayorID;
                        }
                        break;
                    case "CurrentBatch":
                        if (CurrentBatch != null)
                        {
                            if (isReturnCurrent == true)
                            {
                                isReturnCurrent = false;
                                return;
                            }

                            Statement previousStatement = CurrentStatement;
                            BatchNumber = _CurrentBatch.BatchNumber;
                            CurrentBatch.BatchStatements = serviceClients.BatchClient.GetBatchStatementList(_CurrentBatch);
                            CurrentBatch.BatchStatements = new ObservableCollection<Statement>(CurrentBatch.BatchStatements.OrderBy(s => (int)s.StatementNumber).ToList());
                            if (CurrentBatch.BatchStatements != null && CurrentBatch.BatchStatements.Count != 0)
                            {
                                if (previousStatement != null)
                                {
                                    CurrentStatement = CurrentBatch.BatchStatements.Where(bs => bs.StatementID == previousStatement.StatementID).FirstOrDefault() ?? CurrentBatch.BatchStatements[0];
                                }
                                else
                                {
                                    CurrentStatement = CurrentBatch.BatchStatements[0];
                                }
                            }
                            else
                            {
                                CurrentStatement = null;
                            }

                            if (CurrentStatement != null)
                            {
                                if (CurrentStatement.PayorId != null)
                                {   //Crash when payor is null
                                    if (Payors != null)
                                    {
                                        CurrentPayor = Payors.FirstOrDefault(s => s.PayorID == CurrentStatement.PayorId);
                                    }
                                }
                            }

                        }
                        break;
                    case "CurrentStatement":

                        if (CurrentStatement != null)
                        {
                            StatementDate = CurrentStatement.StatementDate;
                            if (isReturnStatement == true)
                            {
                                isReturnStatement = false;
                                return;
                            }

                            StatementNumber = CurrentStatement.StatementNumber;
                            if (string.IsNullOrEmpty(CurrentStatement.FromPage) && string.IsNullOrEmpty(CurrentStatement.ToPage))
                            {
                                txtPagesNumber = string.Empty;
                            }
                            else
                            {
                                txtPagesNumber = CurrentStatement.FromPage + "-" + CurrentStatement.ToPage;
                            }

                            CurrentStatement.DeuEntries = serviceClients.DeuClient.GetDeuEntriesForStatement(CurrentStatement.StatementID);
                            CheckAmount = CurrentStatement.CheckAmount ?? 0;
                            AdjAmount = CurrentStatement.BalanceForOrAdjustment ?? 0;

                            // Code Comment by Ankit on 07-05-2020 for getting value of batch from database not based on deu entries

                            //decimal? dbEntereAmoount = 0;
                            //foreach (var item in CurrentStatement.DeuEntries)
                            //{
                            //    dbEntereAmoount = dbEntereAmoount + item.CommissionTotal;
                            //}
                            //CurrentStatement.EnteredAmount = Convert.ToDecimal(dbEntereAmoount);

                            Statement details = serviceClients.StatementClient.GetFindStatement(CurrentStatement.StatementNumber);
                            CurrentStatement.EnteredAmount = details.EnteredAmount;

                            decimal dbNetAmount = Convert.ToDecimal(CheckAmount) - Convert.ToDecimal(AdjAmount);
                            if (dbNetAmount != 0)
                            {
                                //double dbValue = Convert.ToDouble(dbNetAmount - CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                                double dbValue = Convert.ToDouble(CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                                CurrentStatement.CompletePercentage = dbValue * 100;
                            }
                            else
                                CurrentStatement.CompletePercentage = 0;

                            //sorting by created date
                            CurrentStatement.DeuEntries = new ObservableCollection<ExposedDEU>(CurrentStatement.DeuEntries.OrderBy(s => s.EntryDate));

                            if (CurrentStatement.DeuEntries != null && CurrentStatement.DeuEntries.Count != 0)
                            {
                                CurrentDeuEntry = CurrentStatement.DeuEntries[0];
                            }

                            if (CurrentStatement.PayorId != null && (CurrentPayor == null || CurrentPayor.PayorID != CurrentStatement.PayorId) && Payors != null)
                            {
                                CurrentPayor = Payors.FirstOrDefault(s => s.PayorID == CurrentStatement.PayorId);
                            }

                            if (CurrentStatement.PayorId != null)
                            {
                                if (CurrentStatement.TemplateID != null)
                                {
                                    if (PayorTemplate != null)
                                    {
                                        SelectedPayortempalate = PayorTemplate.Where(t => t.TemplateID == CurrentStatement.TemplateID).FirstOrDefault();
                                        if (SelectedPayortempalate == null)
                                        {
                                            //MessageBox.Show("Payor form is not available");
                                            PayorTool temPayorTool = null;
                                            DeuFormBuilder.PayorTool = temPayorTool;
                                            ImagePath = string.Empty;
                                            PrimaryKey = string.Empty;
                                        }
                                    }
                                }
                                else
                                {
                                    if (PayorTemplate != null)
                                        SelectedPayortempalate = PayorTemplate.FirstOrDefault();
                                }
                            }

                            AssignPaymentText = string.Format(AssignPaymentTextFormat, CurrentStatement.StatementNumber);
                        }
                        else
                        {
                            StatementNumber = null;
                            CheckAmount = 0;
                            AdjAmount = 0;
                            AssignPaymentText = string.Format(AssignPaymentTextFormat, string.Empty);
                        }

                        break;

                    case "SelectedPayortempalate":

                        if (CurrentPayor != null)
                        {
                            PayorTool TmpPayorTool = new PayorTool();
                            if (SelectedPayortempalate.TemplateID == null)
                            {
                                PayorTool = serviceClients.PayorToolClient.GetPayorToolMgr(CurrentPayor.PayorID);
                            }
                            else
                            {
                                PayorTool = serviceClients.PayorToolClient.GetPayorToolMgrWithTemplate(CurrentPayor.PayorID, SelectedPayortempalate.TemplateID);
                            }
                            if (PayorTool != null)
                            {
                                PayorToolCollection.Add(PayorTool);
                                GetImageFromSever(PayorTool);
                            }

                            if (PayorTool != null)
                            {
                                DeuFormBuilder.PayorTool = PayorTool;
                                ImagePath = PayorTool.StatementImageFilePath;
                                CreatePrimaryKey(PayorTool.ToolFields);
                            }
                            else
                            {
                                DeuFormBuilder.PayorTool = PayorTool;
                                ImagePath = string.Empty;
                                PrimaryKey = string.Empty;
                            }
                        }

                        break;

                    default:
                        break;
                }
            }
            catch
            {
            }
        }

        private List<Tempalate> _PayorTemplate;
        public List<Tempalate> PayorTemplate
        {
            get
            {
                if (_PayorTemplate != null)
                    return _PayorTemplate;
                return null;
            }
            set
            {
                _PayorTemplate = value;
                OnPropertyChanged("PayorTemplate");
            }
        }

        private Tempalate _SelectedPayortempalate = null;
        public Tempalate SelectedPayortempalate
        {
            get
            {
                return _SelectedPayortempalate;
            }
            set
            {
                _SelectedPayortempalate = value;
                if (_SelectedPayortempalate != null)
                {
                    OnPropertyChanged("SelectedPayortempalate");

                }
            }
        }

        private void LoadPayorTempalate(Guid PayorID)
        {
            if (PayorID != null)
            {
                PayorTemplate = new List<Tempalate>(serviceClients.PayorToolClient.GetPayorToolTemplate(PayorID)).ToList();
            }

            if (PayorTemplate.Count > 0)
            {
                SelectedPayortempalate = PayorTemplate.FirstOrDefault();
            }
            else
            {
                SelectedPayortempalate = null;
            }
        }

        #region Navigation Mail Address

        private string _NavigationMailAddress = @"mailto:gaurav.pra@gmail.com";
        public string NavigationMailAddress
        {
            get { return _NavigationMailAddress; }
            set
            {
                _NavigationMailAddress = string.Format(MailFormat, value);
                OnPropertyChanged("NavigationMailAddress");
            }
        }

        private ICommand _CmdAlertCommisionDept;
        public ICommand CmdAlertCommisionDept
        {
            get
            {
                if (_CmdAlertCommisionDept == null)
                {
                    _CmdAlertCommisionDept = new BaseCommand(p => OnAlertCommisionDept());
                }
                return _CmdAlertCommisionDept;
            }
        }

        public void ResetStatementDate(DateTime? dt)
        {
            if (CurrentStatement != null)
            {
                CurrentStatement.StatementDate = dt;
            }
        }
        public void SaveStatementDate(DateTime? dt)
        {
            try
            {
                if (CurrentStatement != null)
                {
                    if (objLog != null)
                    {
                        objLog.AddLog("SaveStatementDate request from app: ID - " + CurrentStatement.StatementID + ", Date - " + dt);
                    }
                    serviceClients.StatementClient.UpdateStatementDate(CurrentStatement.StatementID, (DateTime)dt);
                    StatementDate = CurrentStatement.StatementDate = dt;

                }
                else
                {
                    if (objLog != null)
                    {
                        objLog.AddLog("SaveStatementDate request from app: currentStatement found null");
                    }
                }
            }
            catch (Exception ex)
            {
                if (objLog != null)
                {
                    objLog.AddLog("SaveStatementDate exception: ID - " + CurrentStatement.StatementID + ex.Message);
                }
            }
        }

        private void OnAlertCommisionDept()
        {
            //MyAgencyVault.EmailFax.MailData
            MailData _MailData = new MailData();

            try
            {

                _MailData.ToMail = "alert@commissionsdept.com";

                Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();

                Microsoft.Office.Interop.Outlook.MailItem message = (Microsoft.Office.Interop.Outlook.MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                //Microsoft.Office.Interop.Outlook.Recipient oRecip;
                message.Subject = "DEU Alert";
                //Get Mail body and set to HTML Body
                message.HTMLBody = MailBody();

                message.To = "alert@commissionsdept.com";

                message.Display(true);

                //oRecip = (Microsoft.Office.Interop.Outlook.Recipient)message.Recipients.Add(_MailData.ToMail);

                //oRecip.Resolve();

                //oRecip = null;
                message = null;
                oApp = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private string MailBody()
        {
            string MailBody = string.Empty;
            string strPolicy = string.Empty;
            string strClient = string.Empty;
            try
            {
                if (CurrentDeuEntry != null)
                {
                    if (CurrentDeuEntry.PolicyNumber != null)
                        strPolicy = CurrentDeuEntry.PolicyNumber.ToString();

                    if (CurrentDeuEntry.ClientName != null)
                        strClient = CurrentDeuEntry.ClientName.ToString();
                    else
                        strClient = strPolicy;
                }

                if (CurrentPayor == null || CurrentStatement == null)
                {
                    return MailBody;
                }

                MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                                   "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>" +
                                   "</td></tr><tr><td colspan='2'>" + "&nbsp;</td></tr><tr><td colspan='2'>" +
                                   "<tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'></td></tr><tr><td colspan='2'>" +
                                   "&nbsp;</td></tr><tr><td colspan='2'></td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>" +
                                   "</td></tr><tr><td colspan='2'>" +
                                   "</b></span></td></tr><tr>" +
                                   "<td colspan='2'>Payor:<span style='padding-left: 50px'>" + CurrentPayor.PayorName.ToString() +
                                   "</span></td></tr><tr><td colspan='2'>Statement Date: " + CurrentStatement.StatementDate.ToString() +
                                   "<span style='padding-left: 50px'>" +
                                   "</span></td></tr><tr><td colspan='2'>Batch#: <span style='padding-left: 50px'>" + CurrentBatch.BatchNumber.ToString() +
                                   "<br/><tr><td colspan='2'>Statement#: " + CurrentStatement.StatementNumber.ToString() +
                                   "</td></tr><tr><td colspan='2'>Check amount: " + CurrentStatement.CheckAmount.ToString() +

                                   "</td></tr><tr><td colspan='2'>Bal/Adj: " + CurrentStatement.BalanceForOrAdjustment.ToString() +

                                   "</td></tr><tr><td colspan='2'>Net Check: " + CurrentStatement.NetAmount.ToString() +

                                   "</td></tr><tr><td colspan='2'>Entered: " + CurrentStatement.EnteredAmount.ToString() +

                                   "</td></tr><tr><td colspan='2'>Primary key: " + PrimaryKey.ToString() +

                                   "</td></tr><tr><td colspan='2'>Policy number : " + strPolicy +

                                   "</td></tr><tr><td colspan='2'>Client: " + strClient +

                                   "</td></tr></table>";


            }
            catch
            {
            }
            return MailBody;
        }

        #endregion

        #region PayorTool

        private PayorTool _PayorTool;
        public PayorTool PayorTool
        {
            get { return _PayorTool; }
            set
            {
                _PayorTool = value;
                OnPropertyChanged("PayorTool");
            }
        }

        private List<PayorTool> _PayorToolCollection;
        public List<PayorTool> PayorToolCollection
        {
            get { return _PayorToolCollection; }
            set
            {
                _PayorToolCollection = value;
                OnPropertyChanged("PayorToolCollection");
            }
        }

        #endregion

        #region Payor

        private ObservableCollection<DisplayedPayor> _Payors;
        public ObservableCollection<DisplayedPayor> Payors
        {
            get { return _Payors; }
            set { _Payors = value; OnPropertyChanged("Payors"); }
        }

        private DisplayedPayor _CurrentPayor;
        public DisplayedPayor CurrentPayor
        {
            get { return _CurrentPayor; }
            set
            {
                _CurrentPayor = value;
                OnPropertyChanged("CurrentPayor");

            }
        }

        #endregion

        #region Image

        private string _ImagePath;
        public string ImagePath
        {
            get { return _ImagePath; }
            set
            {
                _ImagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        #endregion

        #region Batch and Statement

        private ObservableCollection<Batch> _BatchList;
        public ObservableCollection<Batch> Batches
        {
            get
            {
                return _BatchList;
            }
            set
            {
                _BatchList = value;
                OnPropertyChanged("Batches");
            }

        }

        //private ObservableCollection<Statement> _Statementlist;
        //public ObservableCollection<Statement> Statementlist
        //{
        //    get
        //    {
        //        return _Statementlist;
        //    }
        //    set
        //    {
        //        _Statementlist = value;
        //        OnPropertyChanged("Statementlist");
        //    }
        //}

        private Batch _CurrentBatch;
        public Batch CurrentBatch
        {
            get
            {
                return _CurrentBatch;
            }
            set
            {
                _CurrentBatch = value;
                OnPropertyChanged("CurrentBatch");
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

        #endregion

        #region Policy Searched

        private ObservableCollection<DeuSearchedPolicy> _SearchedPolicy;
        public ObservableCollection<DeuSearchedPolicy> SearchedPolicy
        {
            get
            {
                return _SearchedPolicy;
            }
            set
            {
                _SearchedPolicy = value;
                OnPropertyChanged("SearchedPolicy");
            }
        }

        #endregion

        #region Commission Entry

        //private ObservableCollection<ExposedDEU> _DeuEntries;
        //public ObservableCollection<ExposedDEU> DeuEntries
        //{
        //    get { return _DeuEntries; }
        //    set { _DeuEntries = value; OnPropertyChanged("DeuEntries"); }
        //}

        private ExposedDEU _CurrentDeuEntry;
        public ExposedDEU CurrentDeuEntry
        {
            get { return _CurrentDeuEntry; }
            set
            {
                _CurrentDeuEntry = value;
                InEditableMode = false;
                OnPropertyChanged("CurrentDeuEntry");
            }
        }


        #endregion

        #region Other Properties

        private string _PrimaryKey;
        public string PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;
                OnPropertyChanged("PrimaryKey");
            }

        }

        private int? _BatchNumber;
        public int? BatchNumber
        {
            get { return _BatchNumber; }
            set { _BatchNumber = value; OnPropertyChanged("BatchNumber"); }
        }

        #endregion

        #region Command

        private ICommand _EditDeuEntry;
        public ICommand EditDeuEntry
        {
            get
            {
                if (_EditDeuEntry == null)
                {
                    _EditDeuEntry = new BaseCommand(p => OnEditDeuEntry());
                }
                return _EditDeuEntry;
            }
        }

        private void OnEditDeuEntry()
        {

            if (CurrentDeuEntry != null)
            {
                if (DeuEntryEditText == "Start Edit")
                {
                    InEditableMode = true;
                    DeuEntryEditText = "End Edit";

                    DeuEntryId = CurrentDeuEntry.DEUENtryID;
                    DeuFormBuilder.setDeuFormFieldsValue(DeuEntryId);
                }
                else
                {
                    InEditableMode = false;
                    DeuEntryEditText = "Start Edit";
                    SearchedPolicy = null;
                    DeuFormBuilder.ResetFields();
                }
            }
        }

        private int Getindex(Guid entryID)
        {
            int intIndex = 0;
            int i = 0;

            CurrentStatement.DeuEntries = new ObservableCollection<ExposedDEU>(CurrentStatement.DeuEntries.OrderBy(s => s.EntryDate));

            foreach (var item in CurrentStatement.DeuEntries)
            {
                if (item.DEUENtryID == entryID)
                {
                    intIndex = i;
                    break;
                }
                i++;
            }

            return intIndex;
        }





        private ICommand _DeleteDeuEntry;
        public ICommand DeleteDeuEntry
        {
            get
            {
                if (_DeleteDeuEntry == null)
                {
                    _DeleteDeuEntry = new BaseCommand(p => OnDeleteDeuEntry());
                }
                return _DeleteDeuEntry;
            }
        }

        private void OnDeleteDeuEntry()
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                if (CurrentStatement != null && CurrentStatement.StatusId != 2)
                {
                    if (CurrentDeuEntry != null)
                    {
                        if (MessageBox.Show("Are you sure?", "Delete Message", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            InDeleteMode = true;
                            DeuEntryId = CurrentDeuEntry.DEUENtryID;
                            string logMsg = "Manually Deleted DeuEntry - DeuEntryID : " + DeuEntryId + ",  loggedInUser: " + RoleManager.LoggedInUser;
                            objLog.AddLog(logMsg);
                            Guid Id = CurrentDeuEntry.DEUENtryID; //local variable to keep value for logs, as it is lost after post data

                            ExposedDEU exposeItem = CurrentStatement.DeuEntries.Where(p => p.DEUENtryID == DeuEntryId).FirstOrDefault();
                            OnPostData();

                            //delete deu entry id
                            //BackgroundWorker worker = new BackgroundWorker();
                            //worker.DoWork += new System.ComponentModel.DoWorkEventHandler(workerDelete_DoWork);
                            //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(workerDelete_RunWorkerCompleted);
                            //worker.RunWorkerAsync();

                            //serviceClients.DeuClient.DeleteDeuEntryByIDAsync(exposeItem.DEUENtryID);
                            //serviceClients.DeuClient.DeleteDeuEntryByID(exposeItem.DEUENtryID);


                            try
                            {
                                string mailBody = "DeuEntryID: " + Id + "\n" + "User: " + RoleManager.LoggedInUser + "\n" + "Statement: " + CurrentStatement.StatementNumber + "\n" + "Batch: " + CurrentBatch.BatchNumber;
                                serviceClients.SendMailClient.SendMailToDevAsync("jyotisna@acmeminds.com", "Manual deletion of DEU entry by " + RoleManager.LoggedInUser, mailBody);
                                objLog.AddLog("Mail sent on manual deletion of DEU entry: " + DeuEntryId);
                            }
                            catch (Exception ex)
                            {
                                if (objLog == null)
                                {
                                    objLog = new MastersClient();
                                }
                                objLog.AddLog("Exception sending mail on manual deletion of DEU entry: " + DeuEntryId);
                            }
                            if (exposeItem != null)
                            {
                                CurrentStatement.DeuEntries.Remove(exposeItem);
                            }

                            bindHelpText = string.Empty;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Post entry can not be deleted because statement is closed.", "Statement Closed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                objLog.AddLog(" Exception deleting deuEntryID  : " + DeuEntryId + ", Message: " + ex.Message);
            }
        }

        private ICommand _BatchClose;
        public ICommand BatchClose
        {
            get
            {
                if (_BatchClose == null)
                {
                    _BatchClose = new BaseCommand(p => OnCloseBatch());
                }
                return _BatchClose;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void OnCloseBatch()
        {
            try
            {

                if (BatchNumber == null)
                {
                    MessageBox.Show("Please enter batch to close");
                    return;
                }
                bool IsBatchClosed = false;
                if (CurrentBatch.BatchStatements.Count != 0)
                {
                    int openStatement = CurrentBatch.BatchStatements.Count(s => (s.StatusId == 1 || s.StatusId == 0));

                    string strCompleted = Convert.ToString(CurrentBatch.LastModifiedDate);

                    int intCount = 0;

                    foreach (var item in CurrentBatch.BatchStatements)
                    {
                        intCount = intCount + item.Entries;
                    }

                    if (openStatement == 0)
                    {
                        IsBatchClosed = serviceClients.BatchClient.CloseBatch(CurrentBatch);
                        if (IsBatchClosed)
                        {
                            CurrentBatch.EntryStatus = EntryStatus.BatchCompleted;

                            SendMailToCloseBatch(Convert.ToString(CurrentBatch.BatchNumber), Convert.ToString(CurrentBatch.BatchStatements.Count), Convert.ToString(CurrentBatch.AssignedDeuUserName), Convert.ToString(CurrentBatch.CreatedDate), System.DateTime.Now.ToString(), intCount);

                            Batches.Remove(CurrentBatch);
                            if (Batches.Count != 0)
                                CurrentBatch = Batches.FirstOrDefault();
                            else
                            {
                                CurrentBatch = null;
                                CurrentStatement = null;
                            }
                        }
                    }
                }

                if (!IsBatchClosed)
                {
                    MessageBox.Show("Batch can not be closed.", "Batch", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch
            {
            }
        }

        private void SendMailToCloseBatch(string strBatchNumber, string strTotalStatement, string strEntryUser, string strEntrySatrdate, string strCompleted, int intTotalEntry)
        {
            MailData _MailData = new MailData();
            try
            {
                _MailData.ToMail = "data@commissionsdept.com";
                _MailData.FromMail = "data@commissionsdept.com";
                //mailMessage.Subject = "Batch Closed: " + strBatchNaumber;
                // serviceClients.FollowupIssueClient.SendMailToCloseBatchAsync(_MailData, strBatchNumber, MailContent(strBatchNumber, strTotalStatement, strEntryUser, strEntrySatrdate, System.DateTime.Now.ToString(), intTotalEntry));
                string strSubject = "Batch Closed: " + strBatchNumber;
                serviceClients.FollowupIssueClient.SendNotificationMailAsync(_MailData, strSubject, MailContent(strBatchNumber, strTotalStatement, strEntryUser, strEntrySatrdate, System.DateTime.Now.ToString(), intTotalEntry));
            }
            catch
            {
            }
        }

        private string MailContent(string strBatchNumber, string strTotalStatement, string strEntryUser, string strEntrySatrdate, string strCompleted, int intTotalEntry)
        {
            string MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                       "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>licensee: " +
                    CurrentBatch.LicenseeName +
                    "</td></tr><tr><td colspan='2'>Batch no: " +
                   strBatchNumber +
                       "</td></tr><tr><td colspan='2'>Statements: " +
                   strTotalStatement +
                   "</td></tr><tr><td colspan='2'>DEU:" +
                   strEntryUser +
                   "</td></tr><tr><td colspan='2'>Started: " +
                   strEntrySatrdate +
                       "</td></tr><tr><td colspan='2'>Completed: " +
                   strCompleted +
                  "</td></tr><tr><td colspan='2'>Entries: " +
                   intTotalEntry +
                   "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                   + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>"
                   + "</td></tr></table>";


            return MailBody;

        }


        private ICommand _ReopenBatch;
        public ICommand ReopenBatch
        {
            get
            {
                if (_ReopenBatch == null)
                {
                    _ReopenBatch = new BaseCommand(p => beforeReopenBatch(), p => OpenReopenBatch());
                }
                return _ReopenBatch;
            }
        }


        private bool beforeReopenBatch()
        {
            if (BatchNumber != null)
                return true;
            else
                return false;
        }

        private void OpenReopenBatch()
        {
            if (BatchNumber != null)
            {
                CurrentBatch = (from p1 in Batches where p1.BatchNumber.ToString().Contains(BatchNumber.ToString()) select p1).FirstOrDefault();

                if (CurrentBatch == null)
                {
                    MessageBoxResult result = MessageBox.Show("Do you want to Re-open the Batch:" + BatchNumber, "MyAgencyVault", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        bool IsBatchOpened = serviceClients.BatchClient.OpenBatch(Convert.ToInt32(BatchNumber));
                        if (IsBatchOpened)
                        {
                            LoadData();
                        }
                    }
                }
                else
                    MessageBox.Show("Batches is already open", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Please enter batch number to open", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private ICommand _BatchFind;
        public ICommand BatchFind
        {
            get
            {
                if (_BatchFind == null)
                {
                    _BatchFind = new BaseCommand(p => OnBatchFind());

                }
                return _BatchFind;
            }

        }

        private void OnBatchFind()
        {
            if (BatchNumber != null)
            {
                selectedScreenIndex = 0;
                CurrentBatch = (from p1 in Batches where p1.BatchNumber.ToString().Contains(BatchNumber.ToString()) select p1).FirstOrDefault();
                if (CurrentBatch == null)
                    MessageBox.Show("Entered batch not found.");

            }
            else
                MessageBox.Show("Please enter batch to find.");

        }


        private ICommand _CheckAmountFocusLost;
        public ICommand CheckAmountFocusLost
        {
            get
            {
                if (_CheckAmountFocusLost == null)
                {
                    _CheckAmountFocusLost = new BaseCommand(p => OnCheckAmountChanged());
                }
                return _CheckAmountFocusLost;
            }
        }

        private void OnCheckAmountChanged()
        {
            if (CurrentStatement != null)
            {
                ModifiableStatementData statementData = serviceClients.StatementClient.UpdateCheckAmount(CurrentStatement.StatementID, CurrentStatement.CheckAmount ?? 0, CurrentStatement.NetAmount ?? 0, CurrentStatement.BalanceForOrAdjustment ?? 0);
                LoadStatementData(statementData);
            }
        }

        private ICommand _AdjustmentFocusLost;
        public ICommand AdjustmentFocusLost
        {
            get
            {
                if (_AdjustmentFocusLost == null)
                {
                    _AdjustmentFocusLost = new BaseCommand(p => OnAdjustmentChanged());
                }
                return _AdjustmentFocusLost;
            }
        }

        private void OnAdjustmentChanged()
        {
            if (CurrentStatement != null)
            {
                //ModifiableStatementData statementData = serviceClients.StatementClient.UpdateCheckAmount(CurrentStatement.StatementID, CurrentStatement.CheckAmount ?? 0, CurrentStatement.BalanceForOrAdjustment ?? 0);
                ModifiableStatementData statementData = serviceClients.StatementClient.UpdateCheckAmount(CurrentStatement.StatementID, CurrentStatement.CheckAmount ?? 0, CurrentStatement.NetAmount ?? 0, CurrentStatement.BalanceForOrAdjustment ?? 0);
                LoadStatementData(statementData);
            }
        }

        private ICommand _BatchClear;
        public ICommand BatchClear
        {
            get
            {
                if (_BatchClear == null)
                {
                    _BatchClear = new BaseCommand(p => OnClearBatch());
                }
                return _BatchClear;
            }

        }

        private void OnClearBatch()
        {
            BatchNumber = null;

            if (CurrentBatch != null)
                CurrentBatch = null;

        }

        private ICommand _NewStatement;
        public ICommand NewStatement
        {
            get
            {
                if (_NewStatement == null)
                {
                    _NewStatement = new BaseCommand(p => OnNewStatement());
                }
                return _NewStatement;
            }

        }

        private void OnNewStatement()
        {
            try
            {
                if (CurrentBatch != null && CurrentBatch.BatchId != Guid.Empty)
                {
                    CurrentStatement = new Statement();

                    CurrentStatement.BatchId = CurrentBatch.BatchId;
                    CurrentStatement.StatementID = Guid.NewGuid();
                    CurrentStatement.StatementDate = DateTime.Now;
                    CurrentStatement.PayorId = CurrentPayor.PayorID;
                    CurrentStatement.CreatedBy = RoleManager.userCredentialID;
                    CurrentStatement.StatusId = 0;
                    CurrentStatement.CreatedDate = System.DateTime.Now;
                    CurrentStatement.LastModified = System.DateTime.Now;

                    CurrentStatement.TemplateID = SelectedPayortempalate.TemplateID;
                    CurrentStatement.FromPage = string.Empty;
                    CurrentStatement.ToPage = string.Empty;

                    StatementNumber = serviceClients.StatementClient.AddUpdateStatement(CurrentStatement);
                    CurrentStatement.StatementNumber = Convert.ToInt32(StatementNumber);

                    //CurrentStatement = serviceClients.StatementClient.GetStatement(CurrentStatement.StatementID);
                    CurrentBatch.BatchStatements.Add(CurrentStatement);

                    //Assign Entry user to newly Uploaded batch 
                    if (string.IsNullOrEmpty(CurrentBatch.AssignedDeuUserName))
                        CurrentBatch.AssignedDeuUserName = RoleManager.LoggedInUser;

                    if (DeuEntryEditText == "End Edit")
                    {
                        DeuEntryEditText = "Start Edit";
                    }
                }
                else
                {
                    MessageBox.Show("Please select batch before generating the statement for the batch.", "Select Batch", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch
            {
            }
        }

        //private ICommand _FindStatement;
        //public ICommand FindStatement
        //{
        //  get
        //  {
        //    if (_FindStatement == null)
        //    {
        //      _FindStatement = new BaseCommand(p =>
        //      {
        //        selectedScreenIndex = 1;

        //        Statement statement = null;
        //        foreach (Batch batch in Batches)
        //        {
        //          statement = batch.BatchStatements.FirstOrDefault(s => s.StatementNumber == StatementNumber);
        //          if (statement != null)
        //            break;
        //        }

        //        if (statement != null)
        //        {
        //          if (CurrentBatch.BatchId != statement.BatchId)
        //          {
        //            CurrentBatch = Batches.FirstOrDefault(s => s.BatchId == statement.BatchId);
        //            CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(StatementNumber.ToString()));
        //          }
        //          else
        //            CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(StatementNumber.ToString()));
        //        }
        //      });
        //    }
        //    return _FindStatement;
        //  }

        //}

        private ICommand _btnOpenStatus;
        public ICommand btnOpenStatus
        {
            get
            {
                if (_btnOpenStatus == null)
                {
                    _btnOpenStatus = new BaseCommand(p => beforeOpenSatus(), p => OpenStatement());
                }
                return _btnOpenStatus;
            }
        }

        private ICommand _FindStatement;
        public ICommand FindStatement
        {
            get
            {
                if (_FindStatement == null)
                {
                    _FindStatement = new BaseCommand(p => OnFindStatement());

                }
                return _FindStatement;
            }

        }
        private void OnFindStatement()
        {
            //selectedScreenIndex = 1;
            //bool bIsBatchFind = false;
            //int? selectedNumber = StatementNumber;

            //if (selectedNumber != null)
            //{
            //    Statement statement = null;
            //    foreach (Batch batch in Batches)
            //    {
            //        statement = batch.BatchStatements.FirstOrDefault(s => s.StatementNumber == StatementNumber);
            //        if (statement != null)
            //        {
            //            bIsBatchFind = true;
            //            break;
            //        }
            //    }

            //    if (bIsBatchFind)
            //    {
            //        if (statement != null)
            //        {
            //            if (CurrentBatch == null) return;

            //            if (CurrentBatch.BatchId != statement.BatchId)
            //            {
            //                CurrentBatch = Batches.FirstOrDefault(s => s.BatchId == statement.BatchId);
            //                CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(selectedNumber.ToString()));
            //            }
            //            else
            //                CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(StatementNumber.ToString()));
            //        }
            //    }

            selectedScreenIndex = 1;
            int? selectedNumber = StatementNumber;
            if (selectedNumber != null)
            {
                Statement statement = null;
                //Get Statement number
                statement = serviceClients.StatementClient.GetFindStatement(Convert.ToInt32(selectedNumber));

                if (statement != null)
                {
                    if (CurrentBatch == null) return;

                    if (CurrentBatch.BatchId != statement.BatchId)
                    {
                        CurrentBatch = Batches.FirstOrDefault(s => s.BatchId == statement.BatchId);
                        if (CurrentBatch != null)
                        {
                            if (CurrentBatch.BatchStatements != null)
                            {
                                CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(selectedNumber.ToString()));
                            }

                        }
                    }
                    else
                    {
                        if (CurrentBatch.BatchStatements != null)
                        {
                            CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(p1 => p1.StatementNumber.ToString().Contains(StatementNumber.ToString()));
                        }
                    }
                }

                else
                {
                    MessageBox.Show("Entered statement number is not found", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please enter the statement number", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool beforeOpenSatus()
        {
            if (CurrentStatement == null) return false;

            if (CurrentStatement.StatusId == 2)
                return true;
            else
                return false;
        }

        private void OpenStatement()
        {
            bool bValue = false;
            if (CurrentBatch != null)
            {
                bValue = serviceClients.BatchClient.GetBatchPaidStatus(CurrentBatch.BatchId);
            }

            if (bValue)
            {
                MessageBoxResult result = MessageBox.Show("Statement is mark paid,do you want to continue.", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // need to Querry to eric
                }

            }
            else
            {
                if (CurrentStatement != null)
                {
                    bool openSuccessfull = serviceClients.StatementClient.OpenStatement(CurrentStatement);
                    if (openSuccessfull)
                    {
                        CurrentStatement.StatusId = 1;
                        CurrentStatement = serviceClients.StatementClient.GetStatement(CurrentStatement.StatementID);

                    }
                }
            }
        }

        private ICommand _CloseNewStatement;
        public ICommand CloseNewStatement
        {
            get
            {
                if (StatementNumber != null && BatchNumber != null)
                {
                    if (StatementNumber != null)
                    {
                        if (_CloseNewStatement == null)
                        {
                            _CloseNewStatement = new BaseCommand(p =>
                            {
                                if (CurrentStatement != null)
                                {
                                    bool closeSuccessfull = serviceClients.StatementClient.CloseStatement(CurrentStatement);
                                    if (!closeSuccessfull)
                                        MessageBox.Show("This statement can not be closed because check amount is not matched with entries");
                                    else
                                    {
                                        CurrentStatement.StatusId = 2;
                                        PostStatementCloseActivity();
                                        OnNewStatement();
                                    }
                                }
                                else
                                    MessageBox.Show("Statement is not found.");
                            });

                        }
                    }

                    else
                        MessageBox.Show("Please enter statement number to closed");
                }

                return _CloseNewStatement;
            }

        }

        private ICommand _CloseStatement;
        public ICommand CloseStatement
        {
            get
            {
                if (_CloseStatement == null)
                {
                    _CloseStatement = new BaseCommand(p => OnCloseStatement());
                }
                return _CloseStatement;
            }
        }

        private void OnCloseStatement()
        {
            if (objLog == null) objLog = new MastersClient();
            objLog.AddLog(DateTime.Now.ToString() + " OnCloseStatement entered, user: " + RoleManager.LoggedInUser);

            if (MessageBox.Show("Do you want to close statement", "MyAgencyVault", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                if (StatementNumber != null)
                {
                    //if (CurrentStatement.EnteredAmount == NetAmount)
                    //Needs to be a +/- of $1.00 for right now to allow a statement to close "requirement of eric"

                    if (Math.Abs(CurrentStatement.EnteredAmount - NetAmount) <= 1)
                    {
                        objLog.AddLog(DateTime.Now.ToString() + " OnCloseStatement StatementNumber: " + StatementNumber + ", EnteredAmount : " + CurrentStatement.EnteredAmount + ", netamiunt: " + NetAmount);
                        bool closeSuccessfull = false;
                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.StatementClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            closeSuccessfull = serviceClients.StatementClient.CloseStatement(CurrentStatement);
                        }
                        // bool closeSuccessfull = serviceClients.StatementClient.CloseStatement(CurrentStatement);

                        if (!closeSuccessfull)
                        {

                            objLog.AddLog(DateTime.Now.ToString() + " OnCloseStatement failure, calling CloseStatementFromDeu");
                            bool isClose = serviceClients.StatementClient.CloseStatementFromDeu(CurrentStatement);
                            if (!isClose)
                            {
                                MessageBox.Show("This statement can not be closed because check amount is not matched with entered amount", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                CurrentStatement.StatusId = 2;
                                PostStatementCloseActivity();
                                //Clear all fields when statement is closed
                                StatementNumber = null;
                                CheckAmount = 0;
                                CurrentStatement.StatementDate = null;
                                AdjAmount = 0;
                                NetAmount = 0;
                                CurrentStatement.EnteredAmount = 0;
                                //SendMailAutomatically(CurrentDeuEntry);
                                //clear the statement collection
                                CurrentStatement = null;
                                //Call to load active statement 
                                //CurrentBatch.BatchStatements = new ObservableCollection<Statement>(CurrentBatch.BatchStatements.Where(p => p.StatusId != 2));
                            }
                        }
                        else
                        {
                            objLog.AddLog(DateTime.Now.ToString() + " OnCloseStatement success");

                            CurrentStatement.StatusId = 2;
                            PostStatementCloseActivity();
                            //Clear all fields when statement is closed
                            StatementNumber = null;
                            CheckAmount = 0;
                            CurrentStatement.StatementDate = null;
                            AdjAmount = 0;
                            NetAmount = 0;
                            CurrentStatement.EnteredAmount = 0;
                            //SendMailAutomatically(CurrentDeuEntry);
                            //clear the statement collection
                            CurrentStatement = null;
                            //Call to load active statement 
                            //CurrentBatch.BatchStatements = new ObservableCollection<Statement>(CurrentBatch.BatchStatements.Where(p => p.StatusId != 2));
                        }
                    }
                    else
                        MessageBox.Show("This statement can not be closed because check amount is not matched with entered amount", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Please enter statement number to close", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void SendMailAutomatically(ExposedDEU statementByDeu)
        {
            bool bCarierValue = true;
            bool bCoverageValue = true;
            string strCarrierName = string.Empty;
            string strCoverageName = string.Empty;

            if (statementByDeu != null)
            {
                if (!string.IsNullOrEmpty(statementByDeu.CarrierNickName))
                { //Get  Carrier Name
                    strCarrierName = statementByDeu.CarrierNickName;
                    bCarierValue = serviceClients.CarrierClient.IsValidCarrier(strCarrierName, (Guid)CurrentStatement.PayorId);
                }

                if (!string.IsNullOrEmpty(statementByDeu.CoverageNickName))
                {
                    //Get Coverage
                    strCoverageName = statementByDeu.CoverageNickName;
                    bCoverageValue = serviceClients.CoverageClient.IsValidCoverage(strCarrierName, strCoverageName, (Guid)CurrentStatement.PayorId);
                }
            }

            //string strEntryUser = CurrentStatement.CreatedByDEU;

            MailData _MailData = new MailData();

            _MailData.CarrierName = strCarrierName;
            _MailData.Product = strCoverageName;

            try
            {
                if ((bCarierValue == false) || (bCoverageValue == false))
                {
                    ObservableCollection<LicenseeDisplayData> objCollection = new ObservableCollection<LicenseeDisplayData>();

                    objCollection = serviceClients.LicenseeClient.GetDisplayedLicenseeList(CurrentBatch.LicenseeId);
                    if (statementByDeu != null)
                    {
                        _MailData.FromMail = "data@commissionsdept.com";
                        _MailData.ToMail = "data@commissionsdept.com";
                        string strSubject = "Carrier and/or Product not found" + statementByDeu.ClientName + " Policy # " + statementByDeu.PolicyNumber;
                        serviceClients.FollowupIssueClient.SendNotificationMailAsync(_MailData, strSubject, MailBody(_MailData, statementByDeu));

                    }
                }
            }
            catch
            {
            }

        }

        //private string MailBody(MyAgencyVault.VM.MyAgencyVaultSvc.MailData EmailContentdata, ExposedDEU currentEntry)
        private string MailBody(MailData EmailContentdata, ExposedDEU currentEntry)
        {
            //string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
            //ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);

            string strLinsenseName = string.Empty;
            string strBatch = string.Empty;
            string strEntryUser = string.Empty;
            string strPayorNme = string.Empty;
            string strPolicy = string.Empty;
            string strClient = string.Empty;
            string strInvoiceDate = string.Empty;
            string strCarrier = string.Empty;
            string strProduct = string.Empty;
            //get logo path from deve server
            string strLogoPath = ObjWebDevPath.URL + "/Images/Logo.png";


            if (currentEntry.PolicyNumber != null)
                strPolicy += currentEntry.PolicyNumber + ",";
            else
                strPolicy += "NULL" + ",";

            if (currentEntry.ClientName != null)

                strClient += currentEntry.ClientName + ",";
            else

                strClient += "NULL" + ",";

            if (currentEntry.InvoiceDate != null)
            {
                DateTime dt = Convert.ToDateTime(currentEntry.InvoiceDate);
                strInvoiceDate += Convert.ToString(dt.ToShortDateString()) + ",";
            }
            else
                strInvoiceDate += "NULL" + ",";

            if (currentEntry.CarrierNickName != null)
                strCarrier += currentEntry.CarrierNickName + ",";
            else
                strCarrier += "NULL" + ",";

            if (currentEntry.CoverageNickName != null)
                strProduct += currentEntry.CoverageNickName + ",";
            else
                strProduct += "NULL" + ",";

            if (strPolicy.Length > 1)
                strPolicy = strPolicy.Substring(0, strPolicy.Length - 1);
            else
                strPolicy = string.Empty;

            if (strClient.Length > 1)
                strClient = strClient.Substring(0, strClient.Length - 1);
            else
                strClient = string.Empty;

            if (strInvoiceDate.Length > 1)
                strInvoiceDate = strInvoiceDate.Substring(0, strInvoiceDate.Length - 1);
            else
                strInvoiceDate = string.Empty;

            if (strCarrier.Length > 1)
                strCarrier = strCarrier.Substring(0, strCarrier.Length - 1);
            else
                strCarrier = string.Empty;

            if (strProduct.Length > 1)
                strProduct = strProduct.Substring(0, strProduct.Length - 1);
            else
                strProduct = string.Empty;

            strPayorNme = CurrentPayor.PayorName;
            strLinsenseName = CurrentBatch.LicenseeName;
            strBatch = Convert.ToString(CurrentBatch.BatchNumber);
            strEntryUser = CurrentBatch.AssignedDeuUserName;

            string strStatement = Convert.ToString(CurrentStatement.StatementNumber);
            string strComplete = Convert.ToString(CurrentStatement.CompletePercentage);

            string MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                       "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>Licensee: " +
                   strLinsenseName +
                       "</td></tr><tr><td colspan='2'>Batch: " +
                   strBatch +
                   "</td></tr><tr><td colspan='2'>Statement:" +
                   strStatement +
                   "</td></tr><tr><td colspan='2'>DEU: " +
                   strEntryUser +
                       "</td></tr><tr><td colspan='2'>Payor: " +
                   strPayorNme +
                       "</td></tr><tr><td colspan='2'>% Complete: " +
                   strComplete +
                   "</td></tr><tr><td>&nbsp;</td></tr><tr><td colspan='2'>Record data " +
                    "</td></tr><tr><td>&nbsp;</td></tr><tr><td colspan='2'>Policy#: " +
                   strPolicy +
                   "</td></tr><tr><td colspan='2'>Client: " +
                   strClient +
                    "</td></tr><tr><td colspan='2'>InvoiceDate: " +
                   strInvoiceDate +
                    "</td></tr><tr><td colspan='2'>Carrier nick name: " +
                   strCarrier +
                    "</td></tr><tr><td colspan='2'>Product nick name: " +
                   strProduct +
                   "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                   + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>"
                   + "</td></tr></table>";

            return MailBody;

        }

        private int _selectedScreenIndex;
        public int selectedScreenIndex
        {
            get { return _selectedScreenIndex; }
            set { _selectedScreenIndex = value; OnPropertyChanged("selectedScreenIndex"); }
        }

        private void PostStatementCloseActivity()
        {
            SearchedPolicy = null;
            DeuFormBuilder.ResetFields();
        }

        private ICommand _PostData;
        public ICommand PostData
        {
            get
            {
                if (_PostData == null)
                {
                    _PostData = new BaseCommand(p => BeforeOnPostData(), p => OnPostData());
                }
                return _PostData;
            }

        }

        public DEUFields getDeuFormFieldsValue()
        {
            DEUFields deuFields = new DEUFields();

            try
            {
                List<DataEntryField> deuFormFields = DeuFormBuilder.getDueFormFieldsValue();
                if (InEditableMode || InDeleteMode)
                    deuFields.DeuEntryId = DeuEntryId;
                else
                    deuFields.DeuEntryId = Guid.Empty;

                deuFields.BatchId = CurrentBatch.BatchId;
                deuFields.LicenseeId = CurrentBatch.LicenseeId;
                deuFields.CurrentUser = RoleManager.userCredentialID;
                deuFields.StatementId = CurrentStatement.StatementID;
                deuFields.PayorId = CurrentPayor.PayorID;
                deuFields.DeuFieldDataCollection = new ObservableCollection<DataEntryField>(deuFormFields);


            }
            catch
            {
            }

            return deuFields;
        }

        private bool BeforeOnPostData()
        {
            if (CurrentBatch != null && CurrentStatement != null)
            {
                //Following check added by Jyotisna On Aug30,2018 to restrict user from entering data into closed statement 
                if (CurrentStatement.StatusId == 2)
                {
                    ToolTipError = "Statement# " + CurrentStatement.StatementNumber + " is closed and payment entry cannot be made in closed statement.";
                    return false;
                }
                ToolTipError = null;
                //New check Ends here

                if (!DeuFormBuilder.ValidateCompSchuduleType())
                {
                    ToolTipError = "Comp schedule type cannot be greater than 75 characters.";
                    return false;
                }
                if (!string.IsNullOrEmpty(txtPagesNumber))
                {
                    string strPageNumber = string.Empty;
                    strPageNumber = txtPagesNumber;
                    if (!string.IsNullOrEmpty(strPageNumber))
                    {
                        if (strPageNumber.Contains("-"))
                        {
                            List<string> myList = new List<string>(strPageNumber.Split('-'));
                            if (!string.IsNullOrEmpty(Convert.ToString(myList[0])))
                            {
                                string strFromValue = validation(Convert.ToString(myList[0]));
                                string strToValue = validation(Convert.ToString(myList[1]));
                            }
                            else
                            {
                                string strFromValue = validation(Convert.ToString(myList[1]));
                                string strToValue = validation(Convert.ToString(myList[1]));
                            }
                        }
                        else
                        {
                            string strFromValue = validation(strPageNumber);
                            string strToValue = validation(strPageNumber);
                        }
                    }
                }

                if (DeuFormBuilder.ValidateFields())
                {
                    if (DeuFormBuilder.ValidateEffectiveFields())
                    {
                        return true;
                    }
                    else
                    {
                        ToolTipError = "Effective date cannot be greater than Invoice date.";
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        private string validation(string strNumber)
        {
            string strNumberValue = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strNumber))
                {
                    strNumberValue = string.Empty;
                }
                else if (!string.IsNullOrEmpty(strNumber))
                {
                    int intValue = Convert.ToInt32(strNumber);
                    strNumberValue = Convert.ToString(intValue);

                    if (intValue > 0 && intValue < 10000)
                        strNumberValue = Convert.ToString(intValue);
                    else
                    {
                        //MessageBox.Show("Please enter range between 1 to 9999.");
                        strNumberValue = string.Empty;
                    }


                }
            }
            catch
            {
                //MessageBox.Show("Please enter the numeric value.");
                strNumberValue = string.Empty;
            }

            return strNumberValue;
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

        private string _bindHelpText;
        public string bindHelpText
        {
            get
            {
                return _bindHelpText;
            }
            set
            {
                _bindHelpText = value;
                OnPropertyChanged("bindHelpText");
            }
        }

        private ObservableCollection<Carrier> _CarriersCount;
        public ObservableCollection<Carrier> CarriersCount
        {
            get
            {
                return _CarriersCount;
            }
            set
            {
                _CarriersCount = value;
                OnPropertyChanged("CarriersCount");
            }
        }

        //bool isCarrierPartOfprimarykey = false;
        //bool isCoveragePartOfPrimaryKey = false;
        private void OnPostData()
        {
            //IsBusy = true;
            SharedVMData.IsRefreshpolicy = false;
            EnableEditDelete = false;
            isEnableCanvas = false;
            isEnablePostWrap = false;
            try
            {
                if (CurrentPayor == null)
                {
                    MessageBox.Show("Please select payor before posting the entry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CurrentStatement != null && CurrentStatement.StatusId == 2)
                {
                    MessageBox.Show("Statement# " + CurrentStatement.StatementNumber + " is closed and payment entry cannot be made in closed statement.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CurrentStatement != null && CurrentStatement.DeuEntries != null)
                {
                    if (CurrentStatement.DeuEntries.Count > 0)
                    {
                        if (CurrentStatement.PayorId != null && CurrentStatement.PayorId != CurrentPayor.PayorID)
                        {
                            //MessageBox.Show("Statement payor is not matched with deu entry payor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            //return;
                            //bud id 2123: we should not select new when a message comes” statement payor is not matched with deu entry payor”..
                            if (InEditableMode != true)
                            {
                                StatementNumber = null;
                                OnNewStatement();
                            }
                        }
                    }
                    else
                    {
                        CurrentStatement.PayorId = CurrentPayor.PayorID;
                    }
                }

                if (InDeleteMode)
                {
                    if (CurrentDeuEntry != null)
                    {
                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PostUtilClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            serviceClients.PostUtilClient.DeuPostStartWrapperAsync(PostEntryProcess.Delete, null, CurrentDeuEntry.DEUENtryID, RoleManager.userCredentialID, RoleManager.Role);
                        }

                    }
                }
                else
                {

                    #region carrier, product validation

                    DEUFields deuFields = getDeuFormFieldsValue();

                    string carrierNickName = string.Empty, coverageNickName = string.Empty;
                    DataEntryField dueFieldData = null;
                    bool IsCarrierNickNameFound = true, IsCoverageNickNameFound = true;
                    ValidateDeuField validateDeuField = new ValidateDeuField();
                    if (DeuFormBuilder.IsCarrierPrimaryKey())
                    {
                        dueFieldData = deuFields.DeuFieldDataCollection.FirstOrDefault(s => s.DeuFieldName == "Carrier");
                        if (dueFieldData != null)
                        {
                            IsCarrierNickNameFound = validateDeuField.ValidateCarrier(dueFieldData.DeuFieldValue, PayorTool.PayorID);
                            carrierNickName = dueFieldData.DeuFieldValue;
                            //isCarrierPartOfprimarykey = true;

                        }
                    }

                    if (DeuFormBuilder.IsCoveragePrimaryKey())
                    {
                        dueFieldData = deuFields.DeuFieldDataCollection.FirstOrDefault(s => s.DeuFieldName == "Product");
                        if (dueFieldData != null)
                        {
                            IsCoverageNickNameFound = validateDeuField.ValidateCovergae(carrierNickName, dueFieldData.DeuFieldValue, PayorTool.PayorID);
                            coverageNickName = dueFieldData.DeuFieldValue;
                            //isCoveragePartOfPrimaryKey = true;
                        }
                    }

                    if (!IsCarrierNickNameFound || !IsCoverageNickNameFound)
                    {
                        //serviceClients.SendMailClient.SendMailCompleted += new EventHandler<SendMailCompletedEventArgs>(SendMailClient_SendMailCompleted);
                        if (!IsCarrierNickNameFound && !IsCoverageNickNameFound)
                        {
                            //serviceClients.SendMailClient.SendMailAsync(string.Empty, "Nickname is not found in system", "Carrier Nickname = " + carrierNickName + ",Coverage NickName= " + coverageNickName + ",Payor= " + CurrentPayor.PayorName + ",BatchNo= " + CurrentBatch.BatchNumber + ",StatementNo= " + CurrentStatement.StatementNumber);
                            //SendMailAutomatically(CurrentDeuEntry);
                        }
                        else if (!IsCarrierNickNameFound)
                        {
                            //serviceClients.SendMailClient.SendMailAsync(string.Empty, "Nickname is not found in system", "Carrier Nickname = " + carrierNickName + ",Payor= " + CurrentPayor.PayorName + ",BatchNo= " + CurrentBatch.BatchNumber + ",StatementNo= " + CurrentStatement.StatementNumber);
                            //SendMailAutomatically(CurrentDeuEntry);
                        }
                        else
                        {
                            //serviceClients.SendMailClient.SendMailAsync(string.Empty, "Nickname is not found in system", "Coverage Nickname = " + coverageNickName + ",Payor= " + CurrentPayor.PayorName + ",BatchNo= " + CurrentBatch.BatchNumber + ",StatementNo= " + CurrentStatement.StatementNumber);
                            //SendMailAutomatically(CurrentDeuEntry);
                        }
                    }
                    #endregion

                    if (CurrentStatement.DeuEntries == null)
                        CurrentStatement.DeuEntries = new ObservableCollection<ExposedDEU>();

                    if (deuFields.DeuEntryId == Guid.Empty)
                    {
                        CurrentStatement.PayorId = CurrentPayor.PayorID;
                        CurrentStatement.TemplateID = SelectedPayortempalate.TemplateID;

                        string strPageNumber = string.Empty;
                        strPageNumber = txtPagesNumber;
                        if (!string.IsNullOrEmpty(strPageNumber))
                        {
                            if (strPageNumber.Contains("-"))
                            {
                                List<string> myList = new List<string>(strPageNumber.Split('-'));
                                if (!string.IsNullOrEmpty(Convert.ToString(myList[0])))
                                {
                                    CurrentStatement.FromPage = Convert.ToString(myList[0]);
                                    CurrentStatement.ToPage = Convert.ToString(myList[1]);
                                }
                                else
                                {
                                    CurrentStatement.FromPage = Convert.ToString(myList[1]);
                                    CurrentStatement.ToPage = Convert.ToString(myList[1]);
                                }
                            }
                            else
                            {
                                CurrentStatement.FromPage = strPageNumber;
                                CurrentStatement.ToPage = strPageNumber;
                            }
                        }

                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.StatementClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            serviceClients.StatementClient.AddUpdateStatement(CurrentStatement);
                        }


                        deuFields.ReferenceNo = ReferenceNo++;
                        int totEntry = CurrentStatement.DeuEntries.Count;

                        CurrentStatement.DeuEntries.Insert(totEntry, CreateExposedDeu(deuFields.DeuFieldDataCollection, deuFields.ReferenceNo));

                        CurrentDeuEntry = CurrentStatement.DeuEntries[totEntry];
                        using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PostUtilClient.InnerChannel))
                        {
                            System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                            serviceClients.PostUtilClient.DeuPostStartWrapperAsync(PostEntryProcess.FirstPost, deuFields, Guid.Empty, RoleManager.userCredentialID, RoleManager.Role);
                        }
                    }
                    else
                    {
                        if (InEditableMode)
                        {

                            Guid oldDeuEntryId = deuFields.DeuEntryId;
                            deuFields.DeuEntryId = Guid.Empty;
                            deuFields.ReferenceNo = ReferenceNo++;

                            ExposedDEU tempDeu = CurrentStatement.DeuEntries.FirstOrDefault(s => s.DEUENtryID == oldDeuEntryId);
                            int intIndex = Getindex(oldDeuEntryId);

                            if (tempDeu != null)
                            {
                                CurrentStatement.PayorId = CurrentPayor.PayorID;
                                //add template
                                CurrentStatement.TemplateID = SelectedPayortempalate.TemplateID;

                                string strPageNumber = string.Empty;
                                strPageNumber = txtPagesNumber;
                                if (!string.IsNullOrEmpty(strPageNumber))
                                {
                                    if (strPageNumber.Contains("-"))
                                    {
                                        List<string> myList = new List<string>(strPageNumber.Split('-'));
                                        if (!string.IsNullOrEmpty(Convert.ToString(myList[0])))
                                        {
                                            CurrentStatement.FromPage = Convert.ToString(myList[0]);
                                            CurrentStatement.ToPage = Convert.ToString(myList[1]);
                                        }
                                        else
                                        {
                                            CurrentStatement.FromPage = Convert.ToString(myList[1]);
                                            CurrentStatement.ToPage = Convert.ToString(myList[1]);
                                        }
                                    }
                                    else
                                    {
                                        CurrentStatement.FromPage = strPageNumber;
                                        CurrentStatement.ToPage = strPageNumber;
                                    }
                                }

                                using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.StatementClient.InnerChannel))
                                {
                                    System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                                    serviceClients.StatementClient.AddUpdateStatement(CurrentStatement);
                                }
                                CurrentStatement.DeuEntries.Remove(tempDeu);

                                CurrentStatement.DeuEntries.Insert(intIndex, CreateExposedDeu(deuFields.DeuFieldDataCollection, deuFields.ReferenceNo));
                                CurrentDeuEntry = CurrentStatement.DeuEntries[intIndex];
                            }

                            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)serviceClients.PostUtilClient.InnerChannel))
                            {
                                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add("UserName", Convert.ToString(RoleManager.userCredentialID));
                                serviceClients.PostUtilClient.DeuPostStartWrapperAsync(PostEntryProcess.RePost, deuFields, oldDeuEntryId, RoleManager.userCredentialID, RoleManager.Role);
                            }
                            EnableEditDelete = false;

                            //Call if it is deleteed at first time
                            //serviceClients.DeuClient.DeleteDeuEntryAndPaymentEntryByDeuIDAsync(oldDeuEntryId);

                            DeuEntryEditText = "Start Edit";
                            InEditableMode = false;
                        }
                    }

                    SharedVMData.CachedClientList.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                    DeuFormBuilder.ResetFields();
                    isEnableCanvas = true;
                }

                InEditableMode = false;
                InDeleteMode = false;
                DeuEntryId = Guid.Empty;

                DeuFormBuilder.PostResetFields();
            }
            catch
            {

            }
        }

        void SendMailClient_SendMailCompleted(object sender, SendMailCompletedEventArgs e)
        {

        }

        void PostUtilClient_DeuPostStartWrapperCompleted(object sender, DeuPostStartWrapperCompletedEventArgs e)
        {
            if (objLog == null)
                objLog = new MastersClient();
            try
            {
                PostProcessReturnStatus status = e.Result;
                if (!e.Result.IsComplete && status.ErrorMessage == MessageConst.LockErrorMessage)
                {
                    System.Windows.MessageBox.Show(status.ErrorMessage + ", " + "Please try after some time", "Information !", MessageBoxButton.OK);
                    return;
                }
            if (e.Result.IsComplete)
                {
                    if (status.PostEntryStatus != PostEntryProcess.Delete)
                    {
                        LoadChangedBatchStatementData(status);
                        //send mail when payor and carrier not in application
                        //commented in by vinod ver 65 on 08072016
                        // SendMailAutomatically(CurrentDeuEntry);
                    }
                    else
                    {
                        LoadChangedBatchStatementData(status);
                    }
                    SearchedPolicy = null;
                }
                else
                {
                    SearchedPolicy = null;
                    MessageBox.Show("Payment is not posted sucessfully . Please refersh and try again", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    EnableEditDelete = true;
                    isEnableCanvas = true;
                    isEnablePostWrap = true;
                    DeuFormBuilder.PostResetFields();
                }

                //EnableEditDelete = true;
                //isEnableCanvas = true;
                //isEnablePostWrap = true;
                //DeuFormBuilder.PostResetFields();

            }
            catch (Exception ex)
            {
                objLog.AddLog("DeuPostStartWrapperCompleted Exception  : " + ex.Message);
                MessageBox.Show("Payment is not posted sucessfully . Please refersh and try again", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableEditDelete = true;
                isEnableCanvas = true;
                isEnablePostWrap = true;
                DeuFormBuilder.PostResetFields();

            }

        }
        private bool isReturnStatement = false;
        private bool isReturnCurrent = false;
        private void LoadChangedBatchStatementData(PostProcessReturnStatus postReturnStatus)
        {

            try
            {
                isReturnStatement = true;
                isReturnCurrent = true;

                //EnableEditDelete = true;
                //isEnableCanvas = true;
                //isEnablePostWrap = true;
                //DeuFormBuilder.PostResetFields();

                isEnableCanvas = true;
                isEnablePostWrap = true;
                DeuFormBuilder.PostResetFields();

                if (postReturnStatus.BatchStatementData != null)
                {
                    if (postReturnStatus.BatchStatementData.BatchData != null)
                    {
                        CurrentBatch = Batches.FirstOrDefault(s => s.BatchId == postReturnStatus.BatchStatementData.BatchData.BatchId);
                        if (CurrentBatch != null)
                        {
                            CurrentBatch.LastModifiedDate = postReturnStatus.BatchStatementData.BatchData.LastModifiedDate;
                            CurrentBatch.EntryStatus = postReturnStatus.BatchStatementData.BatchData.EntryStatus;
                        }
                    }

                    if (postReturnStatus.BatchStatementData.StatementData != null)
                    {
                        CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(s => s.StatementID == postReturnStatus.BatchStatementData.StatementData.StatementId);

                        if (CurrentStatement != null)
                        {
                            if (postReturnStatus.PostEntryStatus == PostEntryProcess.FirstPost)
                            {
                                ExposedDEU expDeu = CurrentStatement.DeuEntries.FirstOrDefault(s => s.ReferenceNo == postReturnStatus.ReferenceNo);
                                if (expDeu != null)
                                {
                                    expDeu.DEUENtryID = postReturnStatus.BatchStatementData.ExposedDeu.DEUENtryID;
                                    expDeu.EnableEditDeleteOperation = true;

                                    //adeed by Jyotisna to get amount from DB
                                    expDeu.IsPaymentFailure = postReturnStatus.BatchStatementData.ExposedDeu.IsPaymentFailure;

                                    if ((expDeu.ClientName == "") || (string.IsNullOrEmpty(expDeu.ClientName)))
                                    {
                                        expDeu.ClientName = expDeu.PolicyNumber;
                                    }
                                    if ((expDeu.Insured == "") || (string.IsNullOrEmpty(expDeu.Insured)))
                                    {
                                        expDeu.Insured = expDeu.ClientName;
                                    }
                                }

                            }
                            else if (postReturnStatus.PostEntryStatus == PostEntryProcess.RePost)
                            {
                                ExposedDEU expDeu = CurrentStatement.DeuEntries.FirstOrDefault(s => s.ReferenceNo == postReturnStatus.ReferenceNo);
                                if (expDeu != null)
                                {
                                    expDeu.DEUENtryID = postReturnStatus.BatchStatementData.ExposedDeu.DEUENtryID;
                                    expDeu.EnableEditDeleteOperation = true;
                                    
                                    //adeed by Jyotisna to get amount from DB

                                    expDeu.IsPaymentFailure = postReturnStatus.BatchStatementData.ExposedDeu.IsPaymentFailure;

                                    if ((expDeu.ClientName == "") || (string.IsNullOrEmpty(expDeu.ClientName)))
                                    {
                                        expDeu.ClientName = expDeu.PolicyNumber;
                                    }
                                    if ((expDeu.Insured == "") || (string.IsNullOrEmpty(expDeu.Insured)))
                                    {
                                        expDeu.Insured = expDeu.ClientName;
                                    }
                                }
                            }
                            else
                            {
                                ExposedDEU deu = CurrentStatement.DeuEntries.FirstOrDefault(s => s.DEUENtryID == postReturnStatus.DeuEntryId);
                                if (deu != null)
                                {
                                    CurrentStatement.DeuEntries.Remove(deu);
                                    deu.EnableEditDeleteOperation = true;
                                }
                            }

                            // CurrentStatement.EnteredAmount = postReturnStatus.BatchStatementData.StatementData.EnteredAmount;

                            //decimal? dbEntereAmoount = 0;
                            //CurrentStatement.EnteredAmount = 0;
                            //foreach (var item in CurrentStatement.DeuEntries)
                            //{
                            //    dbEntereAmoount = dbEntereAmoount + item.CommissionTotal;
                            //}
                            //CurrentStatement.EnteredAmount = Convert.ToDecimal(dbEntereAmoount);

                            //adeed by Jyotisna to get amount from DB
                            Statement details = serviceClients.StatementClient.GetFindStatement(CurrentStatement.StatementNumber);
                            CurrentStatement.EnteredAmount = details.EnteredAmount;

                            decimal dbNetAmount = Convert.ToDecimal(CheckAmount) - Convert.ToDecimal(AdjAmount);
                            if (dbNetAmount != 0)
                            {

                                //Change formula
                                //double dbValue = Convert.ToDouble(dbNetAmount - CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                                double dbValue = Convert.ToDouble(CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                                CurrentStatement.CompletePercentage = dbValue * 100;
                            }
                            else
                            {
                                CurrentStatement.CompletePercentage = 0;
                            }

                            EnableEditDelete = true;

                            //CurrentStatement.CompletePercentage = CurrentStatement.CompletePercentage;
                            //CurrentStatement.EnteredAmount = postReturnStatus.BatchStatementData.StatementData.EnteredAmount;
                            CurrentStatement.Entries = postReturnStatus.BatchStatementData.StatementData.Entries;
                            CurrentStatement.LastModified = postReturnStatus.BatchStatementData.StatementData.LastModified;
                            CurrentStatement.StatusId = postReturnStatus.BatchStatementData.StatementData.StatusId;
                            CurrentStatement.PayorId = postReturnStatus.BatchStatementData.StatementData.PayorId;

                            if (CurrentStatement.DeuEntries.Count > 0)
                                CurrentStatement.DeuEntries = new ObservableCollection<ExposedDEU>(CurrentStatement.DeuEntries.OrderBy(s => s.EntryDate));

                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void LoadStatementData(ModifiableStatementData batchStatementData)
        {
            try
            {
                if (batchStatementData != null)
                {
                    CurrentStatement.EnteredAmount = batchStatementData.EnteredAmount;
                    CurrentStatement = CurrentBatch.BatchStatements.FirstOrDefault(s => s.StatementID == batchStatementData.StatementId);
                    if (CurrentStatement != null)
                    {
                        decimal dbNetAmount = Convert.ToDecimal(CheckAmount) - Convert.ToDecimal(AdjAmount);
                        if (dbNetAmount != 0)
                        {
                            //double dbValue = Convert.ToDouble(dbNetAmount - CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                            double dbValue = Convert.ToDouble(CurrentStatement.EnteredAmount) / Convert.ToDouble(dbNetAmount);
                            CurrentStatement.CompletePercentage = dbValue * 100;
                        }
                        else
                            CurrentStatement.CompletePercentage = 0;

                        //CurrentStatement.CompletePercentage = CurrentStatement.CompletePercentage;
                        //CurrentStatement.EnteredAmount = batchStatementData.EnteredAmount;
                        CurrentStatement.Entries = batchStatementData.Entries;
                        CurrentStatement.LastModified = batchStatementData.LastModified;
                        CurrentStatement.StatusId = batchStatementData.StatusId;
                    }
                }
            }
            catch
            {
            }
        }

        private ComparePostDate getComparePostData(DEUFields deuFields)
        {
            ComparePostDate ComPostDate = new ComparePostDate();
            try
            {
                foreach (DataEntryField field in deuFields.DeuFieldDataCollection)
                {
                    switch (field.DeuFieldName)
                    {
                        case "PolicyNumber":
                            ComPostDate.PolicyNumber = CorrectPolicyNo(field.DeuFieldValue);
                            break;

                        case "InvoiceDate":
                            ComPostDate.InvoiceDate = GetDateAsPerFormatting("MM/dd/yyyy", field.DeuFieldValue);
                            break;
                    }
                }

            }
            catch
            {
            }
            return ComPostDate;
        }

        //private bool IsAlreadyPosted(ComparePostDate postData)
        //{
        //    bool isAlreadyPosted = false;
        //    if (CurrentStatement.DeuEntries != null && CurrentStatement.DeuEntries.Count != 0)
        //    {
        //        foreach (ExposedDEU deu in CurrentStatement.DeuEntries)
        //        {
        //            if (deu.PolicyNumber == postData.PolicyNumber && deu.InvoiceDate == postData.InvoiceDate)
        //            {
        //                isAlreadyPosted = true;
        //                break;
        //            }
        //        }
        //    }
        //    return isAlreadyPosted;
        //}

        public static string CorrectPolicyNo(string value)
        {
            try
            {
                value = value.Trim();
                value = value.Replace(" ", "");

                bool IsCharAddedToStringBuilder = false;
                StringBuilder stringBuilder = new StringBuilder(50);

                foreach (char c in value)
                {
                    if (!IsCharAddedToStringBuilder && c == '0')
                    {
                        continue;
                    }

                    if (char.IsLetterOrDigit(c))
                    {
                        stringBuilder.Append(c);
                        IsCharAddedToStringBuilder = true;
                    }
                }

                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }

        }

        public static DateTime GetDateAsPerFormatting(string maskText, string value)
        {
            try
            {
                if (maskText == "Text")
                    return DateTime.ParseExact(value, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);

                DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
                dateTimeFormat.ShortDatePattern = maskText;
                dateTimeFormat.DateSeparator = "-";
                return Convert.ToDateTime(value, dateTimeFormat);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private ICommand _ResetData;
        public ICommand ResetData
        {
            get
            {
                if (_ResetData == null)
                {
                    _ResetData = new BaseCommand(p =>
                    {
                        InEditableMode = false;
                        DeuEntryEditText = "Start Edit";
                        DeuFormBuilder.ResetFields();
                        SearchedPolicy = null;
                    });
                }
                return _ResetData;
            }

        }

        private ICommand _EditPolcyStatementEntry;
        public ICommand EditPolcyStatementEntry
        {
            get
            {
                if (_EditPolcyStatementEntry == null)
                {
                    _EditPolcyStatementEntry = new BaseCommand(p =>
                    {

                    });
                }
                return _EditPolcyStatementEntry;
            }

        }

        private ICommand _DeletePolcyStatementEntry;
        public ICommand DeletePolcyStatementEntry
        {
            get
            {
                if (_DeletePolcyStatementEntry == null)
                {
                    _DeletePolcyStatementEntry = new BaseCommand(p =>
                    {

                    });
                }
                return EditPolcyStatementEntry;
            }

        }

        private ICommand _ViewFile;
        public ICommand ViewFile
        {
            get
            {
                if (_ViewFile == null)
                {
                    _ViewFile = new BaseCommand(p => OnViewFile());
                }
                return _ViewFile;
            }

        }

        public void OnViewFile()
        {
            try
            {
                string RemotePath = string.Empty;
                if (CurrentBatch != null)
                {
                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                    FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);

                    ObjDownload.DownloadComplete += (obj1, obj2) =>
                    {
                        autoResetEvent.Set();
                    };

                    string localPath = Path.GetTempPath();

                    string fileName = Path.GetFileName(CurrentBatch.FileName);

                    string strFileExtension = System.IO.Path.GetExtension(fileName);

                    if (strFileExtension.ToLower().Contains("pdf"))
                    {
                        RemotePath = "/UploadBatch/" + CurrentBatch.FileName;
                    }
                    else
                    {
                        RemotePath = "/UploadBatch/Import/Success/" + CurrentBatch.FileName;
                    }

                    ObjDownload.Download(RemotePath, localPath + fileName);
                    // autoResetEvent.WaitOne(TimeSpan.FromMinutes(2));
                    autoResetEvent.WaitOne();

                    if (File.Exists(localPath + fileName))
                        System.Diagnostics.Process.Start(localPath + fileName);
                    else
                        MessageBox.Show("File named " + fileName + "is not found on server.", "File not found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
            }


            //    try
            //    {
            //        string RemotePath = string.Empty;
            //        Mouse.OverrideCursor = Cursors.Wait;
            //        autoResetEvent = new AutoResetEvent(false);

            //        FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);

            //        string localPath = Path.Combine(System.IO.Path.GetTempPath(), Path.GetFileName(CurrentBatch.FileName));

            //        string strFileExtension = System.IO.Path.GetExtension(CurrentBatch.FileName);

            //        if (strFileExtension.ToLower().Contains("pdf"))
            //        {
            //            RemotePath = "/UploadBatch/" + CurrentBatch.FileName;
            //        }
            //        else
            //        {
            //            RemotePath = "/UploadBatch/Import/Success/" + CurrentBatch.FileName;
            //        }

            //        //string RemotePath = "/UploadBatch/" + SelectedBatch.FileName;

            //        string strLocal = System.IO.Path.GetExtension(localPath);
            //        string strRemoteExt = System.IO.Path.GetExtension(RemotePath);
            //        if (string.IsNullOrEmpty(strRemoteExt))
            //        {
            //            MessageBox.Show("There are no file to view", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //        else
            //        {
            //            ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
            //            ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
            //            ObjDownload.Download(RemotePath, localPath);
            //        }

            //        Mouse.OverrideCursor = Cursors.Arrow;
            //    }
            //    catch
            //    {
            //    }
            //}

            //void ObjDownload_ErrorOccured(Exception error)
            //{
            //    MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            //void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
            //{
            //    if (statusCode.ToString().StartsWith("20"))
            //    {
            //        System.Diagnostics.Process.Start(localFilePath);
            //    }
            //    else
            //    {
            //        MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
        }

        private ICommand _StatementImage;
        public ICommand StatementImage
        {
            get
            {
                if (_StatementImage == null)
                {
                    _StatementImage = new BaseCommand(p =>
                    {
                        if (PayorTool != null)
                        {
                            ImagePath = PayorTool.StatementImageFilePath;
                            DeuFormBuilder.ShowFields();
                        }
                    });

                }
                return _StatementImage;
            }
        }

        private ICommand _ChequeImage;
        public ICommand ChequeImage
        {
            get
            {
                if (_ChequeImage == null)
                {
                    _ChequeImage = new BaseCommand(p =>
                    {
                        if (PayorTool != null)
                        {
                            ImagePath = PayorTool.ChequeImageFilePath;
                            DeuFormBuilder.HideFields();
                        }
                    });

                }
                return _ChequeImage;
            }

        }

        #endregion

        #region Private Methods

        private void CreatePrimaryKey(ObservableCollection<PayorToolField> ToolFields)
        {
            try
            {
                string Key = string.Empty;
                PrimaryKey = string.Empty;
                if (ToolFields != null)
                {
                    //2125 Check and add the availablefieldnames instead of labels 2031
                    foreach (PayorToolField Field in ToolFields)
                    {
                        if (Field.IsPartOfPrimaryKey && !string.IsNullOrEmpty(Field.AvailableFieldName))
                        {
                            Key = Key + Field.AvailableFieldName + ";";
                        }
                    }

                    if (!string.IsNullOrEmpty(Key))
                        PrimaryKey = Key.ToString().Substring(0, Key.ToString().Length - 1);

                }
            }
            catch
            {
            }
        }

        private void GetImageFromSever(PayorTool Tool)
        {

            try
            {
                autoResetEvent = new AutoResetEvent(false);
                //download the payor tool image from server and show on UI
                FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                ObjDownload.DownloadComplete += (obj1, obj2) =>
                {
                    autoResetEvent.Set();
                };

                string ChequeImage = System.IO.Path.GetTempFileName();
                string StatementImage = System.IO.Path.GetTempFileName();

                if (!string.IsNullOrEmpty(Tool.WebDevChequeImageFilePath))
                {
                    ObjDownload.Download(Tool.WebDevChequeImageFilePath, ChequeImage);
                    autoResetEvent.WaitOne();
                }

                if (!string.IsNullOrEmpty(Tool.WebDevStatementImageFilePath))
                {
                    ObjDownload.Download(Tool.WebDevStatementImageFilePath, StatementImage);
                    autoResetEvent.WaitOne();
                }

                Tool.ChequeImageFilePath = ChequeImage;
                Tool.StatementImageFilePath = StatementImage;
            }
            catch
            {
            }
        }

        #endregion

        #region Craete ExposeDeu

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deuFieldDataCollection"></param>
        /// <returns></returns>
        private ExposedDEU CreateExposedDeu(ObservableCollection<DataEntryField> deuFieldDataCollection, int ReferenceNo)
        {
            ExposedDEU exposedDeu = new ExposedDEU();
            exposedDeu.PaymentRecived = 0;
            exposedDeu.CommissionPercentage = 0;
            exposedDeu.Units = 0;
            exposedDeu.Fee = 0;
            exposedDeu.CommissionTotal = 0;
            exposedDeu.SplitPercentage = 100;
            exposedDeu.ReferenceNo = ReferenceNo;
            exposedDeu.EnableEditDeleteOperation = false;
            exposedDeu.ClientName = strClientName;
            exposedDeu.Insured = strinsuredName;
            exposedDeu.EntryDate = System.DateTime.Now;
            exposedDeu.IsPaymentFailure = false;

            try
            {
                foreach (DataEntryField field in deuFieldDataCollection)
                {
                    switch (field.DeuFieldName)
                    {
                        case "PolicyNumber":
                            exposedDeu.PolicyNumber = VMHelper.CorrectPolicyNo(field.DeuFieldValue);
                            break;

                        case "Insured":
                            if (AllCapitals(field.DeuFieldValue))
                                exposedDeu.Insured = FirstCharIsCapital(field.DeuFieldValue);
                            else
                                exposedDeu.Insured = field.DeuFieldValue;
                            break;

                        //case "OriginalEffectiveDate":
                        //    DateTime date1 = DateTime.ParseExact(field.DeuFieldValue, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
                        //    DeuEntry.OriginalEffectiveDate = date1;
                        //    deuData.OriginalEffectiveDate = DeuEntry.OriginalEffectiveDate;
                        //    break;

                        case "InvoiceDate":
                            DateTime date2 = DateTime.ParseExact(field.DeuFieldValue, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
                            exposedDeu.InvoiceDate = date2;
                            break;

                        case "PaymentReceived":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.PaymentRecived = 0;
                            else
                                exposedDeu.PaymentRecived = decimal.Parse(field.DeuFieldValue);
                            break;

                        case "CommissionPercentage":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.CommissionPercentage = 0;
                            else
                                exposedDeu.CommissionPercentage = double.Parse(field.DeuFieldValue);
                            break;

                        //case "Renewal":
                        //    DeuEntry.Renewal = field.DeuFieldValue;
                        //    deuData.Renewal = DeuEntry.Renewal;
                        //    break;

                        //case "Enrolled":
                        //    DeuEntry.Enrolled = field.DeuFieldValue;
                        //    deuData.Enrolled = DeuEntry.Enrolled;
                        //    break;

                        //case "Eligible":
                        //    DeuEntry.Eligible = field.DeuFieldValue;
                        //    deuData.Eligible = DeuEntry.Eligible;
                        //    break;

                        //case "Link1":
                        //    DeuEntry.Link1 = field.DeuFieldValue;
                        //    deuData.Link1 = DeuEntry.Link1;
                        //    break;

                        case "SplitPercentage":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.SplitPercentage = 100;
                            else
                                exposedDeu.SplitPercentage = double.Parse(field.DeuFieldValue);
                            break;

                        //case "PolicyMode":
                        //    DeuEntry.PolicyModeID = BLHelper.GetPolicyMode(field.DeuFieldValue);
                        //    DeuEntry.PolicyModeValue = field.DeuFieldValue;
                        //    deuData.PolicyMode = DeuEntry.PolicyModeID;
                        //    break;

                        case "Carrier":
                            exposedDeu.CarrierNickName = field.DeuFieldValue;
                            break;

                        case "Product":
                            exposedDeu.CoverageNickName = field.DeuFieldValue;
                            break;

                        //case "PayorSysId":
                        //    DeuEntry.PayorSysID = field.DeuFieldValue;
                        //    deuData.PayorSysID = DeuEntry.PayorSysID;
                        //    break;

                        //case "CompScheduleType":
                        //    if (string.IsNullOrEmpty(field.DeuFieldValue))
                        //        DeuEntry.CompScheduleType = null;
                        //    else
                        //        DeuEntry.CompScheduleType = field.DeuFieldValue;
                        //    deuData.CompScheduleType = DeuEntry.CompScheduleType;
                        //    break;

                        //case "CompType":
                        //    if (string.IsNullOrEmpty(field.DeuFieldValue))
                        //        DeuEntry.CompTypeID = null;
                        //    else
                        //        DeuEntry.CompTypeID = BLHelper.getCompTypeId(field.DeuFieldValue);
                        //    deuData.CompTypeID = DeuEntry.CompTypeID;
                        //    break;

                        case "Client":

                            if (AllCapitals(field.DeuFieldValue))
                            {
                                exposedDeu.ClientName = FirstCharIsCapital(field.DeuFieldValue);
                                exposedDeu.UnlinkClientName = exposedDeu.ClientName;
                            }
                            else
                            {
                                exposedDeu.ClientName = field.DeuFieldValue;
                                exposedDeu.UnlinkClientName = exposedDeu.ClientName;
                            }

                            break;

                        case "NumberOfUnits":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.Units = 0;
                            else
                                exposedDeu.Units = int.Parse(field.DeuFieldValue);
                            break;

                        //case "DollerPerUnit":
                        //    if (string.IsNullOrEmpty(field.DeuFieldValue))
                        //        DeuEntry.DollerPerUnit = 0;
                        //    else
                        //        DeuEntry.DollerPerUnit = decimal.Parse(field.DeuFieldValue);
                        //    deuData.DollerPerUnit = DeuEntry.DollerPerUnit;
                        //    break;

                        case "Fee":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.Fee = 0;
                            else
                                exposedDeu.Fee = decimal.Parse(field.DeuFieldValue);
                            break;

                        //case "Bonus":
                        //    if (string.IsNullOrEmpty(field.DeuFieldValue))
                        //        DeuEntry.Bonus = 0;
                        //    else
                        //        DeuEntry.Bonus = decimal.Parse(field.DeuFieldValue);
                        //    deuData.Bonus = DeuEntry.Bonus;
                        //    break;

                        case "CommissionTotal":
                            if (string.IsNullOrEmpty(field.DeuFieldValue))
                                exposedDeu.CommissionTotal = 0;
                            else
                                exposedDeu.CommissionTotal = decimal.Parse(field.DeuFieldValue);
                            break;
                    }
                }
            }
            catch
            {
            }
            return exposedDeu;
        }

        #endregion

        #region"Font case"

        public bool AllCapitals(string inputString)
        {
            foreach (char c in inputString)
            {
                if (char.IsLower(c))
                    return false;
            }
            return true;

        }

        public string FirstCharIsCapital(string stringToModify)
        {

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(stringToModify))
            {
                string[] array = stringToModify.Split(' ');

                for (int i = 0; i < array.Length; i++)
                {

                    string firstLetter = array[i].Substring(0, 1);

                    string secondPart = array[i].Substring(1);

                    sb.Append(firstLetter.ToUpper() + secondPart.ToLower() + " ");

                }
            }
            if (sb.Length > 0)
                return sb.ToString().Remove(sb.Length - 1);
            else
                return sb.ToString();

        }

        #endregion

        #region Refresh Data

        public void Refresh()
        {
            LoadData();
        }

        #endregion
    }

    public class ComparePostDate
    {
        public DateTime InvoiceDate;
        public string PolicyNumber;
    }
}
