using System;
using System.IO;
using Snake2D.Util.Sprite;
using Util.SpriteComposer;

namespace Settlement.Room.Main.Util
{
    public class RoomUtil
    {
        public readonly FilthTexture filth;

        public RoomUtil() : base()
        {
            filth = new FilthTexture();
        }
    }

    public class FilthTexture
    {
        private readonly TILE_SHEET sheet;
        private const int vars = 16;
        private const int amounts = 8;

        public FilthTexture() : base()
        {
            sheet = new ITileSheet(PATHS.SPRITE_SETTLEMENT().GetFolder("map").Get("Filth"), 536, 140)
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.Init(0, 0, 1, 1, vars, amounts, d.s16);
                    s.full.Paste(true);
                    return d.s16.SaveGame();
                }
            }.Get();
        }

        public TextureCoords Texture(double amount, int ran)
        {
            int i = (int)(amount * 7) * vars;
            i += ran & 0x0F;
            return sheet.GetTexture(i);
        }
    }
}