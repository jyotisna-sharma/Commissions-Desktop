using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using System.Windows.Input;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Windows;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigProductScheduleVM : BaseViewModel
    {
        private VMConfigrationManager ParentVM;
        public ScheduleGridData ScheduleGrdData;
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
        public ConfigProductScheduleVM(VMConfigrationManager configManager, ScheduleGridData schData,ConfigPayorCarriersVM CarriersVM,ConfigCarrierCoveragesVM CoveragesVM)
        {
            ParentVM = configManager;
            ScheduleGrdData = schData;

            ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
            ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
            ScheduleGrdData.RateColumnName = "Commission %";

            ParentVM.SelectedPayorChanged += new VMConfigrationManager.SelectedPayorChangedEventHandler(ParentVM_SelectedPayorChanged);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentVM_PropertyChanged);

            GlobalScheduleTypes = new ObservableCollection<PolicyIncomingScheduleType>(serviceClients.MasterClient.GetPolicyIncomingScheduleTypeList());

            if (_NewCovrageSheduleEntry == null)
                _NewCovrageSheduleEntry = new IncomingScheduleEntry();
        }

        void ParentVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
            PayorCarriers = ParentVM.ConfigSharedDataVM.Carriers;
            
            if (PayorCarriers != null)
                SelectedScheduleCarrier = PayorCarriers.FirstOrDefault();
        }

        void ParentVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPayor":
                    CurrentScheduleTypes = GlobalScheduleTypes.Count == 0 ? new PolicyIncomingScheduleType() : GlobalScheduleTypes.FirstOrDefault();
                    SetLabelText();
                    break;

                case "SelectedScheduleCarrier":
                    if (CurrentPayor != null && SelectedScheduleCarrier != null)
                    {
                        CarrierCoverages = serviceClients.CoverageClient.GetPayorCarrierCoverages(CurrentPayor.PayorID, SelectedScheduleCarrier.CarrierId);
                    }
                    else
                    {
                        CarrierCoverages = null;
                    }

                    if (CarrierCoverages != null)
                        SelectedScheduleProduct = CarrierCoverages.FirstOrDefault();
                    break;

                case "SelectedCovrageSheduleEntry":
                    if (SelectedCovrageSheduleEntry != null)
                        NewCovrageSheduleEntry = SelectedCovrageSheduleEntry.Clone() as IncomingScheduleEntry;
                    else
                        NewCovrageSheduleEntry = new IncomingScheduleEntry();

                    break;

                case "SelectedScheduleProduct":
                    if (SelectedScheduleCarrier != null && SelectedScheduleProduct != null)
                    {
                        CurrentCovrageShedule = serviceClients.GlobalIncomingScheduleClient.GetGlobalIncomingSchedule(SelectedScheduleCarrier.CarrierId, SelectedScheduleProduct.CoverageID);

                        if (CurrentCovrageShedule.IncomingScheduleList != null)
                            CurrentCovrageShedule.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(CurrentCovrageShedule.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());

                        if (CurrentCovrageShedule != null && CurrentCovrageShedule.ScheduleTypeId != 0)
                            CurrentScheduleTypes = GlobalScheduleTypes.FirstOrDefault(s => s.ScheduleTypeId == CurrentCovrageShedule.ScheduleTypeId);
                        else
                            CurrentScheduleTypes = GlobalScheduleTypes.FirstOrDefault();
                    }
                    else
                    {
                        CurrentCovrageShedule.IncomingScheduleList = null;
                    }

                    break;
                case "CurrentScheduleTypes":
                    ScheduleGrdData.FromRangeVisibility = Visibility.Visible;
                    ScheduleGrdData.ToRangeVisibility = Visibility.Visible;
                    ScheduleGrdData.RateColumnName = "Commission %";

                    switch (CurrentScheduleTypes.ScheduleTypeId)
                    {
                        case 3:
                            ScheduleGrdData.RateColumnName = "Fee($) / Head";
                            break;
                        case 4:
                            ScheduleGrdData.RateColumnName = "Fee($) / Head";
                            break;
                        case 5:
                            ScheduleGrdData.RateColumnName = "Fee($)";
                            ScheduleGrdData.FromRangeVisibility = Visibility.Collapsed;
                            ScheduleGrdData.ToRangeVisibility = Visibility.Collapsed;
                            break;
                        default:
                            break;
                    }
                    SetLabelText();
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

        /// <summary>
        /// collection to get shedule type
        /// </summary>
        private ObservableCollection<PolicyIncomingScheduleType> _globalScheduleTypes;
        public ObservableCollection<PolicyIncomingScheduleType> GlobalScheduleTypes
        {
            get
            {
                return _globalScheduleTypes;
            }
            set
            {
                _globalScheduleTypes = value;
                OnPropertyChanged("GlobalScheduleTypes");
            }
        }


        private int PrevSelectedScheduleTypeId;
        /// <summary>
        /// to select current shedule type
        /// </summary>
        private PolicyIncomingScheduleType _selectedScheduleTypes;
        public PolicyIncomingScheduleType CurrentScheduleTypes
        {
            get
            {
                return _selectedScheduleTypes == null ? new PolicyIncomingScheduleType() : _selectedScheduleTypes;
            }
            set
            {
                PrevSelectedScheduleTypeId = ((_selectedScheduleTypes != null) ? _selectedScheduleTypes.ScheduleTypeId : 0);
                _selectedScheduleTypes = value;
                OnPropertyChanged("CurrentScheduleTypes");
            }
        }

        /// <summary>
        /// set from test 
        /// </summary>
        private string _FromText;
        public string FromText
        {
            get
            {
                return _FromText;
            }
            set
            {
                _FromText = value;
                OnPropertyChanged("FromText");

            }
        }

        /// <summary>
        /// set commisition test 
        /// </summary>
        private string _CommText;
        public string CommTText
        {
            get
            {
                return _CommText;
            }
            set
            {
                _CommText = value;
                OnPropertyChanged("CommTText");

            }
        }

        /// <summary>
        /// Set To test
        /// </summary>
        private string _ToText;
        public string ToText
        {
            get
            {
                return _ToText;
            }
            set
            {
                _ToText = value;
                OnPropertyChanged("ToText");

            }
        }

        /// <summary>
        ///  To get shedule details
        /// </summary>
        private GlobalIncomingSchedule _CurrentCovrageShedule;
        public GlobalIncomingSchedule CurrentCovrageShedule
        {
            get
            {
                return _CurrentCovrageShedule == null ? new GlobalIncomingSchedule() : _CurrentCovrageShedule;
            }
            set
            {
                _CurrentCovrageShedule = value;
                OnPropertyChanged("CurrentCovrageShedule");
            }
        }

        /// <summary>
        ///  This schedule is used for saving actual schedule before any modification so that we can restore
        ///  original schedule if the opetration is cancelled.
        /// </summary>
        private GlobalIncomingSchedule _SavedCovrageShedule;


        private IncomingScheduleEntry _NewCovrageSheduleEntry;
        public IncomingScheduleEntry NewCovrageSheduleEntry
        {
            get
            {
                return _NewCovrageSheduleEntry == null ? new IncomingScheduleEntry() : _NewCovrageSheduleEntry;
            }
            set
            {
                _NewCovrageSheduleEntry = value;
                OnPropertyChanged("NewCovrageSheduleEntry");
            }
        }

        private IncomingScheduleEntry _SelectedCovrageSheduleEntry;
        public IncomingScheduleEntry SelectedCovrageSheduleEntry
        {
            get
            {
                return _SelectedCovrageSheduleEntry == null ? new IncomingScheduleEntry() : _SelectedCovrageSheduleEntry;
            }
            set
            {
                _SelectedCovrageSheduleEntry = value;
                OnPropertyChanged("SelectedCovrageSheduleEntry");
            }
        }

        private Coverage _SelectedScheduleProduct = null;
        public Coverage SelectedScheduleProduct
        {
            get
            {
                return _SelectedScheduleProduct;
            }
            set
            {
                _SelectedScheduleProduct = value;
                OnPropertyChanged("SelectedScheduleProduct");
            }
        }

        /// <summary>
        /// Selected schedule carreir on the schedule tab.
        /// </summary>
        private Carrier _SelectedScheduleCarrier;
        public Carrier SelectedScheduleCarrier
        {
            get
            {
                return _SelectedScheduleCarrier;
            }
            set
            {
                _SelectedScheduleCarrier = value;
                OnPropertyChanged("SelectedScheduleCarrier");
            }
        }

        private ObservableCollection<Carrier> _PayorCarriers;
        public ObservableCollection<Carrier> PayorCarriers
        {
            get
            {
                return _PayorCarriers;
            }
            set
            {
                _PayorCarriers = value;
                OnPropertyChanged("PayorCarriers");
            }
        }

        private ObservableCollection<Coverage> _CarrierCoverages;
        public ObservableCollection<Coverage> CarrierCoverages
        {
            get
            {
                return _CarrierCoverages;
            }
            set
            {
                _CarrierCoverages = value;
                OnPropertyChanged("CarrierCoverages");
            }
        }

        #endregion

        #region Icommands

        private ICommand _SaveDefaultSchedule = null;
        public ICommand SaveDefaultSchedule
        {
            get
            {
                if (_SaveDefaultSchedule == null)
                    _SaveDefaultSchedule = new BaseCommand(param => BeforeOnSaveDefaultSchedule(),param => OnSaveDefaultSchedule());
                return _SaveDefaultSchedule;
            }

        }

        private ICommand _AddClick = null;
        public ICommand AddClick
        {
            get
            {
                if (_AddClick == null)
                    _AddClick = new BaseCommand(param => BeforeAddScheduleEntry(),param => AddScheduleEntry());
                return _AddClick;
            }

        }


        private ICommand _DeleteCovrageItem = null;
        public ICommand DeleteScheduleEntry
        {
            get
            {
                if (_DeleteCovrageItem == null)
                    _DeleteCovrageItem = new BaseCommand(param => BeforeOnDeleteScheduleEntry(),param => OnDeleteScheduleEntry());
                return _DeleteCovrageItem;
            }

        }

        private ICommand _NewSchedule;
        public ICommand NewSchedule
        {
            get
            {
                if (_NewSchedule == null)
                    _NewSchedule = new BaseCommand(param => BeforeOnNewShedule(),param => OnNewShedule());
                return _NewSchedule;
            }
        }

        private ICommand _CancelShedule;
        public ICommand CancelShedule
        {
            get
            {
                if (_CancelShedule == null)
                    _CancelShedule = new BaseCommand(param => BeforeOnCancelShedule(),param => OnCancelShedule());
                return _CancelShedule;
            }
        }

        
        #endregion

        #region Method

        private bool BeforeOnCancelShedule()
        {
            if (SelectedScheduleProduct == null || SelectedScheduleCarrier == null)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        private bool BeforeOnNewShedule()
        {
            if (SelectedScheduleProduct == null || SelectedScheduleCarrier == null)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        private bool BeforeOnDeleteScheduleEntry()
        {
            if (SelectedScheduleProduct == null || SelectedScheduleCarrier == null)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        private bool BeforeOnSaveDefaultSchedule()
        {
            if (SelectedScheduleProduct == null || SelectedScheduleCarrier == null)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        private bool BeforeAddScheduleEntry()
        {
            if (SelectedScheduleProduct == null || SelectedScheduleCarrier == null)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            return true;
        }

        private void OnNewShedule()
        {
            NewCovrageSheduleEntry = new IncomingScheduleEntry();
        }

        private void OnCancelShedule()
        {
            if (_SavedCovrageShedule != null)
            {
                CurrentCovrageShedule = _SavedCovrageShedule;
            }
        }

        void SetLabelText()
        {
            try
            {
                if (_selectedScheduleTypes == null) return;
                switch (_selectedScheduleTypes.Name)
                {
                    case "Flat $":
                        FromText = "From($)";
                        ToText = "To($)";
                        CommTText = "Fee $ ";
                        break;
                    case "Percentage of Premium":
                        FromText = "From($)";
                        ToText = "To($)";
                        CommTText = "Commission(%) ";
                        break;
                    case "Percentage of Target":
                        FromText = "From($)";
                        ToText = "To($)";
                        CommTText = "Commission(%) ";
                        break;
                    case "Per Head Fee Scale":
                        FromText = "From";
                        ToText = "To";
                        CommTText = "Fee $ ";
                        break;
                    case "Per Head Fee Target":
                        FromText = "From";
                        ToText = "To";
                        CommTText = "Fee $ ";
                        break;
                }
            }
            catch (Exception)
            {
            }

        }

        private void OnSaveDefaultSchedule()
        {
            try
            {
                CurrentCovrageShedule.ScheduleTypeId = CurrentScheduleTypes.ScheduleTypeId;
                serviceClients.GlobalIncomingScheduleClient.AddUpdateGlobalIncomingSchedule(CurrentCovrageShedule);
                CurrentCovrageShedule.IsModified = false;
                _SavedCovrageShedule = null;
            }
            catch (Exception)
            {
            }
        }

        private void AddScheduleEntry()
        {
            try
            {
                if (CurrentScheduleTypes.ScheduleTypeId != 5)
                {
                    if (NewCovrageSheduleEntry.FromRange == null) return;
                    if (NewCovrageSheduleEntry.ToRange == null) return;
                    if (NewCovrageSheduleEntry.FromRange > NewCovrageSheduleEntry.ToRange) return;
                }

                if (!CurrentCovrageShedule.IsModified)
                {
                    _SavedCovrageShedule = CurrentCovrageShedule.Clone() as GlobalIncomingSchedule;
                    CurrentCovrageShedule.IsModified = true;
                }

                if (CurrentCovrageShedule.IncomingScheduleList == null)
                    CurrentCovrageShedule.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>();

                if (NewCovrageSheduleEntry.CoveragesScheduleId == Guid.Empty)
                {
                    NewCovrageSheduleEntry.CoveragesScheduleId = Guid.NewGuid();
                    IncomingScheduleEntry entry = NewCovrageSheduleEntry.Clone() as IncomingScheduleEntry;
                    CurrentCovrageShedule.IncomingScheduleList.Add(entry);
                }
                else
                {
                    IncomingScheduleEntry entry = CurrentCovrageShedule.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewCovrageSheduleEntry.CoveragesScheduleId);
                    if (entry != null)
                    {
                        entry.EffectiveFromDate = NewCovrageSheduleEntry.EffectiveFromDate;
                        entry.EffectiveToDate = NewCovrageSheduleEntry.EffectiveToDate;
                        entry.FromRange = NewCovrageSheduleEntry.FromRange;
                        entry.ToRange = NewCovrageSheduleEntry.ToRange;
                        entry.Rate = NewCovrageSheduleEntry.Rate;
                    }
                }

                CurrentCovrageShedule.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(CurrentCovrageShedule.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());
            }
            catch (Exception)
            {
            }
        }

        public void OnDeleteScheduleEntry()
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (NewCovrageSheduleEntry != null && CurrentCovrageShedule.IncomingScheduleList != null)
                    {
                        IncomingScheduleEntry entry = CurrentCovrageShedule.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == NewCovrageSheduleEntry.CoveragesScheduleId);
                        if (entry != null)
                        {
                            if (!CurrentCovrageShedule.IsModified)
                            {
                                _SavedCovrageShedule = CurrentCovrageShedule.Clone() as GlobalIncomingSchedule;
                                CurrentCovrageShedule.IsModified = true;
                            }

                            CurrentCovrageShedule.IncomingScheduleList.Remove(entry);

                            if (CurrentCovrageShedule.IncomingScheduleList.Count != 0)
                            {
                                NewCovrageSheduleEntry = CurrentCovrageShedule.IncomingScheduleList[0].Clone() as IncomingScheduleEntry;
                            }
                            else
                            {
                                NewCovrageSheduleEntry = new IncomingScheduleEntry();
                                return;
                            }

                            CurrentCovrageShedule.IncomingScheduleList = new ObservableCollection<IncomingScheduleEntry>(CurrentCovrageShedule.IncomingScheduleList.OrderBy(s => s.EffectiveFromDate).OrderBy(s => s.FromRange).ToList());

                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
