using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Tilemap.Floor
{
    public sealed class TGrowth : TileMap.Resource
    {
        private readonly byte[] data = Alloc.Bb(TAREA);

        public readonly List<Grower> All;
        public readonly List<Grower> Growable;

        public readonly Grower Nothing;
        public readonly Grower Tree;
        public readonly Grower Flower;
        public readonly Grower Bush;
        public readonly Grower Mushroom;

        private bool growing = false;

        public TGrowth(Terrain topology)
        {
            LinkedList<Grower> all = new LinkedList<Grower>();
            Nothing = new Grower(all)
            {
                Grow = (tx, ty, max) => { },
                CurrentAmount = (tx, ty) => 0,
                SetRoots = (tx, ty, amount) => { }
            };

            Tree = new Grower(all)
            {
                SetRoots = (tx, ty, am) =>
                {
                    if (am > 0.2)
                    {
                        TERRAIN().TREES.SMALL.PlaceRaw(tx, ty);
                        TERRAIN().TREES.Amount.DM.Set(tx, ty, am);
                    }
                    else if (am > 0)
                    {
                        TERRAIN().BUSH.PlaceFixed(tx, ty);
                    }
                },
                CurrentAmount = (tx, ty) => 0.8 + RND.RFloat() * 0.2,
                Grow = (tx, ty, max) =>
                {
                    if (max <= 0)
                    {
                        if (TERRAIN().TREES.IsTree(tx, ty))
                        {
                            TERRAIN().TREES.Amount.Increment(tx, ty, -1);
                            if (TERRAIN().NADA.Is(tx, ty))
                            {
                                TERRAIN().BUSH.PlaceFixed(tx, ty);
                            }
                        }
                        else if (TERRAIN().BUSH.Is(tx, ty) && RND.OneIn(4))
                        {
                            if (RND.OneIn(8))
                            {
                                TERRAIN().DECOR_WOOD.PlaceFixed(tx, ty);
                            }
                            else
                            {
                                TERRAIN().NADA.PlaceFixed(tx, ty);
                            }
                        }
                    }
                    else if (TERRAIN().TREES.IsTree(tx, ty))
                    {
                        TERRAIN().TREES.Amount.Increment(tx, ty, max);
                    }
                }
            };

            Flower = new Grower(all)
            {
                SetRoots = (tx, ty, amount) =>
                {
                    int am = Math.Max(Math.Min((int)(amount * TERRAIN().Get(tx, ty).Size.Max), 1), TERRAIN().Get(tx, ty).Size.Max);
                    TERRAIN().Get(tx, ty).PlaceRaw(tx, ty);
                    TERRAIN().Get(tx, ty).Size.Set(tx, ty, am);
                },
                CurrentAmount = (tx, ty) => TERRAIN().Get(tx, ty).Size.DM.Get(tx, ty),
                Grow = (tx, ty, max) =>
                {
                    if (max <= 0)
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, -1);
                    }
                    else
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, max);
                    }
                }
            };

            Bush = new Grower(all)
            {
                SetRoots = (tx, ty, amount) =>
                {
                    int am = Math.Max(Math.Min((int)(amount * TERRAIN().Get(tx, ty).Size.Max), 1), TERRAIN().Get(tx, ty).Size.Max);
                    TERRAIN().Get(tx, ty).PlaceRaw(tx, ty);
                    TERRAIN().Get(tx, ty).Size.Set(tx, ty, am);
                },
                CurrentAmount = (tx, ty) => TERRAIN().Get(tx, ty).Size.DM.Get(tx, ty),
                Grow = (tx, ty, max) =>
                {
                    if (max <= 0)
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, -1);
                    }
                    else
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, max);
                    }
                }
            };

            Mushroom = new Grower(all)
            {
                SetRoots = (tx, ty, amount) =>
                {
                    int am = Math.Max(Math.Min((int)(amount * TERRAIN().Get(tx, ty).Size.Max), 1), TERRAIN().Get(tx, ty).Size.Max);
                    TERRAIN().Get(tx, ty).PlaceRaw(tx, ty);
                    TERRAIN().Get(tx, ty).Size.Set(tx, ty, am);
                },
                CurrentAmount = (tx, ty) => TERRAIN().Get(tx, ty).Size.DM.Get(tx, ty),
                Grow = (tx, ty, max) =>
                {
                    if (max <= 0)
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, -1);
                    }
                    else
                    {
                        TERRAIN().Get(tx, ty).Size.Increment(tx, ty, max);
                    }
                }
            };

            foreach (var growable in topology.Growable)
            {
                all.Add(new Grower(all)
                {
                    SetRoots = (tx, ty, amount) =>
                    {
                        int am = Math.Max(Math.Min((int)(amount * growable.Size.Max), 1), growable.Size.Max);
                        growable.PlaceRaw(tx, ty);
                        growable.Size.Set(tx, ty, am);
                    },
                    CurrentAmount = (tx, ty) => growable.Size.DM.Get(tx, ty),
                    Grow = (tx, ty, max) =>
                    {
                        if (max <= 0)
                        {
                            growable.Size.Increment(tx, ty, -1);
                        }
                        else
                        {
                            growable.Size.Increment(tx, ty, max);
                        }
                    }
                });
            }

            Growable = new List<Grower>(all.Where(g => !(g is Nothing)));
            All = new List<Grower>(all);
        }

        public bool Tear(int tx, int ty)
        {
            Grower g = Current(tx, ty);
            if (g != null && g != Tree)
            {
                g.Grow(tx, ty, -1);
            }
            else if (GRASS().CurrentI.Get(tx, ty) > 0)
            {
                GRASS().CurrentI.Increment(tx, ty, -1);
                return true;
            }
            return false;
        }

        public double GrowMaxAmount(int tx, int ty, Grower g)
        {
            double f = SETT.GROUND().MAP.Get(tx, ty).Vegitation;

            if (g == Tree && SETT.GROUND().Types.FOREST.Is(tx, ty))
            {
                f *= 0.65 + 0.35 * SETT.GROUND().MOISTURE_CURRENT.Get(tx, ty);
            }
            else
                f *= SETT.GROUND().MOISTURE_CURRENT.Get(tx, ty);
            f = Math.Min(Math.Max(f, 0), 1);

            double am = MaxAmount.Get(tx, ty);
            am -= (1.0 - f);
            return Math.Min(Math.Max(am, -1), 1);
        }

        public void UpdateTileDay(int tx, int ty, int now)
        {
            GROUND().Adjust(now, tx, ty);

            Room room = ROOMS().Map.Get(tx, ty);
            if (room != null)
            {
                if (!SETT.ROOMS().Construction.IsSer.Is(tx, ty) && room.Constructor() != null && room.Constructor().GrowsGrass(tx, ty))
                {
                    GRASS().Grow(tx, ty, now);
                }
                return;
            }
            TerrainTile t = TERRAIN().Get(tx, ty);
            if ((t.Clearing().IsStructure() && !t.RoofIs()) && !(TERRAIN().Get(tx, ty) is TFence.TFenceTile))
                return;
            if (JOBS().Getter.Is(tx, ty) && JOBS().Getter.Get(tx, ty).JobReservedIs(JOBS().Getter.Get(tx, ty).ResourceCurrentlyNeeded()))
                return;
            if (FLOOR().Getter.Is(tx, ty))
                return;

            GRASS().Grow(tx, ty);

            if (!growing)
                return;

            Grower g = All[Type.Get(now)];
            if (g == Nothing)
                return;

            double m = GrowMaxAmount(tx, ty, g);

            g.Grow(tx, ty, m);
        }

        protected override void Update(double ds, Profiler profiler)
        {
            growing = SETT.WEATHER().Growth.GetD() * SETT.WEATHER().Moisture.GetD() * 4 > 1.0;
        }

        protected override void Save(FilePutter saveFile)
        {
            saveFile.Bs(data);
        }

        protected override void Load(FileGetter saveFile)
        {
            saveFile.Bs(data);
        }

        protected override void ClearAll()
        {
            Array.Fill(data, (byte)0);
        }

        public double CurrentAmount(int tx, int ty)
        {
            double am = MaxAmount.Get(tx, ty);
            am *= SETT.GROUND().MOISTURE_CURRENT.Get(tx, ty);
            am *= SETT.GROUND().MAP.Get(tx, ty).Vegitation;

            am *= 2;
            am -= (double)(GUTIL.Ran2().Get(tx, ty) & 0x0FFFF) / 0x0FFFF;

            return am;
        }

        public Grower Current(int tx, int ty)
        {
            TerrainTile t = SETT.TERRAIN().Get(tx, ty);
            if (t is TGrowable)
            {
                return Growable[((TGrowable)t).Growable.Index()];
            }
            else if (t is Tree)
            {
                return Tree;
            }
            else if (t is TBush)
            {
                return Bush;
            }
            else if (t is TFlower)
            {
                return Flower;
            }
            else if (t is TMushroom)
            {
                return Mushroom;
            }
            return Nothing;
        }

        public Grower Type(int tx, int ty)
        {
            return All[Type.Get(tx, ty)];
        }

        private readonly MAP_INTE Type = new MAP_INTE.INT_MAPEImp(TWIDTH, THEIGHT)
        {
            private Bits bits = new Bits(0b0001_1111),

            public override int Get(int tile)
            {
                return bits.Get(data[tile]);
            },

            public override MAP_INTE Set(int tile, int value)
            {
                data[tile] = (byte)bits.Set(data[tile], value);
                return this;
            }
        };

        public readonly MAP_DOUBLEE MaxAmount = new MAP_DOUBLEE
        {
            private double di = 1.0 / 0b1000,
            private Bits bits = new Bits(0b1110_0000),

            public override double Get(int tile)
            {
                return (1 + bits.Get(data[tile])) * di;
            },

            public override double Get(int tx, int ty)
            {
                return Get(tx + ty * TWIDTH);
            },

            public override MAP_DOUBLEE Set(int tile, double value)
            {
                int i = (int)Math.Round(value * 0b1000);
                i -= 1;
                i = Math.Max(Math.Min(i, 0b0111), 0);
                data[tile] = (byte)bits.Set(data[tile], i);
                return this;
            },

            public override MAP_DOUBLEE Set(int tx, int ty, double value)
            {
                return Set(tx + ty * TWIDTH, value);
            }
        };

        public abstract class MAP_INTE
        {
            public abstract int Get(int tile);
            public abstract MAP_INTE Set(int tile, int value);
        }

        public class INT_MAPEImp : MAP_INTE
        {
            private int[] data;
            private int width, height;

            public INT_MAPEImp(int width, int height)
            {
                this.width = width;
                this.height = height;
                this.data = new int[width * height];
            }

            public override int Get(int tile)
            {
                return data[tile];
            }

            public override MAP_INTE Set(int tile, int value)
            {
                data[tile] = value;
                return this;
            }
        }

        public abstract class MAP_DOUBLEE
        {
            public abstract double Get(int tile);
            public abstract MAP_DOUBLEE Set(int tile, double value);
            public abstract double Get(int tx, int ty);
            public abstract MAP_DOUBLEE Set(int tx, int ty, double value);
        }

        public class DOUBLEE_MAPEImp : MAP_DOUBLEE
        {
            private double[] data;
            private int width, height;

            public DOUBLEE_MAPEImp(int width, int height)
            {
                this.width = width;
                this.height = height;
                this.data = new double[width * height];
            }

            public override double Get(int tile)
            {
                return data[tile];
            }

            public override MAP_DOUBLEE Set(int tile, double value)
            {
                data[tile] = value;
                return this;
            }

            public override double Get(int tx, int ty)
            {
                return data[tx + ty * width];
            }

            public override MAP_DOUBLEE Set(int tx, int ty, double value)
            {
                data[tx + ty * width] = value;
                return this;
            }
        }

        public class Bits
        {
            private int bits;

            public Bits(int bits)
            {
                this.bits = bits;
            }

            public int Get(int value)
            {
                return (value & bits) >> ((~bits & (bits - 1)).CountBits());
            }

            public int Set(int value, int bit)
            {
                return (value & ~bits) | (bit << ((~bits & (bits - 1)).CountBits()));
            }
        }

        public static class Extensions
        {
            public static int CountBits(this int i)
            {
                i = i - ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
            }
        }
    }