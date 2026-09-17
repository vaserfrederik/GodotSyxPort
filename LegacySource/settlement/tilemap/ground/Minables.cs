using System;
using System.IO;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.keymap;
using view.sett;
using view.tool;

namespace settlement.tilemap.ground
{
    public class Minables
    {
        private long[] amounts;
        private Bitsmap1D types;
        private Bitsmap1D amount;
        private Bitsmap1D richness;

        public Minables()
        {
            amounts = new long[RESOURCES.minables().all().Count];
            types = new Bitsmap1D(0, 5, SETT.TAREA);
            amount = new Bitsmap1D(0, 6, SETT.TAREA);
            richness = new Bitsmap1D(0, 1, SETT.TAREA);

            PLACABLE undo = new PlacableMulti("minerals remove")
            {
                Place = (tx, ty, a, t) => getter.set(tx, ty, null),
                IsPlacable = (tx, ty, a, t) => getter.is(tx, ty) ? null : ""
            };
            IDebugPanelSett.add(undo);

            foreach (Minable m in RESOURCES.minables().all())
            {
                IDebugPanelSett.add(new PlacableMulti("mineral " + m.resource.name)
                {
                    Place = (tx, ty, a, t) =>
                    {
                        if (getter.is(tx, ty, m))
                            amountD.increment(tx, ty, 0.1);
                        else
                            getter.set(tx, ty, m);
                    },
                    IsPlacable = (tx, ty, a, t) =>
                    {
                        if (!SETT.IN_BOUNDS(tx, ty))
                            return "";
                        if (SETT.PATH().solidity.is(tx, ty))
                            return "";
                        if (SETT.ROOMS().map.is(tx, ty))
                            return "";
                        return null;
                    },
                    Name = () => "mineral " + m.resource.name,
                    GetIcon = () => m.resource.icon(),
                    GetUndo = () => undo
                });
            }
        }

