using System;
using System.Collections.Generic;
using snake2d;
using util.data.INT;
using util.gui.misc;
using util.text;
using view.keyboard;
using view.main;
using view.sett.ui.room.prints;
using view.subview;
using view.tool;
using view.tool.PlacableMulti;
using static settlement.main.SETT;
using static init.sprite.UI.UI;

namespace view.sett.ui.bottom
{
    final class Options : SPanel
    {
        private static CharSequence ¤¤CopyArea = "Copy Area";

        static
        {
            D.ts(typeof(Options));
        }

        public Options()
        {
            D.gInit(this);

            body().setWidth(BButt.WIDTH * 2);
            GGrid grid = new GGrid(this, 2);

            {
                ACTION a = new ACTION
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().ui.copier.activate();
                    }
                };
                CLICKABLE c = new BButt(SPRITES.icons().l.copy, ¤¤CopyArea)
                {
                    protected override void clickA()
                    {
                        a.exe();
                    }
                };
                c = KeyButt.wrap(a, c, KEYS.SETT(), "COPY_SUPER", ¤¤CopyArea, "");
                SearchToolPanel.add(c, ¤¤CopyArea, "");
                grid.add(c);
            }
            make("COPY_ROOM", ROOMS().copy.copy(), grid);

            {
                CharSequence name = D.g("Planning");

                ACTION a = new ACTION
                {
                    public void exe()
                    {
                        SETT.JOBS().planMode.toggle();
                    }
                };

                CLICKABLE c = new BButt(UI.icons().l.suspend.twin(UI.icons().m.cog, DIR.NW, 1), name)
                {
                    protected override void clickA()
                    {
                        a.exe();
                    }

                    protected override void renAction()
                    {
                        selectedSet(SETT.JOBS().planMode.is());
                    }
                };
                c.hoverInfoSet(D.g("PlanningD", "When enabled, placed jobs will not be performed until manually activated by your grace."));
                c = KeyButt.wrap(a, c, KEYS.SETT(), "PLANNING_MODE", name, "");
                SearchToolPanel.add(c, name, "");
                grid.add(c);
            }

            make("REPAIR", JOBS().tool_repair, grid);
            make("ACTIVATE", JOBS().tool_activate, grid);
            make("DORMANT", JOBS().tool_dormant, grid);

            make("MAINTENANCE_ON", SETT.MAINTENANCE().enablePlacer, grid);
            make("MAINTENANCE_OFF", SETT.MAINTENANCE().enablePlacer.getUndo(), grid);

            make("DIAGONALIZE", TERRAIN().diagonal.placer, grid);
            make("SQUAREIFY", TERRAIN().diagonal.undo, grid);

            PlacableMulti ppundo = new PlacableMulti("")
            {
                public void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    SETT.FLOOR().floorundernot.set(tx, ty, true);
                }

                public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return !SETT.FLOOR().floorundernot.is(tx, ty) ? null : E;
                }

                public void updateRegardless(GameWindow window, AREA selected)
                {
                    SETT.OVERLAY().RODIFY.add();
                    base.updateRegardless(window, selected);
                }
            };

            PlacableMulti pp = new PlacableMulti(D.g("Roadify"), D.g("RoadifyD", "Allow buildings such as fences to visually try to match their tile to the roads around them."), SETT.FLOOR().defaultRoad.getIcon())
            {
                public void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    SETT.FLOOR().floorundernot.set(tx, ty, false);
                }

                public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return !SETT.FLOOR().floorundernot.is(tx, ty) ? E : null;
                }

                public void updateRegardless(GameWindow window, AREA selected)
                {
                    SETT.OVERLAY().RODIFY.add();
                    base.updateRegardless(window, selected);
                }

                public PLACABLE getUndo()
                {
                    return ppundo;
                }
            };

            make("RODIFY", pp, grid);

            {
                ACTION a = new ACTION
                {
                    public void exe()
                    {
                        VIEW.s().ui.prints.open();
                    }
                };

                CLICKABLE c = new BButt(SPRITES.icons().l.prints, UISavedPrints.¤¤title)
                {
                    protected override void clickA()
                    {
                        a.exe();
                    }
                };
                CharSequence desc = D.g("printsD", "Add and manage saved blueprints.");
                c.hoverInfoSet(desc);
                c = KeyButt.wrap(a, c, KEYS.SETT(), "SAVE_PRINT", UISavedPrints.¤¤title, "");
                SearchToolPanel.add(c, UISavedPrints.¤¤title, desc);
                grid.add(c);
            }

            make("UPGRADE_PLACE", new RoomUpgrader(), grid);

            {
                IntImp iii = new IntImp();
                ArrayListGrower<CLICKABLE> li = new ArrayListGrower<CLICKABLE>();

                for (int i = 0; i < SETT.JOBS().paintmap.max() - 1; i++)
                {
                    int k = i;
                    SPRITE c = k == 0 ? COLOR.WHITE10 : COLOR.UNIQUE.get(i).makeSprite(16, 16);
                    li.add(new GButt.ButtPanel(c)
                    {
                        protected override void clickA()
                        {
                            iii.set(k);
                        }

                        protected override void renAction()
                        {
                            selectedSet(iii.get() == k);
                        }
                    });
                }

                pp = new PlacableMulti(D.g("Paint-tool"), D.g("PlanToolD", "Paint the map in different colors. Has no impact on game-play"), SETT.FLOOR().defaultRoad.getIcon())
                {
                    public void place(int tx, int ty, AREA area, PLACER_TYPE type)
                    {
                        SETT.JOBS().paintmap.set(tx, ty, iii.get());
                    }

                    public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                    {
                        return null;
                    }

                    public void updateRegardless(GameWindow window, AREA selected)
                    {
                        SETT.OVERLAY().PAINTER.add();
                        base.updateRegardless(window, selected);
                    }

                    public PLACABLE getUndo()
                    {
                        return null;
                    }

                    public LIST<CLICKABLE> getAdditionalButt()
                    {
                        return li;
                    }
                };

                make("PLAN_PAINT", pp, grid);
            }

            pad(8, 8);
        }

        private void make(string code, PLACABLE p, GGrid grid)
        {
            make(code, p, grid, null);
        }

        private void make(string code, PLACABLE p, GGrid grid, ToolConfig con)
        {
            ACTION a = new ACTION
            {
                public void exe()
                {
                    VIEW.inters().popup.close();
                    if (con != null)
                        VIEW.s().tools.place(p, con);
                    else
                        VIEW.s().tools.place(p);
                }
            };
            CLICKABLE c = new BButt(p.getIcon(), p.name())
            {
                protected override void clickA()
                {
                    a.exe();
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    p.hoverDesc((GBox)text);
                }
            };
            c = KeyButt.wrap(a, c, KEYS.SETT(), code, p.name(), "");
            SearchToolPanel.add(c, p.name(), "");
            grid.add(c);
        }

        public GuiSection get()
        {
            return this;
        }
    }
}