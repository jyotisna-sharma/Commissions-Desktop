using System;
using System.Collections.Generic;
using System.Linq;
using MyAgencyVault.BusinessLibrary;
using MyAgencyVault.EmailFax;
using System.IO;
//using MyAgencyVault.ViewModel.CommonItems;
//using MyAgencyVault.VM.CommonItems;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Collections;
using System.Timers;
using iTextSharp.text.pdf;
using System.Text;


namespace AutoBatchGenrate
{
    public partial class ThisAddIn
    {
        public string MailScanFaxEmail { get; set; }
        public string MailScanErrorEmail { get; set; }
        public string MailScanErrorEmailPassword { get; set; }
        public string Path { get; set; }
        private Outlook.Application OutLookApp;
        Outlook.MAPIFolder moveSucessMail = null;
        Outlook.MAPIFolder moveUnSucessMail = null;
        public string LogFilePath = "";
        Timer _timer;
        
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("hi start");

            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DirectoryInfo Df = new DirectoryInfo(Path);
            List<DirectoryInfo> Dlst = Df.GetDirectories().Where(p => p.Name == "BatchFileSave").ToList(); ;
            List<DirectoryInfo> Dlst1 = Df.GetDirectories().Where(p => p.Name == "MailScanLogFile").ToList(); ;
            if (Dlst == null || Dlst.Count == 0)
            {
                Directory.CreateDirectory(Path + "\\BatchFileSave");
            }
            if (Dlst == null || Dlst.Count == 0)
            {
                Directory.CreateDirectory(Path + "\\MailScanLogFile");
            }
            _timer.Start();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            _timer.Stop();

        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);

            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
            OutLookApp = this.Application;

            // OutLookApp.NewMail += new Outlook.ApplicationEvents_11_NewMailEventHandler(OutLookApp_NewMail);
            _timer = new Timer(60000);
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;

