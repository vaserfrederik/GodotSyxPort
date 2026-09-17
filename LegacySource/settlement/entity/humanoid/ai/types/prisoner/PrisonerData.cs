using System;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using snake2d.util.bit;
using util.data;

public static class PrisonerData
{
    static PrisonerData self;

    public readonly INT_OE<HAI> judged;
    public readonly INT_OE<HAI> stocked;
    readonly INT_OE<HAI> judgeWait;
    public readonly INT_OE<HAI> prisonReported;

    public readonly GETTER_TRANSE<HAI, PUNISHMENT> punishmentSet;
    public readonly INT_OE<HAI> prisonTimeLeft;

    public PrisonerData()
    {
        self = this;
        judged = new Wrap(new Bits(0b0000_0001), AIModules.data().byte1);
        judgeWait = new Wrap(new Bits(0b0000_0110), AIModules.data().byte1);
        prisonReported = new Wrap(new Bits(0b0000_1000), AIModules.data().byte1);
        stocked = new Wrap(new Bits(0b0011_0000), AIModules.data().byte1);

        punishmentSet = new GETTER_TRANSE<HAI, PUNISHMENT>()
        {
            Wrap pp = new Wrap(new Bits(0b1111_1111), AIModules.data().byte2),
            public PUNISHMENT get(HAI f)
            {
                int i = pp.get(f);
                if (i == 0)
                    return null;
                return CRIME_PUNISHMENTS.ALL().get(i - 1);
            },

            public void set(HAI f, PUNISHMENT t)
            {
                int i = t == null ? 0 : t.index() + 1;
                pp.set(f, i);
            }
        };
        prisonTimeLeft = new Wrap(new Bits(0b1111_1111), AIModules.data().byte3);
    }

    public HCLASS clas(Induvidual i)
    {
        if (STATS.LAW().prisonerType.get(i) == CRIMES.WAR())
            return HCLASSES.OTHER();
        if (STATS.POP().COUNT.arrive.get(i) == CAUSE_ARRIVES.EMANCIPATED())
            return HCLASSES.SLAVE();
        return HCLASSES.CITIZEN();
    }

    protected void init(Humanoid a, AIManager d)
    {
        AI.modules().coo(d).set(0, 0);
        AIModules.data().byte1.set(d, 0);
        AIModules.data().byte2.set(d, 0);
        prisonTimeLeft.set(d, AIModule_Prisoner.PRISON_DAYS + 1);
    }

    public void punish(Humanoid a, AIManager d, PUNISHMENT dec)
    {
        STATS.LAW().punish(a.indu(), dec);
    }

    private class Wrap : INT_OE<HAI>
    {
        private readonly Bits bits;
        private readonly INT_OE<AIManager> data;

        public Wrap(Bits bits, INT_OE<AIManager> data)
        {
            this.bits = bits;
            this.data = data;
        }

        public int get(HAI t)
        {
            return bits.get(data.get((AIManager)t));
        }

        public int min(HAI t)
        {
            return 0;
        }

        public int max(HAI t)
        {
            return bits.mask;
        }

        public void set(HAI t, int i)
        {
            int d = data.get((AIManager)t);
            d = bits.set(d, i);
            data.set((AIManager)t, d);
        }
    }
}