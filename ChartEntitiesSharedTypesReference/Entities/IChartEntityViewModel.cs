using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace ChartCanvasNamespace.Entities
{
    public interface IChartEntityViewModel : IVisualWithBackground, IVisualWithShapeViewModel, IViewModelOfVisualWithConnectingThumbs, INotifyPropertyChanged, IEquatable<IChartEntityViewModel>
    {
        double Width { get; set; }
        double Height { get; set; }
        double Angle { get; set; }
        ObservableCollection<IChartEntityViewModel> Children { get; }
        IChartEntity Entity { get; }
        string TemplateKey { get; }
        IChartEntityBorderUserControl UserControl { get; set; }
        ObservableCollection<IChartEntityViewModel> Parents { get; }
        bool IsSelected { get; set; }

        void EntityBorderLoaded(IChartEntityBorderUserControl userControl);
        /// <summary>
        /// This method must add the VM to the viewmodel children collection AND add the model to the Entity model children collection
        /// </summary>
        /// <param name="entityVM"></param>
        /// <returns></returns>
        bool NewChildAddedFromChart(IChartEntityViewModel entityVM);
        /// <summary>
        /// This method must remove this Entity from all parents children collection
        /// </summary>
        /// <returns></returns>
        IEnumerable<IChartEntityViewModel> RemoveMeAsAChild();
        void RemoveChild(IChartEntityViewModel entityVM);
    }
}
