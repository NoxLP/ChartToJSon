using ChartCanvasNamespace.Entities;
using ChartToJson.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPFHelpers.Commands;
using WPFHelpers;
using JsonLibrary;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using ChartToJson.MEF;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.IO;
using System.Windows.Media;
using ChartCanvasNamespace.OtherVisuals;

namespace ChartToJson.ViewModel
{
    public partial class MainVM
    {
        private string _LastSavePathThisSession = "";
        private string _MainWindowTitle = $"{Properties.Settings.Default.MainWindowTitlePrefix} - *";
        private bool _CanSaveWithoutDialog;

        public string MainWindowTitle
        {
            get { return _MainWindowTitle; }
            set
            {
                if (value == null)
                {
                    if (_MainWindowTitle != null)
                    {
                        _MainWindowTitle = null;
                        OnPropertyChanged(nameof(MainWindowTitle));
                    }
                }
                else if (!value.Equals(_MainWindowTitle))
                {
                    _MainWindowTitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));
                }
            }
        }
        public bool CanSaveWithoutDialog
        {
            get { return _CanSaveWithoutDialog; }
            set
            {
                if (value != _CanSaveWithoutDialog)
                {
                    _CanSaveWithoutDialog = value;
                    OnPropertyChanged(nameof(CanSaveWithoutDialog));
                }
            }
        }

        #region commands
        public ICommand NewFileCommand => new DelegateCommand(x => NewFile(), null);
        public ICommand OpenFileCommand => new AsyncDelegateCommand(x => OpenFileAsync(), null);
        public ICommand SaveFileCommand => new AsyncDelegateCommand(x => SaveFileAsync(), null);
        public ICommand SaveAsFileCommand => new AsyncDelegateCommand(x => SaveAsFileAsync(), null);

        private void NewFile(bool askConfirmation = true)
        {
            if (askConfirmation)
            {
                var confirmation = MessageBox.Show(Properties.Settings.Default.NewFileConfirmationMessage, "Warning", MessageBoxButton.YesNoCancel);

                if (confirmation == MessageBoxResult.Cancel)
                    return;
                //Save before new file
                if (confirmation == MessageBoxResult.Yes)
                    Task.Run(() => SaveFileAsync());
            }

            EntitiesVM.Clear();
            VisualTextsVM.Clear();

            _CanvasWidth = Properties.Settings.Default.DefaultCanvasWidth;
            _CanvasHeight = Properties.Settings.Default.DefaultCanvasHeight;
            ChartCanvasNamespace.ChartCustomControl.Instance.ResetZoom();
            SetBrushesToDefault();

            MainWindowTitle = $"{Properties.Settings.Default.MainWindowTitlePrefix} - ";
            UndoRedoSystem.UndoRedoCommandManager.Instance.ClearAll();
        }
        private string OpenFilePluginsErrorsMessage(List<string>[] pluginsErrors, string filename)
        {
            var sb = new StringBuilder();
            sb.Append(Properties.Settings.Default.OpenFileErrorMessage_General.Replace("$", filename));
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            if (pluginsErrors[0].Count > 0)
            {
                sb.Append(Properties.Settings.Default.OpenFileErrorMessage_ResourceDictionaryPluginNotFound);
                sb.Append(Environment.NewLine);
                sb.Append(string.Join($"{Environment.NewLine}- ", pluginsErrors[0]));
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }
            if (pluginsErrors[1].Count > 0)
            {
                sb.Append(Properties.Settings.Default.OpenFileErrorMessage_VMPluginNotFound);
                sb.Append(Environment.NewLine);
                sb.Append(string.Join($"{Environment.NewLine}- ", pluginsErrors[1]));
            }
            return sb.ToString();
        }
        private async Task OpenFileAsync()
        {
            if (!UnsavedChangesInCurrentFile)
            {
                var confirmation = MessageBox.Show(Properties.Settings.Default.OpenFileConfirmationMessage, "Warning", MessageBoxButton.YesNoCancel);

                if (confirmation == MessageBoxResult.Cancel)
                    return;
                //Save before new file
                if (confirmation == MessageBoxResult.Yes)
                    await SaveFileAsync();

                NewFile(false);
            }

            var fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Properties.Settings.Default.LastFileLoadedPath;
            fileDialog.Filter = "Chart to JSON files (*.ctj)|*.ctj|Todos (*.*)|*.*";
            fileDialog.Multiselect = false;

            var result = fileDialog.ShowDialog();

            if(result.HasValue && result.Value)
            {
                var cts = new CancellationTokenSource();
                SetProgressBar("Opening file...", true, 0);
                Task progressTask = AnimateProgressBarTo(20, cts.Token);
                var jsonMan = new JsonManager(new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.All
                });
                ChartSaveProxy saveFile = null;
                try
                {
                    saveFile = await jsonMan.DeserializeJsonFileAsync<ChartSaveProxy>(fileDialog.FileName);
                }
                catch (Exception e)
                {
                    e.ShowException($"JSON Error trying to open file:{Environment.NewLine}{Environment.NewLine}");
                    return;
                }

                var pluginsErrorsTasks = new Task<List<string>>[2] { PluginsHandlerVM.CheckRDErrorsOnLoadingFile(saveFile), PluginsHandlerVM.CheckVMErrorsOnLoadingFile(saveFile) };
                var pluginsErrors = await Task.WhenAll(pluginsErrorsTasks);

                if (pluginsErrors[0].Count > 0 || pluginsErrors[1].Count > 0)
                {
                    cts.Cancel();
                    SetProgressBar("", false, 0);
                    MessageBox.Show(OpenFilePluginsErrorsMessage(pluginsErrors, fileDialog.FileName));
                    return;
                }
                SetProgressBar(20);

                progressTask = AnimateProgressBarTo(90, cts.Token);
                foreach (var item in saveFile.EntitiesVM)
                {
                    Application.Current.Dispatcher.Invoke(() => EntitiesVM.Add(item));
                }
                foreach (var item in saveFile.TextVMs)
                {
                    Application.Current.Dispatcher.Invoke(() => VisualTextsVM.Add(item));
                }
                foreach (var item in saveFile.TextWShapeVMs)
                {
                    Application.Current.Dispatcher.Invoke(() => VisualTextsVM.Add(item));
                }

                while (true)
                {
                    if (EntitiesVM.All(x => x.UserControl != null))
                        break;

                    await Task.Delay(100);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var chart = ChartCanvasNamespace.ChartCustomControl.Instance;
                    chart.AddLineConnectionOnLoadingFile(saveFile.Connections);
                    CanvasWidth = saveFile.CanvasSize[0];
                    CanvasHeight = saveFile.CanvasSize[1];
                    chart.ForceSetZoom(saveFile.Zoom, new Point(saveFile.Origin[0], saveFile.Origin[1]));
                    chart.ShowGrid = saveFile.GridOn;
                    chart.SnapToGrid = saveFile.SnapToGrid;
                    chart.SnapToOtherEntities = saveFile.SnapToEntities;
                    chart.SnapToConnectionAnchorPoints = saveFile.SnapToAnchors;
                    SelectedGridBrush = saveFile.GridBrush;
                    SelectedCanvasBackground = saveFile.CanvasBrush;
                    SelectedWorkspaceBackground = saveFile.WorkspaceBrush;
                });

                Properties.Settings.Default.LastFileLoadedPath = fileDialog.FileName;
                Properties.Settings.Default.Save();

                SetProgressBar("", false, 0);
            }

            MessageBox.Show("File loaded", "Message");
            MainWindowTitle = $"{Properties.Settings.Default.MainWindowTitlePrefix} - {fileDialog.FileName}";
            UndoRedoSystem.UndoRedoCommandManager.Instance.ClearAll();
            _LastSavePathThisSession = fileDialog.FileName;
        }
        private async Task SaveFileAsync()
        {
            var lastPath = Properties.Settings.Default.LastFileLoadedPath;
            if (!File.Exists(lastPath))
            {
                MessageBox.Show($"Couldn't find last saved file: {lastPath}", "Error");
                return;
            }

            var chart = ChartCanvasNamespace.ChartCustomControl.Instance;
            var pluginsMetadata = PluginsHandlerVM.GetMetadataForSave();
            var t = chart.GetCurrentZoom();
            var textsWithShape = VisualTextsVM.Where(x => x is IVisualTextWithShapeViewModel).Select(x => x as IVisualTextWithShapeViewModel);
            var texts = VisualTextsVM.Except(textsWithShape);
            var saveFile = new ChartSaveProxy()
            {
                EntitiesVM = new List<IChartEntityViewModel>(EntitiesVM),
                TextVMs = new List<IVisualTextViewModel>(texts),
                TextWShapeVMs = new List<IVisualTextWithShapeViewModel>(textsWithShape),
                DataTemplatesPlugins = pluginsMetadata.Item1,
                VMPlugins = pluginsMetadata.Item2,
                Connections = chart.GetConnectionsSerializationProxies(),
                CanvasSize = new List<double>(2) { CanvasWidth, CanvasHeight },
                Zoom = t.Item1,
                Origin = new List<double>(2) { t.Item2.X, t.Item2.Y },
                GridOn = chart.ShowGrid,
                SnapToGrid = chart.SnapToGrid,
                SnapToEntities = chart.SnapToOtherEntities,
                SnapToAnchors = chart.SnapToConnectionAnchorPoints,
                GridBrush = SelectedGridBrush,
                CanvasBrush = SelectedCanvasBackground,
                WorkspaceBrush = SelectedWorkspaceBackground
            };

            var jsonMan = new JsonManager(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
            try
            {
                await jsonMan.SerializeToJsonInFile_ReadableAsync(saveFile, lastPath);
            }
            catch (Exception e)
            {
                e.ShowException($"JSON Error trying to save file:{Environment.NewLine}{Environment.NewLine}");
                return;
            }

            Properties.Settings.Default.LastFileLoadedPath = lastPath;
            Properties.Settings.Default.Save();

            MessageBox.Show("File saved", "Message");
            MainWindowTitle = $"{Properties.Settings.Default.MainWindowTitlePrefix} - {lastPath}";
            UndoRedoSystem.UndoRedoCommandManager.Instance.ClearAll();
        }
        private async Task SaveAsFileAsync()
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = Properties.Settings.Default.LastFileLoadedPath;
            fileDialog.Filter = "Chart to JSON files (*.ctj)|*.ctj|Todos (*.*)|*.*";

            var result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var chart = ChartCanvasNamespace.ChartCustomControl.Instance;
                var pluginsMetadata = PluginsHandlerVM.GetMetadataForSave();
                var t = chart.GetCurrentZoom();
                var textsWithShape = VisualTextsVM.Where(x => x is IVisualTextWithShapeViewModel).Select(x => x as IVisualTextWithShapeViewModel);
                var texts = VisualTextsVM.Except(textsWithShape);
                var saveFile = new ChartSaveProxy()
                {
                    EntitiesVM = new List<IChartEntityViewModel>(EntitiesVM),
                    TextVMs = new List<IVisualTextViewModel>(texts),
                    TextWShapeVMs = new List<IVisualTextWithShapeViewModel>(textsWithShape),
                    DataTemplatesPlugins = pluginsMetadata.Item1,
                    VMPlugins = pluginsMetadata.Item2,
                    Connections = chart.GetConnectionsSerializationProxies(),
                    CanvasSize = new List<double>(2) { CanvasWidth, CanvasHeight },
                    Zoom = t.Item1,
                    Origin = new List<double>(2) { t.Item2.X, t.Item2.Y },
                    GridOn = chart.ShowGrid,
                    SnapToGrid = chart.SnapToGrid,
                    SnapToEntities = chart.SnapToOtherEntities,
                    SnapToAnchors = chart.SnapToConnectionAnchorPoints,
                    GridBrush = SelectedGridBrush,
                    CanvasBrush = SelectedCanvasBackground,
                    WorkspaceBrush = SelectedWorkspaceBackground
                };

                var jsonMan = new JsonManager(new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.All
                });
                try
                {
                    await jsonMan.SerializeToJsonInFile_ReadableAsync(saveFile, fileDialog.FileName);
                }
                catch(Exception e)
                {
                    e.ShowException($"JSON Error trying to save file:{Environment.NewLine}{Environment.NewLine}");
                    return;
                }

                Properties.Settings.Default.LastFileLoadedPath = fileDialog.FileName;
                Properties.Settings.Default.Save();
                _LastSavePathThisSession = fileDialog.FileName;
                CanSaveWithoutDialog = UnsavedChangesInCurrentFile;

                MessageBox.Show("File saved", "Message");
                MainWindowTitle = $"{Properties.Settings.Default.MainWindowTitlePrefix} - {fileDialog.FileName}";
                UndoRedoSystem.UndoRedoCommandManager.Instance.ClearAll();
            }
        }
        #endregion
    }
}
