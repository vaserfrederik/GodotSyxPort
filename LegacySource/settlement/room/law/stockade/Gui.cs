using System;
using System.Collections.Generic;
using settlement.room.law.stockade;
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

class Gui : UIRoomModuleImp<StockInstance, ROOM_STOCKADE>
{
    private static readonly CharSequence ¤¤Food = "¤Food To Fetch";
    private static readonly CharSequence ¤¤setAll = "¤Sentence all captives to be: {0}.";
    private static readonly CharSequence ¤¤setSure = "¤Are you sure you wish to inflict the punishment: {0} on all captives?";

    private static readonly CharSequence ¤¤mWTitle = "Security low";
    private static readonly CharSequence ¤¤mWBody = "Since our stockades are poorly staffed or poorly supplied with food, we are running the risk of incidents occurring.";
    private static readonly CharSequence ¤¤mTitle = "Prisoner Escape!";
    private static readonly CharSequence ¤¤mBody = "Since our stockade was poorly staffed and tended, the prisoners have escaped!";
    private static readonly CharSequence ¤¤emp = "¤Insufficient employees or lack of food might cause incidents. Full employment is required.";
    private static readonly CharSequence ¤¤cancel = "Cancel all manually assigned punishments, and let the prisoners be punished according to your law settings.";

    static
    {
        D.ts(typeof(Gui));
    }

    public Gui(ROOM_STOCKADE s) : base(s)
    {
    }

    protected override void AppendPanel(GuiSection section, GGrid grid, GETTER<StockInstance> g, int x1, int y1)
    {
        GuiSection s = new GuiSection();
        int i = 0;

        foreach (ResG e in RESOURCES.EDI().all())
        {
            GButt.ButtPanel b = new GButt.ButtPanel(e.resource.icon())
            {
                protected override void RenAction()
                {
                    SelectedSet(g.get().fetch.has(e.resource));
                }

                protected override void ClickA()
                {
                    g.get().fetch.toggle(e.resource);
                    g.get().jobs.resNotFound.clear();
                }

                public override void HoverInfoGet(GUI_BOX text)
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
            public override void Update(GText text)
            {
                GFORMAT.iofk(text, g.get().prisonersCurrent, g.get().prisonersMax);
            }
        }.hh(Dic.¤¤Capacity));

        section.addRelBody(8, DIR.S, s);

        {
            GuiSection ss = new GuiSection();

            int gi = 0;

            {
                GButt.ButtPanel b = new GButt.ButtPanel(UI.icons().m.cancel)
                {
                    protected override void ClickA()
                    {
                        MakePrisoners(g.get());
                        foreach (Humanoid h in list)
                        {
                            if (AIModule_Prisoner.DATA().punishmentSet.get(h.ai()) != null)
                            {
                                AIModule_Prisoner.DATA().punishmentSet.set(h.ai(), null);
                                h.interrupt();
                            }
                        }
                    }

                    protected override void RenAction()
                    {
                        bool a = false;
                        MakePrisoners(g.get());
                        foreach (Humanoid h in list)
                        {
                            if (AIModule_Prisoner.DATA().punishmentSet.get(h.ai()) != null)
                            {
                                a = true;
                                break;
                            }
                        }
                        ActiveSet(a);
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.text(¤¤cancel);
                    }
                };
                b.pad(4, 4);

                ss.add(b, (gi % 4) * b.body().width(), (gi / 4) * b.body().height());
                gi++;
            }

            foreach (PUNISHMENT p in PUNISHMENT.values)
            {
                GButt.ButtPanel b = new GButt.ButtPanel(p.icon())
                {
                    protected override void ClickA()
                    {
                        MakePrisoners(g.get());
                        foreach (Humanoid h in list)
                        {
                            AIModule_Prisoner.DATA().punishmentSet.set(h.ai(), p);
                        }
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(AIModule_Prisoner.DATA().punishmentSet.get(list[0].ai()) == p);
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(p.name);
                        b.textLL(Dic.¤¤Consumed).add(GFORMAT.i(b.text(), (int)-blueprint.indu.ins().get(p.index()).year.get(g.get())));
                    }
                };
                b.pad(4, 4);

                ss.add(b, (gi % 4) * b.body().width(), (gi / 4) * b.body().height());
                gi++;
            }

            section.addRelBody(8, DIR.S, ss);
        }

        GTable t = new GTable();
        t.addHead("Prisoner");
        t.addHead("Punishment");

        MakePrisoners(g.get());
        foreach (Humanoid h in list)
        {
            GButt.ButtText b = new GButt.ButtText(h.name())
            {
                protected override void ClickA()
                {
                    h.click();
                }
            };
            t.add(b);

            GButt.ButtText p = new GButt.ButtText(AIModule_Prisoner.DATA().punishmentSet.get(h.ai()).name)
            {
                protected override void ClickA()
                {
                    AIModule_Prisoner.DATA().punishmentSet.set(h.ai(), PUNISHMENT.values[(PUNISHMENT.values.IndexOf(AIModule_Prisoner.DATA().punishmentSet.get(h.ai())) + 1) % PUNISHMENT.values.Count]);
                }
            };
            t.add(p);
        }

        section.addRelBody(8, DIR.S, t);
    }

