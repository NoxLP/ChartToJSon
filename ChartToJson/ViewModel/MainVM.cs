using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFHelpers;
using ChartCanvasNamespace;
using System.Collections.ObjectModel;
using ChartCanvasNamespace.Entities;
using System.Windows;
using UndoRedoSystem;
using ChartToJson.Model;
using WPFHelpers.CancelActions;
using System.Linq.Expressions;
using System.Reflection;
using UndoRedoSystem.UndoRedoCommands;
using UndoRedoSystem.ViewModel;
using ChartToJson.MEF;
using ChartToJson.View.Windows;
using WPFHelpers.Commands;
using WPFHelpers.Helpers;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace.Selection;
using System.Windows.Controls;
using ChartCanvasNamespace.Lines;
using System.Collections.Specialized;

namespace ChartToJson.ViewModel
{
    public partial class MainVM : WPFHelpers.ViewModelBase.aListenerViewModelBase, IChartMainVM
    {
        public MainVM()
        {
            UndoRedoVM = new UndoRedoVM();
            PluginsHandlerVM = new PluginsHandlerVMClass();
            //FillMessagesDictionary();
            EntitiesVM.CollectionChanged += VMCollections_CollectionChanged;
            VisualTextsVM.CollectionChanged += VMCollections_CollectionChanged;
            EntitiesSelected.CollectionChanged += ItemsSelectedCollections_CollectionChanged;
            TextsSelected.CollectionChanged += ItemsSelectedCollections_CollectionChanged;
        }

        #region fields
        private MainWindow _Window;
        private PluginsWindow _PluginsWindow;
        private KeysHelpWindow _KeysHelpWindow;
        private bool _UnsavedChangesInCurrentFile;
        private ObservableCollectionRange<IChartEntityViewModel> _EntitiesVM = new ObservableCollectionRange<IChartEntityViewModel>();
        private ObservableCollectionRange<IVisualTextViewModel> _VisualTextsVM = new ObservableCollectionRange<IVisualTextViewModel>();
        private ObservableCollection<IVisualCanBeSelected> _VisualsSelected = new ObservableCollection<IVisualCanBeSelected>();
        private ObservableCollection<IChartEntityViewModel> _EntitiesSelected = new ObservableCollection<IChartEntityViewModel>();
        private ObservableCollection<IVisualTextViewModel> _TextsSelected = new ObservableCollection<IVisualTextViewModel>();
        private bool _AddingEmptyEntity;
        private bool _AddingTextEntity;
        private bool _AddingSingleText;
        private ChartEntityTypeEnum _LastEntityAdded;
        private bool _AddingEmptyLastTypeEntity;
        private bool _SomeItemAdded;
        private Visibility _ProgressRowVisibility = Visibility.Collapsed;
        private string _ProgressText;
        private int _ProgressValue;
        #endregion

