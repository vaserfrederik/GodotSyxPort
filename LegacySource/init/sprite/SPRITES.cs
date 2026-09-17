using System;
using System.IO;

namespace Init.Sprite
{
    using Game;
    using Init.Sprite.UI;
    using Init.Sprite.UI.UIConses;
    using Init.Sprite.UI.UISpecials;
    using Init.Sprite.GameSheets;
    using Init.Sprite.Imps;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.Sprite;

    public class Sprites
    {
        private static Sprites self;
        private readonly UIConses panelsOverlays;
        private readonly Textures textures;

        private readonly SPRITE loadScreen;
        private readonly UISpecials specials;
        private readonly GameSheets game;
        private readonly RLoadPrinter loader;

        public Sprites(GameGame gameg) : base()
        {
            self = this;
            CORE.CheckIn();

            CORE.CheckIn();

            panelsOverlays = new UIConses();
            loadScreen = UI.Image().Get("_LoadScreen", null, null);
            CORE.CheckIn();
            specials = new UISpecials();
            CORE.CheckIn();
            textures = new Textures();
            game = new GameSheets();
            loader = new RLoadPrinter();
            new CustomSprites();
        }

        public static class ColorRemove
        {
            public static void Bad2Good(ColorImp c, double d)
            {
                if (d < 0)
                    d = 0;
                if (d > 1)
                    d = 1;
                double r = (d > 0.5) ? (1.0 - (d - 0.5) * 2) : 1;
                double g = (d < 0.5) ? d * 2 : 1;
                c.Set(30 + (int)(70 * r), 30 + (int)(70 * g), 30);
            }
        }

        public static Icons Icons()
        {
            return UI.Icons();
        }

        public static UIConses Cons()
        {
            return self.panelsOverlays;
        }

        public static SPRITE LoadScreen()
        {
            return self.loadScreen;
        }

        public static UISpecials Specials()
        {
            return self.specials;
        }

        public static Textures Textures()
        {
            return self.textures;
        }

        public static GameSheets Game()
        {
            return self.game;
        }

        public static RLoadPrinter Loader()
        {
            return self.loader;
        }
    }
}