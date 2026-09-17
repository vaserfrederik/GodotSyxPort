using System;
using System.Collections.Generic;
using game.audio;
using init.resources;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;
using util.text;
using view.sett;
using view.tool;

namespace settlement.tilemap.terrain
{
    public sealed class TGrowable : TerrainTile
    {
        public readonly Growable growable;

        private static readonly CharSequence ¤¤Ripe = "(Ripe)";
        private static readonly CharSequence ¤¤RipeNot = "(Not Ripe)";
        private static readonly CharSequence ¤¤Name = "Wild {0}";
        static
        {
            D.ts(typeof(TGrowable));
        }
        public readonly int gIndex;

        private static readonly Bits bsize = new Bits(0b0000_0000_0000_1111);
        private static readonly Bits bfruit = new Bits(0b0000_0000_1111_0000);
        private static readonly Bit doJob = new Bit(0b0000_0001_0000_0000);
        private readonly double sizeI = 1.0 / 0b01111;

        private readonly TerrainClearing clearing = new TerrainClearing()
        {
            private SoundRace sound = AUDIO.race("CLEAR_BUSH"),

            public RESOURCE clear1(int tx, int ty)
            {
                size.increment(tx, ty, -3);

                if (SETT.WEATHER().growthRipe.cropsAreRipe())
                    return growable.resource;
                return null;
            },

            public bool can()
            {
                return true;
            },

            public int clearAll(int tx, int ty)
            {
                int am = (int)Math.Ceiling(resource.get(tx, ty) / 4.0);
                shared.NADA.placeFixed(tx, ty);
                return am;
            },

            public SoundRace sound(int tx, int ty)
            {
                return sound;
            },

            public bool isEasilyCleared()
            {
                return true;
            }
        };

        public readonly TAmount size = new TAmount(11, name())
        {
            public override int get(int tile)
            {
                if (TERRAIN().get(tile) == this)
                {
                    return CLAMP.i(1 + bsize.get(shared.data.get(tile)), 1, max);
                }
                return 0;
            }

            public override MAP_INTE set(int tile, int value)
            {
                if (value <= 0)
                {
                    if (TERRAIN().get(tile) == this)
                        TERRAIN().NADA.placeFixed(tile % TWIDTH, tile / TWIDTH);
                }
                else
                {
                    if (TERRAIN().get(tile) != this)
                        this.placeFixed(tile % TWIDTH, tile / TWIDTH);
                    int d = shared.data.get(tile);
                    d = bsize.set(d, CLAMP.i(value - 1, 0, bsize.mask));
                    shared.data.set(tile, d);
                }
                return this;
            }
        };

        public readonly MAP_BOOLEANE job = new MAP_BOOLEANE()
        {
            public override bool is(int tx, int ty)
            {
                return is(tx + ty * TWIDTH);
            }

            public override bool is(int tile)
            {
                return SETT.TERRAIN().get(tile) is TGrowable && doJob.is(shared.data.get(tile));
            }

            public override MAP_BOOLEANE set(int tx, int ty, bool value)
            {
                return set(tx + ty * TWIDTH, value);
            }

            public override MAP_BOOLEANE set(int tile, bool value)
            {
                shared.data.set(tile, doJob.set(shared.data.get(tile), value));
                return this;
            }
        };

        public readonly MAP_INTE resource = new MAP_INTE()
        {
            public override int get(int tx, int ty)
            {
                return CLAMP.i(bfruit.get(shared.data.get(tx, ty)), 0, size.get(tx, ty));
            }

            public override int get(int tile)
            {
                return CLAMP.i(bfruit.get(shared.data.get(tile)), 0, size.get(tile));
            }

            public override MAP_INTE set(int tx, int ty, int value)
            {
                return set(tx + ty * TWIDTH, value);
            }

            public override MAP_INTE set(int tile, int value)
            {
                value = CLAMP.i(value, 0, 0b01111);

                int data = shared.data.get(tile);

                if (!is(tile))
                {
                    size.set(tile, value);
                    shared.data.set(tile, bfruit.set(data, value));
                    return this;
                }

                value = CLAMP.i(value, 0, size.get(tile));
                int old = bfruit.get(data);
                shared.data.set(tile, bfruit.set(data, value));
                if (old == 0 && value > 0 && doJob.is(data) && SETT.JOBS().getter.get(tile) == null)
                {
                    bool b = SETT.JOBS().planMode.is();
                    SETT.JOBS().planMode.set(false);
                    SETT.JOBS().clearss.food.placer().place(tile % TWIDTH, tile / TWIDTH, null, null);
                    SETT.JOBS().planMode.set(b);
                }
                return this;
            }
        };

