using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Faction;
using Game.Faction.Diplomacy;
using Init.Constant;
using Init.Race;
using Init.Resources;
using Init.Sprite;
using Settlement.Main;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui.Renderable;
using Util.Colors;
using Util.Data;
using Util.Gui.Misc;
using Util.Gui.Table;
using Util.Info;
using Util.Text;
using View.Interrupter;
using View.Main;
using View.Subview;
using View.Tool;
using View.World.Ui;
using World.Army;
using World.Entity.Army;

namespace View.World.Ui.Army
{
    final class List : ISidePanel
    {
        private Faction f = FACTIONS.Player();
        private static int width = 200;
        private readonly GTableBuilder builder;

        public List()
        {
            titleSet(Dic.¤¤Armies);

            {
                int ww = 200;

                section.Add(new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.iIncr(text, AD.conscripts().available(null).Get(f));
                    },
                    HoverInfoGet = (b) =>
                    {
                        b.Title(Dic.¤¤Conscriptable);
                        b.Text(Dic.¤¤ConscriptsD);
                        b.NL(8);

                        foreach (Race r in RACES.All())
                        {
                            if (r.Population().Max == 0)
                                continue;
                            b.Add(r.Appearance().Icon);
                            b.Add(GFORMAT.iIncr(b.Text(), AD.conscripts().available(r).Get(f)));
                            b.NL();
                        }
                    }
                }.Hh(Dic.¤¤Conscriptable, ww));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        int am = 0;
                        foreach (ResSupply res in RESOURCES.SUP().ALL)
                        {
                            am += SETT.ROOMS().SUPPLY.tally.amount.total(res.Resource);
                        }

                        GFORMAT.iIncr(text, am);
                    },
                    HoverInfoGet = (b) =>
                    {
                        b.Title(Dic.¤¤Supplies);
                        b.Text(Dic.¤¤SuppliesD);
                        b.Sep();

                        b.Tab(1);
                        b.TextLL(Dic.¤¤Current);
                        b.Tab(4);
                        b.TextLL(Dic.¤¤Needed);
                        b.Tab(7);
                        b.TextLL(Dic.¤¤Consumed);
                        b.Tab(10);
                        b.TextLL(Dic.¤¤Available);
                        b.NL();

                        RBITImp rs = new RBITImp();
                        rs.Clear();
                        foreach (ResSupply supp in RESOURCES.SUP().ALL)
                        {
                            b.Add(supp.Resource.Icon());
                            ADSupply sup = AD.supplies().Get(supp);
                            b.Tab(1);
                            b.Add(GFORMAT.i(b.Text(), sup.current().Faction(f)));
                            b.Tab(4);
                            b.Add(GFORMAT.i(b.Text(), sup.targetAmount(f)));
                            b.Tab(7);
                            b.Add(GFORMAT.f0(b.Text(), -sup.consumedPerDayCurrent(f)));
                            b.Tab(10);
                            b.Add(GFORMAT.i(b.Text(), SETT.ROOMS().SUPPLY.tally.amount.total(supp.Resource)));
                            b.NL();
                        }
                        b.NL(8);
                    }
                }.Hh(Dic.¤¤Supplies, ww));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, AD.stats().wins.F().Get(f));
                    }
                }.Hh(AD.stats().wins.Name, ww).HoverInfoSet(AD.stats().wins.Desc));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, AD.stats().siegeWon.F().Get(f));
                    }
                }.Hh(AD.stats().siegeWon.Name, ww).HoverInfoSet(AD.stats().siegeWon.Desc));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, AD.stats().defeats.F().Get(f));
                    }
                }.Hh(AD.stats().defeats.Name, ww).HoverInfoSet(AD.stats().defeats.Desc));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, AD.stats().kills.F().Get(f));
                    }
                }.Hh(AD.stats().kills.Name, ww).HoverInfoSet(AD.stats().kills.Desc));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, AD.stats().losses.F().Get(f));
                    }
                }.Hh(AD.stats().losses.Name, ww).HoverInfoSet(AD.stats().losses.Desc));

                section.AddDownC(4, new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.percInc(text, AD.stats().repF().GetD(f));
                    }
                }.Hh(AD.stats().repF().Info().Name, ww).HoverInfoSet(AD.stats().repF().Info().Desc));

                section.body().IncrW(80);
            }

            GButt.ButtPanel bb = new GButt.ButtPanel(Dic.¤¤Recruit)
            {
                p = new Placer(),

                RenAction = () =>
                {
                    activeSet(f.armies().canCreate());
                },

                ClickA = () =>
                {
                    last().Add(this, true);
                    if (FACTIONS.player().armies().all().Count == 0)
                    {
                        COORDINATE c = WORLD.PATH().Rnd(FACTIONS.player().capitolRegion());
                        int tx = c.X();
                        int ty = c.Y();
                        WArmy e = WORLD.ENTITIES().armies.create(tx, ty, FACTIONS.player());
                        VIEW.world().UI.armies.openList(e);
                    }
                    else
                        VIEW.world().tools.place(p);
                }
            };
            bb.body().setWidth(section.body().width());
            section.AddDownC(4, bb);

            builder = new GTableBuilder
            {
                Nr = f.armies().all().Count,
                NrColumns = 1,
                NrExtraColumns = 0,
                ElementConstructor = (index) => new Button(index)
            };
            section.Add(builder);
        }

        private class Button : GButton
        {
            private readonly int index;

            public Button(int index)
            {
                this.index = index;
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                WorldHoverer.hover(text, f.armies().all()[index]);
            }

            private WArmy g()
            {
                return f.armies().all()[index];
            }

            public override void clickA()
            {
                VIEW.world().UI.armies.openList(g(), last());
            }

            public override void renAction()
            {
                selectedSet(last().added(VIEW.world().UI.armies.army) && Army.army == g());
            }
        }

        private class Placer : PlacableSimpleTile
        {
            public Placer() : base(Dic.¤¤Recruit) { }

            public override CharSequence isPlacable(int tx, int ty)
            {
                if (!WORLD.PATH().map.is.is(tx, ty))
                    return Dic.¤¤Unreachable;
                if (WORLD.REGIONS().map.get(tx, ty) == null || WORLD.REGIONS().map.get(tx, ty).faction() != f)
                    return Dic.¤¤MustBeOwnRegion;
                return null;
            }

            public override void place(int tx, int ty)
            {
                WArmy e = WORLD.ENTITIES().armies.create(tx, ty, FACTIONS.player());
                VIEW.world().tools.place(null);
                VIEW.world().UI.armies.openList(e);
            }

            public override void renderOverlay(GameWindow window)
            {
                WORLD.OVERLAY().hoverArmy(FACTIONS.player());
            }
        }
    }
}