using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc.stockpile;
using game.faction.royalty;
using game.faction.trade;
using game.time;
using init.race;
using init.resources;
using init.trade;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util;
using world.army;
using world.region;
using world.region.pop;

public sealed class FactionNPC : Faction, BOOSTABLE_O
{
    private readonly ArrayListGrower<NPCResource> res = new ArrayListGrower<NPCResource>();

    public readonly Str nameIntro = new Str(64);
    private readonly NPCCourt court;
    private readonly FBanner banner;
    private readonly FResources stats;
    private readonly FCredits credits;
    public readonly NPCBonus bonus;
    public readonly NPCStockpile stockpile;
    public readonly NPCRequest request;
    private int iteration;
    public bool sanctified = false;

    public FactionNPC(LISTE<Faction> all, UpdaterNPC up) : base(all)
    {
        court = new NPCCourt(this, res);
        banner = new FBanner(this);
        stats = new FResources(4, TIME.Years())
        {
            public int GetAvailable(TRADABLE t) => (int)stockpile.Res(t).Amount();
        };
        credits = new FCredits(4, TIME.Years());
        bonus = new NPCBonus(this, res);
        stockpile = new NPCStockpile(this, res, credits);
        request = new NPCRequest(this);
    }

    public void Generate(RDRace pref, bool init)
    {
        court.Init();
        sanctified = false;
        if (pref == null)
        {
            pref = RD.RACES().All.Rnd();
            if (CapitolRegion() != null)
            {
                double pop = 0;
                foreach (RDRace r in RD.RACES().All)
                    pop += r.Pop.Growth(CapitolRegion()) / r.Race.Population().Max;
                pop *= RND.rFloat();
                foreach (RDRace r in RD.RACES().All)
                {
                    pop -= r.Pop.Growth(CapitolRegion()) / r.Race.Population().Max;
                    if (pop <= 0)
                    {
                        pref = r;
                        break;
                    }
                }
            }
        }

        nameIntro.Clear().Add(pref.Names.Intros.Next());
        name.Clear().Add(pref.Names.FNames.Next());
        NameFix(pref);
        Event = false;

        if (Realm().Capitol() != null)
        {
            Realm().Capitol().Info.Name().Clear().Add(name);
            iteration++;
            credits.Inc(-credits.GetD(), CTYPE.DIPLOMACY);

            foreach (NPCResource r in res)
            {
                r.Generate(pref, this, init);
            }
        }
    }

    private void NameFix(RDRace pref)
    {
        for (int d = 0; d < 100; d++)
        {
            for (int i = 0; i < FACTIONS.NPCs().Size; i++)
            {
                FactionNPC fo = FACTIONS.NPCs()[i];
                if (fo.IsActive() && fo != this && fo.Name.Equals(name))
                {
                    name.Clear().Add(pref.Names.FNames.Next());
                    break;
                }
            }
        }
    }

    public override Race Race() => court.Race();

    public override FBUYER Buyer(TRADABLE t) => stockpile.Res(t).Buyer;

    public override FSELLER Seller(TRADABLE t) => stockpile.Res(t).Seller;

    public NPCRes Res(TRADABLE t) => stockpile.Res(t);

    protected override void Save(FilePutter file)
    {
        nameIntro.Save(file);
        foreach (NPCResource r in res)
        {
            SAVABLE s = r.Saver();
            if (s != null)
                s.Save(file);
        }
        file.I(iteration);
        request.Save(file);
        file.Bool(sanctified);
        base.Save(file);
    }

    protected override void Load(FileGetter file)
    {
        nameIntro.Load(file);
        foreach (NPCResource r in res)
        {
            SAVABLE s = r.Saver();
            if (s != null)
                s.Load(file);
        }
        iteration = file.I();
        request.Load(file);
        sanctified = file.Bool();
        base.Load(file);
    }

    public override void Clear()
    {
        foreach (NPCResource r in res)
        {
            SAVABLE s = r.Saver();
            if (s != null)
                s.Clear();
        }
        iteration = 0;
        request.Clear();
        sanctified = false;
        base.Clear();
    }

    protected override void Update(double ds)
    {
        foreach (NPCResource r in res)
        {
            r.Update(this, ds);
        }
        request.Update();
        base.Update(ds);
    }

    public int GetWorkers(RESOURCE res) => 0;

    public override FBanner Banner() => banner;

    public override FCredits Credits() => credits;

    public override CharSequence RulerName() => court.King().Name;

    public NPCCourt Court() => court;

    public int Iteration() => iteration;

    public override FResources Res() => stats;

    public Royalty King() => court.King().Roy();

    public override double BoostableValue(BValue v) => v.VGet(this);

    public override double OffensivePower() => AD.Power().Get(this);

    public override int Citizens(Race race)
    {
        if (CapitolRegion() == null)
            return 0;
        if (race == null)
            return RD.RACES().Population.Get(CapitolRegion());

        RDRace r = RD.RACES().Get(race);
        if (r == null)
            return 0;
        return r.Pop.Get(CapitolRegion());
    }

    public void Debug(Debugger d)
    {
        GAME.Events().World.Dip.Debug(d, this);
        d.DebugObject(request);
        Race().KingMessage().Debug(d, this);
        DIP.Debug(d, this);
    }
}