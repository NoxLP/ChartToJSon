using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.OtherVisuals;
using ChartToJson.MEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartToJson.Model
{
    public class ChartSaveProxy
    {
        public HashSet<string> DataTemplatesPlugins;
        public HashSet<string> VMPlugins;
        public List<IChartEntityViewModel> EntitiesVM;
        public List<IVisualTextViewModel> TextVMs;
        public List<IVisualTextWithShapeViewModel> TextWShapeVMs;
        public List<LineConnectionSaveProxy> Connections;
        public List<double> CanvasSize;
        public List<double> Zoom;
        public List<double> Origin;
        public bool GridOn;
        public bool SnapToGrid;
        public bool SnapToEntities;
        public bool SnapToAnchors;
        public Brush GridBrush;
        public Brush CanvasBrush;
        public Brush WorkspaceBrush;
    }
}
