using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;

using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using System.Windows;
using MyAgencyVault.VM.CommonItems;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System.Windows.Forms;
using System.Windows.Data;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VM.VMLib.Configuration;
using MyAgencyVault.VM.ClientProxy;

namespace MyAgencyVault.ViewModel.VMLib
{
  public class VMConfigrationManager : BaseViewModel, IDataRefresh
  {

    #region VMInstances

    private ConfigCarrierCoveragesVM _CarrierCoveragesVM;
    public ConfigCarrierCoveragesVM CarrierCoveragesVM
    {
      get
      {
        return _CarrierCoveragesVM;
      }
      set
      {
        _CarrierCoveragesVM = value;
        OnPropertyChanged("CarrierCoveragesVM");
      }
    }

    private ConfigPayorCarriersVM _PayorCarriersVM;
    public ConfigPayorCarriersVM PayorCarriersVM
    {
      get
      {
        return _PayorCarriersVM;
      }
      set
      {
        _PayorCarriersVM = value;
        OnPropertyChanged("PayorCarriersVM");
      }
    }

    private ConfigPayorDefaultVM _PayorDefaultVM;
    public ConfigPayorDefaultVM PayorDefaultVM
    {
      get
      {
        return _PayorDefaultVM;
      }
      set
      {
        _PayorDefaultVM = value;
        OnPropertyChanged("PayorDefaultVM");
      }
    }

    private ConfigPayorContactsVM _PayorContactsVM;
    public ConfigPayorContactsVM PayorContactsVM
    {
      get
      {
        return _PayorContactsVM;
      }
      set
      {
        _PayorContactsVM = value;
        OnPropertyChanged("PayorContactsVM");
      }
    }

    private ConfigProductScheduleVM _ConfigProductScheduleVM;
    public ConfigProductScheduleVM ConfigProductScheduleVM
    {
      get
      {
        return _ConfigProductScheduleVM;
      }
      set
      {
        _ConfigProductScheduleVM = value;
        OnPropertyChanged("ConfigProductScheduleVM");
      }
    }
    private VMSharedData _SharedVMData;
    public VMSharedData SharedVMData
    {
      get
      {
        if (_SharedVMData == null)
        {
          _SharedVMData = VMSharedData.getInstance();

        }

        return _SharedVMData;
      }
    }

    private ConfigSharedDataVM _ConfigSharedDataVM;
    public ConfigSharedDataVM ConfigSharedDataVM
    {
      get
      {
        return _ConfigSharedDataVM;
      }
      set
      {
        _ConfigSharedDataVM = value;
        OnPropertyChanged("ConfigSharedDataVM");
      }
    }


    public ConfigServiceClient ConfigServiceClient;
    #endregion

    #region Delegate & Events

    public delegate void SelectedPayorChangedEventHandler(Payor SelectedPayor);
    public event SelectedPayorChangedEventHandler SelectedPayorChanged;

    #endregion

    private Action<string> popup;
    public delegate void PopupStatementDate();
    public event PopupStatementDate OnPopupStatementDate;

    public delegate void ClosePopupStatmentDate();
    public event ClosePopupStatmentDate OnClosePopusStatementDate;

    private GenericExcelMapperVM _VmGenericDataMapper = new GenericExcelMapperVM();
    public GenericExcelMapperVM VmGenericDataMapper
    {
      get { return _VmGenericDataMapper; }
    }

    public VMConfigrationManager(Action<string> popup, ScheduleGridData schData)
    {
      try
      {
        using (ServiceClients serviceClients = new ServiceClients())
        {
          this.popup = popup;
          this.PropertyChanged += new PropertyChangedEventHandler(VMConfigrationManager_PropertyChanged);
          ConfigServiceClient = new ConfigServiceClient(this);

          PayorCarriersVM = new ConfigPayorCarriersVM(this);
          CarrierCoveragesVM = new ConfigCarrierCoveragesVM(this, PayorCarriersVM);
          PayorDefaultVM = new ConfigPayorDefaultVM(this);
          PayorContactsVM = new ConfigPayorContactsVM(this);
          ConfigProductScheduleVM = new ConfigProductScheduleVM(this, schData, PayorCarriersVM, CarrierCoveragesVM);
          ConfigSharedDataVM = new ConfigSharedDataVM(this, PayorCarriersVM, CarrierCoveragesVM);

          PayorFillInfo payorFillInfo = new PayorFillInfo { IsCarriersRequired = true, IsContactsRequired = false, IsCoveragesRequired = false, IsWebsiteLoginsRequired = false, PayorStatus = PayorStatus.All };
          Payors = new ObservableCollection<ConfigDisplayedPayor>(ConfigServiceClient.ConfigDisplayPayorClient.GetConfigDisplayPayors(Guid.Empty, payorFillInfo).OrderBy(s => s.PayorName).ToList());

          ResetToFirstPayor = true;
          if (Regions != null && Regions.Count != 0)
            SelectedRegion = Regions.FirstOrDefault(s => s.RegionId == 7);

          DataEntryUsersList = serviceClients.UserClient.UsersWithRole(UserRole.DEP);

          PolicyDetailMasterData masterData = serviceClients.MasterClient.GetPolicyDetailMasterData();
          MasterIncomingPaymentTypeLst = masterData.IncomingPaymentTypes;
          SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.FirstOrDefault();
          strContent = "New";

        }
        LoadCompType();
      }
      catch
      {
      }
    }

