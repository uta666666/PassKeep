using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PassKeep.Material.View.Converter {
    public class BoolVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is bool)) {
                return Visibility.Collapsed;
            }
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is Visibility)) {
                return false;
            }
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
