using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChartCanvasNamespace.VisualsBase
{
    public interface IVisualResizeMoveRotate : IVisualMoveRotate
    {
        FrameworkElement ResizingControl { get; }
        Size UndoSize { get; }

        void OtherVisualFinishResizing();
        void OtherVisualStartResizing();
        void OtherVisualResize(double width, double height);
        void OtherVisualFastResize(double width, double height);
        void AutomaticResizeToWithoutUndoRedo(Size size);
    }
}
