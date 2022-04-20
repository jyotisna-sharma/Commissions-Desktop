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
using MyAgencyVault.ViewModel;
 

namespace MyAgencyVault.VM.VMLib.DEU
{
  public class DeuFormBuilder
  {
    private static DeuFormBuilder deuFormBuilder = null;
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
    public void ResetDeuFormBuilder()
    {
      deuFormBuilder = null;
    }

    private Canvas _ImageCanvas;
    public Canvas ImageCanvas
    {
      get
      {
        return _ImageCanvas;
      }
      set
      {
        _ImageCanvas = value;
      }
    }

    private DeuFieldCollection _DeuFields;
    public DeuFieldCollection DeuFields
    {
      get
      {
        return _DeuFields;
      }
      set
      {
        _DeuFields = value;
      }
    }

    private PayorTool _PayorTool;
    public PayorTool PayorTool
    {
      get
      {
        return _PayorTool;
      }
      set
      {
        _PayorTool = value;
        RemoveDeuFieldsFromCanvas();
        if (_PayorTool != null)
        {
          _DeuFields = new DeuFieldCollection(_PayorTool);
          AddDeuFieldsOnCanvas();
        }
      }
    }

    private VMDeu _VmDeu;
    public VMDeu VmDeu
    {
      get { return _VmDeu; }
      set { _VmDeu = value; }
    }

    private void RemoveDeuFieldsFromCanvas()
    {
      if (ImageCanvas != null && ImageCanvas.Children != null)
        ImageCanvas.Children.RemoveRange(1, ImageCanvas.Children.Count - 1);

      if (VmDeu.SearchedPolicy != null)
        VmDeu.SearchedPolicy = null;
    }

    public static DeuFormBuilder getDeuFormBuilder(VMDeu VmDeu)
    {
      if (deuFormBuilder == null)
      {
        deuFormBuilder = new DeuFormBuilder(VmDeu);
        getDeuFormBuilder().serviceClients.PostUtilClient.GetPoliciesFromUniqueIdentifierCompleted += new EventHandler<VM.MyAgencyVaultSvc.GetPoliciesFromUniqueIdentifierCompletedEventArgs>(deuFormBuilder.PostUtilClient_GetPoliciesFromUniqueIdentifierCompleted);
      }
      return deuFormBuilder;
    }

    void PostUtilClient_GetPoliciesFromUniqueIdentifierCompleted(object sender, VM.MyAgencyVaultSvc.GetPoliciesFromUniqueIdentifierCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        this.VmDeu.SearchedPolicy = e.Result;
      }
    }

    public static DeuFormBuilder getDeuFormBuilder()
    {
      if (deuFormBuilder != null)
        return deuFormBuilder;
      else
        return null;
    }

    private DeuFormBuilder(VMDeu VmDeu)
    {
      this.VmDeu = VmDeu;
    }

