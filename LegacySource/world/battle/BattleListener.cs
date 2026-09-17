using System;
using System.Collections.Generic;
using System.Text;
using game;
using game.faction;
using init.sprite.UI;
using snake2d.util.file;
using snake2d.util.sets;
using util.text;
using world;
using world.battle;
using world.entity.army;
using world.map.pathing;
using world.map.regions;

public abstract class BattleListener
{
    private static readonly string ¤¤siege = "{0} forces have taken control of {1}.";
    private static readonly string ¤¤battle = "An army of {0} defeated an army of {1} near {2}.";

    static BattleListener()
    {
        D.ts(typeof(BattleListener));
    }

    public static int changeI;
    static readonly ArrayListGrower<BattleListener> all = new ArrayListGrower<BattleListener>();

    static BattleListener()
    {
        new GameDisposable
        {
            protected override void Dispose()
            {
                all.Clear();
            }
        };
    }

    public BattleListener()
    {
        all.Add(this);
    }

    public abstract void battle(Faction a, bool victory, int losses, int kills, Faction against);
    public abstract void battle(WArmy a, bool victory, int losses, int kills, Faction against);
    public abstract void siege(WArmy attacker, Region reg);
    public abstract void siege(Faction attacker, Region reg);

    private static Bitmap1D map = new Bitmap1D(FACTIONS.MAX(), false);
    private static int[] ulosses = Alloc.ii(Math.Max(128, FACTIONS.MAX()));

    static void Notify(ResolverSide winner, ResolverSide looser)
    {
        int cas = 0;
        int loss = 0;

        for (int i = 0; i < winner.us.size(); i++)
            cas += winner.us.get(i).losses;
        for (int i = 0; i < looser.us.size(); i++)
            loss += looser.us.get(i).losses;

        Str.TMP.Clear().Add(¤¤battle);
        Str.TMP.Insert(0, FACTIONS.name(winner.us.get(0).unit.faction()));
        Str.TMP.Insert(1, FACTIONS.name(looser.us.get(0).unit.faction()));
        int tx = looser.us.get(0).unit.x();
        int ty = looser.us.get(0).unit.y();
        Region reg = WORLD.REGIONS().map.get(tx, ty);
        if (reg == null)
        {
            RegDist r = WORLD.PATH().regFinder.single(tx, ty, Treaty.DUMMY, WRegSel.DUMMY(null));
            if (r != null)
            {
                reg = r.reg;
            }
        }

        if (reg != null)
        {
            Str.TMP.Insert(2, reg.info.name());
        }
        WORLD.LOG().log(winner.us.get(0).unit.faction(), looser.us.get(0).unit.faction(), UI.icons().s.sword, Str.TMP, tx, ty);

        Noti(winner, true, loss, cas, looser.side);
        Noti(looser, false, cas, loss, winner.side);
    }

    static void Notify(Side side, Region reg)
    {
        map.Clear();
        for (SideUnit u : side.us)
        {
            if (u.a() != null)
            {
                foreach (BattleListener li in BattleListener.all)
                    li.siege(u.a(), reg);
            }
            if (u.faction() != null && !map.get(u.faction().index()))
            {
                map.set(u.faction().index(), true);

                foreach (BattleListener li in BattleListener.all)
                    li.siege(u.faction(), reg);
            }
        }

        Str.TMP.Clear().Add(¤¤siege);
        Str.TMP.Insert(0, FACTIONS.name(side.us.get(0).faction()));
        Str.TMP.Insert(1, reg.info.name());
        WORLD.LOG().log(side.us.get(0).faction(), reg.faction(), UI.icons().s.degrade, Str.TMP, reg.cx(), reg.cy());
    }

    private static void Noti(ResolverSide side, bool victory, int kills, int cas, Side against)
    {
        map.Clear();
        for (int i = 0; i < side.us.size(); i++)
        {
            if (side.us.get(i).unit.faction() != null)
            {
                int mi = side.us.get(i).unit.faction().index();
                if (!map.get(mi))
                {
                    ulosses[mi] = 0;
                    map.set(mi, true);
                }
                ulosses[side.us.get(i).unit.faction().index()] += side.us.get(i).losses;
            }
        }
        map.Clear();
        for (int i = 0; i < side.us.size(); i++)
        {
            if (side.us.get(i).unit.faction() != null)
            {
                int mi = side.us.get(i).unit.faction().index();
                if (!map.get(mi))
                {
                    map.set(mi, true);
                    foreach (BattleListener li in BattleListener.all)
                        li.battle(side.us.get(i).unit.faction(), victory, ulosses[mi], kills, against.us.get(0).faction());
                }
            }
        }

        for (int ui = 0; ui < side.us.size(); ui++)
        {
            if (side.us.get(ui).unit.a() != null)
            {
                foreach (BattleListener li in BattleListener.all)
                    li.battle(side.us.get(ui).unit.a(), victory, cas, kills, against.us.get(0).faction());
            }
        }
    }
}