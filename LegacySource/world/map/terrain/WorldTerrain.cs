using System;
using System.Collections.Generic;
using System.IO;
using game.debug;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using view.tool;
using world;
using world.WORLD;

namespace world.map.terrain
{
    public class WorldTerrain : WorldResource
    {
        private static readonly ArrayListGrower<WorldTerrainResource> resources = new ArrayListGrower<WorldTerrainResource>();

        public readonly WorldClimate climate;
        public readonly WorldForest forest;
        public readonly WorldGround ground;
        public readonly WorldMountain mountain;
        public readonly WorldWater water;

        public WorldTerrain(WORLD world) : base("terrain", "TERRAIN")
        {
            resources.Clear();
            climate = new WorldClimate();
            ground = new WorldGround();
            mountain = new WorldMountain();
            water = new WorldWater();
            forest = new WorldForest(world);
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            Save = (FilePutter file) =>
            {
                foreach (WorldTerrainResource r in resources)
                    r.Save(file);
            },

            Load = (FileGetter file) =>
            {
                foreach (WorldTerrainResource r in resources)
                    r.Load(file);
            },

            Clear = () =>
            {
                foreach (WorldTerrainResource r in resources)
                    r.Clear();
                WORLD.MINIMAP().Repaint();
            },

            MakePlacers = (ToolManager tm) =>
            {
                ArrayListGrower<PLACABLE> res = new ArrayListGrower<PLACABLE>();
                foreach (WorldTerrainResource r in resources)
                    res.Add(r.Placers(tm));
                return res;
            },

            ValidateInit = (WorldError error) =>
            {
                new GeneratorValidator(error);
            },

            Generate = (ACTION loadPrint) =>
            {
                new Generator(WORLD.GEN(), loadPrint);
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        public void SecretFixWays()
        {
            new GeneratorValidator(null);
        }

        protected override void Update(double ds, Profiler prof)
        {
            foreach (WorldTerrainResource r in resources)
                r.Update(ds, prof);
        }

        public abstract class WorldTerrainResource
        {
            protected WorldTerrainResource()
            {
                resources.Add(this);
            }

            protected abstract void Save(FilePutter file);

            protected abstract void Load(FileGetter file);

            protected virtual void Clear()
            {
            }

            protected virtual void Update(double ds, Profiler prof)
            {
            }

            public abstract LIST<PLACABLE> Placers(ToolManager tm);
        }
    }
}