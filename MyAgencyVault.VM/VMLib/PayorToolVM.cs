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
using MyAgencyVault.VM.CommonItems;
using System.Threading;
using System.Windows;
using MyAgencyVault.VM.VMLib.PayorForm;
using System.IO;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib.PayorForm.Evalutor;

namespace MyAgencyVault.ViewModel
{
    /// <summary>
    /// Define the field type.
    /// </summary>
    public enum FieldType
    {
        Date = 1,
        Numeric = 2,
        Text = 3
    }

    public class CanvasDroppedField
    {
        public int FieldId{get;set;}
        public string FieldName { get; set; }
        public int  ControlX{get;set;}
        public int ControlY { get; set; }
    }

    public class PayorToolVM : BaseViewModel, IDataRefresh
    {
        static MastersClient objLog = new MastersClient();
        private string ImageFolderOnWebDev = "Images/";
        private bool _RefreshRequired = false;
        public bool RefreshRequired
        {
            get { return _RefreshRequired; }
            set { _RefreshRequired = value; }
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
        #region Payors

        private ObservableCollection<DisplayedPayor> _Payors;
        public ObservableCollection<DisplayedPayor> Payors
        {
            get
            {
                if (_Payors != null)
                    return _Payors;
                return null;
            }
            set
            {
                _Payors = value;
                OnPropertyChanged("Payors");
            }
        }

        private bool SelectedPayorChangeFromCopyDuplicate;
        private DisplayedPayor _PreviousSelectedPayor;
        public DisplayedPayor PreviousSelectedPayor
        {
            get
            {
                return _PreviousSelectedPayor;
            }
            set
            {
                _PreviousSelectedPayor = value;
            }
        }

        private DisplayedPayor _selectedPayor;
        public DisplayedPayor SelectedPayor
        {
            get
            {
                return _selectedPayor;
            }
            set
            {
                _selectedPayor = value;

                if (_selectedPayor != null)
                {
                    if(m_DesignerCanvas != null)
                        m_DesignerCanvas.DeselectAll();
                
                    if (SelectedPayorChangeFromCopyDuplicate)
                        LoadPayorToolData(PreviousSelectedPayor, SelectedDuplicatePayor,false);
                    else
                        LoadPayorToolData(_selectedPayor, null,false);

                    if (SelectedPayor != null)
                    {
                        LoadPayorTempalate(SelectedPayor.PayorID);
                    }

                    OnPropertyChanged("SelectedPayor");
                }
            }
        }

        private bool isPayorPresent;
        public bool IsPayorPresent
        {
            get { return isPayorPresent; }
            set 
            {
                isPayorPresent = value;
                OnPropertyChanged("IsPayorPresent");
            }
        }

        
        private bool _isFieldPresent;
        public bool IsFieldPresent
        {
            get { return _isFieldPresent; }
            set
            {
                _isFieldPresent = value;
                OnPropertyChanged("IsFieldPresent");
            }
        }

        
        //private bool _IsCheckAmountUpload;
        //public bool IsCheckAmountUpload
        //{
        //    get { return _IsCheckAmountUpload; }
        //    set
        //    {
        //        _IsCheckAmountUpload = value;
        //        OnPropertyChanged("IsCheckAmountUpload");
        //    }
        //}
        #endregion

        

        #region Payor Tool Data

        private PayorToolData _payorToolData;
        public PayorToolData PayToolData
        {
            get { return _payorToolData; }
            set
            {
                _payorToolData = value;
                OnPropertyChanged("PayorToolData");
            }
        }

        #endregion

        #region Duplicate payor

        private List<DisplayedPayor> _DuplicatePayors;
        public List<DisplayedPayor> DuplicatePayors
        {
            get
            {
                if (_DuplicatePayors != null)
                    return _DuplicatePayors;
                return null;
            }
            set
            {
                _DuplicatePayors = value;
                OnPropertyChanged("DuplicatePayors");
            }
        }

        private DisplayedPayor _selectedDuplicatePayor = null;
        public DisplayedPayor SelectedDuplicatePayor
        {
            get
            {
                return _selectedDuplicatePayor;
            }
            set
            {
                _selectedDuplicatePayor = value;

                if (_selectedDuplicatePayor != null)
                {
                    OnPropertyChanged("SelectedDuplicatePayor");
                }
            }
        }

        #endregion

        #region Payor Tool

        private List<PayorTool> SavedPayorToolCollection;

        private PayorTool _CurrentPayorToolInfo;
        public PayorTool CurrentPayorToolInfo
        {
            get
            {
                if (_CurrentPayorToolInfo == null)
                    return new PayorTool();
                return _CurrentPayorToolInfo;
            }
            set
            {

                _CurrentPayorToolInfo = value == null ? new PayorTool() : value;
                OnPropertyChanged("CurrentPayorToolInfo");
            }
        }

        #endregion

        #region Payor Tool Fields

        private ObservableCollection<PayorToolField> _PayorToolFieldsPropperty = new ObservableCollection<PayorToolField>();
        public ObservableCollection<PayorToolField> PayorToolFieldsProperty
        {
            get 
            {
                return _PayorToolFieldsPropperty;
            }
            set
            {
                _PayorToolFieldsPropperty = value;
                OnPropertyChanged("PayorToolFieldsPropperty");
            }
        }

        private PayorToolField _CurrentPayorFieldProperty;
        public PayorToolField CurrentPayorFieldProperty
        {
            get
            {
                return _CurrentPayorFieldProperty;
            }
            set
            {
                if (PayToolData != null && _CurrentPayorFieldProperty != null)
                    PayToolData.SetPayorToolField(_CurrentPayorFieldProperty);

                _CurrentPayorFieldProperty = value;
                if (_CurrentPayorFieldProperty != null)
                {
                    if (_CurrentPayorFieldProperty.AllignedDirection == null)
                        _CurrentPayorFieldProperty.AllignedDirection = "Left";
                    
                    //Get field type
                   //2129: dynamic field is not saving with different mask types .it is taking only text.
                    AvailableMaskFieldType maskedFields = new AvailableMaskFieldType();
                    int intFieldtype = maskedFields.GetAvailbleFieldType(_CurrentPayorFieldProperty.EquivalentDeuField);
                    if (intFieldtype <= 0)                   
                        MaskDisplayFields = MaskPayorFields.ToList();                    
                    else                   
                        //Filter Mask field Types according to control selection
                        MaskDisplayFields = MaskPayorFields.Where(s => s.Type == intFieldtype).ToList();  

                    if (_CurrentPayorFieldProperty.MaskFieldTypeId == 0)
                    {
                        SelectedMaskField = MaskDisplayFields.FirstOrDefault();
                        _CurrentPayorFieldProperty.MaskFieldTypeId = SelectedMaskField.maskFieldID;
                    }
                    else
                    {
                        SelectedMaskField = (from p in MaskDisplayFields where p.maskFieldID == _CurrentPayorFieldProperty.MaskFieldTypeId select p).FirstOrDefault();
                    }
                }

                if (PayToolData != null && _CurrentPayorFieldProperty != null)
                    PayToolData.SetPayorToolData(_CurrentPayorFieldProperty);

                OnPropertyChanged("CurrentPayorFieldProperty");
            }
        }

        #endregion

        #region Visibility

        private Visibility _showDuplicate;
        public Visibility IsShowDuplicate
        {
            get
            {
                return _showDuplicate;
            }
            set
            {
                _showDuplicate = value;
                OnPropertyChanged("IsShowDuplicate");
            }

        }

        #endregion

        #region Images 

        private string ChequeImage { get; set; }
        private string StatementImage { get; set; }

        private string _ImagePath;
        public string ImagePath
        {
            get
            {
                return _ImagePath;
            }
            set
            {
                _ImagePath = value;
                OnPropertyChanged("ImagePath");

            }
        }

        #endregion

        #region Available Fields

        private string _AvaiableFieldName;
        public string AvaiableFieldName
        {
            get
            {
                return _AvaiableFieldName;
            }
            set
            {
                _AvaiableFieldName = value;
                OnPropertyChanged("AvaiableFieldName");
            }
        }

        private ObservableCollection<PayorToolAvailablelFieldType> _Fields;
        public ObservableCollection<PayorToolAvailablelFieldType> FieldsList
        {
            get
            {
                if (_Fields != null)
                    return _Fields;

                return _Fields;
            }
            set
            {
                _Fields = value;
                OnPropertyChanged("FieldsList");
            }
        }

        private ObservableCollection<PayorToolAvailablelFieldType> _DeletableFields;
        public ObservableCollection<PayorToolAvailablelFieldType> DeletableFieldList
        {
            get
            {
                if (_DeletableFields != null)
                    return _DeletableFields;

                return _DeletableFields;
            }
            set
            {
                _DeletableFields = value;
                OnPropertyChanged("DeletableFieldList");
            }
        }

        private PayorToolAvailablelFieldType _Fieldname;
        public PayorToolAvailablelFieldType SelectedFieldName
        {
            get
            {
                if (_Fieldname == null)
                    _Fieldname = new PayorToolAvailablelFieldType();
                return _Fieldname;

            }
            set
            {
                if (value != null)
                    _Fieldname = value;

                OnPropertyChanged("SelectedFieldName");
            }

        }


        private PayorToolAvailablelFieldType _SelectedDeletableFieldName;
        public PayorToolAvailablelFieldType SelectedDeletableFieldName
        {
            get
            {
                return _SelectedDeletableFieldName;
            }
            set
            {
                _SelectedDeletableFieldName = value;
                OnPropertyChanged("SelectedDeletableFieldName");
            }

        }

        #endregion

        #region Mask Fields

        private ObservableCollection<PayorToolMaskedFieldType> _MaskFields;
        public ObservableCollection<PayorToolMaskedFieldType> MaskPayorFields
        {
            get
            {
                return _MaskFields;
            }
            set
            {
                _MaskFields = value;
                OnPropertyChanged("MaskPayorFields");
            }
        }

        private ObservableCollection<PayorToolMaskedFieldType> _CustomMaskFields;
        public ObservableCollection<PayorToolMaskedFieldType> CustomMaskPayorFields
        {
            get
            {
                return _CustomMaskFields;
            }
            set
            {
                _CustomMaskFields = value;
                OnPropertyChanged("CustomMaskPayorFields");
            }
        }
        //Display Mask field Types according to control selection
        private List<PayorToolMaskedFieldType> _MaskDisplayedField;
        public List<PayorToolMaskedFieldType> MaskDisplayFields
        {
            get
            {
                return _MaskDisplayedField;
            }
            set
            {
                _MaskDisplayedField = value;
                OnPropertyChanged("MaskDisplayFields");
            }
        }

        private PayorToolMaskedFieldType _SelectedMaskField;
        public PayorToolMaskedFieldType SelectedMaskField
        {
            get
            {
                return _SelectedMaskField;
            }
            set
            {
                _SelectedMaskField = value;
                OnPropertyChanged("SelectedMaskField");
            }

        }

        #endregion

        #region Private Fields

        private AutoResetEvent autoResetEvent;
        
        private List<int> _Numbers = null;
        
        public delegate void OnOpenFormulaWindow();
        
        public event OnOpenFormulaWindow OpenFormulaWindow;
        
        public static List<CanvasDroppedField> DroppedFieldList = new List<CanvasDroppedField>();
        
        private WebDevPath ObjWebDevPath;

        #endregion

        public PayorToolVM()
        {
            SavedPayorToolCollection = new List<PayorTool>();
         
            this.PropertyChanged += new PropertyChangedEventHandler(PayorToolVM_PropertyChanged);
            try
            {
                string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);
                PayToolData = new PayorToolData();

                LoadData();

                using (serviceClients.MasterClient)
                    MaskPayorFields = serviceClients.MasterClient.GetPayorToolMaskedFieldList();

                using (serviceClients.PayorToolAvailablelFieldTypeClient)
                {
                    FieldsList =
                        new ObservableCollection<PayorToolAvailablelFieldType>(
                            serviceClients.PayorToolAvailablelFieldTypeClient.GetFieldList().OrderBy(s => s.FieldName).
                                ToList()); //Need to work to pass payor ID 
                    DeletableFieldList =
                        new ObservableCollection<PayorToolAvailablelFieldType>(
                            FieldsList.Where(s => s.canDeleted == true).ToList());
                    if (DeletableFieldList == null)
                        DeletableFieldList = new ObservableCollection<PayorToolAvailablelFieldType>();
                }

                AddNumbers();
                IsShowDuplicate = Visibility.Hidden;
            }
            catch(Exception)
            {
                
            }

        }

