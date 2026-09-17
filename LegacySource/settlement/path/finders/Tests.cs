using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Path.Finders
{
    using static Settlement.Main.SETT.PATH;
    using static Settlement.Main.SETT.TAREA;
    using static Settlement.Main.SETT.TWIDTH;

    using Init.Constant;
    using Init.Resources;
    using Init.Sprite;
    using Settlement.Main;
    using Settlement.Path.Path;
    using Snake2D;
    using Snake2D.PathTile;
    using Snake2D.Renderer;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Gui.Clickable;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util;
    using Util.Gui.Misc;
    using Util.Rendering;
    using Util.Text;
    using View.Main;
    using View.Sett;
    using View.Subview;
    using View.Tool;

    class Tests
    {
        private readonly SFINDERS _finders;

        public Tests(SFINDERS finders)
        {
            _finders = finders;
            IDebugPanelSett.Add("Path Compare astar", new PlacerCompare());

            new ON_TOP_RENDERABLE
            {
                Map = null,

                Render = (r, shadowBatch, data, ds) =>
                {
                    if (!TestPath.Tester.IsSuccessFul)
                        return;

                    if (Map == null)
                        return;

                    if (TestPath.Tester.HasNext)
                    {
                        Map = new Bitmap1D(TAREA, false);

                        Map.Set(TestPath.Tester.X + TestPath.Tester.Y * TWIDTH, true);
                        while (TestPath.Tester.SetNext())
                        {
                            Map.Set(TestPath.Tester.X + TestPath.Tester.Y * TWIDTH, true);
                        }
                    }

                    var it = data.OnScreenTiles();
                    while (it.Has())
                    {
                        if (Map.Get(it.Tile()))
                        {
                            SPRITES.Cons().BIG.Dots.Render(r, 0, it.X(), it.Y());
                        }
                        it.Next();
                    }
                }
            }.Add();

            IDebugPanelSett.Add("Path Per Test", new ACTION
            {
                P = new SPath(),

                Exe = () =>
                {
                    const int am = 1000;
                    var sx = new int[am];
                    var sy = new int[am];

                    for (int i = 0; i < am; i++)
                    {
                        sx[i] = RND.RInt(SETT.TWIDTH);
                        sy[i] = RND.RInt(SETT.THEIGHT);
                    }

                    {
                        var dx = new int[am];
                        var dy = new int[am];
                        for (int i = 0; i < am; i++)
                        {
                            dx[i] = RND.RInt(SETT.TWIDTH);
                            dy[i] = RND.RInt(SETT.THEIGHT);
                        }
                        long now = System.currentTimeMillis();
                        int a = 0;
                        now = System.currentTimeMillis();
                        a = 0;
                        for (int i = 0; i < am; i++)
                        {
                            if (Find(sx[i], sy[i], dx[i], dy[i], false))
                                a++;
                        }
                        double d = a;
                        d /= (System.currentTimeMillis() - now);
                        d *= 1000;
                        LOG.Ln("long paths: " + d + "p/s, paths: " + a);
                    }

                    {
                        var dx = new int[am];
                        var dy = new int[am];
                        for (int i = 0; i < am; i++)
                        {
                            dx[i] = CLAMP.I(sx[i] + RND.RInt0(100), 0, TWIDTH);
                            dy[i] = CLAMP.I(sy[i] + RND.RInt0(100), 0, TWIDTH);
                        }
                        long now = System.currentTimeMillis();
                        int a = 0;
                        now = System.currentTimeMillis();
                        a = 0;
                        for (int i = 0; i < am; i++)
                        {
                            if (Find(sx[i], sy[i], dx[i], dy[i], false))
                                a++;
                        }
                        double d = a;
                        d /= (System.currentTimeMillis() - now);
                        d *= 1000;
                        LOG.Ln("short paths: " + d + "p/s, paths: " + a);
                    }

                    {
                        long now = System.currentTimeMillis();
                        int a = 0;

                        now = System.currentTimeMillis();
                        a = 0;
                        for (int i = 0; i < am; i++)
                        {
                            if (FindR(sx[i], sy[i], false))
                                a++;
                        }
                        double d = a;
                        d /= (System.currentTimeMillis() - now);
                        d *= 1000;
                        LOG.Ln("res closest: " + d + "p/s, paths: " + a);
                    }

                    {
                        long now = System.currentTimeMillis();
                        int a = 0;

                        now = System.currentTimeMillis();
                        a = 0;
                        for (int i = 0; i < am; i++)
                        {
                            if (FindJ(sx[i], sy[i]))
                                a++;
                        }
                        double d = a;
                        d /= (System.currentTimeMillis() - now);
                        d *= 1000;
                        LOG.Ln("job: " + d + "p/s, paths: " + a);
                    }
                }
            });

            IDebugPanelSett.Add("Job Per Test", new ACTION
            {
                P = new SPath(),

                Exe = () =>
                {
                    const int am = 1000;
                    var sx = new int[am];
                    var sy = new int[am];

                    for (int i = 0; i < am; i++)
                    {
                        sx[i] = RND.RInt(SETT.TWIDTH);
                        sy[i] = RND.RInt(SETT.THEIGHT);
                    }

                    long now = System.currentTimeMillis();
                    int a = 0;
                    now = System.currentTimeMillis();
                    a = 0;
                    for (int i = 0; i < am; i++)
                    {
                        if (FindJ(sx[i], sy[i]))
                            a++;
                    }
                    double d = a;
                    d /= (System.currentTimeMillis() - now);
                    d *= 1000;
                    LOG.Ln("job: " + d + "p/s, paths: " + a);
                }
            });

            IDebugPanelSett.Add("Job Compare", new ACTION
            {
                P = new SPath(),
                P2 = new PathGame.PathFancy(256),
                Map = null,
                Map2 = null,
                Sx = 0,
                Sy = 0,

                Exe = () =>
                {
                    P.Clear();
                    VIEW.S().Tools.Place(P1);
                }
            });

            IDebugPanelSett.Add("Path Compare", new ACTION
            {
                P = new SPath(),
                P2 = new PathGame.PathFancy(256),
                Map = null,
                Map2 = null,
                Sx = 0,
                Sy = 0,

                Exe = () =>
                {
                    P.Clear();
                    VIEW.S().Tools.Place(P1);
                }
            });
        }

        private bool Find(int sx, int sy, int dx, int dy, bool full)
        {
            if (P.Request(sx, sy, dx, dy, full))
            {
                Map = new Bitmap1D(TAREA, false);
                Map.Set(P.X + P.Y * TWIDTH, true);
                while (P.IsSuccessFul && P.SetNext())
                {
                    Map.Set(P.X + P.Y * TWIDTH, true);
                }
                Ren.Add();
                return true;
            }
            return false;
        }

        private bool FindR(int sx, int sy, bool full)
        {
            if (P.Request(sx, sy, full))
            {
                Map = new Bitmap1D(TAREA, false);
                Map.Set(P.X + P.Y * TWIDTH, true);
                while (P.IsSuccessFul && P.SetNext())
                {
                    Map.Set(P.X + P.Y * TWIDTH, true);
                }
                Ren.Add();
                return true;
            }
            return false;
        }

        private bool FindJ(int sx, int sy)
        {
            if (GUTIL.AStar().GetShortest(P2, SETT.Path.Coster.Player, sx, sy, sx, sy))
            {
                Map = new Bitmap1D(TAREA, false);
                Map.Set(P2.X + P2.Y * TWIDTH, true);
                do
                {
                    while (P2.SetNext())
                    {
                        Map.Set(P2.X + P2.Y * TWIDTH, true);
                    }
                } while (!P2.IsComplete && GUTIL.AStar().GetShortest(P2, SETT.Path.Coster.Player, P2.X, P2.Y, sx, sy));
                Ren.Add();
                return true;
            }
            return false;
        }

        private class PlacerCompare : ACTION
        {
            public Bitmap1D Map;
            public Bitmap1D Map2;
            public int Sx;
            public int Sy;
            public SPath P;
            public PathGame.PathFancy P2;

            public ON_TOP_RENDERABLE Ren;

            public PlacerCompare()
            {
                Ren = new ON_TOP_RENDERABLE
                {
                    Render = (r, shadowBatch, data, ds) =>
                    {
                        if (Map == null)
                        {
                            Ren.Remove();
                            return;
                        }
                        var it = data.OnScreenTiles();
                        while (it.Has())
                        {
                            if (Map.Get(it.Tile()))
                            {
                                SPRITES.Cons().BIG.Dots.Render(r, 0, it.X(), it.Y());
                            }
                            it.Next();
                        }

                        if (Map2 == null)
                            return;
                        it = data.OnScreenTiles();
                        while (it.Has())
                        {
                            COLOR.RED100.Bind();
                            if (Map2.Get(it.Tile()))
                            {
                                SPRITES.Cons().BIG.Dots.Render(r, 0, it.X() + 4, it.Y() + 4);
                            }
                            it.Next();
                        }
                        COLOR.Unbind();
                    }
                };

                P1 = new PlacableSimpleTile("set start")
                {
                    Place = (tx, ty) =>
                    {
                        Sx = tx;
                        Sy = ty;
                        VIEW.S().Tools.Place(P2);
                    },

                    IsPlacable = (tx, ty) =>
                    {
                        if (PATH().Solidity.Is(tx, ty))
                            return E;
                        return null;
                    }
                };

                P2 = new PlacableSimpleTile("set dest")
                {
                    Full = false,

                    PP = new ArrayList<CLICKABLE>(
                        new GButt.Panel("F")
                        {
                            ClickA = () => Full = !Full,
                            RenAction = () => SelectedSet(Full)
                        }
                    ),

                    Place = (tx, ty) =>
                    {
                        if (P.Request(Sx, Sy, tx, ty, Full))
                        {
                            Map = new Bitmap1D(TAREA, false);
                            Map.Set(P.X + P.Y * TWIDTH, true);
                            while (P.IsSuccessFul && P.SetNext())
                            {
                                Map.Set(P.X + P.Y * TWIDTH, true);
                            }
                            Ren.Add();
                        }

                        if (GUTIL.AStar().GetShortest(P2, SETT.Path.Coster.Player, Sx, Sy, tx, ty))
                        {
                            Map2 = new Bitmap1D(TAREA, false);
                            Map2.Set(P2.X + P2.Y * TWIDTH, true);
                            do
                            {
                                while (P2.SetNext())
                                {
                                    Map2.Set(P2.X + P2.Y * TWIDTH, true);
                                }
                            } while (!P2.IsComplete && GUTIL.AStar().GetShortest(P2, SETT.Path.Coster.Player, P2.X, P2.Y, tx, ty));
                            Ren.Add();
                        }
                    },

                    IsPlacable = (tx, ty) => null,

                    GetAdditionalButt = () => PP
                };
            }
        }
    }
}