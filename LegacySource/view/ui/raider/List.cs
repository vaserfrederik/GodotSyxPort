using System;
using System.Collections.Generic;

using Game;
using Game.Raiding;
using Init.Race.Appearance;
using Init.Settings;
using Init.Sprite.UI;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sprite.Text;
using Util.Colors;
using Util.Gui.Misc;
using Util.Gui.Table;

namespace View.UI.Raider
{
    public class List : GuiSection
    {
        private static readonly CharSequence ¤¤name = "Raiders";
        private static readonly CharSequence ¤¤raiding = "Raiding";
        private static readonly CharSequence ¤¤atLarge = "At Large!";
        private static readonly CharSequence ¤¤killed = "R.I.P.";
        private static readonly CharSequence ¤¤hiding = "In Hiding";
        private static readonly CharSequence ¤¤distant = "Distant";

        private Current c;
        private bool all = true;

        static
        {
            D.ts(typeof(List));
        }

        public List(Current c, int height) : base()
        {
            this.c = c;
            Add(new GHeader(¤¤name));

            AddRightC(16, new GButt.ButtPanel(UI.Icons().s.question)
            {
                ClickA = () =>
                {
                    all = !all;
                },
                RenAction = () =>
                {
                    SelectedSet(all);
                }
            });

            if (S.Get().Developer)
            {
                AddRightC(8, new GButt.ButtPanel(UI.Icons().s.question)
                {
                    ClickA = () =>
                    {
                        UIRaiding.Debug = !UIRaiding.Debug;
                    },
                    RenAction = () =>
                    {
                        SelectedSet(UIRaiding.Debug);
                    }
                });

                AddRightC(8, new GButt.ButtPanel(UI.Icons().s.arrow_right)
                {
                    ClickA = () =>
                    {
                        GAME.Raiders().Raid();
                    }
                });

                AddRightC(8, new GButt.ButtPanel(UI.Icons().s.fish)
                {
                    ClickA = () =>
                    {
                        GAME.Raiders().Reset();
                    }
                });
            }

            {
                List<RENDEROBJ> rows = new List<RENDEROBJ>();

                for (int ri = 0; ri < GAME.Raiders().Amount; ri++)
                {
                    rows.Add(new RR(ri));
                }

                GScrollRows rr = new GScrollRows(rows, 70 * ((height - Body().Height() - 16) / 70))
                {
                    PassesFilter = (i, o) =>
                    {
                        if (all)
                            return true;
                        final Raider rr = GAME.Raiders().All()[i];
                        if (UIRaiding.StatsVisible(rr))
                            return true;
                        return false;
                    }
                };

                AddRelBody(8, DIR.S, rr.View());
            }
        }

        private class RR : ClickableAbs
        {
            private readonly int ri;

            public RR(int ri)
            {
                this.ri = ri;
                Body.SetDim(450, 70);
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                // Implementation of HoverInfoGet
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                final Raider rr = GAME.Raiders().All()[ri];

                bool active = true;
                isSelected = ri == c.ri;

                COLOR c = COLOR.WHITE100;
                Str.TMP.Clear();

                if (GAME.Raiders().Current.Current() == rr)
                {
                    c = GCOLOR.T().IBAD;
                    Str.TMP.Add(¤¤raiding);
                }
                else if (rr.Defeated)
                {
                    active = false;
                    c = GCOLOR.T().IGREAT;
                    Str.TMP.Add(¤¤killed);
                }
                else if (!rr.HasInterest())
                {
                    active = false;
                    c = GCOLOR.T().WARNING;
                    Str.TMP.Add(¤¤distant);
                }
                else if (rr.IsScared())
                {
                    active = false;
                    c = GCOLOR.T().WARNING;
                    Str.TMP.Add(¤¤hiding);
                }
                else
                {
                    c = GCOLOR.T().IBAD;
                    Str.TMP.Add(¤¤atLarge);
                }

                GButt.ButtPanel.RenderBG(r, active, isHovered, isSelected, Body);

                if (UIRaiding.PortVisible(rr))
                {
                    RaiderPortrait.Render(r, Body.X1() + 8, Body.Y1() + 6, 1, rr.Indu, rr.Defeated);
                    if (rr.Defeated)
                    {
                        UI.Icons().m.anti.Render(r, Body.X1() + 8, Body.X1() + 8 + RPortrait.P_WIDTH, Body.Y1() + 6, Body.Y1() + 6 + RPortrait.P_WIDTH);
                    }
                }
                else
                {
                    UI.Icons().m.questionmark.RenderC(r, Body.X1() + 8 + RPortrait.P_WIDTH / 2, Body.Y1() + 6 + RPortrait.P_WIDTH / 2);
                }

                c.Bind();
                UI.FONT().S.Render(r, Str.TMP, Body.X1() + 64 + 16, Body.Y1() + 8 + 32);
                COLOR.Unbind();

                (active ? OPACITY.O100 : OPACITY.O50).Bind();
                UI.FONT().H2.RenderCropped(r, rr.Name, Body.X1() + 64, Body.Y1() + 8, 370);

                UI.Icons().s.money.Render(r, Body.X2() - 200, Body.Y1() + 8 + 32);
                if (UIRaiding.StatsVisible(rr))
                    Str.TMP.Clear().Add((int)rr.Worth);
                else
                    Str.TMP.Clear().Add('?');
                UI.FONT().S.Render(r, Str.TMP, Body.X2() - 200 + 24, Body.Y1() + 8 + 32);

                UI.Icons().s.fist.Render(r, Body.X2() - 100, Body.Y1() + 8 + 32);
                if (UIRaiding.StatsVisible(rr))
                    Str.TMP.Clear().Add((int)rr.Army.Power);
                else
                    Str.TMP.Clear().Add('?');

                UI.FONT().S.Render(r, Str.TMP, Body.X2() - 100 + 24, Body.Y1() + 8 + 32);
                COLOR.Unbind();
                OPACITY.Unbind();

                GButt.ButtPanel.RenderFrame(r, Body);
            }

            protected override void ClickA()
            {
                c.ri = ri;
            }
        }
    }
}