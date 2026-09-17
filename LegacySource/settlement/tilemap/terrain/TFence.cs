using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Tilemap.Terrain
{
    public sealed class TFence : MAPPED
    {
        public static RMAP<TFence> Get(Terrain t)
        {
            string KEY = "FENCE";
            string f = KEY.ToLower(CultureInfo.InvariantCulture);
            PATH data = PATHS.INIT_SETTLEMENT().GetFolder(f);
            PATH text = PATHS.TEXT_SETTLEMENT().GetFolder(f);

            string[] keys = data.GetFiles();
            var all = new LISTE<TFence>(keys.Length);

            foreach (var key in keys)
            {
                Json da = new Json(data.Get(key));
                Json te = new Json(text.Get(key));

                Sheets sSquare = new Sheets(SheetType.sCombo, da.Json("SPRITE_SQUARE_COMBO"));
                Sheets sRound = new Sheets(SheetType.sCombo, da.Json("SPRITE_ROUND_COMBO"));

                SPRITE icon = new SPRITE.Imp(Icon.L)
                {
                    Render = (r, X1, X2, Y1, Y2) =>
                    {
                        SheetPair da = sSquare.Get(0);
                        int z = CORE.Renderer().GetZoomout();
                        CORE.Renderer().SetZoom(Integer.NumberOfTrailingZeros(C.SCALE));
                        X1 *= C.SCALE;
                        Y1 *= C.SCALE;
                        int d = C.TILE_SIZE;
                        da.s.Render(da.d, X1, Y1, null, r, DIR.S.Mask | DIR.E.Mask, 0, 0);
                        da.s.Render(da.d, X1 + d, Y1, null, r, DIR.S.Mask | DIR.W.Mask, 0, 0);
                        da.s.Render(da.d, X1, Y1 + d, null, r, DIR.N.Mask | DIR.E.Mask, 0, 0);
                        da.s.Render(da.d, X1 + d, Y1 + d, null, r, DIR.N.Mask | DIR.W.Mask, 0, 0);
                        CORE.Renderer().SetZoom(z);
                    }
                };

                new TFence(key, t, all, da, te, icon, sSquare, sRound);
            }

            if (all.Size > 16)
            {
                throw new Errors.GameError("Too many fences have been declared. Maximum is 16");
            }

            return new RMAP<TFence>(KEY, all);
        }

        public sealed class TFenceTile : TerrainTile, TDestoryable, Diagonalizer
        {
            private readonly TerrainClearing clearing = new TerrainClearing()
            {
                Clear1 = (tx, ty) =>
                {
                    shared.NADA.PlaceFixed(tx, ty);
                    return RND.OneIn(3) ? resource : null;
                },
                CanClear = () => true,
                ClearAll = (tx, ty) =>
                {
                    shared.NADA.PlaceFixed(tx, ty);
                    return 0;
                },
                Sound = (tx, ty) => sound,
                IsEasilyCleared = () => false,
                IsStructure = () => true,
                Destroy = (tx, ty) =>
                {
                    shared.DESTROYED.Place(tx, ty, this, GetDia(tx, ty) ? 1 : 0);
                },
                Strength = () => 10 * C.TILE_SIZE
            };

            private readonly SoundRace sound;
            public readonly COLOR MiniColor;
            public readonly string Name;
            public readonly string Desc;

            private readonly Sheets sSquare;
            private readonly Sheets sRound;

            public readonly RESOURCE Resource;
            public readonly int ResAmount;
            private readonly int DIA = 0x0100;
            public readonly TFence Fence;

            public TFenceTile(TFence fence, Terrain t, Json data, Json text, SPRITE icon, Sheets sSquare, Sheets sRound) : base("FENCE_" + fence.Key, t, text.Text("NAME"), icon, new ColorImp(data, "MINIMAP_COLOR"))
            {
                this.sSquare = sSquare;
                this.sRound = sRound;
                MiniColor = new ColorImp(data, "MINIMAP_COLOR");
                Name = text.Text("NAME");
                Desc = text.Text("DESC");
                Resource = RESOURCES.Map().Read(data);
                ResAmount = data.I("RESOURCE_AMOUNT");
                sound = AUDIO.Race("BUILD_FENCE_" + fence.Key);
                this.Fence = fence;
            }

            public TerrainClearing Clearing => clearing;

            protected override bool Place(int tx, int ty)
            {
                bool dia = (shared.Get(tx, ty) is Diagonalizer) && ((Diagonalizer)shared.Get(tx, ty)).GetDia(tx, ty);

                int res = 0;
                Room r = SETT.ROOMS().Map.Get(tx, ty);
                if (r != null)
                {
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (Is(tx, ty, d))
                        {
                            Room r2 = SETT.ROOMS().Map.Get(tx, ty, d);
                            if (r2 == null || r2 == r)
                                res |= d.Mask();
                        }
                    }
                }
                else
                {
                    foreach (DIR d in DIR.ORTHO)
                        if (Is(tx, ty, d))
                            res |= d.Mask();
                }

                base.PlaceRaw(tx, ty);
                shared.Data.Set(tx, ty, res);
                SetDia(tx, ty, dia);

                return false;
            }

            protected override bool RenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
            {
                return false;
            }

            protected override bool RenderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
            {
                Sheets sh = sSquare;

                if ((data & DIA) != 0)
                {
                    sh = sRound;
                }

                int k = (data >> 4) & 0x0F;
                SheetPair sheet = sh.Get(i.Ran());
                if (sheet == null)
                    return false;
                sheet.D.Color(k).Bind();
                int ran = i.Ran();

                int tile = SheetType.sCombo.Tile(sSquare.Get(0), data & 0x0F, 0, 0);

                sheet.s.Render(sheet.D, i.X(), i.Y(), i, r, tile, ran, 0);
                COLOR.Unbind();
                if (s != null)
                    sheet.s.RenderShadow(sheet.D, i.X(), i.Y(), i, s, tile, ran);
                return false;
            }

            public AVAILABILITY GetAvailability(int x, int y)
            {
                return AVAILABILITY.SOLID;
            }

            public bool IsPlacable(int tx, int ty)
            {
                return true;
            }

            public int MiniDepth()
            {
                return 2;
            }

            public void SetDia(int tx, int ty, bool dia)
            {
                if (Is(tx, ty))
                {
                    int d = shared.Data.Get(tx, ty);
                    if (dia)
                        d |= DIA;
                    else
                        d &= ~DIA;
                    shared.Data.Set(tx, ty, d);
                }
            }

            public bool GetDia(int tx, int ty)
            {
                if (Is(tx, ty))
                {
                    return (shared.Data.Get(tx, ty) & DIA) != 0;
                }
                return false;
            }

            public Job FixJob()
            {
                return SETT.JOBS().Fences.Get(Fence.Index);
            }

            public int ResAmount()
            {
                return 2;
            }

            public RESOURCE BreakableRes()
            {
                return Resource;
            }

            public bool WantsFloorUnderneath(int tx, int ty)
            {
                return true;
            }
        }

        private readonly int index;
        private readonly string key;
        public readonly TFenceTile Tile;

        public TFence(string key, Terrain t, LISTE<TFence> all, Json data, Json text, SPRITE icon, Sheets sSquare, Sheets sRound)
        {
            this.key = key;
            this.index = all.Add(this);
            tile = new TFenceTile(this, t, data, text, icon, sSquare, sRound);
        }

        public int Index()
        {
            return index;
        }

        public string Key()
        {
            return key;
        }
    }
}