using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Forms;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows;

namespace MyAgencyVault.VM.VMLib.Setting
{
    public class SettingPayorCarriersVM : BaseViewModel
    {
        #region Delegate & Event

        public delegate void CarriersChangedEventHandler(ObservableCollection<Carrier> Carriers);
        public event CarriersChangedEventHandler CarriersChanged;

        public delegate void SelectedCarrierChangedEventHandler(Carrier SelectedCarrier);
        public event SelectedCarrierChangedEventHandler SelectedCarrierChanged;

        #endregion

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

        private bool AllowEdit = false;
        private SettingsManagerVM ParentVM;
        private ServiceClients _ServiceClients;
        private ServiceClients serviceClients
        {
          get
          {
            if(_ServiceClients==null)
            {
              _ServiceClients = new ServiceClients();
            }
            return _ServiceClients;
          }
        }
        public SettingPayorCarriersVM(SettingsManagerVM settingVM, bool editAllowed)
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PayorCarriers_PropertyChanged);
            serviceClients.CarrierClient.GetDispalyedCarriersWithCompleted += new EventHandler<GetDispalyedCarriersWithCompletedEventArgs>(CarrierClient_GetDispalyedCarriersWithCompleted);
            settingVM.SelectedPayorChanged += new SettingsManagerVM.SelectedPayorChangedEventHandler(settingVM_SelectedPayorChanged);
            AllowEdit = editAllowed;
            ParentVM = settingVM;
        }


        void PayorCarriers_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "CurrentPayor":
                        if (CurrentPayor != null)
                        {
                            Carriers = serviceClients.CarrierClient.GetPayorCarriersOnly(CurrentPayor.PayorID);
                            
                        }
                        break;
                    case "Carriers":
                        if (Carriers != null)
                            SelectedCarrier = Carriers.FirstOrDefault();

                        if (CarriersChanged != null)
                            CarriersChanged(Carriers);
                        break;
                    case "SelectedCarrier":
                        if (SelectedCarrier != null)
                        {
                            PreviousSelectedCarrierId = SelectedCarrier.CarrierId;
                            if (AllCarriers != null && SelectedCarrier.CarrierId != Guid.Empty)
                                SelectedAllCarrier = AllCarriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId);
                            
                            SavedSelectedCarrier = SelectedCarrier.Clone() as Carrier;
                            SelectedCarrierChanged(SavedSelectedCarrier);
                        }
                        break;
                }
            }
            catch (Exception ex) 
            {
              //  ActionLogger.Logger.WriteLog("Exception occurs while fetching details"+ex.Message, true);
            }
        }

        void settingVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
        }

        private ObservableCollection<Carrier> _Carriers;
        public ObservableCollection<Carrier> Carriers
        {
            get 
            {
                return _Carriers;
            }
            set
            {
                _Carriers = value;
                OnPropertyChanged("Carriers");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Carrier _SavedSelectedCarrier;
        public Carrier SavedSelectedCarrier
        {
            get
            {
                return _SavedSelectedCarrier;
            }
            set
            {
                _SavedSelectedCarrier = value;
                OnPropertyChanged("SavedSelectedCarrier");
            }
        }

        private Carrier _SelectedCarrier;
        public Carrier SelectedCarrier
        {
            get
            {
                return _SelectedCarrier;
            }
            set
            {
                _SelectedCarrier = value;
                OnPropertyChanged("SelectedCarrier");
            }
        }

        private ObservableCollection<DisplayedCarrier> _AllCarriers;
        public ObservableCollection<DisplayedCarrier> AllCarriers
        {
            get
            {
                return _AllCarriers;
            }
            set
            {
                _AllCarriers = value;
                OnPropertyChanged("AllCarriers");
            }
        }

        private DisplayedCarrier _SelectedAllCarrier = null;
        public DisplayedCarrier SelectedAllCarrier
        {
            get
            {
                return _SelectedAllCarrier;
            }
            set
            {
                _SelectedAllCarrier = value;
                OnPropertyChanged("SelectedAllCarrier");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Guid _PreviousSelectedCarrierId;
        public Guid PreviousSelectedCarrierId
        {
            get
            {
                return _PreviousSelectedCarrierId;
            }
            set
            {
                _PreviousSelectedCarrierId = value;
                OnPropertyChanged("PreviousSelectedCarrierId");
            }
        }

        #region Commands

        private BaseCommand _NewCarrier;
        public ICommand NewCarrier
        {
            get
            {
                if (_NewCarrier == null)

                    _NewCarrier = new BaseCommand(param => BeforeOnNewCarrier(), param => OnNewCarrier());
                return _NewCarrier;

            }
        }

        private BaseCommand _SaveCarrier;
        public ICommand SaveCarrier
        {
            get
            {
                if (_SaveCarrier == null)
                    _SaveCarrier = new BaseCommand(param => BeforeOnSaveCarrier(), param => OnSaveCarrier());
                return _SaveCarrier;
            }
        }

        private BaseCommand _DeleteCarrier;
        public ICommand DeleteCarrier
        {
            get
            {
                if (_DeleteCarrier == null)
                    _DeleteCarrier = new BaseCommand(param => BeforeOnDeleteCarrier(), param => OnDeleteCarrier());
                return _DeleteCarrier;
            }
        }

        /// <summary>
        /// 
        /// </summary>        
        private ICommand _Cancelcommand;
        public ICommand Cancelcommand
        {
            get
            {
                if (_Cancelcommand == null)
                {
                    _Cancelcommand = new BaseCommand(x => BeforeOnCancelCarrier(), x => OnCancelCarrier());
                }
                return _Cancelcommand;
            }

        }

        #endregion

        #region Private functions

        private void OnCancelCarrier()
        {
            try
            {
                if (SelectedCarrier.IsNew)
                    Carriers.Remove(SelectedCarrier);
                else
                {
                    SelectedCarrier.CarrierId = SavedSelectedCarrier.CarrierId;
                    SelectedCarrier.Copy(SavedSelectedCarrier);
                    OnPropertyChanged("SelectedCarrier");
                }
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnCancelCarrier()
        {
            try
            {
                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    return false;

                if (SelectedCarrier == null)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool BeforeOnNewCarrier()
        {
            bool IsAllowedToNewCarrier = false;

            try
            {
                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    return false;

                if (SelectedCarrier != null && SelectedCarrier.IsNew == true)
                    return false;

                if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                    return false;

                if (AllowEdit)
                {
                    if (CurrentPayor != null && CurrentPayor.ISGlobal == false)
                        IsAllowedToNewCarrier = true;
                }
            }
            catch (Exception)
            {
            }
            return IsAllowedToNewCarrier;
        }

        private void OnNewCarrier()
        {
            try
            {
                SelectedCarrier = new Carrier() { IsTrackIncomingPercentage = true, IsTrackMissingMonth = true, IsNew = true };
                SelectedCarrier.PayerId = CurrentPayor.PayorID;

                if (SelectedAllCarrier != null)
                {
                    SelectedCarrier.CarrierId = SelectedAllCarrier.CarrierId;
                    SelectedCarrier.CarrierName = SelectedAllCarrier.CarrierName;
                }

                if (Carriers == null)
                    Carriers = new ObservableCollection<Carrier>();

                Carriers.Add(SelectedCarrier);
            }
            catch (Exception)
            {
            }
        }

        private void OnSaveCarrier()
        {
            CarrierClient Client = serviceClients.CarrierClient;
            OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
            operationSet.PreviousCarrierId = PreviousSelectedCarrierId;

            try
            {
                if (SelectedCarrier != null)
                {
                    if (SelectedCarrier.CarrierId == Guid.Empty)
                    {
                        SelectedCarrier.CarrierId = Guid.NewGuid();
                        operationSet.MainOperation = Operation.Add;
                    }
                    else
                    {
                        operationSet.MainOperation = Operation.None;
                    }

                    if(SelectedCarrier.IsNew)
                        operationSet.NickNameOperation = Operation.Add;
                    else
                        operationSet.NickNameOperation = Operation.Upadte;


                    SelectedCarrier.PayerId = CurrentPayor.PayorID;
                    SelectedCarrier.UserID = RoleManager.userCredentialID;
                    SelectedCarrier.LicenseeId = ParentVM.SharedVMData.SelectedLicensee.LicenseeId;
                    SelectedCarrier.CarrierName = NewCarrierName;

                    int carrierCount = Carriers.Where(s => s.CarrierId == SelectedCarrier.CarrierId).Count();
                    if (carrierCount > 1)
                    {
                        System.Windows.MessageBox.Show("Carrier already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ReturnStatus status = Client.AddUpdateDeleteCarrier(SelectedCarrier, operationSet);

                    if (!status.IsError)
                    {
                        if (operationSet.MainOperation == Operation.Add)
                        {
                            PreviousSelectedCarrierId = SelectedCarrier.CarrierId;
                            DisplayedCarrier disCarrier = AllCarriers.FirstOrDefault(s => s.CarrierId == Guid.Empty && s.CarrierName == SelectedCarrier.CarrierName);
                            disCarrier.CarrierId = SelectedCarrier.CarrierId;

                            if (Carriers.Count > 1)
                                CurrentPayor.PayorTypeID = 1;
                        }
                        SavedSelectedCarrier = SelectedCarrier;
                        SelectedCarrier.IsNew = false;
                       
                    }
                    else
                    {
                        if (operationSet.MainOperation == Operation.Add)
                            SelectedCarrier.CarrierId = Guid.Empty;
                        System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    ////change by vinod 27102011
                    if (AllCarriers.Count > 0)
                        AllCarriers = new ObservableCollection<DisplayedCarrier>(AllCarriers.OrderBy(o => o.CarrierName).ToList());
                }
               
            }

            catch (Exception)
            {
            }
        }

        private bool BeforeOnDeleteCarrier()
        {
            bool IsAllowedToDeleteCarrier = false;

            try
            {
                if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                    return false;

                if (SelectedCarrier == null || SelectedCarrier.IsNew)
                    return false;

                if (AllowEdit)
                {
                    if (CurrentPayor != null && CurrentPayor.ISGlobal == false)
                        IsAllowedToDeleteCarrier = true;
                }
            }
            catch (Exception)
            {
            }
            return IsAllowedToDeleteCarrier;
        }

        private void OnDeleteCarrier()
        {
            try
            {
                CarrierClient client = serviceClients.CarrierClient;
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.Delete };
                    ReturnStatus status = client.AddUpdateDeleteCarrier(SelectedCarrier, operationSet);
                    if (!status.IsError)
                    {
                        if (status.IsCarrierOrCoverageRemoved)
                            AllCarriers.Remove(AllCarriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId));

                        Carriers.Remove(Carriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId));
                    }
                    else
                        System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnSaveCarrier()
        {
            bool IsAllowToSaveCarrier = false;

            try
            {
                if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                    return false;

                if (ParentVM.SharedVMData.SelectedLicensee == null || ParentVM.SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                    return IsAllowToSaveCarrier;

                if (string.IsNullOrEmpty(NewCarrierName))
                    return IsAllowToSaveCarrier;

                if (SelectedCarrier == null || string.IsNullOrWhiteSpace(NewCarrierName) || string.IsNullOrWhiteSpace(SelectedCarrier.NickName))
                    return IsAllowToSaveCarrier;

                if (AllowEdit)
                {
                    if (CurrentPayor != null && CurrentPayor.ISGlobal == false)
                        IsAllowToSaveCarrier = true;
                }
            }
            catch (Exception)
            {
            }

            return IsAllowToSaveCarrier;
        }


        private ICommand _CarrierNameLostFocus;
        public ICommand CarrierNameLostFocus
        {
            get
            {
                if (_CarrierNameLostFocus == null)
                    _CarrierNameLostFocus = new BaseCommand(x => OnCarrierNameLostFocus());
                return _CarrierNameLostFocus;
            }
        }

        private void OnCarrierNameLostFocus()
        {
            try
            {
                string newCarrierName = NewCarrierName;
                if (AllCarriers != null)
                {
                    SelectedAllCarrier = AllCarriers.FirstOrDefault(s => s.CarrierName == NewCarrierName);
                    if (SelectedAllCarrier == null)
                    {
                        SelectedAllCarrier = new DisplayedCarrier { CarrierId = Guid.Empty, CarrierName = newCarrierName };
                        AllCarriers.Add(SelectedAllCarrier);
                    }
                }
                else
                    SelectedAllCarrier = null;

                if (SelectedCarrier != null && (SelectedAllCarrier == null || (SelectedAllCarrier != null && SelectedAllCarrier.CarrierId == Guid.Empty)))
                {
                    SelectedCarrier.PayerId = CurrentPayor.PayorID;
                    SelectedCarrier.CarrierId = Guid.Empty;
                    SelectedCarrier.CarrierName = newCarrierName;
                    SelectedCarrier.IsTrackIncomingPercentage = true;
                    SelectedCarrier.IsTrackMissingMonth = true;
                }
                else
                {
                    if (SelectedCarrier != null)
                    {
                        SelectedCarrier.PayerId = CurrentPayor.PayorID;
                        SelectedCarrier.CarrierName = SelectedAllCarrier.CarrierName;
                        SelectedCarrier.CarrierId = SelectedAllCarrier.CarrierId;
                        Carrier car = Carriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId);
                        if (car != null)
                            SelectedCarrier.NickName = car.NickName;
                        else
                            SelectedCarrier.NickName = string.Empty;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private string _NewCarrierName;
        public string NewCarrierName
        {
            get
            {
                return _NewCarrierName;
            }
            set
            {
                _NewCarrierName = value;
                OnPropertyChanged("NewCarrierName");
            }
        }

        public void LoadCarrierLicenseeWise()
        {
            try
            {
                CarrierClient Client = serviceClients.CarrierClient;
                if (ParentVM.SharedVMData.SelectedLicensee != null && ParentVM.SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                    Client.GetDispalyedCarriersWithAsync(ParentVM.SharedVMData.SelectedLicensee.LicenseeId, true);
                else
                    Client.GetDispalyedCarriersWithAsync(Guid.Empty, true);
            }
            catch (Exception)
            {
            }

        }

        void CarrierClient_GetDispalyedCarriersWithCompleted(object sender, GetDispalyedCarriersWithCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //before
                //AllCarriers = e.Result ?? new ObservableCollection<DisplayedCarrier>();

                //vinod
                AllCarriers =new ObservableCollection<DisplayedCarrier>(e.Result.OrderBy(o =>o.CarrierName).ToList()) ?? new ObservableCollection<DisplayedCarrier>();
            }
        }

        #endregion
    }
}
