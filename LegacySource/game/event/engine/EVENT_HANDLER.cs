using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.event.actions;
using game.faction;
using game.time;
using init.sprite.UI;
using init.value;
using settlement.stats;
using snake2d;
using util.data;
using util.text;

public sealed class EVENT_HANDLER : GameResource
{
    private Event current = null;
    private Event previous = null;
    private EContext data = new EContext();

    private readonly Data[] datas;

    private EventMessage mess;

    private readonly KeyMap<bool> tags = new KeyMap<bool>();

    private Induvidual tmp = null;

    public EVENT_HANDLER() : base("ENGINE")
    {
        datas = new Data[Event.all.Count];
        for (int i = 0; i < datas.Length; i++)
            datas[i] = new Data(Event.all[i]);
        new UIEventDebug(this);

        GVALUES.INDU.push("NEW_ARRIVAL", Dic.¤¤newArrival, UI.icons().s.human, new BOOLEANO<Induvidual>()
        {
            public override bool is(Induvidual t)
            {
                return t == tmp;
            }
        });
    }

    protected override void save(FilePutter file)
    {
        if (current != null)
        {
            file.bool(true);
            file.i(current.allIndex);
            file.chars(current.key);
        }
        else
        {
            file.bool(false);
        }

        if (previous != null)
        {
            file.bool(true);
            file.i(previous.allIndex);
            file.chars(previous.key);
        }
        else
        {
            file.bool(false);
        }

        data.write(file);

        file.i(datas.Length);
        foreach (Event k in Event.all)
        {
            file.i(k.allIndex);
            file.chars(k.key);
            file.i(datas[k.allIndex].fired);
            file.d(datas[k.allIndex].acc);
            file.d(datas[k.allIndex].lastTime);
            file.bsE(datas[k.allIndex].choices);
        }

        file.i(tags.all().Count);
        foreach (string s in tags.keys())
        {
            file.chars(s);
            file.bool(tags.get(s));
        }

        if (mess != null)
        {
            file.bool(true);
            file.object(mess);
        }
        else
            file.bool(false);
    }

    protected override void load(FileGetter file) => throw new NotImplementedException();

    protected override void loadFail()
    {
        clear();
    }

    private void clear()
    {
        current = null;
        previous = null;
        foreach (Data d in datas)
        {
            d.acc = 0;
            d.fired = 0;
            d.lastTime = 0;
            d.upI = -1;
            Array.Fill(d.choices, (byte)0);
        }
    }

    protected override void update(double ds, Profiler prof)
    {
        if (current != null)
        {
            double s = timeElapsed();
            foreach (EventAction a in current.on_spawn)
                a.update(current, data, ds, s);

            foreach (ECondition e in current.aborters)
                if (e.request.passes(FACTIONS.player()))
                {
                    setActions(e.on_fulfill);
                    return;
                }

            if (s >= current.duration.seconds)
                if (current.condition != null && current.condition.request.passes(FACTIONS.player()))
                    setActions(current.condition.on_fulfill);
                else
                    setActions(current.duration.on_expire);

            return;
        }
        else
        {
            // TODO: Handle else case
        }
    }

    private void setActions(LIST<EventAction> actions)
    {
        Event e = current;
        foreach (EventAction a in actions)
            a.exe(current, data);

        if (e == current)
            set(null, false, false, false, false);
    }

    public double timeElapsed()
    {
        if (current != null)
            return TIME.currentSecond() - datas[current.allIndex].lastTime;
        return 0;
    }

    public void expire()
    {
        if (current != null)
            datas[current.allIndex].lastTime = TIME.currentSecond() - current.duration.seconds - 1;
    }

    public bool can(Event a)
    {
        if (!a.occurence.plockable.passes(FACTIONS.player()))
            return false;
        int ff = datas[a.allIndex].fired;
        if (ff >= a.occurence.maxSpawns)
            return false;

        if (TIME.currentSecond() - datas[a.allIndex].lastTime < a.occurence.onlyAfterTime)
            return false;

        if (!a.tags.can(tags))
            return false;

        return true;
    }

