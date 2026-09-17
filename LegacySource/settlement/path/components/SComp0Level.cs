using settlement.main;
using snake2d.util.map;

namespace settlement.path.components
{
    public sealed class SComp0Level : SComponentLevel
    {
        public const int SIZE = 8;
        public static int startSize = (SETT.TWIDTH / SIZE) * (SETT.TWIDTH / SIZE);
        private readonly SComp0Factory factory = new SComp0Factory();
        private readonly SComp0Map map = new SComp0Map(factory);
        internal readonly SComp0Quads quads = new SComp0Quads(SIZE);
        private readonly SComp0Updater updater = new SComp0Updater(map, factory, this);

        public SComp0Level()
        {
        }

        protected override void init()
        {
            factory.clear();
            map.clear();
            quads.changeAll();
        }

        public void update(int tx, int ty)
        {
            quads.setChangedAvailability(tx, ty);
        }

        public void changeSerives(int tx, int ty)
        {
            quads.setChangedServices(tx, ty);
        }

        public int comps()
        {
            return factory.maxAmount();
        }

        public SComponent comp(int i)
        {
            SComp0 c = factory.get(i);
            if (c != null && c.superComp() != null)
                return c;
            return null;
        }

        protected override void update()
        {
            quads.update(updater);
        }

        public override int componentsMax()
        {
            return factory.maxAmount();
        }

        public override SComp0 get(int tile)
        {
            return map.get(tile);
        }

        public override SComp0 get(int tx, int ty)
        {
            return map.get(tx, ty);
        }

        public bool uping()
        {
            return quads.updating();
        }

        public MAP_BOOLEAN updating()
        {
            return quads.updating;
        }

        public override SComponent getByIndex(int index)
        {
            return factory.get(index);
        }

        public override int level()
        {
            return 0;
        }

        public override int size()
        {
            return SIZE;
        }
    }
}