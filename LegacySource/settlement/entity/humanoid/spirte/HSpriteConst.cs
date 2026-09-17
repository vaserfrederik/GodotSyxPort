using System;

namespace Settlement.Entity.Humanoid.Sprite
{
    internal static class HSpriteConst
    {
        private HSpriteConst()
        {
        }

        private static int i = 0;
        private const int NR = 8;
        internal const int IFEET_NONE = i++ * NR;
        internal const int IFEET_RIGHT = i++ * NR;
        internal const int IFEET_RIGHT2 = i++ * NR;
        internal const int IFEET_LEFT = i++ * NR;
        internal const int IFEET_LEFT2 = i++ * NR;
        internal const int ITUNIC = i++ * NR;
        internal const int ITORSO_STILL = i++ * NR;
        internal const int ITORSO_RIGHT = i++ * NR;
        internal const int ITORSO_RIGHT2 = i++ * NR;
        internal const int ITORSO_RIGHT3 = i++ * NR;
        internal const int ITORSO_LEFT = i++ * NR;
        internal const int ITORSO_LEFT2 = i++ * NR;
        internal const int ITORSO_LEFT3 = i++ * NR;
        internal const int ITORSO_CARRY = i++ * NR;
        internal const int ITORSO_OUT = i++ * NR;
        internal const int ITORSO_OUT2 = i++ * NR;

        internal const int IHEAD = i++ * NR;
        internal const int ISHADOW = i++ * NR;

        internal static readonly int[][] ITOOL = new int[][]
        {
            new int[] { 0 * NR, 1 * NR, 2 * NR },
            new int[] { 0 * NR, 1 * NR, 2 * NR },
        };

        internal static readonly int[][] IWEAPON = new int[][]
        {
            new int[] { 0 * NR, 1 * NR, 2 * NR },
            new int[] { 0 * NR, 1 * NR, 2 * NR },
        };

        static HSpriteConst()
        {
            i = 0;
        }

        internal const int HAMMER1 = i++;
        internal const int HAMMER2 = i++ * NR;
        internal const int HAMMER3 = i++ * NR;
        internal const int HAMMER4 = i++ * NR;
        internal const int HAMMER5 = i++ * NR;
        internal const int HAMMER6 = i++ * NR;

        internal static readonly int[] TROLLY = new int[] { 0, NR, 2 * NR, 3 * NR };
        internal const int SWORD1 = 0;
        internal const int SWORD2 = NR;
        internal const int SWORD3 = 2 * NR;

        public static void Filth(Induvidual indu, int torse, int x, int y)
        {
            double am = STATS.NEEDS().Grime(indu);
            Texture(indu.Race().Appearance().Sheet(indu).Sheet.Sheet, RACES.Sprites().Grit, am, STATS.RAN().Get(indu, 0), torse, x, y);
        }

        public static void Blood(Induvidual indu, int torse, int x, int y)
        {
            double am = STATS.NEEDS().INJURIES.COUNT.Indu().GetD(indu);
            indu.Race().Appearance().Colors.Blood.Bind();
            Texture(indu.Race().Appearance().Sheet(indu).Sheet.Sheet, RACES.Sprites().Blood, am, STATS.RAN().Get(indu, 0), torse, x, y);
            COLOR.Unbind();
        }

        private static void Texture(TILE_SHEET sheet, TILE_SHEET ex, double am, long ran, int torse, int x, int y)
        {
            if (am == 0)
                return;
            int i = (int)(am * 0x07) * 8;
            i += ran & 0x07;
            sheet.RenderTextured(ex.GetTexture(i), torse, x, y);
        }

        public static void Water(Induvidual indu, int dir, int torso, int x, int y)
        {
            indu.Race().Appearance().Sheet(indu).Sheet.Sheet.RenderTextured(indu.Race().Appearance().Extra.Water.GetTexture(CLAY.exWATER[GAME.Intervals().Get05() & 0b011] + dir), torso, x, y);
        }

        public static class CLAY
        {
            static int i = 0;
            public static readonly int PANTS = i++ * NR;
            static readonly int TORSO = i++ * NR;
            static readonly int ARMS = i++ * NR;
            static readonly int HEAD = i++ * NR;
            static readonly int SHADOW = i++ * NR;
            static
            {
                i = 0;
            }
            private static readonly int[] exWATER = new int[] { 0, NR, 2 * NR, 3 * NR };

            static readonly int off = (24 - 32) * C.SCALE / 2;
            static readonly int offC = 32 * C.SCALE / 2;

            public static void Filth(Induvidual indu, int dir, int x, int y)
            {
                double am = STATS.NEEDS().Grime(indu);
                Texture(indu.Race().Appearance().Sheet(indu).Sheet.Lay, RACES.Sprites().Lgrit, am, STATS.RAN().Get(indu, 0), dir, x, y);
            }

            public static void Filth(Race race, bool adult, double am, int dir, int ran, int x, int y)
            {
                Texture(adult ? race.Appearance().Adult().Sheet.Lay : race.Appearance().Child().Sheet.Lay, RACES.Sprites().Lgrit, am, ran, dir, x, y);
            }

            public static void Blood(Induvidual indu, int dir, int x, int y)
            {
                double am = STATS.NEEDS().INJURIES.COUNT.Indu().GetD(indu);
                indu.Race().Appearance().Colors.Blood.Bind();
                Texture(indu.Race().Appearance().Sheet(indu).Sheet.Lay, RACES.Sprites().Lblood, am, STATS.RAN().Get(indu, 0), dir, x, y);
                COLOR.Unbind();
            }

            private static void Texture(TILE_SHEET sheet, TILE_SHEET ex, double am, long ran, int dir, int x, int y)
            {
                if (am == 0)
                    return;
                int i = (int)(am * 0x07) * 8;
                i += ran & 0x07;
                sheet.RenderTextured(ex.GetTexture(i), SHADOW + dir, x, y);
            }

            public static void Water(Induvidual indu, int dir, int x, int y)
            {
                indu.Race().Appearance().Sheet(indu).Sheet.Lay.RenderTextured(indu.Race().Appearance().Extra.Lwater.GetTexture(CLAY.exWATER[GAME.Intervals().Get05() & 0b011] + dir), CLAY.SHADOW + dir, x, y);
            }
        }
    }
}