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
//using ActionLogger;

namespace ImportToolServiceApplication
{
    class Program
    {
        private ObservableCollection<ImportToolBrokerSetting> _AllImportToolBrokerSetting;
        public ObservableCollection<ImportToolBrokerSetting> AllImportToolBroker
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

        private ImportToolBrokerSetting _selectedImportToolSetting;
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

        public void StartFolderWatcher()
        {
            try
            {
                FileSystemWatcher objFilesystemWatcher = new FileSystemWatcher();
                objFilesystemWatcher.Path = @"D:\ImportPayorTool";
                //objFilesystemWatcher.Path = @"D:\Filemanager\Uploadbatch\Import\Processing";
                objFilesystemWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
                objFilesystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
                objFilesystemWatcher.Created += new FileSystemEventHandler(eventRaised);
                objFilesystemWatcher.EnableRaisingEvents = true;

                Console.WriteLine("folder watcher start");

                //ActionLogger.Logger.WriteLog("folder watcher start ", true);
            }

            catch (Exception ex)
            {
                //if (!EventLog.SourceExists("ImportNotification", "."))
                //{
                //    EventLog.CreateEventSource("ImportNotification", "Application");
                //    EventLog lg = new EventLog("Application", ".", "ImportLog");
                //    lg.WriteEntry(ex.ToString(), EventLogEntryType.Information);
                //}

                //ActionLogger.Logger.WriteLog("vinod Inport tool " + ex, true);
            }
        }

        private void SerchBrkerCode(DataTable dt)
        {
            //Get All Broker code
            Brokercode objbBrokerCode = new Brokercode();
            AllImportToolBroker = new ObservableCollection<ImportToolBrokerSetting>(objbBrokerCode.LoadImportToolBrokerSetting().ToList());

            int row = -1;
            int col = -1;
            string strRelativeSerch = string.Empty;

            foreach (var item in AllImportToolBroker)
            {
                if (!string.IsNullOrEmpty(item.FixedRows) && !string.IsNullOrEmpty(item.FixedRows))
                {
                    row = Convert.ToInt32(item.FixedRows);
                    col = Convert.ToInt32(item.FixedColumns);
                }
                else if (!string.IsNullOrEmpty(item.RelativeRows) && !string.IsNullOrEmpty(item.RelativeColumns))
                {
                    row = Convert.ToInt32(item.RelativeRows);
                    col = Convert.ToInt32(item.RelativeColumns);
                    strRelativeSerch = item.RelativeSearchtext;
                }

                if (row > -1 && col > -1)
                {
                    string strBrokerCode = dt.Rows[row][col].ToString();

                    Brokercode objbrokerCode = new Brokercode();
                    bool bvalue = objbrokerCode.ValidateBrokerCode(strBrokerCode);
                    if (bvalue == false)//means broker code found
                    {
                        List<DisplayBrokerCode> lstBrokerCode = objbrokerCode.GetBrokerCodeByBrokerName(strBrokerCode);
                        selectedDisplayBrokerCode = lstBrokerCode.FirstOrDefault();
                        Guid? guid = selectedDisplayBrokerCode.licenseeID;
                    }
                }
            }
        }

