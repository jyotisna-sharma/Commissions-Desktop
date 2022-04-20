using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media;
using MyAgencyVault.ViewModel.CommonItems;
using Microsoft.Windows.Controls;

namespace MyAgencyVault.WinApp.Validation
{
    public static class ControlValidation
    {
        public static bool OnValidation(Control control)
        {
            ValidationResult validationResult = null;

            ValidatorData validatorData = new ValidatorData();
            ValidationRule failedRule = null;

            validatorData.GroupName = control.GetValue(FieldDescriptor.GroupNameProperty) as string;
            validatorData.FieldName = control.GetValue(FieldDescriptor.NameProperty) as string;

            GroupFieldData groupFieldData = AccessGropFieldData.GetGroupFieldData(validatorData.GroupName);

            if (groupFieldData == null)
                return true;

            if (!groupFieldData.IsRequired)
                groupFieldData.IsRequired = (bool)control.GetValue(FieldDescriptor.IsRequiredProperty);

            groupFieldData.IsCompare = (bool)control.GetValue(FieldDescriptor.IsCompareProperty);

            if (groupFieldData != null)
            {
                validatorData.Pattern = groupFieldData.Pattern;
                validatorData.MinLength = groupFieldData.MinLength;
                validatorData.MaxLength = groupFieldData.MaxLength;
            }

            int? MinValue = control.GetValue(FieldDescriptor.MinLengthProperty) as int?;
            int? MaxValue = control.GetValue(FieldDescriptor.MaxLengthProperty) as int?;

            if (MinValue.HasValue)
                validatorData.MinLength = MinValue.Value;

            if (MaxValue.HasValue)
                validatorData.MaxLength = MaxValue.Value;

            if (!string.IsNullOrEmpty(validatorData.GroupName))
            {
                if (groupFieldData.IsRequired)
                {
                    RequiredFieldValidator requiredFieldValidator = new RequiredFieldValidator(validatorData);
                    failedRule = requiredFieldValidator;
                    validationResult = requiredFieldValidator.Validate(getValue(control), null);
                }

                if (validationResult == null)
                {
                    if (groupFieldData.IsCompare)
                    {
                        CompareFieldValidator compareFieldValidator = new CompareFieldValidator(validatorData);
                        compareFieldValidator.OtherValue = control.GetValue(FieldDescriptor.CompareToProperty) as string;
                        failedRule = compareFieldValidator;
                        validationResult = compareFieldValidator.Validate(getValue(control), null);
                    }
                }

                if (validationResult == null)
                {
                    if (groupFieldData.IsLength)
                    {
                        LengthValidator lengthFieldValidator = new LengthValidator(validatorData);
                        failedRule = lengthFieldValidator;
                        validationResult = lengthFieldValidator.Validate(getValue(control), null);
                    }
                }

                if (validationResult == null)
                {
                    RegexValidator regexValidator = new RegexValidator(validatorData);
                    failedRule = regexValidator;
                    validationResult = regexValidator.Validate(getValue(control), null);
                }

                if (validationResult != null)
                {
                    string message = GetErrorMessage(failedRule, validatorData);
                    ValidationMarkInvalid(control, failedRule, message);
                }

                if (validationResult == null)
                {
                    ValidationClear(control);
                }

                return validationResult == null ? true : false;
            }

            return true;
        }

        public static string getValue(Control control)
        {
            string value = null;
            if (control is TextBox)
            {
                return (control as TextBox).Text;
            }
            else if (control is PasswordBox)
            {
                return (control as PasswordBox).Password;
            }
            else if (control is MaskedTextBox)
            {
                if ((control as MaskedTextBox).Value != null)
                    return (control as MaskedTextBox).Value.ToString();
                else
                    return string.Empty;
            }

            return value;
        }

        private static string GetErrorMessage(ValidationRule failedRule, ValidatorData validatorData)
        {
            string Message = "Validation Failed.";
            if (failedRule is RequiredFieldValidator)
            {
                Message = validatorData.FieldName + " is required field.";
            }
            else if (failedRule is CompareFieldValidator)
            {
                Message = validatorData.FieldName + " is not matched.";
            }
            else if (failedRule is LengthValidator)
            {
                Message = "Length should be in between " + validatorData.MinLength.Value.ToString() + " to " + validatorData.MaxLength.Value.ToString() + ".";
            }
            else if (failedRule is RegexValidator)
            {
                Message = "Provide valid value for " + validatorData.FieldName + ".";
            }
            return Message;
        }

        public static void ValidationMarkInvalid(Control contol, ValidationRule rule, string error)
        {
            DependencyProperty depProp = null;

            if (contol is TextBox)
                depProp = TextBox.TextProperty;
            else if (contol is PasswordBox)
                depProp = PasswordHelper.BoundPassword;
            else
                depProp = MaskedTextBox.ValueProperty;

            BindingExpression bindingInError =
                contol.GetBindingExpression(depProp);

            var validationError = new ValidationError(
                rule,
                bindingInError,
                error,
                null);

            System.Windows.Controls.Validation.MarkInvalid(bindingInError, validationError);
            contol.ToolTip = error;
        }

