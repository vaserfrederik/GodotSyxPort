using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using util.gui;
using view.main;
using settlement.stats;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats.standing;
using init.race;
using init.sprite.UI;
using init.sprite;
using init.constant;
using init.type;
using world;
using init.faction;
using util.colors;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace view.sett.ui.right
{
    final class UIMiniRaces : Expansion
    {
        private readonly INTE t;

        public UIMiniRaces(int index, int y1) : base(index)
        {
            RENDEROBJ[] rows = new RENDEROBJ[RACES.all().size()];
            for (int i = 0; i < RACES.all().size(); i++)
            {
                rows[i] = Noble(i);
            }

            int width = rows[0].body().width();
            body().setDim(width + 6, C.HEIGHT() - y1);

            RENDEROBJ c;
            c = new GButt.Glow(UI.decor().up)
            {
                protected override void renAction()
                {
                    activeSet(t.get() > 0);
                }
                protected override void clickA()
                {
                    t.inc(-1);
                }
            };
            c.body().moveCX(body().cX());
            c.body().moveY1(body().y1() + 3);
            add(c);

            GScrollRows sc = new GScrollRows(rows, C.HEIGHT() - y1 - (c.body().height() + 3) * 2, 0, false);
            addDownC(0, sc.view());

            c = new GButt.Glow(UI.decor().down)
            {
                protected override void renAction()
                {
                    activeSet(t.get() != t.max());
                }
                protected override void clickA()
                {
                    t.inc(1);
                }
            };
            addDownC(0, c);
            body().moveY1(y1);
            t = sc.target;
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            if (visableIs())
            {
                GCOLOR.UI().panBG.render(r, body());
                GCOLOR.UI().borderH(r, body(), 0);
                base.render(r, ds);
            }
            if (!MButt.LEFT.isDown())
                clickI = -1;
        }

        private static readonly CharSequence ¤¤Immigrants = "¤Immigrants";
        private static readonly CharSequence ¤¤ToBeAccepted = "¤To be Accepted";
        private static readonly CharSequence ¤¤Children = "¤Infants";
        private static readonly CharSequence ¤¤Desc = "¤An assortment of opinions from your citizens:";
        private static readonly CharSequence ¤¤Double = "¤Double click to grant access to all immigrants. Right click to open species settings.";
        private static readonly CharSequence ¤¤Incoming = "¤Incoming";
        private static readonly CharSequence ¤¤PerDay = "/day";

        static
        {
            D.ts(typeof(UIMiniRaces));
        }

        static int clickI = -1;

        private static class RaceUI : GuiSection
        {
            private double viewI = -60 * 5;
            private int cache = 0;
            private int old;
            private readonly int ri;
            private readonly ArrayList<Str> tmp = new ArrayList<Str>
            {
                new Str(128),
                new Str(128),
                new Str(128),
                new Str(128)
            };

            public RaceUI(int ri)
            {
                this.ri = ri;

                body().setWidth(Icon.M * 2);
                addDownC(2, new SPRITE.Imp(Icon.M, Icon.M)
                {
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        Race res = FACTIONS.player().races.get(ri);
                        if (res != null)
                        {
                            base.render(r, X1, X2, Y1, Y2);
                            if (hoveredIs() && MButt.RIGHT.consumeClick())
                            {
                                VIEW.s().ui.standing.open(res);
                            }
                        }
                    }
                });

                addDownC(0, new GText(() =>
                {
                    Race res = FACTIONS.player().races.get(ri);
                    if (res != null)
                    {
                        return res.bio().name();
                    }
                    return "";
                }));

                addDownC(0, new GText(() =>
                {
                    Race res = FACTIONS.player().races.get(ri);
                    if (res != null)
                    {
                        return res.bio().description();
                    }
                    return "";
                }));
            }

            public override void hoverInfoGet(GText text)
            {
                Race res = FACTIONS.player().races.get(ri);
                if (res != null)
                {
                    text.add(res.bio().name());
                    text.NL();
                    text.add(res.bio().description());
                    text.NL();
                }
                base.hoverInfoGet(text);
            }

            public override bool click()
            {
                if (MButt.LEFT.isDouble())
                {
                    Race res = FACTIONS.player().races.get(ri);
                    if (res != null)
                    {
                        int am = SETT.ENTRY().immi().wanted(res);
                        if (am > 0)
                            SETT.ENTRY().immi().admit(res, am);
                    }
                }
                clickI = ri;

                return base.click();
            }
        }

        private static RENDEROBJ Noble(int ri)
        {
            return new RaceUI(ri);
        }
    }
}