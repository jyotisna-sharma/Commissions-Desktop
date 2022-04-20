using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.VM.ClientProxy
{
    public class SettingServiceClient
    {
        private SettingsManagerVM SettingManager;

        public SettingServiceClient(SettingsManagerVM settingManager)
        {
            this.SettingManager = settingManager;
        }

        private CarrierClient _carrierClient;
        public CarrierClient CarrierClient
        {
            get
            {
                if (_carrierClient == null || _carrierClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    _carrierClient = new CarrierClient();                    
                }
                return _carrierClient;
            }
        }
    }
}
