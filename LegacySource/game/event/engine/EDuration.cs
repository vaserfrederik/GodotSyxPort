using System;
using System.Collections.Generic;
using game.event.actions;
using game.time;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    public class EDuration
    {
        public readonly double seconds;
        public readonly LIST<EventAction> on_expire;

        public EDuration(Json data, EventActions actions, Event parent)
        {
            if (data.Has("DURATION"))
            {
                data = data.Json("DURATION");
                seconds = data.DTry("DAYS", 0, 1000000, 1) * TIME.SecondsPerDay();
                on_expire = EActions.actions(parent, actions, data);
                data.CheckUnused();
            }
            else
            {
                seconds = 0;
                on_expire = EActions.actions();
            }
        }
    }
}