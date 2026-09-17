using System;
using System.Collections.Generic;
using game.boosting;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.standing;
using settlement.stats.util;
using snake2d.util.gui;
using snake2d.util.sets;
using util.data;
using util.gui.misc;

public abstract class STAT : INDEXED, SETT_STATISTICS
{
    private StatDecree decree;
    private readonly int index;
    protected readonly string key;
    protected readonly StatInfo info;
    public StatStanding standing;
    public readonly BoostSpecs boosters;

    protected STAT(string key, StatsInit init, StatInfo info)
    {
        index = init.stats.Add(this);
        init.coll.all.Add(this);
        if (key != null)
        {
            key = init.coll.key + "_" + key;
            key = key.Replace("__", "_");
        }
        this.key = key;
        if (info == null && key != null)
            info = new StatInfo(init.dText.json(key));
        if (key != null)
            init.statMap.Put(key, this);
        if (info != null)
            this.info = new StatInfo(info);
        else
        {
            this.info = new StatInfo("no use", "no use");
            this.info.SetMatters(false, false);
        }
        boosters = new BoostSpecs(info == null ? "" : info.name, UI.icons().s.human, true);
    }

    public abstract INT_OE<Induvidual> indu();

    public StatInfo info()
    {
        return info;
    }

    public StatStanding standing()
    {
        return standing;
    }

    public string key()
    {
        return key;
    }

    public void addDecree(StatDecree d)
    {
        this.decree = d;
    }

    public StatDecree decree()
    {
        return decree;
    }

    public bool hasIndu()
    {
        return false;
    }

    public int index()
    {
        return index;
    }

    public void hover(GUI_BOX text, HCLASS cl, Race type)
    {
        StatHoverer.hover(text, this);
        GBox b = (GBox)text;
        b.sep();
        StatHoverer.hover(text, this, cl, type);
        b.NL();
        if (boosters.all().Count > 0)
        {
            boosters.hover(text, HCLASS_RACE.clP(type, cl));
        }
    }

    public void hover(GUI_BOX text, Induvidual indu)
    {
        StatHoverer.hover(text, this);
        GBox b = (GBox)text;
        b.sep();
        StatHoverer.hover(text, this, indu);
        b.NL();
        if (boosters.all().Count > 0)
        {
            b.NL(8);
            boosters.hover(text, indu);
        }
    }
}