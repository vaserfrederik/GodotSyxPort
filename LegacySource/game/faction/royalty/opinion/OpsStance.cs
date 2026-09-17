using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty;
using game.faction.royalty.opinion.ROpper;
using game.time;
using init.sprite.UI;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.text;
using world.army;
using world.battle;
using world.entity.army;
using world.map.regions;
using world.region;

public static class OpsStance
{
    private static readonly CharSequence ¤¤WarDec = "War Declaration";
    private static readonly CharSequence ¤¤WarDecD = "Wars that have been declared by you against this faction.";
    private static readonly CharSequence ¤¤WarD = "Time spent at war with this faction.";
    private static readonly CharSequence ¤¤agression = "Aggression";
    private static readonly CharSequence ¤¤agressionD = "Aggression";
    private static readonly CharSequence ¤¤joinW = "Mutual War";
    private static readonly CharSequence ¤¤joinWD = "Mutual War Description";
    private static readonly CharSequence ¤¤betrayal = "Betrayal";
    private static readonly CharSequence ¤¤betrayalD = "Betrayal Description";
    private static readonly CharSequence ¤¤betrayalH = "Betrayal Hover";
    private static readonly CharSequence ¤¤betrayalTime = "Betrayal Time";
    private static readonly CharSequence ¤¤betrayalBase = "Betrayal Base";
    private static readonly CharSequence ¤¤betrayalThis = "Betrayal This";
    private static readonly CharSequence ¤¤betrayalOthers = "Betrayal Others";
    private static readonly CharSequence ¤¤trespassing = "Trespassing";
    private static readonly CharSequence ¤¤trespassingD = "Trespassing Description";
    private static readonly CharSequence ¤¤raiding = "Raiding";
    private static readonly CharSequence ¤¤raidingD = "Raiding Description";

    private static ROpperDown agression;
    private static ROpperDown joint;
    private static ROpperDown war;
    private static ROpperDown peace;
    private static ROpperDown betrayal;
    private static ROpperDown raiding;
    private static ROpperDown trespassing;

    static OpsStance()
    {
        double year = 365 * 24; // Example value for year, adjust as needed

        agression = new ROpperDown(¤¤agression, ¤¤agressionD, UI.Icons.sword, 3, false, year * 16)
        {
            GetModifier = roy => BOOSTABLES.NOBLE.AGRESSION[roy]
        };

        joint = new ROpperDown(¤¤joinW, ¤¤joinWD, UI.Icons.sword, 3, false, year * 16)
        {
            GetModifier = roy => BOOSTABLES.NOBLE.AGRESSION[roy]
        };

        war = new ROpperDown(¤¤WarDec, ¤¤WarDecD, UI.Icons.sword, -8, false, year * 24);

        peace = new ROpperDown(Dic.¤¤peace, ¤¤peaceD, UI.Icons.sprout, 100, false, year * 4 * 100);

        betrayal = new ROpperDown(¤¤betrayal, ¤¤betrayalD, UI.Icons.sword, -10, false, year * 10 * 2)
        {
            GetModifier = roy => BOOSTABLES.NOBLE.HONOUR[roy]
        };

        raiding = new ROpperDown(¤¤raiding, ¤¤raidingD, UI.Icons.sword, -8, false, year * 24);

        trespassing = new ROpperDown(¤¤trespassing, ¤¤trespassingD, UI.Icons.sword, -4, false, year * 4);

        new BattleListener
        {
            Siege = (attacker, reg) =>
            {
                if (reg.Faction is FactionNPC ff && DIP.VASSAL.Is(ff))
                {
                    Royalty r = ff.Court.King.Roy;
                    vassal.Value.IncD(r, 0.25);
                }
            },
            Battle = (a, victory, losses, kills, against) =>
            {
                if (a != FACTIONS.Player)
                    return;

                if (!(against is FactionNPC f))
                    return;

                foreach (Faction o in DIP.WAR.All(f))
                {
                    if (o != FACTIONS.Player)
                    {
                        Royalty r = ((FactionNPC)o).Court.King.Roy;
                        joint.Value.IncD(r, (double)kills / AD.Men(null).Faction(f));
                    }
                }
            }
        };
    }

    public static double Betrayal(FactionNPC npc, DipStance newStance)
    {
        return Betrayal(npc, DIP.Get(npc), newStance);
    }

