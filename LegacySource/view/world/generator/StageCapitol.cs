using System;
using System.Collections.Generic;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.tool;
using view.world.generator.tools;
using world.map.regions.centre;

class StageCapitol
{
    private static string ¤¤name = "Place Capital";
    static string ¤¤none = "¤Settling in this location is not possible.";

    static StageCapitol()
    {
        D.ts(typeof(StageCapitol));
    }

    public StageCapitol(WorldViewGenerator stages, bool clear)
    {
        if (clear)
        {
            WorldViewGenerator.loadPrint.exe();
            clear();
            MINIMAP().repaint();
        }
        stages.minimap.show();

        GuiSection butts = new GuiSection();

        butts.add(new GButt.ButtPanel(new SPRITE.Twin(SPRITES.icons().m.terrain, SPRITES.icons().m.rotate))
        {
            protected override void clickA()
            {
                WORLD.GEN().seed = RND.rInt(int.MaxValue);
                WORLD.TERRAIN().saver().generate(WorldViewGenerator.loadPrint);
                WORLD.LANDMARKS().saver().generate(WorldViewGenerator.loadPrint);
                WorldViewGenerator.loadPrint.exe();
                MINIMAP().repaint();
                WorldViewGenerator.loadPrint.exe();
            }

            protected override void renAction()
            {
                WORLD.OVERLAY().landmarks.add();
            }
        }.hoverInfoSet(WorldViewGenerator.¤¤regenerate));

        butts.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.admin)
        {
            protected override void clickA()
            {
                new StageEdit(stages);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(StageEdit.¤¤name);
            }
        }.hoverInfoSet(Dic.¤¤Terrain));

        if (WORLD.GEN().playerX > -1)
        {
            butts.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.arrow_right)
            {
                protected override void clickA()
                {
                    stages.set();
                }
            }.hoverInfoSet(Dic.¤¤Next));
        }

        PlacableFixedImp t = new PlacableFixedImp(¤¤name, 1, 1)
        {
            final UIWorldToolCapitolPlaceInfo info = new UIWorldToolCapitolPlaceInfo();

            public override int width()
            {
                return WCentre.TILE_DIM;
            }

            public override void place(int tx, int ty, int rx, int ry)
            {
                // TODO Auto-generated method stub
            }

            public override void afterPlaced(int tx1, int ty1)
            {
                int cx = tx1 + WCentre.TILE_DIM / 2;
                int cy = ty1 + WCentre.TILE_DIM / 2;
                stages.reset();
                generate(cx, cy);
                stages.set();
            }

            public override string placableWhole(int tx1, int ty1)
            {
                string p = WorldCentrePlacablity.terrain(tx1, ty1);
                if (p != null)
                    return p;
                return null;
            }

            public override int height()
            {
                return WCentre.TILE_DIM;
            }

            public override LIST<CLICKABLE> getAdditionalButt()
            {
                return null;
            }

            public override string placable(int tx, int ty, int rx, int ry)
            {
                // TODO Auto-generated method stub
                return null;
            }

            public override void placeInfo(GBox b, int x1, int y1)
            {
                info.placeInfo(b, x1, y1, FACTIONS.player().race());
            }
        };

        GPanel p = new GPanel(260, butts.body().height()).setButt();
        p.setTitle(¤¤name);
        p.inner().set(butts);
        butts.add(p);
        butts.moveLastToBack();
        butts.body().moveY1(64).centerX(C.DIM());

        ToolConfig fixed = new ToolConfig()
        {
            public override bool back()
            {
                return false;
            }

            public override void addUI(LISTE<RENDEROBJ> uis)
            {
                uis.add(butts);
            }
        };

        stages.tools.place(t, fixed);
    }

    private void generate(int cx, int cy)
    {
        WORLD.GEN().playerX = cx;
        WORLD.GEN().playerY = cy;
        generate();
    }

    static void regenerate()
    {
        WorldViewGenerator.loadPrint.exe();
        int px = WORLD.GEN().playerX;
        int py = WORLD.GEN().playerY;
        clear();
        WORLD.GEN().playerX = px;
        WORLD.GEN().playerY = py;
        generate();
    }

    public static void generate()
    {
        WORLD.OVERLAY().regNames.active.set(false);
        WorldViewGenerator.loadPrint.exe();
        WORLD.BUILDINGS().saver().generate(WorldViewGenerator.loadPrint);
        WorldViewGenerator.loadPrint.exe();

        WORLD.REGIONS().saver().generate(WorldViewGenerator.loadPrint);
        WORLD.ROADS().saver().generate(WorldViewGenerator.loadPrint);
        WORLD.PATH().saver().generate(WorldViewGenerator.loadPrint);
        WORLD.ENTITIES().saver().generate(WorldViewGenerator.loadPrint);
        WORLD.RD().saver().generate(WorldViewGenerator.loadPrint);

        WorldViewGenerator.loadPrint.exe();
        MINIMAP().repaint();
        WORLD.initBeforePlay();
        WORLD.OVERLAY().regNames.active.set(true);
    }

    public static void clear()
    {
        WorldViewGenerator.loadPrint.exe();
        WORLD.GEN().playerX = -1;
        WORLD.GEN().playerY = -1;
        WORLD.BUILDINGS().saver().clear();
        WORLD.ROADS().saver().clear();
        WORLD.PATH().saver().clear();
        WORLD.ENTITIES().saver().clear();
        WORLD.REGIONS().saver().clear();
        WORLD.RD().saver().clear();
    }
}