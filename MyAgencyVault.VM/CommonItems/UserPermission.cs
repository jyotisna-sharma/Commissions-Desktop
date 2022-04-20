using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.ViewModel.CommonItems
{
    
    public class MessageConst
    {
       
        public const string LockErrorMessage = "Policy is in use";
    }
    public class LinkedMessges
    {
        public const string msg1 = @"Payment has been linked. "+
            "However, this payment was already marked as paid."+
             "   If you would like to redistribute the payment, "+
              "      you will need to reverse the payment from "+
               "         the House account and pay "+
            "the policy’s payees.  Would you like the system to"+
             "   reverse and redistribute for you?";
        public const string msg2 = "Are you sure you want to continue to link? ";
        public const string msg3 = @"The outgoing payment schedule does not equal the " +
                                    "incoming payment and was already marked as paid.  " +
                                    "If you would like to redistribute the payment, " +
                                    "you will need to adjust the outgoing schedule, " +
                                    "reverse the payment from the House account and pay the policy’s payees. " +
                                    "It is recommended you make the necessary changes prior and link after.  " +
                                    "Are you sure you want to continue to link?";
       public const string msg4=@"Payment has been linked properly and distributed to payees.";
       public const string meg5 = @"The outgoing payment schedule does not equal the incoming payment.  " +
                                 "If you would like to redistribute the payment, you will need to adjust " +
                                 "the outgoing schedule  and pay the policy’s payees. It is recommended " +
                                 "you make the necessary changes prior and link after.  Are you sure you want to continue to link?";
        public const string msg6=@"Payment has been linked properly and distributed to the House. " ;


    }
    public enum MasterPolicyMode
    {
        Monthly,
        Quarterly,
        Annually,
        OneTime,
        Random,
    }
        
    public enum ControlLevelAccess
    {
        View = 1,
        Edit = 2,
        NoAccess = 3,
    }

    public enum RoleLeveDefaultPos
    {
        //Agent = 3,
        //DEP = 4,
        //HO = 3,//
        //Super = 8,
        //Admin = 9,
        
        Agent = 2,
        DEP = 3,
        HO = 2,//
        Super = 8,
        Admin = 1,
    }
}
