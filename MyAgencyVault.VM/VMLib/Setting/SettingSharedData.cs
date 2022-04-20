using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Forms;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows;

namespace MyAgencyVault.VM.VMLib.Setting
{
    public class SettingSharedDataVM : BaseViewModel
    {
        private SettingsManagerVM ParentVM;
        public SettingSharedDataVM(SettingsManagerVM settingManager, SettingPayorCarriersVM carrierVM)
        {
            ParentVM = settingManager;
            carrierVM.CarriersChanged += new SettingPayorCarriersVM.CarriersChangedEventHandler(carrierVM_CarriersChanged);
        }

        void carrierVM_CarriersChanged(ObservableCollection<Carrier> Carriers)
        {
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
