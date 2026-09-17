using System;
using System.Collections.Generic;
using game.event.actions;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    internal static class EActions
    {
        public static LIST<EventAction> actions()
        {
            return new ArrayList<EventAction>(0);
        }

        public static LIST<EventAction> actions(Event e, EventActions act, Json data)
        {
            return get(e, null, act, data, true);
        }

        public static LIST<EventAction> actions(Event e, EChoice c, EventActions act, Json data)
        {
            return get(e, c, act, data, true);
        }

        public static LIST<EventAction> actions(string key, Event e, EventActions act, Json data)
        {
            return actions(key, e, null, act, data, true);
        }

        public static LIST<EventAction> actions(string key, Event e, EChoice choice, EventActions act, Json data, bool allowOther)
        {
            if (!data.has(key))
            {
                return new ArrayList<EventAction>(0);
            }
            else
            {
                data = data.json(key);
                return get(e, choice, act, data, allowOther);
            }
        }

        private static LIST<EventAction> get(Event e, EChoice choice, EventActions act, Json data, bool allow)
        {
            if (data.has("ACTIONS"))
                return act.get(data.jsons("ACTIONS"), e, choice, e.allActions, allow);
            else
                return new ArrayList<EventAction>(0);
        }
    }
}