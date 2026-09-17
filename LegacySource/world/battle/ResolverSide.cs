using System;
using System.Collections.Generic;
using game.faction;
using init.constant;
using init.race;
using init.resources;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using view.main;
using world.army;
using world.battle.Side;
using world.battle.spec;
using world.entity.army;
using world.map.regions;
using world.region;

namespace world.battle
{
    internal sealed class ResolverSide : WBattleSide
    {
        private readonly ArrayList<ResolverUnit> all = new ArrayList<ResolverUnit>(Config.battle().DIVISIONS_PER_ARMY);
        public readonly ArrayList<ResolverUnit> us = new ArrayList<ResolverUnit>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<WBattleUnit> units = new ArrayList<WBattleUnit>(Config.battle().DIVISIONS_PER_ARMY);

        public Side side;
        private readonly Coo coo = new Coo();
        public bool player;
        public double powerBalance;
        private int men;
        private int losses;
        private int lossesRetreat;
        private readonly int[] artilleryPieces = Alloc.ii(AD.supplies().arts().size());
        public readonly Coo retreatCoo = new Coo();

        public ResolverSide()
        {
            while (all.HasRoom())
                all.Add(new ResolverUnit());
        }

        public void Init(Side side, double powerBalance)
        {
            this.side = side;
            this.powerBalance = powerBalance;
            men = 0;
            player = false;
            losses = 0;
            lossesRetreat = 0;
            us.ClearSloppy();
            units.ClearSloppy();
            for (int i = 0; i < side.us.size(); i++)
            {
                ResolverUnit u = all.Get(i);
                u.Init(side.us.Get(i));
                player |= side.us.Get(i).faction() == FACTIONS.player();
                men += side.us.Get(i).men();
                us.Add(u);
                units.Add(u);
            }
            coo.Set(side.us.Get(0).x(), side.us.Get(0).y());
            for (int ai = 0; ai < AD.supplies().arts().size(); ai++)
            {
                int am = 0;
                foreach (SideUnit u in side.us)
                {
                    if (u.a() != null)
                    {
                        am += AD.supplies().arts().Get(ai).current(u.a());
                    }
                }
                artilleryPieces[ai] = am;
            }
            retreatCoo.Set(-1, -1);
        }

        public void Count(RCount c, double looseAmount, bool ret)
        {
            foreach (ResolverUnit u in us)
            {
                u.Count(c, looseAmount, ret);
            }
        }

        public ResolverSide Clear()
        {
            losses = 0;
            lossesRetreat = 0;

            foreach (ResolverUnit u in us)
            {
                u.losses = 0;
                u.lossesRetreat = 0;
            }
            return this;
        }

        public void Extract(double looseAmount)
        {
            foreach (ResolverUnit u in us)
            {
                u.Extract(looseAmount);
            }
        }

        public COORDINATE coo()
        {
            return coo;
        }

        public int men()
        {
            return men;
        }

        public int losses()
        {
            return losses;
        }

        public int lossesRetreat()
        {
            return lossesRetreat;
        }

        public LIST<WBattleUnit> units()
        {
            return units;
        }

        public int artillery(ADArtillery a)
        {
            return artilleryPieces[a.index()];
        }

        public double powerBalance()
        {
            return powerBalance;
        }

        internal sealed class ResolverUnit : WBattleUnit
        {
            public SideUnit unit;
            int losses;
            int lossesRetreat;
            double defences;

            private ResolverUnit()
            {
            }

            private void Init(SideUnit u)
            {
                this.unit = u;
                this.defences = 0;
                losses = 0;
                lossesRetreat = 0;
            }

            public void Count(RCount c, double looseAmount, bool ret)
            {
                int losses = (int)Math.Ceiling(looseAmount * unit.men());
                Count(c, losses, ret);
            }

            public void Count(RCount c, int loss, bool ret)
            {
                double looseAm = (double)loss / unit.men();
                int dead = 0;
                if (unit.a() != null)
                {
                    WArmy a = unit.a();
                    double d = (double)1.1 * looseAm;
                    d = CLAMP.d(d, 0, 1);
                    foreach (ADSupply s in AD.supplies().all)
                    {
                        int am = (int)(s.current().Get(a) * d);
                        c.res[s.res.index()] += am;
                    }

                    for (int di = 0; di < a.divs().size(); di++)
                    {
                        WDIV div = a.divs().Get(di);
                        int dd = (int)Math.Ceiling(d * div.men());
                        c.dead[div.race().index] += dd;
                        dead += dd;
                    }
                }
                else
                {
                    Region reg = unit.r();
                    double d = (double)1.1 * looseAm;
                    for (int di = 0; di < RD.MILITARY().divisions(reg).size(); di++)
                    {
                        WDIV div = RD.MILITARY().divisions(reg).Get(di);
                        int dd = (int)Math.Ceiling(d * div.men());
                        dead += dd;
                        c.dead[div.race().index()] += dd;
                    }
                }
                if (ret)
                {
                    ResolverSide.this.lossesRetreat += dead - lossesRetreat;
                    lossesRetreat = dead;
                }
                else
                {
                    ResolverSide.this.losses += dead - losses;
                    losses = dead;
                }
            }

            public void Extract(double looseAmount)
            {
                if (unit.a() != null)
                    Extract(unit.a(), looseAmount);
                else
                    Extract(unit.r(), looseAmount);
            }

            public void Extract(WArmy a, double looseAmount)
            {
                double d = (double)1.1 * looseAmount;
                foreach (ADSupply s in AD.supplies().all)
                {
                    int am = (int)Math.Ceiling(s.current().Get(a) * d);
                    s.current().inc(a, -am);
                }

                for (int di = 0; di < a.divs().size(); di++)
                {
                    WDIV div = a.divs().Get(di);
                    Kill(div, looseAmount);
                }
            }

            public void Extract(Region reg, double looseAmount)
            {
                for (int di = 0; di < RD.MILITARY().divisions(reg).size(); di++)
                {
                    WDIV div = RD.MILITARY().divisions(reg).Get(di);
                    Kill(div, looseAmount);
                }
            }

            private void Kill(WDIV div, double looseAmount)
            {
                int l = (int)Math.Ceiling(looseAmount * div.men());
                losses += l;

                int survivors = div.men() - l;

                double xp = 0;

                if (survivors > 0)
                {
                    xp = (double)0.1 * div.men() / survivors;
                    xp += div.experience();
                    xp = CLAMP.d(xp, 0, 1);
                }

                div.resolve(survivors, xp);
            }

            public override CharSequence name()
            {
                return unit.a() != null ? unit.a().name : unit.r().info.name();
            }

            public override int men()
            {
                return men;
            }

            public override int losses()
            {
                return losses;
            }

            public override int lossesRetreat()
            {
                return lossesRetreat;
            }

            public override SPRITE icon()
            {
                return unit.faction() != null ? unit.faction().banner().MEDIUM : UI.icons().m.rebellion;
            }

            public override void hover(GUI_BOX box)
            {
                if (unit.a() != null)
                {
                    VIEW.world().UI.armies.hover(box, unit.a());
                }
                else if (unit.r() != null)
                {
                    VIEW.world().UI.regions.hoverGarrison(unit.r(), box);
                }
            }

            public override double defences()
            {
                return defences;
            }
        }

        public sealed class RCount
        {
            public int[] res = Alloc.ii(RESOURCES.ALL().size());
            public int[] dead = Alloc.ii(RACES.all().size());

            public RCount Clear()
            {
                Array.Fill(res, 0);
                Array.Fill(dead, 0);
                return this;
            }
        }
    }
}