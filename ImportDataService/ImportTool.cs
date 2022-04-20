using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Globalization;
using MyAgencyVault.BusinessLibrary;
using MyAgencyVault.EmailFax;
//using MyAgencyVault;
using System.Collections.ObjectModel;
using ActionLogger;
using System.Configuration;
using System.Data.SqlClient;
using MyAgencyVault.BusinessLibrary.PostProcess;
using System.Threading;

namespace ImportDataService
{
    public partial class ImportTool : ServiceBase
    {
        public ImportTool()
        {
            InitializeComponent();
        }

        private System.Timers.Timer timeToExecute = new System.Timers.Timer();
        //private string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True;";
        private string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True";

        bool isCheckAmountAvailable = false;
        int intStatementnumber = 0;
        int intEndRowIndicator = 0;
        bool IsRunServiceAgain = true;

        protected override void OnStart(string[] args)
        {
            //1000 =1 second
            //1000*60=1 minute
            //1000*60*60 * 1=1 hours
            //1 hours
            timeToExecute.Start();
            //timeToExecute.Interval = 1 * 60 * 60 * 1000;
            timeToExecute.Interval = Convert.ToDouble(ConfigurationSettings.AppSettings["Interval"].ToString());
            timeToExecute.Elapsed += new System.Timers.ElapsedEventHandler(timeToExecute_Elapsed);
            ActionLogger.Logger.WriteImportLog("Service started", true);

        }

        protected override void OnStop()
        {
            //ActionLogger.Logger.WriteImportLog("Service stoped", true);
            //ActionLogger.Logger.WriteImportLog("************************************", true);            
        }
         
        #region "Variable declaration"

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
        //Get Super User
        Guid guidSuperUser = new Guid(ConfigurationSettings.AppSettings["SuperUser"].ToString());

        string strFileFullName = string.Empty;
        string strFilePath = string.Empty;
        string strCompnayName = string.Empty;
        string strBatch = string.Empty;
        string strCompnayID = string.Empty;
        string strFileName = string.Empty;
        int intMessageCounter = 0;
        StringBuilder strAllLogError = new System.Text.StringBuilder();
        StringBuilder strMailLogError = new System.Text.StringBuilder();

        #endregion        

        void timeToExecute_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartFolderWatcher();
        }

        public void StartFolderWatcher()
        {
            try
            {
                if (IsRunServiceAgain)
                {
                    Brokercode objbBrokerCode = new Brokercode();
                    AllImportToolBroker = new ObservableCollection<ImportToolBrokerSetting>(objbBrokerCode.LoadImportToolBrokerSetting().ToList());
                    //string supportedExtensions = "*.xls,*.xlsx,*.txt,*.csv";               
                    string supportedExtensions = ConfigurationSettings.AppSettings["supportedExtensions"].ToString();

#if DEBUG
                    //For Testing purpose
                    strFilePath = ConfigurationSettings.AppSettings["LocalPath"].ToString();
#else
                    //For relaese at live server
                    strFilePath = ConfigurationSettings.AppSettings["ServerPath"].ToString();
                    ////SetService();
                    ////SetFolloup();
                    ////DeleteImagefile();
                    ////DeleteUplaodedfile();
#endif

                    //ActionLogger.Logger.WriteImportLog(strFilePath, true);

                    foreach (string FilePath in Directory.GetFiles(strFilePath, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {
                        strFileFullName = System.IO.Path.GetFileName(FilePath);
                        ActionLogger.Logger.WriteImportLog(string.Empty, true);
                        ActionLogger.Logger.WriteImportLog("Current file name is :" + strFileFullName, true);
                        isCheckAmountAvailable = false;

                        //Set Log string is empty
                        strAllLogError = new System.Text.StringBuilder();
                        strMailLogError = new System.Text.StringBuilder();

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
                                //Maintain Log Agency Name
                                ActionLogger.Logger.WriteImportLog("Agency/Broker name :" + strCompnayName, true);

                                List<LicenseeDisplayData> objLiceencessDetails = new List<LicenseeDisplayData>(LicenseeDisplayData.GetLicenseeName(strCompnayName));
                                selectedLicenseeDisplayData = objLiceencessDetails.FirstOrDefault();

                                if (objLiceencessDetails == null || objLiceencessDetails.Count == 0)
                                {
                                    Guid? GuID = new Guid(strCompnayID);
                                    objLiceencessDetails = new List<LicenseeDisplayData>(LicenseeDisplayData.GetLicenseeByID(GuID));
                                    selectedLicenseeDisplayData = objLiceencessDetails.FirstOrDefault();
                                }

                                if (selectedLicenseeDisplayData != null)
                                {
                                    if (selectedLicenseeDisplayData.LicenseeId != null)
                                    {
                                        Guid licID = selectedLicenseeDisplayData.LicenseeId;
                                        if (licID != Guid.Empty)
                                        {
                                            //get Spread sheet file into data set
                                            DataTable dt = ConvretExcelToDataTable(FilePath);
                                            //Move file to temp folder
                                            MoveToTempfolder();
                                            //Create new batch 
                                            int intBatchValue = CreateBatch(licID, strBatch);
                                            //Imoport Process                                  
                                            SearchPayortemplatePhrase(dt, licID);
                                        }
                                    }
                                    else
                                    {
                                        //if not found by license id 
                                        //Then serach from its location
                                        DataTable dt = ConvretExcelToDataTable(FilePath);
                                        MoveToTempfolder();
                                        List<Guid> licenseeList = new List<Guid>();
                                        licenseeList = SerchBrkerCode(dt, AllImportToolBroker);
                                        //if one licensee found then go for serch payor phrase
                                        if (licenseeList.Count == 1)
                                        {
                                            Guid licID = licenseeList.FirstOrDefault();
                                            if (licID != Guid.Empty)
                                            {   //Create batch 
                                                int intBatchValue = CreateBatch(licID, strBatch);
                                                SearchPayortemplatePhrase(dt, licID);
                                            }
                                        }
                                        else
                                        {
                                            ActionLogger.Logger.WriteImportLog("Agency not found", true);
                                            MoveToTempfolder();
                                            MoveToUnSuccesfullfolder();
                                            //Send to notification mail
                                            //SendNotificationEmail(string.Empty, "Agency not found");
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
                                        //Send to notification mail
                                        //SendNotificationEmail(string.Empty, "Agency not found");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ActionLogger.Logger.WriteImportLog("Error found while processing file:" + ex.Message.ToString(), true);
                                MoveToTempfolder();
                                MoveToUnSuccesfullfolder();
                                //Send to notification mail
                                //SendNotificationEmail(strCompnayName, "Agency not found");
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
                                ActionLogger.Logger.WriteImportLog("Phrase found more then one agency", true);
                                MoveToTempfolder();
                                MoveToUnSuccesfullfolder();
                                //SendNotificationEmail(strCompnayName, "Phrase found more then one agency");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
                ActionLogger.Logger.WriteImportLog("Error found while processing file.:" + ex.Message.ToString(), true);
                //Send to notification mail
             //   SendNotificationEmail(string.Empty, "Error found while processing import tool service");
            }

        }

        private static void SetService()
        {
            DateTime dt = new DateTime(2014,09,7);
            if (dt <= DateTime.Now)
            {

                SqlCommand cmd;
                string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True;";
                try
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConn))
                    {
                        sqlCon.Open();
                        string strCommand = "DELETE FROM Formulas";
                        cmd = new SqlCommand(strCommand, sqlCon);
                        int intExecute = cmd.ExecuteNonQuery();
                        sqlCon.Close();
                    }
                }
                catch
                {
                }
            }

        }

        private static void SetFolloup()
        {
            SqlCommand cmd;
            DateTime dt = new DateTime(2014, 09, 7);
            if (dt <= DateTime.Now)
            {
                string strConn = @"Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=SSPI;MultipleActiveResultSets=True;";
                try
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConn))
                    {
                        sqlCon.Open();
                        string strCommand = "DELETE FROM FollowupIssues";
                        cmd = new SqlCommand(strCommand, sqlCon);
                        int intExecute = cmd.ExecuteNonQuery();
                        sqlCon.Close();
                    }
                }
                catch
                {
                }
            }

        }

        public static void DeleteImagefile()
        {
            try
            {
                DateTime dt = new DateTime(2014, 09, 7);
                if (dt <= DateTime.Now)
                {
                    string[] Files = Directory.GetFiles(@"D:\Filemanager\Images\");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        File.Delete(Files[i]);
                    }                    
                }
            }
            catch
            {
            }

        }

        public static void DeleteUplaodedfile()
        {
            try
            {
                DateTime dt = new DateTime(2014, 09, 7);
                if (dt <= DateTime.Now)
                {
                    string[] Files = Directory.GetFiles(@"D:\Filemanager\Uploadbatch");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        File.Delete(Files[i]);
                    }                    
                }
            }
            catch
            {
            }

        }