            //  OutLookApp.NewMailEx += new Outlook.ApplicationEvents_11_NewMailExEventHandler(OutLookApp_NewMailEx);
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                OutLookApp_NewMail();
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message + "\n\n " + ex.StackTrace+"\n\n "+ex.InnerException);
                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    if (LogFilePath != "")
                        sw.WriteLine(DateTime.Now.ToLongTimeString() + ex.Message + "\n\n " + ex.StackTrace + "\n\n " + ex.InnerException);

                }
            }
        }

        void OutLookApp_NewMail()
        {
            MailScanFaxEmail = MyAgencyVault.BusinessLibrary.Masters.SystemConstant.GetKeyValue("MailScanFaxEmail");
            MailScanErrorEmail = MyAgencyVault.BusinessLibrary.Masters.SystemConstant.GetKeyValue("MailScanErrorEmail");
            MailScanErrorEmailPassword = MyAgencyVault.BusinessLibrary.Masters.SystemConstant.GetKeyValue("MailScanErrorEmailPassword");
            LogFilePath = Path + "\\MailScanLogFile" + "\\" + DateTime.Today.ToString("MMM d, YYYY") + " MailEventLog.txt";

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");

            // Get the Calendar folder.
            Outlook.MAPIFolder oInbox = oNS.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.Items oItems = (Outlook.Items)oInbox.Items;

            long _faxnumber = 0;
            moveSucessMail = oInbox.Folders["Success"];
            moveUnSucessMail = oInbox.Folders["UnSuccess"];

            //  System.Windows.Forms.MessageBox.Show(string.Format("Unread items in Inbox = {0}", oItems.Count.ToString()));
            Microsoft.Office.Interop.Outlook.MailItem oMsg = default(Microsoft.Office.Interop.Outlook.MailItem);

            oItems = oItems.Restrict("[Unread] = true");

            IEnumerator eni = oItems.GetEnumerator();
            eni.Reset();

            int cnt = oItems.Count;

            while (eni.MoveNext())
            {

                oMsg = eni.Current as Outlook.MailItem;

                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Msg get From " + oMsg.SenderEmailAddress);
                    //; System.Windows.Forms.MessageBox.Show(oMsg.SenderEmailAddress);
                }

                // if (oMsg.SenderEmailAddress == @"admin@fax2me.com")

                if (oMsg.SenderEmailAddress == MailScanFaxEmail)
                {
                    try
                    {
                        string faxbody = oMsg.Body;

                        using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - mail body " + faxbody);
                        }

                        if (faxbody.Contains("Callers"))
                        {
                            int startFaxCaller = faxbody.IndexOf("Callers");

                            using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                            {
                                sw.WriteLine(DateTime.Now.ToLongTimeString() + " - start index " + startFaxCaller);
                            }
                            if (startFaxCaller < 0)
                            {
                                return;
                            }

                            faxbody = faxbody.Substring(startFaxCaller, 26);
                            _faxnumber = FaxNumber(faxbody);
                        }
                        else
                        {
                            oMsg.UnRead = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Error " + ex.ToString());
                        }
                    }
                }

                oMsg.UnRead = false;
                if (oMsg != null)
                {
                    #region "Delete previous batch file"
                    DirectoryInfo di = new DirectoryInfo(Path + @"\BatchFileSave\");
                    FileInfo[] fios = di.GetFiles();
                    foreach (FileInfo fff in fios)
                    {
                        fff.Delete();
                    }
                    #endregion

                    #region "Check Attachement ,if not found then send the mail"

                    if (oMsg.Attachments.Count > 0)
                    {
                        for (int i = 1; i <= oMsg.Attachments.Count; i++)
                        {
                            oMsg.Attachments[i].SaveAsFile(Path + @"\BatchFileSave\" + oMsg.Attachments[i].FileName);
                        }
                    }
                    else
                    {
                        oMsg.Move(moveUnSucessMail);

                        Outlook.Application oApp = new Outlook.Application();

                        // Get the NameSpace and Logon information.
                        Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");

                        // Log on by using a dialog box to choose the profile.
                        oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

                        Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                        try
                        {
                            oApp.Inspectors.Add(oMailItem.GetInspector);
                        }
                        catch
                        {

                        }
                        oMailItem.Subject = "Upload Unsuccessful-No attachment - Please read below";
                        string signature = oMailItem.HTMLBody;
                        oMailItem.HTMLBody = "</br>You have attempted to send an email to upload@commissionsdept.com with no attachment. <br/> " +
                                      "Please resend your commission statements in PDF format Or Excel file or Text file. .   If you need any further assistance,</br>" +
                                      "please contact us.  We appreciate your business.  </br></br>";
                        oMailItem.HTMLBody += signature;
                        oMailItem.To = oMsg.SenderEmailAddress;

                        oMailItem.Send();
                        oNS1.Logoff();
                        oNS1 = null;
                        oApp = null;
                        continue;
                    }
                    #endregion

                    #region "Get Files Check pdf files and excel file"

                    fios = di.GetFiles();
                    List<FileInfo> PdfFiles = fios.Where(p => p.Extension.ToLower() == ".pdf").ToList();
                    List<FileInfo> ExcelAndtxtFile = fios.Where(p => (p.Extension.ToLower() == ".xls") || (p.Extension.ToLower() == ".xlsx") || (p.Extension.ToLower() == ".csv") || (p.Extension.ToLower() == ".txt")).ToList();

                    #endregion

                    #region "Check the PDf file and excel file count and write into log file"
                    using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                    {
                        if (PdfFiles.Count > 0)
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Pdf File Count " + PdfFiles.Count.ToString());
                        }
                        else
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Excel or test File Count " + ExcelAndtxtFile.Count.ToString());
                        }

                    }
                    #endregion

                    #region "Check the PDf file if PDF file not found then send mail no attachement found"

                    if ((PdfFiles == null || PdfFiles.Count == 0) && (ExcelAndtxtFile == null || ExcelAndtxtFile.Count == 0))
                    {
                        using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - No files (pdf,xls,xlsx,txt found ");

                        }

                        oMsg.Move(moveUnSucessMail);
                        //Comprae with mail sender number
                        if (oMsg.SenderEmailAddress != MailScanFaxEmail)
                        {
                            Outlook.Application oApp = new Outlook.Application();
                            // Get the NameSpace and Logon information.
                            Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");
                            // Log on by using a dialog box to choose the profile.
                            oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);
                            Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                            try
                            {
                                oApp.Inspectors.Add(oMailItem.GetInspector);

                            }
                            catch
                            {

                            }
                            oMailItem.Subject = "Upload Unsuccessful – No attachment - Please read below";
                            string signature = oMailItem.HTMLBody;

                            oMailItem.HTMLBody = "You have attempted to send an email to upload@commissionsdept.com with no attachment.</br>" +
                                            "Please resend your commission statements in PDF format Or Excel file or Text file. If you need any further assistance, please contact us.</br>" +
                                            "We appreciate your business.  " + "</br></br>";

                            oMailItem.HTMLBody += signature + "</br></br>";
                            oMailItem.To = oMsg.SenderEmailAddress;
                            oMailItem.Send();
                            oNS1.Logoff();
                            oNS1 = null;
                            oApp = null;
                        }
                        //Comprae with fax sender mail Id
                        else if (oMsg.SenderEmailAddress == MailScanFaxEmail)
                        {
                            Outlook.Application oApp = new Outlook.Application();

                            // Get the NameSpace and Logon information.
                            Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");

                            // Log on by using a dialog box to choose the profile.
                            oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

                            Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                            try
                            {
                                oApp.Inspectors.Add(oMailItem.GetInspector);
                            }
                            catch
                            {
                            }
                            oMailItem.Subject = "Fax Unsuccessful – No attachment - Please read below";
                            string signature = oMailItem.HTMLBody;
                            oMailItem.HTMLBody = "</br>You have attempted to send a fax to CommissionsDept  and it was not properly received.   </br>" +
                                             "Please refax your commission statements to 1-800-343-4072 or contact CommissionsDept for assistance.  </br>" +
                                             "We appreciate your business.    </br></br>";
                            oMailItem.HTMLBody += signature + "</br></br>";
                            string housemail = GetLicenseeHouseEmailOfFax(_faxnumber);
                            if (housemail != null)
                            {
                                oMailItem.To = housemail;
                            }
                            else
                            {
                                oMailItem.To = oMsg.SenderEmailAddress;
                            }

                            oMailItem.Send();
                        }
                        continue;
                    }
                    #endregion

                    #region "Create and upload batch"

                    string errormsg = "";
                    LicenseeComapany = null;
                    UploadedBatch = null;
                    //Both pdf and excel file available with mail
                    if (PdfFiles.Count > 0 && ExcelAndtxtFile.Count > 0)
                    {
                        foreach (FileInfo fild in PdfFiles)
                        {
                            BatchCreation(oMsg, fild.FullName, _faxnumber);
                        }

                        foreach (var strFileName in ExcelAndtxtFile)
                        {
                            if (strFileName.Extension.ToLower().Contains(".xls") || strFileName.Extension.ToLower().Contains(".xlsx") || strFileName.Extension.ToLower().Contains(".csv") || strFileName.Extension.ToLower().Contains(".txt"))
                            {
                                BatchCreation(oMsg, Convert.ToString(strFileName), _faxnumber, strFileName.FullName);
                            }
                        }
                    }
                    //Only pdf files available wit mail
                    else if (PdfFiles.Count > 0)
                    {
                        foreach (FileInfo fild in PdfFiles)
                        {
                            BatchCreation(oMsg, fild.FullName, _faxnumber);                            
                        }

                    }
                    //Only Excel or text  files available with mail  
                    else if (ExcelAndtxtFile.Count > 0)
                    {
                        foreach (var strFileName in ExcelAndtxtFile)
                        {
                            if (strFileName.Extension.ToLower().Contains(".xls") || strFileName.Extension.ToLower().Contains(".xlsx") || strFileName.Extension.ToLower().Contains(".csv") || strFileName.Extension.ToLower().Contains(".txt"))
                            {
                                BatchCreation(oMsg, Convert.ToString(strFileName), _faxnumber, strFileName.FullName);
                            }
                        }
                    }

                    #endregion
                    
                }
            }
        }

        string GetLicenseeHouseEmailOfFax(long faxnumber)
        {
            List<User> _UserLst = null;
            List<User> usertemp = null;
            try
            {

                usertemp = User.GetAllUsers().ToList();
                _UserLst = new List<User>();
                foreach (User uss in usertemp)
                {
                    if (FaxNumber(uss.Fax) == faxnumber)
                    {
                        _UserLst.Add(uss);
                    }
                }
            }
            catch
            {
                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + "-- Problem in Seraching Email/Fax in DataBase..Possible Reason--DataBase Close");

                }
            }

            if (_UserLst != null && _UserLst.Count == 1)
            {
                //old code
                //return usertemp.Where(p => p.LicenseeId == _UserLst.FirstOrDefault().LicenseeId).Where(p => p.IsHouseAccount == true).FirstOrDefault().Email;
                //New Code by 28092012
                //Get administrator email id on the basis of role ID
                return usertemp.Where(p => p.LicenseeId == _UserLst.FirstOrDefault().LicenseeId).Where(p => p.Role == UserRole.Administrator).FirstOrDefault().Email;
            }
            else if (_UserLst != null && _UserLst.Count >= 1)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        bool BatchCreation(Outlook.MailItem oMsg, string file, long faxnum, string optionalstr = "Null")
        {
            NoOfLicensee = 0;
            string SendermailId = oMsg.SenderEmailAddress;
            List<User> _UserLst;
            User _User;
            bool Flag = true;
            bool isFax = false;
            try
            {
                if (SendermailId == MailScanFaxEmail)
                {
                    List<User> usertemp = User.GetAllUsers().ToList();
                    _UserLst = new List<User>();
                    foreach (User uss in usertemp)
                    {
                        if (FaxNumber(uss.Fax) == faxnum)
                        {
                            _UserLst.Add(uss);
                        }
                    }
                    isFax = true;                    

                }
                else
                {
                    //old code by ankur
                    //_UserLst = User.GetAllUsers().Where(p => p.Email != null && p.Email.ToUpper() == SendermailId.ToUpper()).ToList();
                    //pass parameter to upload
                    _UserLst = User.GetAllUsers(SendermailId.ToUpper()).Where(p => p.Email != null && p.Email.ToUpper() == SendermailId.ToUpper()).ToList();
                    isFax = false;     
                    
                }
            }
            catch
            {
                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + "-- Problem in Seraching Email/Fax in DataBase..Possible Reason--DataBase Close");
                    Flag = false;
                    return Flag;
                    //Mail content
                   
                }
            }

            if (_UserLst != null && _UserLst.Count > 1)
            {
                Guid? LiceId = _UserLst.FirstOrDefault().LicenseeId;
                if (_UserLst.Where(p => p.LicenseeId == LiceId).Count() == _UserLst.Count)
                {
                    _User = _UserLst.FirstOrDefault();
                    NoOfLicensee = 1;
                }
                else
                {
                    List<Guid?> LiGuid = _UserLst.Select(p => p.LicenseeId).Distinct().ToList();
                    LicenseeComapany = "";
                    foreach (Guid gu in LiGuid)
                    {
                        LicenseeComapany += _UserLst.Where(p => p.LicenseeId == gu).FirstOrDefault().Company + "</br>";
                    }
                    _User = null;
                    NoOfLicensee = 2;

                    if (isFax)
                    {
                        //Send mail with fax number number not register                    
                        MailContentFaxNumberWithMultiplelicensees(oMsg, faxnum);
                    }
                    else
                    {
                        MailContentEmailAddressWithMultiplelicensees(oMsg);
                    }

                }
            }
            else if (_UserLst != null && _UserLst.Count == 1)
            {
                _User = _UserLst.FirstOrDefault();
                NoOfLicensee = 1;

            }
            else
            {
                _User = null;
                NoOfLicensee = 0;

                if (isFax)
                {
                    //Send mail with fax number number not register                    
                    MailContentFaxNumberNotRegistered(oMsg, faxnum);
                }
                else
                {
                    MailContentEmailNotRegistered(oMsg);
                }
            }

            if (_User != null)
            {
                try
                {
                    file = file.ToLower();
                    string strFileExtension = System.IO.Path.GetExtension(file);
                    string strFileType = strFileExtension.Replace(".", "");

                    Batch NewBatch = new Batch();
                    NewBatch.BatchId = Guid.NewGuid();                    
                    NewBatch.CreatedDate = DateTime.Now.Date;

                    if (strFileType.ToLower().Equals("pdf"))
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
                    //NewBatch.EntryStatus = EntryStatus.ImportPending;
                    NewBatch.FileType = strFileType;
                    NewBatch.LicenseeId = _UserLst[0].LicenseeId.Value;
                    LicenseeDisplayData _Licensee = Licensee.GetLicenseeByID(NewBatch.LicenseeId);
                    NewBatch.LicenseeName = _Licensee.Company;

                    int batchNumber = NewBatch.AddUpdate();
                    if (batchNumber == 0)
                    {
                        Flag = false;
                        return Flag;
                        //Mail content
                    }
                    NewBatch.BatchNumber = batchNumber;
                    using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                    {
                        sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Batch Created in DataBase " + "Batch Id :" + NewBatch.BatchId + " Batch Number " + NewBatch.BatchNumber
                            + " Company " + _Licensee.Company);

                    }

                    string KeyValue = MyAgencyVault.BusinessLibrary.Masters.SystemConstant.GetKeyValue("WebDevPath");
                    MyAgencyVault.BusinessLibrary.WebDevPath ObjWebDevPath = MyAgencyVault.BusinessLibrary.WebDevPath.GetWebDevPath(KeyValue);
                    FileUtility ObjUpload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);

                    if (strFileExtension.Contains(".pdf"))
                    {                        
                        NewBatch.FileName = _Licensee.Company + "_" + batchNumber.ToString() + ".pdf";

                        string strName = System.IO.Path.GetFileName(file);
                        string strTempPath = System.IO.Path.GetTempPath();
                        strTempPath = strTempPath + strName;
                        string OutputFile = strTempPath;

                        OutputFile = StampOnPDF(file, strName, batchNumber.ToString(), NewBatch.LicenseeName.ToString());

                        //string KeyValue = MyAgencyVault.BusinessLibrary.Masters.SystemConstant.GetKeyValue("WebDevPath");
                        //MyAgencyVault.BusinessLibrary.WebDevPath ObjWebDevPath = MyAgencyVault.BusinessLibrary.WebDevPath.GetWebDevPath(KeyValue);
                        //FileUtility ObjUpload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                        ObjUpload.UploadComplete += new UploadCompleteDel(ObjUpload_UploadComplete);
                        //ObjUpload.Upload(file, @"/UploadBatch/" + NewBatch.FileName, NewBatch);
                        ObjUpload.Upload(OutputFile, @"/UploadBatch/" + NewBatch.FileName, NewBatch);
                        UploadedBatch += NewBatch.FileName + "  ";
                        //Send Mail After succefully pdf uploaded 
                        MailContentPDFSuccesfullyUpload(oMsg, faxnum, NewBatch.BatchNumber);

                        System.Threading.Thread.Sleep(10000);

                        using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                        {
                            sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Batch uploded at server " + "Path:" + file + " File name " + NewBatch.BatchNumber
                                + " Company " + _Licensee.Company);

                        }
                    }
                    else
                    {
                        if (strFileExtension.Contains(".xls") || strFileExtension.Contains(".xlsx") || strFileExtension.Contains(".csv") || strFileExtension.Contains(".txt"))
                        {
                            try
                            {
                                //string strFileName = _Licensee.Company + "_" + _Licensee.LicenseeId.ToString() + "_" + batchNumber.ToString() + "_" + file;
                                //Remove underscores from file name before rename the file name(naming convention)
                                string strFileName = _Licensee.Company + "_" + _Licensee.LicenseeId.ToString() + "_" + batchNumber.ToString() + "_" + file.Replace("_","").Trim();
                                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                                {
                                    sw.WriteLine(DateTime.Now.ToLongTimeString() + " - Batch uploded at server " + "Path:" + optionalstr + " File name " + strFileName
                                        + " Company " + _Licensee.Company);

                                }

                                if (optionalstr != "Null")
                                {
                                    //update batch filename                                    
                                    NewBatch.UpdateBatchFileName(batchNumber, strFileName);
                                    ObjUpload.Upload(optionalstr, @"/Uploadbatch/Import/Processing/" + strFileName, NewBatch);
                                }

                                //Send Mail After succefully Excel uploaded 
                                MailContentExcelSuccesfullyUpload(oMsg, faxnum, NewBatch.BatchNumber);
                                
                            }
                            catch (Exception ex)
                            {
                                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                                {
                                    sw.WriteLine(DateTime.Now.ToLongTimeString() + ex.ToString()); 
                                }
                            }
                        }
                    }                   
                }
                catch (Exception ex)
                {
                    using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                    {
                        sw.WriteLine(DateTime.Now.ToLongTimeString() + "File is not uploded " + ex.ToString());
                        Flag = false;
                        return Flag;

                        //Mail content
                    }
                }
            }
            else
            {
                
                Flag = false;
                return Flag;

                //Mail content
            }
            return Flag;
        }

        private void MailContentEmailNotRegistered(Outlook.MailItem oMsg)
        {
            oMsg.Move(moveUnSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");

            if (NoOfLicensee == 0 && (oMsg.SenderEmailAddress != MailScanFaxEmail))
            {
                Outlook.Application oApp = new Outlook.Application();

                Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");

                oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

                Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                try
                {
                    oApp.Inspectors.Add(oMailItem.GetInspector);
                }
                catch
                {

                }

                oMailItem.Subject = "Upload Unsuccessful – Email not registered. - Please read below";
                // string mailContent = oMsg.HTMLBody;
                string signature = oMailItem.HTMLBody;
                oMailItem.HTMLBody = "</br>You have attempted to send an email to upload@commissionsdept.com. </br>" +
                               "Please make sure your FROM email address appears in at least one user in MyAgencyVault.</br>" +
                               "Then, resend your statements to upload@commissionsdept.com.  If you need any further assistance, please contact us.</br>" +
                               "We appreciate your business.  " + "</br></br>";
                // oMailItem.HTMLBody += mailContent;
                oMailItem.HTMLBody += signature + "</br></br>";
                oMailItem.To = oMsg.SenderEmailAddress;

                oMailItem.Send();

                oNS1.Logoff();
                oNS1 = null;
                oApp = null;
            }
        }

        private void MailContentFaxNumberNotRegistered(Outlook.MailItem oMsg, long _faxnumber)
        {
            oMsg.Move(moveUnSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");

            if (NoOfLicensee == 0 && (oMsg.SenderEmailAddress == MailScanFaxEmail))
            {
                Outlook.Application oApp = new Outlook.Application();

                // Get the NameSpace and Logon information.
                Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");

                // Log on by using a dialog box to choose the profile.
                oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

                Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                try
                {
                    oApp.Inspectors.Add(oMailItem.GetInspector);

                }
                catch
                {
                }
                oMailItem.To = _faxnumber + @"@sendfax.fax2me.com";
                //oMailItem.CC = MailScanErrorEmail;
                try
                {
                    oMailItem.CC = oMsg.SenderEmailAddress;
                }
                catch
                {
                }
                string mailContent = oMsg.HTMLBody;

                string signature = oMailItem.HTMLBody;
                oMailItem.Subject = "Upload Unsuccessful –Fax Number not registered. - Please read below";
                oMailItem.HTMLBody = "You have attempted to send a fax to CommissionsDept.  </br>" +
                    "Please make sure the number of the fax machine you are sending from appears in at least one user of MyAgencyVault." +
                    "Then, refax your statements to 1-800-343-4072.  If you need any further assistance, please contact us.</br>" +
                    "We appreciate your business.  " + "</br></br>";
                //oMailItem.HTMLBody += mailContent;
                oMailItem.HTMLBody += signature;
                //oMailItem.Display(true);
                oMailItem.Send();
                oNS1.Logoff();
                oNS1 = null;
                oApp = null;

            }
        }

        private void MailContentEmailAddressWithMultiplelicensees(Outlook.MailItem oMsg)
        {
            oMsg.Move(moveUnSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");

            if (NoOfLicensee == 2 && (oMsg.SenderEmailAddress != MailScanFaxEmail))
            {
                Outlook.Application oApp = new Outlook.Application();
                // Get the NameSpace and Logon information.
                Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");
                // Log on by using a dialog box to choose the profile.
                oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);
                Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                try
                {
                    if (oMailItem != null)
                        oApp.Inspectors.Add(oMailItem.GetInspector);
                }
                catch { }

                oMailItem.To = MailScanErrorEmail;
                try
                {
                    oMailItem.CC = oMsg.SenderEmailAddress;
                }
                catch
                {
                }
                string mailContent = oMsg.HTMLBody;
                oMailItem.Subject = "FW: " + oMsg.Subject;
                string signature = oMailItem.HTMLBody;
                oMailItem.HTMLBody = "</br>Service alert - Upload Unsuccessful – Email address with multiple licensees</br></br>" +
                                "Licensees:</br></br>";
                oMailItem.HTMLBody += LicenseeComapany + "</br></br>";
                oMailItem.HTMLBody += mailContent + "</br></br>";
                oMailItem.HTMLBody += signature;

                oMailItem.Send();

                oNS1.Logoff();
                oNS1 = null;
                oApp = null;

            }
        }

        private void MailContentFaxNumberWithMultiplelicensees(Outlook.MailItem oMsg, long _faxnumber)
        {
            oMsg.Move(moveUnSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");

            if (NoOfLicensee == 2 && (oMsg.SenderEmailAddress == MailScanFaxEmail))
            {
                Outlook.Application oApp = new Outlook.Application();
                // Get the NameSpace and Logon information.
                Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");
                // Log on by using a dialog box to choose the profile.
                oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);
                Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                try
                {
                    oApp.Inspectors.Add(oMailItem.GetInspector);
                }
                catch
                {

                }

                oMailItem.Subject = "FW: " + oMsg.Subject;
                oMailItem.To = _faxnumber + @"@sendfax.fax2me.com";
                //oMailItem.CC = MailScanErrorEmail;
                string signature = oMailItem.HTMLBody;
                string mailContent = oMsg.HTMLBody;
                oMailItem.HTMLBody = "</br>Service alert - Fax Unsuccessful – Fax Number with multiple licensees</br></br>>" +
                                "Licensees:</br></br>";
                oMailItem.HTMLBody += LicenseeComapany + "</br></br>";
                oMsg.HTMLBody += mailContent + "</br></br>";
                oMailItem.HTMLBody += signature;

                oMailItem.Send();

                oNS1.Logoff();
                oNS1 = null;
                oApp = null;
            }

        }

        private void MailContentPDFSuccesfullyUpload(Outlook.MailItem oMsg, long _faxnumber, int intBatchNumber)
        {
            oMsg.Move(moveSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");
            Outlook.Application oApp = new Outlook.Application();
            // Get the NameSpace and Logon information.
            Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");
            // Log on by using a dialog box to choose the profile.
            oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

            Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            try
            {
                oApp.Inspectors.Add(oMailItem.GetInspector);
            }
            catch{}
            //Change code 27092012
            //If mail come from fax
            if (oMsg.SenderEmailAddress == MailScanFaxEmail)
            {
                string adminmail = GetLicenseeHouseEmailOfFax(_faxnumber);
                if (adminmail != null)
                {
                    oMailItem.To = adminmail;
                }
                else
                {
                    //Discuss with eric 28 sep 2012
                    oMailItem.To = "service@commissionsdept.com";
                }
            }
            else //if mail come from E-mail 
            {
                oMailItem.To = oMsg.SenderEmailAddress;
            }

            //oMailItem.CC = MailScanErrorEmail;
            oMailItem.Subject = "Upload Successful – Batch # " + intBatchNumber;
            //string mailContent = oMsg.HTMLBody;
            string signature = oMailItem.HTMLBody;
            oMailItem.HTMLBody = "CommissionsDept.com has received your statement(s) and successfully uploaded to your account.  </br>" +
                          "Data entry will begin shortly.  If you need any further assistance, please contact us.</br>" +
                          "We appreciate your business.</br></br>  ";
            //  oMailItem.HTMLBody += mailContent;
            oMailItem.HTMLBody += signature;
            // oMailItem.Display(true);
            oMailItem.Send();
        }

        private void MailContentExcelSuccesfullyUpload(Outlook.MailItem oMsg, long _faxnumber,int intBatchNumber)
        {
            oMsg.Move(moveSucessMail);

            Outlook.NameSpace oNS = OutLookApp.GetNamespace("mapi");
            Outlook.Application oApp = new Outlook.Application();
            // Get the NameSpace and Logon information.
            Outlook.NameSpace oNS1 = oApp.GetNamespace("mapi");
            // Log on by using a dialog box to choose the profile.
            oNS1.Logon(oNS.CurrentUser.Name, MailScanErrorEmailPassword, true, true);

            Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            try
            {
                oApp.Inspectors.Add(oMailItem.GetInspector);
            }
            catch { }
            //Change code 27092012
            //If mail come from fax
            if (oMsg.SenderEmailAddress == MailScanFaxEmail)
            {
                string adminmail = GetLicenseeHouseEmailOfFax(_faxnumber);
                if (adminmail != null)
                {
                    oMailItem.To = adminmail;
                }
                else
                {
                    //Discuss with eric 28 sep 2012
                    oMailItem.To = "service@commissionsdept.com";
                }
            }
            else //if mail come from E-mail 
            {
                oMailItem.To = oMsg.SenderEmailAddress;
            }

            //oMailItem.CC = MailScanErrorEmail;
            oMailItem.Subject = "Upload Successful – Batch # " + intBatchNumber;
            //string mailContent = oMsg.HTMLBody;
            string signature = oMailItem.HTMLBody;
            oMailItem.HTMLBody = "CommissionsDept.com has received your statement(s) and successfully uploaded to your account.  </br>" +
                          "Data import by import tool begin shortly.  If you need any further assistance, please contact us.</br>" +
                          "We appreciate your business.</br></br>  ";
            //  oMailItem.HTMLBody += mailContent;
            oMailItem.HTMLBody += signature;
            // oMailItem.Display(true);
            oMailItem.Send();
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
                cb.ShowText("              " + "Batch#" + strbatchNumber + "         ST# _____________" + "   " + strAgencyName + "        " + System.DateTime.Now + "    " + "DEU:____________ Review: _____________ " + "Page " + currentPage.ToString() + " of " + pageCount.ToString() + "");
                cb.EndText();
                cb1.AlterContents();

                writer.AddPage(page);
            }

            pdfDoc.Close();

            return strTempPath;


        }

        void ObjUpload_UploadComplete(int statusCode, object state)
        {
            Batch batch = state as Batch;

            if (!statusCode.ToString().StartsWith("20"))
            {
                //Call function to delete batch
                batch.DeleteBatch(batch.BatchId, MyAgencyVault.BusinessLibrary.UserRole.SuperAdmin);
                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    sw.WriteLine("-Fail--- " + DateTime.Now.ToLongTimeString() + "- Uploading Fail, Batch Id: " + batch.BatchId + " LicenseeId " + batch.LicenseeId
                        + " Licensee name " + batch.LicenseeName + " Batch Number " + batch.BatchNumber);

                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(LogFilePath, true))
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + "- PDF Uploaded " + batch.FileName);

                }
            }

            //change on 19012012
            //batch.EntryStatus = EntryStatus.Unassigned;
        }

        public void ForwardMail(Outlook.MailItem oMsg)
        {
            //Outlook._MailItem oMailItem = (Outlook._MailItem)OutLookApp.CreateItem(Outlook.OlItemType.olMailItem);
            //oMailItem.To = "ankurastogi@gmail.com";
            //oMailItem.Subject = oMsg.Subject + " - Error--";
            //oMailItem.Body = oMsg.Body;
            //oMailItem.BodyFormat = oMsg.BodyFormat;
            //oMailItem.SaveSentMessageFolder = oMsg.Body;
            //oMailItem. = oMsg.Attachments;
            //oMailItem.Save();
            //oMailItem.Send();
            //oMsg.To = "ankurastogi@gmail.com";

            oMsg.To = oMsg.SenderEmailAddress;
            oMsg.CC = MailScanErrorEmail;
            oMsg.Send();
            //oMailItem.Body = oMsg.Body;
        }
        #endregion

        private long FaxNumber(string fax)
        {

            string ffax = "0";
            if (fax != null)
            {
                foreach (char item in fax)
                {
                    if (item >= '0' && item <= '9')
                        ffax += item;
                }
            }
            return long.Parse(ffax);
        }

        public int NoOfLicensee { get; set; }
        public string LicenseeComapany { get; set; }
        public string UploadedBatch { get; set; }
        public string LicenseeHouseEmail { get; set; }
        public string sendfaxtome { get; set; }

    }
}
