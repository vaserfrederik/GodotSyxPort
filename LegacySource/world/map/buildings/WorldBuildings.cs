using System;
using System.Collections.Generic;
using System.IO;

namespace World.Map.Buildings
{
    public class WorldBuildings : WorldResource
    {
        private readonly Bitmap2D village = new Bitmap2D(WORLD.TBOUNDS(), false);
        public readonly WorldBuildingSprites sprites = new WorldBuildingSprites();
        private readonly OPACITY[] ops = new OPACITY[16];
        private bool debugVisible = false;

        public WorldBuildings() : base("buildings", "BUILDINGS")
        {
            for (int i = 0; i < ops.Length; i++)
            {
                ops[i] = new OpacityImp((int)(255 * (0.4 + 0.4 * RND.rFloat())));
            }
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            Save = file =>
            {
                village.Save(file);
            },
            Load = file =>
            {
                village.Load(file);
            },
            Clear = () =>
            {
                village.Clear();
            },
            MakePlacers = tm =>
            {
                var placers = new ArrayListGrower<PLACABLE>();
                var p = new Placer();
                placers.Add(p);
                placers.Add(p.GetUndo());
                return placers;
            },
            Generate = loadPrint =>
            {
                loadPrint.Execute();
                new WorldGeneratorBuildings();
            },
            AddDebugView = () =>
            {
                debugVisible = true;
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        public void RenderAboveGround(WRenContext con, RenderIterator it)
        {
            if (village.Is(it.Tile()))
            {
                WorldRaceSheet.Village sh = IsVisible(it.Ran(), it.Tile());

                if (sh != null)
                {
                    int ran = it.Ran() >> 4;
                    ops[(ran >> 5) & 15].Bind();
                    WorldRaceSheet.Farm farm = RD.RACES().All.GetC(it.Ran()).Race.Appearance().World.Farm;
                    farm.Render(con, ran, it.X(), it.Y());
                    OPACITY.Unbind();
                    COLOR.Unbind();
                }
            }
        }

        public void RenderAbove(WRenContext con, RenderIterator it)
        {
            if (village.Is(it.Tile()))
            {
                if ((it.Ran() & 2) == 0)
                {
                    WorldRaceSheet.Village sh = IsVisible(it.Ran(), it.Tile());
                    if (sh != null)
                    {
                        int ran = ((it.Ran() >> 16) & 31);
                        int x = it.X() + ((it.Ran() >> 20) & 7) * C.SCALE;
                        int y = it.Y() + ((it.Ran() >> 24) & 7) * C.SCALE;
                        int li = (it.Ran() >> 28) & 0x07;
                        sh.Render(con.R, con.S, ran, x, y);

                        if (TIME.Light().NightIs() && (TIME.Light().PartOfCircular() * 16 > li))
                        {
                            x += C.TILE_SIZEH / 2 + (GAME.Intervals().Get05() + it.Ran() & 0b11);
                            y += C.TILE_SIZEH / 2 + (GAME.Intervals().Get05() + (it.Ran() >> 4) & 0b11);
                            CORE.Renderer().RenderUniLight(x, y, 2, 128);
                        }
                    }
                }
            }
        }

        public void RenderAboveTerrain(WRenContext con, RenderIterator it)
        {
            Region reg = WORLD.REGIONS().Map.Get(it.Tile());
            if (reg != null && RD.BUILDINGS().LevelMine.Get(reg) * 0x07 > (it.Ran() & 0x0FF) && !WORLD.WATER().Is(it.Tile()))
            {
                int i = it.Ran() & 0b1;
                i *= 4;

                int m = (GAME.Intervals().Get02() + (it.Ran() >> 1)) & 0b0111;
                if (m >= 4)
                {
                    m -= 4;
                    m = 3 - m;
                }
                i += m;
                sprites.Mines.Render(con.R, i, it.X(), it.Y());
            }
        }

        private WorldRaceSheet.Village IsVisible(int ran, int tile)
        {
            if (WORLD.FOREST().Amount.Get(tile) == 1)
                return null;
            if (WORLD.WATER().IsBig.Is(tile))
                return null;
            Region r = WORLD.REGIONS().Map.Get(tile);
            if (debugVisible)
                return RD.RACES().All.GetC(ran).Race.Appearance().World.Village;
            if (r != null)
            {
                double v = RD.RACES().PopSizeD(r) * (1.0 - RD.DEVASTATION().Current.GetD(r));
                int k = (int)(0x0FFFF * v);
                if ((ran & 0x0FFFF) <= k)
                {
                    return RD.RACES().Visuals.VRace(r, ran).Appearance().World.Village;
                }
            }

            return null;
        }

        protected override void AfterRender()
        {
            debugVisible = false;
        }

        protected override void Update(double ds, Profiler prof)
        {
            //prof.LogStart(this);
            //camp.Update(ds);
            //prof.LogEnd(this);
        }
    }
}