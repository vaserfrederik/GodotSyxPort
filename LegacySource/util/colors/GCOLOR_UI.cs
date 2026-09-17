using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Util.Colors
{
    public sealed class GCOLOR_UI
    {
        private JObject d = JObject.Parse(File.ReadAllText(PATHS.SPRITE_UI().GetLikeHells("Colors.txt"))).GetValue("UI").ToObject<JObject>();

        public readonly GColorUIModel NORMAL = new GColorUIModel(new ColorImp(d, "NORMAL"));
        public readonly GColorUIModel BAD = new GColorUIModel(new ColorImp(d, "BAD"));
        public readonly GColorUIModel GOOD = new GColorUIModel(new ColorImp(d, "GOOD"));
        public readonly GColorUIModel NEUTRAL = new GColorUIModel(new ColorImp(d, "NEUTRAL"));
        public readonly GColorUIModel GOOD2 = new GColorUIModel(new ColorImp(d, "GOOD2"));
        public readonly GColorUIModel GREAT = new GColorUIModel(new ColorImp(d, "GREAT"));
        public readonly GColorUIModel SOSO = new GColorUIModel(new ColorImp(d, "SOSO"));
        private static readonly ColorImp tmp = new ColorImp();
        private readonly COLOR badShift = new ColorShifting(bg(), new ColorImp(d, "BAD_SHIFT")).SetSpeed(1);
        private readonly COLOR goodShift = new ColorShifting(bg(), new ColorImp(d, "GOOD_SHIFT")).SetSpeed(1);

        private readonly COLOR border = COLOR.WHITE35;
        private readonly COLOR borderB = border.Shade(1.5);
        private readonly COLOR borderD = border.Shade(0.5);

        public readonly COLOR panBG = COLOR.WHITE15;

        public GCOLOR_UI()
        {
        }

        public readonly COLOR gold = new ColorImp(d, "GOLD");

        public sealed class GColorUIModel
        {
            public readonly COLOR normal;
            public readonly COLOR hovered;
            public readonly COLOR selected;
            public readonly COLOR inactive;

            private GColorUIModel(COLOR color)
            {
                this.inactive = color.Shade(0.55);
                this.normal = color.Shade(0.8);
                this.hovered = color;
                this.selected = color.Shade(1.2);
            }

            public COLOR Get(bool isActive, bool isSelected, bool isHovered)
            {
                if (!isActive)
                    return inactive;
                if (isHovered)
                    return hovered;
                if (isSelected)
                    return selected;
                return normal;
            }
        }

        public COLOR Border()
        {
            return COLOR.WHITE35;
        }

        public COLOR Bg()
        {
            return COLOR.WHITE10;
        }

        public COLOR Bg(bool isActive, bool isSelected, bool isHovered)
        {
            if (!isActive)
                return COLOR.WHITE10;
            if (isSelected)
                return COLOR.WHITE30;
            if (isHovered)
                return COLOR.WHITE25;
            return COLOR.WHITE15;
        }

        public static COLOR Color(COLOR color, bool isActive, bool isSelected, bool isHovered)
        {
            if (isSelected)
                return tmp.Set(color).Add(36);
            if (!isActive)
            {
                tmp.Set(color).SaturateSelf(0.7);
                return tmp.Add(-5);
            }
            if (isHovered)
                return tmp.Set(color).Add(20);
            return color;
        }

        public GColorUIModel BgHov()
        {
            return NORMAL;
        }

        public COLOR BadFlash()
        {
            return badShift;
        }

        public COLOR GoodFlash()
        {
            return goodShift;
        }

        public GColorUIModel BAD()
        {
            return BAD;
        }

        public GColorUIModel GOOD()
        {
            return GOOD;
        }

        public GColorUIModel SOSO()
        {
            return SOSO;
        }

        public void BadToGood(ColorImp imp, double v)
        {
            v = CLAMP.D(v, 0, 1);
            if (v < 0.5)
            {
                imp.Interpolate(BAD.normal, SOSO.normal, v * 2);
            }
            else
            {
                imp.Interpolate(SOSO.normal, GOOD.normal, (v - 0.5) * 2);
            }
        }

        public void Border(SPRITE_RENDERER ren, int X1, int X2, int Y1, int Y2)
        {
            borderB.Render(ren, X1, X1 + 1, Y1, Y2);
            borderB.Render(ren, X1, X2, Y1, Y1 + 1);
            borderD.Render(ren, X2 - 1, X2, Y1 + 1, Y2);
            borderD.Render(ren, X1 + 1, X2, Y2 - 1, Y2);
            border.Render(ren, X1 + 1, X2 - 1, Y1 + 1, Y2 - 1);
        }

        public void Border(SPRITE_RENDERER ren, RECTANGLE b, int m)
        {
            Border(ren, b.X1 + m, b.X2 - m, b.Y1 + m, b.Y2 - m);
        }

        public void BorderH(SPRITE_RENDERER ren, int X1, int X2, int Y1, int Y2)
        {
            borderB.Render(ren, X1, X1 + 1, Y1, Y2);
            border.Render(ren, X1 + 1, X1 + 2, Y1 + 1, Y2 - 1);
            borderD.Render(ren, X1 + 2, X1 + 3, Y1 + 2, Y2 - 2);

            borderD.Render(ren, X2 - 1, X2, Y1, Y2);
            border.Render(ren, X2 - 2, X2 - 1, Y1 + 1, Y2 - 1);
            borderB.Render(ren, X2 - 3, X2 - 2, Y1 + 2, Y2 - 2);

            borderB.Render(ren, X1, X2, Y1, Y1 + 1);
            border.Render(ren, X1 + 1, X2 - 1, Y1 + 1, Y1 + 2);
            borderD.Render(ren, X1 + 2, X2 - 2, Y1 + 2, Y1 + 3);

            borderD.Render(ren, X1, X2, Y2 - 1, Y2);
            border.Render(ren, X1 + 1, X2 - 1, Y2 - 2, Y2 - 1);
            borderB.Render(ren, X1 + 2, X2 - 2, Y2 - 3, Y2 - 2);
        }

        public void BorderH(SPRITE_RENDERER ren, RECTANGLE b, int m)
        {
            BorderH(ren, b.X1 + m, b.X2 - m, b.Y1 + m, b.Y2 - m);
        }
    }
}