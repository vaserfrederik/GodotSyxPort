using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.GAME;
using game.boosting;
using game.faction.FACTIONS;
using game.faction.player;
using game.time;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.stats.POP;
using settlement.stats.standing;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using util.data.INT;
using util.gui.misc;
using util.info;
using util.text;
using util.updating;
using world;

public class Immigration
{
    public const int MAX_POPULATION = ENTETIES.MAX;

    private readonly Immigrator[] imms;
    private bool killall = false;
    private int killAllI = 0;

    public Immigration()
    {
        imms = new Immigrator[Race.All.Count];
        for (int i = 0; i < imms.Length; i++)
        {
            imms[i] = new Immigrator(Race.All[i]);
        }
    }

    public void Update(double ds)
    {
        foreach (var imm in imms)
        {
            imm.Update(ds);
        }
    }

    public int Wanted(Race race)
    {
        return imms[race.Index].Wanted();
    }

    public int Admitted(Race race)
    {
        return imms[race.Index].Admitted();
    }

    public double SecondsTillNext(Race race)
    {
        return imms[race.Index].SecondsTillNext();
    }

    public bool ShouldEmmigrate(Race race)
    {
        return imms[race.Index].ShouldEmmigrate();
    }

    public void Save(FileWriter file)
    {
        foreach (var imm in imms)
        {
            imm.Save(file);
        }
    }

    public void Load(FileReader file)
    {
        foreach (var imm in imms)
        {
            imm.Load(file);
        }
    }

    public void Clear()
    {
        foreach (var imm in imms)
        {
            imm.Clear();
        }
    }

    public void HoverImmigrants(GUI_BOX box, HCLASS_RACE pop)
    {
        if (pop == null)
            return;
        GBox b = (GBox)box;

        if (pop.cl != HCLASSES.CITIZEN())
        {
            b.Warn(¤¤noImmi);
            return;
        }

        Race r = pop.race;
        if (r == null)
            return;

        StandingCitizen st = STANDINGS.CITIZEN();

        b.Text(¤¤immigrantD);
        b.NL(4);

        b.TextL(¤¤available);
        b.Tab(7);
        b.Add(GFORMAT.iBig(b.Text(), Wanted(r)));
        b.NL();

        b.TextL(¤¤admitted);
        b.Tab(7);
        b.Add(GFORMAT.iBig(b.Text(), Admitted(r)));
        b.NL();

        b.TextL(¤¤attracted);
        b.Tab(7);
        b.Add(GFORMAT.i(b.Text(), Math.Max(0, imms[r.Index].WantedUltimately())));
        b.NL();

        b.TextL(¤¤eta);
        b.Tab(7);
        b.Add(GFORMAT.i(b.Text(), (int)imms[r.Index].SecondsTillNext()));
        b.NL();

        b.TextL(st.happiness.Info().name);
        b.Tab(7);
        b.Add(GFORMAT.perc(b.Text(), st.happiness.GetD(r)));
        b.NL();

        if (WORLD.Camps().Available(r))
        {
            b.TextL(¤¤camp);
            b.Tab(7);
            b.Add(GFORMAT.iBig(b.Text(), WORLD.Camps().Current(FACTIONS.Player(), r)));
            b.NL();
        }
        else
        {
            BOOSTABLES.CIVICS().IMMIGRATION.Hover(box, pop, true);
            b.NL();
        }

        b.TextLL(¤¤autoAdmit);
        b.Tab(6);
        b.Add(GFORMAT.i(b.Text(), (int)imms[r.Index].AutoAdmit));
        b.NL();

        b.TextLL(Dic.¤¤Population);
        b.Tab(6);
        b.Add(GFORMAT.i(b.Text(), -POP.Tot(HCLASSES.CITIZEN(), r)));
        b.NL();

        b.TextLL(Dic.¤¤Inbound);
        b.Tab(6);
        b.Add(GFORMAT.i(b.Text(), -(POP.Next(HCLASSES.CITIZEN(), r) - POP.Tot(HCLASSES.CITIZEN(), r))));
        b.NL();

        b.TextLL(Dic.¤¤Total);
        b.Tab(6);
        b.Add(GFORMAT.iIncr(b.Text(), Math.Max(0, imms[r.Index].AutoAdmit - (POP.Next(HCLASSES.CITIZEN(), r)))));
        b.NL();

        b.NL();
        b.NL(8);
    }

    public void SetWanted(Race race, int am)
    {
        imms[race.Index].Timer = am;
    }

    private class Immigrator
    {
        public Race Race { get; }
        public double Timer { get; set; }
        public int AutoAdmit { get; set; }
        private double Emigrants { get; set; }

        public Immigrator(Race race)
        {
            Race = race;
        }

        public int Wanted()
        {
            return Math.Clamp((int)Timer, 0, WantedUltimately());
        }

        public int Admitted()
        {
            return (int)Emigrants;
        }

        public double SecondsTillNext()
        {
            double rem = 1.0 - (Timer - (int)Timer);
            double speed = Speed(WantedUltimately());
            if (speed == 0)
                return double.NaN;
            return rem / speed;
        }

