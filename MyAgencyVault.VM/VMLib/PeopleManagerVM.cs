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
using System.Text.RegularExpressions;
using MyAgencyVault.VM.VMLib;
using System.Windows;
using System.Transactions;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.SortableCollection;
using System.Data.OleDb;
namespace MyAgencyVault.ViewModel
{
    public class PeopleManagerVM : BaseViewModel, IDataRefresh
    {

        #region Member Fields

        private BaseCommand m_LoadCommand;
        private BaseCommand i_LoadCommand;
        private BaseCommand N_LoadCommand;
        private BaseCommand U_LoadCommand;
        private ICommand _Delete;

        private ICollectionView usersview;
        private ICollectionView _UserTypeListview;

        private ObservableCollection<UsersRole> _UserList = new ObservableCollection<UsersRole>();
        private ObservableCollection<LinkedUser> _ChangedUserList = new ObservableCollection<LinkedUser>();
        static MastersClient objLog = new MastersClient();
        private ObservableCollection<Question> _lstQuestions;
        private ObservableCollection<User> AllUsers;
        private ObservableCollection<User> TempAllUsers;
        private IView _ViewValidation;

        #endregion
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

        # region Constructor

        public PeopleManagerVM(IView viewValidation)
        {
            _ViewValidation = viewValidation;
            PropertyChanged += new PropertyChangedEventHandler(PeopleManagerVM_PropertyChanged);

            FillUsers();

            _UserTypeListview = CollectionViewSource.GetDefaultView(_UserList);
            _UserTypeListview.CurrentChanged += new EventHandler(_UserTypeListview_CurrentChanged);
            IsEnabledAlloption = false;
            if (RoleManager.Role == UserRole.SuperAdmin)
            {
                IsEnabledAlloption = true;
                //AllUsers = serviceClients.UserClient.GetAllUsers();
                if (SharedVMData != null)
                {
                    if (SharedVMData.GlobalAgentList != null)
                    {
                        if (SharedVMData.GlobalAgentList.Count > 0)
                        {
                            AllUsers = SharedVMData.GlobalAgentList;  
                        }
                        else
                        {
                           // AllUsers = serviceClients.UserClient.GetAllUsers();
                        }
                    }
                    else
                    {
                        //AllUsers = serviceClients.UserClient.GetAllUsers();
                    }
                }
                else
                {
                    //AllUsers = serviceClients.UserClient.GetAllUsers();
                }

            }
            else
            {
                //AllUsers = serviceClients.UserClient.GetUsersByLicensee(RoleManager.LicenseeId.Value);

                if (SharedVMData != null)
                {
                    if (SharedVMData.GlobalAgentList != null)
                    {
                        if (SharedVMData.GlobalAgentList.Count > 0)
                        {
                            AllUsers = SharedVMData.GlobalAgentList;
                           
                        }
                        else
                        {
                            //AllUsers = serviceClients.UserClient.GetAllPayeeByLicencessID(RoleManager.LicenseeId.Value);
                        }
                    }
                    else
                    {
                        //AllUsers = serviceClients.UserClient.GetAllPayeeByLicencessID(RoleManager.LicenseeId.Value);
                    }
                }
                else
                {
                    //AllUsers = serviceClients.UserClient.GetAllPayeeByLicencessID(RoleManager.LicenseeId.Value);
                }

            }

            if (Currentuser != null)
            {
                if ((RoleManager.Role == UserRole.SuperAdmin) || (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount) || (RoleManager.Role == UserRole.Administrator))
                {
                    FirstYearPercentageEnable = RenewalYearPercentageEnable = true;
                    IsEnableAccountDefaults = true;
                }
                else
                {
                    FirstYearPercentageEnable = RenewalYearPercentageEnable = false;
                    IsEnableAccountDefaults = false;
                }
            } 
          
            PeopleScreenControl(); 
            PopulateQuestionList();
        }


        private bool _IsAgentDiable;
        public bool IsAgentDiable
        {
            get { return _IsAgentDiable; }
            set
            {
                _IsAgentDiable = value;
                OnPropertyChanged("IsAgentDiable");
            }
        }

        private bool _IsAccountExec;
        public bool IsAccountExec
        {
            get { return _IsAccountExec; }
            set
            {
                _IsAccountExec = value;
                OnPropertyChanged("IsAccountExec");
            }
        }


        private bool _FirstYearPercentageEnable;
        public bool FirstYearPercentageEnable
        {
            get { return _FirstYearPercentageEnable; }
            set { _FirstYearPercentageEnable = value; OnPropertyChanged("FirstYearPercentageEnable"); }
        }



        private bool _RenewalYearPercentageEnable;
        public bool RenewalYearPercentageEnable
        {
            get { return _RenewalYearPercentageEnable; }
            set { _RenewalYearPercentageEnable = value; OnPropertyChanged("RenewalYearPercentageEnable"); }
        }

        private bool _IsEnableAccountDefaults;
        public bool IsEnableAccountDefaults
        {
            get { return _IsEnableAccountDefaults; }
            set { _IsEnableAccountDefaults = value; OnPropertyChanged("IsEnableAccountDefaults"); }
        }

