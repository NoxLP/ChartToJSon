using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ChartCanvasNamespace.Converters
{
    public class EntityBorderContentToShapeSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var parameters = parameter as Tuple<bool, ChartEntityMoveRotate>;
            var border = parameters.Item2;
            var margin = border.ContentMargin;
            if (parameters.Item1)//width
                return (double)value + margin.Left + margin.Right;
            else //height
                return (double)value + margin.Top + margin.Bottom;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