    #region "CompType setting"

     public bool isNewClick=false;


    private PolicyIncomingPaymentType selectedMasterIncomingPaymentType;
    public PolicyIncomingPaymentType SelectedMasterIncomingPaymentType
    {
        get { return selectedMasterIncomingPaymentType == null ? new PolicyIncomingPaymentType() : selectedMasterIncomingPaymentType; }
        set { selectedMasterIncomingPaymentType = value; OnPropertyChanged("SelectedMasterIncomingPaymentType"); }
    }

    private ObservableCollection<PolicyIncomingPaymentType> masterIncomingPaymentTypeLst;
    public ObservableCollection<PolicyIncomingPaymentType> MasterIncomingPaymentTypeLst
    {
        get
        {
            return masterIncomingPaymentTypeLst;
        }
        set
        {
            masterIncomingPaymentTypeLst = value;
            OnPropertyChanged("MasterIncomingPaymentTypeLst");
        }
    }

    private ObservableCollection<CompType> _compTypeTypeLst;
    public ObservableCollection<CompType> CompTypeTypeLst
    {
        get
        {
            return _compTypeTypeLst;
        }
        set
        {
            _compTypeTypeLst = value;
            OnPropertyChanged("CompTypeTypeLst");
        }
    }

    private CompType _SelectedCompType;
    public CompType SelectedCompType
    {
        get
        {
            return _SelectedCompType;
        }
        set
        {
            _SelectedCompType = value;
            OnPropertyChanged("SelectedCompType");
        }
    }

    private string _strContent;
    public string strContent
    {
        get
        {
            return _strContent;
        }
        set
        {
            _strContent = value;
            OnPropertyChanged("strContent");
        }
    }


    private string _strCompType;
    public string strCompType
    {
        get
        {
            return _strCompType;
        }
        set
        {
            _strCompType = value;
            OnPropertyChanged("strCompType");
        }
    }

    private ICommand _SaveCompType=null;
    public ICommand SaveCompType
    {

        get
        {
            if (_SaveCompType == null)
                _SaveCompType = new BaseCommand(x => BeforeOnSaveCompType(), param => OnSaveCompType());
            return _SaveCompType;
        }

    }

    private bool BeforeOnSaveCompType()
    {
        return true;
    }

    private void OnSaveCompType()
    {
        try
        {
            if (SelectedCompType == null)
            {
                System.Windows.MessageBox.Show("CompType field is empty");
                return;
            }

            if (string.IsNullOrEmpty(SelectedCompType.Names))
            {
                System.Windows.MessageBox.Show("CompType field is empty");
                return;
            }

            else
            {
                if (IsCompTypeAvailable())
                {
                    System.Windows.MessageBox.Show("Please select unique field");                  
                    return;
                }
                else
                {
                    using (ServiceClients serviceClients = new ServiceClients())
                    {
                        if (SelectedCompType != null)
                        {
                            serviceClients.CompTypeClient.AddUpdateCompType(SelectedCompType);
                        }
                    }
                }
            }
            LoadCompType();
            isNewClick = false;
        }
        catch
        {
        }
    }

    private ICommand _DelCompType = null;
    public ICommand DelCompType
    {

        get
        {
            if (_DelCompType == null)
                _DelCompType = new BaseCommand(x => BeforeDelCompType(), param => DeleteCompType());
            return _DelCompType;
        }
    }

    private bool BeforeDelCompType()
    {
        if (SelectedCompType == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DeleteCompType()
    {
        MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to delete the seleted Comp Type", "Confirm", MessageBoxButton.YesNo);
        if (MessageBoxResult.Yes == result)
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                if (serviceClients.CompTypeClient.DeleteCompType(SelectedCompType))
                {
                    LoadCompType();
                }
            }
        }
        isNewClick = false;
    }

    private ICommand _NewCompType = null;
    public ICommand NewCompType
    {        
        get
        {
            if (_NewCompType == null)
                _NewCompType = new BaseCommand(x => OnNewCompType());
            return _NewCompType;
        }
    }

