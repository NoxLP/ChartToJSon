using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Rewards
{
    public class NodeRewardWithGameObject : aNodeReward
    {
        private Base.StoryGameObject _SGameObject;

        public Base.StoryGameObject SGameObject
        {
            get { return _SGameObject; }
            set
            {
                if (value == null)
                {
                    if (_SGameObject != null)
                    {
                        _SGameObject = null;
                        OnPropertyChanged(nameof(SGameObject));
                    }
                }
                else if (!value.Equals(_SGameObject))
                {
                    _SGameObject = value;
                    OnPropertyChanged(nameof(SGameObject));
                }
            }
        }
    }
}
