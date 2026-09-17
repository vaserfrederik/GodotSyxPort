using System;
using System.Collections.Generic;
using game.GAME;
using game.battle.div;
using game.boosting;
using game.event.engine;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.sprite.UI;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using world.map.regions;
using world.region;

namespace game.event.actions
{
    public sealed class _BOOST : EventActionConstructor
    {
        public readonly ArrayListGrower<Imp> boosts = new ArrayListGrower<Imp>();
        private static readonly CharSequence ¤¤sTitle = "Boosted Subjects";
        private static readonly CharSequence ¤¤sTitleR = "Boosted Regions";

        static _BOOST()
        {
            D.ts(typeof(_BOOST));
        }

        public _BOOST() : base("BOOST")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.parent, data.choice, data.json, data.all);
        }

        public sealed class Imp : EventAction
        {
            public readonly BoostSpecs player = new BoostSpecs("", UI.icons().s.time, false);
            public readonly BoostSpecs subjects = new BoostSpecs("", UI.icons().s.time, false);
            public readonly BoostSpecs regions = new BoostSpecs("", UI.icons().s.time, false);
            public readonly Event parent;
            public readonly EChoice choice;

            public Imp(string key, Event parent, EChoice choice, Json data, LISTE<EventAction> all) : base(key, all)
            {
                this.parent = parent;
                player.read("PLAYER", data, BValue.VALUE1);
                subjects.read("SUBJECTS", data, BValue.VALUE1);
                regions.read("REGIONS", data, BValue.VALUE1);
                this.choice = choice;
                boosts.add(this);
                data.checkUnused();
            }

            public override void hover(GBox b, Event event, EContext context)
            {
                if (player.all().Count > 0)
                {
                    player.hover(b, 1, Dic.¤¤Boosts, -1);
                }
                if (subjects.all().Count > 0)
                {
                    b.textLL(¤¤sTitle);
                    b.add(GFORMAT.i(b.text(), STATS.EVENT().stat().data().get(null)));
                    subjects.hover(b, 1, null, -1);
                }
                if (regions.all().Count > 0)
                {
                    b.textLL(¤¤sTitleR);
                    b.add(GFORMAT.i(b.text(), STATS.EVENT().stat().data().get(null)));
                    regions.hover(b, 1, null, -1);
                }
            }

            public override void addToMessageBody(LISTE<RENDEROBJ> rows, Event event, EContext context, RECTANGLE messBody)
            {
                GRows rr = new GRows(6).setMin(100);
                if (player.all().Count > 0)
                {
                    rows.add(new GHeader(Dic.¤¤Boosts, UI.FONT().S));
                    foreach (BoostSpec s in player.all())
                    {
                        rr.add(new GStat()
                        {
                            update = (GText text) => s.booster.format(text, s.booster.getValue(1)),
                            hoverInfoGet = (GBox b) => s.boostable.hover(b)
                        }.hh(s.boostable.icon));
                    }
                    rows.add(rr.rows());
                    rr = new GRows(6).setMin(100);
                }

                if (subjects.all().Count > 0 && context.indu.am > 0)
                {
                    rows.add(new GStat()
                    {
                        update = (GText text) => GFORMAT.i(text, context.indu.am),
                        hover = (GBox b) => { }
                    }.hh(¤¤sTitle));

                    foreach (BoostSpec s in subjects.all())
                    {
                        rr.add(new GStat()
                        {
                            update = (GText text) => s.booster.format(text, s.booster.getValue(1)),
                            hoverInfoGet = (GBox b) => s.boostable.hover(b)
                        }.hh(s.boostable.icon));
                    }
                    rows.add(rr.rows());
                    rr = new GRows(6).setMin(100);
                }

                if (regions.all().Count > 0 && context.regs.am > 0)
                {
                    rows.add(new GStat()
                    {
                        update = (GText text) => GFORMAT.i(text, context.regs.am),
                        hover = (GBox b) => { }
                    }.hh(¤¤sTitleR));

                    foreach (BoostSpec s in regions.all())
                    {
                        rr.add(new GStat()
                        {
                            update = (GText text) => s.booster.format(text, s.booster.getValue(1)),
                            hoverInfoGet = (GBox b) => s.boostable.hover(b)
                        }.hh(s.boostable.icon));
                    }
                    rows.add(rr.rows());
                    rr = new GRows(6).setMin(100);
                }
            }
        }

        public static void InitBoosts()
        {
            BOOST.connect((BOOSTABLE_O o) => o.boostableValue(Cluster.GetInstance()));
        }

        public static void InitClusters()
        {
            List<Cluster> clusters = new List<Cluster>();
            // Assuming this method will populate clusters based on some logic
            // clusters.Add(new Cluster(boostable, isMul));
        }

        public sealed class Cluster : Booster
        {
            private int upI;
            private double player;
            private double indu;
            private double reg;
            private readonly ArrayListGrower<ClusterEntry> bplayer;
            private readonly ArrayListGrower<ClusterEntry> bindu;
            private readonly ArrayListGrower<ClusterEntry> bregion;
            private readonly bool isMul;

            public Cluster(Boostable boostable, bool isMul)
            {
                bplayer = new ArrayListGrower<ClusterEntry>();
                bindu = new ArrayListGrower<ClusterEntry>();
                bregion = new ArrayListGrower<ClusterEntry>();
                this.isMul = isMul;
            }

            public void AddClusterEntry(ClusterEntry b)
            {
                bplayer.add(b);
                if ((isMul && b.value < 1) || b.value < 0)
                {
                    // Update from and to values
                }
            }

            public void Cache()
            {
                if (upI == GAME.updateI())
                    return;
                upI = GAME.updateI();
                player = def;
                indu = def;
                reg = def;

                if (isMul)
                {
                    foreach (ClusterEntry e in bplayer)
                    {
                        if (IsActive(e))
                            player *= e.value;
                    }

                    foreach (ClusterEntry e in bindu)
                    {
                        if (IsActive(e))
                            indu *= e.value;
                    }

                    foreach (ClusterEntry e in bregion)
                    {
                        if (IsActive(e))
                            reg *= e.value;
                    }

                    indu = indu * player - player;
                    reg = reg * player - player;
                }
                else
                {
                    foreach (ClusterEntry e in bplayer)
                    {
                        if (IsActive(e))
                            player += e.value;
                    }

                    foreach (ClusterEntry e in bregion)
                    {
                        if (IsActive(e))
                            reg += e.value;
                    }

                    foreach (ClusterEntry e in bindu)
                    {
                        if (IsActive(e))
                            indu += e.value;
                    }
                }
            }

            private bool IsActive(ClusterEntry e)
            {
                if (e.imp.choice == null)
                    return GAME.EVENT().current() == e.imp.parent;
                return false;
            }

            public override double getValue(double input)
            {
                return input;
            }

            protected override double pget(BOOSTABLE_O o)
            {
                return o.boostableValue(this);
            }

            public override double vGet(Faction f)
            {
                if (f == FACTIONS.player())
                    return vGet(HCLASS_RACE.clP());
                return def;
            }

            public override double vGet(Region reg)
            {
                Cache();
                return player + (RD.event().ii.get(reg) == 1 ? this.reg : 0);
            }

            public override double vGet(Induvidual indu)
            {
                Cache();
                return player + (STATS.EVENT().has(indu) ? this.indu : 0);
            }

            public override double vGet(Div div)
            {
                if (div.army().faction() == FACTIONS.player())
                {
                    Cache();
                    return player + STATS.EVENT().stat().div().getD(div) * this.indu;
                }
                return def;
            }

            public override double vGet(HCLASS_RACE popTime)
            {
                Cache();
                return player + STATS.EVENT().stat().data(popTime.cl).getD(popTime.race) * this.indu;
            }

            public override double vGet(Player f)
            {
                Cache();
                return player;
            }

            public override double vGet(FactionNPC f)
            {
                return def;
            }

            public override double from()
            {
                return from;
            }

            public override double to()
            {
                return to;
            }
        }

        public sealed class ClusterEntry
        {
            private readonly Imp imp;
            private readonly double value;

            public ClusterEntry(Imp imp, double value)
            {
                this.imp = imp;
                this.value = value;
            }
        }
    }
}