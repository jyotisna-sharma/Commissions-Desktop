using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class AvailableMaskFieldType
    {
        public int GetAvailbleFieldType(string strSeletedFiels)
        {
            int intType = 0;
            switch (strSeletedFiels)
            {
                case "PolicyNumber":
                    intType = 3;
                    break;
                case "Insured":
                    intType = 3;
                    break;
                case "Carrier":
                    intType = 3;
                    break;
                case "Product":
                    intType = 3;
                    break;
                case "ModelAvgPremium":
                    intType = 2;
                    break;
                case "PolicyMode":
                    intType = 3;
                    break;
                case "Enrolled":
                    intType = 2;
                    break;
                case "SplitPercentage":
                    intType = 2;
                    break;
                case "Client":
                    intType = 3;
                    break;
                case "CompType":
                    intType = 3;
                    break;
                case "PayorSysId":
                    intType = 2;
                    break;
                case "Renewal":
                    intType = 2;
                    break;
                case "CompScheduleType":
                    intType = 3;
                    break;
                case "InvoiceDate":
                    intType = 1;
                    break;
                case "PaymentReceived":
                    intType = 2;
                    break;
                case "CommissionPercentage":
                    intType = 2;
                    break;
                case "NumberOfUnits":
                    intType = 2;
                    break;
                case "DollerPerUnit":
                    intType = 2;
                    break;
                case "Fee":
                    intType = 2;
                    break;
                case "Bonus":
                    intType = 2;
                    break;
                case "CommissionTotal":
                    intType = 2;
                    break;

                case "Effective":
                case "OriginalEffectiveDate":
                    intType = 1;
                    break;

            }

            return intType;
        }
    }
}
