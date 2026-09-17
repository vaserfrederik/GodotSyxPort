using System.Collections.Generic;
using game.event.actions;
using game.faction;
using init.value;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    class ECondition
    {
        public readonly LIST<EventAction> on_fulfill;
        public readonly Lockable<Faction> request = GVALUES.FACTION.LOCK.push();

        public ECondition(string key, Json data, EventActions actions, Event parent)
        {
            if (key != null)
                data = data.json(key);
            request.push("REQUIRES", data);
            on_fulfill = EActions.actions(parent, actions, data);
        }
    }
}