    public Event read(int index, string key)
    {
        if (index >= 0 && index < Event.all.Count && Event.all[index].key.Equals(key))
            return Event.all[index];
        return null;
    }

    public double acc(Event a) => datas[a.allIndex].acc;

    public void accInc(Event a)
    {
        if (can(a))
            datas[a.allIndex].acc += a.occurence.occurence();
    }

    public int occ(Event a) => datas[a.allIndex].fired;

    public bool trySet(Event e)
    {
        datas[e.allIndex].acc = 0;
        if (can(e) && data.init(e))
        {
            set(e, false, false, true, true);
            double m = 0;
            foreach (Data d in datas)
            {
                m = Math.Max(m, d.acc);
                //d.acc -= (int)d.acc;
            }
            if (m > 10000)
                foreach (Data d in datas)
                    d.acc /= 2;
            return true;
        }
        return false;
    }

    public void set(Event e, bool keepTime, bool keepInfo, bool clearContext, bool message)
    {
        if (clearContext || e == null)
            data.init(e);
        else if (e != null)
            data.initLight(e);

        if (!keepTime)
            datas[e.allIndex].lastTime = TIME.currentSecond();
        else
            datas[e.allIndex].lastTime = datas[current.allIndex].lastTime;
        if (!keepInfo)
            previous = current;

        current = e;
        foreach (string s in current.tags.adds)
            tags.putReplace(s, true);

        datas[e.allIndex].fired++;

        datas[e.allIndex].acc = 0;
        if (datas[e.allIndex].upI == GAME.updateI())
            throw new Errors.DataError("An event is creating an infinate loop! " + e.key);

        datas[e.allIndex].upI = GAME.updateI();

        if (message && current.info.messages.Length > 0)
        {
            this.mess = new EventMessage(e, data);
            this.mess.send();
        }
        else
            this.mess = null;

        foreach (EventAction a in e.on_spawn)
            a.exe(current, data);
    }

    public void setTmp(Event e)
    {
        EContext data = new EContext();
        data.init(e);

        datas[e.allIndex].lastTime = TIME.currentSecond();
        datas[e.allIndex].fired++;

        datas[e.allIndex].acc = 0;
        if (datas[e.allIndex].upI == GAME.updateI())
            throw new Errors.DataError("An event is creating an infinate loop! " + e.key);

        datas[e.allIndex].upI = GAME.updateI();

        if (e.info.messages.Length > 0)
            new EventMessage(e, data).send();

        foreach (EventAction a in e.on_spawn)
            a.exe(current, data);
    }

    public Event current() => current;

    public EventMessage mess() => mess;

    EContext context() => data;

    private class Data
    {
        public double acc;
        public double lastTime;
        public int fired;
        public int upI = -1;
        public readonly byte[] choices;

        public Data(Event e)
        {
            choices = Alloc.bb(e.choices.size());
        }
    }

    public CLICKABLE butt() => new Butt(this);

    public COLOR color(Induvidual in)
    {
        if (current != null)
        {
            if (STATS.EVENT().has(in))
                return data.colorIndu;
            return data.colorinduAll;
        }
        return null;
    }

    public CharSequence message(Induvidual in)
    {
        if (current != null && current.info.subject.Length > 0)
        {
            if (STATS.EVENT().has(in))
                return current.info.subject;
        }
        return null;
    }

    public bool shouldSet(Induvidual i)
    {
        tmp = i;
        if (current != null && current.selection.indu.filters.Count > 0 && data.indu.am < data.indu.max)
        {
            foreach (Lockable<Induvidual> l in current.selection.indu.filters)
                if (l.passes(i))
                {
                    data.indu.am++;
                    return true;
                }
        }
        tmp = null;
        return false;
    }

    public bool choiceHasBeenSelected(Event parent, int choice)
    {
        Data d = datas[parent.allIndex];
        if (choice >= 0 && choice < d.choices.Length)
            return d.choices[choice] > 0;
        return false;
    }

    void choiceSelect(Event parent, int choice)
    {
        Data d = datas[parent.allIndex];
        d.choices[choice]++;
    }
}