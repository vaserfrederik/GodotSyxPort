using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.panel;
using util.gui.table;
using util.text;
using view.interrupter;
using view.main;
using world;
using world.log;

namespace view.ui.log
{
    public sealed class UILog : Interrupter
    {
        public static readonly string ¤¤name = "World log";
        private readonly GuiSection section;
        private const int ww = 450;

        static UILog()
        {
            D.ts(typeof(UILog));
        }

        public UILog(VIEW view)
        {
            section = new GuiSection();

            GTableBuilder builder = new GTableBuilder
            {
                NrOFEntries = () => WORLD.LOG().all().Count
            };

            builder.Column(null, ww, new GRowBuilder
            {
                Build = (GETTER<int> ier) => new Entry(ier)
            });

            section.Add(builder.CreateHeight(700, false));

            GPanel p = new GPanel().SetBig();
            p.Set(section.body());

            p.SetCloseAction(() => hide());
            p.body().centerY(C.DIM());
            p.body().moveX2(C.WIDTH() - 20);
            section.body().centerIn(p);
            section.Add(p);
            section.moveLastToBack();
            p.setTitle(¤¤name, UI.FONT().H2);
        }

        public void activate()
        {
            show(VIEW.inters().manager);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            section.hover(mCoo);
            return true;
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                section.click();
            if (button == MButt.RIGHT)
                hide();
        }

        protected override void hoverTimer(GBox text)
        {
            section.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            section.render(r, ds);
            return true;
        }

        protected override bool update(float ds)
        {
            return true;
        }

        private class Entry : ClickableAbs
        {
            private readonly GETTER<int> ier;
            private static readonly Str tmp = new Str(128);

            public Entry(GETTER<int> ier) : base(ww, UI.FONT().M.height() * 2 + 30 + 16)
            {
                this.ier = ier;
            }

            private LogEntry e()
            {
                if (ier.get() >= WORLD.LOG().all().Count)
                    return null;
                return WORLD.LOG().all()[WORLD.LOG().all().Count - 1 - ier.get()];
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                LogEntry e = e();
                if (e == null)
                    return;

                if (isHovered)
                    COLOR.WHITE15.render(r, body);

                {
                    int x1 = body.x1() + 16;
                    int cy = body.y1() + 20;

                    e.icon().renderCY(r, x1, cy);

                    if (e.bannerA() != null)
                    {
                        e.bannerA().MEDIUM.renderCY(r, x1 + 24, cy);
                    }
                    if (e.bannerB() != null)
                    {
                        e.bannerB().MEDIUM.renderCY(r, x1 + 50, cy);
                    }

                    tmp.clear();
                    DicTime.setDateShort(tmp, e.daySinceStart() * TIME.secondsPerDay());

                    GCOLOR.T().H1.bind();
                    UI.FONT().H2.render(r, tmp, x1 + 76, cy - UI.FONT().H2.height() / 2);
                    COLOR.unbind();
                }

                {
                    int x1 = body.x1();
                    int y1 = body.y1() + 32;
                    UI.FONT().M.renderIn(r, x1, y1, DIR.NW, e.message, body.width(), body.y2() - y1 - 8, 1);
                }

                GCOLOR.UI().border().render(r, body().x1(), body().x2(), body().y2() - 1, body().y2());
            }

            protected override void clickA()
            {
                LogEntry e = e();
                if (e == null)
                    return;
                VIEW.world().activate();
                VIEW.world().window.centererTile.set(e.tx(), e.ty());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                LogEntry e = e();
                if (e == null)
                    return;
                text.text(e.message);
            }
        }
    }
}