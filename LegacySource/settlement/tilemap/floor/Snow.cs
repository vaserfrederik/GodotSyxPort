using System;
using System.IO;
using init.constant;
using init.paths;
using init.settings;
using settlement.main;
using settlement.tilemap;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.rendering;
using util.spritecomposer;

namespace settlement.tilemap.floor
{
    public sealed class Snow : TileMap.Resource
    {
        private Bitsmap1D amount = new Bitsmap1D(0, 2, SETT.TAREA);
        private readonly TILE_SHEET sheet;

        public Snow(TileMap tileMap) : base(tileMap)
        {
            new ComposerThings.IInit(PATHS.SPRITE_SETTLEMENT_MAP().Get("Snow"), 972, 126);

            sheet = (new ITileSheet
            {
                Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.s24;
                    final ComposerSources.Singles f = s.singles;
                    f.Init(0, 0, 1, 1, 16, 4, t);
                    f.SetVar(0).Paste(true);
                    return t.SaveGame();
                }
            }).Get();

            HeightMap height = new HeightMap(SETT.TWIDTH, SETT.THEIGHT, 8, 8);
            for (int i = 0; i < SETT.TAREA; i++)
            {
                amount.Set(i, (int)Math.Round(height.Get(i) * 3));
            }
        }

        protected override void Save(FilePutter saveFile)
        {
            amount.Save(saveFile);
        }

        protected override void Load(FileGetter saveFile)
        {
            amount.Load(saveFile);
        }

        protected override void ClearAll()
        {
        }

        public void Render(Renderer r, RenderData data)
        {
            if (S.Get().Downpour.Get() == 0)
                return;

            RenderData.RenderIterator i = data.OnScreenTiles(1, 1, 1, 1);

            double snow = SETT.WEATHER().Snow.Get() * 7.0;
            double ri = 1.0 / 0xFFFF;
            if (S.Get().Graphics.Get() == 0)
                snow *= 0.25;

            if (snow == 0)
                return;

            while (i.Has())
            {
                long ran = i.Ran();

                double rr = (ran & 0xFFFF) * ri;
                double s = snow;

                s -= SETT.ENV().Map.Light.Get(i.Tile()) * 7.0;

                s -= amount.Get(i.Tile());
                s -= rr;
                if (s < 1)
                    s -= rr;

                if (s >= 0 && !TERRAIN().Get(i.Tile()).RoofIs() && !TERRAIN().Get(i.Tile()).IsMassiveWall() && !SETT.ROOMS().Placement.Embryo.Is(i.Tile()))
                {
                    int c = (int)s;
                    ran = ran >> 8;
                    if (SETT.ROOMS().Map.Is(i.Tile()) || SETT.FLOOR().Getter.Get(i.Tile()) != null || SETT.MINERALS().AmountInt.Get(i.Tile()) > 0)
                    {
                        c = CLAMP.I(c, -1, 1);
                        c -= ran & 1;
                    }
                    else
                    {
                        c = CLAMP.I(c, -1, 3);
                    }

                    c -= Math.Ceiling(SETT.PATH().Huristics.Getter.Get(i.Tile()) * 16 * 4);
                    if (c >= 0)
                    {
                        int d = (int)(((ran & 0x7) - 7) * C.SCALE);
                        ran = ran >> 3;
                        int x = i.X() + d;
                        d = (int)(((ran & 0x7) - 7) * C.SCALE);
                        ran = ran >> 3;
                        int y = i.Y() + d;
                        sheet.Render(r, (int)(ran & 0xF) + c * 16, x, y);
                    }
                }
                i.Next();
            }
            COLOR.Unbind();
        }
    }
}