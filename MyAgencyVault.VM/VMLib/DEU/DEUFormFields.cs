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
  public class DeuField : MaskingHelper
  {
    public Label DeuFieldLabel;
    public PayorToolField PayorToolField;
    public DeuField NextDeuField;
    public double decimalValue;
    public static string SearchPolicyOnLostFocusFieldName;
    public static bool allPrimaryKeyIsVisble;
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
    public DeuField(PayorToolField payorToolField)
    {
      try
      {
        PayorToolField = payorToolField;

        if (payorToolField.FieldStatusValue == "Required")
        {
          DeuFieldLabel = new Label();
          DeuFieldLabel.Content = PayorToolField.LabelOnField;

        }
        else
        {
          DeuFieldLabel = null;
          PayorToolField.IsNotVisible = true;
        }
        SetTextBoxPropertyFromToolField();

        this.GotFocus += new System.Windows.RoutedEventHandler(DeuField_GotFocus);
        this.LostFocus += new System.Windows.RoutedEventHandler(DeuField_LostFocus);
        //this.MouseEnter+=new MouseEventHandler(DeuField_MouseEnter);
        //this.GotKeyboardFocus+=new KeyboardFocusChangedEventHandler(DeuField_GotKeyboardFocus);
        //this.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(DeuField_KeyBoardFocus);              
         
        //bind textbox with tempPayorToolField.IsNotVisible field
        System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
        binding.Source = payorToolField;
        binding.Path = new PropertyPath("IsNotVisible");
        binding.Converter = new VisibilityConverter();
        this.SetBinding(TextBox.VisibilityProperty, binding);
        if (this.DeuFieldLabel != null)
          this.DeuFieldLabel.SetBinding(Label.VisibilityProperty, binding);


      }
      catch
      {
      }
    }

    private bool isSearch(PayorToolField payorToolField)
    {
      return true;
    }

    void DeuField_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
      DeuField deuField = sender as DeuField;
      PayorToolField deuPayorToolField = deuField.PayorToolField;

      if (deuPayorToolField.AvailableFieldName == "PolicyNumber")
      {
        deuField.Text = VMHelper.CorrectPolicyNo(deuField.Text);
      }

      DeuFormBuilder formBuilder = DeuFormBuilder.getDeuFormBuilder();
      ObservableCollection<UniqueIdenitfier> uniqIds = formBuilder.getUniqueIdentifiers();
      UniqueIdenitfier Id = uniqIds.FirstOrDefault(s => string.IsNullOrEmpty(s.Text));
        if (uniqIds.Count > 0)
        {
          if (Id == null && formBuilder.VmDeu.CurrentBatch != null)
          {
            try
            {
                if (deuField.PayorToolField.IsPartOfPrimaryKey)
                {
                    Guid guLienceID = new Guid();
                    guLienceID = formBuilder.VmDeu.CurrentBatch.LicenseeId;
                    Guid guPayorID = new Guid();
                    guPayorID = formBuilder.VmDeu.CurrentPayor.PayorID;
                    formBuilder.VmDeu.SearchedPolicy = serviceClients.PostUtilClient.GetPoliciesFromUniqueIdentifier(uniqIds, guLienceID, guPayorID);
                }
            }
            catch
            {
            }
          }
          else
            formBuilder.VmDeu.SearchedPolicy = null;
        }
      

      if (deuPayorToolField.MaskFieldType == 2)
      {
        ValidateDeuField validateDeuField = new ValidateDeuField();
        string format = validateDeuField.getNumberFormat(deuPayorToolField.MaskText);
        double value = validateDeuField.GetNumberFromFormatString(deuField.Text, deuPayorToolField.MaskText,deuField.decimalValue);

        if (deuPayorToolField.MaskText.Contains("%"))
        {
          deuField.Text = string.Format(format, value / 100);
        }
        else
        {
          
          deuField.Text = string.Format(format, value);
        }
        deuField.decimalValue = value;
      }

      #region "Check date formate MM/dd/yyyy*"
      if (deuPayorToolField.MaskFieldType == 1)
      {
        string format = deuPayorToolField.MaskText;
        string value = deuField.Text;

        if (deuPayorToolField.MaskText.Contains("*"))
        {
          if (!value.Contains("_"))
          {
            if (value != null)
            {
              DateTime dtInvoiceDate = Convert.ToDateTime(value);
              dtInvoiceDate = dtInvoiceDate.AddMonths(-1);
              deuField.Text = dtInvoiceDate.ToString("MM/dd/yyyy");
            }
          }
        }

      }
      #endregion
    }

    

    List<string> AutoCompleteValues = null;
    void DeuField_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {

        try
        {
            DeuField deuField = sender as DeuField;
            PayorToolField deuPayorToolField = deuField.PayorToolField;

            VMInstances.DeuVM.bindHelpText = deuPayorToolField.HelpText;

            if (VMInstances.DeuVM != null)
            {
                if (VMInstances.DeuVM.CurrentBatch != null)
                {
                    if (deuPayorToolField.AvailableFieldName == "Carrier")
                    {
                        if (VMInstances.DeuVM.CurrentPayor != null)
                            AutoCompleteValues = serviceClients.CarrierClient.GetPayorCarriersOnly(VMInstances.DeuVM.CurrentPayor.PayorID).Select(s => s.NickName).ToList();
                    }
                    if (AutoCompleteValues != null)
                        AutoCompleteValues.RemoveAll(s => string.IsNullOrEmpty(s));

                    TextBoxAutoComplete.SetWordAutoCompleteSource(this, AutoCompleteValues);
                }
            }

            if (deuPayorToolField.IsCalculatedField)
            {
                string Expression = DeuFormBuilder.getDeuFormBuilder().getExpression(deuPayorToolField);
                var ResultValue = new NCalc.Expression(Expression).Evaluate();
                if (ResultValue.ToString().Contains("Infinity") || ResultValue.ToString().Contains("NaN"))
                    ResultValue = 0;
                //ExpressionResult result = ExpressionExecutor.ExecuteExpression(Expression);
                this.Text = ResultValue.ToString();

                if (deuPayorToolField.MaskFieldType == 2)
                {
                    ValidateDeuField validateDeuField = new DEU.ValidateDeuField();
                    string format = validateDeuField.getNumberFormat(deuPayorToolField.MaskText);
                    double value = 0;

                    if (double.TryParse(this.Text, out value))
                    {
                        if (deuPayorToolField.MaskText.Contains("%"))
                        {
                            this.Text = string.Format(format, value / 100);
                        }
                        else
                        {

                            this.Text = string.Format(format, value);
                        }
                        this.decimalValue = value;
                    }
                }

                if (!deuPayorToolField.IsOverrideOfCalcAllowed)
                {
                    if (NextDeuField != null)
                        NextDeuField.Focus();
                }
            }
            else if (deuPayorToolField.IsPopulateIfLinked)
            {
                DeuFormBuilder formBuilder = DeuFormBuilder.getDeuFormBuilder();
                if (formBuilder.VmDeu.SearchedPolicy != null && formBuilder.VmDeu.SearchedPolicy.Count == 1)
                {
                    PolicyLearnedFieldData policyLearnedField = serviceClients.PolicyLearnedFieldClient.GetPolicyLearnedFieldsPolicyWise(formBuilder.VmDeu.SearchedPolicy[0].PolicyId);
                    MapTextToLearnedField(policyLearnedField);
                    if (deuPayorToolField.IsTabbedToNextFieldIfLinked)
                    {
                        if (NextDeuField != null)
                            NextDeuField.Focus();
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Text))
                    this.SelectAll();
            }

           
        }
        catch (Exception)
        {
        }
    }

    private void MapTextToLearnedField(PolicyLearnedFieldData policyLearnedField)
    {
      try
      {
        if (policyLearnedField != null)
        {
          switch (this.PayorToolField.EquivalentLearnedField)
          {
            case "Insured":
              this.Text = policyLearnedField.Insured;
              break;
            case "PolicyNumber":
              this.Text = policyLearnedField.PolicyNumber;
              break;
            case "Effective":
              if (this.PayorToolField.MaskText == "Text")
                this.Text = (policyLearnedField.Effective != null ? policyLearnedField.Effective.Value.ToString("MM/dd/yyyy") : null);
              else
                this.Text = (policyLearnedField.Effective != null ? policyLearnedField.Effective.Value.ToString(this.PayorToolField.MaskText) : null);
              break;
            case "Renewal":
              this.Text = policyLearnedField.Renewal;
              break;
            case "Carrier":
              this.Text = policyLearnedField.CarrierNickName;
              break;
            case "Product":
              this.Text = policyLearnedField.CoverageNickName;
              break;
            case "ModelAvgPremium":
              this.Text = policyLearnedField.ModalAvgPremium.ToString();
              break;
            case "PolicyMode":
              if (policyLearnedField.PolicyModeId != null)
                this.Text = VMHelper.getPolicyMode(policyLearnedField.PolicyModeId.Value);
              else
                this.Text = string.Empty;
              break;
            case "Enrolled":
              this.Text = policyLearnedField.Enrolled;
              break;
            case "Eligible":
              this.Text = policyLearnedField.Eligible;
              break;
            case "Link1":
              this.Text = policyLearnedField.Link1;
              break;
            case "SplitPer":
              this.Text = policyLearnedField.Link2.ToString();
              break;
            case "Client":
              this.Text = policyLearnedField.ClientName;
              break;
            case "CompType":
              if (policyLearnedField.CompTypeId != null)
                this.Text = VMHelper.getCompType(policyLearnedField.CompTypeId.Value);
              else
                this.Text = string.Empty;
              break;
          }
        }
      }
      catch (Exception)
      {
      }
    }


    private void SetTextBoxPropertyFromToolField()
    {
      //this.TabIndex = PayorToolField.FieldOrder;

      try
      {
        this.Name = "TextBox_" + PayorToolField.FieldOrder.ToString();

        if (PayorToolField.FieldOrder == 0)
        {
          Binding binding = new Binding();
          binding.Source = VMInstances.DeuVM;
          binding.Path = new PropertyPath("IsFocusToFirstField");
          this.SetBinding(FocusExtension.IsFocusedProperty, binding);
        }

        if (!string.IsNullOrEmpty(PayorToolField.HelpText.Trim()))
          this.ToolTip = PayorToolField.HelpText;
        else
          this.ToolTip = null;

        this.InputMask = getMappedMaskText(PayorToolField.MaskFieldType, PayorToolField.MaskText);
        this.Text = PayorToolField.DefaultValue;

        if (PayorToolField.AllignedDirection == "Right")
          this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
        else
          this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;

        Popup popup = new Popup();
        popup.Name = "Popup";
        popup.IsOpen = false;
        popup.Placement = PlacementMode.Bottom;
        popup.PlacementTarget = this;

        ListBox listbox = new ListBox();
        listbox.Name = "PART_WordsHost";
        popup.Child = listbox;

        TextBoxAutoComplete.m_Selector = listbox;
        TextBoxAutoComplete.SetWordAutoCompletePopup(this, popup);

        if (PayorToolField.IsCalculatedField)
        {
          if (!PayorToolField.IsOverrideOfCalcAllowed)
            this.IsReadOnly = true;
        }

        if (PayorToolField.FieldStatusValue == "Invisible")
        {
          this.Visibility = System.Windows.Visibility.Hidden;
        }
      }
      catch (Exception)
      {
      }
    }

    private string getMappedMaskText(byte maskFieldType, string maskFieldText)
    {
      try
      {
        if (maskFieldType == 1)
        {
          if (maskFieldText.Contains("MMM"))
            maskFieldText = maskFieldText.Replace("MMM", "AAA");

          maskFieldText = maskFieldText.Replace("d", "i");
          maskFieldText = maskFieldText.Replace("M", "i");
          maskFieldText = maskFieldText.Replace("y", "i");
          //Replace "*" by blank
          maskFieldText = maskFieldText.Replace("*", "");

        }
        //else if (maskFieldType == 2)
        //{
        //    maskFieldText = maskFieldText.Replace("#", "i");
        //}
        else
          maskFieldText = string.Empty;

        return maskFieldText;
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }
  }
}
