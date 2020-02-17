using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChartCanvasNamespace.Entities;
using Newtonsoft.Json;
using StoryChart.ViewModel;
using WPFHelpers;

namespace StoryChart.Model
{
    public class Node : aChartEntityModelBase, IEquatable<Node>, IChartEntity
    {
        #region Constructors
        public Node()
        {
            EntityId = (++NodeIdsHandler._LastId).ToString();
            UserId = EntityId.ToString();
            Rewards = new List<Rewards.aNodeReward>();
            Triggers = new List<Triggers.aNodeTrigger>();
            Children = new List<Node>();
        }
        [JsonConstructor]
        public Node(string userId)
        {
            _IdChangedByJson = true;
            UserId = userId;
            lock (_Lock)
            {
                NodeIdsHandler._LastId = 0;
            }
        }
        #endregion Constructors

        #region Fields
        [JsonIgnore]
        private static object _Lock = new object();
        private string _Description;
        private string _UserId;
        private bool _IdChangedByJson;
        private string _Title;
        #endregion

        #region Properties
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
        public string Title
        {
            get { return _Title; }
            set
            {
                if (value == null)
                {
                    if (_Title != null)
                    {
                        _Title = null;
                        OnPropertyChanged(nameof(Title));
                    }
                }
                else if (!value.Equals(_Title))
                {
                    _Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        [JsonProperty]
        public List<Triggers.aNodeTrigger> Triggers { get; private set; }
        [JsonProperty]
        public List<Rewards.aNodeReward> Rewards { get; private set; }
        #endregion

        /// <summary>
        /// Call NodeIdsHandler.ResetIds() before calling this
        /// </summary>
        /// <returns></returns>
        public Node CopyAsRoot()
        {
            return new Node()
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
                   EntityId == other.EntityId;
        }
        public override int GetHashCode()
        {
            return 863987077 + EntityId.GetHashCode();
        }

        #region ichartentity
        [JsonProperty]
        public override string EntityId { get; protected set; }
        [JsonProperty]
        public List<Node> Children { get; private set; }

        public override bool NewChildAddedFromChart(IChartEntity entity)
        {
            var node = entity as Node;
            if (node == null)
                return false;

            Children.Add(node);
            return true;
        }
        public override bool RemoveChild(IChartEntity entity)
        {
            var node = entity as Node;
            if (node == null || !Children.Contains(node))
                return false;

            Children.Remove(node);
            return true;
        }
        #endregion
    }
}