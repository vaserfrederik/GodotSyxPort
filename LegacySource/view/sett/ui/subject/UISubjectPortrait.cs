using System;
using init.race.appearence;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using util.colors;
using util.gui.misc;
using util.gui.panel;

namespace view.sett.ui.subject
{
    final class UISubjectPortrait : HOVERABLE.HoverableAbs
    {
        private readonly AInfo a;

        UISubjectPortrait(AInfo a, HTYPE t) : base(RPortrait.P_WIDTH * 4 + 16, RPortrait.P_HEIGHT * 4 + 16)
        {
            this.a = a;
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            a.a.indu().hType().color.render(r, body());
            GFrame.render(r, body().x1(), body().x2(), body().y1(), body().y2());

            STATS.APPEARANCE().portraitRender(r, a.a.indu(), body.x1() + 8, body().y1() + 8, 4);

            OPACITY.O25TO100.bind();
            if (SProblem.problem(a.a) != null)
            {
                GCOLOR.UI().BAD.hovered.bind();
                UI.icons().s.flag.renderScaled(r, body().x1() + 8, body().y1() + 8, 2);
            }
            else if (SProblem.warning(a.a) != null)
            {
                GCOLOR.UI().SOSO.hovered.bind();
                UI.icons().s.flag.renderScaled(r, body().x1() + 8, body().y1() + 8, 2);
            }
            OPACITY.unbind();
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            if (SProblem.problem(a.a) != null)
            {
                b.add(b.text().errorify().add(SProblem.problem(a.a)));
            }
            else if (SProblem.warning(a.a) != null)
            {
                b.add(b.text().warnify().add(SProblem.warning(a.a)));
            }
            base.hoverInfoGet(text);
        }
    }
}