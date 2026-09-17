using System;
using snake2d;
using util.datatypes;
using util.gui;
using util.gui.clickable;
using util.gui.misc;
using view.interrupter;
using view.keyboard;
using view.subview;

namespace view.sett.ui.minimap
{
    final class UIMiniMapSettView : Interrupter
    {
        private readonly Rec absBounds = new Rec(C.DIM());

        private readonly GuiSection ss = new GuiSection();
        private readonly GameWindow window = new GameWindow(C.DIM(), SETT.PIXEL_BOUNDS, 0).setzoomoutMax(6);

        bool hovered = false;

        private readonly UIMiniMapSettViewMap mini;

        private readonly GameWindow c;
        private readonly InterManager manager;

        public UIMiniMapSettView(UIMinimapSett m, InterManager i, GameWindow w, UIMinimapSettConfig config)
        {
            this.manager = i;
            persistantSet();
            int zoomout = 4;
            while ((PIXEL_BOUNDS.width() >> zoomout) > C.WIDTH() || (PIXEL_BOUNDS.height() >> zoomout) > C.HEIGHT())
                zoomout++;
            if (zoomout > 6)
                zoomout = 6;
            this.c = w;
            mini = new UIMiniMapSettViewMap(config);

            window.setZoomout(4);
            window.setzoomoutMax(zoomout);

            config.addButtons(ss, w, m);
            ss.body().moveY1(30);
            ss.body().moveX2(C.WIDTH() - 50);
        }

        public void addButt(CLICKABLE c)
        {
            ss.addRightC(0, c);
            ss.body().moveY1(30);
            ss.body().moveX2(C.WIDTH() - 50);
        }

        protected override void hoverTimer(GBox text)
        {
            ss.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            ss.render(r, ds);
            mini.render(r, ds, window, absBounds, window.pixel(), hovered);
            hovered = false;

            return false;
        }

        public void showMin()
        {
            window.setZoomout(4);
            show();
        }

        public void show()
        {
            if (window.zoomout() < 4)
            {
                window.setZoomout(4);
                hide();
            }
            window.setFromOther(c);

            base.show(manager);
        }

        public void showFull()
        {
            window.setZoomout(window.zoomoutmax());
            window.centerAt(c.pixels().cX(), c.pixels().cY());
            up();

            base.show(manager);
        }

        public override void hide()
        {
            base.hide();
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                if (hovered)
                {
                    window.zoomByMouse(4);
                    c.setFromOther(window);
                    hide();
                }
                else
                {
                    ss.click();
                }
            }
            else if (button == MButt.RIGHT)
            {
                hide();
            }
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            ss.hover(mCoo);
            hovered = window.pixel().isWithinRec(SETT.PIXEL_BOUNDS) && !ss.hoveredIs();
            if (!ss.hoveredIs())
                window.hover();

            return true;
        }

        protected override void deactivateAction()
        {
        }

        private void up()
        {
            mini.update();
        }

        protected override bool update(float ds)
        {
            if (window.zoomout() < 4)
            {
                c.setFromOther(window);
                hide();
            }

            if (KEYS.MAIN().MINIMAP.consumeClick())
            {
                hide();
                return true;
            }

            GAME.SPEED.poll();

            window.update(ds);
            up();

            return true;
        }
    }
}