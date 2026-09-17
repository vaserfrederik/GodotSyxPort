using System;
using settlement.main;
using game.time;
using init.constant;
using settlement.entity;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.light;
using snake2d.util.misc;
using util.colors;
using util.gui.panel;
using view.main;
using view.subview;

namespace view.sett.ui.minimap
{
    final class UIMinimapPanel : ClickableAbs
    {
        private readonly int M = GFrame.MARGIN;
        private readonly int HEIGHT = 128;
        public const int WIDTH = 256;
        private readonly Rec tiles = new Rec(WIDTH - 6, HEIGHT - 6);

        private readonly Rec ents = new Rec(WIDTH - 6, HEIGHT - 6);
        private readonly GameWindow w;
        private readonly Coo lastClick = new Coo();
        private readonly UIMinimapSettConfig config;

        public UIMinimapPanel(GameWindow w, UIMinimapSettConfig config)
        {
            this.w = w;
            this.config = config;
            body.setDim(WIDTH, HEIGHT);
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            int x1, y1;

            int cx = w.tiles().cX();
            int cy = w.tiles().cY();

            x1 = cx - WIDTH / 2;
            y1 = cy - HEIGHT / 2;

            tiles.moveX1Y1(x1, y1);

            ents.moveX1Y1(x1, y1);

            CORE.renderer().newLayer(false, 0);
            AmbientLight.full.register(body);

            for (int ty = ents.y1(); ty < ents.y2(); ty++)
            {
                for (int tx = ents.x1(); tx < ents.x2(); tx++)
                {
                    ENTITY e = ENTITIES().getAtTileSingle(tx, ty);
                    if (e != null)
                    {
                        COLOR c = config.col(e);
                        if (c != null)
                        {
                            c.bind();
                            int x = tx - ents.x1() + body().x1() + M;
                            int y = ty - ents.y1() + body().y1() + M;
                            CORE.renderer().renderParticle(x, y);

                        }
                        tx = (tx + 4) & ~3;
                    }
                }
            }

            COLOR.unbind();

            CORE.renderer().newLayer(true, 0);
            SETT.MINIMAP().render(r, body().x1() + M, body().y1() + M, tiles);
            config.shade().bind();
            COLOR.BLACK.render(r, body);
            OPACITY.unbind();

            int w = this.w.tiles().width();
            int h = this.w.tiles().height();

            x1 = CLAMP.i(cx - w / 2 + body().x1() + M - tiles.x1(), body().x1() + M, body().x2() - M);
            y1 = CLAMP.i(cy - h / 2 + body().y1() + M - tiles.y1(), body().y1() + M, body().y2() - M);
            int x2 = CLAMP.i(x1 + w, body().x1() + M, body().x2() - M);
            int y2 = CLAMP.i(y1 + h, body().y1() + M, body().y2() - M);

            OPACITY.O25.bind();
            COLOR.WHITE100.render(r, x1, x2, y1, y2);
            OPACITY.unbind();

            CORE.renderer().newLayer(false, 0);

            TIME.light().applyGuiLight(ds, C.DIM());
            GCOLOR.UI().borderH(r, body(), 0);
        }

        protected override void clickA()
        {
            if (visableIs())
            {
                int tx = VIEW.mouse().x() - M - body().x1() + tiles.x1();
                int ty = VIEW.mouse().y() - M - body().y1() + tiles.y1();
                w.centerAtTile(tx, ty);
                lastClick.set(VIEW.mouse());
            }
        }
    }
}