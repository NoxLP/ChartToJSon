using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartCanvasNamespace.Entities
{
    [Serializable]
    public abstract class aChartEntityViewModelBase : INotifyPropertyChanged, IChartEntityViewModel, IEquatable<aChartEntityViewModelBase>
    {
        #region Constructors
        public aChartEntityViewModelBase(/*IChartEntity entity, */string templateKey)
        {
            //Entity = entity;
            TemplateKey = templateKey;
            ViewModelId = Guid.NewGuid().ToString();
            Children = new ObservableCollection<IChartEntityViewModel>();
            Parents = new ObservableCollection<IChartEntityViewModel>();
        }
        #endregion

        #region Fields
        private double _CanvasX;
        private double _CanvasY;
        private double _Width;
        private double _Height;
        private double _Angle;
        private ObservableCollection<IChartEntityViewModel> _Children;
        private ObservableCollection<IChartEntityViewModel> _Parents;
        private IChartEntity _Entity;
        private string _ShapeTemplateKey;
        private Brush _ShapeStrokeBrush;
        private Brush _BackgroundBrush;
        private IChartEntityBorderUserControl _UserControl;
        private bool _IsSelected;
        #endregion

        #region Properties
        public double CanvasX
        {
            get { return _CanvasX; }
            set
            {
                if (value != _CanvasX)
                {
                    _CanvasX = value;
                    OnPropertyChanged(nameof(CanvasX));
                }
            }
        }
        public double CanvasY
        {
            get { return _CanvasY; }
            set
            {
                if (value != _CanvasY)
                {
                    _CanvasY = value;
                    OnPropertyChanged(nameof(CanvasY));
                }
            }
        }
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        public double Angle
        {
            get { return _Angle; }
            set
            {
                if (value != _Angle)
                {
                    _Angle = value;
                    OnPropertyChanged(nameof(Angle));
                }
            }
        }
        [JsonProperty]
        public ObservableCollection<IChartEntityViewModel> Children
        {
            get { return _Children; }
            protected set
            {
                if (value == null)
                {
                    if (_Children != null)
                    {
                        _Children = null;
                        OnPropertyChanged(nameof(Children));
                    }
                }
                else if (!value.Equals(_Children))
                {
                    _Children = value;
                    OnPropertyChanged(nameof(Children));
                }
            }
        }
        [JsonProperty]
        public ObservableCollection<IChartEntityViewModel> Parents
        {
            get { return _Parents; }
            protected set
            {
                if (value == null)
                {
                    if (_Parents != null)
                    {
                        _Parents = null;
                        OnPropertyChanged(nameof(Parents));
                    }
                }
                else if (!value.Equals(_Parents))
                {
                    _Parents = value;
                    OnPropertyChanged(nameof(Parents));
                }
            }
        }
        [JsonProperty]
        public virtual IChartEntity Entity
        {
            get { return _Entity; }
            protected set
            {
                if (value == null)
                {
                    if (_Entity != null)
                    {
                        _Entity = null;
                        OnPropertyChanged(nameof(Entity));
                    }
                }
                else if (!value.Equals(_Entity))
                {
                    _Entity = value;
                    OnPropertyChanged(nameof(Entity));
                }
            }
        }
        [JsonProperty]
        public virtual string ThumbnailSource { get { return null; } }
        [JsonProperty]
        public string ViewModelId { get; set; }
        [JsonProperty]
        public string TemplateKey { get; protected set; }
        [JsonProperty]
        public string ShapeTemplateKey
        {
            get { return _ShapeTemplateKey; }
            set
            {
                if (value == null)
                {
                    if (_ShapeTemplateKey != null)
                    {
                        _ShapeTemplateKey = null;
                        OnPropertyChanged(nameof(ShapeTemplateKey));
                    }
                }
                else if (!value.Equals(_ShapeTemplateKey))
                {
                    _ShapeTemplateKey = value;
                    OnPropertyChanged(nameof(ShapeTemplateKey));
                }
            }
        }
        [JsonProperty]
        public Brush ShapeStrokeBrush
        {
            get { return _ShapeStrokeBrush; }
            set
            {
                if (value == null)
                {
                    if (_ShapeStrokeBrush != null)
                    {
                        _ShapeStrokeBrush = null;
                        OnPropertyChanged(nameof(ShapeStrokeBrush));
                    }
                }
                else if (!value.Equals(_ShapeStrokeBrush))
                {
                    _ShapeStrokeBrush = value;
                    OnPropertyChanged(nameof(ShapeStrokeBrush));
                }
            }
        }
        [JsonProperty]
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set
            {
                if (value == null)
                {
                    if (_BackgroundBrush != null)
                    {
                        _BackgroundBrush = null;
                        OnPropertyChanged(nameof(BackgroundBrush));
                    }
                }
                else if (!value.Equals(_BackgroundBrush))
                {
                    _BackgroundBrush = value;
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }
        [JsonIgnore]
        public IChartEntityBorderUserControl UserControl
        {
            get { return _UserControl; }
            set
            {
                if (value != _UserControl)
                {
                    _UserControl = value;
                    OnPropertyChanged(nameof(UserControl));
                }
            }
        }
        [JsonIgnore]
        public virtual bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        #endregion

        #region Methods
        public virtual void EntityBorderLoaded(IChartEntityBorderUserControl userControl)
        {
            UserControl = userControl;
        }
        public virtual bool NewChildAddedFromChart(IChartEntityViewModel entityVM)
        {
            if (!Entity.NewChildAddedFromChart(entityVM.Entity))
                return false;

            Children.Add(entityVM);

            return true;
        }
        public virtual IEnumerable<IChartEntityViewModel> RemoveMeAsAChild()
        {
            foreach (var parent in Parents)
            {
                RemoveChild(this);
            }
            return Parents as IEnumerable<IChartEntityViewModel>;
        }
        public virtual void RemoveChild(IChartEntityViewModel entityVM)
        {
            var entity = entityVM as aChartEntityViewModelBase;
            Children.Remove(entity);
            Entity.RemoveChild(entity.Entity);
        }
        #endregion

        #region iequatable
        public override bool Equals(object obj)
        {
            var other = obj as aChartEntityViewModelBase;
            return other != null && this.Equals(other);
        }
        public bool Equals(IChartEntityViewModel other)
        {
            var obj = other as aChartEntityViewModelBase;
            return obj != null && this.Equals(obj);
        }
        public bool Equals(aChartEntityViewModelBase other)
        {
            return other != null &&
                   ViewModelId == other.ViewModelId;
        }
        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(ViewModelId);
        }
        #endregion

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
