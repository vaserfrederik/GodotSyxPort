using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.faction;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util;
using view.tool;
using view.world.panel;
using world;
using world.map.pathing.Comps;
using world.map.pathing.WRegFinder;
using world.map.regions;
using world.map.road;

namespace world.map.pathing
{
    public class WPATHING : WorldResource
    {
        public readonly WDirMap map = new WDirMap();
        private readonly Bitmap2D portArea = new Bitmap2D(WORLD.TBOUNDS(), false);
        private Comps comps = new Comps(map);

        public readonly WRegFinder regFinder = new WRegFinder();

        private static readonly LIST<DIR> dirs = new ArrayList<DIR>(DIR.ALL);

        public WPATHING() : base("pathing", "PATHING")
        {
            IDebugPanelWorld.Add("path test", new ACTION
            {
                exe = () => new DebugTest()
            });

            IDebugPanelWorld.Add("path overlay", new ACTION
            {
                exe = () =>
                {
                    WORLD.OVERLAY().debug = new Comps.DebugOverlay();
                }
            });

            IDebugPanelWorld.Add(new DebugPlacer());
        }

        private readonly WorldResourceManager saver = new WorldResourceManager
        {
            {
                IDebugPanelWorld.Add("generate paths", new ACTION
                {
                    exe = () =>
                    {
                        ACTION load = new ACTION
                        {
                            exe = () =>
                            {
                                LOG.ln("gen...");
                            }
                        };
                        generate(load);
                        validateInit(null);
                    }
                });
            },

            save = file =>
            {
                map.saver.save(file);
                portArea.save(file);
            },

            load = file =>
            {
                map.saver.load(file);
                portArea.load(file);
                comps = Comps.generate(ACTION.NOP, map);
                validateInit(null);
                WORLD.OVERLAY().debug = null;
            },

            clear = () =>
            {
                map.saver.clear();
                portArea.clear();
                comps = new Comps(map);
            },

            validateInit = error =>
            {
                if (!WORLD.IN_BOUNDS(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy()))
                {
                    if (error != null)
                    {
                        error.problem = "The world has no player region centre";
                        error.coo.set(WORLD.TBOUNDS().cX(), WORLD.TBOUNDS().cY());
                    }
                    else
                    {
                        LOG.ln("bad stuff");
                    }

                    return;
                }

                bool[] reached = new bool[WREGIONS.MAX];

                GUTIL.flooder().init(this);
                GUTIL.flooder().pushSloppy(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy(), 0);
                while (GUTIL.flooder().hasMore())
                {
                    PathTile t = GUTIL.flooder().pollSmallest();
                    Region reg = WORLD.REGIONS().map.get(t);
                    if (reg != null && t.isSameAs(reg.cx(), reg.cy()))
                        reached[reg.index()] = true;

                    map.push(t, t.getValue());
                }
                GUTIL.flooder().done();

                for (int ri = 0; ri < WREGIONS.MAX; ri++)
                {
                    Region reg = WORLD.REGIONS().getByIndex(ri);
                    if (reg.active())
                    {
                        if (!reached[reg.index()])
                        {
                            if (error != null)
                            {
                                error.problem = "This region is not connected to other regions through roads " + reg.index();
                                error.coo.set(reg.cx(), reg.cy());
                            }
                            else
                            {
                                LOG.ln("reg " + reg);
                            }

                            return;
                        }
                    }
                }
            },

            generate = loadPrint =>
            {
                clear();
                new Gen().generateAll(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy(), loadPrint);
                comps = Comps.generate(loadPrint, map);
                WORLD.OVERLAY().debug = null;
                //new DebugTest();
            },

            makePlacers = tm => new ArrayList<PLACABLE>(0)
        };

        public override WorldResourceManager saver()
        {
            return saver;
        }


        public static int cost(int fromX, int fromY, DIR d)
        {
            if (WORLD.WATER().isBig.is(fromX, fromY))
            {
                return 1;
            }
            int toX = fromX + d.x();
            int toY = fromY + d.y();
            if (WORLD.WATER().isBig.is(toX, toY))
                return WTRAV.PORT_PENALTY;

            if (WORLD.MOUNTAIN().coversTile(fromX, fromY))
                return 6;
            if (WORLD.FOREST().amount.get(fromX, fromY) == 1.0)
                return 4;
            return 3;
        }

        public static double movementSpeed(int tx, int ty)
        {
            if (WORLD.WATER().isBig.is(tx, ty))
            {
                return 1;
            }
            if (WORLD.MOUNTAIN().heighter.get(tx, ty) >= 1)
                return 0.16;
            if (WORLD.FOREST().amount.get(tx, ty) == 1.0)
                return 0.25;
            return 0.33;
        }

        public PathTile path(int sx, int sy, int destX, int destY)
        {
            return comps.finder.find(sx, sy, destX, destY, Treaty.DUMMY);
        }

        public PathTile path(COORDINATE start, int endX, int endY, Treaty trav)
        {
            return comps.finder.find(start.x(), start.y(), endX, endY, trav);
        }

        public PathTile path(int startX, int startY, COORDINATE end, Treaty trav)
        {
            return comps.finder.find(startX, startY, end.x(), end.y(), trav);
        }

        public PathTile path(COORDINATE start, COORDINATE end, Treaty trav)
        {
            return comps.finder.find(start.x(), start.y(), end.x(), end.y(), trav);
        }

        public PathTile path(int sx, int sy, int destX, int destY, Treaty trav)
        {
            return comps.finder.find(sx, sy, destX, destY, trav);
        }

        public double distance(int sx, int sy, int ex, int ey)
        {
            return comps.finder.distance(sx, sy, ex, ey);
        }

        public MAP_OBJECT<Region> regMap = new MAP_OBJECT<Region>
        {
            get = (tx, ty) =>
            {
                if (WORLD.WATER().isBig.is(tx, ty))
                {
                    if (portArea.is(tx, ty))
                        return WORLD.REGIONS().map.get(tx, ty);
                    return null;
                }
                return WORLD.REGIONS().map.get(tx, ty);
            },
            get = tile =>
            {
                if (WORLD.WATER().isBig.is(tile))
                {
                    if (portArea.is(tile))
                        return WORLD.REGIONS().map.get(tile);
                    return null;
                }
                return WORLD.REGIONS().map.get(tile);
            }
        };
    }
}