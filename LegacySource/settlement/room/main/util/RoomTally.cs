using System;
using System.Collections.Generic;
using settlement.room.main;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;

public abstract class RoomTally
{
    private ArrayListGrower<TallyEntry> all = new ArrayListGrower<TallyEntry>();

    public class TallyEntry : INT_OE<RoomInstance>
    {
        public readonly CharSequence name;
        public readonly int index;
        private readonly IntImp ptot = new IntImp();
        public readonly INT total = ptot;
        public readonly TallyEntry tottot;

        TallyEntry(CharSequence name)
            : this(name, null)
        {
        }

        TallyEntry(CharSequence name, TallyEntry total)
        {
            index = all.Add(this);
            this.name = name;
            this.tottot = total;
        }

        public override int Min(RoomInstance t)
        {
            return 0;
        }

        public override int Max(RoomInstance t)
        {
            return int.MaxValue;
        }

        public override int Get(RoomInstance t)
        {
            return data(t)[index];
        }

        public override void Set(RoomInstance t, int amount)
        {
            int[] data = data(t);
            if (tottot != null)
                tottot.inc(t, -data[index]);
            ptot.inc(-data[index]);
            data[index] = amount;
            if (tottot != null)
                tottot.inc(t, data[index]);
            ptot.inc(data[index]);
        }
    }

    public TallyEntry Make(CharSequence name)
    {
        return new TallyEntry(name);
    }

    public TallyEntry Make(CharSequence name, TallyEntry tot)
    {
        return new TallyEntry(name, tot);
    }

    protected abstract int[] data(RoomInstance ins);

    public int[] MakeInstanceData()
    {
        return Alloc.ii(all.Size());
    }

    public void Clear()
    {
        foreach (TallyEntry e in all)
        {
            e.ptot.set(0);
        }
    }

    public void Load(RoomInstance ins)
    {
        int[] dd = data(ins);
        for (int i = 0; i < all.Size(); i++)
        {
            all.Get(i).ptot.inc(dd[i]);
        }
    }
}