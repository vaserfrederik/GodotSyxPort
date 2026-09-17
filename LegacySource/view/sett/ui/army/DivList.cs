using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Sett.Ui.Army
{
    public class DivList : GuiSection
    {
        private static int xs = 10;
        private static int scale = 1;
        private readonly List<Card> cards = new List<Card>(Config.Battle().DIVISIONS_PER_ARMY);
        private readonly List<Card> current = new List<Card>(Config.Battle().DIVISIONS_PER_ARMY);
        private readonly List<Div> selection;
        private Card clicked;
        private bool dragging;

        public DivList(int height, List<Div> selection)
        {
            this.selection = selection;

            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                cards.Add(new Card(d.IndexArmy()));
            }

            var bu = new GTableBuilder()
            {
                NrOFEntries = () =>
                {
                    int am = Math.Clamp(current.Count, 0, Config.Battle().DIVISIONS_PER_ARMY);
                    return (int)Math.Ceiling((double)am / xs);
                }
            };

            bu.Column(null, xs * VIEW.UI().Div.SettCivic.Width() * scale, new GRowBuilder()
            {
                Build = ier =>
                {
                    return Row(ier);
                }
            });

            Add(bu.CreateHeight(height, false));
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            Init();
            base.Render(r, ds);
            if (!MButt.LEFT.IsDown())
            {
                dragging = false;
            }
        }

        private void Init()
        {
            current.Clear();
            selection.Clear();
            foreach (Card c in cards)
            {
                if (c.Div().Info.Men() > 0)
                {
                    current.Add(c);
                    if (c.SelectedIs())
                        selection.Add(c.Div());
                }
                else
                {
                    c.SelectedSet(false);
                }
            }
        }

        public List<Div> Selection()
        {
            return selection;
        }

        private RENDEROBJ Row(GETTER<int> ier)
        {
            var ss = new GuiSection()
            {
                Render = (r, ds) =>
                {
                    int x1 = Body().X1();
                    int y1 = Body().Y1();
                    Clear();
                    for (int i = 0; i < xs; i++)
                    {
                        int k = ier.Get() * xs + i;
                        if (k >= current.Count)
                        {
                            break;
                        }
                        else
                        {
                            AddRightC(0, current[k]);
                        }
                    }
                    Body().MoveX1Y1(x1, y1);
                    Body().SetWidth(VIEW.UI().Div.SettCivic.Width() * scale * xs);
                    Body().SetHeight(VIEW.UI().Div.SettCivic.Height() * scale);
                    base.Render(r, ds);
                }
            };
            ss.Body().SetWidth(VIEW.UI().Div.SettCivic.Width() * scale * xs);
            ss.Body().SetHeight(VIEW.UI().Div.SettCivic.Height() * scale);
            return ss;
        }

        private class Card : ClickableAbs
        {
            private readonly int di;

            public Card(int di) : base(VIEW.UI().Div.SettCivic.Width() * scale, VIEW.UI().Div.SettCivic.Height() * scale)
            {
                this.di = di;
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                Div div = Div();
                VIEW.UI().Div.SettCivic.Render(r, Body.X1(), Body.Y1(), scale, div, isActive, isSelected, isHovered);

                if (dragging && isHovered && clicked != null && clicked != this && !KEYS.MAIN().UNDO.IsPressed() && !KEYS.MAIN().MOD.IsPressed())
                {
                    COLOR.GREEN100.Render(r, Body().X1() - 2, Body().X1() + 2, Body().Y1(), Body().Y2());
                    if (!MButt.LEFT.IsDown())
                    {
                        GAME.ARMIES().Player().SetDivAtOrderedIndex(div(), clicked.Div());
                        clicked.SelectedSet(false);
                        SelectedSet(true);
                        clicked = this;
                    }
                }
            }

            protected override void ClickA()
            {
                if (KEYS.MAIN().UNDO.IsPressed() && clicked != null)
                {
                    int ci = current.IndexOf(this);
                    int di = current.IndexOf(clicked);
                    int f = Math.Min(ci, di);
                    int t = Math.Max(ci, di);

                    for (int i = 0; i < current.Count; i++)
                    {
                        current[i].SelectedSet(i >= f && i <= t);
                    }
                }
                else if (KEYS.MAIN().MOD.IsPressed())
                {
                    SelectedSet(!SelectedIs());
                }
                else
                {
                    for (int i = 0; i < current.Count; i++)
                    {
                        current[i].SelectedSet(false);
                    }
                    SelectedSet(true);
                    clicked = this;
                    dragging = true;
                }
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                VIEW.UI().Div.SettCivic.Hover(text, Div());
            }

            private Div Div()
            {
                return GAME.ARMIES().Player().Ordered().Get(di);
            }
        }
    }
}