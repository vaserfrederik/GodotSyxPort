using System;
using System.Collections.Generic;
using settlement.room.law.police;
using game.boosting;
using init.race.appearence;
using init.settings;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;
using util;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room;

namespace settlement.room.law.police
{
    class Gui : UIRoomModuleImp<PoliceInstance, ROOM_POLICE>
    {
        private static CharSequence ¤¤suspects = "Suspects";
        private static CharSequence ¤¤AA = "Suspected Sloth";
        private static CharSequence ¤¤BB = "Suspected Witch";
        private static CharSequence ¤¤CC = "Suspected Warlock";
        private static CharSequence ¤¤DD = "Suspected Shapeshifter";
        private static CharSequence ¤¤EE = "Suspected Heretic";
        private static CharSequence ¤¤FF = "Suspected Beastialist";
        private static CharSequence ¤¤GG = "Suspected Turncoat";
        private static CharSequence ¤¤HH = "Suspected Renegade";
        private static CharSequence ¤¤II = "Suspected Collaborator";

        private static CharSequence ¤¤value = "The effect of your police force. Depends on the amount of police divided by the population to keep in check. Diminishing returns.";

        private static CharSequence[] tt = new CharSequence[] { ¤¤AA, ¤¤BB, ¤¤CC, ¤¤DD, ¤¤EE, ¤¤FF, ¤¤GG, ¤¤HH, ¤¤II };

        static
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_POLICE s) : base(s)
        {
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<PoliceInstance> g, int x1, int y1)
        {
            GuiSection s = new GuiSection()
            {
                Render = (r, ds) =>
                {
                    GUTIL.hList().clearSloppy();
                    foreach (COORDINATE c in g.Get().body())
                    {
                        if (g.Get().is(c))
                        {
                            Humanoid a = blueprint.work.client(c.x(), c.y());
                            if (a != null)
                                GUTIL.hList().add(a);
                        }
                    }
                    base.Render(r, ds);
                }
            };

            GTableBuilder bu = new GTableBuilder()
            {
                NrOFEntries = () => GUTIL.hList().size()
            };

            bu.Column(¤¤suspects, 400, new GRowBuilder()
            {
                Build = (ier) =>
                {
                    GButt.BSection r = new GButt.BSection()
                    {
                        HoverInfoGet = (text) =>
                        {
                            if (ier.Get() == null)
                                return;
                            Humanoid h = (Humanoid)GUTIL.hList().Get(ier.Get());
                            VIEW.s().ui.subjects.hoverInfo(h, (GBox)text);
                        }
                    };

                    r.Add(new GStat(UI.FONT().H2())
                    {
                        Update = (text) =>
                        {
                            if (ier.Get() == null)
                                return;
                            Humanoid h = (Humanoid)GUTIL.hList().Get(ier.Get());
                            text.Lablify();
                            text.Add(STATS.APPEARANCE().name(h.indu()));
                        }
                    }, 0, 0);

                    r.AddDown(2, new GStat()
                    {
                        Update = (text) =>
                        {
                            if (ier.Get() == null)
                                return;
                            Humanoid h = (Humanoid)GUTIL.hList().Get(ier.Get());
                            text.Warnify();
                            text.Add(tt[STATS.RAN().get(h.indu(), 5) % tt.Length]);
                        }
                    });

                    r.AddRelBody(8, DIR.W, new SPRITE.Imp(RPortrait.P_WIDTH, RPortrait.P_HEIGHT())
                    {
                        Render = (r, X1, X2, Y1, Y2) =>
                        {
                            if (ier.Get() == null)
                                return;
                            Humanoid h = (Humanoid)GUTIL.hList().Get(ier.Get());
                            STATS.APPEARANCE().portraitRender(r, h.indu(), X1, Y1, 1);
                        }
                    });

                    r.Body().SetWidth(400 - 16);

                    r.Pad(8, 3);

                    return r;
                }
            });
            s.Add(bu.Create(6, false));
            section.AddRelBody(8, DIR.S, s);
        }

        protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
        {
            RENDEROBJ r = null;

            r = new GStat()
            {
                Update = (text) =>
                {
                    GFORMAT.perc(text, blueprint.value());
                },
                HoverInfoGet = (b) =>
                {
                    b.text(¤¤value);
                    b.NL(4);

                    b.textLL(Dic.¤¤Employees);
                    b.tab(6);
                    GFORMAT.i(b.text(), blueprint.employment().employed());
                    b.NL();

                    b.textLL(Dic.¤¤Population);
                    b.NL();
                    foreach (HCLASS_RACE r in HCLASS_RACE.REAL())
                    {
                        if (blueprint.access(r).is())
                        {
                            b.tab(1).Add(r.icon);
                            b.tab(6);
                            GFORMAT.i(b.text(), STATS.POP().POP.data(r.cl).get(r.race));
                            b.NL();
                        }
                    }

                    b.text(Dic.¤¤Value);
                    b.tab(6);
                    GFORMAT.perc(b.text(), blueprint.value());
                    b.NL();
                }
            }.hh(Dic.¤¤Value);
            text.Add(r);

            foreach (BoostSpec s in blueprint.spec.all())
            {
                r = new GStat()
                {
                    Update = (text) =>
                    {
                        s.booster.format(text, s.get(HCLASS_RACE.clP()));
                    }
                }.hh(s.boostable.name);
                text.Add(r);
            }

            GRows rr = new GRows(8);

            HCLASS prev = HCLASS_RACE.REAL()[0].cl;
            foreach (HCLASS_RACE cl in HCLASS_RACE.REAL())
            {
                if (prev != cl.cl)
                {
                    prev = cl.cl;
                    rr.nl();
                }
                if (cl.cl.player)
                {
                    rr.Add(new GButt.ButtPanel(cl.icon)
                    {
                        RenAction = () =>
                        {
                            selectedSet(blueprint.access(cl).is());
                            base.RenAction();
                        },
                        ClickA = () =>
                        {
                            blueprint.access(cl).toggle();
                        },
                        HoverInfoGet = (text) =>
                        {
                            text.title(cl.name);
                            GBox b = (GBox)text;
                            b.textLL(BOOSTABLES.BEHAVIOUR().SUBMISSION.name);
                            b.tab(6);
                            b.add(GFORMAT.perc(b.text(), BOOSTABLES.BEHAVIOUR().SUBMISSION.get(cl)));
                            b.NL();

                            b.textLL(Dic.¤¤Population);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), STATS.POP().POP.data(cl.cl).get(cl.race)));
                            b.NL();
                        }
                    });
                }
            }

            RENDEROBJ rt = new GScrollRows(rr.rows(), rr.rows()[0].body().height() * 5).view();

            text.Add(new GButt.ButtPanel(Dic.¤¤Settings)
            {
                ClickA = () =>
                {
                    VIEW.inters().popup.show(rt, this);
                }
            });
        }

        protected override void hover(GBox box, PoliceInstance i)
        {
            box.NL();
            box.text(¤¤suspects);
            box.add(GFORMAT.i(box.text(), i.prisoners));
            if (S.get().developer)
            {
                box.add(GFORMAT.i(box.text(), i.prisonersMax()));
            }
        }
    }
}