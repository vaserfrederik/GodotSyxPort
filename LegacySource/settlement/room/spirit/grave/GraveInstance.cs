using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite.text;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.spirit.dump;
using settlement.room.spirit.grave;

namespace settlement.room.spirit.grave
{
    [Serializable]
    internal class GraveInstance : RoomInstance
    {
        private int available = 0;
        private readonly int total;

        public const double WORKER_PER_GRAVE = 0.1;
        public Jobs jobs;
        public long[] datas;
        public int[] names;

        protected GraveInstance(RoomBlueprintIns<GraveInstance> blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            int i = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && data().grave(c.x(), c.y()) != null)
                {
                    data().grave(c.x(), c.y()).init(c.x(), c.y(), i);
                    i++;
                }
            }

            datas = new long[i];
            names = Alloc.ii(i);
            jobs = new Jobs(this);
            int w = (int)Math.Ceiling(WORKER_PER_GRAVE * jobs.size());
            employees().maxSet(w);
            employees().neededSet(w);
            available = jobs.size();
            total = jobs.size();
            activate();
        }

        public override RoomBlueprintIns<? extends RoomInstance> blueprintI()
        {
            return (RoomBlueprintIns<? extends RoomInstance>)blueprint();
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            jobs.searchAgain();
        }

        protected override void activateAction()
        {
            data().activate(this, available, total);
        }

        protected override void deactivateAction()
        {
            data().deactivate(this, available, total);
            data().deactivate(this);
        }

        public override void updateTileDay(int tx, int ty)
        {
            Grave g = data().grave(tx, ty);
            if (g != null)
                g.updateDay2();
        }

        private GraveData data()
        {
            return ((GRAVE_DATA_HOLDER)blueprint()).graveData();
        }

        protected override void dispose()
        {
            data().dispose(this, available, total);
        }

        public void count(int a)
        {
            if (active())
                data().deactivate(this, available, total);
            available += a;
            if (active())
                data().activate(this, available, total);
        }

        public int total()
        {
            return total;
        }

        public int available()
        {
            return available;
        }

        private bool prompt()
        {
            int time = 0;
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Grave g = data().grave(c.x(), c.y());
                    if (g != null)
                    {
                        int t = g.daysTillDecompose(c.x(), c.y());
                        if (t > 0)
                        {
                            am++;
                            if (t > time)
                                time = t;
                        }
                    }
                }
            }

            if (am > 0)
            {
                Str.TMP.clear();
                Str.TMP.add(ROOM_DUMP.¤¤RemoveProblem);
                Str.TMP.insert(0, am);
                Str.TMP.insert(1, time);
                VIEW.inters().yesNo.activate(Str.TMP, null, null, false);
                return true;
            }
            return false;
        }

        protected override bool canRemoveAndRemoveAction(int tx, int ty, bool scatter, object obj, bool force)
        {
            if (force || !prompt())
                return true;
            return false;
        }

        public class Jobs : JobPositions<GraveInstance>
        {
            public Jobs(GraveInstance ins) : base(ins)
            {
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.data().grave(tx, ty) != null;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                if (ins.data().grave(tx, ty) != null)
                    return ins.data().grave(tx, ty).job(tx, ty);
                return null;
            }
        }
    }
}