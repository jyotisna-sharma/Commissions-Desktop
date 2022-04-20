//Expression.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class Expression
    {
        #region Properties

        public List<Token> Tokens { get; set; }

        #endregion

        #region Constructors

        public Expression()
        {
            Tokens = new List<Token>();
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return (string.Join("  ", (from token in this.Tokens
                                       select token.LinearToken).ToArray()));
        }

        #endregion
    }
}
