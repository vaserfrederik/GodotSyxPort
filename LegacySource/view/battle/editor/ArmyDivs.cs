using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data.GETTER;
using util.data.INT;
using util.gui.slider;
using util.gui.table;
using util.text;
using view.main;
using view.ui.div;
using world.army;

namespace view.battle.editor
{
    class ArmyDivs : GuiSection
    {
        private GETTER_IMP<ArmySide> current;
        private ArrayList<RDiv> all = new ArrayList<RDiv>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly UIDivEditor editor;

        public ArmyDivs(GETTER_IMP<ArmySide> current, UIDivEditor editor)
        {
            this.editor = editor;
            this.current = current;
            while (all.HasRoom())
                all.Add(new RDiv());

            GMatrixDraggable m = new GMatrixDraggable(4, 9, VIEW.UI().div.normal.width(), VIEW.UI().div.normal.height())
            {
                NrOfEntries = () => current.Get().divs.size(),
                Get = (i, columnI) =>
                {
                    all.Get(i).Div = current.Get().divs.Get(i);
                    return all.Get(i);
                },
                Move = (oldI, newI) =>
                {
                    DIV_SPEC after = current.Get().divs.Get(newI);
                    DIV_SPEC dd = current.Get().divs.RemoveOrdered(oldI);
                    if (dd == null)
                        return;
                    int ii = current.Get().divs.IndexOf(after);
                    current.Get().divs.Insert(ii, dd);
                }
            };
            Add(m);

            GuiSection asSection = new GuiSection();

            foreach (ADArtillery a in AD.supplies().arts())
            {
                INTE ii = new INTE()
                {
                    Min = () => 0,
                    Max = () => ADSupplies.artilleryMax,
                    Get = () => current.Get().artillery[a.index()],
                    Set = (t) => current.Get().artillery[a.index()] = t
                };

                GuiSection s = new GuiSection();
                s.hoverInfoSet(a.art.info.names);
                s.Add(a.art.icon, 0, 0);
                s.AddRightC(8, new GTarget(48, false, true, ii));

                asSection.AddRightC(16, s);
            }

            AddRelBody(6, DIR.N, asSection);
        }

        private class RDiv : ClickableAbs
        {
            private DIV_SPEC Div;
            private bool exitHovered;

            public RDiv()
            {
                Body.SetDim(VIEW.UI().div.normal.width(), VIEW.UI().div.normal.height());
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                VIEW.UI().div.normal.render(r, Body.X1(), Body.Y1(), 1, Div, isActive, isSelected, isHovered);
                if (isHovered)
                {
                    if (!exitHovered)
                        OPACITY.O66.Bind();
                    UI.icons().s.cancel.render(r, Body.X2() - 16, Body.Y1());
                    OPACITY.Unbind();
                }
            }

            protected override void clickA()
            {
                if (exitHovered)
                {
                    current.Get().divs.RemoveOrdered(Div);
                }
                else
                    editor.div().copyFrom(Div);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (exitHovered)
                {
                    text.Title(Dic.¤¤remove);
                    text.Text(Dic.¤¤RightClick);
                }
                else
                    VIEW.UI().div.normal.hover(Div, text);
            }

            public override bool hover(COORDINATE mCoo)
            {
                exitHovered = false;
                if (base.hover(mCoo))
                {
                    if (mCoo.IsWithin(Body.X2() - 16, Body.X2(), Body.Y1(), Body.Y1() + 16))
                        exitHovered = true;
                    if (MButt.RIGHT.ConsumeClick())
                        current.Get().divs.RemoveOrdered(Div);
                    return true;
                }
                return false;
            }
        }
    }
}