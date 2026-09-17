using System;
using System.Collections.Generic;
using game.boosting;
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
using util.info;
using util.text;

namespace game.event.actions
{
    public class _OPINION : EventActionConstructor
    {
        private static readonly CharSequence ¤¤sTitle = "Affected Royalties";

        static _OPINION()
        {
            D.ts(typeof(_OPINION));
        }

        public _OPINION()
            : base("OPINION")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(key, data.parent, data.choice, data.json, data.all);
        }

        public class Imp : EventAction
        {
            private readonly SuperSpecImp<Royalty> spec;

            public Imp(string key, Event parent, EChoice choice, Json data, LISTE<EventAction> all)
                : base(key, all)
            {
                BSourceInfo info = new BSourceInfo(parent.info.name, parent.info.icon);
                string desc = "" + info.name;
                if (choice != null)
                    desc += " - " + choice.name + ". ";

                double time = data.dTry("LENGHT_DAYS", 0, 1000, -1);
                double value = data.d("VALUE", -1000, 1000);
                bool isMul = data.bool("IS_MUL", false);
                double increase = data.dTry("INCREASE_PER_DAY", -100, 100, 0);
                string k = "EVENT_" + key;
                if (increase < 0)
                {
                    spec = new SuperSpec.Downer<Royalty>(-increase, ROPINION.BOOST(), k, info, desc, value, isMul, time);
                }
                else if (increase > 0)
                {
                    spec = new SuperSpec.Uper<Royalty>(increase, time, ROPINION.BOOST(), k, info, desc, value, isMul);
                }
                else if (time > 0)
                {
                    spec = new SuperSpec.TimeLimit<Royalty>(time, ROPINION.BOOST(), k, info, desc, value, isMul);
                }
                else
                {
                    spec = new SuperSpec.Permanent<Royalty>(ROPINION.BOOST(), k, info, desc, value, isMul);
                }
                spec.hidden = true;
                data.checkUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                foreach (FactionNPC f in FACTIONS.NPCs())
                {
                    foreach (Royalty r in f.court().all())
                    {
                        if (r.Event())
                        {
                            spec.Activate(r, true);
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
                int am = context.royalty.am;

                rows.Add(new GStat()
                {
                    public void Update(GText text)
                    {
                        GFORMAT.i(text, am);
                    }
                }.hh(¤¤sTitle));

                rows.Add(new GStat()
                {
                    public void Update(GText text)
                    {
                        if (spec.isMul)
                        {
                            text.Add('*').s();
                            GFORMAT.f1(text, spec.to());
                        }
                        else
                        {
                            GFORMAT.f0(text, spec.to());
                        }
                    }
                }.hh(ROPINION.BOOST().bo.name));
            }
        }
    }
}