        public void LoadData()
        {
            try
            {
                PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
                //PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = VM.MyAgencyVaultSvc.PayorStatus.Active };
                Payors = new ObservableCollection<DisplayedPayor>(
                    serviceClients.DisplayedPayorClient.GetDisplayPayors(Guid.Empty, fillInfo).OrderBy(p => p.PayorName));
               

                if (Payors.Count != 0)
                    IsPayorPresent = true;
                else
                {
                    IsPayorPresent = false;
                    return;
                }
            }
            catch(Exception)
            {
                
            }
        }

        #region"Load Payor template"

        public delegate void OnOpenAddTemplate(string strCommandType);
        public event OnOpenAddTemplate OpenAddTemplate;

        private ICommand _btnAddTemplate;
        public ICommand btnAddTemplate
        {
            get
            {
                if (_btnAddTemplate == null)
                {
                    _btnAddTemplate = new BaseCommand(param => AddTemplate(param));
                }
                return _btnAddTemplate;
            }
        }

        private ICommand _btnDeleteTempalte;
        public ICommand btnDeleteTempalte
        {
            get
            {
                if (_btnDeleteTempalte == null)
                {
                    _btnDeleteTempalte = new BaseCommand(param => DeletePayorTempalate());
                }
                return _btnDeleteTempalte;
            }
        }

        private void AddTemplate(object param)
        {
            if (OpenAddTemplate != null)
                OpenAddTemplate("Add");

            //Call to Load Tempalate when added Tempalte
            if (SelectedPayor != null)
            {
                LoadPayorTempalate(SelectedPayor.PayorID);
            }

            //Load to duplicate payor     
            if (SelectedDuplicatePayor != null)
            {
                LoadDuplicatePayorTempalate(SelectedDuplicatePayor.PayorID);
            }

        }

        private List<Tempalate> _DuplicatePayorTemplate;
        public List<Tempalate> DuplicatePayorTemplate
        {
            get
            {
                if (_DuplicatePayorTemplate != null)
                    return _DuplicatePayorTemplate;
                return null;
            }
            set
            {
                _DuplicatePayorTemplate = value;
                OnPropertyChanged("DuplicatePayorTemplate");
            }
        }

        private Tempalate _DuplicateSelectedPayortempalate = null;
        public Tempalate DuplicateSelectedPayortempalate
        {
            get
            {
                return _DuplicateSelectedPayortempalate;
            }
            set
            {
                _DuplicateSelectedPayortempalate = value;
                if (_DuplicateSelectedPayortempalate != null)
                {
                    OnPropertyChanged("DuplicateSelectedPayortempalate");

                }
            }
        }


        private List<Tempalate> _PayorTemplate;
        public List<Tempalate> PayorTemplate
        {
            get
            {
                if (_PayorTemplate != null)
                    return _PayorTemplate;
                return null;
            }
            set
            {
                _PayorTemplate = value;
                OnPropertyChanged("PayorTemplate");
            }
        }


        private Tempalate _SelectedPayortempalate = null;
        public Tempalate SelectedPayortempalate
        {
            get
            {
                return _SelectedPayortempalate;
            }
            set
            {
                _SelectedPayortempalate = value;
                if (_SelectedPayortempalate != null)
                {
                    OnPropertyChanged("SelectedPayortempalate");

                }
            }
        }

        private void LoadPayorTempalate(Guid PayorID)
        {
            if (PayorID != null)
            {
                PayorTemplate = new List<Tempalate>(serviceClients.PayorToolClient.GetPayorToolTemplate(PayorID)).ToList();
            }

            if (PayorTemplate.Count > 0)
            {
                SelectedPayortempalate = PayorTemplate.FirstOrDefault();
            }
            else
            {
                SelectedPayortempalate = null;
            }
        }
        #endregion

        #region"Load duplicate Payor data"

        private void LoadDuplicatePayorTempalate(Guid PayorID)
        {
            if (PayorID != null)
            {
                DuplicatePayorTemplate = new List<Tempalate>(serviceClients.PayorToolClient.GetPayorToolTemplate(PayorID)).ToList();
            }

            if (DuplicatePayorTemplate.Count > 0)
            {
                DuplicateSelectedPayortempalate = DuplicatePayorTemplate.FirstOrDefault();
            }
            else
            {
                DuplicateSelectedPayortempalate = null;
            }
        }
        #endregion

