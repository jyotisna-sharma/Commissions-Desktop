using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Windows.Input;
using MyAgencyVault.VM.BaseVM;

namespace MyAgencyVault.VM.VMLib
{
    public class StatementManagerVM : BaseVM.BaseViewModel
    {
        #region "Properties"
        //
        List<Batch> Batches { get; set; }
       
        //
        PayorSiteLoginInfo PayorSiteLoginInformation { get; set; }
        //

        //private int _currentProgress;
        //public int CurrentProgress
        //{
        //    get { return this._currentProgress; }
        //    private set
        //    {
        //        if (this._currentProgress != value)
        //        {
        //            this._currentProgress = value;
        //            OnPropertyChanged("CurrentProgress");
        //        }
        //    }
        //}

        #endregion 
        #region "Commands"
        private ICommand _clearDownloadStatusCommand;
        public ICommand ClearDownloadStatusCommand
        {
            get
            {
                if (_clearDownloadStatusCommand == null)
                {
                    _clearDownloadStatusCommand = new BaseCommand(x => CanExecuteClearDownloadStatusCommand(), x=>ExcuteClearDownloadStatus(x));
                }
                return _clearDownloadStatusCommand;
            }
        }

        private bool CanExecuteClearDownloadStatusCommand()
        {
            throw new NotImplementedException();
        }

        public void ExcuteClearDownloadStatus(object x)
        {
            
        }

        ICommand DeleteRecordCommand;
        ICommand LaunchWebSiteCommand;
        ICommand ShowDownloadAvailableCommand;
        ICommand ShowInProcessCommand;
        ICommand ShowOpenCommand;
        ICommand ShowClosedCommand;
        ICommand ShowAllCommand;
        ICommand SaveRecordCommand;
        
        #endregion 


    
        
    }
}
