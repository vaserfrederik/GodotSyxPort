using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using init.race.appearence;
using init.race.bio;
using init.race.home;
using init.resources;
using init.sprite.UI;
using settlement.stats;
using settlement.stats.equip;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.keymap;

public class Race : MAPPED
{
    public readonly BoostSpecs boosts;
    private KeyMap<LinkedList<BoostSpec>> bmap = null;
    private RacePreferrence pref;
    public readonly RaceInfo info;
    public readonly Physics physics;
    public readonly int index;
    public readonly bool playable;
    public readonly string key;

    private KingMessages kmess;
    private RaceStats data;
    private RaceServiceSorter service;
    private RacePopulation population;
    private RaceHome home;
    private Bio bio;
    TourismRace tourism;

    private RAppearence appearance;

    private static readonly LIST<RES_AMOUNT> rNo = new ArrayList<RES_AMOUNT>(0);

    private LIST<RES_AMOUNT> resources = rNo;
    private LIST<RES_AMOUNT> resourceGroom = rNo;

    public Race(string key, Json data, Json text, ArrayList<Race> list)
    {
        this.key = key;
        index = list.Add(this);

        info = new RaceInfo(data, text);
        playable = data.Bool("PLAYABLE");
        physics = new Physics(data);
        boosts = new BoostSpecs(info.names, new SPRITE.Imp(Icon.S)
        {
            public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                appearance.iconBig.Render(r, X1, X2, Y1, Y2);
            }
        }, false);

        boosts.Read(data, null);
    }

    void Expand(ExpandInit init) throw IOException
    {
        Json data = new Json(init.p.GetS(key));
        population = new RacePopulation(data);

        appearance = new RAppearence(this, data, init, physics.hitBoxsize());
        pref = new RacePreferrence(data, this);

        kmess = KingMessages.Make(data, init);

        this.data = new RaceStats(this, data);
        service = new RaceServiceSorter(this);

        double[] ds = RESOURCES.Map().ReadFill(data, 100000);
        ArrayList<RES_AMOUNT> resources = new ArrayList<>(ds.Length);
        foreach (RESOURCE r in RESOURCES.ALL())
        {
            if (ds[r.index()] > 0)
            {
                resources.Add(new RES_AMOUNT.Imp(r, (int)ds[r.index()]));
            }
        }
        this.resources = resources;

        if (data.Has("RESOURCE_GROOMING"))
            resourceGroom = RES_AMOUNT.Make(data.Json("RESOURCE_GROOMING"));
        this.home = new RaceHome(data.Value("HOME"));
        bio = new Bio(data, this);
        tourism = new TourismRace(data, this);

        if (data.Has("EQUIPMENT_NOT_ENABLED"))
        {
            foreach (Equip b in STATS.EQUIP().collAll.ReadMany("EQUIPMENT_NOT_ENABLED", data))
            {
                b.SetAllowed(this, false);
            }
        }

        if (data.Has("EQUIPMENT_ENABLED"))
        {
            foreach (Equip b in STATS.EQUIP().collAll.ReadMany("EQUIPMENT_ENABLED", data))
            {
                b.SetAllowed(this, true);
            }
        }
    }

    public RAppearence Appearance()
    {
        return appearance;
    }

    public RacePreferrence Pref()
    {
        return pref;
    }

    public RaceStats Stats()
    {
        return data;
    }

    public RaceServiceSorter Service()
    {
        return service;
    }

    public RacePopulation Population()
    {
        return population;
    }

    public RaceHome Home()
    {
        return home;
    }

    public Bio Bio()
    {
        return bio;
    }

    public override string ToString()
    {
        return "" + info.name + "#" + index;
    }

    public LIST<RES_AMOUNT> Resources()
    {
        return resources;
    }

    public LIST<RES_AMOUNT> ResourcesGroom()
    {
        return resourceGroom;
    }

    public override int Index()
    {
        return index;
    }

    public TourismRace Tourism()
    {
        return tourism;
    }

    public KingMessages KingMessage()
    {
        return kmess;
    }

    public override string Key()
    {
        return key;
    }

    private static ArrayList<BoostSpec> dummy = new ArrayList<>(0);

    public LIST<BoostSpec> All(Boostable bo)
    {
        if (bmap == null)
        {
            bmap = new KeyMap<LinkedList<BoostSpec>>();
            foreach (BoostSpec boost in boosts.All())
            {
                if (!bmap.ContainsKey(boost.boostable.key))
                    bmap.Put(boost.boostable.key, new LinkedList<>());
                bmap.Get(boost.boostable.key).Add(boost);
            }
        }

        if (bmap.ContainsKey(bo.key))
            return bmap.Get(bo.key);
        return dummy;
    }

    public double BValue(Boostable bo, double input, double add, double mul)
    {
        double padd = add > 0 ? add : 0;
        double sub = add < 0 ? add : 0;
        foreach (BoostSpec s in All(bo))
        {
            if (s.booster.isMul)
                mul *= s.booster.GetValue(input);
            else
            {
                double a = s.booster.GetValue(input);
                if (a < 0)
                    sub += a;
                else
                    padd += a;
            }
        }
        return CLAMP.d(padd * mul + sub, bo.minValue, double.MaxValue);
    }

    public double BValue(Boostable bo)
    {
        return BValue(bo, 1.0, bo.baseValue, 1.0);
    }
}