        #region "Delete Payor Tempalate"
        private void DeletePayorTempalate()
        {
            try
            {

                if (PayorTemplate.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("No seleted item to delete.");
                    return;
                }

                if (MessageBox.Show("Do you want to delete selected payor tempalate?", "Delete Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (SelectedPayortempalate != null)
                    {
                        if (SelectedPayortempalate.TemplateID == null)
                        {
                            if (serviceClients.PayorToolClient.DeletePayorToolTemplate(CurrentPayorToolInfo, null))
                            {
                                //load after delete
                                if (SelectedPayor != null)
                                    LoadPayorTempalate(SelectedPayor.PayorID);
                                if (SelectedDuplicatePayor != null)
                                    LoadDuplicatePayorTempalate(SelectedDuplicatePayor.PayorID);

                                System.Windows.Forms.MessageBox.Show("Default payor template form is deleted.");
                            }
                        }
                        else
                        {
                            string strDeletePayorName = Convert.ToString(SelectedPayortempalate.TemplateName);

                            if (serviceClients.PayorToolClient.DeletePayorToolTemplate(CurrentPayorToolInfo, (Guid)SelectedPayortempalate.TemplateID))
                            {
                                //load after delete
                                if (SelectedPayor != null)
                                    LoadPayorTempalate(SelectedPayor.PayorID);
                                if (SelectedDuplicatePayor != null)
                                    LoadDuplicatePayorTempalate(SelectedDuplicatePayor.PayorID);

                                if (SelectedPayortempalate != null)
                                {
                                    System.Windows.Forms.MessageBox.Show(strDeletePayorName + " payor template form is deleted.");
                                }

                            }
                        }

                    }
                }
            }
            catch
            {
            }

        }
        #endregion

        void PayorToolVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "SelectedMaskField":
                        if (PayToolData != null)
                            PayToolData.MaskFieldChanged(SelectedMaskField);
                        break;

                    case "ImagePath":
                        if (DisplayImage == "StatementUpload")
                            CurrentPayorToolInfo.StatementImageFilePath = ImagePath;
                        else
                            CurrentPayorToolInfo.ChequeImageFilePath = ImagePath;
                        break;

                    case "SelectedPayortempalate":

                        //Call to Load Tempalate when added Tempalte   
                        LoadPayorToolData(SelectedPayor, null, false);

                        break;

                    case "PayorTemplate":

                        if (PayorTemplate.Count == 0)
                        {
                            //Call to Load Tempalate when added Tempalte   
                            LoadPayorToolData(SelectedPayor, null, false);
                        }

                        break;

                    case "SelectedDuplicatePayor":

                        if (SelectedDuplicatePayor != null)
                        {
                            LoadDuplicatePayorTempalate(SelectedDuplicatePayor.PayorID);
                        }

                        break;


                    default:
                        break;
                }
            }
            catch
            {

            }
        }

        private void AddNumbers()
        {
            _Numbers = new List<int>();
            _Numbers.Add(1);
            _Numbers.Add(2);
            _Numbers.Add(3);
            _Numbers.Add(4);
            _Numbers.Add(5);
            _Numbers.Add(6);
            _Numbers.Add(7);
            _Numbers.Add(8);
            _Numbers.Add(9);
            _Numbers.Add(10);
            _Numbers.Add(11);
            _Numbers.Add(12);
            _Numbers.Add(13);
            _Numbers.Add(14);
            _Numbers.Add(15);
            _Numbers.Add(16);
            _Numbers.Add(17);
            _Numbers.Add(18);
            _Numbers.Add(19);
            _Numbers.Add(20);
        }

        #region Public Properties
             
        public int SelectedOrder { get; set; }

        public int TabIndex { get; set; }

 
        public List<CanvasDroppedField> _PayorToolDroppedFields;
        public List<CanvasDroppedField> PayorToolDroppedFields
        {
            get
            {
                return _PayorToolDroppedFields;
            }
            set
            {
                _PayorToolDroppedFields = value;
                OnPropertyChanged("PayorToolDroppedFields");
            }


        }

        public List<int> TabOrders
        {
            get
            {
                return _Numbers;
            }
        }

        public int CurrentIndex { get; set; }

        #endregion

        #region Icommands
        
        private ICommand _addField;
        public ICommand AddAvailableField
        {
            get
            {
                if (_addField == null)
                {
                    _addField = new BaseCommand(param => AddField());
                }
                return _addField;
            }

        }

        
        private ICommand _DeletePayorTool;
        public ICommand DeletePayorTool
        {
            get
            {
                if (_DeletePayorTool == null)
                {
                    _DeletePayorTool = new BaseCommand(param => OnDeletePayorTool());
                }
                return _DeletePayorTool;
            }

        }

        private void OnDeletePayorTool()
        {
            if (MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                serviceClients.PayorToolClient.DeletePayorToolMgr(CurrentPayorToolInfo,null);
                CurrentPayorToolInfo.StatementImageFilePath = string.Empty;
                CurrentPayorToolInfo.ChequeImageFilePath = string.Empty;

                m_DesignerCanvas.DeleteAllFieldsForPayor(SelectedPayor.PayorID);

                ImagePath = "";
                PayorToolDroppedFields = null;               
                PayorToolFieldsProperty.Clear();
                CurrentPayorFieldProperty = null;
                //Enable and disable grid if no filed 
                IsFieldPresent = false;

            }
        }

        private ICommand _OpenFWindow;
        public ICommand OpenWindow
        {
            get
            {
                if (_OpenFWindow == null)
                {
                    _OpenFWindow = new BaseCommand(param => OpenFWindow());
                }
                return _OpenFWindow;
            }
        }


        private ICommand _Delete;
        public ICommand RemoveField
        {
            get
            {
                if (_Delete == null)
                {
                    _Delete = new BaseCommand(param => deleteField());
                }
                return _Delete;
            }
        }

        private ICommand _SavePayorToolInfo;
        public ICommand SavePayorToolInfo
        {
            get
            {
                if (_SavePayorToolInfo == null)
                {
                    _SavePayorToolInfo = new BaseCommand(param => savePayorToolData(param));
                }
                return _SavePayorToolInfo;
            }
        }


        private ICommand _CancelPayorToolInfo;
        public ICommand CancelPayorToolInfo
        {
            get
            {
                if (_CancelPayorToolInfo == null)
                {
                    _CancelPayorToolInfo = new BaseCommand(param => OnCancelPayorTool());
                }
                return _CancelPayorToolInfo;
            }
        }

        /// <summary>
        /// Load payor data from the database.
        /// </summary>
        private void OnCancelPayorTool()
        {
            int selectedFieldId = CurrentPayorFieldProperty.PTAvailableFieldId;
            LoadPayorToolData(SelectedPayor, null,true);

            PayorToolField selectedField = CurrentPayorToolInfo.ToolFields.FirstOrDefault(s => s.PTAvailableFieldId == selectedFieldId);
            if (selectedField != null && !string.IsNullOrEmpty(selectedField.AvailableFieldName))
            {
                m_DesignerCanvas.SelectField(selectedField.PTAvailableFieldId, SelectedPayor.PayorID);
            }
        }

        private ICommand _ShowDuplicatePayors;
        public ICommand ShowDuplicatePayors
        {
            get
            {
                if (_ShowDuplicatePayors == null)
                {
                    _ShowDuplicatePayors = new BaseCommand(param =>
                    {
                        IsShowDuplicate = Visibility.Visible;
                        DuplicatePayors = new List<DisplayedPayor>((from p in Payors select p).ToList().OrderBy(p => p.PayorName));

                        if (DuplicatePayors.Count != 0)
                        {
                            SelectedDuplicatePayor = DuplicatePayors[0];

                        }
                        else
                            SelectedDuplicatePayor = null;
                    }
                    );
                }
                return _ShowDuplicatePayors;
            }

        }

        private ICommand _ClosePanel;
        public ICommand ClosePanel
        {
            get
            {
                if (_ClosePanel == null)
                {
                    _ClosePanel = new BaseCommand(param =>DuplicatePayorAndTemplate());
                   
                }
                return _ClosePanel;

            }

        }

        //Duplicate payor
         void DuplicatePayorAndTemplate()
        {
            if (SelectedPayor == null)
            {
                MessageBox.Show("Please select payor", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedPayortempalate == null)
            {
                MessageBox.Show("Please select template", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedDuplicatePayor == null)
            {
                MessageBox.Show("Please select payor to duplicate", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (DuplicateSelectedPayortempalate == null)
            {
                MessageBox.Show("Please select template to duplicate", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsShowDuplicate = Visibility.Hidden;

            Guid SourceSelPayorID = SelectedPayor.PayorID;
            Guid? SourceTemplateID = SelectedPayortempalate.TemplateID;
            Guid DestiduplicatePayor = SelectedDuplicatePayor.PayorID;
            Guid? DestiduplicateTemplateID = DuplicateSelectedPayortempalate.TemplateID;
            
            serviceClients.PayorToolClient.UpdateDulicatePayorTool(SourceSelPayorID, SourceTemplateID, DestiduplicatePayor, DestiduplicateTemplateID);

            //if (serviceClients.PayorToolClient.IsAvailablePayorTempalate(SourceSelPayorID, SourceTemplateID, DestiduplicatePayor, DestiduplicateTemplateID))
            //{
            // bool bValue = serviceClients.PayorToolClient.UpdateDulicatePayorTool(SourceSelPayorID, SourceTemplateID, DestiduplicatePayor, DestiduplicateTemplateID);
            //}
        }

        private ICommand _CancelDuplicate;
        public ICommand CancelDuplicate
        {
            get
            {
                if (_CancelDuplicate == null)
                {
                    _CancelDuplicate = new BaseCommand(param =>
                    {
                        IsShowDuplicate = Visibility.Hidden;
                    }
                    );
                }
                return _CancelDuplicate;
            }

        }

        private string displayType = "StatementUpload";
        public string DisplayImage
        {
            get { return displayType; }
            set 
            { 
                displayType = value;
                OnPropertyChanged("DisplayImage");
            }
        }

        private ICommand _StatementUpload;
        public ICommand StatementUpload
        {
            get
            {
                if (_StatementUpload == null)
                {
                    _StatementUpload = new BaseCommand(param => DisplayType(param));
                }
                return _StatementUpload;
            }
        }

        public void DisplayType(object param)
        {
            string Image = param.ToString();

            if (Image == "StatementUpload")
            {
                DisplayImage = Image;
                ImagePath = CurrentPayorToolInfo.StatementImageFilePath;
                m_DesignerCanvas.ShowPayorFields(SelectedPayor.PayorID);

                int fieldId = (CurrentPayorFieldProperty != null) ? (CurrentPayorFieldProperty.PTAvailableFieldId) : 0;
                m_DesignerCanvas.SelectField(fieldId, SelectedPayor.PayorID);

                
            }
            else
            {
                DisplayImage = Image;
                ImagePath = CurrentPayorToolInfo.ChequeImageFilePath;
                m_DesignerCanvas.DeselectAll();
                m_DesignerCanvas.ShowPayorFields(Guid.Empty);


            }
        }

        private ICommand _AmountUpload;
        public ICommand AmountUpload
        {
            get
            {
                if (_AmountUpload == null)
                {
                    _AmountUpload = new BaseCommand(param => DisplayType(param));
                }
                return _AmountUpload;
            }

        }

        #endregion

        #region Private Methods

        //private void savePayorToolData(object param)
        //{
        //    //Save current selected field to the payor tool field...
        //    if (PayToolData != null && _CurrentPayorFieldProperty != null)
        //        PayToolData.SetPayorToolField(_CurrentPayorFieldProperty);

        //    //Validation...
        //    bool hasSameOrderFields = false;
        //    bool hasInvalidExpression = false;
        //    ExpressionResult expResult = new ExpressionResult();

        //    foreach (PayorToolField field in _PayorToolFieldsPropperty)
        //    {
        //        if (field.IsCalculatedField)
        //        {    //change in && !string.IsNullOrEmpty(field.CalculationFormula.FormulaExpression))
        //            if (field.CalculationFormula != null && !string.IsNullOrEmpty(field.CalculationFormula.FormulaExpression))
        //            {
        //                expResult = ExpressionExecutor.IsValidExpression(field.CalculationFormula.FormulaExpression);
        //                if (expResult.ErrorInEvalution)
        //                {
        //                    expResult.ErrorDescription = "Enter valid formula for calculated fields.";
        //                    hasInvalidExpression = true;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                expResult.ErrorInEvalution = true;
        //                expResult.ErrorDescription = "Blank formula is not allowed for calculated fields.";
        //                hasInvalidExpression = true;
        //                break;
        //            }
        //        }

        //        //before
        //        PayorToolField field1 = _PayorToolFieldsPropperty.FirstOrDefault(s => s.FieldOrder == field.FieldOrder && s != field);
        //        if (field1 != null)
        //        {
        //            hasSameOrderFields = true;
        //            break;
        //        }

        //    }

        //    if(hasInvalidExpression)
        //    {
        //        MessageBox.Show(expResult.ErrorDescription, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (hasSameOrderFields)
        //    {
        //        MessageBox.Show("Fields should not have same order.","Error",MessageBoxButton.OK,MessageBoxImage.Error);
        //        return;
        //    }

        //    bool hasCarrierField = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "Carrier");
        //    bool hasProdcutField = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "Product");
        //    bool hasInvoiceDate = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "InvoiceDate");

        //    if (hasCarrierField == false && hasProdcutField == true)
        //    {
        //        MessageBox.Show("PayorTool Form must include Carrier field because it has Product field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (hasCarrierField == true && hasProdcutField == true)
        //    {
        //        int carrierFieldTabOrder = _PayorToolFieldsPropperty.FirstOrDefault(s => s.AvailableFieldName == "Carrier").FieldOrder;
        //        int productFieldTabOrder = _PayorToolFieldsPropperty.FirstOrDefault(s => s.AvailableFieldName == "Product").FieldOrder;

                
        //        //if (carrierFieldTabOrder >= productFieldTabOrder)
        //        //{
        //        //    MessageBox.Show("Product field order value should be greater than Carrier field order value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        //    return;
        //        //}
        //    }

        //    if (!hasInvoiceDate)
        //    {
        //        MessageBox.Show("PayorTool Form must include InvoiceDate field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;

        //    }

        //    Mouse.OverrideCursor = Cursors.Wait;
        //    try
        //    {
        //        DesignerCanvas canvas = param as DesignerCanvas;

        //        //Gaurav Update the position and the size of the fields availabe on the form.

        //        if (CurrentPayorToolInfo.PayorToolId == Guid.Empty)
        //            CurrentPayorToolInfo.PayorToolId = Guid.NewGuid();

        //        CurrentPayorToolInfo.PayorID = SelectedPayor.PayorID;

        //        Guid PayorToolID=Guid.Empty;
        //        PayorToolID = CurrentPayorToolInfo.PayorToolId;

        //        foreach (PayorToolField field in _PayorToolFieldsPropperty)
        //        {
        //            //old
        //            //DesignerItem item = canvas.DesignerItems.Find(s => ((s.Content as TextBoxDI).Field.FieldName == field.AvailableFieldName) && ((s.Content as TextBoxDI).PayorId == SelectedPayor.PayorID));

        //            //new
        //            DesignerItem item = canvas.DesignerItems.Find(s => ((s.Content as TextBoxDI).Field != null) && ((s.Content as TextBoxDI).Field.FieldName == field.AvailableFieldName) && ((s.Content as TextBoxDI).PayorId == SelectedPayor.PayorID));

        //            if (!string.IsNullOrEmpty(field.EquivalentIncomingField))
        //                field.EquivalentDeuField = field.EquivalentIncomingField;
        //            else
        //                field.EquivalentDeuField = field.EquivalentLearnedField;

        //            if (item != null)
        //            {
        //                field.ControlWidth = (item.Content as TextBoxDI).ActualWidth;
        //                field.ControlHeight = (item.Content as TextBoxDI).ActualHeight;
        //                field.ControlX = DesignerCanvas.GetLeft(item);
        //                field.ControlY = DesignerCanvas.GetTop(item);

        //            }
        //        }

        //        CurrentPayorToolInfo.ToolFields = _PayorToolFieldsPropperty;
        //        //get the web dev path
        //        // save the selected image on the remote server
        //         FileUtility ObjUpload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password,ObjWebDevPath.DomainName);
        //        autoResetEvent = new AutoResetEvent(false);
        //        ObjUpload.UploadComplete += (i, j) =>
        //            {
        //                autoResetEvent.Set();
        //            };


        //        if ((!string.IsNullOrEmpty(CurrentPayorToolInfo.StatementImageFilePath)) && !CurrentPayorToolInfo.StatementImageFilePath.EndsWith(".tmp"))
        //        {
        //            ObjUpload.Upload(CurrentPayorToolInfo.StatementImageFilePath, ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.StatementImageFilePath));
        //            CurrentPayorToolInfo.WebDevStatementImageFilePath = ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.StatementImageFilePath);
        //            autoResetEvent.WaitOne();
        //        }
        //        else
        //        {
        //            if (string.IsNullOrEmpty(CurrentPayorToolInfo.StatementImageFilePath))
        //            {
        //                CurrentPayorToolInfo.WebDevStatementImageFilePath = string.Empty;
        //            }
        //        }

        //        if ((!string.IsNullOrEmpty(CurrentPayorToolInfo.ChequeImageFilePath)) && !CurrentPayorToolInfo.ChequeImageFilePath.EndsWith(".tmp"))
        //        {
        //            ObjUpload.Upload(CurrentPayorToolInfo.ChequeImageFilePath, ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.ChequeImageFilePath));
        //            CurrentPayorToolInfo.WebDevChequeImageFilePath = ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.ChequeImageFilePath);
        //            autoResetEvent.WaitOne();
        //        }
        //        else
        //        {
        //            if (string.IsNullOrEmpty(CurrentPayorToolInfo.ChequeImageFilePath))
        //            {
        //                CurrentPayorToolInfo.WebDevChequeImageFilePath = string.Empty;
        //            }
        //        }

        //        using (serviceClients.PayorToolClient)
        //        {
        //            try
        //            {
        //                if (CurrentPayorToolInfo.PayorToolId != Guid.Empty)
        //                {
        //                    serviceClients.PayorToolClient.AddUpdatePayorToolMgr(CurrentPayorToolInfo);

        //                    if (VMInstances.DeuVM != null)
        //                        VMInstances.DeuVM.RefreshRequired = true;
        //                }
        //            }
        //            catch
        //            {
        //            }
        //        }

        //    }
        //    finally
        //    {
        //        Mouse.OverrideCursor = null;
        //    }
        //}

        private void savePayorToolData(object param)
        {
            if (SelectedPayor == null)
            {
                MessageBox.Show("Please select payor", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedPayortempalate == null)
            {
                MessageBox.Show("Please select template","Information",MessageBoxButton.OK,MessageBoxImage.Information);
                return;
            }
            //Save current selected field to the payor tool field...
            if (PayToolData != null && _CurrentPayorFieldProperty != null)
                PayToolData.SetPayorToolField(_CurrentPayorFieldProperty);

            //Validation...
            bool hasSameOrderFields = false;
            bool hasInvalidExpression = false;
            ExpressionResult expResult = new ExpressionResult();

            foreach (PayorToolField field in _PayorToolFieldsPropperty)
            {
                if (field.IsCalculatedField)
                {    //change in && !string.IsNullOrEmpty(field.CalculationFormula.FormulaExpression))
                    if (field.CalculationFormula != null && !string.IsNullOrEmpty(field.CalculationFormula.FormulaExpression))
                    {
                        expResult = ExpressionExecutor.IsValidExpression(field.CalculationFormula.FormulaExpression);
                        if (expResult.ErrorInEvalution)
                        {
                            expResult.ErrorDescription = "Enter valid formula for calculated fields.";
                            hasInvalidExpression = true;
                            break;
                        }
                    }
                    else
                    {
                        expResult.ErrorInEvalution = true;
                        expResult.ErrorDescription = "Blank formula is not allowed for calculated fields.";
                        hasInvalidExpression = true;
                        break;
                    }
                }

                //before
                PayorToolField field1 = _PayorToolFieldsPropperty.FirstOrDefault(s => s.FieldOrder == field.FieldOrder && s != field);
                if (field1 != null)
                {
                    hasSameOrderFields = true;
                    break;
                }

            }

            if (hasInvalidExpression)
            {
                MessageBox.Show(expResult.ErrorDescription, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (hasSameOrderFields)
            {
                MessageBox.Show("Fields should not have same order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool hasCarrierField = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "Carrier");
            bool hasProdcutField = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "Product");
            bool hasInvoiceDate = _PayorToolFieldsPropperty.ToList().Exists(s => s.AvailableFieldName == "InvoiceDate");

            if (hasCarrierField == false && hasProdcutField == true)
            {
                MessageBox.Show("PayorTool Form must include Carrier field because it has Product field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (hasCarrierField == true && hasProdcutField == true)
            {
                int carrierFieldTabOrder = _PayorToolFieldsPropperty.FirstOrDefault(s => s.AvailableFieldName == "Carrier").FieldOrder;
                int productFieldTabOrder = _PayorToolFieldsPropperty.FirstOrDefault(s => s.AvailableFieldName == "Product").FieldOrder;

            }

            if (!hasInvoiceDate)
            {
                MessageBox.Show("PayorTool Form must include InvoiceDate field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                DesignerCanvas canvas = param as DesignerCanvas;

                //Gaurav Update the position and the size of the fields availabe on the form.

                if (CurrentPayorToolInfo.PayorToolId == Guid.Empty)
                    CurrentPayorToolInfo.PayorToolId = Guid.NewGuid();

                CurrentPayorToolInfo.PayorID = SelectedPayor.PayorID;

                Guid PayorToolID = Guid.Empty;
                PayorToolID = CurrentPayorToolInfo.PayorToolId;

                foreach (PayorToolField field in _PayorToolFieldsPropperty)
                {
                    //old
                    //DesignerItem item = canvas.DesignerItems.Find(s => ((s.Content as TextBoxDI).Field.FieldName == field.AvailableFieldName) && ((s.Content as TextBoxDI).PayorId == SelectedPayor.PayorID));

                    //new

                    if (canvas == null) return;

                    DesignerItem item = canvas.DesignerItems.Find(s => ((s.Content as TextBoxDI).Field != null) && ((s.Content as TextBoxDI).Field.FieldName == field.AvailableFieldName) && ((s.Content as TextBoxDI).PayorId == SelectedPayor.PayorID));

                    if (!string.IsNullOrEmpty(field.EquivalentIncomingField))
                        field.EquivalentDeuField = field.EquivalentIncomingField;
                    else
                        field.EquivalentDeuField = field.EquivalentLearnedField;

                    if (item != null)
                    {
                        field.ControlWidth = (item.Content as TextBoxDI).ActualWidth;
                        field.ControlHeight = (item.Content as TextBoxDI).ActualHeight;
                        field.ControlX = DesignerCanvas.GetLeft(item);
                        field.ControlY = DesignerCanvas.GetTop(item);

                    }
                }

                CurrentPayorToolInfo.ToolFields = _PayorToolFieldsPropperty;
                //get the web dev path
                // save the selected image on the remote server
                FileUtility ObjUpload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                autoResetEvent = new AutoResetEvent(false);
                ObjUpload.UploadComplete += (i, j) =>
                {
                    autoResetEvent.Set();
                };


                if ((!string.IsNullOrEmpty(CurrentPayorToolInfo.StatementImageFilePath)) && !CurrentPayorToolInfo.StatementImageFilePath.EndsWith(".tmp"))
                {
                    ObjUpload.Upload(CurrentPayorToolInfo.StatementImageFilePath, ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.StatementImageFilePath));
                    CurrentPayorToolInfo.WebDevStatementImageFilePath = ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.StatementImageFilePath);
                    autoResetEvent.WaitOne();
                }
                else
                {
                    if (string.IsNullOrEmpty(CurrentPayorToolInfo.StatementImageFilePath))
                    {
                        CurrentPayorToolInfo.WebDevStatementImageFilePath = string.Empty;
                    }
                }

                if ((!string.IsNullOrEmpty(CurrentPayorToolInfo.ChequeImageFilePath)) && !CurrentPayorToolInfo.ChequeImageFilePath.EndsWith(".tmp"))
                {
                    ObjUpload.Upload(CurrentPayorToolInfo.ChequeImageFilePath, ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.ChequeImageFilePath));
                    CurrentPayorToolInfo.WebDevChequeImageFilePath = ImageFolderOnWebDev + Path.GetFileName(CurrentPayorToolInfo.ChequeImageFilePath);
                    autoResetEvent.WaitOne();
                }
                else
                {
                    if (string.IsNullOrEmpty(CurrentPayorToolInfo.ChequeImageFilePath))
                    {
                        CurrentPayorToolInfo.WebDevChequeImageFilePath = string.Empty;
                    }
                }

                using (serviceClients.PayorToolClient)
                {
                    if (SelectedPayortempalate != null)
                    {
                        if (SelectedPayortempalate.TemplateName != "Default" || SelectedPayortempalate.TemplateID != null)
                        {
                            CurrentPayorToolInfo.TemplateID = SelectedPayortempalate.TemplateID;
                        }
                        else
                        {
                            CurrentPayorToolInfo.TemplateID = null;
                        }
                        serviceClients.PayorToolClient.AddUpdatePayorToolMgr(CurrentPayorToolInfo);

                        if (VMInstances.DeuVM != null)
                            VMInstances.DeuVM.RefreshRequired = true;
                    }
                }

            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// If DestinationPayor is null then load the source payor data.
        /// if DestinationPayor is not null then copy the source payor data 
        /// to destination payor data and load the destination payor.
        /// </summary>
        /// <param name="SourcePayor"></param>
        /// <param name="DestinationPayor"></param>
        /// //Old code .....
        //private void LoadPayorToolData(DisplayedPayor SourcePayor, DisplayedPayor DestinationPayor, bool LoadSourcePayorFromDB)
        //{
        //    Guid loadPayorId = Guid.Empty;
        //    bool isCopyCase = false;
        //    bool isSourcePayorToolLoadedFromDB = false;

        //    if (DestinationPayor == null)
        //    {
        //        isCopyCase = false;
        //        loadPayorId = SourcePayor.PayorID;
        //    }
        //    else
        //    {
        //        isCopyCase = true;
        //        loadPayorId = DestinationPayor.PayorID;
        //    }

        //    using (serviceClients.PayorToolClient)
        //    {
        //        if (!isCopyCase)
        //        {
        //            if (!LoadSourcePayorFromDB)
        //            {
        //                CurrentPayorToolInfo = SavedPayorToolCollection.Find(s => s.PayorID == loadPayorId);

        //                if (CurrentPayorToolInfo == null || CurrentPayorToolInfo.PayorID == Guid.Empty)
        //                {
        //                    CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgr(loadPayorId);
        //                    SavedPayorToolCollection.Remove(SavedPayorToolCollection.FirstOrDefault(s => s.PayorID == loadPayorId));
        //                    SavedPayorToolCollection.Add(CurrentPayorToolInfo);
        //                    isSourcePayorToolLoadedFromDB = true;
        //                }
        //            }
        //            else
        //            {
        //                m_DesignerCanvas.DeleteAllFieldsForPayor(loadPayorId);
        //                CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgr(loadPayorId);
        //                SavedPayorToolCollection.Remove(SavedPayorToolCollection.FirstOrDefault(s => s.PayorID == loadPayorId));
        //                SavedPayorToolCollection.Add(CurrentPayorToolInfo);
        //                isSourcePayorToolLoadedFromDB = true;
        //            }

        //        }
        //        else
        //        {
        //            //Find destination payor tool present in memory or not.
        //            PayorTool payorTool = SavedPayorToolCollection.Find(s => s.PayorID == DestinationPayor.PayorID);
                    
        //            PayorTool tempPayorTool = null;

        //            //if destination payor tool is not present in the memory.
        //            if (payorTool == null || payorTool.PayorID == Guid.Empty)
        //            {
        //                //I am assuming that the source payor tool must be present in the memory.
        //                tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == SourcePayor.PayorID);
                        
        //                //asserting if source payor tool is not present in the memory.
        //                if (tempPayorTool == null || tempPayorTool.PayorID == Guid.Empty)
        //                    System.Diagnostics.Debug.Assert(tempPayorTool == null || tempPayorTool.PayorID == Guid.Empty, "Source payor tool must present in the memory.");

        //                //Clone the source payor tool for destination.
        //                CurrentPayorToolInfo = ClonePayorTool(tempPayorTool);
        //                CurrentPayorToolInfo.PayorToolId = serviceClients.PayorToolClient.GetPayorToolId(DestinationPayor.PayorID);

        //                CurrentPayorToolInfo.PayorID = DestinationPayor.PayorID;
        //                SavedPayorToolCollection.Add(CurrentPayorToolInfo);
        //            }
        //            else//if destination payor tool is present in the memory
        //            {
        //                //Find the source payor tool from the memory.
        //                tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == SourcePayor.PayorID);

        //                //Clone the source payor tool.
        //                CurrentPayorToolInfo = ClonePayorTool(tempPayorTool);

        //                //Find the destination payor tool from the memory because it is present in the memory as per condition.
        //                tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == DestinationPayor.PayorID);
                        
        //                //Change the cloned source payor tool Ids.
        //                CurrentPayorToolInfo.PayorToolId = tempPayorTool.PayorToolId;
        //                CurrentPayorToolInfo.PayorID = tempPayorTool.PayorID;

        //                //Remove old destination payor tool from the memory.
        //                SavedPayorToolCollection.Remove(tempPayorTool);

        //                //Add new destination payor tool to the memory.
        //                SavedPayorToolCollection.Add(CurrentPayorToolInfo);
        //            }
        //        }

        //        PayorToolFieldsProperty = CurrentPayorToolInfo.ToolFields;
        //        if (PayorToolFieldsProperty == null)
        //            PayorToolFieldsProperty = CurrentPayorToolInfo.ToolFields = new ObservableCollection<PayorToolField>();

        //        if (PayorToolFieldsProperty.Count != 0)
        //            CurrentPayorFieldProperty = PayorToolFieldsProperty[0];
        //        else
        //            CurrentPayorFieldProperty = null;

        //        if (isCopyCase)
        //        {
        //            m_DesignerCanvas.CopyFieldsFromPayor(SourcePayor.PayorID, PayorToolFieldsProperty);
        //            //.On duplicating a payor form from one payor to another, the duplicated payor form is of larger font.
        //            //This loop is for payor tool that are duplicate from the payor to set the font size.
        //            foreach (PayorToolField payorToolField in PayorToolFieldsProperty)
        //            {
        //                PayorToolAvailablelFieldType payorFld = FieldsList.FirstOrDefault(s => s.FieldName == payorToolField.AvailableFieldName);
        //                m_DesignerCanvas.DropFieldOnCanvas(payorToolField, payorFld, payorToolField.ControlX, payorToolField.ControlY, payorToolField.ControlWidth, payorToolField.ControlHeight);
        //            }
        //        }
        //        else
        //        {
        //            CurrentPayorToolInfo.PayorID = SourcePayor.PayorID;

        //            if (isSourcePayorToolLoadedFromDB)
        //            {
        //                //This loop is for payor tool that are loaded from the database.
        //                foreach (PayorToolField payorToolField in PayorToolFieldsProperty)
        //                {
        //                    PayorToolAvailablelFieldType payorFld = FieldsList.FirstOrDefault(s => s.FieldName == payorToolField.AvailableFieldName);
        //                    m_DesignerCanvas.DropFieldOnCanvas(payorToolField, payorFld, payorToolField.ControlX, payorToolField.ControlY, payorToolField.ControlWidth, payorToolField.ControlHeight);

        //                }
        //            }
                    
        //            //Enable and disable grid if no filed 
        //            if (PayorToolFieldsProperty.Count > 0)                    
        //                IsFieldPresent = true;                    
        //            else                   
        //                IsFieldPresent = false;


        //            autoResetEvent = new AutoResetEvent(false);

        //            //download the payor tool image from server and show on UI
        //            FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
        //            ObjDownload.DownloadComplete += (obj1, obj2) =>
        //            {
        //                autoResetEvent.Set();
        //            };

        //            if (!string.IsNullOrEmpty(CurrentPayorToolInfo.WebDevChequeImageFilePath))
        //            {
        //                CurrentPayorToolInfo.ChequeImageFilePath = System.IO.Path.GetTempFileName();
        //                ObjDownload.Download(CurrentPayorToolInfo.WebDevChequeImageFilePath, CurrentPayorToolInfo.ChequeImageFilePath);
        //                autoResetEvent.WaitOne(TimeSpan.FromMinutes(1));
        //            }

        //            if (!string.IsNullOrEmpty(CurrentPayorToolInfo.WebDevStatementImageFilePath))
        //            {
        //                CurrentPayorToolInfo.StatementImageFilePath = System.IO.Path.GetTempFileName();
        //                ObjDownload.Download(CurrentPayorToolInfo.WebDevStatementImageFilePath, CurrentPayorToolInfo.StatementImageFilePath);
        //                autoResetEvent.WaitOne(TimeSpan.FromMinutes(1));
        //            }
        //        }

        //        if (CurrentPayorToolInfo.PayorToolId == Guid.Empty)
        //            System.Diagnostics.Debug.Assert(CurrentPayorToolInfo.PayorToolId == Guid.Empty, "PayorToolId can not be null.");

        //        DisplayType(DisplayImage);
        //    }
        //}

        private void LoadPayorToolData(DisplayedPayor SourcePayor, DisplayedPayor DestinationPayor, bool LoadSourcePayorFromDB)
        {
            try
            {
                Guid loadPayorId = Guid.Empty;
                bool isCopyCase = false;
                bool isSourcePayorToolLoadedFromDB = false;

                if (DestinationPayor == null)
                {
                    isCopyCase = false;
                    loadPayorId = SourcePayor.PayorID;
                }
                else
                {
                    isCopyCase = true;
                    loadPayorId = DestinationPayor.PayorID;
                }

                using (serviceClients.PayorToolClient)
                {
                    if (!isCopyCase)
                    {
                        if (!LoadSourcePayorFromDB)
                        {

                            m_DesignerCanvas.DeleteAllFieldsForPayor(loadPayorId);
                            m_DesignerCanvas.DeselectAll();

                            CurrentPayorToolInfo = SavedPayorToolCollection.Find(s => s.PayorID == loadPayorId);

                            if (SelectedPayortempalate != null)
                            {
                                if (SelectedPayortempalate.TemplateName == "Default" || SelectedPayortempalate.TemplateID == null)
                                {
                                    CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgr(loadPayorId);
                                }
                                else
                                {
                                    CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgrWithTemplate(loadPayorId, SelectedPayortempalate.TemplateID);
                                }
                            }

                            SavedPayorToolCollection.Remove(SavedPayorToolCollection.FirstOrDefault(s => s.PayorID == loadPayorId));

                            if (PayorTemplate != null)
                            {
                                if (PayorTemplate.Count > 0)
                                {
                                    SavedPayorToolCollection.Add(CurrentPayorToolInfo);
                                }
                            }

                            isSourcePayorToolLoadedFromDB = true;

                        }
                        else
                        {
                            m_DesignerCanvas.DeleteAllFieldsForPayor(loadPayorId);
                            m_DesignerCanvas.DeselectAll();

                            //CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgr(loadPayorId);
                            if (SelectedPayortempalate.TemplateName == "Default" || SelectedPayortempalate.TemplateID == null)
                            {
                                CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgr(loadPayorId);
                            }
                            else
                            {
                                CurrentPayorToolInfo = serviceClients.PayorToolClient.GetPayorToolMgrWithTemplate(loadPayorId, SelectedPayortempalate.TemplateID);
                            }

                            SavedPayorToolCollection.Remove(SavedPayorToolCollection.FirstOrDefault(s => s.PayorID == loadPayorId));
                            SavedPayorToolCollection.Add(CurrentPayorToolInfo);
                            isSourcePayorToolLoadedFromDB = true;
                        }

                    }
                    else
                    {
                        //Find destination payor tool present in memory or not.
                        PayorTool payorTool = SavedPayorToolCollection.Find(s => s.PayorID == DestinationPayor.PayorID);

                        PayorTool tempPayorTool = null;

                        //if destination payor tool is not present in the memory.
                        if (payorTool == null || payorTool.PayorID == Guid.Empty)
                        {
                            //I am assuming that the source payor tool must be present in the memory.
                            tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == SourcePayor.PayorID);

                            //asserting if source payor tool is not present in the memory.
                            if (tempPayorTool == null || tempPayorTool.PayorID == Guid.Empty)
                                System.Diagnostics.Debug.Assert(tempPayorTool == null || tempPayorTool.PayorID == Guid.Empty, "Source payor tool must present in the memory.");

                            //Clone the source payor tool for destination.
                            CurrentPayorToolInfo = ClonePayorTool(tempPayorTool);
                            CurrentPayorToolInfo.PayorToolId = serviceClients.PayorToolClient.GetPayorToolId(DestinationPayor.PayorID);

                            CurrentPayorToolInfo.PayorID = DestinationPayor.PayorID;

                            SavedPayorToolCollection.Add(CurrentPayorToolInfo);
                        }
                        else//if destination payor tool is present in the memory
                        {
                            //Find the source payor tool from the memory.
                            tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == SourcePayor.PayorID);

                            //Clone the source payor tool.
                            CurrentPayorToolInfo = ClonePayorTool(tempPayorTool);

                            //Find the destination payor tool from the memory because it is present in the memory as per condition.
                            tempPayorTool = SavedPayorToolCollection.Find(s => s.PayorID == DestinationPayor.PayorID);

                            //Change the cloned source payor tool Ids.
                            CurrentPayorToolInfo.PayorToolId = tempPayorTool.PayorToolId;
                            CurrentPayorToolInfo.PayorID = tempPayorTool.PayorID;
                            //Remove old destination payor tool from the memory.
                            SavedPayorToolCollection.Remove(tempPayorTool);

                            //Add new destination payor tool to the memory.
                            SavedPayorToolCollection.Add(CurrentPayorToolInfo);
                        }
                    }

                    PayorToolFieldsProperty = CurrentPayorToolInfo.ToolFields;
                    if (PayorToolFieldsProperty == null)
                        PayorToolFieldsProperty = CurrentPayorToolInfo.ToolFields = new ObservableCollection<PayorToolField>();

                    if (PayorToolFieldsProperty.Count != 0)
                        CurrentPayorFieldProperty = PayorToolFieldsProperty[0];
                    else
                        CurrentPayorFieldProperty = null;

                    if (isCopyCase)
                    {
                        m_DesignerCanvas.CopyFieldsFromPayor(SourcePayor.PayorID, PayorToolFieldsProperty);

                        //This loop is for payor tool that are duplicate from the payor to set the font size.
                        foreach (PayorToolField payorToolField in PayorToolFieldsProperty)
                        {
                            PayorToolAvailablelFieldType payorFld = FieldsList.FirstOrDefault(s => s.FieldName == payorToolField.AvailableFieldName);
                            m_DesignerCanvas.DropFieldOnCanvas(payorToolField, payorFld, payorToolField.ControlX, payorToolField.ControlY, payorToolField.ControlWidth, payorToolField.ControlHeight);
                        }

                    }
                    else
                    {
                        CurrentPayorToolInfo.PayorID = SourcePayor.PayorID;

                        if (isSourcePayorToolLoadedFromDB)
                        {
                            //This loop is for payor tool that are loaded from the database.
                            foreach (PayorToolField payorToolField in PayorToolFieldsProperty)
                            {
                                PayorToolAvailablelFieldType payorFld = FieldsList.FirstOrDefault(s => s.FieldName == payorToolField.AvailableFieldName);
                                m_DesignerCanvas.DropFieldOnCanvas(payorToolField, payorFld, payorToolField.ControlX, payorToolField.ControlY, payorToolField.ControlWidth, payorToolField.ControlHeight);

                            }
                        }

                        //Enable and disable grid if no filed 
                        if (PayorToolFieldsProperty.Count > 0)
                            IsFieldPresent = true;
                        else
                            IsFieldPresent = false;


                        autoResetEvent = new AutoResetEvent(false);

                        //download the payor tool image from server and show on UI
                        FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                        ObjDownload.DownloadComplete += (obj1, obj2) =>
                        {
                            autoResetEvent.Set();
                        };

                        if (!string.IsNullOrEmpty(CurrentPayorToolInfo.WebDevChequeImageFilePath))
                        {
                            CurrentPayorToolInfo.ChequeImageFilePath = System.IO.Path.GetTempFileName();
                            ObjDownload.Download(CurrentPayorToolInfo.WebDevChequeImageFilePath, CurrentPayorToolInfo.ChequeImageFilePath);
                            autoResetEvent.WaitOne(TimeSpan.FromMinutes(1));
                        }

                        if (!string.IsNullOrEmpty(CurrentPayorToolInfo.WebDevStatementImageFilePath))
                        {
                            CurrentPayorToolInfo.StatementImageFilePath = System.IO.Path.GetTempFileName();
                            ObjDownload.Download(CurrentPayorToolInfo.WebDevStatementImageFilePath, CurrentPayorToolInfo.StatementImageFilePath);
                            autoResetEvent.WaitOne(TimeSpan.FromMinutes(1));
                        }
                    }

                    if (CurrentPayorToolInfo.PayorToolId == Guid.Empty)
                        System.Diagnostics.Debug.Assert(CurrentPayorToolInfo.PayorToolId == Guid.Empty, "PayorToolId can not be null.");

                    DisplayType(DisplayImage);

                }
            }
            catch
            {
            }

        }

        private PayorTool ClonePayorTool(PayorTool source)
        {
            PayorTool destination = new PayorTool();
            destination.ChequeImageFilePath = source.ChequeImageFilePath;
            destination.StatementImageFilePath = source.StatementImageFilePath;
            destination.PayorID = SelectedPayor.PayorID;
            destination.PayorToolId = source.PayorToolId;
            destination.WebDevChequeImageFilePath = source.WebDevChequeImageFilePath;
            destination.WebDevStatementImageFilePath = source.WebDevStatementImageFilePath;
            destination.ToolFields = new ObservableCollection<PayorToolField>();

            foreach (PayorToolField field in source.ToolFields)
            {
                PayorToolField newField = new PayorToolField();

                newField.PayorFieldID = Guid.NewGuid();
                newField.AllignedDirection = field.AllignedDirection;
                newField.AvailableFieldName = field.AvailableFieldName;

                if (field.CalculationFormula != null)
                {
                    newField.CalculationFormula = new Formula();
                    newField.CalculationFormula.FormulaExpression = field.CalculationFormula.FormulaExpression;
                    newField.CalculationFormula.FormulaTtitle = field.CalculationFormula.FormulaTtitle;
                    newField.CalculationFormula.FormulaID = Guid.NewGuid();
                }
                else
                    newField.CalculationFormula = null;
                
                newField.ControlHeight = field.ControlHeight;
                newField.ControlWidth = field.ControlWidth;
                newField.ControlX = field.ControlX;
                newField.ControlY = field.ControlY;
                newField.DefaultValue = field.DefaultValue;
                newField.EquivalentIncomingField = field.EquivalentIncomingField;
                newField.EquivalentLearnedField = field.EquivalentLearnedField;
                newField.EquivalentDeuField = field.EquivalentDeuField;
                newField.FieldOrder = field.FieldOrder;
                newField.FieldStatusValue = field.FieldStatusValue;
                newField.FieldValue = field.FieldValue;
                newField.HelpText = field.HelpText;
                newField.IsCalculatedField = field.IsCalculatedField;
                newField.IsDeleted = field.IsDeleted;
                newField.IsNotVisible = false;
                newField.IsOverrideOfCalcAllowed = field.IsOverrideOfCalcAllowed;
                newField.IsPartOfPrimaryKey = field.IsPartOfPrimaryKey;
                newField.IsPopulateIfLinked = field.IsPopulateIfLinked;
                newField.IsTabbedToNextFieldIfLinked = field.IsTabbedToNextFieldIfLinked;
                newField.IsZeroorBlankAllowed = field.IsZeroorBlankAllowed;
                newField.LabelOnField = field.LabelOnField;
                newField.MaskFieldType = field.MaskFieldType;
                newField.MaskFieldTypeId = field.MaskFieldTypeId;
                //newField.PositionOnImage = field.PositionOnImage;
                newField.PTAvailableFieldId = field.PTAvailableFieldId;

                destination.ToolFields.Add(newField);
            }

            return destination;
        }

        public string FindSomeObject(PayorToolAvailablelFieldType Type)
        {

            if (Type == null) return null;
            return Type.FieldName;
        }

        private bool IsValidField()
        {
            if (!string.IsNullOrEmpty(AvaiableFieldName))
            {
                PayorToolAvailablelFieldType _Type = (from p in FieldsList where p.FieldName == AvaiableFieldName select p).FirstOrDefault();
                if (_Type != null)
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
          
        private void OpenFWindow()
        {
            if (OpenFormulaWindow != null)
                OpenFormulaWindow();
        }
        
        #endregion

        #region Validation

        private ICommand _DeleteFormula;
        public ICommand DeleteFormula
        {
            get
            {
                if (_DeleteFormula == null)
                {
                    _DeleteFormula = new BaseCommand(param => DeleteExpFormula(param));
                }
                return _DeleteFormula;
            }

        }

        private void DeleteExpFormula(object param)
        {
            if (CurrentPayorFieldProperty != null)
            {
                if (MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    CurrentPayorFieldProperty.CalculationFormula = null;

                }
            }
        }

        private void disableAfterDelete()
        {
            
        }

        #endregion


        private PayorToolField GetInitialisePayorToolField()
        {
            PayorToolField field = new PayorToolField();
            field.PTAvailableFieldId = 0;
            field.PayorFieldID = Guid.NewGuid();
            field.MaskFieldTypeId = 0;
            field.LabelOnField = string.Empty;
            field.AllignedDirection = "Left";
            field.CalculationFormula = null;
            field.DefaultValue = string.Empty;
            field.EquivalentIncomingField = "None";
            field.EquivalentLearnedField = "None";
            field.FieldOrder = 0;
            field.FieldStatusValue = "Required";
            field.FieldValue = string.Empty;
            field.HelpText = string.Empty;
            field.IsCalculatedField = false;
            field.IsOverrideOfCalcAllowed = false;
            field.IsPartOfPrimaryKey = false;
            field.IsPopulateIfLinked = false;
            field.IsTabbedToNextFieldIfLinked = false;
            field.IsZeroorBlankAllowed = false;
            field.MaskFieldType = 0;

            return field;
        }

        /// <summary>
        /// Occur when field is dropped on image.
        /// </summary>
        /// <param name="payorToolField"></param>
        /// <returns></returns>
        internal bool OnDropValidation(PayorToolAvailablelFieldType payorToolField)
        {
            bool isValidFieldDrop = true;
            try
            {
                PayorToolField field =
                    _PayorToolFieldsPropperty.FirstOrDefault(s => s.PTAvailableFieldId == payorToolField.FieldID);

                if ((field != null) || (DisplayImage != "StatementUpload"))
                    isValidFieldDrop = false;
            }
            catch(Exception)
            {
                
            }
            return isValidFieldDrop;
        }

        /// <summary>
        /// Occur when field drop is completed.
        /// </summary>
        internal PayorToolField OnDropCompleted(PayorToolAvailablelFieldType payorToolField)
        {
            try
            {
                PayorToolField field = GetInitialisePayorToolField();
                field.PTAvailableFieldId = payorToolField.FieldID;
                field.PayorFieldID = Guid.NewGuid();
                field.PayorToolId = _CurrentPayorToolInfo.PayorID;
                field.AvailableFieldName = payorToolField.FieldName;
                field.EquivalentDeuField = payorToolField.EquivalentDeuField;
                field.EquivalentIncomingField = payorToolField.EquivalentIncomingField;
                field.EquivalentLearnedField = payorToolField.EquivalentLearnedField;

                PayorToolFieldsProperty.Add(field);
                CurrentPayorFieldProperty = field;
                return field;
            }
            catch( Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adding New Available Field to List View.
        /// </summary>
        private void AddField()
        {
            try
            {
                if (IsValidField() == false) return;

                if (FieldsList == null)
                    FieldsList = new ObservableCollection<PayorToolAvailablelFieldType>();

                PayorToolAvailablelFieldType _ObjTemp = new PayorToolAvailablelFieldType
                                                            {
                                                                FieldName = AvaiableFieldName,
                                                                FieldDiscription = "XML DeuField",
                                                                EquivalentDeuField = AvaiableFieldName
                                                            };
                int PayorFieldID =
                    serviceClients.PayorToolAvailablelFieldTypeClient.AddUpdatePayorToolAvailablelFieldType(_ObjTemp);
                _ObjTemp.FieldID = PayorFieldID;
                FieldsList.Add(_ObjTemp);
                DeletableFieldList.Add(_ObjTemp);
            }
            catch(Exception)
            {
                
            }
        }

        /// <summary>
        /// Deleting Available Field From the List View.
        /// </summary>
        private void deleteField()
        {
            try
            {
                if (objLog == null) objLog = new MastersClient();
                if (SelectedDeletableFieldName == null) return;

                MessageBoxResult mdr = MessageBox.Show("Do you want to delete", "information", MessageBoxButton.YesNo);

                if (mdr == MessageBoxResult.No)
                {
                    return;
                }
                if (serviceClients.PayorToolAvailablelFieldTypeClient.DeletePayorToolAvailablelFieldType(SelectedDeletableFieldName))
                {
                    objLog.AddLog("PAyorTool Vm deletefield request:  SelectedDeletableFieldName " + SelectedDeletableFieldName.FieldName + ", User: " + RoleManager.userCredentialID);
                    PayorToolAvailablelFieldType Temp = SelectedDeletableFieldName;

                    FieldsList.Remove(Temp);
                    DeletableFieldList.Remove(Temp);
                }
                else
                {
                    string strCustomFeilds = SelectedDeletableFieldName.FieldName.ToString();
                    MessageBox.Show("Custom  " + strCustomFeilds + " field available in any payor form ","Information");
                }
            }
            catch(Exception ex)
            {
               objLog .AddLog("PAyorTool Vm deletefield exception:  " + ex.Message);
            }
        }

        /// <summary>
        /// Occur when the field is deleted from the form.
        /// </summary>
        /// <param name="payorToolAvailablelFieldType"></param>
        internal void OnFieldDeleted(PayorToolAvailablelFieldType payorToolAvailablelFieldType)
        {
            _PayorToolFieldsPropperty.Remove(_PayorToolFieldsPropperty.FirstOrDefault(s => s.PTAvailableFieldId == payorToolAvailablelFieldType.FieldID));
        }

        internal void OnFieldSelectionChanged(PayorToolAvailablelFieldType selectedField)
        {
            CurrentPayorFieldProperty = _PayorToolFieldsPropperty.FirstOrDefault(s => s.PTAvailableFieldId == selectedField.FieldID);
            
            //Enable and disable grid if no filed available
            if (CurrentPayorFieldProperty != null)
                IsFieldPresent = true;
            else
                IsFieldPresent = false;
        }

        DesignerCanvas m_DesignerCanvas;
        internal void SetCanvas(DesignerCanvas designerCanvas)
        {
            m_DesignerCanvas = designerCanvas;
        }

        internal ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken> getVariableExprTokens()
        {
            ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken> Fields = new ObservableCollection<VM.VMLib.PayorForm.Formula.ExpressionToken>();
            try
            {
                foreach (PayorToolField field in _PayorToolFieldsPropperty)
                {
                    VM.VMLib.PayorForm.Formula.ExpressionToken token = new VM.VMLib.PayorForm.Formula.ExpressionToken { TokenString = field.AvailableFieldName, TokenType = VM.VMLib.PayorForm.Formula.ExpressionTokenType.Variable };
                    Fields.Add(token);
                }
                VM.VMLib.PayorForm.Formula.ExpressionToken expToken = new VM.VMLib.PayorForm.Formula.ExpressionToken();

                expToken.TokenString = "100";
                expToken.TokenType = VM.VMLib.PayorForm.Formula.ExpressionTokenType.Value;

                Fields.Add(expToken);
            }
            catch (Exception)
            {
            }
            return Fields;
        }

        internal Guid getPayorId()
        {
            if (SelectedPayor != null)
                return SelectedPayor.PayorID;
            else
                return Guid.Empty;
        }

        public void Refresh()
        {
            LoadData();
            //Payor SavedPayor = SelectedPayor;

            //using (serviceClients.PayorClient)
            //{
            //    Payors = serviceClients.PayorClient.GetPayorsOnly(Guid.Empty);
            //    if (Payors.Count != 0)
            //        IsPayorPresent = true;
            //    else
            //    {
            //        IsPayorPresent = false;
            //        return;
            //    }
            //    SelectedPayor = Payors.FirstOrDefault(s => s.PayorID == SavedPayor.PayorID);
            //}

        }
    }

    public class PayorToolData : BaseViewModel
    {
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
        public void SetPayorToolData(PayorToolField field)
        {
            LabelOnImage = field.LabelOnField;
            DefaultValue = field.DefaultValue;
            HelpText = field.HelpText;
            MappedIncomingField = field.EquivalentIncomingField;
            MappedLearnedField = field.EquivalentLearnedField;
            MaskedFieldId = field.MaskFieldTypeId;

            if (field.FieldStatusValue == "Required")
            {
                //ChangeInFieldStatus("Visible");               
                VisibleField = true;                
            }
            else
            {
                //ChangeInFieldStatus("NotVisible");
                VisibleField = false;
                
            }

            if (field.IsPartOfPrimaryKey)
            {
                ChangeInPrimaryKey("Yes");
                //PrimaryKeyYesField = true;
            }
            else
            {
                ChangeInPrimaryKey("No");
                //PrimaryKeyYesField = false;
            }

            if (field.IsPopulateIfLinked)
            {
                ChangeInPopulatedIfLink("Yes");
            }
            else
            {
                ChangeInPopulatedIfLink("No");
            }

            if (field.IsTabbedToNextFieldIfLinked)
            {
                ChangeInTabToNext("Yes");
            }
            else
            {
                ChangeInTabToNext("No");
            }

            if (field.IsCalculatedField)
            {
                ChangeInCalculatedField("Yes");
            }
            else
            {
                ChangeInCalculatedField("No");
            }

            if (field.IsOverrideOfCalcAllowed)
            {
                ChangeInOverrideCalcField("Yes");
            }
            else
            {
                ChangeInOverrideCalcField("No");
            }

            if (field.IsZeroorBlankAllowed)
            {
                ChangeInAllowZeroOrBlankField("Yes");
            }
            else
            {
                ChangeInAllowZeroOrBlankField("No");
            }

            if (field.AllignedDirection == "Left")
            {
                ChangeInJustifyField("Yes");
            }
            else
            {
                ChangeInJustifyField("No");
            }

            TabOrder = field.FieldOrder;
        }

        public void SetPayorToolField(PayorToolField payorToolField)
        {
            payorToolField.LabelOnField = LabelOnImage;
            payorToolField.HelpText = HelpText;
            payorToolField.DefaultValue = DefaultValue;

            if (VisibleField)
                payorToolField.FieldStatusValue = "Required";
            else
                payorToolField.FieldStatusValue = "Invisible";

            if (PrimaryKeyYesField)
                payorToolField.IsPartOfPrimaryKey = true;
            else
                payorToolField.IsPartOfPrimaryKey = false;

            if (PopulatedIfLinkYesField)
                payorToolField.IsPopulateIfLinked = true;
            else
                payorToolField.IsPopulateIfLinked = false;

            if (TabToNextYesField)
                payorToolField.IsTabbedToNextFieldIfLinked = true;
            else
                payorToolField.IsTabbedToNextFieldIfLinked = false;

            if (CalculatedYesField)
                payorToolField.IsCalculatedField = true;
            else
                payorToolField.IsCalculatedField = false;

            if (OverrideCalculationYesField)
                payorToolField.IsOverrideOfCalcAllowed = true;
            else
                payorToolField.IsOverrideOfCalcAllowed = false;

            if (AllowZeroBlankYesField)
                payorToolField.IsZeroorBlankAllowed = true;
            else
                payorToolField.IsZeroorBlankAllowed = false;

            if (JustifyLeftField)
                payorToolField.AllignedDirection = "Left";
            else
                payorToolField.AllignedDirection = "Right";

            payorToolField.FieldOrder = TabOrder;
            payorToolField.EquivalentIncomingField = MappedIncomingField;
            payorToolField.EquivalentLearnedField = MappedLearnedField;
            payorToolField.MaskFieldTypeId = MaskedFieldId;
        }

        internal void MaskFieldChanged(PayorToolMaskedFieldType maskedField)
        {
            if (maskedField != null)
            {
                MaskedFieldId = maskedField.maskFieldID;

                if (maskedField.Type != 3)
                {
                    JustifyLeftField = true;
                    if (maskedField.Type == 1)
                    {
                        DefaultValueLabel = "Default(Date)";
                    }
                    else
                    {
                        DefaultValueLabel = "Default(Numeric)";
                    }
                }
                else
                {
                    DefaultValueLabel = "Default(Text)";
                    JustifyLeftField = false;
                }
            }
        }

        #region Index Data

        private int _TabOrder;
        public int TabOrder
        {
            get { return _TabOrder; }
            set
            {
                _TabOrder = value;
                OnPropertyChanged("TabOrder");
            }
        }

        #endregion

        #region Text Data

        private string labelOnImage;
        public string LabelOnImage
        {
            get { return labelOnImage; }
            set 
            {
                labelOnImage = value;
                OnPropertyChanged("LabelOnImage");
            }
        }

        private string defaultValue;
        public string DefaultValue
        {
            get { return defaultValue; }
            set
            {
                defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        private string defaultValueLabel = "Default(Text)";
        public string DefaultValueLabel
        {
            get { return defaultValueLabel; }
            set
            {
                defaultValueLabel = value;
                OnPropertyChanged("DefaultValueLabel");
            }
        }

        private string helpText;
        public string HelpText
        {
            get { return helpText; }
            set
            {
                helpText = value;
                OnPropertyChanged("HelpText");
            }
        }

        private string mappedIncomingField;
        public string MappedIncomingField
        {
            get { return mappedIncomingField; }
            set
            {
                mappedIncomingField = value;
                OnPropertyChanged("MappedIncomingField");
            }
        }

        private string mappedLearnedField;
        public string MappedLearnedField
        {
            get { return mappedLearnedField; }
            set
            {
                mappedLearnedField = value;
                OnPropertyChanged("MappedLearnedField");
            }
        }

        private int maskedFieldId;
        public int MaskedFieldId
        {
            get { return maskedFieldId; }
            set
            {
                maskedFieldId = value;
                OnPropertyChanged("MaskedFieldId");
            }
        }
        #endregion

        #region Radio Button Data

        private bool visibleField;
        public bool VisibleField
        {
            get { return visibleField; }
            set
            {
                visibleField = value;
                NotVisibleField = !visibleField;
                OnPropertyChanged("VisibleField");
            }
        }

        private bool notVisibleField;
        public bool NotVisibleField
        {
            get { return notVisibleField; }
            set
            {
                notVisibleField = value;
                OnPropertyChanged("NotVisibleField");
            }
        }

        private bool primaryKeyYesField;
        public bool PrimaryKeyYesField
        {
            get { return primaryKeyYesField; }
            set
            {
                primaryKeyYesField = value;
                PrimaryKeyNoField = !primaryKeyYesField;
                OnPropertyChanged("PrimaryKeyYesField");
            }
        }

        private bool primaryKeyNoField;
        public bool PrimaryKeyNoField
        {
            get { return primaryKeyNoField; }
            set
            {
                primaryKeyNoField = value;
                OnPropertyChanged("PrimaryKeyNoField");
            }
        }

        private bool populatedIfLinkYesField;
        public bool PopulatedIfLinkYesField
        {
            get { return populatedIfLinkYesField; }
            set
            {
                populatedIfLinkYesField = value;
                PopulatedIfLinkNoField = !populatedIfLinkYesField;
                OnPropertyChanged("PopulatedIfLinkYesField");
            }
        }

        private bool populatedIfLinkNoField;
        public bool PopulatedIfLinkNoField
        {
            get { return populatedIfLinkNoField; }
            set
            {
                populatedIfLinkNoField = value;
                OnPropertyChanged("PopulatedIfLinkNoField");
            }
        }

        private bool tabToNextYesField;
        public bool TabToNextYesField
        {
            get { return tabToNextYesField; }
            set
            {
                tabToNextYesField = value;
                TabToNextNoField = !tabToNextYesField;
                OnPropertyChanged("TabToNextYesField");
            }
        }

        private bool tabToNextNoField;
        public bool TabToNextNoField
        {
            get { return tabToNextNoField; }
            set
            {
                tabToNextNoField = value;
                OnPropertyChanged("TabToNextNoField");
            }
        }

        private bool calculatedYesField;
        public bool CalculatedYesField
        {
            get { return calculatedYesField; }
            set
            {
                calculatedYesField = value;
                CalculatedNoField = !calculatedYesField;
                OnPropertyChanged("CalculatedYesField");
            }
        }

        private bool calculatedNoField;
        public bool CalculatedNoField
        {
            get { return calculatedNoField; }
            set
            {
                calculatedNoField = value;
                OnPropertyChanged("CalculatedNoField");
            }
        }

        private bool overrideCalculationYesField;
        public bool OverrideCalculationYesField
        {
            get { return overrideCalculationYesField; }
            set
            {
                overrideCalculationYesField = value;
                OverrideCalculationNoField = !overrideCalculationYesField;
                OnPropertyChanged("OverrideCalculationYesField");
            }
        }

        private bool overrideCalculationNoField;
        public bool OverrideCalculationNoField
        {
            get { return overrideCalculationNoField; }
            set
            {
                overrideCalculationNoField = value;
                OnPropertyChanged("OverrideCalculationNoField");
            }
        }

        private bool allowZeroBlankYesField;
        public bool AllowZeroBlankYesField
        {
            get { return allowZeroBlankYesField; }
            set
            {
                allowZeroBlankYesField = value;
                AllowZeroBlankNoField = !allowZeroBlankYesField;
                OnPropertyChanged("AllowZeroBlankYesField");
            }
        }

        private bool allowZeroBlankNoField;
        public bool AllowZeroBlankNoField
        {
            get { return allowZeroBlankNoField; }
            set
            {
                allowZeroBlankNoField = value;
                OnPropertyChanged("AllowZeroBlankNoField");
            }
        }

        private bool justifyLeftField;
        public bool JustifyLeftField
        {
            get { return justifyLeftField; }
            set
            {
                justifyLeftField = value;
                JustifyRightField = !justifyLeftField;
                OnPropertyChanged("JustifyLeftField");
            }
        }

        private bool justifyRightField;
        public bool JustifyRightField
        {
            get { return justifyRightField; }
            set
            {
                justifyRightField = value;
                OnPropertyChanged("JustifyRightField");
            }
        }

        #endregion

        #region Command

        #region Field Status Command

        private ICommand _FieldStatusChanged;
        public ICommand FieldStatusChanged
        {
            get
            {
                if (_FieldStatusChanged == null)
                {
                    _FieldStatusChanged = new BaseCommand(param => ChangeInFieldStatus(param));
                }
                return _FieldStatusChanged;
            }

        }

        internal void ChangeInFieldStatus(object param)
        {
            string str = param as string;
            if (str == "Visible")
            {
                VisibleField = true;
                EnableDisablePartPrimaryKey = true;
                
            }
            else
            {
                VisibleField = false;
                if (string.IsNullOrEmpty(DefaultValue))
                {
                    PrimaryKeyYesField = false;
                }
                EnableDisablePartPrimaryKey = false;
            }
            
        }

        #endregion

        #region Primary Key Command

        private ICommand _PrimaryKeyChanged;
        public ICommand PrimaryKeyChanged
        {
            get
            {
                if (_PrimaryKeyChanged == null)
                {
                    _PrimaryKeyChanged = new BaseCommand(param => ChangeInPrimaryKey(param));
                }
                return _PrimaryKeyChanged;
            }

        }

        internal void ChangeInPrimaryKey(object param)
        {
            string str = param as string;
            if (str == "Yes")
            {
                PrimaryKeyYesField = true;

                if (string.IsNullOrEmpty(DefaultValue))
                {
                    VisibleField = true;
                }

                PopulatedIfLinkYesField = false;
                TabToNextYesField = false;
                CalculatedYesField = false;


                EnableDisablePopolatedIfLink = false;
                EnableDisableTabToNextIfLinked = false;
                EnableDisableCalculatedField = false;

                //Enable and disable allow zero or blank according to primary key
                AllowZeroBlankYesField = false;
                //Enable and disable allow zero or blank to select 
                EnableDisablePrimaryKey = false;


            }
            else
            {
                PrimaryKeyYesField = false;

                EnableDisablePopolatedIfLink = true;
                if (TabToNextYesField)
                {
                    EnableDisableTabToNextIfLinked = true;
                }
                else
                {
                    EnableDisableTabToNextIfLinked = false;
                }               
                CalculatedYesField = false;
                EnableDisableCalculatedField = true;


                //Enable and disable allow zero or blank to select 
                EnableDisablePrimaryKey = true;
            }
        }

        #endregion

        #region Populated If Link Command

        private ICommand _PopIfLink;
        public ICommand PopulateInLinked
        {
            get
            {
                if (_PopIfLink == null)
                {
                    _PopIfLink = new BaseCommand(param => ChangeInPopulatedIfLink(param));
                }
                return _PopIfLink;
            }

        }

        internal void ChangeInPopulatedIfLink(object param)
        {
            string str = param as string;
            if (str == "Yes")
            {
                PopulatedIfLinkYesField = true;
                EnableDisableTabToNextIfLinked = true;
            }
            else
            {
                PopulatedIfLinkYesField = false;

                TabToNextYesField = false;
                EnableDisableTabToNextIfLinked = false;
            }
        }

        #endregion

        #region Tab To Next Field Command

        private ICommand _TabToNextField;
        public ICommand TabToNextField
        {
            get
            {
                if (_TabToNextField == null)
                {
                    _TabToNextField = new BaseCommand(param => ChangeInTabToNext(param));
                }
                return _TabToNextField;
            }

        }

        internal void ChangeInTabToNext(object param)
        {
            string str = param as string;
            if (str == "Yes")
                TabToNextYesField = true;
            else
                TabToNextYesField = false;
        }

        #endregion

        #region Calculated Field Command

        private ICommand _CalcField;
        public ICommand CalculatedField
        {
            get
            {
                if (_CalcField == null)
                {
                    _CalcField = new BaseCommand(param => ChangeInCalculatedField(param));
                }
                return _CalcField;
            }

        }

        internal void ChangeInCalculatedField(object param)
        {
            string str = param as string;
            if (str == "Yes")
            {
                CalculatedYesField = true;

                EnableDisableAllowOverrideCalculation = true;
                EnableDisableAddFormula = true;
                EnableDisableDeleteFormula = true;
            }
            else
            {
                CalculatedYesField = false;

                OverrideCalculationYesField = false;

                EnableDisableAllowOverrideCalculation = false;
                EnableDisableAddFormula = false;
                EnableDisableDeleteFormula = false;
            }
        }
        
        #endregion

        #region Override Calculation Field Command

        private ICommand _OverrideCalcField;
        public ICommand OverrideCalculatedField
        {
            get
            {
                if (_OverrideCalcField == null)
                {
                    _OverrideCalcField = new BaseCommand(param => ChangeInOverrideCalcField(param));
                }
                return _OverrideCalcField;
            }

        }

        internal void ChangeInOverrideCalcField(object param)
        {
            string str = param as string;
            if (str == "Yes")
                OverrideCalculationYesField = true;
            else
                OverrideCalculationYesField = false;
        }

        #endregion

        #region AllowZeroOrBlank Field Command

        private ICommand _AllowZeroOrBlank;
        public ICommand AllowZeroOrBlank
        {
            get
            {
                if (_AllowZeroOrBlank == null)
                {
                    _AllowZeroOrBlank = new BaseCommand(param => ChangeInAllowZeroOrBlankField(param));
                }
                return _AllowZeroOrBlank;
            }

        }

        internal void ChangeInAllowZeroOrBlankField(object param)
        {
            string str = param as string;
            if (str == "Yes")
                AllowZeroBlankYesField = true;
            else
                AllowZeroBlankYesField = false;
        }

        #endregion

        #region Justify Field Command

        private ICommand _Justify;
        public ICommand Justify
        {
            get
            {
                if (_Justify == null)
                {
                    _Justify = new BaseCommand(param => ChangeInJustifyField(param));
                }
                return _Justify;
            }

        }

        internal void ChangeInJustifyField(object param)
        {
            string str = param as string;
            if (str == "Yes")
                JustifyLeftField = true;
            else
                JustifyLeftField = false;
        }

        #endregion
        #endregion

        #region Enable Disable Variables

        private bool _enableDisableStatusField = true;
        public bool EnableDisableStatusField
        {
            get { return _enableDisableStatusField; }
            set
            {
                _enableDisableStatusField = value;
                OnPropertyChanged("EnableDisableStatusField");
            }
        }

        private bool _enableDisablePartPrimaryKey = true;
        public bool EnableDisablePartPrimaryKey
        {
            get { return _enableDisablePartPrimaryKey; }
            set
            {
                _enableDisablePartPrimaryKey = value;
                OnPropertyChanged("EnableDisablePartPrimaryKey");
            }
        }

        private bool _enableDisablePopolatedIfLink = true;
        public bool EnableDisablePopolatedIfLink
        {
            get { return _enableDisablePopolatedIfLink; }
            set
            {
                _enableDisablePopolatedIfLink = value;
                OnPropertyChanged("EnableDisablePopolatedIfLink");
            }
        }

        private bool _enableDisableTabToNextIfLinked = true;
        public bool EnableDisableTabToNextIfLinked
        {
            get { return _enableDisableTabToNextIfLinked; }
            set
            {
                _enableDisableTabToNextIfLinked = value;
                OnPropertyChanged("EnableDisableTabToNextIfLinked");
            }
        }

        private bool _enableDisableCalculatedField = true;
        public bool EnableDisableCalculatedField
        {
            get { return _enableDisableCalculatedField; }
            set
            {
                _enableDisableCalculatedField = value;
                OnPropertyChanged("EnableDisableCalculatedField");
            }
        }

        private bool _enableDisableAllowOverrideCalculation = true;
        public bool EnableDisableAllowOverrideCalculation
        {
            get { return _enableDisableAllowOverrideCalculation; }
            set
            {
                _enableDisableAllowOverrideCalculation = value;
                OnPropertyChanged("EnableDisableAllowOverrideCalculation");
            }
        }

        private bool _enableDisableAddFormula = true;
        public bool EnableDisableAddFormula
        {
            get { return _enableDisableAddFormula; }
            set
            {
                _enableDisableAddFormula = value;
                OnPropertyChanged("EnableDisableAddFormula");
            }
        }

        private bool _enableDisableDeleteFormula = true;
        public bool EnableDisableDeleteFormula
        {
            get { return _enableDisableDeleteFormula; }
            set
            {
                _enableDisableDeleteFormula = value;
                OnPropertyChanged("EnableDisableDeleteFormula");
            }
        }

        private bool _EnableDisablePrimaryKey = true;
        public bool EnableDisablePrimaryKey
        {
            get { return _EnableDisablePrimaryKey; }
            set
            {
                _EnableDisablePrimaryKey = value;
                OnPropertyChanged("EnableDisablePrimaryKey");
            }
        }

        

        #endregion
    }
}
