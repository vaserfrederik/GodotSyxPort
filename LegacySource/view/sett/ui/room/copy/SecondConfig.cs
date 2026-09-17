using System;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.panel;
using util.rendering;
using util.text;
using view.main;
using view.tool;

namespace view.sett.ui.room.copy
{
    public class SecondConfig : ToolConfig
    {
        private readonly GuiSection section = new GuiSection();
        private readonly ON_TOP_RENDERABLE top;
        private readonly GPanel p = new GPanel();
        private readonly RENDEROBJ butts;
        private readonly CLICKABLE butt;
        private readonly First first;
        private readonly FirstConfig fConfig;
        private readonly ACTION exit = new ACTION()
        {
            public void exe()
            {
                VIEW.s().tools.placer.deactivate();
            }
        };

        public SecondConfig(Source source, First first, FirstConfig fConfig)
        {
            this.first = first;
            this.fConfig = fConfig;
            butt = new GButt.Panel(SPRITES.icons().m.arrow_left)
            {
                protected override void clickA()
                {
                    VIEW.s().tools.place(first, fConfig);
                }

                protected override void renAction()
                {
                    activeSet(false);
                    foreach (COORDINATE c in source.area())
                    {
                        if (source.is(c))
                        {
                            activeSet(true);
                            return;
                        }
                    }
                }
            };

            butts = new RENDEROBJ.RenderImp(32, 32)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    UI.PANEL().butt.render(r, body, 0);
                }
            };

            top = new ON_TOP_RENDERABLE()
            {
                public override void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
                {
                    RenderIterator it = data.onScreenTiles();
                    while (it.has())
                    {
                        if (source.is(it.tile()))
                        {
                            int m = 0;
                            foreach (DIR d in DIR.ORTHO)
                            {
                                if (source.is(it.tx(), it.ty(), d))
                                    m |= d.mask();
                            }
                            SPRITES.cons().BIG.dashed.render(r, m, it.x(), it.y());
                        }
                        it.next();
                    }
                    top.remove();
                }
            };
        }

        public override void addUI(LISTE<RENDEROBJ> uis)
        {
            section.clear();

            VIEW.s().tools.placer.stealButtons(section);

            section.pad(20, 0);

            section.addRelBody(4, DIR.S, butts);
            butt.body().centerIn(butts);
            section.add(butt);

            section.body().centerX(C.DIM());

            p.setButt();
            p.inner().set(section);

            p.setCloseAction(exit);
            p.setTitle(Dic.¤¤Copy);
            section.add(p);
            section.moveLastToBack();
            section.body().moveY1(100);
            section.body().centerX(C.DIM());

            uis.add(section);
        }

        public override void update(bool UIHovered)
        {
            top.add();
        }

        public override bool back()
        {
            VIEW.s().tools.place(first, fConfig);
            return false;
        }
    }
}