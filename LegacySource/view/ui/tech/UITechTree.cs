using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.text;
using view.ui.manage;

namespace view.ui.tech
{
    public class UITechTree : IFullView
    {
        // final Tree tr2ee;
        private readonly Search search;
        private readonly InfoBonuses bonuses;
        private readonly Prompt prompt = new Prompt();
        private readonly CLICKABLE.ClickSwitch swit;
        private GuiSection currentTree;

        private readonly StringInputSprite filter = new StringInputSprite(16, UI.FONT().S)
        {
            protected override void change()
            {
                if (text() == null || text().Length == 0)
                    swit.set(currentTree);
                else
                    swit.set(search.set(text()));
            }
        };

        public UITechTree()
            : base(PTech.¤¤name, UI.icons().l.vial)
        {
            filter.placeHolder(Dic.¤¤Filter);
            Info info = new Info(this, WIDTH);
            {
                GuiSection trees = new GuiSection();
                foreach (TechTree t in TECHS.TREES())
                {
                    GuiSection tree = new Tree(t, HEIGHT - info.body().height() - 48, WIDTH);
                    if (currentTree == null)
                        currentTree = tree;
                    trees.addRightC(0, new GButt.ButtPanel(t.icon)
                    {
                        protected override void clickA()
                        {
                            currentTree = tree;
                            swit.set(tree);
                            filter.text().clear();
                            base.clickA();
                        }

                        protected override void renAction()
                        {
                            selectedSet(swit.current() == tree);
                        }
                    }.setDim(50, 40).hoverTitleSet(t.name));
                }
                trees.addRightC(8, filter());
                GuiSection ss = new GuiSection()
                {
                    public override void render(SPRITE_RENDERER r, float ds)
                    {
                        GCOLOR.UI().border().renderFrame(r, 0, C.WIDTH(), body().y1(), body().y2(), 0, 2);
                        base.render(r, ds);
                    }
                };
                ss.body().setDim(WIDTH + 32, trees.body().height() + 2);
                trees.body().centerIn(ss);
                ss.add(trees);
                ss.pad(8, 4);
                ss.body().moveY1(info.body().y2() + 4);
                ss.body().moveCX(WIDTH / 2);
                info.add(ss);
            }

            bonuses = new InfoBonuses(this, HEIGHT - (info.body().height() + 16), WIDTH);
            search = new Search(info.body().height() + 16, WIDTH);
            //tree = new Tree(HEIGHT - (info.body().height() + 16), WIDTH);
            swit = new CLICKABLE.ClickSwitch(currentTree);
            section.add(swit, 0, 0);

            bonuses.body().moveX1(section.body().x2() + 8);
            bonuses.body().moveY1(section.body().y1());

            section.addRelBody(16, DIR.N, info);
        }

        private GuiSection filter()
        {
            GuiSection s = new GuiSection();

            GInput in = new GInput(this.filter);
            s.add(in);

            CLICKABLE boosts = new GButt.ButtPanel("+++")
            {
                protected override void clickA()
                {
                    bool bo = bonuses();
                    filter.set(Dic.empty);
                    bonuses(bo ? false : true);
                }

                protected override void renAction()
                {
                    selectedSet(bonuses());
                }
            };
            boosts.hoverInfoSet(Dic.¤¤Boosts);

            s.addRightC(4, boosts);
            s.pad(32, 4);
            return s;
        }

        public void filter(CharSequence ss)
        {
            filter.set(ss);
        }

        public override bool back()
        {
            if (swit.current() != currentTree)
            {
                filter.set(Dic.empty);
                return true;
            }
            return base.back();
        }

        private void bonuses(bool b)
        {
            if (b)
                swit.set(bonuses);
            else
                swit.set(currentTree);
        }

        private bool bonuses()
        {
            return swit.current() == bonuses;
        }
    }
}