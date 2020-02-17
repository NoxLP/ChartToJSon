using ChartCanvasNamespace;
using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Lines;
using ChartCanvasNamespace.OtherVisuals;
using ChartCanvasNamespace.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UndoRedoSystem;
using WPFHelpers.Commands;
using JsonLibrary;
using Newtonsoft.Json;
using System.Collections;
using WPFHelpers;
using WPFHelpers.Async;
using System.Dynamic;

namespace ChartToJson.ViewModel
{
    public partial class MainVM
    {
        private readonly string _ClipboardPasteProxy = "CTJClipboardDataPasteProxy";
        private bool _ClipboardContainsData = false;
        private bool _PasteIsChecked;
        private bool _Duplicated;
        private PasteDataProxy _PasteData; 
        private object[] _PasteDuplicateRedoParameters = new object[4];
        private object[] _PasteDuplicateUndoParameters = new object[4];
        private Dictionary<IViewModelOfVisualWithConnectingThumbs, bool> _PastedItemsLoaded;

        public bool ClipboardContainsData
        {
            get { return _ClipboardContainsData; }
            set
            {
                if (value != _ClipboardContainsData)
                {
                    _ClipboardContainsData = value;
                    OnPropertyChanged(nameof(ClipboardContainsData));
                }
            }
        }
        public bool PasteIsChecked
        {
            get { return _PasteIsChecked; }
            set
            {
                if (value != _PasteIsChecked)
                {
                    _PasteIsChecked = value;
                    if (value)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                    OnPropertyChanged(nameof(PasteIsChecked));
                }
            }
        }

        #region commands
        public ICommand CutCommand => new DelegateCommand(x => CutToClipboard(), null);
        public ICommand CopyCommand => new DelegateCommand(x => CopyToClipboard(), null);
        public ICommand DuplicateCommand => new DelegateCommand(x => Duplicate(), null);

