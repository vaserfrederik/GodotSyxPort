using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.faction;
using init.sprite;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.common;
using util.gui.misc;
using util.text;
using world;
using world.map.regions;

namespace view.ui.profile
{
    public class UIFactionBanner : GuiSection
    {
        private static readonly string ¤¤Banner = "¤Banner";
        private static readonly string ¤¤BackGround = "¤Background";
        private static readonly string ¤¤Foreground = "¤Foreground";
        private static readonly string ¤¤Pole = "¤Pole";
        private static readonly string ¤¤Border = "¤Border";

        static UIFactionBanner()
        {
            D.ts(typeof(UIFactionBanner));
        }

        private readonly Faction f;

        public UIFactionBanner(Faction f)
        {
            this.f = f;
            Add(bannerHeader());
            AddRelBody(8, DIR.S, banner());
            AddRelBody(16, DIR.E, colors());
        }

        private GuiSection banner()
        {
            return new BitmapSpriteEditor(f.banner().sprite);
        }

        private GuiSection bannerHeader()
        {
            FBanner b = f.banner();
            GuiSection s = new GuiSection();
            s.Add(b.HUGE, 0, 0);
            s.AddRightC(8, b.BIG);
            s.AddRightC(8, b.MEDIUM);

            s.AddRightC(20, new GHeader(¤¤Banner));
            s.AddRightC(16, new GButt.ButtPanel(SPRITES.icons().m.arrow_left)
            {
                protected override void clickA()
                {
                    b.bannerTypeSet(b.bannerType() - 1);
                }
            });
            s.AddRightC(0, new GButt.ButtPanel(SPRITES.icons().m.arrow_right)
            {
                protected override void clickA()
                {
                    b.bannerTypeSet(b.bannerType() + 1);
                }
            });
            return s;
        }

        private GuiSection colors()
        {
            FBanner b = f.banner();
            GuiSection s = new GuiSection();
            s.Add(new GColorPicker(false, ¤¤BackGround)
            {
                public override ColorImp color()
                {
                    return b.colorBG();
                }

                public override void change()
                {
                    foreach (Region r in f.realm().all())
                        WORLD.MINIMAP().updateRegion(r);
                }
            });
            s.AddDownC(8, new GColorPicker(false, ¤¤Foreground)
            {
                public override ColorImp color()
                {
                    return b.colorFG();
                }
            });
            s.AddDownC(8, new GColorPicker(false, ¤¤Border)
            {
                public override ColorImp color()
                {
                    return b.colorBorder();
                }
            });
            s.AddDownC(8, new GColorPicker(false, ¤¤Pole)
            {
                public override ColorImp color()
                {
                    return b.colorPole();
                }
            });
            return s;
        }
    }
}