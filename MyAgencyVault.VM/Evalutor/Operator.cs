﻿//Operator.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class Operator : ITokenObject
    {
        #region Properties

        public OperatorSymbol Symbol { get; set; }
        public string SymbolText { get; set; }
        public int OperandCount { get; set; }

        /// <summary>
        /// Indicates precedence over other operators in the expression. Operators with higher precedence
        /// are evaluated before operators with lower precedence. The precedence-level value starts with 1 (highest).
        /// (i.e. the lesser the number, the higher the precedence).
        /// </summary>
        public int PrecedenceLevel { get; set; }

        public OperatorAssociativity Associativity { get; set; }

        #endregion

        #region Constructors

        public Operator()
        {
            Symbol = OperatorSymbol.None;
            SymbolText = string.Empty;
            OperandCount = 0;
            PrecedenceLevel = -1;
            Associativity = OperatorAssociativity.None;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return (SymbolText);
        }

        #endregion
    }

    /// <summary>
    /// List of all operators supported.
    /// </summary>
    public static class AllOperators
    {
        public static List<Operator> Operators { get; set; }

        #region Constructors

        static AllOperators()
        {
            Operators = new List<Operator>();

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.Plus,
                SymbolText = "+",
                OperandCount = 2,
                PrecedenceLevel = 4,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.Minus,
                SymbolText = "-",
                OperandCount = 2,
                PrecedenceLevel = 4,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.Multiply,
                SymbolText = "*",
                OperandCount = 2,
                PrecedenceLevel = 3,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.Divide,
                SymbolText = "/",
                OperandCount = 2,
                PrecedenceLevel = 3,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.Modulus,
                SymbolText = "%",
                OperandCount = 2,
                PrecedenceLevel = 3,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.OpenParenthesis,
                SymbolText = "(",
                OperandCount = 0,
                PrecedenceLevel = 1,
                Associativity = OperatorAssociativity.LeftToRight
            });

            Operators.Add(new Operator()
            {
                Symbol = OperatorSymbol.CloseParenthesis,
                SymbolText = ")",
                OperandCount = 0,
                PrecedenceLevel = 1,
                Associativity = OperatorAssociativity.LeftToRight
            });
        }

        #endregion

        #region Methods

        public static Operator Find(OperatorSymbol symbol)
        {
            foreach (Operator op in Operators)
            {
                if (op.Symbol == symbol)
                {
                    return (op);
                }
            }
            return (null);
        }

        public static Operator Find(string symbolText)
        {
            foreach (Operator op in Operators)
            {
                if (op.SymbolText.Equals(symbolText))
                {
                    return (op);
                }
            }
            return (null);
        }

        #endregion
    }
}
