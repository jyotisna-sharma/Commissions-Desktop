using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using MyAgencyVault.ViewModel.CommonItems;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigSharedDataVM : BaseViewModel
    {
        private VMConfigrationManager ParentVM;
        public ConfigSharedDataVM(VMConfigrationManager configManager,ConfigPayorCarriersVM carrierVM, ConfigCarrierCoveragesVM coverageVM)
        {
            ParentVM = configManager;
            carrierVM.CarriersChanged += new ConfigPayorCarriersVM.CarriersChangedEventHandler(carrierVM_CarriersChanged);
        }


        private void carrierVM_CarriersChanged(ObservableCollection<Carrier> Carriers)
        {
            //Add by vinod
            //Carriers = new ObservableCollection<Carrier>(Carriers.OrderBy(s => s.CarrierName).ToList());

            this.Carriers = Carriers;

        }

        private ObservableCollection<Carrier> _Carriers;
        public ObservableCollection<Carrier> Carriers
        {
            get 
            {
                return _Carriers;
            }
            set 
            {
                _Carriers = value;
                OnPropertyChanged("Carriers");
            }
        }
    }
}
