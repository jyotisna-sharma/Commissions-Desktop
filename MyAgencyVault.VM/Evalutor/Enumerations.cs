//Enumerations.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public enum TokenType
    {
        None = 0,
        Variable = 1,
        Constant = 2,
        Operator = 3
    }

    public enum OperatorSymbol
    {
        None = 0,
        Plus = 1,
        Minus = 2,
        Multiply = 3,
        Divide = 4,
        Modulus = 5,
        OpenParenthesis = 6,
        CloseParenthesis = 7
    }

    public enum OperatorAssociativity
    {
        None = 0,
        LeftToRight = 1,
        RightToLeft = 2
    }
}
