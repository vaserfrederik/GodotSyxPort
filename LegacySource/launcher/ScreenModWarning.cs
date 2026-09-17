using System;
using System.Text;
using launcher.GUI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.text;

namespace launcher
{
    final class ScreenModWarning : GuiSection
    {
        private static readonly CharSequence ¤¤warn = "You are about to launch the game with code mods enabled. These mods contain code that could harm your PC, and you play at your own risk. Make sure you trust the author and the source of the mod.";
        private static readonly CharSequence ¤¤warnEasy = "You are about to launch the game with mods enabled. Be prepared that vanilla features might not work such as the tutorial.";
        private static readonly CharSequence ¤¤launch = "Launch";
        private static readonly CharSequence ¤¤cancel = "Cancel";

        static
        {
            D.ts(typeof(ScreenModWarning));
        }

        public ScreenModWarning(Launcher l)
        {
            CharSequence w = PATHS.SCRIPT().hasExternal(l.s.mods.get()) ? ¤¤warn : ¤¤warnEasy;

            Text t = new Text(l.res.font, w).setScale(1);
            t.setMaxWidth(600);
            add(t, 0, 0);

            GuiSection bb = new GuiSection();

            bb.add(new BText(l.res, ¤¤launch, 200)
            {
                protected override void clickA()
                {
                    l.s.save();
                    Launcher.startGame = true;
                    CORE.annihilate();
                }
            });

            bb.addRightC(0, new BText(l.res, ¤¤cancel, 200)
            {
                protected override void clickA()
                {
                    l.setMods();
                }
            });

            addRelBody(8, DIR.S, bb);

            body().moveC(Sett.WIDTH / 2, Sett.HEIGHT / 2);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            OPACITY.O75.bind();
            COLOR.BLACK.render(r, 0, Sett.WIDTH, 0, Sett.HEIGHT);
            OPACITY.unbind();
            base.render(r, ds);
        }
    }
}