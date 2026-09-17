using System;
using game.event.actions;
using game.event.engine;
using settlement.stats;
using snake2d.util.file;

class Amount
{
    public readonly CInt amount;
    public double rel = 0;
    public double perPerson = 0;
    public double abs = 0;

    public Amount(CInt amount)
    {
        this.amount = amount;
    }

    public void Read(Json json, int min)
    {
        rel = json.dTry("RELATIVE", min, 1000, 0);
        perPerson = json.dTry("PER_PERSON", min, 1000, 0);
        abs = json.dTry("AMOUNT", min, int.MaxValue, 0);
        json.checkUnused();
    }

    public void Set(Event eventObj, EContext c, int available)
    {
        int am = (int)(rel * available);
        am += perPerson * STATS.POP().POP.data().get(null);
        am += abs;
        amount.set(eventObj, c, am);
    }

    public void Inc(Event eventObj, EContext c, int available)
    {
        int am = (int)(rel * available);
        am += perPerson * STATS.POP().POP.data().get(null);
        am += abs;
        amount.set(eventObj, c, amount.get(eventObj, c) + am);
    }
}