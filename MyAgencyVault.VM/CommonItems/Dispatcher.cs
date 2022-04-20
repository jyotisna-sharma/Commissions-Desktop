using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.VM.CommonItems
{
    public static class DispatchService
    {
        public static Dispatcher DispatchObject { get; set; }

        public static void Dispatch(Action action)
        {
            if (DispatchObject == null || DispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                DispatchObject.Invoke(action);
            }
        }
    }
}
