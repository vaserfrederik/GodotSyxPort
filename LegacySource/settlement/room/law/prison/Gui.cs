using System;
using System.Collections.Generic;
using game;
using init.resources;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.types.prisoner;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using view.sett.ui.room;
using view.ui.message;

namespace settlement.room.law.prison
{
    class Gui : UIRoomModuleImp<PrisonInstance, ROOM_PRISON>
    {
        private static string ¤¤Food = "¤Food To Fetch";
        private static string ¤¤setAll = "¤Sentence all prisoners to be: {0}";
        private static string ¤¤setSure = "¤Are you sure you wish to inflict the punishment: {0} on all prisoners?";

        private static string ¤¤mWTitle = "Security low";
        private static string ¤¤mWBody = "Since our prisons are poorly staffed or poorly supplied with food, we are running the risk of having incidents occur. Make sure they are fully staffed and have access to food.";
        private static string ¤¤mTitle = "Prisoner Escape!";
        private static string ¤¤mBody = "Since our prison was poorly staffed and tended, the prisoners have escaped!";
        private static string ¤¤emp = "¤Insufficient employees or lack of food might cause incidents. Full employment is required.";
        private static string ¤¤cancel = "Cancel all manually assigned punishments, and let the prisoners be punished according to your law settings.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_PRISON s) : base(s) { }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<PrisonInstance> g, int x1, int y1)
        {
            GuiSection s = new GuiSection();
            int i = 0;

            foreach (ResG e in RESOURCES.EDI().all())
            {
                GButt.ButtPanel b = new GButt.ButtPanel(e.resource.icon())
                {
                    protected override void renAction()
                    {
                        selectedSet(g.get().fetch.has(e.resource));
                    }

                    protected override void clickA()
                    {
                        g.get().fetch.toggle(e.resource);
                        g.get().jobs.resNotFound.clear();
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(e.resource.names);
                        b.textLL(Dic.¤¤Consumed).add(GFORMAT.i(b.text(), (int)-blueprint.indu.ins().get(e.index()).year.get(g.get())));
                    }
                };
                b.pad(4, 4);

                s.add(b, (i % 4) * b.body().width(), (i / 4) * b.body().height());
                i++;
            }
            s.addRelBody(8, DIR.N, new GHeader(¤¤Food));

            s.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofk(text, g.get().prisoners(), g.get().prisonersMax());
                }
            }.hh(Dic.¤¤Capacity));

            section.addRelBody(8, DIR.S, s);

            {
                GuiSection ss = new GuiSection();
                int gi = 0;

                {
                    GButt.ButtPanel b = new GButt.ButtPanel(UI.icons().m.cancel)
                    {
                        protected override void clickA()
                        {
                            makePrisoners(g.get());
                            foreach (Humanoid h in list)
                            {
                                if (AIModule_Prisoner.DATA().punishmentSet.get(h.ai()) != null)
                                {
                                    AIModule_Prisoner.DATA().punishmentSet.set(h.ai(), null);
                                    h.interrupt();
                                }
                            }
                        }

                        protected override void renAction()
                        {
                            selectedSet(g.get().fetch.has(e.resource));
                        }

                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            GBox b = (GBox)text;
                            b.title(e.resource.names);
                            b.textLL(Dic.¤¤Consumed).add(GFORMAT.i(b.text(), (int)-blueprint.indu.ins().get(e.index()).year.get(g.get())));
                        }
                    };
                    b.pad(4, 4);

                    s.add(b, (i % 4) * b.body().width(), (i / 4) * b.body().height());
                    i++;
                }
                s.addRelBody(8, DIR.N, new GHeader(¤¤Food));

                s.addRelBody(8, DIR.S, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.iofk(text, g.get().prisoners(), g.get().prisonersMax());
                    }
                }.hh(Dic.¤¤Capacity));

                section.addRelBody(8, DIR.S, s);
            }

            private final ArrayListResize<Humanoid> list = new ArrayListResize<Humanoid>(164, 1024 * 2);
            private int upI = -1;

            protected override void problem(PrisonInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
            {

                if (i.riotChance < 1 && (i.employees().employed() < i.employees().max() || !i.jobs.resNotFound.isClear()))
                {

                    errors.add(¤¤emp);
                }

                base.problem(i, free, errors, warnings);
            }

            private void makePrisoners(PrisonInstance ins)
            {
                if (upI == GAME.updateI())
                    return;
                list.clearSoft();
                if (ins == null)
                    return;
                foreach (ENTITY e in SETT.ENTITIES().getAllEnts())
                {
                    if (e instanceof Humanoid)
                    {
                        Humanoid a = (Humanoid)e;
                        if (AIModule_Prisoner.isPrisoner(a, ins))
                        {
                            list.add(a);
                        }
                    }
                }
                upI = GAME.updateI();
            }

            protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
            {

                GuiSection s = new GuiSection();
                GChart cc = new GChart();
                int i = 0;
                foreach (ResG e in RESOURCES.EDI().all())
                {
                    RENDEROBJ r = new GStat()
                    {

                        public override void update(GText text)
                        {
                            GFORMAT.i(text, -blueprint.indu.ins().get(e.index()).history().get());
                        }

                        public override void hoverInfoGet(GBox b)
                        {
                            b.title(e.resource.name);
                            b.textLL(Dic.¤¤Consumed).add(GFORMAT.i(b.text(), (int)-blueprint.indu.ins().get(e.index()).history().get()));
                            b.NL();
                            cc.clear();
                            cc.add(blueprint.indu.ins().get(e.index()).history());
                            b.add(cc);
                        };
                    }.hv(e.resource.icon());

                    s.add(r, (i % 4) * 42, (i / 4) * 48);
                    i++;

                }

                s.addRelBody(8, DIR.S, new GStat()
                {

                    public override void update(GText text)
                    {
                        GFORMAT.iofk(text, blueprint.punishUsed(), blueprint.punishTotal());
                    }
                }.hh(Dic.¤¤Capacity));

                text.add(s);


                RENDEROBJ r = null;

                r = new GStat()
                {

                    public override void update(GText text)
                    {
                        GFORMAT.iofk(text, blueprint.punishUsed(), blueprint.punishTotal());
                    }
                }.hh(blueprint.constructor.prisoners.name()).hoverInfoSet(blueprint.constructor.prisoners.desc());
                text.add(r);
            }

            protected override void hover(GBox box, PrisonInstance i)
            {
                box.NL();
                box.text(blueprint.constructor.prisoners.name());
                box.add(GFORMAT.iofk(box.text(), i.prisoners(), i.prisonersMax()));
            }

            static void mWarn(PrisonInstance ins)
            {

                new Mess(¤¤mWTitle, ¤¤mWBody, ins.body().cX(), ins.body().cY()).send();

            }

            static void m(PrisonInstance ins)
            {

                new Mess(¤¤mTitle, ¤¤mBody, ins.body().cX(), ins.body().cY()).send();

            }


            private static class Mess : MessageSection
            {

                /**
                 * 
                 */
                private static readonly long serialVersionUID = 1L;
                private int tx, ty;
                private string desc;

                Mess(CharSequence title, CharSequence desc, int tx, int ty) : base(title)
                {
                    this.desc = "" + desc;
                    this.tx = tx;
                    this.ty = ty;
                }

                protected override void make(GuiSection section)
                {

                    paragraph(desc);
                    section.addRelBody(16, DIR.N, SETT.ROOMS().PRISON.iconBig().scaled(2));
                    section.addRelBody(16, DIR.S, new GButt.ButtPanel(UI.icons().m.crossair)
                    {
                        protected override void clickA()
                        {
                            VIEW.s().activate();
                            VIEW.s().getWindow().centererTile.set(tx, ty);
                        }
                    });
                }
            }
        }
    }
}