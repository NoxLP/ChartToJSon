using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public class NodeTriggerWithGameObject : aNodeTrigger
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
