using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.battle;
using game.boosting;
using game.faction;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats.stat;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;

[Serializable]
public sealed class Induvidual : HumanoidResource, BOOSTABLE_O
{
    private static readonly long serialVersionUID = 1L;
    private long[] data;
    private byte race;
    private byte type;

    private bool added = false;

    public Induvidual(HTYPE type, Race race)
    {
        this.race = (byte)race.Index;
        this.type = (byte)type.Index();

        if (type == HTYPES.SLAVE())
        {
            GAME.count().ENSLAVED.inc(1);
        }
        else if (type == HTYPES.PRISONER())
            STATS.LAW().prisonerType.set(this, CRIMES.WAR());
        STATS.get().construct(this);
    }

    private void ReadObject(ObjectInputStream inStream)
    {
        inStream.defaultReadObject();

        long[] oldData = data;
        data = new long[STATS.count().longCount()];
        STATS.count().loader().wash(this, oldData);
    }

    public Induvidual(FileGetter p) : base(p)
    {
        STATS.count().loader().load(this, p);

        race = (byte)RACES.map().loader().loadB(p, FACTIONS.player().race()).Index;
        type = (byte)HTYPES.MAP().loader().loadB(p, HTYPES.SUBJECT()).Index();
        added = p.bool();
        if (added)
        {
            STATS.get().add(this);
        }
    }

    public override void save(FilePutter p)
    {
        STATS.count().saver().save(this, p);
        RACES.map().saver().save(race(), p);
        HTYPES.MAP().saver().save(hType(), p);

        p.bool(added);
    }

    public void copyFrom(Induvidual other)
    {
        STATS.get().copy.copy(this, other);
    }

    public void copyFromHard(Induvidual other)
    {
        if (added)
            throw new Exception();

        STATS.POP().COUNT.reg(other, CAUSE_LEAVES.OTHER());

        STATS.get().remove(this);
        this.race = other.race;
        this.type = other.type;
        STATS.get().add(this);

        STATS.get().copy.copy(this, other);
    }

    protected override void add(Humanoid h, CAUSE_ARRIVE a)
    {
        if (added)
            return;
        added = true;
        foreach (Tuple<STAT, double> t in race().stats().arrivalStats())
        {
            t.a().indu().setD(this, CLAMP.d(RND.rFloat0(0.2) * t.b(), 0, 1));
        }
        STATS.get().add(this);

        STATS.POP().COUNT.reg(this, a);
    }

    protected override void cancel(Humanoid h)
    {
        if (!added)
            return;
        STATS.get().cancel(h);
        added = false;
    }

    public bool added()
    {
        return added;
    }

    public HTYPE hType()
    {
        return HTYPES.ALL().get(type & 0x0FF);
    }

    public HCLASS clas()
    {
        return HTYPES.ALL().get(type & 0x0FF).CLASS;
    }

    public HCLASS_RACE popCL()
    {
        return HTYPES.ALL().get(type & 0x0FF).CLASS.get(race());
    }

    public void hTypeSet(Humanoid h, HTYPE t, CAUSE_LEAVE leave, CAUSE_ARRIVE arr)
    {
        if (t != hType())
        {
            HTYPE old = hType();
            STATS.POP().COUNT.reg(h.indu(), leave);
            SETT.PATH().finders.entity.report(h, -1);
            STATS.WORK().EMPLOYED.set(h, null);
            STATS.HOME().dump(h);
            STATS.HOME().GETTER.set(h, null);
            STATS.BATTLE().ROUTING.indu().set(this, 0);
            Div d = STATS.BATTLE().DIV.get(h);
            if (!AI.modules().battle.has(t))
                STATS.BATTLE().DIV.set(h, null);

            STATS.get().remove(this);

            this.type = (byte)t.Index();
            STATS.get().add(this);

            SETT.PATH().finders.entity.report(h, 1);
            if (keepDiv(old) && keepDiv(t))
            {
                h.setDivision(d);
            }
            STATS.POP().COUNT.reg(h.indu(), arr);
            if (t == HTYPES.SLAVE())
            {
                GAME.count().ENSLAVED.inc(1);
            }
            else if (old == HTYPES.SLAVE() && t.CLASS.player)
                GAME.count().FREED_SLAVES.inc(1);
        }
    }

    public void raceSet(Humanoid h, Race race, CAUSE_LEAVE leave, CAUSE_ARRIVE arr)
    {
        if (this.race != race.Index())
        {
            STATS.POP().COUNT.reg(h.indu(), leave);
            SETT.PATH().finders.entity.report(h, -1);
            STATS.WORK().EMPLOYED.set(h, null);
            STATS.HOME().dump(h);
            STATS.HOME().GETTER.set(h, null);
            STATS.BATTLE().ROUTING.indu().set(this, 0);
            STATS.BATTLE().DIV.set(h, null);

            STATS.get().remove(this);
            this.race = (byte)race.Index();
            STATS.get().add(this);

            SETT.PATH().finders.entity.report(h, 1);
            STATS.POP().COUNT.reg(h.indu(), arr);
        }
    }

    private bool keepDiv(HTYPE t)
    {
        return t == HTYPES.STUDENT() || t == HTYPES.SUBJECT() || t == HTYPES.RECRUIT();
    }

    public Race race()
    {
        return RACES.all().get(race);
    }

    public bool i2sDead()
    {
        return false;
    }

    protected override void update(Humanoid h, int updateI, bool newDay)
    {
        STATS.update(h, updateI, newDay);
    }

    protected override void update(Humanoid h, double ds)
    {
        throw new Exception();
    }

    public bool player()
    {
        return clas().player;
    }

    public bool hostile()
    {
        return hType().isHostile();
    }

    public Army army()
    {
        return hType().isHostile() ? GAME.ARMIES().enemy() : GAME.ARMIES().player();
    }

    public Div division()
    {
        return STATS.BATTLE().DIV.get(this);
    }

    public Faction faction()
    {
        if (hType() != HTYPES.ENEMY())
            return FACTIONS.player();
        return FACTIONS.otherFaction();
    }

    public double boostableValue(BValue v)
    {
        return v.vGet(this);
    }
}