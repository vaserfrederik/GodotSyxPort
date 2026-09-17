using System;
using System.Collections.Generic;
using world.map.regions;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using util;
using world;
using world.overlay;

namespace world.map.road
{
    final class GenPortBuild
    {
        private readonly MAP_DOUBLE cost;

        GenPortBuild(ACTION util, MAP_DOUBLE cost, MAP_BOOLEAN marked, MAP_BOOLEAN bridge)
        {
            this.cost = cost;
            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    if (marked.is(it.tile()))
                    {
                        COLOR.BLUEISH.bind();
                        SPRITES.cons().ICO.crosshair.render(r, it.x(), it.y());
                        COLOR.unbind();
                    }
                    else if (bridge.is(it.tile()))
                    {
                        COLOR.ORANGE100.bind();
                        SPRITES.cons().BIG.line.render(r, 0, it.x(), it.y());
                        COLOR.unbind();
                    }
                }
            };

            int i = 0;

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (marked.is(c))
                {
                    i++;
                    if (i % 10 == 0)
                        util.exe();
                    build(c, false);
                }
                else if (bridge.is(c))
                {
                    WORLD.ROADS().set(c, true);
                    if (WORLD.ROADS().canBridge.is(c))
                        WORLD.ROADS().bridge.set(c, true);
                    build(c, true);
                }
            }
        }

        void build(COORDINATE start, bool mini)
        {
            Flooder f = GUTIL.flooder();
            f.init(this);
            f.pushSloppy(start.x(), start.y(), 0, null);
            Region home = WORLD.REGIONS().map.get(start);

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                Region r = WORLD.REGIONS().map.get(t);
                if (r != home)
                    continue;
                if (t.isSameAs(r.cx(), r.cy()))
                {
                    if (mini)
                    {
                        if (!WORLD.ROADS().is(t))
                        {
                            WORLD.ROADS().set(t, true);
                            WORLD.ROADS().minified.set(t, true);
                        }
                        while (t != null)
                        {
                            if (!WORLD.ROADS().is(t) && WORLD.ROADS().placable.is(t))
                            {
                                WORLD.ROADS().set(t, true);
                                WORLD.ROADS().minified.set(t, true);
                            }
                            t = t.getParent();
                        }
                    }
                    else
                    {
                        WORLD.ROADS().set(start, true);
                        WTRAV.makeRoad(t);
                        f.done();
                    }
                    f.done();
                    return;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WTRAV.canLand(t.x(), t.y(), d, false))
                    {
                        int dx = t.x() + d.x();
                        int dy = t.y() + d.y();
                        double v = cost.get(dx, dy) + WTRAV.cost(t.x(), t.y(), d);
                        if (WTRAV.canLand(t.x(), t.y(), d, true))
                            v *= 0.5;
                        f.pushSmaller(dx, dy, t.getValue() + v * d.tileDistance(), t);
                    }
                }
            }

            f.done();
        }
    }
}