using ChartCanvasNamespace.Entities;
using ChartToJson.Model;
using ChartToJson.View.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using WPFHelpers;
using WPFHelpers.Commands;

namespace ChartToJson.MEF
{
    public class PluginsHandlerVMClass : aNotifyPropertyChanged
    {
        public PluginsHandlerVMClass()
        {
            //First create a catalog of exports
            //It can be TypeCatalog(typeof(IView), typeof(SomeOtherImportType)) 
            //to search for all exports by specified types
            //DirectoryCatalog(pluginsPath, "App*.dll") to search specified directories
            //and matching specified file name
            //An aggregate catalog that combines multiple catalogs
            _Catalog = new AggregateCatalog();

            //Here we add all the parts found in all assemblies in directory of executing assembly directory
            //with file name matching Plugin*.dll
            string pluginsPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _Catalog.Catalogs.Add(new DirectoryCatalog(pluginsPath, "*Plugin.dll"));


            //also we add to a search path a subdirectory plugins
            pluginsPath = System.IO.Path.Combine(pluginsPath, "plugins");
            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);
            _Catalog.Catalogs.Add(new DirectoryCatalog(pluginsPath, "*Plugin.dll"));

            //Create the CompositionContainer with the parts in the catalog.
            _Container = new CompositionContainer(_Catalog);

            if (Properties.Settings.Default.Plugins_ImportAllPluginsOnStartup)
                ImportPlugins();
        }

        #region fields
        private AggregateCatalog _Catalog;
        private CompositionContainer _Container;
        [ImportMany(typeof(ResourceDictionary), AllowRecomposition = true)]
        private List<Lazy<ResourceDictionary, IChartEntitiesDataTemplateMetadata>> _RDPlugins;
        [ImportMany(typeof(IChartEntityViewModel), AllowRecomposition = true)]
        private List<ExportFactory<IChartEntityViewModel, IChartEntitiesViewModelMetadata>> _VMPlugins;
        private ExportFactory<IChartEntityViewModel, IChartEntitiesViewModelMetadata> _SelectedFactory;
        public SelectEntityOnLoadedPluginsWindow _SelectEntityWindow;
        private KeyValuePair<string, Shape> _SelectedShapeTemplate;
        #endregion

        #region properties
        public int RDCount { get { return _RDPlugins != null ? _RDPlugins.Count : 0; } }
        public Dictionary<string, DataTemplate> ImportedContentTemplates { get; private set; }
        public Dictionary<string, Shape> ImportedShapes { get; private set; }
        public ICollectionView RDPluginsCollView
        {
            get
            {
                var coll = CollectionViewSource.GetDefaultView(_RDPlugins);
                return coll;
            }
        }
        public ICollectionView VMPluginsCollView
        {
            get
            {
                var coll = CollectionViewSource.GetDefaultView(_VMPlugins);
                return coll;
            }
        }
        public ICollectionView ShapesPluginsCollView
        {
            get
            {
                var coll = CollectionViewSource.GetDefaultView(ImportedShapes);
                coll.SortDescriptions.Add(new SortDescription(nameof(KeyValuePair<string, DataTemplate>.Key), ListSortDirection.Ascending));
                return coll; 
            }
        }
        public ExportFactory<IChartEntityViewModel, IChartEntitiesViewModelMetadata> SelectedFactory
        {
            get { return _SelectedFactory; }
            set
            {
                if (value == null)
                {
                    if (_SelectedFactory != null)
                    {
                        _SelectedFactory = null;
                        OnPropertyChanged(nameof(SelectedFactory));
                    }
                }
                else if (!value.Equals(_SelectedFactory))
                {
                    _SelectedFactory = value;
                    OnPropertyChanged(nameof(SelectedFactory));
                }
            }
        }
        public KeyValuePair<string, Shape> SelectedShapeTemplate
        {
            get { return _SelectedShapeTemplate; }
            set
            {
                if (!value.Equals(_SelectedShapeTemplate))
                {
                    _SelectedShapeTemplate = value;
                    OnPropertyChanged(nameof(SelectedShapeTemplate));
                    var items = ChartCanvasNamespace.ChartCustomControl.Instance.VisualsWithShapeSelected;
                    foreach (var item in items)
                    {
                        item.ShapeTemplateKey = value.Key;
                    }
                }
            }
        }
        #endregion

        #region commands
        public ICommand VMSelectionWindowOkCommand => new DelegateCommand(x => VMSelectionWindowOk(), null);
        public ICommand VMSelectionWindowCancelCommand => new DelegateCommand(x => VMSelectionWindowCancel(), null);

