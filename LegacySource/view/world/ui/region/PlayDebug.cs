using game;
using game.boosting;
using game.faction;
using game.faction.npc;
using init.resources;
using init.sprite.UI;
using init.value;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.slider;
using view.ui.util;
using world.map.regions;
using world.region;
using world.region.building;
using world.region.pop;

namespace view.world.ui.region
{
    class PlayDebug : GuiSection
    {
        Region reg;

        public PlayDebug()
        {
            foreach (RDBuildPoint p in RD.BUILDINGS().costs.ALL)
            {
                padd(new GButt.ButtPanel(p.bo.name)
                {
                    protected override void clickA()
                    {
                        new RBooster(new BSourceInfo("cheat", null), 0, 2500, false)
                        {
                            Region rr = reg;

                            protected override double get(Region reg)
                            {
                                return reg == rr ? 1 : 0;
                            }
                        }.add(p.bo);
                    }
                });
            }

            padd(new GButt.Checkbox("claim")
            {
                protected override void renAction()
                {
                    selectedSet(RD.REALM(reg) == RD.REALM(FACTIONS.player()));
                }

                protected override void clickA()
                {
                    if (reg.capitol())
                    {
                        FACTIONS.remove((FactionNPC)reg.faction(), true);
                    }

                    RD.setFaction(reg, selectedIs() ? null : FACTIONS.player(), true);
                }
            });

            padd(new GButt.ButtPanel("affiliate")
            {
                protected override void clickA()
                {
                    RD.OWNER().affiliation.setD(reg, 1.0);
                }
            });

            {
                GuiSection s = new GuiSection();
                s.add(new GText(UI.FONT().S, "devastation"), 0, 0);
                INTE ii = new INTE()
                {
                    public override int min()
                    {
                        return 0;
                    }

                    public override int max()
                    {
                        return RD.DEVASTATION().current.max(null);
                    }

                    public override int get()
                    {
                        return RD.DEVASTATION().current.get(reg);
                    }

                    public override void set(int t)
                    {
                        RD.DEVASTATION().current.set(reg, t);
                    }
                };

                s.addRightC(8, new GSliderInt(ii, 100, false));
                padd(s);
            }

            padd(new GButt.ButtPanel("garrison")
            {
                protected override void clickA()
                {
                    RD.MILITARY().garrison.inc(reg, 50);
                }
            });

            padd(new GButt.ButtPanel("build")
            {
                protected override void clickA()
                {
                    RD.UPDATER().BUILD(reg);
                }
            });

            padd(new GButt.ButtPanel("boost")
            {
                TmpBoostSpec s = new TmpBoostSpec("_DEBUG", "PlayDebug", "PlayDebug", UI.icons().s.alert);
                {
                    s.spec.push(new BoosterValue(BValue.VALUE1, new BSourceInfo("", UI.icons().s.alert), 100, false), RD.OUTPUT().get(RESOURCES.WOOD().tr()).boost);
                    s.spec.push(new BoosterValue(BValue.VALUE1, new BSourceInfo("", UI.icons().s.alert), 4, true), RD.OUTPUT().get(RESOURCES.WOOD().tr()).boost);
                }

                protected override void clickA()
                {
                    GAME.BOOST().regions.toggle(reg, s);
                }
            });

            padd(new GButt.ButtPanel("pop ini")
            {
                protected override void clickA()
                {
                    foreach (RDRace rr in RD.RACES().all)
                        rr.pop.init(reg);
                }
            });

            padd(new GButt.ButtPanel("pop clear")
            {
                protected override void clickA()
                {
                    foreach (RDRace r in RD.RACES().all)
                    {
                        r.pop.set(reg, 0);
                    }
                }
            });

            padd(UIValues.butt(GVALUES.REGION, new GETTER<Region>()
            {
                public override Region get()
                {
                    return reg;
                }
            }));

            foreach (RDRace r in RD.RACES().all)
            {
                padd(new GButt.ButtPanel(r.race + "++")
                {
                    protected override void clickA()
                    {
                        r.pop.inc(reg, 100);
                    }
                });
            }
        }

        void padd(RENDEROBJ o)
        {
            if (getLastX2() > 600)
            {
                add(o, 0, body().y2());
            }
            else
            {
                addRightC(0, o);
            }
        }
    }
}