using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace
{
    public interface IChartObjectCanBeRemovedByParent
    {
        bool AlreadyRemovedByParent { get; set; }

        void AddToLastCommandMyUndoRedoCommands();
        void RemovedByParent();
    }
}
