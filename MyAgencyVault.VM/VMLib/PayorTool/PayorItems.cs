using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MyAgencyVault.ViewModel.PayorToolLib
{
  public  class PayorItems
    {

      public  List<PayorData> _item = new List<PayorData>();
      public  PayorItems()
        {
           _item.Add(new PayorData {Name="sunil", PayorID="1234"});
            _item.Add(new PayorData {Name="amit", PayorID="1234"});
            _item.Add(new PayorData {Name="sumit", PayorID="1234"});
        }     
        public List<PayorData> Returndata
        {
            get
            {
                return _item;
            }
                    
        }
        public string SelectedName { get; set; }        

    }
   public class PayorData
    {
        public string Name{get;set;}
        public string PayorID{get;set;}
    }
}
