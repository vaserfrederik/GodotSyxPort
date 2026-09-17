using System;
using init.race.appearence;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using util.colors;
using util.gui.misc;
using util.gui.panel;

namespace view.sett.ui.subject
{
    final class UISubjectInfo : GuiSection
    {
        public static readonly int width = 560;

        public UISubjectInfo(AInfo a, int height, HTYPE t)
        {
            HOVERABLE sprite = new HOVERABLE.HoverableAbs(RPortrait.P_WIDTH * 4 + 16, RPortrait.P_HEIGHT * 4 + 16)
            {
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
            };

            RENDEROBJ l = new UISubjectEquip(a, t);
            RENDEROBJ r = new UISubjectActions(a, t);

            int w = Math.Max(l.body().width(), r.body().width()) + 16;
            body().setWidth(w * 2 + sprite.body().width());
            addC(sprite, body().cX(), 0);

            l.body().moveCY(body().cY());
            l.body().moveCX(body().x1() + w / 2);
            add(l);

            r.body().moveCY(body().cY());
            r.body().moveCX(body().x2() - w / 2);
            add(r);
            addRelBody(8, DIR.S, top(a));

            addRelBody(4, DIR.S, new SInfoDesc(a, height - body().height()));
        }

        static GuiSection top(AInfo a)
        {
            GuiSection s = new GuiSection();

            s.addRelBody(2, DIR.S, new RENDEROBJ.RenderImp(500, UI.FONT().H2.height())
            {
                final GText name = new GText(UI.FONT().H2, 24);
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    GCOLOR.T().H2.bind();
                    name.clear();
                    name.add(STATS.APPEARANCE().name(a.a.indu()));
                    name.setMaxWidth(550);
                    name.setMultipleLines(false);
                    name.lablify();
                    name.adjustWidth();
                    name.renderC(r, body().cX(), body().cY());
                }
            });

            s.addRelBody(2, DIR.S, new RENDEROBJ.RenderImp(400, UI.FONT().S.height())
            {
                GText text = new GText(UI.FONT().S, 36);
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    text.clear();
                    a.a.ai().getOccupation(a.a, text);
                    text.normalify();
                    text.adjustWidth();
                    text.renderC(r, body().cX(), body().cY());
                }
            });

            return s;
        }
    }
}