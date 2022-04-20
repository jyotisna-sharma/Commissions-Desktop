using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class ExpressionResult
    {
        public bool ErrorInEvalution;
        public string ErrorDescription;
        public double Result;
    }

    public class ExpressionExecutor
    {
        public static ExpressionResult IsValidExpression(string expression)
        {
             ExpressionResult ExpValidationResult = new ExpressionResult();
             Expression inputExpression = Tokenizer.Split(expression);
            List<Token> postfixTokens = new List<Token>();
            bool isError = false;
            int errorTokenIndex = -1;
            string errorMessage = string.Empty;
            try
            {

                ExpressionValidator.ValidateWithLog(inputExpression, ref postfixTokens, out isError, out errorTokenIndex, out errorMessage);
            }
            catch
            { }
            if (isError)
            {
                ExpValidationResult.ErrorDescription = "Error at Token" + errorTokenIndex + " ['" + inputExpression.Tokens[errorTokenIndex].LinearToken + "']. " + errorMessage;
                ExpValidationResult.ErrorInEvalution = true;
                ExpValidationResult.Result = 0.0;
            }

            return ExpValidationResult;
        }

        public static ExpressionResult ExecuteExpression(string expression)
        {
            ExpressionResult ExpResult = new ExpressionResult();
            Expression inputExpression = Tokenizer.Split(expression);
            List<Token> postfixTokens = new List<Token>();
            bool isError = false;
            int errorTokenIndex = -1;
            string errorMessage = string.Empty;
            try
            {

                ExpressionValidator.ValidateWithLog(inputExpression, ref postfixTokens, out isError, out errorTokenIndex, out errorMessage);
            }
            catch { }
            if (!ExpressionUtility.IsArithmeticExpression(inputExpression))
            {
                ExpResult.ErrorDescription = "Expression is not arithmatic expression.";
                ExpResult.ErrorInEvalution = true;
                ExpResult.Result = 0.0;
            }
            else if (isError)
            {
                ExpResult.ErrorDescription = "Error at Token" + errorTokenIndex + " ['" + inputExpression.Tokens[errorTokenIndex].LinearToken + "']. " + errorMessage;
                ExpResult.ErrorInEvalution = true;
                ExpResult.Result = 0.0;
            }
            else
            {
                double resultValue = ExpressionEvaluator.EvaluateBasic(postfixTokens);
                ExpResult.ErrorDescription = string.Empty;
                ExpResult.ErrorInEvalution = false;
                ExpResult.Result = resultValue;
            }

            return ExpResult;
        }
    }
}
