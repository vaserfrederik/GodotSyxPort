using System;
using System.Collections.Generic;
using System.IO;
using static world.WORLD;
using game.debug;
using init.constant;
using init.paths;
using init.sprite.UI;
using init.type;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.rendering;
using util.spritecomposer;
using util.spritecomposer.ComposerThings;
using view.tool;
using world.map.terrain;

public class WorldForest : WorldTerrainResource
{
    private readonly Bitsmap1D data = new Bitsmap1D(0, 4, TAREA());
    private const int SET = 16;

    private const int max = 3;

    private const int colorA = 64;

    public readonly SPRITE icon;

    private readonly Sprites sprites = new Sprites();

    public WorldForest(WORLD m) : base(m)
    {
        icon = new SPRITE.Imp(Icon.L)
        {
            public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                int t = 16 * 2;
                COLOR.WHITE100.bind();
                sprites.bg.render(r, t, X1 - 1, X2 - 1, Y1 - 1, Y2 - 1);
                COLOR.BLACK.bind();
                sprites.bg.render(r, t, X1 + 1, X2 + 1, Y1 + 1, Y2 + 1);
                sprites.colors[0][0].bind();

                sprites.bg.render(r, t, X1, X2, Y1, Y2);
                sprites.sheet.render(r, t, X1, X2, Y1, Y2);
                COLOR.unbind();
            }
        };
    }

    public readonly MAP_DOUBLEE amount = new MAP_DOUBLEE()
    {
        private readonly double amI = 1.0 / max;

        public double get(int tile)
        {
            return data.get(tile) * amI;
        }

        public double get(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
                return get(tx + ty * TWIDTH());
            return 0;
        }

        public MAP_DOUBLEE set(int tile, double value)
        {
            data.set(tile, CLAMP.i((int)Math.Ceiling(value * max), 0, max));
            WORLD.changeTile(tile % TWIDTH(), tile / TWIDTH());
            return this;
        }

        public MAP_DOUBLEE set(int tx, int ty, double value)
        {
            if (IN_BOUNDS(tx, ty))
                set(tx + ty * TWIDTH(), value);
            return this;
        }
    };

    public readonly MAP_BOOLEAN is = new MAP_BOOLEAN()
    {
        public bool is(int tx, int ty)
        {
            return amount.get(tx, ty) > 0;
        }

        public bool is(int tile)
        {
            return amount.get(tile) > 0;
        }
    };

    public readonly MAP_BOOLEAN placable = new MAP_BOOLEAN()
    {
        public bool is(int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return false;

            if (WATER().coversTile.is(tx, ty))
                return false;
            return true;
        }

        public bool is(int tile)
        {
            return is(tile % TWIDTH(), tile / TWIDTH());
        }
    };

    protected override void save(FilePutter saveFile)
    {
        data.save(saveFile);
    }

    protected override void load(FileGetter saveFile)
    {
        data.load(saveFile);
    }

    protected override void clear()
    {
        data.setAll(0);
    }

    protected override void update(double ds, Profiler prof)
    {
    }

    private int[] cols = Alloc.ii(3);

    public void render(SPRITE_RENDERER r, ShadowBatch s, RenderData data)
    {
        for (CLIMATE z : CLIMATES.ALL())
        {
            cols[z.index()] = (int)((colorA - colorA / 4) + z.getPartOfYear() * colorA) % colorA;
        }

        int off = (C.SCALE * 24 - C.TILE_SIZE) / 2;
        int rMask = colorA - 1;

        RenderIterator it = data.onScreenTiles(1, 1, 1, 0);
        s.setHeight(4);
        s.setDistance2Ground(0);
        s.setSoft();
        while (it.has())
        {
            int t = this.data.get(it.tile());
            if (REGIONS().isCentre.is(it.tx(), it.ty()))
            {
                it.next();
                continue;
            }

            t = this.data.get(it.tile());
            t = CLAMP.i(t, 0, max);
            sprites.sheet.getTile(t).render(r, it.tx(), it.ty());

            it.next();
        }

        it = data.onScreenTiles(1, 1, 1, 0);
        while (it.has())
        {
            int t = this.data.get(it.tile());
            if (REGIONS().isCentre.is(it.tx(), it.ty()))
            {
                it.next();
                continue;
            }

            t = this.data.get(it.tile());
            t = CLAMP.i(t, 0, max);
            sprites.bg.getTile(t).render(r, it.tx(), it.ty());

            it.next();
        }
    }

    private class Sprites
    {
        public readonly TILE_SHEET bg;
        public readonly TILE_SHEET sheet;

        public Sprites() : base()
        {
            bg = new TILE_SHEET(PATH.get("Forest"), 972, 280);
            sheet = new TILE_SHEET();
        }
    }

    public override LIST<PLACABLE> placers(ToolManager tm)
    {
        ArrayListGrower<PLACABLE> placers = new ArrayListGrower<>();

        PLACABLE CLEAR = new PlacableMulti("clear forest", "", icon.twin(UI.icons().m.anti, DIR.C, 0))
        {
            public void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                amount.set(tx, ty, 0);
            }

            public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                return null;
            }
        };

        placers.add(new PlacableMulti("forest")
        {
            public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                return WorldForest.this.placable.is(tx, ty) ? null : "";
            }

            public void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                int i = tx + ty * TWIDTH();
                data.set(i, CLAMP.i(data.get(i) + 1, 0, max));
            }

            public PLACABLE getUndo()
            {
                return CLEAR;
            }

            public SPRITE getIcon()
            {
                return icon;
            }
        });

        placers.add(CLEAR);

        return placers;
    }
}