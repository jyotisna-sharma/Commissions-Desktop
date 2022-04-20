using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigCarrierCoveragesVM : BaseViewModel
    {
        #region Delegate & Event

        public delegate void CoveragesChangedEventHandler(ObservableCollection<Coverage> Coverages);
        public event CoveragesChangedEventHandler CoveragesChanged;

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
        private VMConfigrationManager ParentVM;
        public ConfigCarrierCoveragesVM(VMConfigrationManager configManager, ConfigPayorCarriersVM CarrierVM)
        {
            ParentVM = configManager;
            ParentVM.SelectedPayorChanged += new VMConfigrationManager.SelectedPayorChangedEventHandler(ParentVM_SelectedPayorChanged);
            CarrierVM.CarriersChanged += new ConfigPayorCarriersVM.CarriersChangedEventHandler(CarrierVM_CarriersChanged);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentVM_PropertyChanged);
            //before
            //AllProducts = serviceClients.CoverageClient.GetDisplayedCarrierCoverages(Guid.Empty);
            //change
            AllProducts =new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetDisplayedCarrierCoverages(Guid.Empty).OrderBy(c=>c.Name).ToList());
            if (AllProducts != null && AllProducts.Count != 0)
                SelectedAllProduct = AllProducts.FirstOrDefault();
        }

        void CarrierVM_CarriersChanged(ObservableCollection<Carrier> Carriers)
        {
            PayorCarriers = Carriers;
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
                    PayorCarriers = ParentVM.ConfigSharedDataVM.Carriers;
                    if (PayorCarriers.Count < 1)                   
                        Coverages = new ObservableCollection<Coverage>();
                   
                    break;
                case "PayorCarriers":
                    if (PayorCarriers != null)
                        SelectedProductCarrier = PayorCarriers.FirstOrDefault();
                    break;

                case "SelectedProductCarrier":
                    if (SelectedProductCarrier == null)
                        return;
                    else
                    {
                        if (CurrentPayor != null)
                        {
                            Coverages = serviceClients.CoverageClient.GetPayorCarrierCoverages(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId);
                            if (Coverages != null)
                            {
                                IEnumerable<Coverage> cov = from c in Coverages
                                                            where c.NickName != null
                                                            orderby c.Name
                                                            select c;

                                Coverages = new ObservableCollection<Coverage>(cov);

                            }

                        }
                    }
                    break;

                case "Coverages":
                    if (Coverages != null)
                        SelectedProduct = Coverages.FirstOrDefault();

                    if (CoveragesChanged != null)
                        CoveragesChanged(Coverages);
                    break;
                case "SelectedProduct":
                    if (SelectedProduct != null)
                    {
                        PreviousSelectedCoverageId = SelectedProduct.CoverageID;
                        previousCovarageNickName = SelectedProduct.NickName;

                        SavedSelectedProduct = SelectedProduct.Clone() as Coverage;

                        if (SelectedProduct.CoverageID != Guid.Empty)
                            SelectedAllProduct = AllProducts.FirstOrDefault(s => s.CoverageID == SelectedProduct.CoverageID);

                        //if (AllProducts != null)
                        //    AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.Where(s => s.CoverageID != Guid.Empty).ToList());
                    }
                    break;
            }
        }

        #region Collection and Selected Property

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

        private ObservableCollection<DisplayedCoverage> _AllProducts = null;
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

        private DisplayedCoverage _SelectedAllProduct = null;
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

        private Coverage _SelectedProduct;
        public Coverage SelectedProduct
        {
            get
            {
                return _SelectedProduct;
            }
            set
            {
                _SelectedProduct = value;
                OnPropertyChanged("SelectedProduct");
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

        private string _previousCovarageNickName;
        public string previousCovarageNickName
        {
            get
            {
                return _previousCovarageNickName;
            }
            set
            {
                _previousCovarageNickName = value;
                OnPropertyChanged("previousCovarageNickName");
            }
        }

        //private Coverage _SelectedCarrierWiseProduct = null;
        //public Coverage SelectedCarrierWiseProduct
        //{
        //    get
        //    {
        //        return _SelectedCarrierWiseProduct;
        //    }
        //    set
        //    {
        //        _SelectedCarrierWiseProduct = value;
        //        OnPropertyChanged("SelectedCarrierWiseProduct");
        //    }
        //}

        /// <summary>
        /// Selected schedule carreir on the schedule tab.
        /// </summary>
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

        #endregion

        #region Commands

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

        private ICommand _saveproduct;
        public ICommand SaveProductCmd
        {
            get
            {
                if (_saveproduct == null)
                    _saveproduct = new BaseCommand(param => BeforeOnSaveProduct(), param => OnSaveProduct());

                return _saveproduct;
            }

        }

        private ICommand _newProduct;
        public ICommand NewProduct
        {
            get
            {
                if (_newProduct == null)
                    _newProduct = new BaseCommand(param => BeforeOnNewProduct(), param => OnNewProduct());
                return _newProduct;
            }
        }

        private ICommand _deleteproduct;
        public ICommand DeleteProductCmd
        {
            get
            {
                if (_deleteproduct == null)
                    _deleteproduct = new BaseCommand(param => BeforeOnDeleteProduct(), param => OnDeleteProduct());

                return _deleteproduct;
            }
        }

        private ICommand _deleteproductType;
        public ICommand DeleteProductType
        {
            get
            {
                if (_deleteproductType == null)
                    _deleteproductType = new BaseCommand(param => BeforeOnDeleteProductType(), param => OnDeleteProductType());

                return _deleteproductType;
            }
        }

        #endregion

        #region Methods

        private void OnProductNameLostFocus()
        {
            string newCoverageName = NewProductName;
            try
            {
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
            catch (Exception)
            {
            }
        }

        private void OnCancelProduct()
        {
            try
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
            catch (Exception)
            {
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

        private bool BeforeOnSaveProduct()
        {
            bool IsAllowedToSaveProduct = true;

            try
            {
                if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                    IsAllowedToSaveProduct = false;

                if (string.IsNullOrWhiteSpace(NewProductName))
                    IsAllowedToSaveProduct = false;

                if (SelectedProduct == null || string.IsNullOrWhiteSpace(NewProductName) || string.IsNullOrWhiteSpace(SelectedProduct.NickName))
                    IsAllowedToSaveProduct = false;
            }
            catch (Exception)
            {
            }

            return IsAllowedToSaveProduct;
        }

        //private void OnSaveProduct()
        //{
        //    try
        //    {
        //        if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(SelectedProduct.Name)))
        //        {
        //            OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
        //            operationSet.PreviousCoverageId = PreviousSelectedCoverageId;

        //            if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(NewProductName)))
        //            {
        //                if (SelectedProduct.CoverageID == Guid.Empty)
        //                {
        //                    SelectedProduct.CoverageID = Guid.NewGuid();
        //                    operationSet.MainOperation = Operation.Add;
        //                }
        //                else
        //                {
        //                    operationSet.MainOperation = Operation.None;
        //                }

        //                if (SelectedProduct.IsNew)
        //                    operationSet.NickNameOperation = Operation.Add;
        //                else
        //                    operationSet.NickNameOperation = Operation.Upadte;

        //                SelectedProduct.PayorID = CurrentPayor.PayorID;
        //                SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;
        //                SelectedProduct.UserID = RoleManager.userCredentialID;
        //                SelectedProduct.LicenseeId = RoleManager.LicenseeId;
        //                SelectedProduct.Name = NewProductName.Trim();
        //                SelectedProduct.NickName = SelectedProduct.NickName.Trim();
        //                SelectedProduct.IsGlobal = true;


        //                int covergageCount = Coverages.Where(s => s.CoverageID == SelectedProduct.CoverageID).Count();
        //                if (covergageCount > 1)
        //                {
        //                    System.Windows.MessageBox.Show("Coverage already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    return;
        //                }

        //                ReturnStatus status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);

        //                if (!status.IsError)
        //                {
        //                    if (operationSet.MainOperation == Operation.Add)
        //                    {
        //                        DisplayedCoverage disProduct = AllProducts.FirstOrDefault(s => s.CoverageID == Guid.Empty && s.Name == SelectedProduct.Name);
        //                        disProduct.CoverageID = SelectedProduct.CoverageID;
        //                        PreviousSelectedCoverageId = SelectedProduct.CoverageID;
        //                    }

        //                    SavedSelectedProduct = SelectedProduct;
        //                    SelectedProduct.IsNew = false;
        //                }
        //                else
        //                {
        //                    if (operationSet.MainOperation == Operation.Add)
        //                        SelectedProduct.CoverageID = Guid.Empty;
        //                    System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }
        //            }
        //            else
        //            {
        //                System.Windows.MessageBox.Show("Please specify the Name or NickName.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

               
        //        //To sort by product name
        //        if (AllProducts.Count > 0)
        //            AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
        //    }
        //    catch (Exception ex)
        //    {
                
        //    }
        //}

        //private void OnSaveProduct()
        //{
        //    try
        //    {
        //        if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(SelectedProduct.Name)))
        //        {
        //            ReturnStatus status = null;
        //            OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
        //            operationSet.PreviousCoverageId = PreviousSelectedCoverageId;
        //            operationSet.previousCovarageNickName = previousCovarageNickName;

        //            if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(NewProductName)))
        //            {
        //                if (SelectedProduct.CoverageID == Guid.Empty)
        //                {
        //                    SelectedProduct.CoverageID = Guid.NewGuid();
        //                    operationSet.MainOperation = Operation.Add;
        //                }
        //                else
        //                {
        //                    operationSet.MainOperation = Operation.None;
        //                }

        //                if (SelectedProduct.IsNew)
        //                {
        //                    operationSet.NickNameOperation = Operation.Add;
        //                }
        //                else
        //                {
        //                    operationSet.NickNameOperation = Operation.Upadte;
        //                }

                       
        //                int covergageCount = Coverages.Where(s => s.CoverageID == SelectedProduct.CoverageID).Count();
        //                if (covergageCount > 1)
        //                {
        //                    System.Windows.MessageBox.Show("Coverage already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    return;
        //                }
        //                //Compare Nick name when going to update
        //                if (operationSet.NickNameOperation == Operation.Upadte)
        //                {
        //                    List<string> lstNickNameToDelete = CompareNickNames(previousCovarageNickName.Trim(), SelectedProduct.NickName.Trim());
        //                    foreach (var nickName in lstNickNameToDelete)
        //                    {
        //                        if (!string.IsNullOrEmpty(nickName.Trim()))
        //                        {
        //                            serviceClients.CoverageClient.DeleteNickName(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId, operationSet.PreviousCoverageId, nickName.Trim());
        //                        }
        //                    }
        //                }
                        
        //                SelectedProduct.PayorID = CurrentPayor.PayorID;
        //                SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;
        //                SelectedProduct.UserID = RoleManager.userCredentialID;
        //                SelectedProduct.LicenseeId = RoleManager.LicenseeId;
        //                SelectedProduct.Name = NewProductName.Trim();
        //                SelectedProduct.IsGlobal = true;

        //                //Split selected nick name 
        //                string strNickName = SelectedProduct.NickName.Trim();
        //                string[] arNickname = null;
        //                //bool isNickExists = false;
        //                if (strNickName.Contains(";"))
        //                {
        //                    arNickname = strNickName.Split(';');
        //                }

        //                //If there are  multiple nick names
        //                if (arNickname != null)
        //                {
        //                    foreach (var strNickValue in arNickname)
        //                    {
        //                        if (!string.IsNullOrEmpty(strNickValue.Trim()))
        //                        {
        //                            SelectedProduct.NickName = strNickValue.Trim();
        //                            status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);
        //                            if (status != null)
        //                            {
        //                                if (!status.IsError)
        //                                {
        //                                    if (operationSet.MainOperation == Operation.Add)
        //                                    {
        //                                        DisplayedCoverage disProduct = AllProducts.FirstOrDefault(s => s.CoverageID == Guid.Empty && s.Name == SelectedProduct.Name);
        //                                        disProduct.CoverageID = SelectedProduct.CoverageID;
        //                                        PreviousSelectedCoverageId = SelectedProduct.CoverageID;
        //                                        previousCovarageNickName = SelectedProduct.NickName;
        //                                    }

        //                                    SavedSelectedProduct = SelectedProduct;
        //                                    SelectedProduct.IsNew = false;
        //                                }
        //                                else
        //                                {
        //                                    if (operationSet.MainOperation == Operation.Add)
        //                                    {
        //                                        SelectedProduct.CoverageID = Guid.Empty;
        //                                    }
                                            
        //                                }
        //                            }
        //                        }
        //                    }
        //                    //Refersh with multiple nick name
        //                    RefershMultipleNickName();
        //                }
        //                else
        //                {
        //                    //If there are no multiple nick names
        //                    if (!string.IsNullOrEmpty(SelectedProduct.NickName.Trim()))
        //                    {
        //                        SelectedProduct.NickName = SelectedProduct.NickName.Trim();
        //                        status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);
        //                        if (status != null)
        //                        {
        //                            if (!status.IsError)
        //                            {
        //                                if (operationSet.MainOperation == Operation.Add)
        //                                {
        //                                    DisplayedCoverage disProduct = AllProducts.FirstOrDefault(s => s.CoverageID == Guid.Empty && s.Name == SelectedProduct.Name);
        //                                    disProduct.CoverageID = SelectedProduct.CoverageID;
        //                                    PreviousSelectedCoverageId = SelectedProduct.CoverageID;
        //                                    previousCovarageNickName = SelectedProduct.NickName;
        //                                }

        //                                SavedSelectedProduct = SelectedProduct;
        //                                SelectedProduct.IsNew = false;
        //                            }
        //                            else
        //                            {
        //                                if (operationSet.MainOperation == Operation.Add)
        //                                    SelectedProduct.CoverageID = Guid.Empty;
        //                                System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                System.Windows.MessageBox.Show("Please specify the Name or NickName.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //        //To sort by product name
        //        if (AllProducts.Count > 0)
        //            AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        private void OnSaveProduct()
        {
            try
            {
                if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(SelectedProduct.Name)))
                {
                    ReturnStatus status = null;
                    OperationSet operationSet = new OperationSet { MainOperation = Operation.None, NickNameOperation = Operation.None };
                    operationSet.PreviousCoverageId = PreviousSelectedCoverageId;
                    operationSet.previousCovarageNickName = previousCovarageNickName;

                    if (!(string.IsNullOrEmpty(SelectedProduct.NickName) || string.IsNullOrEmpty(NewProductName)))
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
                        {
                            operationSet.NickNameOperation = Operation.Add;
                        }
                        else
                        {
                            operationSet.NickNameOperation = Operation.Upadte;
                        }

                        int covergageCount = Coverages.Where(s => s.CoverageID == SelectedProduct.CoverageID).Count();
                        if (covergageCount > 1)
                        {
                            System.Windows.MessageBox.Show("Coverage already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        //Vaidate Nick Name before save the nick name
                        string outNickName = string.Empty;
                        if (ValidateNickName(Coverages, SelectedProduct.NickName, out outNickName))
                        {
                            System.Windows.MessageBox.Show("Coverage nick name (" + outNickName + ") already exist.", outNickName, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        //Compare Nick name when going to update
                        if (operationSet.NickNameOperation == Operation.Upadte)
                        {
                            List<string> lstNickNameToDelete = CompareNickNames(previousCovarageNickName.Trim(), SelectedProduct.NickName.Trim());
                            foreach (var nickName in lstNickNameToDelete)
                            {
                                if (!string.IsNullOrEmpty(nickName.Trim()))
                                {
                                    status = serviceClients.CoverageClient.DeleteNickName(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId, operationSet.PreviousCoverageId, nickName.Trim());
                                    if (status != null)
                                    {
                                        if (status.IsError)
                                        {
                                            System.Windows.MessageBox.Show(status.ErrorMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }

                        SelectedProduct.PayorID = CurrentPayor.PayorID;
                        SelectedProduct.CarrierID = SelectedProductCarrier.CarrierId;
                        SelectedProduct.UserID = RoleManager.userCredentialID;
                        SelectedProduct.LicenseeId = RoleManager.LicenseeId;
                        SelectedProduct.Name = NewProductName.Trim();
                        SelectedProduct.IsGlobal = true;

                        string[] arNicknames = null;                      
                        if (SelectedProduct.NickName.Contains(";"))
                        {
                            arNicknames = SelectedProduct.NickName.Split(';');
                        }

                        //If there are  multiple nick names
                        if (arNicknames != null)
                        {
                            foreach (var strNickValue in arNicknames)
                            {
                                if (!string.IsNullOrEmpty(strNickValue.Trim()))
                                {
                                    SelectedProduct.NickName = strNickValue.Trim();
                                    //Save nick name
                                    status = SaveNickName(operationSet);
                                }
                            }
                            //Refersh with multiple nick name
                            RefershMultipleNickName();
                        }
                        else
                        {
                            //If there are no multiple nick names
                            if (!string.IsNullOrEmpty(SelectedProduct.NickName.Trim()))
                            {
                                SelectedProduct.NickName = SelectedProduct.NickName.Trim();
                                //Don't need this code
                                //status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);
                                status = SaveNickName(operationSet);
                                if (status != null)
                                {
                                    if (status.IsError)
                                    {
                                        System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                               
                            }
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please specify the Name or NickName.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                //To sort by product name
                if (AllProducts.Count > 0)
                    AllProducts = new ObservableCollection<DisplayedCoverage>(AllProducts.OrderBy(s => s.Name).ToList());
            }
            catch (Exception)
            {

            }
        }

        private ReturnStatus SaveNickName(OperationSet operationSet)
        {
            ReturnStatus status = null;
            try
            {
                status = serviceClients.CoverageClient.AddUpdateDeleteCoverage(SelectedProduct, operationSet);
                if (status != null)
                {
                    if (!status.IsError)
                    {
                        if (operationSet.MainOperation == Operation.Add)
                        {
                            DisplayedCoverage disProduct = AllProducts.FirstOrDefault(s => s.CoverageID == Guid.Empty && s.Name == SelectedProduct.Name);
                            disProduct.CoverageID = SelectedProduct.CoverageID;
                            PreviousSelectedCoverageId = SelectedProduct.CoverageID;
                            previousCovarageNickName = SelectedProduct.NickName;
                        }

                        SavedSelectedProduct = SelectedProduct;
                        SelectedProduct.IsNew = false;
                    }
                    else
                    {
                        if (operationSet.MainOperation == Operation.Add)
                        {
                            SelectedProduct.CoverageID = Guid.Empty;
                        }

                    }
                }
            }
            catch (Exception)
            {
            }
            return status;            
        }
    
        private void RefershMultipleNickName()
        {
            if (CurrentPayor != null)
            {
                Coverages = serviceClients.CoverageClient.GetPayorCarrierCoverages(CurrentPayor.PayorID, SelectedProductCarrier.CarrierId);
                if (Coverages != null)
                {
                    IEnumerable<Coverage> cov = from c in Coverages
                                                where c.NickName != null
                                                orderby c.Name
                                                select c;

                    Coverages = new ObservableCollection<Coverage>(cov);

                }

            }
        }

        private List<string> CompareNickNames(string strPreviousNickName, string strSurrentSelectedNickNames)
        {
            //List<string> lstPreviousNickName = strPreviousNickName.Split(';').ToList();
            //List<string> lstCurrentSeletedNickNames = strSurrentSelectedNickNames.Split(';').ToList();

            List<string> lstPreviousNickName = new List<string>();
            List<string> lstCurrentSeletedNickNames = new List<string>();
            List<string> PreviousNickNameNotSelectedNickName = new List<string>();
            //Split selected nick name            
            string[] arPreviousNickName = null;
            string[] arCurrentNickName = null;

            try
            {
                if (strPreviousNickName.Contains(";"))
                {
                    arPreviousNickName = strPreviousNickName.Split(';');
                    foreach (var item in arPreviousNickName)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            lstPreviousNickName.Add(item.Trim());
                        }
                    }
                }

                if (strPreviousNickName.Contains(";"))
                {
                    arCurrentNickName = strSurrentSelectedNickNames.Split(';');
                    foreach (var item in arCurrentNickName)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            lstCurrentSeletedNickNames.Add(item.Trim());
                        }
                    }
                }

                PreviousNickNameNotSelectedNickName = lstPreviousNickName.Except(lstCurrentSeletedNickNames).ToList();
            }
            catch (Exception)
            {

            }

            return PreviousNickNameNotSelectedNickName;

            //List<string> SeletedNickNamesNotFirst = lstCurrentSeletedNickNames.Except(lstPreviousNickName).ToList();

        }

        private bool ValidateNickName(ObservableCollection<Coverage> objCoverage, string strSelectedNickName,out string outNickName)
        {
            bool isFound = false;
            outNickName = string.Empty;

            objCoverage = new ObservableCollection<Coverage>(objCoverage.Where(s => s.CoverageID != SelectedProduct.CoverageID));

            string[] arSelectedNickName = strSelectedNickName.Split(';');
            foreach (var item in objCoverage)
            {
                string strNickName = item.NickName;
                if (!string.IsNullOrEmpty(strNickName.Trim()))
                {
                    string[] arValue = strNickName.Split(';');

                    foreach (var itemNickValue in arValue)
                    {
                        if (!string.IsNullOrEmpty(itemNickValue.Trim()))
                        {
                            foreach (var itemSelecteNickName in arSelectedNickName)
                            {
                                if (itemSelecteNickName.Trim().ToLower() == itemNickValue.Trim().ToLower())
                                {
                                    isFound = true;
                                    outNickName = itemNickValue.Trim();
                                    //Break inner foreach
                                    break;
                                }
                            }
                        }
                        //Break Outer foreach
                        if (isFound)
                        {
                            break;
                        }

                    }
                }
            }
            return isFound;
        }

        private bool BeforeOnDeleteProduct()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedProduct == null || SelectedProduct.IsNew)
                return false;
            else
                return true;
        }

        private bool BeforeOnDeleteProductType()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedProduct == null || SelectedProduct.IsNew)
                return false;
            else
                return true;
        }

        private void OnDeleteProductType()
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to delete Product Type?", "Delete Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (SelectedProduct != null)
                    {
                        if (!string.IsNullOrEmpty(SelectedProduct.NickName))
                        {
                            ReturnStatus ojReturnStatus = new ReturnStatus();
                            ojReturnStatus = serviceClients.CoverageClient.DeleteProductType(SelectedProduct.PayorID, SelectedProduct.CarrierID, SelectedProduct.CoverageID, SelectedProduct.NickName);
                            if (ojReturnStatus.IsError)
                            {
                                MessageBox.Show(ojReturnStatus.ErrorMessage.ToString(), "Information", MessageBoxButton.OK);
                            }
                            else
                            {
                                MessageBox.Show("Deleted sucessfully", "Information", MessageBoxButton.OK);
                                //Get all product when delete the product successfull                            
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
            }
        }

        private void OnDeleteProduct()
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to delete Product?", "Delete Warning", MessageBoxButton.YesNo);
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
                    //Get all product when delete the product successful
                    AllProducts = new ObservableCollection<DisplayedCoverage>(serviceClients.CoverageClient.GetDisplayedCarrierCoverages(Guid.Empty).OrderBy(c => c.Name).ToList());
                    if (AllProducts.Count > 0)
                        SelectedAllProduct = AllProducts.FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
        }

        private bool BeforeOnNewProduct()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (SelectedProductCarrier == null)
            {
                if (Coverages != null && Coverages.Count > 0)
                    Coverages.Clear();

                return false;
            }

            if (Coverages == null || Coverages.Count == 0)
                return true;
            else
            {
                if (SelectedProduct != null && SelectedProduct.IsNew == true)
                    return false;
                else
                    return true;
            }
        }

        private void OnNewProduct()
        {
            try
            {
                SelectedProduct = new Coverage { PayorID = CurrentPayor.PayorID, CoverageID = Guid.Empty, IsNew = true };

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

        #endregion
    }
}
