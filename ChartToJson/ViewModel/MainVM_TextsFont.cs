using ChartCanvasNamespace.OtherVisuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartToJson.ViewModel
{
    public partial class MainVM
    {
        private FontFamily _ChoosedFontFamily = new FontFamily("Arial");
        private double _ChoosedFontSize = 10d;
        private FontStyle _ChoosedFontStyle = FontStyles.Normal;
        private FontStretch _ChoosedFontStretch = FontStretches.Normal;
        private FontWeight _ChoosedFontWeight = FontWeights.Normal;
        private Brush _ChoosedFontBrush = Brushes.Black;
        private TextDecorationCollection _ChoosedTextDecoration = null;
        private TextAlignment _ChoosedHorizontalAlignment = TextAlignment.Left;
        private VerticalAlignment _ChoosedVerticalAlignment = VerticalAlignment.Center;

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
                    DoInAllSelectedTexts(x => x.ChoosedFontFamily = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedFontSize = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedFontStyle = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedFontStretch = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedFontWeight = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedFontBrush = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedTextDecoration = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedHorizontalAlignment = value);
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
                    DoInAllSelectedTexts(x => x.ChoosedVerticalAlignment = value);
                }
            }
        }

        private void DoInAllSelectedTexts(Action<IVisualTextViewModel> action)
        {
            Parallel.ForEach(ChartCanvasNamespace.ChartCustomControl.Instance.VisualTextsSelected, action);
        }
    }
}
