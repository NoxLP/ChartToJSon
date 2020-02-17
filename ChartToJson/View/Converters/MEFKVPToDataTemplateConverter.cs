using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChartToJson.View.Converters
{
    public class MEFKVPToDataTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is KeyValuePair<string, DataTemplate>)
            {
                var kvp = (KeyValuePair<string, DataTemplate>)value;
                return kvp.Value;
            }
            return null; 
        }
    }
}
