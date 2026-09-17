using System;
using System.Collections.Generic;
using game.boosting.superb;
using game.event.engine;
using game.faction;
using game.faction.npc;
using game.faction.royalty;
using game.faction.royalty.opinion;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;

namespace game.event.actions
{
    internal sealed class _OPINION_CLEAR : EventActionConstructor
    {
        public _OPINION_CLEAR() : base("OPINION_REMOVE")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(key, data.parent, data.choice, data.json, data.all);
        }

        public sealed class Imp : EventAction
        {
            private readonly string[] keys;

            public Imp(string key, Event parent, EChoice choice, Json data, LISTE<EventAction> all) : base(key, all)
            {
                keys = data.Texts("EVENTS");
                data.CheckUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                KeyMap<ArrayListGrower<SuperSpecImp<Royalty>>> map = new KeyMap<ArrayListGrower<SuperSpecImp<Royalty>>>();

                foreach (SuperSpecImp<Royalty> s in ROPINION.BOOST().Imps())
                {
                    if (!map.ContainsKey(s.Key))
                        map.Put(key, new ArrayListGrower<SuperSpecImp<Royalty>>());
                    map.Get(s.Key).Add(s);
                }

                foreach (string k in keys)
                {
                    if (!map.ContainsKey(k))
                        continue;
                    foreach (SuperSpecImp<Royalty> s in map.Get(k))
                    {
                        foreach (FactionNPC reg in FACTIONS.NPCs())
                        {
                            foreach (Royalty r in reg.Court().All())
                            {
                                s.Activate(r, false);
                            }
                        }
                    }
                }

                base.Exe(e, data);
            }

            public override void Hover(GBox b, Event event, EContext context)
            {
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event event, EContext context, RECTANGLE messBody)
            {
            }
        }
    }
}