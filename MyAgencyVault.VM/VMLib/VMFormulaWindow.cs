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
using MyAgencyVault.VM.VMLib;
using System.Data;
using System.Windows.Controls;

namespace MyAgencyVault.ViewModel.VMLib
{
  public class FormulaWindowVM : BaseViewModel
  {

    // PayorToolAvailablelFieldType _CalculateField=null;
    ObservableCollection<PayorToolAvailablelFieldType> _PayorToolAvailableFields = null;
    ObservableCollection<string> _NewSelecetedItem = null;
    PayorToolAvailablelFieldType _CurrAvailableFields = null;
    string _FieldForTestFormula;
    bool _IsItemChecked;
    Formula _formulafields = null;

    public delegate void OnCloseWindow();
    public event OnCloseWindow CloseWindow;

    public delegate void OnTestFormula();
    public event OnTestFormula TestFormula;

    public delegate void OnCheckItem();
    public event OnCheckItem SCheckItem;

    public delegate void OnClickOk();
    public event OnClickOk SClickOk;

    public delegate void OnClickCancel();
    public event OnClickCancel SClickCancel;


    int _totalAmt;
    int _enterValue;
    public FormulaWindowVM(PayorToolVM objPayor)
    {
      using (ServiceClients serviceClients = new ServiceClients())
      {
        PayorToolAvailableField = new ObservableCollection<PayorToolAvailablelFieldType>();
        NewSelecetedItem = new ObservableCollection<string>();
        PayorToolAvailableField = serviceClients.PayorToolAvailablelFieldTypeClient.GetFieldList();


        selectedAvailableFields = PayorToolAvailableField.FirstOrDefault();
        FieldForTestFormula = NewSelecetedItem.FirstOrDefault();
      }
    }

    public bool IsItemChecked
    {
      get
      {
        return _IsItemChecked;
      }

      set
      {
        _IsItemChecked = value;
        OnPropertyChanged("IsItemChecked");
      }
    }

    private ObservableCollection<string> _formulaCollection;
    public ObservableCollection<string> FormulaCollection
    {
      get
      {
        return _formulaCollection;
      }

      set
      {
        _formulaCollection = value;
        OnPropertyChanged("FormulaCollection");
      }
    }

    private string _FormulaChar;
    public string FormulaChar
    {
      get
      {
        return _FormulaChar;
      }

      set
      {
        _FormulaChar = value;
        OnPropertyChanged("FormulaChar");
      }

    }



    public int EnterValue
    {
      get
      {
        return _enterValue;
      }
      set
      {
        _enterValue = value;
        OnPropertyChanged("EnterValue");
      }
    }

    public int totalValue
    {
      get
      {
        return _totalAmt;
      }
      set
      {
        _totalAmt = value;
        OnPropertyChanged("totalValue");
      }
    }

    public Formula formulaFields
    {
      get
      {
        return _formulafields;
      }

      set
      {
        _formulafields = value;
        OnPropertyChanged("formulaFields");
      }

    }

    public PayorToolAvailablelFieldType selectedAvailableFields
    {
      get
      {
        return _CurrAvailableFields;
      }
      set
      {
        _CurrAvailableFields = value;
        OnPropertyChanged("selectedAvailableFields");
      }
    }

    public string FieldForTestFormula
    {
      get
      {


        return _FieldForTestFormula;

      }

      set
      {
        _FieldForTestFormula = value;

        OnPropertyChanged("FieldForTestFormula");
      }
    }

    public ObservableCollection<string> NewSelecetedItem
    {
      get
      {
        return _NewSelecetedItem;
      }

      set
      {
        _NewSelecetedItem = value;
        OnPropertyChanged("NewSelecetedItem");

      }

    }

    public ObservableCollection<PayorToolAvailablelFieldType> PayorToolAvailableField
    {
      get
      {
        return _PayorToolAvailableFields;
      }

      set
      {
        _PayorToolAvailableFields = value;
        OnPropertyChanged("PayorToolAvailableFields");

      }

    }





