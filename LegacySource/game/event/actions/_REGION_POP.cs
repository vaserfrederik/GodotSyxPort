using game.event.engine;
using init.race;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using world;
using world.map.regions;
using world.region;
using world.region.pop;

namespace game.event.actions
{
    public sealed class _REGION_POP : EventActionConstructor
    {
        public _REGION_POP() : base("REGION_POP") { }

        public override EventAction Action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public sealed class Imp : EventAction
        {
            private readonly LIST<Race> races;
            private readonly double amountRel;
            private readonly int amountAbs;
            private readonly CInt amount;

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                races = RACES.Map().ReadMany(data);
                amountRel = data.dTry("AMOUNT_REL", -1, 1, 0);
                amountAbs = data.i("AMOUNT_ABS", int.MinValue, int.MaxValue, 0);
                amount = new CInt("AMOUNT");
                data.CheckUnused();
            }

            public override void SetContext(Event @event, EContext data)
            {
                amount.Set(@event, data, 0);
                foreach (Region reg in WORLD.REGIONS().Active())
                {
                    if (RD.Event().ii.Get(reg) == 1)
                    {
                        foreach (Race rr in races)
                        {
                            RDRace r = RD.Race(rr);
                            if (r == null)
                                continue;
                            int am = r.Pop.Get(reg);
                            am = (int)(am * amountRel);
                            am += amountAbs;
                            amount.Set(@event, data, amount.Get(@event, data) + am);
                        }
                    }
                }
            }

            public override void Exe(Event @event, EContext data)
            {
                foreach (Region reg in WORLD.REGIONS().Active())
                {
                    if (RD.Event().ii.Get(reg) == 1)
                    {
                        foreach (Race rr in races)
                        {
                            RDRace r = RD.Race(rr);
                            if (r == null)
                                continue;
                            int am = r.Pop.Get(reg);
                            am = (int)(am * amountRel);
                            am += amountAbs;
                            r.Pop.Inc(reg, am);
                        }
                    }
                }
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event @event, EContext data, RECTANGLE messBody)
            {
                rows.Add(new GStat()
                {
                    Update = text =>
                    {
                        GFORMAT.i(text, amount.Get(@event, data));
                    }
                }.Hh(UI.Icons().s.death));
            }

            public override void Hover(GBox b, Event @event, EContext context)
            {
            }
        }
    }
}