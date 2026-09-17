using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.faction.FResources;
using init.resources;
using init.sprite;
using init.trade;
using settlement.main;
using settlement.room.industry.module;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using world.map.regions;
using world.region;

namespace view.ui.goods
{
    final class Row : GuiSection
    {
        private static readonly int w = 5;
        private static int amount = FACTIONS.player().res().total().history(RESOURCES.ALL().get(0).tr()).historyRecords();
        private static readonly int height = 60;

        private readonly GStaples[] dias;
        private int hi;
        private readonly GETTER<RESOURCE> res;

        Row(GETTER<RESOURCE> r, Pop pop)
        {
            this.res = r;
            INTE im = new INT.IntImp();
            im.set(-1);

            add(new HOVERABLE.HoverableAbs(Icon.M * 2)
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    SPRITE s = res.get() == null ? SPRITES.icons().m.urn.big : res.get().icon();
                    s.render(r, body);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (res.get() != null)
                        res.get().hoverDetailed(text);
                    else
                        text.title(Dic.¤¤Total);
                }
            });

            dias = new GStaples[]
            {
                new StorageDiagram(r),
                new ProductionDiagram(r),
            };

            foreach (GStaples ss in dias)
            {
                addRelBody(4, DIR.E, ss);
            }
            addRelBody(4, DIR.E, prod(r, pop));
            pad(2, 6);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            hi = -1;
            foreach (GStaples ss in dias)
            {
                if (ss.hoveredIs())
                {
                    hi = ss.hoverI();
                }
            }
            if (hi >= 0)
            {
                foreach (GStaples ss in dias)
                {
                    ss.setHovered(hi);
                }
            }

            base.render(r, ds);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (hi < 0)
            {
                base.hoverInfoGet(text);
                return;
            }
            GBox b = (GBox)text;

            int si = amount - hi - 1;

            {
                GText t = b.text();
                t.lablify();
                DicTime.setAgo(t, si * GAME.player().res().time.bitSeconds());
                b.add(t);
                b.NL(4);
            }

            RESOURCE res = this.res.get();

