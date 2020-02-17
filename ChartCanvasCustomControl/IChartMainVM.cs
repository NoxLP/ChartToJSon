using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.OtherVisuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UndoRedoSystem;

namespace ChartCanvasNamespace
{
    public interface IChartMainVM
    {
        bool AddingEmptyEntity { get; }
        bool AddingEmptyLastTypeEntity { get; }
        bool AddingTextEntity { get; }
        bool AddingSingleText { get; }

        bool PasteIsChecked { get; }

        FontFamily ChoosedFontFamily { get; set; }
        double ChoosedFontSize { get; set; }
        FontStyle ChoosedFontStyle { get; set; }
        FontStretch ChoosedFontStretch { get; set; }
        FontWeight ChoosedFontWeight { get; set; }
        Brush ChoosedFontBrush { get; set; }
        TextDecorationCollection ChoosedTextDecoration { get; set; }
        TextAlignment ChoosedHorizontalAlignment { get; set; }
        VerticalAlignment ChoosedVerticalAlignment { get; set; }

        void MouseUpWhileAddingEntity(Point canvasLocation);
        void RemoveSelectedEntities();
        void CheckForPastedConnections(IViewModelOfVisualWithConnectingThumbs visual);
    }
}
