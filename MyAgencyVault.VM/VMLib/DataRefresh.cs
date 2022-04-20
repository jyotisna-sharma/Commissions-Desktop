using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib
{
    interface IDataRefresh
    {
        void Refresh();
        bool RefreshRequired
        {
            get;
            set;
        }
    }
}
