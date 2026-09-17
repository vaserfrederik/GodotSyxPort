using System;
using System.Collections.Generic;
using snake2d;
using settlement.main;
using settlement.room.main;
using settlement.room.main.category;
using util.gui.misc;
using util.gui.table;
using view.interrupter;

namespace view.sett.ui.room
{
    final class UIPanelUtil : ISidePanel
    {
        static abstract class BlueprintList : GuiSection
        {
            private RoomCategoryMain catCurrent = SETT.ROOMS().CATS.MAINS[0];
            private GuiSection list = new GuiSection();

            BlueprintList(int height)
            {
                CatButt first = null;

                foreach (RoomCategoryMain m in SETT.ROOMS().CATS.MAINS)
                {
                    CatButt b = new CatButt(m);
                    b.add(m.icon, 0, 0);
                    addToCat(b, m);
                    b.pad(8, 8);

                    RENDEROBJ list = makeList(m, height - b.body().height() - 8);

                    if (list != null)
                    {
                        b.list = list;
                        if (first == null)
                        {
                            first = b;
                        }
                        addRightC(0, b);
                    }
                }

                if (first == null)
                    return;

                list.add(first.list);
                addRelBody(4, DIR.S, list);
            }

            private void set(RoomCategoryMain m, RENDEROBJ list)
            {
                int x1 = this.list.body().x1();
                int y1 = this.list.body().y1();
                this.list.clear();
                this.list.add(list);
                this.list.body().moveX1Y1(x1, y1);
                catCurrent = m;
            }

            private RENDEROBJ makeList(RoomCategoryMain cat, int height)
            {
                List<RENDEROBJ> rows = new List<RENDEROBJ>();
                List<RoomBlueprintIns<?>> rooms = new List<RoomBlueprintIns<?>>();
                foreach (RoomCategorySub s in cat.subs)
                {
                    foreach (RoomBlueprintImp p in s.rooms())
                    {
                        if (p is RoomBlueprintIns<?> && p.isAvailable(SETT.ENV().climate()))
                        {
                            rooms.Add((RoomBlueprintIns<?>)p);
                            RENDEROBJ r = row((RoomBlueprintIns<?>)p);
                            if (r != null)
                            {
                                rows.Add(r);
                            }
                        }
                    }
                }
                foreach (RoomBlueprintImp p in cat.misc.rooms())
                {
                    if (p is RoomBlueprintIns<?> && p.isAvailable(SETT.ENV().climate()))
                    {
                        rooms.Add((RoomBlueprintIns<?>)p);
                        RENDEROBJ r = row((RoomBlueprintIns<?>)p);
                        if (r != null)
                        {
                            rows.Add(r);
                        }
                    }
                }
                if (rows.Count == 0)
                    return null;
                GScrollRows s = new GScrollRows(rows, height)
                {
                    protected override bool passesFilter(int i, RENDEROBJ o)
                    {
                        return passes(rooms[i]);
                    }
                };
                return s.view();
            }

            abstract RENDEROBJ row(RoomBlueprintIns<?> b);
            protected void addToCat(GuiSection s, RoomCategoryMain cat) { }

            void hoverCat(GBox b, RoomCategoryMain cat)
            {
                b.title(cat.name);
            }

            private class CatButt : GButt.BSection
            {
                RENDEROBJ list;
                private readonly RoomCategoryMain m;

                CatButt(RoomCategoryMain m)
                {
                    this.m = m;
                }

                protected override void clickA()
                {
                    if (hovered() == null || !(hovered() is CLICKABLE))
                    {
                        catCurrent = m;
                        set(m, list);
                    }
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    hoverCat((GBox)text, m);
                }

                protected override void renAction()
                {
                    selectedSet(catCurrent == m);
                }
            }

            public bool passes(RoomBlueprintIns<?> blue)
            {
                return true;
            }
        }

        static class RoomRow : GButt.BSection
        {
            protected readonly RoomBlueprintImp p;

            RoomRow(RoomBlueprintImp p)
            {
                this.p = p;
                addRightC(4, p.iconBig());
                addRightC(8, new GHeader(p.info.names, 13).subify());
                body().setWidth(280);
                pad(2, 4);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(p.info.names);
            }
        }
    }
}