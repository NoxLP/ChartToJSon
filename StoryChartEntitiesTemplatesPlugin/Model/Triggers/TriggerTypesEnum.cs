using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryChart.Model.Triggers
{
    public enum TriggerTypesEnum : int
    {
        None,
        Conversation,
        MonsterDefeat,
        ItemInteraction,
        ItemGrab,
        Location
    }
}
