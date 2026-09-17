using System;
using settlement.main;

namespace settlement.path.components
{
    public sealed class SCompNLevel : SComponentLevel
    {
        private readonly int level;
        private readonly SCompNFactory factory;
        private readonly SCompNUpdater updater;
        private readonly int size;

        public SCompNLevel(SComponentLevel prev, int level, int size)
        {
            this.level = level;
            factory = new SCompNFactory(level, size);
            this.size = size;
            updater = new SCompNUpdater(this, factory, prev, size);
        }

        void Remove(SComponent toBeRemoved)
        {
            updater.Remove(toBeRemoved);
        }

        void AddNew(SComponent newSubComponent)
        {
            updater.Add(newSubComponent);
        }

        public override SComponent Get(int tile)
        {
            SComponent c = SETT.PATH().comps.all[level - 1][tile];
            if (c == null)
                return null;
            return c.SuperComp();
        }

        public override SComponent Get(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
                return Get(tx + ty * TWIDTH);
            return null;
        }

        public override int ComponentsMax()
        {
            return factory.MaxAmount();
        }

        public override SComponent GetByIndex(int index)
        {
            return factory.Get(index);
        }

        protected override void Update()
        {
            updater.Update();
        }

        public override int Level()
        {
            return level;
        }

        public override int Size()
        {
            return size;
        }

        protected override void Init()
        {
            factory.Clear();
        }
    }
}