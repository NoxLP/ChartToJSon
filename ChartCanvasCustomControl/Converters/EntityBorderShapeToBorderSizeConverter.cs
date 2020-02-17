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
    public class EntityBorderShapeToBorderSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var parameters = parameter as Tuple<bool, ChartEntityMoveRotate>;
            var border = parameters.Item2;
            var db = (double)value;
            if (parameters.Item1)//width
                return db + border.BaseRootGrid.ColumnDefinitions[0].ActualWidth + border.BaseRootGrid.ColumnDefinitions[2].ActualWidth;
            else //height
                return db + border.BaseRootGrid.RowDefinitions[0].ActualHeight + border.BaseRootGrid.RowDefinitions[2].ActualHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
