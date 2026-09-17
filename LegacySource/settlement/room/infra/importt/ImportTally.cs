using System;
using System.IO;
using init.resources;
using snake2d;
using snake2d.util.file;
using util.data;
using util.keymap;

public sealed class ImportTally
{
    private readonly RMapInt<RESOURCE> pAmount = new RMapInt<RESOURCE>(RESOURCES.map());
    private readonly RMapInt<RESOURCE> pCapacity = new RMapInt<RESOURCE>(RESOURCES.map());
    public readonly INT_O<RESOURCE> amount = pAmount;
    public readonly INT_O<RESOURCE> capacity = pCapacity;

    public void Debug(RESOURCE res)
    {
        LOG.Ln(res.name);
        LOG.Ln("am " + pAmount.Get(res));
        LOG.Ln("ca " + capacity.Get(res));
        LOG.Ln(SpaceForTribute(res));
        LOG.Ln();
    }

    public ImportTally()
    {
    }

    private readonly SAVABLE saver = new SAVABLE
    {
        Save = (file) =>
        {
        },
        Load = (file) =>
        {
            pAmount.Clear();
            pCapacity.Clear();
        },
        Clear = () =>
        {
            pAmount.Clear();
            pCapacity.Clear();
        }
    };

    public void Count(RESOURCE r, int amount, int capacity)
    {
        if (r != null)
        {
            this.pAmount.Inc(r, amount);
            this.pCapacity.Inc(r, capacity);
        }
    }

    public int SpaceForTribute(RESOURCE res)
    {
        int am = capacity.Get(res) - amount.Get(res);
        if (am < 0)
        {
            return 0;
        }
        return am;
    }
}