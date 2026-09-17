using System;
using System.Collections.Generic;
using game.event.actions;
using game.faction;
using init.value;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    public class EChoice
    {
        public readonly string name;
        public readonly Lockable<Faction> request = GVALUES.FACTION.LOCK.Push();
        public readonly int index;
        public readonly LIST<EventAction> actions;

        public EChoice(Event e, int index, EventActions act, Json data, string name)
        {
            this.name = name;
            request.Push("REQUIRES", data);
            this.index = index;
            actions = EActions.actions(e, this, act, data);
            data.CheckUnused();
        }
    }
}