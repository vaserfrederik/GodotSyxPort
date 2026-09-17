using System;
using init.constant;
using init.sprite;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.main;

namespace view.sett.ui.room.construction
{
    sealed class SFrame
    {
        private int width = C.SG * 470;
        private readonly State state;
        private readonly GPanel panel = new GPanel();
        private GuiSection section = new GuiSection();
        private readonly Str title = new Str(50);
        private readonly GuiSection bottomButtons = new GuiSection();

        static SFrame()
        {
            D.gInit(typeof(SFrame));
        }

        private readonly string sconstruction = D.g("{0} construction");
        private readonly string sexpensive = D.g("expensive", "The layout of the room will make it more expensive to construct and maintain. The yellow squares denote where support for the room is weak and will need extra materials. To increase support, remove some of the room in this area, so that it can be used to build supportive walls. Proceed anyway?");
        private bool message = false;

        public SFrame(State state)
        {
            this.state = state;
        }

        {
            GuiSection s = new GuiSection()
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    UI.PANEL().butt.render(r, body(), 0);
                    base.render(r, ds);
                }
            };

            s.addRightC(0, new GButt.Panel(SPRITES.icons().m.trash, D.g("removeRoom", "remove room"))
            {
                protected override void clickA()
                {
                    state.config.build = false;
                    VIEW.s().tools.placer.deactivate();
                }
            });

            s.addRightC(32, new GButt.Panel(SPRITES.icons().m.arrow_left, D.g("undo"))
            {
                protected override void clickA()
                {
                    SETT.ROOMS().placement.placer.popHistory();
                }

                protected override void renAction()
                {
                    activeSet(SETT.ROOMS().placement.placer.hasHistory());
                }
            });

            ACTION create = new ACTION()
            {
                public override void exe()
                {
                    SETT.ROOMS().placement.placer.create();
                    VIEW.s().tools.placer.deactivate();
                }
            };

            string cc = D.g("construct!");

            s.addRightC(0, new GButt.Panel(SPRITES.icons().m.ok)
            {
                protected override void clickA()
                {
                    string s = SETT.ROOMS().placement.placer.createProblem();
                    if (s != null)
                    {
                        if (SETT.ROOMS().placement.placer.createProblemItem() != null)
                        {
                            state.problemGroup = SETT.ROOMS().placement.placer.createProblemItem();
                            state.problemTimer = VIEW.renderSecond() + 4;
                        }
                        if (SETT.ROOMS().placement.placer.createProblemWalls())
                        {
                            state.problemTimer = VIEW.renderSecond() + 4;
                            state.problemneedDoor = true;
                        }
                    }
                    else
                    {
                        string warn = SETT.ROOMS().placement.placer.createWarning();
                        if (warn != null)
                        {
                            VIEW.inters().yesNo.activate(warn, create, ACTION.NOP, true);
                        }
                        else
                        {
                            if (!message && state.b.constructor().mustBeIndoors() && state.placement.placer.cost().support() > 0)
                            {
                                message = true;
                                VIEW.inters().yesNo.activate(sexpensive, create, ACTION.NOP, true);
                            }
                            else
                                create.exe();
                        }
                    }
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title(cc);
                    string s = SETT.ROOMS().placement.placer.createProblem();
                    if (s != null)
                    {
                        GBox b = (GBox)text;
                        b.error(s);
                    }
                    base.hoverInfoGet(text);
                }
            });

            bottomButtons.add(s);
        }

        public GuiSection get(GuiSection s)
        {
            section.clear();
            if (state.collection != null)
                title.clear().add(state.collection.name());
            else
                title.clear().add(sconstruction).insert(0, state.b.info.name);
            title.toUpper();

            if (s.body().width() < width)
                s.pad((width - s.body().width()) / 2, 0);
            panel.inner().set(s);
            panel.setTitle(title);
            section.add(panel);
            s.body().centerIn(panel.inner());
            section.add(s);

            if (state.b.constructor().usesArea())
            {
                bottomButtons.body().moveX2(section.body().x2() - 60);
                bottomButtons.body().moveCY(section.body().y2());
                section.add(bottomButtons);
            }

            return section;
        }
    }
}