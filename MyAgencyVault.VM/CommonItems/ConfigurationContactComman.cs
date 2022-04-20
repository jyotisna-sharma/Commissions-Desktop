using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.ViewModel.CommonItems
{
    public static class ConfigurationContactComman
    { 
        public static List<Priority>  PrirtyLst { get; set; }
        public static List<ContactPref> PrirtyContectLst { get; set; }
        
        static ConfigurationContactComman()
        {
            GetPriority();
            PrirtyContectLst = new List<ContactPref>(); 
            PrirtyContectLst.Add(new ContactPref { ContPrefId = 0, ContPref = "Phone" });
            PrirtyContectLst.Add(new ContactPref { ContPrefId = 1, ContPref = "Fax" });
            PrirtyContectLst.Add(new ContactPref { ContPrefId = 2, ContPref = "Email" }); 
        }
       

        private static void  GetPriority()
        {
            PrirtyLst = new List<Priority>(); 
            List<Priority> _priority=new List<Priority>() ;
            for (int i = 0; i < 10; i++)
            {
                Priority _pri=new Priority();
                
                _pri.PId = i;
                _pri.PriorityName = i + 1;
                PrirtyLst.Add(_pri);  
            }
            
        }
    }
    public class Priority
    {
        public int PId { get; set; }
        public int PriorityName { get; set; }
    }
    public class ContactPref
    {
        public int ContPrefId { get; set; }
        public string ContPref { get; set; }
    }
    public enum PolicyType
    {
        New,
        Replace,
    }
   

}
