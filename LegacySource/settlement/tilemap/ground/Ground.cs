using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Tilemap.Ground
{
    public class Ground : TileMap.Resource
    {
        public static string ¤¤soilType = "Soil Type";
        public static string ¤¤moisture = "Moisture";
        public static string ¤¤moistureB = "Moisture (base)";

        private static string ¤¤moistureMap = "Moisture Map";
        private static string ¤¤moistureMApD = "The natural moisture content of the city map.";

        static Ground()
        {
            D.ts(typeof(Ground));
        }

        public static readonly int MOISTURE_MAX = 15;
        public static readonly double MOISTURE_MAXI = 1.0 / MOISTURE_MAX;
        public readonly GroundTypes types;

        public readonly ColorImp dry = new ColorImp();
        public readonly ColorImp wet = new ColorImp();

        private readonly Bitsmap1D mapTypes;
        private readonly Bitsmap1D mapMoistureBase;
        private readonly Bitsmap1D mapMoistureCurrent;
        private readonly Bitmap1D edge;
        public readonly Minables minerals;

        public readonly DOUBLE.DoubleImp baseMoisture;

        public Ground(TileMap m) : base(m)
        {
            types = new GroundTypes();
            new Debug(this);
            baseMoisture.info = new INFO(¤¤moistureMap, ¤¤moistureMApD);
        }

        protected override void Save(StreamWriter saveFile)
        {
            mapTypes.Save(saveFile);
            mapMoistureBase.Save(saveFile);
            mapMoistureCurrent.Save(saveFile);
            dry.Save(saveFile);
            wet.Save(saveFile);
        }

        protected override void Load(StreamReader saveFile)
        {
            mapTypes.Load(saveFile);
            mapMoistureBase.Load(saveFile);
            mapMoistureCurrent.Load(saveFile);
            dry.Load(saveFile);
            wet.Load(saveFile);
        }

        public void SetEdge(int tx, int ty)
        {
            GroundType g = MAP.Get(tx, ty);
            int m = mapMoistureCurrent.Get(tx + ty * SETT.TWIDTH);
            bool set = false;
            for (int di = 0; di < DIR.ORTHO.Length; di++)
            {
                int x = tx + DIR.ORTHO[di].x();
                int y = ty + DIR.ORTHO[di].y();
                if (Joins(g, m, x, y))
                {
                    set = true;
                    break;
                }
            }
            edge.Set(tx + ty * SETT.TWIDTH, set);
        }

        private bool Joins(GroundType g, int m, int x, int y)
        {
            if (SETT.IN_BOUNDS(x, y))
            {
                int i2 = MAP.Get(x, y).index;

                if (g.index < i2)
                    return true;
                return g.index == i2 && m < mapMoistureCurrent.Get(x + y * SETT.TWIDTH);
            }
            return false;
        }

        private void Update(int tx, int ty)
        {
            SetEdge(tx, ty);
            SETT.TILE_MAP().MiniCUpdate(tx, ty);
        }

        public void Render(Renderer r, RenderIterator it)
        {
            int tile = it.Tile();
            int ran = it.Ran();
            int x = it.X();
            int y = it.Y();

            GroundType g = MAP.Get(tile);
            int m = mapMoistureCurrent.Get(tile);
            g.tmps[m].Bind();
            g.sheet.Render(r, ran & (GroundTypes.VARS - 1), x, y);
            if (edge.Get(it.Tile()))
            {

                for (int di = 0; di < 4; di++)
                {
                    DIR d = DIR.ORTHO[di];

                    int dx = it.Tx() + d.x();
                    int dy = it.Ty() + d.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        GroundType g2 = MAP.Get(dx, dy);
                        int m2 = mapMoistureCurrent.Get(dx + dy * SETT.TWIDTH);
                        if (g.index < g2.index || (g.index == g2.index && m < m2))
                        {
                            g2.tmps[m2].Bind();
                            ran = ran >> 4;

                            DIR d2 = d.Next(2);
                            int t = it.Tile() + d2.x() + d2.y() * SETT.TWIDTH;
                            if (SETT.IN_BOUNDS(it.Tx(), it.Ty(), d2) && g2.index == mapTypes.Get(t) && m2 == mapMoistureCurrent.Get(t))
                            {
                                types.c_masks.RenderTextured(g2.sheet.GetTexture(ran & (GroundTypes.VARS - 1)), 8 * d.OrthoID() + (ran & 7), x, y);
                            }
                            else
                            {
                                types.s_masks.RenderTextured(g2.sheet.GetTexture(ran & (GroundTypes.VARS - 1)), 8 * d.OrthoID() + (ran & 7), x, y);
                            }

                        }
                    }
                }


            }

            COLOR.Unbind();
        }

        public void Render(Renderer r, float ds, ShadowBatch s, RenderData data)
        {
            RenderData.RenderIterator i = data.OnScreenTiles();

            while (i.Has())
            {
                Render(r, i);
                minerals.Render(r, i.Tile(), i.Ran(), i.X(), i.Y());
                i.Next();
            }
        }

        public void RenderMinerals(Renderer r, int tile, int ran, int x, int y)
        {
            minerals.Render(r, tile, ran, x, y);
        }

        public TextureCoords GetTexture(int tile, int ran)
        {
            return MAP.Get(tile).sheet.GetTexture(ran & 15);
        }


        public void Hover(GUI_BOX box, int tx, int ty)
        {
            GBox b = (GBox)box;
            b.TextLL(Str.TMP.Clear().Add(¤¤soilType).Add(':'));
            b.TextL(b.Text().Add(MAP.Get(tx, ty).name));
            b.NL();
            b.Text(b.Text().Add(MAP.Get(tx, ty).desc));
            b.NL(6);
            b.TextLL(¤¤moisture);
            b.Tab(6);
            b.Add(GFORMAT.Perc(b.Text(), MOISTURE_TOT.Get(tx, ty)));
            b.NL();
            b.TextLL(¤¤moistureB);
            b.Tab(6);
            b.Add(GFORMAT.Perc(b.Text(), MOISTURE_BASE.Get(tx, ty)));
            b.NL();

            if (S.Get().developer)
            {
                b.TextLL(Dic.¤¤High);
                b.Tab(6);
                b.Add(GFORMAT.Perc(b.Text(), MOISTURE_CURRENT.Get(tx, ty)));
                b.Sep();
            }


            if (minerals.Getter.Is(tx, ty))
            {
                b.Add(MINERALS().Getter.Get(tx, ty).Resource.Icon());
                b.TextLL(MINERALS().Getter.Get(tx, ty).name);
                b.Tab(6);
                b.Add(GFORMAT.PercInc(b.Text(), -0.1 + 0.2 * MINERALS().Value.Get(tx, ty)));
                b.Sep();
            }
        }

        public readonly SMinimapGetter minimap = new SMinimapGetter()
        {
            MiniColorPimped = (ColorImp original, int x, int y, bool northern, bool southern) =>
            {
                if (minerals.Getter.Is(x, y))
                {
                    return minerals.MiniC(original, MAP.Get(x, y).miniC, x, y);
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (TERRAIN().WATER.is.Is(x + d.x(), y + d.y()) || TERRAIN().MOUNTAIN.is.Is(x + d.x(), y + d.y()))
                    {
                        original.ShadeSelf(0.75);
                        return original;
                    }
                }
                if (northern || southern)
                    original.ShadeSelf(0.9);
                return original;
            },
            MiniC = (int x, int y) => MAP.Get(x, y).miniC
        };
    }
}