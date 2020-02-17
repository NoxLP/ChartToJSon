using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChartCanvasNamespace.Selection
{
    public class SelectedBorderResizingData : IEquatable<SelectedBorderResizingData>
    {
        public IVisualResizeMoveRotate Visual;
        public Point MovingThumbPositionWhenClickedRelativeToCanvas;
        public Size OriginalItemSize;
        public Point Distance;

        public void SetDistanceBetweenOriginalAndThis(IVisualResizeMoveRotate originalMoved)
        {
            //Distance = new Point(Border.AnchorPoint.X - originalMoved.AnchorPoint.X, Border.AnchorPoint.Y - originalMoved.AnchorPoint.Y);
            Distance = new Point(Visual.CenterX - originalMoved.CenterX, Visual.CenterY - originalMoved.CenterY);
        }

        #region equatable
        public override bool Equals(object obj)
        {
            return Equals(obj as SelectedBorderResizingData);
        }
        public bool Equals(SelectedBorderResizingData other)
        {
            return other != null &&
                   EqualityComparer<IVisualResizeMoveRotate>.Default.Equals(Visual, other.Visual);
        }
        public override int GetHashCode()
        {
            return -979861770 + EqualityComparer<IVisualResizeMoveRotate>.Default.GetHashCode(Visual);
        }
        #endregion
    }
}
