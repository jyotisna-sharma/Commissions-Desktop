using System;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VMLib;
using System.Collections.Generic;
using System.Linq;
using MyAgencyVault.ViewModel.VMLib;
using System.Windows;

namespace MyAgencyVault.VM.VMLib
{
    public class PolicyClientVm : BaseViewModel
    {
        public Client PreviousClientDataBeforeEdit { get; set; }

        public delegate void OpenClient();
        public event OpenClient OpenClientEvent;
        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
          get
          {
            if (_SharedVMData == null)
            {
              _SharedVMData = VMSharedData.getInstance();
            }

            return _SharedVMData;
          }
        }
        public delegate void CloseClient();
        public event CloseClient CloseEvent;
        private IView _ViewValidation;
        private ServiceClients _ServiceClients;
        private ServiceClients serviceClients
        {
          get
          {
            if (_ServiceClients == null)
            {
              _ServiceClients = new ServiceClients();
            }
            return _ServiceClients;
          }
        }
        public PolicyClientVm(IView viewValidation)
        {
            _ViewValidation = viewValidation;
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

        private ObservableCollection<Client> _clientLst;
        public ObservableCollection<Client> ClientLst
        {
            get
            {
                if (_clientLst == null)
                {
                  _clientLst = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);
                }
                return _clientLst;
            }
            set { _clientLst = value; }
        }

        private ICommand _InsertClient;
        public ICommand InsertClient
        {
            get
            {
                if (_InsertClient == null)
                {
                    _InsertClient = new BaseCommand(x => InserClientData());
                }
                return _InsertClient;
            }

        }

        private ICommand _ZipLostFocus;
        public ICommand ZipLostFocus
        {
            get
            {
                if (_ZipLostFocus == null)
                    _ZipLostFocus = new BaseCommand(param => OnZipLostFocus());
                return _ZipLostFocus;
            }
        }

        private void OnZipLostFocus()
        {
            try
            {

                if (SelectedClient.Zip != null && SelectedClient.Zip.Length == 5)
                {
                    Zip zipData = serviceClients.MasterClient.GetZip(SelectedClient.Zip);
                    if (zipData != null)
                    {
                        SelectedClient.City = zipData.City;
                        SelectedClient.State = zipData.State;
                    }
                }
                else
                {
                    SelectedClient.City = null;
                    SelectedClient.State = null;
                }
            }
            catch
            {
            }
        }

        private void InserClientData()
        {
            if (!_ViewValidation.Validate("Client"))
            {
                MessageBox.Show("Validation failed.");

                //Fill the previous data into the field when validation failed
                CancelValues();

                return;
            }

            try
            {
                if (SharedVMData.UpdateMode == UpdateMode.Add)
                {
                    ClientLst = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);
                    if (SelectedClient.ClientId == Guid.Empty)
                        SelectedClient.ClientId = Guid.NewGuid();

                    SelectedClient.Name = SelectedClient.Name.Trim();

                    int _PolicyCount = (from s in ClientLst where s.Name.ToLower().Trim() == SelectedClient.Name.ToLower().Trim() select s).ToList().Count;
                    if (_PolicyCount > 0 || string.IsNullOrEmpty(SelectedClient.Name))
                    {
                        if(_PolicyCount > 0)
                            System.Windows.MessageBox.Show("Client is already into system", "Information", System.Windows.MessageBoxButton.OK);
                        else
                            System.Windows.MessageBox.Show("Client Name can not be blank.", "Information", System.Windows.MessageBoxButton.OK);

                        SelectedClient.ClientId = Guid.Empty;
                        return;
                    }
                    Client _client = GetClientData(SelectedClient);
                    _client.ClientId = SelectedClient.ClientId;

                    serviceClients.ClientClient.AddUpdateClient(_client);
                    SelectedClient = _client;
                    ClientLst = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);
                    if (CloseEvent != null)
                        CloseEvent();
                }
                else
                {
                    Client _client = GetClientData(SelectedClient);
                    SelectedClient.Name = SelectedClient.Name.Trim();

                    int _PolicyCount = (from s in ClientLst where (s.Name.ToLower().Trim() == SelectedClient.Name.ToLower().Trim()) && (s.ClientId != _client.ClientId) select s).ToList().Count;
                    if (_PolicyCount > 0 || string.IsNullOrEmpty(SelectedClient.Name))
                    {
                        if(_PolicyCount > 0)
                            System.Windows.MessageBox.Show("Client is already into system", "Information", System.Windows.MessageBoxButton.OK);
                        else
                            System.Windows.MessageBox.Show("Client name can not be blank", "Information", System.Windows.MessageBoxButton.OK);
                        
                        return;
                    }

                    serviceClients.ClientClient.AddUpdateClient(_client);
                    SelectedClient = _client;
                  
                    ClientLst = serviceClients.ClientClient.GetClientList(SharedVMData.SelectedLicensee.LicenseeId);

                    if (CloseEvent != null)
                        CloseEvent();
                }
                //Sorting client name with ASC               
                ClientLst = new ObservableCollection<Client>(ClientLst.OrderBy(c => c.Name).ToList());


            }
            catch(Exception)
            {
            }

        }

        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new BaseCommand(x => CancelValues());
                }
                return _cancelCommand;
            }

        }

        private void CancelValues()
        {
           // SelectedClient.Name = "sdfdfd";
            try
            {
                if (PreviousClientDataBeforeEdit != null)
                {
                    SelectedClient.ClientId = PreviousClientDataBeforeEdit.ClientId;
                    SelectedClient.LicenseeId = PreviousClientDataBeforeEdit.LicenseeId;
                    SelectedClient.Name = PreviousClientDataBeforeEdit.Name;
                    SelectedClient.InsuredName = PreviousClientDataBeforeEdit.InsuredName;
                    SelectedClient.Address = PreviousClientDataBeforeEdit.Address;
                    SelectedClient.Zip = PreviousClientDataBeforeEdit.Zip;
                    SelectedClient.City = PreviousClientDataBeforeEdit.City;
                    SelectedClient.State = PreviousClientDataBeforeEdit.State;
                    SelectedClient.Email = PreviousClientDataBeforeEdit.Email;
                    SelectedClient.IsDeleted = PreviousClientDataBeforeEdit.IsDeleted;
                    SelectedClient.LogInUserId = PreviousClientDataBeforeEdit.LogInUserId;
                }
                if (CloseEvent != null)
                    CloseEvent();
            }
            catch(Exception)
            { 
            }
        }

        public Client Show()
        {
            if (OpenClientEvent != null)
            {
                OpenClientEvent();
            }
            return SelectedClient;
        }
        public Client GetClientData(Client Client)
        {
            return (new Client
            {
                Address = Client.Address,
                City = Client.City,
                ClientId = Client.ClientId,
                Email = Client.Email,
                LicenseeId = SharedVMData.SelectedLicensee.LicenseeId,//GlobalData.PolicyManagerSelectedLicenseeId,
                Name = Client.Name,
                State = Client.State,
                Zip = Client.Zip,
                IsDeleted = false,

            });
        }


    }

}
