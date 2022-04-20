using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.BaseVM;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM;
using System.Data;
using System.Windows.Controls;
using MyAgencyVault.VM.VMLib.PayorForm.Evalutor;
using MyAgencyVault.ViewModel.Converters;
using System.Windows;
using MyAgencyVault.VM.VMLib.PayorForm.Formula;
using MyAgencyVault.ViewModel.Behaviour;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using MyAgencyVault.ViewModel.VMLib;
//using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Threading;

namespace MyAgencyVault.VM.VMLib.DEU
{
  public class DeuFieldCollection
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
    private ObservableCollection<DeuField> _DeuFields;
    public ObservableCollection<DeuField> DeuFields
    {
      get { return _DeuFields; }
      set { _DeuFields = value; }
    }

    public DeuField this[int index]
    {
      get
      {
        if (_DeuFields.Count > index && index >= 0)
          return _DeuFields[index];
        else
          return null;
      }
    }

    public int Count
    {
      get
      {
        if (_DeuFields != null)
          return _DeuFields.Count;
        else
          return 0;
      }
    }

    private DeuField _FirstDeuField;
    public DeuField FirstDeuField
    {
      get { return _FirstDeuField; }
      set { _FirstDeuField = value; }
    }

    public bool ValidateDeuFields()
    {
      if (DeuFields == null)
        return false;

      bool ValidationSuccess = true;
      string carrierNickName = string.Empty;
      string strInvoice = string.Empty;
      string strEffective = string.Empty;

      try
      {
        foreach (DeuField field in DeuFields)
        {
          ValidationSuccess = true;
          ValidateDeuField validateDeuField = new ValidateDeuField();
          if (!field.PayorToolField.IsNotVisible)
          {
            if (field.PayorToolField.MaskFieldType == 1)
            {
              ValidationSuccess = validateDeuField.ValidateDate(field.Text, field.PayorToolField.MaskText);
            }
            else if (field.PayorToolField.MaskFieldType == 2)
            {
              ValidationSuccess = validateDeuField.ValidateNumber(field.Text, field.PayorToolField.MaskText, field.PayorToolField.IsZeroorBlankAllowed);
            }
            else if (field.PayorToolField.IsNotVisible)
            {
              ValidationSuccess = validateDeuField.ValidateDate(Convert.ToString(System.DateTime.Now), "MM-dd-yyyy");
            }

            else
            {
              if (!field.PayorToolField.IsZeroorBlankAllowed)
                ValidationSuccess = !string.IsNullOrEmpty(field.Text.Trim());
            }
          }

          if (ValidationSuccess == false)
            break;
        }
      }
      catch
      {
      }
      return ValidationSuccess;
    }

    public bool ValidateEffectiveDate()
    {
        if (DeuFields == null)
            return false;

        bool ValidationSuccess = true;        
        string strInvoice = string.Empty;
        string strEffective = string.Empty;

            string maskEffective = string.Empty;
            string maskInvoice = string.Empty;

        try
        {
            foreach (DeuField field in DeuFields)
            {
                ValidationSuccess = true;
                ValidateDeuField validateDeuField = new ValidateDeuField();
                if (!field.PayorToolField.IsNotVisible)
                {
                    if (field.PayorToolField.MaskFieldType == 1)
                    {
                            if (field.PayorToolField.AvailableFieldName.ToString() == "InvoiceDate")
                            {
                                strInvoice = field.Text;
                                maskInvoice = field.PayorToolField.MaskText;
                            }
                            if (field.PayorToolField.AvailableFieldName.ToString() == "EffectiveDate")
                            {
                                strEffective = field.Text;
                                maskEffective = field.PayorToolField.MaskText;
                            }
                            

                        if ((!string.IsNullOrEmpty(strEffective)) && (!string.IsNullOrEmpty(strInvoice)))
                        {
                            DateTime dtEffective;
                            DateTime dtInvoice;
                            try
                            {
                                string strMask = field.PayorToolField.MaskText;
                                dtEffective = !string.IsNullOrEmpty(maskEffective) ? DateTime.ParseExact(strEffective, maskEffective, System.Globalization.CultureInfo.InvariantCulture) : Convert.ToDateTime(strEffective);
                                dtInvoice = !string.IsNullOrEmpty(maskInvoice) ? DateTime.ParseExact(strInvoice, maskInvoice, System.Globalization.CultureInfo.InvariantCulture) : Convert.ToDateTime(strInvoice);
                            }
                            catch (Exception ex)
                            {
                                 dtEffective = Convert.ToDateTime(strEffective);
                                 dtInvoice = Convert.ToDateTime(strInvoice);
                            }
                            //DateTime dt = DateTime.ParseExact("01/13", "MM/yy", System.Globalization.CultureInfo.InvariantCulture);

                            if (dtEffective > dtInvoice)
                                ValidationSuccess = false;
                        }

                    }
                }
                if (ValidationSuccess == false)
                    break;
            }
        }
        catch (Exception)
        {
            ValidationSuccess = false;
        }
        return ValidationSuccess;
    }

