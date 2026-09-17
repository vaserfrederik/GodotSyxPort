using System;
using System.Collections.Generic;
using Util.Data;
using Util.Gui.Misc;
using Util.Gui.Slider;
using Util.Gui.Table;
using Util.Info;
using Util.Text;
using View.Main;
using View.Sett.Ui.Standing.Cats;
using Init.Race;
using Init.Sprite.UI;
using Init.Type;
using Settlement.Main;
using Settlement.Room.Infra.Elderly;
using Settlement.Room.Knowledge.School;
using Settlement.Room.Knowledge.University;
using Settlement.Stats;
using Settlement.Stats.Colls;
using Settlement.Stats.Standing;
using Settlement.Stats.Stat;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sets;

class CatOccupation : Cat
{
    private static string ¤¤workPrio = "Work Priorities";

    static CatOccupation()
    {
        D.ts(typeof(CatOccupation));
    }

    public CatOccupation(HCLASS cl, GETTER<Race> race) : base(new StatCollection[] { STATS.WORK(), STATS.EDUCATION() })
    {
        titleSet(Dic.¤¤Occupation);
        LinkedList<RENDEROBJ> rens = new LinkedList<RENDEROBJ>();

        rens.Add(new StatRow.Title(STATS.WORK().info));
        foreach (STAT s in STATS.WORK().workStats)
        {
            rens.Add(new StatRow(s, cl, race));
        }

        rens.Add(new GButt.ButtPanel(¤¤workPrio)
        {
            protected override void clickA()
            {
                VIEW.s().ui.rooms.prio(cl, race.get(), this);
            }
        }.pad(16, 2));

        if (cl != HCLASSES.SLAVE())
        {
            rens.Add(new StatRow(STATS.WORK().RET.RETIREMENT_AGE, cl, race));
            rens.Add(new StatRow(STATS.WORK().RET.RETIREMENT_HOME, cl, race));
        }

        if (cl == HCLASSES.CITIZEN())
        {
            rens.Add(new StatRow.Title(STATS.EDUCATION().info));
            foreach (StatEducation e in STATS.EDUCATION().all)
            {
                RENDEROBJ ch = new GButt.CheckboxSelect()
                {
                    protected override void clickA()
                    {
                        STATS.EDUCATION().policySet(cl, race.get(), e);
                        base.clickA();
                    }

                    protected override void renAction()
                    {
                        selectedSet(STATS.EDUCATION().policy(cl, race.get()) == e);
                    }
                };

                rens.Add(new StatRow(e.total, ch, cl, race));
            }

            foreach (AgeType aa in STATS.EDUCATION().allAges)
            {
                GuiSection s = new GuiSection()
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        aa.hoverLimit(text, HCLASS_RACE.clP(race.get(), cl));
                    }
                };
                s.body().incrW(8).incrH(1);

                s.addRightC(0, aa.icon);
                s.addRightC(0, new GHeader(aa.name));

                INTE ii = new INTE()
                {
                    public int min()
                    {
                        return 0;
                    }

                    public int max()
                    {
                        return StatsEducation.LIMIT_MAX;
                    }

                    public int get()
                    {
                        return aa.limit(cl, race.get());
                    }

                    public void set(int t)
                    {
                        aa.limitSet(cl, race.get(), t);
                    }
                };

                s.addRightCAbs(200, new GSliderInt(ii, 200, true));
                s.pad(8, 4);
                rens.Add(s);
            }

