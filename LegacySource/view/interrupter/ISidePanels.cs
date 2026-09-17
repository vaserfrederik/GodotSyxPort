using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.gui.misc;
using util.text;
using view.ui.top;

namespace view.interrupter
{
    public sealed class ISidePanels : Interrupter
    {
        private readonly List<Panel> free = new List<Panel>(16);
        private readonly List<Panel> added = new List<Panel>(16);
        private readonly GuiSection section = new GuiSection();
        private int x2;
        private readonly int x1;
        private readonly InterManager m;

        public ISidePanels(InterManager m, int x1)
        {
            this.m = m;
            for (int i = 0; i < 16; i++)
                free.Add(new Panel());
            this.x1 = x1 - 1;
        }

        public void Add(ISidePanel panel, bool clear)
        {
            Add(panel, clear, false);
        }

        public void AddDontRemove(ISidePanel panel, ISidePanel panel2)
        {
            if (Added(panel))
            {
                Add(panel, true);
                Add(panel2, false);
            }
            else
            {
                Add(panel2, true);
            }
        }

        public void AddDontRemove(ISidePanel panel, ISidePanel panel2, ISidePanel panel3)
        {
            bool p1 = Added(panel);
            bool p2 = Added(panel2);
            Clear();
            if (p1)
                Add(panel, false);
            if (p2)
                Add(panel2, false);
            Add(panel3, false);
        }

        public void Toggle(ISidePanel panel, bool clear)
        {
            if (Added(panel))
                Remove(panel);
            else
                Add(panel, clear, false);
        }

        public void Add(ISidePanel panel, bool clear, bool pin)
        {
            if (clear)
            {
                Remove();
            }
            for (int i = 0; i < added.Count; i++)
            {
                Panel p = added[i];
                if (p.panel == panel)
                {
                    p.Set(panel);
                    Rearrange();
                    Show(m);
                    return;
                }
            }

            AddP(panel, pin);
            Show(m);
        }

        public void Remove(ISidePanel panel)
        {
            for (int i = 0; i < added.Count; i++)
            {
                Panel p = added[i];
                if (p.panel == panel)
                {
                    added.RemoveOrdered(i);
                    free.Add(p);
                    Rearrange();
                    return;
                }
            }
        }

        protected override bool OtherClick(MButt button)
        {
            if (button == MButt.RIGHT && added.Count > 0)
            {
                for (int i = added.Count - 1; i >= 0; i--)
                {
                    Panel p = added[i];
                    if (p.panel.Back())
                        return false;
                    if (!p.pinned)
                    {
                        added.RemoveOrdered(i);
                        free.Add(p);
                        Rearrange();
                        return true;
                    }
                }
            }
            return false;
        }

        public void Clear()
        {
            foreach (Panel p in added)
                free.Add(p);
            added.Clear();
            Rearrange();
        }

        public bool Added(ISidePanel panel)
        {
            if (!base.IsActivated())
                return false;
            for (int i = 0; i < added.Count; i++)
            {
                Panel p = added[i];
                if (p.panel == panel)
                    return true;
            }
            return false;
        }

        private void AddP(ISidePanel panel, bool pinned)
        {
            Panel p = free.RemoveLast();
            p.Set(panel);
            added.Add(p);
            Rearrange();
            p.pinned = pinned;
            panel.AddAction();
            panel.Update(0);
        }

        private void Remove()
        {
            for (int i = 0; i < added.Count; i++)
            {
                Panel p = added[i];
                if (!p.pinned)
                {
                    free.Add(p);
                    added.RemoveOrdered(i);
                    i--;
                }
            }
        }

        private void Rearrange()
        {
            section.Clear();
            x2 = x1;
            foreach (Panel p in added)
            {
                p.panel.last = this;
                p.Body().MoveX1Y1(section.GetLastX2(), UIPanelTop.HEIGHT);
                section.Add(p);
            }
            section.Body().MoveX1(x1);
            section.Body().MoveY1(UIPanelTop.HEIGHT);
            x2 = section.Body().x2();
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return section.Hover(mCoo);
        }

        protected override void MouseClick(MButt button)
        {
            if (MButt.LEFT == button)
                section.Click();
            if (MButt.RIGHT == button)
            {
                OtherClick(button);
            }
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            if (x2 > 0)
            {
                addManager.ViewPort().MoveX1(x2);
                addManager.ViewPort().SetWidth(C.WIDTH() - x2);
            }
            section.Render(r, ds);
            return true;
        }

        protected override bool Update(float ds)
        {
            foreach (Panel p in added)
                p.panel.Update(ds);
            return true;
        }

        private class Panel : GuiSection
        {
            public bool pinned;
            private readonly GText title = new GText(UI.FONT().H2, 20).Lablify();
            private ISidePanel panel;
            private readonly CLICKABLE close = new GButt.ButtPanel(SPRITES.icons().m.exit)
            {
                protected override void ClickA()
                {
                    remove(panel);
                }

                public override void HoverInfoGet(GUI_BOX text)
                {
                    text.Title(Dic.¤¤Close);
                    text.Add(text.text().Add('(').Add(Dic.¤¤RightClick).Add(')'));
                }
            };

            public void Set(ISidePanel panel)
            {
                Clear();
                CLICKABLE s = panel.Section();
                Body().SetHeight(C.HEIGHT() - UIPanelTop.HEIGHT);
                Body().SetWidth(s.Body().width() + ISidePanel.M * 2);
                Body().MoveY1(ISidePanel.Y1);
                s.Body().CenterIn(this);
                s.Body().MoveY1(ISidePanel.Y2 + ISidePanel.M);
                Add(s);
                close.Body().MoveC(Body().x2() - (close.Body().width() / 2 + 8), ISidePanel.Y1 + (ISidePanel.Y2 - ISidePanel.Y1) / 2);
                Add(close);
                this.panel = panel;
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                if (panel.title != null)
                    this.title.Clear().Add(panel.title).AdjustWidth();

                COLOR.WHITE10.Render(r, Body().x1(), Body().x2(), ISidePanel.Y1, C.HEIGHT());
                UI.PANEL().butt.Render(r, Body().x1(), Body().x2() - 3, ISidePanel.Y1 + UI.PANEL().butt.margin, ISidePanel.Y2 - UI.PANEL().butt.margin, 0, DIR.N.mask() | DIR.S.mask());

                GCOLOR.UI().Border(r, Body().x1(), Body().x1() + 3, ISidePanel.Y1, C.HEIGHT());
                GCOLOR.UI().Border(r, Body().x2() - 3, Body().x2(), ISidePanel.Y1, C.HEIGHT());

                if (title.Length() != 0)
                {
                    title.AdjustWidth();
                    int x = Body().x1() + (close.Body().x1() - Body().x1()) / 2;
                    int y = close.Body().CY();
                    title.RenderC(r, x, y);
                }

                base.Render(r, ds);
            }
        }
    }
}