            {
                b.textL(Dic.¤¤Stored);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), SETT.ROOMS().STOCKPILE.tally().amountsDay().history(res).get(si)));
                b.NL(8);
            }
            {
                FResources rr = FACTIONS.player().res();
                foreach (RTYPE t in RTYPE.all)
                {
                    b.add(b.text().normalify().add(t.name));
                    b.tab(6);
                    b.add(GFORMAT.iIncr(b.text(), rr.in(t).history(tr(res)).get(si)));
                    b.tab(8);
                    b.add(GFORMAT.iIncr(b.text(), -rr.out(t).history(tr(res)).get(si)));
                    b.NL();
                }

                b.NL(4);
                b.textL(Dic.¤¤Net);
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), GAME.player().res().total().history(tr(res)).get(si)));
            }

            {
                b.NL(8);
                si = amount - hi - 1;
                b.text(Dic.¤¤buyPrice);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesBuy.history(tr(res)).get(si)));
                b.NL();
                b.text(Dic.¤¤sellPrice);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesSell.history(tr(res)).get(si)));
                b.NL();
                if (res != null)
                {
                    b.text(Dic.¤¤avePrice);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), si == 0 ? FACTIONS.PRICE().get(tr(res)) : FACTIONS.player().trade.pricesAve.history(tr(res)).get(si)));
                    b.NL();
                }

                b.textL(Dic.¤¤Earnings);
                b.tab(6);
                b.add(GFORMAT.iIncr(b.text(), GAME.player().trade.inExported.history(tr(res)).get(si) - GAME.player().trade.outImported.history(tr(res)).get(si)));
                b.text(Dic.¤¤Curr);
            }
        }

        private static class StorageDiagram : GStaples
        {
            private readonly GETTER<RESOURCE> res;
            private GStat t = new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkNoColor(text, SETT.ROOMS().STOCKPILE.tally().amountTotal(res.get()), SETT.ROOMS().STOCKPILE.tally().space.total(res.get()));
                }
            }.bg();

            StorageDiagram(GETTER<RESOURCE> res) : base(amount)
            {
                this.res = res;
                body().setWidth(w * amount).setHeight(height);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                base.render(r, ds, hoveredIs());
                t.render(r, body().x1() + w, body().y1() + w / 2);
            }

            protected override double getValue(int stapleI)
            {
                double c = SETT.ROOMS().STOCKPILE.tally().space.total(res.get());
                double d = SETT.ROOMS().STOCKPILE.tally().amountsDay().history(res.get()).get(amount - 1 - stapleI);
                if (c == 0)
                    d = d > 0 ? 1 : 0;
                else
                    d /= c;

                return d;
            }

            protected override void setColor(Color color, int stapleI)
            {
                color.set(GCOLOR.UI().GOOD.normal);
            }
        }

        private static class ProductionDiagram : GStaples
        {
            private readonly GETTER<RESOURCE> res;
            private GStat t = new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkNoColor(text, SETT.ROOMS().STOCKPILE.tally().amountTotal(res.get()), SETT.ROOMS().STOCKPILE.tally().space.total(res.get()));
                }
            }.bg();

            ProductionDiagram(GETTER<RESOURCE> res) : base(amount)
            {
                this.res = res;
                body().setWidth(w * amount).setHeight(height);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                base.render(r, ds, hoveredIs());
                t.render(r, body().x1() + w, body().y1() + w / 2);
            }

            protected override double getValue(int stapleI)
            {
                double c = SETT.ROOMS().STOCKPILE.tally().space.total(res.get());
                double d = SETT.ROOMS().STOCKPILE.tally().amountsDay().history(res.get()).get(amount - 1 - stapleI);
                if (c == 0)
                    d = d > 0 ? 1 : 0;
                else
                    d /= c;

                return d;
            }

            protected override void setColor(Color color, int stapleI)
            {
                if (stapleI % 2 == 0)
                    color.set(GCOLOR.UI().GOOD.normal);
                else
                    color.set(GCOLOR.UI().BAD.normal);
            }
        }

        private static TRADABLE tr(GETTER<RESOURCE> g)
        {
            if (g.get() == null)
                return null;
            return g.get().tr();
        }

        private static TRADABLE tr(RESOURCE res)
        {
            if (res == null)
                return null;
            return res.tr();
        }

        private static RENDEROBJ prod(GETTER<RESOURCE> gres, Pop pop)
        {
            GStat s = new GStat()
            {
                public override void update(GText text)
                {
                    double tot = 0;
                    {
                        RESOURCE res = gres.get();
                        foreach (Source rr in SETT.ROOMS().PROD.producers(res))
                        {
                            if (rr.am() == 0)
                                continue;
                            tot += rr.am();
                        }
                        foreach (Source rr in SETT.ROOMS().PROD.consumers(res))
                        {
                            if (rr.am() == 0)
                                continue;
                            tot -= rr.am();
                        }
                    }

                    GFORMAT.iIncr(text, (long)tot);
                }
            };

            ClickableAbs c = new ClickableAbs()
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    if (gres.get() == null)
                        return;
                    GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                    GButt.ButtPanel.renderFrame(r, body);
                    s.renderC(r, body);
                    for (int i = 0; i < FACTIONS.player().realm().regions(); i++)
                    {
                        Region re = FACTIONS.player().realm().region(i);
                        if (RD.OUTPUT().get(tr(gres)).getDelivery(re) > 0)
                        {
                            return;
                        }
                    }
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body);
                    OPACITY.unbind();
                }

                protected override void clickA()
                {
                    if (gres.get() == null)
                        return;
                    for (int i = 0; i < FACTIONS.player().realm().regions(); i++)
                    {
                        Region re = FACTIONS.player().realm().region(i);
                        if (RD.OUTPUT().get(tr(gres)).getDelivery(re) > 0)
                        {
                            pop.res = gres.get();
                            VIEW.inters().popup.show(pop, this);
                            return;
                        }
                    }
                    base.clickA();
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (gres.get() == null)
                        return;
                    gres.get().hoverDetailed(text);
                }
            };
            c.body.setDim(64, 62);

            return c;
        }
    }
}