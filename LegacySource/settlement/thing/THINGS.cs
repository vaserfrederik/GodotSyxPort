using System;
using System.Collections.Generic;
using System.IO;
using static settlement.main.SETT;

namespace settlement.thing
{
    public class THINGS : SETT.SettResource
    {
        private readonly List<ThingFactory<?>> all = new List<ThingFactory<?>>(16);
        private readonly Thing[,] grid = new Thing[THEIGHT, TWIDTH];
        private readonly List<Thing> tmp = new List<Thing>(16000);
        private const int MAX_OUT_TILES = 2;

        public readonly Sprites sprites = new Sprites();
        public readonly ThingsGore gore = new ThingsGore(all, 1.0f, sprites);
        public readonly ThingsResources resources = new ThingsResources(all);
        public readonly ThingsFactory factory = new ThingsFactory(all, grid, tmp, MAX_OUT_TILES);

        public THINGS() : base("THINGS")
        {
        }

        protected override void Load(StreamReader reader)
        {
            // Load implementation
        }

        protected override void Save(StreamWriter writer)
        {
            // Save implementation
        }
    }

    public abstract class ThingFactory
    {
        private readonly List<ThingFactory<?>> all;
        private readonly Thing[,] grid;
        private readonly List<Thing> tmp;
        private readonly int maxOutTiles;

        protected ThingFactory(List<ThingFactory<?>> all, Thing[,] grid, List<Thing> tmp, int maxOutTiles)
        {
            this.all = all;
            this.grid = grid;
            this.tmp = tmp;
            this.maxOutTiles = maxOutTiles;
        }

        public abstract void Update(double ds);

        protected void Remove(Thing thing)
        {
            short next = thing.AddedNext;
            short prev = thing.AddedPrev;

            if (next != -1)
            {
                all[next].AddedPrev = prev;
            }

            if (prev != -1)
            {
                all[prev].AddedNext = next;
            }

            if (thing.Index == firstAdded)
            {
                firstAdded = next;
            }

            if (thing.Index == lastAdded)
            {
                lastAdded = prev;
            }

            thing.AddedPrev = -1;
            thing.AddedNext = -1;
            free.Push(thing.Index);
            addedHistory.Set(Added());
        }

        protected void Add(Thing thing)
        {
            if (thing.AddedNext != -1)
                throw new Exception();

            if (thing.AddedPrev != -1)
                throw new Exception();

            int i = free.Pop();
            addedHistory.Set(Added());
            if (i != thing.Index)
                throw new Exception();

            if (firstAdded == -1)
            {
                firstAdded = thing.Index;
                lastAdded = thing.Index;
                return;
            }

            all[lastAdded].AddedNext = thing.Index;
            thing.AddedPrev = lastAdded;
            lastAdded = thing.Index;
        }

        public int Added()
        {
            return all.Count - free.Count;
        }

        public int RemainingToAdd()
        {
            return free.Count;
        }

        protected T First()
        {
            if (firstAdded >= 0)
                return all[firstAdded];
            return null;
        }

        protected T Next(T thing)
        {
            if (thing.AddedNext != -1)
                return all[thing.AddedNext];
            return null;
        }
    }

    public abstract class Thing : BODY_HOLDER
    {
        private short ix = -1;
        private short iy = -1;
        public short AddedNext = -1;
        public short AddedPrev = -1;
        private readonly short index;
        private Thing next;
        private Thing prev;
        protected readonly RBITImp resourcemask = new RBITImp();

        public Thing(int index)
        {
            this.index = (short)index;
        }

        public bool IsRemoved()
        {
            return ix == -1;
        }

        public void Remove()
        {
            if (ix == -1)
                throw new Exception();

            THINGS m = THINGS();
            resourcemask.Clear();
            if (next != null)
            {
                next.prev = prev;
            }
            if (prev != null)
            {
                prev.next = next;
            }

            if (m.grid[iy, ix] == this)
            {
                m.grid[iy, ix] = next;
            }

            next = null;
            prev = null;
            ix = -1;
            Factory().Remove(this);
            RemoveAction();
        }

        protected virtual void AddAction()
        {
        }

        protected virtual void RemoveAction()
        {
        }

        protected final void AddColdAsHell()
        {
            THINGS m = THINGS();
            next = null;
            if (m.grid[iy, ix] == null)
            {
                m.grid[iy, ix] = this;
                return;
            }

            resourcemask.Or(m.grid[iy, ix].resourcemask);

            if (m.grid[iy, ix].z() >= z())
            {
                m.grid[iy, ix].prev = this;
                next = m.grid[iy, ix];
                m.grid[iy, ix] = this;
                return;
            }

            Thing parent = m.grid[iy, ix];
            while (parent.next != null && parent.next.z() < z())
                parent = parent.next;

            if (parent.next != null)
            {
                parent.next.prev = this;
                next = parent.next;
            }

            parent.next = this;
            prev = parent;
        }

        protected final void Add()
        {
            if (ix != -1)
                throw new Exception();

            ix = (short)Ctx();
            iy = (short)Cty();
            if (!TILE_BOUNDS.HoldsPoint(ix, iy))
            {
                ix = -1;
                return;
            }

            AddColdAsHell();
            if (Factory() != null)
                Factory().Add(this);
            AddAction();
        }

        protected void Move(ESpeed speed, double ds, float restitution, RECTANGLEE body, bool tileCollide)
        {
            body.IncrX(speed.x() * ds);
            body.IncrY(speed.y() * ds);

            if (ix != Ctx() || iy != Cty())
            {
                if (!IsRemoved())
                    Remove();
                Add();
            }
        }

        protected void Move()
        {
            if (ix != Ctx() || iy != Cty())
            {
                Remove();
                Add();
            }
        }

        public int Ctx()
        {
            return body().CX() >> C.T_SCROLL;
        }

        public int Cty()
        {
            return body().CY() >> C.T_SCROLL;
        }

        protected abstract int Z();

        protected final void SaveP(FilePutter f)
        {
            f.Bool(!IsRemoved());
            resourcemask.Save(f);
        }

        protected final void LoadP(FileGetter f)
        {
            Clear();

            if (f.Bool())
            {
                resourcemask.Load(f);
                ix = (short)Ctx();
                iy = (short)Cty();
                AddColdAsHell();
                Factory().Add(this);
            }
        }

        protected final void Clear()
        {
            AddedNext = -1;
            AddedPrev = -1;
            next = null;
            prev = null;
            ix = -1;
            iy = -1;
        }

        protected abstract void Save(FilePutter f);

        protected abstract void Load(FileGetter f);
    }
}