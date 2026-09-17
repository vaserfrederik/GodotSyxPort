using System;
using settlement.stats.event;
using game;
using settlement.stats;
using settlement.stats.stat;
using util.data.INT_O;

public class StatsEvent : StatCollection
{
    private readonly STATData stat;
    public bool hasChange = true;
    public INT_OE<Induvidual> mark;

    public StatsEvent(StatsInit init) : base(init, "EVENT", "", "")
    {
        stat = new STATData("EVENT", "EVENTD", init, init.count.new DataBit("EVENT_STATUS"));
        mark = init.count.new DataShort("EVENT_MARK");

        init.onArrival.add(new StatInitable
        {
            public void init(Induvidual h)
            {
                if (GAME.EVENT().shouldSet(h))
                {
                    stat.indu().set(h, 1);
                }
            }
        });

        init.upers.add(new StatUpdatable
        {
            public void update(double ds)
            {
                if (hasChange)
                {
                    
                }
            }
        });
    }

    public STAT stat()
    {
        return stat;
    }

    public bool has(Induvidual t)
    {
        return stat.indu().get(t) == 1;
    }

    public void set(Induvidual t, bool has)
    {
        stat.indu().set(t, has ? 1 : 0);
    }
}