using System;
using System.Collections.Generic;
using System.IO;
using game.GAME;
using game.debug;
using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using view.tool;
using world.army;
using world.battle;
using world.entity;
using world.entity.haven;
using world.log;
using world.map.buildings;
using world.map.fow;
using world.map.landmark;
using world.map.pathing;
using world.map.regions;
using world.map.regions.centre;
using world.map.road;
using world.map.terrain;
using world.map.terrain;
using world.map.terrain;
using world.map.terrain;
using world.map.terrain;
using world.overlay;

namespace world
{
    public class WORLD : GameResource
    {
        private static Data w;

        public static int TWIDTH()
        {
            return w.tWidth;
        }

        public static int THEIGHT()
        {
            return w.tHeight;
        }

        public static RECTANGLE TBOUNDS()
        {
            return w.tDim;
        }

        public static RECTANGLE PIXELS()
        {
            return w.dim;
        }

        public static int PWIDTH()
        {
            return w.width;
        }

        public static int PHEIGHT()
        {
            return w.height;
        }

        public static int TAREA()
        {
            return w.tHeight * w.tWidth;
        }

        public static bool IN_BOUNDS(int tx, int ty)
        {
            return w.tDim.holdsPoint(tx, ty);
        }

        public static bool IN_BOUNDS(COORDINATE c, DIR d)
        {
            return IN_BOUNDS(c.x() + d.x(), c.y() + d.y());
        }

        public static bool IN_BOUNDS(int tx, int ty, DIR d)
        {
            return IN_BOUNDS(tx + d.x(), ty + d.y());
        }

        public static WorldTerrain TERRAIN()
        {
            return w.terrain;
        }

        public static WorldMountain MOUNTAIN()
        {
            return w.terrain.mountain;
        }

        public static WorldWater WATER()
        {
            return w.terrain.water;
        }

        public static WorldGround GROUND()
        {
            return w.terrain.ground;
        }

        public static WorldForest FOREST()
        {
            return w.terrain.forest;
        }

        public static WEntities ENTITIES()
        {
            return w.ENTITIES;
        }

        public static WorldClimate CLIMATE()
        {
            return w.terrain.climate;
        }

        public static MAP_DOUBLE MOISTURE()
        {
            return w.terrain.ground.moisture;
        }

        public static WREGIONS REGIONS()
        {
            return w.areas;
        }

        public static AD ARMIES()
        {
            return w.armies;
        }

        public static WorldBuildings BUILDINGS()
        {
            return w.buildings;
        }

        public static WorldLandmarks LANDMARKS()
        {
            return w.landmarks;
        }

        public static WorldMinimap MINIMAP()
        {
            return w.minimap;
        }

        public static WorldOverlays OVERLAY()
        {
            return w.overlay;
        }

        public static Sprites sprites()
        {
            return w.sprites;
        }

        public static WHavens camps()
        {
            return w.ENTITIES.havens;
        }

        public static WorldGen GEN()
        {
            return w.stage;
        }

        public static WorldRoads ROADS()
        {
            return w.roads;
        }

        public static WPATHING PATH()
        {
            return w.pathing;
        }

        public static WCentre CENTRE()
        {
            return w.centre;
        }

        public static FOW FOW()
        {
            return w.fow;
        }

        public static WorldLog LOG()
        {
            return w.log;
        }

        public static WBattles BATTLES()
        {
            return w.battles;
        }

        public static RD RD()
        {
            return w.rd;
        }

        public static LIST<WorldResource> RESOURCES()
        {
            return w.resources;
        }

        private sealed class Data
        {
            private readonly ArrayList<WorldResource> resources = new ArrayList<WorldResource>(100);
            private readonly RECTANGLE dim;
            private readonly RECTANGLE tDim;
            private readonly int tHeight;
            private readonly int tWidth;
            private readonly int height;
            private readonly int width;

            private readonly Sprites sprites;
            private readonly WorldTerrain terrain;
            private readonly WorldLandmarks landmarks;
            private readonly WEntities ENTITIES;
            private readonly WREGIONS areas;
            private readonly AD armies;
            private readonly WorldBuildings buildings;
            private readonly WorldMinimap minimap;
            private readonly WorldOverlays overlay;
            private readonly WorldRoads roads;
            private readonly WPATHING pathing;
            private readonly WorldGen stage;
            private readonly WCentre centre;
            private readonly Render render;
            private readonly FOW fow;
            private readonly WorldLog log;
            private readonly WBattles battles;
            private readonly RD rd;
            readonly SuperSaver<WorldResource> saver;