        public readonly MAP_OBJECTE<Minable> getter = new MAP_OBJECTE<Minable>()
        {
            Get = tile => amount.get(tile) == 0 ? null : RESOURCES.minables().getAt(types.get(tile)),
            GetXY = (tx, ty) => Get(tx + ty * SETT.TWIDTH),
            Set = (tile, object_) =>
            {
                Minable old = Get(tile);

                if (object_ == null)
                    amount.set(tile, 0);
                else
                {
                    types.set(tile, object_.index);
                    amount.set(tile, 1);
                }

                if (object_ != old)
                    SETT.TILE_MAP().miniCUpdate(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            },
            SetXY = (tx, ty, object_) =>
            {
                if (SETT.IN_BOUNDS(tx, ty))
                    Set(tx + ty * SETT.TWIDTH, object_);
            }
        };

        public readonly MAP_INTE amountInt = new MAP_INTE()
        {
            GetXY = (tx, ty) => IN_BOUNDS(tx, ty) ? Get(tx + ty * SETT.TWIDTH) : 0,
            Get = tile => amount.get(tile),
            SetXY = (tx, ty, value) =>
            {
                if (IN_BOUNDS(tx, ty))
                    Set(tx + ty * SETT.TWIDTH, value);
                return this;
            },
            Set = (tile, value) =>
            {
                Minable old = getter.get(tile);
                if (value < 0)
                    value = 0;
                if (value > amount.maxValue())
                    value = amount.maxValue();

                amount.set(tile, value);
                if (getter.get(tile) != old)
                    SETT.TILE_MAP().miniCUpdate(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
                return this;
            }
        };

        public readonly MAP_DOUBLEE amountD = new MAP_DOUBLEE()
        {
            GetXY = (tx, ty) => IN_BOUNDS(tx, ty) ? Get(tx + ty * SETT.TWIDTH) : 0,
            Get = tile => amount.get(tile) * 1.0 / amount.maxValue(),
            Set = (tile, value) =>
            {
                amount.set(tile, (int)(value * amount.maxValue()));
                return this;
            },
            SetXY = (tx, ty, value) => Set(tx + ty * SETT.TWIDTH, value)
        };

        public readonly MAP_DOUBLEE richness = new MAP_DOUBLEE();

        public readonly PLACABLE CLEAR = new PLACABLE()
        {
            Place = (tx, ty) => getter.set(tx, ty, null),
            IsPlacable = (tx, ty) => SETT.IN_BOUNDS(tx, ty) ? null : ""
        };

        public readonly PLACABLE INCREASE = new PLACABLE()
        {
            Place = (tx, ty) => amountInt.increment(tx, ty, 8),
            IsPlacable = (tx, ty) => SETT.IN_BOUNDS(tx, ty) ? null : ""
        };

        public readonly PLACABLE DECREASE = new PLACABLE()
        {
            Place = (tx, ty) => amountInt.increment(tx, ty, -8),
            IsPlacable = (tx, ty) => SETT.IN_BOUNDS(tx, ty) ? null : ""
        };

        public readonly PLACABLE ADD = new PLACABLE()
        {
            Place = (tx, ty) =>
            {
                Minable m = getter.get(tx, ty);
                if (m != null)
                    amountInt.increment(tx, ty, 8);
            },
            IsPlacable = (tx, ty) => SETT.IN_BOUNDS(tx, ty) ? null : ""
        };

        public readonly PLACABLE REMOVE = new PLACABLE()
        {
            Place = (tx, ty) =>
            {
                Minable m = getter.get(tx, ty);
                if (m != null)
                    amountInt.increment(tx, ty, -8);
            },
            IsPlacable = (tx, ty) => SETT.IN_BOUNDS(tx, ty) ? null : ""
        };

        void save(FilePutter saveFile)
        {
            MAPSAVE.saveMeta(saveFile, RESOURCES.minables().all());
            amount.save(saveFile);
            types.save(saveFile);
            richness.save(saveFile);
            saveFile.lsE(amounts);
        }

        void load(FileGetter saveFile)
        {
            int[] order = MAPSAVE.saveWash(saveFile, RESOURCES.minables().all(), -1);
            amount.load(saveFile);
            types.load(saveFile);
            richness.load(saveFile);
            saveFile.lsE(amounts);

            if (order != null)
            {
                for (int i = 0; i < SETT.TAREA; i++)
                {
                    int oi = types.get(i);
                    int ni = order[oi];
                    if (ni == -1)
                    {
                        amount.set(i, 0);
                        types.set(i, 0);
                    }
                    else
                    {
                        types.set(i, ni);
                    }
                }

                amounts = new long[RESOURCES.minables().all().Count];
                for (int i = 0; i < SETT.TAREA; i++)
                {
                    amounts[getter.get(i).resource.index()] += amount.get(i);
                }
            }
        }

        private readonly DIR[] dirs = new DIR[] { DIR.W, DIR.NW, DIR.N };

        void render(Renderer r, int tile, int ran, int x, int y)
        {
            double d = amountD.get(tile);

            if (d == 0)
                return;

            d *= 0.5 + 0.5 * richness.get(tile);

            int t = (int)(d * 8);
            t = CLAMP.i(t, 0, 3);
            t *= 8;
            t += ran & 0x07;
            ran = ran >> 3;
            Minable m = RESOURCES.minables().getAt(types.get(tile));
            m.sheet.render(r, t, x, y);

            int a = (int)(d * 12);
            a = (int)Math.Ceiling(a);

            int iters = (int)CLAMP.i(a / 3, 0, 3);
            if (iters > 0)
            {
                int dd = ran % 3;
                ran = ran >> 2;

                for (int i = 0; i < iters; i++)
                {
                    int tt = (ran & 31);
                    ran = ran >> 5;
                    DIR dir = dirs[dd];
                    dd++;
                    dd %= 3;
                    m.sheet.render(r, tt, x + dir.x() * C.TILE_SIZEH, y + dir.y() * C.TILE_SIZEH);
                }
            }
        }

        COLOR miniC(ColorImp col, COLOR ground, int tx, int ty)
        {
            Minable m = getter.get(tx, ty);
            col.interpolate(m.miniColor, ground, 0.5 + amountD.get(tx, ty) * 0.5);
            foreach (DIR d in DIR.ORTHO)
            {
                if (getter.get(tx, ty, d) != m)
                {
                    return col.shadeSelf(0.75);
                }
            }
            return col;
        }

        public readonly DOUBLE_O<Minable> totals = new DOUBLE_O<Minable>()
        {
            GetD = t => (double)amounts[t.index()] / amount.maxValue()
        };
    }
}