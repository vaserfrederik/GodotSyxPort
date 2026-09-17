using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using view.main;
using view.tool;

namespace view.sett.ui.room.construction
{
    final class Config : ToolConfig
    {
        private readonly State s;
        public bool build = true;
        private GuiSection section = new GuiSection();
        private readonly SShape sshape;
        private readonly SMaterial sMaterial;
        private readonly SFrame frame;
        private readonly SItems items;
        private readonly SStats stats;
        private readonly SCollection coll;
        private readonly Separator sep1 = new Separator();
        private readonly Separator sep2 = new Separator();

        Config(State s)
        {
            this.s = s;
            sshape = new SShape(s);
            frame = new SFrame(s);
            items = new SItems(s);
            stats = new SStats(s);
            sMaterial = new SMaterial(s);
            coll = new SCollection(s);
        }

        public void addUI(LISTE<RENDEROBJ> uis)
        {
            if (s.b.constructor().overlay() != null && SETT.ROOMS().placement.placer.showOverlay.is())
            {
                s.b.constructor().overlay().add();
            }
            else if (s.b.constructor().isHeavy() && SETT.ROOMS().placement.placer.showFoundation.is())
            {
                SETT.OVERLAY().FOUNDATION.add();
            }

            section.clear();

            if (s.collection != null)
            {
                VIEW.s().tools.placer.stealButtons(section);
                section.addRelBody(12, DIR.N, coll.get());
            }
            else if (s.b.constructor().usesArea())
            {
                section.add(sshape.get());

                if (s.b.constructor().mustBeIndoors())
                {
                    section.addDownC(6, sMaterial.get());
                }

                section.addRelBody(12, DIR.E, sep1.get(section.body().height()));

                if (s.b.constructor().groups().size() > 0)
                {
                    section.addRelBody(12, DIR.E, items.get());
                    section.addRelBody(12, DIR.E, sep2.get(section.body().height()));
                }

                section.addRelBody(12, DIR.E, stats.get());

            }
            else
            {
                if (s.b.constructor().mustBeIndoors())
                {
                    section.addDownC(6, sMaterial.get());
                    section.addRelBody(12, DIR.E, sep1.get(section.body().height()));
                    if (s.b.constructor().groups().size() > 1)
                    {
                        section.addRelBody(12, DIR.E, items.getFlat());
                    }
                    else
                    {
                        section.addRelBody(12, DIR.E, items.getSingle());
                    }
                }
                else if (s.b.constructor().groups().size() > 1)
                {
                    section.addRelBody(12, DIR.E, items.getFlat());
                    if (s.b.constructor().overlay() != null)
                        section.addRelBody(8, DIR.E, sshape.buttOverlay);
                }
                else
                {
                    VIEW.s().tools.placer.addStandardButtons(uis, false);
                    if (s.b.constructor().overlay() != null)
                        section.addRelBody(8, DIR.E, sshape.buttOverlay);
                    return;
                }
            }

            GuiSection s = frame.get(section);

            s.body().moveCX(C.WIDTH() / 2);
            s.body().moveY1(C.SG * 80);
            if (VIEW.s().getWindow().tiles().y1() == 0)
            {
                s.body().moveY2(C.HEIGHT() - C.SG * 80);
            }
            if (VIEW.s().getWindow().tiles().x2() == SETT.TWIDTH)
            {
                s.body().moveX1(C.SG * 80);
            }
            uis.add(s);
        }

        public bool back()
        {
            if (SETT.ROOMS().placement.placer.popHistory())
                return false;

            if (!s.refurnishing && SETT.ROOMS().placement.placer.removeAllItems())
                return false;
            if (!s.refurnishing && SETT.ROOMS().placement.placer.removeArea())
                return false;
            return true;
        }

        public void update(bool UIHovered)
        {
            if (VIEW.renderSecond() > s.problemTimer)
            {
                s.problemGroup = null;
                s.problemneedArea = false;
                s.problemneedDoor = false;
            }
        }

        public void activateAction()
        {
        }

        public void deactivateAction()
        {
            if (s.refurnishing && build && SETT.ROOMS().placement.placer.createProblem() == null)
                SETT.ROOMS().placement.placer.create();
            SETT.ROOMS().placement.placer.init(null, 0);
            build = true;
        }

        private class Separator : SPRITE.Imp
        {
            public SPRITE get(int h)
            {
                this.height = h;
                this.width = 2;
                return this;
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                GCOLOR.UI().border(r, X1, X2, Y1, Y2);
            }
        }
    }
}