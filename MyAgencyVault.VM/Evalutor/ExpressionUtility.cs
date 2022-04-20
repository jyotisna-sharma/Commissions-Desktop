//ExpressionBuilderUtility.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class ExpressionUtility
    {
        public static bool IsOpenParenthesis(Token token)
        {
            return (token.Type == TokenType.Operator && token.LinearToken.Equals(AllOperators.Find(OperatorSymbol.OpenParenthesis).SymbolText));
        }

        public static bool IsCloseParenthesis(Token token)
        {
            return (token.Type == TokenType.Operator && token.LinearToken.Equals(AllOperators.Find(OperatorSymbol.CloseParenthesis).SymbolText));
        }

        public static bool IsArithmeticOperator(Token token)
        {
            return (token.Type == TokenType.Operator && !IsOpenParenthesis(token) && !IsCloseParenthesis(token));
        }

        //checks whether the input expression contains only number-constants and operators.
        public static bool IsArithmeticExpression(Expression expression)
        {
            foreach (Token token in expression.Tokens)
            {
                if (!(token.Type == TokenType.Constant || token.Type == TokenType.Operator))
                {
                    return (false);
                }
            }

            return (true);
        }

        public static Token CreateOperatorToken(char op)
        {
            Operator operatorObject = AllOperators.Find("" + op);
            return (new Token(TokenType.Operator, operatorObject));
        }

        public static Token CreateNumberConstantToken(string numberString)
        {
            Constant constant = new Constant() { Value = numberString };
            return (new Token(TokenType.Constant, constant));
        }
    }
}
