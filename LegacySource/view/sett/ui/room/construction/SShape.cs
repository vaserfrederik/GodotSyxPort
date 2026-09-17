using System;
using init.sprite;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using util.text;
using view.main;

namespace view.sett.ui.room.construction
{
    internal class SShape
    {
        private static readonly string ¤¤Expand = "Expand room. Items can only be placed on the designated room area.";
        private static readonly string ¤¤ExpandOver = "Expand room over existing structures";
        private static readonly string ¤¤Overlay = "Toggle overlay";
        private static readonly string ¤¤Shrink = "Shrink Room";

        static SShape()
        {
            D.ts(typeof(SShape));
        }

        private readonly GuiSection ss = new GuiSection()
        {
            render = (r, ds) =>
            {
                base.render(r, ds);
                bool b = VIEW.s().tools.placer.getCurrent() == s.placement.placer.area() || VIEW.s().tools.placer.getCurrent() == s.placement.placer.area().getUndo();
                if (b)
                    SETT.ROOMS().placement.placer.renderExpense();
            }
        };

        private readonly State s;
        private readonly GuiSection pButts = new GuiSection();
        private readonly GuiSection butts = new GuiSection();
        private readonly GHeader title = new GHeader(Dic.¤¤Shape).subify();

        private readonly CLICKABLE buttExpand = new GButt.ButtPanel(SPRITES.icons().m.expand)
        {
            clickA = () =>
            {
                s.placement.placer.buildOnWalls.set(false);
                VIEW.s().tools.place(s.placement.placer.area(), s.config);
            },
            renAction = () =>
            {
                selectedSet(VIEW.s().tools.placer.getCurrent() == s.placement.placer.area() && !s.placement.placer.buildOnWalls.is());
            }
        }.hoverInfoSet(¤¤Expand);

        private readonly CLICKABLE buttExpandWalls = new GButt.ButtPanel(new SPRITE.Twin(SPRITES.icons().m.expand, SPRITES.icons().m.plus))
        {
            clickA = () =>
            {
                s.placement.placer.buildOnWalls.set(true);
                VIEW.s().tools.place(s.placement.placer.area(), s.config);
            },
            renAction = () =>
            {
                selectedSet(VIEW.s().tools.placer.getCurrent() == s.placement.placer.area() && s.placement.placer.buildOnWalls.is());
            }
        }.hoverInfoSet(¤¤ExpandOver);

        private readonly CLICKABLE buttOverlay = new GButt.ButtPanel(UI.icons().s.eye.sized(Icon.M))
        {
            clickA = () =>
            {
                s.placement.placer.showOverlay.toggle();
            },
            renAction = () =>
            {
                selectedSet(s.placement.placer.showOverlay.is());
            },
            hoverInfoGet = text =>
            {
                text.title(¤¤Overlay);
                if (s.placement.placer.blueprint().constructor().overlay() != null && s.placement.placer.blueprint().constructor().overlay().desc != null)
                {
                    text.text(s.placement.placer.blueprint().constructor().overlay().desc);
                }
            }
        };

        private readonly CLICKABLE buttShrink = new GButt.ButtPanel(SPRITES.icons().m.shrink)
        {
            clickA = () =>
            {
                VIEW.s().tools.place(s.placement.placer.area().getUndo(), s.config);
            },
            renAction = () =>
            {
                bg(GCOLOR.UI().BAD.normal);
                selectedSet(VIEW.s().tools.placer.getCurrent() == s.placement.placer.area().getUndo());
            }
        }.hoverInfoSet(¤¤Shrink);

        public SShape(State s)
        {
            this.s = s;
        }

        public GuiSection get()
        {
            ss.clear();

            ss.add(title);

            butts.clear();
            butts.add(buttExpand, 0, 0);
            butts.addRightC(2, buttExpandWalls);
            if (SETT.ROOMS().placement.placer.blueprint().constructor().overlay() != null)
                butts.addRightC(2, buttOverlay);
            butts.addRightC(2, buttShrink);

            butts.body().incrW(buttExpand.body().width() + 10);

            ss.addRelBody(4, DIR.S, butts);

            pButts.clear();
            pButts.body().setDim(1, 32);
            if (VIEW.s().tools.placer.getCurrent() == s.placement.placer.area() || VIEW.s().tools.placer.getCurrent() == s.placement.placer.area().getUndo())
                VIEW.s().tools.placer.stealButtons(pButts, true);
            ss.addRelBody(0, DIR.S, pButts);

            return ss;
        }
    }
}