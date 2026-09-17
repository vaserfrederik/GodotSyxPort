using System;
using System.Collections.Generic;
using System.IO;
using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.player;
using game.faction.royalty.opinion;
using game.time;
using init.sprite.UI;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;

class War
{
    private const double WAR_INTERVAL = 16 * TIME.secondsPerDay();
    private double warCount = 0;
    private FactionNPC warner = null;
    private double teachings = 0;

    public War()
    {
        BValue v = new BValue.BValuePlayerOnly
        {
            vGet = (f) => 0,
            vGetPlayer = (f) => teachings
        };

        BoosterValue bo = new BoosterValue(v, new BSourceInfo(WarMessages.¤¤teachings, UI.icons().s.book), -0.15, false);
        bo.add(BOOSTABLES.BEHAVIOUR().LOYALTY);
        bo.add(BOOSTABLES.BEHAVIOUR().SUBMISSION);
    }

    public void Save(FilePutter file)
    {
        file.d(warCount);
        file.i(warner == null ? -1 : warner.Index());
    }

    public void Load(FileGetter file)
    {
        warCount = file.d();
        int fi = file.i();
        warner = fi >= 0 ? (FactionNPC)FACTIONS.all().Get(fi) : null;
    }

    public void Clear()
    {
        warCount = 0;
        warner = null;
    }

    void UpdateAll(double ds)
    {
        teachings -= ds / (16.0 * TIME.secondsPerDay());
        if (teachings < 0)
            teachings = 0;

        if (SETT.INVADOR().Invading())
            return;
        if (DIP.Overlord(FACTIONS.player()) != null)
            return;

        if (DIP.WAR().Any(FACTIONS.player()))
        {
            warner = null;
            return;
        }

        DipWarPlayer w = DIP.WAR_PLAYER();

        if (warner != null)
        {
            if (!warner.IsActive())
            {
                warner = null;
                return;
            }
            if (warner.Request.Has())
                return;
            if (!w.Potential(warner))
            {
                warner = null;
                warCount = -WAR_INTERVAL;
                return;
            }
            Start(warner);
            warner = null;
        }

        double attack = 0;

        foreach (FactionNPC f in w.Potential())
        {
            attack += Math.Max(1 - ROPINION.Trust().Get(f), 0);
        }

        warCount += attack * ds;

        if (warCount < WAR_INTERVAL)
            return;

        warCount -= WAR_INTERVAL;

        attack *= RND.rFloat();

        foreach (FactionNPC f in w.Potential())
        {
            attack -= Math.Max(1 - ROPINION.Trust().Get(f), 0);

            if (attack <= 0)
            {
                warner = f;
                WarMessages.Warn(f);
                return;
            }
        }
    }

    bool UpdateDay(FactionNPC f)
    {
        if (SETT.INVADOR().Invading())
            return false;
        if (DIP.Overlord(FACTIONS.player()) != null)
            return false;
        if (!DIP.WAR().Any(FACTIONS.player()))
            return false;
        if (DIP.WAR().Is(f))
            return false;

        double maxAmount = FACTIONS.player().OffensivePower() * (1 + AD.stats().RepF().GetD(FACTIONS.player()) * 2);
        foreach (Faction ff in DIP.WAR().All(FACTIONS.player()))
            maxAmount -= ff.OffensivePower();

        if (maxAmount < 0)
            return false;

        if (DIP.WAR_PLAYER().Willing(f))
        {
            DIP.WAR().Set(f);
            WarMessages.Join(f);
            return true;
        }
        else if (DIP.WAR_PLAYER().Proxy(f))
        {
            ProxyMove(f);
        }

        return false;
    }

    private bool ProxyMove(FactionNPC f)
    {
        LIST<RegDist> proxies = WORLD.PATH().RegFinder.All(f, Treaty.FACTION_REACHABLE_NPC_TRADE, WRegSel.DUMMY());
        int am = 0;
        foreach (RegDist d in proxies)
        {
            if (d.Reg.Faction() != null && d.Reg.Faction() != f && DIP.WAR().Is(FACTIONS.player(), d.Reg.Faction()))
            {
                am++;
            }
        }
        if (am == 0)
            return false;
        am = RND.rInt(am);
        foreach (RegDist d in proxies)
        {
            if (d.Reg.Faction() != null && d.Reg.Faction() != f && DIP.WAR().Is(FACTIONS.player(), d.Reg.Faction()))
            {
                am--;
                if (am <= 0)
                {
                    int a = ProxyMove(f, (FactionNPC)d.Reg.Faction());
                    WarMessages.Proxy((FactionNPC)d.Reg.Faction(), f, a);
                    return true;
                }
            }
        }
        return false;
    }

    private int ProxyMove(FactionNPC fromFaction, FactionNPC toFaction)
    {
        for (int ai = 0; ai < fromFaction.Armies().All().Size(); ai++)
        {
            WArmy a = fromFaction.Armies().All().Get(ai);
            if (AD.Men(null).Get(a) > 50)
            {
                return ProxyMove(a, toFaction);
            }
        }
        return 0;
    }

    private int ProxyMove(WArmy fromArmy, FactionNPC toFaction)
    {
        int ii = RND.rInt(toFaction.Armies().All().Size());
        int am = 0;
        for (int ai = 0; ai < toFaction.Armies().All().Size(); ai++)
        {
            WArmy toArmy = toFaction.Armies().All().Get(ii + ai);
            am += ProxyMove(fromArmy, toArmy);
        }

        if (fromArmy.Divs().Size() > 0)
        {
            COORDINATE c = WORLD.PATH().Rnd(toFaction.CapitolRegion());
            WArmy to = WORLD.ENTITIES().Armies.Create(c.X(), c.Y(), toFaction);
            if (to != null)
                am += ProxyMove(fromArmy, to);
        }
        return am;
    }

    private int ProxyMove(WArmy fromArmy, WArmy toArmy)
    {
        int am = 0;
        while (fromArmy.Divs().Size() > 0 && toArmy.Divs().CanAdd())
        {
            ADDiv toMove = fromArmy.Divs().Get(fromArmy.Divs().Size() - 1);

            am += toMove.Men();
            toMove.Reassign(toArmy);
        }
        return am;
    }

    private void Start(FactionNPC f)
    {
        DipWarPlayer w = DIP.WAR_PLAYER();
        if (w.Willing(warner))
        {
            DIP.WAR().Set(f);
            WarMessages.Start(f);
        }
        else if (w.Potential(warner))
        {
            DIP.SecondSincestartInc(f, FACTIONS.player(), -0.5);

            switch (RND.rInt(3))
            {
                case 0:
                    bool any = false;
                    foreach (FactionNPC ff in RD.DIST().Neighs())
                    {
                        if (f != ff)
                        {
                            any = true;
                            double am = (RND.rFloat(3)) / (1 + BOOSTABLES.NOBLE().TOLERANCE.Get(ff.King().Induvidual));
                            ROPINION.OTHER().Poison(ff, am);
                        }
                    }
                    if (any)
                    {
                        WarMessages.Poision(f);
                        return;
                    }
                    break;

                case 1:
                    int aa = RND.rInt(FACTIONS.player().Realm().All().Size());

                    foreach (Region reg in FACTIONS.player().Realm().All())
                    {
                        aa--;
                        if (aa <= 0 && !reg.Capitol())
                        {
                            RD.OWNER().Affiliation.Set(reg, 0);
                            WarMessages.Ngo(f, reg);
                            return;
                        }
                    }
                    break;
            }

            teachings = 1;
            WarMessages.Teachings(f);
        }
    }
}