        public static void ValidationClear(Control control)
        {
            DependencyProperty depProp = null;

            if (control is TextBox)
                depProp = TextBox.TextProperty;
            else if (control is PasswordBox)
                depProp = PasswordHelper.BoundPassword;
            else
                depProp = MaskedTextBox.ValueProperty;

            control.ToolTip = null;
            System.Windows.Controls.Validation.ClearInvalid(control.GetBindingExpression(depProp));
        }

        public static bool ForceValidation(DependencyObject parent)
        {
            bool isValid = true;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox || child is PasswordBox)
                    isValid = isValid & OnValidation(child as Control);

                isValid = isValid & ForceValidation(child);
            }
            return isValid;
        }
    }

    class RequiredFieldValidator : ValidationRule
    {
        public RequiredFieldValidator(ValidatorData validatorData)
        {
            ValidationData = validatorData;
        }

        public ValidatorData ValidationData { get; set; }
        public override ValidationResult Validate(Object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value.ToString().Trim()))
                return new ValidationResult(false, "This field is required.");
            else
                return null;
        }
    }

    class CompareFieldValidator : ValidationRule
    {
        public CompareFieldValidator(ValidatorData validatorData)
        {
            ValidationData = validatorData;
        }

        public ValidatorData ValidationData { get; set; }
        public string OtherValue { get; set; }
        public override ValidationResult Validate(Object value, CultureInfo cultureInfo)
        {
            if (string.Compare(value.ToString(), OtherValue) != 0)
                return new ValidationResult(false, "Should be equal");
            else
                return null;
        }
    }

    class LengthValidator : ValidationRule
    {
        public LengthValidator(ValidatorData validatorData)
        {
            ValidationData = validatorData;
        }

        public ValidatorData ValidationData { get; set; }
        public override ValidationResult Validate(Object value, CultureInfo cultureInfo)
        {
            if(!string.IsNullOrEmpty(value.ToString().TrimStart()))
            {
                if (ValidationData != null && ValidationData.MinLength.HasValue && ValidationData.MaxLength.HasValue)
                {
                    if (value.ToString().Length < ValidationData.MinLength.Value || value.ToString().Length > ValidationData.MaxLength.Value)
                        return new ValidationResult(false, "Length should be in between " + ValidationData.MinLength.Value.ToString() + " to " + ValidationData.MaxLength.Value.ToString() + ".");
                    else
                        return null;
                }
            }
            return null;
        }
    }

    class RegexValidator : ValidationRule
    {
        public RegexValidator(ValidatorData validatorData)
        {
            ValidationData = validatorData;
        }

        public ValidatorData ValidationData { get; set; }
        public override ValidationResult Validate(Object value, CultureInfo cultureInfo)
        {
            if (ValidationData != null)
            {
                if (!string.IsNullOrEmpty(value.ToString()) && !string.IsNullOrEmpty(ValidationData.Pattern) && !new Regex(ValidationData.Pattern).IsMatch(value.ToString()))
                {
                    return new ValidationResult(false, "Not Matched");
                }
                else
                    return null;
            }
            else
                return null;
        }
    }

    class ValidatorData
    {
        public string FieldName { get; set; }
        public string GroupName { get; set; }
        public string Pattern { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
    }

    public static class FieldDescriptor
    {
        public static DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name",
                    typeof(string),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetName(TextBox target, string value)
        {
            target.SetValue(FieldDescriptor.NameProperty, value);
        }

        public static string GetName(TextBox target)
        {
            return (string)target.GetValue(FieldDescriptor.NameProperty);
        }

        public static DependencyProperty MinLengthProperty = DependencyProperty.RegisterAttached("MinLength",
                    typeof(int?),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetMinLength(TextBox target, int? value)
        {
            target.SetValue(FieldDescriptor.MinLengthProperty, value);
        }

        public static int? GetMinLength(TextBox target)
        {
            return (int?)target.GetValue(FieldDescriptor.MinLengthProperty);
        }

        public static DependencyProperty MaxLengthProperty = DependencyProperty.RegisterAttached("MaxLength",
                    typeof(int?),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetMaxLength(TextBox target, int? value)
        {
            target.SetValue(FieldDescriptor.MaxLengthProperty, value);
        }

        public static int? GetMaxLength(TextBox target)
        {
            return (int?)target.GetValue(FieldDescriptor.MaxLengthProperty);
        }

        public static DependencyProperty GroupNameProperty = DependencyProperty.RegisterAttached("GroupName",
                    typeof(string),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetGroupName(TextBox target, string value)
        {
            target.SetValue(FieldDescriptor.GroupNameProperty, value);
        }

        public static string GetGroupName(TextBox target)
        {
            return (string)target.GetValue(FieldDescriptor.GroupNameProperty);
        }

        public static DependencyProperty IsRequiredProperty = DependencyProperty.RegisterAttached("IsRequired",
                    typeof(bool),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsRequired(TextBox target, bool value)
        {
            target.SetValue(FieldDescriptor.IsRequiredProperty, value);
        }

        public static bool GetIsRequired(TextBox target)
        {
            return (bool)target.GetValue(FieldDescriptor.IsRequiredProperty);
        }

        public static DependencyProperty IsCompareProperty = DependencyProperty.RegisterAttached("IsCompare",
                    typeof(bool),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(false));

        public static void SetIsCompare(TextBox target, bool value)
        {
            target.SetValue(FieldDescriptor.IsCompareProperty, value);
        }

        public static bool GetIsCompare(TextBox target)
        {
            return (bool)target.GetValue(FieldDescriptor.IsCompareProperty);
        }

        public static DependencyProperty CompareToProperty = DependencyProperty.RegisterAttached("CompareTo",
                    typeof(string),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetCompareTo(TextBox target, string value)
        {
            target.SetValue(FieldDescriptor.IsCompareProperty, value);
        }

        public static string GetCompareTo(TextBox target)
        {
            return (string)target.GetValue(FieldDescriptor.IsCompareProperty);
        }

        public static DependencyProperty IsLengthRangeProperty = DependencyProperty.RegisterAttached("IsLengthRange",
                    typeof(bool),
                    typeof(FieldDescriptor),
                    new FrameworkPropertyMetadata(null));

        public static void SetIsLengthRange(TextBox target, bool value)
        {
            target.SetValue(FieldDescriptor.IsCompareProperty, value);
        }

        public static bool GetIsLengthRange(TextBox target)
        {
            return (bool)target.GetValue(FieldDescriptor.IsCompareProperty);
        }
    }

    public class GroupFieldData
    {
        public string Pattern { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        
        public bool IsRequired { get; set; }
        public bool IsCompare { get; set; }
        public bool IsLength { get; set; }
    }

    public static class AccessGropFieldData
    {
        public static Dictionary<string, GroupFieldData> GroupFieldDataDictionary = new Dictionary<string, GroupFieldData>();

        static AccessGropFieldData()
        {
            GroupFieldData data = new GroupFieldData { Pattern = @"^[a-zA-Z ]*$", MinLength = 3, MaxLength = 20, IsRequired = false, IsLength = true };
            GroupFieldDataDictionary.Add("Name", data);

            data = new GroupFieldData { Pattern = @"^[a-zA-Z0-9._^%$#!~@,-]*$", MinLength = 6, MaxLength = 30, IsRequired = true, IsLength = true };
            GroupFieldDataDictionary.Add("Password", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ ^%$#!~@,-]+", MinLength = 3, MaxLength = 200, IsRequired = false, IsLength = true };
            GroupFieldDataDictionary.Add("Address", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ @-]+", MinLength = 6, MaxLength = 30, IsRequired = true, IsLength = true };
            GroupFieldDataDictionary.Add("UserName", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ ^%$#!~@,-]+", MinLength = 3, MaxLength = 30, IsRequired = true, IsLength = true };
            GroupFieldDataDictionary.Add("SecurityQuestionAnswer", data);

            data = new GroupFieldData { Pattern = @"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}", IsRequired = false };
            GroupFieldDataDictionary.Add("CellPhone", data);

            data = new GroupFieldData { Pattern = @"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}", IsRequired = false };
            GroupFieldDataDictionary.Add("OfficePhone", data);

            data = new GroupFieldData { Pattern = @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$", IsRequired = false, IsLength = true };
            GroupFieldDataDictionary.Add("E-mail", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ ^%$#!~@,-]+", IsRequired = false, IsLength = true };
            GroupFieldDataDictionary.Add("Company", data);

            data = new GroupFieldData { Pattern = @"\d{5}", IsRequired = false };
            GroupFieldDataDictionary.Add("ZipCode", data);

            data = new GroupFieldData { Pattern = @"^[a-zA-Z ]*$", IsRequired = false };
            GroupFieldDataDictionary.Add("State", data);

            data = new GroupFieldData { Pattern = @"^[a-zA-Z ]*$", IsRequired = false };
            GroupFieldDataDictionary.Add("City", data);

            data = new GroupFieldData { Pattern = @"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}", IsRequired = false };
            GroupFieldDataDictionary.Add("Fax", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ @-]+", IsRequired = true, IsLength = true, MinLength = 3, MaxLength = 30 };
            GroupFieldDataDictionary.Add("NickName", data);

            data = new GroupFieldData { Pattern = @"[a-zA-Z0-9._ ^%$#!~@,-]+", MinLength = 3, MaxLength = 100, IsRequired = true, IsLength = true };
            GroupFieldDataDictionary.Add("ClientName", data);
        }

        public static GroupFieldData GetGroupFieldData(string GroupFieldName)
        {
            if (string.IsNullOrEmpty(GroupFieldName))
                return null;

            if (GroupFieldDataDictionary.ContainsKey(GroupFieldName))
                return GroupFieldDataDictionary[GroupFieldName];
            else
                return null;
        }
    }
}
