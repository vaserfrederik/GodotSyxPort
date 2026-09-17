using System;

namespace Util.Colors
{
    public class GCOLOR
    {
        private static GCOLOR_TEXT text;
        private static GCOLOR_UI ui;
        private static COLOR_MAP map;

        public static GCOLOR_TEXT T()
        {
            return text;
        }

        public static GCOLOR_UI UI()
        {
            return ui;
        }

        public static COLOR_MAP MAP()
        {
            return map;
        }

        public static void Read()
        {
            text = new GCOLOR_TEXT();
            ui = new GCOLOR_UI();
            map = new COLOR_MAP();
        }
    }
}