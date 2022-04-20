//Variable.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class Variable : ITokenObject
    {
        #region Properties

        public string Name { get; set; }
        public string Value { get; set; }

        #endregion

        #region Constructors

        public Variable()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return (this.Name);
        }

        #endregion
    }
}
