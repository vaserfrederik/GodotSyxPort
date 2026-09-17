using System;
using System.IO;
using System.Collections.Generic;
using settlement.entity;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.file.SAVABLE;

namespace settlement.thing.projectiles
{
    internal sealed class PData
    {
        private const int CHUNK = 2 * 16384;
        private int activeLast;

        private float[] space;
        private int[] next;
        private short[] type;
        private float[] ref;
        private int[] shooter;

        private readonly Data data = new Data();
        private readonly Data dd = new Data();
        private readonly Map map;

        private static int SPACESIZE = 7;

        private readonly VectorImp vec = new VectorImp();

        public PData(Map map)
        {
            this.map = map;
            saver.Clear();
        }

        public int Create(int x, int y, int height, double dx, double dy, double dz, Projectile t, double reff, ENTITY entity)
        {
            if (!SETT.PIXEL_IN_BOUNDS(x, y))
                return -1;
            if (activeLast >= next.Length)
            {
                int nz = next.Length + CHUNK;

                float[] space = new float[nz * SPACESIZE];
                for (int i = 0; i < this.space.Length; i++)
                    space[i] = this.space[i];
                this.space = space;
                int[] next = Alloc.ii(nz);
                short[] type = new short[nz];
                float[] ref = new float[nz];
                int[] shooter = Alloc.ii(nz);
                Array.Fill(shooter, -1);
                for (int i = 0; i < this.next.Length; i++)
                {
                    next[i] = this.next[i];
                    type[i] = this.type[i];
                    ref[i] = this.ref[i];
                    shooter[i] = this.shooter[i];
                }
                this.next = next;
                this.type = type;
                this.ref = ref;
                this.shooter = shooter;
            }
            dd.sI = activeLast * SPACESIZE;

            dd.xSet(x);
            dd.ySet(y);
            dd.zSet(height);

            double m = vec.Set(dx, dy);
            dd.nxSet(vec.nX());
            dd.nySet(vec.nY());
            dd.dzSet(dz);
            dd.magnitudeSet(m);

            shooterSet(activeLast, entity);
            type(activeLast, t.index);
            ref(activeLast, reff);
            map.Add(activeLast);
            t.soundRelease().Rnd(x, y, 1);
            activeLast++;
            return activeLast - 1;
        }

        public void Remove(int index)
        {
            map.Remove(index);
            activeLast--;
            if (index != activeLast)
            {
                map.Remove(activeLast);
                Copy(activeLast, index);
                map.Add(index);
            }
        }

        public bool Move(int i, double nx, double ny)
        {
            float x = (float)nx;
            float y = (float)ny;

            if (!SETT.PIXEL_IN_BOUNDS((int)x, (int)y))
            {
                Remove(i);
                return false;
            }

            dd.sI = i * SPACESIZE;
            if (((int)x) >> Map.gridScroll != dd.qx() || ((int)y) >> Map.gridScroll != dd.qy())
            {
                map.Remove(i);
                dd.xSet(x);
                dd.ySet(y);
                map.Add(i);
            }
            else
            {
                dd.xSet(x);
                dd.ySet(y);
            }
            return true;
        }

        private void Copy(int fromI, int toI)
        {
            type[toI] = type[fromI];
            next[toI] = next[fromI];
            ref[toI] = ref[fromI];
            toI *= SPACESIZE;
            fromI *= SPACESIZE;

            for (int i = 0; i < SPACESIZE; i++)
            {
                space[toI + i] = space[fromI + i];
            }
        }

        internal sealed class Data
        {
            private int sI;

            private Data()
            {
            }

            public float x()
            {
                return space[sI + 0];
            }
            private void xSet(float x)
            {
                space[sI + 0] = x;
            }
            public float y()
            {
                return space[sI + 1];
            }
            private void ySet(float x)
            {
                space[sI + 1] = x;
            }
            public float z()
            {
                return space[sI + 2];
            }
            public void zSet(double x)
            {
                space[sI + 2] = (float)x;
            }

