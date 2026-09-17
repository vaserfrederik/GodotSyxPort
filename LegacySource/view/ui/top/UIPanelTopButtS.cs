using System;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.sprite;
using util.gui.misc;

namespace view.ui.top
{
    public abstract class UIPanelTopButtS : UIPanelTopButtAbs
    {
        public UIPanelTopButtS(SPRITE icon) : base(icon, width(), 24)
        {
        }

        protected override void render(SPRITE_RENDERER r, SPRITE label, GStat stat, bool active)
        {
            label.renderCY(r, body().x1() + 4, body().cY());

            if (active)
            {
                OPACITY.O35.bind();
                stat.adjust();
                COLOR.BLACK.render(r, body().x1() + 4 + label.width() + 0,
                        body().x1() + 4 + label.width() + 2 + stat.width() + 2, body().y1() + 4, body().y2() - 4);
                OPACITY.unbind();
                stat.renderCY(r, body().x1() + 4 + label.width() + 2, body().cY());
            }
        }

        public static int width()
        {
            return Icon.S + UI.FONT().S.height() * 4 - 6;
        }
    }
}