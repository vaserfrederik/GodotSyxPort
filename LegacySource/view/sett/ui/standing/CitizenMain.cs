using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.sett.ui.standing.Cats;
using view.sett.ui.standing.decree;

namespace view.sett.ui.standing
{
    final class CitizenMain : GuiSection
    {
        static int width = 220;
        private readonly INT.IntImp hov = new INT.IntImp();

        public CitizenMain(HCLASS cl, GETTER<Race> race, int HEIGHT, Cats cats)
        {
            add(infoButt(cats, cl, race));
            addRelBody(8, DIR.S, mainHappiness(cats, hov, cl, race));

            List<RENDEROBJ> rens = new List<RENDEROBJ>(STATS.COLLECTIONS().size());

            foreach (Cat c in cats.all)
            {
                rens.Add(new CatButt(cats, c, cl, race, hov));
            }

            int hh = HEIGHT - body().height() - 8;
            hh = hh / rens[0].body().height();
            hh *= rens[0].body().height();

            GScrollRows r = new GScrollRows(rens, hh);
            add(r.view(), body().x1(), body().y2() + 16);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
            hov.set(-1);
        }

        private static RENDEROBJ infoButt(Cats cats, HCLASS cl, GETTER<Race> race)
        {
            GuiSection s = new GuiSection();

            s.add(new GButton(() =>
            {
                if (race.get() == null)
                {
                    race.set(FACTION.player().races()[0]);
                }
                else
                {
                    int index = Array.IndexOf(FACTION.player().races(), race.get()) + 1;
                    if (index >= FACTION.player().races().Length)
                    {
                        index = 0;
                    }
                    race.set(FACTION.player().races()[index]);
                }
            }, new GText("Change Race")));

            s.addRight(8, new GText(() => race.get() != null ? race.get().name : "No Race Selected"));

            s.addRight(8, new GButton(() =>
            {
                if (cats.cs != null)
                {
                    List<Cat> selected = new List<Cat>();
                    foreach (CatButt b in cats.cs)
                    {
                        if (b.selected)
                        {
                            selected.Add(b.cs);
                        }
                    }
                    if (selected.Count > 0)
                    {
                        foreach (Cat cat in selected)
                        {
                            cat.cs.delete();
                        }
                    }
                }
            }, new GText("Delete Selected")));

            s.addRight(8, new GButton(() =>
            {
                if (cats.cs != null)
                {
                    foreach (CatButt b in cats.cs)
                    {
                        b.selected = false;
                    }
                }
            }, new GText("Deselect All")));

            s.addRight(8, new GButton(() =>
            {
                if (cats.cs != null)
                {
                    foreach (CatButt b in cats.cs)
                    {
                        b.selected = true;
                    }
                }
            }, new GText("Select All")));

            return s;
        }

