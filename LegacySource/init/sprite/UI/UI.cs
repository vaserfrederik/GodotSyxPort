using System;
using System.IO;

namespace INIT.Sprite.UI
{
    public class UI : InitResource
    {
        private static UIDecor decor;
        private static UIPanels panels;
        private static UIFonts fonts;
        private static Icons icons;
        private static UIImageMaker image;

        public UI(INIT init) : base(init)
        {
            GCOLOR.Read();
            fonts = new UIFonts();
            panels = new UIPanels();
            decor = new UIDecor();
            icons = new Icons();
            image = new UIImageMaker();
        }

        public static UIFonts FONT()
        {
            return fonts;
        }

        public static UIPanels PANEL()
        {
            return panels;
        }

        public static UIDecor Decor()
        {
            return decor;
        }

        public static Icons Icons()
        {
            return icons;
        }

        public static UIImageMaker Image()
        {
            return image;
        }
    }
}