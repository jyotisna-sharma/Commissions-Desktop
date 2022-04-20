//Constant.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class Constant : ITokenObject
    {
        #region Properties

        public string Value { get; set; }

        #endregion

        #region Constructors

        public Constant()
        {
            Value = string.Empty;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return (this.Value);
        }

        #endregion
    }
}
