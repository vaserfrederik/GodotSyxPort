using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World
{
    using static WORLD;
    using game.faction;
    using init.sprite;
    using snake2d;
    using snake2d.util.datatypes;
    using snake2d.util.world;
    using util.colors;

    public class WorldMinimap
    {
        public const int WIDTH = 1024;
        public const int HEIGHT = 1024;

        private readonly int _chunkSize = 16;
        private readonly Minimap _map;
        private readonly Bitmap1D _changes;
        private int _changedI;
        private readonly ColorImp _cWork;
        private readonly ColorImp _cNone;
        private readonly ColorImp _cBorderDark;
        private readonly ColorImp _cBorderLight;
        private readonly ColorImp _cOcean;
        private readonly ColorImp _cOceanDeep;
        private readonly ColorImp _cOceanBorder;
        private readonly ColorImp _cMountainTop;
        private readonly ColorImp _cMountainBorder;

        public WorldMinimap() : this(WIDTH)
        {
        }

        public WorldMinimap(int width)
        {
            _map = new Minimap(width);
            _changes = new Bitmap1D(WREGIONS.MAX, false);
            _changedI = WREGIONS.MAX;
            _cWork = new ColorImp();
            _cNone = new ColorImp(100, 100, 100);
            _cBorderDark = new ColorImp(35, 35, 35);
            _cBorderLight = new ColorImp(127, 127, 127);
            _cOcean = new ColorImp(30, 35, 60);
            _cOceanDeep = _cOcean.Shade(0.75);
            _cOceanBorder = _cOcean.Shade(0.5);
            _cMountainTop = new ColorImp(81, 75, 70);
            _cMountainBorder = _cMountainTop.Shade(0.3);
        }

        public void UpdateRegion(Region region)
        {
            _changedI = Math.Max(region.Index, _changedI);
            _changes.Set(region.Index, true);
        }

        public void Clear()
        {
            _changedI = WREGIONS.MAX;
            _changes.Clear();
        }

        public void Redraw(int x1, int y1, int w, int h)
        {
            int px1 = Math.Max(0, Math.Min(_map.Width, (int)(_map.Width * x1 / WORLD.TWIDTH())));
            int py1 = Math.Max(0, Math.Min(_map.Height, (int)(_map.Height * y1 / WORLD.THEIGHT())));
            int px2 = Math.Max(0, Math.Min(_map.Width, (int)(_map.Width * (x1 + w) / WORLD.TWIDTH())));
            int py2 = Math.Max(0, Math.Min(_map.Height, (int)(_map.Height * (y1 + h) / WORLD.THEIGHT())));

            for (int py = py1; py < py2; py++)
            {
                for (int px = px1; px < px2; px++)
                {
                    _cWork.Set(GetColorP(px, py));
                    _cWork.ShadeSelf(0.5);
                    _map.PutPixel(px, py, _cWork);
                }
            }
        }

        private COLOR GetColorP(int pixelX, int pixelY)
        {
            double dx = WORLD.TWIDTH() / (double)_map.Width;
            double dy = WORLD.THEIGHT() / (double)_map.Height;

            double wx = WORLD.TWIDTH() * pixelX / (double)_map.Width;
            double wy = WORLD.THEIGHT() * pixelY / (double)_map.Height;

            {
                double rx = dx * 6;
                double ry = dy * 6;

                for (int y = (int)(wy - ry); y <= wy + ry; y++)
                {
                    for (int x = (int)(wx - rx); x <= wx + rx; x++)
                    {
                        if (!WORLD.IN_BOUNDS(x, y))
                            continue;
                        Region r2 = WORLD.REGIONS().Map.Get(x, y);
                        if (r2 != null && r2.Capitol && r2.Info.Cx == x && r2.Info.Cy == y)
                        {
                            int px = (int)(x / dx);
                            int py = (int)(y / dy);
                            int ddx = pixelX - px;
                            int ddy = pixelY - py;
                            double rad = Math.Abs(ddx) + Math.Abs(ddy);
                            if (rad < 3)
                                return COLOR.BLACK;
                            if (rad == 3)
                                return COLOR.WHITE100;
                        }
                    }
                }
            }

            Region r = REGIONS().Map.Get((int)wx, (int)wy);

            if (r != null)
            {
                COLOR c = r.Faction == null ? _cNone : r.Faction.Banner().ColorBG();

                if (WORLD.IN_BOUNDS((int)(wx + dx), (int)(wy)) && !REGIONS().Map.Is((int)(wx + dx), (int)(wy), r))
                    return ColorImp.TMP.Interpolate(c, _cBorderDark, 0.75);
                else if (WORLD.IN_BOUNDS((int)(wx), (int)(wy + dy)) && !REGIONS().Map.Is((int)(wx), (int)(wy + dy), r))
                    return ColorImp.TMP.Interpolate(c, _cBorderDark, 0.75);
                else if (WORLD.IN_BOUNDS((int)(wx - dx), (int)(wy)) && IsDiffRealm((int)(wx - dx), (int)(wy), r))
                    return ColorImp.TMP.Interpolate(c, _cBorderLight, 0.75);
                else if (WORLD.IN_BOUNDS((int)(wx), (int)(wy - dy)) && IsDiffRealm((int)(wx), (int)(wy - dy), r))
                    return ColorImp.TMP.Interpolate(c, _cBorderLight, 0.75);

                return c;
            }

            int tx = (int)wx;
            int ty = (int)wy;
            if (WATER().Has.Is(tx, ty) && WATER().CoversTile.Is(tx, ty))
            {
                foreach (DIR d in DIR.ORTHO)
                {
                    int ddx = (int)(tx + d.X * dx);
                    int ddy = (int)(ty + d.Y * dy);
                    if (!WATER().CoversTile.Is(ddx, ddy) || WORLD.REGIONS().Map.Is(ddx, ddy))
                        return _cOceanBorder;
                }
                if (WATER().OCEAN.Deep.Is(tx, ty) || WATER().LAKE.Deep.Is(tx, ty))
                    return _cOceanDeep;
                return _cOcean;
            }
            else if (MOUNTAIN().Is(tx, ty))
            {
                foreach (DIR d in DIR.ORTHO)
                {
                    int ddx = (int)(tx + d.X * dx);
                    int ddy = (int)(ty + d.Y * dy);

                    if (!MOUNTAIN().Is(ddx, ddy) || WORLD.REGIONS().Map.Is(ddx, ddy))
                        return _cMountainBorder;
                }
                return ColorImp.TMP.Interpolate(_cMountainBorder, _cMountainTop, MOUNTAIN().GetHeight(tx, ty) / 15.0);
            }
            return _cNone;
        }

        private bool IsDiffRealm(int x, int y, Region r)
        {
            Region r2 = REGIONS().Map.Get(x, y);
            if (r == r2)
                return false;
            if (r2 == null)
                return true;
            if (r.Realm == null || r2.Realm == null || r.Realm != r2.Realm)
                return true;
            return false;
        }

        public void Repaint()
        {
            SPRITES.Loader().Print(¤¤painting);
            Clear();
            int pWidth = _map.Width;
            int pHeight = _map.Height;

            byte[] pixels = new byte[pWidth * pHeight * 4];

            int i = 0;

            for (int py = 0; py < pHeight; py++)
            {
                for (int px = 0; px < pWidth; px++)
                {
                    SetPixel(pixels, i, GetColorP(px, py));
                    i += 4;
                }
            }

            _map.PutPixels(pixels);
        }

        private static void SetPixel(byte[] pixels, int i, COLOR c)
        {
            pixels[i + 0] = (byte)(c.Red() & 0x0FF);
            pixels[i + 1] = (byte)(c.Green() & 0x0FF);
            pixels[i + 2] = (byte)(c.Blue() & 0x0FF);
            pixels[i + 3] = 255;
        }
    }
}