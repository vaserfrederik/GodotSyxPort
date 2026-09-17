using System;
using System.Collections.Generic;
using init.constant;
using init.sprite.UI;
using settlement.main;
using settlement.path.components;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util;
using util.colors;
using util.rendering;

namespace settlement.overlay
{
    public sealed class ServiceRadius : Addable
    {
        private readonly SComponentChecker check = new SComponentChecker(SETT.PATH().comps.zero);
        private RoomFinderHaser ser;
        private readonly Rec tiles = new Rec();

        public ServiceRadius()
            : base(null, null, null, null, true, false)
        {
            exclusive = true;
        }

        public void Add(RoomFinderHaser ser)
        {
            base.Add();
            this.ser = ser;
        }

        public override void InitBelow(RenderData data)
        {
            check.Init();
            GUTIL.Flooder().Init(this);

            int radius = ser.Radius();

            if (ser is RoomBlueprintIns)
            {
                RoomBlueprintIns b = (RoomBlueprintIns)ser;
                foreach (RoomInstance ins in b.All())
                {
                    foreach (COORDINATE c in ins.Body())
                    {
                        if (ins.Is(c) && b.Service(c.X, c.Y) != null)
                        {
                            foreach (DIR d in DIR.ORTHO)
                                GUTIL.Flooder().PushSloppy(c, d, 0);
                        }
                    }
                }
            }
            else
            {
                tiles.SetDim(data.TBounds());
                tiles.IncrW(radius * 2 + 2);
                tiles.IncrH(radius * 2 + 2);
                tiles.CenterIn(data.TBounds());

                foreach (COORDINATE c in tiles)
                {
                    if (SETT.ROOMS().Map.Blueprint.Get(c) == ser)
                    {
                        foreach (DIR d in DIR.ORTHO)
                            GUTIL.Flooder().PushSloppy(c, d, 0);
                    }
                }
            }

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                if (t.Value > radius)
                    continue;
                SComponent c = SETT.PATH().comps.zero.Get(t);
                if (c != null)
                {
                    check.IsSetAndSet(c);
                    SComponentEdge e = c.EdgeFirst();
                    while (e != null)
                    {
                        GUTIL.Flooder().PushSmaller(e.To().CentreX(), e.To().CentreY(), t.Value + e.Distance(), t);
                        e = e.Next();
                    }
                }
            }
            GUTIL.Flooder().Done();
        }

        public override void RenderBelow(Renderer r, RenderIterator it)
        {
            double radius = ser.Radius();

            COLOR c = COLOR.WHITE10;
            SComponent comp = SETT.PATH().comps.zero.Get(it.Tile());

            if (comp != null && check.Is(comp))
            {
                PathTile t = GUTIL.Flooder().Get(comp.CentreX(), comp.CentreY());
                if (t != null)
                {
                    double v = t.Value;
                    v /= radius;
                    v = 1.0 - v;

                    if (ser.Finder().Map.Has(it.Tx(), it.Ty()))
                    {
                        if (ser.Finder().Map.Fail(it.Tx(), it.Ty()))
                        {
                            c = ColorImp.TMP.Interpolate(COLOR.WHITE25, GCOLOR.MAP().OVERLAY_BAD, v);
                        }
                        else
                        {
                            c = ColorImp.TMP.Interpolate(COLOR.WHITE25, COLOR.WHITE85, v);
                        }
                    }
                    else
                    {
                        c = ColorImp.TMP.Interpolate(COLOR.WHITE25, COLOR.WHITE85, v);
                    }

                    int tx = it.Tx() - 1;
                    int ty = it.Ty() - 1;

                    SComponent c2 = SETT.PATH().comps.zero.Get(tx, ty);
                    if (c2 != null && check.Is(c2) && c2.CentreX() == tx && c2.CentreY() == ty)
                    {
                        if (ser.Finder().Map.Fail(it.Tx(), it.Ty()))
                        {
                            c = ColorImp.TMP.Interpolate(COLOR.WHITE25, GCOLOR.MAP().OVERLAY_BAD, v);
                            UI.icons().l.thumbsDown.RenderCScaled(r, it.X() - C.TILE_SIZEH, it.Y() - C.TILE_SIZEH, C.SCALE);
                        }
                    }
                }
            }

            RenderUnder(c, r, it);
        }

        public override void FinishBelow()
        {
        }
    }
}