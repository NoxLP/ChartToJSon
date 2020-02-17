using ChartCanvasNamespace;
using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.VisualsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFHelpers.Commands;

namespace ChartToJson.ViewModel
{
    public partial class MainVM
    {
        #region fields
        private bool _CanvasSizeChanged;
        private double _CanvasWidth = Properties.Settings.Default.DefaultCanvasWidth;
        private double _CanvasHeight = Properties.Settings.Default.DefaultCanvasHeight;
        private bool _Maintain12Proportion = true;
        private Brush _DefaultGridBrush = SystemColors.ActiveBorderBrush;
        private Brush _DefaultCanvasBackground = Application.Current.FindResource("CanvasBackground") as SolidColorBrush;
        private Brush _DefaultWorkspaceBackground = Brushes.White;
        private Brush _SelectedGridBrush;
        private Brush _SelectedCanvasBackground;
        private Brush _SelectedWorkspaceBackground;
        private Brush _EntitiesBackgroundBrush;
        private Brush _EntitiesShapeBrush;

        private bool _UserSelectedMeasureScriptChanged;
        private double _UserSelectedEntitiesAngle;
        private bool _MaintainEntitiesSizeProportion;
        private double _EntitiesWHSizeProportion;
        private bool _EntitesSizeChangedByScript;
        private double _UserSelectedEntitiesWidth;
        private double _UserSelectedEntitiesHeight;
        #endregion

