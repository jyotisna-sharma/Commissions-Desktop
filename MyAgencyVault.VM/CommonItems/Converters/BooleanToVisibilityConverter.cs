using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
namespace MyAgencyVault.ViewModel.Converters
{
    public enum Visibilities
    {
       Hidden,
       Collespe,
       Visible,
    }
//    class BooleanToVisibilityConverter : IValueConverter
//    {
//        //public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//        //{
//        //    Visibilities _visistate = (Visibilities)value;
//        //    Visibilities _visi = (Visibilities)Enum.Parse(typeof(Visibilities), value.ToString());
//        //    switch (_visistate)
//        //    {
//        //        case Visibilities.Collespe:
//        //            if (_visi == Visibilities.Collespe)
//        //            {
//        //                return Visibility.Collapsed;
//        //            }
//        //            break;
//        //    }
//        //}

//        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
}
