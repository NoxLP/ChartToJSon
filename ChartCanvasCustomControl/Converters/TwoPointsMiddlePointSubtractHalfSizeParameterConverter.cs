using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChartCanvasNamespace.Converters
{
    public class TwoPointsMiddlePointSubtractHalfSizeParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var points = values.Cast<Point>().ToArray();
            var halfSize = parameter != null ? ((int)parameter) * 0.5d : 0d;

            return new Point(
                ((points[0].X + points[1].X) * 0.5d) - halfSize,
                ((points[0].Y + points[1].Y) * 0.5d) - halfSize);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
