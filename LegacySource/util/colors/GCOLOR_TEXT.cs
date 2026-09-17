using System;
using game.faction;
using init.paths;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;

namespace util.colors
{
    public sealed class GCOLOR_TEXT
    {
        private Json d = new Json(PATHS.SPRITE_UI().getLikeHells("Colors.txt")).json("TEXT");

        public readonly COLOR IGREAT = new ColorImp(d, "IGREAT");
        public readonly COLOR IGOOD = new ColorImp(d, "IGOOD");
        public readonly COLOR INORMAL = new ColorImp(d, "INORMAL");
        public readonly COLOR IBAD = new ColorImp(d, "IBAD");
        public readonly COLOR IWORST = new ColorImp(d, "IWORST");

        public readonly COLOR HOVERABLE = new ColorImp(d, "HOVERABLE");
        public readonly COLOR H1 = new ColorImp(d, "H1"); //105,65,7
        public readonly COLOR H2 = new ColorImp(d, "H2");
        public readonly COLOR ERROR = new ColorImp(d, "ERROR");
        public readonly COLOR WARNING = new ColorImp(d, "WARNING");

        public readonly COLOR CLICKABLE = new ColorImp(d, "CLICKABLE");
        public readonly COLOR HOVERED = new ColorShifting(new ColorImp(d, "HOVERED"),
                new ColorImp(d, "HOVERED_SELECTED"));
        public readonly COLOR SELECTED = new ColorImp(d, "SELECTED");
        public readonly COLOR HOVER_SELECTED = new ColorShifting(new ColorImp(d, "HOVERED"),
                new ColorImp(d, "HOVERED_SELECTED"));
        public readonly COLOR INACTIVE = new ColorImp(d, "INACTIVE"); //COLOR.BROWN; //72, 58, 33
        public readonly COLOR NORMAL = new ColorImp(d, "NORMAL");//new ColorImp(110,75,25);
        public readonly COLOR NORMAL2 = new ColorImp(d, "NORMAL2");

        public COLOR faction(Faction faction)
        {
            if (faction == null)
                return COLOR.WHITE65;
            return ColorImp.TMP.set(faction.banner().colorBG()).shadeSelf(1.5);
        }

        private readonly ColorImp tmp = new ColorImp();

        public COLOR bronzeGold(double d)
        {
            d = CLAMP.d(d, 0, 1);
            if (d < 0.5)
            {
                tmp.interpolate(INACTIVE, H2, d * 2);
            }
            else
            {
                tmp.interpolate(H2, H1, (d - 0.5) * 2);
            }
            return tmp;
        }

        private GCOLOR_TEXT()
        {
        }
    }
}