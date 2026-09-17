using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.faction.FWorth;
using game.faction.player;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.colors;
using util.data;
using util.gui.misc;
using util.info;
using util.text;

namespace view.ui.economy
{
    final class MainDetails : GuiSection
    {
        private readonly GText t = new GText(UI.FONT().S, 48).lablify();

        public MainDetails(IntImp ii)
        {
            int i = 0;

            foreach (CredHistory h in GAME.player().credits().all())
            {
                HOVERABLE hh = new HOVERABLE.Sprite(new SDetail(h)
                {
                    @override
                    void up(GText text)
                    {
                        int i = ii.get();
                        if (i < 0)
                            i = GAME.player().credits().creditsH().historyRecords() - 1;
                        i = GAME.player().credits().creditsH().historyRecords() - i - 1;
                        GFORMAT.iIncr(text, h.IN.get(i) - h.OUT.get(i));
                    }
                }).hoverTitleSet(h.type.name).hoverInfoSet(h.type.desc);

                hh.body().moveX1Y1((i % 1) * (hh.body().width() + 32), (i / 1) * (hh.body().height() + 2));
                add(hh);
                i++;
            }

            addDown(0, new HOVERABLE.HoverableAbs(260, 32)
            {
                GText t = new GText(UI.FONT().S, 48).lablify();
                @override
                protected void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    GCOLOR.T().H1.bind();
                    UI.FONT().H2.renderCY(r, body().x1() + UI.FONT().S.height() * 2, body().cY(), Dic.¤¤Treasury);
                    t.clear();
                    int i = ii.get();
                    if (i < 0)
                        i = GAME.player().credits().creditsH().historyRecords() - 1;
                    i = GAME.player().credits().creditsH().historyRecords() - i - 1;
                    GFORMAT.i(t, FACTIONS.player().credits().creditsH().get(i));
                    t.adjustWidth();
                    t.renderCY(r, body().x2() - t.width(), body().cY());
                }
            });

            addDown(0, new HOVERABLE.HoverableAbs(260, 32)
            {
                GText t = new GText(UI.FONT().S, 48).lablify();
                @override
                protected void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    GCOLOR.T().H1.bind();
                    UI.FONT().H2.renderCY(r, body().x1() + UI.FONT().S.height() * 2, body().cY(), Dic.¤¤NetWorth);
                    t.clear();
                    int i = ii.get();
                    if (i < 0)
                        i = GAME.player().credits().worth.historyRecords() - 1;
                    i = GAME.player().credits().creditsH().historyRecords() - i - 1;
                    GFORMAT.i(t, FACTIONS.player().credits().worth.get(i));
                    t.adjustWidth();
                    t.renderCY(r, body().x2() - t.width(), body().cY());
                }

                @override
                public void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    foreach (WINT d in FACTIONS.WORTH().faction)
                    {
                        b.add(d.icon);
                        b.textL(d.info.name);
                        b.tab(6);
                        b.add(GFORMAT.iIncr(b.text(), d.player()));
                        b.NL();
                        b.text(d.info.desc);
                        b.NL(5);
                    }
                    base.hoverInfoGet(text);
                }
            });
        }

        private abstract class SDetail : SPRITE
        {
            private readonly GStat stat = new GStat()
            {
                @override
                public void update(GText text)
                {
                    up(text);
                }
            };
            private readonly CredHistory cr;

            SDetail(CredHistory cr)
            {
                this.cr = cr;
            }

            abstract void up(GText text);

            @override
            public int width()
            {
                return 260;
            }

            @override
            public int height()
            {
                return stat.height();
            }

            @override
            public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                ColorImp.TMP.set(COLOR.UNIQUE.getC(cr.type.ordinal())).shadeSelf(0.5f);
                ColorImp.TMP.render(r, X1, X1 + height(), Y1, Y1 + height());
                ColorImp.TMP.set(COLOR.UNIQUE.getC(cr.type.ordinal()));
                ColorImp.TMP.render(r, X1 + 2, X1 + height() - 2, Y1 + 2, Y1 + height() - 2);

                t.clear().add(cr.type.name);

                t.render(r, X1 + height() * 2, Y1);

                stat.adjust();

                stat.render(r, X2 - stat.width(), Y1);
            }

            @override
            public void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}