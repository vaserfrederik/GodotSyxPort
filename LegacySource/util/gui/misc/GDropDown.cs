using System;
using System.Collections.Generic;
using util.gui.misc;

namespace util.gui.misc
{
    public class GDropDown<E> : CLICKABLE.ClickableAbs where E : CLICKABLE
    {
        private readonly SPRITE title;
        private readonly int mX = C.SG * 4;
        private readonly int mY = C.SG * 1;
        private E selected;
        private GuiSection expansion = new GuiSection();
        private readonly Inter inter;
        private readonly ArrayListResize<E> es = new ArrayListResize<E>(20, 500);
        private readonly CLICKABLE.ClickableAbs dummy = new CLICKABLE.ClickableAbs()
        {
            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                // TODO Auto-generated method stub
            }
        };

        public GDropDown(SPRITE title)
        {
            this.title = title;
            body.setHeight(UI.FONT().S.height() + 2 * mY);
            this.inter = new Inter();
        }

        public GDropDown(CharSequence title) : this((SPRITE)new GText(UI.FONT().S, title).lablify()) { }

        public GDropDown(CharSequence title, int width) : this(sp(title, width)) { }

        private static SPRITE sp(CharSequence title, int width)
        {
            GText t = new GText(UI.FONT().S, title).lablify();
            return new SPRITE.Imp(width, t.height() + 4)
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    t.render(r, X1 + 2, Y1 + 2);
                }
            };
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            COLOR.WHITE05.render(r, body);

            if (!isActive)
                COLOR.WHITE15.render(r, body.x1() + 1, body.x2() - 1, body.y1() + 1, body.y2() - 1);
            else if (isHovered)
            {
                COLOR.WHITE30.render(r, body.x1() + 1, body.x2() - 1, body.y1() + 1, body.y2() - 1);
            }
            else
                COLOR.WHITE20.render(r, body.x1() + 1, body.x2() - 1, body.y1() + 1, body.y2() - 1);

            COLOR.WHITE05.render(r, body.x1() + title.width() + 2 * mX, body.x1() + title.width() + 2 * mX + 1, body.y1(), body.y2());

            if (!isActive)
                COLOR.WHITE50.bind();
            else if (isHovered)
            {
                SPRITES.icons().s.arrowDown.render(r, body.x2() - Icon.S - mX, body.y1() + (body.height() - Icon.S) / 2);
                COLOR.WHITE150.bind();
            }

            title.render(r, body.x1() + mX, body.y1() + (body.height() - title.height()) / 2);

            COLOR.unbind();

            if (selected != null)
            {
                int x1 = selected.body().x1();
                int y1 = selected.body().y1();
                selected.body().centerY(body);
                selected.body().moveX1(body.x1() + title.width() + 3 * mX);
                selected.render(r, ds);
                selected.body().moveX1Y1(x1, y1);
            }
        }

        public override bool click()
        {
            if (base.click())
            {
                if (!inter.isActivated())
                    inter.show();
                else
                    inter.hide();
                return true;
            }
            return false;
        }

        public E selected()
        {
            return selected;
        }

        public void setSelected(E s)
        {
            selected = s;
        }

        public GDropDown<E> add(E e)
        {
            es.add(e);
            if (selected == null)
                selected = e;
            return this;
        }

        public GDropDown<E> init()
        {
            expansion.clear();
            int w = 0;
            int h = 0;
            foreach (E e in es)
            {
                if (e.body().width() > w)
                    w = e.body().width();
                if (e.body().height() > h)
                    h = e.body().height();
            }
            body.setWidth(title.width() + 4 * mX + w);
            dummy.body.setWidth(w).setHeight(h);
            es.trim();

            GTableBuilder builder = new GTableBuilder()
            {
                public override int nrOFEntries()
                {
                    return es.size();
                }
            };

            final int width = w;
            final int height = h;

            builder.column(null, w + 2 * mX, new GRowBuilder()
            {
                public override RENDEROBJ build(GETTER<Integer> ier)
                {
                    CLICKABLE.ClickWrap wr = new CLICKABLE.ClickWrap(width, height)
                    {
                        protected override CLICKABLE pget()
                        {
                            if (ier.get() == null)
                                return dummy;
                            int i = ier.get();
                            if (i >= es.size())
                                return dummy;
                            return es.get(i);
                        }

                        public override bool click()
                        {
                            int i = ier.get();
                            setSelected(es.get(i));
                            if (base.click())
                            {
                                if (i < es.size())
                                {
                                    inter.hide();
                                }
                                return true;
                            }
                            return false;
                        }
                    };
                    return wr;
                }
            });

            int rows = Math.Min(10, es.size());
            expansion = builder.create(rows, true);
            GPanel p = new GPanel();
            p.inner().set(expansion);
            expansion.add(p);
            expansion.moveLastToBack();

            return this;
        }

        private final class Inter : Interrupter
        {
            protected Inter() : base() { }

            protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                return expansion.hover(mCoo);
            }

            protected override void mouseClick(MButt button)
            {
                if (button == MButt.LEFT)
                    expansion.click();
                else if (button == MButt.RIGHT)
                    hide();
            }

            private void show()
            {
                if (isActivated())
                    return;

                expansion.body().moveC(body().cX(), 0);
                expansion.body().moveY1(body().y2());
                if (expansion.body().y2() > C.HEIGHT())
                    expansion.body().moveY2(body().y2());
                base.show(VIEW.current().uiManager);
            }

            protected override bool otherClick(MButt button)
            {
                hide();
                return false;
            }

            public override void hide()
            {
                base.hide();
            }

            protected override void hoverTimer(GBox text)
            {
                expansion.hoverInfoGet(text);
            }

            protected override bool render(Renderer r, float ds)
            {
                expansion.render(r, ds);
                return true;
            }

            protected override bool update(float ds)
            {
                if (KEYS.anyDown())
                    hide();
                return true;
            }
        }
    }
}