        static LIST<TGrowable> make(Terrain t)
        {
            var all = new ArrayList<TGrowable>(RESOURCES.growable().all().size());
            foreach (Growable g in RESOURCES.growable().all())
            {
                all.add(new TGrowable(t, g));
            }
            IDebugPanelSett.add(new PlacableMulti("TGrowable increase size")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    if (TERRAIN().get(tx, ty) is TGrowable)
                    {
                        TGrowable g = (TGrowable)TERRAIN().get(tx, ty);
                        g.size.increment(tx, ty, 1);
                    }
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }
            });

            IDebugPanelSett.add(new PlacableMulti("TGrowable decrease size")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    if (TERRAIN().get(tx, ty) is TGrowable)
                    {
                        TGrowable g = (TGrowable)TERRAIN().get(tx, ty);
                        g.size.increment(tx, ty, -1);
                    }
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }
            });

            IDebugPanelSett.add(new PlacableMulti("TGrowable increase res")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    if (TERRAIN().get(tx, ty) is TGrowable)
                    {
                        TGrowable g = (TGrowable)TERRAIN().get(tx, ty);
                        g.resource.increment(tx, ty, 1);
                    }
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }
            });

            IDebugPanelSett.add(new PlacableMulti("TGrowable decrease res")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    if (TERRAIN().get(tx, ty) is TGrowable)
                    {
                        TGrowable g = (TGrowable)TERRAIN().get(tx, ty);
                        g.resource.increment(tx, ty, -1);
                    }
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }
            });

            return all;
        }

        private TGrowable(Terrain t, Growable g) : base("GROWABLE_" + g.resource.key, t, new Str(¤¤Name).insert(0, g.resource.name), g.resource.icon(), t.colors.minimap.growable)
        {
            this.growable = g;
            this.gIndex = g.index();
        }

        public override TerrainClearing clearing()
        {
            return clearing;
        }

        protected override bool place(int tx, int ty)
        {
            if (!is(tx, ty))
            {
                base.placeRaw(tx, ty);
                size.set(tx, ty, 1);
            }
            return false;
        }

        public bool isEdible(int tx, int ty)
        {
            return RESOURCES.EDI().get(growable.resource) != null && SETT.WEATHER().growthRipe.cropsAreRipe();
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            i.countVegetation();
            growable.sprite.render(r, s, i, size.DM.get(i.tile()), bfruit.get(data) * sizeI);

            //SETT.TERRAIN().colors.tree.get(i.ran()).bind();
            //growable.render(r, s, i, bamount.get(data), isRipe());
            //COLOR.unbind();
            return false;
        }

        public override AVAILABILITY getAvailability(int x, int y)
        {
            return AVAILABILITY.PENALTY2;
        }

        public override bool isPlacable(int tx, int ty)
        {
            return true;
        }

        public override void hoverInfo(GBox box, int tx, int ty)
        {
            box.add(growable.resource.icon());
            base.hoverInfo(box, tx, ty);
            box.tab(6);
            box.add(GFORMAT.iofkInv(box.text(), resource.get(tx, ty), size.get(tx, ty)));
            box.text(SETT.WEATHER().growthRipe.cropsAreRipe() ? ¤¤Ripe : ¤¤RipeNot);

            box.NL();
        }

        public override COLOR miniColorPimped(ColorImp c, int x, int y, bool northern, bool southern)
        {
            COLOR col = SETT.GROUND().minimap.miniC(x, y);
            c.interpolate(col, miniC, 0.25 + 0.75 * size.DM.get(x, y));
            return c;
        }
    }
}