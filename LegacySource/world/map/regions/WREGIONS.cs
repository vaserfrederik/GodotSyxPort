using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.rendering;
using view.tool;
using world;
using world.map.regions.centre;

public sealed class WREGIONS : WorldResource
{
    public static readonly COLOR cNone = new ColorImp(100, 100, 100);
    public const int MAX = 1023;
    private readonly ArrayList<Region> areas = new ArrayList<Region>(MAX);
    private readonly ArrayList<Region> active = new ArrayList<Region>(MAX);
    public readonly RegionMap pmap = new RegionMap();
    public readonly Region player;
    public readonly MAP_OBJECT<Region> map;
    private readonly Bitmap2D edge;
    private readonly Bitmap2D besige;
    private readonly Bitmap2D ctile;

    public WREGIONS()
        : base("Regions", "REGIONS")
    {
        player = new Region(0);
        areas.add(player);
        for (int i = 1; i < MAX; i++)
        {
            Region r = new Region(i);
            areas.add(r);
        }
        active.add(areas);
        map = pmap;
        edge = new Bitmap2D(WORLD.TBOUNDS(), false);
        besige = new Bitmap2D(WORLD.TBOUNDS(), false);
        ctile = new Bitmap2D(WORLD.TBOUNDS(), false);
    }

    public Region getByIndex(int index)
    {
        return areas.get(index);
    }

    public LIST<Region> all()
    {
        return areas;
    }

    private readonly WorldResourceManager saver = new WorldResourceManager()
    {
        public void save(FilePutter file)
        {
            pmap.save(file);
            foreach (Region a in areas)
                a.save(file);
        }

        public void load(FileGetter file) throws IOException
        {
            pmap.load(file);
            foreach (Region a in areas)
                a.load(file);

            dirty = true;
            init();
        }

        public void clear()
        {
            pmap.clear();
            foreach (Region r in REGIONS().all())
                r.clear();

            active.clearSloppy();
            active.add(areas);
            WORLD.MINIMAP().repaint();
            dirty = true;
        }

        public LIST<PLACABLE> makePlacers(ToolManager tm)
        {
            return new Placer();
        }

        public void generate(ACTION loadPrint)
        {
            new Gen(loadPrint);
            validateInit(null);
        }

        public void validateInit(WorldError error)
        {
            new GenValidator(error);
            init();
        }
    };

    private void init()
    {
        edge.clear();
        foreach (COORDINATE c in WORLD.TBOUNDS())
        {
            Region r = map.get(c);
            foreach (DIR d in DIR.ALL)
            {
                if (WORLD.IN_BOUNDS(c, d) && map.get(c, d) != r)
                {
                    edge.set(c, true);
                    break;
                }
            }
        }

        besige.clear();
        ctile.clear();
        Rec bb = new Rec(WCentre.TILE_DIM + 2, WCentre.TILE_DIM + 2);
        foreach (Region reg in active)
        {
            bb.moveX1Y1(reg.cx(), reg.cy());
            bb.incr(-Math.Ceiling(WCentre.TILE_DIM / 2.0), -Math.Ceiling(WCentre.TILE_DIM / 2.0));
            foreach (COORDINATE c in bb)
            {
                if (WORLD.TBOUNDS().holdsPoint(c) && bb.isOnEdge(c.x(), c.y()))
                {
                    besige.set(c, true);
                }
            }

            foreach (DIR d in DIR.ALLC)
            {
                ctile.set(reg.cx(), reg.cy(), d, true);
            }
        }

        WORLD.FOW().setDirty();
        dirty = true;
    }

    public override WorldResourceManager saver()
    {
        return saver;
    }

    protected override void update(double ds, Profiler prof)
    {
    }

    private bool dirty = true;

    public LIST<Region> active()
    {
        if (dirty)
        {
            dirty = false;
            active.clearSloppy();
            foreach (Region r in areas)
            {
                if (r.info.area() > 0 && map.get(r.cx(), r.cy()) == r)
                {
                    active.add(r);
                }
            }
        }
        return active;
    }

