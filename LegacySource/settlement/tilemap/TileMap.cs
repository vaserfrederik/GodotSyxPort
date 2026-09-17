using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.TileMap
{
    public class TileMap : SETT.SettResource
    {
        static TileMap()
        {
            Resource.resources.Clear();
        }

        public readonly Ground ground;
        public readonly Floors floors;
        public readonly Grass grass;
        public readonly Terrain topology;
        public readonly Snow snow;
        public readonly TGrowth growth;
        public readonly SettMarks marks;
        private readonly TerrainHotspots hotspots = new TerrainHotspots();
        private readonly SRenderer renderer = new SRenderer(this);

        public TileMap() : base("TMAP", true)
        {
            ground = new Ground(this);
            grass = new Grass();
            floors = new Floors(this);
            topology = new Terrain(this);
            growth = new TGrowth(topology);
            snow = new Snow(this);
            marks = new SettMarks();
            new GeneratorTests();
        }

        protected override void Clear()
        {
            foreach (var r in Resource.resources)
                r.ClearAll();
        }

        protected override void Generate(CapitolArea area)
        {
            minimap.Clear();
            SETT.MINIMAP().SetOpen(false);
            new Generator(area);
            SETT.MINIMAP().SetOpen(true);
            minimap.Clear();
        }

        protected override void Load(FileGetter saveFile)
        {
            foreach (var r in Resource.resources)
            {
                saveFile.Check(r);
                SPRITES.loader().Print(r.ToString());
                r.Load(saveFile);
            }
            AvailabilityListener.ListenAll(false);
            for (int y = 0; y < THEIGHT; y++)
            {
                for (int x = 0; x < TWIDTH; x++)
                {
                    PATH().availability.UpdateAvailability(x, y);
                }
            }
            AvailabilityListener.ListenAll(true);
        }

        protected override void Save(FilePutter saveFile)
        {
            foreach (var r in Resource.resources)
            {
                saveFile.Mark(r);
                SPRITES.loader().Print(r.ToString());
                r.Save(saveFile);
            }
        }

        protected override void Update(double ds, Profiler profiler)
        {
            foreach (var r in Resource.resources)
            {
                profiler.LogStart(r);
                r.Update(ds, profiler);
                profiler.LogEnd(r);
            }
        }

        protected override void Init(bool loaded)
        {
            hotspots.Init();
            minimap.Clear();
            Generator.PaintMinimap();
        }

        public void RenderAboveEnts(Renderer r, ShadowBatch s, float ds, int zoomout, RenderData renData)
        {
            renderer.RenderAboveEnts(r, s, ds, zoomout, renData);
        }

        public void RenderTheRest(Renderer r, ShadowBatch s, float ds, int zoomout, RenderData renData, RECTANGLE renWindow, int offX, int offY)
        {
            renderer.RenderTheRest(r, s, ds, zoomout, renData, renWindow, offX, offY);
        }

        public void RenderSemiMap(Renderer r, float ds, RenderData renData)
        {
            renderer.RenderSemiMap(r, ds, renData);
        }

        public void RenderMiniMap(Renderer r, float ds, RenderData renData, int zoomout)
        {
            renderer.RenderMiniMap(r, ds, renData, zoomout);
        }

        protected override void AfterTick()
        {
            foreach (var r in Resource.resources)
                r.AfterTick();
            minimap.Update();
        }

        public TerrainHotspots Hotspots()
        {
            return hotspots;
        }

        public void UpdateTileDay(int tx, int ty, int tile)
        {
            growth.UpdateTileDay(tx, ty, tile);
        }

        public readonly MinimapColorGetter minimap = new MinimapColorGetter();

        public COLOR MiniC(int tx, int ty)
        {
            return minimap.Get(tx, ty);
        }

        public void MiniCUpdate(int tx, int ty)
        {
            minimap.Update(tx, ty);
        }

        public abstract class Resource
        {
            private static readonly List<Resource> resources = new List<Resource>(10);
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly int index;

            protected Resource()
            {
                index = resources.Add(this);
            }

            protected abstract void Save(FilePutter saveFile);
            protected abstract void Load(FileGetter saveFile);

            protected virtual void Update(double ds, Profiler profiler)
            {
            }

            protected virtual void AfterTick()
            {
            }

            protected abstract void ClearAll();
        }

        public LOS LOS(int tx, int ty)
        {
            return topology.Get(tx, ty).los(tx, ty);
        }

        public interface SMinimapGetter
        {
            COLOR MiniC(int x, int y);

            COLOR MiniColorPimped(ColorImp c, int x, int y, bool northern, bool southern);
        }
    }
}