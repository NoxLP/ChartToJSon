using System.Text;
using System.Threading.Tasks;
using WPFHelpers.ViewModelBase;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json;
using System;

namespace ChartToJson.ViewModel
{
    [Serializable]
    public class TextVM : aViewModelBase, IVisualTextViewModel
    {
        public TextVM()
        {
            ViewModelId = Guid.NewGuid().ToString();
            var mainVM = (IChartMainVM)ChartCustomControl.Instance.DataContext;
            _ChoosedFontFamily = mainVM.ChoosedFontFamily;
            _ChoosedFontSize = mainVM.ChoosedFontSize;
            _ChoosedFontStyle = mainVM.ChoosedFontStyle;
            _ChoosedFontStretch = mainVM.ChoosedFontStretch;
            _ChoosedFontWeight = mainVM.ChoosedFontWeight;
            _ChoosedFontBrush = mainVM.ChoosedFontBrush;
            _ChoosedTextDecoration = mainVM.ChoosedTextDecoration;
            _ChoosedHorizontalAlignment = mainVM.ChoosedHorizontalAlignment;
            _ChoosedVerticalAlignment = mainVM.ChoosedVerticalAlignment;
        }

        private double _CanvasX;
        private double _CanvasY;
        private double _Width;
        private double _Height;
        private double _Angle;
        private string _Text;
        private bool _IsSelected;
        private string _ShapeTemplateKey;
        private Brush _ShapeStrokeBrush;
        private Brush _BackgroundBrush;
        private IVisualText _UserControl;
        private ChartEntityTypeEnum _Type;

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
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value == null)
                {
                    if (_Text != null)
                    {
                        _Text = null;
                        OnPropertyChanged(nameof(Text));
                    }
                }
                else if (!value.Equals(_Text))
                {
                    _Text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        [JsonIgnore]
        public bool IsSelected
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
        public IVisualText UserControl
        {
            get { return _UserControl; }
            set
            {
                if (value == null)
                {
                    if (_UserControl != null)
                    {
                        _UserControl = null;
                        OnPropertyChanged(nameof(UserControl));
                    }
                }
                else if (!value.Equals(_UserControl))
                {
                    _UserControl = value;
                    OnPropertyChanged(nameof(UserControl));
                }
            }
        }
        public ChartEntityTypeEnum Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }
        [JsonProperty]
        public string ViewModelId { get; set; }

        #region font and alignment
        private FontFamily _ChoosedFontFamily;
        private double _ChoosedFontSize;
        private FontStyle _ChoosedFontStyle;
        private FontStretch _ChoosedFontStretch;
        private FontWeight _ChoosedFontWeight;
        private Brush _ChoosedFontBrush;
        private TextDecorationCollection _ChoosedTextDecoration;
        private TextAlignment _ChoosedHorizontalAlignment;
        private VerticalAlignment _ChoosedVerticalAlignment;

        public FontFamily ChoosedFontFamily
        {
            get { return _ChoosedFontFamily; }
            set
            {
                if (value == null)
                {
                    if (_ChoosedFontFamily != null)
                    {
                        _ChoosedFontFamily = null;
                        OnPropertyChanged(nameof(ChoosedFontFamily));
                    }
                }
                else if (!value.Equals(_ChoosedFontFamily))
                {
                    _ChoosedFontFamily = value;
                    OnPropertyChanged(nameof(ChoosedFontFamily));
                }
            }
        }
        public double ChoosedFontSize
        {
            get { return _ChoosedFontSize; }
            set
            {
                if (value != _ChoosedFontSize)
                {
                    _ChoosedFontSize = value;
                    OnPropertyChanged(nameof(ChoosedFontSize));
                }
            }
        }
        public FontStyle ChoosedFontStyle
        {
            get { return _ChoosedFontStyle; }
            set
            {
                if (value != _ChoosedFontStyle)
                {
                    _ChoosedFontStyle = value;
                    OnPropertyChanged(nameof(ChoosedFontStyle));
                }
            }
        }
        public FontStretch ChoosedFontStretch
        {
            get { return _ChoosedFontStretch; }
            set
            {
                if (value != _ChoosedFontStretch)
                {
                    _ChoosedFontStretch = value;
                    OnPropertyChanged(nameof(ChoosedFontStretch));
                }
            }
        }
        public FontWeight ChoosedFontWeight
        {
            get { return _ChoosedFontWeight; }
            set
            {
                if (value != _ChoosedFontWeight)
                {
                    _ChoosedFontWeight = value;
                    OnPropertyChanged(nameof(ChoosedFontWeight));
                }
            }
        }
        public Brush ChoosedFontBrush
        {
            get { return _ChoosedFontBrush; }
            set
            {
                if (value == null)
                {
                    if (_ChoosedFontBrush != null)
                    {
                        _ChoosedFontBrush = null;
                        OnPropertyChanged(nameof(ChoosedFontBrush));
                    }
                }
                else if (!value.Equals(_ChoosedFontBrush))
                {
                    _ChoosedFontBrush = value;
                    OnPropertyChanged(nameof(ChoosedFontBrush));
                }
            }
        }
        public TextDecorationCollection ChoosedTextDecoration
        {
            get { return _ChoosedTextDecoration; }
            set
            {
                if (value == null)
                {
                    if (_ChoosedTextDecoration != null)
                    {
                        _ChoosedTextDecoration = null;
                        OnPropertyChanged(nameof(ChoosedTextDecoration));
                    }
                }
                else if (!value.Equals(_ChoosedTextDecoration))
                {
                    _ChoosedTextDecoration = value;
                    OnPropertyChanged(nameof(ChoosedTextDecoration));
                }
            }
        }
        public TextAlignment ChoosedHorizontalAlignment
        {
            get { return _ChoosedHorizontalAlignment; }
            set
            {
                if (value != _ChoosedHorizontalAlignment)
                {
                    _ChoosedHorizontalAlignment = value;
                    OnPropertyChanged(nameof(ChoosedHorizontalAlignment));
                }
            }
        }
        public VerticalAlignment ChoosedVerticalAlignment
        {
            get { return _ChoosedVerticalAlignment; }
            set
            {
                if (value != _ChoosedVerticalAlignment)
                {
                    _ChoosedVerticalAlignment = value;
                    OnPropertyChanged(nameof(ChoosedVerticalAlignment));
                }
            }
        }
        #endregion

        public void TextLoaded(IVisualText userControl)
        {
            UserControl = userControl;
        }
    }
}
