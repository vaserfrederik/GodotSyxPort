using System;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.keyboard;
using view.main;
using view.sett.ui.room.construction;

namespace view.sett.ui.bottom
{
    public sealed class UIBuildPanel : Interrupter
    {
        private readonly GuiSection section = new GuiSection();
        private SearchToolPanel searchPanel;
        private BuildMain main;
        private readonly UIRoomPlacer placer;

        public UIBuildPanel(UIRoomPlacer placer, InterManager m)
        {
            pin();
            m.add(this);
            this.placer = placer;
            SearchToolPanel.all = new LinkedList<SearchToolPanel>();
            SETT.AddGeneratorHook(() =>
            {
                SearchToolPanel.all = new LinkedList<SearchToolPanel>();
                Create();
            });
            Create();
        }

        private void Create()
        {
            section.clear();
            section.add(SPRITES.specials().lowerPanel(), 0, 0);

            section.body().centerX(0, C.WIDTH());
            section.body().moveY2(C.HEIGHT());

            GuiSection s = new GuiSection();
            D.gInit(this);

            Inter inter = new Inter();

            {
                CLICKABLE c = new GButt.ButtPanel(new SPRITE.Wrap(SPRITES.icons().m.search, 32, 32));
                ACTION sa = () =>
                {
                    searchPanel.open(c, inter);
                };
                c.clickActionSet(sa);
                CLICKABLE cc = KeyButt.wrap(sa, c, KEYS.SETT(), "toolSearch", D.g("Search"), D.g("SearchD", "Search for tools and rooms"), KEYCODES.KEY_LEFT_CONTROL, KEYCODES.KEY_F);
                s.addRightC(8, cc);
            }
            main = new BuildMain(inter, placer);
            s.addRightC(8, main.create());

            {
                Options sec = new Options();

                CLICKABLE c = new GButt.ButtPanel(new SPRITE.Wrap(SPRITES.icons().m.cog_big, 32, 32))
                {
                    protected override void clickA()
                    {
                        inter.set(this, sec);
                    }

                    protected override void renAction()
                    {
                        selectedSet(SETT.JOBS().planMode.is());
                    }
                };
                c.hoverTitleSet(Dic.¤¤Tools);
                s.addRightC(0, c);
            }

            {
                Delete delete = new Delete();
                CLICKABLE c = new GButt.ButtPanel(new SPRITE.Wrap(SPRITES.icons().m.cancel, 32, 32))
                {
                    protected override void clickA()
                    {
                        inter.set(this, delete);
                    }
                };
                c.hoverTitleSet(Dic.¤¤delete);
                s.addRightC(0, c);
            }

            s.body().centerIn(section);
            s.body().incrY(4);
            section.add(s);

            searchPanel = new SearchToolPanel();
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return section.hover(mCoo);
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                section.click();
        }

        protected override void hoverTimer(GBox text)
        {
            section.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            section.render(r, ds);

            return true;
        }

        public void hilight(string key)
        {
            main.hilight(key);
        }

        protected override bool update(float ds)
        {
            return true;
        }

        protected sealed class Butt : GButt.ButtPanel
        {
            private readonly PLACABLE p;

            public Butt(PLACABLE p) : base(p.getIcon())
            {
                this.p = p;
            }

            public Butt(PLACABLE p, Icon icon) : base(p.getIcon())
            {
                this.p = p;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                p.hoverDesc((GBox)text);
            }

            protected override void clickA()
            {
                VIEW.s().tools.place(p);
            }

            protected override void renAction()
            {
                selectedSet(p == VIEW.s().tools.placer.getCurrent());
            }
        }
    }
}