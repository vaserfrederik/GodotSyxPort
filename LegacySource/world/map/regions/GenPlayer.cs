using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util;
using world;
using world.map.regions.centre;
using world.map.road;

namespace world.map.regions
{
    class GenPlayer
    {
        public static bool gen()
        {
            int pcx = -1;
            int pcy = -1;

            if (WORLD.REGIONS().player.is(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy()))
            {
                pcx = WORLD.REGIONS().player.cx();
                pcy = WORLD.REGIONS().player.cy();
            }
            else if (WORLD.GEN().playerX != -1)
            {
                pcx = WORLD.GEN().playerX;
                pcy = WORLD.GEN().playerY;
            }
            else
            {
                for (int i = 0; i < 1000; i++)
                {
                    int x = RND.rInt(WORLD.TWIDTH());
                    int y = RND.rInt(WORLD.THEIGHT());

                    if (WorldCentrePlacablity.terrainC(x, y) == null)
                    {
                        pcx = x;
                        pcy = y;
                        break;
                    }
                }
                if (pcx == -1)
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                    {
                        if (WorldCentrePlacablity.terrainC(c.x(), c.y()) == null)
                        {
                            pcx = c.x();
                            pcy = c.y();
                            break;
                        }
                    }
                }
            }

            if (pcx == -1)
                return false;

            fixWays(pcx, pcy);

            Rec pp = new Rec(WCentre.TILE_DIM);
            pp.moveC(pcx + 1, pcy + 1);
            bool pharbour = false;

            foreach (COORDINATE c in pp)
            {
                WORLD.REGIONS().pmap.set(c, WORLD.REGIONS().player);
                pharbour |= WORLD.WATER().isBig.is(c);
            }

            pharbour = false;

            if (pharbour)
            {
                GUTIL.flooder().init(GenPlayer.class);
                if (WTRAV.isGoodLandTile(pcx, pcy))
                {
                    GUTIL.flooder().pushSloppy(pcx, pcy, 0);
                }
                if (WORLD.WATER().isBig.is(pcx, pcy))
                {
                    for (int di = 0; di < DIR.ORTHO.size(); di++)
                    {
                        DIR d = DIR.ORTHO.get(di);
                        int dx = pcx + d.x();
                        int dy = pcy + d.y();
                        if (WTRAV.isGoodLandTile(dx, dy) && WTRAV.canLand(pcx, pcy, d, false))
                        {
                            GUTIL.flooder().pushSloppy(dx, dy, 0);
                        }
                    }
                }
                Rec cc = new Rec(WCentre.TILE_DIM + 2);
                cc.moveC(pp.cX(), pp.cY());
                while (GUTIL.flooder().hasMore())
                {
                    PathTile t = GUTIL.flooder().pollSmallest();
                    if (!pp.holdsPoint(t) && WTRAV.isHarbour(t.x(), t.y()))
                    {
                        int px = t.x();
                        int py = t.y();
                        while (t != null)
                        {
                            WORLD.REGIONS().pmap.set(t, WORLD.REGIONS().player);
                            t = t.getParent();
                        }
                        GUTIL.flooder().done();
                        GUTIL.flooder().init(GenPlayer.class);
                        GUTIL.flooder().pushSloppy(px, py, 0);
                        while (GUTIL.flooder().hasMore())
                        {
                            t = GUTIL.flooder().pollSmallest();
                            if (t.getValue() > 2)
                                continue;
                            WORLD.REGIONS().pmap.set(t, WORLD.REGIONS().player);
                            foreach (DIR d in DIR.ORTHO)
                            {
                                if (WORLD.WATER().isBig.is(t.x(), t.y(), d))
                                    GUTIL.flooder().pushSmaller(t, d, t.getValue() + 1, t);
                            }
                        }
                        break;
                    }

                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (cc.holdsPoint(t, d) && WTRAV.canLand(t.x(), t.y(), d, false))
                            GUTIL.flooder().pushSmaller(t, d, t.getValue() + 1, t);
                    }
                }

                GUTIL.flooder().done();
            }

            WORLD.REGIONS().player.info.centreSet(pcx, pcy);

            WORLD.REGIONS().player.info.name().clear().add(FACTIONS.player().name);
            return true;
        }

        private static void fixWays(int px, int py)
        {
            WORLD.TERRAIN().secretFixWays();
        }
    }
}