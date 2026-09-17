using System;
using System.Collections.Generic;
using System.Linq;
using game.event.actions;
using game.event.engine;
using init.constant;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;

namespace game.event.actions
{
    public sealed class _SUBJECTS_KILL : EventActionConstructor
    {
        _SUBJECTS_KILL() : base("SUBJECTS_KILL") { }

        private static readonly VectorImp tVec = new VectorImp();
        private static readonly ECollision coll = new ECollision();

        public override EventAction action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public sealed class Imp : EventAction
        {
            private readonly CAUSE_LEAVE cause;
            private readonly bool damage;
            private readonly bool useSelection;
            private readonly int selectionFrom;
            private readonly int selectionTo;
            private readonly List<RAmount> datas = new List<RAmount>();

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                cause = CAUSE_LEAVES.MAP().read("DEATH_CAUSE", data);
                RACES.map().new KJson("AMOUNTS", data, (Race s, Json j, string key, bool isWeak) =>
                {
                    RAmount d = new RAmount(s, new CInt(s.key + "_AMOUNTS"));
                    d.read(j.json(key), 0);
                    datas.Add(d);
                });
                damage = data.bool("DAMAGE", false);
                useSelection = data.bool("USE_SELECTION", false);
                selectionFrom = data.i("SELECTION_FROM", 0, int.MaxValue, 0);
                selectionTo = data.i("SELECTION_TO", 0, selectionFrom, int.MaxValue);
                data.checkUnused();
            }

            public override void setContext(Event eventObj, EContext data)
            {
                foreach (RAmount d in datas)
                {
                    if (useSelection)
                    {
                    }
                    else
                    {
                        d.set(eventObj, data, STATS.POP().POP.data().get(d.t));
                    }
                }
            }

            public override void exe(Event eventObj, EContext data)
            {
                ENTITY[] ee = SETT.ENTITIES().getAllEnts();

                int[] ams = Alloc.ii(RACES.all().size());
                foreach (RAmount a in datas)
                {
                    ams[a.t.index()] += a.amount.get(eventObj, data);
                }

                int si = 0;
                for (int ie = 0; ie < ee.Length; ie++)
                {
                    ENTITY e = ee[ie];
                    if (!(e is Humanoid))
                        continue;

                    Humanoid a = (Humanoid)e;

                    if (useSelection)
                    {
                        if (STATS.EVENT().has(a.indu()))
                        {
                            if (si >= selectionFrom && si < selectionTo)
                            {
                                slap(data, e, damage ? 1 : 0, cause);
                            }
                            ie--;
                            si++;
                        }
                    }
                    else if (ams[a.race().index] > 0)
                    {
                        slap(data, e, damage ? 1 : 0, cause);
                        ams[a.race().index]--;
                        ie--;
                    }
                }
            }

            public override void addToMessageBody(LISTE<RENDEROBJ> rows, Event eventObj, EContext data, RECTANGLE messBody)
            {
                GRows rr = new GRows(6).setMin(100);
                if (useSelection)
                {
                    foreach (HCLASS_RACE c in HCLASS_RACE.ALL())
                    {
                        if (c.race != null && c.cl != null)
                        {
                            int am = STATS.EVENT().stat().data(c.cl).get(c.race);
                            if (am > 0)
                            {
                                rr.add(new GStat()
                                {
                                    public override void update(GText text)
                                    {
                                        GFORMAT.i(text, -am);
                                    }

                                    public override void hoverInfoGet(GBox b)
                                    {
                                        b.title(b.text().add(c.race.info.names).s().add('(').add(c.cl.names).add(')'));
                                        b.add(GFORMAT.i(b.text(), -am));
                                        b.NL();
                                    }
                                }.hh(c.race.appearance().icon.twin(UI.icons().s.death, DIR.NE, 1)));
                            }
                        }
                    }
                }
                else
                {
                    foreach (RAmount d in datas)
                    {
                        rr.add(new GStat()
                        {
                            public override void update(GText text)
                            {
                                GFORMAT.i(text, -d.amount.get(eventObj, data));
                            }

                            public override void hoverInfoGet(GBox b)
                            {
                                b.title(b.text().add(d.t.info.names));
                                b.add(GFORMAT.i(b.text(), -d.amount.get(eventObj, data)));
                                b.NL();
                            }
                        }.hh(d.t.appearance().icon.twin(UI.icons().s.death, DIR.NE, 1)));
                    }
                }

                rows.add(rr.rows());
            }

            public override void hover(GBox b, Event eventObj, EContext context)
            {
                int t = 0;
                if (useSelection)
                {
                    foreach (HCLASS_RACE c in HCLASS_RACE.ALL())
                    {
                        if (c.race != null && c.cl != null)
                        {
                            int am = STATS.EVENT().stat().data(c.cl).get(c.race);
                            if (am > 0)
                            {
                                if (t > 5)
                                {
                                    t = 0;
                                    b.NL();
                                }
                                b.tab(t * 3);
                                t++;
                                b.add(c.race.appearance().icon);
                                b.add(GFORMAT.i(b.text(), -am));
                            }
                        }
                    }
                }
                else
                {
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
                        b.add(GFORMAT.i(b.text(), -d.amount.get(eventObj, context)));
                    }
                }
            }
        }

        private static void slap(EContext t, ENTITY e, double dam, CAUSE_LEAVE cause)
        {
            double mom = EPHYSICS.MOM_TRESHOLDI + RND.rFloat() * 2 * EPHYSICS.MOM_TRESHOLDI;
            if (dam > 0)
            {
                e.speed.setRaw(e.speed.x() + RND.rFloat0(1) * C.TILE_SIZE * 3, e.speed.y() + RND.rFloat0(1) * C.TILE_SIZE * 3);

                coll.dirDot = 1.0;
                coll.tileMomentum = mom * e.physics.getMass();
                coll.damagetileStrength = 0;
                coll.norX = tVec.nX();
                coll.norY = tVec.nY();
                coll.leave = CAUSE_LEAVES.getAccident();
                coll.other = null;
                if (e is Humanoid)
                {
                    Humanoid h2 = (Humanoid)e;
                    h2.inflictDamage(dam, cause);
                    if (e.isRemoved())
                    {
                        return;
                    }
                    else if (!STATS.NEEDS().INJURIES.inDanger(h2.indu()))
                    {
                        HEvent.Handler.alertDanger(h2);
                    }
                }
                e.collide(coll);
            }

            if (!e.isRemoved())
            {
                ((Humanoid)e).kill(false, cause);
            }
        }

        private class RAmount : Amount
        {
            public readonly Race t;

            public RAmount(Race res, CInt amount) : base(amount)
            {
                this.t = res;
            }
        }
    }
}