        #region properties
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set
            {
                if (value != _CanvasWidth)
                {
                    if (!Maintain12Proportion || _CanvasSizeChanged)
                    {
                        _CanvasWidth = value;
                        OnPropertyChanged(nameof(CanvasWidth));
                    }
                    else
                        CanvasSizeChanged(false, value);
                }
            }
        }
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set
            {
                if (value != _CanvasHeight)
                {
                    if (!Maintain12Proportion || _CanvasSizeChanged)
                    {
                        _CanvasHeight = value;
                        OnPropertyChanged(nameof(CanvasHeight));
                    }
                    else
                        CanvasSizeChanged(true, value);
                }
            }
        }
        public bool Maintain12Proportion
        {
            get { return _Maintain12Proportion; }
            set
            {
                if (value != _Maintain12Proportion)
                {
                    _Maintain12Proportion = value;
                    OnPropertyChanged(nameof(Maintain12Proportion));
                }
            }
        }
        public Brush DefaultGridBrush
        {
            get { return _DefaultGridBrush; }
            set
            {
                if (value == null)
                {
                    if (_DefaultGridBrush != null)
                    {
                        _DefaultGridBrush = null;
                        OnPropertyChanged(nameof(DefaultGridBrush));
                    }
                }
                else if (!value.Equals(_DefaultGridBrush))
                {
                    _DefaultGridBrush = value;
                    OnPropertyChanged(nameof(DefaultGridBrush));
                }
            }
        }
        public Brush DefaultCanvasBackground
        {
            get { return _DefaultCanvasBackground; }
            set
            {
                if (value == null)
                {
                    if (_DefaultCanvasBackground != null)
                    {
                        _DefaultCanvasBackground = null;
                        OnPropertyChanged(nameof(DefaultCanvasBackground));
                    }
                }
                else if (!value.Equals(_DefaultCanvasBackground))
                {
                    _DefaultCanvasBackground = value;
                    OnPropertyChanged(nameof(DefaultCanvasBackground));
                }
            }
        }
        public Brush DefaultWorkspaceBackground
        {
            get { return _DefaultWorkspaceBackground; }
            set
            {
                if (value == null)
                {
                    if (_DefaultWorkspaceBackground != null)
                    {
                        _DefaultWorkspaceBackground = null;
                        OnPropertyChanged(nameof(DefaultWorkspaceBackground));
                    }
                }
                else if (!value.Equals(_DefaultWorkspaceBackground))
                {
                    _DefaultWorkspaceBackground = value;
                    OnPropertyChanged(nameof(DefaultWorkspaceBackground));
                }
            }
        }
        public Brush SelectedGridBrush
        {
            get { return _SelectedGridBrush; }
            set
            {
                if (value == null)
                {
                    if (_SelectedGridBrush != null)
                    {
                        _SelectedGridBrush = null;
                        OnPropertyChanged(nameof(SelectedGridBrush));
                    }
                }
                else if (!value.Equals(_SelectedGridBrush))
                {
                    _SelectedGridBrush = value;
                    OnPropertyChanged(nameof(SelectedGridBrush));
                }
            }
        }
        public Brush SelectedCanvasBackground
        {
            get { return _SelectedCanvasBackground; }
            set
            {
                if (value == null)
                {
                    if (_SelectedCanvasBackground != null)
                    {
                        _SelectedCanvasBackground = null;
                        OnPropertyChanged(nameof(SelectedCanvasBackground));
                    }
                }
                else if (!value.Equals(_SelectedCanvasBackground))
                {
                    _SelectedCanvasBackground = value;
                    OnPropertyChanged(nameof(SelectedCanvasBackground));
                }
            }
        }
        public Brush SelectedWorkspaceBackground
        {
            get { return _SelectedWorkspaceBackground; }
            set
            {
                if (value == null)
                {
                    if (_SelectedWorkspaceBackground != null)
                    {
                        _SelectedWorkspaceBackground = null;
                        OnPropertyChanged(nameof(SelectedWorkspaceBackground));
                    }
                }
                else if (!value.Equals(_SelectedWorkspaceBackground))
                {
                    _SelectedWorkspaceBackground = value;
                    OnPropertyChanged(nameof(SelectedWorkspaceBackground));
                }
            }
        }
        public Brush EntitiesBackgroundBrush
        {
            get { return _EntitiesBackgroundBrush; }
            set
            {
                if (value == null)
                {
                    if (_EntitiesBackgroundBrush != null)
                    {
                        _EntitiesBackgroundBrush = null;
                        OnPropertyChanged(nameof(EntitiesBackgroundBrush));
                    }
                }
                else if (!value.Equals(_EntitiesBackgroundBrush))
                {
                    _EntitiesBackgroundBrush = value;
                    OnPropertyChanged(nameof(EntitiesBackgroundBrush));
                    foreach (var item in ChartCanvasNamespace.ChartCustomControl.Instance.VisualsWithBackgroundSelected)
                    {
                        item.BackgroundBrush = value;
                    }
                }
            }
        }
        public Brush EntitiesShapeBrush
        {
            get { return _EntitiesShapeBrush; }
            set
            {
                if (value == null)
                {
                    if (_EntitiesShapeBrush != null)
                    {
                        _EntitiesShapeBrush = null;
                        OnPropertyChanged(nameof(EntitiesShapeBrush));
                    }
                }
                else if (!value.Equals(_EntitiesShapeBrush))
                {
                    _EntitiesShapeBrush = value;
                    OnPropertyChanged(nameof(EntitiesShapeBrush));
                    foreach (var item in ChartCanvasNamespace.ChartCustomControl.Instance.VisualsWithShapeSelected)
                    {
                        item.ShapeStrokeBrush = value;
                    }
                }
            }
        }
        public double UserSelectedEntitiesAngle
        {
            get { return _UserSelectedEntitiesAngle; }
            set
            {
                if (value != _UserSelectedEntitiesAngle)
                {
                    _UserSelectedEntitiesAngle = value;
                    OnPropertyChanged(nameof(UserSelectedEntitiesAngle));
                    if (!_UserSelectedMeasureScriptChanged)
                        RotateEntitiesTo(value);
                }
            }
        }
        public bool MaintainEntitiesSizeProportion
        {
            get { return _MaintainEntitiesSizeProportion; }
            set
            {
                if (value != _MaintainEntitiesSizeProportion)
                {
                    _MaintainEntitiesSizeProportion = value;
                    OnPropertyChanged(nameof(MaintainEntitiesSizeProportion));
                }
            }
        }
        public double UserSelectedEntitiesWidth
        {
            get { return _UserSelectedEntitiesWidth; }
            set
            {
                if (value != _UserSelectedEntitiesWidth)
                {
                    if (!_UserSelectedMeasureScriptChanged)
                    {
                        if (!_MaintainEntitiesSizeProportion)
                        {
                            _EntitiesWHSizeProportion = value / _UserSelectedEntitiesHeight;
                            _UserSelectedEntitiesWidth = value;
                        }
                        else if (_MaintainEntitiesSizeProportion && _EntitesSizeChangedByScript)
                        {
                            _EntitiesWHSizeProportion = value / _UserSelectedEntitiesHeight;
                            _UserSelectedEntitiesWidth = value;
                            _EntitesSizeChangedByScript = false;
                            ResizeEntities();
                        }
                        else
                        {
                            _UserSelectedEntitiesWidth = value;
                            _EntitesSizeChangedByScript = true;
                            UserSelectedEntitiesHeight = _EntitiesWHSizeProportion * value;
                        }
                    }
                    else
                        _UserSelectedEntitiesWidth = value;
                    OnPropertyChanged(nameof(UserSelectedEntitiesWidth));
                }
            }
        }
        public double UserSelectedEntitiesHeight
        {
            get { return _UserSelectedEntitiesHeight; }
            set
            {
                if (value != _UserSelectedEntitiesHeight)
                {
                    if (!_UserSelectedMeasureScriptChanged)
                    {
                        if (!_MaintainEntitiesSizeProportion)
                        {
                            _EntitiesWHSizeProportion = _UserSelectedEntitiesWidth / value;
                            _UserSelectedEntitiesHeight = value;
                        }
                        else if (_MaintainEntitiesSizeProportion && _EntitesSizeChangedByScript)
                        {
                            _EntitiesWHSizeProportion = _UserSelectedEntitiesWidth / value;
                            _UserSelectedEntitiesHeight = value;
                            _EntitesSizeChangedByScript = false;
                            ResizeEntities();
                        }
                        else
                        {
                            _UserSelectedEntitiesHeight = value;
                            _EntitesSizeChangedByScript = true;
                            UserSelectedEntitiesWidth = value / _EntitiesWHSizeProportion;
                        }
                    }
                    else
                        _UserSelectedEntitiesHeight = value;
                    OnPropertyChanged(nameof(UserSelectedEntitiesHeight));
                }
            }
        }
        #endregion

        #region commands
        public ICommand RotateEntity_90_Command => new DelegateCommand(x => RotateEntities(-90d), null);
        public ICommand RotateEntity90_Command => new DelegateCommand(x => RotateEntities(90d), null);

        private void RotateEntities(object angle)
        {
            if (angle is double)
            {
                double a = (double)angle;
                if (EntitiesSelected.Count != 0)
                {
                    var item = (EntityBorderUserControl)EntitiesSelected[0].UserControl;
                    UserSelectedEntitiesAngle = EntitiesSelected[0].Angle + a;
                }
                else if (TextsSelected.Count != 0)
                {
                    var item = (ChartEntityMoveRotate)TextsSelected[0].UserControl;
                    UserSelectedEntitiesAngle = TextsSelected[0].Angle + a;
                }
            }
        }
        #endregion

        private void RotateEntitiesTo(object angle)
        {
            if (angle is double)
            {
                double a = (double)angle;
                if (EntitiesSelected.Count != 0)
                    ((EntityBorderUserControl)EntitiesSelected[0].UserControl).RotateTo(a);
                else if (TextsSelected.Count != 0)
                    ((ChartEntityMoveRotate)TextsSelected[0].UserControl).RotateTo(a);
            }
        }
        private void ResizeEntities()
        {
            ChartCustomControl.Instance.ResizeSelectedEntities(UserSelectedEntitiesWidth, UserSelectedEntitiesHeight);
        }
        private void SetBrushesToDefault()
        {
            SelectedGridBrush = DefaultGridBrush;
            SelectedCanvasBackground = DefaultCanvasBackground;
            SelectedWorkspaceBackground = DefaultWorkspaceBackground;
        }
    }
}
