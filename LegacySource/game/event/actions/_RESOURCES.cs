using System;
using System.Collections.Generic;
using game.event.actions;
using game.event.engine;
using game.faction;
using game.faction.FResources;
using init.trade;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;

namespace game.event.actions
{
    internal class _RESOURCES : EventActionConstructor
    {
        private static readonly CharSequence ¤¤noEnough = "¤You don't have enough resources available.";

        static _RESOURCES()
        {
            D.ts(typeof(_RESOURCES));
        }

        public _RESOURCES() : base("RESOURCES")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public class Imp : EventAction
        {
            private readonly ArrayListGrower<RAmount> datas = new ArrayListGrower<RAmount>();

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                TR.MAP().new KJson("AMOUNTS", data, delegate (TRADABLE s, Json j, string key, bool isWeak)
                {
                    RAmount d = new RAmount(s, new CInt(s.Key()));
                    d.Read(j.Json(key), int.MinValue);
                    datas.Add(d);
                });

                data.CheckUnused();
            }

            public override void SetContext(Event eventObj, EContext data)
            {
                foreach (RAmount d in datas)
                {
                    d.Set(eventObj, data, d.t.Ps().PlayerOwned());
                }
            }

            public override void Exe(Event eventObj, EContext data)
            {
                foreach (RAmount d in datas)
                {
                    int am = d.amount.Get(eventObj, data);
                    if (am < 0)
                    {
                        FACTIONS.Player().Seller(d.t).Vanish(-am, RTYPE.DIPLOMACY);
                    }
                    else if (am > 0)
                    {
                        FACTIONS.Player().Buyer(d.t).AddReserve(am, TRADE_TYPE.Diplomacy, 0, null);
                        FACTIONS.Player().Buyer(d.t).AddDeliver(am, TRADE_TYPE.Diplomacy);
                    }
                }
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event eventObj, EContext data, RECTANGLE messBody)
            {
                GRows rr = new GRows(6).SetMin(100);
                foreach (RAmount d in datas)
                {
                    rr.Add(new GStat
                    {
                        Update = delegate (GText text)
                        {
                            GFORMAT.I(text, d.amount.Get(eventObj, data));
                        },
                        HoverInfoGet = delegate (GBox b)
                        {
                            b.Title(d.t.Names);
                            int ava = d.t.Ps().PlayerOwned();
                            if (d.amount.Get(eventObj, data) < 0)
                            {
                                b.TextLL(Dic.¤¤Needed);
                                b.Tab(6);
                                GText t = b.Text();
                                b.Add(GFORMAT.I(t, d.amount.Get(eventObj, data)));
                                if (ava < -d.amount.Get(eventObj, data))
                                    t.Errorify();
                                b.NL();
                            }
                            b.TextLL(Dic.¤¤Available);
                            b.Tab(6);
                            b.Add(GFORMAT.I(b.Text(), ava));
                            b.NL();
                        }
                    }.Hh(d.t.Icon()).HoverInfoSet(d.t.Names));
                }
                rows.Add(rr.Rows());
            }

            public override void Hover(GBox b, Event eventObj, EContext context)
            {
                int t = 0;

                foreach (RAmount d in datas)
                {
                    if (t > 5)
                    {
                        t = 0;
                        b.NL();
                    }
                    b.Tab(t * 3);
                    t++;
                    b.Add(d.t.Icon());
                    int ava = d.t.Ps().PlayerOwned();
                    {
                        GText te = b.Text();
                        GFORMAT.I(te, d.amount.Get(eventObj, context));
                        if (d.amount.Get(eventObj, context) < 0 && -d.amount.Get(eventObj, context) > ava)
                            te.Errorify();
                        else
                            te.Normalify2();
                        b.Add(te);

                        te = b.Text();
                        te.Add('(');
                        GFORMAT.I(te, ava);
                        te.Add(')');
                        te.Normalify();
                        b.Add(te);
                    }
                }
            }

            public override CharSequence Problem(Event eventObj, EContext context)
            {
                foreach (RAmount d in datas)
                {
                    int ava = d.t.Ps().PlayerOwned();
                    if (d.amount.Get(eventObj, context) < 0 && -d.amount.Get(eventObj, context) > ava)
                        return ¤¤noEnough;
                }
                return null;
            }
        }

        private class RAmount : Amount
        {
            public readonly TRADABLE t;

            public RAmount(TRADABLE res, CInt amount) : base(amount)
            {
                this.t = res;
            }
        }
    }
}