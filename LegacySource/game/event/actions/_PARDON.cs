using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using util.gui.misc;

namespace game.event.actions
{
    class _PARDON : EventActionConstructor
    {
        public _PARDON() : base("PARDON") { }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public class Imp : EventAction
        {
            private readonly bool useSelection;
            private readonly int amount;

            public Imp(string key, Json data, List<EventAction> all) : base(key, all)
            {
                amount = data.i("PRISONER_TARGET", 1, 10000);
                useSelection = data.bool("USE_SELECTION", false);
                data.checkUnused();
            }

            public override void exe(Event event, EContext data)
            {
                ENTITY[] es = SETT.ENTITIES().getAllEnts();
                int ri = GAME.updateI();
                int am = 0;
                for (int i = 0; i < es.Length; i++)
                {
                    int ei = i + ri;
                    ei = MATH.mod(ei, es.Length);

                    ENTITY e = es[ei];
                    if (e is Humanoid)
                    {
                        Humanoid a = (Humanoid)e;
                        if (useSelection && !STATS.EVENT().has(a.indu()))
                            continue;
                        if (a.indu().hType() == HTYPES.PRISONER() && AIModule_Prisoner.DATA().punishmentSet.get(a.ai()) != CRIME_PUNISHMENTS.STOCKS())
                        {
                            AIModule_Prisoner.DATA().punishmentSet.set(a.ai(), CRIME_PUNISHMENTS.STOCKS());
                            am++;
                        }
                        if (am >= amount || (useSelection && am >= data.indu.am))
                            break;
                    }
                }
            }

            public override void addToMessageBody(List<RENDEROBJ> rows, Event event, EContext data, RECTANGLE messBody)
            {
                rows.Add(new GStat()
                {
                    public override void update(GText text)
                    {
                        int am = 0;
                        if (useSelection)
                        {
                            am = STATS.EVENT().stat().data().get(null);
                        }
                        else
                        {
                            am = amount;
                        }
                        GFORMAT.i(text, -am);
                    }
                }.hh(UI.icons().s.slave));
            }

            public override void hover(GBox b, Event event, EContext context)
            {
                int am = 0;
                if (useSelection)
                {
                    am = STATS.EVENT().stat().data().get(null);
                }
                else
                {
                    am = amount;
                }
                b.add(UI.icons().s.slave);
                b.add(GFORMAT.i(b.text(), -am));
                b.NL();
            }

            public override CharSequence problem(Event event, EContext context)
            {
                return null;
            }
        }
    }
}