using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using game;
using game.boosting;
using game.faction;
using game.faction.player;
using init.sprite.UI;
using init.type;
using init.value;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using world.region;

namespace view.ui.profile
{
    final class Level : GuiSection
    {
        private readonly string ¤¤Title = "¤{0} {1} of {2}";

        public Level(int height)
        {
            D.t(this);

            GTextR title = new GTextR(new GText(UI.FONT().H1, 64).lablify())
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    text().clear();
                    text().add(¤¤Title);
                    text().insert(0, FACTIONS.player().level().current().name());
                    text().insert(1, FACTIONS.player().rulerName());
                    text().insert(2, FACTIONS.player().name);
                    text().adjustWidth();
                    base.render(r, ds, isHovered);
                }
            }.setAlign(DIR.N);
            add(title);

            RENDEROBJ desc = new RENDEROBJ.RenderImp(720, 64)
            {
                private readonly GText text = new GText(UI.FONT().M, 200);
                private readonly GText tmp = new GText(UI.FONT().M, 200);

                public override void render(SPRITE_RENDERER r, float ds)
                {
                    text.clear();

                    int y1 = body().y1() + text.height() / 2;

                    foreach (PTitle t in FACTIONS.player().titles.all())
                    {
                        if (t.selected())
                        {
                            tmp.set(text);
                            if (text.width() > 0 && tmp.width() + text.width() > body().width())
                            {
                                text.renderC(r, body().cX(), y1);
                                text.clear();
                                text.add(t.name);
                                text.adjustWidth();
                                y1 += text.height();
                            }
                            else
                            {
                                if (text.width() > 0)
                                    text.add(',').s();
                                text.add(t.name);
                                text.adjustWidth();
                            }
                        }
                    }

                    if (text.width() > 0)
                    {
                        text.renderC(r, body().cX(), y1 + text.height() / 2);
                    }
                }
            };

            addDownC(8, desc);

            GuiSection stats = new GuiSection();

            stats.add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, POP.tot(HCLASSES.CITIZEN(), null));
                }
            }.hv(HCLASSES.CITIZEN().names));

            stats.addRightCAbs(120, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, POP.tot(null));
                }
            }.hv(Dic.¤¤Population));

            stats.addRightCAbs(120, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, POP.tot(null) + RD.RACES().population.faction().get(FACTIONS.player()) - RD.RACES().population.get(FACTIONS.player().realm().capitol()));
                }
            }.hv(Dic.¤¤Subjects));

            addRelBody(0, DIR.S, stats);

            RENDEROBJ[] rens = new RENDEROBJ[GAME.player().level().all().size()];
            for (int i = 0; i < rens.Length; i++)
            {
                rens[i] = new TRow(GAME.player().level().all().get(i));
            }

            int h = height - getLastY2() - 16;
            int am = h / rens[0].body().height();
            h = am * rens[0].body().height();

            RENDEROBJ r = new GScrollRows(rens, h, 0).view();

            addDownC(8, r);
        }

        private class TRow : GuiSection
        {
            PLevels.Level l;

            TRow(PLevels.Level l)
            {
                this.l = l;

                int w = 500;

                add(new GHeader(l.name()));

                body().setWidth(w);

                GuiSection s = new GuiSection();

                foreach (BoostSpec b in l.boosters.all())
                {
                    s.addRightC(2, b.boostable.icon);
                    if (s.body().width() + s.getLast().width() >= w - body().width())
                        break;
                }
                s.body().moveX2(w);
                s.body().moveCY(body().cY());
                absorb(s);

                s = new GuiSection();
                s.body().setHeight(32);
                foreach (Lock<?> b in l.lockers.all())
                {
                    s.addRightC(8, b.lockable.icon);
                    if (s.body().width() + s.getLast().width() >= w)
                        break;
                }

                bool f = true;
                foreach (BoostSpec b in l.boosters.all())
                {
                    s.addRightC(f ? 8 : 64, new GStat()
                    {
                        public override void update(GText text)
                        {
                            b.booster.format(text, b.booster.to());
                        }
                    }.hh(b.boostable.icon.big));
                    f = false;
                    if (s.body().width() + s.getLast().width() >= w)
                        break;
                }

                s.body().moveX1(body().x1());
                s.body().moveY1(body().y2() + 8);
                absorb(s);

                SPRITE nn = new GText(UI.FONT().H2, GFORMAT.toNumeral(l.index() + 1));
                add(nn, -16 * 4, 0);

                pad(8);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                GCOLOR.UI().border().render(r, body());
                GCOLOR.UI().bg().render(r, body(), -1);
                base.render(r, ds);
                if (l.index() > GAME.player().level().current().index())
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body(), -1);
                    OPACITY.unbind();
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                l.hoverInfoGet(text);
            }
        }
    }
}