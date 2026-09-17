using System;
using System.Collections.Generic;
using game.GAME;
using game.event.engine;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.actions
{
    public sealed class EventActions
    {
        private readonly KeyMap<EventActionConstructor> map = new KeyMap<EventActionConstructor>();
        private readonly EventCollection coll;
        public readonly _BOOST boosts;

        public EventActions(EventCollection coll)
        {
            register(new _PASTURE());
            register(new _ORCHARD());
            register(new _PARDON());
            register(new _INVASION());
            register(new _DESTRUCTION());
            register(new _OUTBREAK());
            register(new _RESOURCES());
            register(new _CREDITS());
            register(new _EARTHQUAKE());
            register(new _SUBJECTS_ADD());
            register(new _SUBJECTS_KILL());
            register(new _SOUND_AMBIENT());
            register(new _ALTER_SELECTION());
            register(new _COLOR());
            boosts = new _BOOST();
            register(boosts);
            register(new _EVENT());
            register(new _WEATHER());
            register(new _BOOST_PERM());
            register(new _BOOST_PERM_REMOVE());
            register(new _OPINION());
            register(new _OPINION_CLEAR());
            register(new _REGION_POP());
            this.coll = coll;
        }

        private void register(EventActionConstructor e)
        {
            map.put(e.key, e);
        }

        private static bool hasWarned = false;

        public LIST<EventAction> get(Json[] jsons, Event parent, EChoice choice, LISTE<EventAction> all, bool allow)
        {
            if (jsons == null)
                return new ArrayList<EventAction>(0);
            ArrayList<EventAction> res = new ArrayList<EventAction>(jsons.Length);

            EventActionConstructor.Data data = new EventActionConstructor.Data();
            data.all = all;
            data.engine = coll;
            data.choice = choice;
            data.parent = parent;

            foreach (Json j in jsons)
            {
                string t = j.value("TYPE");
                if (!map.containsKey(t))
                {
                    string s = "There is no Action Type named " + t;
                    if (!hasWarned)
                    {
                        hasWarned = true;
                        s += Environment.NewLine;
                        s += "Available:";
                        s += Environment.NewLine;
                        s += map.keysString();
                        GAME.Warn(j.errorGet(s, "TYPE"));
                    }
                    else
                        LOG.err(j.errorGet(s, "TYPE"));
                }
                else
                {
                    if (!allow && t.Equals("EVENT"))
                        j.error("spawning an event here is not allowed", t);

                    bool hideUI = j.bool("HIDE_UI", false);

                    data.json = j;

                    EventAction a = map.get(t).action(data);
                    a.hideUI = hideUI;
                    res.add(a);
                }
            }

            return res;
        }

        public void init()
        {
            _BOOST.init(coll);
            EventActionContext.check(coll.all);
        }
    }
}