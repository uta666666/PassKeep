using PassKeep.Material.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassKeep.Material.View.Converter
{
    public class CategoryIDNameConverter : IMultiValueConverter
    {
        private CategoryList _categories;
        private Account _account;
        private string _inputText;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
            {
                return string.Empty;
            }
            
            if (!(values[0] is Account) || !(values[1] is CategoryList))
            {
                throw new ArgumentException();
            }
            _account = values[0] as Account;
            _categories = values[1] as CategoryList;

            if (_account.CategoryID == 0)
            {
                return string.Empty;
            }
            return _categories.FirstOrDefault(n => n.ID == _account.CategoryID)?.Name ?? _inputText;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            _inputText = value.ToString();
            _account.CategoryID = 0;
            _account.CategoryID = _categories.FirstOrDefault(n => n.Name == value.ToString())?.ID ?? -1;
            return new object[] { _account, _categories };
        }
    }
}
