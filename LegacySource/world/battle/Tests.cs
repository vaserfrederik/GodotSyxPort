using System;
using System.Collections.Generic;
using game.battle.state;
using game.faction;
using init.race;
using init.resources;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.text;
using view.main;
using view.tool;
using view.world.panel;
using world;
using world.army;
using world.army.ADSupplies;
using world.battle.spec;
using world.map.regions;
using world.region;

class Tests
{
    public Tests()
    {
        IDebugPanelWorld.Add(new PlacableSimpleTile("battle create")
        {
            public override void Place(int tx, int ty)
            {
                WBattleSpec s = spec(tx, ty, null, null);
                VIEW.world().UI.battle.battle(s);
            }

            public override CharSequence IsPlacable(int tx, int ty)
            {
                return WORLD.IN_BOUNDS(tx, ty) ? null : E;
            }
        });

        IDebugPanelWorld.Add(new PlacableSimpleTile("battle assist")
        {
            public override void Place(int tx, int ty)
            {
                WBattleSpec s = spec(tx, ty, null, null);
                VIEW.world().UI.battle.assist(s);
            }

            public override CharSequence IsPlacable(int tx, int ty)
            {
                return WORLD.IN_BOUNDS(tx, ty) ? null : E;
            }
        });

        IDebugPanelWorld.Add(new PlacableSimpleTile("battle lastStand")
        {
            public override void Place(int tx, int ty)
            {
                Region reg = WORLD.REGIONS().map.Get(tx, ty);

                WBattleUnit ss = new WBattleUnit()
                {
                    public override CharSequence Name()
                    {
                        return reg.info.Name();
                    }

                    public override int Men()
                    {
                        return RD.MILITARY().garrison.Get(reg);
                    }

                    public override int LossesRetreat()
                    {
                        return Men();
                    }

                    public override int Losses()
                    {
                        return Men();
                    }

                    public override SPRITE Icon()
                    {
                        return UI.icons().m.building;
                    }

                    public override void Hover(GUI_BOX box)
                    {
                        box.Title(Dic.¤¤Region);
                    }

                    public override double Defences()
                    {
                        return RD.MILITARY().fort.GetD(reg);
                    }
                };

                WBattleSpec s = spec(tx, ty, ss, null);
                VIEW.world().UI.battle.lastStand(s);
            }

            public override CharSequence IsPlacable(int tx, int ty)
            {
                return WORLD.REGIONS().centreTile().Is(tx, ty) ? null : E;
            }
        });

        IDebugPanelWorld.Add(new PlacableSimpleTile("battle sally")
        {
            public override void Place(int tx, int ty)
            {
                Region reg = WORLD.REGIONS().map.Get(tx, ty);

                WBattleUnit ss = new WBattleUnit()
                {
                    public override CharSequence Name()
                    {
                        return reg.info.Name();
                    }

                    public override int Men()
                    {
                        return RD.MILITARY().garrison.Get(reg);
                    }

                    public override int LossesRetreat()
                    {
                        return 0;
                    }

                    public override int Losses()
                    {
                        return (int)(Men() * 0.5);
                    }

                    public override SPRITE Icon()
                    {
                        return UI.icons().m.building;
                    }

                    public override void Hover(GUI_BOX box)
                    {
                        box.Title(Dic.¤¤Region);
                    }

                    public override double Defences()
                    {
                        return 0;
                    }
                };

                WBattleSpec s = spec(tx, ty, ss, null);
                VIEW.world().UI.battle.sally(s);
            }

            public override CharSequence IsPlacable(int tx, int ty)
            {
                return WORLD.REGIONS().centreTile().Is(tx, ty) ? null : E;
            }
        });

        IDebugPanelWorld.Add(new PlacableSimpleTile("battle auto")
        {
            public override void Place(int tx, int ty)
            {
                WBattleSpec s = spec(tx, ty, null, null);
                s.auto();
            }

            public override CharSequence IsPlacable(int tx, int ty)
            {
                return WORLD.IN_BOUNDS(tx, ty) ? null : E;
            }
        });
    }

    private WBattleSpec spec(int tx, int ty, WBattleUnit p, WBattleUnit e)
    {
        WBattleSpec s = new WBattleSpec();

        double power = RND.rFloat();
        s.player = side(power, tx, ty, p);
        s.enemy = side(1.0 - power, tx + 1, ty, e);
        s.victory = power >= 0.5;
        return s;
    }

    private WBattleSide side(double power, int tx, int ty, WBattleUnit first)
    {
        ArrayList<WBattleUnit> us = new ArrayList<WBattleUnit>(1 + RND.rInt(10) + 1);

        int tloss = 0;
        int tlossRet = 0;
        int tmen = 0;

        if (first != null)
            us.Add(first);

        while (us.HasRoom())
        {
            CharSequence name = "" + RACES.all().Rnd().info.armyNames.Rnd();
            int men = 1 + RND.rInt(20000);
            int losses = 1 + RND.rInt(men);
            int lossesRetreat = RND.rInt(losses);
            int ri = RND.rInt();
            tloss += losses;
            tlossRet += lossesRetreat;
            tmen += men;

            SPRITE icon = FACTIONS.NPCs().GetC(ri).banner().MEDIUM;
            WBattleUnit u = new WBattleUnit()
            {
                public override void Hover(GUI_BOX box)
                {
                    box.Text(Dic.¤¤Accept);
                    box.NL();
                    box.Add(box.text().Add(men));
                }

                public override CharSequence Name()
                {
                    return name;
                }

                public override int Men()
                {
                    return men;
                }

                public override int Losses()
                {
                    return losses;
                }

                public override int LossesRetreat()
                {
                    return lossesRetreat;
                }

                public override SPRITE Icon()
                {
                    return icon;
                }

                public override double Defences()
                {
                    return 0;
                }
            };
            us.Add(u);
        }
        final int men = tmen;
        final int loss = tloss;
        final int lossR = tlossRet;
        WBattleSide a = new WBattleSide()
        {
            Coo coo = new Coo(tx, ty);

            public override COORDINATE Coo()
            {
                return coo;
            }

            public override int Men()
            {
                return men;
            }

            public override int Losses()
            {
                return loss;
            }

            public override int LossesRetreat()
            {
                return lossR;
            }

            public override LIST<WBattleUnit> Units()
            {
                return us;
            }

            public override int Artillery(ADArtillery a)
            {
                return 2;
            }

            public override double PowerBalance()
            {
                return power;
            }
        };

        return a;
    }

    private void init(WBattleResult a, WBattleSide s, int i)
    {
        int captured = RND.rInt(1 + s.Men());

        foreach (Race race in RACES.all())
        {
            if (captured == 0)
                break;
            a.capturedRaces[race.index()] = i * RND.rInt(captured);
            captured -= a.capturedRaces[race.index()];
        }

        foreach (RESOURCE res in RESOURCES.ALL())
        {
            if (RND.oneIn(5))
            {
                a.lostResources[res.index()] = i * RND.rInt(5000);
            }
        }
    }
}