    private void OnNewCompType()
    {
        try
        {
            if (strContent == "New")
            {
                SelectedCompType = new CompType { IncomingPaymentTypeID = SelectedMasterIncomingPaymentType.PaymentTypeId, PaymentTypeName = SelectedMasterIncomingPaymentType.PaymenProcedureName, Names = string.Empty };
                CompTypeTypeLst.Add(SelectedCompType);
                isNewClick = true;
                strContent = "Cancel";
            }
            else if (strContent=="Cancel")
            {
                LoadCompType();
                strContent = "New";
                isNewClick = false;
            }

        }
        catch (Exception)
        {
        }
    }

    private void LoadCompType()
    {
        using (ServiceClients serviceClients = new ServiceClients())
        {
            CompTypeTypeLst = new ObservableCollection<CompType>(serviceClients.CompTypeClient.GetAllComptype().OrderBy(p => p.PaymentTypeName).ToList());
            SelectedCompType = CompTypeTypeLst.FirstOrDefault();
        }
    }

    private bool IsCompTypeAvailable()
    {
        bool bValue = false;
        using (ServiceClients serviceClients = new ServiceClients())
        {
            bValue = serviceClients.CompTypeClient.FindCompTypeName(SelectedCompType.Names);
        }
        return bValue;
    }

    #endregion

    private ObservableCollection<Carrier> _CarriersCount;
    public ObservableCollection<Carrier> CarriersCount
    {
      get
      {
        return _CarriersCount;
      }
      set
      {
        _CarriersCount = value;
        OnPropertyChanged("CarriersCount");
      }
    }

    private bool ResetToFirstPayor = true;
    void VMConfigrationManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      try
      {
        switch (e.PropertyName)
        {
          case "SelectedRegion":
            FilterDisplayedPayors(ResetToFirstPayor);
            break;

          case "SelectedCompType":

            if (SelectedCompType != null)
            {
                if (SelectedCompType.PaymentTypeName != null)
                {
                    SelectedMasterIncomingPaymentType = MasterIncomingPaymentTypeLst.Where(p => p.PaymenProcedureName == SelectedCompType.PaymentTypeName).FirstOrDefault();
                }
            }
            
            break;

          case "SelectedMasterIncomingPaymentType":

            if (isNewClick)
            {
                SelectedCompType.PaymentTypeName = SelectedMasterIncomingPaymentType.PaymenProcedureName;
                SelectedCompType.IncomingPaymentTypeID = selectedMasterIncomingPaymentType.PaymentTypeId;
            }
            break;

          case "Payors":
            if (Payors != null && Payors.Count != 0)
              SelectedDisplayPayor = Payors.FirstOrDefault();            
            break;

          case "SelectedPayor":
            if (SelectedPayor != null && SelectedPayor.PayorID != Guid.Empty)
            {
                using (ServiceClients serviceClients = new ServiceClients())
                {
                    //set selected region
                    SelectedPayor.Region = SelectedDisplayPayor.Region;
                    //Get carrier in selected payor
                    CarriersCount = serviceClients.CarrierClient.GetPayorCarriersOnly(SelectedPayor.PayorID); 
                    //Set single agent or general agent
                    if (CarriersCount.Count > 1)
                        //general agent
                        SelectedPayor.PayorTypeID = 1;
                    else
                        //single agent 
                        SelectedPayor.PayorTypeID = 0;

                    VmGenericDataMapper.LoadData(this);
                    //call to load Payor notes
                    LoadPayorNotes();
                }
               
            }

            if (SelectedPayorChanged != null)
              SelectedPayorChanged(SelectedPayor);
            break;

          case "SelectedDisplayPayor":
            if (SelectedDisplayPayor == null)
              return;

            if (SelectedDisplayPayor.PayorID != Guid.Empty)
              SelectedPayor = ConfigServiceClient.PayorClient.GetPayorByID(SelectedDisplayPayor.PayorID);
            else
              SelectedPayor = SelectedDisplayPayor.CreatePayor();

            SelectedDisplayPayor.PropertyChanged += new PropertyChangedEventHandler(SelectedPayor_PropertyChanged);
            SelectedDisplayPayor.Region = RightRegions.FirstOrDefault(s => s.RegionId == SelectedDisplayPayor.RegionID);

            break;

          default:
            break;
        }
      }
      catch (Exception)
      {
      }
    }

