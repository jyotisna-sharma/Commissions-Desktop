using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.ViewModel.CommonItems
{
   public class AddPayee
    {
       public Guid UserCiD { get; set; }
       public bool IsSelect { get; set; }
       public string LastName { get; set; }
       public string FirstName { get; set; }
       public string NickName { get; set; }
       public string Company { get; set; }
       public bool IsHouse { get; set; }
    }    
}
