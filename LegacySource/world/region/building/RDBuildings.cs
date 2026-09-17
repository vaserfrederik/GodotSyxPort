using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.boosting;
using game.faction;
using init.paths;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.keymap;
using world.map.regions;
using world.region;
using world.region.pop;

public sealed class RDBuildings
{
    public readonly LIST<RDBuilding> all;
    public readonly LIST<RDBuilding> sorted;
    public readonly LIST<RDBuildingCat> cats;

    public readonly RDBoostCache levelRoad;
    //public readonly RDBoostCache levelFarm;
    public readonly RDBoostCache levelMine;
    public readonly RDBoostCache levelWall;

    private readonly RDLevelsTmp tmp;

    public readonly RDBuildPoints costs = new RDBuildPoints();

    public RDBuildings(RDInit init) : this(init, false) { }

    public RDBuildings(RDInit init, bool loadFromFile) : base()
    {
        levelRoad = new RDBoostCache(init, "VISUAL_ROADS", "", "", null);
        //levelFarm = new RDBoostCache(init, "VISUAL_FARMS", "", "", null);
        levelMine = new RDBoostCache(init, "VISUAL_MINE", "", "", null);
        levelWall = new RDBoostCache(init, "VISUAL_WALL", "", "", null)
        {
            protected override double pget(Region reg)
            {
                if (reg == FACTIONS.player().capitolRegion())
                {
                    double am = 0;
                    foreach (TFortification f in SETT.TERRAIN().FORTIFICATIONS.all())
                    {
                        am += f.tile.count();
                    }
                    return CLAMP.d(am / (SETT.TWIDTH * 4.0), 0, 1);
                }
                return base.pget(reg);
            }
        };

        ResFolder f = PATHS.WORLD().folder("building");

        Tree<RDBuildingCat> sort = new Tree<RDBuildingCat>(f.init.folders().Length)
        {
            protected override bool isGreaterThan(RDBuildingCat current, RDBuildingCat cmp)
            {
                return current.order > cmp.order;
            }
        };

        LinkedList<RDBuilding> all = new LinkedList<RDBuilding>();

        Creator creator = new Creator(this);
        foreach (string k in f.init.folders())
        {
            sort.add(new RDBuildingCat(creator, all, init, k, f.folder(k)));
        }

        ArrayListGrower<RDBuildingCat> cats = new ArrayListGrower<RDBuildingCat>();
        while (sort.hasMore())
            cats.add(sort.pollSmallest());

        this.cats = cats;

        ArrayListGrower<RDBuilding> sorted = new ArrayListGrower<RDBuilding>();

        foreach (RDBuildingCat c in cats)
            foreach (RDBuilding b in c.all)
                sorted.add(b);
        this.sorted = sorted;

        this.all = new ArrayList<RDBuilding>(all);

        tmp = new RDLevelsTmp(all.size());

        init.points = costs;
    }

    public void init(RDInit init)
    {
        RMAP<RDBuilding> MAP = new RMAP<RDBuilding>("WORLD_BUILDING", all);

        foreach (RDRace rdrace in RD.RACES().all)
        {
            if (rdrace.race.pref().worldBuildingOverride == null)
                continue;

            Json json = rdrace.race.pref().worldBuildingOverride;
            rdrace.race.pref().worldBuildingOverride = null;

            MAP.new KJson("WORLD_BUILDING", json)
            {
                protected override void process(RDBuilding t, Json json, string key, bool isWeak)
                {
                    double v = json.d(key);

                    for (int i = 1; i < t.levels.size(); i++)
                    {
                        RDBuildingLevel l = t.levels.get(i);
                        for (int bi = 0; bi < l.local.all().size(); bi++)
                        {
                            BoostSpec sp = l.local.all().get(bi);

                            if (sp.boostable == rdrace.loyalty.target)
                            {
                                replace(l.local, bi, sp, v);
                            }
                            else if (sp.boostable == rdrace.pop.dtarget)
                            {
                                replace(l.local, bi, sp, v);
                            }
                            else if (sp.boostable == rdrace.pop.growth)
                            {
                                replace(l.local, bi, sp, v);
                            }
                        }
                    }
                }

                void replace(BoostSpecs l, int i, BoostSpec sp, double value)
                {
                    double from = sp.booster.from();
                    double to = sp.booster.to();
                    if (sp.booster.isMul)
                    {
                        from = (from - 1) * value + 1;
                        to = (to - 1) * value + 1;
                    }
                    else
                    {
                        from *= value;
                        to *= value;
                    }

                    RBooster nn = new RBooster(sp.booster.info, from, to, sp.booster.isMul)
                    {
                        public override double get(Region t)
                        {
                            return 1.0;
                        }
                    };

                    l.replace(i, nn, sp.boostable);
                }
            };
        }

        foreach (RDBuilding b in all)
        {
            b.connect(init);
        }
    }

    public void update()
    {
        if (tmp.active > 0)
        {
            tmp.active--;
        }
    }

    public bool isTmp()
    {
        return tmp.active > 0;
    }

    public RDLevelsTmp tmp(bool init, Region reg)
    {
        costs.setDirty();
        tmp.active = 2;
        tmp.reg = reg;
        if (init)
            tmp.init(reg);
        return tmp;
    }

    public RDLevelsTmp tmp()
    {
        return tmp;
    }
}