using System;
using System.Collections.Generic;
using game.battle;
using init.constant;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;
using view.keyboard;
using view.main;

namespace view.battle
{
    final class UIPanelUnitCards : ISidePanel
    {
        private static int xs = 5;
        private readonly ArrayList<DivButton> cards = new ArrayList<DivButton>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<DivButton> current = new ArrayList<DivButton>(Config.battle().DIVISIONS_PER_ARMY);
        private DivButton clicked;
        private bool dragging;
        private readonly Army army;
        private readonly DivSelection selection;

        public UIPanelUnitCards(Army army, DivSelection selection) : base()
        {
            titleSet(Dic.¤¤Army);
            this.army = army;
            this.selection = selection;
            this.section = new GuiSection
            {
                Render = (r, ds) =>
                {
                    init();
                    base.Render(r, ds);
                    if (!MButt.LEFT.isDown())
                    {
                        dragging = false;
                    }
                }
            };
            foreach (Div d in army.divisions())
            {
                cards.add(new DivButton(d, selection));
            }
            GTableBuilder bu = new GTableBuilder
            {
                NrOFEntries = () =>
                {
                    int am = CLAMP.i(current.size(), 0, Config.battle().DIVISIONS_PER_ARMY);
                    return (int)Math.Ceiling((double)am / xs);
                }
            };

            bu.Column(null, xs * VIEW.UI().div.battle.width, new GRowBuilder
            {
                Build = (ier) => row(ier)
            });

            section.Add(bu.CreateHeight(HEIGHT, false));
        }

        private RENDEROBJ row(GETTER<int> ier)
        {
            GuiSection ss = new GuiSection
            {
                Render = (r, ds) =>
                {
                    int x1 = body().x1();
                    int y1 = body().y1();
                    clear();
                    for (int i = 0; i < xs; i++)
                    {
                        int k = ier.Get() * xs + i;
                        if (k >= current.size())
                        {
                            break;
                        }
                        else
                        {
                            addRightC(0, current.get(k));
                        }
                    }
                    body().moveX1Y1(x1, y1);
                    body().setWidth(VIEW.UI().div.battle.width * xs);
                    body().setHeight(VIEW.UI().div.battle.height);
                    base.Render(r, ds);
                }
            };
            ss.body().setWidth(VIEW.UI().div.battle.width * xs);
            ss.body().setHeight(VIEW.UI().div.battle.height);
            return ss;
        }

        private void init()
        {
            current.clearSloppy();
            foreach (Div d in army.ordered())
            {
                if (d.menNrOf() > 0)
                {
                    current.add(cards.get(d.indexArmy()));
                }
                else
                {
                    selection.deSelect(d);
                }
            }
        }

        private class DivButton : ClickableAbs
        {
            private readonly Div div;
            private readonly DivSelection selection;

            public DivButton(Div div, DivSelection selection)
            {
                body.setDim(VIEW.UI().div.battle);
                this.div = div;
                this.selection = selection;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                isSelected = selection.selected(div);
                isHovered |= selection.hovered(div);
                VIEW.UI().div.battle.render(div, body().x1(), body().y1(), 1, r, isActive, isSelected, isHovered);

                if (dragging && isHovered && clicked != null && clicked != this && !KEYS.MAIN().UNDO.isPressed() && !KEYS.MAIN().MOD.isPressed())
                {
                    COLOR.GREEN100.render(r, body().x1() - 2, body().x1() + 2, body().y1(), body().y2());
                    if (!MButt.LEFT.isDown())
                    {
                        army.setDivAtOrderedIndex(div, clicked.div);
                        selection.deSelect(clicked.div);
                        selection.select(div);
                        clicked = this;
                    }
                }
            }

            protected override void clickA()
            {
                if (KEYS.MAIN().UNDO.isPressed() && clicked != null)
                {
                    int ci = current.indexOf(this);
                    int di = current.indexOf(clicked);
                    int f = Math.Min(ci, di);
                    int t = Math.Max(ci, di);

                    for (int i = 0; i < current.size(); i++)
                    {
                        if (i >= f && i <= t)
                            selection.select(current.get(i).div);
                        else
                            selection.deSelect(current.get(i).div);
                    }
                }
                else if (KEYS.MAIN().MOD.isPressed())
                {
                    selection.sToggle(div);
                }
                else
                {
                    for (int i = 0; i < current.size(); i++)
                    {
                        selection.deSelect(current.get(i).div);
                    }
                    selection.select(div);
                    clicked = this;
                    dragging = true;
                    if (MButt.LEFT.isDouble() && div.menNrOf() > 0)
                    {
                        VIEW.s().battle.getWindow().centerer.set(div.reporter.body().cX(), div.reporter.body().cY());
                        VIEW.b().getWindow().centerer.set(div.reporter.body().cX(), div.reporter.body().cY());
                        VIEW.inters().popup.show(VIEW.UI().div.battle.hovBox(div), this);
                    }
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (VIEW.inters().popup.showing())
                    return;
                div.hoverInfo((GBox)text);
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    selection.hover(div);
                    return true;
                }
                return false;
            }
        }
    }
}