using System;
using System.Text;
using settlement.path.components.finder;
using snake2d;
using snake2d.util.color;
using snake2d.util.misc;
using util.rendering;
using view.main;
using view.sett;
using view.tool;

namespace settlement.path.components
{
    public class SCompTests
    {
        private readonly SCOMPONENTS comps;

        public SCompTests(SCOMPONENTS comps)
        {
            this.comps = comps;
            IDebugPanelSett.Add("Path Comp", new Placer());
        }

        private class Placer : ACTION
        {
            int sx, sy;
            SCompPath res = null;

            ON_TOP_RENDERABLE ren = new ON_TOP_RENDERABLE()
            {
                public void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
                {
                    if (res == null)
                        return;
                    RenderIterator it = data.onScreenTiles();
                    while (it.Has())
                    {
                        if (res.Is(it.Tile()))
                        {
                            if (it.Tx() == sx && it.Ty() == sy)
                                COLOR.GREEN100.Bind();
                            SPRITES.cons().BIG.dots.Render(r, 0, it.X(), it.Y());
                            COLOR.Unbind();
                        }
                        it.Next();
                    }
                }
            };

            PlacableSimpleTile p1 = new PlacableSimpleTile("set start")
            {
                public void place(int tx, int ty)
                {
                    sx = tx;
                    sy = ty;
                    VIEW.s().tools.place(p2);
                }

                public CharSequence isPlacable(int tx, int ty)
                {
                    if (comps.zero.Get(tx, ty) == null)
                        return E;
                    return null;
                }
            };

            PlacableSimpleTile p2 = new PlacableSimpleTile("set dest")
            {
                public void place(int tx, int ty)
                {
                    res = comps.pather.findDest(sx, sy, tx, ty);
                    ren.Add();
                }

                public CharSequence isPlacable(int tx, int ty)
                {
                    if (comps.zero.Get(tx, ty) == null)
                        return E;
                    return null;
                }
            };

            public void exe()
            {
                res = null;
                VIEW.s().tools.place(p1);
            }
        }
    }
}