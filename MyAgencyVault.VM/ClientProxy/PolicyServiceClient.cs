using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VMLib;

namespace MyAgencyVault.VM.ClientProxy
{
    public class PolicyServiceClient
    {
        private VMOptimizePolicyManager PolicyManagerVM;

        public PolicyServiceClient(VMOptimizePolicyManager policyManager)
        {
            PolicyManagerVM = policyManager;
        }

        private PolicyClient _policyClient;
        public PolicyClient PolicyClient
        {
            get
            {
                if (_policyClient == null || _policyClient.State == System.ServiceModel.CommunicationState.Closed || _policyClient.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    _policyClient = new PolicyClient();
                    
                }
                return _policyClient;
            }
        }

        private DisplayPayorClient _DisplayedPayorClient;
        public DisplayPayorClient DisplayedPayorClient
        {
            get
            {
                if (_DisplayedPayorClient == null || _DisplayedPayorClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    _DisplayedPayorClient = new DisplayPayorClient();
                    _DisplayedPayorClient.GetDisplayPayorsInChunkCompleted += new EventHandler<GetDisplayPayorsInChunkCompletedEventArgs>(PolicyManagerVM.PayorClient_GetPayorsCompleted);
                }
                return _DisplayedPayorClient;
            }
        }

        private CarrierClient _carrierClient;
        public CarrierClient CarrierClient
        {
            get
            {
                if (_carrierClient == null || _carrierClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    _carrierClient = new CarrierClient();
                    _carrierClient.GetPayorCarriersOnlyCompleted += new EventHandler<GetPayorCarriersOnlyCompletedEventArgs>(PolicyManagerVM.CarrierClient_GetPayorCarriersOnlyCompleted);
                }
                return _carrierClient;
            }
        }

        private CoverageClient _coverageClient;
        public CoverageClient CoverageClient
        {
            get
            {
                if (_coverageClient == null || _coverageClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    _coverageClient = new CoverageClient();
                    //_coverageClient.GetPayorCarrierCoveragesCompleted += new EventHandler<GetPayorCarrierCoveragesCompletedEventArgs>(PolicyManagerVM.CoverageClient_GetPayorCarrierCoveragesCompleted);
                }
                return _coverageClient;
            }
        }
    }
}
