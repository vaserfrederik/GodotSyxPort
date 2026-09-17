using System;
using System.Collections.Generic;
using Util.Colors;
using Util.Gui.Clickable;
using Util.Gui.Clickable.Scrollable;
using Util.Gui.Misc;
using Util.Gui.Table;
using Util.Text;
using View.Keyboard;

namespace View.Menu
{
    class ScKeys : GuiSection
    {
        private readonly string ¤¤nameBig = "¤KEY SETTINGS";
        private readonly string ¤¤name = "¤key settings";
        private int page = 0;
        private Key hoveredKey;

        public ScKeys(IMenu m, Font font, Font small)
        {
            D.t(this);

            CLICKABLE restore = new GButt.Glow(font.GetText(D.g("restore")))
            {
                protected override void ClickA()
                {
                    KEYS.Get().Restore();
                    KEYS.Get().Save();
                }
            };
            Add(restore);
            CLICKABLE cancel = new GButt.Glow(font.GetText(D.g("cancel")))
            {
                protected override void ClickA()
                {
                    m.SetMain();
                }
            };
            AddRightC(100, cancel);

            GuiSection keys = new GuiSection();

            ScrollRow[] butts = new ScrollRow[]
            {
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
                new Row(),
            };

            Scrollable s = new GScrollable(butts)
            {
                public override int NrOFEntries()
                {
                    return (int)Math.Ceiling(KEYS.Pages().Get(page).All().Count / 2.0);
                }
            };
            keys.Add(s.GetView());

            {
                GuiSection ss = new GuiSection();
                ss.Add(new GButt.Glow(SPRITES.Icons().M.arrow_left)
                {
                    protected override void ClickA()
                    {
                        page--;
                        if (page < 0)
                            page += KEYS.Pages().Size();
                    }
                });
                ss.AddRightC(100, new GStat()
                {
                    public override void Update(GText text)
                    {
                        text.SetFont(UI.Font().H2);
                        text.Add(KEYS.Pages().Get(page).Name());
                    }
                }.R(DIR.N));
                ss.AddRightC(100, new GButt.Glow(SPRITES.Icons().M.arrow_right)
                {
                    protected override void ClickA()
                    {
                        page++;
                        if (page >= KEYS.Pages().Size())
                            page -= KEYS.Pages().Size();
                    }
                });

                keys.AddRelBody(8, DIR.N, ss);
            }

            keys.AddRelBody(8, DIR.S, new GStat()
            {
                public override void Update(GText text)
                {
                    if (hoveredKey != null)
                    {
                        text.Clear().Add(hoveredKey.Desc);
                    }
                }
            }.R(DIR.N));

            keys.Body().IncrH(24);

            keys.Add(UI.Decor().Frame(keys.Body()));

            AddRelBody(32, DIR.N, keys);
            AddRelBody(0, DIR.N, UI.Decor().Decorate(¤¤name));

            Body().CenterIn(C.DIM());
        }

        public GuiSection Activate()
        {
            hoveredKey = null;
            page = 0;
            //GSettings.Get().ReadFromFile();
            return this;
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            base.Render(r, ds);
            hoveredKey = null;
        }

        private class Row : GuiSection, ScrollRow
        {
            private readonly KeyCode a;
            private readonly KeyCode b;

            public Row()
            {
                a = new KeyCode();
                b = new KeyCode();
                Add(a);
                AddRightC(20, b);
            }

            public void Init(int index)
            {
                a.Init(index);
                b.Init((int)(Math.Ceiling(KEYS.Pages().Get(page).All().Count / 2.0) + index));
            }
        }

        private class KeyCode : CLICKABLE.ClickableAbs
        {
            private Key key;

            protected KeyCode()
            {
                body.SetWidth(550);
                body.SetHeight(UI.Font().M.Height());
                VisableSet(false);
            }

            protected override void ClickA()
            {
                if (key.Rebindable)
                {
                    KEYS.Bind(key);
                }
                base.ClickA();
            }

            void Init(int i)
            {
                if (i < KEYS.Pages().Get(page).All().Count)
                {
                    VisableSet(true);
                    key = KEYS.Pages().Get(page).All()[i];
                }
                else
                {
                    VisableSet(false);
                }
            }

            public override bool Hover(COORDINATE mCoo)
            {
                if (base.Hover(mCoo))
                {
                    hoveredKey = key;
                    return true;
                }
                return false;
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive,
                bool isSelected, bool isHovered)
            {
                Str.TMP.Clear();
                Str.TMP.Add(key.Name);

                isActive &= key.Rebindable;

                if (!isActive)
                    GCOLOR.T().INACTIVE.Bind();
                else if (isHovered && isSelected)
                    GCOLOR.T().HOVER_SELECTED.Bind();
                else if (isHovered)
                    GCOLOR.T().HOVERED.Bind();
                else if (isSelected)
                    GCOLOR.T().SELECTED.Bind();
                else
                    GCOLOR.T().CLICKABLE.Bind();
                UI.Font().M.Render(r, Str.TMP, body.X1(), body.Y1());
                UI.Font().M.Render(r, key.Repr(), body.X1() + 200, body.Y1());
                COLOR.Unbind();
            }
        }
    }
}