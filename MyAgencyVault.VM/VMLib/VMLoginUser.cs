using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;
using MyAgencyVault.ViewModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using System.Workflow.ComponentModel;
using System.IO;
using System.Collections;
using System.Windows;
using System.Security.Permissions;
using System.Security;
using MyAgencyVault.EmailFax;
using System.Net;
using System.Xml;
using MyAgencyVault.VMLib;

namespace MyAgencyVault.ViewModel.VMLib
{
    public class VMLoginUser : BaseViewModel
    {
        #region "Command Decleration"
        public ICommand _loginClick = null;
        public ICommand _ForgotPassword = null;
        public ICommand _SendEmailClick = null;
        public ICommand _OnChecked = null;
        public ICommand _closeCommand = null;
        public ICommand _OpenCommand = null;
        public ICommand _OpenCommandFPassword = null;
        public ICommand _OnCloseFPassword = null;
        public ICommand _OnCloseAplication = null;
        public ICommand _OnOpenLoginForm = null;
        #endregion

        #region "Locial Variable"

        private string _userName = string.Empty;
        private string _passWord = string.Empty;
        private string _eMail = string.Empty;
        private string _pHintQ = string.Empty;
        private string _pHintA = string.Empty;
        private string _loginStatus = string.Empty;
        private string filePath = string.Empty;
        private string dirPath = string.Empty;
        private string _forgotPassStatus = string.Empty;
        private MyAgencyVault.VM.MyAgencyVaultSvc.UserRole _UserRole;
        //private bool _loginResult;
        private bool _rChecked;
        private bool _sChecked;
        public delegate void OnOpenWindow();
        public event OnOpenWindow onOpenWindowClick;
        ArrayList loginInfoList = null;
        User _userInfo = null;


        #endregion

        /// <summary>
        ///  Default  constructor
        /// </summary>
        public VMLoginUser()
        {
            loginInfoList = new ArrayList();
            dirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\MyAgencyVault");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            filePath = string.Concat(dirPath, @"\LoginInfo.txt");
            //filePath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "LoginInfo.txt");
            if (ReadFile()) SetLoginInfo();

        }

        #region "Command"
        /// <summary>
        /// To get login command
        /// </summary>
        public ICommand Login_Click
        {
            get
            {
                if (_loginClick == null)
                {
                    _loginClick = new BaseCommand(param => CanLogin(), param => DoLogin());

                }
                return _loginClick;
            }

        }

        /// <summary>
        /// To get login command
        /// </summary>
        public ICommand SendEmailClick
        {
            get
            {
                if (_SendEmailClick == null)
                {
                    _SendEmailClick = new BaseCommand(param => CanSendMail(), param => DoSendMail());

                }
                return _SendEmailClick;
            }

        }

        public event Action CloseFPassword;

        /// <summary>
        /// To get login command
        /// </summary>
        public ICommand OnCloseFPassword
        {
            get
            {
                if (_OnCloseFPassword == null)
                {
                    _OnCloseFPassword = new BaseCommand(param => CanCloseFPassword(), param => DoCloseFPassword());

                }
                return _OnCloseFPassword;
            }

        }

        public event Action OpenLoginForm;

        /// <summary>
        /// To get login command
        /// </summary>
        public ICommand OnOpenLoginForm
        {
            get
            {
                if (_OnOpenLoginForm == null)
                {
                    _OnOpenLoginForm = new BaseCommand(param => CanOpenLoginForm(), param => DoOpenLoginForm());

                }
                return _OnOpenLoginForm;
            }

        }

        public event Action CloseAplication;

        /// <summary>
        /// To get login command
        /// </summary>
        public ICommand OnCloseAplication
        {
            get
            {
                if (_OnCloseAplication == null)
                {
                    _OnCloseAplication = new BaseCommand(param => CanCloseAplication(), param => DoCloseAplication());

                }
                return _OnCloseAplication;
            }

        }

