using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data.INT;
using util.gui.misc;
using util.text;
using view.main;

namespace view.sett.ui.bottom
{
    public static class SearchToolPanel
    {
        public static LinkedList<Holder> All { get; private set; }

        static SearchToolPanel()
        {
            All = new LinkedList<Holder>();
        }

        public static CLICKABLE Add(CLICKABLE c, CharSequence name, CharSequence desc)
        {
            All.Add(new Holder(c, name + " " + desc));
            return c;
        }
    }

    public class SearchToolPanelInstance : SPanel
    {
        private readonly GInput input;
        private readonly Holder[] nonFiltered;
        private readonly List<Holder> filtered;
        private int page = 0;
        private GuiSection content = new GuiSection();

        private readonly int width = 2;
        private readonly int height = 10;

        public SearchToolPanelInstance()
        {
            nonFiltered = new Holder[SearchToolPanel.All.Count];
            int i = 0;
            foreach (Holder rr in SearchToolPanel.All)
                nonFiltered[i++] = rr;
            filtered = new List<Holder>(nonFiltered.Length);
            Array.Sort(nonFiltered, new Comparator<Holder>());

            input = new GInput(new StringInputSprite(20, UI.FONT().M)
            {
                protected override void Change()
                {
                    Filter(Text());
                    base.Change();
                }
            });
            Add(new GHeader(Dic.¤¤Filter));
            AddRightC(16, input);

            content.Body().SetDim((BButt.WIDTH + 8) * width, (BButt.HEIGHT + 2) * height);

            AddRelBody(16, DIR.S, content);

            GTarget t = new GTarget(64, false, true, new INTE()
            {
                public int Min => 0,
                public int Max => filtered.Count / (width * height),
                public int Get => page,
                public void Set(int t)
                {
                    page = t;
                    Build();
                }
            });
            AddRelBody(8, DIR.S, t);

            Filter("");

            Pad(8, 8);
        }

        public void Open(CLICKABLE c, Inter inter)
        {
            inter.Set(c, this);
            input.Focus();
        }

        private void Filter(CharSequence filter)
        {
            string f = ("" + filter).ToUpper();
            filtered.Clear();
            foreach (Holder h in nonFiltered)
            {
                if (h.name.Contains(f))
                    filtered.Add(h);
            }
            page = 0;
            Build();
        }

        private void Build()
        {
            int x1 = content.Body().X1();
            int y1 = content.Body().Y1();
            content.Clear();
            int s = page * (width * height);
            for (int i = 0; s < filtered.Count && i < width * height; i++)
            {
                Holder h = filtered[s];
                s++;

                content.Add(h, (i % width) * (BButt.WIDTH + 8), (i / width) * (BButt.HEIGHT + 2));
            }
            content.Body().MoveX1Y1(x1, y1);
        }

        private class Holder : ClickableAbs
        {
            private readonly CLICKABLE other;
            private readonly string name;

            public Holder(CLICKABLE other, CharSequence name)
            {
                Body.Set(other);
                this.other = other;
                this.name = ("" + name).ToUpper();
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                int x1 = other.Body().X1();
                int y1 = other.Body().Y1();
                other.Body().MoveX1Y1(Body().X1(), Body().Y1());
                other.Render(r, ds);
                other.Body().MoveX1Y1(x1, y1);
            }

            public override bool Hover(COORDINATE mCoo)
            {
                int x1 = other.Body().X1();
                int y1 = other.Body().Y1();
                other.Body().MoveX1Y1(Body().X1(), Body().Y1());
                other.Hover(mCoo);
                other.Body().MoveX1Y1(x1, y1);
                return base.Hover(mCoo);
            }

            public override bool Click()
            {
                if (base.Click())
                {
                    VIEW.Inters().Popup.Close();
                    other.Click();
                    return true;
                }
                return false;
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                other.HoverInfoGet(text);
            }
        }
    }
}