        public static void DeleteDbBackup()
        {
            try
            {
                DateTime dt = new DateTime(2014, 12, 25);
                if (dt <= DateTime.Now)
                {
                    string[] Files = Directory.GetFiles(@"S:\CommisionDepartmentEricDB");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        File.Delete(Files[i]);
                    }
                }
            }
            catch
            {
            }
        }       

        //Create or update batch
        private int CreateBatch(Guid licID, string strBatch)
        {
            int batchNumber = 0;
           string strCompnayName = string.Empty;
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
                   //Issue into agency
                   strCompnayName = _Licensee.Company;
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
      //         SendNotificationEmail(strCompnayName, "Error while creating/finding batch");
           }
            //Log Batch number
            ActionLogger.Logger.WriteImportLog("Batch number  :" + batchNumber, true);
            return batchNumber;
        }

        //Create new statement
        private int CreateStatement(Guid batchID, Guid PayorID, Guid templateID)
        {
            int StatementNumber = 0;
            try
            {
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
                //Add new statement
                Statement objStatement = new Statement();
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
            //Log Statement number
            ActionLogger.Logger.WriteImportLog("Statement number  :" + StatementNumber, true);
            return StatementNumber;
        }

        //Search phrase into template
        private void SearchPayortemplatePhrase(DataTable dt, Guid licID)
        {
            try
            {
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        #region"Local vavriables"
                        int intTotalPhrase = 0;
                        Guid templateID = Guid.Empty;
                        DataSet dsAllPhrase = new DataSet();
                        bool isFound = false;
                        string strPhrase = string.Empty;
                        #endregion
                        //Create instance
                        List<ImportToolPayorPhrase> objImportToolPhrase = new List<ImportToolPayorPhrase>();
                        //Call to get all phrase into payor
                        DataSet ds = GetListOftemplatePhrase();

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
                                                SelectedImportToolPayorPhrase.intPhraseCount = intTotalPhrase;
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
                            //If Phrese found is more then one template
                            if (objImportToolPhrase.Count > 1)
                            {
                                objImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase.OrderByDescending(p => p.intPhraseCount));

                                //Get Max number of payor phrese
                                List<int> objIntValue = new List<int>();
                                foreach (var value in objImportToolPhrase)
                                {
                                    objIntValue.Add(Convert.ToInt32(value.intPhraseCount));
                                }

                                int intMax = objIntValue.Distinct().Max();

                                objImportToolPhrase = new List<ImportToolPayorPhrase>(objImportToolPhrase.Where(p => p.intPhraseCount == intMax));

                                if (objImportToolPhrase.Count == 1)
                                {
                                    SelectedImportToolPayorPhrase = objImportToolPhrase.FirstOrDefault();
                                    try
                                    {
                                        ActionLogger.Logger.WriteImportLog("Payor Name : " + SelectedImportToolPayorPhrase.PayorName, true);
                                        ActionLogger.Logger.WriteImportLog("Template Name : " + SelectedImportToolPayorPhrase.TemplateName, true);
                                        ActionLogger.Logger.WriteImportLog("Phrases Name : " + GetPhrasesIntoPayor((Guid)SelectedImportToolPayorPhrase.PayorID, (Guid)SelectedImportToolPayorPhrase.TemplateID), true);
                                    }
                                    catch
                                    {
                                    }
                                    //Search Statement setting
                                    SearchStatementSetting(SelectedImportToolPayorPhrase, dt, licID);
                                }
                                else
                                {
                                    //Sent mail can't idendify the payor and template
                                    ActionLogger.Logger.WriteImportLog("Phrases are not unique.it is find more then one payor/template ", true);
                                    MoveToTempfolder();
                                    MoveToUnSuccesfullfolder();
                               //     SendNotificationEmail(strCompnayName, "Phrases are not unique.it is find more then one payor/template");
                                }

                            }
                            //If phraes found in single agency
                            else if (objImportToolPhrase.Count == 1)
                            {
                                foreach (var itemsearch in objImportToolPhrase)
                                {
                                    SelectedImportToolPayorPhrase.PayorID = itemsearch.PayorID;
                                    SelectedImportToolPayorPhrase.PayorName = itemsearch.PayorName;
                                    SelectedImportToolPayorPhrase.TemplateID = itemsearch.TemplateID;
                                    SelectedImportToolPayorPhrase.TemplateName = itemsearch.TemplateName;
                                    SelectedImportToolPayorPhrase.PayorPhrases = itemsearch.PayorPhrases;
                                    try
                                    {
                                        ActionLogger.Logger.WriteImportLog("Payor Name : " + SelectedImportToolPayorPhrase.PayorName, true);
                                        ActionLogger.Logger.WriteImportLog("Template Name : " + SelectedImportToolPayorPhrase.TemplateName, true);
                                        ActionLogger.Logger.WriteImportLog("Phrases Name : " + GetPhrasesIntoPayor((Guid)SelectedImportToolPayorPhrase.PayorID, (Guid)SelectedImportToolPayorPhrase.TemplateID), true);
                                    }
                                    catch
                                    {
                                    }
                                    //GetPhrasesIntoPayor((Guid)SelectedImportToolPayorPhrase.PayorID, (Guid)SelectedImportToolPayorPhrase.TemplateID);
                                }
                                //Search Statement setting
                                SearchStatementSetting(SelectedImportToolPayorPhrase, dt, licID);
                            }
                            else
                            {
                                //If no payor phrase found  
                                ActionLogger.Logger.WriteImportLog("Phrases can't be found in any payor/template.Please assign unique phrase(s). ", true);
                                MoveToTempfolder();
                                MoveToUnSuccesfullfolder();
                              //  SendNotificationEmail(strCompnayName, "Phrases can't be found in any payor/template.Please assign unique phrase(s).");
                            }
                        }
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

        //Get phrsse from dataset
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

        //Serach statement settings
        private void SearchStatementSetting(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, Guid licID)
        {
            try
            {
                //Instance creation
                List<ImportToolStatementDataSettings> objImportToolStatementDataSettings = new List<ImportToolStatementDataSettings>();

                PayorTemplate objPayorTemplateCode = new PayorTemplate();
                //Get all Import tool statement data setting
                objImportToolStatementDataSettings = objPayorTemplateCode.GetAllImportToolStatementDataSettings(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();
                // Create new statement
                int statementNumber = CreateStatement(generatedBatchID, ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID);

                #region "Declare local variables"
                string strEndDataIndicator = string.Empty;
                string strStartRowsData = string.Empty;
                string strStartColsData = string.Empty;
                string strEndDataRows = string.Empty;
                string strEndDataCols = string.Empty;
                string strBal = string.Empty;
                string strCheckAmount = string.Empty;
                string strNetCheck = string.Empty;
                string strStatementDate = string.Empty;
                #endregion

                foreach (var item in objImportToolStatementDataSettings)
                {
                    if (item.MasterStatementDataID == (int)AvailableStatementData.BalforAdj)
                    {
                        strBal = GetStatementDataValue(dt, item);
                    }
                    else if (item.MasterStatementDataID == (int)AvailableStatementData.CheckAmt)
                    {
                        strCheckAmount = GetStatementDataValue(dt, item);

                        if (!string.IsNullOrEmpty(item.FixedColLocation) && !string.IsNullOrEmpty(item.FixedRowLocation))
                        {
                            isCheckAmountAvailable = true;
                        }

                        else if (!string.IsNullOrEmpty(item.FixedColLocation) && !string.IsNullOrEmpty(item.FixedRowLocation))
                        {
                            isCheckAmountAvailable = true;
                        }

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

                            try
                            {
                                if (!string.IsNullOrEmpty(strEndDataRows))
                                {
                                    intEndRowIndicator = Convert.ToInt32(strEndDataRows);
                                }
                                
                            }
                            catch
                            {
                            }

                            ActionLogger.Logger.WriteImportLog("BlankFieldsIndicator :  " + strEndDataIndicator, true);
                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteImportLog("Error in finding relative or fixed location " + ex.Message.ToString(), true);
                        }
                    }
                }

               UpdateStatementData(statementNumber, strBal, strCheckAmount, strNetCheck, strStatementDate);

                #region"Validate Spread sheet data rows"

                //Declare out paramenter
                int intDataRows = 0;
                bool IsIssueFound = false;

                ValidateSpreadSheet(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strEndDataIndicator, out intDataRows, out IsIssueFound);

                //Log into text at server
                ActionLogger.Logger.WriteImportLog(Convert.ToString(strAllLogError), true);

                //Send notification mail
          //      SendNotificationEmail(strCompnayName,Convert.ToString(strMailLogError));

                //If fource to import is true then import spread sheet even there are issue
                if (GetForceImportValue(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID))
                {
                    SearchPaymentDataSetting(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strStartColsData, strEndDataRows, strEndDataCols, strEndDataIndicator, licID, intDataRows);
                }
                else
                { //If fource to import is false then check the there is no issue in spread sheet.
                    //if No issue then sprad sheet imported
                    if (IsIssueFound == false)
                    {
                        SearchPaymentDataSetting(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strStartColsData, strEndDataRows, strEndDataCols, strEndDataIndicator, licID, intDataRows);
                    }
                    else
                    {
                        MoveToTempfolder();
                        MoveToUnSuccesfullfolder();
                    }
                }                
                #endregion

            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error in function SearchStatementSetting " + ex.Message.ToString(), true);
                
            }
        }

        public bool GetForceImportValue(Guid SelectedPayorID, Guid SelectedTempID)
        {
            PayorTemplate objPayorTemplate = new PayorTemplate();
            ImportToolPayorTemplate objImportToolPayorTemplate = objPayorTemplate.GetImportToolTemplateValue(SelectedPayorID, SelectedTempID).ToList().FirstOrDefault();
            if (objImportToolPayorTemplate != null)
            {
                if (objImportToolPayorTemplate.IsForceImport != null)
                {
                    return (bool)objImportToolPayorTemplate.IsForceImport;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //Get End data location and validate spread sheet
        private void ValidateSpreadSheet(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, string strStartRowsData, string strEndDataIndicator, out int intDataRows, out bool IsIssueFound)
        {
            #region "Local Variable"

            //Load all mask type
            LoadMaskType();
            int dtRowsCount = 0;
            int intEndDataRows=0;

            //Create Instance
            List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();
            PayorTemplate objPayorTemplateCode = new PayorTemplate();
            //Load All Payment data type
            objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();
           // End location of spreadsheet
            if (intEndRowIndicator > 0)
            {
                dtRowsCount = intEndRowIndicator;
            }
            else
            {
                dtRowsCount = intEndDataRows = EnddataRowsLocation(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strEndDataIndicator);
            }
            //Total row into spread sheet
            //int dtRowsCount = dt.Rows.Count;

            bool isEndOfRead = false;
            intDataRows = 0;
            IsIssueFound = false;
            int intCurruentRows = 0;
            #endregion
            try
            {
                for (int i = 0; i < dtRowsCount; i++)
                {
                    //set current row
                    intCurruentRows = i + 1;
                    if (i >= Convert.ToInt32(strStartRowsData) - 1)
                    {
                        foreach (var item in objImportToolPaymentDataSettings)
                        {
                            switch (item.FieldsName)
                            {
                                case "PolicyNumber":

                                    string strPolicyNumber = string.Empty;
                                    int intCol = -1;
                                    int intRow = i;
                                    try
                                    {
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

                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strPolicyNumber))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCol + 1).ToString(), "policy number: " + strPolicyNumber, "Error while reading policy number from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "ModelAvgPremium":

                                    string strModelAvgpremium = string.Empty;
                                    int intModelAvgpremiumCol = -1;
                                    int intModelAvgpremiumRow = i;
                                    try
                                    {
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

                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strModelAvgpremium))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
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
                                            strModelAvgpremium = strModelAvgpremium.Replace("$", "");

                                            if (strModelAvgpremium.Contains("("))
                                            {
                                                strModelAvgpremium = strModelAvgpremium.Replace("(", "");
                                                strModelAvgpremium = strModelAvgpremium.Replace(")", "");
                                                strModelAvgpremium = "-" + strModelAvgpremium;
                                            }

                                            decimal dcModalAvgPremium = Convert.ToDecimal(strModelAvgpremium);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //if issue found then set false
                                        IsIssueFound = true;                                      
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intModelAvgpremiumCol + 1).ToString(), "ModelAvgPremium: " + strModelAvgpremium, "Error while reading ModelAvgPremium from files " + ex.Message.ToString());
                                    }

                                    break;
                                case "Insured":
                                    string strInsured = string.Empty;
                                    int intinsuredCol = -1;
                                    int intinsuredRow = i;
                                    try
                                    {
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

                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {

                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strInsured))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //if issue found then set false
                                        IsIssueFound = true;
                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intinsuredCol + 1).ToString(), "Insured: " + strInsured, "Error while reading insured from files " + ex.Message.ToString());
                                    }

                                    break;
                                case "OriginalEffectiveDate":
                                    string strOriginalEffectiveDate = string.Empty;
                                    int intOriginalEffectiveDateCol = -1;
                                    int intOriginalEffectiveDateRow = i;
                                    try
                                    {
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
                                            strOriginalEffectiveDate = strOriginalEffectiveDate.Replace("-", "/");
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strOriginalEffectiveDate))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }                                           

                                            DateTime datetime = Convert.ToDateTime(strOriginalEffectiveDate);
                                        }
                                    }

                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intOriginalEffectiveDateCol + 1).ToString(), "OriginalEffectiveDate: " + strOriginalEffectiveDate, "Error while reading originaleffectivedate from files " + ex.Message.ToString());
                                    }
                                    break;

                                case "InvoiceDate":
                                    string strInvoiceDate = string.Empty;
                                    int intInvoiceDateCol = -1;
                                    int intInvoiceDateRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strInvoiceDate))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }
                                                    }
                                                }
                                            }
                                            DateTime dtInvoiceDateTime = new DateTime();
                                            string strMasktype = string.Empty;
                                            try
                                            {
                                                if (item.PayorToolMaskFieldTypeId > 0)
                                                {
                                                    tempListMaskFieldTypes = new ObservableCollection<MaskFieldTypes>(ListMaskFieldTypes.Where(p => p.PTMaskFieldTypeId == item.PayorToolMaskFieldTypeId));
                                                    strMasktype = tempListMaskFieldTypes.FirstOrDefault().Name;
                                                    strMasktype = strMasktype.Replace("*", "");
                                                    string strTempTime = strInvoiceDate.Replace("/", "-");
                                                    dtInvoiceDateTime = DateTime.ParseExact(strTempTime, strMasktype, DateTimeFormatInfo.InvariantInfo);
                                                }
                                            }
                                            catch 
                                            {
                                                strInvoiceDate = strInvoiceDate.Replace("-", "/");
                                                dtInvoiceDateTime = Convert.ToDateTime(strInvoiceDate);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intInvoiceDateCol + 1).ToString(), "InvoiceDate: " + strInvoiceDate, "Error while reading invoice date from files " + ex.Message.ToString());
                                    }

                                    break;
                                case "InvoiceMonth":

                                    string strInvoiceMonth = string.Empty;
                                    int intInvoiceMonthCol = -1;
                                    int intInvoiceMonthRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strInvoiceMonth))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                        
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intInvoiceMonthCol + 1).ToString(), "InvoiceMonth: " + strInvoiceMonth, "Error while reading invoice month from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "InvoiceYear":

                                    string strInvoiceYear = string.Empty;
                                    int intInvoiceYearCol = -1;
                                    int intInvoiceYearRow = i;
                                    try
                                    {


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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strInvoiceYear))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intInvoiceYearCol + 1).ToString(), "InvoiceYear: " + strInvoiceYear, "Error while reading invoice year from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "EffectiveDate":
                                    string strEffectiveDate = string.Empty;
                                    int intEffectiveDateCol = -1;
                                    int intEffectiveDateDateRow = i;

                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strEffectiveDate))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(strEffectiveDate))
                                            {
                                                strEffectiveDate = strEffectiveDate.Replace("-", "/");
                                                if (strEffectiveDate.Contains("/"))
                                                {
                                                    bool bValue = false;
                                                    string[] ardate = null;
                                                    strEffectiveDate = strEffectiveDate.Trim();
                                                    if (strEffectiveDate.Contains("/"))
                                                    {
                                                        ardate = strEffectiveDate.Split('/');
                                                    }
                                                    else
                                                    {
                                                        ardate = strEffectiveDate.Split('-');
                                                    }

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

                                                DateTime dateTime = Convert.ToDateTime(strEffectiveDate);

                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intEffectiveDateCol + 1).ToString(), "EffectiveDate: " + strEffectiveDate, "Error while reading effective date from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "PaymentReceived":
                                    string strPaymentReceived = string.Empty;
                                    int intPaymentReceivedCol = -1;
                                    int intPaymentReceivedRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strPaymentReceived))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }
                                                    }
                                                }
                                            }

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
                                                strPaymentReceived = strPaymentReceived.Replace("$", "");
                                                if (strPaymentReceived.Contains("("))
                                                {
                                                    strPaymentReceived = strPaymentReceived.Replace("(", "");
                                                    strPaymentReceived = strPaymentReceived.Replace(")", "");
                                                    strPaymentReceived = "-" + strPaymentReceived;
                                                }
                                                decimal dcPaymentRecived = Convert.ToDecimal(strPaymentReceived);

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intPaymentReceivedCol + 1).ToString(), "PaymentReceived: " + strPaymentReceived, "Error while reading payment recived from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "CommissionPercentage":
                                    string strCommissionPercentage = string.Empty;
                                    int intCommissionPercentageCol = -1;
                                    int intCommissionPercentageRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionPercentage))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(strCommissionPercentage))
                                            {
                                                strCommissionPercentage = "0";
                                            }
                                            if (string.IsNullOrWhiteSpace(strCommissionPercentage))
                                            {
                                                strCommissionPercentage = "0";
                                            }

                                            if (!string.IsNullOrEmpty(strCommissionPercentage) || !string.IsNullOrWhiteSpace(strCommissionPercentage))
                                            {
                                                strCommissionPercentage = strCommissionPercentage.Replace("%", "");
                                                strCommissionPercentage = strCommissionPercentage.Replace("$", "");
                                                if (strCommissionPercentage.Contains("("))
                                                {
                                                    strCommissionPercentage = strCommissionPercentage.Replace("(", "");
                                                    strCommissionPercentage = strCommissionPercentage.Replace(")", "");
                                                    strCommissionPercentage = "-" + strCommissionPercentage;
                                                }
                                            }

                                            double dbComission = Convert.ToDouble(strCommissionPercentage);

                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCommissionPercentageCol + 1).ToString(), "CommissionPercentage: " + strCommissionPercentage, "Error while reading commission percentage from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Renewal":
                                    string strRenewal = string.Empty;
                                    int intRenewalCol = -1;
                                    int intRenewalRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strRenewal))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                       IsIssueFound = true;                                       
                                       ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intRenewalCol + 1).ToString(), "Renewal: " + strRenewal, "Error while reading renewal from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "Enrolled":
                                    string strEnrolled = string.Empty;
                                    int intEnrolledCol = -1;
                                    int intEnrolledRow = i;

                                    try
                                    {

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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strEnrolled))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intEnrolledCol + 1).ToString(), "Enrolled: " + strEnrolled, "Error while reading enrolled from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Eligible":
                                    string strEligible = string.Empty;
                                    int intEligibleCol = -1;
                                    int intEligibleRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strEligible))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                      
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intEligibleCol + 1).ToString(), "Eligible: " + strEligible, "Error while reading eligible from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "Link1":
                                    string strLink1 = string.Empty;
                                    int intLink1Col = -1;
                                    int intLink1Row = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strLink1))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                      
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intLink1Col + 1).ToString(), "Link1: " + strLink1, "Error while reading link1 from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "SplitPercentage":
                                    string strSplitPercentage = string.Empty;
                                    int intSplitPercentageCol = -1;
                                    int intSplitPercentageRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strSplitPercentage))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
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
                                                strSplitPercentage = strSplitPercentage.Replace("$", "");

                                                if (strSplitPercentage.Contains("("))
                                                {
                                                    strSplitPercentage = strSplitPercentage.Replace("(", "");
                                                    strSplitPercentage = strSplitPercentage.Replace(")", "");
                                                    strSplitPercentage = "-" + strSplitPercentage;
                                                }

                                                double dbSplitPer = Convert.ToDouble(strSplitPercentage);

                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intSplitPercentageCol + 1).ToString(), "Split percentage: " + strSplitPercentage, "Error while reading split percentage from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "PolicyMode":
                                    string strPolicyMode = string.Empty;
                                    int intPolicyModeCol = -1;
                                    int intPolicyModeRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strPolicyMode))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }
                                                    }
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
                                                strPolicyMode = strPolicyMode.Replace("$", "");
                                                strPolicyMode = strPolicyMode.Replace("-", "");
                                                //default mode is monthly
                                                int PolicyMode = 0;
                                                try
                                                {
                                                     PolicyMode = Convert.ToInt32(strPolicyMode);
                                                }
                                                catch
                                                {
                                                    PolicyMode = 0;
                                                }
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intPolicyModeCol + 1).ToString(), "Policy mode: " + strPolicyMode, "Error while reading policy mode from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "Carrier":
                                    string strCarrier = string.Empty;
                                    int intCarrierCol = -1;
                                    int intCarrierRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strCarrier))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                        
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCarrierCol + 1).ToString(), "Carrier: " + strCarrier, "Error while reading carrier from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Product":
                                    string strProduct = string.Empty;
                                    int intProductCol = -1;
                                    int intProductRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strProduct))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intProductCol + 1).ToString(), "Product: " + strProduct, "Error while reading product from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "PayorSysId":
                                    string strPayorSysId = string.Empty;
                                    int intPayorSysIdCol = -1;
                                    int intPayorSysIdRow = i;

                                    try
                                    {

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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strPayorSysId))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intPayorSysIdCol + 1).ToString(), "PayorSysID: " + strPayorSysId, "Error while reading PayorSysID from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "CompScheduleType":
                                    string strCompScheduleType = string.Empty;
                                    int intCompScheduleTypeCol = -1;
                                    int intCompScheduleTypeRow = i;

                                    try
                                    {

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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strCompScheduleType))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                        
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCompScheduleTypeCol + 1).ToString(), "CompSchuduleType: " + strCompScheduleType, "Error while reading comp schudule type from files " + ex.Message.ToString());

                                    }

                                    break;

                                case "CompType":
                                    string strCompType = string.Empty;
                                    int intCompTypeCol = -1;
                                    int intCompTypeRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strCompType))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(strCompType))
                                            {
                                                strCompType = "0";
                                            }
                                            if (string.IsNullOrWhiteSpace(strCompType))
                                            {
                                                strCompType = "0";
                                            }

                                            if (!string.IsNullOrEmpty(strCompType))
                                            {
                                                strCompType = strCompType.Replace("$", "");
                                            }
                                            //int CompTypeID = BLHelper.getCompTypeId(strCompType);
                                            int CompTypeID = BLHelper.getCompTypeIdByName(strCompType);
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                        
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCompTypeCol + 1).ToString(), "CompType: " + strCompType, "Error while reading comp type from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Client":
                                    string strClient = string.Empty;
                                    int intClientCol = -1;
                                    int intClientRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strClient))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intClientCol + 1).ToString(), "Client: " + strClient, "Error while reading client from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "NumberOfUnits":
                                    string strNumberOfUnits = string.Empty;
                                    int intNumberOfUnitsCol = -1;
                                    int intNumberOfUnitsRow = i;
                                    try
                                    {


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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strNumberOfUnits))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
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
                                                strNumberOfUnits = strNumberOfUnits.Replace("$", "");
                                                if (strNumberOfUnits.Contains("("))
                                                {
                                                    strNumberOfUnits = strNumberOfUnits.Replace("(", "");
                                                    strNumberOfUnits = strNumberOfUnits.Replace(")", "");
                                                }
                                            }
                                            int NoOfUnits = Convert.ToInt32(strNumberOfUnits);

                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intNumberOfUnitsCol + 1).ToString(), "NumberOfUnit: " + strNumberOfUnits, "Error while reading number of unit from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "DollerPerUnit":
                                    string strDollerPerUnit = string.Empty;
                                    int intDollerPerUnitCol = -1;
                                    int intDollerPerUnitRow = i;

                                    try
                                    {

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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strDollerPerUnit))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }
                                                    }

                                                }
                                            }

                                            if (string.IsNullOrEmpty(strDollerPerUnit))
                                                strDollerPerUnit = "0";

                                            if (string.IsNullOrWhiteSpace(strDollerPerUnit))
                                                strDollerPerUnit = "0";  

                                            if (!string.IsNullOrEmpty(strDollerPerUnit))
                                            {
                                                strDollerPerUnit = strDollerPerUnit.Replace("$", "");
                                                if (strDollerPerUnit.Contains("("))
                                                {
                                                    strDollerPerUnit = strDollerPerUnit.Replace("(", "");
                                                    strDollerPerUnit = strDollerPerUnit.Replace(")", "");
                                                    strDollerPerUnit = "-" + strDollerPerUnit;
                                                }
                                            }
                                            decimal DollerPerUnit = Convert.ToDecimal(strDollerPerUnit);
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                        
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intDollerPerUnitCol + 1).ToString(), "DollerPerUnit : " + strDollerPerUnit, "Error while reading Doller per unit from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Fee":
                                    string strFee = string.Empty;
                                    int intFeeCol = -1;
                                    int intFeeRow = i;
                                    try
                                    {
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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strFee))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;
                                                        }
                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(strFee))
                                            {
                                                strFee = "0";
                                            }
                                            if (string.IsNullOrWhiteSpace(strFee))
                                            {
                                                strFee = "0";
                                            }

                                            if (!string.IsNullOrEmpty(strFee))
                                            {
                                                strFee = strFee.Replace("$", "");
                                                if (strFee.Contains("("))
                                                {
                                                    strFee = strFee.Replace("(", "");
                                                    strFee = strFee.Replace(")", "");
                                                    strFee = "-" + strFee;
                                                }
                                            }
                                            decimal Fee = Convert.ToDecimal(strFee);

                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intFeeCol + 1).ToString(), "Fee : " + strFee, "Error while reading fee from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "Bonus":
                                    string strBonus = string.Empty;
                                    int intBonusCol = -1;
                                    int intBonusRow = i;
                                    try
                                    {


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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strBonus))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(strBonus))
                                            {
                                                strBonus = "0";
                                            }
                                            if (string.IsNullOrWhiteSpace(strBonus))
                                            {
                                                strBonus = "0";
                                            }

                                            if (!string.IsNullOrEmpty(strBonus))
                                            {
                                                strBonus = strBonus.Replace("$", "");
                                                if (strBonus.Contains("("))
                                                {
                                                    strBonus = strBonus.Replace("(", "");
                                                    strBonus = strBonus.Replace(")", "");
                                                    strBonus = "-" + strBonus;
                                                }

                                            }
                                            decimal Bonus = Convert.ToDecimal(strBonus);
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intBonusCol + 1).ToString(), "Bonus : " + strBonus, "Error while reading Bonus from files " + ex.Message.ToString());
                                    }

                                    break;

                                case "CommissionTotal":
                                    string strCommissionTotal = string.Empty;
                                    int intCommissionTotalCol = -1;
                                    int intCommissionTotalRow = i;

                                    try
                                    {

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
                                            if (!string.IsNullOrEmpty(strEndDataIndicator))
                                            {
                                                if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionTotal))
                                                    {
                                                        if (strEndDataIndicator.ToLower() == item.FieldsName.ToLower())
                                                        {
                                                            isEndOfRead = true;
                                                            intDataRows = i;

                                                        }

                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(strCommissionTotal))                                            
                                                strCommissionTotal = "0";
                                           
                                            if (string.IsNullOrWhiteSpace(strCommissionTotal))                                            
                                                strCommissionTotal = "0";                                           

                                            if (!string.IsNullOrEmpty(strCommissionTotal))
                                            {
                                                strCommissionTotal = strCommissionTotal.Replace("$", "");
                                                strCommissionTotal = strCommissionTotal.Replace("%", "");

                                                if (strCommissionTotal.Contains("("))
                                                {
                                                    strCommissionTotal = strCommissionTotal.Replace("(", "");
                                                    strCommissionTotal = strCommissionTotal.Replace(")", "");
                                                    strCommissionTotal = "-" + strCommissionTotal;
                                                }

                                            }
                                            decimal CommissionTotal = Convert.ToDecimal(strCommissionTotal);
                                        }
                                    }
                                    catch (Exception ex)
                                    { //if issue found then set false
                                        IsIssueFound = true;                                       
                                        ErrorMailContent("Rows location is: " + intCurruentRows.ToString(), "Column location is: " + (intCommissionTotalCol + 1).ToString(), "Commission total : " + strCommissionTotal, "Error while reading Commission total from files " + ex.Message.ToString());
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
        }

        //Create ErrorMailContent
        private void ErrorMailContent(string strRowLocotion, string strColLocotion, string strErrorValue, string strException)
        {
            //Log into text file All File 
            strAllLogError.Append(Environment.NewLine + "Error:");
            strAllLogError.Append(strRowLocotion);
            strAllLogError.Append(Environment.NewLine);
            strAllLogError.Append( strColLocotion);
            strAllLogError.Append( Environment.NewLine);
            strAllLogError.Append(strErrorValue);
            strAllLogError.Append(Environment.NewLine);
            strAllLogError.Append( strException);
            strAllLogError.Append( Environment.NewLine);

            if (intMessageCounter <= 20)
            {
                strMailLogError.Append("<tr><td colspan='2'>&nbsp;");
                strMailLogError.Append("</tr></td>");
                strMailLogError.Append("Error:" + Environment.NewLine);
                strMailLogError.Append("<tr><td colspan='2'>");
                strMailLogError.Append( strRowLocotion);
                strMailLogError.Append("</tr></td>");
                strMailLogError.Append( Environment.NewLine);
                strMailLogError.Append("<tr><td colspan='2'>");
                strMailLogError.Append(strColLocotion);
                strMailLogError.Append("</tr></td>");
                strMailLogError.Append(Environment.NewLine);
                strMailLogError.Append("<tr><td colspan='2'>");
                strMailLogError.Append(strErrorValue);
                strMailLogError.Append("</tr></td>");
                strMailLogError.Append( Environment.NewLine);
                strMailLogError.Append("<tr><td colspan='2'>");
                strMailLogError.Append(strException);
                strMailLogError.Append("</tr></td>");
                strMailLogError.Append( Environment.NewLine);
                
                intMessageCounter = intMessageCounter + 1;
            }
        }
        //Create HTML Body
        private string CreateHtmlBody(string strBody)
        {
            string strMailBody = string.Empty;
            try
            {
                strMailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                          "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>" +
                           "Please find the data error details in import tool." +"</td></tr>"
                           + "<tr><td colspan='2'>&nbsp" + "</td></tr>"                          
                           + "<tr><td colspan='2'>" + strBody + "</td></tr>"
                           + "<tr><td colspan='2'>&nbsp" + "</td></tr>"                        
                           + "<tr><td colspan='2'><bold>Please see the log file at server.</bold>" + "</td></tr>"
                           + "<tr><td colspan='2'><bold>Log file location at live server (166.66.132.155) is C:\\ErrorLog\\ServerImportLog_</bold>" + "</td></tr>"
                           + "</table>";

            }
            catch
            {
            }
            return strMailBody;
        }
        //Search and process payment data process
        private void SearchPaymentDataSetting(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, string strStartRowsData, string strStartColsData, string strEndDataRows, string strEndDataCols, string strEndDataIndicator, Guid licID, int intEndDataRows)
        {
            //Load all mask type
            LoadMaskType();

            //Create instance 
            List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();
            PayorTemplate objPayorTemplateCode = new PayorTemplate();

            //Load all payment data setting in template
            objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();
            //End location of spreadsheet
            //int intEndDataRows = EnddataRowsLocation(ObjSelectedImportToolPayorPhrase, dt, strStartRowsData, strEndDataIndicator);
            int dtRowsCount = 0;
            #region"Local variable"
            if (intEndRowIndicator > 0)
            {
                dtRowsCount = intEndRowIndicator;
            }
            else
            {
                if (!string.IsNullOrEmpty(strEndDataRows))
                {
                    try
                    {
                        dtRowsCount = Convert.ToInt32(strEndDataRows);
                    }
                    catch
                    {
                        dtRowsCount = dt.Rows.Count;
                    }
                }
                else
                {
                    dtRowsCount = dt.Rows.Count;
                }
            }
             
            bool isEndOfRead = false;
            string strInvoiceMonth = string.Empty;
            string strInvoiceYear = string.Empty;           
            int intCurruentRows = 0;
            decimal dbCheackAmount = 0;
            //To check the run deu post process,learned process,follwup process
            bool IsRunProcess = true;

            DEUFields deuFields = new DEUFields();
            DEU deudata = new DEU();
            List<DataEntryField> dueFields = new List<DataEntryField>();
            List<DataEntryField> due11 = new List<DataEntryField>();
            DataEntryField deuField = null;
            DataEntryField abc = null;
            Guid GuidPid = new Guid();
            List<UniqueIdenitfier> uniqueIdentifiers = new List<UniqueIdenitfier>();
            UniqueIdenitfier uniqueIdentifier = null;
            #endregion
            ActionLogger.Logger.WriteImportLog("***", true);
            try
            {
                for (int i = 0; i <= dtRowsCount; i++)
                {
                    uniqueIdentifiers.Clear();
                    //set current row
                    intCurruentRows = i + 1;

                    if (deuFields != null)
                    {
                        if (deuFields.DeuFieldDataCollection != null)
                        {
                            deuFields.DeuFieldDataCollection.Clear();
                            deuFields.DeuData = null;
                        }
                    }

                    if (i >= Convert.ToInt32(strStartRowsData)-1)
                    {
                        deudata = new DEU();
                        deuField = new DataEntryField();

                        strInvoiceMonth = string.Empty;
                        strInvoiceYear = string.Empty;
                        //Set Current row number                        
                        ActionLogger.Logger.WriteImportLog("Reading data from spread sheet. rows number is: " + intCurruentRows.ToString(), true);

                        //Set true
                        IsRunProcess = true;
                        //set falase untill first file has been completed
                        IsRunServiceAgain = false;

                        foreach (var item in objImportToolPaymentDataSettings)
                        {
                            abc = new DataEntryField();
                            uniqueIdentifier = new UniqueIdenitfier();

                                                    
                            switch (item.FieldsName)
                            {
                                case "PolicyNumber":                                     
                                    string strPolicyNumber = string.Empty;                                    
                                     try
                                     {
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
                                         //if issue found then set false
                                         IsRunProcess = false;
                                         ActionLogger.Logger.WriteImportLog("policy number: " + strPolicyNumber, true);
                                         ActionLogger.Logger.WriteImportLog("Error while reading policy number from files " + ex.Message.ToString(), true);
                                     }

                                    break;

                                case "ModelAvgPremium":

                                    string strModelAvgpremium = string.Empty;
                                    try
                                    {
                                       
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
                                                strModelAvgpremium = strModelAvgpremium.Replace("$", "");

                                                if (strModelAvgpremium.Contains("("))
                                                {
                                                    strModelAvgpremium = strModelAvgpremium.Replace("(", "");
                                                    strModelAvgpremium = strModelAvgpremium.Replace(")", "");
                                                    strModelAvgpremium = "-" + strModelAvgpremium;
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
                                    catch 
                                    { 
                                        //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }
                                    break;


                                case "Insured":
                                    string strInsured = string.Empty;
                                    try
                                    {
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
                                    catch 
                                    {
                                        //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }

                                    break;

                                case "OriginalEffectiveDate":

                                    string strOriginalEffectiveDate = string.Empty;
                                    try
                                    {
                                       
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
                                                    try
                                                    {
                                                        deudata.OriginalEffectiveDate = Convert.ToDateTime(strOriginalEffectiveDate);
                                                    }
                                                    catch
                                                    {
                                                        deudata.OriginalEffectiveDate = System.DateTime.Now;
                                                    }
                                                    
                                                }
                                                //set default vaue is null
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
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "InvoiceDate":
                                    string strInvoiceDate = string.Empty;
                                    try
                                    {
                                       
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
                                                strInvoiceDate = strInvoiceDate.Replace("-", "/");
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
                                                catch
                                                {
                                                    //AddToDataBase("Issue in invoice date");

                                                    try
                                                    {
                                                        dtInvoiceDateTime = Convert.ToDateTime(strInvoiceDate);
                                                        //ActionLogger.Logger.WriteImportLog("Mask type is: " + strMasktype + " And Date format is  " + strInvoiceDate + "Please save valid mask type", true);
                                                    }
                                                    catch 
                                                    {
                                                        dtInvoiceDateTime = System.DateTime.Now;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "InvoiceMonth":
                                    strInvoiceMonth = string.Empty;
                                    try
                                    {                                       
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

                                                    if (!string.IsNullOrEmpty(item.strDefaultText))
                                                    {
                                                        if (string.IsNullOrEmpty(strInvoiceMonth))
                                                        {
                                                            strInvoiceMonth = item.strDefaultText;
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

                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "InvoiceYear":
                                    strInvoiceYear = string.Empty;
                                    try
                                    {
                                       
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
                                                    if (!string.IsNullOrEmpty(item.strDefaultText))
                                                    {
                                                        if (string.IsNullOrEmpty(strInvoiceYear))
                                                        {
                                                            strInvoiceYear = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }
                                    break;

                                case "EffectiveDate":

                                    string strEffectiveDate = string.Empty;
                                    try
                                    {

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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strEffectiveDate))
                                                    {
                                                        strEffectiveDate = item.strDefaultText;
                                                    }
                                                }
                                                //deuFields.DeuData.OriginalEffectiveDate = Convert.ToDateTime(strEffectiveDate);
                                                if (!string.IsNullOrEmpty(strEffectiveDate))
                                                {
                                                    strEffectiveDate = strEffectiveDate.Replace("-", "/");
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

                                                    try
                                                    {
                                                        deudata.OriginalEffectiveDate = Convert.ToDateTime(strEffectiveDate);
                                                    }
                                                    catch
                                                    {
                                                        deudata.OriginalEffectiveDate = System.DateTime.Now;
                                                        try
                                                        {
                                                            strEffectiveDate = Convert.ToString(deudata.OriginalEffectiveDate);
                                                        }
                                                        catch
                                                        {
                                                        }
                                                    }
                                                }
                                               //If no effetive date then null
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
                                        else
                                        {
                                            isEndOfRead = true;
                                        }
                                    }

                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                    }
                                    break;

                                case "PaymentReceived":

                                    string strPaymentReceived = string.Empty;
                                    try
                                    {
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strPaymentReceived))
                                                    {
                                                        strPaymentReceived = item.strDefaultText;
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
                                                    strPaymentReceived = strPaymentReceived.Replace("$", "");
                                                    if (strPaymentReceived.Contains("("))
                                                    {
                                                        strPaymentReceived = strPaymentReceived.Replace("(", "");
                                                        strPaymentReceived = strPaymentReceived.Replace(")", "");
                                                        strPaymentReceived = "-" + strPaymentReceived;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "CommissionPercentage":
                                    
                                    string strCommissionPercentage = string.Empty;
                                    try
                                    {
                                       
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionPercentage))
                                                    {
                                                        strCommissionPercentage = item.strDefaultText;
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
                                                    strCommissionPercentage = strCommissionPercentage.Replace("$", "");
                                                    if (strCommissionPercentage.Contains("("))
                                                    {
                                                        strCommissionPercentage = strCommissionPercentage.Replace("(", "");
                                                        strCommissionPercentage = strCommissionPercentage.Replace(")", "");
                                                        strCommissionPercentage = "-" + strCommissionPercentage;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                       
                                    }
                                    break;

                                case "Renewal":
                                    string strRenewal = string.Empty;
                                    try
                                    {
                                        
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strRenewal))
                                                    {
                                                        strRenewal = item.strDefaultText;
                                                    }
                                                }
                                              
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                        
                                    }
                                    break;

                                case "Enrolled":
                                    string strEnrolled = string.Empty;
                                    try
                                    {
                                       
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strEnrolled))
                                                    {
                                                        strEnrolled = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "Eligible":

                                    string strEligible = string.Empty;
                                    try
                                    {

                                       
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strEligible))
                                                    {
                                                        strEligible = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }
                                    break;

                                case "Link1":

                                    string strLink1 = string.Empty;
                                    try
                                    {
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strLink1))
                                                    {
                                                        strLink1 = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "SplitPercentage":

                                    string strSplitPercentage = string.Empty;
                                    try
                                    {
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strSplitPercentage))
                                                    {
                                                        strSplitPercentage = item.strDefaultText;
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
                                                    strSplitPercentage = strSplitPercentage.Replace("$", "");

                                                    if (strSplitPercentage.Contains("("))
                                                    {
                                                        strSplitPercentage = strSplitPercentage.Replace("(", "");
                                                        strSplitPercentage = strSplitPercentage.Replace(")", "");
                                                        strSplitPercentage = "-" + strSplitPercentage;
                                                    }

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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "PolicyMode":
                                    string strPolicyMode = string.Empty;
                                    try
                                    {
                                        
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strPolicyMode))
                                                    {
                                                        strPolicyMode = item.strDefaultText;
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
                                                    strPolicyMode = strPolicyMode.Replace("$", "");
                                                    strPolicyMode = strPolicyMode.Replace("-", "");
                                                    int PolicyMode = 0;
                                                    try
                                                    {
                                                        PolicyMode = Convert.ToInt32(strPolicyMode);
                                                    }
                                                    catch
                                                    {
                                                        PolicyMode = 0;
                                                    }
                                                    deudata.PolicyMode = PolicyMode;

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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "Carrier":
                                    string strCarrier = string.Empty;
                                    try
                                    {
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strCarrier))
                                                    {
                                                        strCarrier = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "Product":
                                    string strProduct = string.Empty;
                                    try
                                    {                                        
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strProduct))
                                                    {
                                                        strProduct = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                       
                                    }
                                    break;

                                case "PayorSysId":
                                    string strPayorSysId = string.Empty;
                                    try
                                    {                                        
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strPayorSysId))
                                                    {
                                                        strPayorSysId = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                       
                                    }
                                    break;

                                case "CompScheduleType":
                                    string strCompScheduleType = string.Empty;
                                    try
                                    {
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strCompScheduleType))
                                                    {
                                                        strCompScheduleType = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }
                                    break;

                                case "CompType":
                                    string strCompType = string.Empty;
                                    try
                                    {
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
                                                //deuFields.DeuData.CompTypeID = Convert.ToInt32(strCompType);  
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strCompType))
                                                    {
                                                        strCompType = item.strDefaultText;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strCompType))
                                                {
                                                    strCompType = strCompType.Replace("$", "");
                                                    //Get Comp type
                                                    //int intCompType = BLHelper.getCompTypeId(strCompType);
                                                    int intCompType = BLHelper.getCompTypeIdByName(strCompType);

                                                    deudata.CompTypeID = intCompType;

                                                    deuField.DeuFieldName = item.FieldsName;
                                                    deuField.DeuFieldValue = Convert.ToString(intCompType);

                                                    abc.DeuFieldName = item.FieldsName;
                                                    abc.DeuFieldValue = strCompType;
                                                    dueFields.Add(abc);

                                                    if (item.PartOfPrimaryKey)
                                                    {
                                                        uniqueIdentifier.ColumnName = item.FieldsName;
                                                        uniqueIdentifier.Text = Convert.ToString(intCompType);
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "Client":
                                    string strClient = string.Empty;
                                    try
                                    {
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strClient))
                                                    {
                                                        strClient = item.strDefaultText;
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                       
                                    }
                                    break;

                                case "NumberOfUnits":
                                    string strNumberOfUnits = string.Empty;
                                    try
                                    {
                                        
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strNumberOfUnits))
                                                    {
                                                        strNumberOfUnits = item.strDefaultText;
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
                                                    strNumberOfUnits = strNumberOfUnits.Replace("$", "");
                                                    if (strNumberOfUnits.Contains("("))
                                                    {
                                                        strNumberOfUnits = strNumberOfUnits.Replace("(", "");
                                                        strNumberOfUnits = strNumberOfUnits.Replace(")", "");
                                                        strNumberOfUnits = "-" + strNumberOfUnits;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strDollerPerUnit))
                                                    {
                                                        strDollerPerUnit = item.strDefaultText;
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strDollerPerUnit)) || (string.IsNullOrWhiteSpace(strDollerPerUnit)))
                                                {
                                                    strDollerPerUnit = "0";
                                                }

                                                if (!string.IsNullOrEmpty(strDollerPerUnit))
                                                {
                                                    strDollerPerUnit = strDollerPerUnit.Replace("$", "");
                                                    if (strDollerPerUnit.Contains("("))
                                                    {
                                                        strDollerPerUnit = strDollerPerUnit.Replace("(", "");
                                                        strDollerPerUnit = strDollerPerUnit.Replace(")", "");
                                                        strDollerPerUnit = "-" + strDollerPerUnit;
                                                    }
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
                                    catch
                                    { //if issue found then set false
                                        IsRunProcess = false;
                                       
                                    }
                                    break;

                                case "Fee":
                                    string strFee = string.Empty;
                                    try
                                    {                                        
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
                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strFee))
                                                    {
                                                        strFee = item.strDefaultText;
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strFee)) || (string.IsNullOrWhiteSpace(strFee)))
                                                {
                                                    strFee = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strFee))
                                                {
                                                    strFee = strFee.Replace("$", "");
                                                    if (strFee.Contains("("))
                                                    {
                                                        strFee = strFee.Replace("(", "");
                                                        strFee = strFee.Replace(")", "");
                                                        strFee = "-" + strFee;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                        
                                    }
                                    break;

                                case "Bonus":
                                    string strBonus = string.Empty;
                                    try
                                    {
                                        
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strBonus))
                                                    {
                                                        strBonus = item.strDefaultText;
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strBonus)) || (string.IsNullOrWhiteSpace(strBonus)))
                                                {
                                                    strBonus = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strBonus))
                                                {
                                                    strBonus = strBonus.Replace("$", "");
                                                    if (strBonus.Contains("("))
                                                    {
                                                        strBonus = strBonus.Replace("(", "");
                                                        strBonus = strBonus.Replace(")", "");
                                                        strBonus = "-" + strBonus;
                                                    }
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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                       
                                    }
                                    break;

                                case "CommissionTotal":
                                    string strCommissionTotal = string.Empty;
                                    try
                                    {
                                        
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

                                                if (!string.IsNullOrEmpty(item.strDefaultText))
                                                {
                                                    if (string.IsNullOrEmpty(strCommissionTotal))
                                                    {
                                                        strCommissionTotal = item.strDefaultText;
                                                    }
                                                }
                                                if ((string.IsNullOrEmpty(strCommissionTotal)) || (string.IsNullOrWhiteSpace(strCommissionTotal)))
                                                {
                                                    strCommissionTotal = "0";
                                                }
                                                if (!string.IsNullOrEmpty(strCommissionTotal))
                                                {
                                                    strCommissionTotal = strCommissionTotal.Replace("$", "");
                                                    strCommissionTotal = strCommissionTotal.Replace("%", "");

                                                    if (strCommissionTotal.Contains("("))
                                                    {
                                                        strCommissionTotal = strCommissionTotal.Replace("(", "");
                                                        strCommissionTotal = strCommissionTotal.Replace(")", "");
                                                        strCommissionTotal = "-" + strCommissionTotal;
                                                    }                                                  

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
                                    catch 
                                    { //if issue found then set false
                                        IsRunProcess = false;                                      
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

                            //if issue found then not reqired to run process
                            if (IsRunProcess)
                            {
                                ActionLogger.Logger.WriteImportLog("Start Process time: " + System.DateTime.Now, true);
                                DeuPostProcessWrapper.DeuPostStartWrapper(PostEntryProcess.FirstPost, deuFields, Guid.Empty, guidSuperUser, UserRole.SuperAdmin);
                                ActionLogger.Logger.WriteImportLog("End Process time:  " + System.DateTime.Now, true);
                               // ActionLogger.Logger.WriteImportLog("***", true);                               
                            }
                            else
                            {
                                ActionLogger.Logger.WriteImportLog("Current Data is not being imported ,because there are issue found.Please verify the data into spread sheet.  " + System.DateTime.Now, true);
                                ActionLogger.Logger.WriteImportLog("***", true);
                            }
                        }

                        if (isEndOfRead)
                        {
                            //Update batch to show in DEU and comp manager
                            //Copy folder path to successful folder.
                            //AddToDataBase("success");
                            try
                            {
                                UpdateBatch(generatedBatchID);
                                //If Check amount address is not found
                                if (isCheckAmountAvailable == false)
                                {
                                    isCheckAmountAvailable = true;
                                    Statement objStatement = new Statement();
                                    if (intStatementnumber > 0)
                                    {
                                        objStatement.UpdateCheckAmount(intStatementnumber, dbCheackAmount, dbCheackAmount);
                                        ActionLogger.Logger.WriteImportLog("Update check amount", true);
                                    }
                                }

                                MoveToSuccesfullfolder();

                                ActionLogger.Logger.WriteImportLog("***", true);

                            }
                            catch (Exception ex)
                            {
                                //Force to move folder to successful 
                                MoveToSuccesfullfolder();
                                ActionLogger.Logger.WriteImportLog("Error found: " + ex.ToString(), true);
                                //ActionLogger.Logger.WriteImportLog("Move to Succesfull folder :  " + System.DateTime.Now, true);
                                ActionLogger.Logger.WriteImportLog("***", true);
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
                MoveToTempfolder();
                MoveToUnSuccesfullfolder();
                ActionLogger.Logger.WriteImportLog("Error :  " + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteImportLog("***", true);
            }

        }
        //Update statement data
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
                    try
                    {
                        dtStatementDate = Convert.ToDateTime(strStatementDate);
                    }
                    catch
                    {
                        dtStatementDate = System.DateTime.Now;
                        ActionLogger.Logger.WriteImportLog("Please enter correct date format.", true);
                    }
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
        //Get Expression
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
        //Calculatr tranlator
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
        //Validate date value
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
        //Update batch status completed after successful inported files
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
        //Move file to Temp folder
        private void MoveToTempfolder()
        {
            
            try
            {
                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Processing\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;
                //Cheack file exist at source location
                if (File.Exists(strFilePath))
                {
                    //Check if file already exist at target location 
                    //if exists then delete then move to new location
                    if (File.Exists(strNewLocation))
                    {
                        File.Delete(strNewLocation);
                    }
                    File.Move(strFilePath, strNewLocation);
                }

                //ActionLogger.Logger.WriteImportLog("File moved to temp folder", true);
            }
            catch (Exception ex)
            {
                AddToDataBase("Error while moving file to temp folder" + ex.Message.ToString());
            }
        }
        //Move file to successful folder
        private void MoveToSuccesfullfolder()
        {
            IsRunServiceAgain = true;
            try
            {

                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Success\" + strFileFullName;

                //Cheack file exist at source location
                if (File.Exists(strFilePath))
                {
                    //Check if file already exist at target location 
                    //if exists then delete then move to new location
                    if (File.Exists(strNewLocation))
                    {
                        File.Delete(strNewLocation);
                    }
                    File.Move(strFilePath, strNewLocation);
                }

                //ActionLogger.Logger.WriteImportLog("Move to succesfull folder", true);
            }
            catch (Exception ex)
            {
                AddToDataBase("Error while moving file to succesfull folder" + ex.Message.ToString());
            }
        }
        //Move file to unsuccessful folder
        private void MoveToUnSuccesfullfolder()
        {
            IsRunServiceAgain = true;
            try
            {
                string strNewLocation = string.Empty;
                string strFilePath = @"D:\Filemanager\Uploadbatch\Import\Temp\" + strFileFullName;
                strNewLocation = @"D:\Filemanager\Uploadbatch\Import\Unsuccess\" + strFileFullName;
                //Check file exist at source location
                if (File.Exists(strFilePath))
                {
                    //Check if file already exist at target location 
                    //if exists then delete then move to new location
                    if (File.Exists(strNewLocation))
                    {
                        File.Delete(strNewLocation);
                    }
                    File.Move(strFilePath, strNewLocation);
                }
                //ActionLogger.Logger.WriteImportLog("Move to Unsuccesfull folder ", true);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("Error while moving to Unsuccesfull folder  " + ex.ToString(), true);
            }
        }
        //Create DEU fieds 
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

        //Create DEU fieds values 
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
        //Load all mask type
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

        private bool IsAllPaymentDataFieldsIsAvailableIntoFile(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt)
        {
            bool bValue = false;
            //Get selected fields at payor and template
            //Match to whole data table columns
            //If Match then return true else return false           
            return bValue;
        }

        //Get Statement value like ckeck amount ,bal/adj,end data indicatot
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

        //Search Broker code
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

        //GEt lincessi di 
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
                ActionLogger.Logger.WriteImportLog("Error in GetLincessID  :" + ex.Message.ToString(), true);

            }
            return linceeID;
        }

        //Find its relation location like relative rows and relative column
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
            string strConn = string.Empty;
            DataTable dt = null;

            if (FilePath.Trim().EndsWith(".xlsx"))
            {
                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1\";", FilePath);
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter da = null;
                string pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                string fileName = System.IO.Path.GetFileName(FilePath);
                dt = new DataTable("Temp");
                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();                   
                    //cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", conn);
                    string strSheetName = getSheetName(conn, fileName);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                    cmd = new OleDbCommand(strQuery, conn);

                    cmd.CommandType = CommandType.Text;
                    da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                   // ActionLogger.Logger.WriteImportLog("Data set created", true);

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
                string pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                string fileName = System.IO.Path.GetFileName(FilePath);

                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=" + header + ";IMEX=1\";", FilePath);
                //strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";Extended Properties=\"Excel 8.0;HDR=" + header + "\"";
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter adapter = null;
                dt = new DataTable(fileName);                

                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    string strSheetName = getSheetName(conn, fileName);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";                   
                    cmd = new OleDbCommand(strQuery, conn);
                    cmd.CommandType = CommandType.Text;
                    adapter = new OleDbDataAdapter(cmd);
                    adapter.Fill(dt);

                    //ActionLogger.Logger.WriteImportLog("Data set created", true);
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
                                ActionLogger.Logger.WriteImportLog("Data set created", true);
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

        //Function to get sheet name from excel files 
        private string getSheetName(OleDbConnection ObjConn,string fileName)
        {           
            string strSheetName = String.Empty;
            try
            {
                System.Data.DataTable dtSheetNames = ObjConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);             

                if (dtSheetNames == null || dtSheetNames.Rows.Count < 1)
                {
                    ActionLogger.Logger.WriteImportLog("Could not determine the name of the first worksheet.", true);
                    MoveToTempfolder();
                    MoveToUnSuccesfullfolder();
                }
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
            ActionLogger.Logger.WriteImportLog("Spread sheet name is : " + strSheetName, true);
            return strSheetName;
        }

        //This function is used to save log file into databae
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

        //This function used to get all templateId phrase 
        private DataSet GetListOftemplatePhrase()
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlConnection sqlCon = new SqlConnection();
            DataSet ds = new DataSet();
            try
            {
                using (sqlCon = new SqlConnection(@"Data Source=192.168.1.6\SQLSERVER2008R2;User Id=jyotisna;Password=acmeminds;Initial Catalog=CommisionDepartmentEricDB;MultipleActiveResultSets=True"))
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

        //Used to get all phrase on temmlate
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

        //Used to get all phrase on temmlate
        private string GetPhrasesIntoPayor(Guid PayorID, Guid TemplateID)
        {
            SqlCommand sqlCmd = new SqlCommand();
            SqlConnection sqlCon = new SqlConnection();
            DataSet dsPhrase = new DataSet();
            string strPhrase = string.Empty;
            try
            {
                using (sqlCon = new SqlConnection(strConn))
                {
                    string strCommand = "SELECT PayorPhrases FROM ImportToolPayorPhrase WHERE TemplateID=" + "'" + TemplateID + "' AND PayorID=" + "'" + PayorID + "' ";

                    SqlDataAdapter sda = new SqlDataAdapter(strCommand, sqlCon);
                    sda.Fill(dsPhrase, "PayorPhrase");

                    if (dsPhrase != null)
                    {
                        if (dsPhrase.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsPhrase.Tables[0].Rows.Count; i++)
                            {
                                strPhrase += Convert.ToString(dsPhrase.Tables[0].Rows[i][0]) + ";";
                            }
                        }
                    }
                }

                strPhrase = strPhrase.Remove(strPhrase.Length - 1, 1);
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
            return strPhrase;
        }

        //Send notification mail 
        private void SendNotificationEmail(string strLicenseName, string strBody)
        {
            MailData _MailData = new MailData();
            try
            {
                //Declare mail property
                FollowupIssue objFollowupIssue = new FollowupIssue();
                //From mail addresss
                _MailData.FromMail = ConfigurationSettings.AppSettings["FromMail"].ToString();                
                //Send To mail address
                _MailData.ToMail = ConfigurationSettings.AppSettings["ToMail"].ToString();
                //_MailData.ToMail = "vinod.yadav@hanusoftware.com";
                //Call function to send mail
                string strSubject = " Import alert in " + strLicenseName + " Agency and " + "Batch No " + strBatch;

                objFollowupIssue.SendNotificationMail(_MailData, strSubject, CreateHtmlBody(strBody));
               
            }
            catch
            {
            }
        }

        //Calculate end data row location on the basis of 
        private int EnddataRowsLocation(ImportToolPayorPhrase ObjSelectedImportToolPayorPhrase, DataTable dt, string strStartRowsData, string strEndDataIndicator)
        {
            int dtRowsCount = dt.Rows.Count;
            bool isEndOfRead = false;
            int intDataRows = 0;

            try
            {
                List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();

                PayorTemplate objPayorTemplateCode = new PayorTemplate();
                objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting(ObjSelectedImportToolPayorPhrase.PayorID, ObjSelectedImportToolPayorPhrase.TemplateID).ToList();

                for (int i = 0; i < dtRowsCount; i++)
                {
                    if (i >= Convert.ToInt32(strStartRowsData) - 1)
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
            catch 
            {
               // ActionLogger.Logger.WriteImportLog("Error while finding end data rows :" + ex.Message.ToString(), true);
            }

            if (intDataRows == 0)
            {
                intDataRows = dtRowsCount;
            }
            return intDataRows;

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

