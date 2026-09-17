using System.Collections.Generic;
using util.rendering;
using world.map.pathing;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.rendering;
using world;
using world.map.regions;
using world.map.road;

class Gen
{
    public void generateAll(int px, int py, ACTION astep)
    {
        WORLD.PATH().saver().clear();
        astep.exe();

        WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
        {
            protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
            {
                if (WORLD.PATH().map.is.is(it.tile()))
                {
                    COLOR.ORANGE100.bind();
                    for (int di = 0; di < DIR.ALL.size(); di++)
                    {
                        DIR d = DIR.ALL.get(di);
                        if (WORLD.PATH().map.can(it.tx(), it.ty(), d))
                            SPRITES.cons().ICO.arrows2.get(d.id()).render(r, it.x(),
                                it.y());
                    }
                    COLOR.unbind();
                }
            }
        };

        new GenLand(astep);
        astep.exe();
        new GenPort(astep);
        astep.exe();
        new GenConnect(astep);
        astep.exe();

        foreach (COORDINATE c in WORLD.TBOUNDS())
        {
            if (WORLD.PATH().map.is.is(c))
            {
                Region reg = WORLD.PATH().regMap.get(c);
                bool w = WORLD.WATER().isBig.is(c);
                foreach (DIR d in DIR.ALL)
                {
                    if (w != WORLD.WATER().isBig.is(c))
                        continue;
                    if (!w && reg != WORLD.PATH().regMap.get(c, d))
                        continue;

                    if (WTRAV.can(c.x(), c.y(), d, true) && WORLD.PATH().map.is.is(c, d))
                    {
                        WORLD.PATH().map.add(c, d);
                        WORLD.PATH().map.add(c.x() + d.x(), c.y() + d.y(), d.perpendicular());
                    }
                }
            }
        }
    }

    static void connect(PathTile t)
    {
        PathTile parent = t;
        t = t.getParent();
        while (t != null)
        {
            WORLD.PATH().map.add(parent, DIR.get(parent, t));
            WORLD.PATH().map.add(t, DIR.get(t, parent));
            parent = t;
            t = t.getParent();
        }
    }
}