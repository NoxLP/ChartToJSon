using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using StoryChart.ViewModel;

namespace StoryChart.Model
{
    public class Node : aNotifyPropertyChanged, IEquatable<Node>
    {
        #region Constructors
        public Node(bool root)
        {
            if (!root)
            {
                Id = ++NodeIdsHandler._LastId;
                UserId = Id.ToString();
            }
            else
            {
                NodeIdsHandler._LastId = 0;
                Id = NodeIdsHandler._LastId;
                UserId = Id.ToString();
            }

            Root = root;
            Rewards = new ObservableCollection<Rewards.aNodeReward>();
            Triggers = new ObservableCollection<Triggers.aNodeTrigger>();
            Children = new ObservableCollection<NodeViewModel>();
            SharedChildren = new ObservableCollection<NodeViewModel>();
        }
        [JsonConstructor]
        public Node(bool root, string id)
        {
            UserId = id;
            if (root)
            {
                lock (_Lock)
                {
                    _IdChangedByJson = true;
                    NodeIdsHandler._LastId = 0;
                }
            }
            else
                NodeIdsHandler._LastId++;
            Root = root;
        }
        #endregion Constructors

        #region Fields
        [JsonIgnore]
        private static object _Lock = new object();
        private ObservableCollection<NodeViewModel> _Children;
        private string _Description;
        private string _UserId;
        private bool _IdChangedByJson;
        private int _Id;
        private ObservableCollection<Rewards.aNodeReward> _Rewards;
        private string _Title;
        private ObservableCollection<Triggers.aNodeTrigger> _Triggers;
        #endregion

        #region Properties
        public ObservableCollection<NodeViewModel> Children
        {
            get { return _Children; }
            set
            {
                if (!value.Equals(_Children))
                {
                    _Children = value;
                    OnPropertyChanged(nameof(Children));
                }
            }
        }
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value == null)
                {
                    if (_Description != null)
                    {
                        _Description = null;
                        OnPropertyChanged(nameof(Description));
                    }
                }
                else if (!value.Equals(_Description))
                {
                    _Description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public string UserId
        {
            get { return _UserId; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Ids can't be null");
                }
                else if (!value.Equals(_UserId))
                {
                    if (!_IdChangedByJson)
                    {
                        if (NodeIdsHandler.IdIsOk(value, this))
                        {
                            _UserId = value;
                            OnPropertyChanged(nameof(UserId));
                        }
                    }
                    else
                    {
                        _UserId = value;
                        OnPropertyChanged(nameof(UserId));
                        _IdChangedByJson = false;
                    }
                }
            }
        }
        public int Id
        {
            get { return _Id; }
            private set
            {
                if (value != _Id)
                {
                    _Id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        public ObservableCollection<Rewards.aNodeReward> Rewards
        {
            get { return _Rewards; }
            set
            {
                if (value == null)
                {
                    if (_Rewards != null)
                    {
                        _Rewards = null;
                        OnPropertyChanged(nameof(Rewards));
                    }
                }
                else if (!value.Equals(_Rewards))
                {
                    _Rewards = value;
                    OnPropertyChanged(nameof(Rewards));
                }
            }
        }
        public bool Root { get; private set; }
        public string Title
        {
            get { return _Title; }
            set
            {
                if (!value.Equals(_Title))
                {
                    _Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public ObservableCollection<Triggers.aNodeTrigger> Triggers
        {
            get { return _Triggers; }
            set
            {
                if (value == null)
                {
                    if (_Triggers != null)
                    {
                        _Triggers = null;
                        OnPropertyChanged(nameof(Triggers));
                    }
                }
                else if (!value.Equals(_Triggers))
                {
                    _Triggers = value;
                    OnPropertyChanged(nameof(Triggers));
                }
            }
        }
        #endregion

        #region shared children (draw lines between shared nodes)
        private ObservableCollection<NodeViewModel> _SharedChildren;

        public ObservableCollection<NodeViewModel> SharedChildren
        {
            get { return _SharedChildren; }
            set
            {
                if (value == null)
                {
                    if (_SharedChildren != null)
                    {
                        _SharedChildren = null;
                        OnPropertyChanged(nameof(SharedChildren));
                    }
                }
                else if (!value.Equals(_SharedChildren))
                {
                    _SharedChildren = value;
                    OnPropertyChanged(nameof(SharedChildren));
                }
            }
        }
        #endregion

        /// <summary>
        /// Call NodeIdsHandler.ResetIds() before calling this
        /// </summary>
        /// <returns></returns>
        public Node CopyAsRoot()
        {
            return new Node(true)
            {
                Children = Children,
                Description = Description,
                UserId = UserId,
                Rewards = Rewards,
                Title = Title,
                Triggers = Triggers
            };
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }
        public bool Equals(Node other)
        {
            return other != null &&
                   Id == other.Id;
        }
        public override int GetHashCode()
        {
            return 863987077 + Id.GetHashCode();
        }
    }
}