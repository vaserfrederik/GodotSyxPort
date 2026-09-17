using System;
using settlement.overlay;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using util;
using util.colors;
using util.rendering;
using util.text;
using view.main;

namespace settlement.overlay
{
    final class RoomProblem : Addable
    {
        private static CharSequence ¤¤desc = "Show problems that exists with your rooms.";
        static
        {
            D.ts(typeof(RoomProblem));
        }

        RoomProblem() : base(UI.icons().s.alert, "PROBLEM", Dic.¤¤Problem, ¤¤desc, true, false)
        {
            exclusive = true;
        }

        public override void initBelow(RenderData data)
        {
            GUTIL.flooder().init(this);
        }

        public override bool render(Renderer r, RenderIterator it)
        {
            return false;
        }

        public override void renderBelow(Renderer r, RenderIterator it)
        {
            Room ro = SETT.ROOMS().map.get(it.tx(), it.ty());
            if (ro != null)
            {
                int mx = ro.mX(it.tx(), it.ty());
                int my = ro.mY(it.tx(), it.ty());
                if (!GUTIL.flooder().hasBeenPushed(mx, my))
                {
                    int v = 0;
                    if (VIEW.s().ui.rooms.problem(ro, mx, my))
                        v = 1;
                    else if (VIEW.s().ui.rooms.warning(ro, mx, my))
                        v = 2;
                    GUTIL.flooder().close(mx, my, v);
                }

                double v = GUTIL.flooder().getValue(mx, my);
                if (v == 0)
                    renderUnder(GCOLOR.MAP().OVERLAY_GOOD, r, it);
                else if (v == 1)
                    renderUnder(GCOLOR.MAP().OVERLAY_BAD, r, it);
                else
                    renderUnder(GCOLOR.MAP().SOSO, r, it);
            }
            else
            {
                renderUnder(COLOR.WHITE15, r, it);
            }
        }

        public override void finishBelow()
        {
            GUTIL.flooder().done();
        }
    }
}