    public static double Betrayal(FactionNPC npc, DipStance old, DipStance newStance)
    {
        return BetrayalD(npc, old, newStance) * betrayal.To() * betrayal.GetModifier(npc.Court.King.Roy);
    }

    private static double BetrayalD(FactionNPC npc, DipStance old, DipStance newStance)
    {
        double dp = newStance.Loyalty - old.Loyalty;
        if (dp > 0)
        {
            return dp;
        }

        if (newStance == DIP.WAR())
        {
            dp *= 2;
            dp -= 1;
        }

        double time = DIP.SecondSinceStance(npc) / (TIME.SecondsPerDay() * 16.0 * 8);
        time = 1.0 - time;
        time = CLAMP.D(time, 0.01, 1.0);
        dp *= time;

        return dp;
    }

    public static void SetNewStance(FactionNPC npc, DipStance newStance, bool playerDidIt)
    {
        DipStance old = DIP.Get(npc, FACTIONS.Player);

        if (old == newStance)
            return;

        double betray = -BetrayalD(npc, old, newStance);
        if (betray > 0)
        {
            foreach (FactionNPC o in FACTIONS.NPCs())
            {
                double v = 0.25 * CLAMP.D(128.0 / RD.DIST.Distance(o), 0, 1);
                if (o == npc)
                    v = 1.0;
                foreach (Royalty r in o.Court.All())
                {
                    betrayal.Value.IncD(r, r.IsKing ? betray * v : betray * 0.25 * v);
                }
            }
        }

        if (old == DIP.WAR())
        {
            SignPeace();
        }

        newStance.Set(npc);

        if (newStance == DIP.WAR())
        {
            foreach (Royalty r in npc.Court.All())
            {
                war.Value.IncD(r, r.IsKing ? 1.0 : 0.25);
                joint.Value.SetD(r, 0);
                peace.Value.SetD(r, 0);
            }
            foreach (FactionNPC o in FACTIONS.NPCs())
            {
                if (o != npc)
                {
                    double v = 0.5 * CLAMP.D(1 - RD.DIST.Distance(npc) / 256.0, 0, 1);
                    foreach (Royalty r in o.Court.All())
                    {
                        agression.Value.IncD(r, r.IsKing ? v : 0.25 * v);
                    }
                }
            }
        }
    }

    private static void SignPeace()
    {
        foreach (Faction f in DIP.WAR.All(FACTIONS.Player))
        {
            ROPINION.SetOpinionValue((FactionNPC)f, peace, 0.5);
        }
    }

    public static void Raid(FactionNPC f, double time)
    {
        double inc = 0.1 + time * TIME.SecondsPerDayI();

        foreach (Royalty r in f.Court.All())
        {
            raiding.Value.IncD(r, r.IsKing ? inc : inc * 0.5);
        }
    }

    public static void TresPass(FactionNPC f, double time)
    {
        if (DIP.Get(f).Ally)
            return;

        double inc = 0.1 + time * TIME.SecondsPerDayI();

        foreach (Royalty r in f.Court.All())
        {
            trespassing.Value.IncD(r, r.IsKing ? inc : inc * 0.5);
        }
    }

    public static double TrustWorthiness(FactionNPC f)
    {
        return CLAMP.D(1.0 - betrayal.Value.GetD(f.King), 0, 1);
    }

    public static void BetrayalHover(GBox box, FactionNPC npc, DipStance stance)
    {
        box.Title(¤¤betrayal);
        box.Text(¤¤betrayalH);
        box.Sep();
        box.TextLL(¤¤betrayalTime);
        box.NL();
        double time = DIP.SecondSinceStance(npc) / (TIME.SecondsPerDay() * 16.0 * 8);
        time = 1.0 - time;
        time = CLAMP.D(time, 0, 1);
        box.Add(GFORMAT.Perc(box.Text(), time));
        box.Sep();

        box.TextLL(¤¤betrayalBase);
        box.NL();
        box.Add(GFORMAT.F0(box.Text(), Betrayal(npc, stance)));
        box.Sep();

        box.TextLL(¤¤betrayalThis);
        box.NL();
        double m = Betrayal(npc, stance) * betrayal.GetModifier(npc.King);
        box.Add(GFORMAT.F0(box.Text(), m));
        box.Sep();

        box.TextLL(¤¤betrayalOthers);
        box.NL();
        m = Betrayal(npc, stance) * 0.25; // Example multiplier, adjust as needed
        box.Add(GFORMAT.F0(box.Text(), m));
        box.Sep();
    }
}