    private void FilterDisplayedPayors(bool changeCurrentPayor)
    {
      try
      {
        if (SelectedRegion != null && Payors != null)
        {
          Guid? CurrentPayorId = null;
          if (SelectedDisplayPayor != null)
            CurrentPayorId = SelectedDisplayPayor.PayorID;

          if (SelectedRegion.RegionId != 7)
          {
            DisplayedPayors = new ObservableCollection<ConfigDisplayedPayor>(Payors.Where(s => (s.RegionID == SelectedRegion.RegionId || s.RegionID == 5)).OrderBy(s => s.PayorName).ToList());
          }
          else
          {
            DisplayedPayors = new ObservableCollection<ConfigDisplayedPayor>(Payors.OrderBy(s => s.PayorName).ToList());
          }

          if (changeCurrentPayor)
          {
            if (DisplayedPayors != null && DisplayedPayors.Count > 0)
              SelectedDisplayPayor = DisplayedPayors.FirstOrDefault();
          }
          else
          {
            if (CurrentPayorId != null)
              SelectedDisplayPayor = DisplayedPayors.FirstOrDefault(s => s.PayorID == CurrentPayorId.Value);
          }
        }
      }
      catch (Exception)
      {
      }
    }


    #region Payors
    #region "Public Properties"

    /// <summary>
    /// done
    /// </summary>
    private ObservableCollection<Region> _regionList;
    public ObservableCollection<Region> Regions
    {
      get
      {
        if (_regionList == null)
        {
          using (ServiceClients serviceClients = new ServiceClients())
          {
            _regionList = serviceClients.MasterClient.GetRegionList();
          }
        }
        return _regionList;
      }
    }

    /// <summary>
    /// done
    /// </summary>
    private ObservableCollection<Region> _rightRegionList;
    public ObservableCollection<Region> RightRegions
    {
      get
      {
        if (_rightRegionList == null)
          _rightRegionList = new ObservableCollection<Region>(Regions.Where(u => u.RegionId != 7).ToList());
        return _rightRegionList;
      }
      set
      {
        _rightRegionList = value;
        OnPropertyChanged("RightRegions");
      }

    }

    /// <summary>
    /// done 
    /// </summary>
    private Region _selectedRegion;
    public Region SelectedRegion
    {
      get
      {
        return _selectedRegion;

      }
      set
      {
        _selectedRegion = value;
        OnPropertyChanged("SelectedRegion");
      }
    }
    /// <summary>
    /// 
    /// </summary>
    private ObservableCollection<ConfigDisplayedPayor> _payors = null;
    public ObservableCollection<ConfigDisplayedPayor> Payors
    {
      get
      {
        return _payors;
      }
      set
      {
        _payors = value;
        OnPropertyChanged("Payors");
      }
    }


    /// <summary>
    /// Represent the collection of displayed payors according to the selected region.
    /// </summary>
    private ObservableCollection<ConfigDisplayedPayor> _displayedPayors;
    public ObservableCollection<ConfigDisplayedPayor> DisplayedPayors
    {
      get
      {
        return _displayedPayors;
      }
      set
      {
        _displayedPayors = value;
        OnPropertyChanged("DisplayedPayors");
      }
    }

    private Payor _SelectedPayor;
    public Payor SelectedPayor
    {
      get
      {
        return _SelectedPayor;
      }
      set
      {
        _SelectedPayor = value;
        OnPropertyChanged("SelectedPayor");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private ConfigDisplayedPayor _SelectedDisplayPayor;
    public ConfigDisplayedPayor SelectedDisplayPayor
    {
      get
      {
        return _SelectedDisplayPayor;
      }
      set
      {
        _SelectedDisplayPayor = value;
        OnPropertyChanged("SelectedDisplayPayor");
      }
    }

    void SelectedPayor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "Region":
          if (SelectedDisplayPayor != null && SelectedDisplayPayor.Region != null)
          {
            SelectedPayor.PayorRegionID = SelectedDisplayPayor.RegionID = SelectedDisplayPayor.Region.RegionId;
            SelectedPayor.Region = SelectedDisplayPayor.Region;
          }
          break;
      }
    }

    #endregion

    #region Commands

    private ICommand _SavePayor = null;
    public ICommand SavePayor
    {
      get
      {
        if (_SavePayor == null)
          _SavePayor = new BaseCommand(x => BeforeOnPayorSave(), param => OnSavePayor());
        return _SavePayor;
      }

    }

    private ICommand _DeletePayor = null;
    public ICommand DeletePayor
    {
      get
      {
        if (_DeletePayor == null)
          _DeletePayor = new BaseCommand(param => BeforeOnDeletePayor(), param => OnDeletePayor());
        return _DeletePayor;
      }

    }

    private ICommand _NewPayor = null;
    public ICommand NewPayor
    {
      get
      {
        if (_NewPayor == null)
          _NewPayor = new BaseCommand(param => BeforeOnNewPayor(), param => OnNewPayor());
        return _NewPayor;
      }

    }

    #endregion

    #region Methods

    private bool BeforeOnNewPayor()
    {
      if (SelectedPayor != null && SelectedPayor.PayorID == Guid.Empty)
        return false;

      return true;
    }

