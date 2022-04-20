using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib.PayorForm.Formula;
using MyAgencyVault.VM.VMLib.PayorForm.Evalutor;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for AddEditFormula.xaml
    /// </summary>
    public partial class AddEditFormula : Window
    {
        FormulaBindingData formulaBinding;
        ExpressionStack expressionStack;
        Guid FormulaID = Guid.Empty;

        public AddEditFormula(Formula formula, string IdentifyPage)
        {
            InitializeComponent();
            formulaBinding = new FormulaBindingData();

            if (formula == null)
            {
                expressionStack = new ExpressionStack();
            }
            else
            {
                formulaBinding.Expression = formula.FormulaExpression;
                expressionStack = new ExpressionStack(formula.FormulaExpression);
                txtTitle.Text = formula.FormulaTtitle;
                FormulaID = formula.FormulaID;
            }
            myGrid.DataContext = formulaBinding;

            //add code for text chage save button

            if (IdentifyPage == "PayorTool")
            {
                btnSave.Content = "Save";
            }
            if (IdentifyPage == "BillingManager")
            {
                btnSave.Content = "Done";
            }

        }
        private void Clear_Clicked(object sender, EventArgs e)
        {
            expressionStack.ClearFormula();
            formulaBinding.Expression = string.Empty;
        }

        private void formulaCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == formulaCombobox)
            {
                ExpressionToken selectedToken = formulaCombobox.SelectedItem as ExpressionToken;
                expressionStack.PushToken(selectedToken);
                formulaBinding.Expression = expressionStack.Expression;
            }
        }

        public AddEditFormula(FormulaWindowVM objFromula)
        {
            InitializeComponent();
            formulaBinding = new FormulaBindingData();
            expressionStack = new ExpressionStack();
            myGrid.DataContext = formulaBinding;
        }

        private void TestFormula_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(expTextBlock.Text.Trim())) return;
            string Expression = formulaBinding.Expression;
            List<ExpressionToken> expTokens = expressionStack.getExpressionTokenList();

            if (expTokens != null)
            {
                expTokens = expTokens.Where(s => s.TokenType == ExpressionTokenType.Variable).Distinct().OrderByDescending(s => s.TokenString.Length).ToList();
                foreach (ExpressionToken token in expTokens)
                {
                    if (token.TokenType == ExpressionTokenType.Variable)
                    {
                        if (Expression.Contains(token.TokenString))
                        {
                            TestFormula testFormulaDialog = new TestFormula(token.TokenString, 0);
                            testFormulaDialog.ShowDialog();
                            Expression = Expression.Replace(token.TokenString, testFormulaDialog.Value.ToString());
                        }
                    }
                }

                //ExpressionResult result = ExpressionExecutor.ExecuteExpression(Expression);
                //double result = (double)new NCalc.Expression(Expression).Evaluate();
                var result = new NCalc.Expression(Expression).Evaluate();
                if (result.ToString().Contains("Infinity") || result.ToString().Contains("NaN"))
                    result = 0;
                TestFormula testFormuladialog = new TestFormula("m_ResultedValue",Convert.ToDouble(result));
                testFormuladialog.ShowDialog();
            }
        }

        private int _importTool;

        public int ImportTool
        {
            get { return _importTool; }
            set { _importTool = value; }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (btnSave.Content == "Done")
            {
                _importTool = 1;

            }
            else
            {
                PayorToolVM vm = VMInstances.PayorToolVM;
                if (vm != null && vm.CurrentPayorFieldProperty != null)
                {
                    if (vm.CurrentPayorFieldProperty.CalculationFormula == null)
                    {
                        vm.CurrentPayorFieldProperty.CalculationFormula = new Formula();
                        vm.CurrentPayorFieldProperty.CalculationFormula.FormulaID = Guid.NewGuid();
                    }

                    vm.CurrentPayorFieldProperty.FormulaId = vm.CurrentPayorFieldProperty.CalculationFormula.FormulaID;
                    vm.CurrentPayorFieldProperty.CalculationFormula.FormulaTtitle = txtTitle.Text.Trim();
                    vm.CurrentPayorFieldProperty.CalculationFormula.FormulaExpression = expTextBlock.Text.Trim();
                }
            }
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void itemCombobox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender == itemCombobox)
            {
                ExpressionToken selectedToken = itemCombobox.SelectedItem as ExpressionToken;
                if (selectedToken != null)
                {
                    expressionStack.PushToken(selectedToken);
                    formulaBinding.Expression = expressionStack.Expression;
                }
            }
        }

        private void formulaCombobox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender == formulaCombobox)
            {
                ExpressionToken selectedToken = formulaCombobox.SelectedItem as ExpressionToken;
                expressionStack.PushToken(selectedToken);
                formulaBinding.Expression = expressionStack.Expression;
            }
        }
    }
}
