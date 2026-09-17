using System;
using System.Collections.Generic;
using util;
using util.colors;
using util.rendering;
using util.text;
using settlement.main;
using settlement.room.infra.janitor;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.sets;

namespace settlement.overlay
{
    public sealed class OverlayMaintenance : Addable
    {
        private readonly SComponentChecker check = new SComponentChecker(SETT.PATH().comps.zero);
        private Bitmap1D checkS = new Bitmap1D(1024, false);
        private RoomInstance special;
        private static readonly CharSequence ¤¤desc = "Highlights which tiles need maintenance.";

        static
        {
            D.ts(typeof(OverlayMaintenance));
        }

        public OverlayMaintenance()
            : base(UI.icons().s.degrade, "MAINTENANCE", Dic.¤¤Maintenance, ¤¤desc, true, true)
        {
            exclusive = true;
        }

        public void Add(RoomInstance ins)
        {
            base.Add();
            this.special = ins;
        }

        public override void Add()
        {
            this.special = null;
            base.Add();
        }

        public override void InitBelow(RenderData data)
        {
            check.Init();
            GUTIL.flooder().Init(this);

            int radius = ROOM_JANITOR.radius;

            foreach (RoomInstance ins in SETT.ROOMS().JANITOR.all())
            {
                if (ins.active())
                {
                    GUTIL.flooder().PushSloppy(ins.mX(), ins.mY(), 0);
                }
            }

            while (GUTIL.flooder().HasMore())
            {
                PathTile t = GUTIL.flooder().PollSmallest();
                if (t.GetValue() > radius)
                    continue;
                SComponent c = SETT.PATH().comps.zero.Get(t);
                if (c != null)
                {
                    check.IsSetAndSet(c);
                    SComponentEdge e = c.EdgeFirst();
                    while (e != null)
                    {
                        GUTIL.flooder().PushSmaller(e.To().centreX(), e.To().centreY(), t.GetValue() + e.Distance(), t);
                        GUTIL.flooder().SetValue2(e.To().centreX(), e.To().centreY(), 0);
                        e = e.Next();
                    }
                }
            }
            GUTIL.flooder().Done();

            if (special == null)
                return;

            if (checkS.Size() < SETT.PATH().comps.zero.ComponentsMax())
                checkS = new Bitmap1D(SETT.PATH().comps.zero.ComponentsMax(), false);
            checkS.Clear();
            GUTIL.flooder().Init(this);
            GUTIL.flooder().PushSloppy(special.mX(), special.mY(), 0);

            while (GUTIL.flooder().HasMore())
            {
                PathTile t = GUTIL.flooder().PollSmallest();
                if (t.GetValue() > radius)
                    continue;
                SComponent c = SETT.PATH().comps.zero.Get(t);
                if (c != null)
                {
                    checkS.Set(c.Index(), true);
                    SComponentEdge e = c.EdgeFirst();
                    while (e != null)
                    {
                        GUTIL.flooder().PushSmaller(e.To().centreX(), e.To().centreY(), t.GetValue() + e.Distance(), t);
                        GUTIL.flooder().SetValue2(e.To().centreX(), e.To().centreY(), 0);
                        e = e.Next();
                    }
                }
            }
            GUTIL.flooder().Done();
        }

        public override void RenderBelow(Renderer r, RenderIterator it)
        {
            COLOR c = COLOR.WHITE10;

            SComponent comp = SETT.PATH().comps.zero.Get(it.tile());
            if (comp == null)
            {

            }
            else
            {
                if (SETT.MAINTENANCE().disabled.Is(it.tile()))
                {
                    c = GCOLOR.MAP().SOSO;
                }
                else if (!SETT.MAINTENANCE().needs.Is(it.tx(), it.ty()))
                {
                    c = COLOR.WHITE50;
                }
                else if (SETT.MAINTENANCE().degrade.Get(it.tx(), it.ty()) > 0)
                {
                    c = ColorImp.TMP.Interpolate(GCOLOR.MAP().OVERLAY_GOOD, GCOLOR.MAP().OVERLAY_BAD, SETT.MAINTENANCE().degrade.Get(it.tx(), it.ty()));
                }
                else
                {
                    c = GCOLOR.MAP().OVERLAY_GOOD;
                }

                ColorImp.TMP.Set(c);
                if (checkS != null && checkS.Get(comp.Index()))
                    ColorImp.TMP.ShadeSelf(1.25f);
                else if (!check.Is(comp))
                    ColorImp.TMP.ShadeSelf(0.75f);
                c = ColorImp.TMP;
            }

            renderUnder(c, r, it);
        }

        public override bool Render(Renderer r, RenderIterator it)
        {
            if (SETT.MAINTENANCE().isser.Is(it.tile()))
            {
                COLOR c = GCOLOR.MAP().BAD;
                if (SETT.MAINTENANCE().disabled.Is(it.tile()))
                {
                    c = COLOR.WHITE50;
                }
                else if (SETT.MAINTENANCE().reserved.Is(it.tx(), it.ty()))
                    c = GCOLOR.MAP().BEST_DARK;
                else if (SETT.MAINTENANCE().degrade.Get(it.tx(), it.ty()) > 0)
                    c = GCOLOR.MAP().BAD;
                else
                    c = GCOLOR.MAP().SOSO;

                c.Bind();
                SPRITES.cons().BIG.outline.Render(r, 0, it.x(), it.y());
                COLOR.Unbind();
                RESOURCE res = SETT.MAINTENANCE().resource.Get(it.tx(), it.ty());
                if (res != null)
                {
                    res.icon().RenderScaled(r, it.x() + C.TILE_SIZEH / 4, it.y() + C.TILE_SIZEH / 4, 2);
                }
                return true;
            }
            return false;
        }

        public override void FinishBelow()
        {
            special = null;
        }
    }
}