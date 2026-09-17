using System.IO;

namespace World.Map.Buildings
{
    using Init.Paths;
    using Snake2D.Util.Sprite;
    using Util.SpriteComposer;
    using Util.SpriteComposer.ComposerThings;

    public class WorldBuildingSprites
    {
        private readonly PATH getter = PATHS.SPRITE().GetFolder("world").GetFolder("map").GetFolder("buildings");

        // public final TILE_SHEET houses = new ITileSheet(getter.getFolder("houses").get("Normal"), 460, 62) {
        //     @Override
        //     protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
        //         s.singles.init(0, 0, 1, 1, 16, 4, d.s8);
        //         s.singles.paste(true);
        //         return d.s8.saveGame();
        //     }
        // }.get();

        // public final TILE_SHEET village = new ITileSheet(getter.getFolder("houses").get("Village"), 460, 34) {
        //     @Override
        //     protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
        //         s.singles.init(0, 0, 1, 1, 16, 2, d.s8);
        //         s.singles.paste(true);
        //         return d.s8.saveGame();
        //     }
        // }.get();

        // public final TILE_SHEET farms = new ITileSheet(getter.get("Farms"), 364, 50) {
        //     @Override
        //     protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
        //         s.singles.init(0, 0, 1, 1, 8, 2, d.s16);
        //         s.singles.paste(true);
        //         return d.s16.saveGame();
        //     }
        // }.get();

        public readonly TILE_SHEET garrison = new ITileSheet(getter.Get("Garrison"), 236, 20)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Singles.Init(0, 0, 1, 1, 8, 1, d.S8);
                s.Singles.Paste(3, true);
                return d.S8.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET terrainStencil = new ITileSheet(getter.Get("TerrainStencil"), 120, 60)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Full.Init(0, 0, 1, 1, 3, 3, d.S16);
                s.Full.Paste(true);
                return d.S16.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET mines = new ITileSheet(getter.Get("Mines"), 364, 100)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Singles.Init(0, 0, 1, 1, 8, 1, d.S16);
                s.Singles.Paste(true);
                return d.S16.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET roads = new ITileSheet(getter.Get("Roads"), 576, 172)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.House.Init(0, 0, 4, 2, d.S16);
                for (int i = 0; i < 4; i++)
                    s.House.SetVar(i).Paste(1, true);
                return d.S16.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET roadsMini = new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.House.Init(0, 0, 4, 2, d.S16);
                for (int i = 0; i < 4; i++)
                    s.House.SetVar(4 + i).Paste(1, true);
                return d.S16.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET bridge = new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Singles.Init(0, s.House.Body().Y2(), 1, 1, 3, 1, d.S16);
                for (int i = 0; i < 3; i++)
                    s.Singles.SetSkip(i, 1).Paste(3, true);
                return d.S16.SaveGame();
            }
        }.Get();

        // public final TILE_SHEET wallCity = new ITileSheet(getter.get("Wall"), 600, 120) {
        //     @Override
        //     protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
        //         ComposerDests.Tile t = d.s16;
        //         final ComposerSources.Full f = s.full;
        //         f.init(0, 0, 5, 2, 3, 3, t);
        //         for (int i = 0; i < 10; i++)
        //             f.setVar(i).paste(true);
        //         return t.saveGame();
        //     }
        // }.get();

        // public final TILE_SHEET wallTown = new ITileSheet(getter.get("WallTown"), 616, 44) {
        //     @Override
        //     protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
        //         ComposerDests.Tile t = d.s16;
        //         final ComposerSources.Full f = s.full;
        //         f.init(0, 0, 7, 1, 2, 2, t);
        //         for (int i = 0; i < 7; i++)
        //             f.setVar(i).paste(true);
        //         return t.saveGame();
        //     }
        // }.get();

        public readonly TILE_SHEET siege = new ITileSheet(getter.Get("Siege"), 120, 60)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.S16;
                final ComposerSources.Full f = s.Full;
                f.Init(0, 0, 1, 1, 3, 3, t);
                f.Paste(true);
                return t.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET harbour = new ITileSheet(getter.Get("Harbour"), 252, 220)
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Singles.Init(0, 0, 1, 1, 4, 4, d.S24);
                s.Singles.Paste(3, true);
                return d.S24.SaveGame();
            }
        }.Get();

        public readonly TILE_SHEET harbourRiver = new ITileSheet()
        {
            protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.Singles.Init(0, s.Singles.Body().Y2(), 1, 1, 4, 4, d.S16);
                s.Singles.Paste(3, true);
                return d.S16.SaveGame();
            }
        }.Get();

        public WorldBuildingSprites()
        {
            // Constructor logic if needed
        }
    }
}