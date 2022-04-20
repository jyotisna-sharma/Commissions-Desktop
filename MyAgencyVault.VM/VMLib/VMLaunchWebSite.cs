using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using MyAgencyVault.VM.CommonItems;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.VM.BaseVM;
using System.ComponentModel;

namespace MyAgencyVault.VMLib
{

    public class VMLaunchWebSite : BaseViewModel
    {

        
        public VMLaunchWebSite(string reqUrl)
        {
            RequestWebUrl = reqUrl;
        }

        private string _requestUrl;
        public string RequestWebUrl
        {
            get
            { return _requestUrl; }
            set
            { _requestUrl = value; }
        }

        private string _filePath;
        private string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                _filePath = value;
            }


        }

        private enum FileExt
        {
            txt = 0,
            pdf,
            csv,
            xls,
            xlsm,

        }
    }
}
