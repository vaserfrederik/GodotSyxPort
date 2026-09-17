using System;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.text;

namespace game.boosting.tmp
{
    public class TmpBoostingButt
    {
        private static readonly string ¤¤no = "Nothing out of the ordinary is having an effect.";

        static TmpBoostingButt()
        {
            D.ts(typeof(TmpBoostingButt));
        }

        public static <T> CLICKABLE make(GETTER<T> get, TmpBoostable<T> type) where T : INDEXED
        {
            GButt.ButtPanel p = new GButt.ButtPanel(UI.icons().l.event)
            {
                renAction = () =>
                {
                },

                render = (SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered) =>
                {
                    base.render(r, ds, isActive, isSelected, isHovered);
                    if (!type.any(get.get()))
                    {
                        OPACITY.O50.bind();
                        COLOR.BLACK.render(r, body, -2);
                        OPACITY.unbind();
                    }
                },

                clickA = () =>
                {
                    base.clickA();
                },

                hoverInfoGet = (GUI_BOX text) =>
                {
                    if (type.any(get.get()))
                    {
                        GBox b = (GBox)text;
                        type.hover(b, get.get());
                    }
                    else
                    {
                        text.text(¤¤no);
                    }
                }
            };

            return p;
        }
    }
}