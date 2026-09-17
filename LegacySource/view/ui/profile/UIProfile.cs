using System;
using game.faction;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.ui.manage;

namespace view.ui.profile
{
    public sealed class UIProfile : IFullView
    {
        private static readonly string ¤¤Name = "¤Player";
        private static readonly string ¤¤Save = "¤Save as Default";
        private static readonly string ¤¤Load = "¤Load Default";
        private static readonly string ¤¤FName = "¤Faction Name";
        private static readonly string ¤¤RName = "¤Ruler Name";

        static UIProfile()
        {
            D.ts(typeof(UIProfile));
        }

        public UIProfile(bool crashThing) : base(¤¤Name, FACTIONS.player().banner().BIG)
        {
            section.body().setWidth(WIDTH).setHeight(1);

            section.addRelBody(16, DIR.S, section(crashThing));
        }

        public static GuiSection section(bool crashThing)
        {
            GuiSection s = new GuiSection();
            s.add(new UIFactionBanner(FACTIONS.player()));

            s.addRelBody(16, DIR.N, info(crashThing));
            s.addRelBody(8, DIR.S, loadButts());
            return s;
        }

        private static GuiSection loadButts()
        {
            GuiSection s = new GuiSection();

            s.add(new GButt.ButtPanel(¤¤Save)
            {
                protected override void clickA()
                {
                    FactionProfileFlusher.flush(FACTIONS.player());
                }
            });

            s.addRightC(32, new GButt.ButtPanel(¤¤Load)
            {
                protected override void renAction()
                {
                    activeSet(FactionProfileFlusher.canLoad(FACTIONS.player()));
                }

                protected override void clickA()
                {
                    FactionProfileFlusher.load(FACTIONS.player());
                }
            });

            return s;
        }

        private static GuiSection info(bool crashThing)
        {
            GuiSection s = new GuiSection();
            StringInputSprite t = new StringInputSprite(24, UI.FONT().H2)
            {
                public override Str text()
                {
                    return FACTIONS.player().name;
                }

                protected override void change()
                {
                    if (FACTIONS.player().capitolRegion() != null)
                        FACTIONS.player().capitolRegion().info.name().clear().add(text());
                    base.change();
                }
            };
            GInput in = new GInput(t);
            s.add(new GHeader(¤¤FName));
            s.addRightCAbs(210, in);

            t = new StringInputSprite(24, UI.FONT().H2)
            {
                public override Str text()
                {
                    return FACTIONS.player().rulerName;
                }
            };
            in = new GInput(t);
            s.add(new GHeader(¤¤RName), 0, s.getLastY2() + 12);
            s.addRightCAbs(210, in);

            if (crashThing)
                s.addRelBody(16, DIR.E, new ColorPop().butt());

            return s;
        }
    }
}