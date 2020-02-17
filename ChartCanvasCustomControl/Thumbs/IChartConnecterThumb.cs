using ChartCanvasNamespace.Lines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFHelpers.CancelActions;

namespace ChartCanvasNamespace.Thumbs
{
    public enum ConnecterThumbTypesEnum : int { Entity, Line }

    public interface IChartConnecterThumb : IChartThumb, IObjectWithCancellableAction
    {
        ConnecterThumbTypesEnum ConnecterType { get; }
        bool? IsChecked { get; set; }
        bool ScriptUnchecked { get; set; }

        void SetNewLineCancellableAction();
    }

    public interface IChartEntityConnectingThumb : IChartConnecterThumb
    {
        void CreateNewLineAsLineEndThumb(IChartEntityConnectingThumb startThumb);
    }

    public interface IChartLineConnecterThumb : IChartConnecterThumb, IChartThumbThatShowsOnMouseOver
    {

    }
}