        #region properties
        public string VMCancellableActionsToken => "";
        public UndoRedoVM UndoRedoVM { get; private set; }
        public PluginsHandlerVMClass PluginsHandlerVM { get; private set; }
        public bool UnsavedChangesInCurrentFile
        {
            get { return _UnsavedChangesInCurrentFile; }
            set
            {
                if (value != _UnsavedChangesInCurrentFile)
                {
                    _UnsavedChangesInCurrentFile = value;
                    OnPropertyChanged(nameof(UnsavedChangesInCurrentFile));
                    CanSaveWithoutDialog = !string.IsNullOrEmpty(_LastSavePathThisSession) && value;
                }
            }
        }
        public ObservableCollectionRange<IChartEntityViewModel> EntitiesVM
        {
            get { return _EntitiesVM; }
            set
            {
                if (value == null)
                {
                    if (_EntitiesVM != null)
                    {
                        _EntitiesVM = null;
                        OnPropertyChanged(nameof(EntitiesVM));
                    }
                }
                else if (!value.Equals(_EntitiesVM))
                {
                    _EntitiesVM = value;
                    OnPropertyChanged(nameof(EntitiesVM));
                }
            }
        }
        public ObservableCollectionRange<IVisualTextViewModel> VisualTextsVM
        {
            get { return _VisualTextsVM; }
            set
            {
                if (value == null)
                {
                    if (_VisualTextsVM != null)
                    {
                        _VisualTextsVM = null;
                        OnPropertyChanged(nameof(VisualTextsVM));
                    }
                }
                else if (!value.Equals(_VisualTextsVM))
                {
                    _VisualTextsVM = value;
                    OnPropertyChanged(nameof(VisualTextsVM));
                }
            }
        }
        public ObservableCollection<IVisualCanBeSelected> VisualsSelected
        {
            get { return _VisualsSelected; }
            set
            {
                if (value == null)
                {
                    if (_VisualsSelected != null)
                    {
                        _VisualsSelected = null;
                        OnPropertyChanged(nameof(VisualsSelected));
                    }
                }
                else if (!value.Equals(_VisualsSelected))
                {
                    _VisualsSelected = value;
                    OnPropertyChanged(nameof(VisualsSelected));
                }
            }
        }
        public ObservableCollection<IChartEntityViewModel> EntitiesSelected
        {
            get { return _EntitiesSelected; }
            set
            {
                if (value == null)
                {
                    if (_EntitiesSelected != null)
                    {
                        _EntitiesSelected = null;
                        OnPropertyChanged(nameof(EntitiesSelected));
                    }
                }
                else if (!value.Equals(_EntitiesSelected))
                {
                    _EntitiesSelected = value;
                    OnPropertyChanged(nameof(EntitiesSelected));
                }
            }
        }
        public ObservableCollection<IVisualTextViewModel> TextsSelected
        {
            get { return _TextsSelected; }
            set
            {
                if (value == null)
                {
                    if (_TextsSelected != null)
                    {
                        _TextsSelected = null;
                        OnPropertyChanged(nameof(TextsSelected));
                    }
                }
                else if (!value.Equals(_TextsSelected))
                {
                    _TextsSelected = value;
                    OnPropertyChanged(nameof(TextsSelected));
                }
            }
        }
        public bool AddingEmptyEntity
        {
            get { return _AddingEmptyEntity; }
            set
            {
                if (value != _AddingEmptyEntity)
                {
                    _AddingEmptyEntity = value;
                    if (value)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                    OnPropertyChanged(nameof(AddingEmptyEntity));
                }
            }
        }
        public bool AddingTextEntity
        {
            get { return _AddingTextEntity; }
            set
            {
                if (value != _AddingTextEntity)
                {
                    _AddingTextEntity = value;
                    if (value)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                    OnPropertyChanged(nameof(AddingTextEntity));
                }
            }
        }
        public bool AddingSingleText
        {
            get { return _AddingSingleText; }
            set
            {
                if (value != _AddingSingleText)
                {
                    _AddingSingleText = value;
                    if (value)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                    OnPropertyChanged(nameof(AddingSingleText));
                }
            }
        }
        public bool AddingEmptyLastTypeEntity
        {
            get { return _AddingEmptyLastTypeEntity; }
            set
            {
                if (value != _AddingEmptyLastTypeEntity)
                {
                    _AddingEmptyLastTypeEntity = value;
                    if (value)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                    OnPropertyChanged(nameof(AddingEmptyLastTypeEntity));
                }
            }
        }
        public bool SomeItemAdded
        {
            get { return _SomeItemAdded; }
            set
            {
                if (value != _SomeItemAdded)
                {
                    _SomeItemAdded = value;
                    OnPropertyChanged(nameof(SomeItemAdded));
                }
            }
        }
        public Visibility ProgressRowVisibility
        {
            get { return _ProgressRowVisibility; }
            set
            {
                if (value != _ProgressRowVisibility)
                {
                    _ProgressRowVisibility = value;
                    OnPropertyChanged(nameof(ProgressRowVisibility));
                }
            }
        }
        public string ProgressText
        {
            get { return _ProgressText; }
            set
            {
                if (value == null)
                {
                    if (_ProgressText != null)
                    {
                        _ProgressText = null;
                        OnPropertyChanged(nameof(ProgressText));
                    }
                }
                else if (!value.Equals(_ProgressText))
                {
                    _ProgressText = value;
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }
        public int ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                if (value != _ProgressValue)
                {
                    _ProgressValue = value;
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }
        #endregion

        #region commands
        public ICommand RemoveEntityCommand => new DelegateCommand(x => RemoveSelectedEntities(), null);
        public ICommand ViewLoadedPluginsCommand => new DelegateCommand(x => ViewLoadedPlugins(), null);
        public ICommand ViewHelpKeysWindowCommand => new DelegateCommand(x => ViewHelpKeysWindow(), null);
        public ICommand ZoomFillScreenCommand => new DelegateCommand(x => ZoomFillScreen(), null);

        public void RemoveSelectedEntities()
        {
            Console.WriteLine("RemoveSelectedEntities");
            var toRemoveFromVisuals = new List<LineConnection>();
            var undoConns = new List<LineConnectionSaveProxy>();
            var toRemoveFromVMEntities = new List<IChartEntityViewModel>();
            var toRemoveFromVMTexts = new List<IVisualTextViewModel>();
            foreach (var item in EntitiesSelected)
            {
                var uc = item.UserControl as UserControl;
                if (uc != null)
                {
                    toRemoveFromVMEntities.Add((IChartEntityViewModel)uc.DataContext);
                }
            }
            foreach (var item in TextsSelected)
            {
                var uc = item.UserControl as UserControl;
                if (uc != null)
                {
                    toRemoveFromVMTexts.Add((IVisualTextViewModel)uc.DataContext);
                }
            }

            object[] parameters;
            bool newCommand = UndoRedoCommandManager.Instance.LastCommandIsNull || !UndoRedoCommandManager.Instance.LastCommandName.Equals(Properties.UndoRedoNames.Default.General_Cut);
            if (newCommand)
            {
                parameters = new object[3] { this, toRemoveFromVMEntities, toRemoveFromVMTexts };
                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.Entities_Remove, x => UndoEntitiesRemoved(x), parameters, x => RedoEntitiesRemoved(x), parameters);
            }

            foreach (var item in toRemoveFromVMEntities)
            {
                EntitiesVM.Remove(item);
            }
            foreach (var item in toRemoveFromVMTexts)
            {
                VisualTextsVM.Remove(item);
            }
            foreach (var item in VisualsSelected)
            {
                var line = item as LineConnection;
                if (line == null)
                    continue;
                toRemoveFromVisuals.Add(line);
                undoConns.Add(line.GetSerializationProxy());
            }
            foreach (var item in toRemoveFromVisuals)
            {
                item.RemoveThis();
            }

            if (!newCommand)
            {
                parameters = new object[4] { this, toRemoveFromVMEntities, toRemoveFromVMTexts, undoConns };
                var redoParameters = new object[4] { this, toRemoveFromVMEntities, toRemoveFromVMTexts, toRemoveFromVisuals };
                UndoRedoCommandManager.Instance.AddToLastCommand(x => UndoEntitiesRemoved(x), parameters, x => RedoEntitiesRemoved(x), redoParameters);
            }
            else
            {
                parameters = new object[4] { this, null, null, undoConns };
                var redoParameters = new object[4] { this, null, null, toRemoveFromVisuals };
                UndoRedoCommandManager.Instance.AddToLastCommand(x => UndoEntitiesRemoved(x), parameters, x => RedoEntitiesRemoved(x), redoParameters);
            }
        }
        private void ViewLoadedPlugins()
        {
            (new PluginsWindow()).Show();
        }
        private void ViewHelpKeysWindow()
        {
            (new KeysHelpWindow()).Show();
        }
        private void ZoomFillScreen()
        {
            ChartCustomControl.Instance.ZoomToFillScreen();
        }
        #endregion

        private void VMCollections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (EntitiesVM.Count == 0 && VisualTextsVM.Count == 0)
                UnsavedChangesInCurrentFile = false;
            else
                UnsavedChangesInCurrentFile = true;
        }
        private void ItemsSelectedCollections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (EntitiesSelected.Count > 0)
            {
                _UserSelectedMeasureScriptChanged = true;
                var item0 = EntitiesSelected[0];
                UserSelectedEntitiesAngle = item0.Angle;
                UserSelectedEntitiesWidth = item0.Width;
                UserSelectedEntitiesHeight = item0.Height;
                _UserSelectedMeasureScriptChanged = false;
            }
            else if (TextsSelected.Count > 0)
            {
                _UserSelectedMeasureScriptChanged = true;
                var item0 = TextsSelected[0];
                UserSelectedEntitiesAngle = item0.Angle;
                UserSelectedEntitiesWidth = item0.Width;
                UserSelectedEntitiesHeight = item0.Height;
                _UserSelectedMeasureScriptChanged = false;
            }
        }
        private void CanvasSizeChanged(bool height, double value)
        {
            _CanvasSizeChanged = true;
            if(height)
            {
                var w = value * 2;
                CanvasHeight = value;
                CanvasWidth = w;
            }
            else
            {
                var h = value * 0.5d;
                CanvasHeight = h;
                CanvasWidth = value;
            }
            _CanvasSizeChanged = false;
        }
        public void MouseUpWhileAddingEntity(Point canvasLocation)
        {
            SomeItemAdded = true;
            Mouse.OverrideCursor = null;
            if (PasteIsChecked)
            {
                PasteOn(canvasLocation);
            }
            else if (AddingEmptyEntity || (AddingEmptyLastTypeEntity && _LastEntityAdded == ChartEntityTypeEnum.Entity))
            {
                if (!AddingEmptyLastTypeEntity)
                {
                    var vmSelectionWindow = new SelectEntityOnLoadedPluginsWindow();
                    PluginsHandlerVM._SelectEntityWindow = vmSelectionWindow;
                    vmSelectionWindow.ShowDialog();
                }

                var vm = PluginsHandlerVM.GetVMInstance();
                if (vm == null)
                {
                    AddingEmptyEntity = false;
                    AddingEmptyLastTypeEntity = false;
                    return;
                }

                vm.CanvasX = canvasLocation.X;
                vm.CanvasY = canvasLocation.Y;

                var parameters = new object[1] { vm };
                Action<object[]> undo = x =>
                {
                    var mvm = (IChartEntityViewModel)x[0];
                    EntitiesVM.Remove(mvm);
                };
                Action<object[]> redo = x => EntitiesVM.Add((IChartEntityViewModel)x[0]);

                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.Entities_Add, undo, parameters, redo, parameters);
                EntitiesVM.Add(vm);
                AddingEmptyEntity = false;
                AddingEmptyLastTypeEntity = false;
                _LastEntityAdded = ChartEntityTypeEnum.Entity;
            }
            else if (AddingTextEntity || (AddingEmptyLastTypeEntity && _LastEntityAdded == ChartEntityTypeEnum.TextEntity))
            {
                var vm = new TextEntityVM()
                {
                    CanvasX = canvasLocation.X,
                    CanvasY = canvasLocation.Y,
                    Type = ChartEntityTypeEnum.TextEntity,
                    ShapeTemplateKey = "SHAPE_A1RectRounded"
                };

                var parameters = new object[1] { vm };
                Action<object[]> undo = x =>
                {
                    var mvm = (IVisualTextWithShapeViewModel)x[0];
                    VisualTextsVM.Remove(mvm);
                };
                Action<object[]> redo = x => VisualTextsVM.Add((IVisualTextWithShapeViewModel)x[0]);

                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.Entities_AddTextEntity, undo, parameters, redo, parameters);
                VisualTextsVM.Add(vm);
                AddingTextEntity = false;
                AddingEmptyLastTypeEntity = false;
                _LastEntityAdded = ChartEntityTypeEnum.TextEntity;
            }
            else //if (AddingSingleText || (AddingEmptyLastTypeEntity && _LastEntityAdded == ChartEntityTypeEnum.SingleText))
            {
                var vm = new TextVM()
                {
                    CanvasX = canvasLocation.X,
                    CanvasY = canvasLocation.Y,
                    Type = ChartEntityTypeEnum.SingleText
                };

                var parameters = new object[1] { vm };
                Action<object[]> undo = x =>
                {
                    var mvm = (IVisualTextViewModel)x[0];
                    VisualTextsVM.Remove(mvm);
                };
                Action<object[]> redo = x => VisualTextsVM.Add((IVisualTextViewModel)x[0]);

                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.Entities_AddText, undo, parameters, redo, parameters);
                VisualTextsVM.Add(vm);
                AddingSingleText = false;
                AddingEmptyLastTypeEntity = false;
                _LastEntityAdded = ChartEntityTypeEnum.SingleText;
            }
        }
        public void OnWindowLoaded(MainWindow window)
        {
            _Window = window;

            if (PluginsHandlerVM.RDCount > 0)
            {
                ChartCustomControl.Instance.SetEntitiesContentTemplates(PluginsHandlerVM.ImportedContentTemplates);
                ChartCustomControl.Instance.SetEntitiesShapes(PluginsHandlerVM.ImportedShapes);
            }
        }

        #region messenger
        //private void FillMessagesDictionary()
        //{
        //    _MessagesActions = new Dictionary<string, Action<object>>()
        //    {
        //        { Properties.Messages.Default.SetProgressTo,
        //            x =>
        //            {
        //                if (_ProgressAnimation != null && !_ProgressAnimation.IsCompleted)
        //                    _CTS.Cancel();
        //                SetProgressBar((int)x);
        //            }
        //        },
        //        { Properties.Messages.Default.AnimateProgressTo,
        //            async x =>
        //            {
        //                if (_ProgressAnimation != null && !_ProgressAnimation.IsCompleted)
        //                    _CTS.Cancel();
        //                _ProgressAnimation = AnimateProgressBarTo((int)x, _CTS.Token);
        //            }
        //        }
        //    };
        //}
        //public override object MsgWithResponse(string key, object msg)
        //{
        //    if (key.Equals(Properties.Messages.Default.GetCanvasRender))
        //    {
        //        if(msg is int)
        //            return Task.Run(() => RenderCanvas((int)msg));
        //        return null;
        //    }

        //    return null;
        //}
        protected override void RegisterListener()
        {
            //WPFHelpers.MessengerNSpace.Messenger.RegisterListener(this, Properties.Messages.Default.SetProgressTo);
            //WPFHelpers.MessengerNSpace.Messenger.RegisterListener(this, Properties.Messages.Default.AnimateProgressTo);
            //WPFHelpers.MessengerNSpace.Messenger.RegisterListener(this, Properties.Messages.Default.GetCanvasRender);
        }
        #endregion

        #region undo/redo
        private void UndoEntitiesRemoved(object[] parameters)
        {
            var vm = parameters[0] as MainVM;
            if (parameters[1] != null)
            {
                var removedEntitiesColl = parameters[1] as IEnumerable<IChartEntityViewModel>;
                foreach (var item in removedEntitiesColl)
                {
                    vm.EntitiesVM.Add(item);
                }
            }
            if (parameters[2] != null)
            {
                var removedTextsColl = parameters[2] as IEnumerable<IVisualTextViewModel>;
                foreach (var item in removedTextsColl)
                {
                    vm.VisualTextsVM.Add(item);
                }
            }
            if (parameters.Length > 3 && parameters[3] != null)
            {
                ChartCustomControl.Instance.AddLineConnectionsByProxies(parameters[3] as List<LineConnectionSaveProxy>);
            }
        }
        private void RedoEntitiesRemoved(object[] parameters)
        {
            var vm = parameters[0] as MainVM;
            if (parameters[1] != null)
            {
                var removedColl = parameters[1] as IEnumerable<IChartEntityViewModel>;
                foreach (var item in removedColl)
                {
                    vm.EntitiesVM.Remove(item);
                }
            }
            if (parameters[2] != null)
            {
                var removedTextsColl = parameters[2] as IEnumerable<IVisualTextViewModel>;
                foreach (var item in removedTextsColl)
                {
                    vm.VisualTextsVM.Remove(item);
                }
            }
            if (parameters.Length > 3 && parameters[3] != null)
            {
                var removedConns = parameters[3] as List<LineConnection>;
                foreach (var item in removedConns)
                {
                    item.RemoveThis();
                }
            }
        }
        #endregion
    }
}
