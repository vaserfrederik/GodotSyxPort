using settlement.room.service.breeder;
using init.type;
using settlement.entity;
using settlement.room.industry.module;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.info;
using util.text;
using view.sett.ui.room;

class Gui : UIRoomModuleImp<BreederInstance, ROOM_BREEDER>
{
    private static CharSequence ¤¤limitGlobal = "Global limit";
    private static CharSequence ¤¤limit = "Class limit";
    
    private static CharSequence ¤¤limitGlobalD = "A limit for when to stop producing children, based on total population of all classes and species.";
    private static CharSequence ¤¤limitD = "A limit for when to stop producing children, based on population of current class and species.";
    
    private static CharSequence ¤¤limitGlobalProb = "Your total population of all races exceeds the limit set.";
    private static CharSequence ¤¤limitProb = "Your population of citizens of the current species exceeds the limit set.";
    
    private static CharSequence ¤¤population = "Population";
    private static CharSequence ¤¤incoming = "Incoming";
    private static CharSequence ¤¤total = "Total";
    private static CharSequence ¤¤toBreed = "To Breed";

    static Gui()
    {
        D.ts(Gui.class);
    }
    
    public Gui(ROOM_BREEDER s) : base(s)
    {
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GETTER<BreederInstance> getter, int x1, int y1)
    {
        RENDEROBJ rr = new GStat()
        {
            public override void update(GText text)
            {
                double n = IndustryUtil.calcProductionRate(blueprint.PRODUCTION_SPEED_DAY, blueprint.productionData, getter.get());
                GFORMAT.fRel(text, n, blueprint.PRODUCTION_SPEED_DAY);
            }

            public override void hoverInfoGet(GBox b)
            {
                IndustryUtil.hoverProductionRate(b, blueprint.PRODUCTION_SPEED_DAY, blueprint.productionData, getter.get());
            }
        }.hh(blueprint.race.appearance().iconBig);

        section.addRelBody(8, DIR.S, rr);

        rr = new GButt.ButtPanel(STATS.MULTIPLIERS().PROSECUTION.name)
        {
            protected override void clickA()
            {
                blueprint.prosecute = !blueprint.prosecute;
            }

            protected override void renAction()
            {
                selectedSet(blueprint.prosecute);
                base.renAction();
            }
        };
        section.addRelBody(8, DIR.S, rr);
    }
    
    protected override void appendMain(GGrid icons, GGrid r, GuiSection sExtra)
    {
        {
            GuiSection s = new GuiSection()
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.title(¤¤limitGlobal);
                    b.text(¤¤limitGlobalD);
                    b.NL(8);

                    b.textLL(¤¤population);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.tot()));
                    b.NL();

                    b.textLL(¤¤incoming);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.next() - POP.tot()));
                    b.NL();

                    b.textLL(¤¤total);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.next()));
                    b.NL();

                    b.textLL(¤¤toBreed);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), blueprint.limitTotal - POP.next()));
                    b.NL();
                }
            };

            s.add(new GHeader(¤¤limitGlobal));

            INTE in = new INTE()
            {
                public override int min()
                {
                    return 0;
                }

                public override int max()
                {
                    return ENTETIES.MAX;
                }

                public override int get()
                {
                    return CLAMP.i(blueprint.limitTotal, min(), max());
                }

                public override void set(int t)
                {
                    CLAMP.i(t, min(), max());
                    blueprint.limitTotal = t;
                }
            };
            s.addRelBody(2, DIR.S, new GSliderInt(in, 200, true));

            r.section.addRelBody(0, DIR.S, s);
        }

        {
            GuiSection s = new GuiSection()
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.title(¤¤limit);
                    b.text(¤¤limitD);
                    b.NL(8);

                    b.textLL(¤¤population);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.tot(HCLASSES.CITIZEN(), blueprint.race)));
                    b.NL();

                    b.textLL(¤¤incoming);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.next(HCLASSES.CITIZEN(), blueprint.race) - POP.tot(HCLASSES.CITIZEN(), blueprint.race)));
                    b.NL();

                    b.textLL(¤¤total);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), POP.next(HCLASSES.CITIZEN(), blueprint.race)));
                    b.NL();

                    b.textLL(¤¤toBreed);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), blueprint.limitSpecies - POP.next(HCLASSES.CITIZEN(), blueprint.race)));
                    b.NL();
                }
            };

            s.add(new GHeader(¤¤limit));

            INTE in = new INTE()
            {
                public override int min()
                {
                    return 0;
                }

                public override int max()
                {
                    return ENTETIES.MAX;
                }

                public override int get()
                {
                    return CLAMP.i(blueprint.limitSpecies, min(), max());
                }

                public override void set(int t)
                {
                    CLAMP.i(t, min(), max());
                    blueprint.limitSpecies = t;
                }
            };
            s.addRelBody(2, DIR.S, new GSliderInt(in, 200, true));

            r.section.addRelBody(0, DIR.S, s);
        }
    }
    
    protected override void hover(GBox box, BreederInstance i)
    {
    }
    
    protected override void problem(BreederInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
    {
        if (POP.next(HCLASSES.CITIZEN(), blueprint.race) >= blueprint.limitSpecies)
        {
            errors.add(¤¤limitProb);
        }

        if (POP.next(null, null) >= blueprint.limitTotal)
        {
            errors.add(¤¤limitGlobalProb);
        }

        base.problem(i, free, errors, warnings);
    }
}