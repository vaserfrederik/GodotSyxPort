using System;
using game.faction.npc;
using game.time;
using init.trade;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;

public class NPCStockpileDebugUI : GuiSection
{
    public int[] tamounts;

    public NPCStockpileDebugUI(FactionNPC faction)
    {
        int HEIGHT = 800;

        Add(table(faction, HEIGHT - 132));

        {
            GuiSection s = new GuiSection();

            s.AddRightC(2, new GButt.ButtPanel("update")
            {
                protected override void clickA()
                {
                    for (int i = 0; i < tamounts.Length; i++)
                    {
                        faction.stockpile.res(TR.ALL().Get(i)).inc(tamounts[i]);
                        tamounts[i] = 0;
                    }

                    faction.stockpile.update(faction, 0);
                }
            });

            s.AddRightC(2, new GButt.ButtPanel("update day")
            {
                protected override void clickA()
                {
                    for (int i = 0; i < tamounts.Length; i++)
                    {
                        faction.stockpile.res(TR.ALL().Get(i)).inc(tamounts[i]);
                        tamounts[i] = 0;
                    }
                    faction.stockpile.update(faction, TIME.secondsPerDay());
                }
            });

            s.AddRightC(2, new GButt.ButtPanel("clear")
            {
                protected override void clickA()
                {
                    faction.stockpile.saver().clear();
                    faction.stockpile.update(faction, 0);
                    for (int i = 0; i < tamounts.Length; i++)
                    {
                        tamounts[i] = 0;
                    }
                }
            });

            AddRelBody(2, DIR.S, s);
        }

        {
            GuiSection s = new GuiSection();
            s.AddRightC(2, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.perc(text, faction.stockpile.creditScore());
                }
            }.hh("score"));
            s.AddRightC(120, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, (long) faction.credits().credits());
                }
            }.hh("cash"));

            INTE inTE = new INTE()
            {
                public override int min()
                {
                    return 0;
                }

                public override int max()
                {
                    return 200000000;
                }

                public override int get()
                {
                    return (int) faction.credits().getD() + 100000000;
                }

                public override void set(int t)
                {
                    faction.credits().set(t - 100000000);
                }
            };

            s.AddRightC(120, new GSliderInt(inTE, 200, true));

            s.AddRightC(120, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, (long) faction.stockpile.credit());
                }
            }.hh("worth"));
            AddRelBody(2, DIR.S, s);
        }

        tamounts = Alloc.ii(TR.ALL().size());
    }

    private GuiSection table(FactionNPC faction, int height)
    {
        GuiSection section = new GuiSection();

        GTableBuilder bu = new GTableBuilder()
        {
            public override int nrOFEntries()
            {
                return TR.ALL().size();
            }
        };

        bu.column("", 32, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new RENDEROBJ.RenderImp(24)
                {
                    public override void render(SPRITE_RENDERER r, float ds)
                    {
                        TRADABLE re = TR.ALL().Get(ier.get());
                        re.icon().render(r, body);
                    }
                };
            }
        });

        bu.column("rate", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.rate());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("rateT", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.rateTot());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("traded", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.offset());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("amTarget", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.amountTarget());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("amTot", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.amountTarget() + res.offset());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("tradable", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.amount());
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("priceMul", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        GFORMAT.f(text, res.amMulAt(0));
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("priceB", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.f(text, faction.buyer(TR.ALL().Get(ier.get())).addPrice(1));
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("B-price", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                return new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.f(text, faction.seller(TR.ALL().Get(ier.get())).removePrice(1));
                    }
                }.r(DIR.NW);
            }
        });

        bu.column("trade", 100, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<Integer> ier)
            {
                INTE inTE = new INTE()
                {
                    public override int min()
                    {
                        NPCRes res = faction.stockpile.res(TR.ALL().Get(ier.get()));
                        return -(int) res.amount();
                    }

                    public override int max()
                    {
                        return 1000000;
                    }

                    public override int get()
                    {
                        return tamounts[ier.get()];
                    }

                    public override void set(int t)
                    {
                        tamounts[ier.get()] = t;
                    }
                };

                return new GInputInt(inTE);
            }
        });

        section.Add(bu.createHeight(height, true));
        faction.stockpile.update(faction, 0);
        return section;
    }
}