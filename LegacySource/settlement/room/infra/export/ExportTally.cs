using System;
using System.IO;
using settlement.room.infra.export;
using init.resources;
using settlement.main;
using snake2d.util.file;
using util.data;
using util.keymap;

public sealed class ExportTally
{
    private readonly RMapInt<RESOURCE> pAmount = new RMapInt<RESOURCE>(RESOURCES.map());
    private readonly RMapInt<RESOURCE> pCapacity = new RMapInt<RESOURCE>(RESOURCES.map());
    public readonly INT_O<RESOURCE> amount = pAmount;
    public readonly INT_O<RESOURCE> capacity = pCapacity;

    public ExportTally()
    {
    }

    private readonly SAVABLE saver = new SAVABLE
    {
        Save = (FilePutter file) =>
        {
            pAmount.Save(file);
            pCapacity.Save(file);
        },

        Load = (FileGetter file) =>
        {
            pAmount.Load(file);
            pCapacity.Load(file);
            pAmount.Clear();
            foreach (ExportInstance i in SETT.ROOMS().EXPORT.all())
            {
                if (i.Resource() != null)
                    pAmount.Inc(i.Resource(), i.Amount());
            }
        },

        Clear = () =>
        {
            pAmount.Clear();
            pCapacity.Clear();
        }
    };

    public void Inc(RESOURCE r, int amount, int capacity)
    {
        this.pAmount.Inc(r, amount);
        this.pCapacity.Inc(r, capacity);
    }
}