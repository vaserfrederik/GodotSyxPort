using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.gui.panel;
using view.keyboard;

namespace view.interrupter
{
    public class IDebugPanelAbs : Interrupter
    {
        private readonly GuiSection section = new GuiSection();
        private readonly Type current;

        private readonly GPanel panel;
        private readonly StringInputSprite filter = new StringInputSprite(20, UI.FONT().M)
        {
            protected override void Change()
            {
                current.Init(0);
            }
        }.PlaceHolder("Search");
        private readonly InputClickable fc = filter.C(DIR.W).Colors(COLOR.WHITE65, COLOR.WHITE2WHITE);
        private readonly InterManager manager;

        public void Show()
        {
            base.Show(manager);
            fc.Focus();
        }

        public IDebugPanelAbs(InterManager manager, SortedDictionary<string, CLICKABLE> hash)
        {
            DesturberSet();
            this.manager = manager;
            AddMisc();
            panel = new GPanel().SetDim(1000, 700);
            panel.SetBig();
            panel.SetCloseAction(new ACTION
            {
                public void Exe()
                {
                    Hide();
                }
            });
            panel.SetTitle("Debugger Panel", UI.FONT().H2);
            section.Add(panel);

            current = new Type(hash);

            section.Body().CenterIn(C.DIM());

            RECTANGLE bounds = panel.Inner();
            fc.Body().MoveX1Y1(bounds.x1(), bounds.y1() + 20);
            section.Add(fc);

            hash.Clear();
            current.Init(0);
        }

        protected void AddMisc()
        {

        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            section.Hover(mCoo);
            current.Hover(mCoo);
            return true;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                section.Click();
                current.Click();
            }
            else if (button == MButt.RIGHT)
            {
                Hide();
            }
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            section.Render(r, ds);
            current.Render(r, ds);
            return true;
        }

        protected override bool Update(float ds)
        {
            if (KEYS.MAIN().ESCAPE.ConsumeClick())
                Hide();

            return false;
        }

        private class Type : GuiSection
        {
            private readonly SortedDictionary<string, CLICKABLE> items;
            private int itemCount = 0;
            private int itemLast = 0;
            private CLICKABLE next;
            private CLICKABLE prev;
            private readonly int maxrows = (int)(25.0 * C.HEIGHT() / 1000);

            Type(SortedDictionary<string, CLICKABLE> items)
            {
                this.items = new SortedDictionary<string, CLICKABLE>(items);
                next = new GButt.ButtPanel(SPRITES.icons().m.arrow_right)
                {
                    protected override void ClickA()
                    {
                        prev.ActiveSet(true);
                        Init(itemLast);
                    }
                };
                prev = new GButt.ButtPanel(SPRITES.icons().m.arrow_left)
                {
                    protected override void ClickA()
                    {
                        next.ActiveSet(true);
                        Init(itemLast - itemCount - maxrows * 2);
                    }
                };
            }

            void Init(int first)
            {
                this.itemCount = 0;
                this.itemLast = first;
                base.Clear();
                RECTANGLE bounds = panel.Inner();
                Body().MoveX1Y1(bounds.x1(), fc.Body().Y2() + 10);
                int x1 = Body().X1();
                int y1 = Body().Y1();
                prev.ActiveSet(first != 0);
                next.ActiveSet(false);
                int i = 0;
                int rows = 0;
                int cols = 0;

                foreach (var c in items)
                {
                    if (filter.Text().Length == 0 || Str.ContainsText(c.Key, filter.Text()))
                    {
                        i++;
                        if (i < first)
                            continue;
                        rows++;
                        if (rows > maxrows)
                        {
                            cols++;
                            if (cols == 2)
                            {
                                next.ActiveSet(true);
                                break;
                            }

                            rows = 1;
                            x1 += bounds.width / 2;
                            y1 = Body().Y1();
                        }
                        itemLast++;
                        itemCount++;
                        CLICKABLE cl = c.Value;
                        cl.Body().MoveX1Y1(x1, y1);
                        Add(cl);
                        y1 += cl.Body().Height();
                    }
                }
                prev.Body().MoveX1(bounds.x1());
                prev.Body().MoveY2(bounds.y2());
                next.Body().MoveX2(bounds.x2());
                next.Body().MoveY2(bounds.y2());
                Add(prev);
                Add(next);
            }
        }
    }
}