    private void OnNewPayor()
    {
      try
      {
        if (SelectedRegion.RegionId != 7)
        {
          SelectedDisplayPayor = new ConfigDisplayedPayor { PayorID = Guid.Empty, RegionID = SelectedRegion.RegionId };
        }
        else
        {
          if (SelectedDisplayPayor != null && SelectedPayor != null)
          {
            SelectedDisplayPayor = new ConfigDisplayedPayor { PayorID = Guid.Empty, RegionID = SelectedDisplayPayor.RegionID };
          }
          else
          {
            SelectedDisplayPayor = new ConfigDisplayedPayor { PayorID = Guid.Empty, RegionID = 0 };
          }
        }
          //set empty payor notes when create new payor 
        RTPayorNote = string.Empty;
      }
      catch (Exception)
      {
      }
    }

    private bool BeforeOnPayorSave()
    {
      bool isAllowedToSavePayor = true;

      try
      {
        if (SelectedPayor == null)
          return isAllowedToSavePayor = false;

        if (SelectedPayor != null && (string.IsNullOrWhiteSpace(SelectedPayor.PayorName) || string.IsNullOrWhiteSpace(SelectedPayor.NickName)))
          return isAllowedToSavePayor = false;

        if (SelectedPayor != null && SelectedPayor.Region == null)
          return isAllowedToSavePayor = false;
      }
      catch (Exception)
      {
      }

      return isAllowedToSavePayor;
    }

    private void OnSavePayor()
    {
        try
        {
            ReturnStatus statusMessage = null;

            if (SelectedPayor != null)
            {
                if (SelectedPayor.PayorName != null)
                {
                    if (SelectedPayor.NickName != null)
                    {
                        statusMessage = ConfigServiceClient.PayorClient.ValdateLocalPayor(SelectedPayor, SelectedPayor.PayorName.Trim(), SelectedPayor.NickName.Trim());
                    }
                }
            }
            
            if (statusMessage.IsError)
            {
                string str = statusMessage.ErrorMessage.ToString();

                DialogResult result = System.Windows.Forms.MessageBox.Show(str + Environment.NewLine + " Do you want to continue ? ", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    savePayor();
                }
                else
                {
                    return;
                }

            }
            else
            {
                //Call to load without 
                savePayor();               
                _SharedVMData.IsLoadPayor = true;
            }
        }
        catch
        {
        }
    }