    public MAP_BOOLEAN border()
    {
        return edge;
    }

    public MAP_BOOLEAN centreEdgeTile()
    {
        return besige;
    }

    public MAP_BOOLEAN centreTile()
    {
        return ctile;
    }

    public readonly MAP_BOOLEAN isCentre = new MAP_BOOLEAN()
    {
        private readonly int min = WCentre.TILE_DIM / 2;
        private readonly int max = WCentre.TILE_DIM / 2;

        public bool is(int tx, int ty)
        {
            Region r = map.get(tx, ty);
            if (r != null)
            {
                int dx = tx - r.info.cx();
                int dy = ty - r.info.cy();
                return dx >= -min && dx <= max && dy >= -min && dy <= max;
            }
            return false;
        }

        public bool is(int tile)
        {
            return is(tile % WORLD.TWIDTH(), tile / WORLD.TWIDTH());
        }
    };

    public readonly MAP_OBJECT<Region> cTile = new MAP_OBJECT<Region>()
    {
        public Region get(int tile)
        {
            int tx = tile % WORLD.TWIDTH();
            int ty = tile / WORLD.TWIDTH();
            return get(tx, ty);
        }

        public Region get(int tx, int ty)
        {
            Region r = map.get(tx, ty);
            if (r != null && r.cx() == tx && r.cy() == ty)
                return r;
            return null;
        }
    };

    public readonly MAP_OBJECT<Region> centre = new MAP_OBJECT<Region>()
    {
        public Region get(int tile)
        {
            int tx = tile % WORLD.TWIDTH();
            int ty = tile / WORLD.TWIDTH();
            return get(tx, ty);
        }

        public Region get(int tx, int ty)
        {
            const int min = WCentre.TILE_DIM / 2;
            const int max = WCentre.TILE_DIM - min;

            for (int dy = -min; dy < max; dy++)
            {
                for (int dx = -min; dx < max; dx++)
                {
                    Region r = map.get(tx + dx, ty + dy);
                    if (r != null && r.cx() == tx + dx && r.cy() == ty + dy)
                        return r;
                }
            }
            return null;
        }
    };

    public readonly MAP_OBJECT<Faction> faction = new MAP_OBJECT<Faction>()
    {
        public Faction get(int tile)
        {
            Region reg = map.get(tile);
            if (reg != null)
                return reg.faction();
            return null;
        }

        public Faction get(int tx, int ty)
        {
            if (!WORLD.IN_BOUNDS(tx, ty))
                return null;
            return get(tx + ty * WORLD.TWIDTH());
        }
    };

    public void renderBorders(Renderer r, RenderIterator it)
    {
        if (!border().is(it.tile()))
            return;

        Region a = map.get(it.tile());
        if (a != null)
        {
            int m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (!IN_BOUNDS(it.tx(), it.ty(), d))
                {
                    m |= d.mask();
                    continue;
                }
                if (faction.get(it.tx(), it.ty(), d) == a.faction())
                {
                    m |= d.mask();
                }
            }
            int c = 0;
            foreach (DIR d in DIR.NORTHO)
            {
                if (!IN_BOUNDS(it.tx(), it.ty(), d))
                {
                    continue;
                }
                if (faction.get(it.tx(), it.ty(), d) != a.faction() && faction.get(it.tx(), it.ty(), d.next(1)) == a.faction() && faction.get(it.tx(), it.ty(), d.next(-1)) == a.faction())
                {
                    c |= d.mask();
                }
            }
            if (m != 0x0F || c != 0)
            {
                if (a.faction() == null)
                    COLOR.WHITE35.bind();
                else
                    a.faction().banner().colorBG().bind();
                OPACITY.O50.bind();
                SPRITES.cons().BIG.outline.render(r, m, c, it.x(), it.y());
                OPACITY.O75.bind();
                SPRITES.cons().BIG.dashed_hollow.render(r, m, c, it.x(), it.y());
            }
            OPACITY.unbind();
        }
    }
}