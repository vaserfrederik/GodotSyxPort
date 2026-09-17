using System;
using System.Collections.Generic;
using settlement.room.infra.importt;
using game.faction;
using init.resources;
using init.settings;
using init.sprite;
using init.trade;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable.CLICKABLE.ClickWrap;
using snake2d.util.gui.renderable.RENDEROBJ;
using snake2d.util.misc.Dictionary;
using snake2d.util.sets.LISTE;
using snake2d.util.sprite.SPRITE;
using util.data.GETTER;
using util.gui.common.UIPickerRes;
using util.gui.misc.GBox;
using util.gui.misc.GButt;
using util.gui.misc.GGrid;
using util.gui.misc.GHeader;
using util.gui.misc.GMeter;
using util.gui.misc.GStat;
using util.gui.misc.GText;
using util.gui.table.GTableSorter.GTFilter;
using util.gui.table.GTableSorter.GTSort;
using util.info.GFORMAT;
using util.text.D;
using util.text.Dic;
using view.main;
using view.sett.ui.room.UIRoomBulkApplier;
using view.sett.ui.room.UIRoomModule;
using view.ui.goods;

class Gui : UIRoomModuleImp<ImportInstance, ROOM_IMPORT>
{
    private static readonly CharSequence ¤¤TotalSpace = "¤Total Space";
    private static readonly CharSequence ¤¤UsedSpace = "¤Used Space";
    private static readonly CharSequence ¤¤Incoming = "¤Incoming Wares";
    private static readonly CharSequence ¤¤Accepting = "¤Accepting";

    static
    {
        D.ts(typeof(Gui));
    }

    public Gui(ROOM_IMPORT s) : base(s)
    {
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GETTER<ImportInstance> g, int x1, int y1)
    {
        RENDEROBJ r = null;

        grid = new GGrid(section, section.body().width() + 100, 2, 0, section.getLastY2() + 8);

        r = new GStat
        {
            update = text =>
            {
                GFORMAT.i(text, g.get().capacity());
            }
        }.hh(¤¤TotalSpace);
        grid.add(r);

        r = new GStat
        {
            update = text =>
            {
                int am = g.get().amount();
                GFORMAT.i(text, am);
            }
        }.hh(¤¤UsedSpace);
        grid.add(r);

        r = new GStat
        {
            update = text =>
            {
                GFORMAT.i(text, g.get().spaceReserved());
            }
        }.hh(¤¤Incoming);
        grid.add(r);

        if (S.get().developer)
        {
            r = new GStat
            {
                update = text =>
                {
                    ImportInstance i = g.get();
                    int res = 0;
                    foreach (COORDINATE c in i.body())
                    {
                        if (i.is(c))
                        {
                            res += blueprint.UNLOADER.reserved(i.resource(), c);
                        }
                    }

                    GFORMAT.iBig(text, res);
                }
            }.hh("reserved crates");
            grid.add(r);

            r = new GStat
            {
                update = text =>
                {
                    ImportInstance i = g.get();
                    RESOURCE r = i.resource();
                    if (r != null)
                    {
                        GFORMAT.iBig(text, blueprint.tally.amount.get(r));
                    }
                }
            }.hh("t amount");
            grid.add(r);

            r = new GStat
            {
                update = text =>
                {
                    ImportInstance i = g.get();
                    RESOURCE r = i.resource();
                    if (r != null)
                    {
                        GFORMAT.iBig(text, blueprint.tally.capacity.get(r));
                    }
                }
            }.hh("t capacity");
            grid.add(r);
        }

        section.body().incrW(48);

        section.addRelBody(8, DIR.S, new GHeader(¤¤Accepting));

        final UIPickerRes pop = new UIPickerRes(true)
        {
            select = (r, li) =>
            {
                g.get().allocate(r);
                VIEW.inters().popup.close();
            },
            getResource = () => g.get().resource(),
            hoverResource = (res, b) =>
            {
                FACTIONS.player().buyer(TR.get(res)).hover(b);
            }
        };

        SPRITE la = new SPRITE.Imp(Icon.M)
        {
            render = (r, X1, X2, Y1, Y2) =>
            {
                if (g.get().resource() != null)
                    g.get().resource().icon().render(r, X1, X2, Y1, Y2);
                else
                    SPRITES.icons().m.questionmark.render(r, X1, X2, Y1, Y2);
            }
        };

        section.addRelBody(2, DIR.S, new GButt.ButtPanel(la)
        {
            clickA = () =>
            {
                VIEW.inters().popup.show(pop, this, true);
            }
        });

        {
            UIGoodsImport ex = new UIGoodsImport();
            ClickWrap s = new ClickWrap(ex)
            {
                pget = () =>
                {
                    if (g.get().resource() == null)
                        return null;
                    ex.res.set(g.get().resource().tr());
                    return ex;
                }
            };

            section.addRelBody(8, DIR.S, s);
        }

        // makeTable(g, section, section.getLastY2() + 20);
    }

    protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
    {
        RENDEROBJ r = null;

        r = new GStat
        {
            update = text =>
            {
                int am = 0;
                foreach (RESOURCE r in RESOURCES.ALL())
                    am += blueprint.tally.capacity.get(r);
                GFORMAT.i(text, am);
            }
        }.hh(¤¤TotalSpace);
        text.add(r);

        r = new GStat
        {
            update = text =>
            {
                int am = 0;
                foreach (RESOURCE r in RESOURCES.ALL())
                    am += blueprint.tally.amount.get(r);
                GFORMAT.i(text, am);
            }
        }.hh(¤¤UsedSpace);
        text.add(r);

        r = new GStat
        {
            update = text =>
            {
                int am = 0;
                foreach (RESOURCE r in RESOURCES.ALL())
                    am += SETT.HALFENTS().caravans.deliveries(r, null);
                GFORMAT.i(text, am);
            }
        }.hh(¤¤Incoming);
        text.add(r);
    }

    protected override void appendTableButt(GuiSection s, GETTER<RoomInstance> ins)
    {
        s.add(new SPRITE.Imp(Icon.S)
        {
            render = (r, X1, X2, Y1, Y2) =>
            {
                RESOURCE ro = ((ImportInstance)ins.get()).resource();
                SPRITE s = ro == null ? SPRITES.icons().s.cancel : ro.icon().small;
                s.render(r, X1, Y1);
            }
        }, 0, s.body().y2());

        s.addRightC(8, new SPRITE.Imp(s.body().width() - 8 - s.getLastX2(), 12)
        {
            render = (r, X1, X2, Y1, Y2) =>
            {
                ImportInstance in = (ImportInstance)ins.get();

                double t = in.capacity();
                double n = in.amount();
                double i = in.spaceReserved();
                GMeter.renderDelta(r, n / t, (n + i) / 2, X1, X2, Y1, Y2);
            }
        });
    }

    protected override void hover(GBox box, ImportInstance i)
    {
        base.hover(box, i);
        if (i.resource() != null)
        {
            box.setResource(i.resource(), i.amount(), i.capacity());
        }
    }

    protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
    {
        final CharSequence none = "--";
        GTSort<RoomInstance> s = new GTSort<RoomInstance>(Dic.¤¤Resource)
        {
            cmp = (current, cmp) => Dictionary.compare(name(current), name(cmp)),
            format = (h, text) => text.add(name(h)),
            name = ins =>
            {
                if (ins != null && ins is ImportInstance)
                {
                    ImportInstance i = (ImportInstance)ins;
                    if (i.resource() == null)
                        return none;
                    return i.resource().name;
                }
                return none;
            }
        };
        sorts.add(s);
    }
}