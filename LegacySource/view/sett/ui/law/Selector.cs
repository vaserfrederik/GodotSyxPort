using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using init.sprite.UI;
using init.type.CRIMES;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;

namespace view.sett.ui.law
{
    class Selector : GuiSection
    {
        private Race race = null;
        int am;

        public Selector(int HEIGHT, LIST<CRIME> types)
        {
            ArrayListGrower<RENDEROBJ> rens = new ArrayListGrower<RENDEROBJ>();

            for (int ii = 0; ii < RACES.all().size(); ii++)
            {
                final int ri = ii;
                SPRITE s = new SPRITE.Imp(Icon.L * 2)
                {
                    GText t = new GText(UI.FONT().S, 8);

                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        int am = 0;
                        foreach (CRIME c in types)
                            am += c.stat().criminals(FACTIONS.player().races.get(ri));
                        t.clear();
                        GFORMAT.i(t, am);
                        t.adjustWidth();
                        if (am > 0)
                            FACTIONS.player().races.get(ri).appearance().iconBig.render(r, X1, X2, Y1, Y2);
                        OPACITY.O50.bind();
                        if (am <= 0)
                            FACTIONS.player().races.get(ri).appearance().iconBig.render(r, X1, X2, Y1, Y2);
                        COLOR.BLACK.render(r, X1, X1 + t.width() + 8, Y1, Y1 + t.height() + 6);
                        OPACITY.unbind();
                        t.render(r, X1 + 4, Y1 + 3);
                    }
                };

                GButt.ButtPanel b = new GButt.ButtPanel(s)
                {
                    protected override void clickA()
                    {
                        race = FACTIONS.player().races.get(ri);
                    }

                    protected override void renAction()
                    {
                        selectedSet(getRace() == FACTIONS.player().races.get(ri));
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.title(FACTIONS.player().races.get(ri).info.names);
                        base.hoverInfoGet(text);
                    }
                };
                rens.add(b);
            }

            GScrollRows sc = new GScrollRows(rens, rens.get(0).body().height() * (HEIGHT / rens.get(0).body().height()), 0);
            add(sc.view());
        }

        public Race getRace()
        {
            if (race == null)
                race = FACTIONS.player().race();
            return race;
        }
    }
}