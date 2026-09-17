using System;
using System.Collections.Generic;
using game.faction.FACTIONS;
using game.faction.diplomacy.DIP;
using game.faction.npc.FactionNPC;
using game.faction.royalty.opinion.ROPINION;
using game.faction.trade.TradeManager;
using init.sprite.UI.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using world.region;

namespace view.ui.economy
{
    final class Factions : GuiSection
    {
        private ArrayList<FactionNPC> all = new ArrayList<FactionNPC>(FACTIONS.MAX());
        private static readonly int width = 96;
        private readonly GText t = new GText(UI.FONT().S, 16);

        Factions(int HEIGHT)
        {
            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () => all.size()
            };

            bu.column("", width, new GRowBuilder
            {
                build = (GETTER<int> ier) => new Row(ier)
            });

            add(bu.createHeight(48 * (HEIGHT / 48), false));
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            all.clearSloppy();
            foreach (FactionNPC f in FACTIONS.NPCs())
            {
                if (RD.DIST().reachable(f))
                {
                    all.add(f);
                }
            }
            foreach (FactionNPC f in FACTIONS.NPCs())
            {
                if (!RD.DIST().reachable(f))
                {
                    all.add(f);
                }
            }

            base.render(r, ds);
        }

        private class Row : ClickableAbs
        {
            private readonly GETTER<int> ier;

            Row(GETTER<int> ier)
            {
                this.ier = ier;
                body.setDim(width, 48);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                GButt.ButtPanel.renderBG(r, RD.DIST().reachable(f()), DIP.get(f()).trades, isHovered, body);
                f().banner().BIG.renderCY(r, body.x1() + 8, body.cY());

                t.clear();
                GFORMAT.percInv(t, ROPINION.tradeCost(f()));
                t.render(r, body().x2() - 48, body.y1() + 8);

                t.clear();
                GFORMAT.i(t, Math.Round(TradeManager.toll(f())));
                t.render(r, body().x2() - 48, body.y1() + 8 + 18);

                GButt.ButtPanel.renderFrame(r, body);

                if (!RD.DIST().reachable(f()))
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body, -1);
                    OPACITY.unbind();
                }
            }

            FactionNPC f()
            {
                return all.get(ier.get());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                VIEW.world().UI.factions.hover(text, f());
                GBox b = (GBox)text;
                b.sep();

                b.add(UI.icons().s.wheel);
                b.textLL(Dic.¤¤Toll);
                b.tab(7);
                b.add(GFORMAT.f(b.text(), TradeManager.toll(f())));
                b.NL();

                b.add(UI.icons().s.money);
                b.textLL(Dic.¤¤CreditScore);
                b.tab(7);
                b.add(GFORMAT.percInc(b.text(), f().stockpile.creditScore() - 1.0));
                b.NL();

                b.add(UI.icons().s.angry);
                b.textLL(Dic.¤¤Tariff);
                b.tab(7);
                b.add(GFORMAT.percInv(b.text(), ROPINION.tradeCost(f())));
                b.NL();
            }

            protected override void clickA()
            {
                VIEW.UI().manager.close();
                VIEW.world().UI.factions.open(f());
            }
        }
    }
}