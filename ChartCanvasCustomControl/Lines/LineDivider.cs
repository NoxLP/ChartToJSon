using ChartCanvasNamespace.Lines.LineSegments;
using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChartCanvasNamespace.Lines
{
    public partial class LineDivider : aButtonThumbThatShowsOnMouseOver
    {
        public LineDivider(LineConnection line, aLineSegmentBase segment)
        {
            _Segment = segment;
            _Connection = line;
            Width = Size;
            Height = Size;
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_LineDivider);
            Click += OnClick;
            ToolTip = Properties.ToolTips.Default.ToolTips_LineDivider;
        }

        private LineConnection _Connection;
        internal aLineSegmentBase _Segment;
        public static int Size { get { return 17; } }

        protected void OnClick(object sender, RoutedEventArgs e)
        {
            _Connection.DivideLine(_Segment);
            e.Handled = true;
        }
    }
}
