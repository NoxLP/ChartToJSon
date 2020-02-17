using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartCanvasNamespace.OtherVisuals
{
    public interface IVisualTextViewModel : IVisualWithBackground, IViewModelOfVisualWithConnectingThumbs
    {
        double Width { get; set; }
        double Height { get; set; }
        double Angle { get; set; }
        string Text { get; set; }
        bool IsSelected { get; set; }
        IVisualText UserControl { get; set; }
        ChartEntityTypeEnum Type { get; }

        FontFamily ChoosedFontFamily { get; set; }
        double ChoosedFontSize { get; set; }
        FontStyle ChoosedFontStyle { get; set; }
        FontStretch ChoosedFontStretch { get; set; }
        FontWeight ChoosedFontWeight { get; set; }
        Brush ChoosedFontBrush { get; set; }
        TextDecorationCollection ChoosedTextDecoration { get; set; }
        TextAlignment ChoosedHorizontalAlignment { get; set; }
        VerticalAlignment ChoosedVerticalAlignment { get; set; }

        void TextLoaded(IVisualText userControl);
    }
}
