using System;
using System.Collections.Generic;
using game;
using game.battle.util;
using game.raiding;
using init.constant;
using init.settings;
using init.sprite.UI;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

namespace view.ui.raider
{
    internal class Current : GuiSection
    {
        private static readonly CharSequence ¤¤atLargeD = "This bandit is currently at large, and is contemplating their next raid on us.";
        private static readonly CharSequence ¤¤killed = "This bandit is but a memory and was brought to justice {0}.";
        private static readonly CharSequence ¤¤hidingD = "This bandit does not have the strength to attack us currently and will leave us alone for now.";
        private static readonly CharSequence ¤¤distantD = "We are too insignificant and poor for this bandit to bother us.";
        private static readonly CharSequence ¤¤raidingD = "This bandit is currently raiding you.";

        private static readonly CharSequence ¤¤Ransom = "Ransom";
        private static readonly CharSequence ¤¤Raids = "Raids";

        static Current()
        {
            D.ts(Current);
        }

        private int ri = 0;

        public Current(int height) : base()
        {
            AddDown(0, new GStat
            {
                Update = (text) =>
                {
                    text.SetFont(UI.FONT().H2);
                    GCOLOR.T().H1.Bind();
                    text.Add(Rr().Name);
                }
            }.r(DIR.NW));

            AddDown(8, new GStat
            {
                Update = (text) =>
                {
                    text.SetMaxWidth(700);
                    text.SetMultipleLines(true);
                    if (GAME.Raiders().Current.Current() == Rr())
                    {
                        text.Color(GCOLOR.T().IBAD).Add(¤¤raidingD);
                    }
                    else if (Rr().Defeated)
                    {
                        Str.TMP.Clear();
                        DicTime.SetDate(Str.TMP.Clear(), (int)Rr().SecondDefeated);
                        text.Add(¤¤killed);
                        text.Insert(0, Str.TMP);
                        text.Color(GCOLOR.T().IGREAT);
                    }
                    else if (!Rr().HasInterest())
                    {
                        text.Color(GCOLOR.T().WARNING).Add(¤¤distantD);
                    }
                    else if (Rr().IsScared())
                    {
                        text.Color(GCOLOR.T().WARNING).Add(¤¤hidingD);
                    }
                    else
                    {
                        text.Color(GCOLOR.T().IBAD).Add(¤¤atLargeD);
                    }
                }
            }.r(DIR.NW));

            {
                GuiSection s = new GuiSection();
                int dd = 150;
                int gi = 0;

                s.AddGridD(new GStat
                {
                    Update = (text) =>
                    {
                        if (UIRaiding.StatsVisible(Rr()))
                        {
                            GFORMAT.i(text, Rr().Army.Men);
                        }
                        else
                        {
                            text.Add('?');
                        }
                    }
                }.hv(Dic.¤¤Soldiers), gi++, dd, 100, dd, DIR.N);

                s.AddGridD(new GStat
                {
                    Update = (text) =>
                    {
                        if (UIRaiding.StatsVisible(Rr()))
                        {
                            GFORMAT.i(text, Rr().Army.Power);
                        }
                        else
                        {
                            text.Add('?');
                        }
                    }
                }.hv(Dic.¤¤Power), gi++, dd, 100, dd, DIR.N);

                s.AddGridD(new GStat
                {
                    Update = (text) =>
                    {
                        if (UIRaiding.StatsVisible(Rr()))
                        {
                            GFORMAT.i(text, (int)Rr().Worth);
                        }
                        else
                        {
                            text.Add('?');
                        }
                    }
                }.hv(¤¤Ransom), gi++, dd, 100, dd, DIR.N);

                s.AddGridD(new GStat
                {
                    Update = (text) =>
                    {
                        GFORMAT.i(text, (int)Rr().Raids);
                    }
                }.hv(¤¤Raids), gi++, dd, 100, dd, DIR.N);

                Add(s, 80, Body().y2() + 40);
            }

            {
                GuiSection ss = new GuiSection();

                RaiderPortrait p = new RaiderPortrait(4)
                {
                    protected override Induvidual Raider()
                    {
                        Dead(Rr().Defeated);
                        return Rr().Indu;
                    }
                };

                SPRITE sp = new SPRITE.Imp(p.Width(), p.Height())
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        if (UIRaiding.PortVisible(Rr()) || S.Get().Developer)
                        {
                            p.Render(r, X1, Y1);
                        }
                        else
                        {
                            UI.Icons().m.questionmark.RenderC(r, X1, X2, Y1, Y2);
                        }

                        GCOLOR.UI().border().RenderFrame(r, X1, X2, Y1, Y2, 2, 2);
                    }
                };

                ss.Add(sp, 0, 0);

                int am = 10;

                ArrayList<GuiSection> rows = new ArrayList<GuiSection>((int)Math.Ceiling(Config.Battle().DIVISIONS_PER_ARMY / (double)am));

                for (int i = 0; i < rows.Max(); i++)
                {
                    rows.Add(new GuiSection());
                }

                for (int i = 0; i < Config.Battle().DIVISIONS_PER_ARMY; i++)
                {
                    GuiSection s = rows.Get(i / am);
                    s.AddRightC(2, new Button(i));
                }

                ss.AddRelBody(8, DIR.E, new GScrollRows(rows, sp.Height())
                {
                    protected override bool PassesFilter(int i, RENDEROBJ o)
                    {
                        return i <= Math.Ceiling(Rr().Army.SDivs.Size() / (double)am);
                    }
                }.View());

                Add(ss, 0, Body().y2() + 16);
            }
        }

        private static readonly int dim = 50;

        private class Button : HOVERABLE.HoverableAbs
        {
            private readonly int ii;

            public Button(int ii) : base()
            {
                this.ii = ii;
                Body.SetDim(dim);
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                if (ii >= Rr().Army.SDivs.Size())
                    return;

                DIV_SPEC d = Rr().Army.SDivs.Get(ii);

                if (d == null)
                    return;

                GButt.ButtPanel.RenderBG(r, true, isHovered, false, Body);

                if (Rr().Raids > 0 || UIRaiding.Debug)
                {
                    d.Race().Appearance().Icon.RenderC(r, Body.CX(), Body.CY() - 6);

                    VIEW.UI().Div.RenderPower(Body.X2() - 16, Body.Y1() + 4, r, GAME.Battle().Power.Get(d));

                    int w = (int)((Body.Width() - 8) * CLAMP.d((double)(d.Men() + Config.Battle().MEN_PER_DIVISION / 5) / Config.Battle().MEN_PER_DIVISION, 0, 1));

                    GMeterCol col = GMeter.C_REDBLUE;

                    GMeter.Render(r, col, 1.0, Body.X1() + 4, Body.X1() + 4 + w, Body.Y2() - 14, Body.Y2() - 6);
                    OPACITY.Unbind();
                }
                else
                {
                    UI.Icons().s.question.RenderC(r, Body.CX(), Body.CY());
                }

                GButt.ButtPanel.RenderFrame(r, Body);
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                if (ii >= Rr().Army.SDivs.Size())
                    return;
                GBox b = (GBox)text;
                if (Rr().Raids > 0 || UIRaiding.Debug)
                {
                    DIV_SPEC d = Rr().Army.SDivs.Get(ii);
                    VIEW.UI().Div.Normal.Hover(d, b);
                }
                else
                {
                    b.Add(b.Text().Add('?'));
                }
            }
        }

        public Raider Rr()
        {
            return GAME.Raiders().All().Get(ri);
        }
    }
}