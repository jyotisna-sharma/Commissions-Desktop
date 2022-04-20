using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.ViewModel.VMLib
{
    public interface IView
    {
        bool Validate(string ScreenName);
    }
}