        private void VMSelectionWindowOk()
        {
            if (SelectedFactory == null)
            {
                MessageBox.Show(Properties.Settings.Default.Texts_NoEntitySelected);
                return;
            }

            _SelectEntityWindow.Close();
        }
        private void VMSelectionWindowCancel()
        {
            _SelectEntityWindow.Close();
        }
        #endregion

        #region helpers
        private void SaveTemplates()
        {
            ImportedContentTemplates = new Dictionary<string, DataTemplate>();
            ImportedShapes = new Dictionary<string, Shape>();

            foreach (var plugin in _RDPlugins)
            {
                var dictionary = plugin.Value as ResourceDictionary;
                if (dictionary == null)
                    continue;

                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = entry.Key;
                    var sKey = key as string;
                    if (string.IsNullOrEmpty(sKey))
                        continue;

                    var condition = plugin.Metadata.TakeOnlyTemplatesWhoseKeyContains;
                    if (!string.IsNullOrEmpty(condition) && !sKey.Contains(condition))
                        continue;
                    if (sKey.Contains(Properties.Settings.Default.MEFPluginTemplatesKeysString_Content))
                    {
                        var dt = dictionary[key] as DataTemplate;
                        if (dt == null)
                            continue;
                        ImportedContentTemplates.Add(sKey, dt);
                    }
                    else if(sKey.Contains(Properties.Settings.Default.MEFPluginTemplatesKeysString_Shape))
                    {
                        var shape = dictionary[key] as Shape;
                        if (shape == null)
                            continue;
                        ImportedShapes.Add(sKey, shape);
                    }
                }
            }
        }
        #endregion

        private void ImportPlugins()
        {
            try
            {
                _Container.ComposeParts(this);
                SaveTemplates();
            }
            catch (Exception e)
            {
                e.ShowException("Error loading plugin:" + Environment.NewLine);
            }

            //refresh catalog for any changes in plugins
            //catalog.Refresh();

            //Fill the imports of this object
            //finds imports and fills in all preperties decorated
            //with Import attribute in this instance
            //_Container.ComposeParts(this);
            //another option
            //container.SatisfyImportsOnce(this);

            //if we expect more than one plugin then we can operate on its metadata
            //information before creating plugin instance

            //if (string.IsNullOrEmpty(CurrentPluginName))
            //{
            //    var pluginContainer = _Plugins.FirstOrDefault();
            //    if (pluginContainer != null)
            //    {
            //        PluginView = pluginContainer.Value;
            //        CurrentPluginName = pluginContainer.Metadata.Name;
            //    }
            //    else
            //        CurrentPluginName = "<No plugins found>";
            //}
            //else
            //{
            //    var pluginContainer = _Plugins.Where(pc => pc.Metadata.Name != CurrentPluginName).FirstOrDefault();
            //    if (pluginContainer != null)
            //    {
            //        PluginView = pluginContainer.Value;
            //        CurrentPluginName = pluginContainer.Metadata.Name;
            //    }
            //    else
            //        CurrentPluginName = "<No other plugins found>";
            //}
        }
        public IChartEntityViewModel GetVMInstance()
        {
            if(SelectedFactory != null)
                return SelectedFactory.CreateExport().Value;
            return null;
        }
        internal Tuple<HashSet<string>, HashSet<string>> GetMetadataForSave()
        {
            //return new Tuple<List<IChartEntitiesDataTemplateMetadata>, List<IChartEntitiesViewModelMetadata>>(
            //    _RDPlugins.Select(x => x.Metadata).ToList(),
            //    _VMPlugins.Select(x => x.Metadata).ToList());
            return new Tuple<HashSet<string>, HashSet<string>>(
                new HashSet<string>(_RDPlugins.Select(x => x.Metadata.Name)),
                new HashSet<string>(_VMPlugins.Select(x => x.Metadata.Name)));
        }
        public async Task<List<string>> CheckRDErrorsOnLoadingFile(ChartSaveProxy saveFile)
        {
            var errors = new List<string>();
            foreach (var item in saveFile.DataTemplatesPlugins)
            {
                if (!_RDPlugins.Any(x => x.Metadata.Name.Equals(item)))
                {
                    errors.Add(item);
                }
            }
            return errors;
        }
        public async Task<List<string>> CheckVMErrorsOnLoadingFile(ChartSaveProxy saveFile)
        {
            var errors = new List<string>();
            foreach (var item in saveFile.VMPlugins)
            {
                if (!_VMPlugins.Any(x => x.Metadata.Name.Equals(item)))
                {
                    errors.Add(item);
                }
            }
            return errors;
        }
    }
}
