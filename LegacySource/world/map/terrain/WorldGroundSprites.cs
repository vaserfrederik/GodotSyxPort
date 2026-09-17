using System;
using System.IO;

namespace World.Map.Terrain
{
    class WorldGroundSprites
    {
        public readonly TILE_SHEET[] Sheets;
        public readonly TILE_SHEET Stencil;
        public readonly TILE_SHEET[] Cracked;
        public const int DIM = 8;

        public WorldGroundSprites() 
        {
            Sheets = new TILE_SHEET[8];

            Stencil = new ITileSheet(PATHS.SPRITE_WORLD_MAP().Get("Ground"), 576, 300)
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.Init(0, 0, 4, 1, d.s16);
                    for (int i = 0; i < 4; i++)
                        s.house.SetVar(i).Paste(true);
                    s.full.Init(0, s.house.Body().Y2(), 2, 4, DIM, DIM, d.s16);
                    return d.s16.SaveGame();
                }
            }.Get();

            for (int i = 0; i < Sheets.Length; i++)
            {
                double k = i;
                Sheets[i] = new ITileSheet()
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        double dd = k / (Sheets.Length - 1);
                        int fg = 1;

                        if (dd > 0.5)
                        {
                            fg = 3;
                            dd = (dd - 0.5) * 2.0;
                            dd = 1.0 - dd;
                        }
                        else
                        {
                            dd *= 2;
                        }

                        s.full.SetVar(0);
                        s.full.Paste(false);
                        s.full.SetVar(fg);
                        s.full.PasteOverBackground(true, dd);

                        return d.s16.SaveGame();
                    }
                }.Get();
            }

            Cracked = new TILE_SHEET[]
            {
                new ITileSheet()
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.SetVar(0);
                        s.full.Paste(false);
                        s.full.SetVar(2);
                        s.full.PasteOverBackground(true, 0.5);

                        return d.s16.SaveGame();
                    }
                }.Get(),
                new ITileSheet()
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.SetVar(2);
                        s.full.Paste(true);
                        return d.s16.SaveGame();
                    }
                }.Get()
            };
        }

        public int Ran(int tx, int ty)
        {
            return (tx & (DIM - 1)) + (ty & (DIM - 1)) * DIM;
        }

        public void RenderNormal(TILE_SHEET sheet, SPRITE_RENDERER r, int x, int y, int ran)
        {
            sheet.Render(r, ran & 63, x, y);
        }

        public void RenderStenciled(TILE_SHEET sheet, SPRITE_RENDERER r, int x, int y, int mask, int ran1, int ran2)
        {
            Stencil.RenderTextured(sheet.GetTexture(ran1 & 63), mask + 16 * ((ran2) & 3), x, y);
        }
    }
}