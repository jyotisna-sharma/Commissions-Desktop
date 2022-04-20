using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VMLib;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.VM.ClientProxy
{
    public class ConfigServiceClient
    {
        private VMConfigrationManager ConfigManagerVM;

        public ConfigServiceClient(VMConfigrationManager ConfigManagerVM)
        {
            this.ConfigManagerVM = ConfigManagerVM;
        }

        private PayorClient _payorClient;
        public PayorClient PayorClient
        {
            get
            {
                if (_payorClient == null || _payorClient.State == System.ServiceModel.CommunicationState.Closed || _payorClient.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    _payorClient = new PayorClient();
                }
                return _payorClient;
            }
        }

        private ConfigDisplayPayorClient _configDisplayPayorClient;
        public ConfigDisplayPayorClient ConfigDisplayPayorClient
        {
            get
            {
                if (_configDisplayPayorClient == null || _configDisplayPayorClient.State == System.ServiceModel.CommunicationState.Closed || _configDisplayPayorClient.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    _configDisplayPayorClient = new ConfigDisplayPayorClient();
                }
                return _configDisplayPayorClient;
            }
        }

        private CarrierClient _carrierClient;
        public CarrierClient CarrierClient
        {
            get
            {
                if (_carrierClient == null || _carrierClient.State == System.ServiceModel.CommunicationState.Closed || _carrierClient.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    _carrierClient = new CarrierClient();
                }
                return _carrierClient;
            }
        }
    }
}
