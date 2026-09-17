using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Entity.Animal
{
    using Init.Paths;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite;
    using Util.SpriteComposer;

    class Sprites
    {
        private readonly TileSheet _textureBlood;
        private readonly List<TileSheet> _textureWater;
        private readonly TileSheet _crate;

        public Sprites()
        {
            _textureBlood = new TileSheet(PATHS.SPRITE().GetFolder("animal").Get("_Texture"), 328, 196)
            {
                Init = (c, ss, d) =>
                {
                    var s = ss.Singles;

                    Tile t = d.S32;
                    s.Init(0, 0, 3, 1, 2, 10, t);
                    s.SetVar(0);
                    for (int i = 0; i < 5; i++)
                    {
                        s.SetSkip(i * 2, 2).Paste(3, true);
                    }
                    return t.SaveGame();
                }
            }.Get();

            _textureWater = new TileSheetList
            {
                Init = (c, s, d) =>
                {
                    s.Singles.SetVar(1);
                    return 4;
                },
                Next = (i, c, s, d) =>
                {
                    s.Singles.SetSkip(i * 2, 2).Paste(3, true);
                    return d.S32.SaveGame();
                }
            }.Get();

            _crate = new TileSheet
            {
                Init = (c, ss, d) =>
                {
                    var s = ss.Singles;
                    s.SetSkip(8, 2).Paste(3, true);
                    return d.S32.SaveGame();
                }
            }.Get();
        }
    }
}