            {
                GuiSection s = new GuiSection();

                GRows rows = new GRows(2);

                rows.add(s);

                foreach (ROOM_UNIVERSITY u in SETT.ROOMS().UNIVERSITIES)
                {
                    s = new GuiSection();

                    s.add(new GStat()
                    {
                        public void update(GText text)
                        {
                            GFORMAT.iofk(text, u.employment().employed(), u.employment().employedMax());
                        }
                    }, 0, 0);

                    s.addRelBody(8, DIR.W, u.iconBig());
                    s.body().setWidth(135);
                    s.body().pad(0, 2);

                    GButt.ButtPanel b = new GButt.ButtPanel(s.asSprite())
                    {
                        public void hoverInfoGet(GUI_BOX box)
                        {
                            GBox b = (GBox)box;
                            b.title(u.info.names);
                            b.text(u.info.desc);
                            b.NL();

                            b.textLL(u.employment().title);
                            b.tab(6);
                            b.add(GFORMAT.iofk(b.text(), u.employment().employed(), u.employment().employedMax()));
                            b.NL();
                        }

                        protected override void clickA()
                        {
                            VIEW.s().panels.add(VIEW.s().ui.rooms.open(u), true);
                        }
                    };
                    rows.add(b);
                }

                rens.Add(rows.rows());
            }

            {
                GuiSection s = new GuiSection();
                s.add(new GHeader(HTYPES.CHILD().names));
                s.addRightC(8, new GStat()
                {
                    public void update(GText text)
                    {
                        GFORMAT.i(text, STATS.POP().pop(HTYPES.CHILD()));
                    }
                });
                rens.Add(s);

                GRows rows = new GRows(2);

                foreach (ROOM_SCHOOL u in SETT.ROOMS().SCHOOLS)
                {
                    s = new GuiSection();

                    s.addRightC(0, u.iconBig());

                    s.addRightC(8, new GStat()
                    {
                        public void update(GText text)
                        {
                            GFORMAT.percInv(text, u.service().load());
                        }
                    });

                    s.body().setWidth(120);
                    s.body().pad(16, 0);

                    GButt.ButtPanel b = new GButt.ButtPanel(s.asSprite())
                    {
                        public void hoverInfoGet(GUI_BOX box)
                        {
                            GBox b = (GBox)box;

                            b.textLL(Dic.¤¤Employees);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), u.employment().employed()));
                            b.NL();

                            b.textLL(Dic.¤¤load);
                            b.tab(6);
                            b.add(GFORMAT.perc(b.text(), u.service().load()));

                            b.NL();
                        }

                        protected override void clickA()
                        {
                            VIEW.s().panels.add(VIEW.s().ui.rooms.open(u), true);
                        }
                    };

                    rows.add(b);
                }

                rens.Add(rows.rows());
            }
        }

        section.add(new GScrollRows(rens, HEIGHT, 0).view());
    }

    public GuiSection makeHomes(HCLASS c, GETTER<Race> race)
    {
        GuiSection s = new GuiSection();
        int i = 0;

        foreach (ROOM_RESTHOME hh in SETT.ROOMS().RESTHOMES)
        {
            final ROOM_RESTHOME h = hh;

            SPRITE icon = new SPRITE.Imp(Icon.L)
            {
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    if (race.get() != null && race.get().pref().getWork(h.employment()) <= 0)
                    {
                        OPACITY.O50.bind();
                        COLOR.BLACK.render(r, X1, X2, Y1, Y2);
                        OPACITY.unbind();
                    }
                    h.iconBig().render(r, X1, X2, Y1, Y2);
                }
            };

            RENDEROBJ r = new GStat()
            {
                public void update(GText text)
                {
                    GFORMAT.iIncr(text, h.employment().neededWorkers() - h.employment().employed());
                }

                public void hoverInfoGet(GBox b)
                {
                    b.title(h.info.names);
                    b.text(h.info.desc);
                    b.NL(8);
                    b.textLL(HTYPES.RETIREE().names);
                    b.tab(5);
                    b.add(GFORMAT.iofk(b.text(), h.employment().employed(), h.employment().neededWorkers()));
                    b.NL();
                    b.textLL(Dic.¤¤Quality);
                    b.tab(5);
                    b.add(GFORMAT.perc(b.text(), h.quality()));
                    b.NL();
                    if (race.get() != null)
                    {
                        b.textLL(STANDINGS.get(c).fullfillment.info().name);
                        b.tab(5);
                        b.add(GFORMAT.perc(b.text(), race.get().pref().getWork(h.employment())));
                    }
                }
            }.hh(icon);

            s.add(r, 150 * (i % 4), 40 * (i / 4));
            i++;
        }

        s.pad(8);
        return s;
    }
}