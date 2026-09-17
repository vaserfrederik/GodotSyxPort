using System;
using System.Collections.Generic;
using Util.Gui.Misc;
using View.Main;
using View.Sett.IDebugPanelSett;
using Util.Gui.Renderable;
using Util.Gui;
using Util.Misc;
using Util.Color;
using Util.SpriteRenderer;

namespace View.Sett.Ui.Subject
{
    internal class SPortraitsDebug : GuiSection
    {
        private int start = 0;
        private int race;
        private Color bg = Color.Black;

        public SPortraitsDebug()
        {
            body().setWidth(C.WIDTH());
            body().setHeight(C.HEIGHT());

            RenderObj r;

            r = new GButt.Glow("randomize")
            {
                protected override void ClickA()
                {
                    start += 50;
                }
            };

            r.body().moveX1Y1(20, 20);
            Add(r);

            r = new GButt.Glow("racify")
            {
                protected override void ClickA()
                {
                    race++;
                }
            };

            AddRightC(20, r);

            r = new GButt.Glow("green")
            {
                protected override void ClickA()
                {
                    bg = Color.Green100;
                }
            };

            AddRightC(20, r);

            IDebugPanelSett.Add("Portraits", new Action()
            {
                public void Execute()
                {
                    VIEW.inters().section.Activate(this);
                }
            });
        }

        public override void Render(SpriteRenderer r, float ds)
        {
            bg.Render(r, body());
            base.Render(r, ds);
            ENTITY[] ents = SETT.ENTITIES().getAllEnts();

            int m = 20;
            int y = 60;
            int x = m;
            int w = RPortrait.P_WIDTH * 4;
            int h = RPortrait.P_HEIGHT * 4;

            Race ra = RACES.all().get(race % RACES.all().size());

            for (int i = 0; i < ents.Length; i++)
            {
                int k = (i + start) % ents.Length;
                if (!(ents[k] is Humanoid))
                    continue;
                Humanoid hu = (Humanoid)ents[k];

                if (hu.race() != ra)
                    continue;

                STATS.APPEARANCE().portraitRender(r, hu.indu(), x, y, 4);

                x += w + m;
                if (x + w > C.WIDTH())
                {
                    y += h + m;
                    if (y + h > C.HEIGHT())
                        break;
                    x = m;
                }
            }
        }
    }
}