        /// <summary>
        /// To get OnChecked command
        /// </summary>
        public ICommand OnChecked
        {

            get
            {
                if (_OnChecked == null)
                {
                    _OnChecked = new BaseCommand(param => CanOnChecked(), param => DoForOnChecked());

                }
                return _OnChecked;
            }

        }


        #endregion

        #region "Method Decleration"
        /// <summary>
        /// check for user login
        /// </summary>
        /// <returns></returns>
        private bool CanLogin()
        {
            return true;
        }

        /// <summary>
        /// check for  login form open
        /// </summary>
        /// <returns></returns>
        private bool CanOpenLoginForm()
        {
            return true;
        }

        /// <summary>
        /// check for close application 
        /// </summary>
        /// <returns></returns>
        private bool CanCloseAplication()
        {
            return true;
        }

        /// <summary>
        /// check for Forgot Password
        /// </summary>
        /// <returns></returns>
        private bool CanCloseFPassword()
        {
            return true;
        }

        /// <summary>
        /// Do processes for Close Aplication
        /// </summary>
        /// <returns></returns>
        public void DoCloseAplication()
        {
            if (CloseAplication != null)
            {
                if (RembChecked != true)
                {
                    Password = string.Empty;
                    UserName = string.Empty;
                }
                //Acme - March 17, 2017 added code to track logout time
                serviceClients.UserClient.AddLoginLogoutTime(RoleManager.userCredentialID, System.Configuration.ConfigurationSettings.AppSettings["DisplayVersion"], "Logout");

                CloseAplication();
                //DoOpenLoginForm();
            }
        }

        /// <summary>
        /// Do processes for Close Forgot Password
        /// </summary>
        /// <returns></returns>
        private void DoCloseFPassword()
        {
            if (CloseFPassword != null)
            {
                CloseFPassword();
            }
        }

        /// <summary>
        /// Do processes for Open Login Form
        /// </summary>
        /// <returns></returns>
        private void DoOpenLoginForm()
        {
            if (OpenLoginForm != null)
            {

                OpenLoginForm();
            }
        }

        /// <summary>
        /// Do processes for Open Main Form
        /// </summary>
        /// <returns></returns>
        public void DoOpen()
        {
            if (RequestOpen != null)
            {
                RequestOpen();
            }
        }

        /// <summary>
        /// check for Open Main Form
        /// </summary>
        /// <returns></returns>
        private bool CanOpen()
        {
            return true;
        }

        /// <summary>
        /// Do processes for close login Form
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (RequestClose != null)
            {
                RequestClose();
            }
        }

        /// <summary>
        /// check for Open Forgot Password
        /// </summary>
        /// <returns></returns>
        private bool CanOpenFPassword()
        {
            return true;
        }

        /// <summary>
        /// Do processes for Forgot Password
        /// </summary>
        /// <returns></returns>
        private void DoOpenFPassword()
        {
            if (RequestOpenFPassword != null)
            {
                //GetLoginInfo();
                //RequestOpenFPassword();
            }
        }

        private bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// check for Sign in checked
        /// </summary>
        /// <returns></returns>
        private bool CanOnChecked()
        {
            return true;
        }

        /// <summary>
        /// check for Send Mail
        /// </summary>
        /// <returns></returns>
        private bool CanSendMail()
        {
            return true;
        }
        /// <summary>
        /// Do processes for Sign in checked
        /// </summary>
        private void DoForOnChecked()
        {
            try
            {
                if (SChecked == true)
                {
                    RembChecked = true;
                }
                else
                {
                    RembChecked = false;
                }
            }
            catch
            {
            }

        }

