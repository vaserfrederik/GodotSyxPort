using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using view.main;
using view.tool;

namespace settlement.job
{
    final class JobComboPlacer : ToolConfig
    {
        protected GuiSection section;
        private readonly GPanel panel = new GPanel();
        private GuiSection full = new GuiSection();
        private Job place;
        private readonly string selectKey;
        private readonly LIST<? extends Job> jobs;

        ACTION exit = new ACTION()
        {
            public void exe()
            {
                VIEW.s().tools.placer.deactivate();
            }
        };

        public ToolConfig get(Job j)
        {
            place = j;
            return this;
        }

        public Job current()
        {
            if (place == null || place.lockText() != null)
            {
                int i = PROP.propI(this.selectKey, 0);
                i = CLAMP.i(i, 0, jobs.size() - 1);
                place = jobs.get(i);
                if (place == null || place.lockText() != null)
                {
                    int ii = 0;
                    foreach (Job jj in jobs)
                    {
                        ii++;
                        if (jj.lockText() == null)
                        {
                            this.place = jj;
                            PROP.propISet(selectKey, ii);
                        }
                    }
                }
            }
            return place;
        }

        JobComboPlacer(LIST<? extends Job> jobs, string ss)
        {
            this.selectKey = "JOB_SELECTION_" + ss;
            this.jobs = jobs;
            this.section = new GuiSection();

            int in = 0;
            foreach (Job j in jobs)
            {
                final int inn = in;
                in++;

                GButt.ButtPanel b = new GButt.ButtPanel(j.placer().getIcon())
                {
                    protected override void clickA()
                    {
                        if (j.lockText() == null)
                        {
                            place = j;
                            PROP.propISet(selectKey, inn);
                            VIEW.s().tools.place(j.placer(), JobComboPlacer.this);
                        }
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        j.placer().hoverDesc(b);
                        if (j.lockText() != null)
                        {
                            b.NL(8);
                            b.error(j.lockText());
                        }
                    }

                    protected override void renAction()
                    {
                        selectedSet(VIEW.s().tools.placer.getCurrent() == j.placer());
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        base.render(r, ds, isActive, isSelected, isHovered);
                        if (j.lockText() != null)
                        {
                            OPACITY.O50.bind();
                            COLOR.BLACK.render(r, body);
                            OPACITY.unbind();
                        }
                    }
                };

                section.addRightC(0, b);
            }

            int i = PROP.propI(this.selectKey, 0);
            i = CLAMP.i(i, 0, jobs.size() - 1);
            place = jobs.get(i);

            foreach (Job jj in jobs)
                if (jj.lockText() == null)
                    this.place = jj;
        }

        public void addUI(LISTE<RENDEROBJ> uis)
        {
            full.clear();

            VIEW.s().tools.placer.stealButtons(full);
            if (place.placer().getAdditionalButt() != null)
                foreach (CLICKABLE p in place.placer().getAdditionalButt())
                    full.addRightC(0, p);
            full.body().centerX(C.DIM());
            full.addRelBody(C.SG * 8, DIR.N, section);

            panel.setButt();
            panel.inner().set(full);
            panel.clickActionSet(exit);
            full.add(panel);
            full.moveLastToBack();
            full.body().moveY1(90);
            uis.add(full);
        }
    }
}