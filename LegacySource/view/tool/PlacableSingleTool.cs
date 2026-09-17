using System;
using System.Collections.Generic;
using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util;
using util.colors;
using view.main;
using view.subview;

namespace view.tool
{
    final class PlacableSingleTool : placeFunc
    {
        private PlacableSingle placable;

        override void updateHovered(float ds, GameWindow window, bool pressed)
        {
        }

        override void render(SPRITE_RENDERER r, float ds, GameWindow window)
        {
            int tx = window.tile().x();
            int ty = window.tile().y();

            placable.init(tx, ty);

            CharSequence problem = placable.isPlacable(tx, ty);

            if (problem == null)
            {
                int t = 0;
                GUTIL.filler().init(this);
                GUTIL.filler().fill(tx, ty);
                while (GUTIL.filler().hasMore())
                {
                    COORDINATE c = GUTIL.filler().poll();
                    t++;
                    int mask = 0;
                    foreach (DIR d in DIR.ORTHO)
                    {
                        int dx = c.x() + d.x();
                        int dy = c.y() + d.y();
                        if (!SETT.IN_BOUNDS(dx, dy))
                            continue;
                        if (dx == tx && dy == ty)
                        {
                            mask |= d.mask();
                        }
                        else if (GUTIL.filler().isFilled(dx, dy) || (placable.isPlacable(dx, dy) == null && placable.expandsTo(c.x(), c.y(), dx, dy)))
                        {
                            mask |= d.mask();
                            GUTIL.filler().fill(dx, dy);
                        }
                    }
                    render(r, mask, c.x(), c.y(), true, window);
                }
                GUTIL.filler().done();
                placable.placeInfo(VIEW.hoverBox(), t);
            }
            else
            {
                render(r, 0, tx, ty, false, window);
                VIEW.hoverBox().error(problem);
            }
            COLOR.unbind();
        }

        private void render(SPRITE_RENDERER r, int mask, int tx, int ty, bool placable, GameWindow window)
        {
            if (placable)
                GCOLOR.MAP().OK.bind();
            else
                GCOLOR.MAP().BAD.bind();
            int x = (tx - window.tile().x()) * C.TILE_SIZE + window.tile().rel().x();
            int y = (ty - window.tile().y()) * C.TILE_SIZE + window.tile().rel().y();
            this.placable.renderPlaceHolder(r, mask, x, y, tx, ty, placable);
        }

        override void click(GameWindow window)
        {
            int tx = window.tile().x();
            int ty = window.tile().y();

            CharSequence problem = placable.isPlacable(tx, ty);
            if (problem != null)
                return;

            placable.placeFirst(tx, ty);

            GUTIL.filler().init(this);
            GUTIL.filler().fill(tx, ty);
            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                placable.placeExpanded(c.x(), c.y());
                foreach (DIR d in DIR.ORTHO)
                {
                    int dx = c.x() + d.x();
                    int dy = c.y() + d.y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;
                    if (dx == tx && dy == ty)
                        continue;
                    if ((placable.isPlacable(dx, dy) == null && placable.expandsTo(c.x(), c.y(), dx, dy)))
                    {
                        GUTIL.filler().fill(dx, dy);
                    }
                }
            }
            GUTIL.filler().done();
        }

        override void activate(PLACABLE placer, GameWindow window)
        {
            placable = (PlacableSingle)placer;
        }

        override void clickRelease(GameWindow window)
        {
        }

        override LIST<CLICKABLE> gui()
        {
            return null;
        }
    }
}