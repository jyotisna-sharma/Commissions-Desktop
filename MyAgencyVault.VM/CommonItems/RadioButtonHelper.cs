using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MyAgencyVault.ViewModel.CommonItems
{
    public class BooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null || value == null)
                return false;
            
            if(value is Enum)
                value = ((Enum)value).ToString("d");

            return parameter.Equals(value.ToString().Trim());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType.BaseType.Name == "Enum")
            {
                int? s = value as int?;
                if (s.HasValue)
                    return Enum.ToObject(targetType, s.Value);
            }

            string parameterString = parameter as string;
            
            //if (parameterString == null || value == null)
            //    return null;

            return parameterString;
        }
        #endregion
    } 
}
