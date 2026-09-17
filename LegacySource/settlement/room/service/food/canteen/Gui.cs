using System;
using System.Collections.Generic;
using settlement.room.service.food.canteen;
using init.resources;
using init.settings;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

class Gui : UIRoomModuleImp<CanteenInstance, ROOM_CANTEEN>
{
    private readonly string ¤¤Food = "¤Meals";

    public Gui(ROOM_CANTEEN s) : base(s)
    {
        D.t(this);
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GETTER<CanteenInstance> g, int x1, int y1)
    {
        GuiSection s = new GuiSection();
        int i = 0;
        foreach (ResG e in RESOURCES.EDI().all())
        {
            GButt.BSection ss = new GButt.BSection
            {
                HoverInfoGet = delegate (GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.title(e.resource.name);
                    b.textLL(¤¤Food).add(GFORMAT.i(b.text(), g.get().amount(e)));
                    if (S.get().developer)
                    {
                        b.NL();
                        b.textL(Dic.¤¤Access);
                        b.add(GFORMAT.i(b.text(), g.get().amountReserved(e)));
                    }
                },
                RenAction = delegate
                {
                    selectedSet(g.get().uses(e));
                },
                ClickA = delegate
                {
                    g.get().usesToggle(e);
                }
            };

            ss.addRightC(4, e.resource.icon());

            ss.addRightC(4, new GStat
            {
                Update = delegate (GText text)
                {
                    GFORMAT.i(text, g.get().amount(e));
                }
            });

            ss.body().incrW(48);
            ss.pad(4);

            s.add(ss, (i % 3) * ss.body().width(), (i / 3) * ss.body().height());
            i++;
        }

        s.addRelBody(2, DIR.N, new GStat
        {
            Update = delegate (GText text)
            {
                GFORMAT.iofk(text, g.get().amountTotal(), g.get().maxAmount * RESOURCES.EDI().all().size());
            }
        }.hh(¤¤Food));

        section.addRelBody(8, DIR.S, s);
    }

    protected override void hover(GBox b, CanteenInstance i)
    {
        b.NL();
        b.textLL(¤¤Food).add(GFORMAT.i(b.text(), i.amountTotal()));
        b.NL();
    }

    protected override void appendMain(GGrid gg, GGrid text, GuiSection sExtra)
    {
        GuiSection s = new GuiSection();
        int i = 0;
        foreach (ResG e in RESOURCES.EDI().all())
        {
            RENDEROBJ r = new GStat
            {
                Update = delegate (GText text)
                {
                    GFORMAT.i(text, blueprint.amount(e));
                },
                HoverInfoGet = delegate (GBox b)
                {
                    b.title(e.resource.name);
                    b.textLL(¤¤Food).add(GFORMAT.i(b.text(), blueprint.amount(e)));
                }
            }.hv(e.resource.icon());

            s.add(r, (i % 4) * 42, (i / 4) * 48);
            i++;
        }

        s.add(new GStat
        {
            Update = delegate (GText text)
            {
                GFORMAT.i(text, blueprint.total);
            }
        }.hh(¤¤Food), 0, s.body().y1() - 16);

        text.add(s);
    }
}