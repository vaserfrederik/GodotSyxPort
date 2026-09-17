using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Battle.Invasion
{
    public class Attack : BattleStateExiter
    {
        private static readonly string ¤¤vTitle = "¤Victory";
        private static readonly string ¤¤vBody = "¤The enemy is beaten. Rejoice! Spoils from the battlefield will soon arrive. Do you wish to accept the captives, or should we 'release' them my lord?.";

        private static readonly string ¤¤dTitle = "¤Defeat";
        private static readonly string ¤¤dBody = "¤You have lost! Our men have thrown away their lives in vain. The enemy will be be at our gates at any moment! We must pray for mercy.";

        static Attack()
        {
            D.ts(typeof(Attack));
        }

        private readonly int ref;

        public Attack(InvasionSpec invasion)
        {
            ref = invasion.ref;

            BattleStateSpec s = new BattleStateSpec();

            s.player.wCoo.set(FACTIONS.Player().capitolRegion().cx(), FACTIONS.Player().capitolRegion().cy());
            s.player.moraleBase = 1.0;
            foreach (WDIV d in RD.MILITARY().divisions(FACTIONS.Player().capitolRegion()))
            {
                s.player.divs.add(d.generate());
            }
            foreach (WArmy a in armies())
            {
                for (int di = 0; di < a.divs().size(); di++)
                {
                    if (s.player.divs.hasRoom())
                    {
                        s.player.divs.add(a.divs().get(di).generate());
                    }
                }
            }

            foreach (DivGeneration g in invasion.divs)
            {
                s.enemy.divs.add(g);
            }

            for (int i = 0; i < invasion.artillery.Length; i++)
            {
                s.enemy.artillery[i] = invasion.artillery[i];
            }

            BattleState.setGenerate(this, s);
        }

        public override void afterExit(BattleStateResult result)
        {
            InvasionSpec invasion = SETT.INVADOR().spec(ref);
            AD.stats().report(FACTIONS.Player(), result.result == BATTLE_RESULT.VICTORY, result.playerLosses, result.enemyLosses);

            if (result.result == BATTLE_RESULT.VICTORY)
            {
                int di = 0;

                int[] ress = Alloc.ii(TR.ALL().size());

                {
                    double tot = 0;
                    double death = 0;

                    foreach (DivGeneration s in invasion.divs)
                    {
                        tot += s.indus.Length;
                        death += (s.indus.Length - result.enemySurvivors[di] - result.enemyCaptured[di]);
                    }
                    double d = death / (1 + tot);
                    d *= 0.25;

                    foreach (DivGeneration s in invasion.divs)
                    {
                        for (int i = 0; i < s.indus.Length; i++)
                        {
                            for (int ei = 0; ei < STATS.EQUIP().BATTLE_ALL().size(); ei++)
                            {
                                EquipBattle e = STATS.EQUIP().BATTLE_ALL().get(ei);
                                invasion.loot.add(TR.get(e.resource), e.get(s.indus[i]));
                            }
                        }
                    }

                    foreach (TRADABLE res in TR.ALL())
                    {
                        int am = (int)Math.Ceiling(d * invasion.loot.get(res));
                        ress[res.index()] = am;
                    }
                }

                foreach (Race r in RACES.all())
                {
                    int am = result.enemyCaptured[r.index()];
                    ress[TR.get(r).index()] = am;
                }
                invasion.canBeAttacked = false;

                new MVictory2(ress).send();

                foreach (InvasionListener ll in InvasionListener.all)
                {
                    ll.victory(result.playerLosses, result.enemyLosses, invasion.ref);
                }

                GAME.count().INVASIONS_WON.inc(1);
            }
            else
            {
                StockpileImp stock = new StockpileImp();
                List<DivGeneration> nnew = new List<DivGeneration>(invasion.divs.Count);

                for (int di = 0; di < invasion.divs.Count; di++)
                {
                    nnew.Add(invasion.divs[di]);
                }

                for (int i = 0; i < nnew.Count; i++)
                {
                    nnew[i] = invasion.divs[i];
                }

                for (int i = 0; i < invasion.artillery.Length; i++)
                {
                    nnew[i] = invasion.artillery[i];
                }

                foreach (Race r in RACES.all())
                {
                    int am = result.enemyCaptured[r.index()];
                    ress[TR.get(r).index()] = am;
                }
                invasion.canBeAttacked = false;

                new MVictory2(ress).send();

                foreach (InvasionListener ll in InvasionListener.all)
                {
                    ll.victory(result.playerLosses, result.enemyLosses, invasion.ref);
                }

                GAME.count().INVASIONS_WON.inc(1);
            }

            foreach (Race r in RACES.all())
            {
                int am = result.enemyCaptured[r.index()];
                ress[TR.get(r).index()] = am;
            }
            invasion.canBeAttacked = false;

            new MVictory2(ress).send();

            foreach (InvasionListener ll in InvasionListener.all)
            {
                ll.victory(result.playerLosses, result.enemyLosses, invasion.ref);
            }

            GAME.count().INVASIONS_WON.inc(1);
        }

        private static class MVictory2 : MessageSection
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int[] res;
            private readonly double time = TIME.currentSecond();
            private bool accepted = false;

            public MVictory2(int[] res)
                : base(¤¤vTitle)
            {
                this.res = res;
            }

            protected override void make(GuiSection section)
            {
                string st = "" + Str.TMP.clear().add(¤¤vBody);

                section.addDown(8, new GText(UI.FONT().M, st).setMaxWidth(WIDTH));

                Bitmap1D selected = new Bitmap1D(TR.ALL().size(), false);
                selected.setAll(true);

                GRows rr = new GRows(4);

                foreach (TRADABLE r in TR.ALL())
                {
                    if (r.index() >= res.Length || res[r.index()] <= 0)
                        continue;

                    GuiSection ss = new GuiSection()
                    {
                        hoverInfoGet = (GUI_BOX text) =>
                        {
                            text.title(r.name);
                        }
                    };

                    ss.add(new GButt.Checkbox()
                    {
                        clickA = () =>
                        {
                            selected.toggle(r.index());
                        },
                        renAction = () =>
                        {
                            selectedSet(selected.get(r.index()));
                        }
                    });

                    ss.addRightC(2, r.icon());
                    ss.addRightCAbs(40, new GStat()
                    {
                        update = (GText text) =>
                        {
                            GFORMAT.i(text, res[r.index()]);
                        }
                    });

                    ss.body().incrW(48);
                    rr.add(ss);
                }

                bool f = true;
                foreach (RENDEROBJ o in rr.rows())
                {
                    section.addRelBody(f ? 16 : 2, DIR.S, o);
                    f = false;
                }

                section.addRelBody(8, DIR.S, new GButt.ButtPanel(Dic.¤¤Accept)
                {
                    clickA = () =>
                    {
                        if (TIME.currentSecond() - time < TIME.secondsPerDay() && !accepted)
                        {
                            accepted = true;
                            foreach (TRADABLE r in TR.ALL())
                            {
                                int am = res[r.index()];
                                if (am > 0 && selected.get(r.index()))
                                {
                                    FACTIONS.Player().buyer(r).addReserveAndDeliver(am, TRADE_TYPE.spoils);
                                }
                            }
                            VIEW.inters().messages.hide();
                        }
                        base.clickA();
                    },
                    renAction = () =>
                    {
                        activeSet(TIME.currentSecond() - time < TIME.secondsPerDay() && !accepted);
                    }
                });

                section.addRelBody(8, DIR.S, new GButt.ButtPanel(Dic.¤¤Decline)
                {
                    clickA = () =>
                    {
                        if (TIME.currentSecond() - time < TIME.secondsPerDay() && !accepted)
                        {
                            accepted = true;
                            VIEW.inters().messages.hide();
                        }
                        base.clickA();
                    },
                    renAction = () =>
                    {
                        activeSet(TIME.currentSecond() - time < TIME.secondsPerDay() && !accepted);
                    }
                });
            }
        }
    }
}