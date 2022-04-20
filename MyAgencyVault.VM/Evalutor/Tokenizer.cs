//Tokenizer.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class Tokenizer
    {
        #region Methods

        public static Expression Split(string expressionString)
        {
            List<Token> tokens = new List<Token>();

            //read the input-expression letter-by-letter and build tokens
            string alphaNumericString = string.Empty;
            try
            {
                if (String.IsNullOrEmpty(expressionString))
                {
                    // return (new Expression() { Tokens = tokens });
                }
                for (int index = 0; index < expressionString.Length; index++)
                {
                    char currentChar = expressionString[index];

                    if (AllOperators.Find("" + currentChar) != null) //if operator
                    {
                        if (alphaNumericString.Length > 0)
                        {
                            tokens.Add(Token.Resolve(alphaNumericString));
                            alphaNumericString = string.Empty;
                        }

                        tokens.Add(ExpressionUtility.CreateOperatorToken(currentChar));
                    }
                    else if (Char.IsLetterOrDigit(currentChar) || currentChar == '.') //if alphabet or digit or dot
                    {
                        alphaNumericString += currentChar;
                    }
                }

                //check if any token at last
                if (alphaNumericString.Length > 0)
                {
                    tokens.Add(Token.Resolve(alphaNumericString));
                }
            }
            catch { }
            return (new Expression() { Tokens = tokens });
        }

        #endregion
    }
}
