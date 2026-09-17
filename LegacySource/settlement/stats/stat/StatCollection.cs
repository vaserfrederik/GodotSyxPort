using System;
using System.Collections.Generic;
using snake2d.util.sets;
using util.info;

public abstract class StatCollection : INDEXED
{
    public readonly string key;
    public readonly INFO info;
    private readonly int index;
    private readonly ArrayListGrower<STAT> all = new ArrayListGrower<STAT>();

    protected StatCollection(StatsInit init, string key, string name, string desc)
    {
        index = init.holders.add(this);
        init.init(key, this);
        this.key = key;
        this.info = new INFO(name, desc);
    }

    public LIST<STAT> all()
    {
        return all;
    }

    public override int index()
    {
        return index;
    }
}