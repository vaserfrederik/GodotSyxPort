using System;
using System.Collections.Generic;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.main;

namespace view.ui.wiki
{
    class WikiList : GuiSection
    {
        private readonly StringInputSprite filter = new StringInputSprite(18, UI.FONT().M)
        {
            protected override void change()
            {
                filter();
            }
        }.placeHolder("Search");

        private readonly GInput fc = new GInput(filter);
        private readonly ArrayList<Article> all;
        private readonly ArrayList<Article> filtered;
        public static readonly int width = 400;
        private readonly GTableBuilder builder;

        public WikiList(ArrayList<Article> all, int HEIGHT)
        {
            int cats = 0;
            CharSequence lastCat = null;
            foreach (Article e in all)
            {
                if (lastCat == null || !e.category.Equals(lastCat))
                {
                    lastCat = e.category;
                    cats++;
                }
            }
            this.all = new ArrayList<Article>(all.size + cats);
            lastCat = null;
            foreach (Article e in all)
            {
                if (lastCat == null || !e.category.Equals(lastCat))
                {
                    this.all.Add(null);
                    lastCat = e.category;
                }
                this.all.Add(e);
            }
            filtered = new ArrayList<Article>(this.all);

            fc.body().centerX(0, width - 8);
            add(fc, 4, 0);

            builder = new GTableBuilder
            {
                nrOFEntries = () => filtered.size,
                click = (index) =>
                {
                    Article e = filtered.get(index);
                    if (e != null)
                        VIEW.UI().wiki.set(e);
                },
                selectedIs = (index) =>
                {
                    Article a = filtered.get(index);
                    if (a == null)
                        return false;
                    return (VIEW.UI().wiki.added().size() > 0 && VIEW.UI().wiki.added().get(0) == a);
                }
            };

            builder.column(null, width - C.SG * 24 - 8, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new ClickableAbs(width - C.SG * 24 - 8, 38)
                    {
                        tt = new GText(UI.FONT().M, 24)
                        {
                            maxwidth = body().width() - 24
                        },

                        render = (SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered) =>
                        {
                            Article e = filtered.get(ier.get());
                            if (e == null)
                            {
                                tt.setFont(UI.FONT().H2);
                                tt.lablify();
                                tt.set(filtered.get(ier.get() + 1).category);
                                tt.renderCY(r, body().x1() + 8, body().cY());
                            }
                            else
                            {
                                isSelected |= VIEW.UI().wiki.added().contains(e);
                                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                                GButt.ButtPanel.renderFrame(r, body);
                                tt.setFont(UI.FONT().M);
                                tt.normalify2();
                                tt.set(e.title);
                                tt.renderCY(r, body().x1() + 16, body().cY());
                            }
                            COLOR.unbind();
                        },

                        clickA = () =>
                        {
                            Article e = filtered.get(ier.get());
                            if (e != null)
                                VIEW.UI().wiki.set(e);
                        }
                    };
                }
            });

            RENDEROBJ o = builder.createHeight(HEIGHT - body().height() - 10, false);

            o.body().moveY1(fc.body().y2() + 5);
            add(o);
            pad(4, 3);
        }

        public void setList(Article a)
        {
            filter.text().clear();
            filter();

            for (int i = 0; i < all.size(); i++)
            {
                if (all.get(i) == a)
                {
                    builder.set(i - 5);
                    return;
                }
            }
        }

        public void set(Article e)
        {
            filter.text().clear();
            filter();
            if (e == null)
                fc.focus();
        }

        private void filter()
        {
            filtered.clear();
            CharSequence f = filter.text();
            if (f.length() == 0)
            {
                filtered.add(all);
                return;
            }

            for (int i = 0; i < all.size(); i++)
            {
                Article e = all.get(i);
                if (e == null)
                    continue;
                CharSequence t = e.key;
                if (testFilter(f, t))
                {
                    if (filtered.isEmpty() || !filtered.get(filtered.size() - 1).category.Equals(e.category))
                        filtered.add(null);
                    filtered.add(all.get(i));
                }
            }
        }

        private bool testFilter(CharSequence filter, CharSequence key)
        {
            outer:
            for (int ti = 0; ti < key.length(); ti++)
            {
                if (char.ToLower(filter.charAt(0)) == char.ToLower(key.charAt(ti)))
                {
                    for (int i = 1; i < filter.length(); i++)
                    {
                        int k = i + ti;
                        if (k >= key.length())
                            return false;
                        if (char.ToLower(filter.charAt(i)) != char.ToLower(key.charAt(k)))
                            continue outer;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}