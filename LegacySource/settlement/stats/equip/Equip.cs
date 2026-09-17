using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using game.battle.div;
using game.boosting;
using game.faction;
using game.faction.player;
using game.time;
using init.paths;
using init.race;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.standing;
using settlement.stats.stat;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;
using world.map.regions;

public abstract class Equip : MAPPED, WearableResource
{
    public readonly string sTarget;
    public readonly INFO targetInfo;

    public readonly RESOURCE resource;
    private readonly int index;
    public readonly double wearPerYear;
    private readonly double wearRateI;
    public readonly int equipMax;
    public readonly int arrivalAmount;
    public readonly int targetDefault;
    public readonly string eKey;
    protected readonly STAT stat;
    private readonly INT_OE<Induvidual> counter;
    private readonly bool[] available;

    public Equip(string coll, string key, PATH path, LISTE<Equip> all, StatsInit init)
        : this(coll, key, path, all, init, null)
    {
    }

    public Equip(string coll, string key, PATH path, LISTE<Equip> all, StatsInit init, Json text)
    {
        Json data = new Json(path.gets(key));
        key = (coll + "_" + key).Replace("__", "_");
        eKey = key;
        index = all.add(this);
        resource = RESOURCES.map().read(data);
        wearPerYear = data.d("WEAR_RATE", 0, 100);
        equipMax = data.i("MAX_AMOUNT", 1, 15);
        arrivalAmount = data.i("ARRIVAL_AMOUNT", 0, equipMax);
        targetDefault = data.i("DEFAULT_TARGET");
        StandingDef standing = new StandingDef(data);

        wearRateI = wearPerYear / 16.0;
        sTarget = new Str(StatsEquip.¤¤Target).insert(0, resource.name).trim();
        targetInfo = new INFO(new Str(StatsEquip.¤¤Level).insert(0, resource.name).trim(), StatsEquip.¤¤Level_desc);

        stat = new STATData(key, init, init.count.new DataNibble(coll + "_" + key + "_MAX", equipMax), text == null ? new StatInfo(resource.name, resource.names, resource.desc) : new StatInfo(text));

        stat.standing = new StatStanding(stat, 0, standing);
        stat.info().setInt();
        counter = init.count.new DataByte(coll + "_" + key + "_COUNTER");

        push(stat.boosters, data);

        stat.info().icon = resource.icon();
        available = new bool[RACES.all().size()];
        Array.Fill(available, true);
    }

    public void setAllowed(Race race, bool allowed)
    {
        available[race.index] = allowed;
    }

    public bool allowed(Race race)
    {
        return available[race.index];
    }

    private void push(BoostSpecs boosters, Json data)
    {
        boosters.read(data, bvalue);
    }

    public void set(Induvidual t, int i)
    {
        int old = stat.indu().get(t);
        if (i != old)
        {
            stat.indu().set(t, CLAMP.i(i, 0, max(t)));
            if (t.player() && t.added())
            {
                FACTIONS.player().res().inc(resource, RTYPE.EQUIPPED, old - stat.indu().get(t));
            }
        }
    }

    public void inc(Induvidual t, int am)
    {
        set(t, stat().indu().get(t) + am);
    }

    public int get(Induvidual i)
    {
        return stat.indu().get(i);
    }

    public int index()
    {
        return index;
    }

    void update16(Humanoid h, int updateI, int updateR, bool day)
    {
        if (RND.rFloat() < wearRateI)
        {
            Induvidual i = h.indu();
            int am = stat.indu().get(i) - (counter.get(i) >> 4);
            if (am > 0)
                counter.inc(i, am);
        }
    }

    public int needed(Induvidual i)
    {
        int am = target(i) - get(i) + (counter.get(i) >> 4);
        if (am < 0)
        {
            wearOut(i);
            am = target(i) - get(i);
            if (am < 0)
            {
                int c = counter.get(i) & 0x0F;
                if (RND.rInt(16) < c)
                    stat.indu().inc(i, -1);
                counter.set(i, 0);
                return target(i) - get(i);
            }
        }
        return am;
    }

    public void wearOut(Induvidual i)
    {
        int c = counter.get(i);
        int am = c >> 4;
        c &= 0x0F;
        counter.set(i, c);
        if (am == 0)
            return;

        am = CLAMP.i(am, 0, stat.indu().get(i));
        stat.indu().inc(i, -am);
    }

    public STAT stat()
    {
        return stat;
    }

    public abstract double bValue(double equipped);

    public double wearRate()
    {
        return wearPerYear;
    }

    public RESOURCE resource(Induvidual i)
    {
        return resource;
    }

    public RESOURCE resource()
    {
        return resource;
    }

    public double wearPerYear(Induvidual i)
    {
        return wearPerYear;
    }

    protected void hoverP(GUI_BOX box)
    {
        GBox b = (GBox)box;
        box.title(resource.name);
        box.text(resource.desc);
        b.NL();
        b.textL(StatsEquip.¤¤Wear);
        b.tab(8);
        b.add(GFORMAT.f0(b.text(), -wearPerYear * 16 / TIME.years().bitConversion(TIME.days())));
        b.NL();
    }

    public void hover(GUI_BOX box)
    {
        hoverP(box);
        GBox b = (GBox)box;
        b.sep();
        stat.boosters.hover(b, 1.0, -1);
    }

    public void hover(GUI_BOX box, Div div)
    {
        hoverP(box);
        GBox b = (GBox)box;
        b.textLL(Dic.¤¤Equipped);
        b.tab(7);
        b.add(GFORMAT.fofkInv(b.text(), stat.div().getD(div) * equipMax, equipMax));

        b.sep();

        stat.boosters.hover(b, div);
    }

    public void hover(GUI_BOX box, HCLASS cl, Race r)
    {
        hoverP(box);
        GBox b = (GBox)box;
        b.textLL(Dic.¤¤Equipped);
        b.tab(7);
        b.add(GFORMAT.fofkInv(b.text(), stat.data(cl).getD(r) * equipMax, equipMax));

        b.sep();

        stat.boosters.hover(b, HCLASS_RACE.clP(r, cl));
    }

    public void hover(GUI_BOX box, Induvidual h)
    {
        hoverP(box);
        GBox b = (GBox)box;

        b.textLL(Dic.¤¤Equipped);
        b.tab(7);
        b.add(GFORMAT.iofkInv(b.text(), stat.indu().get(h), equipMax));

        b.sep();
        stat.boosters.hover(b, h);
    }

    public string eKey()
    {
        return eKey;
    }

    public override string ToString()
    {
        return eKey;
    }

    public readonly BValue bvalue = new BValue()
    {
        public double vGet(Player f)
        {
            return 0;
        }

        public double vGet(FactionNPC f)
        {
            return 0;
        }

        public double vGet(Region reg)
        {
            return vGet(reg.faction());
        }

        public double vGet(HCLASS_RACE t)
        {
            return Equip.this.bValue(stat.data(t.cl).getD(t.race));
        }

        public double vGet(Div div)
        {
            return Equip.this.bValue(stat.div().getD(div));
        }

        public double vGet(Induvidual indu)
        {
            return Equip.this.bValue(stat.indu().getD(indu));
        }
    };
}