using System;
using System.IO;
using System.Numerics;
using Snake2D.Util.Sprite;
using Util.SpriteComposer;
using Init.Paths;

namespace Init.Sprite
{
    public sealed class Textures
    {
        public readonly TileTexture DisBig;
        public readonly TileTexture DisSmall;
        public readonly TileTexture DisTiny;
        public readonly TileTexture Fire;
        public readonly TileTexture Water;
        public readonly TileTexture Bumps;
        public readonly TileTexture Dots;
        public readonly TileTexture DisLow;

        public Textures()
        {
            PATH path = PATHS.Sprite().GetFolder("textures");

            Dots = Get(path.Get("Dots"));
            DisLow = Get(path.Get("Displacement_low"));
            DisBig = Get(path.Get("Displacement_Big"));
            DisSmall = Get(path.Get("Displacement_small"));
            DisTiny = Get(path.Get("Displacement_tiny"));
            Bumps = Get(path.Get("Bumps"));
            Water = Get(path.Get("Water"));
            Fire = Get(path.Get("Fire"));
        }

        private static TileTexture Get(Path path)
        {
            return new ITileTexture(8, 8, path, 280, 140)
            {
                protected override SpriteData Init(ComposerUtil c, ComposerSources s, ComposerDests d, ComposerTexturer t)
                {
                    return t.Paste(0, 0, 8, 8);
                }
            }.Get();
        }
    }
}