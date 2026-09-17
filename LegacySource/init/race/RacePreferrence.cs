using System;
using System.Collections.Generic;
using System.Linq;
using init.resources;
using init.sprite.UI;
using init.trade;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.infra.elderly;
using settlement.room.main;
using settlement.room.water.pool;
using settlement.stats;
using settlement.tilemap.floor.Floor;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;
using view.main;

public sealed class RacePreferrence
{
    private static readonly CharSequence ¤¤preference = "preference";
    static
    {
        D.ts(typeof(RacePreferrence));
    }

    public readonly LIST<ResG> food;
    public readonly LIST<ResG> drink;
    public readonly RBIT drinkMask;
    public readonly RBIT foodMask;
    private readonly double[] structure;
    private readonly double[] pools;
    private readonly double[] work;
    private readonly double[] others;
    private Json jj;
    private readonly REN_PREF hov;
    public Json worldBuildingOverride;
    public readonly LIST<ROOM_RESTHOME> resthomes;

    private readonly double[] priceCaps;
    private readonly double[] priceMuls;

    private readonly double[] crimeFreedom;
    private readonly double[] crimeLaw;
    private readonly double[] punishment;

    public readonly Race mostHated;

    public RacePreferrence(Json data, Race race)
    {
        food = new LIST<ResG>();
        drink = new LIST<ResG>();
        structure = new double[BUILDING_PREF.ALL().Count];
        pools = new double[ROOM_POOL.ALL().Count];
        work = new double[RoomEmploymentSimple.ALL().Count];
        others = new double[Race.ALL().Count];
        priceCaps = new double[TRADABLE.ALL().Count];
        priceMuls = new double[RESOURCE.ALL().Count];
        crimeFreedom = new double[CRIME.ALL().Count];
        crimeLaw = new double[CRIME.ALL().Count];
        punishment = new double[PUNISHMENT.ALL().Count];

        LoadData(data, race);
    }

    private void LoadData(Json data, Race race)
    {
        jj = data;

        // Load food and drink preferences
        // ...

        // Load structure preferences
        // ...

        // Load pool preferences
        // ...

        // Load work preferences
        // ...

        // Load other race preferences
        // ...

        // Load resource price multipliers and caps
        // ...

        // Determine most hated race
        // ...

        // Load crime freedom, law, and punishment preferences
        // ...

        worldBuildingOverride = data;
    }

    public static void Init()
    {
        // Initialize other race preferences
        // ...
    }

    public ResG PrefAllowedFood(Humanoid a)
    {
        RBIT b = STATS.FOOD().FetchMask(a);
        double ma = 0;
        foreach (ResG f in food)
        {
            if (b.Has(f.Resource))
                ma++;
        }
        if (ma == 0)
            return food.Rnd();
        ma *= RND.rFloat();
        foreach (ResG f in food)
        {
            if (b.Has(f.Resource))
            {
                if (ma <= 1)
                    return f;
                ma -= 1;
            }
        }
        return food.Rnd();
    }

    public double Structure(BUILDING_PREF p)
    {
        return structure[p.Index()];
    }

    public double Pool(ROOM_POOL p)
    {
        return pools[p.TypeIndex()];
    }

    public double GetWork(RoomEmploymentSimple e)
    {
        return work[e.EIndex()];
    }

    public double Race(Race race)
    {
        return others[race.Index];
    }

    public double PriceMul(TRADABLE res)
    {
        return priceMuls[res.Index()];
    }

    public double PriceCap(TRADABLE res)
    {
        return priceCaps[res.Index()];
    }

    public void HoverOther(GUI_BOX box)
    {
        GBox b = (GBox)box;
        b.TextLL(STATS.ENV().OTHERS.Info.Name);
        b.Add(hov);
    }

    public class REN_PREF : HoverableAbs
    {
        private readonly bool big;
        private readonly int dim;
        private readonly Race rr;

        public REN_PREF(Race rr, bool big, int width)
        {
            this.big = big;
            this.rr = rr;
            dim = big ? Icon.L + 8 : Icon.M + 6;

            int w = Math.Min(dim * (width / dim), dim * Race.ALL().Count);

            body.SetWidth(w);
            body.SetHeight(dim + Race.ALL().Count / (w / dim));
        }

        protected override void Render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            int x1 = body().x1();
            int y1 = body().y1();
            foreach (Race ra in Race.ALL())
            {
                if (ra == rr)
                    continue;
                ColorImp col = ColorImp.TMP;
                double l = rr.Pref().Race(ra);
                GCOLOR.UI().BadToGood(col, l);
                col.Render(r, x1 + 1, x1 + dim - 1, y1 + 1, y1 + dim - 1);
                col.ShadeSelf(0.5);
                col.RenderFrame(r, x1 + 1, x1 + dim - 1, y1 + 1, y1 + dim - 1, 0, 1);
                SPRITE s = big ? ra.Appearance().IconBig : ra.Appearance().Icon;
                s.RenderC(r, x1 + dim / 2, y1 + dim / 2);
                x1 += dim;
                if (x1 >= body.x2())
                {
                    x1 = body.x1();
                    y1 += dim;
                }
            }
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            int tx = VIEW.Mouse().x();
            int ty = VIEW.Mouse().y();
            int x1 = body().x1();
            int y1 = body().y1();
            foreach (Race ra in Race.ALL())
            {
                if (ra == rr)
                    continue;
                if (tx >= x1 && tx < x1 + dim && ty >= y1 && ty < y1 + dim)
                {
                    b.Title(ra.Info.Name);
                    b.TextLL(¤¤preference);
                    b.Tab(7);
                    b.Add(GFORMAT.Perc(b.Text(), rr.Pref().Race(ra)));
                    b.NL();
                }
                x1 += dim;
                if (x1 >= body.x2())
                {
                    x1 = body.x1();
                    y1 += dim;
                }
            }
        }
    }

    public double CrimeFreedom(CRIME c)
    {
        return crimeFreedom[c.Index()];
    }

    public double CrimeLaw(CRIME c)
    {
        return crimeLaw[c.Index()];
    }

    public double Punishment(PUNISHMENT c)
    {
        return punishment[c.Index()];
    }
}