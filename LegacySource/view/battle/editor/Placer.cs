using System;
using System.Collections.Generic;
using static world.WORLD.MINIMAP;
using game.battle.state;
using game.battle.util;
using game.faction;
using game.time;
using init.constant;
using init.sprite.UI;
using menu;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.colors;
using util.data.INT;
using util.gui.misc;
using util.text;
using view.main;
using view.tool;
using view.world.generator;
using view.world.generator.tools;
using world;

class Placer : PlacableSimpleTile
{
    private static readonly CharSequence ¤¤select = "Select Location";
    private static readonly CharSequence ¤¤time = "time of day";

    static
    {
        D.ts(typeof(Placer));
    }

    private ArrayListGrower<CLICKABLE> butts = new ArrayListGrower<CLICKABLE>();
    private readonly UIWorldGenerateTerrain terrain = new UIWorldGenerateTerrain(WORLD.GEN());

    public readonly ACTION generate = new ACTION()
    {
        public override void exe()
        {
            int time = RND.rInt(100);
            TIME.set(TIME.secondsPerDay() * time / 100);
            WORLD.TERRAIN().saver().generate(WorldViewGenerator.loadPrint);
            WORLD.LANDMARKS().saver().generate(WorldViewGenerator.loadPrint);
            WorldViewGenerator.loadPrint.exe();
            MINIMAP().repaint();
            WorldViewGenerator.loadPrint.exe();
            WORLD.GEN().hasGeneratedTerrain = true;
            FACTIONS.otherFaction().bonus.clear();
        }
    };

    private readonly ArmySide player;
    private readonly ArmySide enemy;

    public Placer(ArmySide player, ArmySide enemy) : base(¤¤select)
    {
        this.player = player;
        this.enemy = enemy;

        terrain.addRelBody(2, DIR.S, new GButt.ButtPanel(Dic.¤¤Generate)
        {
            protected override void clickA()
            {
                generate.exe();
            }
        });

        butts.add(new GButt.ButtPanel(UI.icons().m.arrow_left)
        {
            protected override void clickA()
            {
                VIEW.b().editor.tools.placer.deactivate();
            }
        }.hoverInfoSet(Dic.¤¤Back));

        butts.add(new GButt.ButtPanel(UI.icons().m.terrain)
        {
            protected override void clickA()
            {
                VIEW.inters().popup.show(terrain, this);
            }
        }.hoverInfoSet(Dic.¤¤Generate));

        {
            INTE ii = new INTE()
            {
                public override int min()
                {
                    return 0;
                }

                public override int max()
                {
                    return 100;
                }

                public override int get()
                {
                    return CLAMP.i((int)(100 * TIME.currentSecond() / TIME.secondsPerDay()), 0, 100);
                }

                public override void set(int t)
                {
                    TIME.set(TIME.secondsPerDay() * t / 100.0);
                }
            };

            GSliderInt sl = new GSliderInt(ii, 100, false)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(¤¤time);
                }
            };
            sl.addRelBody(4, DIR.W, UI.icons().s.clock);
            sl.pad(8, 2);
            butts.add(sl);
        }
    }

    public override CharSequence isPlacable(int tx, int ty)
    {
        for (int i = 0; i < DIR.ORTHO.size(); i++)
        {
            if (BattleState.okWorldTile(tx, ty, DIR.ORTHO.get(i)))
                return null;
        }
        return Dic.empty;
    }

    public override void place(int tx, int ty)
    {
        BattleStateSpec spec = new BattleStateSpec();

        DIR d = DIR.ORTHO.rnd();

        for (int i = 0; i < DIR.ORTHO.size(); i++)
        {
            if (BattleState.okWorldTile(tx, ty, d))
                break;
            d = d.next(2);
        }

        set(player, spec.player, tx, ty);
        set(enemy, spec.enemy, tx + d.x(), ty + d.y());

        BattleStateExiter res = new BattleStateExiter()
        {
            public override void afterExit(BattleStateResult res)
            {
                CORE.setCurrentState(new CORE_STATE.Constructor()
                {
                    public override CORE_STATE getState()
                    {
                        return Menu.make();
                    }
                });
            }
        };

        BattleState.setGenerate(res, spec);
    }

    private void set(ArmySide s, SpecSide ss, int tx, int ty)
    {
        for (int i = 0; i < ss.artillery.Length; i++)
        {
            ss.artillery[i] = s.artillery[i];
        }
        ss.wCoo.set(tx, ty);
        ss.moraleBase = 1.0;
        foreach (DIV_SPEC d in s.divs)
        {
            ss.divs.add(new DivGeneration(d, d));
        }
    }

    public override void renderPlaceHolder(SPRITE_RENDERER r, int tx, int ty, int cx, int cy, bool isPlacable)
    {
        base.renderPlaceHolder(r, tx, ty, cx, cy, isPlacable);
        if (!isPlacable)
            GCOLOR.MAP().OK.bind();
        else
            GCOLOR.MAP().BAD.bind();

        int ri = (int)(C.TILE_SIZE * VIEW.renderSecond() * 0.5);
        ri %= C.TILE_SIZE;
        ri = C.TILE_SIZE - ri;
        foreach (DIR d in DIR.ORTHO)
        {
            UI.icons().s.chevron(d.perpendicular()).renderCScaled(r, cx + d.x() * C.TILE_SIZE + d.x() * ri, cy + d.y() * C.TILE_SIZE + d.y() * ri, C.SCALE);
        }

        COLOR.unbind();
    }

    public override LIST<CLICKABLE> getAdditionalButt()
    {
        return butts;
    }
}