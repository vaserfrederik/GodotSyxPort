using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util.data;
using util.text;
using view.sett;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.crime;
using settlement.main;
using settlement.stats;
using init.type;
using init.sprite.UI;
using game;
using game.time;

public sealed class AIModule_Crime : AIModule
{
    public readonly AIPLAN theft = new Theft("CrimeTheft", this);
    public readonly AIPLAN murder = new Murder("CrimeMurder", this);
    public readonly AIPLAN vandal = new Vandalism("CrimeVandal", this);
    public readonly AIPLAN flash = new Flasher("crimeFlash", this);
    public readonly AIPLAN disrespect = new Disrespect("crimeDisres", this);
    public readonly AIPLAN speech = new Disrespect("crimeSpeech", this);
    public readonly AIPLAN serial = new SerialKiller("crimeSerial");

    private readonly INT_OE<AIManager> commitCrime = new wrap(new Bits(0b0000_0001));
    private readonly INT_OE<AIManager> criminal = new wrap(new Bits(0b0000_0010));
    
    private static readonly CharSequence ¤¤name = "Mischief";
    private static readonly CharSequence ¤¤desc = "Commit crimes";
    static
    {
        D.ts(AIModule_Crime.class);
    }

    private bool debug = false;

    public AIModule_Crime() : base(UI.icons().s.law, ¤¤name, ¤¤desc)
    {
        foreach (CRIME c in CRIMES.ALL())
            getPlan(c);
        IDebugPanelSett.add("CRIMES_TEST_TOGGLE", new ACTION
        {
            exe = () => debug = !debug
        });
    }

    public override AiPlanActivation getPlan(Humanoid a, AIManager d)
    {
        if (GAME.events().killer.theKiller() == a && GAME.events().killer.theKillerShouldKill())
            return serial.activate(a, d);

        CRIME crime = STATS.LAW().prisonerType.get(a.indu());

        if (crime == CRIMES.PERSECUTED(a.indu().clas()))
        {
            long m = 0;
            Induvidual i = a.indu();
            foreach (CRIME c in CRIMES.all(i.clas()))
            {
                if (c.isCriminal())
                    m += (long)(1024 * c.tyrrany(i.clas(), i.race()));
            }
            m *= RND.rFloat();
            foreach (CRIME c in CRIMES.all(i.clas()))
            {
                if (c.isCriminal())
                    m -= (long)(1024 * c.tyrrany(i.clas(), i.race()));
                if (m <= 0)
                {
                    STATS.LAW().prisonerType.set(i, c);
                    break;
                }
            }
        }

        return getPlan(crime).activate(a, d);
    }

    private AIPLAN getPlan(CRIME crime)
    {
        if (crime == CRIMES.THEFT() || crime == CRIMES.S_THEFT())
            return theft;
        if (crime == CRIMES.DISRESPECT() || crime == CRIMES.S_DISRESPECT())
            return disrespect;
        if (crime == CRIMES.FLASHING())
            return flash;
        if (crime == CRIMES.MURDER() || crime == CRIMES.S_MURDER())
            return murder;
        if (crime == CRIMES.VANDALISM())
            return vandal;

        return speech;
    }

    protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
    {
        if ((updateOfDay & 0b011) == 0 && STATS.MULTIPLIERS().PROSECUTION.markIs(a))
        {
            SETT.ROOMS().GUARD.reporter.reportCriminal(a);
        }

        if (a.indu().hType() == HTYPES.GUARD())
        {
            return;
        }

        if (STATS.LAW().getCurfew().isSetForADay())
        {
            commitCrime.set(d, 0);
        }
        else
        {
            if (debug)
            {
                commitCrime.setMax(d);
            }
            double r = BOOSTABLES.BEHAVIOUR().LAWFULNESS.get(a.indu());

            if (a.indu().clas() == HCLASSES.CITIZEN())
            {
                double pop = STATS.POP().POP.data(HCLASSES.CITIZEN()).get(null) + 1;
                double guards = STATS.POP().POP.type().get(HTYPE_RACE.get(a.race(), HTYPES.GUARD())) + 1;
                double dd = (pop + guards) / pop;
                r *= dd;
            }

            if (r < 0)
            {
                r = 0;
            }
            r *= 16 * 16 * 5;
            r += 16;

            if (RND.oneIn(r))
            {
                commitCrime.setMax(d);
            }
        }
    }

    void commitCrime(Humanoid a, AIManager d, bool notify, CRIME crime)
    {
        STATS.LAW().crimes.get(crime.index()).commit(a.indu());
        STATS.LAW().prisonerType.set(a.indu(), crime);

        if (notify && STATS.LAW().crimes.get(crime.index()).punishment(a.indu()) != CRIME_PUNISHMENTS.PARDON())
        {
            SETT.ROOMS().GUARD.reporter.reportCriminal(a);
            AIModule_Crime.notify(a);
        }
        commitCrime.set(d, 0);
        criminal.set(d, 1);
    }

    public bool catchPrisoner(Humanoid a)
    {
        if (STATS.MULTIPLIERS().PROSECUTION.markIs(a))
        {
            STATS.LAW().prisonerType.set(a.indu(), CRIMES.PERSECUTED(a.indu().clas()));
            CRIMES.PERSECUTED(a.indu().clas()).stat().commit(a.indu());
        }
        CRIME c = STATS.LAW().prisonerType.get(a.indu());

        c.stat().catchh(a.indu());
        return true;
    }

    public override int getPriority(Humanoid a, AIManager d)
    {
        if (GAME.events().killer.theKiller() == a)
        {
            if (GAME.events().killer.theKillerShouldKill())
            {
                return TIME.light().nightIs() ? 4 : 0;
            }
            return 0;
        }
        if (commitCrime.isMax(d))
        {
            return 6;
        }
        return 0;
    }

    public static void notify(Humanoid criminal)
    {
        foreach (ENTITY e in SETT.ENTITIES().getInProximity(criminal, 8))
        {
            if (e is Humanoid)
            {
                HEvent.Handler.notifyCrime((Humanoid)e, criminal);
            }
        }
    }

    public bool isCriminal(Humanoid a)
    {
        AIManager d = (AIManager)a.ai();
        if (a.indu().hostile())
            return true;
        if (STATS.MULTIPLIERS().PROSECUTION.markIs(a) && STATS.LAW().crimes.get(CRIMES.PERSECUTED(a.indu().clas()).index()).punishment(a.indu()) != CRIME_PUNISHMENTS.PARDON())
            return true;
        if (criminal.get(d) > 0 && STATS.LAW().crimes.get(STATS.LAW().prisonerType.get(a.indu()).index()).punishment(a.indu()) != CRIME_PUNISHMENTS.PARDON())
            return true;
        return false;
    }

    protected override void init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
    {
        commitCrime.set(d, 0);
        criminal.set(d, 0);
    }

    protected override void cancel(Humanoid a, AIManager d)
    {
        base.cancel(a, d);
    }

    private class wrap : INT_OE<AIManager>
    {
        private readonly Bits bits;

        public wrap(Bits bits)
        {
            this.bits = bits;
        }

        public int get(AIManager t)
        {
            return bits.get(AIModules.data().byte1.get(t));
        }

        public int min(AIManager t)
        {
            return 0;
        }

        public int max(AIManager t)
        {
            return bits.mask;
        }

        public void set(AIManager t, int i)
        {
            int d = bits.set(AIModules.data().byte1.get(t), i);
            AIModules.data().byte1.set(t, d);
        }
    }
}