    public void AddDeuFieldsOnCanvas()
    {
      try
      {
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField deuField = DeuFields[index];

          Canvas.SetLeft(deuField, deuField.PayorToolField.ControlX);
          //Canvas.SetTop(deuField, deuField.PayorToolField.ControlY);
          Canvas.SetTop(deuField, deuField.PayorToolField.ControlY + 3);
          deuField.Width = deuField.PayorToolField.ControlWidth;
          deuField.Height = deuField.PayorToolField.ControlHeight;
            //set the font size 9
          deuField.FontSize = 9;
          _ImageCanvas.Children.Add(deuField);

          if (deuField.DeuFieldLabel != null)
          {
            Canvas.SetLeft(deuField.DeuFieldLabel, deuField.PayorToolField.ControlX);
            //Canvas.SetTop(deuField.DeuFieldLabel, deuField.PayorToolField.ControlY - 18);
            Canvas.SetTop(deuField.DeuFieldLabel, deuField.PayorToolField.ControlY - 10);
            deuField.DeuFieldLabel.FontSize = 9;
            _ImageCanvas.Children.Add(deuField.DeuFieldLabel);
          }
        }
      }
      catch
      {
      }
    }

    public string getExpression(PayorToolField deuPayorToolField)
    {
      if (DeuFields.Count == 0)
        return string.Empty;

      string Expression = deuPayorToolField.CalculationFormula.FormulaExpression;
      try
      {
        ExpressionStack expressionStack = new ExpressionStack(Expression);
        List<ExpressionToken> expTokens = expressionStack.getExpressionTokenList();

        if (expTokens != null)
        {
          expTokens = expTokens.Where(s => s.TokenType == ExpressionTokenType.Variable).Distinct().OrderByDescending(s => s.TokenString.Length).ToList();
          foreach (ExpressionToken token in expTokens)
          {
            if (token.TokenType == ExpressionTokenType.Variable)
            {
              if (Expression.Contains(token.TokenString))
              {
                DeuField field = DeuFields.DeuFields.FirstOrDefault(s => s.PayorToolField.AvailableFieldName.Trim() == token.TokenString.Trim());
                string value = field.Text.Replace("$", "");
                value = value.Replace("%", "");
                value = value.Replace(",", "");
                Expression = Expression.Replace(token.TokenString, field.decimalValue.ToString());
              }
            }
          }
        }
      }
      catch (Exception)
      {
      }

      return Expression;
    }

    public void GetSearchedPolicies()
    {
      //try
      //{
      //    ObservableCollection<UniqueIdenitfier> uniqIds = this.getUniqueIdentifiers();
      //    UniqueIdenitfier Id = uniqIds.FirstOrDefault(s => string.IsNullOrEmpty(s.Text));

      //    if (Id == null && this.VmDeu.CurrentBatch != null)
      //    {
      //             serviceClients.PostUtilClient.GetPoliciesFromUniqueIdentifierAsync(uniqIds, this.VmDeu.CurrentBatch.LicenseeId, this.VmDeu.CurrentPayor.PayorID);
      //    }
      //    else
      //        this.VmDeu.SearchedPolicy = null;
      //}
      //catch (Exception ex)
      //{
      //}
    }

    public void HideFields()
    {
      try
      {
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField field = DeuFields[index];
          field.PayorToolField.IsNotVisible = true;
        }
      }
      catch (Exception)
      {
      }
    }

    public void ShowFields()
    {
      try
      {
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField field = DeuFields[index];
          if (field.PayorToolField.FieldStatusValue == "Required")
            field.PayorToolField.IsNotVisible = false;
        }
      }
      catch (Exception)
      {
      }
    }

    public void PostResetFields()
    {
        try
        {
            for (int index = 0; index < DeuFields.Count; index++)
            {
                DeuField field = DeuFields[index];

                if (string.IsNullOrEmpty(field.Text))
                {
                    field.Text = field.PayorToolField.DefaultValue;                    
                    DeuFields.FirstDeuField.Focus();
                }  
            }

            if (DeuFields != null && DeuFields.FirstDeuField != null)
                DeuFields.FirstDeuField.Focus();
        }
        catch (Exception)
        {
        }
    }

    public void ResetFields()
    {
      try
      {
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField field = DeuFields[index];
          field.Text = field.PayorToolField.DefaultValue;
        }

        if (DeuFields != null && DeuFields.FirstDeuField != null)
          DeuFields.FirstDeuField.Focus();
      }
      catch (Exception)
      {
      }
    }

    public bool ValidateFields()
    {
      if (DeuFields != null)
        return DeuFields.ValidateDeuFields();
      else
        return true;
    }

    public bool ValidateEffectiveFields()
    {
        if (DeuFields != null)
            return DeuFields.ValidateEffectiveDate();
        else
            return true;
    }

    public bool ValidateCompSchuduleType()
    {
        if (DeuFields != null)
            return DeuFields.ValidateCompSchuduleType();
        else
            return true;
    }
    
    public bool ValidateCompSchuduleForEmpty()
    {
        if (DeuFields != null)
            return DeuFields.ValidateCompSchuduleForEmpty();
        else
            return true;
    }

    public void setDeuFormFieldsValue(Guid DeuEntryId)
    {
      try
      {
        List<VM.MyAgencyVaultSvc.DataEntryField> deuData = serviceClients.DeuClient.GetDeuFields(DeuEntryId).ToList();
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField field = DeuFields[index];
          VM.MyAgencyVaultSvc.DataEntryField singleField = deuData.FirstOrDefault(s => field.PayorToolField.AvailableFieldName == s.DeuFieldName);

          if (singleField != null)
          {
            string formatedValue = getFormattedValue(field.PayorToolField, singleField.DeuFieldValue);
            field.Text = formatedValue;
          }
          else
            field.Text = string.Empty;
        }

        if (DeuFields != null && DeuFields.FirstDeuField != null)
          DeuFields.FirstDeuField.Focus();
      }
      catch (Exception)
      {
      }
    }

    private string getFormattedValue(VM.MyAgencyVaultSvc.PayorToolField payorToolField, string value)
    {
      if (payorToolField.MaskText == "Text")
        return value;

      string retValue = string.Empty;
      try
      {
        if (payorToolField.MaskFieldType == 1)
        {
          DateTime date = DateTime.ParseExact(value, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
          string dateText = date.ToString(payorToolField.MaskText);
          retValue = dateText;
        }
        else
        {
          ValidateDeuField validateDeuField = new ValidateDeuField();
          string format = validateDeuField.getNumberFormat(payorToolField.MaskText);
          double dblvalue = 0;
          if (double.TryParse(value, out dblvalue))
          {
            if (payorToolField.MaskText.Contains("%"))
            {
              retValue = string.Format(format, dblvalue / 100);
            }
            else
            {
              retValue = string.Format(format, dblvalue);
            }
          }
        }
      }
      catch (Exception)
      {
      }

      return retValue;
    }

    public bool IsCarrierPrimaryKey()
    {
      bool IsCarrierPrimaryKey = false;
      try
      {
        if (DeuFields != null && DeuFields.DeuFields != null)
        {
          DeuField field = DeuFields.DeuFields.FirstOrDefault(s => s.PayorToolField.AvailableFieldName == "Carrier");

          if (field != null)
            return field.PayorToolField.IsPartOfPrimaryKey;
        }

      }
      catch (Exception)
      {
      }
      return IsCarrierPrimaryKey;
    }

    public bool IsCoveragePrimaryKey()
    {
      bool IsCoveragePrimaryKey = false;
      if (DeuFields != null && DeuFields.DeuFields != null)
      {
        DeuField field = DeuFields.DeuFields.FirstOrDefault(s => s.PayorToolField.AvailableFieldName == "Product");

        if (field != null)
          return field.PayorToolField.IsPartOfPrimaryKey;
      }

      return IsCoveragePrimaryKey;
    }

    public List<DataEntryField> getDueFormFieldsValue()
    {
      List<DataEntryField> dueFields = new List<DataEntryField>();
      DataEntryField deuField = null;
      try
      {
        for (int index = 0; index < DeuFields.Count; index++)
        {
          DeuField field = DeuFields[index];

          PayorToolField PTField = field.PayorToolField;
          deuField = new DataEntryField
          {
            DeuFieldName = PTField.AvailableFieldName,
            DeuFieldType = field.PayorToolField.MaskFieldType,
          };

          if (deuField.DeuFieldType == 1)
          {
            DateTime value = new DateTime();
            //Replace "*" by blank
            PTField.MaskText = PTField.MaskText.Replace("*", "");

            if (PTField.FieldStatusValue == "Invisible")
            {
              //field.Text = Convert.ToString(System.DateTime.Now);
              //PTField.MaskText="MM/dd/yyyy";
              value = System.DateTime.Now;
            }
            else
            {
              value = DateTime.ParseExact(field.Text, PTField.MaskText, DateTimeFormatInfo.InvariantInfo);
            }
            deuField.DeuFieldValue = value.ToString("MM/dd/yyyy");

          }
          else if (deuField.DeuFieldType == 2)
          {
              string value = field.decimalValue.ToString().Replace("$", "");
              value = value.Replace("%", "");
              value = value.Replace(",", "");
              deuField.DeuFieldValue = value.Trim();
              //string value = field.decimalValue.ToString();
              //deuField.DeuFieldValue = value;
          }
          else
          {
              if (PTField.AvailableFieldName == "CompScheduleType")
              {
                  deuField.DeuFieldValue = field.Text.Trim();
              }
              else
              {
                  string value = field.Text.Replace("$", "");
                  value = value.Replace("%", "");
                  value = value.Replace(",", "");
                  deuField.DeuFieldValue = value.Trim();
              }
          }
          dueFields.Add(deuField);
        }
      }
      catch (Exception)
      {
      }
      return dueFields;
    }

    internal ObservableCollection<UniqueIdenitfier> getUniqueIdentifiers()
    {
      ObservableCollection<UniqueIdenitfier> uniqueIdentifiers = new ObservableCollection<UniqueIdenitfier>();
      UniqueIdenitfier uniqueIdentifier = null;
      try
      {
          List<DeuField> Fields = null;

          if (DeuFields.DeuFields != null)
              Fields = DeuFields.DeuFields.Where(s => s.PayorToolField.IsPartOfPrimaryKey).ToList();

          if (Fields != null)
          {
              string FieldValue = string.Empty;
              foreach (DeuField field in Fields)
              {
                  if (field.PayorToolField.MaskFieldType == 2)
                  {
                      FieldValue = field.decimalValue.ToString();
                  }
                  else
                  {
                      FieldValue = field.Text;
                  }
                  uniqueIdentifier = new UniqueIdenitfier
                  {
                      ColumnName = field.PayorToolField.EquivalentLearnedField,
                      Text = FieldValue,
                      MaskedText = field.PayorToolField.MaskText
                  };
                  uniqueIdentifiers.Add(uniqueIdentifier);

              }
          }
      }
      catch (Exception)
      {
      }

      return uniqueIdentifiers;
    }
  }
}
