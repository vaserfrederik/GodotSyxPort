using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;
using view.tool;
using world;

namespace world.map.landmark
{
    public sealed class WorldLandmarks : WorldResource
    {
        private static readonly string ¤¤name = "Landmark";

        static WorldLandmarks()
        {
            D.ts(typeof(WorldLandmarks));
        }

        private const int nothing = 0;
        private const int MAX = 255;
        private readonly ArrayList<WorldLandmark> areas = new ArrayList<WorldLandmark>(MAX + 1);
        private readonly Bitsmap1D mapID = new Bitsmap1D(0, 8, WORLD.TAREA());

        public WorldLandmarks(WORLD world) : base(¤¤name, "LANDMARKS")
        {
            areas.Add(null);
            for (int i = 1; i <= MAX; i++)
                areas.Add(new WorldLandmark(i));
        }

        public WorldLandmark GetByIndex(int index)
        {
            return areas[index];
        }

        public LIST<WorldLandmark> All()
        {
            return areas;
        }

        public MAP_OBJECTE<WorldLandmark> Setter => new MAP_OBJECTE<WorldLandmark>
        {
            Get = tile =>
            {
                int i = mapID.Get(tile);
                return areas[i];
            },
            Get = (tx, ty) =>
            {
                if (WORLD.IN_BOUNDS(tx, ty))
                    return Get(tx + ty * WORLD.TWIDTH());
                return null;
            },
            Set = (tile, @object) =>
            {
                if (@object == null)
                    mapID.Set(tile, nothing);
                else
                    mapID.Set(tile, @object.Index());
            },
            Set = (tx, ty, @object) =>
            {
                if (WORLD.IN_BOUNDS(tx, ty))
                    Set(tx + ty * WORLD.TWIDTH(), @object);
            }
        };

        private readonly WorldResourceManager saver = new WorldResourceManager
        {
            overlay = new PlacerOverlay(),
            Save = file =>
            {
                foreach (WorldLandmark a in areas)
                    if (a != null)
                        a.Save(file);
                mapID.Save(file);
            },
            Load = file =>
            {
                foreach (WorldLandmark a in areas)
                    if (a != null)
                        a.Load(file);
                mapID.Load(file);
            },
            Clear = () =>
            {
                mapID.SetAll(0);
                foreach (WorldLandmark a in areas)
                    if (a != null)
                        a.Clear();
            },
            Generate = loadPrint =>
            {
                new GeneratorLandmark(loadPrint);
            },
            ValidateInit = error =>
            {
                new GeneratorLandmarkValidator(error);
            },
            MakePlacers = tm =>
            {
                return new Placers(this, overlay);
            },
            AddDebugView = () =>
            {
                overlay.Add();
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }
    }
}