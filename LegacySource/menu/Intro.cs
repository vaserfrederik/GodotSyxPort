using System;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Gui;
using Snake2D.Util.Light;
using Snake2D.Util.Sprite;
using Util.Text;

namespace Menu
{
    internal sealed class Intro
    {
        private double timer = 0;
        private int stage = 0;
        private AmbientLight moon = new AmbientLight();
        {
            D.GInit(this);
        }
        private HOVERABLE head = GUI.GetBigText(D.G("greeting", "HAIL MIGHTY DESPOT!"));

        private SPRITE[] greeting = new SPRITE[]
        {
            GUI.GetSmallText(D.G("0", "You are about to enter the world of Syx.")),
            GUI.GetSmallText(D.G("1", "")),
            GUI.GetSmallText(D.G("2", "This game is still in active development and some features might be changed, removed or added.")),
            GUI.GetSmallText(D.G("3", "If you're a pirate, no one will come after you. But when times are good, consider a purchase!")),
            GUI.GetSmallText(D.G("4", "Suggestions and feedback are welcome.")),
            GUI.GetSmallText(D.G("5", "Beware, if you are using mods, you play at your PC's own risk.")),
            GUI.GetSmallText(D.G("6", "")),
            GUI.GetSmallText(D.G("7", "May the Astari guide your hand to swift victory...")),
        };
        private readonly ScMain main;
        private readonly Background bg;

        public Intro(ScMain main, Background bg)
        {
            moon.Set(AmbientLight.Strongmoonlight, 0);
            this.main = main;
            this.bg = bg;
        }

        public bool Update(float ds)
        {
            timer += ds;

            switch (stage)
            {
                case 0:
                    moon.Set(AmbientLight.Strongmoonlight, timer / 1.5);
                    if (timer > 1.5)
                    {
                        moon.Set(AmbientLight.Strongmoonlight, 1.0);
                        timer = 0;
                        stage++;
                    }
                    break;
                case 1:
                    if (timer > 11)
                    {
                        timer = 0;
                        stage++;
                    }
                    break;
                case 2:
                    moon.Set(AmbientLight.Strongmoonlight, 1.0 - timer * 2.0);
                    if (timer > 0.5)
                    {
                        moon.Set(AmbientLight.Strongmoonlight, 0);
                        timer = 0;
                        stage++;
                    }
                    break;
                case 3:
                    if (timer > 0.5)
                    {
                        timer = 0;
                        stage++;
                    }
                    break;
                case 4:
                    moon.Set(AmbientLight.Strongmoonlight, timer / 2.0);
                    if (timer > 1.0)
                    {
                        mask.SetRed((int)((timer - 1) * 128));
                        mask.SetGreen((int)((timer - 1) * 128));
                        mask.SetBlue((int)((timer - 1) * 128));
                    }

                    if (timer > 2.0)
                    {
                        moon.Set(AmbientLight.Strongmoonlight, 1.0);
                        timer = 0;
                        stage++;
                    }
                    break;
                case 5:
                    return false;

            }

            return true;
        }

        private readonly ColorImp mask = new ColorImp(COLOR.BLACK);

        protected void Render(Renderer r, float ds)
        {
            if (stage >= 0 && stage < 3)
            {
                int y = GUI.Inner.CY() - 100;
                moon.Register(C.DIM());
                head.Body().MoveY1(y);
                head.Body().CenterX(C.DIM());
                head.Render(r, ds);

                y += head.Body().Height() * 2;
                foreach (SPRITE s in greeting)
                {
                    int x1 = (C.WIDTH() - s.Width()) / 2;
                    s.Render(r, x1, y);
                    y += s.Height();
                }
            }

            if (stage > 3)
            {
                moon.Register(C.DIM());
                main.Render(r, ds);
            }

            if (stage >= 4)
            {
                r.NewLayer(false, 0);
                mask.Bind();
                bg.Render(r, ds);
            }
        }
    }
}