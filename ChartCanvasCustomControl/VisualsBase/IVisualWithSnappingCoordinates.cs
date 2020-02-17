using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChartCanvasNamespace.VisualsBase
{
    public interface IVisualWithSnappingCoordinates : IEquatable<IVisualWithSnappingCoordinates>
    {
        double CenterX { get; }
        double CenterY { get; }
        Point Left { get; }
        Point Right { get; }
        Point Top { get; }
        Point Bottom { get; }

        int GetHashCode();
    }
}
