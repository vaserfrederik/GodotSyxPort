using System;
using System.Collections.Generic;
using System.Linq;

namespace settlement.room.food.pasture
{
    using init.resources;
    using settlement.room.industry.module;
    using settlement.room.main;
    using snake2d.util.datatypes;
    using snake2d.util.gui.GUI_BOX;
    using snake2d.util.gui.GuiSection;
    using snake2d.util.gui.renderable.RENDEROBJ;
    using snake2d.util.misc;
    using snake2d.util.sets;
    using snake2d.util.sprite.text;
    using util.data;
    using util.gui.misc;
    using util.gui.table.GTableSorter;
    using util.info;
    using util.text;
    using view.main;
    using view.sett.ui.room;
    using view.sett.ui.room.UIRoomModule;

    class Gui : UIRoomModuleImp<PastureInstance, ROOM_PASTURE>
    {
        static readonly CharSequence ¤¤Animals = "¤Animals";
        static readonly CharSequence ¤¤Adults = "¤Adult Animals";
        static readonly CharSequence ¤¤Tending = "¤Tending";
        static readonly CharSequence ¤¤Skill = "¤Skill";
        static readonly CharSequence ¤¤SkillD = "¤Skill that gets put into the tending, multiplying the output.";
        static readonly CharSequence ¤¤BaseRate = "¤Base Rate";

        static readonly CharSequence ¤¤DailyWork = "¤Daily Tending";
        static readonly CharSequence ¤¤DailyWorkD = "¤Daily Tending is the amount of work needed to keep this pasture functioning. The amount of work needed depends on the amount of animals. If the workers fail to do the tending, animals will start to die. Resets each day.";
        static readonly CharSequence ¤¤SlaughterAll = "¤Slaughter all";
        static readonly CharSequence ¤¤SlaughterAllDesc = "¤Slaughter all animals and immediately receive some produce?";

        static readonly CharSequence ¤¤ProdExp = "¤Production of the current day is based on the work of the previous day. The workers must tend to the animals each day. Failing to tend to the animals for one day will result in lower produce. Failing two days in a row results in livestock dying.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        Gui(ROOM_PASTURE s) : base(s)
        {
        }

        public override void hover(GBox box, PastureInstance i)
        {
            base.hover(box, i);
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<PastureInstance> getter, int x1, int y1)
        {
            GuiSection s = new GuiSection();

            s.addRightC(32, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkInv(text, getter.get().animalsCurrent, getter.get().animalsMax);
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.textLL(¤¤Adults);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), getter.get().animalsCurrent - getter.get().animalsCubs));
                }
            }.hv(¤¤Animals));

            s.addRightC(32, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkInv(text, CLAMP.i(getter.get().work, 0, getter.get().workMax), getter.get().neededWork(getter.get().animalsCurrent()));
                }
            }.hv(¤¤DailyWork, ¤¤DailyWorkD));

            s.addRightC(32, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.perc(text, getter.get().skill());
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.title(¤¤Skill);
                    b.text(¤¤SkillD);
                    b.NL(8);
                    IndustryUtil.hoverBoosts(b, 1, null, getter.get().blueprintI().indus.get(0).bonus(), getter.get(), 1);
                }
            }.hv(¤¤Skill));

            section.addRelBody(8, DIR.S, s);

            RENDEROBJ b = new GButt.ButtPanel(¤¤SlaughterAll)
            {
                private readonly ACTION yes = new ACTION()
                {
                    public override void exe()
                    {
                        getter.get().slaughterAll();
                    }
                };
                protected override void clickA()
                {
                    VIEW.inters().yesNo.activate(¤¤SlaughterAllDesc, yes, ACTION.NOP, true);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    double produce = blueprint.slaughterAmount(false, getter.get().industry()) * (getter.get().animalsCurrent - getter.get().animalsCubs);
                    produce += blueprint.slaughterAmount(true, getter.get().industry()) * (getter.get().animalsCubs);

                    foreach (IndustryResource r in getter.get().industry().outs())
                    {
                        if (r.resource == RESOURCES.LIVESTOCK())
                            continue;
                        double am = produce * r.rate;

                        b.add(r.resource.icon());
                        b.text(r.resource.name);
                        b.tab(7);
                        b.add(GFORMAT.f0(b.text(), am));
                        b.NL();
                    }
                }
            }.hoverInfoSet(¤¤SlaughterAllDesc);

            section.addRelBody(8, DIR.S, b);
        }

        protected override void problem(PastureInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            base.problem(i, free, errors, warnings);
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }

        public static void industryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins)
        {
            PastureInstance ii = (PastureInstance)ins;
            b.NL(8);
            b.text(¤¤ProdExp);
            b.NL(8);
            b.textLL(Dic.¤¤Multipliers);
            b.NL();

            b.text(¤¤BaseRate);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), i.rate));
            b.NL();

            double prod = i.rate;

            foreach (RoomBoost bb in ii.blueprintI().indus.get(0).boosts())
            {
                b.text(bb.info().name);
                b.tab(6);
                b.add(GFORMAT.f1(b.text(), bb.get(ii)));
                b.NL();
                prod *= bb.get(ii);
            }

            b.NL(8);

            b.textL(Dic.¤¤Total);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), prod));
            b.NL();
        }
    }
}