using settlement.room.industry.module.consumption;
using game.time;
using init.resources;
using init.sprite.UI;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room;
using view.sett.ui.room.UIRoomBulkApplier;
using view.sett.ui.room.UIRoomModule;

public class ConsumptionGui<A extends RoomInstance, B extends RoomBlueprintIns<A>> : UIRoomModuleImp<A, B>
{
    private static readonly CharSequence ¤¤use = "If usage is enabled, the employees produce x{0} as much.";
    private static readonly CharSequence ¤¤ConsumedDay = "¤Consumed today";
    private static readonly CharSequence ¤¤ConsumedNow = "¤Consumed This Year";
    private static readonly CharSequence ¤¤ConsumedYEsterday = "¤Consumed Yesterday";
    private static readonly CharSequence ¤¤ConsumedPrevious = "¤Consumed last year";
    private static readonly CharSequence ¤¤ConsumptionD = "¤Estimation of how many resources are consumed each day.";
    private static readonly CharSequence ¤¤Consumption = "¤Consumption";

    private static readonly CharSequence ¤¤Stored = "¤Stored";
    private static readonly CharSequence ¤¤Incoming = "¤Fetchers";

    static
    {
        D.ts(ConsumptionGui.class);
    }

    private readonly RoomConsumption cons;
    private readonly GChart chart = new GChart();

    public ConsumptionGui(B s, RoomConsumption cons) : base(s)
    {
        this.cons = cons;
    }

    protected override void AppendPanel(GuiSection section, GGrid grid, GETTER<A> getter, int x1, int y1)
    {
        GuiSection t = new GuiSection();

        foreach (IndustryResource rr in cons.ins())
        {
            SPRITE sp = new SPRITE.Imp(48, 64)
            {
                GText t = new GText(UI.FONT().S, 8);
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    t.clear();

                    RoomInstance ins = getter.get();
                    ROOM_IDATA_INSTANCE insi = (ROOM_IDATA_INSTANCE)getter.get();

                    double am = ins.employees().employed() * IndustryUtil.CalcConsumptionRate(rr.rate, ins, cons);
                    if (cons.stored(rr).get(insi) <= 0)
                        am *= 1 + cons.boost(rr);

                    GFORMAT.i(t, -(int)am);
                    rr.resource.icon().renderC(r, X1, X2, Y1, Y1 + 32);
                    t.adjustWidth();
                    t.renderC(r, X1, X2, Y2 - 32, Y2);

                    ROOM_IDATA_INSTANCE iins = (ROOM_IDATA_INSTANCE)getter.get();
                    if (!iins.getWork().resourceReachable(rr.resource))
                    {
                        GCOLOR.T().IBAD.bind();
                        UI.icons().s.alert.render(r, X1 + 4, Y1 + 4);
                        COLOR.unbind();
                    }
                }
            };

            HOVERABLE h;

            h = new GButt.ButtPanel(sp)
            {
                protected override void clickA()
                {
                    cons.enabledToggle(rr, (ROOM_IDATA_INSTANCE)getter.get(), getter.get());
                }

                protected override void renAction()
                {
                    selectedSet(cons.enabled(rr, (ROOM_IDATA_INSTANCE)getter.get()));
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    GText t = b.text();
                    t.add(¤¤use);
                    t.insert(0, 1 + cons.boost(rr), 2);
                    b.add(t);
                    b.NL();

                    b.textLL(¤¤Stored);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), cons.stored(rr).get((ROOM_IDATA_INSTANCE)getter.get())));
                    b.NL();
                    b.textLL(¤¤Incoming);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), cons.reseved(rr).get((ROOM_IDATA_INSTANCE)getter.get())));
                    b.NL();

                    b.sep();

                    hoverConsumptionIns(text, rr, getter.get(), cons);
                }
            };

            t.addRightC(0, h);
        }

        t.addRelBody(30, DIR.E, ModuleIndustry.makeFetch(getter));

        section.addRelBody(8, DIR.S, t);
    }

    protected override void AppendMain(GGrid r, GGrid text, GuiSection sExtra)
    {
        foreach (IndustryResource rr in cons.ins())
        {
            GStat s = new GStat()
            {
                public void update(GText text)
                {
                    int am = rr.history().getPeriodSum(-(int)TIME.years().bitConversion(TIME.days()), 0);
                    GFORMAT.iIncr(text, -am);
                }
            };

            r.add(new GHeader.HeaderHorizontal(rr.resource.icon(), s)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    hoverConsumption(text, rr, cons, chart);
                }
            });
        }
    }

    public static void hoverConsumptionIns(GUI_BOX text, IndustryResource i, RoomInstance ins, RoomConsumptionAbs cons)
    {
        GBox b = (GBox)text;
        b.title(i.resource.name);

        ROOM_IDATA_INSTANCE iins = (ROOM_IDATA_INSTANCE)ins;
        if (!iins.getWork().resourceReachable(i.resource))
        {
            b.error(Dic.¤¤Unreachable);
            b.NL();
        }

        ROOM_IDATA_INSTANCE p = (ROOM_IDATA_INSTANCE)ins;

        b.NL(8);
        b.textLL(¤¤ConsumedDay);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), (int)i.day.getD(p)));
        b.NL();

        b.textLL(¤¤ConsumedYEsterday);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), i.dayPrev.get(p)));

        b.NL(0);
        b.textLL(¤¤ConsumedNow);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), i.year.get(p)));
        b.NL();

        b.textLL(¤¤ConsumedPrevious);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), i.yearPrev.get(p)));

        b.NL(8);

        b.textLL(¤¤ConsumptionD);
        b.NL();

        IndustryUtil.hoverConsumptionRate(text, i.rate, ins, cons);
    }

    public static void hoverConsumption(GUI_BOX text, IndustryResource rr, RoomConsumptionAbs cons, GChart chart)
    {
        GBox b = (GBox)text;

        RESOURCE res = rr.resource;

        b.title(res.name);
        b.add(text.text().add(¤¤Consumption).s().add('(').add(Dic.¤¤Total).add(')'));
        b.NL(4);

        b.textLL(¤¤ConsumedDay);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), rr.history().get(0)));
        b.NL();

        b.textLL(¤¤ConsumedYEsterday);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), rr.history().get(1)));
        b.NL();

        int yearStart = (int)(-TIME.days().bitOfYear() * TIME.years().bitConversion(TIME.years()));

        b.textLL(¤¤ConsumedNow);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), rr.history().getPeriodSum(-yearStart, 0)));
        b.NL();

        b.textLL(¤¤ConsumedPrevious);
        b.tab(7);
        b.add(GFORMAT.i(b.text(), rr.history().getPeriodSum(-(int)TIME.years().bitConversion(TIME.days()) - yearStart, -(int)yearStart)));
        b.NL();

        b.NL(8);
        b.textLL(DicTime.¤¤Days);
        chart.clear();
        chart.add(rr.history());
        text.NL();
        text.add(chart.sprite);
    }

    protected override void AppendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
    {
    }

    protected override void hover(GBox box, A ins)
    {
        base.hover(box, ins);
        int t = 0;
        foreach (IndustryResource i in cons.ins())
        {
            box.tab(t * 3);
            box.add(i.resource.icon().small);
            t++;
            if (t == 3)
            {
                t = 0;
                box.NL();
            }
        }

        box.NL(8);
    }
}