        /// <summary>
        /// Do processes for Send Mail
        /// </summary>
        private void DoSendMail()
        {
            try
            {
                if (CheckForPHintQ())
                {
                    SendMail.Email = EMail;
                    SendMail.SendEmail();
                    ForgotPassStatus = "Mail sent successfully";
                }
                else
                {
                    ForgotPassStatus = "Mail Can't be sent";
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Check for Password Hint Answer
        /// </summary>
        /// <returns></returns>
        private bool CheckForPHintQ()
        {
            if (string.IsNullOrEmpty(UserInfo.PasswordHintQ)) return false;
            if (string.IsNullOrEmpty(UserInfo.PasswordHintA)) return false;
            if (UserInfo.PasswordHintA.Contains(PHintA))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Encript data and write file on disk
        /// </summary>
        private void EncriptData()
        {
            string encUserName = string.Empty;
            string encUserPassword = string.Empty;
            string remStatus = string.Empty;
            string sStatus = string.Empty;
            try
            {
                encUserName = EncriptClass.Encrypt(UserName);
                encUserPassword = EncriptClass.Encrypt(Password);
                remStatus = EncriptClass.Encrypt("R " + RembChecked);
                sStatus = EncriptClass.Encrypt("S " + SChecked);

                WriteFile(encUserName, encUserPassword, remStatus, sStatus);
            }
            catch 
            {
            }

        }

        public void OnDirectLogin()
        {
            try
            {
                if (!File.Exists(string.Concat(dirPath, @"\Application.logout")))
                {
                    if ((UserInfo.SStatus).Equals("S True"))
                    {
                        //Close();
                        DoLogin();
                    }
                }
                else
                {
                    FileIOPermission fp = new FileIOPermission(PermissionState.Unrestricted);
                    fp.Assert();
                    File.Delete(string.Concat(dirPath, @"\Application.logout"));
                    CodeAccessPermission.RevertAssert();

                }
            }
            catch 
            {
               // throw new Exception();
            }
        }

        public bool GetLoginInfo(bool forgetPassword)
        {
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              if (forgetPassword)
                UserInfo = serviceClients.UserClient.GetValidIdentityUsingName(UserName);
              else
              {
                UserClient _userclient = serviceClients.UserClient;
                UserInfo = _userclient.GetValidIdentity(UserName, Password);
              }

              if (UserInfo == null)
                return false;

              if ((UserInfo.Role != UserRole.SuperAdmin) && (UserInfo.IsDeleted == true || UserInfo.IsLicenseDeleted == true))
                return false;

              if (UserInfo.Role == UserRole.SuperAdmin)
                UserInfo.Permissions = null;

              PHintQ = UserInfo.PasswordHintQ;
              UserRole = UserInfo.Role;
            }
          }
          catch(Exception ex)
          {
            return false;
          }
            return true;
        }

        ObservableCollection<SystemConstant> _systemConstantsDetail = null;
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

        private bool _IsBusy;
        public bool IsBusy
        {
            get 
            {
                return _IsBusy;
            }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }
  
        public void LoadUserData()
        {
            PHintA = string.Empty;
            PHintQ = string.Empty;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                this.GetLoginInfo(true);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public void SendPassMail()
        {
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              if (string.Compare(PHintA, UserInfo.PasswordHintA, true) == 0)
              {
                if (UserInfo.Email != null && UserInfo.Email.Trim() != string.Empty)
                {
                  if (serviceClients.SendMailClient.SendMail(UserInfo.Email, "PasswHanu@gord Recovery Mail", "Password:" + UserInfo.Password))
                  {
                    ForgotPassStatus = "Mail sent successfully";
                    PHintA = string.Empty;
                  }
                  else
                  {
                    ForgotPassStatus = "Mail is not sent successfully due to some problem";
                    PHintA = string.Empty;
                  }
                }
                else
                {
                  ForgotPassStatus = "System does not have mail address for this user.";
                  PHintA = string.Empty;
                }
              }
              else
              {
                ForgotPassStatus = "Information is not correct.";
                PHintA = string.Empty;
              }
            }
          }
          catch
          {
          }
        }

        private DateTime GetNISTDate(bool convertToLocalTime)
        {
            Random ran = new Random(DateTime.Now.Millisecond);
            DateTime date = DateTime.Today;
            string serverResponse = string.Empty;

            // Represents the list of NIST servers
            string[] servers = new string[] {
                         "64.90.182.55",
                         "206.246.118.250",
                         "207.200.81.113",
                         "128.138.188.172",
                         "64.113.32.5",
                         "64.147.116.229",
                         "64.125.78.85",
                         "128.138.188.172"
                          };

            // Try each server in random order to avoid blocked requests due to too frequent request
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    // Open a StreamReader to a random time server
                    StreamReader reader = new StreamReader(new System.Net.Sockets.TcpClient(servers[ran.Next(0, servers.Length)], 13).GetStream());
                    serverResponse = reader.ReadToEnd();
                    reader.Close();

                    // Check to see that the signiture is there
                    if (serverResponse.Length > 47 && serverResponse.Substring(38, 9).Equals("UTC(NIST)"))
                    {
                        // Parse the date
                        int jd = int.Parse(serverResponse.Substring(1, 5));
                        int yr = int.Parse(serverResponse.Substring(7, 2));
                        int mo = int.Parse(serverResponse.Substring(10, 2));
                        int dy = int.Parse(serverResponse.Substring(13, 2));
                        int hr = int.Parse(serverResponse.Substring(16, 2));
                        int mm = int.Parse(serverResponse.Substring(19, 2));
                        int sc = int.Parse(serverResponse.Substring(22, 2));

                        if (jd > 51544)
                            yr += 2000;
                        else
                            yr += 1999;

                        date = new DateTime(yr, mo, dy, hr, mm, sc);

                        // Convert it to the current timezone if desired
                        if (convertToLocalTime)
                            date = date.ToLocalTime();

                        // Exit the loop
                        break;
                    }

                }
                catch 
                {
                    /* Do Nothing...try the next server */
                }
            }

