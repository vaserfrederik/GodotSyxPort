using System;
using settlement.tilemap.generator;
using static settlement.main.SETT;
using static settlement.main.SettlementGrid;
using static snake2d.util.datatypes.DIR;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;
using world;

class GeneratorOcean
{
    private readonly HeightMap height;
    private readonly FertilityTmp fer;
    private readonly double table;

    private readonly int MARGIN = (int)(QUAD_SIZE / 2.2);

    private readonly double MAX_VALUE = MARGIN;
    private readonly double ferValue = -3;

    public GeneratorOcean(CapitolArea area, GeneratorUtil util)
    {
        table = area.getWatertabe();
        TERRAIN().WATER.groundWaterSalt.Clear();
        this.fer = util.fer;
        this.height = util.height;

        GUTIL.flooder().Init(this);

        for (int i = 0; i < GRID.tiles().size(); i++)
        {
            COORDINATE c = area.ts().Get(i);
            SettlementGrid.Tile ut = GRID.tile(i);
            if (WORLD.WATER().OCEAN.is.is(c))
            {
                Add(ut.coo(DIR.C).x(), ut.coo(DIR.C).y());
                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.WATER().OCEAN.is.is(c, d))
                    {
                        Add(ut.coo(d).x(), ut.coo(d).y());
                    }
                }
            }
        }

        bool has = GUTIL.flooder().hasMore();
        
        GenerateWater(MAX_VALUE);

        GUTIL.flooder().Done();

        if (has)
            Smooth();
    }

    private void Smooth()
    {
        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            GUTIL.flooder().SetValue2(c, 0);
        }

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (SETT.TERRAIN().WATER.SHALLOW.is(c) && SETT.TERRAIN().WATER.groundWaterSalt.is(c))
            {
                Smooth(c);
            }
        }
    }
    
    private void Smooth(COORDINATE start)
    {
        if (GUTIL.flooder().GetValue(start) != 0)
            return;
        
        GUTIL.flooder().Init(this);
        GUTIL.flooder().PushSloppy(start, 0);
        
        int am = 0;
        
        while (GUTIL.flooder().hasMore())
        {
            PathTile t = GUTIL.flooder().PollSmallest();
            if (!SETT.TERRAIN().WATER.is.is(t))
            {
                GUTIL.flooder().Done();
                return;
            }
            GUTIL.flooder().SetValue2(t, 1);
            if (SETT.TERRAIN().WATER.SHALLOW.is(t) && SETT.TERRAIN().WATER.groundWaterSalt.is(t))
            {
                am++;
                
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.flooder().PushSmaller(t, d, t.GetValue() + 1);
                    }
                }
                
            }
        }
        GUTIL.flooder().Done();
        
        if (am > 100)
        {
            return;
        }
        
        GUTIL.flooder().Init(this);
        GUTIL.flooder().PushSloppy(start, 0);
        
        while (GUTIL.flooder().hasMore())
        {
            PathTile t = GUTIL.flooder().PollSmallest();
            if (SETT.TERRAIN().WATER.SHALLOW.is(t) && SETT.TERRAIN().WATER.groundWaterSalt.is(t))
            {
                SETT.TERRAIN().WATER.DEEP.placeRaw(t.x(), t.y());
                
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.flooder().PushSmaller(t, d, t.GetValue() + 1);
                    }
                }
                
            }
        }
        
        GUTIL.flooder().Done();
    }
    
    private void Place(int x, int y)
    {
        TERRAIN().WATER.DEEP.placeRaw(x, y);
        TERRAIN().WATER.groundWaterSalt.Set(x, y, true);
        fer.increment(x, y, ferValue);
        // height.set(x, y, 0);
    }

    private void Add(int x, int y)
    {
        PathTile t = GUTIL.flooder().PushSloppy(x, y, 0);
        if (t != null)
            t.SetValue2(0);
    }

    private void GenerateWater(double max)
    {
        Flooder f = GUTIL.flooder();

        while (f.hasMore())
        {
            PathTile t = f.pollSmallest();
            if (t.getValue() >= max)
            {
                break;
            }
            if (t.getValue() > 4.5 * max / 5)
            {
                TERRAIN().WATER.SHALLOW.placeRaw(t.x(), t.y());
                TERRAIN().WATER.groundWaterSalt.Set(t, true);
                fer.increment(t.x(), t.y(), ferValue);
            }
            else
            {
                Place(t.x(), t.y());
            }

            for (int i = 0; i < DIR.ALL.size(); i++)
            {
                int x = t.x() + DIR.ALL.get(i).x();
                int y = t.y() + DIR.ALL.get(i).y();

                if (IN_BOUNDS(x, y))
                {
                    double d = DIR.ALL.get(i).tileDistance();
                    double radius = t.getValue2() + d;
                    float h = (float)height.get(x, y);
                    double value = 0.8 * radius;
                    value += max * h * h * h;
                    if (value > max)
                        value = max;
                    if (h < table)
                        value = 0;
                    PathTile t2 = GUTIL.flooder().PushSloppy(x, y, (float)value);
                    if (t2 != null)
                        t2.SetValue2((float)radius);
                }
            }
        }

        double delta = 15.0;
        max += delta;

        while (f.hasMore())
        {
            PathTile t = f.pollSmallest();
            double v = t.getValue();
            if (v >= max)
            {
                break;
            }
            double dd = 1.0 - (v - max + delta) / delta;
            if (dd < 0)
                dd = 0;
            if (dd > 1)
                dd = 1;
            double fe = dd * ferValue * (0.7 + 0.3 * RND.rFloat());

            {
                double r = max - t.getValue2();
                if (r < 8)
                    TERRAIN().WATER.groundWaterSalt.Set(t, true);
            }

            fer.increment(t.x(), t.y(), fe);
            if (fer.get(t.x(), t.y()) < 0.1)
                SETT.GROUND().types.SAND.placeFixed(t.x(), t.y());
            
            if (RND.oneIn(100 - 95 * dd) && SETT.TERRAIN().NADA.is(t))
            {
                SETT.TERRAIN().DECOR_BEACH.placeRaw(t.x(), t.y());
            }

            for (int i = 0; i < DIR.ALL.size(); i++)
            {
                int x = t.x() + DIR.ALL.get(i).x();
                int y = t.y() + DIR.ALL.get(i).y();

                if (IN_BOUNDS(x, y))
                {
                    double d = DIR.ALL.get(i).tileDistance();
                    double radius = t.getValue2() + d;
                    float h = (float)height.get(x, y);
                    double value = 0.8 * radius;
                    value += max * h * h * h;
                    if (value > max)
                        value = max;
                    PathTile t2 = GUTIL.flooder().PushSloppy(x, y, (float)value);
                    if (t2 != null)
                        t2.SetValue2((float)radius);
                }
            }
        }
    }
}