        private void eventRaised(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    //Get All Broker code

                    //ActionLogger.Logger.WriteLog("Event  raised start ", true);

                    Console.WriteLine("Event watcher start");

                    Brokercode objbBrokerCode = new Brokercode();
                    AllImportToolBroker = new ObservableCollection<ImportToolBrokerSetting>(objbBrokerCode.LoadImportToolBrokerSetting().ToList());

                    string supportedExtensions = "*.xls,*.xlsx,*.txt,*.csv";

                    foreach (string FilePath in Directory.GetFiles(@"D:\ImportPayorTool", "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {
                        string strFileFullName = System.IO.Path.GetFileName(FilePath);

                        if (strFileFullName.Contains("_"))
                        {
                            string[] arrValue = strFileFullName.Split('_');
                            string strCompnayName = arrValue[0];
                            string strBatch = arrValue[1];
                            string strFileName = arrValue[2];

                            List<LicenseeDisplayData> objLiceencessDetails = new List<LicenseeDisplayData>(LicenseeDisplayData.GetLicenseeName(strCompnayName));

                            selectedLicenseeDisplayData = objLiceencessDetails.FirstOrDefault();

                            ////Get lincessee broker code
                            //Brokercode objBrokerCode = new Brokercode();
                            //List<DisplayBrokerCode> ObjSelectedBrokerCode = new List<DisplayBrokerCode>(objBrokerCode.LoadBrokerCode(LicId)).ToList();

                            //string strBrokerCode = string.Empty;

                            //if (ObjSelectedBrokerCode.Count > 0)
                            //{
                            //    foreach (var varValue in ObjSelectedBrokerCode)
                            //    {
                            //        strBrokerCode = varValue.Code;
                            //        break;
                            //    }
                            //}

                            //selectedImportToolSetting = new ObservableCollection<ImportToolBrokerSetting>(AllImportToolBroker.Where(s=>s.);


                        }
                        else if (strFileFullName.Contains("_"))
                        {
                        }
                        else
                        {
                            DataTable dt = ConvretExcelToDataTable(FilePath);
                            SerchBrkerCode(dt);
                        }
                        //DataTable dt = ConvretExcelToDataTable(Convert.ToString(e.FullPath));
                        //if (dt != null)
                        //{
                        //    string s = "CompType";

                        //    for (int i = 0; i < dt.Rows.Count; i++)
                        //    {
                        //        for (int j = 0; j < dt.Columns.Count; j++)
                        //        {
                        //            if (Convert.ToString(dt.Rows[i][j]) != null)
                        //            {
                        //                if (dt.Rows[i][j].ToString().ToLower() == s.ToLower())
                        //                {

                        //                    break;
                        //                }
                        //            }
                        //        }
                        //    }

                        //}
                        //else
                        //{

                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                //ActionLogger.Logger.WriteLog("vinod Inport tool " + ex, true);
                Console.WriteLine(ex.ToString());
            }
        }

        public DataTable ConvretExcelToDataTable(string FilePath)
        {
            string strConn = string.Empty;
            DataTable dt = null;

            if (FilePath.Trim().EndsWith(".xlsx"))
            {
                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", FilePath);
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter da = null;
                dt = new DataTable();
                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", conn);
                    cmd.CommandType = CommandType.Text;
                    da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    //if (!EventLog.SourceExists("ImportNotification", "."))
                    //{
                    //    EventLog.CreateEventSource("ImportNotification", "Application");
                    //    EventLog lg = new EventLog("Application", ".", "ImportLog");
                    //    lg.WriteEntry(ex.ToString(), EventLogEntryType.Information);
                    //}

                   // ActionLogger.Logger.WriteLog("vinod(.xslx) Inport tool " + ex, true);
                    Console.WriteLine(ex.ToString());
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
                    cmd = new OleDbCommand("SELECT * FROM [Sheet1$] ", conn);
                    cmd.CommandType = CommandType.Text;
                    adapter = new OleDbDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                   // ActionLogger.Logger.WriteLog("vinod(.xls) Inport tool " + ex, true);
                    Console.WriteLine(ex.ToString());
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
                    //if (!EventLog.SourceExists("ImportNotification", "."))
                    //{
                    //    EventLog.CreateEventSource("ImportNotification", "Application");
                    //    EventLog lg = new EventLog("Application", ".", "ImportLog");
                    //    lg.WriteEntry(ex.ToString(), EventLogEntryType.Information);
                    //}
                    //ActionLogger.Logger.WriteLog("vinod(.csv) Inport tool " + ex, true);
                    Console.WriteLine(ex.ToString());
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

        static void Main(string[] args)
        {
            Program pg = new Program();
            pg.StartFolderWatcher();
        }
    }

}
