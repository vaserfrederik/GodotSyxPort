using settlement.misc.util;
using settlement.path.finders;

namespace settlement.thing
{
    public abstract class ThingFindable : Thing, FINDABLE
    {
        public ThingFindable(int index) : base(index)
        {
        }

        public override void findableReserve()
        {
            if (!findableReservedCanBe())
                throw new System.RuntimeException();
            reserve(1);
            if (!findableReservedCanBe())
                finder().report(this, -1);
        }

        protected abstract void reserve(int delta);

        public override void findableReserveCancel()
        {
            if (!findableReservedIs())
                return;
            if (findableReservedCanBe())
                return;
            reserve(-1);
            finder().report(this, 1);
        }

        public abstract SFinderFindable finder();

        protected override void addAction()
        {
            if (findableReservedCanBe())
                finder().report(this, 1);
        }

        protected override void removeAction()
        {
            if (findableReservedCanBe())
                finder().report(this, -1);
        }
    }
}