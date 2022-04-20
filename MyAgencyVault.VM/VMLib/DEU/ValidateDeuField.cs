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
  public class ValidateDeuField
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
    public bool ValidateDate(string date, string format)
    {
      //Replace "*" When mask type "mm-ddd-yyyy*"
      format = format.Replace("*", "");

      var deuFormBuilder = DeuFormBuilder.getDeuFormBuilder();
      try
      {
        DateTime temptime;

        DateTime date1 = DateTime.ParseExact(date, format, DateTimeFormatInfo.InvariantInfo);
        DateTime.TryParse(date,out temptime);
        deuFormBuilder.VmDeu.ToolTipError = null;
      }
      catch
      {

        deuFormBuilder.VmDeu.ToolTipError = "Please enter valid date.";
        return false;
      }
      return true;
    }

    public double GetNumberFromFormatString(string number, string maskText,double originalValue)
    {
      double Value = 0;
      try
      {
        if (!string.IsNullOrEmpty(number))
        {
          if (maskText.Contains("$"))
          {
            Value = double.Parse(number.Replace("$", ""));
          }
          else if (maskText.Contains("%"))
          {
            Value = double.Parse(number.Replace("%", ""));
          }
          else
          {
            Value = double.Parse(number.Replace("%", ""));
          }
        }
      }
      catch
      {
        Value = originalValue;
      }

      return Value;
    }

    public bool ValidateNumber(string number, string maskText, bool IsZeroorBlankAllowed)
    {
        //var deuFormBuilder = DeuFormBuilder.getDeuFormBuilder();
        //deuFormBuilder.VmDeu.ToolTipError = null;
        //carsh occurs
        if (string.IsNullOrEmpty(number))
        {
            return  true;
        }

        number = number.Replace("(", "");
        number = number.Replace(")", "");

        decimal Value = 0;
        bool ValidNumber = false;
        try
        {
            if (maskText.Contains("$"))
            {
                Value = decimal.Parse(number.Replace("$", ""));
            }
            else if (maskText.Contains("%"))
            {
                Value = decimal.Parse(number.Replace("%", ""));
            }
            else
            {
                Value = decimal.Parse(number.Replace("%", ""));
            }

            if (Value == 0 && !IsZeroorBlankAllowed)
            {
                ValidNumber = false;
                //deuFormBuilder.VmDeu.ToolTipError = "Please enter valid number in numeric fields.";
            }
            else
                ValidNumber = true;
        }
        catch(Exception ex)
        {

        }

        return ValidNumber;
    }

    public string getNumberFormat(string maskText)
    {
      string[] numbers = maskText.Split('.');

      if (numbers.Length == 1)
      {
        if (maskText.Contains("$"))
        {
          return "{0:c0}";
        }
        else if (maskText.Contains("%"))
        {
          return "{0:p0}";
        }
        else
        {
          return "{0:n0}";
        }
      }
      else
      {
        int hashCount = numbers[1].Count(s => s == '#');
        if (maskText.Contains("$"))
        {
          return "{0:c" + hashCount.ToString() + "}";
        }
        else if (maskText.Contains("%"))
        {
          return "{0:p" + hashCount.ToString() + "}";
        }
        else
        {
          return "{0:n" + hashCount.ToString() + "}";
        }
      }
    }

    public bool ValidateCarrier(string carrierNickName, Guid payorId)
    {
      return serviceClients.CarrierClient.IsValidCarrier(carrierNickName, payorId);
    }

    public bool ValidateCovergae(string carrierNickName, string coverageNickName, Guid payorId)
    {
      return serviceClients.CoverageClient.IsValidCoverage(carrierNickName, coverageNickName, payorId);
    }
  }

}
