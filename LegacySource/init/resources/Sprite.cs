using System;
using System.IO;
using snake2d.util.color;
using snake2d.util.sprite;
using util.spritecomposer;

namespace Init.Resources
{
    internal class Sprite
    {
        public readonly TILE_SHEET carry;
        public readonly TILE_SHEET lay;
        public readonly COLOR color;

        public Sprite(Path path) : this(path, "244", "94")
        {
        }

        public Sprite(Path path, string width, string height) : this(path, int.Parse(width), int.Parse(height))
        {
        }

        public Sprite(Path path, int width, int height)
        {
            carry = new ITileSheet(path, width, height)
            {
                Init = (c, s, d) =>
                {
                    s.singles.Init(0, 0, 1, 1, 1, 4, d.s16);
                    s.singles.SetSkip(0, 2).Paste(3, true);
                    return d.s16.SaveGame();
                }
            }.Get();

            color = new IColorSamplerSingle()
            {
                Init = (c, s, d) =>
                {
                    s.singles.SetSkip(2, 1);
                    return s.singles.Sample();
                }
            }.Get();

            lay = new ITileSheet()
            {
                Init = (c, s, d) =>
                {
                    s.singles.Init(
                        s.singles.Body().X2(), 0,
                        1, 1,
                        4, 4,
                        d.s16
                    );
                    s.singles.Paste(true);
                    return d.s16.SaveGame();
                }
            }.Get();
        }

        internal class Util
        {
            public TILE_SHEET GetMinable(Path path)
            {
                return new ITileSheet(path, 364, 94)
                {
                    Init = (c, s, d) =>
                    {
                        s.singles.Init(0, 0, 1, 1, 8, 4, d.s16);
                        s.singles.Paste(true);
                        return d.s16.SaveGame();
                    }
                }.Get();
            }

            public TILE_SHEET GetGrowable(Path path)
            {
                return new ITileSheet(path, 364, 182)
                {
                    Init = (c, s, d) =>
                    {
                        s.singles.Init(0, 0, 1, 1, 8, 8, d.s16);
                        s.singles.Paste(true);
                        return d.s16.SaveGame();
                    }
                }.Get();
            }
        }
    }
}