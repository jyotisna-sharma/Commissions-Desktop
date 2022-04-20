using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows.Input;

namespace MyAgencyVault.VM.VMLib.Setting
{
    public class SettingCarrierCoveragesVM : BaseViewModel
    {
        public delegate void SelectedProductTypeChangedEventHandler(CoverageNickName SelectedProductType);
        public event SelectedProductTypeChangedEventHandler SelectedProductTypeChanged;

        public delegate void SelectedCoverageChangedEventHandler(DisplayedCoverage SelectedCoverage);
        public event SelectedCoverageChangedEventHandler SelectedCoverageChanged;

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
        public SettingCarrierCoveragesVM(SettingsManagerVM settingVM, bool editAllowed)
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PayorCarriers_PropertyChanged);
            settingVM.SelectedPayorChanged += new SettingsManagerVM.SelectedPayorChangedEventHandler(settingVM_SelectedPayorChanged);
            serviceClients.CoverageClient.GetDisplayedCarrierCoveragesCompleted += new EventHandler<GetDisplayedCarrierCoveragesCompletedEventArgs>(CoverageClient_GetDisplayedCarrierCoveragesCompleted);
            AllowEdit = editAllowed;
            ParentVM = settingVM; 
        }

        void PayorCarriers_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPayor":
                    //if (CurrentPayor != null) //Acme Changes
                    //    AllProducts = new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetPayorCoverages(CurrentPayor.PayorID).ToList());

                    PayorCarriers = ParentVM.SettingSharedDataVM.Carriers;
                    if (PayorCarriers.Count < 1)
                        Coverages = new ObservableCollection<Coverage>();

                    break;

                case "PayorCarriers":
                    if(PayorCarriers != null)
                        SelectedProductCarrier = PayorCarriers.FirstOrDefault();
                    break;

                case "SelectedProductCarrier":
                    if (SelectedProductCarrier == null)
                        return;
                    else
                    {
                        if (CurrentPayor != null)
                            Coverages = serviceClients.CoverageClient.GetPayorCarrierCoverages(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId);
                        //Get product and nick name on the basis of payor
                        if (Coverages != null)
                        {
                            IEnumerable<Coverage> cov = from c in Coverages
                                                        where c.NickName != null
                                                        orderby c.Name
                                                        select c;

                            Coverages = new ObservableCollection<Coverage>(cov);
                            SelectedProduct = Coverages.FirstOrDefault();

                        }
                        else
                        {
                            Coverages = new ObservableCollection<Coverage>();
                        }
                    }
                    break;

                //case "Coverages":
                //    if (Coverages != null)
                //        SelectedProduct = Coverages.FirstOrDefault();
                //    break;

                case "SelectedProduct":
                    if (SelectedProduct != null)
                    {
                        PreviousSelectedCoverageId = SelectedProduct.CoverageID;
                        if (AllProducts != null && SelectedProduct.CoverageID != Guid.Empty)
                            SelectedAllProduct = AllProducts.FirstOrDefault(s => s.CoverageID == SelectedProduct.CoverageID);

                        SavedSelectedProduct = SelectedProduct.Clone() as Coverage;

                        //if (AllProducts != null)
                        //    AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.Where(s => s.CoverageID != Guid.Empty).ToList());
                    }
                    break;
                case "SelectedProductType":
                        if(SelectedProductTypeChanged != null)
                        {
                            SelectedProductTypeChanged(SelectedProductType);
                        }
                    break;
                case "SelectedAllProduct":
                    if (SelectedAllProduct != null)
                    {
                        CoveragesNickName = serviceClients.CoverageClient.GetAllNickNames(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId, SelectedAllProduct.CoverageID);

                        if(SelectedCoverageChanged != null)
                        {
                            SelectedCoverageChanged(SelectedAllProduct);
                        }
                        //Get product and nick name on the basis of payor
                        //if (CoveragesNickName != null)
                        //{
                        //    IEnumerable<CoverageNickName> cov = from c in CoveragesNickName
                        //                                where c.NickName != null
                        //                                orderby c.NickName
                        //                                select c;

                        //    CoveragesNickName = new ObservableCollection<CoverageNickName>(cov);

                        //}
                        //else
                        //{
                        //    CoveragesNickName = new ObservableCollection<CoverageNickName>();
                        //}
                    }

                    break;
                    //case "PayorCoverages":
                    //    //if (SelectedProductCarrier == null)
                    //    //    return;
                    //    //else
                    //    //{
                    //        if (CurrentPayor != null)
                    //            Coverages = serviceClients.CoverageClient.GetPayorCoverages(CurrentPayor.PayorID);
                    //        //Get product and nick name on the basis of payor
                    //        if (Coverages != null)
                    //        {
                    //            IEnumerable<Coverage> cov = from c in Coverages
                    //                                        where c.NickName != null
                    //                                        orderby c.Name
                    //                                        select c;

                    //            Coverages = new ObservableCollection<Coverage>(cov);

                    //        }
                    //        else
                    //        {
                    //            Coverages = new ObservableCollection<Coverage>();
                    //        }
                    //    //}
                    //    break;
            }
        }

        public void FillProductTypesOnCarrierChange(Carrier SelectedCarrier)
        {
            SelectedProductCarrier = SelectedCarrier;
            if (SelectedAllProduct != null)
            {
                CoveragesNickName = serviceClients.CoverageClient.GetAllNickNames(CurrentPayor.PayorID, SelectedCarrier.CarrierId, SelectedAllProduct.CoverageID);
            }
            //else
            //{
            //    CoveragesNickName = null;
            //}

        }
        void settingVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
        }

        private ObservableCollection<Coverage> _Coverages;
        public ObservableCollection<Coverage> Coverages
        {
            get
            {
                return _Coverages;
            }
            set
            {
                _Coverages = value;
                OnPropertyChanged("Coverages");
            }
        }

        private ObservableCollection<CoverageNickName> _CoverageNickName;
        public ObservableCollection<CoverageNickName> CoveragesNickName
        {
            get
            {
                return _CoverageNickName;
            }
            set
            {
                _CoverageNickName = value;
               OnPropertyChanged("CoveragesNickName");
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

        private Coverage _SavedSelectedProduct;
        public Coverage SavedSelectedProduct
        {
            get
            {
                return _SavedSelectedProduct;
            }
            set
            {
                _SavedSelectedProduct = value;
                OnPropertyChanged("SavedSelectedProduct");
            }
        }

        private Carrier _SelectedProductCarrier;
        public Carrier SelectedProductCarrier
        {
            get
            {
                return _SelectedProductCarrier;
            }
            set
            {
                _SelectedProductCarrier = value;
                OnPropertyChanged("SelectedProductCarrier");
            }
        }

        private ObservableCollection<DisplayedCoverage> _AllProducts;
        public ObservableCollection<DisplayedCoverage> AllProducts
        {
            get
            {
                return _AllProducts;
            }
            set
            {
                _AllProducts = value;
                OnPropertyChanged("AllProducts");
            }
        }

        private DisplayedCoverage _SelectedAllProduct;
        public DisplayedCoverage SelectedAllProduct
        {
            get
            {
                return _SelectedAllProduct;
            }
            set
            {
                _SelectedAllProduct = value;
                OnPropertyChanged("SelectedAllProduct");
            }
        }

        private Coverage _SelectedProduct;
        public Coverage SelectedProduct
        {
            get
            {
                return _SelectedProduct;
            }
            set
            {
                if (value != null)
                    _SelectedProduct = value;
                else
                    _SelectedProduct = null;

                OnPropertyChanged("SelectedProduct");
            }
        }

        private CoverageNickName _SelectedProductType;
        public CoverageNickName SelectedProductType
        {
            get
            {
                return _SelectedProductType;
            }
            set
            {
                if (value != null)
                    _SelectedProductType = value;
                else
                    _SelectedProductType = null;

                OnPropertyChanged("SelectedProductType");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Guid _PreviousSelectedCoverageId;
        public Guid PreviousSelectedCoverageId
        {
            get
            {
                return _PreviousSelectedCoverageId;
            }
            set
            {
                _PreviousSelectedCoverageId = value;
                OnPropertyChanged("PreviousSelectedCoverageId");
            }
        }

        #region Private Method

        private bool BeforeOnNewProduct()
        {
            bool AllowedToCreateNewProduct = false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedProduct != null && SelectedProduct.IsNew == true)
                return false;

            if (SelectedProductCarrier == null)
                return false;
            
            if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                return false;

            if (SelectedProductCarrier != null && SelectedProductCarrier.IsGlobal == false)
                AllowedToCreateNewProduct = true;

            return AllowedToCreateNewProduct;
        }

        private void OnNewProduct()
        {
            try
            {
                SelectedProduct = new Coverage { CoverageID = Guid.Empty, IsNew = true };
                if (SelectedAllProduct != null)
                {
                    SelectedProduct.CoverageID = SelectedAllProduct.CoverageID;
                    SelectedProduct.Name = SelectedAllProduct.Name;
                }

                if (SelectedProductCarrier != null)
                    SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;

                if (CurrentPayor != null)
                    SelectedProduct.PayorID = CurrentPayor.PayorID;

                Coverages.Add(SelectedProduct);
            }
            catch (Exception)
            {
            }
        }

        private void OnSaveProduct()
        {
            try
            {
                if (SelectedProduct != null)
                {
                    OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
                    operationSet.PreviousCoverageId = PreviousSelectedCoverageId;

                    CoverageClient Client = serviceClients.CoverageClient;

                    if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(SelectedProduct.Name)))
                    {
                        if (SelectedProduct.CoverageID == Guid.Empty)
                        {
                            SelectedProduct.CoverageID = Guid.NewGuid();
                            operationSet.MainOperation = Operation.Add;
                        }
                        else
                        {
                            operationSet.MainOperation = Operation.None;
                        }

                        if (SelectedProduct.IsNew)
                            operationSet.NickNameOperation = Operation.Add;
                        else
                            operationSet.NickNameOperation = Operation.Upadte;

                        SelectedProduct.PayorID = CurrentPayor.PayorID;
                        SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;
                        SelectedProduct.UserID = RoleManager.userCredentialID;
                        SelectedProduct.LicenseeId = ParentVM.SharedVMData.SelectedLicensee.LicenseeId;
                        SelectedProduct.Name = NewProductName;

                        int covergageCount = Coverages.Where(s => s.CoverageID == SelectedProduct.CoverageID).Count();
                        if (covergageCount > 1)
                        {
                            System.Windows.MessageBox.Show("Coverage already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        ReturnStatus status = Client.AddUpdateDeleteCoverage(SelectedProduct, operationSet);

                        if (!status.IsError)
                        {
                            if (operationSet.MainOperation == Operation.Add)
                            {
                                DisplayedCoverage disProduct = AllProducts.FirstOrDefault(s => s.CoverageID == Guid.Empty && s.Name == SelectedProduct.Name);
                                disProduct.CoverageID = SelectedProduct.CoverageID;
                                PreviousSelectedCoverageId = SelectedProduct.CoverageID;
                            }

                            SavedSelectedProduct = SelectedProduct;
                            SelectedProduct.IsNew = false;
                        }
                        else
                        {
                            if (operationSet.MainOperation == Operation.Add)
                                SelectedProduct.CoverageID = Guid.Empty;
                            MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please specify the Name or NickName.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnDeleteProduct()
        {
            bool IsAllowedToDeleteProduct = false;

            try
            {
                if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                    return false;

                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    return false;

                if (SelectedProduct == null || SelectedProduct.IsNew)
                    return false;

                if (AllowEdit)
                {
                    if (SelectedProductCarrier != null && SelectedProductCarrier.IsGlobal == false)
                        IsAllowedToDeleteProduct = true;
                }
            }
            catch (Exception)
            {
            }
            return IsAllowedToDeleteProduct;
        }

        private void OnDeleteProduct()
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    if (!SelectedProduct.IsNew)
                    {
                        OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.Delete };
                        ReturnStatus status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);
                        if (!status.IsError)
                        {
                            if (status.IsCarrierOrCoverageRemoved)
                                AllProducts.Remove(AllProducts.FirstOrDefault(s => s.CoverageID == SelectedProduct.CoverageID));

                            Coverages.Remove(SelectedProduct);
                        }
                        else
                            System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Coverages.Remove(SelectedProduct);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void loadProductDataLicenseeWise()
        {
            try
            {
                CoverageClient Client = serviceClients.CoverageClient;
                if (ParentVM.SharedVMData.SelectedLicensee != null && ParentVM.SharedVMData.SelectedLicensee.LicenseeId != Guid.Empty)
                    Client.GetDisplayedCarrierCoveragesAsync(ParentVM.SharedVMData.SelectedLicensee.LicenseeId);
                else
                    Client.GetDisplayedCarrierCoveragesAsync(Guid.Empty);
            }
            catch (Exception)
            {
            }
        }

        void CoverageClient_GetDisplayedCarrierCoveragesCompleted(object sender, GetDisplayedCarrierCoveragesCompletedEventArgs e)
        {
            if (e.Error == null)
                AllProducts = e.Result ?? new ObservableCollection<DisplayedCoverage>();
        }

        private bool BeforeOnSaveProduct()
        {
            bool IsAllowedToSaveProduct = false;

            try
            {
                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    return false;

                if (RoleManager.UserAccessPermission(MasterModule.Settings) == ModuleAccessRight.Read)
                    return false;

                if (CurrentPayor.ISGlobal)
                    return IsAllowedToSaveProduct;

                if (ParentVM.SharedVMData.SelectedLicensee == null || ParentVM.SharedVMData.SelectedLicensee.LicenseeId == Guid.Empty)
                    return IsAllowedToSaveProduct;

                if (string.IsNullOrEmpty(NewProductName))
                    return IsAllowedToSaveProduct;

                if (SelectedProduct == null || string.IsNullOrWhiteSpace(NewProductName) || string.IsNullOrWhiteSpace(SelectedProduct.NickName))
                    return IsAllowedToSaveProduct;

                if (AllowEdit)
                {
                    if (SelectedProductCarrier != null)
                        IsAllowedToSaveProduct = true;
                }

            }
            catch (Exception)
            {
            }
            return IsAllowedToSaveProduct;
        }

        #endregion

        #region Property

        private string _NewProductName;
        public string NewProductName
        {
            get
            {
                return _NewProductName;
            }
            set
            {
                _NewProductName = value;
                OnPropertyChanged("NewProductName");
            }
        }

        private ICommand _ProductNameLostFocus;
        public ICommand ProductNameLostFocus
        {
            get
            {
                if (_ProductNameLostFocus == null)
                    _ProductNameLostFocus = new BaseCommand(x => OnProductNameLostFocus());
                return _ProductNameLostFocus;
            }
        }

        private void OnProductNameLostFocus()
        {
            string newCoverageName = NewProductName;
            if (AllProducts != null)
            {
                SelectedAllProduct = AllProducts.FirstOrDefault(s => s.Name == NewProductName);
                if (SelectedAllProduct == null)
                {
                    SelectedAllProduct = new DisplayedCoverage { CoverageID = Guid.Empty, Name = newCoverageName };
                    AllProducts.Add(SelectedAllProduct);
                }
            }
            else
                SelectedAllProduct = null;

            if (SelectedAllProduct == null && SelectedProduct != null)
            {
                SelectedProduct.PayorID = CurrentPayor.PayorID;
                SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;
                SelectedProduct.CoverageID = Guid.Empty;
                SelectedProduct.Name = newCoverageName;
            }
            else
            {
                if (SelectedProduct != null)
                {
                    SelectedProduct.PayorID = CurrentPayor.PayorID;
                    SelectedProduct.CoverageID = SelectedAllProduct.CoverageID;
                    SelectedProduct.Name = SelectedAllProduct.Name;

                    Coverage cov = (Coverages != null ? Coverages.FirstOrDefault(s => s.CoverageID == SelectedProduct.CoverageID) : null);
                    if (cov != null)
                        SelectedProduct.NickName = cov.NickName;
                    else
                        SelectedProduct.NickName = string.Empty;
                }
            }
        }

        private ICommand _Saveproduct = null;
        private ICommand _NewProduct = null;
        private ICommand _Deleteproduct = null;

        public ICommand SaveProduct
        {
            get
            {
                if (_Saveproduct == null)
                    _Saveproduct = new BaseCommand(param => BeforeOnSaveProduct(), param => OnSaveProduct());
                return _Saveproduct;
            }

        }
        public ICommand RemoveProduct
        {
            get
            {
                if (_Deleteproduct == null)
                    _Deleteproduct = new BaseCommand(param => BeforeOnDeleteProduct(), param => OnDeleteProduct());
                return _Deleteproduct;
            }
        }
        public ICommand NewProduct
        {
            get
            {
                if (_NewProduct == null)
                    _NewProduct = new BaseCommand(param => BeforeOnNewProduct(), param => OnNewProduct());
                return _NewProduct;
            }
        }

        private ICommand _CancelProductCmd;
        public ICommand CancelProductCmd
        {
            get
            {
                if (_CancelProductCmd == null)
                    _CancelProductCmd = new BaseCommand(param => BeforeOnCancelProduct(), param => OnCancelProduct());
                return _CancelProductCmd;
            }
        }

        private bool BeforeOnCancelProduct()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedProduct == null)
                return false;
            else
                return true;
        }

        private void OnCancelProduct()
        {
            if (SelectedProduct.IsNew)
                Coverages.Remove(SelectedProduct);
            else
            {
                SelectedProduct.CoverageID = SavedSelectedProduct.CoverageID;
                SelectedProduct.Copy(SavedSelectedProduct);
                OnPropertyChanged("SelectedProduct");
            }
        }

        #endregion
    }
}
