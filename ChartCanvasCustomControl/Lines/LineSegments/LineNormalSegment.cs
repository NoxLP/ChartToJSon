using ChartCanvasNamespace.Thumbs;
using JsonManagerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChartCanvasNamespace.Lines.LineSegments
{
    public class LineNormalSegment : aLineSegmentBase, ILineSegmentWithDragger, ILineSegmentWithNextSegmentDraggerReference, IObjectWithSerializationProxy<LineNormalSegmentProxy>
    {
        public LineNormalSegment(LineConnection connection, Point start, Point end, LineDragger nextSegmentDragger, int index)
            : base(LineSegmentTypesEnum.Normal, connection, start, end)
        {
            _Index = index;
            MyLineDragger = new LineDragger(connection, this, Start.X, Start.Y);
            var b = new Binding()
            {
                Source = MyLineDragger,
                Path = new PropertyPath(LineDragger.AnchorPointProperty),
                Converter = _PointHalfParameterConverter,
                ConverterParameter = LineDragger.Size
            };
            SetBinding(StartProperty, b);
            //var b = new Binding()
            //{
            //    Source = this,
            //    Path = new PropertyPath(StartProperty),
            //    Converter = _PointHalfParameterConverter,
            //    ConverterParameter = LineDragger.Size,
            //    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            //};
            //BindingOperations.SetBinding(MyLineDragger, LineDragger.AnchorPointProperty, b);
            ((ILineSegmentWithNextSegmentDraggerReference)this).NextSegmentDragger = nextSegmentDragger;
        }

        private LineDragger _NextSegmentDragger;

        LineDragger ILineSegmentWithNextSegmentDraggerReference.NextSegmentDragger
        {
            get { return _NextSegmentDragger; }
            set
            {
                if (!LineConnection._LoadingFile && value == null)
                    throw new ArgumentNullException();
                if (_NextSegmentDragger == null || !_NextSegmentDragger.Equals(value))
                {
                    _NextSegmentDragger = value;
                    var b = new Binding()
                    {
                        Source = _NextSegmentDragger,
                        Path = new PropertyPath(LineDragger.AnchorPointProperty)
                    };
                    SetBinding(EndProperty, b);
                }
            }
        }
        public LineDragger MyLineDragger { get; private set; }

        public override IEnumerable<IChartThumb> GetAllThumbs()
        {
            yield return MyLineDragger;
            yield return _LineDivider;
            yield break;
        }
        protected override void UpdateThumbsBingdings()
        {
            base.UpdateThumbsBingdings();
            BindingOperations.GetBindingExpression(this, StartProperty).UpdateSource();
        }

        public LineNormalSegmentProxy GetSerializationProxy()
        {
            return new LineNormalSegmentProxy() { Start = Start, End = End, Index = _Index };
        }
    }

    public class LineNormalSegmentProxy : aLineSegmentProxy
    {
        public LineNormalSegmentProxy()
        {
            Type = LineSegmentTypesEnum.Normal;
        }

        public int Index;
    }
}