        private static RENDEROBJ mainHappiness(Cats cats, INT.IntImp hov, HCLASS cl, GETTER<Race> race)
        {
            GuiSection s = new GuiSection();

            GuiSection ss = new GuiSection();
            ss.add(new GHeader("Loyalty"));
            ss.addRightCAbs(184, new GStat(() =>
            {
                return race.get() != null ? GFORMAT.perc(new GText(), STANDINGS.get(cl).loyalty.getD(race.get())) : "";
            }));

            RENDEROBJ r = new RENDEROBJ.RenderImp(width, 24)
            {
                render = (SR, ds) =>
                {
                    if (race.get() != null)
                    {
                        double now = STANDINGS.get(cl).loyalty.getD(race.get());
                        GMeter.renderSuperDelta(SR, now, now, body, true);
                    }
                }
            };
            ss.add(r, 0, ss.body().y2() + 4);

            GStaples st = new GStaples(STATS.DAYS_SAVED)
            {
                render = (SR, ds, isHovered) =>
                {
                    isHovered = true;
                    setHovered(hov.get());
                    base.render(SR, ds, isHovered);
                },
                hover = (box, stapleI) =>
                {
                    if (race.get() != null)
                    {
                        box.title(STANDINGS.get(cl).loyalty.name);
                        int fromZero = STATS.DAYS_SAVED - stapleI - 1;
                        GText t = box.text();
                        DicTime.setDaysAgo(t, fromZero);
                        box.add(t);

                        box.tab(6);
                        box.add(GFORMAT.perc(box.text(), STANDINGS.get(cl).loyalty.getD(race.get(), fromZero)));
                        box.NL();

                        if (fromZero >= STATS.DAYS_SAVED - 1)
                        {
                            return;
                        }

                        box.sep();

                        box.textLL(STANDINGS.get(cl).bhappiness.name);
                        box.tab(7);
                        box.add(GFORMAT.percInc(box.text(), CLAMP.d(STANDINGS.get(cl).loyalty.getD(race.get(), fromZero) - STANDINGS.get(cl).loyalty.getD(race.get(), fromZero - 1), 0, 100)));
                        box.NL();

                        foreach (Cat ca in cats.all)
                        {
                            int v1 = (int)(100 * CatButt.Staples.value(stapleI, ca.cs, cl, race));
                            int v2 = v1;
                            if (stapleI > 0)
                                v2 = (int)(100 * CatButt.Staples.value(stapleI - 1, ca.cs, cl, race));
                            if (v1 != v2)
                            {
                                box.tab(1);
                                box.textL(ca.cs[0].info.name);
                                box.tab(7);
                                double d = (v1 - v2) / 100.0;
                                box.add(GFORMAT.f0(box.text(), d));
                                box.NL();
                            }
                        }

                        double d = STANDINGS.get(cl).expectation.getD(race.get(), fromZero);
                        double d2 = STANDINGS.get(cl).expectation.getD(race.get(), fromZero + 1);
                        double v = d / d2;
                        if (v != 1)
                        {
                            box.tab(1);
                            box.textL(STANDINGS.get(cl).expectation.info.name);
                            box.tab(7);
                        }

                        if (v < 1)
                        {
                            box.add(GFORMAT.percInc(box.text(), (1 - v)));
                        }
                        else if (v > 1)
                        {
                            box.add(GFORMAT.percInc(box.text(), -(v - 1)));
                        }

                        box.sep();

                        for (int i = 0; i < STANDINGS.get(cl).loyalty.bo.all().size(); i++)
                        {
                            Booster b = STANDINGS.get(cl).loyalty.bo.all()[i];
                            double n = STANDINGS.get(cl).loyalty.factor(race.get(), i, fromZero);
                            double p = STANDINGS.get(cl).loyalty.factor(race.get(), i, fromZero + 1);
                            box.add(b.info.icon);
                            box.textLL(b.info.name);
                            box.tab(7);

                            if (b.isMul)
                            {
                                box.add(GFORMAT.percInc(box.text(), n - p));
                            }
                            else
                            {
                                box.add(GFORMAT.f0(box.text(), n - p));
                            }
                            box.NL();
                        }
                        box.NL();
                    }
                },
                hover = (box, stapleI) =>
                {
                    if (base.hover(box, stapleI))
                    {
                        hov.set(hoverI());
                        return true;
                    }
                    return false;
                },
                getValue = (stapleI) =>
                {
                    int fromZero = STATS.DAYS_SAVED - stapleI - 1;
                    return STANDINGS.get(cl).loyalty.getD(race.get(), fromZero);
                },
                setColor = (c, stapleI, value) =>
                {
                    c.interpolate(GCOLOR.UI().BAD.hovered, GCOLOR.UI().GOOD2.hovered, value);
                }
            };
            st.normalize(false);
            st.body().setWidth(7 * STATS.DAYS_SAVED);
            st.body().setHeight(64);
            st.body().centerY(ss);
            st.body().moveX1(width + 8);
            ss.add(st);

            s.addDown(2, ss);

            return s;
        }
    }
}