        public void Update(double ds)
        {
            int wanted = WantedUltimately();

            if (wanted < 0)
            {
                Emigrants += -ds * wanted / (2.0 * TIME.SecondsPerDay());
                Timer = 0;
                return;
            }
            Emigrants = 0;

            Timer += Speed(wanted) * ds;

            Timer = Math.Clamp(Timer, 0, wanted);
            int a = Auto.Get();
            a -= POP.Next(HCLASSES.CITIZEN(), Race);
            int w = Wanted();

            if (a > 0 && w > 0)
            {
                int am = Math.Clamp(w, 0, a);
                SETTLEMENT.Entry.Add(Race, HTYPE.SUBJECT, am);
                Timer -= am;
            }
        }

        public bool ShouldEmmigrate()
        {
            if (Emigrants > 1)
            {
                Emigrants--;
                return true;
            }
            return false;
        }

        public void Save(FileWriter file)
        {
            file.D(Timer);
            file.I(AutoAdmit);
            file.D(Emigrants);
        }

        public void Load(FileReader file)
        {
            Timer = file.D();
            AutoAdmit = file.I();
            Emigrants = file.D();
        }

        public void Clear()
        {
            Timer = 0;
            AutoAdmit = 0;
        }

        private double Speed(int wanted)
        {
            if (WORLD.Camps().Available(Race))
            {
                return BOOSTABLES.CIVICS().IMMIGRATION.Get(HCLASS_RACE.clP(Race, HCLASSES.CITIZEN())) * WORLD.Camps().Current(FACTIONS.Player(), Race);
            }
            return BOOSTABLES.CIVICS().IMMIGRATION.Get(HCLASS_RACE.clP(Race, HCLASSES.CITIZEN()));
        }

        private int WantedUltimately()
        {
            double pop = StandingCitizen.ExpectationPop(HCLASSES.CITIZEN(), Race);

            if (WORLD.Camps().Available(Race))
            {
                return Math.Max(WORLD.Camps().Current(FACTIONS.Player(), Race) - POP.Next(HCLASSES.CITIZEN(), Race), 0);
            }

            if (pop == 0)
            {
                double hap = BOOSTABLES.BEHAVIOUR().HAPPI.Get(HCLASS_RACE.clP(Race, HCLASSES.CITIZEN()));
                return (int)Math.Ceiling(hap - 0.1);
            }

            double hap = BOOSTABLES.BEHAVIOUR().HAPPI.Get(HCLASS_RACE.clP(Race, HCLASSES.CITIZEN()));
            hap = Math.Clamp(hap, 0, 2);
            hap -= threshold;
            if (hap <= 0)
                return (int)(pop * hap / threshold);
            hap *= 0.5;
            double am = hap * Race.Population().Max * pop;
            if (am > 1)
            {
                double d = am / (am + pop);
                am = am * (1 - d) + d * Math.Pow(am, 1 / STANDINGS.CITIZEN().FullPow(Race));
            }

            int res = (int)Math.Ceiling(am);
            return Math.Max(res, 0);
        }

        public INTE Auto => new INTE
        {
            Get = () => Race.Population().Max == 0 ? ENTETIES.MAX : AutoAdmit,
            Min = 0,
            Max = ENTETIES.MAX,
            Set = t => AutoAdmit = t
        };
    }

    private static int GetImmigrants(Race r)
    {
        double pop = StandingCitizen.ExpectationPop(HCLASSES.CITIZEN(), r);

        if (WORLD.Camps().Available(r))
        {
            return Math.Max(WORLD.Camps().Current(FACTIONS.Player(), r) - POP.Next(HCLASSES.CITIZEN(), r), 0);
        }

        if (pop == 0)
        {
            double hap = BOOSTABLES.BEHAVIOUR().HAPPI.Get(HCLASS_RACE.clP(r, HCLASSES.CITIZEN()));
            return (int)Math.Ceiling(hap - 0.1);
        }

        double hap = BOOSTABLES.BEHAVIOUR().HAPPI.Get(HCLASS_RACE.clP(r, HCLASSES.CITIZEN()));
        hap = Math.Clamp(hap, 0, 2);
        hap -= threshold;
        if (hap <= 0)
            return (int)(pop * hap / threshold);
        hap *= 0.5;
        double am = hap * r.Population().Max * pop;
        if (am > 1)
        {
            double d = am / (am + pop);
            am = am * (1 - d) + d * Math.Pow(am, 1 / STANDINGS.CITIZEN().FullPow(r));
        }

        int res = (int)Math.Ceiling(am);
        return Math.Max(res, 0);
    }

    private static readonly double threshold = 0.5;
    private static readonly string ¤¤noImmi = "No Immigration";
    private static readonly string ¤¤immigrantD = "Immigration Details";
    private static readonly string ¤¤available = "Available";
    private static readonly string ¤¤admitted = "Admitted";
    private static readonly string ¤¤attracted = "Attracted";
    private static readonly string ¤¤eta = "ETA";
    private static readonly string ¤¤autoAdmit = "Auto Admit";
}