    public bool ValidateCompSchuduleType()
    {
        string strLenghth = string.Empty;
        if (DeuFields == null)
            return false;

        bool ValidationSuccess = true;

        try
        {
            foreach (DeuField field in DeuFields)
            {
                ValidationSuccess = true;
                ValidateDeuField validateDeuField = new ValidateDeuField();

                if (field.PayorToolField.AvailableFieldName.ToString() == "CompScheduleType")
                {
                    strLenghth = field.Text;

                    if (strLenghth.Length > 75)
                    {
                        ValidationSuccess = false;
                    }
                }

                if (ValidationSuccess == false)
                    break;
            }
        }
        catch (Exception)
        {
        }
        return ValidationSuccess;
    }
         
    public bool ValidateCompSchuduleForEmpty()
    {

        if (DeuFields == null)
            return false;

        bool ValidationSuccess = true;

        try
        {
            foreach (DeuField field in DeuFields)
            {
                ValidationSuccess = true;
                ValidateDeuField validateDeuField = new ValidateDeuField();

                if (field.Text == "" || field.Text == string.Empty)
                {
                    ValidationSuccess = false;

                }

                if (field.PayorToolField.AvailableFieldName.ToString() == "InvoiceDate")
                {
                    if (field.Text == "___-__" || field.Text == "___-__")
                    {
                        ValidationSuccess = false;

                    }
                }


                if (ValidationSuccess == false)
                    break;
            }
        }
        catch (Exception)
        {
        }
        return ValidationSuccess;
    }

    public DeuFieldCollection(PayorTool payorTool)
    {
      PayorTool = payorTool;

      try
      {
        List<PayorToolField> ToolFields = payorTool.ToolFields.OrderBy(s => s.FieldOrder).OrderBy(s => !s.IsPartOfPrimaryKey).ToList();
        PayorToolField lastPrimaryKey = ToolFields.FindLast(s => s.IsPartOfPrimaryKey);

        DeuField.SearchPolicyOnLostFocusFieldName = null;

        List<PayorToolField> allPrimaryKey = ToolFields.OrderBy(s => s.IsPartOfPrimaryKey).ToList();
        DeuField.allPrimaryKeyIsVisble = true;

        foreach (var item in allPrimaryKey)
        {
            if (item.IsPartOfPrimaryKey)
            {
                if (item.FieldStatusValue == "Invisible")
                    DeuField.allPrimaryKeyIsVisble = false;
                else
                {
                    DeuField.allPrimaryKeyIsVisble = true;
                    break;
                }
            }
        }


        if (lastPrimaryKey != null)
          DeuField.SearchPolicyOnLostFocusFieldName = lastPrimaryKey.AvailableFieldName;

        if (payorTool != null && payorTool.ToolFields != null && payorTool.ToolFields.Count != 0)
        {
          DeuField FieldTextBox = null;
          DeuField PreviousDeuField = null;

          _DeuFields = new ObservableCollection<DeuField>();
          foreach (PayorToolField pField in ToolFields)
          {
            FieldTextBox = new DeuField(pField);

            if (PreviousDeuField != null)
              PreviousDeuField.NextDeuField = FieldTextBox;
            else
              FirstDeuField = FieldTextBox;

            PreviousDeuField = FieldTextBox;
            DeuFields.Add(FieldTextBox);
          }
        }
      }
      catch (Exception)
      {
      }
    }

    public PayorTool PayorTool { get; set; }
  }
}
