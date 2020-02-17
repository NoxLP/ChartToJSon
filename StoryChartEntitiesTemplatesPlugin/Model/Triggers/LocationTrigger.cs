using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public class LocationTrigger : aNodeTrigger
    {
        public LocationTrigger()
        {
            Type = TriggerTypesEnum.Location;
        }

        #region Fields
        private Base.Vector3 _Location;
        #endregion
        
        #region Properties
        public Base.Vector3 Location
        {
            get
            {
                if (_Location == null)
                    _Location = new Base.Vector3();

                return _Location;
            }
            set
            {
                if (value == null)
                {
                    if (_Location != null)
                    {
                        _Location = null;
                        OnPropertyChanged(nameof(Location));
                    }
                }
                else if (!value.Equals(_Location))
                {
                    _Location = value;
                    OnPropertyChanged(nameof(Location));
                }
            }
        }
        #endregion

    }
}