            private Data(int tileSizeX, int tileSizeY) : this()
            {
                w = this;
                tWidth = tileSizeX;
                tHeight = tileSizeY;

                if (tWidth > 512 || tHeight > 512)
                    throw new Errors.DataError("too big a map!");

                width = tWidth * C.TILE_SIZE;
                height = tHeight * C.TILE_SIZE;
                tDim = new Rec(0, tWidth, 0, tHeight);
                dim = new Rec(0, width, 0, height);

                sprites = new Sprites();
                render = new Render(tileSizeX, tileSizeY);

                terrain = new WorldTerrain(WORLD.this);
                landmarks = new WorldLandmarks(WORLD.this);
                areas = new WREGIONS();
                centre = new WCentre();
                rd = new RD(null);
                armies = new AD(WORLD.this);
                buildings = new WorldBuildings();
                roads = new WorldRoads(WORLD.this);
                pathing = new WPATHING();
                ENTITIES = new WEntities(WORLD.this);
                fow = new FOW();
                minimap = new WorldMinimap(tileSizeX, tileSizeY);
                log = new WorldLog();
                overlay = new WorldOverlays();
                stage = new WorldGen(WORLD.this);
                battles = new WBattles();
                saver = new SuperSaver<WORLD.WorldResource>(GetType(), resources)
                {
                    protected override string key(WorldResource t)
                    {
                        return t.key;
                    }

                    protected override void save(WorldResource t, FilePutter f)
                    {
                        t.saver().save(f);
                    }

                    protected override void load(WorldResource t, FileGetter f)
                    {
                        t.saver().load(f);
                    }

                    protected override void clear(WorldResource t)
                    {
                        t.saver().clear();
                    }
                };
            }
        }

        public WORLD(int tileSizeX, int tileSizeY) : base("WORLD", false)
        {
            new Data(tileSizeX, tileSizeY);
        }

        protected override void save(FilePutter saveFile)
        {
            w.saver.save(saveFile);
            w.stage.save(saveFile);
        }

        protected override void load(FileGetter file)
        {
            w.saver.load(file);
            w.stage.load(file);

            if (SETT.exists())
            {
                SETT.WORLD_AREA().info.initCity(SETT.WORLD_AREA().tiles().x1(), SETT.WORLD_AREA().tiles().y1());
            }
        }

        protected override void update(double ds, Profiler prof)
        {
            prof.logStart(typeof(WORLD));
            for (int i = 0; i < w.resources.size(); i++)
            {
                w.resources.get(i).update(ds, prof);
            }
            prof.logStart(w.minimap.GetType());
            w.minimap.update();
            prof.logEnd(w.minimap.GetType());
            prof.logEnd(typeof(WORLD));
        }

        public static void initBeforePlay()
        {
            for (int i = 0; i < w.resources.size(); i++)
            {
                w.resources.get(i).initBeforePlay();
            }
        }

        public void render(Renderer r, float ds, int zoomout, RECTANGLE renWindow, int offX, int offY)
        {
            w.render.render(r, ds, zoomout, renWindow, offX, offY);
        }

        public abstract class WorldResource
        {
            public readonly CharSequence name;
            readonly string key;

            protected WorldResource(CharSequence name, string key)
            {
                this.name = name;
                w.resources.add(this);
                this.key = key;
            }

            public abstract WorldResourceManager saver();

            protected void update(double ds, Profiler prof)
            {
            }

            protected void afterTick()
            {
            }

            protected void afterRender()
            {
            }

            protected void initBeforePlay()
            {
            }
        }

        public abstract class WorldResourceManager : SAVABLE
        {
            public void generate(ACTION loadPrint)
            {
            }

            public void validateInit(WorldError error)
            {
            }

            public LIST<PLACABLE> makePlacers(ToolManager tm)
            {
                return new ArrayListGrower<PLACABLE>();
            }

            public void addDebugView()
            {
            }
        }

        public static void changeTile(int tx, int ty)
        {
            WORLD.MINIMAP().update(tx, ty);
        }

        public static class WorldError
        {
            public readonly Coo coo = new Coo();
            public CharSequence problem = null;
            public CharSequence warning = null;
        }
    }
}