using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.VM.BaseVM;
using System.Windows.Input;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM;
using System.Windows;
using MyAgencyVault.ViewModel.Converters;
using System.ComponentModel;

namespace MyAgencyVault.VMLib.PolicyManager
{
    public class VMPolicyNote : BaseViewModel
    {
        #region Constructor

        public VMPolicyNote(VMOptimizePolicyManager OptimizedPolicyManager)
        {
            PropertyChanged += new PropertyChangedEventHandler(VMPolicyNote_PropertyChanged);
            OptimizedPolicyManager.SelectedPolicyChanged += new VMOptimizePolicyManager.SelectedPolicyChangedEventHandler(OptimizedPolicyManager_SelectedPolicyChanged);

            serviceClients.NoteClient.GetNotesPolicyWiseCompleted += new EventHandler<GetNotesPolicyWiseCompletedEventArgs>(NoteClient_GetNotesPolicyWiseCompleted);
        }

        void VMPolicyNote_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectNote":
                    RTFNotecontent = SelectNote.Content;
                    break;
            }
        }

        #endregion
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
        #region Selected Object

        private PolicyDetailsData SelectedPolicy;

        void OptimizedPolicyManager_SelectedPolicyChanged(PolicyDetailsData SelectedPolicy)
        {
            this.SelectedPolicy = SelectedPolicy;
            if (this.SelectedPolicy == null || this.SelectedPolicy.PolicyId == Guid.Empty)
            {
                PolicyNote = null;
            }
            else
            {
                serviceClients.NoteClient.GetNotesPolicyWiseAsync(SelectedPolicy.PolicyId);
                try
                {
                    //PolicyNote = serviceClients.NoteClient.GetNotesPolicyWise(SelectedPolicy.PolicyId);
                    //SelectNote = PolicyNote.FirstOrDefault();
                    //if (SelectNote != null)
                    //{
                    //    RTFNotecontent = SelectNote.Content;
                    //}

                }
                catch
                {
                }
            }
        }

        #endregion

        #region PolicyNotes

        void NoteClient_GetNotesPolicyWiseCompleted(object sender, GetNotesPolicyWiseCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                PolicyNote = e.Result;
                try
                {
                    SelectNote = PolicyNote.FirstOrDefault();
                    if (SelectNote != null)
                    {
                        RTFNotecontent = SelectNote.Content;
                    }
                }
                catch
                {
                }
            }
        }

        private ObservableCollection<PolicyNotes> _policyNote;
        public ObservableCollection<PolicyNotes> PolicyNote
        {
            get
            {
                return _policyNote;
            }
            set
            {
                _policyNote = value;
                OnPropertyChanged("PolicyNote");
            }
        }

        private PolicyNotes _selectNote;
        public PolicyNotes SelectNote
        {
            get
            {
                return _selectNote == null ? new PolicyNotes() : _selectNote;
            }
            set
            {
                _selectNote = value;
                OnPropertyChanged("SelectNote");
            }
        }

        private ICommand _newNote;
        public ICommand NewNote
        {
            get
            {
                if (_newNote == null)
                {
                    _newNote = new BaseCommand(X => BeforeCreateNewNote(), X => CreateNewNote());
                }
                return _newNote;
            }

        }

        private bool BeforeCreateNewNote()
        {   

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;

            //check if policy is saved or not
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;

        }

        private string _notecontent;
        public string RTFNotecontent
        {
            get { return _notecontent; }
            set { _notecontent = value; OnPropertyChanged("RTFNotecontent"); }
        }
        

        private void CreateNewNote()
        {
            try
            {
                RTFNotecontent = "";
                if (PolicyNote == null)
                {
                    PolicyNote = new ObservableCollection<PolicyNotes>();
                }

                if (SelectedPolicy != null)
                {
                    PolicyNotes _PolicyNote = new PolicyNotes()
                    {
                        NoteID = Guid.NewGuid(),
                        PolicyID = SelectedPolicy.PolicyId,
                        Content = RTFNotecontent,
                        CreatedDate = DateTime.Today,
                        LastModifiedDate = DateTime.Today,
                    };

                    PolicyNote.Add(_PolicyNote);
                    SelectNote = _PolicyNote;
                }
            }
            catch
            {
            }
        }

        private ICommand _AddNote;
        public ICommand AddNote
        {
            get
            {
                if (_AddNote == null)
                {
                    _AddNote = new BaseCommand(X => BeforeAddNewNote(), X => AddNewNote());
                }
                return _AddNote;
            }

        }

        private bool BeforeAddNewNote()
        {    
            

            if (SelectedPolicy == null || SelectedPolicy.PolicyId == Guid.Empty)
                return false;
            //change by vinod
            //check if policy is saved or not
            if (SelectedPolicy.IsSavedPolicy == false) return false;

            if (SelectNote.NoteID == Guid.Empty)
                return false;

            if (string.IsNullOrEmpty(RTFNotecontent))
                return false;

            //if (RoleManager.Role == UserRole.Agent && RoleManager.IsHouseAccount == false)
            //    return false;

            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;
            else
                return true;
        }

        private void AddNewNote()
        {
            try
            {
                if (RTFNotecontent == null || RTFNotecontent == string.Empty) return;

                if (SelectedPolicy.PolicyId == Guid.Empty) return;
                SelectNote.Content = RTFNotecontent;

                RTFToTextConverter converter = new RTFToTextConverter();
                SelectNote.SimpleTextContent = converter.Convert(SelectNote.Content);

                SelectNote.LastModifiedDate = DateTime.Today;
                serviceClients.NoteClient.AddUpdateNote(SelectNote);

                if (!PolicyNote.Contains(SelectNote))
                {
                    PolicyNote.Add(SelectNote);
                    PolicyNote = new ObservableCollection<PolicyNotes>(PolicyNote.OrderByDescending(p => p.CreatedDate));

                }
                ///Empty RTFNoteContent
                RTFNotecontent = "";

            }
            catch (Exception)
            {
            }
        }

        private ICommand _deleteNote;
        public ICommand DeleteNote
        {
            get
            {
                if (_deleteNote == null)
                {
                    _deleteNote = new BaseCommand(X => BeforeDODeleteNote(), X => DODeleteNote());
                }
                return _deleteNote;
            }

        }

        private bool BeforeDODeleteNote()
        {
            if (RoleManager.UserAccessPermission(MasterModule.PolicyManager) == ModuleAccessRight.Read)
                return false;

            if (SelectNote == null || SelectNote.NoteID == Guid.Empty)
                return false;
            else
                return true;

            
        }

        private void DODeleteNote()
        {
            try
            {
                if (SelectNote.NoteID == Guid.Empty) return;
                if (MessageBox.Show("Do you want to delete Selected Note", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }

                serviceClients.NoteClient.DeleteNote(SelectNote);
                if (PolicyNote.Contains(SelectNote))
                {
                    PolicyNote.Remove(SelectNote);
                    SelectNote = PolicyNote.FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
