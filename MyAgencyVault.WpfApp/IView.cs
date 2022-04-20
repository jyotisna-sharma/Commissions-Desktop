using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.WinApp.UserControls;
using MyAgencyVault.WinApp.Validation;
using System.Windows;

namespace MyAgencyVault.WinApp
{
    public class ViewValidation : IView
    {
        public bool Validate(string ScreenName)
        {
            bool IsValidated = false;
            switch (ScreenName)
            {
                case "People Manager":
                    PeopleManager peopleManager = VMUserControl.getPeopleManagerUserControl();
                    IsValidated = ControlValidation.ForceValidation(peopleManager);
                    break;
                case "Client":
                    CreateClient client = VMUserControl.CreateClient;
                    IsValidated = ControlValidation.ForceValidation(client);
                    break;
                case "Billing Manager":
                    BillingManager billingManager = VMUserControl.getBillingManagerUserControl();
                    IsValidated = ControlValidation.ForceValidation(billingManager);
                    break;
            }

            return IsValidated;
        }
    }
}
