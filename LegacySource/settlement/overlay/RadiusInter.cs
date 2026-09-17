using System;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.path.components;
using settlement.path.finders;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util;
using util.rendering;

namespace settlement.overlay
{
    final class RadiusInter : Addable
    {
        private RoomBlueprintIns<? extends RADIUS_INTER> blue;
        private SFinderFindable fin;
        private readonly int half = 10;
        private readonly int m = 5;
        private double er;
        private int ex, ey;

        RadiusInter()
            : base(null, null, null, null, true, true)
        {
            exclusive = true;
        }

        public void Add(RoomBlueprintIns<? extends RADIUS_INTER> blue, SFinderFindable fin, int ex, int ey, double er)
        {
            base.Add();
            this.er = er;
            this.ex = ex;
            this.ey = ey;
            this.blue = blue;
            this.fin = fin;
        }

        public void Add(RoomBlueprintIns<? extends RADIUS_INTER> blue, SFinderFindable fin)
        {
            Add(blue, fin, -1, -1, -1);
        }

        private double Value(int tx, int ty)
        {
            SComponent c = SETT.PATH().comps.zero.Get(tx, ty);
            if (c == null || !GUTIL.Flooder().HasBeenPushed(c.CentreX(), c.CentreY()))
                return 0;

            double v = GUTIL.Flooder().GetValue(c.CentreX(), c.CentreY());
            int i = (int)GUTIL.Flooder().GetValue2(c.CentreX(), c.CentreY());

            if (i < 0 || i >= blue.InstancesSize())
                return (v / er);
            RADIUS_INTER r = blue.GetInstance(i);
            return v / r.Radius();
        }

        public override void InitBelow(RenderData data)
        {
            GUTIL.Flooder().Init(this);
            AddPoint(ex, ey, er, -1);
            for (int i = 0; i < blue.InstancesSize(); i++)
            {
                RADIUS_INTER r = blue.GetInstance(i);
                AddPoint(r.Rx(), r.Ry(), r.Radius(), i);
            }

            SComp0Level comps = SETT.PATH().comps.zero;

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollGreatest();
                SComponent c = comps.Get(t);
                SComponentEdge e = c.EdgeFirst();

                while (e != null)
                {
                    double v = t.GetValue() - e.Cost2();
                    if (v >= 0)
                    {
                        if (GUTIL.Flooder().PushGreater(e.To().CentreX(), e.To().CentreY(), v) != null)
                            GUTIL.Flooder().SetValue2(e.To().CentreX(), e.To().CentreY(), t.GetValue2());
                    }
                    e = e.Next();
                }
            }
        }

        private void AddPoint(int tx, int ty, double radius, int value2)
        {
            if (radius <= 1)
                return;
            SComp0Level comps = SETT.PATH().comps.zero;
            SComponent c = comps.Get(tx, ty);
            if (c == null)
            {
                for (int di = 0; di < DIR.ORTHO.Size(); di++)
                {
                    DIR d = DIR.ORTHO.Get(di);
                    c = comps.Get(tx, ty, d);
                    if (c != null)
                        break;
                }
            }
            if (c != null)
            {
                if (GUTIL.Flooder().PushSloppy(c.CentreX(), c.CentreY(), radius) != null)
                    GUTIL.Flooder().SetValue2(c.CentreX(), c.CentreY(), value2);
            }
        }

        public override bool Render(Renderer r, RenderIterator it)
        {
            if ((it.Tx() & m) != half || (it.Ty() & m) != half)
                return false;

            if (fin.Map.Has(it.Tx(), it.Ty()))
            {
                SPRITE s = fin.Map.Is(it.Tx(), it.Ty()) ?
                    SPRITES.icons().s.alert : SPRITES.icons().s.allRight;

                int X1 = it.X() - 8;
                int Y1 = it.Y() - 8;
                int X2 = it.X() + C.TILE_SIZE + 16;
                int Y2 = it.Y() + C.TILE_SIZE + 16;

                COLOR.BLACK.Bind();

                s.Render(r, X1 + 8, X2 + 8, Y1 + 8, Y2 + 8);

                COLOR.Unbind();

                s.Render(r, X1, X2, Y1, Y2);
            }

            return false;
        }

        public override void RenderBelow(Renderer r, RenderIterator it)
        {
            double v = Value(it.Tx(), it.Ty());
            RenderUnder(v, r, it, false);
        }

        public override void FinishBelow()
        {
            GUTIL.Flooder().Done();
        }
    }
}