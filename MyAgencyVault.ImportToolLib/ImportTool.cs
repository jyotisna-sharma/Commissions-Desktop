using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Globalization;
using MyAgencyVault.BusinessLibrary;
using MyAgencyVault.WcfService;
using MyAgencyVault.EmailFax;
using System.Collections.ObjectModel;
using ActionLogger;
using System.Configuration;
using System.Data.SqlClient;
using MyAgencyVault.BusinessLibrary.PostProcess;
using System.Threading;

namespace MyAgencyVault.ImportToolLib
{
    public class ImportTool
    {

    }

    public class startService
    {
       
        bool isCheckAmountAvailable = false;
        int intStatementnumber = 0;
        //int intStartServiceAgain = 0;
        //private string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True;";
        private string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True";

        private ObservableCollection<MyAgencyVault.BusinessLibrary.ImportToolBrokerSetting> _AllImportToolBrokerSetting;
        public ObservableCollection<MyAgencyVault.BusinessLibrary.ImportToolBrokerSetting> AllImportToolBroker
        {
            get
            {
                return _AllImportToolBrokerSetting;
            }
            set
            {
                _AllImportToolBrokerSetting = value;

            }
        }

        private MyAgencyVault.BusinessLibrary.ImportToolBrokerSetting _selectedImportToolSetting;
        public ImportToolBrokerSetting selectedImportToolSetting
        {
            get
            {
                return _selectedImportToolSetting;
            }
            set
            {
                _selectedImportToolSetting = value;

            }
        }

        private DisplayBrokerCode _selectedDisplayBrokerCode;
        public DisplayBrokerCode selectedDisplayBrokerCode
        {
            get
            {
                return _selectedDisplayBrokerCode;
            }
            set
            {
                _selectedDisplayBrokerCode = value;

            }
        }

        private LicenseeDisplayData _selectedLicenseeDisplayData;
        public LicenseeDisplayData selectedLicenseeDisplayData
        {
            get
            {
                return _selectedLicenseeDisplayData;
            }
            set
            {
                _selectedLicenseeDisplayData = value;

            }
        }

        private ImportToolPayorPhrase _SelectedImportToolPayorPhrase;
        public ImportToolPayorPhrase SelectedImportToolPayorPhrase
        {
            get
            {
                return _SelectedImportToolPayorPhrase;
            }
            set
            {
                _SelectedImportToolPayorPhrase = value;
            }
        }

        private List<MaskFieldTypes> _ListMaskFieldTypes;
        public List<MaskFieldTypes> ListMaskFieldTypes
        {
            get
            {
                return _ListMaskFieldTypes;
            }
            set
            {
                _ListMaskFieldTypes = value;

            }
        }

        private ObservableCollection<MaskFieldTypes> _tempListMaskFieldTypes;
        public ObservableCollection<MaskFieldTypes> tempListMaskFieldTypes
        {
            get
            {
                return _tempListMaskFieldTypes;
            }
            set
            {
                _tempListMaskFieldTypes = value;

            }
        }

        private MaskFieldTypes _SelectedMaskFieldsTypes;
        public MaskFieldTypes SelectedMaskFieldsTypes
        {
            get
            {
                return _SelectedMaskFieldsTypes;
            }
            set
            {
                _SelectedMaskFieldsTypes = value;

            }
        }

        private Statement _CurrentStatement;
        public Statement CurrentStatement
        {
            get { return _CurrentStatement; }
            set
            {
                _CurrentStatement = value;

            }
        }

        public Guid generatedBatchID = Guid.Empty;
        public int generatedStatementNumber = 0;
        Guid guidSuperUser = new Guid("AA38DF84-2E30-43CA-AED3-7276224D1B7E");
        string strFileFullName = string.Empty;
        

