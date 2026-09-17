using System;
using settlement.main;
using game.battle;
using settlement.entity.humanoid;
using settlement.misc.util;
using settlement.room.main;
using snake2d.util.datatypes;

namespace settlement.path.finders
{
    public sealed class SFinderSoldierManning : SFinderFindable
    {
        private readonly bool army;

        public SFinderSoldierManning(bool army) : base("s_manning")
        {
            this.army = army;
            new TestPath(name, this);
        }

        public override FINDABLE_MANNING GetReservable(int x, int y)
        {
            Room i = ROOMS().map.get(x, y);
            if (i == null || !(i is FINDABLE_MANNING_INSTANCE))
                return null;
            FINDABLE_MANNING f = ((FINDABLE_MANNING_INSTANCE)i).GetManning(x, y);
            if (f != null && f.army().player() == army && f.findableReservedCanBe())
                return f;
            return null;
        }

        public override FINDABLE_MANNING GetReserved(int x, int y)
        {
            Room i = ROOMS().map.get(x, y);
            if (i == null || !(i is FINDABLE_MANNING_INSTANCE))
                return null;
            FINDABLE_MANNING f = ((FINDABLE_MANNING_INSTANCE)i).GetManning(x, y);
            if (f != null && f.army().player() == army && f.findableReservedIs())
                return f;
            return null;
        }

        public interface FINDABLE_MANNING : FINDABLE
        {
            public DIR faceDIR();
            public void work(double time, Humanoid a);
            public bool needsWork();
            public Army army();
        }

        public interface FINDABLE_MANNING_INSTANCE
        {
            public FINDABLE_MANNING GetManning(int tx, int ty);
        }
    }
}