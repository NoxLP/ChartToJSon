using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Entities
{
    [Serializable]
    public abstract class aChartEntityModelBase : INotifyPropertyChanged, IChartEntity
    {
        [JsonProperty]
        public abstract string EntityId { get; protected set; }

        public abstract bool NewChildAddedFromChart(IChartEntity entity);
        public abstract bool RemoveChild(IChartEntity entity);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
