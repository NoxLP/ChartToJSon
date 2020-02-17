using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ChartCanvasNamespace.Entities
{
    public class EntityRotatingThumb : Thumb, IChartThumb
    {
        public EntityRotatingThumb()
        {
            ToolTip = Properties.ToolTips.Default.ToolTips_EntityRotatingThumb;
            Style = (Style)Application.Current.FindResource("EntityBorderRotatingThumbStyle");
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_EntityMovingThumb);
        }

        public Point GetAnchorPoint()
        {
            Size size = RenderSize;
            Point ofs = new Point(size.Width / 2, size.Height / 2); //isInput ? 0 : size.Height);
            return this.TranslatePoint(ofs, ChartCustomControl.Instance.ChartCanvas); //TransformToVisual(MyCanvas).Transform(ofs);
        }
    }
}
