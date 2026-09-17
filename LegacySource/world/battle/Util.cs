using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using init.constant;
using init.race;
using init.resources;
using init.sprite.UI;
using init.trade;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sets;
using util;
using util.text;
using view.ui.message;
using world;
using world.army;
using world.entity.army;
using world.entity.caravan;
using world.map.regions;
using world.map.regions.centre;
using world.region;
using world.region.building;
using world.region.pop;

namespace world.battle
{
    class Util
    {
        Util()
        {
        }

        public static bool Allies(Faction a, Faction b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            return DIP.Get(a, b).ally;
        }

        public static bool Enemies(Faction a, Faction b)
        {
            return DIP.WAR().Is(a, b);
        }

        private readonly Pair res = new Pair();

        public Pair Fill(Faction a, Faction b, int cx, int cy)
        {
            res.a.ClearSloppy();
            res.b.ClearSloppy();

            GUTIL.Flooder().Init(typeof(Util));
            GUTIL.Flooder().PushSloppy(cx, cy, 0);

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();

                if (t.GetValue() > WArmy.ReinforceTiles)
                    break;

                foreach (WArmy ar in WORLD.ENTITIES().armies.FillTile(t.x(), t.y()))
                {
                    if (Valid(ar) == null)
                        continue;
                    if (ar.Ctx() != t.x() || ar.Cty() != t.y())
                        continue;

                    if (Allies(a, ar.Faction()) && Enemies(b, ar.Faction()))
                    {
                        res.a.Add(ar);
                    }
                    else if (Allies(b, ar.Faction()) && Enemies(a, ar.Faction()))
                        res.b.Add(ar);
                }

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    if (WORLD.PATH().map.Can(t, d))
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance());
                }
            }
            GUTIL.Flooder().Done();
            return res;
        }

        private Rec fillBounds = new Rec(WCentre.TILE_DIM * 2);

        public WArmy GetBesieger(Region reg)
        {
            if (!reg.Active())
                return null;
            if (!reg.Besieged())
                return null;
            fillBounds.MoveC(reg.Cx(), reg.Cy());
            foreach (WArmy a in WORLD.ENTITIES().armies.FillTiles(fillBounds))
            {
                if (Valid(a) != null && a.Faction() == FACTIONS.Player() && a.Besieging(reg))
                {
                    return a;
                }
            }
            foreach (WArmy a in WORLD.ENTITIES().armies.FillTiles(fillBounds))
            {
                if (Valid(a) != null && a.Besieging(reg))
                {
                    return a;
                }
            }
            return null;
        }

        public static WArmy Valid(WArmy a)
        {
            if (a == null)
                return null;
            if (AD.men(null).Get(a) <= 0)
                return null;
            return a;
        }

        public static COORDINATE RetTile(WArmy a)
        {
            if (a.Faction() == null)
                return null;

            GUTIL.Flooder().Init(typeof(Util));
            GUTIL.Flooder().PushSloppy(a.Ctx(), a.Cty(), 0);

            double ap = AD.power().Get(a);
            double pow = EnemyPower(a.Faction(), a.Ctx(), a.Cty());

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                if (t.GetValue() > 8)
                    break;

                double p = EnemyPower(a.Faction(), t.x(), t.y());

                if (p > pow)
                    continue;

                if (p < ap && (a.Ctx() != t.x() || a.Cty() != t.y()))
                {
                    GUTIL.Flooder().Done();
                    return t;
                }

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    if (WORLD.PATH().map.Can(t, d))
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance());
                }
            }
            GUTIL.Flooder().Done();
            return null;
        }

        public static double EnemyPower(Faction f, int tx, int ty)
        {
            double pow = 0;
            Region reg = WORLD.REGIONS().map.Get(tx, ty);
            if (reg != null)
            {
                if (Allies(f, reg.Faction()))
                {
                    pow += RD.DEFENSE().current.GetD(reg);
                }
                else if (Enemies(f, reg.Faction()))
                {
                    pow += RD.ATTACK().current.GetD(reg);
                }
            }
            return pow;
        }

        public static void Conquer(Side toSide, Side fromSide, int[] slaves, int[] loot)
        {
            Shipment s = null;

            Faction to = toSide.us[0].Faction();

            if (to != null && to.CapitolRegion() != null)
            {
                foreach (Race r in RACES.all)
                {
                    if (slaves[r.index] > 0)
                    {
                        if (s == null)
                        {
                            s = WORLD.ENTITIES().caravans.Create(fromSide.us[0].x(), fromSide.us[0].y(), to.CapitolRegion(), TRADE_TYPE.spoils);
                        }
                        if (s != null)
                        {
                            s.LoadAndReserve(TR.Get(r), slaves[r.index]);
                        }
                    }
                }
            }

            for (SideUnit u : toSide.us)
            {
                if (u.a != null && AD.men(null).Get(u.a) > 0)
                {
                    WArmy a = u.a;
                    for (ADSupply su : AD.supplies().all)
                    {
                        double n = su.needed(a);
                        if (n == 0)
                            continue;
                        int am = (int)Math.Ceiling((n * loot[su.res.index()] / needs[su.index()]));
                        am = CLAMP.i(am, 0, loot[su.res.index()]);
                        am = CLAMP.i(am, 0, (int)n);
                        su.current().inc(a, am);
                        loot[su.res.index()] -= am;
                    }
                }
            }

            if (to == null || to.CapitolRegion() == null)
                return;

            for (RESOURCE res : RESOURCES.ALL())
            {
                if (loot[res.index()] > 0)
                {
                    if (s == null)
                    {
                        s = WORLD.ENTITIES().caravans.Create(fromSide.us[0].x(), fromSide.us[0].y(), to.CapitolRegion(), TRADE_TYPE.spoils);
                    }
                    if (s != null)
                    {
                        s.LoadAndReserve(TR.Get(res), loot[res.index()]);
                    }
                }
            }
        }

        public static void Conquer(Side toSide, Side fromSide, int[] slaves, int[] loot)
        {
            Shipment s = null;

            Faction to = toSide.us[0].Faction();

            if (to != null && to.CapitolRegion() != null)
            {
                foreach (Race r in RACES.all)
                {
                    if (slaves[r.index] > 0)
                    {
                        if (s == null)
                        {
                            s = WORLD.ENTITIES().caravans.Create(fromSide.us[0].x(), fromSide.us[0].y(), to.CapitolRegion(), TRADE_TYPE.spoils);
                        }
                        if (s != null)
                        {
                            s.LoadAndReserve(TR.Get(r), slaves[r.index]);
                        }
                    }
                }
            }

            for (SideUnit u : toSide.us)
            {
                if (u.a != null && AD.men(null).Get(u.a) > 0)
                {
                    WArmy a = u.a;
                    for (ADSupply su : AD.supplies().all)
                    {
                        double n = su.needed(a);
                        if (n == 0)
                            continue;
                        int am = (int)Math.Ceiling((n * loot[su.res.index()] / needs[su.index()]));
                        am = CLAMP.i(am, 0, loot[su.res.index()]);
                        am = CLAMP.i(am, 0, (int)n);
                        su.current().inc(a, am);
                        loot[su.res.index()] -= am;
                    }
                }
            }

            if (to == null || to.CapitolRegion() == null)
                return;

            for (RESOURCE res : RESOURCES.ALL())
            {
                if (loot[res.index()] > 0)
                {
                    if (s == null)
                    {
                        s = WORLD.ENTITIES().caravans.Create(fromSide.us[0].x(), fromSide.us[0].y(), to.CapitolRegion(), TRADE_TYPE.spoils);
                    }
                    if (s != null)
                    {
                        s.LoadAndReserve(TR.Get(res), loot[res.index()]);
                    }
                }
            }
        }
    }
}