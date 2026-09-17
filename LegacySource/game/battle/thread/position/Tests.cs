using System;
using System.Collections.Generic;
using Game.Battle.Div;
using Init.Constant;
using Init.Sprite.UI;
using Settlement.Main;
using Snake2D.Renderer;
using Snake2D.Util.Misc;
using Snake2D.Util.Sprite.Text;
using Util.Rendering;
using View.Interrupter;

namespace Game.Battle.Thread.Position
{
    class Tests
    {
        public Tests(DivCentres status)
        {
            ON_TOP_RENDERABLE top = new ON_TOP_RENDERABLE
            {
                Render = (Renderer r, ShadowBatch shadowBatch, RenderData data, double ds) =>
                {
                    foreach (Div d in GAME.ARMIES().divisions())
                    {
                        DivCentre s = status.centre(d);
                        if (s.Cx == -1)
                            continue;
                        int x = s.CxSoft;
                        int y = s.CySoft;

                        x = data.transformGX(x);
                        y = data.transformGY(y);
                        UI.icons().s.alert.renderCScaled(r, x, y, C.SCALE * 2);
                        UI.FONT().S.renderC(r, x, y + 16 * C.SCALE * 2, Str.TMP.clear().add(s.inFormation()), C.SCALE * 2);
                    }
                }
            };

            IDebugPanel.add("battle div centres", new ACTION
            {
                Exe = () =>
                {
                    top.add();
                }
            });
        }
    }
}