            return date;
        }

        //bool sendMailWithAttachment(string toAddress, string subject, string body, string fileName)
        //{
        //    MyAgencyVault.BusinessLibrary.MailServerDetail mInfo = MyAgencyVault.BusinessLibrary.getDevMailServerDetail();

        //    //MyAgencyVault.BusinessLibrary.MailServerDetail mInfo = new MyAgencyVault.BusinessLibrary.MailServerDetail();
        //    //mInfo.ServerName = "smtp.gmail.com";
        //    //mInfo.Email = "testcommissiondept@gmail.com";
        //    //mInfo.UserName = "testcommissiondept@gmail.com";
        //    //mInfo.Password = "Acme2018";
        //    //mInfo.PortNo = "587";



        //    if (string.IsNullOrEmpty(toAddress))
        //        toAddress = mInfo.Email;

        //    bool mailSendSuccessfully = true;

        //    try
        //    {
        //        EmailFax.EmailFax wpfMail = new EmailFax.EmailFax(mInfo.UserName, mInfo.Password, mInfo.Email, toAddress, mInfo.ServerName, mInfo.PortNo, subject, body);
        //        wpfMail.IsHtml = true;
        //        wpfMail.BCC = System.Configuration.ConfigurationManager.AppSettings["ReportBCC"];
        //        wpfMail.AttachmentPath = fileName;
        //        wpfMail.SendEmail();

        //    }
        //    catch
        //    {
        //        mailSendSuccessfully = false;
        //    }

        //    return mailSendSuccessfully;

        //}
        /// <summary>
        /// Do processes for user login
        /// </summary>
        private void DoLogin()
        {
         //   sendMailWithAttachment("jyotisna@acmeminds.com", "test", "test", "E:\\test.xls");
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                
                if (!GetLoginInfo(false))
                {
                    LoginStatus = "Login Failed";
                    return;
                }
                else
                {
                    RoleManager.Role = UserInfo.Role;
                    RoleManager.LoggedInUser = UserName;
                    RoleManager.userCredentialID = UserInfo.UserCredentialID;
                    RoleManager.IsHouseAccount = UserInfo.IsHouseAccount;
                    RoleManager.LicenseeId = UserInfo.LicenseeId ?? Guid.Empty;
                    RoleManager.LicenseName = UserInfo.Company;
                    RoleManager.IsNewsToFlash = UserInfo.IsNewsToFlash;
                    RoleManager.UserPermissions = UserInfo.Permissions == null ? null : UserInfo.Permissions.ToList();                   
                    RoleManager.HouseOwnerId = UserInfo.HouseOwnerId;
                    RoleManager.AdminId = UserInfo.AdminId;
                    RoleManager.WebDavPath = UserInfo.WebDavPath;
                    RoleManager.IsEditDisable = UserInfo.DisableAgentEditing;

                    BackgroundWorker worker2 = new BackgroundWorker();
                    worker2.DoWork += new System.ComponentModel.DoWorkEventHandler(worker2_DoWork);
                    worker2.RunWorkerAsync();
              
                    EncriptData();
                    LoginStatus = "";
                    if (onOpenWindowClick != null)
                    {
                        onOpenWindowClick();
                        DoOpen();
                    }                  

                    Close();

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(sendLoginMail);
                    worker.RunWorkerAsync();
                }

            }
            finally
            {
               Mouse.OverrideCursor = null;
            }
        }

        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
               
                 _SharedVMData = VMSharedData.getInstance();
               
                return _SharedVMData;
            }
        }

        ObservableCollection<User> AgentList = null;

        void worker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                SharedVMData.isAgentLoded = false;
                SharedVMData.GlobalAgentList = new ObservableCollection<User>();
                AgentList = new ObservableCollection<User>();
                //Acme - MArch 17, 2017 - Added code to track login detail
                serviceClients.UserClient.AddLoginLogoutTime(RoleManager.userCredentialID, System.Configuration.ConfigurationSettings.AppSettings["DisplayVersion"], "Login");
              //  serviceClients.UserClient.DeleteTempFolderContentsAsync();
                
                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {
                    try
                    {
                        int intCount = serviceClients.UserClient.GetAllPayeeCount();
                        int takeRecordCount = 500;
                        int asyncCallingCount = Convert.ToInt32(Math.Ceiling(intCount / Convert.ToDouble(takeRecordCount)));
                        int remainingRecords = intCount;
                        int skipRecordCount = 0;

                        for (int count = 0; count < asyncCallingCount; count++)
                        {
                            ObservableCollection<User> tempAgentList = new ObservableCollection<User>();
                            if (remainingRecords < takeRecordCount)
                            {
                                tempAgentList = serviceClients.UserClient.GetAllUsersByChunck(skipRecordCount, takeRecordCount);
                                SharedVMData.GlobalAgentList.AddRange(tempAgentList);
                            }
                            else
                            {
                                tempAgentList = serviceClients.UserClient.GetAllUsersByChunck(skipRecordCount, takeRecordCount);
                                SharedVMData.GlobalAgentList.AddRange(tempAgentList);
                            }

                            remainingRecords -= takeRecordCount;
                            skipRecordCount += takeRecordCount;

                            AgentList.AddRange(tempAgentList.Where(d => d.Role == UserRole.Agent));
                            SharedVMData.CachedAgentList.Clear();
                                                        
                            SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);

                            SharedVMData.GlobalReportAgentList = new ObservableCollection<User>(AgentList);
                        }
                    }

                    catch
                    {
                    }

                    //SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllUsers());
                }
                else
                {
                    try
                    {
                        Guid guidID = (Guid)RoleManager.LicenseeId;
                        int intCount = serviceClients.UserClient.GetPayeeCount(guidID);
                        int takeRecordCount = 100;
                        int asyncCallingCount = Convert.ToInt32(Math.Ceiling(intCount / Convert.ToDouble(takeRecordCount)));
                        int remainingRecords = intCount;
                        int skipRecordCount = 0;

                        for (int count = 0; count < asyncCallingCount; count++)
                        {
                            ObservableCollection<User> tempAgentList = new ObservableCollection<User>();

                            if (remainingRecords < takeRecordCount)
                            {
                                tempAgentList = serviceClients.UserClient.GetAllUsersByLicChunck(guidID, skipRecordCount, takeRecordCount);
                                SharedVMData.GlobalAgentList.AddRange(tempAgentList);
                            }
                            else
                            {
                                tempAgentList = serviceClients.UserClient.GetAllUsersByLicChunck(guidID, skipRecordCount, takeRecordCount);
                                SharedVMData.GlobalAgentList.AddRange(tempAgentList);
                            }
                            remainingRecords -= takeRecordCount;
                            skipRecordCount += takeRecordCount;

                            AgentList.AddRange(tempAgentList.Where(d => d.Role == UserRole.Agent));
                            SharedVMData.CachedAgentList.Clear();
                            SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);
                            
                            SharedVMData.GlobalReportAgentList = new ObservableCollection<User>(AgentList);
                        }

                        //SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetUsersByLicensee(guidID));
                        //payee
                        //AgentList = new ObservableCollection<User>(SharedVMData.GlobalAgentList.Where(d => d.Role == UserRole.Agent));
                        //SharedVMData.CachedAgentList.Add(SharedVMData.SelectedLicensee.LicenseeId, AgentList);

                    }
                    catch
                    {
                    }
                }

            }
            catch
            {
            }

            SharedVMData.GlobalReportAgentList = new ObservableCollection<User>(AgentList);
            SharedVMData.isAgentLoded = true;
            System.Threading.Tasks.Task.Factory.StartNew(() => DeleteTempFolderContents());
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

        void sendLoginMail(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                RoleManager.LoggedInUser = UserName;
                RoleManager.LicenseName = UserInfo.Company;
                if (string.IsNullOrEmpty(UserInfo.Company))
                {
                    SendMailToLoginLogout(UserName, "Access Log:" + UserName + " ", UserInfo.Company);
                }
                else
                {
                    SendMailToLoginLogout(UserName, "Access Log:" + UserName + " Of " + UserInfo.Company + " ", UserInfo.Company);
                }
            }
            catch
            {
            }
        }

        private void SendMailToLoginLogout(string strUserName, string strType, string strLicenseName)
        {
            //MyAgencyVault.EmailFax.
            MailData _MailData = new MailData();
            try
            {
                _MailData.ToMail = "service@commissionsdept.com";
                _MailData.FromMail = "service@commissionsdept.com";
                //serviceClients.FollowupIssueClient.SendLoginLogoutMail(_MailData, strType + System.DateTime.Now, MailContent(strUserName, strLicenseName));
            }
            catch
            {
            }
        }

        private string MailContent(string strUserName, string strLicenseName)
        {
            string MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                              "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>User Name: " +
                              strUserName +
                              "</td></tr><tr><td colspan='2'>Log In:" +
                              System.DateTime.Now +
                              "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                              + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>"
                              + "</td></tr></table>";

            return MailBody;

        }

        private List<UserPermissions> GetUserPermission()
        {
          using (ServiceClients serviceClients = new ServiceClients())
          {
            if (RoleManager.Role == UserRole.SuperAdmin)
              return null;
            List<UserPermissions> _userpermission = serviceClients.UserClient.GetCurrentPermission(RoleManager.userCredentialID).ToList();
            return _userpermission;
          }
        }

        /// <summary>
        /// to set login info
        /// </summary>
        private void SetLoginInfo()
        {
           UserName = this.UserInfo.UserName;
           Password = this.UserInfo.Password;
           if ((this.UserInfo.RembStatus).Equals("R True")) RembChecked = true;
           if ((this.UserInfo.SStatus).Equals("S True")) SChecked = true;
        }

        /// <summary>
        /// to read file from local disk
        /// </summary>
        /// <returns></returns>
        private bool ReadFile()
        {
            string strTest;

            try
            {

                if (File.Exists(filePath))
                {
                    FileStream strStream = new FileStream(filePath, FileMode.Open);
                    StreamReader strReader = new StreamReader(strStream);
                    UserInfo = new User();
                    while ((strTest = strReader.ReadLine()) != null)
                    {
                        strTest = EncriptClass.Decrypt(strTest);
                        loginInfoList.Add(strTest);
                    }

                    if ((loginInfoList.Count <= 0)) return false;

                    this.UserInfo.UserName = loginInfoList[0].ToString();
                    this.UserInfo.Password = loginInfoList[1].ToString();
                    this.UserInfo.RembStatus = loginInfoList[2].ToString();
                    this.UserInfo.SStatus = loginInfoList[3].ToString();

                    if ((this.UserInfo.RembStatus).Equals("R True")) return true;

                }
                return false;
            }

            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// To write file on local disk
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="rembStatus"></param>
        /// <param name="sStatus"></param>
        public void WriteFile(string userName, string passWord, string rembStatus, string sStatus)
        {
         
            FileStream strStream;
            StreamWriter strWriter;
            try
            {
                // Perform operation that uses the assert
                FileIOPermission fp = new FileIOPermission(PermissionState.Unrestricted);
                fp.Assert();
                strStream = new FileStream(filePath, FileMode.Create);
                strWriter = new StreamWriter(strStream);
                strWriter.WriteLine(userName);
                strWriter.WriteLine(passWord);
                strWriter.WriteLine(rembStatus);
                strWriter.WriteLine(sStatus);
                strWriter.Close();
                // Revert the assert when operation is completed
                CodeAccessPermission.RevertAssert();
            }

            catch 
            {
               // MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region "Properties"
        /// <summary>
        ///  To get User details
        /// </summary>
        public User UserInfo
        {
            get
            {
                return _userInfo;

            }
            set
            {
                _userInfo = value;
            }

        }

        public event Action RequestClose;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new BaseCommand(
                         param => CanClose(),
                       param => Close()
                  );
                }
                return _closeCommand;
            }
        }

        public event Action RequestOpen;

        public ICommand OpenCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _OpenCommand = new BaseCommand(
                         param => CanOpen(),
                       param => DoOpen()
                  );
                }
                return _OpenCommand;
            }
        }


        public event Action RequestOpenFPassword;

        public ICommand OpenCommandFPassword
        {
            get
            {
                if (_closeCommand == null)
                {
                    _OpenCommand = new BaseCommand(
                         param => CanOpenFPassword(),
                       param => DoOpenFPassword()
                  );
                }
                return _OpenCommand;
            }
        }


        private ICommand _LogoutClicked;
        public ICommand LogoutClicked
        {
            get
            {
                if (_LogoutClicked == null)
                {
                    _LogoutClicked = new BaseCommand(param => SetLogoutClicked());
                }
                return _LogoutClicked;
            }
        }

        private bool LogoutBtnClicked = false;
        private void SetLogoutClicked()
        {
            LogoutBtnClicked = true;
        }

        /// <summary>
        ///  To get/set EMail
        /// </summary>
        public String EMail
        {
            get
            {
                return _eMail;

            }
            set
            {
                _eMail = value;
            }

        }

        /// <summary>
        /// To get/set Remember password
        /// </summary>
        public bool RembChecked
        {
            get
            {
                return _rChecked;

            }
            set
            {
                _rChecked = value;
                OnPropertyChanged("RembChecked");
            }

        }

        /// <summary>
        /// To get/set Sign in automatically 
        /// </summary>
        public bool SChecked
        {
            get
            {
                //if (LogoutBtnClicked)
                //{
                //    LogoutBtnClicked = false;
                //    return LogoutBtnClicked;
                //}
                //else
                    return _sChecked;

            }
            set
            {
                _sChecked = value;
                OnPropertyChanged("SChecked");
            }

        }

        /// <summary>
        /// To get/set PHintQ 
        /// </summary>
        public String PHintQ
        {
            get
            {
                return _pHintQ;

            }
            set
            {
                _pHintQ = value;
                OnPropertyChanged("PHintQ");
            }

        }

        /// <summary>
        /// To get/set PHintA 
        /// </summary>
        public String PHintA
        {
            get
            {
                return _pHintA;

            }
            set
            {
                _pHintA = value;
                OnPropertyChanged("PHintA");
            }

        }

        /// <summary>
        /// To get/set UserName 
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;

            }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }

        }

        /// <summary>
        /// To get/set Password 
        /// </summary>
        public string Password
        {
            get
            {
                return _passWord;
            }

            set
            {
                _passWord = value;
                OnPropertyChanged("Password");
            }

        }

        /// <summary>
        /// To get/set Password 
        /// </summary>
        public MyAgencyVault.VM.MyAgencyVaultSvc.UserRole UserRole
        {
            get
            {
                return _UserRole;
            }

            set
            {
                _UserRole = value;
                // OnPropertyChanged("Password");
            }

        }




        /// <summary>
        /// to set login status
        /// </summary>
        public string LoginStatus
        {

            get
            {
                return _loginStatus;
            }

            set
            {
                _loginStatus = value;
                OnPropertyChanged("LoginStatus");
            }
        }

        /// <summary>
        /// To do processes of forgot password
        /// </summary>
        public string ForgotPassStatus
        {

            get
            {
                return _forgotPassStatus;
            }

            set
            {
                _forgotPassStatus = value;
                OnPropertyChanged("ForgotPassStatus");
            }
        }

        #endregion


        long getDirSize(DirectoryInfo d)
        {
            long size = 0;
            try
            {
                size = d.EnumerateFiles().Sum(file => file.Length);
                size += d.EnumerateDirectories().Sum(dir => getDirSize(dir));
            //    ActionLogger.Logger.WriteLog("getTempDirSize  " + size.ToString(), true);
            }
            catch (Exception ex)
            {
              //  ActionLogger.Logger.WriteLog("getTempDirSize exception: " + ex.Message, true);
            }
            return size;
        }

        public void DeleteTempFolderContents()
        {
            try
            {
    /*            MastersClient m = new MastersClient();
                TempFolderDetails t = m.GetTempFolderSetting();

                if (!t.AllowDelete)
                {
                    return;
                }

                string strFileLimit = t.FileSizeToBeDeleted;
                int allowedSize = 0;
                Int32.TryParse(strFileLimit, out allowedSize);

                if (allowedSize > 0)
                {
                    string tempPath = System.IO.Path.GetTempPath();
                    DirectoryInfo d = new DirectoryInfo(tempPath);
                    long size = getDirSize(d) / 1024/1024/1024; // from bytes to GB

                    if (size > allowedSize)
                    {
                         long sizeinGB = 0;
                         while (sizeinGB < allowedSize) //this is to delete 3GB data at a time
                         {
                             foreach (FileInfo fi in d.GetFiles())
                             {
                                 try
                                 {
                                     sizeinGB += ((fi.Length / 1024) / 1024) / 1024;
                                     fi.Delete();
                                 }
                                 catch (Exception ex)
                                 {
                                 }
                             }
                             foreach (DirectoryInfo dir in d.GetDirectories())
                             {
                                 try
                                 {
                                     sizeinGB += ((getDirSize(dir) / 1024) / 1024) / 1024;
                                     dir.Delete(true);
                                 }
                                 catch (Exception ex)
                                 {
                                 }
                             }
                         }
                    }
                }*/
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("DeleteTempFolderContents exception: " + ex.Message, true);
            }
        }

    }
}
