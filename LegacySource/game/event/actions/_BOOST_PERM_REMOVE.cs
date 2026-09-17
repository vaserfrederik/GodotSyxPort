using System;
using System.Collections.Generic;
using game.event.engine;
using game.faction;
using init.type;
using snake2d.util.datatypes;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using world;
using world.map.regions;

namespace game.event.actions
{
    internal class _BOOST_PERM_REMOVE : EventActionConstructor
    {
        public _BOOST_PERM_REMOVE() : base("BOOST_PERM_REMOVE")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(key, data);
        }

        public sealed class Imp : EventAction
        {
            private readonly string[] keys;

            public Imp(string key, Data data) : base(key, data.all)
            {
                if (data.json.Has("EVENTS"))
                {
                    keys = data.json.Values("EVENTS");

                    for (int i = 0; i < keys.Length; i++)
                    {
                        string k = keys[i];
                        Event e = data.engine.Read(data.parent, k, data.json, "EVENTS");
                        if (e != null)
                        {
                            k = "EVENT_" + e.key;
                        }
                        keys[i] = k;
                    }
                }
                else
                {
                    keys = null;
                }

                data.json.CheckUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                if (keys != null)
                {
                    KeyMap<ArrayListGrower<TmpBoostSpec>> map = new KeyMap<ArrayListGrower<TmpBoostSpec>>();

                    foreach (TmpBoostSpec s in GAME.BOOST().Specs())
                    {
                        if (!map.ContainsKey(s.key))
                            map.Put(s.key, new ArrayListGrower<TmpBoostSpec>());

                        map.Get(s.key).Add(s);
                    }

                    foreach (string k in keys)
                    {
                        if (!map.ContainsKey(k))
                            continue;
                        foreach (TmpBoostSpec s in map.Get(k))
                        {
                            foreach (Region reg in WORLD.REGIONS().All())
                            {
                                GAME.BOOST().Regions.Set(reg, s, false);
                            }
                            foreach (HCLASS_RACE cl in HCLASS_RACE.ALL())
                            {
                                GAME.BOOST().Popcl.Set(cl, s, false);
                            }
                            foreach (Faction reg in FACTIONS.All())
                            {
                                GAME.BOOST().Factions.Set(reg, s, false);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Region reg in WORLD.REGIONS().All())
                    {
                        GAME.BOOST().Regions.Clear(reg);
                    }
                    foreach (HCLASS_RACE cl in HCLASS_RACE.ALL())
                    {
                        GAME.BOOST().Popcl.Clear(cl);
                    }
                    foreach (Faction reg in FACTIONS.All())
                    {
                        GAME.BOOST().Factions.Clear(reg);
                    }
                }

                base.Exe(e, data);
            }

            public override void Hover(GBox b, Event eventObj, EContext context)
            {
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event eventObj, EContext data, RECTANGLE messBody)
            {
            }
        }
    }
}