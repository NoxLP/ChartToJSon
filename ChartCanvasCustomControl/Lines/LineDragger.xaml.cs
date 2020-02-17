using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFHelpers;

namespace ChartCanvasNamespace.Lines
{
    /// <summary>
    /// Lógica de interacción para LineDragger.xaml
    /// </summary>
    public partial class LineDragger : UserControl, IChartThumb
    {
        public LineDragger(LineBetweenConnectingThumbs line)
        {
            InitializeComponent();
            _MyLine = line;
            Width = Size;
            Height = Size;
            Panel.SetZIndex(this, 999);
        }

        internal LineBetweenConnectingThumbs _MyLine;
        private bool isDragging;
        private Point clickPosition;

        public static int Size { get { return 8; } }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                var dragger = thumb.GetParentsOfType<LineDragger>().FirstOrDefault();
                if (dragger == null)
                    throw new Exceptions.LineDraggerUserControlNotFoundException();

                var canvas = ChartCustomControl.Instance;
                Point p = new Point(Canvas.GetLeft(dragger) + e.HorizontalChange, Canvas.GetTop(dragger) + e.VerticalChange); //e.GetPosition(canvas);

                var snap = canvas.SnapToObjectsHandler.CheckPointShouldSnap(p);

                if (snap != null)
                {
                    if (snap.X.HasValue)
                        p.X = snap.X.Value;
                    if (snap.Y.HasValue)
                        p.Y = snap.Y.Value;
                }

                Canvas.SetLeft(dragger, p.X - dragger.ActualWidth / 2);
                Canvas.SetTop(dragger, p.Y - dragger.ActualHeight / 2);

                _MyLine.Destination = p;
                _MyLine.NextLine.Source = p;
                _MyLine.MoveLine();
                _MyLine.NextLine.MoveLine();
                e.Handled = true;
            }
        }
    }
}

#region old
//private void LineDraggerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
//{
//    isDragging = true;
//    var draggableControl = sender as Thumb;
//    clickPosition = e.GetPosition(this);
//    draggableControl.CaptureMouse();
//    e.Handled = true;
//}
//private void LineDraggerMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
//{
//    isDragging = false;
//    var draggable = sender as Thumb;
//    draggable.ReleaseMouseCapture();
//    e.Handled = true;
//}
//private void LineDraggerMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
//{
//    var thumb = sender as Thumb;

//    if (thumb != null && isDragging)
//    {
//        var dragger = thumb.GetParentsOfType<LineDragger>().FirstOrDefault();
//        if (dragger == null)
//            throw new Exceptions.LineDraggerUserControlNotFoundException();

//        var canvas = ChartCustomControl.Instance;
//        Point p = e.GetPosition(canvas);

//        var snap = canvas.SnapToObjectsHandler.CheckPointShouldSnap(p);

//        if (snap != null)
//        {
//            if (snap.X.HasValue)
//                p.X = snap.X.Value;
//            if (snap.Y.HasValue)
//                p.Y = snap.Y.Value;
//        }

//        Canvas.SetLeft(dragger, p.X - dragger.ActualWidth / 2);
//        Canvas.SetTop(dragger, p.Y - dragger.ActualHeight / 2);

//        _MyLine.Destination = p;
//        _MyLine.NextLine.Source = p;
//        _MyLine.MoveLine();
//        _MyLine.NextLine.MoveLine();
//        e.Handled = true;
//    }
//}
#endregion
