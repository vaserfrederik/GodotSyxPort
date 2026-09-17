using System;
using System.Collections.Generic;
using game;
using game.nobility;
using init.constant;
using snake2d.util.gui;
using util.gui.misc;
using util.gui.table;
using view.main;

namespace view.sett.ui.noble
{
    class NobleAssigns : GuiSection
    {
        Noble n;

        public NobleAssigns()
        {
            GRows rows = new GRows(8);

            foreach (NobleOffice o in GAME.NOBLE().OFFICES)
            {
                if (!o.special)
                    rows.add(new BB(o));
            }
            rows.nl();
            foreach (NobleOffice o in GAME.NOBLE().OFFICES)
            {
                if (o.special)
                    rows.add(new BB(o));
            }

            GScrollRows rr = new GScrollRows(rows.rows(), C.HEIGHT() - 300);

            add(rr.view());
        }

        private class BB : GButt.ButtPanel
        {
            private readonly NobleOffice o;

            public BB(NobleOffice o) : base(o.icon.huge)
            {
                this.o = o;
                pad(4, 4);
            }

            protected override void renAction()
            {
                selectedSet(n.office() == o);
                //activeSet(GAME.NOBLE().allocations(o) == 0 || n.office() == o);
            }

            protected override void clickA()
            {
                GAME.NOBLE().setOffice(n, o);
                VIEW.inters().popup.close();
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                o.hover(text);
            }
        }
    }
}