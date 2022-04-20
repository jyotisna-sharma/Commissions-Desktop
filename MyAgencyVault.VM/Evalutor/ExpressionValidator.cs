//ExpressionValidation.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class ExpressionValidator
    {
        public static List<LogEntry> _log = new List<LogEntry>();
        public static bool _useLog = true;

        #region Methods

        public static void ValidateWithLog(Expression expression, ref List<Token> postfixTokens, out bool isError, out int errorTokenIndex, out string errorMessage)
        {
            _log.Clear();

            //add header-log-entry
            LogEntry headerLogEntry = new LogEntry();
            headerLogEntry.TokenAction = "INFIX TO POSTFIX:";
            _log.Add(headerLogEntry);

            #region Preprocess all tokens

            //initialize index of each token
            int tokenIndex = 0;
            foreach (Token token in expression.Tokens)
            {
                token.Index = tokenIndex;
                tokenIndex += 1;
            }

            #endregion

            #region Generate postfix-form of the input expression

            Stack<Token> postfixStack = new Stack<Token>();
            foreach (Token token in expression.Tokens)
            {
                LogEntry newLogEntry = new LogEntry();
                newLogEntry.TokenAction = "Read: " + token.LinearToken;

                if (token.Type == TokenType.Constant) //handle constants
                {
                    postfixTokens.Add(token);
                    AddLogOperation(ref newLogEntry, "Add " + token.LinearToken + " to output", postfixTokens, postfixStack);
                }
                else if (token.Type == TokenType.Variable) //handle variables
                {
                    postfixTokens.Add(token);
                    AddLogOperation(ref newLogEntry, "Add " + token.LinearToken + " to output", postfixTokens, postfixStack);
                }
                else if (token.Type == TokenType.Operator) //handle operators
                {
                    if (ExpressionUtility.IsOpenParenthesis(token)) //handle open-parenthesis
                    {
                        postfixStack.Push(token);
                        AddLogOperation(ref newLogEntry, "Push " + token.LinearToken + " to stack", postfixTokens, postfixStack);
                    }
                    else if (ExpressionUtility.IsCloseParenthesis(token)) //handle close-parenthesis
                    {
                        //pop all operators off the stack onto the output (until left parenthesis)
                        while (true)
                        {
                            if (postfixStack.Count == 0)
                            {
                                isError = true;
                                errorTokenIndex = token.Index;
                                errorMessage = "Mismatched parenthesis.";
                                return;
                            }

                            Token top = postfixStack.Pop();
                            AddLogOperation(ref newLogEntry, "Pop " + top.LinearToken + " from stack", postfixTokens, postfixStack);

                            if (ExpressionUtility.IsOpenParenthesis(top))
                            {
                                AddLogOperation(ref newLogEntry, "Found open-parenthesis", postfixTokens, postfixStack);
                                break;
                            }
                            else
                            {
                                postfixTokens.Add(top);
                                AddLogOperation(ref newLogEntry, "Add " + top.LinearToken + " to output", postfixTokens, postfixStack);
                            }
                        }
                    }
                    else //handle other operators (usually arithmetic)
                    {
                        Operator operator1 = AllOperators.Find(token.LinearToken);

                        while (true)
                        {
                            if (postfixStack.Count == 0)
                            {
                                break;
                            }

                            Token top = postfixStack.Peek();
                            if (ExpressionUtility.IsArithmeticOperator(top))
                            {
                                bool readyToPopOperator2 = false;
                                Operator operator2 = AllOperators.Find(top.LinearToken);
                                if (operator1.Associativity == OperatorAssociativity.LeftToRight && operator1.PrecedenceLevel >= operator2.PrecedenceLevel) //'>' operator implies less precedence
                                {
                                    readyToPopOperator2 = true;
                                }
                                else if (operator1.Associativity == OperatorAssociativity.RightToLeft && operator1.PrecedenceLevel > operator2.PrecedenceLevel)
                                {
                                    readyToPopOperator2 = true;
                                }

                                if (readyToPopOperator2)
                                {
                                    postfixStack.Pop();
                                    AddLogOperation(ref newLogEntry, "Pop " + top.LinearToken + " from stack", postfixTokens, postfixStack);
                                    postfixTokens.Add(top);
                                    AddLogOperation(ref newLogEntry, "Add " + top.LinearToken + " to output", postfixTokens, postfixStack);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        postfixStack.Push(token);
                        AddLogOperation(ref newLogEntry, "Push " + token.LinearToken + " to stack", postfixTokens, postfixStack);
                    }
                }

                //add current operation to log
                _log.Add(newLogEntry);
            }

            //pop entire stack to output
            LogEntry lastLogEntry = new LogEntry();
            lastLogEntry.TokenAction = "(After reading all tokens)";

            while (postfixStack.Count > 0)
            {
                Token top = postfixStack.Pop();
                AddLogOperation(ref lastLogEntry, "Pop " + top.LinearToken + " from stack", postfixTokens, postfixStack);

                if (ExpressionUtility.IsOpenParenthesis(top) || ExpressionUtility.IsCloseParenthesis(top))
                {
                    isError = true;
                    errorTokenIndex = top.Index;
                    errorMessage = "Mismatched Parenthesis.";
                    return;
                }
                else
                {
                    postfixTokens.Add(top);
                    AddLogOperation(ref lastLogEntry, "Add " + top.LinearToken + " to output", postfixTokens, postfixStack);
                }
            }

            _log.Add(lastLogEntry);

            LogEntry postfixLogEntry = new LogEntry();
            postfixLogEntry.TokenAction = "POSTFIX: " + (new Expression() { Tokens = postfixTokens }).ToString();
            _log.Add(postfixLogEntry);

            #endregion

            #region Evaluate the postfix-expression with dummy constants for unknown variables (in order to find errors)

            Stack<Token> evaluateStack = new Stack<Token>();
            foreach (Token token in postfixTokens)
            {
                if (token.Type == TokenType.Constant)
                {
                    evaluateStack.Push(token);
                }
                else if (token.Type == TokenType.Variable)
                {
                    evaluateStack.Push(token);
                }
                else if (token.Type == TokenType.Operator)
                {
                    if (evaluateStack.Count >= 2)
                    {
                        Token temp = evaluateStack.Pop();
                        Token top = evaluateStack.Pop();

                        //add a dummy constant as result of arithmetic evaluation
                        Token dummyConstantToken = ExpressionUtility.CreateNumberConstantToken("1");
                        evaluateStack.Push(dummyConstantToken);
                    }
                    else
                    {
                        isError = true;
                        errorTokenIndex = token.Index;
                        errorMessage = "Missing Operand for Operator '" + token.LinearToken + "'.";
                        return;
                    }
                }
            }

            if (evaluateStack.Count == 1) //there should be exactly one token left in evaluate-stack
            {
                //leave the last token intact (as we are not evaluating to find the result)
                isError = false;
                errorTokenIndex = -1;
                errorMessage = string.Empty;
            }
            else
            {
                isError = true;
                errorTokenIndex = expression.Tokens.Count - 1;
                errorMessage = "Incomplete Expression.";
            }

            #endregion
        }

        public static bool InfixToPostfix(Expression inputExpression, out Expression postfixExpression)
        {
            List<Token> postfixTokens = new List<Token>();
            Stack<Token> postfixStack = new Stack<Token>();

            //process all tokens in input-expression, one by one
            foreach (Token token in inputExpression.Tokens)
            {
                if (token.Type == TokenType.Constant) //handle constants
                {
                    postfixTokens.Add(token);
                }
                else if (token.Type == TokenType.Variable) //handle variables
                {
                    postfixTokens.Add(token);
                }
                else if (token.Type == TokenType.Operator) //handle operators
                {
                    if (ExpressionUtility.IsOpenParenthesis(token)) //handle open-parenthesis
                    {
                        postfixStack.Push(token);
                    }
                    else if (ExpressionUtility.IsCloseParenthesis(token)) //handle close-parenthesis
                    {
                        //pop all operators off the stack onto the output (until left parenthesis)
                        while (true)
                        {
                            if (postfixStack.Count == 0)
                            {
                                postfixExpression = null; //error: mismatched parenthesis
                                return (false);
                            }

                            Token top = postfixStack.Pop();
                            if (ExpressionUtility.IsOpenParenthesis(top)) break;
                            else postfixTokens.Add(top);
                        }
                    }
                    else //handle arithmetic operators
                    {
                        Operator currentOperator = AllOperators.Find(token.LinearToken);

                        if (postfixStack.Count > 0)
                        {
                            Token top = postfixStack.Peek();
                            if (ExpressionUtility.IsArithmeticOperator(top))
                            {
                                Operator stackOperator = AllOperators.Find(top.LinearToken);
                                if ((currentOperator.Associativity == OperatorAssociativity.LeftToRight &&
                                     currentOperator.PrecedenceLevel >= stackOperator.PrecedenceLevel) ||
                                    (currentOperator.Associativity == OperatorAssociativity.RightToLeft &&
                                     currentOperator.PrecedenceLevel > stackOperator.PrecedenceLevel)) //'>' operator implies less precedence
                                {
                                    postfixStack.Pop();
                                    postfixTokens.Add(top);
                                }
                            }
                        }

                        postfixStack.Push(token); //push operator to stack
                    }
                }
            }

            //after reading all tokens, pop entire stack to output
            while (postfixStack.Count > 0)
            {
                Token top = postfixStack.Pop();
                if (ExpressionUtility.IsOpenParenthesis(top) || ExpressionUtility.IsCloseParenthesis(top))
                {
                    postfixExpression = null; //error: mismatched parenthesis
                    return (false);
                }
                else
                {
                    postfixTokens.Add(top);
                }
            }

            postfixExpression = new Expression() { Tokens = postfixTokens };
            return (true);
        }

        #endregion

        #region Helper-Methods

        public static void AddLogOperation(ref LogEntry logEntry, string operationText, List<Token> output, Stack<Token> stack)
        {
            LogOperation operation = new LogOperation();
            operation.OperationText = operationText;
            operation.OutputContents = string.Join("  ", (from token in output select token.LinearToken).ToArray());
            operation.StackContents = string.Join("  ", (from token in stack select token.LinearToken).ToArray());
            logEntry.Operations.Add(operation);
        }

        #endregion
    }
}
