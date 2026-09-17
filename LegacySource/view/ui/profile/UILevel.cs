using System;
using game;
using game.boosting;
using game.faction;
using init.sprite.UI;
using init.type;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.data;
using util.gui.misc;
using util.text;
using view.ui.manage;

namespace view.ui.profile
{
    public sealed class UILevel : IFullView
    {
        private readonly CLICKABLE.ClickSwitch switcher;
        private readonly Level level;
        private readonly UIBonus bonus;
        private readonly Titles titles;

        private static readonly CharSequence ¤¤Name = "¤Status";

        static UILevel()
        {
            D.ts(typeof(UILevel));
        }

        public UILevel() : base(¤¤Name, UI.icons().l.up)
        {
            section.body().setWidth(WIDTH).setHeight(1);

            section.addRelBody(8, DIR.S, picker());

            int height = HEIGHT - section.body().height() - 16;

            level = new Level(height);
            GETTER<BOOSTABLE_O> g = new GETTER<BOOSTABLE_O>()
            {
                public BOOSTABLE_O get()
                {
                    return HCLASS_RACE.clP(null, null);
                }
            };

            GETTER<Faction> fff = new GETTER<Faction>()
            {
                public Faction get()
                {
                    return FACTIONS.player();
                }
            };

            bonus = new UIBonus(g, fff, height)
            {
                protected override bool is(Boostable bo)
                {
                    return true;
                }
            };

            titles = new Titles(height);

            switcher = new CLICKABLE.ClickSwitch(level);
            switcher.setD(DIR.N);

            section.addRelBody(16, DIR.S, switcher);
        }

        private GuiSection picker()
        {
            GuiSection s = new GuiSection();

            s.addRightC(0, new GButt.ButtPanel(GAME.player().level().info.name)
            {
                protected override void clickA()
                {
                    switcher.set(level);
                }

                protected override void renAction()
                {
                    selectedSet(switcher.current() == level);
                }
            }.setDim(180, 32).hoverSet(GAME.player().level().info));

            s.addRightC(0, new GButt.ButtPanel(FACTIONS.player().titles.info.name)
            {
                protected override void clickA()
                {
                    switcher.set(titles);
                }

                protected override void renAction()
                {
                    selectedSet(switcher.current() == titles);
                    if (!selectedIs() && !hoveredIs() && FACTIONS.player().titles.hasNew())
                    {
                        bg(COLOR.WHITE202WHITE100);
                    }
                    else
                    {
                        bgClear();
                    }
                }
            }.setDim(180, 32).hoverSet(FACTIONS.player().titles.info));

            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Boosts)
            {
                protected override void clickA()
                {
                    switcher.set(bonus);
                }

                protected override void renAction()
                {
                    selectedSet(switcher.current() == bonus);
                }
            }.setDim(180, 32).hoverTitleSet(Dic.¤¤Boosts));

            return s;
        }

        //public void activate()
        //{
        //    show(VIEW.inters().manager);
        //}

        //protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        //{
        //    return section.hover(mCoo);
        //}

        //protected override void mouseClick(MButt button)
        //{
        //    if (button == MButt.RIGHT)
        //        hide();
        //    else if (button == MButt.LEFT)
        //        section.click();
        //}

        //protected override bool otherClick(MButt button)
        //{
        //    hide();
        //    return true;
        //}

        //protected override void hoverTimer(GBox text)
        //{
        //    section.hoverInfoGet(text);
        //}

        //protected override bool render(Renderer r, float ds)
        //{
        //    section.render(r, ds);
        //    return true;
        //}

        //protected override bool update(float ds)
        //{
        //    // TODO Auto-generated method stub
        //    return true;
        //}
    }
}