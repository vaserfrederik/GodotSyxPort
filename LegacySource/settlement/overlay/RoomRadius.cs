using System.Collections.Generic;
using settlement.main;
using settlement.path.components;
using settlement.room.main;
using snake2d;
using util;
using util.rendering;

namespace settlement.overlay
{
    final class RoomRadius : Addable
    {
        private RoomInstance ins;
        private int radius;
        private readonly SComponentChecker comps;

        RoomRadius(SComponentChecker comps) : base(null, null, null, null, true, false)
        {
            exclusive = true;
            this.comps = comps;
        }

        public void add(RoomInstance ins, int radius)
        {
            this.ins = ins;
            this.radius = radius;
            base.add();
        }

        public override void initBelow(RenderData data)
        {
            GUTIL.flooder().init(this);

            foreach (COORDINATE c in ins.body())
            {
                if (ins.is(c))
                {
                    GUTIL.flooder().pushSloppy(c, 0);
                }
            }

            comps.init();
            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                if (t.getValue() > radius)
                    continue;
                SComponent c = SETT.PATH().comps.zero.get(t);
                if (c == null)
                    continue;
                comps.isSetAndSet(c);
                SComponentEdge e = c.edgefirst();

                while (e != null)
                {
                    double dist = e.distance();
                    GUTIL.flooder().pushSmaller(e.to().centreX(), e.to().centreY(), t.getValue() + dist);
                    e = e.next();
                }
            }
            GUTIL.flooder().done();
        }

        public override bool render(Renderer r, RenderIterator it)
        {
            return false;
        }

        public override void renderBelow(Renderer r, RenderIterator it)
        {
            SComponent c = SETT.PATH().comps.zero.get(it.tile());
            if (c != null && comps.isSet(c.index()))
                renderUnder(1, r, it, false);
            else
                renderUnder(0, r, it, false);
        }

        public override void finishBelow()
        {
        }
    }
}