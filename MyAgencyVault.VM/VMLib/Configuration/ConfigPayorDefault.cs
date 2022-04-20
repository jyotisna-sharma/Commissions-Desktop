using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Windows.Input;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigPayorDefaultVM : BaseViewModel
    {
        private VMConfigrationManager ParentVM;
        private ServiceClients _ServiceClients;
        string strIsrunFollowUpService = string.Empty;
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
        public ConfigPayorDefaultVM(VMConfigrationManager configManager)
        {
            ParentVM = configManager;
            ParentVM.SelectedPayorChanged += new VMConfigrationManager.SelectedPayorChangedEventHandler(ParentVM_SelectedPayorChanged);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentVM_PropertyChanged);

            GlobalFileTypes = new ObservableCollection<FileType>(serviceClients.MasterClient.GetSupportedFileTypeList());
            AllPayorStatementDates = serviceClients.StatementDatesClient.GetStatementDates();
            LoadFollowUpSetting();
        }
        
        void ParentVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
        }

        void ParentVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPayor":
                    if (CurrentPayor != null)
                    {
                        PayorDefaults payorDefaultLocal = serviceClients.PayorDefaultsClient.GetPayorDefaultBy(CurrentPayor.PayorID);
                        if (payorDefaultLocal == null || payorDefaultLocal.PayorDefaultSettingsId == Guid.Empty)
                        {
                            CurrentPayorDefault = new PayorDefaults();
                            CurrentFileTypes = GlobalFileTypes.FirstOrDefault();
                        }
                        else
                        {
                            CurrentPayorDefault = payorDefaultLocal;
                            CurrentFileTypes = GlobalFileTypes.FirstOrDefault(s => s.FileTypeId == CurrentPayorDefault.FileTypeId);
                        }
                    }
                    break;
            }
        }

        #region Property

        private Payor _CurrentPayor;
        public Payor CurrentPayor
        {
            get
            {
                return _CurrentPayor;
            }
            set
            {
                _CurrentPayor = value;
                OnPropertyChanged("CurrentPayor");
            }
        }

        #region "Add/update followUP Setting"
       
        private bool _IsStartService;
        public bool IsStartService
        {
            get
            {
                return _IsStartService;
            }
            set
            {
                _IsStartService = value;
                OnPropertyChanged("IsStartService");
            }
        }

        private bool _IsStopService;
        public bool IsStopService
        {
            get
            {
                return _IsStopService;
            }
            set
            {
                _IsStopService = value;
                OnPropertyChanged("IsStopService");
            }
        }

        private int _intNextFollowupDays;
        public int intNextFollowupDays
        {
            get
            {
                return _intNextFollowupDays;
            }
            set
            {
                _intNextFollowupDays = value;
                OnPropertyChanged("intNextFollowupDays");
            }
        }

        private ICommand _UpdateFollowUpsetting;
        public ICommand UpdateFollowUpsetting
        {
            get
            {
                if (_UpdateFollowUpsetting == null)
                {
                    _UpdateFollowUpsetting = new BaseCommand(x => BeforeUpdateFollowUpsetting(), x => SaveUpdateFollowUpsetting());
                }
                return _UpdateFollowUpsetting;
            }

        }

        private void LoadFollowUpSetting()
        {
            intNextFollowupDays = Convert.ToInt32(serviceClients.MasterClient.GetSystemConstantKeyValue("NextFollowUpRunDaysCount"));
            strIsrunFollowUpService = serviceClients.MasterClient.GetSystemConstantKeyValue("FollowUpService");
            if (strIsrunFollowUpService == "Running")
            {
                IsStopService = false;
                IsStartService = true;
            }
            else
            {
                IsStartService = false;
                IsStopService = true;
            }
        }

        private bool BeforeUpdateFollowUpsetting()
        {           
            if (intNextFollowupDays > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private void SaveUpdateFollowUpsetting()
        {
            //Call to refresh again after save
            serviceClients.PayorSourceClient.UpdateFollowUpDateAndserviceStatus("NextFollowUpRunDaysCount", intNextFollowupDays.ToString());
            if (IsStartService)
            {
                serviceClients.PayorSourceClient.UpdateFollowUpDateAndserviceStatus("FollowUpService", "Running");
            }
            else
            {
                serviceClients.PayorSourceClient.UpdateFollowUpDateAndserviceStatus("FollowUpService", "Stop");
            }
            
            LoadFollowUpSetting();
        }

        #endregion

        /// <summary>
        /// to select default payor
        /// </summary>
        private PayorDefaults _selectedPayorDefault;
        public PayorDefaults CurrentPayorDefault
        {
            get
            {
                if (_selectedPayorDefault != null && _selectedPayorDefault.IsTotalExists == true)
                {
                    IsLocationColumn = true;
                    IsLocationRow = true;
                }
                else
                {
                    IsLocationColumn = false;
                    IsLocationRow = false;
                }
                return _selectedPayorDefault == null ? new PayorDefaults() : _selectedPayorDefault;

            }
            set
            {
                _selectedPayorDefault = value;
                OnPropertyChanged("CurrentPayorDefault");
            }
        }

        /// <summary>
        /// collection to get statement date details
        /// </summary>
        private ObservableCollection<StatementDates> _AllPayorStatementDates;
        public ObservableCollection<StatementDates> AllPayorStatementDates
        {
            get
            {
                return _AllPayorStatementDates;
            }
            set
            {
                _AllPayorStatementDates = value;
            }
        }

        private bool _IsLocationRow;
        public bool IsLocationRow
        {
            get { return _IsLocationRow; }

            set { _IsLocationRow = value; OnPropertyChanged("IsLocationRow"); }

        }

        private bool _IsLocationColumn;
        public bool IsLocationColumn
        {
            get { return _IsLocationColumn; }

            set { _IsLocationColumn = value; OnPropertyChanged("IsLocationColumn"); }

        }

        /// <summary>
        /// collection to get file type
        /// </summary>
        private ObservableCollection<FileType> _globalFileTypes;
        public ObservableCollection<FileType> GlobalFileTypes
        {
            get
            {
                return _globalFileTypes == null ? new ObservableCollection<FileType>() : _globalFileTypes; ;
            }
            set
            {
                _globalFileTypes = value;
                OnPropertyChanged("GlobalFileTypes");
            }
        }

        /// <summary>
        /// get current selected file
        /// </summary>
        private FileType _selectedFileTypes;
        public FileType CurrentFileTypes
        {

            get
            {
                return _selectedFileTypes == null ? new FileType() : _selectedFileTypes;

            }
            set
            {
                _selectedFileTypes = value;
                OnPropertyChanged("CurrentFileTypes");


            }
        }


        #endregion

        #region Commands

        /// <summary>
        /// save payor default information 
        /// </summary>
        private ICommand _savePayorDefault;
        public ICommand SavePayorDefault
        {
            get
            {
                if (_savePayorDefault == null)
                {
                    _savePayorDefault = new BaseCommand(x => canExecute(), x => InsertUpdatePayorDefault());
                }
                return _savePayorDefault;
            }

        }

        private ICommand _OnOpenStatementDate;
        public ICommand OnOpenStatementDate
        {
            get
            {
                if (_OnOpenStatementDate == null)
                {
                    _OnOpenStatementDate = new BaseCommand(param => BeforeDoOpenStatementDate(),param => DoOpenStatementDate());

                }
                return _OnOpenStatementDate;
            }

        }

        private bool BeforeDoOpenStatementDate()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        private void InsertUpdatePayorDefault()
        {
            try
            {
                if (CurrentPayorDefault == null)
                    return;

                if (CurrentPayorDefault.PayorDefaultSettingsId == Guid.Empty)
                    CurrentPayorDefault.PayorDefaultSettingsId = Guid.NewGuid();

                CurrentPayorDefault.GlobalPayorId = CurrentPayor.PayorID;
                CurrentPayorDefault.FileTypeId = CurrentFileTypes.FileTypeId;
                serviceClients.PayorDefaultsClient.AddUpdatePayorDefaults(CurrentPayorDefault);
            }
            catch (Exception)
            {
            }
        }

        private bool canExecute()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        public void OnCloseCalender(ObservableCollection<StatementDates> statementDates)
        {
            try
            {
                if (statementDates != null && statementDates.Count != 0)
                    serviceClients.StatementDatesClient.AddUpdateStatementDates(statementDates);
            }
            catch (Exception)
            {
            }
        }

        public void DoOpenStatementDate()
        {
            try
            {
                ParentVM.DoOpenStatementDate();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
