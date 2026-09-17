using System;
using System.Collections.Generic;
using System.IO;
using Launcher.GUI;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Clickable;
using Util.Text;

namespace Launcher
{
    final class ScreenLang : GuiSection
    {
        private readonly PATH plang = PATHS_BASE.langs();
        private CharSequence hov;
        private readonly Launcher l;

        public ScreenLang(Launcher l, bool exit) : base()
        {
            this.l = l;
            int i = 0;
            int cols = 10;
            int width = 64;
            int height = 64;

            AddGridD(new Butt(null, i), i++, cols, width, height, DIR.C);
            foreach (string s in plang.Folders())
            {
                AddGridD(new Butt(s, i), i++, cols, width, height, DIR.C);
            }

            body().MoveC(Sett.WIDTH / 2, Sett.HEIGHT / 2);

            D.GInit(this);

            if (exit)
            {
                CLICKABLE b = new BText(l.res, D.G("Back"))
                {
                    protected override void ClickA()
                    {
                        l.SetMain();
                    }
                };
                b.body().MoveY1(16).MoveX2(Sett.WIDTH - 16);
                Add(b);
            }
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            OPACITY.O75.Bind();
            COLOR.BLACK.Render(r, 0, Sett.WIDTH, 0, Sett.HEIGHT);
            OPACITY.Unbind();
            base.Render(r, ds);
            if (hov != null)
            {
                l.res.font.RenderC(r, Sett.WIDTH / 2, body().y2() + 24, hov);
                hov = null;
            }
        }

        public CLICKABLE Butt()
        {
            int si = 0;
            if (!l.s.lang.Get().Equals(""))
            {
                foreach (string s in plang.Folders())
                {
                    si++;
                    if (l.s.lang.Get().Equals(s))
                    {
                        break;
                    }
                }
            }

            return new GUI.Button(l.res.langs[si].Scaled(2.0f))
            {
                protected override void ClickA()
                {
                    l.SetLang();
                }
            };
        }

        private class Butt : GUI.Button
        {
            private readonly string code;
            private readonly string name;

            public Butt(string folder, int iconI) : base(l.res.langs[iconI].Scaled(2.0f))
            {
                if (folder == null)
                {
                    code = "";
                    name = "English";
                }
                else
                {
                    code = folder;
                    Json j = new Json(plang.GetFolder(folder).Gets("_Info"));
                    name = j.Text("NAME") + " " + (int)(100 * j.D("COVERAGE")) + "%";
                }
            }

            public override bool Hover(COORDINATE mCoo)
            {
                if (base.Hover(mCoo))
                {
                    hov = name;
                    return true;
                }
                return false;
            }

            protected override void ClickA()
            {
                if (l.s.lang.Get().Equals(code))
                {
                    l.SetMain();
                }
                else
                {
                    l.s.lang.Set(code);
                    l.s.Save();
                    l.Reboot();
                }
            }
        }
    }
}