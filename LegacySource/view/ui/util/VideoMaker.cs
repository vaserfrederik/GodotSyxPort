using System;
using System.IO;
using System.Linq;
using game;
using init.paths;
using init.sprite;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.data.INT;
using util.gui.misc;
using view.interrupter;
using view.main;
using view.sett.ui.minimap;
using view.subview;
using world;

namespace view.ui.util
{
    public class VideoMaker
    {
        private readonly Placer start = new Placer("start area");
        private readonly Placer end = new Placer("end area");
        private readonly GuiSection s = new GuiSection();
        private readonly Mode world;
        private readonly Mode sett;
        private readonly Mode battle;

        private readonly IntImp duration = new IntImp();

        private Mode current;

        public VideoMaker()
        {
            world = new Mode()
            {
                Window = () => VIEW.world().window,
                Render = (bounds) =>
                {
                    WORLD.OVERLAY().hide();
                    bool t = WORLD.FOW().toggled.is();
                    WORLD.FOW().toggled.set(false);
                    GAME.world().render(CORE.renderer(), 0, 0, bounds, 0, 0);
                    WORLD.FOW().toggled.set(t);
                }
            };

            sett = new Mode()
            {
                Window = () => VIEW.s().getWindow(),
                Render = (bounds) =>
                {
                    GAME.s().render(CORE.renderer(), 0, 0, bounds, 0, 0, UIMinimapSettConfig.NORMAL);
                }
            };

            battle = new Mode()
            {
                Window = () => VIEW.b().getWindow(),
                Render = (bounds) =>
                {
                    GAME.s().render(CORE.renderer(), 0, 0, bounds, 0, 0, UIMinimapSettConfig.NORMAL);
                }
            };

            foreach (Placer p in new[] { start, end })
            {
                GuiSection rr = new GuiSection();

                rr.add(new GText(UI.FONT().S, p.name), 0, 0);

                rr.addRightCAbs(100, new GButt.ButtPanel(UI.icons().m.crossair)
                {
                    protected override void clickA()
                    {
                        p.set();
                    }
                });

                rr.addRightC(16, UI.FONT().S.getText("x1"));
                rr.addRightC(4, new GInputInt(p.x1));

                rr.addRightC(16, UI.FONT().S.getText("width"));
                rr.addRightC(4, new GInputInt(p.w));

                rr.addRightC(16, UI.FONT().S.getText("y1"));
                rr.addRightC(4, new GInputInt(p.y1));

                rr.addRightC(16, UI.FONT().S.getText("height"));
                rr.addRightC(4, new GInputInt(p.h));

                s.addDown(4, rr);
            }

            {
                GuiSection rr = new GuiSection();
                rr.addRightC(16, UI.FONT().S.getText("duration (ms)"));
                rr.addRightC(4, new GInputInt(duration));
                s.addDown(4, rr);
            }

            s.addRightC(32, new GButt.ButtPanel("action!")
            {
                protected override void clickA()
                {
                    start.rec.moveX1Y1(start.x1.get(), start.y1.get()).setDim(start.w.get(), start.h.get());
                    end.rec.moveX1Y1(end.x1.get(), end.y1.get()).setDim(end.w.get(), end.h.get());

                    string f = PATHS.local().VIDEO.get().toAbsolutePath() + Path.DirectorySeparatorChar + "frame";
                    SPRITES.loader().init();
                    new VIDEO_MAKER(start.rec, end.rec, duration.get(), f)
                    {
                        public override void render(RECTANGLE gamebounds)
                        {
                            current.render(gamebounds);
                        }

                        public override void renderProgress(int frame, int totFrames, double frameTime)
                        {
                            SPRITES.loader().print("frame " + (frame + 1) + "/" + (totFrames + 1));
                            GAME.update(frameTime);
                        }
                    };
                }
            });

            IDebugPanel.add("video maker", new ACTION()
            {
                public void exe()
                {
                    current = null;
                    if (VIEW.s().isActive())
                        current = sett;
                    else if (VIEW.b().isActive())
                        current = battle;
                    else if (VIEW.world().isActive())
                        current = world;
                    if (current == null)
                    {
                        LOG.ln("can't make video for view: " + VIEW.current());
                    }
                    VIEW.inters().popup.show(s, null);
                }
            });
        }

        private abstract class Mode
        {
            public abstract GameWindow Window();
            public abstract void Render(RECTANGLE bounds);
        }

        private class Placer : Interrupter
        {
            public readonly string name;
            private readonly Rec rec = new Rec();
            private readonly Rec tmp = new Rec();
            public readonly IntImp x1 = new IntImp();
            public readonly IntImp y1 = new IntImp();
            public readonly IntImp w = new IntImp();
            public readonly IntImp h = new IntImp();
            private bool clicked = false;
            private Coo start = new Coo();

            public Placer(string name)
            {
                this.name = name;
            }

            void set()
            {
                clicked = false;
                VIEW.inters().manager.add(this);
            }

            protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                return true;
            }

            protected override void mouseClick(MButt button)
            {
                if (button == MButt.LEFT && !clicked)
                {
                    clicked = true;
                    start.set(current.window().pixel().x(), current.window().pixel().y());
                }
                else
                {
                    hide();
                }
            }

            protected override void hoverTimer(GBox text)
            {
                // TODO Auto-generated method stub
            }

            protected override bool render(Renderer r, float ds)
            {
                if (!clicked)
                    return true;

                GameWindow w = current.window();

                int x1 = rec.x1();
                int y1 = rec.y1();

                x1 -= w.pixels().x1();
                y1 -= w.pixels().y1();

                x1 = x1 >> w.zoomout();
                y1 = y1 >> w.zoomout();

                x1 += w.viewWindow().x1();
                y1 += w.viewWindow().y1();

                tmp.moveX1Y1(x1, y1);
                tmp.setDim(rec.width() >> w.zoomout(), rec.height() >> w.zoomout());

                COLOR.GREEN100.renderFrame(r, tmp, 0, 3);

                return true;
            }

            protected override bool update(float ds)
            {
                if (clicked)
                {
                    rec.setDim(1);
                    rec.moveX1Y1(start);
                    rec.unify(current.window().pixel().x(), current.window().pixel().y());

                    int cs = rec.cX();
                    int cy = rec.cY();

                    double d = (double)CORE.getGraphics().nativeHeight / CORE.getGraphics().nativeWidth;
                    rec.setHeight(rec.width() * d);

                    rec.moveC(cs, cy);

                    if (!MButt.LEFT.isDown())
                    {
                        clicked = false;

                        x1.set(rec.x1());
                        y1.set(rec.y1());
                        w.set(rec.width());
                        h.set(rec.height());

                        hide();
                        VIEW.inters().popup.show(s, null);
                    }
                }

                return false;
            }
        }
    }
}