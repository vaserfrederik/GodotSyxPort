using System.Collections.Generic;
using game.faction.royalty;
using init.race.appearence;
using init.sprite.UI;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;

namespace view.world.ui.faction
{
    public class UIRoyalty
    {
        private static class Portrait : PortraitAbs
        {
            private readonly GETTER<Royalty> g;

            public Portrait(int scale, GETTER<Royalty> g) : base(scale)
            {
                this.g = g;
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                base.render(r, X1, X2, Y1, Y2);
            }

            protected override Induvidual indu()
            {
                return g.get().induvidual;
            }

            protected override int succ()
            {
                return g.get().successionI();
            }
        }

        private static readonly ArrayList<COLOR> cols = new ArrayList<COLOR>(
            new ColorImp(127, 127, 50),
            new ColorImp(100, 100, 100),
            new ColorImp(88, 75, 62)
        );

        public static void render(SPRITE_RENDERER r, int X1, int Y1, Royalty roy, int scale)
        {
            if (roy == null)
                return;

            int y = Y1;
            Induvidual ro = roy.induvidual;

            STATS.APPEARANCE().portraitRender(r, ro, X1, y, scale);

            int X2 = X1 + scale * RPortrait.P_WIDTH;

            if (roy.successionI() == 0)
                ro.race().appearance().crown.crowns().get(0).renderScaled(r, X1, Y1 + 8 * scale, scale);
            else
            {
                cols.getC(roy.successionI() - 1).bind();
                int w = scale / 2;
                w = CLAMP.i(w, 1, 2);
                UI.icons().s.star.render(r, X2 - Icon.S * w - 4, X2 - 4, Y1 + 4, Y1 + 4 + w * Icon.S);
                COLOR.unbind();
            }
        }

        public abstract class PortraitAbs : SPRITE.Imp
        {
            protected readonly int scale;

            public PortraitAbs(int scale) : base(RPortrait.P_WIDTH * scale, RPortrait.P_HEIGHT * scale)
            {
                this.scale = scale;
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                Induvidual ro = indu();

                if (ro == null)
                    return;

                int y = Y1;

                STATS.APPEARANCE().portraitRender(r, ro, X1, y, scale);

                if (succ() == 0)
                    ro.race().appearance().crown.crowns().get(0).renderScaled(r, X1, y + 8 * scale, scale);
                else
                {
                    cols.getC(succ() - 1).bind();
                    int w = scale / 2;
                    w = CLAMP.i(w, 1, 2);
                    UI.icons().s.star.render(r, X2 - Icon.S * w - 4, X2 - 4, Y1 + 4, Y1 + 4 + w * Icon.S);
                    COLOR.unbind();
                }
            }

            protected abstract Induvidual indu();
            protected abstract int succ();
        }
    }
}