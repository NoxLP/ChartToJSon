using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public abstract class aNodeTrigger : aNotifyPropertyChanged
    {
        #region Fields
        private string _Description;
        private TriggerTypesEnum _Type;
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
        public TriggerTypesEnum Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }
        #endregion
    }
}
