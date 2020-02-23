using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChartCanvasNamespace.Lines
{
    //Based on http://www.charlespetzold.com/blog/2007/04/191200.html
    public abstract class aChartArrowLineBase : aShapeCanBeSelected
    {
        /// <summary>
        ///     Initializes a new instance of ArrowLineBase.
        /// </summary>
        public aChartArrowLineBase()
        {
            _Pathgeo = new PathGeometry();

            _PathfigLine = new PathFigure();
            _PolysegLine = new PolyLineSegment();
            _PathfigLine.Segments.Add(_PolysegLine);

            _PathfigHead1 = new PathFigure();
            _PolysegHead1 = new PolyLineSegment();
            _PathfigHead1.Segments.Add(_PolysegHead1);

            _PathfigHead2 = new PathFigure();
            _PolysegHead2 = new PolyLineSegment();
            _PathfigHead2.Segments.Add(_PolysegHead2);
        }

        #region fields
        private PathFigure _PathfigHead1;
        private PolyLineSegment _PolysegHead1;
        private PathFigure _PathfigHead2;
        private PolyLineSegment _PolysegHead2;
        protected PathGeometry _Pathgeo;
        protected PathFigure _PathfigLine;
        protected PolyLineSegment _PolysegLine;
        #endregion

        #region dependency properties
        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }
        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(aChartArrowLineBase),
                new FrameworkPropertyMetadata(45.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));
        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }
        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(aChartArrowLineBase),
                new FrameworkPropertyMetadata(6.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));
        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }
        }
        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowEnds",
                typeof(ArrowEnds), typeof(aChartArrowLineBase),
                new FrameworkPropertyMetadata(ArrowEnds.End,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));
        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }
        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty =
            DependencyProperty.Register("IsArrowClosed",
                typeof(bool), typeof(aChartArrowLineBase),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                int count = _PolysegLine.Points.Count;

                if (count > 0)
                {
                    // Draw the arrow at the start of the line.
                    if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                    {
                        Point pt1 = _PathfigLine.StartPoint;
                        Point pt2 = _PolysegLine.Points[0];
                        _Pathgeo.Figures.Add(CalculateArrow(_PathfigHead1, pt2, pt1));
                    }

                    // Draw the arrow at the end of the line.
                    if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                    {
                        Point pt1 = count == 1 ? _PathfigLine.StartPoint :
                                                 _PolysegLine.Points[count - 2];
                        Point pt2 = _PolysegLine.Points[count - 1];
                        _Pathgeo.Figures.Add(CalculateArrow(_PathfigHead2, pt1, pt2));
                    }
                }
                return _Pathgeo;
            }
        }

        PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();
            matx.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = pt2 + vect * matx;
            polyseg.Points.Add(pt2);

            matx.Rotate(-ArrowAngle);
            polyseg.Points.Add(pt2 + vect * matx);
            pathfig.IsClosed = IsArrowClosed;

            return pathfig;
        }
    }
}
