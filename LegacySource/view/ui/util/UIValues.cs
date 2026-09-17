using System;
using System.Collections.Generic;
using init.sprite.UI;
using init.value;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.main;

namespace view.ui.util
{
    public class UIValues<T> : GuiSection
    {
        public UIValues(GValueCat<T> vv, GETTER<T> g)
        {
            GInput filter = new GInput(new StringInputSprite(24, UI.FONT().S));
            add(filter);
            final LIST<Value<T>> all = vv.map().allSorted();
            List<RENDEROBJ> rows = new List<RENDEROBJ>(vv.all().size());
            foreach (Value<T> v in all)
            {
                GuiSection s = new GButt.BSection()
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.title(v.name);
                    }
                };
                s.add(v.icon, 0, 0);
                GText t = new GText(UI.FONT().S, v.key);
                t.setMaxChars(20);
                s.addRightC(2, t);
                s.addCentredY(new GStat()
                {
                    public override void update(GText text)
                    {
                        text.add(v.d.getD(g.get()));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.add(v.icon);
                        b.text(v.name);
                    }
                }, 400);
                s.body().setWidth(s.getLastX2() + 100);
                s.body().pad(4, 2);
                rows.Add(s);
            }

            GScrollRows s = new GScrollRows(rows, 800)
            {
                protected override bool passesFilter(int i, RENDEROBJ o)
                {
                    if (filter.text().Length == 0)
                        return true;
                    if (Str.containsText(all.get(i).key, filter.text()) || Str.containsText(all.get(i).name, filter.text()))
                        return true;
                    return false;
                }
            };

            addDown(4, s.view());
        }

        public static CLICKABLE butt(GValueCat<T> vv, GETTER<T> g)
        {
            UIValues<T> pop = new UIValues<T>(vv, g);
            return new GButt.ButtPanel(UI.icons().s.menu)
            {
                protected override void clickA()
                {
                    VIEW.inters().popup2.show(pop, this);
                }
            };
        }
    }
}