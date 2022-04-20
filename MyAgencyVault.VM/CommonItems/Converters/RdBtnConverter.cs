using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Media;

namespace MyAgencyVault.ViewModel.Converters
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class StringToDateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value != "")
            {
                string dt = DateTime.Parse(value.ToString()).ToShortDateString();
                string str = string.Format("{0:MM/dd/yyyy}", dt);
                return str;
            }
            else
            {

                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value != "")
            {
                string dt = DateTime.Parse(value.ToString()).ToShortDateString();
                string str = string.Format("{0:MM/dd/yyyy}", dt);
                return str;
            }
            else
                return null;
        }
        #endregion
    }
    public class RdBtnConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string Strparameter = parameter as string;
            if (Strparameter == null || value == null)

                return false;

            return Strparameter.Equals(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null || value == null)
                return null;
            return parameterString;
        }

        #endregion
    }

    public class RdBtnCounterConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tmpbool = (bool)value;
            if (tmpbool && (int.Parse(parameter.ToString()) == 1))
                return true;
            else if (!tmpbool && (int.Parse(parameter.ToString()) == 2))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tmpbool = (bool)value;
            if (tmpbool && (int.Parse(parameter.ToString()) == 1))
                return true;
            else if (!tmpbool && (int.Parse(parameter.ToString()) == 2))
                return true;
            else
                return false;
        }

        #endregion
    }
    public class EqualityConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string Strparameter = parameter as string;
            if (Strparameter == null || value == null)

                return false;

            return Strparameter.Equals(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //string parameterString = parameter as string;
            //if (parameterString == null || value == null)
            //    return null;

            //return parameterString.Equals(value.ToString());

            return System.Convert.ToBoolean(parameter);
        }

        #endregion
    }

    public class YesNoToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? s = value as bool?;
            if (s.HasValue)
                if (s.Value)
                    return "Yes";
                else
                    return "No";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value as string;
            if (s == "Yes")
                return true;
            else
                return false;
        }
    }

    public class ZipToLongConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long? s = value as long?;
            try
            {
                if (s.HasValue)
                {
                    string sZip = s.ToString();
                    sZip = sZip.Substring(0, 5) + "-" + sZip.Substring(5, 5);

                    return sZip;
                }
                else
                    return "";
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value as string;
            long lZip = 0;
            if (s != null)
            {
                string str = s.Substring(0, 5) + s.Substring(6, 5);
                lZip = long.Parse(str);
            }
            return lZip;
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? s = value as bool?;
            if (s.Value)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? s = value as Visibility?;
            if (s.Value == Visibility.Visible)
                return false;
            else
                return true;
        }
    }

    public class Int2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            if (value is Enum)
                value = ((Enum)value).ToString("d");
            else
                value = value.ToString();

            int intVal = 0;

            if (int.TryParse(value.ToString(), out intVal))
            {
                if ("StatementStatus" == (string)parameter)
                {
                    int? s = intVal as int?;

                    if (s == null)
                        return string.Empty;

                    if (s.Value == 0)
                        return "Not Started";
                    else if (s.Value == 1)
                        return "In Progress";
                    else
                        return "Close";
                }
                else if ("BatchStatus" == (string)parameter)
                {
                    int? s = intVal as int?;

                    if (s == null)
                        return string.Empty;

                    if (s.Value == 1)
                        return "Unassigned";
                    else if (s.Value == 2)
                        return "Import Pending";
                    else if (s.Value == 3)
                        return "Import Unsuccessfull";
                    else if (s.Value == 4)
                        return "Pending Data Entry";
                    else if (s.Value == 5)
                        return "In Data Entry";
                    else if (s.Value == 6)
                        return "Complete/Unpaid";
                    else if (s.Value == 7)
                        return "Partial Unpaid";
                    else if (s.Value == 8)                        
                        return "Paid";
                }
                else if ("UploadBatchStatus" == (string)parameter)
                {
                    int? s = intVal as int?;

                    if (s == null)
                        return string.Empty;

                    if (s.Value == 1)
                        return "Available";
                    else if (s.Value == 2)
                        return "In Progress";
                    else if (s.Value == 3)
                        return "Completed";
                    else if (s.Value == 4)
                        return "Manual";
                    else if (s.Value == 5)
                        return "Automatic";
                }
                else if ("PayorRegion" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 0)
                        return "NorthEast";
                    else if (s.Value == 1)
                        return "SouthEast";
                    else if (s.Value == 2)
                        return "MidWest";
                    else if (s.Value == 3)
                        return "SouthWest";
                    else if (s.Value == 4)
                        return "West";
                    else if (s.Value == 5)
                        return "National";
                    else if (s.Value == 6)
                        return "Central";
                    else
                        return "";
                }
                else if ("PayorType" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 0)
                        return "Single Carrier";
                    else if (s.Value == 1)
                        return "General Agent";
                    else
                        return "";
                }
                else if ("PayorStatus" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 0)
                        return "Active";
                    else if (s.Value == 1)
                        return "InActive";
                    else
                        return "";
                }
                else if ("LicenseeStatus" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 0)
                        return "Active";
                    else if (s.Value == 1)
                        return "InActive";
                    else if (s.Value == 2)
                        return "Pending";
                    else
                        return "";
                }
                else if ("LicenseePaymentMode" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 1)
                        return "Cheque";
                    else if (s.Value == 2)
                        return "Visa";
                    else if (s.Value == 3)
                        return "Master Card";
                    else if (s.Value == 4)
                        return "AmEx";
                    else
                        return "";
                }
                else if ("DataType" == (string)parameter)
                {
                    int? s = intVal as int?;
                    if (s.Value == 1)
                        return "DateTime";
                    else if (s.Value == 2)
                        return "Number";
                    else if (s.Value == 3)
                        return "Text";
                    else
                        return "";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int retVal = 0;
            if ("LicenseeStatus" == (string)parameter)
            {
                string s = value as string;
                if (s == "Active")
                    retVal = 0;
                else if (s == "In Active")
                    retVal = 1;
                else if (s == "Pending")
                    retVal = 2;
                else
                    retVal = 0;
            }
            else if ("LicenseePaymentMode" == (string)parameter)
            {
                string s = value as string;
                if (s == "Cheque")
                    retVal = 1;
                else if (s == "Visa")
                    retVal = 2;
                else if (s == "Master Card")
                    retVal = 3;
                else if (s == "AmEx")
                    retVal = 4;
                else
                    retVal = 0;
            }

            if (targetType.BaseType.Name == "Enum")
            {
                int? s = retVal as int?;
                if (s.HasValue)
                    return Enum.ToObject(targetType, s.Value);
            }

            return retVal;
        }
    }

    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int? s = value as int?;
            if (s.HasValue)
                return Enum.ToObject(targetType, s.Value);
            else
                return null;
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class IntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int perValue = 0;
            if (value != null)
            {
                perValue = System.Convert.ToInt32(value);
            }
            return perValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value.ToString();
            strValue = strValue.Trim();

            int perValue = 0;
            bool isDoubleValue = int.TryParse(strValue, out perValue);

            return perValue;
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public class DollerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double perValue = 0.0;
            if (value != null)
            {
                perValue = System.Convert.ToDouble(value);
            }
            return perValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value.ToString();
            strValue = strValue.Replace("$", "");
            strValue = strValue.Trim();

            double perValue = 0.0;
            bool isDoubleValue = double.TryParse(strValue, out perValue);

            return perValue;
        }
    }
    [ValueConversion(typeof(double), typeof(string))]
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double? perValue = value as double?;

            if (perValue.HasValue)
                return (perValue.Value / 100);
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value.ToString();
            strValue = strValue.Replace("%", "");
            strValue = strValue.Trim();

            double perValue = 0.0;
            bool isDoubleValue = double.TryParse(strValue, out perValue);

            return perValue;
        }
    }

    public class RTFToTextConverter
    {
        public string Convert(string textValue)
        {
            System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
            try
            {
                rtBox.Rtf = textValue;
            }
            catch
            {
                rtBox.Text = textValue;
            }

            return rtBox.Text;
        }
    }


    public class EmptyToNullCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value != null ? value.ToString() : null;
            if (string.IsNullOrEmpty(strValue))
                return null;
            else
            {
                int outParam = 0;
                if (!int.TryParse(strValue, out outParam))
                    return null;
                return outParam;
            }
        }
    }

    public class NullToBooleanCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }

    public class RateCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value != null ? value.ToString() : null;
            if (string.IsNullOrEmpty(strValue))
                return null;
            else
            {
                double outParam = 0;
                if (!double.TryParse(strValue, out outParam))
                    return null;
                return outParam;
            }
        }

    }

    public class CellBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return null;

            var value1 = values[0];
            var value2 = values[1];

            string strStatusName = string.Empty;

            if (value2 != null && value2 != DependencyProperty.UnsetValue)
            {
                strStatusName = ((MyAgencyVault.VM.MyAgencyVaultSvc.IssueStatus)value2).StatusName;
            }

            if (strStatusName != null)
            {
                // get the cell value by accessing the Row via Column header,
                // which is identical to the column name in the data table.
                switch (strStatusName.ToString())
                {
                    case "Closed":
                        return Brushes.Silver;

                    case "Open":
                        return Brushes.White;

                    default:
                        return Brushes.White;
                }
            }

            return SystemColors.WindowBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("This method isn't supposed to be called.");
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            return System.Convert.ToBoolean(value) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF042271"));
          
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibleValue = false; if (value != null && value is Visibility)
            {
                visibleValue = !((Visibility)value == Visibility.Collapsed);
            }
            return visibleValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibleValue = Visibility.Collapsed;
            if (value is bool)
            {
                if ((bool)value == true) { visibleValue = Visibility.Visible; }
            }
            return visibleValue;
        }
    }

}