    protected override void Hover(GBox b, StockInstance ins)
    {
        b.NL();
        b.textLL(Dic.¤¤Capacity);
        b.tab(6);
        b.add(GFORMAT.iofk(b.text(), ins.prisonersCurrent, ins.prisonersMax));

        b.NL();
    }

    protected override void Problem(StockInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
    {
        if (i.riotChance < 1 && (i.employees().employed() < i.employees().max() || !i.jobs.resNotFound.isClear()))
        {
            errors.add(¤¤emp);
        }

        base.Problem(i, free, errors, warnings);
    }

    protected override void AppendMain(GGrid gg, GGrid text, GuiSection sExtra)
    {
        GuiSection s = new GuiSection();
        GChart cc = new GChart();
        int i = 0;
        foreach (ResG e in RESOURCES.EDI().all())
        {
            GStat r = new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.i(text, -blueprint.indu.ins().get(e.index()).history().get());
                }

                public override void HoverInfoGet(GBox b)
                {
                    b.title(e.resource.name);
                    b.textLL(Dic.¤¤Consumed).add(GFORMAT.i(b.text(), (int)-blueprint.indu.ins().get(e.index()).history().get()));
                    b.NL();
                    cc.clear();
                    cc.add(blueprint.indu.ins().get(e.index()).history());
                    b.add(cc);
                }
            }.hv(e.resource.icon());

            s.add(r, (i % 4) * 42, (i / 4) * 48);
            i++;
        }

        s.addRelBody(8, DIR.S, new GStat()
        {
            public override void Update(GText text)
            {
                GFORMAT.iofk(text, blueprint.prisoners, blueprint.prisonersMax);
            }
        }.hh(Dic.¤¤Capacity));

        text.add(s);
    }

    private readonly ArrayListResize<Humanoid> list = new ArrayListResize<Humanoid>(164, 1024 * 2);
    private int upI = -1;

    private void MakePrisoners(StockInstance ins)
    {
        if (upI == GAME.updateI())
            return;
        list.clearSoft();
        if (ins == null)
            return;
        foreach (ENTITY e in SETT.ENTITIES().getAllEnts())
        {
            if (e is Humanoid)
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

    static void mWarn(StockInstance ins)
    {
        new Mess(¤¤mWTitle, ¤¤mWBody, ins.body().cX(), ins.body().cY()).send();
    }

    static void m(StockInstance ins)
    {
        new Mess(¤¤mTitle, ¤¤mBody, ins.body().cX(), ins.body().cY()).send();
    }

    private class Mess : MessageSection
    {
        private int tx, ty;
        private string desc;

        public Mess(CharSequence title, CharSequence desc, int tx, int ty) : base(title)
        {
            this.desc = "" + desc;
            this.tx = tx;
            this.ty = ty;
        }

        protected override void Make(GuiSection section)
        {
            paragraph(desc);
            section.addRelBody(16, DIR.N, SETT.ROOMS().STOCKADE.iconBig().scaled(2));
            section.addRelBody(16, DIR.S, new GButt.ButtPanel(UI.icons().m.crossair)
            {
                protected override void ClickA()
                {
                    VIEW.s().activate();
                    VIEW.s().getWindow().centererTile.set(tx, ty);
                }
            });
        }
    }
}