using System;
using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using util.gui.panel;
using util.gui.table;
using util.info;
using util.text;

namespace view.battle
{
    public abstract class UIBattleResult : GuiSection
    {
        private static readonly CharSequence ¤¤casulties = "Casualties";
        private static readonly CharSequence ¤¤kills = "Enemy Kills";

        static UIBattleResult()
        {
            D.ts(typeof(UIBattleResult));
        }

        public UIBattleResult(CharSequence title)
        {
            add(side(GAME.ARMIES().player()));
            addRelBody(8, DIR.E, side(GAME.ARMIES().enemy()));

            GText tt = new GText(UI.FONT().H1, title).lablify();
            RENDEROBJ thing = new RENDEROBJ.RenderImp(tt.width(), UI.PANEL().titleBoxes[UI.PANEL().titleBoxes.Length - 1].height)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    UI.PANEL().titleBoxes[UI.PANEL().titleBoxes.Length - 1].renderCY(r, body.x1(), body.cY(), body().width());
                    tt.renderC(r, body);
                }
            };
            addRelBody(16, DIR.N, thing);

            GuiSection s = new GuiSection();
            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Close)
            {
                protected override void clickA()
                {
                    close();
                }
            });
            s.addRightC(0, new GButt.ButtPanel(BattlePanel.¤¤restart)
            {
                protected override void clickA()
                {
                    VIEW.b().state().reloadBattle();
                }
            });
            addRelBody(16, DIR.S, s);

            add(new GPanel(this.body()));
            moveLastToBack();
        }

        private RENDEROBJ side(Army army)
        {
            GRows rr = new GRows(8);
            int deaths = 0;
            foreach (Div div in army.divisions())
            {
                if (div.info.men() <= 0)
                {
                    continue;
                }
                deaths += div.info.men() - div.menNrOf();
                rr.add(new Card(div));
            }

            GuiSection s = new GuiSection();
            int dd = deaths;
            s.add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, dd);
                }
            }.hh(UI.icons().s.death));

            s.addRelBody(8, DIR.S, new GScrollRows(rr.rowsCentered(VIEW.UI().div.normal.width() * 8), new Card(GAME.ARMIES().division((short)0)).body().height() * 6).view());
            return s;
        }

        private class Card : HOVERABLE.HoverableAbs
        {
            int kills;
            private readonly Div div;

            public Card(Div div)
            {
                body.setDim(VIEW.UI().div.normal.width(), VIEW.UI().div.normal.height() + 8);
                this.div = div;
                kills = GAME.ARMIES().factors.kills(div);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                GButt.ButtPanel.renderBG(r, true, false, isHovered, body);
                VIEW.UI().div.renderBasics(r, body.x1(), body.y1(), 1, div.info);
                double menTot = div.info.men();
                double menNow = div.menNrOf();
                GMeter.renderDelta(r, 1.0, (double)menNow / menTot, body.x1(), body.x2(), body.y2() - 12, body.y2());
                GButt.ButtPanel.renderFrame(r, body);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                int menTot = div.info.men();
                int menNow = div.menNrOf();
                b.title(div.info.name());
                b.textLL(¤¤casulties);
                b.tab(6);
                b.add(GFORMAT.iofk(b.text(), menTot - menNow, menTot));
                b.NL();
                b.textLL(¤¤kills);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), kills));
                base.hoverInfoGet(text);
            }
        }

        protected abstract void close();
    }
}