        void PeopleManagerVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Currentuser":
                    {
                        try
                        {
                            if (_CurrentUser == null)
                                return;

                            IsNotHouseOwner = (Currentuser.Role == UserRole.Administrator || Currentuser.IsHouseAccount == true) ? false : true;
                           
                            UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
                            IsAdministrator = role == (UserRole)2 ? true : false;
                            
                            if (_CurrentUser.DisableAgentEditing != null)
                            {
                                IsAgentDiable = Convert.ToBoolean(_CurrentUser.DisableAgentEditing);
                            }
                            else
                            {
                                IsAgentDiable = false;
                            }

                            if (_CurrentUser.IsAccountExec == null)
                            {
                                IsAccountExec = false;
                            }
                            else if (_CurrentUser.IsAccountExec == false)
                            {
                                IsAccountExec = false;
                            }
                            else
                            {
                                IsAccountExec = true;
                            }

                            Currentuser.LinkedUsers = new ObservableCollection<LinkedUser>();
                            Guid SelectedLic = SharedVMData.SelectedLicensee.LicenseeId;
                            //this.Role == UserRole.Agent && this.IsHouseAccount == false
                            TempAllUsers = new ObservableCollection<User>(AllUsers.Where(s => s.LicenseeId == SelectedLic /*&& s.IsHouseAccount == false*/).ToList());

                            if (TempAllUsers != null)
                            {
                                if (TempAllUsers.Count > 0)
                                {
                                    foreach (var item in TempAllUsers)
                                    {
                                        if (_CurrentUser.UserCredentialID != item.UserCredentialID)
                                        {
                                            if (_CurrentUser.NickName != item.NickName)
                                            {
                                                LinkedUser objLinkedUser = new LinkedUser();
                                                objLinkedUser.UserId = item.UserCredentialID;
                                                objLinkedUser.FirstName = item.FirstName;
                                                objLinkedUser.LastName = item.LastName;
                                                objLinkedUser.NickName = item.NickName;
                                                objLinkedUser.UserName = item.UserName;
                                               
                                                Currentuser.LinkedUsers.Add(objLinkedUser);

                                            }
                                        }
                                    }
                                }
                            }

                            if (_CurrentUser.PasswordHintQ != null && HntQuestions != null)
                            {
                                SelectedQuestion = (from p in HntQuestions where p.QuestionText == _CurrentUser.PasswordHintQ select p).FirstOrDefault();
                                selectedIndex = (from l in HntQuestions where l.QuestionText == _CurrentUser.PasswordHintQ select l.QuestionId).FirstOrDefault();
                            }
                            else
                            {
                                if (HntQuestions != null)
                                {
                                    SelectedQuestion = HntQuestions.FirstOrDefault();
                                    selectedIndex = SelectedQuestion.QuestionId;
                                }
                            }

                            //List<LinkedUser> objAllLinkedList = new List<LinkedUser>();
                            //objAllLinkedList = serviceClients.UserClient.GetAllLinkedUser(AllUsers, (Guid)_CurrentUser.LicenseeId, (Guid)_CurrentUser.UserCredentialID, 3, false).ToList();
                            //_CurrentUser.LinkedUsers = new ObservableCollection<LinkedUser>(objAllLinkedList);

                            List<LinkedUser> objList = new List<LinkedUser>();
                            objList = serviceClients.UserClient.GetLinkedUser(_CurrentUser.UserCredentialID, _CurrentUser.Role, _CurrentUser.IsHouseAccount).ToList();

                            if (objList.Count > 0)
                            {
                                if (_CurrentUser.LinkedUsers != null)
                                {
                                    foreach (var item in _CurrentUser.LinkedUsers)
                                    {
                                        foreach (var item1 in objList)
                                        {
                                            if (item.UserId == item1.UserId)
                                            {
                                                item.IsConnected = true;
                                                break;
                                            }
                                            else
                                            {
                                                item.IsConnected = false;
                                            }

                                        }
                                    }
                                }

                            }
                            else
                            {
                                if (_CurrentUser.LinkedUsers != null)
                                {
                                    foreach (var item in _CurrentUser.LinkedUsers)
                                    {
                                        item.IsConnected = false;
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteLog("Exception Occurs in ONPropertyChange:" + ex.Message, true);
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        public void SelectedLicenseeChanged()
        {
            if (SharedVMData.SelectedLicensee == null)
                return;           
            try
            {
                if (SharedVMData.GlobalAgentList != null)
                {
                    if (SharedVMData.GlobalAgentList.Count > 0)
                    {
                        AllUsers = SharedVMData.GlobalAgentList;
                    }
                }

            }
            catch(Exception)
            {
                //string strValue = ex.ToString();
                //MessageBox.Show(ex.ToString());
            } 
            FillList();
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

        #region ControlLevelProerty
        #region PeopleScreen

        private void PeopleScreenControl()
        {
            if (RoleManager.Role == UserRole.Agent)
            {
                LicencesDetailComboEnable = false;
                if (RoleManager.UserPermissions[(int)MasterModule.PeopleManager - 1].Permission == ModuleAccessRight.Read)
                    LicencesDetailComboEnable = false;
            }
            else if (RoleManager.Role == UserRole.Administrator)
            {
                LicencesDetailComboEnable = false;
                if (RoleManager.UserPermissions[(int)MasterModule.PeopleManager - 1].Permission == ModuleAccessRight.Read)
                    LicencesDetailComboEnable = false;
            }
            else if (RoleManager.Role == UserRole.DEP)
                LicencesDetailComboEnable = false;
            else if (RoleManager.Role == UserRole.HO)
                LicencesDetailComboEnable = false;
            else if (RoleManager.Role == UserRole.SuperAdmin)
                LicencesDetailComboEnable = true;

        }
        private bool licencessdetailcomboenable = false;

        public bool LicencesDetailComboEnable
        {
            get { return licencessdetailcomboenable; }
            set
            {
                licencessdetailcomboenable = value;
                OnPropertyChanged("LicencesDetailComboEnable");
            }
        }

        #endregion
        #endregion
        private void PopulateQuestionList()
        {
            HntQuestions = new ObservableCollection<Question>();
            Question obgQuestion1 = new Question();
            obgQuestion1.QuestionId = 1;
            obgQuestion1.QuestionText = "First school";
            HntQuestions.Add(obgQuestion1);

            Question obgQuestion2 = new Question();
            obgQuestion2.QuestionId = 2;
            obgQuestion2.QuestionText = "First Teacher";
            HntQuestions.Add(obgQuestion2);

            Question obgQuestion3 = new Question();
            obgQuestion3.QuestionId = 3;
            obgQuestion3.QuestionText = "First company";
            HntQuestions.Add(obgQuestion3);

            //HntQuestions = serviceClients.MasterClient.GetQuestions();
        }

        private void FillUsers()
        {
            if (RoleManager.Role == VM.MyAgencyVaultSvc.UserRole.SuperAdmin || RoleManager.Role == VM.MyAgencyVaultSvc.UserRole.Administrator)
            {
                _UserList.Add(new UsersRole() { Name = "Agent", RoleID = 3 });
                _UserList.Add(new UsersRole() { Name = "Administrator", RoleID = 2 });
            }
            else
            {
                _UserList.Add(new UsersRole() { Name = "Agent", RoleID = 3 });
            }
        }

        void _UserTypeListview_CurrentChanged(object sender, EventArgs e)
        {
            FillList();
            selectedIndex = 0;
        }

        public void FillList()
        {
            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return;

            UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
           LicenseeUsers = GetUsersBySelectedLicensee(role, SharedVMData.SelectedLicensee.LicenseeId);

            if (LicenseeUsers.Count != 0)
            {
                Currentuser = LicenseeUsers[0];
                if (Currentuser.IsAccountExec == null)
                {
                    IsAccountExec = false;
                }
                else if (Currentuser.IsAccountExec == false)
                {
                    IsAccountExec = false;
                }
                else
                {
                    IsAccountExec = (bool)Currentuser.IsAccountExec;
                }
            }
        }

        #endregion
        #region User Details
        #region Properties

        public ICommand saveRecords
        {
            get
            {
                if (m_LoadCommand == null)
                {
                    m_LoadCommand = new BaseCommand(param => BeforeOnSaveAgent(), param => OnSaveAgent());
                }
                return m_LoadCommand;
            }
        }

        public ICommand ImportRecords
        {
            get
            {
                if (i_LoadCommand == null)
                {
                    i_LoadCommand = new BaseCommand(param => BeforeImportRecords(), param => OnImportRecords());
                }
                return i_LoadCommand;
            }
        }



        public ICommand NewRecord
        {
            get
            {
                if (N_LoadCommand == null)
                {
                    N_LoadCommand = new BaseCommand(param => BeforeOnNewAgent(), param => OnNewAgent());
                }
                return N_LoadCommand;
            }
        }


        public ICommand UpdateAgents
        {
            get
            {
                if (U_LoadCommand == null)
                {
                    U_LoadCommand = new BaseCommand(param => BeforeOnUpdateAgentData(), pram => OnUpdateAgentData());
                }
                return U_LoadCommand;
            }

        }

        public ICommand DeleteUser
        {
            get
            {
                if (_Delete == null)
                    _Delete = new BaseCommand(param => BeforeOnUserDelete(), param => OnUserDelete());
                return _Delete;
            }
        }


        private ICommand _ZipLostFocus;
        public ICommand ZipLostFocus
        {
            get
            {
                if (_ZipLostFocus == null)
                    _ZipLostFocus = new BaseCommand(param => OnZipLostFocus());
                return _ZipLostFocus;
            }
        }

        private ICommand _UserNameLostFocus;
        public ICommand UserNameLostFocus
        {
            get
            {
                if (_UserNameLostFocus == null)
                    _UserNameLostFocus = new BaseCommand(param => OnUserNameLostFocus());
                return _UserNameLostFocus;
            }
        }

        private void OnUserNameLostFocus()
        {
            //if (Currentuser != null && string.IsNullOrEmpty(Currentuser.NickName))
            if (Currentuser != null)
            {
                if (Currentuser.UserName != null)
                {
                    Currentuser.UserName = Currentuser.UserName.Trim();
                    Currentuser.NickName = Currentuser.UserName;
                }
            }
        }

        private ICommand _HouseAccountTransferred;
        public ICommand HouseAccountTransferred
        {
            get
            {
                if (_HouseAccountTransferred == null)
                    _HouseAccountTransferred = new BaseCommand(param => OnHouseAccountTransferred());
                return _HouseAccountTransferred;
            }
        }

        private void OnHouseAccountTransferred()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to transfer house account to another agent?", "House Account Transfer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var options = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(15)
                };
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    try
                    {
                        serviceClients.UserClient.HouseAccoutTransferProcess(Currentuser);
                        ts.Complete();
                        User user = LicenseeUsers.FirstOrDefault(s => s.IsHouseAccount == true && s.UserCredentialID != Currentuser.UserCredentialID);
                        if(user != null)
                        {
                            LicenseeUsers.First(s => s.IsHouseAccount == true && s.UserCredentialID != Currentuser.UserCredentialID).IsHouseAccount = false;
                        }
                        IsNotHouseOwner = false;
                        MessageBox.Show("House Account is successfully transferred.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        Currentuser.IsHouseAccount = false;
                        IsNotHouseOwner = true;
                        if (objLog != null)
                        {
                            objLog.AddLog("Exception OnHouseAccountTransferred:UserID: " + ", Messgae: "  + ex.Message);
                        }
                        MessageBox.Show("House Account transfer is failed. Try again.", "Fail", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            else
            {
                //change by vinod
                //if  transfer house account to another agent is Set to "No" then set ishouseAccount is false and ischecked is false
                Currentuser.IsHouseAccount = false;
                Currentuser.IsChecked = false;
            }
        }

        private void OnZipLostFocus()
        {
            if (Currentuser.ZipCode != null && Currentuser.ZipCode.ToString().Length == 5)
            {
                Zip zipData = serviceClients.MasterClient.GetZip(Currentuser.ZipCode);
                if (zipData != null)
                {
                    Currentuser.City = zipData.City;
                    Currentuser.State = zipData.State;
                }
            }
        }

        public ObservableCollection<Question> HntQuestions
        {
            get
            {
                return _lstQuestions;
            }
            set
            {
                _lstQuestions = value;
                OnPropertyChanged("HntQuestions");
            }
        }

        public ObservableCollection<UsersRole> userType
        {
            get
            {
                return _UserList;
            }
        }

        public int selectedIndex
        {
            get;
            set;
        }

        private bool _setfocus = true;
        public bool setFocus
        {
            get
            {
                return _setfocus;
            }
            set
            {
                _setfocus = value;
                OnPropertyChanged("setFocus");
            }
        }

        private string _isVisible = "Hidden";
        public string IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private SortableObservableCollection<User> GetUsersBySelectedLicensee(UserRole role, Guid LicenseeId)
        {

            // Ankit-User this code for getting the list  of administartor
            //if (role == (UserRole)2)
            //{
            //    IEnumerable<User> getList = new List<User>();
            //    getList = serviceClients.UserClient.UsersWithLicenseeId(LicenseeId, role);
            //    return new SortableObservableCollection<User>((getList.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && s.Role == role).OrderBy(s => s.NickName).ToList()));

            //}
            //else
            //{
            if (AllUsers == null || AllUsers.Count == 0)
                    return null;

                return new SortableObservableCollection<User>((AllUsers.Where(s => s.LicenseeId == SharedVMData.SelectedLicensee.LicenseeId && s.Role == role).OrderBy(s => s.NickName).ToList()));
           // }  
           
        }


        private SortableObservableCollection<User> _LicenseeUsers;
        public SortableObservableCollection<User> LicenseeUsers
        {
            get
            {
                if (_LicenseeUsers == null)
                    _LicenseeUsers = new SortableObservableCollection<User>();

                if (_LicenseeUsers.SortDescriptions.Count == 0)
                {
                    _LicenseeUsers.SortDescriptions.Add(new VM.SortableCollection.SortDescription { PropertyName = "NickName", Direction = SortDirection.Ascending });
                }

                return _LicenseeUsers;
            }
            set
            {

                _LicenseeUsers = value;
                OnPropertyChanged("LicenseeUsers");

            }
        }

        private bool _IsNotHouseOwner;
        public bool IsNotHouseOwner
        {
            get { return _IsNotHouseOwner; }
            set
            {
                _IsNotHouseOwner = value;
                OnPropertyChanged("IsNotHouseOwner");
            }
        }
        private bool _IsAdministrator;
        public bool IsAdministrator
        {
            get { return _IsAdministrator; }
            set
            {
                _IsAdministrator = value;
                OnPropertyChanged("IsAdministrator");
            }
        }

        private User _CurrentUser;
        public User Currentuser
        {
            get
            {
                return _CurrentUser == null ? new User() : _CurrentUser;
            }
            set
            {
                _CurrentUser = value;
                OnPropertyChanged("Currentuser");
            }
        }

        private Question _question;
        public Question SelectedQuestion
        {
            get
            {
                return _question;
            }
            set
            {
                _question = value;
                if (_question != null)
                    Currentuser.PasswordHintQ = _question.QuestionText;
                OnPropertyChanged("SelectedQuestion");
            }
        }

        #endregion
        #region Private Functions

        private void PopulateLinkedAgents()
        {
            if (Currentuser.LinkedUsers == null)
                Currentuser.LinkedUsers = new ObservableCollection<LinkedUser>();
            try
            {

                foreach (User user in LicenseeUsers)
                {
                    LinkedUser linkedUser = new LinkedUser();
                    linkedUser.Copy(user);
                    Currentuser.LinkedUsers.Add(linkedUser);
                }
            }
            catch (Exception)
            {
            }
        }

        private void PopulateDefaultPermissionForUser()
        {
            if (Currentuser.Permissions == null)
                Currentuser.Permissions = new ObservableCollection<UserPermissions>();
            try
            {
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PeopleManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PolicyManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.Settings, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.FollowUpManger, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.HelpUpdate, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.CompManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.ReportManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });


                #region Eric requirement
                //Change by vinod 
                //And please make a pop up when the agent is first created saying "Please make sure you update the default permissions on the settings tab for this agent."
                //No access to all except..
                //Policy manager.. Read
                //Help... Read
                #endregion


                // Ankit-User this code for default set a permissions for administartor
                //if (Currentuser.Role== (UserRole)2)
                //{
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PeopleManager, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PolicyManager, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.Settings, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.FollowUpManger, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.HelpUpdate, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.CompManager, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //    Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.ReportManager, Permission = ModuleAccessRight.Write, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //}
                //else
                //{

                //}
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PeopleManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.PolicyManager, Permission = ModuleAccessRight.Read, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.Settings, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.FollowUpManger, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.HelpUpdate, Permission = ModuleAccessRight.Read, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.CompManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.ReportManager, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });
                //Dashboard permission for web
                Currentuser.Permissions.Add(new UserPermissions { Module = MasterModule.Dashboard, Permission = ModuleAccessRight.NoAccess, UserID = Currentuser.UserCredentialID, UserPermissionId = Guid.NewGuid() });

            }
            catch
            {
            }
        }

        private bool BeforeOnUserDelete()
        {
            UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
            if (role == UserRole.Administrator)
                return false;

            //if (RoleManager.UserAccessPermission(MasterModule.PeopleManager) == ModuleAccessRight.Read)
            //    return false;

            if (Currentuser.IsHouseAccount == true)
                return false;

            if (SharedVMData.SelectedLicensee == null)
                return false;

            //if (Currentuser.FirstName == null)
            //    return false;

            return true;
        }

        private void OnUserDelete()
        {
            if (Currentuser != null)
            {
                if (Currentuser.UserCredentialID == null || _LicenseeUsers == null)
                    return;

                MessageBoxResult result = MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if(objLog != null)
                    {
                        objLog.AddLog(DateTime.Now.ToString() +  " Delete User request: " + Currentuser.UserCredentialID);
                    }
                    UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
                    Currentuser.Role = role;

                    if (serviceClients.UserClient.DeleteUserInfo(Currentuser) == true)
                    {
                        foreach (User user in LicenseeUsers)
                        {
                            if (user.LinkedUsers != null && user.UserCredentialID != Currentuser.UserCredentialID)
                                user.LinkedUsers.Remove(user.LinkedUsers.FirstOrDefault(s => s.UserId == Currentuser.UserCredentialID));
                        }
                        //remove new user
                        SharedVMData.GlobalAgentList.Remove(Currentuser);
                        //Jyotisna 30 Aug, 2018 - This statement moved from before for loop to here, as it automatically deleted user from global list
                        LicenseeUsers.Remove(LicenseeUsers.FirstOrDefault(s => s.UserCredentialID == Currentuser.UserCredentialID));
                        if (objLog != null)
                        {
                            objLog.AddLog(DateTime.Now.ToString() + " Delete User request success: " + Currentuser.UserCredentialID);
                        }
                    }
                    else
                    {
                        if (objLog != null)
                        {
                            objLog.AddLog(DateTime.Now.ToString() + " Delete User failed, message shown: " + Currentuser.UserCredentialID);
                        }
                        MessageBox.Show("User can not be deleted as he has either associated outgoing payments or schedules or policies created by him in the system.");
                    }
                }
            }
        }

        private bool BeforeOnNewAgent()
        {
            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return false;

            if (RoleManager.UserAccessPermission(MasterModule.PeopleManager) == ModuleAccessRight.Read)
                return false;

            UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
            //if (role == UserRole.Administrator)
            //    return true;

            return true;
        }

        bool isShowpopUp = false;

        private void OnNewAgent()
        {
            try
            {
                Currentuser = new User();
                Currentuser.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                Currentuser.Role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
                Currentuser.UserCredentialID = Guid.NewGuid();
                PopulateDefaultPermissionForUser();
                PopulateLinkedAgents();
                setFocus = true;
                isShowpopUp = true;               
                SharedVMData.GlobalAgentList.Add(Currentuser);

            }
            catch
            {
            }

        }

        private bool BeforeImportRecords()
        {
            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return false;

            if (RoleManager.Role == UserRole.DEP)
                return false;
          
            if (RoleManager.UserAccessPermission(MasterModule.PeopleManager) == ModuleAccessRight.Read)
                return false;
           

            return true;
        }
       
        public System.Data.DataTable TempTable = null;

        public delegate void delgateOpenImportPolicy();
        public event delgateOpenImportPolicy OpenImportPolicy;
        public delegate void delgateCloseImportPolicy();

        private void OnImportRecords()
        {
            #region"New to select filter type"
                Microsoft.Win32.OpenFileDialog objOpenFileDialog = new Microsoft.Win32.OpenFileDialog();
                objOpenFileDialog.DefaultExt = ".xlsx";
                objOpenFileDialog.Filter = "Excel sheet (.xlsx)|*.xlsx| All files (*)|*.*";

                if (objOpenFileDialog.ShowDialog() == true)
                {
                    TempTable = ConvretExcelToDataTable(objOpenFileDialog.FileName);

                    if (OpenImportPolicy != null)
                    {
                        OpenImportPolicy();
                    }
                }
                #endregion
            
        }

        public System.Data.DataTable ConvretExcelToDataTable(string FilePath)
        {
            string strConn = string.Empty;
            System.Data.DataTable dt = null;

            if (FilePath.Trim().EndsWith(".xlsx"))
            {

                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", FilePath);
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter da = null;
                string pathOnly = System.IO.Path.GetDirectoryName(FilePath);
                string fileName = System.IO.Path.GetFileName(FilePath);
                dt = new System.Data.DataTable("Temp");
                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    string strSheetName = getSheetName(conn, fileName);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                    cmd = new OleDbCommand(strQuery, conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
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

                strConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=" + header + ";IMEX=1\";", FilePath);
                //strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";Extended Properties=\"Excel 8.0;HDR=" + header + "\"";
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                OleDbDataAdapter adapter = null;
                dt = new System.Data.DataTable(fileName);

                try
                {
                    conn = new OleDbConnection(strConn);
                    conn.Open();
                    string strSheetName = getSheetName(conn, fileName);
                    string strQuery = "SELECT * FROM " + "[" + strSheetName + "]";
                    cmd = new OleDbCommand(strQuery, conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    adapter = new OleDbDataAdapter(cmd);
                    adapter.Fill(dt);

                    //ActionLogger.Logger.WriteImportLog("Data set created", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                        conn.Close();
                    conn.Dispose();
                    cmd.Dispose();
                    adapter.Dispose();
                }

            }

            return dt;

        }
        //Function to get sheet name from excel files 
        private string getSheetName(OleDbConnection ObjConn, string fileName)
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
            catch (Exception)
            {
            }

            return strSheetName;
        }

        public void ImportAgent(System.Data.DataTable TempTable)
        {
            int iCount = 0;
            try
            {
                string UserName = string.Empty;
                string Password = string.Empty;
                string Role = string.Empty;
                string PasswordHintQuestion = string.Empty;
                string PasswordHintAnswer = string.Empty;
                string IsHouseAccount = string.Empty;
                string IsNewsToFlash = string.Empty;
                string IsAccountExec = string.Empty;
                string FirstName = string.Empty;
                string LastName = string.Empty;
                string Company = string.Empty;
                string NickName = string.Empty;
                string Address = string.Empty;
                string ZipCode = string.Empty;
                string City = string.Empty;
                string State = string.Empty;
                string Email = string.Empty;
                string OfficePhone = string.Empty;
                string CellPhone = string.Empty;
                string Fax = string.Empty;
                string FirstYearDefault = string.Empty;
                string RenewalDefault = string.Empty;
                string ReportForEntireAgency = string.Empty;
                string ReportForOwnBusiness = string.Empty;
                string AddPayeeOn = string.Empty;
                string DisableAgentEditing = string.Empty;
                string Moduletodisplay = string.Empty;
                string AccessRight = string.Empty;

                if (TempTable != null)
                {
                    int intColIndex = TempTable.Columns.Count - 1;

                    for (int i = 0; i < TempTable.Rows.Count; i++)
                    {
                        Currentuser = new User();
                        if (intColIndex >= 0)
                        {
                            try
                            {
                                UserName = Convert.ToString(TempTable.Rows[i][0]);
                                Currentuser.UserName = UserName;
                                string strUserName = string.Empty;
                                if (!string.IsNullOrEmpty(UserName))
                                {
                                    if (UserName.Length > 48)
                                    {
                                        strUserName = UserName.Substring(0, 48);
                                        Currentuser.UserName = strUserName;
                                    }


                                }
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 1)
                        {
                            try
                            {
                                Password = Convert.ToString(TempTable.Rows[i][1]);
                                Currentuser.Password = Password;
                                Currentuser.ConfirmPassword = Password;

                                string strPassword = string.Empty;
                                if (!string.IsNullOrEmpty(Password))
                                {
                                    if (Password.Length > 48)
                                    {
                                        strPassword = Password.Substring(0, 48);
                                        Currentuser.NickName = strPassword;
                                        Currentuser.ConfirmPassword = strPassword;
                                    }


                                }

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 2)
                        {
                            try
                            {
                                Role = Convert.ToString(TempTable.Rows[i][2]);
                                Currentuser.Role = UserRole.Agent;

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 3)
                        {
                            try
                            {
                                PasswordHintQuestion = Convert.ToString(TempTable.Rows[i][3]);
                                Currentuser.PasswordHintQ = PasswordHintQuestion;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 4)
                        {
                            try
                            {
                                PasswordHintAnswer = Convert.ToString(TempTable.Rows[i][4]);
                                Currentuser.PasswordHintA = PasswordHintAnswer;

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 5)
                        {
                            try
                            {
                                IsHouseAccount = Convert.ToString(TempTable.Rows[i][5]);
                                Currentuser.IsHouseAccount = false;

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 6)
                        {
                            try
                            {
                                IsNewsToFlash = Convert.ToString(TempTable.Rows[i][6]);
                                Currentuser.IsNewsToFlash = false;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 7)
                        {
                            try
                            {
                                IsAccountExec = Convert.ToString(TempTable.Rows[i][7]);
                                Currentuser.IsAccountExec = false;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 8)
                        {
                            try
                            {
                                FirstName = Convert.ToString(TempTable.Rows[i][8]);
                                Currentuser.FirstName = FirstName;

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 9)
                        {
                            try
                            {
                                LastName = Convert.ToString(TempTable.Rows[i][9]);
                                Currentuser.LastName = LastName;
                                string strLastName = string.Empty;
                                if (!string.IsNullOrEmpty(LastName))
                                {
                                    if (LastName.Length > 48)
                                    {
                                        strLastName = LastName.Substring(0, 48);
                                        Currentuser.LastName = strLastName;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 10)
                        {
                            try
                            {
                                Company = Convert.ToString(TempTable.Rows[i][10]);
                                Currentuser.Company = Company;
                                string strCompany = string.Empty;
                                if (!string.IsNullOrEmpty(Company))
                                {
                                    if (Company.Length > 48)
                                    {
                                        strCompany = Company.Substring(0, 48);
                                        Currentuser.Company = strCompany;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 11)
                        {
                            try
                            {
                                NickName = Convert.ToString(TempTable.Rows[i][11]);
                                string strNick = string.Empty;
                                Currentuser.NickName = NickName;
                                if (!string.IsNullOrEmpty(NickName))
                                {
                                    if (NickName.Length > 48)
                                    {
                                        strNick = NickName.Substring(0, 48);
                                        Currentuser.NickName = strNick;
                                    }                                    
                                }

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 12)
                        {
                            try
                            {
                                Address = Convert.ToString(TempTable.Rows[i][12]);
                                Currentuser.Address = Address;
                                string strAddress = string.Empty;
                                if (!string.IsNullOrEmpty(Address))
                                {
                                    if (Address.Length > 48)
                                    {
                                        strAddress = Address.Substring(0, 48);
                                        Currentuser.Address = strAddress;
                                    }                                   
                                }

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 13)
                        {
                            try
                            {
                                ZipCode = Convert.ToString(TempTable.Rows[i][13]);
                                Currentuser.ZipCode = ZipCode;

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 14)
                        {
                            try
                            {
                                City = Convert.ToString(TempTable.Rows[i][14]);
                                Currentuser.City = City;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 15)
                        {
                            try
                            {
                                State = Convert.ToString(TempTable.Rows[i][15]);
                                Currentuser.State = State;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 16)
                        {
                            try
                            {
                                Email = Convert.ToString(TempTable.Rows[i][16]);
                                Currentuser.Email = Email;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 17)
                        {
                            try
                            {
                                OfficePhone = Convert.ToString(TempTable.Rows[i][17]);
                                Currentuser.OfficePhone = OfficePhone;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 18)
                        {
                            try
                            {
                                CellPhone = Convert.ToString(TempTable.Rows[i][18]);
                                Currentuser.CellPhone = CellPhone;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 19)
                        {
                            try
                            {
                                Fax = Convert.ToString(TempTable.Rows[i][19]);
                                Currentuser.Fax = Fax;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 20)
                        {
                            try
                            {
                                FirstYearDefault = Convert.ToString(TempTable.Rows[i][20]);
                                Currentuser.FirstYearDefault = Convert.ToDouble(FirstYearDefault);
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 21)
                        {
                            try
                            {
                                RenewalDefault = Convert.ToString(TempTable.Rows[i][21]);
                                Currentuser.RenewalDefault = Convert.ToDouble(RenewalDefault);
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 22)
                        {
                            try
                            {
                                ReportForEntireAgency = Convert.ToString(TempTable.Rows[i][22]);
                                Currentuser.ReportForEntireAgency = false;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 23)
                        {
                            try
                            {
                                ReportForOwnBusiness = Convert.ToString(TempTable.Rows[i][23]);
                                Currentuser.ReportForOwnBusiness = false;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 24)
                        {
                            try
                            {
                                AddPayeeOn = Convert.ToString(TempTable.Rows[i][24]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 25)
                        {
                            try
                            {
                                DisableAgentEditing = Convert.ToString(TempTable.Rows[i][25]);
                                Currentuser.DisableAgentEditing = false;
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 26)
                        {
                            try
                            {
                                Moduletodisplay = Convert.ToString(TempTable.Rows[i][26]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 27)
                        {
                            try
                            {
                                AccessRight = Convert.ToString(TempTable.Rows[i][27]);

                            }
                            catch
                            {
                            }
                        }

                        try
                        {
                            if (Currentuser.Permissions == null)
                                Currentuser.Permissions = new ObservableCollection<UserPermissions>();

                            UserPermissions objUserPermission = new UserPermissions();
                            objUserPermission.Module = MasterModule.PolicyManager;
                            objUserPermission.Permission = ModuleAccessRight.Read;
                            objUserPermission.UserPermissionId = Guid.NewGuid();
                            Currentuser.Permissions.Add(objUserPermission);

                            UserPermissions objUserPermission1 = new UserPermissions();
                            objUserPermission1.Module = MasterModule.PeopleManager;
                            objUserPermission1.Permission = ModuleAccessRight.Read;
                            objUserPermission1.UserPermissionId = Guid.NewGuid();
                            Currentuser.Permissions.Add(objUserPermission1);

                            //UserPermissions objUserPermission2 = new UserPermissions();
                            //objUserPermission2.Module = MasterModule.HelpUpdate;
                            //objUserPermission2.Permission = ModuleAccessRight.Read;
                            //objUserPermission2.UserPermissionId = Guid.NewGuid();
                            //Currentuser.Permissions.Add(objUserPermission2);

                            //UserPermissions objUserPermission3 = new UserPermissions();
                            //objUserPermission3.Module = MasterModule.Settings;
                            //objUserPermission3.Permission = ModuleAccessRight.Read;
                            //objUserPermission3.UserPermissionId = Guid.NewGuid();
                            //Currentuser.Permissions.Add(objUserPermission3);

                            //UserPermissions objUserPermission4 = new UserPermissions();
                            //objUserPermission4.Module = MasterModule.FollowUpManger;
                            //objUserPermission4.Permission = ModuleAccessRight.Read;
                            //objUserPermission4.UserPermissionId = Guid.NewGuid();
                            //Currentuser.Permissions.Add(objUserPermission4);

                            ImportAgent();
                            iCount = iCount + 1;
                        }
                        catch
                        {
                        }
                    }
                }

                //Guid guid = (Guid)Currentuser.LicenseeId;
                //serviceClients.UserClient.ImportHouseUsers(TempTable, guid);
                //Right Stored proce to inserrt into database


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            MessageBox.Show(iCount.ToString() + " Agents Imported successfully ");
        }

        private void ImportAgent()
        {

            UserClient Client = serviceClients.UserClient;
            try
            {
                using ((IDisposable)Client)
                {
                    Currentuser.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                    Currentuser.Role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);

                    if (serviceClients.UserClient.IsUserNameExist(Currentuser.UserCredentialID, Currentuser.UserName))
                    {
                        return;
                    }

                    Client.AddUpdateUser(Currentuser);
                    Currentuser.IsSaved = true;

                    if (VMInstances.PolicyManager != null)
                        VMInstances.PolicyManager.RefreshRequired = true;

                    if (!LicenseeUsers.Any(s => s.UserCredentialID == Currentuser.UserCredentialID))
                    {
                        foreach (User user in LicenseeUsers)
                        {
                            if (user.LinkedUsers == null) user.LinkedUsers = new ObservableCollection<LinkedUser>();
                            LinkedUser linkedUser = new LinkedUser();
                            linkedUser.Copy(Currentuser);
                            if (user.IsHouseAccount)
                            {
                                linkedUser.IsConnected = true;
                                user.LinkedUsers.Add(linkedUser);
                            }
                            else
                                user.LinkedUsers.Add(linkedUser);
                        }
                        LicenseeUsers.Add(Currentuser);
                        LicenseeUsers.RefreshSort();
                    }
                    else
                    {
                        foreach (User user in LicenseeUsers)
                        {
                            if (user.LinkedUsers != null)
                            {
                                LinkedUser linkedUser = user.LinkedUsers.FirstOrDefault(s => s.UserId == Currentuser.UserCredentialID);
                                if (linkedUser != null)
                                {
                                    bool ConnectedState = linkedUser.IsConnected;
                                    linkedUser.Copy(Currentuser);
                                    linkedUser.IsConnected = ConnectedState;
                                }
                            }
                        }
                        LicenseeUsers.RefreshSort();
                    }
                }
                if (isShowpopUp)
                {
                    MessageBox.Show("Please make sure you update the default permissions on the settings tab for this agent.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    isShowpopUp = false;
                }

                SharedVMData.isRefeshAgentList = true;
                //RefershPayeeList();

            }
            catch
            {
                SharedVMData.isRefeshAgentList = false;
            }
        }

        private int RoleID(string strRole)
        {
            int value = 3;//agent ID

            if (string.IsNullOrEmpty(strRole))
            {
                value = 3;
            }
            else if (strRole.ToLower() == "superadmin")
            {
                value = 1;
            }
            else if (strRole.ToLower() == "administrator")
            {
                value = 2;
            }
            else if (strRole.ToLower() == "agent")
            {
                value = 3;
            }
            else if (strRole.ToLower() == "dep")
            {
                value = 5;
            }
            return value;

        }

        private bool trueOrFalse(string strValue)
        {
            bool bvalue = false;//agent ID

            if (string.IsNullOrEmpty(strValue))
            {
                bvalue = false;
            }
            else if (strValue.ToLower() == "no")
            {
                bvalue = false;
            }
            else if (strValue.ToLower() == "yes")
            {
                bvalue = true;
            }

            return bvalue;

        }

       

        private bool BeforeOnSaveAgent()
        {
            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return false;

            if (RoleManager.Role == UserRole.DEP)
                return false;

            //if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
            //{
            //    if (Currentuser.UserName != RoleManager.LoggedInUser)
            //        return false;
            //}

            if (RoleManager.UserAccessPermission(MasterModule.PeopleManager) == ModuleAccessRight.Read)
                return false;

            if (string.IsNullOrEmpty(Currentuser.UserName))
                return false;

            if (string.IsNullOrEmpty(Currentuser.Password))
                return false;

            if (Currentuser.Password != Currentuser.ConfirmPassword)
                return false;

            return true;
        }

        private void OnSaveAgent()
        {
            if (!_ViewValidation.Validate("People Manager"))
            {
                MessageBox.Show("Validation failed.");
                return;
            }

            UserClient Client = serviceClients.UserClient;
            try
            {
                using ((IDisposable)Client)
                {
                    Currentuser.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                    Currentuser.Role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);

                    if (serviceClients.UserClient.IsUserNameExist(Currentuser.UserCredentialID, Currentuser.UserName))
                    {
                        MessageBox.Show("Username already exist. Please use different UserName.", "UserName Exist", MessageBoxButton.OK);
                        return;
                    }

                    Client.AddUpdateUser(Currentuser);
                    Currentuser.IsSaved = true;

                    if (VMInstances.PolicyManager != null)
                        VMInstances.PolicyManager.RefreshRequired = true;

                    if (!LicenseeUsers.Any(s => s.UserCredentialID == Currentuser.UserCredentialID))
                    {
                        UserRole role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
                        foreach (User user in LicenseeUsers)
                        {
                            if (user.LinkedUsers == null) user.LinkedUsers = new ObservableCollection<LinkedUser>();
                            LinkedUser linkedUser = new LinkedUser();
                            linkedUser.Copy(Currentuser);
                            if (user.IsHouseAccount)
                            {
                                linkedUser.IsConnected = true;
                                user.LinkedUsers.Add(linkedUser);
                            }
                            else
                                user.LinkedUsers.Add(linkedUser);
                        }
                        LicenseeUsers.Add(Currentuser);
                        LicenseeUsers.RefreshSort();
                    }
                    else
                    {
                        foreach (User user in LicenseeUsers)
                        {
                            if (user.LinkedUsers != null)
                            {
                                LinkedUser linkedUser = user.LinkedUsers.FirstOrDefault(s => s.UserId == Currentuser.UserCredentialID);
                                if (linkedUser != null)
                                {
                                    bool ConnectedState = linkedUser.IsConnected;
                                    linkedUser.Copy(Currentuser);
                                    linkedUser.IsConnected = ConnectedState;
                                }
                            }
                        }
                        LicenseeUsers.RefreshSort();
                    }
                }
                if (isShowpopUp)
                {
                    MessageBox.Show("Please make sure you update the default permissions on the settings tab for this agent.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    isShowpopUp = false;
                }
                //Ankita
                SharedVMData.isRefeshAgentList = true;
                //RefershPayeeList();

            }
            catch
            {
                SharedVMData.isRefeshAgentList = false;
            }
        }

        private void RefershPayeeList()
        {

            if (SharedVMData != null)
            {
                BackgroundWorker worker2 = new BackgroundWorker();
                worker2.DoWork += new System.ComponentModel.DoWorkEventHandler(worker2_DoWork);
                worker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker2_RunWorkerCompleted);
                worker2.RunWorkerAsync();

            }
        }

        void worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SharedVMData.isRefeshAgentList = false;
        }

        void worker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                {
                    SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllPayee());
                }
                else
                {
                    Guid guidID = (Guid)RoleManager.LicenseeId;
                    SharedVMData.GlobalAgentList = new ObservableCollection<User>(serviceClients.UserClient.GetAllPayeeByLicencessID(guidID));
                }
            }
            catch
            {
            }
        }
        //private void PopulateLicenseeList()
        //{
        //    LicenseeClient client = serviceClients.LicenseeClient;
        //    SharedVMData.Licensees = new ObservableCollection<LicenseeDisplayData>(
        //        client.GetDisplayedLicenseeList(RoleManager.LicenseeId ?? Guid.Empty).OrderBy(p=>p.Company));
        //}

        private List<User> GetPeopleManagerInfo(ObservableCollection<User> _data)
        {
            if (_data == null)
                return null;
            return (from _Info in _data
                    select new User
                    {
                        Address = _Info.Address,
                        PasswordHintA = _Info.PasswordHintA,
                        CellPhone = _Info.CellPhone,
                        City = _Info.City,
                        Company = _Info.Company,
                        Email = _Info.Email,
                        Fax = _Info.Fax,
                        FirstName = _Info.FirstName,
                        FirstYearDefault = _Info.FirstYearDefault,
                        IsHouseAccount = _Info.IsHouseAccount,
                        LastName = _Info.LastName,
                        LicenseeId = _Info.LicenseeId,
                        LicenseeName = _Info.LicenseeName,
                        NickName = _Info.NickName,
                        OfficePhone = _Info.OfficePhone,
                        PasswordHintQ = _Info.PasswordHintQ,
                        RenewalDefault = _Info.RenewalDefault,
                        State = _Info.State,
                        UserCredentialID = _Info.UserCredentialID,
                        UserName = _Info.UserName,
                        Password = _Info.Password,
                        ZipCode = _Info.ZipCode,
                        DisableAgentEditing = _Info.DisableAgentEditing,
                        Role = _Info.Role,
                        Permissions = _Info.Permissions,
                        LinkedUsers = _Info.LinkedUsers
                    }).ToList();


        }
        #endregion
        #endregion
        #region User Permissions
        #region Properties
        private ICommand _SelectAll;
        public ICommand SelectAll
        {
            get
            {
                if (_SelectAll == null)
                {

                    _SelectAll = new BaseCommand(param => SelectAlluser());

                }
                return _SelectAll;
            }

        }
        private ICommand _DeSelectAll;
        public ICommand SelectNone
        {
            get
            {
                if (_DeSelectAll == null)
                {

                    _DeSelectAll = new BaseCommand(param => DeSelectAlluser());

                }
                return _DeSelectAll;
            }

        }

        #endregion
        #region Private functions

        void _objPermissionView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private bool BeforeOnUpdateAgentData()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PeopleManager) == ModuleAccessRight.Read)
                return false;

            if (Currentuser == null || Currentuser.IsSaved == false)
                return false;

            if (SharedVMData.SelectedLicensee == null || SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                return false;

            return true;
        }

        private void OnUpdateAgentData()
        {
            try
            {
                UserClient Client = serviceClients.UserClient;
                using ((IDisposable)Client)
                {

                    Currentuser.LicenseeId = SharedVMData.SelectedLicensee.LicenseeId;
                    Currentuser.DisableAgentEditing = IsAgentDiable;
                    Currentuser.IsAccountExec = IsAccountExec;

                    Currentuser.Role = (UserRole)Enum.Parse(typeof(UserRole), (_UserTypeListview.CurrentItem as UsersRole).Name);
                                        
                    if (Client.CheckAccoutExec(Currentuser.UserCredentialID))
                    {
                        if (Currentuser.IsAccountExec == false && Client.HasAssociatedPolicies(Currentuser.UserCredentialID, Currentuser.NickName))
                        {
                            MessageBox.Show(Currentuser.NickName + " is account exec and it used in policy . so account exec will not update.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            IsAccountExec = true;
                            Currentuser.IsAccountExec = true;
                            return; //Acme - bug fix 
                        }
                    }

                    Client.AddUpdateUserPermissionAndOtherData(Currentuser);

                    if (!LicenseeUsers.Any(s => s.UserCredentialID == Currentuser.UserCredentialID))
                    {
                        LicenseeUsers.Add(Currentuser);
                    }
                    MessageBox.Show("Update succesfully ", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                   
                }
            }
            catch (Exception)
            {
               // MessageBox.Show(ex.ToString());
            }

            
        }

        private void SelectAlluser()
        {
            foreach (LinkedUser linkedUser in Currentuser.LinkedUsers)
                linkedUser.IsConnected = true;
        }

        private void DeSelectAlluser()
        {
            foreach (LinkedUser linkedUser in Currentuser.LinkedUsers)
                linkedUser.IsConnected = false;
        }

        #endregion
        #endregion

        #region Refresh

        public void Refresh()
        {
            if (SharedVMData.SelectedLicensee == null)
                return;
            try
            {
                SharedVMData.RefreshLicensees();
                SelectedLicenseeChanged();


                Guid SelectedUserId = Currentuser.UserCredentialID;
                UserRole SelectedUserRole = Currentuser.Role;

                if (RoleManager.Role == UserRole.SuperAdmin)
                    AllUsers = serviceClients.UserClient.GetAllUsers();
                else
                    AllUsers = serviceClients.UserClient.GetUsersByLicensee(RoleManager.LicenseeId.Value);

                if (AllUsers != null)
                    AllUsers.ToList().ForEach(s => s.ConfirmPassword = s.Password);

                if (SharedVMData.Licensees != null && SharedVMData.Licensees.Count != 0)
                {
                    if (SelectedUserRole == UserRole.Administrator)
                        _UserTypeListview.MoveCurrentToPosition(1);

                    Currentuser = LicenseeUsers.FirstOrDefault(s => s.UserCredentialID == SelectedUserId);
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

    public class UsersRole
    {
        public int RoleID { get; set; }
        public string Name { get; set; }
    }
}