        public void StartFolderWatcher()
        {
            try
            {
                Brokercode objbBrokerCode = new Brokercode();
                AllImportToolBroker = new ObservableCollection<ImportToolBrokerSetting>(objbBrokerCode.LoadImportToolBrokerSetting().ToList());
                //AddToDataBase("Reading all broker code");
                string supportedExtensions = "*.xls,*.xlsx,*.txt,*.csv";
                string strBatch = string.Empty;
                string strCompnayName = string.Empty;
                string strCompnayID = string.Empty;
                string strFileName = string.Empty;
                //string strserver = @"D:\Filemanager\Uploadbatch\Import\Processing";
                string strserver = @"D:\ImportPayorTool\Processing";
                string strFilePath = strserver;
                // string strFilePath = strLocal;

                //intStartServiceAgain = Directory.GetFiles(strFilePath, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).Count();

                foreach (string FilePath in Directory.GetFiles(strFilePath, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())))
                {
                    strFileFullName = System.IO.Path.GetFileName(FilePath);
                    ActionLogger.Logger.WriteImportLog("************************************", true);
                    ActionLogger.Logger.WriteImportLog("Import start reading file.Current file name is :" + strFileFullName, true);
                    //AddToDataBase(strFileFullName);
                    isCheckAmountAvailable = false;
                    if (strFileFullName.Contains("_"))
                    {
                        try
                        {
                            string[] arrValue = strFileFullName.Split('_');
                            if (arrValue.Length > 3)
                            {
                                strCompnayName = arrValue[0];
                                strCompnayID = arrValue[1];
                                strBatch = arrValue[2];
                                strFileName = arrValue[3];
                            }
                            else if (arrValue.Length == 3)
                            {
                                strCompnayName = arrValue[0];
                                strBatch = arrValue[1];
                                strFileName = arrValue[2];
                            }
                            //AddToDataBase("1");
                            //AddToDataBase(strCompnayName);
                            //AddToDataBase(strCompnayID);
                            ActionLogger.Logger.WriteImportLog("Agency name is :" + strCompnayName, true);

                            List<LicenseeDisplayData> objLiceencessDetails = new List<LicenseeDisplayData>(LicenseeDisplayData.GetLicenseeName(strCompnayName));
                            selectedLicenseeDisplayData = objLiceencessDetails.FirstOrDefault();
                            //AddToDataBase("6");
                            if (objLiceencessDetails == null || objLiceencessDetails.Count == 0)
                            {
                                Guid? GuID = new Guid(strCompnayID);
                                objLiceencessDetails = new List<LicenseeDisplayData>(LicenseeDisplayData.GetLicenseeByID(GuID));
                                selectedLicenseeDisplayData = objLiceencessDetails.FirstOrDefault();
                                // AddToDataBase("GuID");
                            }

                            if (selectedLicenseeDisplayData != null)
                            {
                                if (selectedLicenseeDisplayData.LicenseeId != null)
                                {
                                    //AddToDataBase("7");
                                    Guid licID = selectedLicenseeDisplayData.LicenseeId;
                                    //AddToDataBase("8");
                                    if (licID != Guid.Empty)
                                    {
                                        // ActionLogger.Logger.WriteImportLog("Agency found :" + strCompnayName, true);
                                        //AddToDataBase("2");
                                        //AddToDataBase(licID.ToString());

                                        //Create new batch 
                                        int intBatchValue = CreateBatch(licID, strBatch);
                                        //AddToDataBase("3");
                                        //AddToDataBase(intBatchValue.ToString());
                                        //Then serach from its location
                                        //AddToDataBase("start read data table");
                                        DataTable dt = ConvretExcelToDataTable(FilePath);
                                        //ActionLogger.Logger.WriteImportLog("Read xls file succesfull  :", true);
                                        MoveToTempfolder();
                                        //AddToDataBase("File moved to temp folder");
                                        //ActionLogger.Logger.WriteImportLog("File moved to temp folder", true);
                                        // AddToDataBase("Complete readiing data table");
                                        //AddToDataBase(dt.Rows.Count.ToString());
                                        //AddToDataBase("Star payor phrase");
                                        SearchPayortemplatePhrase(dt, licID);
                                    }
                                }
                                else
                                {
                                    //if not found by license id 
                                    //Then serach from its location
                                    DataTable dt = ConvretExcelToDataTable(FilePath);
                                    MoveToTempfolder();
                                    //AddToDataBase("File moved to temp folder");
                                    //ActionLogger.Logger.WriteImportLog("File moved to temp folder", true);
                                    List<Guid> licenseeList = new List<Guid>();
                                    licenseeList = SerchBrkerCode(dt, AllImportToolBroker);
                                    //if one licensee found then go for serch payor phrase
                                    if (licenseeList.Count == 1)
                                    {
                                        Guid licID = licenseeList.FirstOrDefault();
                                        if (licID != Guid.Empty)
                                        {
                                            //Create batch 
                                            int intBatchValue = CreateBatch(licID, strBatch);
                                            SearchPayortemplatePhrase(dt, licID);
                                        }
                                    }
                                    else
                                    {
                                        ActionLogger.Logger.WriteImportLog("Agency not found", true);
                                        MoveToTempfolder();
                                        MoveToUnSuccesfullfolder();
                                    }
                                }
                            }
                            else
                            {
                                DataTable dt = ConvretExcelToDataTable(FilePath);
                                List<Guid> licenseeList = new List<Guid>();
                                licenseeList = SerchBrkerCode(dt, AllImportToolBroker);
                                //if one licensee found then go for serch payor phrase
                                if (licenseeList.Count == 1)
                                {
                                    Guid licID = licenseeList.FirstOrDefault();
                                    if (licID != Guid.Empty)
                                    {
                                        //Create new batch 
                                        int intBatchValue = CreateBatch(licID, strBatch);
                                        //Serch payor id and template Id
                                        SearchPayortemplatePhrase(dt, licID);
                                    }
                                }
                                else
                                {
                                    ActionLogger.Logger.WriteImportLog("Agency not found", true);
                                    MoveToTempfolder();
                                    MoveToUnSuccesfullfolder();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //string strex = ex.Message.ToString();
                            //AddToDataBase(strex);
                            //AddToDataBase("execption");
                            MoveToTempfolder();
                            MoveToUnSuccesfullfolder();
                            ActionLogger.Logger.WriteImportLog("Error foound while processing file:" + ex.Message.ToString(), true);
                        }
                    }
                    else
                    {
                        DataTable dt = ConvretExcelToDataTable(FilePath);
                        List<Guid> licenseeList = new List<Guid>();
                        licenseeList = SerchBrkerCode(dt, AllImportToolBroker);
                        //if one licensee found then go for serch payor phrase
                        if (licenseeList.Count == 1)
                        {
                            Guid licID = licenseeList.FirstOrDefault();
                            if (licID != Guid.Empty)
                            {
                                //Create new batch                              
                                int intBatchValue = CreateBatch(licID, strBatch);
                                SearchPayortemplatePhrase(dt, licID);
                            }
                        }
                        else
                        {
                            //Send mail more then Agency found
                            ActionLogger.Logger.WriteImportLog("Need to send mail ,phrase found more then one agency", true);
                            MoveToTempfolder();
                            MoveToUnSuccesfullfolder();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
                ActionLogger.Logger.WriteImportLog("Error foound while processing file:" + ex.Message.ToString(), true);

            }
        }

        private int CreateBatch(Guid licID, string strBatch)
        {
            int batchNumber = 0;
            try
            {
                if (string.IsNullOrEmpty(strBatch))
                {
                    Batch NewBatch = new Batch();
                    NewBatch.BatchId = Guid.NewGuid();
                    NewBatch.CreatedDate = DateTime.Now.Date;
                    NewBatch.IsManuallyUploaded = false;
                    NewBatch.EntryStatus = EntryStatus.Importedfiletype;
                    NewBatch.UploadStatus = UploadStatus.ImportXls;
                    //NewBatch.EntryStatus = EntryStatus.ImportPending;
                    NewBatch.FileType = "xlxs";
                    NewBatch.LicenseeId = licID;
                    LicenseeDisplayData _Licensee = Licensee.GetLicenseeByID(NewBatch.LicenseeId);
                    NewBatch.LicenseeName = _Licensee.Company;
                    batchNumber = NewBatch.AddUpdate();
                    NewBatch.BatchNumber = batchNumber;
                    generatedBatchID = NewBatch.BatchId;

                }
                else
                {
                    Batch ObjBatch = new Batch();
                    batchNumber = Convert.ToInt32(strBatch);
                    generatedBatchID = ObjBatch.GetBatchID(batchNumber);
                }

            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while creating/finding batch  :" + ex.Message.ToString(), true);
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
            }

            ActionLogger.Logger.WriteImportLog("Batch number  :" + batchNumber, true);
            return batchNumber;
        }

        private int CreateStatement(Guid batchID, Guid PayorID, Guid templateID)
        {
            int StatementNumber = 0;
            try
            {
                Statement objStatement = new Statement();

                CurrentStatement = new Statement();
                CurrentStatement.BatchId = batchID;
                CurrentStatement.StatementID = Guid.NewGuid();
                CurrentStatement.StatementDate = DateTime.Now;
                CurrentStatement.PayorId = PayorID;
                CurrentStatement.CreatedBy = guidSuperUser;
                CurrentStatement.StatusId = 0;
                CurrentStatement.CreatedDate = System.DateTime.Now;
                CurrentStatement.LastModified = System.DateTime.Now;

                CurrentStatement.TemplateID = templateID;
                StatementNumber = objStatement.AddStatementNumber(CurrentStatement);
                CurrentStatement.StatementNumber = Convert.ToInt32(StatementNumber);
                //Asign statement 
                intStatementnumber = StatementNumber;
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while creating new statement  :" + ex.Message.ToString(), true);
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
            }

            return StatementNumber;
        }

        private void SearchPayortemplatePhrase(DataTable dt, Guid licID)
        {
            try
            {
                DataSet ds = GetListOftemplatePhrase();
                int intTotalPhrase = 0;
                Guid templateID = Guid.Empty;
                DataSet dsAllPhrase = new DataSet();
                bool isFound = false;
                string strPhrase = string.Empty;
                List<ImportToolPayorPhrase> objImportToolPhrase = new List<ImportToolPayorPhrase>();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        int Count = 0;
                        SelectedImportToolPayorPhrase = new ImportToolPayorPhrase();
                        intTotalPhrase = Convert.ToInt32(ds.Tables[0].Rows[i][1]);
                        if (intTotalPhrase > 0)
                        {
                            templateID = (Guid)(ds.Tables[0].Rows[i][0]);
                            dsAllPhrase = AllPhraseBytemplateID(templateID);
                            if (dsAllPhrase == null)
                            {
                                return;
                            }

                            for (int j = 0; j < dsAllPhrase.Tables[0].Rows.Count; j++)
                            {
                                strPhrase = Convert.ToString(dsAllPhrase.Tables[0].Rows[j][12]);
                                isFound = GetPharse(dt, strPhrase);
                                if (isFound)
                                {
                                    Count++;
                                    if (Count == intTotalPhrase)
                                    {
                                        SelectedImportToolPayorPhrase.PayorID = (Guid)(dsAllPhrase.Tables[0].Rows[j][1]);
                                        SelectedImportToolPayorPhrase.PayorName = Convert.ToString(dsAllPhrase.Tables[0].Rows[j][2]);
                                        SelectedImportToolPayorPhrase.TemplateID = (Guid)(dsAllPhrase.Tables[0].Rows[j][3]);
                                        SelectedImportToolPayorPhrase.TemplateName = Convert.ToString(dsAllPhrase.Tables[0].Rows[j][4]);
                                        SelectedImportToolPayorPhrase.PayorPhrases = strPhrase;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                        }
                        if (Count == intTotalPhrase)
                        {
                            objImportToolPhrase.Add(SelectedImportToolPayorPhrase);
                        }
                    }

                    if (objImportToolPhrase.Count > 1)
                    {
                        //sent mail can't idendify the payor and template
                        ActionLogger.Logger.WriteImportLog("Phrases are not unique.it is find more then one payor/template ", true);
                        MoveToTempfolder();
                        MoveToUnSuccesfullfolder();
                    }

                    else if (objImportToolPhrase.Count == 1)
                    {
                        foreach (var itemsearch in objImportToolPhrase)
                        {
                            SelectedImportToolPayorPhrase.PayorID = itemsearch.PayorID;
                            SelectedImportToolPayorPhrase.PayorName = itemsearch.PayorName;
                            SelectedImportToolPayorPhrase.TemplateID = itemsearch.TemplateID;
                            SelectedImportToolPayorPhrase.TemplateName = itemsearch.TemplateName;
                            SelectedImportToolPayorPhrase.PayorPhrases = itemsearch.PayorPhrases;
                        }
                        SearchStatementSetting(SelectedImportToolPayorPhrase, dt, licID);
                    }

                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while searching phrases  :" + ex.Message.ToString(), true);
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
            }

        }

        private bool GetPharse(DataTable dt, string searchText)
        {
            bool isBoolBreak = false;

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (Convert.ToString(dt.Rows[i][j]) != null)
                        {
                            if (dt.Rows[i][j].ToString().ToLower().Contains(searchText.ToLower()))
                            {
                                isBoolBreak = true;
                                break;
                            }
                        }
                    }
                    if (isBoolBreak)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while searching the phrases: " + ex.Message.ToString(), true);
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
            }

            return isBoolBreak;
        }

        private void SearchStatementSetting(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, Guid licID)
        {
            try
            {
                List<ImportToolStatementDataSettings> objImportToolStatementDataSettings = new List<ImportToolStatementDataSettings>();

                PayorTemplate objPayorTemplateCode = new PayorTemplate();
                objImportToolStatementDataSettings = objPayorTemplateCode.GetAllImportToolStatementDataSettings(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();

                int statementNumber = CreateStatement(generatedBatchID, ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID);
                ActionLogger.Logger.WriteImportLog("Statement Number: " + statementNumber.ToString(), true);

                string strEndDataIndicator = string.Empty;
                string strStartRowsData = string.Empty;
                string strStartColsData = string.Empty;
                string strEndDataRows = string.Empty;
                string strEndDataCols = string.Empty;

                string strBal = string.Empty;
                string strCheckAmount = string.Empty;
                string strNetCheck = string.Empty;
                string strStatementDate = string.Empty;

                foreach (var item in objImportToolStatementDataSettings)
                {
                    if (item.MasterStatementDataID == (int)AvailableStatementData.BalforAdj)
                    {
                        strBal = GetStatementDataValue(dt, item);
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.CheckAmt)
                    {
                        strCheckAmount = GetStatementDataValue(dt, item);
                        isCheckAmountAvailable = true;
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.NetCheck)
                    {
                        strNetCheck = GetStatementDataValue(dt, item);
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.StatementDate)
                    {
                        strStatementDate = GetStatementDataValue(dt, item);
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.StartData)
                    {
                        try
                        {
                            int rows = -1;
                            int cols = -1;

                            if (!string.IsNullOrEmpty(item.RelativeSearch))
                            {
                                int intRelativeRowLocation = 0;
                                int intColoLocation = 0;

                                if (!string.IsNullOrEmpty(item.RelativeRowLocation))
                                {
                                    intRelativeRowLocation = Convert.ToInt32(item.RelativeRowLocation);
                                }

                                if (!string.IsNullOrEmpty(item.RelativeColLocation))
                                {
                                    intColoLocation = Convert.ToInt32(item.RelativeColLocation);
                                }

                                GetRelativeLocation(dt, item.RelativeSearch, intRelativeRowLocation, intColoLocation, out rows, out cols);
                                strStartRowsData = rows.ToString();
                                strStartColsData = cols.ToString();
                            }
                            else
                            {
                                strStartRowsData = item.FixedRowLocation;
                                strStartColsData = item.FixedColLocation;
                            }
                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteImportLog("Error in finding relative or fixed location " + ex.Message.ToString(), true);
                        }
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.EndData)
                    {
                        strEndDataIndicator = item.BlankFieldsIndicator;
                        int rows = -1;
                        int cols = -1;

                        try
                        {
                            if (!string.IsNullOrEmpty(item.RelativeSearch))
                            {
                                int intRelativeRowLocation = 0;
                                int intColoLocation = 0;

                                if (!string.IsNullOrEmpty(item.RelativeRowLocation))
                                {
                                    intRelativeRowLocation = Convert.ToInt32(item.RelativeRowLocation);
                                }

                                if (!string.IsNullOrEmpty(item.RelativeColLocation))
                                {
                                    intColoLocation = Convert.ToInt32(item.RelativeColLocation);
                                }

                                GetRelativeLocation(dt, item.RelativeSearch, intRelativeRowLocation, intColoLocation, out rows, out cols);
                                strEndDataRows = rows.ToString();
                                strEndDataCols = cols.ToString();
                            }
                            else
                            {
                                strEndDataRows = item.FixedRowLocation;
                                strEndDataCols = item.FixedColLocation;
                            }
                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteImportLog("Error in finding relative or fixed location " + ex.Message.ToString(), true);
                        }
                    }
                }

                UpdateStatementData(statementNumber, strBal, strCheckAmount, strNetCheck, strStatementDate);
                SearchPaymentDataSetting(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strStartColsData, strEndDataRows, strEndDataCols, strEndDataIndicator, licID);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error in function SearchStatementSetting " + ex.Message.ToString(), true);
            }

        }

        private void UpdateStatementData(int StatementNumber, string strBal, string strCheckAmount, string strNetCheck, string strStatementDate)
        {
            // public static void UpdateImporttoolStatementData(int intStatementNumber, decimal? dbCheckAmount, decimal? dbNetCheck,decimal? enteredAmount,int? intEntries)

            try
            {
                decimal? dcBalAdj = null;
                if (!string.IsNullOrEmpty(strBal))
                {
                    dcBalAdj = Convert.ToDecimal(strBal);
                }
                decimal? dcCheckAmount = null;
                if (!string.IsNullOrEmpty(strCheckAmount))
                {
                    dcCheckAmount = Convert.ToDecimal(strCheckAmount);
                }

                DateTime? dtStatementDate = null;
                if (!string.IsNullOrEmpty(strStatementDate))
                {
                    dtStatementDate = Convert.ToDateTime(strStatementDate);
                }
                else
                {
                    dtStatementDate = System.DateTime.Now;
                }

                decimal? EnteredAmount = null;
                int? TotalEntry = null;

                Statement objStatement = new Statement();
                objStatement.UpdateImporttoolStatementData(StatementNumber, dcCheckAmount, dcBalAdj, EnteredAmount, TotalEntry, dtStatementDate);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error in function UpdateStatementData " + ex.Message.ToString(), true);
            }

        }

        public string getExpression(ImportToolPaymentDataFieldsSettings deuPayorToolField, List<DataEntryField> dueFields)
        {
            string Expression = deuPayorToolField.FormulaExpression;
            try
            {

                ExpressionStack expressionStack = new ExpressionStack(Expression);
                List<ExpressionToken> expTokens = expressionStack.getExpressionTokenList();
                DataEntryField deuField = null;

                if (expTokens != null)
                {
                    expTokens = expTokens.Where(s => s.TokenType == ExpressionTokenType.Variable).Distinct().OrderByDescending(s => s.TokenString.Length).ToList();
                    foreach (ExpressionToken token in expTokens)
                    {
                        if (token.TokenType == ExpressionTokenType.Variable)
                        {
                            if (Expression.Contains(token.TokenString))
                            {
                                deuField = dueFields.FirstOrDefault(s => s.DeuFieldName.Trim() == token.TokenString.Trim());
                                string value = deuField.DeuFieldValue.Replace("$", "");
                                value = value.Replace("%", "");
                                value = value.Replace(",", "");
                                Expression = Expression.Replace(token.TokenString, deuField.DeuFieldValue.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error in function getExpression " + ex.Message.ToString(), true);
            }

            return Expression;
        }

        private void SearchPaymentDataSetting(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, string strStartRowsData, string strStartColsData, string strEndDataRows, string strEndDataCols, string strEndDataIndicator, Guid licID)
        {
            //AddToDataBase("start payment");
            LoadMaskType();
            List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();
            PayorTemplate objPayorTemplateCode = new PayorTemplate();
            objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();
            int dtRowsCount = dt.Rows.Count;
            bool isEndOfRead = false;

            string strInvoiceMonth = string.Empty;
            string strInvoiceYear = string.Empty;

            int intEndDataRows = EnddataRowsLocation(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strEndDataIndicator);
            decimal dbCheackAmount = 0;

            //ActionLogger.Logger.WriteImportLog("Total statement: " + intEndDataRows.ToString(), true);

            //ActionLogger.Logger.WriteImportLog("Starting Read value from excel files:  " + System.DateTime.Now, true);

            try
            {

                DEUFields deuFields = new DEUFields();
                DEU deudata = new DEU();

                List<DataEntryField> dueFields = new List<DataEntryField>();
                List<DataEntryField> due11 = new List<DataEntryField>();
                DataEntryField deuField = null;
                DataEntryField abc = null;
                Guid GuidPid = new Guid();

                List<UniqueIdenitfier> uniqueIdentifiers = new List<UniqueIdenitfier>();

                UniqueIdenitfier uniqueIdentifier = null;

                for (int i = 0; i <= dtRowsCount; i++)
                {
                    uniqueIdentifiers.Clear();
                    if (deuFields != null)
                    {
                        if (deuFields.DeuFieldDataCollection != null)
                        {
                            deuFields.DeuFieldDataCollection.Clear();
                            deuFields.DeuData = null;
                        }
                    }

                    if (i >= Convert.ToInt32(strStartRowsData))
                    {
                        deudata = new DEU();
                        deuField = new DataEntryField();

                        strInvoiceMonth = string.Empty;
                        strInvoiceYear = string.Empty;

                        foreach (var item in objImportToolPaymentDataSettings)
                        {
                            abc = new DataEntryField();
                            uniqueIdentifier = new UniqueIdenitfier();
                            switch (item.FieldsName)
                            {
                                case "PolicyNumber":

                                    try
                                    {
                                        string strPolicyNumber = string.Empty;
                                        string strDefaultText = string.Empty;
                                        int intCol = -1;
                                        int intRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intCol = Convert.ToInt32(item.FixedColLocation);
                                            intCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCol);
                                            }
                                        }

                                        if (intEndDataRows > intRow)
                                        {
                                            if (intCol > -1)
                                            {
                                                strPolicyNumber = dt.Rows[intRow][intCol].ToString().Trim();
                                                strDefaultText = item.strDefaultText;

                                                if (!string.IsNullOrEmpty(strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strPolicyNumber))
                                                    {
                                                        strPolicyNumber = strDefaultText;
                                                    }
                                                }

                                                deudata.PolicyNumber = strPolicyNumber;
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strPolicyNumber;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strPolicyNumber;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strPolicyNumber;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading policy number from files " + ex.Message.ToString(), true);
                                    }

                                    break;

                                case "ModelAvgPremium":
                                    try
                                    {
                                        string strModelAvgpremium = string.Empty;

                                        int intModelAvgpremiumCol = -1;
                                        int intModelAvgpremiumRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intModelAvgpremiumCol = Convert.ToInt32(item.FixedColLocation);
                                            intModelAvgpremiumCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative serch
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intModelAvgpremiumCol);
                                            }
                                        }

                                        if (intEndDataRows > intModelAvgpremiumRow)
                                        {
                                            if (intModelAvgpremiumCol > -1)
                                            {
                                                strModelAvgpremium = dt.Rows[intModelAvgpremiumRow][intModelAvgpremiumCol].ToString().Trim();
                                                // deuFields.DeuData.ModalAvgPremium =Convert.ToDecimal(strModelAvgpremium);
                                                string strDefaultText = item.strDefaultText;

                                                if (!string.IsNullOrEmpty(strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strModelAvgpremium))
                                                    {
                                                        strModelAvgpremium = strDefaultText;
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(strModelAvgpremium))
                                                {
                                                    strModelAvgpremium = "0";
                                                }
                                                if (string.IsNullOrWhiteSpace(strModelAvgpremium))
                                                {
                                                    strModelAvgpremium = "0";
                                                }
                                                deudata.ModalAvgPremium = Convert.ToDecimal(strModelAvgpremium);
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strModelAvgpremium;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strModelAvgpremium;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strModelAvgpremium;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading ModelAvgPremium from files " + ex.Message.ToString(), true);
                                    }
                                    break;


                                case "Insured":

                                    try
                                    {
                                        string strInsured = string.Empty;
                                        int intinsuredCol = -1;
                                        int intinsuredRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {

                                            //intinsuredCol = Convert.ToInt32(item.FixedColLocation);
                                            intinsuredCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative serch
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intinsuredCol);
                                            }
                                        }

                                        if (intEndDataRows > intinsuredRow)
                                        {
                                            if (intinsuredCol > -1)
                                            {
                                                strInsured = dt.Rows[intinsuredRow][intinsuredCol].ToString().Trim();
                                                // deuFields.DeuData.Insured = strInsured;

                                                string strDefaultText = item.strDefaultText;

                                                if (!string.IsNullOrEmpty(strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strInsured))
                                                    {
                                                        strInsured = strDefaultText;
                                                    }
                                                }

                                                deudata.Insured = strInsured;
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strInsured;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strInsured;
                                                dueFields.Add(abc);
                                                //   ssInsured.Add(strInsured);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strInsured;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading insured from files " + ex.Message.ToString(), true);
                                    }

                                    break;

                                case "OriginalEffectiveDate":
                                    try
                                    {
                                        string strOriginalEffectiveDate = string.Empty;
                                        int intOriginalEffectiveDateCol = -1;
                                        int intOriginalEffectiveDateRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intOriginalEffectiveDateCol = Convert.ToInt32(item.FixedColLocation);
                                            intOriginalEffectiveDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search    
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intOriginalEffectiveDateCol);
                                            }
                                        }

                                        if (intEndDataRows > intOriginalEffectiveDateRow)
                                        {
                                            if (intOriginalEffectiveDateCol > -1)
                                            {
                                                strOriginalEffectiveDate = dt.Rows[intOriginalEffectiveDateRow][intOriginalEffectiveDateCol].ToString().Trim();
                                                //deuFields.DeuData.OriginalEffectiveDate = Convert.ToDateTime(strOriginalEffectiveDate);

                                                string strDefaultText = item.strDefaultText;

                                                if (!string.IsNullOrEmpty(strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strOriginalEffectiveDate))
                                                    {
                                                        strOriginalEffectiveDate = strDefaultText;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strOriginalEffectiveDate))
                                                {
                                                    deudata.OriginalEffectiveDate = Convert.ToDateTime(strOriginalEffectiveDate);
                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strOriginalEffectiveDate;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strOriginalEffectiveDate;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strOriginalEffectiveDate;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading originaleffectivedate from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "InvoiceDate":
                                    try
                                    {
                                        string strInvoiceDate = string.Empty;
                                        int intInvoiceDateCol = -1;
                                        int intInvoiceDateRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intInvoiceDateCol = Convert.ToInt32(item.FixedColLocation);
                                            intInvoiceDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceDateCol);
                                            }
                                        }

                                        if (intEndDataRows > intInvoiceDateRow)
                                        {
                                            if (intInvoiceDateCol > -1)
                                            {
                                                DateTime dtInvoiceDateTime = new DateTime();

                                                strInvoiceDate = dt.Rows[intInvoiceDateRow][intInvoiceDateCol].ToString().Trim();
                                                string strDefaultText = item.strDefaultText;

                                                if (!string.IsNullOrEmpty(strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strInvoiceDate))
                                                    {
                                                        strInvoiceDate = strDefaultText;
                                                    }
                                                }

                                                string strMasktype = string.Empty;
                                                try
                                                {
                                                    if (item.PayorToolMaskFieldTypeId > 0)
                                                    {
                                                        tempListMaskFieldTypes = new ObservableCollection<MaskFieldTypes>(ListMaskFieldTypes.Where(p => p.PTMaskFieldTypeId == item.PayorToolMaskFieldTypeId));
                                                        strMasktype = tempListMaskFieldTypes.FirstOrDefault().Name;
                                                        strMasktype = strMasktype.Replace("*", "");
                                                        dtInvoiceDateTime = DateTime.ParseExact(strInvoiceDate, strMasktype, DateTimeFormatInfo.InvariantInfo);
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    //AddToDataBase("Issue in invoice date");
                                                    dtInvoiceDateTime = Convert.ToDateTime(strInvoiceDate);
                                                    try
                                                    {
                                                        ActionLogger.Logger.WriteImportLog("Mask type is: " + strMasktype + " And Date format is  " + strInvoiceDate + "Please save valid mask type", true);
                                                    }
                                                    catch { }
                                                }

                                                deuFields.DeuData.InvoiceDate = dtInvoiceDateTime;
                                                deudata.InvoiceDate = dtInvoiceDateTime;
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = dtInvoiceDateTime.ToShortDateString();

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = dtInvoiceDateTime.ToString("MM/dd/yyyy");
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strInvoiceDate;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }

                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading invoice date from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "InvoiceMonth":

                                    try
                                    {
                                        strInvoiceMonth = string.Empty;
                                        int intInvoiceMonthCol = -1;
                                        int intInvoiceMonthRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intInvoiceMonthCol = Convert.ToInt32(item.FixedColLocation);
                                            intInvoiceMonthCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceMonthCol);
                                            }
                                        }

                                        if (intEndDataRows > intInvoiceMonthRow)
                                        {
                                            if (intInvoiceMonthCol > -1)
                                            {
                                                try
                                                {
                                                    strInvoiceMonth = dt.Rows[intInvoiceMonthRow][intInvoiceMonthCol].ToString().Trim();

                                                    if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                    {
                                                        if (string.IsNullOrEmpty(strInvoiceMonth))
                                                        {
                                                            strInvoiceMonth = item.strDefaultText.Trim();
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading invoice month from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "InvoiceYear":

                                    try
                                    {
                                        strInvoiceYear = string.Empty;
                                        int intInvoiceYearCol = -1;
                                        int intInvoiceYearRow = i;
                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intInvoiceYearCol = Convert.ToInt32(item.FixedColLocation);
                                            intInvoiceYearCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceYearCol);
                                            }
                                        }

                                        if (intEndDataRows > intInvoiceYearRow)
                                        {
                                            if (intInvoiceYearCol > -1)
                                            {
                                                try
                                                {
                                                    strInvoiceYear = dt.Rows[intInvoiceYearRow][intInvoiceYearCol].ToString().Trim();                                                  
                                                    if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                    {
                                                        if (string.IsNullOrEmpty(strInvoiceYear))
                                                        {
                                                            strInvoiceYear = item.strDefaultText.Trim();
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading invoice year from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "EffectiveDate":
                                    try
                                    {
                                        string strEffectiveDate = string.Empty;
                                        int intEffectiveDateCol = -1;
                                        int intEffectiveDateDateRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            intEffectiveDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEffectiveDateCol);
                                            }
                                        }
                                        if (intEndDataRows > intEffectiveDateDateRow)
                                        {
                                            if (intEffectiveDateCol > -1)
                                            {
                                                strEffectiveDate = dt.Rows[intEffectiveDateDateRow][intEffectiveDateCol].ToString().Trim();

                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strEffectiveDate))
                                                    {
                                                        strEffectiveDate = item.strDefaultText.Trim();
                                                    }
                                                }
                                                //deuFields.DeuData.OriginalEffectiveDate = Convert.ToDateTime(strEffectiveDate);
                                                if (!string.IsNullOrEmpty(strEffectiveDate))
                                                {
                                                    if (strEffectiveDate.Contains("/"))
                                                    {
                                                        bool bValue = false;
                                                        strEffectiveDate = strEffectiveDate.Trim();
                                                        string[] ardate = strEffectiveDate.Split('/');

                                                        if (ardate[0].Length < 2)
                                                        {
                                                            ardate[0] = "0" + ardate[0];
                                                            bValue = true;
                                                        }

                                                        if (ardate[1].Length < 2)
                                                        {
                                                            ardate[1] = "0" + ardate[1];
                                                            bValue = true;
                                                        }
                                                        if (bValue)
                                                        {
                                                            strEffectiveDate = ardate[0] + "/" + ardate[1] + "/" + ardate[2];
                                                        }

                                                    }

                                                    deudata.OriginalEffectiveDate = Convert.ToDateTime(strEffectiveDate);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strEffectiveDate;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strEffectiveDate;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strEffectiveDate;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading effective date from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "PaymentReceived":
                                    try
                                    {
                                        string strPaymentReceived = string.Empty;
                                        int intPaymentReceivedCol = -1;
                                        int intPaymentReceivedRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intPaymentReceivedCol = Convert.ToInt32(item.FixedColLocation);
                                            intPaymentReceivedCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPaymentReceivedCol);
                                            }
                                        }

                                        if (intEndDataRows > intPaymentReceivedRow)
                                        {
                                            if (intPaymentReceivedCol > -1)
                                            {
                                                strPaymentReceived = dt.Rows[intPaymentReceivedRow][intPaymentReceivedCol].ToString().Trim();
                                              
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strPaymentReceived))
                                                    {
                                                        strPaymentReceived = item.strDefaultText.Trim();
                                                    }
                                                }
                                                //deuFields.DeuData.PaymentRecived = Convert.ToDecimal(strPaymentReceived);
                                                if (string.IsNullOrEmpty(strPaymentReceived))
                                                {
                                                    strPaymentReceived = "0";
                                                }
                                                if (string.IsNullOrWhiteSpace(strPaymentReceived))
                                                {
                                                    strPaymentReceived = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strPaymentReceived))
                                                {
                                                    deudata.PaymentRecived = Convert.ToDecimal(strPaymentReceived);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strPaymentReceived;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strPaymentReceived;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strPaymentReceived;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }

                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading payment recived from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "CommissionPercentage":
                                    try
                                    {
                                        string strCommissionPercentage = string.Empty;
                                        int intCommissionPercentageCol = -1;
                                        int intCommissionPercentageRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intCommissionPercentageCol = Convert.ToInt32(item.FixedColLocation);
                                            intCommissionPercentageCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCommissionPercentageCol);
                                            }
                                        }
                                        if (intEndDataRows > intCommissionPercentageRow)
                                        {
                                            if (intCommissionPercentageCol > -1)
                                            {
                                                strCommissionPercentage = dt.Rows[intCommissionPercentageRow][intCommissionPercentageCol].ToString().Trim();

                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionPercentage))
                                                    {
                                                        strCommissionPercentage = item.strDefaultText.Trim();
                                                    }
                                                }

                                                //deuFields.DeuData.CommissionPercentage = Convert.ToDouble(strCommissionPercentage);
                                                if (string.IsNullOrEmpty(strCommissionPercentage))
                                                {
                                                    strCommissionPercentage = "0";
                                                }
                                                if (string.IsNullOrWhiteSpace(strCommissionPercentage))
                                                {
                                                    strCommissionPercentage = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strCommissionPercentage))
                                                {
                                                    strCommissionPercentage = strCommissionPercentage.Replace("%", "");

                                                    deudata.CommissionPercentage = Convert.ToDouble(strCommissionPercentage);
                                                    //Newly added code
                                                    int? intValue = item.TransID;
                                                    deudata.CommissionPercentage = calculateCommisionPercentage(intValue, Convert.ToDouble(strCommissionPercentage));

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    //deuField.DeuFieldValue = strCommissionPercentage;
                                                    deuField.DeuFieldValue = Convert.ToString(deudata.CommissionPercentage);


                                                    abc.DeuFieldName = item.FieldsName;
                                                    //abc.DeuFieldValue = strCommissionPercentage;
                                                    abc.DeuFieldValue = Convert.ToString(deudata.CommissionPercentage);
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        //uniqueIdentifier.Text = strCommissionPercentage;
                                                        uniqueIdentifier.Text = Convert.ToString(deudata.CommissionPercentage);
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }

                                                if (item.CalculatedFields)
                                                {
                                                    try
                                                    {
                                                        string strExpression = item.FormulaExpression;
                                                        var ResultValue = new NCalc.Expression(strExpression).Evaluate();
                                                        if (ResultValue.ToString().Contains("Infinity") || ResultValue.ToString().Contains("NaN"))
                                                            ResultValue = 0;
                                                        //ExpressionResult result = ExpressionExecutor.ExecuteExpression(Expression);
                                                        strCommissionPercentage = ResultValue.ToString();
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading commission percentage from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Renewal":
                                    try
                                    {
                                        string strRenewal = string.Empty;
                                        int intRenewalCol = -1;
                                        int intRenewalRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intRenewalCol = Convert.ToInt32(item.FixedColLocation);
                                            intRenewalCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search    
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intRenewalCol);
                                            }
                                        }

                                        if (intEndDataRows > intRenewalRow)
                                        {
                                            if (intRenewalCol > -1)
                                            {
                                                strRenewal = dt.Rows[intRenewalRow][intRenewalCol].ToString().Trim();
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strRenewal))
                                                    {
                                                        strRenewal = item.strDefaultText.Trim();
                                                    }
                                                }
                                                //deuFields.DeuData.Renewal = strRenewal;
                                                deudata.Renewal = strRenewal;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strRenewal;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strRenewal;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strRenewal;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading renewal from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Enrolled":
                                    try
                                    {
                                        string strEnrolled = string.Empty;
                                        int intEnrolledCol = -1;
                                        int intEnrolledRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intEnrolledCol = Convert.ToInt32(item.FixedColLocation);
                                            intEnrolledCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEnrolledCol);
                                            }
                                        }

                                        if (intEndDataRows > intEnrolledRow)
                                        {
                                            if (intEnrolledCol > -1)
                                            {
                                                strEnrolled = dt.Rows[intEnrolledRow][intEnrolledCol].ToString().Trim();
                                                //deuFields.DeuData.Enrolled = strEnrolled;
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strEnrolled))
                                                    {
                                                        strEnrolled = item.strDefaultText.Trim();
                                                    }
                                                }

                                                deudata.Enrolled = strEnrolled;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strEnrolled;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strEnrolled;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strEnrolled;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading enrolled from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Eligible":
                                    try
                                    {

                                        string strEligible = string.Empty;
                                        int intEligibleCol = -1;
                                        int intEligibleRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intEligibleCol = Convert.ToInt32(item.FixedColLocation);
                                            intEligibleCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEligibleCol);
                                            }
                                        }
                                        if (intEndDataRows > intEligibleRow)
                                        {
                                            if (intEligibleCol > -1)
                                            {
                                                strEligible = dt.Rows[intEligibleRow][intEligibleCol].ToString().Trim();                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strEligible))
                                                    {
                                                        strEligible = item.strDefaultText.Trim();
                                                    }
                                                }
                                                deudata.Eligible = strEligible;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strEligible;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strEligible;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strEligible;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading eligible  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Link1":
                                    try
                                    {

                                        string strLink1 = string.Empty;
                                        int intLink1Col = -1;
                                        int intLink1Row = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intLink1Col = Convert.ToInt32(item.FixedColLocation);
                                            intLink1Col = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intLink1Col);
                                            }
                                        }
                                        if (intEndDataRows > intLink1Row)
                                        {
                                            if (intLink1Col > -1)
                                            {
                                                strLink1 = dt.Rows[intLink1Row][intLink1Col].ToString().Trim();
                                                //deuFields.DeuData.Link1 = strLink1;                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strLink1))
                                                    {
                                                        strLink1 = item.strDefaultText.Trim();
                                                    }
                                                }

                                                deudata.Link1 = strLink1;
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strLink1;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strLink1;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strLink1;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading link1  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "SplitPercentage":
                                    try
                                    {
                                        string strSplitPercentage = string.Empty;
                                        int intSplitPercentageCol = -1;
                                        int intSplitPercentageRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intSplitPercentageCol = Convert.ToInt32(item.FixedColLocation);
                                            intSplitPercentageCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intSplitPercentageCol);
                                            }
                                        }

                                        if (intEndDataRows > intSplitPercentageRow)
                                        {
                                            if (intSplitPercentageCol > -1)
                                            {
                                                strSplitPercentage = dt.Rows[intSplitPercentageRow][intSplitPercentageCol].ToString();
                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strSplitPercentage))
                                                    {
                                                        strSplitPercentage = item.strDefaultText.Trim();
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(strSplitPercentage))
                                                {
                                                    strSplitPercentage = "0";
                                                }

                                                if (string.IsNullOrWhiteSpace(strSplitPercentage))
                                                {
                                                    strSplitPercentage = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strSplitPercentage))
                                                {
                                                    strSplitPercentage = strSplitPercentage.Replace("%", "");
                                                    deudata.SplitPer = Convert.ToDouble(strSplitPercentage);
                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strSplitPercentage;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strSplitPercentage;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strSplitPercentage;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading split percentage from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "PolicyMode":
                                    try
                                    {
                                        string strPolicyMode = string.Empty;
                                        int intPolicyModeCol = -1;
                                        int intPolicyModeRow = i;
                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intPolicyModeCol = Convert.ToInt32(item.FixedColLocation);
                                            intPolicyModeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPolicyModeCol);
                                            }
                                        }
                                        if (intEndDataRows > intPolicyModeRow)
                                        {
                                            if (intPolicyModeCol > -1)
                                            {
                                                strPolicyMode = dt.Rows[intPolicyModeRow][intPolicyModeCol].ToString().Trim();
                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strPolicyMode))
                                                    {
                                                        strPolicyMode = item.strDefaultText.Trim();
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(strPolicyMode))
                                                {
                                                    strPolicyMode = "0";
                                                }
                                                if (string.IsNullOrWhiteSpace(strPolicyMode))
                                                {
                                                    strPolicyMode = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strPolicyMode))
                                                {
                                                    deudata.PolicyMode = Convert.ToInt32(strPolicyMode);
                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strPolicyMode;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strPolicyMode;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strPolicyMode;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading policy mode from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Carrier":
                                    try
                                    {
                                        string strCarrier = string.Empty;
                                        int intCarrierCol = -1;
                                        int intCarrierRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intCarrierCol = Convert.ToInt32(item.FixedColLocation);
                                            intCarrierCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCarrierCol);
                                            }

                                        }
                                        if (intEndDataRows > intCarrierRow)
                                        {
                                            if (intCarrierCol > -1)
                                            {
                                                strCarrier = dt.Rows[intCarrierRow][intCarrierCol].ToString().Trim();
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strCarrier))
                                                    {
                                                        strCarrier = item.strDefaultText.Trim();
                                                    }
                                                }
                                                deudata.CarrierName = strCarrier;
                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strCarrier;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strCarrier;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strCarrier;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading carrier  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Product":

                                    try
                                    {
                                        string strProduct = string.Empty;
                                        int intProductCol = -1;
                                        int intProductRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intProductCol = Convert.ToInt32(item.FixedColLocation);
                                            intProductCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intProductCol);
                                            }
                                        }
                                        if (intEndDataRows > intProductRow)
                                        {
                                            if (intProductCol > -1)
                                            {
                                                strProduct = dt.Rows[intProductRow][intProductCol].ToString().Trim();
                                                                                         
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strProduct))
                                                    {
                                                        strProduct = item.strDefaultText.Trim();
                                                    }
                                                }

                                                deudata.ProductName = strProduct;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strProduct;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strProduct;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strProduct;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading product from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "PayorSysId":
                                    try
                                    {
                                        string strPayorSysId = string.Empty;
                                        int intPayorSysIdCol = -1;
                                        int intPayorSysIdRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intPayorSysIdCol = Convert.ToInt32(item.FixedColLocation);
                                            intPayorSysIdCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPayorSysIdCol);
                                            }

                                        }
                                        if (intEndDataRows > intPayorSysIdRow)
                                        {
                                            if (intPayorSysIdCol > -1)
                                            {
                                                strPayorSysId = dt.Rows[intPayorSysIdRow][intPayorSysIdCol].ToString().Trim();
                                            
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strPayorSysId))
                                                    {
                                                        strPayorSysId = item.strDefaultText.Trim();
                                                    }
                                                }
                                                deudata.PayorSysID = strPayorSysId;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strPayorSysId;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strPayorSysId;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strPayorSysId;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading payorsys id  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "CompScheduleType":
                                    try
                                    {
                                        string strCompScheduleType = string.Empty;
                                        int intCompScheduleTypeCol = -1;
                                        int intCompScheduleTypeRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                          
                                            intCompScheduleTypeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCompScheduleTypeCol);
                                            }

                                        }
                                        if (intEndDataRows > intCompScheduleTypeRow)
                                        {
                                            if (intCompScheduleTypeCol > -1)
                                            {
                                                strCompScheduleType = dt.Rows[intCompScheduleTypeRow][intCompScheduleTypeCol].ToString().Trim();
                                                //deuFields.DeuData.CompScheduleType = strCompScheduleType;
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strCompScheduleType))
                                                    {
                                                        strCompScheduleType = item.strDefaultText.Trim();
                                                    }
                                                }

                                                deudata.CompScheduleType = strCompScheduleType;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strCompScheduleType;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strCompScheduleType;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strCompScheduleType;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }

                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading comp schudule type  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "CompType":
                                    try
                                    {
                                        string strCompType = string.Empty;
                                        int intCompTypeCol = -1;
                                        int intCompTypeRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intCompTypeCol = Convert.ToInt32(item.FixedColLocation);
                                            intCompTypeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCompTypeCol);
                                            }
                                        }
                                        if (intEndDataRows > intCompTypeRow)
                                        {
                                            if (intCompTypeCol > -1)
                                            {
                                                strCompType = dt.Rows[intCompTypeRow][intCompTypeCol].ToString().Trim();
                                                // deuFields.DeuData.CompTypeID = Convert.ToInt32(strCompType);  
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strCompType))
                                                    {
                                                        strCompType = item.strDefaultText.Trim();
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strCompType))
                                                {
                                                    deudata.CompTypeID = Convert.ToInt32(strCompType);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strCompType;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strCompType;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strCompType;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading comp type from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Client":
                                    try
                                    {
                                        string strClient = string.Empty;
                                        int intClientCol = -1;
                                        int intClientRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intClientCol = Convert.ToInt32(item.FixedColLocation);
                                            intClientCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intClientCol);
                                            }
                                        }

                                        if (intEndDataRows > intClientRow)
                                        {
                                            if (intClientCol > -1)
                                            {
                                                strClient = dt.Rows[intClientRow][intClientCol].ToString().Trim();
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strClient))
                                                    {
                                                        strClient = item.strDefaultText.Trim();
                                                    }
                                                }

                                                deudata.ClientName = strClient;

                                                deuField.DeuFieldName = item.FieldsName;
                                                deuField.DeuFieldValue = strClient;

                                                abc.DeuFieldName = item.FieldsName;
                                                abc.DeuFieldValue = strClient;
                                                dueFields.Add(abc);

                                                if (item.PartOfPrimaryKey)
                                                {
                                                    uniqueIdentifier.ColumnName = item.FieldsName;
                                                    uniqueIdentifier.Text = strClient;
                                                    uniqueIdentifiers.Add(uniqueIdentifier);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading client  from files. " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "NumberOfUnits":
                                    try
                                    {
                                        string strNumberOfUnits = string.Empty;
                                        int intNumberOfUnitsCol = -1;
                                        int intNumberOfUnitsRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            // intNumberOfUnitsCol = Convert.ToInt32(item.FixedColLocation);
                                            intNumberOfUnitsCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intNumberOfUnitsCol);
                                            }
                                        }

                                        if (intEndDataRows > intNumberOfUnitsRow)
                                        {
                                            if (intNumberOfUnitsCol > -1)
                                            {
                                                strNumberOfUnits = dt.Rows[intNumberOfUnitsRow][intNumberOfUnitsCol].ToString().Trim();
                                                //deuFields.DeuData.NoOfUnits = Convert.ToInt32(strNumberOfUnits);
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strNumberOfUnits))
                                                    {
                                                        strNumberOfUnits = item.strDefaultText.Trim();
                                                    }
                                                }
                                                if (string.IsNullOrEmpty(strNumberOfUnits))
                                                {
                                                    strNumberOfUnits = "0";
                                                }
                                                if (string.IsNullOrWhiteSpace(strNumberOfUnits))
                                                {
                                                    strNumberOfUnits = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strNumberOfUnits))
                                                {
                                                    deudata.NoOfUnits = Convert.ToInt32(strNumberOfUnits);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strNumberOfUnits;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strNumberOfUnits;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strNumberOfUnits;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading number of unit  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "DollerPerUnit":

                                    string strDollerPerUnit = string.Empty;
                                    try
                                    {
                                        int intDollerPerUnitCol = -1;
                                        int intDollerPerUnitRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intDollerPerUnitCol = Convert.ToInt32(item.FixedColLocation);
                                            intDollerPerUnitCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intDollerPerUnitCol);
                                            }

                                        }
                                        if (intEndDataRows > intDollerPerUnitRow)
                                        {
                                            if (intDollerPerUnitCol > -1)
                                            {
                                                strDollerPerUnit = dt.Rows[intDollerPerUnitRow][intDollerPerUnitCol].ToString().Trim();
                                                // deuFields.DeuData.DollerPerUnit = Convert.ToDecimal(strDollerPerUnit);                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strDollerPerUnit))
                                                    {
                                                        strDollerPerUnit = item.strDefaultText.Trim();
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strDollerPerUnit)) || (string.IsNullOrWhiteSpace(strDollerPerUnit)))
                                                {
                                                    strDollerPerUnit = "0";
                                                }
                                                
                                                if (!string.IsNullOrEmpty(strDollerPerUnit))
                                                {
                                                    deudata.DollerPerUnit = Convert.ToDecimal(strDollerPerUnit);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strDollerPerUnit;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strDollerPerUnit;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strDollerPerUnit;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading Doller per unit  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Fee":
                                    try
                                    {
                                        string strFee = string.Empty;
                                        int intFeeCol = -1;
                                        int intFeeRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intFeeCol = Convert.ToInt32(item.FixedColLocation);
                                            intFeeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intFeeCol);
                                            }
                                        }

                                        if (intEndDataRows > intFeeRow)
                                        {
                                            if (intFeeCol > -1)
                                            {
                                                strFee = dt.Rows[intFeeRow][intFeeCol].ToString().Trim();                                              
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strFee))
                                                    {
                                                        strFee = item.strDefaultText.Trim();
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strFee)) || (string.IsNullOrWhiteSpace(strFee)))
                                                {
                                                    strFee = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strFee))
                                                {
                                                    deudata.Fee = Convert.ToDecimal(strFee);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strFee;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strFee;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strFee;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading fee  from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "Bonus":
                                    try
                                    {
                                        string strBonus = string.Empty;
                                        int intBonusCol = -1;
                                        int intBonusRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intBonusCol = Convert.ToInt32(item.FixedColLocation);
                                            intBonusCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intBonusCol);
                                            }
                                        }

                                        if (intEndDataRows > intBonusRow)
                                        {
                                            if (intBonusCol > -1)
                                            {
                                                strBonus = dt.Rows[intBonusRow][intBonusCol].ToString().Trim();                                               
                                               
                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strBonus))
                                                    {
                                                        strBonus = item.strDefaultText.Trim();
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strBonus)) || (string.IsNullOrWhiteSpace(strBonus)))
                                                {
                                                    strBonus = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strBonus))
                                                {
                                                    deudata.Bonus = Convert.ToDecimal(strBonus);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strBonus;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strBonus;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strBonus;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading bonus from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                case "CommissionTotal":
                                    try
                                    {
                                        string strCommissionTotal = string.Empty;
                                        int intCommissionTotalCol = -1;
                                        int intCommissionTotalRow = i;

                                        if (!string.IsNullOrEmpty(item.FixedColLocation))
                                        {
                                            //intCommissionTotalCol = Convert.ToInt32(item.FixedColLocation);
                                            intCommissionTotalCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                        }
                                        else
                                        {
                                            //Go for relative search
                                            if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                            {
                                                int rows = -1;
                                                GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCommissionTotalCol);
                                            }
                                        }

                                        if (intEndDataRows > intCommissionTotalRow)
                                        {
                                            if (intCommissionTotalCol > -1)
                                            {
                                                strCommissionTotal = dt.Rows[intCommissionTotalRow][intCommissionTotalCol].ToString().Trim();
                                                //deuFields.DeuData.CommissionTotal = Convert.ToDecimal(strCommissionTotal);.
                                                //deuFields.DeuData.Bonus =Convert.ToDecimal(strBonus);

                                                if (!string.IsNullOrEmpty(item.strDefaultText.Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionTotal))
                                                    {
                                                        strCommissionTotal = item.strDefaultText.Trim();
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strCommissionTotal)) || (string.IsNullOrWhiteSpace(strCommissionTotal)))
                                                {
                                                    strCommissionTotal = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strCommissionTotal))
                                                {
                                                    deudata.CommissionTotal = Convert.ToDecimal(strCommissionTotal);

                                                    dbCheackAmount += Convert.ToDecimal(strCommissionTotal);

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = strCommissionTotal;

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strCommissionTotal;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = strCommissionTotal;
                                                        uniqueIdentifiers.Add(uniqueIdentifier);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ActionLogger.Logger.WriteImportLog("Error while reading commission total from files " + ex.Message.ToString(), true);
                                    }
                                    break;

                                default:
                                    break;

                            }


                            if (item.CalculatedFields)
                            {
                                try
                                {
                                    string strVa = getExpression(item, dueFields);
                                    var ResultValue = new NCalc.Expression(strVa).Evaluate();
                                    if (ResultValue.ToString().Contains("Infinity") || ResultValue.ToString().Contains("NaN"))
                                        ResultValue = 0;

                                    DataEntryField itemValue = dueFields.Where(s => s.DeuFieldName.ToLower() == item.FieldsName.ToLower()).FirstOrDefault();

                                    if (itemValue != null)
                                        itemValue.DeuFieldValue = ResultValue.ToString();

                                }
                                catch
                                {
                                }
                            }

                            deuFields.DeuEntryId = Guid.Empty;
                            deuFields.BatchId = generatedBatchID;
                            deuFields.LicenseeId = licID;
                            deuFields.CurrentUser = guidSuperUser;
                            deuFields.StatementId = CurrentStatement.StatementID;
                            deuFields.PayorId = objImportToolPaymentDataSettings[0].PayorID;

                            GuidPid = objImportToolPaymentDataSettings[0].PayorID;

                            deuFields.DeuData = deudata;
                            deuFields.DeuFieldDataCollection = dueFields;

                        }

                        if (deuFields.DeuFieldDataCollection != null && deuFields.DeuFieldDataCollection.Count > 0)
                        {
                            //Guid guLienceID = new Guid();
                            //guLienceID = licID;
                            //Guid guPayorID = new Guid();
                            //guPayorID = GuidPid;
                            //deuFields.SearchedPolicyList = PostUtill.GetPoliciesFromUniqueIdentifier(uniqueIdentifiers, licID, GuidPid);
                            //getExpression

                            if ((!string.IsNullOrEmpty(strInvoiceMonth)) && (!string.IsNullOrEmpty(strInvoiceYear)))
                            {
                                try
                                {
                                    string strInvoice = validateDate(strInvoiceMonth, strInvoiceYear);
                                    if (!string.IsNullOrEmpty(strInvoice))
                                    {
                                        try
                                        {
                                            DateTime dtInvoice = Convert.ToDateTime(strInvoice);
                                            foreach (var item in deuFields.DeuFieldDataCollection)
                                            {
                                                if (item.DeuFieldName == "InvoiceDate")
                                                    item.DeuFieldValue = dtInvoice.ToString("MM/dd/yyyy");
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            ActionLogger.Logger.WriteImportLog("Start post proess,followup process and lerned process:  " + System.DateTime.Now, true);

                            DeuPostProcessWrapper.DeuPostStartWrapper(PostEntryProcess.FirstPost, deuFields, Guid.Empty, guidSuperUser, UserRole.SuperAdmin);

                            ActionLogger.Logger.WriteImportLog("End post proess,followup process and lerned process:  " + System.DateTime.Now, true);
                            //ActionLogger.Logger.WriteImportLog("process number :  " + i.ToString(), true);
                        }

                        if (isEndOfRead)
                        {
                            //Update batch to show in DEU and comp manager
                            //Copy folder path to successful folder.
                            //AddToDataBase("success");
                            try
                            {
                                UpdateBatch(generatedBatchID);
                                MoveToSuccesfullfolder();
                                //If Check amount address is not found
                                if (isCheckAmountAvailable == false)
                                {
                                    isCheckAmountAvailable = true;
                                    Statement objStatement = new Statement();
                                    if (intStatementnumber > 0)
                                    {
                                        objStatement.UpdateCheckAmount(intStatementnumber, dbCheackAmount, dbCheackAmount);
                                    }
                                }
                                //ActionLogger.Logger.WriteImportLog("Move to Succesfull folder :  " + System.DateTime.Now, true);
                                ActionLogger.Logger.WriteImportLog("*************************************", true);

                            }
                            catch (Exception ex)
                            {
                                //Force to move folder to successful 
                                MoveToSuccesfullfolder();
                                ActionLogger.Logger.WriteImportLog("Error found: " + ex.ToString(), true);
                                //ActionLogger.Logger.WriteImportLog("Move to Succesfull folder :  " + System.DateTime.Now, true);
                                ActionLogger.Logger.WriteImportLog("*************************************", true);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                //Copy folder path to Unsuccessful folder.
                //Send mail ..batch is not uploded successfully
                //AddToDataBase("Error:" + ex.Message.ToString());
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
                ActionLogger.Logger.WriteImportLog("Error :  " + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteImportLog("*************************************", true);
            }

        }

        private double calculateCommisionPercentage(int? intValue, double dbValue)
        {
            switch (intValue)
            {
                case 2:
                    dbValue = dbValue * 100;
                    break;

                case 3:
                    dbValue = dbValue / 100;
                    break;
                default:
                    break;
            }

            return dbValue;
        }

        private string validateDate(string strMonth, string strYear)
        {
            string strValue = string.Empty;
            int intmonth = 0;
            int intYear = 0;

            try
            {

                if (strMonth.Length <= 2)
                {
                    intmonth = Convert.ToInt32(strMonth);

                }

                if (strYear.Length == 4)
                {
                    intYear = Convert.ToInt32(strYear);
                }

                else if (strYear.Length == 2)
                {
                    strYear = "20" + strYear;
                    intYear = Convert.ToInt32(strYear);
                }

                else if (strYear.Length == 1)
                {
                    strYear = "200" + strYear;
                    intYear = Convert.ToInt32(strYear);
                }

                if (intmonth > 0 && intmonth < 13 && intYear > 0)
                {
                    strValue = "01" + "/" + strMonth + "/" + strYear;
                }
            }
            catch
            {
            }

            return strValue;
        }

        private void UpdateBatch(Guid batchID)
        {
            try
            {
                Batch objBatch = new Batch();
                int CompleteUnpaid = 6;
                int intManual = 4;
                objBatch.UpdateBatchByBatchId(batchID, CompleteUnpaid, intManual);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while updating batch status  :" + ex.Message.ToString(), true);

            }
        }

        private void MoveToTempfolder()
        {
            try
            {
                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Processing\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;

                if (File.Exists(strFilePath))
                {
                    File.Move(strFilePath, strNewLocation);
                }

                ActionLogger.Logger.WriteImportLog("File moved to temp folder", true);
            }
            catch (Exception ex)
            {
                AddToDataBase("Error while moving file to temp folder" + ex.Message.ToString());
            }
        }

        private void MoveToSuccesfullfolder()
        {
            try
            {
                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Success\" + strFileFullName;

                if (File.Exists(strFilePath))
                {
                    File.Move(strFilePath, strNewLocation);
                }

                ActionLogger.Logger.WriteImportLog("Move to succesfull folder", true);
            }
            catch (Exception ex)
            {
                AddToDataBase("Error while moving file to succesfull folder" + ex.Message.ToString());
            }
        }

        private void MoveToUnSuccesfullfolder()
        {
            try
            {
                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Unsuccess\" + strFileFullName;
                if (File.Exists(strFilePath))
                {
                    File.Move(strFilePath, strNewLocation);
                }
                ActionLogger.Logger.WriteImportLog("Move to Unsuccesfull folder ", true);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while moving to Unsuccesfull folder  " + ex.ToString(), true);
            }
        }

        public DEUFields getDeuFormFieldsValue(List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings, Guid licID)
        {
            DEUFields deuFields = new DEUFields();

            try
            {
                List<DataEntryField> deuFormFields = getDueFormFieldsValue(objImportToolPaymentDataSettings);

                deuFields.DeuEntryId = Guid.Empty;

                deuFields.BatchId = generatedBatchID;
                deuFields.LicenseeId = licID;
                deuFields.CurrentUser = guidSuperUser;
                deuFields.StatementId = CurrentStatement.StatementID;
                deuFields.PayorId = objImportToolPaymentDataSettings[0].PayorID;

                deuFields.DeuFieldDataCollection = deuFormFields;


            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while manageging deu fields  :" + ex.Message.ToString(), true);

            }

            return deuFields;
        }

        public List<DataEntryField> getDueFormFieldsValue(List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings)
        {
            List<DataEntryField> dueFields = new List<DataEntryField>();
            DataEntryField deuField = null;
            try
            {
                for (int index = 0; index < objImportToolPaymentDataSettings.Count; index++)
                {
                    deuField = new DataEntryField();
                    deuField.DeuFieldName = objImportToolPaymentDataSettings[index].FieldsName;
                    dueFields.Add(deuField);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog(ex.Message.ToString(), true);

            }
            return dueFields;
        }

        private void LoadMaskType()
        {
            try
            {
                PayorTemplate objPayorTemplate = new PayorTemplate();
                ListMaskFieldTypes = objPayorTemplate.AllMaskType();
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while getting mask type  :" + ex.Message.ToString(), true);

            }
        }

        private int EnddataRowsLocation(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, string strStartRowsData, string strEndDataIndicator)
        {
            List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();

            PayorTemplate objPayorTemplateCode = new PayorTemplate();
            objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();

            int dtRowsCount = dt.Rows.Count;
            bool isEndOfRead = false;
            int intDataRows = 0;

            try
            {

                for (int i = 0; i < dtRowsCount; i++)
                {
                    if (i > Convert.ToInt32(strStartRowsData) - 1)
                    {
                        foreach (var item in objImportToolPaymentDataSettings)
                        {
                            switch (item.FieldsName)
                            {
                                case "PolicyNumber":

                                    string strPolicyNumber = string.Empty;
                                    int intCol = -1;
                                    int intRow = i;
                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCol);
                                        }
                                    }

                                    if (intCol > -1)
                                    {
                                        strPolicyNumber = dt.Rows[intRow][intCol].ToString();
                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strPolicyNumber))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i;
                                                    return intDataRows;
                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "ModelAvgPremium":
                                    string strModelAvgpremium = string.Empty;

                                    int intModelAvgpremiumCol = -1;
                                    int intModelAvgpremiumRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intModelAvgpremiumCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative serch
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intModelAvgpremiumCol);
                                        }
                                    }

                                    if (intModelAvgpremiumCol > -1)
                                    {
                                        strModelAvgpremium = dt.Rows[intModelAvgpremiumRow][intModelAvgpremiumCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strModelAvgpremium))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i;
                                                    return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;
                                case "Insured":
                                    string strInsured = string.Empty;
                                    int intinsuredCol = -1;
                                    int intinsuredRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intinsuredCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative serch
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intinsuredCol);
                                        }
                                    }

                                    if (intinsuredCol > -1)
                                    {
                                        strInsured = dt.Rows[intinsuredRow][intinsuredCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strInsured))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i;
                                                    return intDataRows;

                                                }

                                            }
                                        }

                                    }

                                    break;
                                case "OriginalEffectiveDate":
                                    string strOriginalEffectiveDate = string.Empty;
                                    int intOriginalEffectiveDateCol = -1;
                                    int intOriginalEffectiveDateRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intOriginalEffectiveDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search   
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intOriginalEffectiveDateCol);
                                        }
                                    }

                                    if (intOriginalEffectiveDateCol > -1)
                                    {
                                        strOriginalEffectiveDate = dt.Rows[intOriginalEffectiveDateRow][intOriginalEffectiveDateCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strOriginalEffectiveDate))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "InvoiceDate":
                                    string strInvoiceDate = string.Empty;

                                    int intInvoiceDateCol = -1;
                                    int intInvoiceDateRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intInvoiceDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceDateCol);
                                        }

                                    }

                                    if (intInvoiceDateCol > -1)
                                    {
                                        strInvoiceDate = dt.Rows[intInvoiceDateRow][intInvoiceDateCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strInvoiceDate))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }
                                            }
                                        }
                                    }

                                    break;
                                case "InvoiceMonth":

                                    string strInvoiceMonth = string.Empty;

                                    int intInvoiceMonthCol = -1;
                                    int intInvoiceMonthRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intInvoiceMonthCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceMonthCol);
                                        }
                                    }

                                    if (intInvoiceMonthCol > -1)
                                    {
                                        strInvoiceMonth = dt.Rows[intInvoiceMonthRow][intInvoiceMonthCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strInvoiceMonth))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }
                                            }
                                        }
                                    }

                                    break;

                                case "InvoiceYear":

                                    string strInvoiceYear = string.Empty;

                                    int intInvoiceYearCol = -1;
                                    int intInvoiceYearRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intInvoiceYearCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intInvoiceYearCol);
                                        }
                                    }

                                    if (intInvoiceYearCol > -1)
                                    {
                                        strInvoiceYear = dt.Rows[intInvoiceYearRow][intInvoiceYearCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strInvoiceYear))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }
                                            }
                                        }
                                    }

                                    break;

                                case "EffectiveDate":
                                    string strEffectiveDate = string.Empty;
                                    int intEffectiveDateCol = -1;
                                    int intEffectiveDateDateRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intEffectiveDateCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEffectiveDateCol);
                                        }

                                    }

                                    if (intEffectiveDateCol > -1)
                                    {
                                        strEffectiveDate = dt.Rows[intEffectiveDateDateRow][intEffectiveDateCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strEffectiveDate))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;
                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "PaymentReceived":
                                    string strPaymentReceived = string.Empty;
                                    int intPaymentReceivedCol = -1;
                                    int intPaymentReceivedRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intPaymentReceivedCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPaymentReceivedCol);
                                        }

                                    }

                                    if (intPaymentReceivedCol > -1)
                                    {
                                        strPaymentReceived = dt.Rows[intPaymentReceivedRow][intPaymentReceivedCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strPaymentReceived))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "CommissionPercentage":
                                    string strCommissionPercentage = string.Empty;
                                    int intCommissionPercentageCol = -1;
                                    int intCommissionPercentageRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCommissionPercentageCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCommissionPercentageCol);
                                        }
                                    }

                                    if (intCommissionPercentageCol > -1)
                                    {
                                        strCommissionPercentage = dt.Rows[intCommissionPercentageRow][intCommissionPercentageCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strCommissionPercentage))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;
                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Renewal":
                                    string strRenewal = string.Empty;
                                    int intRenewalCol = -1;
                                    int intRenewalRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intRenewalCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intRenewalCol);
                                        }
                                    }

                                    if (intRenewalCol > -1)
                                    {
                                        strRenewal = dt.Rows[intRenewalRow][intRenewalCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strRenewal))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;
                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Enrolled":
                                    string strEnrolled = string.Empty;
                                    int intEnrolledCol = -1;
                                    int intEnrolledRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intEnrolledCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEnrolledCol);
                                        }

                                    }
                                    if (intEnrolledCol > -1)
                                    {
                                        strEnrolled = dt.Rows[intEnrolledRow][intEnrolledCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strEnrolled))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Eligible":
                                    string strEligible = string.Empty;
                                    int intEligibleCol = -1;
                                    int intEligibleRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intEligibleCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intEligibleCol);
                                        }

                                    }

                                    if (intEligibleCol > -1)
                                    {
                                        strEligible = dt.Rows[intEligibleRow][intEligibleCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strEligible))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Link1":
                                    string strLink1 = string.Empty;
                                    int intLink1Col = -1;
                                    int intLink1Row = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intLink1Col = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intLink1Col);
                                        }

                                    }

                                    if (intLink1Col > -1)
                                    {
                                        strLink1 = dt.Rows[intLink1Row][intLink1Col].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strLink1))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "SplitPercentage":
                                    string strSplitPercentage = string.Empty;
                                    int intSplitPercentageCol = -1;
                                    int intSplitPercentageRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intSplitPercentageCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intSplitPercentageCol);
                                        }

                                    }

                                    if (intSplitPercentageCol > -1)
                                    {
                                        strSplitPercentage = dt.Rows[intSplitPercentageRow][intSplitPercentageCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strSplitPercentage))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }

                                    }

                                    break;

                                case "PolicyMode":
                                    string strPolicyMode = string.Empty;
                                    int intPolicyModeCol = -1;
                                    int intPolicyModeRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intPolicyModeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPolicyModeCol);
                                        }

                                    }

                                    if (intPolicyModeCol > -1)
                                    {
                                        strPolicyMode = dt.Rows[intPolicyModeRow][intPolicyModeCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strPolicyMode))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }

                                    }

                                    break;

                                case "Carrier":
                                    string strCarrier = string.Empty;
                                    int intCarrierCol = -1;
                                    int intCarrierRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCarrierCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCarrierCol);
                                        }

                                    }

                                    if (intCarrierCol > -1)
                                    {
                                        strCarrier = dt.Rows[intCarrierRow][intCarrierCol].ToString();
                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strCarrier))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Product":
                                    string strProduct = string.Empty;
                                    int intProductCol = -1;
                                    int intProductRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intProductCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intProductCol);
                                        }

                                    }

                                    if (intProductCol > -1)
                                    {
                                        strProduct = dt.Rows[intProductRow][intProductCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strProduct))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "PayorSysId":
                                    string strPayorSysId = string.Empty;
                                    int intPayorSysIdCol = -1;
                                    int intPayorSysIdRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intPayorSysIdCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intPayorSysIdCol);
                                        }

                                    }

                                    if (intPayorSysIdCol > -1)
                                    {
                                        strPayorSysId = dt.Rows[intPayorSysIdRow][intPayorSysIdCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strPayorSysId))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "CompScheduleType":
                                    string strCompScheduleType = string.Empty;
                                    int intCompScheduleTypeCol = -1;
                                    int intCompScheduleTypeRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCompScheduleTypeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCompScheduleTypeCol);
                                        }

                                    }

                                    if (intCompScheduleTypeCol > -1)
                                    {
                                        strCompScheduleType = dt.Rows[intCompScheduleTypeRow][intCompScheduleTypeCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strCompScheduleType))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "CompType":
                                    string strCompType = string.Empty;
                                    int intCompTypeCol = -1;
                                    int intCompTypeRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCompTypeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCompTypeCol);
                                        }

                                    }

                                    if (intCompTypeCol > -1)
                                    {
                                        strCompType = dt.Rows[intCompTypeRow][intCompTypeCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strCompType))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Client":
                                    string strClient = string.Empty;
                                    int intClientCol = -1;
                                    int intClientRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intClientCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intClientCol);
                                        }

                                    }

                                    if (intClientCol > -1)
                                    {
                                        strClient = dt.Rows[intClientRow][intClientCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strClient))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "NumberOfUnits":
                                    string strNumberOfUnits = string.Empty;
                                    int intNumberOfUnitsCol = 0;
                                    int intNumberOfUnitsRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intNumberOfUnitsCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intNumberOfUnitsCol);
                                        }

                                    }

                                    if (intNumberOfUnitsCol > -1)
                                    {
                                        strNumberOfUnits = dt.Rows[intNumberOfUnitsRow][intNumberOfUnitsCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strNumberOfUnits))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "DollerPerUnit":
                                    string strDollerPerUnit = string.Empty;
                                    int intDollerPerUnitCol = -1;
                                    int intDollerPerUnitRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intDollerPerUnitCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intDollerPerUnitCol);
                                        }

                                    }
                                    if (intDollerPerUnitCol > -1)
                                    {
                                        strDollerPerUnit = dt.Rows[intDollerPerUnitRow][intDollerPerUnitCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strDollerPerUnit))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Fee":
                                    string strFee = string.Empty;
                                    int intFeeCol = -1;
                                    int intFeeRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intFeeCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intFeeCol);
                                        }

                                    }

                                    if (intFeeCol > -1)
                                    {
                                        strFee = dt.Rows[intFeeRow][intFeeCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strFee))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "Bonus":
                                    string strBonus = string.Empty;
                                    int intBonusCol = -1;
                                    int intBonusRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intBonusCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intBonusCol);
                                        }

                                    }
                                    if (intBonusCol > -1)
                                    {
                                        strBonus = dt.Rows[intBonusRow][intBonusCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strBonus))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i; return intDataRows;

                                                }

                                            }
                                        }
                                    }

                                    break;

                                case "CommissionTotal":
                                    string strCommissionTotal = string.Empty;
                                    int intCommissionTotalCol = -1;
                                    int intCommissionTotalRow = i;

                                    if (!string.IsNullOrEmpty(item.FixedColLocation))
                                    {
                                        intCommissionTotalCol = Convert.ToInt32(item.FixedColLocation) - 1;
                                    }
                                    else
                                    {
                                        //Go for relative search
                                        if ((!string.IsNullOrEmpty(item.RelativeRowLocation)) && (!string.IsNullOrEmpty(item.RelativeColLocation)))
                                        {
                                            int rows = -1;
                                            GetRelativeLocation(dt, item.HeaderSearch, Convert.ToInt32(item.RelativeRowLocation), Convert.ToInt32(item.RelativeColLocation), out rows, out intCommissionTotalCol);
                                        }

                                    }

                                    if (intCommissionTotalCol > -1)
                                    {
                                        strCommissionTotal = dt.Rows[intCommissionTotalRow][intCommissionTotalCol].ToString();

                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                        {
                                            if (string.IsNullOrEmpty(strCommissionTotal))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    isEndOfRead = true;
                                                    intDataRows = i;
                                                    return intDataRows;
                                                }

                                            }
                                        }
                                    }

                                    break;

                                default:
                                    break;
                            }

                        }

                        if (isEndOfRead)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while finding end data rows :" + ex.Message.ToString(), true);

            }
            if (intDataRows == 0)
            {
                intDataRows = dtRowsCount;
            }
            return intDataRows;

        }

        private bool IsAllPaymentDataFieldsIsAvailableIntoFile(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt)
        {
            bool bValue = false;
            //Get selected fields at payor and template
            //Match to whole data table columns
            //If Match then return true else return false           
            return bValue;
        }

        private string GetStatementDataValue(DataTable dt, ImportToolStatementDataSettings item)
        {
            int row = -1;
            int col = -1;
            string strRelativeSerch = string.Empty;

            string strValue = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(item.FixedColLocation) && !string.IsNullOrEmpty(item.FixedRowLocation))
                {
                    //row = Convert.ToInt32(item.FixedRowLocation);
                    //col = Convert.ToInt32(item.FixedColLocation);

                    row = Convert.ToInt32(item.FixedRowLocation) - 1;
                    col = Convert.ToInt32(item.FixedColLocation) - 1;

                    if (row > -1 && col > -1)
                    {
                        strValue = dt.Rows[row][col].ToString();

                    }
                }
                else if (!string.IsNullOrEmpty(item.FixedColLocation) && !string.IsNullOrEmpty(item.FixedRowLocation))
                {
                    //row = Convert.ToInt32(item.RelativeRowLocation);
                    //col = Convert.ToInt32(item.RelativeColLocation);

                    row = Convert.ToInt32(item.RelativeRowLocation) - 1;
                    col = Convert.ToInt32(item.RelativeColLocation) - 1;

                    strRelativeSerch = item.RelativeSearch;

                    int intSearchRows = -1;
                    int intSearchCols = -1;

                    if (!string.IsNullOrEmpty(strRelativeSerch))
                    {
                        if (dt != null)
                        {
                            //Get New Rows and columns
                            GetRelativeLocation(dt, strRelativeSerch, row, col, out intSearchRows, out intSearchCols);

                            if (intSearchRows > -1 && intSearchCols > -1)
                            {
                                strValue = dt.Rows[intSearchRows][intSearchCols].ToString();
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error :" + ex.Message.ToString(), true);

            }

            return strValue;
        }

        private List<Guid> SerchBrkerCode(DataTable dt, ObservableCollection<ImportToolBrokerSetting> AllAvailableBrokerCode)
        {
            List<Guid> licList = new List<Guid>();

            try
            {

                int row = -1;
                int col = -1;
                string strRelativeSerch = string.Empty;

                foreach (var item in AllAvailableBrokerCode)
                {
                    if (!string.IsNullOrEmpty(item.FixedRows) && !string.IsNullOrEmpty(item.FixedColumns))
                    {
                        row = Convert.ToInt32(item.FixedRows);
                        col = Convert.ToInt32(item.FixedColumns);

                        if (row > -1 && col > -1)
                        {
                            Guid? licID = GetLincessID(dt, row, col);

                            if (licID != null)
                            {
                                licList.Add((Guid)licID);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.RelativeRows) && !string.IsNullOrEmpty(item.RelativeColumns))
                    {
                        row = Convert.ToInt32(item.RelativeRows);
                        col = Convert.ToInt32(item.RelativeColumns);
                        strRelativeSerch = item.RelativeSearchtext;

                        int intSearchRows = -1;
                        int intSearchCols = -1;

                        if (!string.IsNullOrEmpty(strRelativeSerch))
                        {
                            if (dt != null)
                            {
                                //Get New Rows and columns
                                GetRelativeLocation(dt, strRelativeSerch, row, col, out intSearchRows, out intSearchCols);

                                if (intSearchRows > -1 && intSearchCols > -1)
                                {
                                    Guid? licID = GetLincessID(dt, intSearchRows, intSearchCols);

                                    if (licID != null)
                                    {
                                        licList.Add((Guid)licID);
                                    }
                                }

                            }

                        }
                    }
                }

                licList = new List<Guid>(licList.Distinct());
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while serching broker code  :" + ex.Message.ToString(), true);

            }

            return licList;
        }

        private Guid? GetLincessID(DataTable dt, int rows, int colums)
        {
            string strBrokerCode = dt.Rows[rows][colums].ToString();
            Guid? linceeID = null;
            Brokercode objbrokerCode = new Brokercode();
            try
            {
                bool bvalue = objbrokerCode.ValidateBrokerCode(strBrokerCode);
                if (bvalue == false)//means broker code found
                {
                    List<DisplayBrokerCode> lstBrokerCode = objbrokerCode.GetBrokerCodeByBrokerName(strBrokerCode);
                    selectedDisplayBrokerCode = lstBrokerCode.FirstOrDefault();
                    linceeID = selectedDisplayBrokerCode.licenseeID;
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error :" + ex.Message.ToString(), true);

            }
            return linceeID;
        }

        private void GetRelativeLocation(DataTable dt, string strRelativeSerch, int relativeRows, int relativeCol, out int intSearchRows, out int intSearchCols)
        {
            bool isBoolBreak = false;
            intSearchRows = -1;
            intSearchCols = -1;

            try
            {

                if (string.IsNullOrEmpty(strRelativeSerch))
                {
                    return;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i][j])))
                        {
                            string strValue = dt.Rows[i][j].ToString().ToLower().Trim();

                            if (strValue.Equals(strRelativeSerch.ToLower().Trim()))
                            {
                                intSearchRows = i;
                                intSearchCols = j;
                                isBoolBreak = true;
                                break;
                            }
                        }
                    }
                    if (isBoolBreak)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while finding relative location  :" + ex.Message.ToString(), true);

            }

            intSearchRows = intSearchRows + relativeRows;
            intSearchCols = intSearchCols + relativeCol;
        }

        public DataTable ConvretExcelToDataTable(string FilePath)
        {
            //AddToDataBase("start reading");
            string strConn = string.Empty;
            DataTable dt = null;

            if (FilePath.Trim().EndsWith(".xlsx"))
            {
                //AddToDataBase("start xlsx");
                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1\";", FilePath);
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter da = null;
                dt = new DataTable("Temp");
                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    //AddToDataBase("open success");
                    //cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", conn);
                    string strSheetName = getSheetName(conn);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                    cmd = new OleDbCommand(strQuery, conn);

                    cmd.CommandType = CommandType.Text;
                    da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLog("Error while trying to open .xlsx files  :" + ex.Message.ToString(), true);
                    MoveToTempfolder();
                    MoveToUnSuccesfullfolder();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Dispose();
                    cmd.Dispose();
                    da.Dispose();
                }

            }
            else if (FilePath.Trim().EndsWith(".xls"))
            {
                //strConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=No;IMEX=1\";", FilePath);
                string header = "No";
                string pathOnly = string.Empty;
                string fileName = string.Empty;
                pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                fileName = System.IO.Path.GetFileName(FilePath);
                strConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=" + header + ";IMEX=1\";", FilePath);
                //strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";Extended Properties=\"Excel 8.0;HDR=" + header + "\"";
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter adapter = null;
                dt = new DataTable(fileName);

                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    string strSheetName = getSheetName(conn);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                    cmd = new OleDbCommand(strQuery, conn);
                    cmd.CommandType = CommandType.Text;
                    adapter = new OleDbDataAdapter(cmd);
                    adapter.Fill(dt);

                    AddToDataBase("Dataset created");
                    //MoveToTempfolder();
                    //AddToDataBase("File moved to temp folder");
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLog("Error while trying to open .xls files  :" + ex.Message.ToString(), true);
                    MoveToTempfolder();
                    MoveToUnSuccesfullfolder();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Dispose();
                    cmd.Dispose();
                    adapter.Dispose();
                }

            }
            else if (FilePath.Trim().EndsWith(".txt"))
            {

                //Read the file as one string.
                System.IO.StreamReader txtFiles = new System.IO.StreamReader(FilePath);
                string myString = txtFiles.ReadToEnd();

                string[] arstring = myString.Split('\r');

                DataRow dr;
                dt = new DataTable();
                DataColumn col = new DataColumn("test");
                col.DataType = System.Type.GetType("System.String");
                dt.Columns.Add(col);

                foreach (var item in arstring)
                {
                    dr = dt.NewRow();
                    dr[0] = item.ToString();
                    dt.Rows.Add(dr);
                }

                txtFiles.Close();
                // Suspend the screen.
                Console.ReadLine();
                return dt;

            }

            else if (FilePath.Trim().EndsWith(".csv"))
            {
                string header = "No";
                string sql = string.Empty;
                dt = null;
                string pathOnly = string.Empty;
                string fileName = string.Empty;
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter adapter = null;
                try
                {

                    pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                    fileName = System.IO.Path.GetFileName(FilePath);
                    sql = @"SELECT * FROM [" + fileName + "]";
                    using (conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly + ";Extended Properties=\"Text;HDR=" + header + "\""))
                    {
                        using (cmd = new OleDbCommand(sql, conn))
                        {
                            using (adapter = new OleDbDataAdapter(cmd))
                            {
                                dt = new DataTable();
                                dt.Locale = CultureInfo.CurrentCulture;
                                adapter.Fill(dt);

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLog("Error while trying to open .csv files  :" + ex.Message.ToString(), true);
                    MoveToTempfolder();
                    MoveToUnSuccesfullfolder();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Dispose();
                    cmd.Dispose();
                    adapter.Dispose();
                }
            }

            return dt;

        }

        private string getSheetName(OleDbConnection ObjConn)
        {
            string strSheetName = String.Empty;
            try
            {
                System.Data.DataTable dtSheetNames = ObjConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dtSheetNames.Rows.Count > 0)
                {
                    strSheetName = dtSheetNames.Rows[0]["TABLE_NAME"].ToString();
                }

            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while getting excel file name  :" + ex.Message.ToString(), true);
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
            }

            return strSheetName;
        }

        private void AddToDataBase(string strValue)
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlConnection sqlCon = new SqlConnection();
            try
            {
                using (sqlCon = new SqlConnection(strConn))
                {

                    Guid ID = new Guid();
                    ID = Guid.NewGuid();
                    string strCommand = "INSERT INTO  Test VALUES ('" + ID + "','" + strValue + "')";

                    sqlCmd = new SqlCommand(strCommand, sqlCon);
                    sqlCon.Open();
                    int i = (int)sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //if error then rollback transaction
                sqlCon.Close();
                ActionLogger.Logger.WriteImportLog(ex.ToString(), true);

            }
            finally
            {
                sqlCmd = null;
                sqlCon.Close();
            }
        }

        private DataSet GetListOftemplatePhrase()
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlConnection sqlCon = new SqlConnection();
            DataSet ds = new DataSet();
            try
            {
                using (sqlCon = new SqlConnection(strConn))
                {
                    string strCommand = "SELECT TemplateID,COUNT(TemplateID) AS TotalPhrase FROM ImportToolPayorPhrase GROUP BY TemplateID ORDER BY TotalPhrase DESC";

                    SqlDataAdapter sda = new SqlDataAdapter(strCommand, sqlCon);
                    sda.Fill(ds, "TemplatePhrase");
                }
            }
            catch (Exception ex)
            {

                ActionLogger.Logger.WriteImportLog("Error while trying to get all template phrase  :" + ex.Message.ToString(), true);
            }
            finally
            {
                sqlCmd = null;
                sqlCon.Close();
            }
            return ds;
        }

        private DataSet AllPhraseBytemplateID(Guid TemplateID)
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlConnection sqlCon = new SqlConnection();
            DataSet ds = new DataSet();
            try
            {
                using (sqlCon = new SqlConnection(strConn))
                {
                    string strCommand = "SELECT * FROM ImportToolPayorPhrase WHERE TemplateID=" + "'" + TemplateID + "'";

                    SqlDataAdapter sda = new SqlDataAdapter(strCommand, sqlCon);
                    sda.Fill(ds, "AllPhrase");
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while try to read all phrase by template id  :" + ex.Message.ToString(), true);

            }
            finally
            {
                sqlCmd = null;
                sqlCon.Close();
            }
            return ds;
        }
    }

    public class FormulaBindingData : INotifyPropertyChanged
    {
       
        public FormulaBindingData()
        {
           
        }

        private ObservableCollection<ExpressionToken> _operators;
        public ObservableCollection<ExpressionToken> Operators
        {
            get { return _operators; }
            set
            {
                _operators = value;
                OnPropertyChanged("Operators");
            }
        }

        private ObservableCollection<ExpressionToken> _variables;
        public ObservableCollection<ExpressionToken> Variables
        {
            get { return _variables; }
            set
            {
                _variables = value;
                OnPropertyChanged("Variables");
            }
        }

        private string _expression;
        public string Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                OnPropertyChanged("Expression");
            }
        }

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public enum ExpressionTokenType
    {
        Operator = 1,
        Variable = 2,
        OpenParanthesis = 3,
        CloseParathesis = 4,
        Value = 5
    }

    public class ExpressionToken : INotifyPropertyChanged
    {
        private string _TokenString;
        public string TokenString
        {
            get { return _TokenString; }
            set
            {
                _TokenString = value;
                OnPropertyChanged("TokenString");
            }
        }

        private ExpressionTokenType _TokenType;
        public ExpressionTokenType TokenType
        {
            get { return _TokenType; }
            set
            {
                _TokenType = value;
                OnPropertyChanged("TokenType");
            }
        }

        /// <summary>
        /// ExpressionToken that can be stack.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<ExpressionToken> getOperatorExprTokens()
        {
            ObservableCollection<ExpressionToken> expressionToken = new ObservableCollection<ExpressionToken>();

            ExpressionToken token = new ExpressionToken { TokenString = "+", TokenType = ExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "-", TokenType = ExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "*", TokenType = ExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "/", TokenType = ExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "(", TokenType = ExpressionTokenType.OpenParanthesis };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = ")", TokenType = ExpressionTokenType.CloseParathesis };
            expressionToken.Add(token);

            return expressionToken;
        }

        public static ObservableCollection<ExpressionToken> getVariableExprTokens()
        {
            ObservableCollection<ExpressionToken> expressionToken = new ObservableCollection<ExpressionToken>();

            ExpressionToken token = new ExpressionToken { TokenString = "Item1", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "Item2", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "Item3", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "Item4", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "Item5", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ExpressionToken { TokenString = "Item6", TokenType = ExpressionTokenType.Variable };
            expressionToken.Add(token);

            return expressionToken;
        }

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// 
    /// </summary>
    public class ExpressionStack : Stack<ExpressionToken>
    {
        public int ParanthesisCount = 0;
        public string Expression = string.Empty;
        public string TempExpression = string.Empty;
        public int Length = 0;

        public ExpressionStack() { }

        /// <summary>
        /// Causion :- T
        /// </summary>
        /// <param name="expression"></param>
        public ExpressionStack(string expression)
        {
            if (string.IsNullOrEmpty(expression.Trim()))
                return;

            this.Expression = string.Empty;
            this.TempExpression = expression;

            ExpressionToken tkn = getExpressionToken();

            do
            {
                this.PushToken(tkn);
                tkn = getExpressionToken();
            } while (tkn != null);

            Length = this.PeekToken().TokenString.Length;
        }

        private ExpressionToken getExpressionToken()
        {
            string tkn = ExtractToken();

            if (string.IsNullOrEmpty(tkn.Trim()))
                return null;

            ExpressionToken expToken = new ExpressionToken();
            switch (tkn)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    expToken.TokenType = ExpressionTokenType.Operator;
                    expToken.TokenString = tkn;
                    break;
                case "(":
                    expToken.TokenType = ExpressionTokenType.OpenParanthesis;
                    expToken.TokenString = tkn;
                    break;
                case ")":
                    expToken.TokenType = ExpressionTokenType.CloseParathesis;
                    expToken.TokenString = tkn;
                    break;
                default:
                    if (tkn == "100")
                    {
                        expToken.TokenType = ExpressionTokenType.Value;
                        expToken.TokenString = tkn;
                    }
                    else
                    {
                        expToken.TokenType = ExpressionTokenType.Variable;
                        expToken.TokenString = tkn;
                    }
                    break;
            }
            return expToken;
        }

        private string ExtractToken()
        {
            string token = string.Empty;
            List<string> OperatorList = new List<string> { "+", "-", "*", "/", "(", ")" };
            int index = 0;

            TempExpression = TempExpression.Trim();
            if (TempExpression.Length != 0)
            {
                if (OperatorList.Contains(TempExpression[0].ToString()))
                {
                    index = 1;
                }
                else
                {
                    index = 0;
                    while ((index < TempExpression.Length) && (!OperatorList.Contains(TempExpression[index].ToString())))
                    {
                        index++;
                    }
                }
            }

            token = TempExpression.Substring(0, index);
            TempExpression = TempExpression.Remove(0, index);
            return token;
        }

        public List<ExpressionToken> getExpressionTokenList()
        {
            return this.Reverse().ToList();
        }

        public void ClearFormula()
        {
            Expression = string.Empty;
            Length = 0;
            Clear();
        }

        public void PushToken(ExpressionToken token)
        {
            ExpressionToken peekToken = PeekToken();
            //bool replaceCase = false;

            //if ((peekToken != null) && (peekToken.TokenType == token.TokenType))
            //    replaceCase = true;

            //if (replaceCase == false)
            //{
            if (!ExpressionValidationRule.ValidationRule(peekToken, token))
                return;
            //}

            if (token.TokenType == ExpressionTokenType.OpenParanthesis)
                ParanthesisCount++;

            if (token.TokenType == ExpressionTokenType.CloseParathesis)
                ParanthesisCount--;

            //if (replaceCase)
            //{
            //    PopToken();
            //    Push(token);
            //}
            //else
            //{
            Push(token);
            //}

            Expression += token.TokenString;
            Length = token.TokenString.Length;
        }

        public ExpressionToken PopToken()
        {
            ExpressionToken expToken = Pop();

            if (expToken.TokenType == ExpressionTokenType.OpenParanthesis)
                ParanthesisCount--;

            if (expToken.TokenType == ExpressionTokenType.CloseParathesis)
                ParanthesisCount++;

            Expression = Expression.Substring(0, Expression.Length - Length);

            if (PeekToken() != null)
                Length = PeekToken().TokenString.Length;
            else
                Length = 0;

            return expToken;
        }

        private ExpressionToken PeekToken()
        {
            if (this.Count != 0)
                return Peek();
            else
                return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class ExpressionValidationRule
    {
        public bool CanVariableAppended;
        public bool CanOperatorAppended;
        public bool CanOpenParanthesisAppended;
        public bool CanClosedParanthesisAppended;
        public bool CanStartExpression;

        private static ExpressionValidationRule Rule;

        private ExpressionValidationRule() { }

        private static ExpressionValidationRule getValidationRule()
        {
            if (Rule == null)
                Rule = new ExpressionValidationRule();
            return Rule;
        }

        public static bool ValidationRule(ExpressionToken peekToken, ExpressionToken nextToken)
        {
            bool allowed = false;
            try
            {
                ExpressionValidationRule valRule = null;

                if (nextToken == null)
                    return allowed;

                if (peekToken == null)
                {
                    valRule = getExpressionRule(nextToken.TokenType);
                    if (valRule.CanStartExpression)
                        allowed = true;
                }
                else
                {
                    valRule = getExpressionRule(peekToken.TokenType);

                    switch (nextToken.TokenType)
                    {
                        case ExpressionTokenType.Operator:
                            if (valRule.CanOperatorAppended)
                                allowed = true;
                            break;
                        case ExpressionTokenType.Variable:
                        case ExpressionTokenType.Value:
                            if (valRule.CanVariableAppended)
                                allowed = true;
                            break;
                        case ExpressionTokenType.OpenParanthesis:
                            if (valRule.CanOpenParanthesisAppended)
                                allowed = true;
                            break;
                        case ExpressionTokenType.CloseParathesis:
                            if (valRule.CanClosedParanthesisAppended)
                                allowed = true;
                            break;
                        default:
                            break;
                    }
                }

            }
            catch { }
            return allowed;
        }

        private static ExpressionValidationRule getExpressionRule(ExpressionTokenType tokenType)
        {

            ExpressionValidationRule token = getValidationRule();
            if (token != null)
            {
                token.CanClosedParanthesisAppended = false;
                token.CanOpenParanthesisAppended = false;
                token.CanOperatorAppended = false;
                token.CanStartExpression = false;
                token.CanVariableAppended = false;

                switch (tokenType)
                {
                    case ExpressionTokenType.Operator:
                        token.CanOpenParanthesisAppended = true;
                        token.CanVariableAppended = true;
                        break;
                    case ExpressionTokenType.Variable:
                    case ExpressionTokenType.Value:
                        token.CanClosedParanthesisAppended = true;
                        token.CanOperatorAppended = true;
                        token.CanStartExpression = true;
                        break;
                    case ExpressionTokenType.OpenParanthesis:
                        token.CanVariableAppended = true;
                        token.CanOpenParanthesisAppended = true;
                        token.CanStartExpression = true;
                        break;
                    case ExpressionTokenType.CloseParathesis:
                        token.CanClosedParanthesisAppended = true;
                        token.CanOperatorAppended = true;
                        break;
                    default:
                        break;
                }
            }
            return token;

        }
    }   
}
