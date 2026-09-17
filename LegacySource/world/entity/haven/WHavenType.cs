using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.faction;
using init.paths;
using init.race;
using init.type;
using init.value;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.spritecomposer;

public sealed class WHavenType : INDEXED
{
    private readonly int index;
    public readonly TILE_SHEET sheet;
    public readonly COLOR cMask;
    public readonly LIST<CharSequence> names;
    public readonly CharSequence sJoin;
    public readonly CharSequence sLeave;
    public readonly Race race;
    public readonly int popFrom;
    public readonly int popTo;
    public readonly double replenishMin;
    public readonly double replenishMax;
    public readonly Lockable<Faction> reqsFrom;
    public readonly Lockable<Faction> reqsTo;

    public readonly double[] climates;
    public readonly double[] terrains;

    private ArrayListGrower<Delta> deltas = new ArrayListGrower<Delta>();
    private ArrayListGrower<Gauge> gauges = new ArrayListGrower<Gauge>();

    public WHavenType(string key, LISTE<WHavenType> all, Json jdata, Json jtext, TILE_SHEET sheet) : base(all)
    {
        index = all.add(this);
        this.sheet = sheet;
        cMask = new ColorImp(jdata, "COLOR_MASK");
        names = new ArrayList(jtext.texts("NAMES", 1, 500));
        sJoin = jtext.text("JOIN");
        sLeave = jtext.text("LEAVE");

        race = RACES.map().get(jdata.value("RACE"), jdata);
        climates = CLIMATES.MAP().readFill(jdata, 1);
        terrains = TERRAINS.MAP().readFill(jdata, 100);
        reqsFrom = GVALUES.FACTION.LOCK.push("WORLD_CAMP_" + key, race.info.names, race.info.names, race.appearance().icon);
        reqsFrom.push("REQUIRES_MIN", jdata);
        reqsTo = GVALUES.FACTION.LOCK.push();
        reqsTo.push("REQUIRES_MAX", jdata);

        GVALUES.FACTION.new LockJson("REQUIRES_MIN", jdata)
        {
            public void callback(COMPARATOR comp, Value<Faction> value, string key, Json json)
            {
                add(true, comp, value, json.d(key));
            }
        };

        GVALUES.FACTION.new LockJson("REQUIRES_MAX", jdata)
        {
            public void callback(COMPARATOR comp, Value<Faction> value, string key, Json json)
            {
                add(false, comp, value, json.d(key));
            }
        };

        popFrom = jdata.i("CAMP_SIZE_FROM", 1, 1000);
        popTo = jdata.i("CAMP_SIZE_TO", popFrom, 1000);

        replenishMin = jdata.d("REPLENISH_PER_DAY_FROM");
        replenishMax = jdata.d("REPLENISH_PER_DAY_TO");
    }

    private void add(bool from, COMPARATOR comp, Value<Faction> value, double target)
    {
        foreach (Delta d in deltas)
        {
            if (d.comp == comp && d.value == value)
            {
                if (from)
                    d.from = target;
                else
                    d.to = target;
                return;
            }
        }

        Delta d = new Delta();
        d.comp = comp;
        d.value = value;
        if (from)
            d.from = target;
        else
            d.to = target;
        deltas.add(d);

        gauges.add(new Gauge(d));
    }

    public static LIST<WHavenType> types()
    {
        LinkedList<WHavenType> all = new LinkedList<WHavenType>();
        KeyMap<TILE_SHEET> sheets = new KeyMap<TILE_SHEET>();

        ResFolder f = PATHS.RACE().folder("worldcamp");

        foreach (string file in f.init.getFiles())
        {
            Json jdata = new Json(f.init.gets(file));
            Json jtext = new Json(f.text.gets(file));

            string ssprite = jdata.value("SPRITE");
            TILE_SHEET sheet = sheets.get(ssprite);
            if (sheet == null)
            {
                TILE_SHEET s = new ITileSheet(f.sprite.get(ssprite), 132, 126)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.init(0, 0, 1, 1, 2, 4, d.s24);
                        s.singles.paste(3, true);
                        return d.s24.saveGame();
                    }
                }.get();
                sheets.put(ssprite, s);
                sheet = s;
            }
            new WHavenType(file, all, jdata, jtext, sheet);
        }

        return new ArrayList(all);
    }

    public override int index()
    {
        return index;
    }

    private class Delta
    {
        public double from = double.NaN;
        public double to = double.NaN;
        public COMPARATOR comp;
        public Value<Faction> value;
    }

    public double amount()
    {
        if (!reqsFrom.passes(FACTIONS.player()))
            return 0;

        double d = 1.0;
        int am = 0;

        foreach (Delta de in deltas)
        {
            if (de.to == double.NaN)
                continue;

            double v = de.value.d.getD(FACTIONS.player());

            if (de.from == double.NaN)
            {
                //d += CLAMP.d(de.comp.progress(v, de.to), 0, 1);
            }
            else
            {
                double zero = CLAMP.d(de.comp.progress(de.from, de.to), 0, 1);
                if (zero >= 1)
                {
                    //d += 1;
                }
                else
                {
                    double delta = 1.0 - zero;
                    double vv = CLAMP.d(de.comp.progress(v, de.to), 0, 1);
                    vv -= zero;
                    vv /= delta;

                    vv = CLAMP.d(vv, 0, 1);
                    d += vv;
                }
            }
            am++;
        }

        if (am > 0)
            d /= am;

        d = 0.2 + 0.8 * ((int)Math.Round(d * 5) / 5.0);

        return d;
    }

    public void hoverProgress(GUI_BOX bb)
    {
        GBox b = (GBox)bb;
        foreach (Delta de in deltas)
        {
            if (de.to == double.NaN)
                continue;

            double v = de.value.d.getD(FACTIONS.player());

            b.text(de.value.name);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), v, 2));
            b.tab(8);
            b.add(b.text().add('/'));

            b.add(GFORMAT.f(b.text(), de.to, 2));
            b.NL();
        }

        foreach (Gauge de in gauges)
        {
            b.add(de);
            b.NL();
        }
    }

    public double progress()
    {
        double d = 0;
        int am = 0;
        foreach (Delta de in deltas)
        {
            am++;
            d += de.value.d.getD(FACTIONS.player());
        }

        return d / am;
    }

    private class Gauge : SPRITE.Imp
    {
        private readonly Delta de;

        public Gauge(Delta delta) : base(128, 16)
        {
            this.de = delta;
        }

        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            double vv = 0;
            double v = de.value.d.getD(FACTIONS.player());
            vv = de.comp.progress(v, de.to);
            vv = CLAMP.d(vv, 0, 1);
            GMeter.render(r, GMeter.C_BLUE, vv, X1, X2, Y1, Y2);
        }
    }
}