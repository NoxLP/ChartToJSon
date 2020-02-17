using ChartCanvasNamespace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChartCanvasNamespace.VisualsBase
{
    public interface IVisualWithConnectingThumbs
    {
        EntityConnectingThumb _ThLeft { get; }
        EntityConnectingThumb _ThRight { get; }
        EntityConnectingThumb _ThTop { get; }
        EntityConnectingThumb _ThBottom { get; }

        double BorderContentWidth { get; }
        double BorderContentHeight { get; }
        Grid BaseRootGrid { get; }

        UIElement GetUIElement { get; }
    }
}
