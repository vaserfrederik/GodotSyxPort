using System;
using System.Collections.Generic;
using game;
using init.constant;
using init.race.appearence;
using init.settings;
using init.sprite.UI;
using settlement.stats;
using settlement.stats.equip;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using world.army;
using static util.colors.GCOLOR;

namespace view.world.ui.army
{
    class UIMerenaries
    {
        private static readonly string ¤¤intro = "Captain {0}'s";

        static UIMerenaries()
        {
            D.ts(typeof(UIMerenaries));
        }

        private int max = AD.mercenaries().size();
        private readonly Card[] cards = new Card[max];
        private readonly List<Card> active = new List<Card>(max);
        private GuiSection scards = new GuiSection();
        private readonly GuiSection section = new GuiSection()
        {
            render = (r, ds) =>
            {
                arrange();
                base.render(r, ds);
            }
        };

        private static int xs = 10;
        private int width = RPortrait.P_WIDTH * 2 + 20;
        private int height = RPortrait.P_HEIGHT * 2 + 12 + 52 - 20;

        public UIMerenaries()
        {
            for (int i = 0; i < AD.mercenaries().size(); i++)
            {
                Card c = new Card(i);
                cards[i] = c;
            }

            scards.body().setDim(xs * width, (int)Math.Ceiling((double)max / xs) * height);

            section.add(scards);

            GuiSection bb = new GuiSection();

            bb.addRightC(8, new GStat()
            {
                update = text =>
                {
                    GFORMAT.i(text, (int)FACTIONS.player().credits().credits());
                }
            }.hh(Dic.¤¤Currs));

            bb.addRightC(64, new GStat()
            {
                update = text =>
                {
                    int co = cost();
                    if (co > FACTIONS.player().credits().credits())
                        text.errorify();
                    else
                        text.normalify();

                    GFORMAT.iIncr(text, -co);
                }
            }.hh(Dic.¤¤Cost));

            bb.addRightC(64, new GButt.ButtPanel(Dic.¤¤Recruit)
            {
                renAction = () =>
                {
                    activeSet(Army.army.added() && Army.army.divs().canAdd() && cost() > 0 && cost() <= FACTIONS.player().credits().credits());
                },
                clickA = () =>
                {
                    foreach (Card c in active)
                    {
                        if (c.selectedIs() && Army.army.divs().canAdd())
                        {
                            int cost = AD.mercenaries().signingCost(c.ii);
                            if (cost < FACTIONS.player().credits().credits())
                            {
                                c.div.reassign(Army.army);
                                GAME.player().credits().inc(-AD.mercenaries().signingCost(c.ii), CTYPE.MERCINARIES);
                            }
                        }
                    }
                    VIEW.inters().popup.close();
                    base.clickA();
                }
            });

            if (S.get().developer)
            {
                bb.addRightC(16, new GButt.ButtPanel("shuffle")
                {
                    clickA = () =>
                    {
                        AD.mercenaries().debug();
                    }
                });
            }

            section.addRelBody(8, DIR.S, bb);
        }

        public void arrange()
        {
            active.Clear();
            int max = AD.mercenaries().max();
            for (int i = 0; i < max; i++)
            {
                WDivMercenary d = AD.mercenaries().get(i);
                if (d.army() != null)
                    continue;
                if (d.men() == 0)
                    continue;
                if (d.disbanded())
                    continue;
                active.Add(cards[i]);
            }

            int x1 = scards.body().x1();
            int y1 = scards.body().y1();
            scards.clear();
            for (int i = 0; i < active.Count; i++)
            {
                Card c = active[i];
                scards.add(c, (i % xs) * c.body().width(), (i / xs) * (c.body().height()));
            }
            scards.body().moveX1Y1(x1, y1);
        }

        private int cost()
        {
            int am = 0;
            foreach (Card c in active)
            {
                if (c.selectedIs())
                {
                    am += AD.mercenaries().signingCost(c.ii);
                }
            }
            return am;
        }

        public GuiSection get()
        {
            foreach (Card c in cards)
                c.selectedSet(false);
            arrange();
            return section;
        }

        private GText tmp = new GText(UI.FONT().S, 8);

        private class Card : ClickableAbs
        {
            private readonly int ii;
            private readonly WDivMercenary div;

            public Card(int ii)
            {
                this.ii = ii;
                div = AD.mercenaries().get(ii);
                body.setDim(width, height);
            }

            private SPRITE title = new SPRITE.Imp(400, 16 * 2 + 8)
            {
                render = (r, X1, X2, Y1, Y2) =>
                {
                    Str.TMP.clear().add(¤¤intro);
                    Str.TMP.insert(0, STATS.APPEARANCE().name(div.cheif()));
                    T().H1.bind();
                    UI.FONT().H2.renderCX(r, X1 + (X2 - X1) / 2, Y1, Str.TMP);
                    T().H2.bind();
                    UI.FONT().M.renderCX(r, X1 + (X2 - X1) / 2, Y1 + 18, div.name());
                }
            };

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                isActive = AD.mercenaries().signingCost(ii) <= FACTIONS.player().credits().credits();
                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                GButt.ButtPanel.renderFrame(r, body);

                STATS.APPEARANCE().portraitRender(r, div.cheif(), body.x1() + 10, body.y1(), 2);
                div.cheif().race().appearance().crown.merc().getC(STATS.RAN().get(div.cheif(), 9)).renderScaled(r, body.x1() + 10, body.y1() + 8, 2);

                div.banner().renderSymbol(r, body.x1() + 4, body.y1() + 8, 1);

                int y2 = body.y1() + RPortrait.P_HEIGHT * 2 + 4;
                GMeter.render(r, GMeter.C_GRAY, (double)div.men() / Config.battle().MEN_PER_DIVISION, body.x1() + 6, body.x2() - 6, y2, y2 + 8);

                y2 += 10;
                {
                    int tot = 0;
                    foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                    {
                        tot += (int)Math.Ceiling(div.equipI(e) / 3);
                    }

                    int x1 = body.cX() - tot * 10 / 2;

                    foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                    {
                        int am = (int)Math.Ceiling(div.equipI(e) / 3);
                        for (int i = 0; i < am; i++)
                        {
                            e.resource.icon().small.render(r, x1, y2);
                            x1 += 10;
                        }
                    }
                }

                y2 += 12;

                tmp.clear();
                GFORMAT.i(tmp, AD.mercenaries().signingCost(ii));
                tmp.adjustWidth();
                if (cost() + AD.mercenaries().signingCost(ii) > FACTIONS.player().credits().credits())
                    tmp.errorify();
                else
                    tmp.normalify();
                tmp.adjustWidth();
                O50.bind();
                int x1 = body.cX() - tmp.width() / 2;
                COLOR.BLACK.render(r, x1 - 1, x1 + tmp.width() + 2, y2 - 1, y2 + 18);
                O50.unbind();
                tmp.render(r, x1, y2);

                VIEW.UI().div.renderPower(body.x2() - 18, body.y1() + 6, r, div.provess());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.add(title);
                text.NL();
                VIEW.UI().div.world.hover(div, text);
                text.title(null);
            }

            protected override void clickA()
            {
                selectedToggle();
            }
        }
    }
}