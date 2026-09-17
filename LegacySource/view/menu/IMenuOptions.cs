using System;
using System.Collections.Generic;
using Init.Constant;
using Init.Settings;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sprite;
using Snake2D.Util.Sprite.Text;
using Util.Colors;
using Util.Data.INT;
using Util.Gui.Slider;
using Util.Gui.Table;
using Util.Text;

namespace View.Menu
{
    class IMenuOptions : GuiSection
    {
        public IMenuOptions(IMenu m, Font font, Font small)
        {
            MenuScreen sc = new MenuScreen(Dic.¤¤OPTIONS, GCOLOR.T().H1)
            {
                Back = () => m.SetMain()
            };

            Add(sc);

            int am = S.Get().All().Count;

            RENDEROBJ[] rs = new RENDEROBJ[am];
            am = 0;
            foreach (Setting s in S.Get().All())
            {
                rs[am++] = new OptionLine(s, small);
            }

            RENDEROBJ r = new GScrollRows(rs, 300, 0).View();

            r.Body().CenterIn(this.Body());
            Add(r);
        }

        private class OptionLine : GuiSection
        {
            private readonly SPRITE label;
            private readonly GSliderInt sl;
            private readonly Setting sett;
            private readonly Font font;

            public OptionLine(Setting s, Font font)
            {
                this.sett = s;
                this.font = font;
                Body().SetWidth(600);

                label = font.GetText(s.Name);

                INTE ii = new INTE()
                {
                    Min = () => 0,
                    Max = () => s.Max(),
                    Get = () => s.Get(),
                    Set = (t) =>
                    {
                        s.Set(t);
                        S.Get().ApplyRuntimeConfigs();
                    }
                };

                sl = new GSliderInt(ii, 135, false);
                Add(sl, Body().CX() - sl.Body().Width / 2, 0);
                Pad(0, 4);
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                base.Render(r, ds);
                GCOLOR.T().NORMAL.Bind();
                Str.TMP.Clear();
                sett.GetValue(Str.TMP);
                font.Render(r, Str.TMP, Body().X1() + 400, Body().Y1());
                GCOLOR.T().H1.Bind();
                label.RenderCY(r, Body().CX() - C.SCALE * 20 - label.Width(), Body().CY());
                COLOR.Unbind();
            }
        }
    }
}