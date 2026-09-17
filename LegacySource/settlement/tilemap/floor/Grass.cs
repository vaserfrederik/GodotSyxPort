using System;
using System.IO;
using game.audio;
using game.debug;
using init.paths;
using settlement.main;
using settlement.tilemap;
using settlement.tilemap.ground;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.rendering;
using view.sett;
using view.tool;

namespace settlement.tilemap.floor
{
    public class Grass : TileMap.Resource
    {
        private readonly Bitsmap1D data = new Bitsmap1D(0, 4, TAREA);

        public readonly SoundRace clearSound = AUDIO.race("CLEAR_GRASS");
        public const int TYPES = 0x0F;
        private const double TYPESI = 1.0 / TYPES;

        private readonly GrassRenderer renderer;

        public Grass() : base()
        {
            renderer = new GrassRenderer(this);

            new ComposerThings.IInit(PATHS.SPRITE_SETTLEMENT_MAP().get("Grass"), 972, 390);

            var ppu = new PlacableMulti("Remove")
            {
                place = (tx, ty, area, type) =>
                {
                    data.set(tx + ty * TWIDTH, CLAMP.i(data.get(tx + ty * TWIDTH) - 1, 0, TYPES));
                },
                isPlacable = (tx, ty, area, type) => null
            };

            var pp = new PlacableMulti("Grass")
            {
                place = (tx, ty, area, type) =>
                {
                    data.set(tx + ty * TWIDTH, CLAMP.i(data.get(tx + ty * TWIDTH) + 1, 0, TYPES));
                },
                isPlacable = (tx, ty, area, type) => null,
                getUndo = () => ppu
            };

            IDebugPanelSett.add(pp);
        }

        public void grow(int tx, int ty)
        {
            grow(tx, ty, 1 + RND.rInt(2));
        }

        private readonly double[] treepenalty = new double[] { 0.1, 0.20, 0.20, 0.1 };

        public void grow(int tx, int ty, int amount)
        {
            int tile = tx + ty * TWIDTH;

            int b = growthMax(tx, ty);
            int c = data.get(tile);

            if (c < b)
            {
                c += amount;
                if (c > b)
                    c = b;
            }
            else if (c > b)
            {
                c -= amount;
                if (c < b)
                    c = b;
            }

            data.set(tile, c);
        }

        public int growthMax(int tx, int ty)
        {
            GroundType t = SETT.GROUND().MAP.get(tx, ty);
            double v = SETT.GROUND().MOISTURE_CURRENT.get(tx, ty);
            v *= t.vegitation;
            if (t == SETT.GROUND().types.FOREST)
                v *= 0.6;
            else if (t == SETT.GROUND().types.PASTURE)
                v *= 0.8;

            if (v > 0.4)
            {
                for (int i = 0; i < treepenalty.Length; i++)
                {
                    if (SETT.TERRAIN().TREES.isTree(tx, ty + i))
                    {
                        v = CLAMP.d(v - treepenalty[i], 0.4, v);
                    }
                }
            }

            int b = CLAMP.i((int)(TYPES * v), 0, TYPES);
            return b;
        }

        protected override void update(double ds, Profiler profiler)
        {
            renderer.update(ds);
        }

        private readonly OPACITY[] op = new OPACITY[TYPES];
        {
            for (int i = 0; i < TYPES; i++)
            {
                int p = (int)(127 * (i + 1.0) / TYPES);
                op[i] = new OpacityImp(p);
            }
        }

        public void render(double ds, Renderer r, RenderData data)
        {
            renderer.render(ds, r, data);
        }

        public void render(RenderIterator it)
        {
            renderer.render(it, CORE.renderer());
        }

        public COLOR color(int ran)
        {
            return renderer.color(ran);
        }

        protected override void save(FilePutter saveFile)
        {
            data.save(saveFile);
        }

        protected override void load(FileGetter saveFile)
        {
            data.load(saveFile);
        }

        protected override void clearAll()
        {
            data.clear();
        }

        public readonly MAP_DOUBLEE current = new MAP_DOUBLEE.DoubleMapImp(TWIDTH, THEIGHT)
        {
            get = (tile) => (double)data.get(tile) * TYPESI,
            set = (tile, value) =>
            {
                currentI.set(tile, (int)(value * TYPES));
                return this;
            }
        };

        public readonly MAP_INTE currentI = new MAP_INTE.INT_MAPEImp(TWIDTH, THEIGHT)
        {
            get = (tile) => data.get(tile),
            set = (tile, value) =>
            {
                value = CLAMP.i(value, 0, TYPES);
                data.set(tile, value);
                return this;
            }
        };
    }
}