            public float nx()
            {
                return space[sI + 3];
            }
            public void nxSet(double x)
            {
                space[sI + 3] = (float)x;
            }
            public float ny()
            {
                return space[sI + 4];
            }
            public void nySet(double x)
            {
                space[sI + 4] = (float)x;
            }
            public float dz()
            {
                return space[sI + 5];
            }
            public void dzSet(double x)
            {
                space[sI + 5] = (float)x;
            }
            void magnitudeSet(double x)
            {
                space[sI + 6] = (float)x;
            }
            public double dMagnitude()
            {
                return space[sI + 6];
            }

            public double speedX()
            {
                return nx() * dMagnitude();
            }

            public double speedY()
            {
                return ny() * dMagnitude();
            }

            public int qx()
            {
                return ((int)x()) >> Map.gridScroll;
            }

            public int qy()
            {
                return ((int)y()) >> Map.gridScroll;
            }
        }

        public Data data(int index)
        {
            data.sI = index * SPACESIZE;
            return data;
        }

        public int next(int index)
        {
            return next[index];
        }

        public Projectile type(int index)
        {
            return Projectile.ALL.Get(type[index] & 0b01111111);
        }

        public double ref(int index)
        {
            return ref[index];
        }

        public void ref(int index, double ref)
        {
            this.ref[index] = (float)ref;
        }

        public void nextSet(int index, int n)
        {
            next[index] = n;
        }

        public void type(int index, short t)
        {
            type[index] &= t & 0b1000000000000000;
            type[index] |= t;
        }

        public ENTITY shooter(int index)
        {
            return SETT.ENTITIES().getByID(shooter[index]);
        }

        public void shooterSet(int index, ENTITY e)
        {
            int i = e == null ? -1 : e.id();
            shooter[index] = i;
        }

        public void live(int index, bool live)
        {
            if (!live)
            {
                type[index] |= 0b0100000000000000;
            }
            else
            {
                type[index] &= ~0b0100000000000000;
            }
        }

        public bool live(int index)
        {
            return (type[index] & 0b0100000000000000) == 0;
        }

        public int last()
        {
            return activeLast;
        }

        internal readonly SAVABLE saver = new SAVABLE()
        {
            public void Save(FilePutter file)
            {
                file.i(activeLast);
                for (int i = 0; i < activeLast; i++)
                {
                    file.s(type[i]);
                    file.f(ref[i]);
                    file.i(shooter[i]);
                }
                int am = activeLast * SPACESIZE;
                for (int i = 0; i < am; i++)
                {
                    file.f(space[i]);
                }
            }

            public void Load(FileGetter file)
            {
                activeLast = file.i();
                int l = (int)Math.Ceiling((double)(activeLast + 1) / CHUNK);
                space = new float[l * CHUNK * SPACESIZE];
                next = Alloc.ii(l * CHUNK);
                type = new short[l * CHUNK];
                ref = new float[l * CHUNK];
                shooter = Alloc.ii(l * CHUNK);
                int MZ = Projectile.ALL.size();
                for (int i = 0; i < activeLast; i++)
                {
                    type[i] = file.s();
                    if (type[i] > MZ)
                        type[i] = 0;
                    ref[i] = file.f();
                    shooter[i] = file.i();
                }
                Arrays.Fill(next, -1);
                for (int i = 0; i < last(); i++)
                {
                    map.Add(i);
                }
            }

            public void Clear()
            {
                space = new float[CHUNK * SPACESIZE];
                next = Alloc.ii(CHUNK);
                Arrays.Fill(next, -1);
                type = new short[CHUNK];
                ref = new float[CHUNK];
                shooter = Alloc.ii(CHUNK);
                Arrays.Fill(shooter, -1);
                activeLast = 0;
            }
        };
    }
}