using System;

namespace Init.Constant
{
    public class C
    {
        private C() { }

        public const int STEAM_ID = 1162750;

        public const int MAX_SPRITES = 65536;
        public const string NAME = "Songs of Syx";
        public const int SCALE = 4;
        public const int SCALE_NORMAL = 2;
        /**
         * SCALE GUI
         */
        public const int SG = 1;
        public const int GM = 24 * SG;
        public const int T_PIXELS = 16;
        public const int T_SCROLL = 6;
        public const int TILE_SIZE = T_PIXELS * SCALE;
        public const double ITILE_SIZE = 1.0 / TILE_SIZE;
        public const int TILE_SIZEH = TILE_SIZE / 2;
        public const int T_MASK = TILE_SIZE - 1;
        private static int WIDTH = 1280;
        private static int HEIGHT = 768;

        public const int MIN_WIDTH = 1280;
        public const int MIN_HEIGHT = 768;

        public const int MAX_SCREEN_AREA = 2500 * 1080;

        private static readonly Rec DIM = new Rec(0, WIDTH, 0, HEIGHT);
        // public const int TWIDTH = 768;
        // public static readonly Rectangle SETTLE_TDIM = new Rectangle(0, TWIDTH, 0, TWIDTH);
        public const double SQR2 = Math.Sqrt(2.0);
        public const double SQR2I = 1.0 / Math.Sqrt(2.0);
        public const string WEB_PAGE = "https://songsofsyx.com";
        public const string BUG_MAIL = "info@songsofsyx.com";

        public static int WIDTH()
        {
            return WIDTH;
        }

        public static int HEIGHT()
        {
            return HEIGHT;
        }

        public static Rec DIM()
        {
            return DIM;
        }

        public static void Init(int width, int height)
        {
            WIDTH = width;
            HEIGHT = height;
            DIM.Set(0, WIDTH, 0, HEIGHT);
        }
    }
}