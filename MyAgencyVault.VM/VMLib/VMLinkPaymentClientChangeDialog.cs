using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VMLib
{
    public class DialogResultCustom
    {
        public Guid ClientId { get; set; }
    }
    public class VMLinkPaymentClientChangeDialog : BaseViewModel
    {
        public delegate void PopUpWindosForLinkedPolicyClient();
        public event PopUpWindosForLinkedPolicyClient PopupLinkedClientOpen;

        public delegate void PopUpWindosForLinkedPolicyClientClose();
        public event PopUpWindosForLinkedPolicyClient PopupLinkedClientClose;

        public VMCompManager _VMCompManager = null;
        public VMLinkPaymentClientChangeDialog()
        {

            PropertyChanged += new PropertyChangedEventHandler(VMLinkPaymentClientChangeDialog_PropertyChanged);

        }

        void VMLinkPaymentClientChangeDialog_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private ICommand _OpenDialog;

        public ICommand OpenDialog
        {
            get
            {
                if (_OpenDialog != null)
                {
                    _OpenDialog = new BaseCommand(Param => Show());
                }
                return _OpenDialog;
            }
            set
            {
                _OpenDialog = value;
                OnPropertyChanged("OpenDialog");
            }
        }

        public ICommand _selectCurrentClient;
        public ICommand SelectCurrentClient
        {
            get
            {
                // if (_selectCurrentClient != null)
                // {
                _selectCurrentClient = new BaseCommand(Param => DoGetCurrentClient());
                // }
                return _selectCurrentClient;
            }
            set
            {
                _selectCurrentClient = value;
                OnPropertyChanged("SelectCurrentClient");
            }
        }
        public ICommand _ChooseClient;
        public ICommand ChooseClient
        {
            get
            {
                //if (_ChooseClient != null)
                //{
                _ChooseClient = new BaseCommand(Param => DoChooseClient());
                // }
                return _ChooseClient;
            }
            set
            {
                _ChooseClient = value;
                OnPropertyChanged("ChooseClient");
            }
        }
        private Client _currentClient;
        public Client CurrentClient
        {
            get
            {

                return _currentClient == null ? new Client() : _currentClient;
            }
            set
            {
                _currentClient = value;
                OnPropertyChanged("CurrentClient");
            }
        }
        private Client _selectedClient;
        public Client SelectedClient
        {
            get
            {
                return _selectedClient == null ? new Client() : _selectedClient;
            }
            set
            {
                _selectedClient = value;
                OnPropertyChanged("SelectedClient");
            }
        }
        private ObservableCollection<Client> _clientlst;
        public ObservableCollection<Client> ClientLst
        {
            get
            {
                return _clientlst == null ? new ObservableCollection<Client>() : _clientlst;
            }
            set
            {
                _clientlst = value;
                OnPropertyChanged("ClientLst");
            }
        }
        private Client _resultClient;
        public Client ResultClient
        {
            get
            {
                return _resultClient == null ? new Client() : _resultClient;
            }
            set
            {
                _resultClient = value;
                OnPropertyChanged("ResultClient");
            }
        }
        private void DoChooseClient()
        {
            ResultClient = SelectedClient;
            System.Windows.Forms.MessageBox.Show("Policy is now Active.  System has defaulted 100% of the outgoing payments to the house account.  You may adjust the payments in the Commission Dashboard tab of the Policy Manager.");
            if (PopupLinkedClientClose != null)
            {
                PopupLinkedClientClose();
            }
        }

        private void DoGetCurrentClient()
        {
            if (CurrentClient.Name != null)
            {
                CurrentClient.Name = CurrentClient.Name.Trim();
            }
            if (String.IsNullOrEmpty(CurrentClient.Name))
            {
                System.Windows.MessageBox.Show("No Client Selecetd", "Information", System.Windows.MessageBoxButton.OK);
                return;
            }

            try
            {
              using (ServiceClients serviceClients = new ServiceClients())
              {
                ObservableCollection<Client> _ClientLst = serviceClients.ClientClient.GetClientList(_VMCompManager.SharedVMData.SelectedLicensee.LicenseeId);
                Client cl1 = _ClientLst.Where(p => p.Name == CurrentClient.Name).FirstOrDefault();

                if (cl1 != null && cl1.ClientId != Guid.Empty)
                {
                  ResultClient = cl1;
                  SharedVMData.NewClientid = cl1.ClientId;
                }
                else
                {
                  Client newClient = new Client();
                  newClient.Name = CurrentClient.Name;
                  newClient.ClientId = Guid.NewGuid();
                  newClient.LicenseeId = CurrentClient.LicenseeId;
                  serviceClients.ClientClient.AddUpdateClient(newClient);
                  ResultClient = newClient;
                  SharedVMData.NewClientid = newClient.ClientId;
                }
                System.Windows.Forms.MessageBox.Show("Policy is now Active.  System has defaulted 100% of the outgoing payments to the house account.  You may adjust the payments in the Commission Dashboard tab of the Policy Manager.");

                if (PopupLinkedClientClose != null)
                {
                  PopupLinkedClientClose();
                }
              }
            }
            catch
            {
            }
        }
        private void FillClientLst(Guid? licenseeId)
        {
          try
          {
            using (ServiceClients serviceClients = new ServiceClients())
            {
              ClientLst = serviceClients.ClientClient.GetClientList(licenseeId);

              foreach (Client ci in ClientLst)
              {
                if (ci.ClientId == recieveClient)
                {
                  CurrentClient = ci;
                  ClientLst.Remove(CurrentClient);
                  break;
                }
              }
              //SelectedClient = ClientLst[0];
              SelectedClient = ClientLst.FirstOrDefault();
            }
          }
          catch (Exception)
          {
          }
        }

        public VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
            get
            {
                if (_SharedVMData == null)
                    _SharedVMData = VMSharedData.getInstance();

                return _SharedVMData;
            }
        }

        public Guid LicenseeId
        {
            get;
            set;
        }
        public Guid recieveClient
        {
            get;
            set;
        }
        public Client Show()
        {
            // _DialogResultCustom = new DialogResultCustom();
            try
            {
                if (PopupLinkedClientOpen != null)
                {
                    LicenseeId = _VMCompManager.SharedVMData.SelectedLicensee.LicenseeId;
                    recieveClient = _VMCompManager.LinkPaymentSelectedPendingPolicies.ClientId;//.LinkPaymentSelectedActivePolicies.ClientId;
                    FillClientLst(LicenseeId);

                    PopupLinkedClientOpen();

                }
            }
            catch (Exception)
            {
            }
            return ResultClient;
        }
    }
}
