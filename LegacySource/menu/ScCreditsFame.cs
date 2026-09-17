using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Menu
{
    using static GUI;
    using static GUI.Inner;

    using Core;
    using Init.Constant;
    using Init.Paths;
    using Init.Sprite.UI;
    using Util.Color;
    using Util.DataTypes;
    using Util.Gui;
    using Util.Gui.Hoverable;
    using Util.Gui.Clickable;
    using Util.Gui.Renderable;
    using Util.Light;
    using Util.Misc;
    using Util.Rnd;
    using Util.Sets;
    using Util.Sprite;
    using Util.Sprite.Text;
    using Util.Text;
    using View.Menu;

    class ScCreditsFame : Shadower, IScreen
    {
        private readonly IClickable next;
        private readonly IClickable prev;
        private int currentScreen = 0;
        private readonly ArrayListGrower<Screen> all = new ArrayListGrower<Screen>();

        private readonly Text sname = UI.FONT().H2.GetText(200);
        private readonly Text stitles = UI.FONT().M.GetText(200);

        static readonly CharSequence ¤¤name = "¤hall of fame";
        static
        {
            D.ts(typeof(ScCreditsFame));
        }

        public ScCreditsFame(Menu menu)
        {
            D.gInit(this);

            var screen = new MenuScreen(¤¤name, GUI.labelColor)
            {
                Back = () => menu.SwitchScreen(menu.main)
            };

            Add(screen);

            all.Add(Legends(menu));
            all.Add(Heroes(menu));
            all.Add(Others());

            prev = GetNavButt("<<");
            prev.ClickActionSet(new Action(() =>
            {
                if (currentScreen > 0)
                    currentScreen--;
                prev.ActiveSet(currentScreen > 0);
                next.ActiveSet(currentScreen < all.Size - 1);
            }));
            screen.AddButt(prev);

            next = GetNavButt(">>");
            next.ClickActionSet(new Action(() =>
            {
                if (currentScreen < all.Size - 1)
                    currentScreen++;
                prev.ActiveSet(currentScreen > 0);
                next.ActiveSet(currentScreen < all.Size - 1);
            }));
            screen.AddButt(next);

            Body().CenterX(C.DIM());

            prev.ActiveSet(currentScreen > 0);
            next.ActiveSet(currentScreen < all.Size - 1);

            foreach (var s in all)
            {
                s.Body().CenterIn(MenuScreen.inner);
                s.Body().MoveY1(MenuScreen.inner.y1());
            }
        }

        private IList<Screen> Legends(Menu menu)
        {
            string[] names = new string[] {
                "Jake",
                "Natalia Jasinska",
                "Gianluca Borg",
                "Superwutz",
                "ProRt",
                "Connor Bryant",
                "JollyWarhammer",
                "Bendigeidfran",
            };
            string[] descs = new string[] {
                "Supreme Developer, Creator of worlds, Bringer of Syxians",
                "Mistress of soundtracks",
                "High Councelor, Spokesman of the Plebs, Guardian of History",
                "Sacred voice of modability, Father of the Agonosh, He whose name is hard to remember",
                "First knighted, Finder of bugs",
                "Generous benefactor",
                "Warrior Monk",
                "Champion of Art",
            };

            LinkedList<Screen> screens = new LinkedList<Screen>();
            Screen current = null;
            var light = new PointLight();
            light.SetRadius(200);
            light.SetZ(200);
            double ii = 1.5;
            light.SetRed(ii).SetGreen(ii).SetBlue(ii);

            for (int i = 0; i < names.Length; i++)
            {
                if (i % 4 == 0)
                {
                    current = new Screen();
                    screens.Add(current);
                }

                string name = names[i];
                string desc = descs[i];
                SPRITE frame = menu.Res.S().CreditsBigFrame;
                SPRITE ps = menu.Res.S().CreditsBig[i];

                current.AddRightC(16, new HOVERABLE.HoverableAbs(frame)
                {
                    Render = (r, ds, isHovered) =>
                    {
                        if (isHovered)
                        {
                            light.Set(Body.CX(), Body.CY());
                            light.Register();
                        }

                        ps.Render(r, Body.X1(), Body.Y1());
                        frame.Render(r, Body.X1(), Body.Y1());
                    },

                    Hover = (mCoo) =>
                    {
                        sname.Clear();
                        if (base.Hover(mCoo))
                        {
                            sname.Set(name);
                            stitles.Clear().Set(desc);
                            return true;
                        }
                        return false;
                    }
                });
            }
            return screens;
        }

        private IList<Screen> Heroes(Menu menu)
        {
            string[] names = new string[] {
                "Laki 95",
                "Dr. Kelloggs",
                "Licher",
                "Qbjik",
                "ProRt",
                "Connor Bryant",
                "JollyWarhammer",
                "Bendigeidfran",
            };
            string[] descs = new string[] {
                "Supreme Developer, Creator of worlds, Bringer of Syxians",
                "Mistress of soundtracks",
                "High Councelor, Spokesman of the Plebs, Guardian of History",
                "Sacred voice of modability, Father of the Agonosh, He whose name is hard to remember",
                "First knighted, Finder of bugs",
                "Generous benefactor",
                "Warrior Monk",
                "Champion of Art",
            };

            LinkedList<Screen> screens = new LinkedList<Screen>();
            Screen current = null;
            var light = new PointLight();
            light.SetRadius(200);
            light.SetZ(200);
            double ii = 1.5;
            light.SetRed(ii).SetGreen(ii).SetBlue(ii);

            for (int i = 0; i < names.Length; i++)
            {
                if (i % 4 == 0)
                {
                    current = new Screen();
                    screens.Add(current);
                }

                string name = names[i];
                string desc = descs[i];
                SPRITE frame = menu.Res.S().CreditsBigFrame;
                SPRITE ps = menu.Res.S().CreditsBig[i];

                current.AddRightC(16, new HOVERABLE.HoverableAbs(frame)
                {
                    Render = (r, ds, isHovered) =>
                    {
                        if (isHovered)
                        {
                            light.Set(Body.CX(), Body.CY());
                            light.Register();
                        }

                        ps.Render(r, Body.X1(), Body.Y1());
                        frame.Render(r, Body.X1(), Body.Y1());
                    },

                    Hover = (mCoo) =>
                    {
                        sname.Clear();
                        if (base.Hover(mCoo))
                        {
                            sname.Set(name);
                            stitles.Clear().Set(desc);
                            return true;
                        }
                        return false;
                    }
                });
            }
            return screens;
        }

        private IList<Screen> Others()
        {
            List<Tuple<string, string[]>> allData = new List<Tuple<string, string[]>>();

            try
            {
                allData.Add(new Tuple<string, string[]>(
                    D.g("citizens"),
                    File.ReadAllLines(Paths.Citizens)
                ));
                allData.Add(new Tuple<string, string[]>(
                    D.g("citizens"),
                    File.ReadAllLines(Paths.Citizens)
                ));
                allData.Add(new Tuple<string, string[]>(
                    D.g("citizens"),
                    File.ReadAllLines(Paths.Citizens)
                ));
            }
            catch (Exception)
            {
            }

            LinkedList<Screen> screens = new LinkedList<Screen>();

            foreach (var t in allData)
            {
                int x1 = inner.X1();
                int x2 = inner.X2() + 100;

                int scale = 2;
                int margin = 20;
                int height = UI.FONT().H2.Height() * scale;
                int y2 = inner.Y2() - height;

                string[] names = t.Item2;
                string title = t.Item1;

                int i = 0;
                while (i < names.Length)
                {
                    var s = new Screen
                    {
                        Render = (r, ds) =>
                        {
                            light.Set(inner.CX(), inner.CY());
                            light.Register();
                            OPACITY.O99.Bind();
                            base.Render(r, ds);
                            OPACITY.Unbind();
                        }
                    };
                    s.Add(new RENDEROBJ.Sprite(UI.FONT().H2.GetText((object)title).SetScale(2)));
                    s.Body().MoveY1(inner.Y1());
                    s.Body().CenterX(inner);
                    int y1 = s.Body().Y2() + 10;
                    int x = x1;
                    while (i < names.Length)
                    {
                        x += RND.RInt(20);
                        var o = new RENDEROBJ.Sprite(new Name(names[i]));
                        i++;
                        o.SetColor(COLOR.WHITE100);
                        if (x + o.Body().Width() > x2)
                        {
                            y1 += height;
                            if (y1 > y2)
                                break;
                            x = x1 + RND.RInt(30);
                        }
                        o.Body().MoveX1(x).MoveY1(y1 + RND.RInt(20));
                        x += o.Body().Width() + margin;
                        s.Add(o);
                    }
                    screens.Add(s);
                }
            }

            return screens;
        }

        private class Name : SPRITE
        {
            private readonly int width;
            private readonly CharSequence name;

            public Name(CharSequence s)
            {
                this.name = s;
                width = UI.FONT().H2.GetDim(s).X() * 2;
            }

            public int Width()
            {
                return width;
            }

            public int Height()
            {
                return UI.FONT().H2.Height() * 2;
            }

            public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                UI.FONT().H2.Render(r, name, X1, Y1, 2);
            }

            public void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            base.Render(r, ds);
            if (sname.Length() != 0)
            {
                COLORS.label.Bind();
                sname.RenderC(r, Body().CX(), inner.Y2());
                COLORS.copper.Bind();
                stitles.RenderC(r, Body().CX(), inner.Y2() + sname.Height() + 4);
                COLOR.Unbind();
                sname.Clear();
                stitles.Clear();
            }
        }

        public override void RenderBackground(Background back, float ds, COORDINATE mCoo)
        {
            back.RenderFame(CORE.Renderer(), ds, mCoo, all.Get(currentScreen).ran);
            all.Get(currentScreen).Render(CORE.Renderer(), ds);
        }

        public override bool Hover(COORDINATE mCoo)
        {
            all.Get(currentScreen).Hover(mCoo);
            return base.Hover(mCoo);
        }

        public override bool Back(Menu menu)
        {
            menu.SwitchScreen(menu.main);
            return true;
        }

        private class Screen : GuiSection
        {
            public readonly double ran = RND.RFloat();
        }
    }
}