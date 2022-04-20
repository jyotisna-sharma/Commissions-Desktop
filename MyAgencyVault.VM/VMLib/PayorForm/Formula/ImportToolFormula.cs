using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
namespace MyAgencyVault.VM.VMLib.PayorForm.Formula
{
    public class ImportToolFormula : INotifyPropertyChanged
    {
        private ObservableCollection<ImportToolExpressionToken> _operators;
        public ObservableCollection<ImportToolExpressionToken> Operators
        {
            get { return _operators; }
            set
            {
                _operators = value;
                OnPropertyChanged("Operators");
            }
        }

        private ObservableCollection<ImportToolExpressionToken> _variables;
        public ObservableCollection<ImportToolExpressionToken> Variables
        {
            get { return _variables; }
            set
            {
                _variables = value;
                OnPropertyChanged("Variables");
            }
        }

        private string _expression;
        public string Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                OnPropertyChanged("Expression");
            }
        }

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ImportToolExpressionTokenType
    {
        Operator = 1,
        Variable = 2,
        OpenParanthesis = 3,
        CloseParathesis = 4,
        Value = 5
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImportToolExpressionToken : INotifyPropertyChanged
    {
        private string _TokenString;
        public string TokenString
        {
            get { return _TokenString; }
            set
            {
                _TokenString = value;
                OnPropertyChanged("TokenString");
            }
        }

        private ImportToolExpressionTokenType _TokenType;
        public ImportToolExpressionTokenType TokenType
        {
            get { return _TokenType; }
            set
            {
                _TokenType = value;
                OnPropertyChanged("TokenType");
            }
        }

        /// <summary>
        /// ExpressionToken that can be stack.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<ImportToolExpressionToken> getOperatorExprTokens()
        {
            ObservableCollection<ImportToolExpressionToken> expressionToken = new ObservableCollection<ImportToolExpressionToken>();

            ImportToolExpressionToken token = new ImportToolExpressionToken { TokenString = "+", TokenType = ImportToolExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "-", TokenType = ImportToolExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "*", TokenType = ImportToolExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "/", TokenType = ImportToolExpressionTokenType.Operator };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "(", TokenType = ImportToolExpressionTokenType.OpenParanthesis };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = ")", TokenType = ImportToolExpressionTokenType.CloseParathesis };
            expressionToken.Add(token);

            return expressionToken;
        }

        public static ObservableCollection<ImportToolExpressionToken> getVariableExprTokens()
        {
            ObservableCollection<ImportToolExpressionToken> expressionToken = new ObservableCollection<ImportToolExpressionToken>();

            ImportToolExpressionToken token = new ImportToolExpressionToken { TokenString = "Item1", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "Item2", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "Item3", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "Item4", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "Item5", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);
            token = new ImportToolExpressionToken { TokenString = "Item6", TokenType = ImportToolExpressionTokenType.Variable };
            expressionToken.Add(token);

            return expressionToken;
        }

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// 
    /// </summary>
    public class ImportToolExpressionStack : Stack<ExpressionToken>
    {
        public int ParanthesisCount = 0;
        public string Expression = string.Empty;
        public string TempExpression = string.Empty;
        public int Length = 0;

        public ImportToolExpressionStack() { }

        /// <summary>
        /// Causion :- T
        /// </summary>
        /// <param name="expression"></param>
        public ImportToolExpressionStack(string expression)
        {
            if (string.IsNullOrEmpty(expression.Trim()))
                return;

            this.Expression = string.Empty;
            this.TempExpression = expression;

            ExpressionToken tkn = getExpressionToken();

            do
            {
                this.PushToken(tkn);
                tkn = getExpressionToken();
            } while (tkn != null);

            Length = this.PeekToken().TokenString.Length;
        }

        private ExpressionToken getExpressionToken()
        {
            string tkn = ExtractToken();

            if (string.IsNullOrEmpty(tkn.Trim()))
                return null;

            ExpressionToken expToken = new ExpressionToken();
            switch (tkn)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    expToken.TokenType = ExpressionTokenType.Operator;
                    expToken.TokenString = tkn;
                    break;
                case "(":
                    expToken.TokenType = ExpressionTokenType.OpenParanthesis;
                    expToken.TokenString = tkn;
                    break;
                case ")":
                    expToken.TokenType = ExpressionTokenType.CloseParathesis;
                    expToken.TokenString = tkn;
                    break;
                default:
                    if (tkn == "100")
                    {
                        expToken.TokenType = ExpressionTokenType.Value;
                        expToken.TokenString = tkn;
                    }
                    else
                    {
                        expToken.TokenType = ExpressionTokenType.Variable;
                        expToken.TokenString = tkn;
                    }
                    break;
            }
            return expToken;
        }

        private string ExtractToken()
        {
            string token = string.Empty;
            List<string> OperatorList = new List<string> { "+", "-", "*", "/", "(", ")" };
            int index = 0;

            TempExpression = TempExpression.Trim();
            if (TempExpression.Length != 0)
            {
                if (OperatorList.Contains(TempExpression[0].ToString()))
                {
                    index = 1;
                }
                else
                {
                    index = 0;
                    while ((index < TempExpression.Length) && (!OperatorList.Contains(TempExpression[index].ToString())))
                    {
                        index++;
                    }
                }
            }

            token = TempExpression.Substring(0, index);
            TempExpression = TempExpression.Remove(0, index);
            return token;
        }

        public List<ExpressionToken> getExpressionTokenList()
        {
            return this.Reverse().ToList();
        }

        public void ClearFormula()
        {
            Expression = string.Empty;
            Length = 0;
            Clear();
        }

        public void PushToken(ExpressionToken token)
        {
            ExpressionToken peekToken = PeekToken();
            //bool replaceCase = false;

            //if ((peekToken != null) && (peekToken.TokenType == token.TokenType))
            //    replaceCase = true;

            //if (replaceCase == false)
            //{
            if (!ExpressionValidationRule.ValidationRule(peekToken, token))
                return;
            //}

            if (token.TokenType == ExpressionTokenType.OpenParanthesis)
                ParanthesisCount++;

            if (token.TokenType == ExpressionTokenType.CloseParathesis)
                ParanthesisCount--;

            //if (replaceCase)
            //{
            //    PopToken();
            //    Push(token);
            //}
            //else
            //{
            Push(token);
            //}

            Expression += token.TokenString;
            Length = token.TokenString.Length;
        }

        public ExpressionToken PopToken()
        {
            ExpressionToken expToken = Pop();

            if (expToken.TokenType == ExpressionTokenType.OpenParanthesis)
                ParanthesisCount--;

            if (expToken.TokenType == ExpressionTokenType.CloseParathesis)
                ParanthesisCount++;

            Expression = Expression.Substring(0, Expression.Length - Length);

            if (PeekToken() != null)
                Length = PeekToken().TokenString.Length;
            else
                Length = 0;

            return expToken;
        }

        private ExpressionToken PeekToken()
        {
            if (this.Count != 0)
                return Peek();
            else
                return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class ImportToolExpressionValidationRule
    {
        public bool CanVariableAppended;
        public bool CanOperatorAppended;
        public bool CanOpenParanthesisAppended;
        public bool CanClosedParanthesisAppended;
        public bool CanStartExpression;

        private static ImportToolExpressionValidationRule Rule;

        private ImportToolExpressionValidationRule() { }

        private static ImportToolExpressionValidationRule getValidationRule()
        {
            if (Rule == null)
                Rule = new ImportToolExpressionValidationRule();
            return Rule;
        }

        public static bool ValidationRule(ImportToolExpressionToken peekToken, ImportToolExpressionToken nextToken)
        {
            bool allowed = false;
            try
            {
                ImportToolExpressionValidationRule valRule = null;

                if (nextToken == null)
                    return allowed;

                if (peekToken == null)
                {
                    valRule = getExpressionRule(nextToken.TokenType);
                    if (valRule.CanStartExpression)
                        allowed = true;
                }
                else
                {
                    valRule = getExpressionRule(peekToken.TokenType);

                    switch (nextToken.TokenType)
                    {
                        case ImportToolExpressionTokenType.Operator:
                            if (valRule.CanOperatorAppended)
                                allowed = true;
                            break;
                        case ImportToolExpressionTokenType.Variable:
                        case ImportToolExpressionTokenType.Value:
                            if (valRule.CanVariableAppended)
                                allowed = true;
                            break;
                        case ImportToolExpressionTokenType.OpenParanthesis:
                            if (valRule.CanOpenParanthesisAppended)
                                allowed = true;
                            break;
                        case ImportToolExpressionTokenType.CloseParathesis:
                            if (valRule.CanClosedParanthesisAppended)
                                allowed = true;
                            break;
                        default:
                            break;
                    }
                }

            }
            catch { }
            return allowed;
        }

        private static ImportToolExpressionValidationRule getExpressionRule(ImportToolExpressionTokenType tokenType)
        {

            ImportToolExpressionValidationRule token = getValidationRule();
            if (token != null)
            {
                token.CanClosedParanthesisAppended = false;
                token.CanOpenParanthesisAppended = false;
                token.CanOperatorAppended = false;
                token.CanStartExpression = false;
                token.CanVariableAppended = false;

                switch (tokenType)
                {
                    case ImportToolExpressionTokenType.Operator:
                        token.CanOpenParanthesisAppended = true;
                        token.CanVariableAppended = true;
                        break;
                    case ImportToolExpressionTokenType.Variable:
                    case ImportToolExpressionTokenType.Value:
                        token.CanClosedParanthesisAppended = true;
                        token.CanOperatorAppended = true;
                        token.CanStartExpression = true;
                        break;
                    case ImportToolExpressionTokenType.OpenParanthesis:
                        token.CanVariableAppended = true;
                        token.CanOpenParanthesisAppended = true;
                        token.CanStartExpression = true;
                        break;
                    case ImportToolExpressionTokenType.CloseParathesis:
                        token.CanClosedParanthesisAppended = true;
                        token.CanOperatorAppended = true;
                        break;
                    default:
                        break;
                }
            }
            return token;

        }
    }
}