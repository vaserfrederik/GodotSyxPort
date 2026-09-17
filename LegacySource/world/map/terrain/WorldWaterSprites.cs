using System;
using System.IO;

namespace World.Map.Terrain
{
    public class WorldWaterSprites
    {
        public readonly TILE_SHEET bg = (new ITileSheet(PATHS.SPRITE_WORLD_MAP().Get("Water"), 576, 272)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.house.Init(0, 0, 4, 1, t);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET bgSingles = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.full.Init(0, s.house.Body().y2(), 1, 1, 16, 1, d.s16);
                s.full.Paste(true);
                return d.s16.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET sheet = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.house.Init(0, s.full.Body().y2(), 4, 1, d.s16);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return d.s16.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET sheetCorners = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.house.SetVar(0).SetSkip(0, 1).PasteEdges(true);
                return d.s16.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET sheetSingles = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.full.Init(0, s.house.Body().y2(), 1, 1, 16, 1, d.s16);
                s.full.Paste(true);
                return d.s16.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET deep = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.house.Init(0, s.full.Body().y2(), 4, 1, d.s16);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return d.s16.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET riverBG = (new ITileSheet(PATHS.SPRITE_WORLD().GetFolder("map").Get("RiverBig"), 576, 172)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.house.Init(0, 0, 4, 1, t);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET riverFG = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.house.Init(0, s.house.Body().y2(), 4, 1, t);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET deltaShore = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.singles.Init(0, s.house.Body().y2(), 1, 1, 4, 1, t);
                for (int i = 0; i < 4; i++)
                {
                    s.singles.SetSkip(i, 1).Paste(3, true);
                }
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET delta = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.singles.Init(s.singles.Body().x2(), s.singles.Body().y1(), 1, 1, 4, 1, t);
                for (int i = 0; i < 4; i++)
                {
                    s.singles.SetSkip(i, 1).Paste(3, true);
                }
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET riverSmallBG = (new ITileSheet(PATHS.SPRITE_WORLD().GetFolder("map").Get("RiverSmall"), 576, 144)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.house.Init(0, 0, 4, 1, t);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return t.SaveGame();
            }
        }).Get();

        public readonly TILE_SHEET riverSmallFG = (new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                s.house.Init(0, s.house.Body().y2(), 4, 1, t);

                for (int i = 0; i < 4; i++)
                    s.house.SetVar(i).Paste(1, true);
                return t.SaveGame();
            }
        }).Get();

        public WorldWaterSprites() { }

        public void RenderBackground(SPRITE_RENDERER r, RenderIterator it, int rot, int corner)
        {
            if (rot == 0)
            {
                bgSingles.Render(r, it.Ran() & 0x0F, it.X(), it.Y());
            }
            else if (rot != 0x0F && corner != 0x0F)
            {
                bg.Render(r, 16 * (it.Ran() & 0b0111) + rot, it.X(), it.Y());
            }
        }

        public void Render(SPRITE_RENDERER r, RenderIterator it, int rot, int corner)
        {
            if (rot == 0)
            {
                sheetSingles.Render(r, it.Ran() & 0x0F, it.X(), it.Y());
            }
            else
            {
                sheet.Render(r, 16 * (it.Ran() & 0b0111) + rot, it.X(), it.Y());
                sheetCorners.Render(r, corner, it.X(), it.Y());
            }
        }

        public void RenderTexture(RenderData.RenderIterator i)
        {
            o2.Bind();

            COLOR c = CORE.Renderer().ColorGet();
            COLOR.Unbind();

            CORE.Renderer().RenderDisplace(
                dis1.x1(i.tx()), dis1.y1(i.ty()), tex1.x1(i.tx()), tex1.y1(i.ty()), 16, 16, 8, i.x(), i.x() + C.TILE_SIZE, i.y(), i.y() + C.TILE_SIZE);

            c.Bind();
            o1.Bind();
            CORE.Renderer().RenderDisplace(
                dis2.x1(i.tx()), dis2.y1(i.ty()), tex2.x1(i.tx()), tex2.y1(i.ty()), 16, 16, 4, i.x(), i.x() + C.TILE_SIZE, i.y(), i.y() + C.TILE_SIZE);

            OPACITY.Unbind();
        }

        private readonly TileDisplaceEffect dis1 = new TileDisplaceEffect();
        private readonly TileDisplaceEffect dis2 = new TileDisplaceEffect();
        private readonly TileTextureEffect tex1 = new TileTextureEffect();
        private readonly TileTextureEffect tex2 = new TileTextureEffect();
        private readonly OpacityImp o1 = new OpacityImp((int)(255 * 0.1));
        private readonly OpacityImp o2 = new OpacityImp((int)(255 * 0.2));

        public void Update(double ds)
        {
            dis1.Update(ds);
            dis2.Update(ds);
            tex1.Update(ds);
            tex2.Update(ds);
        }
    }
}