        private void CutToClipboard()
        {
            string jsonString = CopySelectedItemsToPasteProxyJSonString();
            if (string.IsNullOrEmpty(jsonString))
                return;

            DataObject data = new DataObject();
            data.SetData(_ClipboardPasteProxy, jsonString);

            Clipboard.SetDataObject(data);
            ClipboardContainsData = true;

            UndoRedoCommandManager.Instance.NewEmptyCollectionCommand(Properties.UndoRedoNames.Default.General_Cut);

            RemoveSelectedEntities();
        }
        private void CopyToClipboard()
        {
            string jsonString = CopySelectedItemsToPasteProxyJSonString();
            if (string.IsNullOrEmpty(jsonString))
                return;

            DataObject data = new DataObject();
            data.SetData(_ClipboardPasteProxy, jsonString);

            Clipboard.SetDataObject(data);
            ClipboardContainsData = true;
        }
        private void Duplicate()
        {
            string jsonString = CopySelectedItemsToPasteProxyJSonString();
            if (string.IsNullOrEmpty(jsonString))
                return;

            _Duplicated = true;
            var jsonMan = new JsonManager(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            try
            {
                _PasteData = jsonMan.DeserializeJson<PasteDataProxy>(jsonString);
            }
            catch (Exception e)
            {
                e.ShowException($"JSON Error trying to deserialize duplicated items:{Environment.NewLine}{Environment.NewLine}");
                return;
            }

            _PasteDuplicateRedoParameters[0] = this;
            _PasteDuplicateUndoParameters[0] = this;
            var distance = 50d * ChartCustomControl.Instance.Scale;

            var connsByStartId = _PasteData.ConnsProxies.ToDictionary(x => x.StartVM, x => x);
            var connsByEndId = _PasteData.ConnsProxies.ToDictionary(x => x.EndVM, x => x);
            if (_PasteData.Entities != null && _PasteData.Entities.Count > 0)
            {
                for (int i = 0; i < _PasteData.Entities.Count; i++)
                {
                    var item = _PasteData.Entities[i];
                    item.CanvasX = item.CanvasX + distance;
                    item.CanvasY = item.CanvasY + distance;
                    item.UserControl = null;

                    var oldId = item.ViewModelId;
                    item.ViewModelId = Guid.NewGuid().ToString();
                    foreach (var connProxy in connsByStartId.Where(x => x.Key.Equals(oldId)))
                    {
                        connProxy.Value.StartVM = item.ViewModelId;
                    }
                    foreach (var connProxy in connsByEndId.Where(x => x.Key.Equals(oldId)))
                    {
                        connProxy.Value.EndVM = item.ViewModelId;
                    }

                    EntitiesVM.Add(item);
                }

                _PasteDuplicateRedoParameters[2] = _PasteData.Entities;
                _PasteDuplicateUndoParameters[2] = _PasteData.Entities;
            }
            if (_PasteData.Texts != null && _PasteData.Texts.Count > 0)
            {
                for (int i = 0; i < _PasteData.Texts.Count; i++)
                {
                    var item = _PasteData.Texts[i];
                    item.CanvasX = item.CanvasX + distance;
                    item.CanvasY = item.CanvasY + distance;
                    item.UserControl = null;

                    var oldId = item.ViewModelId;
                    item.ViewModelId = Guid.NewGuid().ToString();
                    foreach (var connProxy in connsByStartId.Where(x => x.Key.Equals(oldId)))
                    {
                        connProxy.Value.StartVM = item.ViewModelId;
                    }
                    foreach (var connProxy in connsByEndId.Where(x => x.Key.Equals(oldId)))
                    {
                        connProxy.Value.EndVM = item.ViewModelId;
                    }

                    VisualTextsVM.Add(item);
                }

                _PasteDuplicateRedoParameters[3] = _PasteData.Texts;
                _PasteDuplicateUndoParameters[3] = _PasteData.Texts;
            }
        }
        #endregion

        private class PasteDataProxy
        {
            public List<LineConnectionSaveProxy> ConnsProxies;
            public List<IChartEntityViewModel> Entities = null;
            public List<IVisualTextViewModel> Texts = null;
            public double FirstX;
            public double FirstY;
        }
        public void PasteOn(Point canvasLocation)
        {
            _PasteDuplicateRedoParameters[0] = this;
            _PasteDuplicateUndoParameters[0] = this;
            List<LineConnection> conns = null;
            DataObject data = (DataObject)Clipboard.GetDataObject();

            var jsonMan = new JsonManager(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            if(data.GetDataPresent(_ClipboardPasteProxy))
            {
                try
                {
                    _PasteData = jsonMan.DeserializeJson<PasteDataProxy>(data.GetData(_ClipboardPasteProxy) as string);
                }
                catch (Exception e)
                {
                    e.ShowException($"JSON Error trying to deserialize paste:{Environment.NewLine}{Environment.NewLine}");
                    return;
                }

                var connsByStartId = _PasteData.ConnsProxies.ToDictionary(x => x.StartVM, x => x);
                var connsByEndId = _PasteData.ConnsProxies.ToDictionary(x => x.EndVM, x => x);
                if (_PasteData.Entities != null && _PasteData.Entities.Count > 0)
                {
                    for (int i = 0; i < _PasteData.Entities.Count; i++)
                    {
                        var item = _PasteData.Entities[i];
                        item.CanvasX = item.CanvasX - _PasteData.FirstX + canvasLocation.X;
                        item.CanvasY = item.CanvasY - _PasteData.FirstY + canvasLocation.Y;
                        item.UserControl = null;

                        var oldId = item.ViewModelId;
                        item.ViewModelId = Guid.NewGuid().ToString();
                        foreach (var connProxy in connsByStartId.Where(x => x.Key.Equals(oldId)))
                        {
                            connProxy.Value.StartVM = item.ViewModelId;
                        }
                        foreach (var connProxy in connsByEndId.Where(x => x.Key.Equals(oldId)))
                        {
                            connProxy.Value.EndVM = item.ViewModelId;
                        }

                        EntitiesVM.Add(item);
                    }

                    _PasteDuplicateRedoParameters[2] = _PasteData.Entities;
                    _PasteDuplicateUndoParameters[2] = _PasteData.Entities;
                }
                if(_PasteData.Texts != null && _PasteData.Texts.Count > 0)
                {
                    for (int i = 0; i < _PasteData.Texts.Count; i++)
                    {
                        var item = _PasteData.Texts[i];
                        item.CanvasX = item.CanvasX - _PasteData.FirstX + canvasLocation.X;
                        item.CanvasY = item.CanvasY - _PasteData.FirstY + canvasLocation.Y;
                        item.UserControl = null;

                        var oldId = item.ViewModelId;
                        item.ViewModelId = Guid.NewGuid().ToString();
                        foreach (var connProxy in connsByStartId.Where(x => x.Key.Equals(oldId)))
                        {
                            connProxy.Value.StartVM = item.ViewModelId;
                        }
                        foreach (var connProxy in connsByEndId.Where(x => x.Key.Equals(oldId)))
                        {
                            connProxy.Value.EndVM = item.ViewModelId;
                        }

                        VisualTextsVM.Add(item);
                    }

                    _PasteDuplicateRedoParameters[3] = _PasteData.Texts;
                    _PasteDuplicateUndoParameters[3] = _PasteData.Texts;
                }
            }
        }
        private string CopySelectedItemsToPasteProxyJSonString()
        {
            var pasteProxy = GetCurrentPasteDataProxy();
            if (pasteProxy == null)
                return null;

            var jsonMan = new JsonManager(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            string jsonString;
            try
            {
                jsonString = jsonMan.SerializeToJsonInMemory(pasteProxy);
            }
            catch (Exception e)
            {
                e.ShowException($"JSON Error trying to serialize paste:{Environment.NewLine}{Environment.NewLine}");
                return null;
            }

            return jsonString;
        }
        private PasteDataProxy GetCurrentPasteDataProxy()
        {
            if (EntitiesSelected.Count == 0 && TextsSelected.Count == 0)
                return null;

            var pasteProxy = new PasteDataProxy()
            {
                Entities = EntitiesSelected.ToList(),
                Texts = TextsSelected.ToList()
            };
            if (EntitiesSelected.Count > 0)
            {
                pasteProxy.FirstX = EntitiesSelected[0].CanvasX;
                pasteProxy.FirstY = EntitiesSelected[0].CanvasY;
            }
            else
            {
                pasteProxy.FirstX = TextsSelected[0].CanvasX;
                pasteProxy.FirstY = TextsSelected[0].CanvasY;
            }
            var connsProxies = new List<LineConnectionSaveProxy>();
            foreach (var item in ChartCustomControl.Instance.VisualItemsSelected)
            {
                var connection = item as LineConnection;
                if (connection == null)
                    continue;
                connsProxies.Add(connection.GetSerializationProxy());
            }
            pasteProxy.ConnsProxies = connsProxies;

            return pasteProxy;
        }
        public void CheckForPastedConnections(IViewModelOfVisualWithConnectingThumbs visual)
        {
            if (_PasteData == null)
                return;

            if (_PastedItemsLoaded == null)
            {
                _PastedItemsLoaded = new Dictionary<IViewModelOfVisualWithConnectingThumbs, bool>();
                foreach (var item in _PasteData.Entities)
                {
                    bool loaded = visual.Equals(item);
                    _PastedItemsLoaded.Add(item, loaded);
                }
                foreach (var item in _PasteData.Texts)
                {
                    bool loaded = visual.Equals(item);
                    _PastedItemsLoaded.Add(item, loaded);
                }
                return;
            }

            var loadedItemsCount = _PastedItemsLoaded.Where(kvp => kvp.Value).Count();
            if (loadedItemsCount != _PastedItemsLoaded.Count)
            {
                if (_PastedItemsLoaded.ContainsKey(visual))
                    _PastedItemsLoaded[visual] = true;

                if (loadedItemsCount != (_PastedItemsLoaded.Count - 1))
                    return;
            }

            if(_Duplicated)
            {
                if (_PasteData.ConnsProxies != null && _PasteData.ConnsProxies.Count > 0)
                {
                    _PasteDuplicateRedoParameters[1] = _PasteData.ConnsProxies;
                    List<LineConnection> conns = ChartCustomControl.Instance.AddLineConnectionsByProxiesAndReturn(_PasteData.ConnsProxies);
                    _PasteDuplicateUndoParameters[1] = conns;
                }

                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.General_Duplicate, UndoPaste, _PasteDuplicateUndoParameters, RedoPaste, _PasteDuplicateRedoParameters);
                _Duplicated = false;
            }
            else if(PasteIsChecked)
            {
                if (_PasteData.ConnsProxies != null && _PasteData.ConnsProxies.Count > 0)
                {
                    _PasteDuplicateRedoParameters[1] = _PasteData.ConnsProxies;
                    List<LineConnection> conns = ChartCustomControl.Instance.AddLineConnectionsByProxiesAndReturn(_PasteData.ConnsProxies);
                    _PasteDuplicateUndoParameters[1] = conns;
                }
                UndoRedoCommandManager.Instance.NewCommand(Properties.UndoRedoNames.Default.General_Paste, UndoPaste, _PasteDuplicateUndoParameters, RedoPaste, _PasteDuplicateRedoParameters);
                PasteIsChecked = false;
            }

            _PastedItemsLoaded = null;
            _PasteData = null;
            _PasteDuplicateUndoParameters = null;
            _PasteDuplicateRedoParameters = null;
        }

        #region undo/redo
        private void UndoPaste(object[] parameters)
        {
            RedoEntitiesRemoved(parameters);
        }
        private void RedoPaste(object[] parameters)
        {
            UndoEntitiesRemoved(parameters);
        }
        #endregion
    }
}
