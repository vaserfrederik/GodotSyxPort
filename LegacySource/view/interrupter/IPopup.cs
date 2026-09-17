using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.panel;
using view.main;

namespace view.interrupter
{
    public sealed class IPopup
    {
        private readonly GuiSection s = new GuiSection();
        private readonly Inter inter = new Inter(s);
        private readonly InterManager m;
        private CLICKABLE trigger;

        public IPopup(InterManager manager)
        {
            this.m = manager;
        }

        public void Show(RENDEROBJ s, CLICKABLE trigger)
        {
            Show(s, trigger, false);
        }

        public void Show(RENDEROBJ s, CLICKABLE trigger, bool centreAtMouse)
        {
            old = null;
            this.s.Clear();
            this.s.Add(s);
            this.trigger = trigger;
            if (trigger != null)
                ShowP(trigger.Body().CX(), trigger.Body().CY(), centreAtMouse);
            else
            {
                int x1 = C.WIDTH() / 2 - s.Body().Width() / 2;
                int y1 = C.HEIGHT() / 2 - s.Body().Height() / 2;
                ShowP(x1, y1, centreAtMouse);
            }
        }

        RENDEROBJ old;
        CLICKABLE oldC;

        public void Push(RENDEROBJ s, CLICKABLE trigger)
        {
            RENDEROBJ old = null;
            oldC = this.trigger;
            if (inter.IsActivated())
            {
                old = this.s.Elements().Get(0);
            }
            Show(s, trigger);
            this.old = old;
        }

        public void Pop()
        {
            if (inter.IsActivated() && old != null)
            {
                Show(old, oldC);
            }
            else
                Close();
        }

        public GuiSection Section()
        {
            return s;
        }

        public void Close()
        {
            inter.Hide();
        }

        public bool Showing()
        {
            return inter.IsActivated();
        }

        public RENDEROBJ Current()
        {
            return inter.IsActivated() ? s.Elements().Get(0) : null;
        }

        protected void ShowP(int x, int y, bool centre)
        {
            int M = C.SG * 32;

            if (centre)
            {
                s.Body().MoveC(VIEW.Mouse());
                if (!inter.IsActivated())
                {
                    m.Add(inter);
                }
            }
            else
            {
                s.Body().MoveCX(x);
                if (y > C.HEIGHT() / 2)
                {
                    s.Body().MoveY2(y - M);
                }
                else
                {
                    s.Body().MoveY1(y + M);
                }
            }

            if (s.Body().X2() + M >= C.WIDTH())
            {
                s.Body().MoveX2(C.WIDTH() - M);
            }

            if (s.Body().X1() - M < 0)
            {
                s.Body().MoveX1(x + M);
            }

            if (s.Body().Y2() + M >= C.HEIGHT())
            {
                s.Body().MoveY2(C.HEIGHT() - M);
            }

            if (s.Body().Y1() - M < 0)
            {
                s.Body().MoveY1(M);
            }

            inter.hidden = true;

            if (!inter.IsActivated())
            {
                m.Add(inter);
            }
        }

        private class Inter : Interrupter
        {
            private bool hidden = true;
            private readonly GPanel box;
            ACTION exit = new ACTION
            {
                Exe = () =>
                {
                    if (hidden)
                        return;
                    Hide();
                }
            };

            public Inter(GuiSection s)
            {
                box = new GPanel();
                box.SetButt();
            }

            protected override void HoverTimer(GBox text)
            {
                s.HoverInfoGet(text);
            }

            protected override void MouseClick(MButt button)
            {
                if (button == MButt.RIGHT)
                {
                    Hide();
                }
                else if (button == MButt.LEFT)
                {
                    if (!s.Click())
                        box.Click();
                }
            }

            public override void Hide()
            {
                if (old != null)
                {
                    IPopup.this.Show(old, oldC);
                }
                else
                    base.Hide();
            }

            protected override bool OtherClick(MButt butt)
            {
                Hide();
                if (butt == MButt.RIGHT)
                    return true;
                return false;
            }

            protected override void OtherAdd(Interrupter other)
            {
                Hide();
            }

            protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                return s.Hover(mCoo) || box.Hover(mCoo);
            }

            protected override bool Render(Renderer r, float ds)
            {
                hidden = false;
                box.Inner().Set(s);
                box.ClickActionSet(exit);
                box.Render(r, ds);
                s.Render(r, ds);
                if (trigger != null)
                {
                    trigger.SelectTmp();
                }
                return true;
            }

            protected override bool Update(float ds)
            {
                return true;
            }
        }
    }
}