using ChartCanvasNamespace.Entities;
using Newtonsoft.Json;
using StoryChart.Model;
using StoryChart.Model.Rewards;
using StoryChart.Model.Triggers;
using StoryChart.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFHelpers.Commands;
using WPFHelpers.ViewModelBase;

namespace StoryChart.ViewModel
{
    [Export(typeof(IChartEntityViewModel))]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesViewModelMetadata.Name), "Story chart plugin entities view-model")]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesViewModelMetadata.Description), "Entites view-model for story chart plugin. Plugin sample of ChartToJson")]
    [ExportMetadata(nameof(ChartToJson.MEF.IChartEntitiesViewModelMetadata.ThumbnailSource), "pack://application:,,,/StoryChartEntitiesTemplatesPlugin;component/Resources/Icons/NodeThumbnail.png")]
    public class NodeViewModel : aChartEntityViewModelBase, IEquatable<NodeViewModel>
    {
        #region Constructors
        public NodeViewModel() : base("CONTENT_StoryChartTemplate_StoryNodeUserControlTemplate")
        {
            AddRewardCommand = new DelegateCommand(x => AddReward(), null);
            AddTriggerCommand = new DelegateCommand(x => AddTrigger(), null);
            Node = new Node();
            ShapeTemplateKey = "SHAPE_A1RectRounded";
        }
        [JsonConstructor]
        public NodeViewModel(Node node) : base("CONTENT_StoryChartTemplate_StoryNodeUserControlTemplate")
        {
            AddRewardCommand = new DelegateCommand(x => AddReward(), null);
            AddTriggerCommand = new DelegateCommand(x => AddTrigger(), null);
            Node = node;
            ShapeTemplateKey = "SHAPE_A1RectRounded";
        }
        #endregion

        #region Fields
        private readonly static Brush _BackgroundBrushUnselected = (Brush)Application.Current.FindResource(SystemColors.ControlLightLightBrushKey);
        private readonly static Brush _BackgroundBrushSelected = new SolidColorBrush(Colors.AliceBlue);
        private Node _Node;
        private Brush _Background = (Brush)Application.Current.FindResource(SystemColors.ControlLightLightBrushKey);
        public NodeViewModel _MyParent;
        private aNodeReward _SelectedReward;
        private aNodeTrigger _SelectedTrigger;
        #endregion

        #region Properties
        [JsonIgnore]
        public ICollectionView Rewards
        {
            get
            {
                var rewards = CollectionViewSource.GetDefaultView(Node.Rewards);
                rewards.SortDescriptions.Add(new SortDescription() { PropertyName = nameof(Model.Rewards.aNodeReward.Id) });

                return rewards;
            }
        }
        [JsonIgnore]
        public ICollectionView Triggers
        {
            get
            {
                var triggers = CollectionViewSource.GetDefaultView(Node.Triggers);
                triggers.SortDescriptions.Add(new SortDescription() { PropertyName = nameof(Model.Triggers.aNodeTrigger.Type) });

                return triggers;
            }
        }
        public Node Node
        {
            get { return _Node; }
            set
            {
                if (value == null)
                {
                    if (_Node != null)
                    {
                        _Node = null;
                        OnPropertyChanged(nameof(Node));
                    }
                }
                else if (!value.Equals(_Node))
                {
                    _Node = value;
                    OnPropertyChanged(nameof(Node));
                }
            }
        }
        [JsonIgnore]
        public Brush Background
        {
            get { return _Background; }
            set
            {
                if (value == null)
                {
                    if (_Background != null)
                    {
                        _Background = null;
                        OnPropertyChanged(nameof(Background));
                    }
                }
                else if (!value.Equals(_Background))
                {
                    _Background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }
        [JsonIgnore]
        public aNodeReward SelectedReward
        {
            get { return _SelectedReward; }
            set
            {
                if (value == null)
                {
                    if (_SelectedReward != null)
                    {
                        _SelectedReward = null;
                        OnPropertyChanged(nameof(SelectedReward));
                    }
                }
                else if (!value.Equals(_SelectedReward))
                {
                    _SelectedReward = value;
                    OnPropertyChanged(nameof(SelectedReward));
                }
            }
        }
        [JsonIgnore]
        public aNodeTrigger SelectedTrigger
        {
            get { return _SelectedTrigger; }
            set
            {
                if (value == null)
                {
                    if (_SelectedTrigger != null)
                    {
                        _SelectedTrigger = null;
                        OnPropertyChanged(nameof(SelectedTrigger));
                    }
                }
                else if (!value.Equals(_SelectedTrigger))
                {
                    _SelectedTrigger = value;
                    OnPropertyChanged(nameof(SelectedTrigger));
                }
            }
        }
        #endregion

        #region IChartEntityViewModel
        private bool _IsSelected;

        public override IChartEntity Entity { get { return Node; } }
        public override bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    if (!value)
                        Background = _BackgroundBrushUnselected;
                    else
                        Background = _BackgroundBrushSelected;
                }
            }
        }
        #endregion

        #region commands
        public ICommand AddRewardCommand { get; private set; }
        public ICommand AddTriggerCommand { get; private set; }
        public ICommand RemoveSelectedRewardCommand => new DelegateCommand(x => RemoveSelectedReward(), null);
        public ICommand RemoveSelectedTriggerCommand => new DelegateCommand(x => RemoveSelectedTrigger(), null);

        private void AddReward()
        {
            var window = new NewRewardWindow();
            var result = window.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            Node.Rewards.Add((aNodeReward)((NewRewardWindowVM)window.DataContext).Reward);
            Rewards.Refresh();
            OnPropertyChanged(nameof(Rewards));
        }
        private void AddTrigger()
        {
            var window = new NewTriggerWindow();
            var result = window.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            Node.Triggers.Add((aNodeTrigger)((NewTriggerWindowVM)window.DataContext).Trigger);
            Triggers.Refresh();
            OnPropertyChanged(nameof(Triggers));
        }
        private void RemoveSelectedReward()
        {
            Node.Rewards.Remove(SelectedReward);
            OnPropertyChanged(nameof(Rewards));
        }
        private void RemoveSelectedTrigger()
        {
            Node.Triggers.Remove(SelectedTrigger);
            OnPropertyChanged(nameof(Triggers));
        }
        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj as NodeViewModel);
        }
        public bool Equals(NodeViewModel other)
        {
            return other != null &&
                   EqualityComparer<Node>.Default.Equals(Node, other.Node);
        }
        public override int GetHashCode()
        {
            return -56134859 + EqualityComparer<Node>.Default.GetHashCode(Node);
        }
    }
}
