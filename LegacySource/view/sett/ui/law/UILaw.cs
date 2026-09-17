using System.Collections.Generic;
using Init.Settings;
using Init.Sprite;
using Init.Type;
using Settlement.Stats;
using Settlement.Stats.Law;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Util.Gui.Misc;
using Util.Text;
using View.Interrupter;

namespace View.Sett.Ui.Law
{
    public class UILaw : ISidePanel
    {
        private readonly GuiSection sec = new GuiSection();

        public UILaw()
        {
            TitleSet(Dic.¤¤Law);

            GuiSection sel = new GuiSection();
            int w = 0;

            int HH = HEIGHT - 64;

            {
                GuiSection s = new UILawCrimeList(HH, HCLASSES.CITIZEN());
                w = Math.Max(s.Body.Width, w);
                sel.AddRightC(0, Butt(HCLASSES.CITIZEN().Names, s));
                if (sec.Elements.Count == 0)
                    Set(s);
            }

            {
                GuiSection s = new UILawCrimeList(HH, HCLASSES.SLAVE());
                w = Math.Max(s.Body.Width, w);
                sel.AddRightC(0, Butt(HCLASSES.SLAVE().Names, s));
                if (sec.Elements.Count == 0)
                    Set(s);
            }

            {
                GuiSection s = new WarCriminals(HH);
                w = Math.Max(s.Body.Width, w);
                sel.AddRightC(0, Butt(CRIMES.WAR().Names, s));
                if (sec.Elements.Count == 0)
                    Set(s);
            }

            {
                GButt.ButtPanel cur = new GButt.ButtPanel(SPRITES.Icons().M.Building)
                {
                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        text.Title(Curfew.¤¤name);
                        text.Text(Curfew.¤¤desc);
                        text.NL(8);
                        if (STATS.LAW().GetCurfew().IsSetForADay())
                            text.Text(Dic.¤¤Deactivate);
                        else
                            text.Text(Dic.¤¤Activate);
                    },

                    RenAction = () => selectedSet(STATS.LAW().GetCurfew().IsSetForADay()),

                    ClickA = () => STATS.LAW().GetCurfew().SetForADay(!STATS.LAW().GetCurfew().IsSetForADay())
                };

                cur.Pad(14, 0);

                GuiSection s = new GuiSection();
                w = Math.Max(s.Body.Width, w);
                sel.AddRightC(0, cur);
            }

            if (S.Get().Developer)
            {
                GButt.ButtPanel b = new GButt.ButtPanel(UI.Icons().S.ArrowUp)
                {
                    ClickA = () => STATS.LAW().Debug = 1.0
                };
                sel.AddRightC(0, b);
            }

            w = Math.Max(w, sel.Body.Width);
            sec.Body.SetWidth(w);
            Set(sec.Elements[0]);

            Section.Add(sel);
            sec.Body.CenterIn(Section);
            sec.Body.MoveY1(48);
            Section.Add(sec);
        }

        private RENDEROBJ Butt(CharSequence name, GuiSection s)
        {
            GButt.ButtPanel b = new GButt.ButtPanel(name)
            {
                ClickA = () => Set(s),

                RenAction = () => selectedSet(sec.Elements[0] == s)
            };

            b.Body.SetWidth(160);
            return b;
        }

        private void Set(RENDEROBJ s)
        {
            int x1 = sec.Body.X1;
            int y1 = sec.Body.Y1;
            int w = sec.Body.Width;

            sec.Clear();
            sec.Body.SetWidth(w);
            sec.AddRelBody(0, DIR.S, s);
            sec.Body.MoveX1Y1(x1, y1);
        }
    }
}