using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.ViewModel.VMLib;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows;

namespace MyAgencyVault.VM.VMLib.Configuration
{
    public class ConfigPayorContactsVM : BaseViewModel
    {
        private VMConfigrationManager ParentVM;
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
        public ConfigPayorContactsVM(VMConfigrationManager configManager)
        {
            ParentVM = configManager;
            ParentVM.SelectedPayorChanged += new VMConfigrationManager.SelectedPayorChangedEventHandler(ParentVM_SelectedPayorChanged);
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentVM_PropertyChanged);

            PriorityList = ConfigurationContactComman.PrirtyLst;
            ContactPrefLst = ConfigurationContactComman.PrirtyContectLst;
        }

        void ParentVM_SelectedPayorChanged(Payor SelectedPayor)
        {
            CurrentPayor = SelectedPayor;
        }

        void ParentVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {

                    case "CurrentPayor":
                        if (CurrentPayor != null)// cnange by vinod
                            ContactList = serviceClients.GlobalPayorContactClient.getContacts(CurrentPayor.PayorID);

                        if (ContactList != null)
                            SelectedContact = ContactList.FirstOrDefault();

                        break;

                    case "SelectedContact":
                        SelectedContactPref = SelectedContact.ContactPref == null ? ContactPrefLst.FirstOrDefault() : (from u in ContactPrefLst
                                                                                                                       where (u.ContPref == SelectedContact.ContactPref)
                                                                                                                       select u).FirstOrDefault();
                        SelectedPriorty = SelectedContact.Priority == null ? PriorityList.FirstOrDefault() : (from u in PriorityList
                                                                                                              where (u.PriorityName == SelectedContact.Priority)
                                                                                                              select u).FirstOrDefault();
                        break;

                }
            }
            catch
            {
            }

        }

        #region Property

        private Payor _CurrentPayor;
        public Payor CurrentPayor
        {
            get
            {
                return _CurrentPayor;
            }
            set
            {
                _CurrentPayor = value;
                OnPropertyChanged("CurrentPayor");
            }
        }

        private ObservableCollection<GlobalPayorContact> _contactlst;
        public ObservableCollection<GlobalPayorContact> ContactList
        {
            get
            {
                return _contactlst;
            }
            set
            {
                _contactlst = value;
                OnPropertyChanged("ContactList");
            }
        }

        private GlobalPayorContact _selectedContact;
        public GlobalPayorContact SelectedContact
        {
            get
            {
                if (_selectedContact == null)
                {
                    _selectedContact = new GlobalPayorContact { FirstName = string.Empty };
                }
                return _selectedContact;
            }
            set
            {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
        }

        public List<Priority> PriorityList { get; set; }
        
        public List<ContactPref> ContactPrefLst { get; set; }
        
        private Priority _selectedPriority;
        public Priority SelectedPriorty
        { 
            get { return _selectedPriority == null ? new Priority { PId = 0 } : _selectedPriority; } 
            set { _selectedPriority = value; OnPropertyChanged("SelectedPriorty"); } 
        }

        private ContactPref _selectedcontactPref;
        public ContactPref SelectedContactPref
        {
            get { return _selectedcontactPref == null ? new ContactPref { ContPrefId = 0 } : _selectedcontactPref; }
            set { _selectedcontactPref = value; OnPropertyChanged("SelectedContactPref"); }
        }

        private string _ZipCode;
        public string ZipCode
        {
            get { return _ZipCode; }
            set { _ZipCode = value; OnPropertyChanged("ZipCode"); }
        }

        #endregion

        #region Commands

        /// <summary>
        /// 
        /// </summary>
        private ICommand _newComdContact;
        public ICommand NewComdContact
        {
            get
            {
                if (_newComdContact == null)
                    _newComdContact = new BaseCommand(x => BeforeNewCommandContact(),x => NewCommandContact());
                return _newComdContact;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private ICommand _saveCmdContact;
        public ICommand SaveContact
        {
            get
            {
                if (_saveCmdContact == null)
                {
                    _saveCmdContact = new BaseCommand(x => ValidateContact(), x => InsertUpdateContact());
                }
                return _saveCmdContact;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private ICommand _deletePayorContact;
        public ICommand DeletePayorContact
        {
            get
            {
                if (_deletePayorContact == null)
                {
                    _deletePayorContact = new BaseCommand(x => BeforeDelteContact(),x => DelteContact());
                }
                return _deletePayorContact;
            }

        }

        #endregion

        #region Methods

        private bool BeforeNewCommandContact()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;
            else
                return true;
        }

        private bool BeforeDelteContact()
        {
            if (SelectedContact == null || SelectedContact.GlobalPayorId == Guid.Empty)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;
            
            return true;
        }

        private bool ValidateContact()
        {
            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (CurrentPayor == null || CurrentPayor.PayorID == Guid.Empty)
                return false;

            if (string.IsNullOrEmpty(SelectedContact.FirstName) || string.IsNullOrEmpty(SelectedContact.LastName))
                return false;

            return true;
        }

        private void DelteContact()
        {
            try
            {
                if (SelectedContact.PayorContactId != null)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure?", "Delete Warning", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!SelectedContact.IsNew)
                            serviceClients.GlobalPayorContactClient.DeleteGlobalPayorContact(SelectedContact);

                        ContactList.Remove(SelectedContact);
                        SelectedContact = ContactList.FirstOrDefault();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void NewCommandContact()
        {
            SelectedContact = new GlobalPayorContact { PayorContactId = Guid.Empty,IsNew = true };
        }

        private void InsertUpdateContact()
        {
            try
            {
                if (CurrentPayor.PayorID == Guid.Empty)
                {
                    System.Windows.MessageBox.Show("Please save payor.", "Error", MessageBoxButton.OK);
                    return;
                }

                if (ContactList == null)
                    ContactList = new ObservableCollection<GlobalPayorContact>();

                int DuplicateUsers = ContactList.Where(u => u.Email == SelectedContact.Email).Count();

                if ((ContactList.Contains(SelectedContact) || DuplicateUsers > 0) && SelectedContact.PayorContactId == Guid.Empty)
                {
                    System.Windows.MessageBox.Show("Contact Already Exist", "Error", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    SelectedContact.PayorContactId = SelectedContact.PayorContactId == Guid.Empty ? Guid.NewGuid() : SelectedContact.PayorContactId;
                    SelectedContact.GlobalPayorId = CurrentPayor.PayorID;
                    SelectedContact.Priority = SelectedPriorty.PriorityName;
                    SelectedContact.ContactPref = SelectedContactPref.ContPref;
                    serviceClients.GlobalPayorContactClient.AddUpdateGlobalPayorContact(SelectedContact);
                    SelectedContact.IsNew = false;

                    if (CurrentPayor.Contacts == null)
                        CurrentPayor.Contacts = new ObservableCollection<GlobalPayorContact>();

                    if (DuplicateUsers == 0)
                    {
                        ContactList.Add(SelectedContact);
                        CurrentPayor.Contacts.Add(SelectedContact);
                    }
                    else
                    {
                        CurrentPayor.Contacts.Remove((from s in CurrentPayor.Contacts where (s.GlobalPayorId == SelectedContact.GlobalPayorId) select s).FirstOrDefault());
                        CurrentPayor.Contacts.Add(SelectedContact);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region based on zipcode find city and state

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
            if (SelectedContact.Zip != null && SelectedContact.Zip.ToString().Length == 5)
            {
                Zip zipData = serviceClients.MasterClient.GetZip(SelectedContact.Zip);
                if (zipData != null)
                {
                    SelectedContact.City = zipData.City;
                    SelectedContact.State = zipData.State;
                }
            }
        }

        #endregion
    }
}