    private ICommand _CheckItem;
    public ICommand CheckItem
    {
      get
      {
        if (_CheckItem == null)
        {
          _CheckItem = new BaseCommand(param => CheckedSelItem());
        }
        return _CheckItem;
      }

    }



    private ICommand _TestFormula;
    public ICommand TestFormulaWindow
    {
      get
      {
        if (_TestFormula == null)
        {
          _TestFormula = new BaseCommand(param => TestFWindow());
        }
        return _TestFormula;
      }

    }

    private ICommand _saveFormula;
    public ICommand SaveFormulaWindow
    {
      get
      {
        if (_saveFormula == null)
        {
          _saveFormula = new BaseCommand(param => SaveFormulaWindowDetail());
        }
        return _saveFormula;
      }

    }


    //private ICommand _CancelWindow;
    //public ICommand CloseFormulaWindow
    //{
    //    get
    //    {
    //        if (_CancelWindow == null)
    //        {
    //            _CancelWindow = new BaseCommand(param => CloseFWindow());
    //        }
    //        return _CancelWindow;
    //    }

    //}

    private ICommand _OnWindowOk;
    public ICommand OkTestWindow
    {
      get
      {
        if (_OnWindowOk == null)
        {
          _OnWindowOk = new BaseCommand(param => OnWindowOk());
        }
        return _OnWindowOk;
      }

    }

    private ICommand _OnWindowCancel;
    public ICommand CloseTestWindow
    {
      get
      {
        if (_OnWindowCancel == null)
        {
          _OnWindowCancel = new BaseCommand(param => OnWindowCancel());
        }
        return _OnWindowCancel;
      }

    }

    //private void CloseFWindow()
    //{
    //    if (CloseWindow != null)
    //    {
    //        CloseWindow();
    //    }

    //}


    private void CheckedSelItem()
    {
      if (SCheckItem != null)
      {
        SCheckItem();

      }

    }

    private void OnWindowOk()
    {
      if (SClickOk != null)
      {
        SClickOk();

      }

    }

    private void OnWindowCancel()
    {
      if (SClickCancel != null)
      {
        SClickCancel();

      }

    }

    private void SaveFormulaWindowDetail()
    {
      try
      {


      }
      catch
      {

      }

    }


    private void TestFWindow()
    {
      try
      {
        if (TestFormula != null)
        {
          TestFormula();
        }
      }
      catch
      {

      }

    }



    private void RemoveItem(string selectedItem)
    {
      try
      {
        foreach (var item in NewSelecetedItem.ToList())
        {
          if (item.Contains("-") == true)
          {
            if (item == string.Concat("- ", selectedItem))
            {
              NewSelecetedItem.Remove("-" + " " + selectedItem);
              break;
            }

          }
          else if (item.Contains("+") == true)
          {
            if (item == string.Concat("+ ", selectedItem))
            {
              NewSelecetedItem.Remove("+" + " " + selectedItem);
              break;
            }

          }
          else if (item.Contains("*") == true)
          {
            if (item == string.Concat("* ", selectedItem))
            {
              NewSelecetedItem.Remove("*" + " " + selectedItem);
              break;
            }

          }
          else if (item.Contains("/") == true)
          {
            if (item == string.Concat("/ ", selectedItem))
            {
              NewSelecetedItem.Remove("/" + " " + selectedItem);
              break;
            }

          }
          else
          {
            NewSelecetedItem.Remove(selectedItem);
          }

        }
      }
      catch (Exception)
      {
      }
    }

    public void AddExpression(object sender, string FormulaChar)
    {
      CheckBox obj = new CheckBox();

      try
      {
        if (sender.GetType() == typeof(CheckBox))
        {
          obj = (CheckBox)sender;
          if (obj.IsChecked == true)
          {
            if (NewSelecetedItem.Count == 0)
            {
              NewSelecetedItem.Add(obj.Content.ToString());
            }

            else if (FormulaChar != string.Empty)
            {
              NewSelecetedItem.Add(FormulaChar + " " + obj.Content.ToString());
            }

          }
          else
          {

            RemoveItem(obj.Content.ToString());


          }
        }
      }
      catch (Exception)
      {
      }

    }


  }
}
