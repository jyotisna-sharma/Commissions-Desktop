using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigPayorCarriersVM : BaseViewModel
    {
        #region Delegate & Event

        public delegate void CarriersChangedEventHandler(ObservableCollection<Carrier> Carriers);
        public event CarriersChangedEventHandler CarriersChanged;

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
        private VMConfigrationManager _ParentVM;
        public ConfigPayorCarriersVM(VMConfigrationManager configManager)
        {
            _ParentVM = configManager;
            _ParentVM.SelectedPayorChanged += new VMConfigrationManager.SelectedPayorChangedEventHandler(ParentVM_SelectedPayorChanged);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ConfigPayorCarriersVM_PropertyChanged);
            //before
            //AllCarriers = serviceClients.CarrierClient.GetDispalyedCarriersWith(Guid.Empty, true);
            //change
            AllCarriers = new ObservableCollection<DisplayedCarrier>(serviceClients.CarrierClient.GetDispalyedCarriersWith(Guid.Empty, true).OrderBy(o => o.CarrierName).ToList());
            if (AllCarriers != null && AllCarriers.Count != 0)
                SelectedAllCarrier = AllCarriers.FirstOrDefault();
        }

        void ConfigPayorCarriersVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPayor":
                    if (CurrentPayor != null)//check null 
                    {
                        Carriers = _ParentVM.ConfigServiceClient.CarrierClient.GetPayorCarriersOnly(CurrentPayor.PayorID);
                        
                    }
                    break;

                case "Carriers":
                    if (Carriers != null)
                        SelectedCarrier = Carriers.FirstOrDefault();
                    
                        if(CarriersChanged != null)
                            CarriersChanged(Carriers);
                    break;
                case "SelectedCarrier":
                    if(SelectedCarrier != null)
                        PreviousSelectedCarrierId = SelectedCarrier.CarrierId;

                    if (AllCarriers != null && SelectedCarrier != null)
                    {
                        if (SelectedCarrier.CarrierId != Guid.Empty)
                            SelectedAllCarrier = AllCarriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId);
                        SavedSelectedCarrier = SelectedCarrier.Clone() as Carrier;
                    }
                    break;
            }
        }

        void ParentVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
        }


        /// <summary>
        /// 
        /// </summary>
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

        private string _CarrierName;
        public string CarrierName
        {
            get { return _CarrierName; }
            set
            {
                _CarrierName = value;
                OnPropertyChanged("CarrierName");
            }
        }

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
                if (value != null)
                    _SavedSelectedCarrier = value;
                else
                    _SavedSelectedCarrier = null;

                OnPropertyChanged("SavedSelectedCarrier");
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

        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// This is carrier list is all global carriers.
        /// </summary>
        private ObservableCollection<DisplayedCarrier> _AllCarriers;
        public ObservableCollection<DisplayedCarrier> AllCarriers
        {
            get { return _AllCarriers; }
            set
            {
                _AllCarriers = value;
                OnPropertyChanged("AllCarriers");
            }
        }

        /// <summary>
        /// Selected carrier on the carrier tab.
        /// </summary>
        private DisplayedCarrier _SelectedAllCarrier;
        public DisplayedCarrier SelectedAllCarrier
        {
            get { return _SelectedAllCarrier; }
            set
            {
                _SelectedAllCarrier = value;
                OnPropertyChanged("SelectedAllCarrier");
            }
        }

        #region Carrier Commands
        /// <summary>
        /// 
        /// </summary>
        private ICommand _newComdCarrier;
        public ICommand NewCarrierCmd
        {
            get
            {
                if (_newComdCarrier == null)
                {
                    _newComdCarrier = new BaseCommand(x => BeforeOnNewCarrier(), x => OnNewCarrier());
                }
                return _newComdCarrier;
            }

        }

        /// <summary>
        /// 
        /// </summary>        
        private ICommand _saveCmdCarrier;
        public ICommand SaveCarrierCmd
        {
            get
            {
                if (_saveCmdCarrier == null)
                {
                    _saveCmdCarrier = new BaseCommand(x => BeforeOnSaveCarrier(), x => OnSaveCarrier());
                }
                return _saveCmdCarrier;
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

        /// <summary>
        /// 
        /// </summary>
        private ICommand _deleteCmdCarrier;
        public ICommand DeleteCarrierCmd
        {
            get
            {
                if (_deleteCmdCarrier == null)
                {
                    _deleteCmdCarrier = new BaseCommand(x => BeforeOnDeleteCarrier(), x => OnDeleteCarrier());
                }
                return _deleteCmdCarrier;
            }

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

        #endregion

        #region Carrier Command Methods

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

        private bool BeforeOnDeleteCarrier()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedCarrier == null || SelectedCarrier.IsNew)
                return false;
            else
                return true;
        }

        private void OnDeleteCarrier()
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (!SelectedCarrier.IsNew)
                    {
                        OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.Delete };
                        ReturnStatus status = serviceClients.CarrierClient.AddUpdateDeleteCarrier(SelectedCarrier, operationSet);
                        if (!status.IsError)
                        {
                            if (status.IsCarrierOrCoverageRemoved)
                                AllCarriers.Remove(AllCarriers.FirstOrDefault(s => s.CarrierId == SelectedCarrier.CarrierId));

                            Carriers.Remove(SelectedCarrier);

                            if (CarriersChanged != null)
                                CarriersChanged(Carriers);
                        }
                        else
                            System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Carriers.Remove(SelectedCarrier);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnNewCarrier()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (Carriers == null || Carriers.Count == 0)
                return true;
            else
            {
                if(SelectedCarrier != null && SelectedCarrier.IsNew == true)
                    return false;
                else
                    return true;
            }
        }

        private void OnCancelCarrier()
        {
            try
            {
                if (SelectedCarrier.IsNew)
                    Carriers.Remove(SelectedCarrier);
                else
                {
                    if (SavedSelectedCarrier != null)
                    {
                        SelectedCarrier.CarrierId = SavedSelectedCarrier.CarrierId;
                        SelectedCarrier.Copy(SavedSelectedCarrier);
                    }
                }
            }
            catch (Exception)
            {
            }

            //if (AllCarriers != null)
            //    AllCarriers = new ObservableCollection<DisplayedCarrier>(AllCarriers.Where(s => s.CarrierId != Guid.Empty).ToList());
        }

        private bool BeforeOnCancelCarrier()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedCarrier == null)
                return false;
            else
                return true;
        }

        private void OnNewCarrier()
        {
            try
            {
                SelectedCarrier = new Carrier { PayerId = CurrentPayor.PayorID, IsTrackIncomingPercentage = true, IsTrackMissingMonth = true, IsNew = true };

                if (SelectedAllCarrier != null)
                {
                    SelectedCarrier.CarrierName = SelectedAllCarrier.CarrierName;
                    SelectedCarrier.CarrierId = SelectedAllCarrier.CarrierId;
                }

                if (Carriers == null)
                    Carriers = new ObservableCollection<Carrier>();

                Carriers.Add(SelectedCarrier);

                if (CarriersChanged != null)
                    CarriersChanged(Carriers);
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnSaveCarrier()
        {
            //Set IsAllowToSaveCarrier true             
            bool IsAllowToSaveCarrier = true;

            try
            {
                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    IsAllowToSaveCarrier = false;

                if (string.IsNullOrWhiteSpace(NewCarrierName))
                    IsAllowToSaveCarrier = false;

                if (SelectedCarrier == null || string.IsNullOrWhiteSpace(NewCarrierName) || string.IsNullOrWhiteSpace(SelectedCarrier.NickName))
                    IsAllowToSaveCarrier = false;
            }
            catch (Exception)
            {
            }

            return IsAllowToSaveCarrier;
        }

        private void OnSaveCarrier()
        {
            try
            {
                CarrierClient Client = serviceClients.CarrierClient;
                OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
                operationSet.PreviousCarrierId = PreviousSelectedCarrierId;

                using (Client)
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

                        if (SelectedCarrier.IsNew)
                            operationSet.NickNameOperation = Operation.Add;
                        else
                            operationSet.NickNameOperation = Operation.Upadte;

                        SelectedCarrier.PayerId = CurrentPayor.PayorID;
                        SelectedCarrier.UserID = RoleManager.userCredentialID;
                        SelectedCarrier.LicenseeId = RoleManager.LicenseeId;
                        SelectedCarrier.CarrierName = NewCarrierName.Trim();
                        SelectedCarrier.NickName = SelectedCarrier.NickName.Trim();
                        SelectedCarrier.IsGlobal = true;

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

                    }

                }
                
                //sort by carrier
                if (AllCarriers.Count > 0)
                    AllCarriers = new ObservableCollection<DisplayedCarrier>(AllCarriers.OrderBy(s => s.CarrierName).ToList());
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
