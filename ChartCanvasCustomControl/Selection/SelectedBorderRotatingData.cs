using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Selection
{
    public class SelectedBorderRotatingData : IEquatable<SelectedBorderRotatingData>
    {
        public IVisualMoveRotate Visual;
        public double OriginalAngle;

        #region equatable
        public override bool Equals(object obj)
        {
            return Equals(obj as SelectedBorderResizingData);
        }
        public bool Equals(SelectedBorderRotatingData other)
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
