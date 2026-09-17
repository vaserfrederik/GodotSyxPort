using System;
using System.Collections.Generic;
using init.race.appearence;
using init.resources;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.home.house
{
    final class HomeHoverer : UIRoomModule
    {
        private static readonly CharSequence ¤¤Residents = "¤{0}  ({1})";
        private static readonly CharSequence ¤¤VacantFor = "¤Vacant for {0}:";
        private static readonly CharSequence ¤¤Any = "¤Any species of any class.";
        private static readonly CharSequence ¤¤None = "¤None";

        static HomeHoverer()
        {
            D.ts(typeof(HomeHoverer));
        }

        private PO[] pos = new PO[40];

        public HomeHoverer()
        {
            GText t = new GText(UI.FONT().S, 64);
            for (int i = 0; i < pos.Length; i++)
                pos[i] = new PO(t);
        }

        public override void hover(GBox box, Room in, int rx, int ry)
        {
            HomeInstance h = SETT.ROOMS().HOME.getter.get(rx, ry);

            if (h == null)
                return;

            box.textL(Dic.¤¤Upgrade);
            box.tab(6);
            box.add(GFORMAT.iofkInv(box.text(), h.upgrade(), SETT.ROOMS().HOME.upgrades().max()));
            box.NL(8);

            if (h.occupants() > 0)
            {
                GText t = box.text();
                t.add(¤¤Residents);
                t.insert(0, h.race().info.namePosessive);
                t.insert(1, h.occupant(0).indu().clas().names);
                box.NL();
                box.textLL(t);
                box.add(GFORMAT.iofk(box.text(), h.occupants(), h.occupantsMax()));
                box.NL();

                int ti = 0;
                for (int i = 0; i < h.occupants(); i++)
                {
                    PO po = pos[i];
                    po.h = h.occupant(i);
                    box.add(po);
                    if ((ti & 1) == 1)
                        box.NL();
                    ti++;
                }

                box.NL(8);

                int ri = 0;
                ti = 0;
                foreach (RES_AMOUNT ra in h.race().home().clas(h.occupant(0).indu().clas()).resources())
                {
                    box.tab(ti * 2);
                    box.add(ra.resource().icon());
                    int curr = 0;
                    int max = ra.amount() * h.occupants();
                    for (int oi = 0; oi < h.occupants(); oi++)
                    {
                        curr += STATS.HOME().current(h.occupant(oi), ri);
                    }
                    ri++;
                    box.add(GFORMAT.iofkInv(box.text(), curr, max));
                    ti++;
                    if (ti >= 4)
                    {
                        ti = 0;
                        box.NL();
                    }
                }
            }
            else
            {
                GText t = box.text();
                box.NL();
                t.add(¤¤VacantFor);
                t.insert(0, h.occupantsMax());
                box.add(t);
                box.NL();

                HTypeBits s = h.setting();

                {
                    int a = 0;

                    foreach (HGROUP tt in HGROUP.all())
                    {
                        if (s.is(tt))
                            a++;
                    }

                    int am = 0;
                    if (a == HGROUP.all().size())
                    {
                        box.text(¤¤Any);
                    }
                    else if (a == 0)
                    {
                        box.text(¤¤None);
                    }
                    else if (a <= HGROUP.all().size() / 2)
                    {
                        foreach (HGROUP tt in HGROUP.all())
                        {
                            if (s.is(tt))
                            {
                                box.add(tt.icon);
                                am++;
                                if (am > 10)
                                {
                                    am = 0;
                                    box.NL();
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (HGROUP tt in HGROUP.all())
                        {
                            if (!s.is(tt))
                            {
                                box.add(tt.icon);
                                box.rewind(tt.icon.width());
                                box.add(UI.icons().m.anti);
                                am++;
                                if (am > 10)
                                {
                                    am = 0;
                                    box.NL();
                                }
                            }
                        }
                    }
                }
            }

            box.NL(8);
            box.textLL(Dic.¤¤Isolation);
            box.tab(5);
            box.add(GFORMAT.perc(box.text(), h.isolation()));

            base.hover(box, in, rx, ry);
        }

        private class PO : SPRITE.Imp
        {
            public Humanoid h;
            private readonly GText t;

            public PO(GText t) : base(RPortrait.P_WIDTH * 8, RPortrait.P_HEIGHT)
            {
                this.t = t;
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                STATS.APPEARANCE().portraitRender(r, h.indu(), X1, Y1, 1);
                t.clear().add(STATS.APPEARANCE().name(h.indu()));
                t.setMaxChars(22);
                t.lablifySub();
                t.render(r, X1 + RPortrait.P_WIDTH + 4, Y1);
                t.clear().add(h.title());
                t.normalify();
                t.render(r, X1 + RPortrait.P_WIDTH + 4, Y1 + t.height() + 2);
            }
        }
    }
}