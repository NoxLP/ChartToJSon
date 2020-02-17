using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChartCanvasNamespace.VisualsBase
{
    public interface IVisualMoveRotate : IEquatable<IVisualMoveRotate>
    {
        double CenterX { get; }
        double CenterY { get; }
        Point UndoMovingCoordinates { get; }
        double UndoAngle { get; }
        UIElement GetUIElement { get; }

        int GetHashCode();
        void DraggingFinished();
        void BindShapeSize();

        void OtherVisualFinishMoving();
        void OtherVisualFinishRotating();

        void OtherVisualStartMoving();
        void OtherVisualStartRotating();

        void OtherVisualMove(double x, double y);
        void OtherVisualRotate(double angle);
        void OtherVisualFastRotate(double angle);

        void AutomaticMoveToWithoutUndoRedo(Point p);
        void AutomaticRotateToWithoutUndoRedo(double angle);
    }
}
