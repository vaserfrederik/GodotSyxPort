using System;
using System.Collections.Generic;
using game.event.actions;
using game.event.engine;
using init.race;
using init.type;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;

namespace game.event.actions
{
    final class _SUBJECTS_ADD : EventActionConstructor
    {
        _SUBJECTS_ADD() : base("SUBJECTS_ADD")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public final class Imp : EventAction
        {
            private readonly HTYPE immType;
            private readonly ArrayListGrower<RAmount> datas = new ArrayListGrower<RAmount>();

            Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                immType = HTYPES.MAP().readTry("IMMIGRANT_TYPE", data);
                RACES.map().new KJson("AMOUNTS", data)
                {
                    protected override void process(Race s, Json j, string key, bool isWeak)
                    {
                        RAmount d = new RAmount(s, new CInt(s.key + "_AMOUNT"));
                        d.read(j.json(key), 0);
                        datas.add(d);
                    }
                };

                data.checkUnused();
            }

            public override void setContext(Event event, EContext data)
            {
                foreach (RAmount d in datas)
                {
                    d.set(event, data, STATS.POP().pop(d.t, immType));
                }
            }

            public override void exe(Event event, EContext data)
            {
                foreach (RAmount d in datas)
                {
                    int am = d.amount.get(event, data);
                    if (am > 0)
                        SETT.ENTRY().add(d.t, immType, am);
                }
            }

            public override void addToMessageBody(LISTE<RENDEROBJ> rows, Event event, EContext data, RECTANGLE messBody)
            {
                GRows rr = new GRows(6).setMin(100);
                foreach (RAmount d in datas)
                {
                    rr.add(new GStat()
                    {
                        public override void update(GText text)
                        {
                            GFORMAT.i(text, d.amount.get(event, data));
                        }

                        public override void hoverInfoGet(GBox b)
                        {
                            b.title(b.text().add(d.t.info.names).s().add('(').add(immType.names).add(')'));
                            b.add(GFORMAT.iIncr(b.text(), d.amount.get(event, data)));
                            b.NL();
                        }
                    }.hh(d.t.appearance().icon));
                }
                rows.add(rr.rows());
            }

            public override void hover(GBox b, Event event, EContext context)
            {
                int t = 0;
                foreach (RAmount d in datas)
                {
                    if (t > 5)
                    {
                        t = 0;
                        b.NL();
                    }
                    b.tab(t * 3);
                    t++;
                    b.add(d.t.appearance().icon);
                    b.add(GFORMAT.iIncr(b.text(), d.amount.get(event, context)));
                }
            }

            public override CharSequence problem(Event event, EContext context)
            {
                return null;
            }
        }

        private static class RAmount : Amount
        {
            public readonly Race t;

            public RAmount(Race res, CInt amount) : base(amount)
            {
                this.t = res;
            }
        }
    }
}