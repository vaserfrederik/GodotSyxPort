using System;
using System.Text;
using System.Collections.Generic;
using game;
using game.nobility;
using init.race.appearence;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.main;

namespace view.sett.ui.noble
{
    class NobleRow : GuiSection
    {
        private static string ¤¤Rank = "Current rank of this noble. More ranks allows a noble to contribute a lot more towards their current assignment. Ranks are gained by levelling up your city. A noble can not be stripped of ranks, but the ranks will become available after their death.";
        private static string ¤¤no = "Unassigned Nobility";
        private static string ¤¤assign = "Assign this noble to an office.";
        static NobleRow()
        {
            D.ts(typeof(NobleRow));
        }

        public static int width = 500;
        private readonly GETTER<int> ier;

        public NobleRow(GETTER<int> ier)
        {
            this.ier = ier;

            GStat s = new GStat(UI.FONT().S)
            {
                update = (GText text) =>
                {
                    text.lablifySub();
                    text.add(n().rankName());
                }
            };
            add(s, 0, 0);

            s = new GStat(UI.FONT().H2)
            {
                update = (GText text) =>
                {
                    text.lablify();
                    text.add(STATS.APPEARANCE().name(n().subject().indu()));
                    text.setMaxWidth(420);
                    text.setMultipleLines(false);
                }
            };
            addDown(2, s);

            s = new GStat(UI.FONT().S)
            {
                update = (GText text) =>
                {
                    if (n().office() == null)
                    {
                        text.warnify().add(¤¤no);
                    }
                    else
                        text.lablifySub().add(n().title());
                }
            };
            addDown(2, s);

            {
                GButt.ButtPanel p = new GButt.ButtPanel(new Assignments())
                {
                    clickA = () =>
                    {
                        VIEW.s().ui.nobles.assigns.n = n();
                        VIEW.inters().popup.show(VIEW.s().ui.nobles.assigns, this);
                    },
                    hoverInfoGet = (GUI_BOX text) =>
                    {
                        if (n().office() == null)
                            text.text(¤¤assign);
                        else
                            n().hoverOffice(text);
                    }
                };
                p.pad(4, 4);
                addDown(4, p);

                GuiSection rank = new GuiSection
                {
                    hoverInfoGet = (GUI_BOX text) =>
                    {
                        text.text(¤¤Rank);
                    }
                };

                rank.addRightC(8, new GButt.ButtPanel(UI.icons().s.chevron(DIR.N))
                {
                    clickA = () => GAME.NOBLE().ranksAllocate(n()),
                    renAction = () => activeSet(n().rank() < GAME.NOBLE().maxRanks() - 1 && GAME.NOBLE().ranksAllocated() < (int)GAME.NOBLE().MAX_RANKS.get(HCLASS_RACE.clP()))
                }.pad(4, 4));

                rank.addRightC(4, new GStat
                {
                    update = (GText text) => GFORMAT.i(text, n().rank())
                });

                addRightC(16, rank);

                addRight(32, new GButt.ButtPanel(UI.icons().m.crossair)
                {
                    clickA = () =>
                    {
                        VIEW.s().activate();
                        VIEW.s().getWindow().centerer.set(n().subject().body().cX(), n().subject().body().cY());
                    }
                });
            }

            SPRITE p = new SPRITE.Imp(RPortrait.P_WIDTH * 2, RPortrait.P_HEIGHT * 2)
            {
                render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                {
                    STATS.APPEARANCE().portraitRender(r, n().subject().indu(), X1, Y1, 2);
                }
            };

            addRelBody(16, DIR.W, p);
            body().pad(16, 2);
            body().setWidth(width);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            GButt.ButtPanel.renderBG(r, true, false, false, body());
            GButt.ButtPanel.renderFrame(r, body());
            base.render(r, ds);
        }

        private Noble n()
        {
            return GAME.NOBLE().active().get(ier.get());
        }

        private class Assignments : SPRITE.Imp
        {
            public Assignments() : base(Icon.L, Icon.L) { }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                int cy = Y1 + (Y2 - Y1) / 2;
                int cx = X1 + (X2 - X1) / 2;
                NobleOffice o = n().office();
                if (o == null)
                    UI.icons().m.questionmark.renderC(r, cx, cy);
                else
                    o.icon.renderC(r, cx, cy);
            }
        }
    }
}