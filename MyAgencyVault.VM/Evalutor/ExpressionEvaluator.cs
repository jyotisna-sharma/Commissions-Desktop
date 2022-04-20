//ExpressionEvaluator.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class ExpressionEvaluator
    {
        public static List<LogEntry> _log = new List<LogEntry>();

        #region Methods

        /// <summary>
        /// Evaluates an arithmetic expression. An error-free postfix expression is required as input.
        /// The input expression should contain operators and numbers only.
        /// </summary>
        /// <param name="postfixTokens">The list of tokens of postfix form of an expression.</param>
        /// <returns>The evaluated result (in double).</returns>
        public static double EvaluateBasic(List<Token> postfixTokens)
        {
          
            _log.Clear();

            //add header-log-entry
            LogEntry headerLogEntry = new LogEntry();
            headerLogEntry.TokenAction = "EVALUATE POSTFIX:";
            _log.Add(headerLogEntry);

            //start evaluation
            Stack<Token> stack = new Stack<Token>();

            foreach (Token token in postfixTokens)
            {
                LogEntry newLogEntry = new LogEntry();
                newLogEntry.TokenAction = "Read: " + token.LinearToken;

                if (token.Type == TokenType.Constant)
                {
                    stack.Push(token);
                    ExpressionValidator.AddLogOperation(ref newLogEntry, "Push " + token.LinearToken + " to stack", new List<Token>(), stack);
                }
                else if (token.Type == TokenType.Operator)
                {
                    Token temp = stack.Pop();
                    ExpressionValidator.AddLogOperation(ref newLogEntry, "Pop " + temp.LinearToken + " from stack", new List<Token>(), stack);
                    Token top = stack.Pop();
                    ExpressionValidator.AddLogOperation(ref newLogEntry, "Pop " + top.LinearToken + " from stack", new List<Token>(), stack);

                    ExpressionValidator.AddLogOperation(ref newLogEntry, "Evaluate " + top.LinearToken + token.LinearToken + temp.LinearToken, new List<Token>(), stack);
                    Token returnValue = EvaluateArithmetic(top, temp, token);
                    stack.Push(returnValue);
                    ExpressionValidator.AddLogOperation(ref newLogEntry, "Push " + returnValue.LinearToken + " to stack", new List<Token>(), stack);
                }

                //add current operation to log
                try
                {
                    _log.Add(newLogEntry);
                }
                catch { }
            }

            LogEntry lastLogEntry = new LogEntry();
            lastLogEntry.TokenAction = "(After reading all tokens)";

            double result = double.NaN;
            if (stack.Count == 1)
            {
                Token top = stack.Pop();
                ExpressionValidator.AddLogOperation(ref lastLogEntry, "Pop " + top.LinearToken + " from stack", new List<Token>(), stack);
                result = double.Parse(top.LinearToken);
            }

            try
            {
                _log.Add(lastLogEntry);
            }
            catch { }

            return (result);
        }

        #endregion

        #region Internal-Methods

        private static Token EvaluateArithmetic(Token token1, Token token2, Token operatorToken)
        {
            //check if both tokens are numeric constants
            double number1 = double.NaN;
            double number2 = double.NaN;
            bool validNumber1 = (token1.Type == TokenType.Constant) && double.TryParse(token1.LinearToken, out number1);
            bool validNumber2 = (token2.Type == TokenType.Constant) && double.TryParse(token2.LinearToken, out number2);

            if (validNumber1 && validNumber2)
            {
                Operator op = AllOperators.Find(operatorToken.LinearToken);
                double returnValue = double.NaN;

                switch (op.Symbol)
                {
                    case OperatorSymbol.Plus:
                        {
                            returnValue = number1 + number2;
                            break;
                        }
                    case OperatorSymbol.Minus:
                        {
                            returnValue = number1 - number2;
                            break;
                        }
                    case OperatorSymbol.Multiply:
                        {
                            returnValue = number1 * number2;
                            break;
                        }
                    case OperatorSymbol.Divide:
                        {
                            try
                            {

                                returnValue = number1 / number2;
                            }
                            catch { }
                            break;
                        }
                    case OperatorSymbol.Modulus:
                        {
                            try
                            {
                                returnValue = number1 % number2;
                            }
                            catch { }
                            break;
                        }
                }

                returnValue = Math.Round(returnValue, 4);
                Constant returnValueConstant = new Constant() { Value = returnValue.ToString() };
                return (new Token(TokenType.Constant, returnValueConstant));
            }
            else
            {
                return (ExpressionUtility.CreateNumberConstantToken("1"));
            }
        }

        #endregion
    }
}