    private void savePayor()
    {
        try
        {
            SelectedPayor.ISGlobal = true;
            SelectedPayor.UserID = RoleManager.userCredentialID;
            SelectedPayor.PayorLicensee = RoleManager.LicenseeId;

            SelectedPayor.PayorName = SelectedPayor.PayorName.Trim();
            SelectedPayor.NickName = SelectedPayor.NickName.Trim();

            bool IsAddCase = false;
            ReturnStatus status = null;

            if (SelectedPayor.PayorID == Guid.Empty)
            {
                SelectedPayor.PayorID = Guid.NewGuid();
                status = ConfigServiceClient.PayorClient.AddUpdateDeletePayor(SelectedPayor, Operation.Add);
                IsAddCase = true;
            }
            else
            {
                status = ConfigServiceClient.PayorClient.AddUpdateDeletePayor(SelectedPayor, Operation.Upadte);
                IsAddCase = false;

            }

            if (!status.IsError)
            {
                ConfigDisplayedPayor configPayor = null;
                if (IsAddCase)
                {
                    configPayor = SelectedPayor.CreateConfigDisplayPayor();
                    Payors.Add(configPayor);
                    SelectedDisplayPayor = configPayor;
                }
                else
                {
                    configPayor = Payors.FirstOrDefault(s => s.PayorID == SelectedPayor.PayorID);
                    configPayor.Copy(SelectedPayor);
                }

                if (SelectedRegion.RegionId != 7)
                {
                    if (SelectedDisplayPayor.RegionID != SelectedRegion.RegionId)
                        ResetToFirstPayor = false;
                    SelectedRegion = Regions.FirstOrDefault(s => s.RegionId == SelectedDisplayPayor.RegionID);
                }
                else
                    FilterDisplayedPayors(false);

                if (VMInstances.SettingManager != null)
                    VMInstances.SettingManager.RefreshRequired = true;

                if (VMInstances.PayorToolVM != null)
                    VMInstances.PayorToolVM.RefreshRequired = true;

                if (VMInstances.DeuVM != null)
                    VMInstances.DeuVM.RefreshRequired = true;



                #region"Call to Save Notes"
                SavePayorNotes();
                #endregion

            }
            else
            {
                if (IsAddCase)
                    SelectedPayor.PayorID = Guid.Empty;

                System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    //private void OnSavePayor()
    //{
    //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
    //    try
    //    {
    //        SelectedPayor.ISGlobal = true;
    //        SelectedPayor.UserID = RoleManager.userCredentialID;
    //        SelectedPayor.PayorLicensee = RoleManager.LicenseeId;

    //        SelectedPayor.PayorName = SelectedPayor.PayorName.Trim();
    //        SelectedPayor.NickName = SelectedPayor.NickName.Trim();

    //        ReturnStatus status = null;
    //        bool IsAddCase = false;

    //        if (SelectedPayor.PayorID == Guid.Empty)
    //        {
    //            SelectedPayor.PayorID = Guid.NewGuid();
    //            status = ConfigServiceClient.PayorClient.AddUpdateDeletePayor(SelectedPayor, Operation.Add);
    //            IsAddCase = true;
    //        }
    //        else
    //        {
    //            status = ConfigServiceClient.PayorClient.AddUpdateDeletePayor(SelectedPayor, Operation.Upadte);
    //            IsAddCase = false;
    //        }

    //        if (!status.IsError)
    //        {
    //            ConfigDisplayedPayor configPayor = null;
    //            if (IsAddCase)
    //            {
    //                configPayor = SelectedPayor.CreateConfigDisplayPayor();
    //                Payors.Add(configPayor);
    //                SelectedDisplayPayor = configPayor;
    //            }
    //            else
    //            {
    //                configPayor = Payors.FirstOrDefault(s => s.PayorID == SelectedPayor.PayorID);
    //                configPayor.Copy(SelectedPayor);
    //            }

    //            if (SelectedRegion.RegionId != 7)
    //            {
    //                if (SelectedDisplayPayor.RegionID != SelectedRegion.RegionId)
    //                    ResetToFirstPayor = false;
    //                SelectedRegion = Regions.FirstOrDefault(s => s.RegionId == SelectedDisplayPayor.RegionID);
    //            }
    //            else
    //                FilterDisplayedPayors(false);

    //            if (VMInstances.SettingManager != null)
    //                VMInstances.SettingManager.RefreshRequired = true;

    //            if (VMInstances.PayorToolVM != null)
    //                VMInstances.PayorToolVM.RefreshRequired = true;

    //            if (VMInstances.DeuVM != null)
    //                VMInstances.DeuVM.RefreshRequired = true;


    //            #region"Call to Save Notes"
    //            SavePayorNotes();                
    //            #endregion

    //        }
    //        else
    //        {
    //            if (IsAddCase)
    //                SelectedPayor.PayorID = Guid.Empty;

    //            System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }
    //    finally
    //    {
    //        Mouse.OverrideCursor = null;
    //    }

        
    //}

    private bool BeforeOnDeletePayor()
    {
      if (SelectedDisplayPayor == null)
        return false;

      return true;
    }

    private void OnDeletePayor()
    {
      Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
      try
      {
        if (SelectedDisplayPayor != null)
        {
          MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
          if (result == MessageBoxResult.Yes)
          {
            if (SelectedDisplayPayor.PayorID != Guid.Empty)
            {
              ReturnStatus status = ConfigServiceClient.PayorClient.DeletePayor(SelectedDisplayPayor.PayorID);
              if (!status.IsError)
              {
                  if (SharedVMData.CachedPayorLists.Count > 0)
                  {
                      if (SharedVMData.SelectedLicensee != null)
                      {
                          if (SharedVMData.SelectedLicensee.LicenseeId != null)
                          {
                              SharedVMData.CachedPayorLists.Remove(SharedVMData.SelectedLicensee.LicenseeId);
                          }
                      }
                  }

                  if (VMInstances.SettingManager != null)
                      VMInstances.SettingManager.RefreshRequired = true;

                  if (VMInstances.PayorToolVM != null)
                      VMInstances.PayorToolVM.RefreshRequired = true;

                  if (VMInstances.DeuVM != null)
                      VMInstances.DeuVM.RefreshRequired = true;

              }
              else
              {
                  //if payor form is avalable for payor then need to delete payor form first
                  System.Windows.MessageBox.Show(status.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                  return;
              }
            }

            Payors.Remove(Payors.FirstOrDefault(s => s.PayorID == SelectedDisplayPayor.PayorID));
            FilterDisplayedPayors(true);

          }
        }
      }
      finally
      {
        Mouse.OverrideCursor = null;
      }
      //check if no payor available then selected payor will be null
      if (DisplayedPayors.Count <= 0)
        SelectedPayor = null;

    }

    #endregion

    #endregion

    #region Carriers
    #region Public Properties

    private string _editable;
    public string Editable
    {
      get
      {
        return _editable;
      }
      set
      {
        _editable = value;
        OnPropertyChanged("Editable");
      }
    }

    #endregion

    #endregion

    #region Data Entry Persons

    #region Public Properties
    private ObservableCollection<User> _dataEntryUsersList;
    public ObservableCollection<User> DataEntryUsersList
    {
      get
      {
        return _dataEntryUsersList;
      }
      set
      {
        _dataEntryUsersList = value;
        OnPropertyChanged("DataEntryUsersList");
      }
    }

    private User _selectedDataEntryUser;
    public User SelectedDataEntryUser
    {
      get
      {
        return _selectedDataEntryUser;
      }
      set
      {
        _selectedDataEntryUser = value;
        OnPropertyChanged("SelectedDataEntryUser");
      }
    }

    #endregion

    #region Command
    /// <summary>
    /// 
    /// </summary>
    private ICommand _newComdDEU;
    public ICommand NewDEU
    {
      get
      {
        if (_newComdDEU == null)
          _newComdDEU = new BaseCommand(x => BeforeNewCommandDEU(), x => NewCommandDEU());
        return _newComdDEU;
      }

    }

    /// <summary>
    /// 
    /// </summary>
    private ICommand _saveCmdDEU;
    public ICommand SaveDEU
    {
      get
      {
        if (_saveCmdDEU == null)
        {
          _saveCmdDEU = new BaseCommand(x => ValidateDataEntry(), x => InsertUpdateDEU());
        }
        return _saveCmdDEU;
      }

    }

    /// <summary>
    /// 
    /// </summary>
    private ICommand _deleteCmdDEU;
    public ICommand DeleteDUE
    {
      get
      {
        if (_deleteCmdDEU == null)
        {
          _deleteCmdDEU = new BaseCommand(x => BeforeDeleteDEU(), x => DeleteDEU());
        }
        return _deleteCmdDEU;
      }

    }

    private bool BeforeNewCommandDEU()
    {
      if (SelectedDataEntryUser != null && SelectedDataEntryUser.UserCredentialID == Guid.Empty)
        return false;
      else
        return true;
    }

    private bool BeforeDeleteDEU()
    {
      if (SelectedDataEntryUser == null)
        return false;
      else
        return true;
    }

    #endregion

    #region Methods
    private void InsertUpdateDEU()
    {
      try
      {
        using (ServiceClients serviceClients = new ServiceClients())
        {
          if (DataEntryUsersList == null)
            return;

          if (SelectedDataEntryUser.UserName.Length < 6 || SelectedDataEntryUser.Password.Length < 6)          
              return;          

          SelectedDataEntryUser.UserCredentialID = SelectedDataEntryUser.UserCredentialID == Guid.Empty ? Guid.NewGuid() : SelectedDataEntryUser.UserCredentialID;
          SelectedDataEntryUser.Role = UserRole.DEP;
          if (!string.IsNullOrEmpty(SelectedDataEntryUser.UserName) && !string.IsNullOrEmpty(SelectedDataEntryUser.UserName))
          {
            SelectedDataEntryUser.FullName = SelectedDataEntryUser.LastName + " " + SelectedDataEntryUser.FirstName;
            serviceClients.UserClient.AddUpdateUser(SelectedDataEntryUser);

            if (!DataEntryUsersList.Exist(s => s.UserCredentialID == SelectedDataEntryUser.UserCredentialID))
              DataEntryUsersList.Add(SelectedDataEntryUser);
          }
        }
      }
      catch (Exception)
      {
      }
    }

    private void NewCommandDEU()
    {
      try
      {
        SelectedDataEntryUser = new User { UserCredentialID = Guid.Empty };
        DataEntryUsersList.Add(SelectedDataEntryUser);
      }
      catch (Exception)
      {
      }
    }


    private void DeleteDEU()
    {
      try
      {
        using (ServiceClients serviceClients = new ServiceClients())
        {
          if (SelectedDataEntryUser != null)
          {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
              if (SelectedDataEntryUser.UserCredentialID != Guid.Empty)
                serviceClients.UserClient.DeleteUserInfo(SelectedDataEntryUser);

              DataEntryUsersList.Remove(SelectedDataEntryUser);
              if (DataEntryUsersList.Count == 0)
                SelectedDataEntryUser = null;
              else
                SelectedDataEntryUser = DataEntryUsersList.FirstOrDefault();
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }
    private bool ValidateDataEntry()
    {
        if (SelectedDataEntryUser != null && !string.IsNullOrEmpty(SelectedDataEntryUser.UserName) && !string.IsNullOrEmpty(SelectedDataEntryUser.Password))
        {
            if (SelectedDataEntryUser.UserName.Length < 6 || SelectedDataEntryUser.Password.Length < 6)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    #endregion
    #endregion

    #region "Commands"

    public void DoOpenStatementDate()
    {
      if (OnPopupStatementDate != null)
      {
        OnPopupStatementDate();
      }
    }

    #endregion

    #region Setting

    #region Constructor

    private DateTime? _SelectedDate;
    public DateTime? EffectiveFromDate
    {
      get
      {
        return _SelectedDate;
      }
      set
      {
        _SelectedDate = value;
        OnPropertyChanged("EffectiveFromDate");
      }
    }

    #endregion

    #region Private Methods
    bool _flag;

    void _ObjPayorView_CurrentChanged(object sender, EventArgs e)
    {
      _flag = false;
      OnPropertyChanged("CurrentPayor");
    }

    #endregion


    #endregion

    #region"Add and get payor notes"
    private ICommand _RTPayorNoteLostFocus;
    public ICommand RTPayorNoteLostFocus
    {
        get
        {
            if (_RTPayorNoteLostFocus == null)
                _RTPayorNoteLostFocus = new BaseCommand(param => BeforeOnPayorNotesLostFocus(), param => OnPayorNotesLostFocus());
            return _RTPayorNoteLostFocus;
        }
    }

    string strConfigNotes = string.Empty;
    private void OnPayorNotesLostFocus()
    {
        //set rich text box value into string for saving on button click
        if (RTPayorNote != null)
        {
            strConfigNotes = RTPayorNote.ToString();
        }
    }

    
    private void SavePayorNotes()
    {
        if (SelectedDisplayPayor != null)
        {
            if (SelectedDisplayPayor.PayorID != null )
            {
                if (SelectedDisplayPayor.PayorID != new Guid())
                {
                    using (ServiceClients serviceClients = new ServiceClients())
                    {
                        if (SharedVMData != null)
                        {
                            if (SharedVMData.SelectedLicensee != null)
                            {
                                if (SharedVMData.SelectedLicensee.LicenseeId != null)
                                {
                                    PayorSource _PayorSource = new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = SelectedDisplayPayor.PayorID };
                                    _PayorSource.ConfigNotes = strConfigNotes;
                                    serviceClients.PayorSourceClient.AddPayorConfigSource(_PayorSource);
                                    RTPayorNote = strConfigNotes;

                                }

                            }

                        }

                    }
                }
            }

        }
    }

    private void LoadPayorNotes()
    {
        using (ServiceClients serviceClients = new ServiceClients())
        {
            if (SharedVMData != null)
            {
                if (SharedVMData.SelectedLicensee != null)
                {
                    if (SharedVMData.SelectedLicensee.LicenseeId != null)
                    {
                        PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = SelectedDisplayPayor.PayorID });
                        RTPayorNote = _PayorSource.ConfigNotes;
                    }
                }
                else
                {
                    RTPayorNote = null;
                }
            }
        }
    }


    private bool BeforeOnPayorNotesLostFocus()
    {
        if (string.IsNullOrEmpty(RTPayorNote))
            return false;
        else
            return true;

    }

    private string _RTPayorNote;
    public string RTPayorNote
    {
        get
        {
            return _RTPayorNote;
        }
        set
        {
            _RTPayorNote = value;
            OnPropertyChanged("RTPayorNote");
        }
    }


    #endregion

    public void Refresh()
    {

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

  }

  public class ScheduleGridData : INotifyPropertyChanged
  {
    private string _RateColumnName = "Commission %";
    public string RateColumnName
    {
      get { return _RateColumnName; }
      set
      {
        _RateColumnName = value;
        OnPropertyChanged("RateColumnName");
      }
    }

    private Visibility _FromRangeVisibility = Visibility.Visible;
    public Visibility FromRangeVisibility
    {
      get { return _FromRangeVisibility; }
      set
      {
        _FromRangeVisibility = value;
        OnPropertyChanged("FromRangeVisibility");
      }
    }

    private Visibility _ToRangeVisibility = Visibility.Visible;
    public Visibility ToRangeVisibility
    {
      get { return _ToRangeVisibility; }
      set
      {
        _ToRangeVisibility = value;
        OnPropertyChanged("ToRangeVisibility");
      }
    }

    public void OnPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  #region Validations

  public class ContactValidate : GlobalPayorContact, IDataErrorInfo
  {
    #region IDataErrorInfo Members

    public string Error
    {
      get
      {
        StringBuilder error = new StringBuilder();
        ValidationResults results = Validation.ValidateFromConfiguration<ContactValidate>(this, "ContactValidation");
        foreach (ValidationResult result in results)
        {
          error.AppendLine(result.Message);
        }

        return error.ToString();
      }
    }
    public string this[string columnName]
    {
      get
      {
        ValidationResults results = Validation.ValidateFromConfiguration<ContactValidate>(this, "ContactValidation");
        foreach (ValidationResult result in results)
        {
          if (result.Key == columnName)
          {
            return result.Message;
          }
        }

        return string.Empty;
      }
    }

    #endregion
  }
}

  #endregion


