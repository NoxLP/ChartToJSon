using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public class ItemGrabTrigger : NodeTriggerWithGameObject
    {
        public ItemGrabTrigger()
        {
            Type = TriggerTypesEnum.ItemGrab;
        }
    }
}
