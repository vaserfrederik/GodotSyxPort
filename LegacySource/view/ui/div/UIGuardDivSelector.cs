using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using game.boosting;
using init.constant;
using init.type;
using settlement.main;
using settlement.room.law.guard;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

namespace view.ui.div
{
    public class UIGuardDivSelector : GuiSection
    {
        private static readonly CharSequence ¤¤guards = "Guard Force";

        static UIGuardDivSelector()
        {
            D.ts(typeof(UIGuardDivSelector));
        }

        private readonly int cols = 8;
        private readonly ArrayList<Card> allCards = new ArrayList<Card>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<GuiSection> allRows = new ArrayList<GuiSection>((int)Math.Ceiling(Config.battle().DIVISIONS_PER_ARMY / (double)cols));

        public UIGuardDivSelector()
        {
            while (allRows.HasRoom())
                allRows.Add(new GuiSection());

            while (allCards.HasRoom())
                allCards.Add(new Card(allCards.Size()));

            {
                GuiSection s = new GuiSection();

                s.Add(new GHeader(¤¤guards));

                s.AddDown(2, new GStat
                {
                    Update = (GText text) =>
                    {
                        int a = 0;
                        foreach (Div d in GAME.ARMIES().player().divisions())
                            if (blue().activeDuty.is(d))
                                a++;
                        GFORMAT.i(text, a);
                    }
                }.hh(Dic.¤¤Divisions, 120));

                s.Add(new GStat
                {
                    Update = (GText text) =>
                    {
                        int tot = 0;
                        foreach (Div d in GAME.ARMIES().player().divisions())
                            if (blue().activeDuty.is(d))
                                tot += d.info.men();
                        GFORMAT.iofk(text, STATS.POP().pop(HTYPES.GUARD()), tot);
                    }
                }.hh(Dic.¤¤Soldiers, 120), 0, s.body().y2() + 4);

                s.Add(new GStat
                {
                    Update = (GText text) =>
                    {
                        GFORMAT.f0(text, blue().power.get());
                    }
                }.hh(Dic.¤¤Power, 120), 0, s.body().y2() + 4);

                s.Add(new GStat
                {
                    Update = (GText text) =>
                    {
                        GFORMAT.f0(text, STATS.LAW().guards.data(null).getD(null));
                    }
                }.hh(BOOSTABLES.CIVICS().LAW.name, 120), 0, s.body().y2() + 4);

                Add(s);
            }

            setDims();
            RENDEROBJ ss = new GScrollRows(allRows, allCards.Get(0).body.height() * 5)
            {
                PassesFilter = (int i, RENDEROBJ o) => true // allRows.Get(i).elements().size() > 0;
            }.view();

            AddRelBody(8, DIR.S, ss);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            foreach (GuiSection s in allRows)
                s.clear();

            int rowI = 0;

            foreach (Div d in GAME.ARMIES().player().divisions())
            {
                if (d.info.men() > 0)
                {
                    allRows.Get(rowI).addRightC(0, allCards.Get(d.indexArmy()));
                    if (allRows.Get(rowI).elements().size() == cols)
                        rowI++;
                }
            }
            setDims();

            base.render(r, ds);
        }

        private void setDims()
        {
            foreach (GuiSection s in allRows)
            {
                s.body().setWidth(allCards.Get(0).body.width() * cols);
                s.body().setHeight(allCards.Get(0).body.height());
            }
        }

        private ROOM_GUARD blue()
        {
            return SETT.ROOMS().GUARD;
        }

        private sealed class Card : ClickableAbs
        {
            private readonly int di;

            public Card(int di) : base(VIEW.UI().div.settCivic.width(), VIEW.UI().div.settCivic.height())
            {
                this.di = di;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                Div d = GAME.ARMIES().player().divisions().Get(di);
                isSelected = blue().activeDuty.is(d);
                VIEW.UI().div.settCivic.render(r, body.x1(), body.y1(), 1, d, isActive, isSelected, isHovered);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                Div d = GAME.ARMIES().player().divisions().Get(di);
                VIEW.UI().div.settCivic.hover(text, d);
                base.hoverInfoGet(text);
            }

            protected override void clickA()
            {
                Div d = GAME.ARMIES().player().divisions().Get(di);
                blue().activeDuty.toggle(d);
            }
        }
    }
}