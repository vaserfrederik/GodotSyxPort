using System;
using System.Collections.Generic;
using game;
using init.constant;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.main;
using view.ui.div;
using world.army;
using world.entity.army;

namespace view.sett.ui.army
{
    public class UIArmyCitySendOut : GuiSection
    {
        private static int xs = 8;
        private readonly ArrayList<Card> cards = new ArrayList<Card>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<Card> current = new ArrayList<Card>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<Div> li = new ArrayList<Div>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly UIDivCardSett card = VIEW.UI().div.settCivic;
        private WArmy army;

        public UIArmyCitySendOut()
        {
            foreach (Div d in GAME.ARMIES().player().divisions())
            {
                cards.add(new Card(d));
            }

            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () =>
                {
                    int am = CLAMP.i(current.size() + 1, 0, Config.battle().DIVISIONS_PER_ARMY);
                    return (int)Math.Ceiling((double)am / xs);
                }
            };

            bu.column(null, xs * card.width(), new GRowBuilder
            {
                build = (GETTER<Integer> ier) => row(ier)
            });

            add(bu.create(4, false));

            GuiSection s = new GuiSection();
            GButt.ButtPanel but = new GButt.ButtPanel(Dic.¤¤confirm)
            {
                renAction = () => activeSet(li.size() > 0 && Actions.sendProblem(li) == null),
                clickA = () =>
                {
                    foreach (Card c in current)
                    {
                        if (army.divs().canAdd() && c.selectedIs() && c.div().info.men() > 0 && AD.cityDivs().attachedArmy(c.div()) == null)
                        {
                            AD.cityDivs().attach(army, c.div());
                        }
                    }

                    VIEW.inters().popup.close();
                },
                hoverInfoGet = (GUI_BOX text) =>
                {
                    Actions.hoverSendOutProblem(li, text);
                    base.hoverInfoGet(text);
                }
            };
            s.add(but);
            but = new GButt.ButtPanel(UI.icons().m.fast_forw)
            {
                clickA = () => SETT.BATTLE().info.sendOutWithoutTraining(!SETT.BATTLE().info.sendOutWithoutTraining()),
                renAction = () => selectedSet(SETT.BATTLE().info.sendOutWithoutTraining())
            };
            but.hoverInfoSet(Dic.¤¤SendOutArmyToggleD);
            s.addRightC(0, but);

            addRelBody(16, DIR.S, s);
        }

        public void init(WArmy a)
        {
            this.army = a;
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            init();
            if (!VIEW.world().isActive())
                VIEW.inters().popup.close();
            base.render(r, ds);
        }

        private void init()
        {
            current.clearSloppy();
            li.clearSloppy();
            foreach (Card c in cards)
            {
                if (c.div().info.men() > 0 && AD.cityDivs().attachedArmy(c.div()) == null)
                {
                    current.add(c);
                    if (c.selectedIs())
                    {
                        li.add(c.div());
                    }
                }
                else
                {
                    c.selectedSet(false);
                }
            }
        }

        private RENDEROBJ row(GETTER<Integer> ier)
        {
            GuiSection ss = new GuiSection
            {
                render = (SPRITE_RENDERER r, float ds) =>
                {
                    int x1 = body().x1();
                    int y1 = body().y1();
                    clear();
                    for (int i = 0; i < xs; i++)
                    {
                        int k = ier.get() * xs + i;
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
                    body().setWidth(card.width() * xs);
                    body().setHeight(card.height());
                    base.render(r, ds);
                }
            };
            ss.body().setWidth(card.width() * xs);
            ss.body().setHeight(card.height());
            return ss;
        }

        private class Card : ClickableAbs
        {
            private readonly int di;

            public Card(Div div) : base(card.width(), card.height())
            {
                di = div.indexArmy();
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                isActive = UIDivCardWorld.supplyError(div()) == null;
                card.render(r, body.x1(), body.y1(), 1, div(), isActive, isSelected, isHovered);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                VIEW.UI().div.settCivic.hover(text, div());
            }

            protected override void clickA()
            {
                selectedSet(!selectedIs());
            }

            public Div div()
            {
                return GAME.ARMIES().player().ordered().get(di);
            }
        }
    }
}