using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChartCanvasNamespace.Selection
{
    public class SelectedBorderMovingData : IEquatable<SelectedBorderMovingData>
    {
        public IVisualMoveRotate Visual;
        public Point MovingThumbPositionWhenClickedRelativeToCanvas;
        public Point ItemCoordsWhenClickedRelativeToCanvas;
        public Point Distance;

        public void SetDistanceBetweenOriginalAndThis(IVisualMoveRotate originalMoved)
        {
            //Distance = new Point(Border.AnchorPoint.X - originalMoved.AnchorPoint.X, Border.AnchorPoint.Y - originalMoved.AnchorPoint.Y);
            Distance = new Point(Visual.CenterX - originalMoved.CenterX, Visual.CenterY - originalMoved.CenterY);
        }

        #region equatable
        public override bool Equals(object obj)
        {
            return Equals(obj as SelectedBorderMovingData);
        }
        public bool Equals(SelectedBorderMovingData other)
        {
            return other != null &&
                   EqualityComparer<IVisualMoveRotate>.Default.Equals(Visual, other.Visual);
        }
        public override int GetHashCode()
        {
            return -979861770 + EqualityComparer<IVisualMoveRotate>.Default.GetHashCode(Visual);
        }
        #endregion
    }
}
