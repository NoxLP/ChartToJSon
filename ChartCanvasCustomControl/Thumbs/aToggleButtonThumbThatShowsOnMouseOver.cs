using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ChartCanvasNamespace.Thumbs
{
    public abstract class aToggleButtonThumbThatShowsOnMouseOver : ToggleButton, IChartThumbThatShowsOnMouseOver
    {
        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && Visibility != Visibility.Visible)
                Visibility = Visibility.Visible;
            else if(!(bool)e.NewValue && Visibility != Visibility.Hidden)
                Visibility = Visibility.Hidden;
        }
    }
}
