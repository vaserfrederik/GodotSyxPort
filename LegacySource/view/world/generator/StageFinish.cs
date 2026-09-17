using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.text;
using view.main;
using view.tool;
using world;
using world.map.landmark;
using world.map.pathing;
using world.map.regions;

namespace view.world.generator
{
    class StageFinish
    {
        private static CharSequence ¤¤inspectRegions = "Inspect World";

        static StageFinish()
        {
            D.ts(typeof(StageFinish));
        }

        public StageFinish(WorldViewGenerator stages)
        {
            stages.minimap.show();
            LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();

            final Coo start = new Coo(-1, -1);

            final PlacableSimple simp = new PlacableSimple(¤¤inspectRegions)
            {
                public override void place(int x, int y)
                {
                }

                public override CharSequence isPlacable(int x, int y)
                {
                    return null;
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return butts;
                }

                public override void renderPlaceHolder(SPRITE_RENDERER r, int cx, int cy, bool isPlacable)
                {
                }

                public override void renderAction(int cx, int cy)
                {
                    if (start.x() >= 0)
                    {
                        int tx = cx / C.TILE_SIZE;
                        int ty = cy / C.TILE_SIZE;
                        WORLD.OVERLAY().path.add(start.x(), start.y(), tx, ty, Treaty.DUMMY);
                    }

                    // World.OVERLAY().landmarks().add();
                    // World.OVERLAY().regions().add();
                    base.renderAction(cx, cy);
                }

                public override void placeInfo(GBox b, int cx, int cy)
                {
                    int tx = cx / C.TILE_SIZE;
                    int ty = cy / C.TILE_SIZE;
                    Region reg = WORLD.REGIONS().map.get(tx, ty);

                    WORLD.OVERLAY().landmarks.add();

                    if (reg != null)
                    {
                        WORLD.OVERLAY().hoverBox(reg);

                        WORLD.OVERLAY().regionOutline.add(reg);
                        if (WORLD.REGIONS().isCentre.is(tx, ty))
                        {
                            VIEW.world().UI.regions.hover(reg, b);
                        }
                    }
                    else if (WORLD.LANDMARKS().setter.get(tx, ty) != null)
                    {
                        WorldLandmark m = WORLD.LANDMARKS().setter.get(tx, ty);
                        b.title(m.name);
                        b.text(m.description);
                        WORLD.OVERLAY().landmarks.hover(WORLD.LANDMARKS().setter.get(tx, ty));
                    }

                    b.NL(8);
                    b.add(b.text().add(tx).add(':').add(ty));
                    b.NL();
                }
            };

            butts.add(new GButt.ButtPanel(SPRITES.icons().m.arrow_left)
            {
                protected override void clickA()
                {
                    new StageCapitol(stages, true);
                }

                protected override void renAction()
                {
                }
            }.hoverInfoSet(Dic.¤¤Back));

            butts.add(new GButt.ButtPanel(new SPRITE.Twin(SPRITES.icons().m.terrain, SPRITES.icons().m.rotate))
            {
                protected override void clickA()
                {
                    StageCapitol.regenerate();
                }
            }.hoverInfoSet(WorldViewGenerator.¤¤regenerate));

            butts.add(new GButt.ButtPanel(SPRITES.icons().m.crossair)
            {
                protected override void clickA()
                {
                    stages.window.centerAtTile(WORLD.GEN().playerX, WORLD.GEN().playerY);
                }
            }.hoverInfoSet(WorldViewGenerator.¤¤home));

            butts.add(new GButt.ButtPanel(SPRITES.icons().m.arrow_right)
            {
                protected override void clickA()
                {
                    SPRITES.loader().minify(false, Dic.¤¤Generating);
                    GAME.factions().prime();
                    WORLD.ARMIES().saver().generate(WorldViewGenerator.loadPrint);
                    WORLD.GEN().isDone = true;
                    WORLD.FOW().toggled.set(true);
                    VIEW.world().activate();
                    VIEW.world().window.centererTile.set(FACTIONS.player().capitolRegion().cx(), FACTIONS.player().capitolRegion().cy());
                    GAME.s().CreateFromWorldMap(WORLD.GEN().playerX - 1, WORLD.GEN().playerY - 1, false);
                    GAME.saver().saveNew();
                    CORE.getInput().clearAllInput();
                }
            }.hoverInfoSet(Dic.¤¤OK));

            ToolConfig fixed = new ToolConfig()
            {
                public override bool back()
                {
                    return false;
                }

                public override void addUI(LISTE<RENDEROBJ> uis)
                {
                    stages.tools.placer.addStandardButtons(uis, false);
                }
            };

            stages.tools.place(simp, fixed);
        }
    }
}