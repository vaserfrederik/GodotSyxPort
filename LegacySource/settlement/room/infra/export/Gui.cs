using System;
using System.Collections.Generic;
using System.Linq;
using static settlement.room.infra.logistics.MoveDic;

namespace settlement.room.infra.export
{
    class Gui : UIRoomModuleImp<ExportInstance, ROOM_EXPORT>
    {
        private static readonly string ¤¤NoResource = "¤No resource has been selected for export.";
        private static readonly string ¤¤prioProb = "¤Globally, there is not enough stored goods to fetch from the un-prioritized storage rooms in the vicinity. Increase the global priority fetch limit.";
        private static readonly string ¤¤prio = "¤Workers will only fetch as long as the condition for the fetch limit below is met.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_EXPORT s) : base(s)
        {
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<ExportInstance> g, int x1, int y1)
        {
            {
                var pop = new UIPickerRes(true)
                {
                    Select = (r, li) =>
                    {
                        g.Get().resourceSet(r);
                        VIEW.inters().popup.close();
                    },
                    GetResource = () => g.Get().resource(),
                    HoverResource = (res, b) =>
                    {
                        FACTIONS.player().seller(TR.get(res)).hover(b);
                    }
                };

                var s = new GuiSection();
                var la = new SPRITE.Imp(Icon.M)
                {
                    Render = (r, X1, X2, Y1, Y2) =>
                    {
                        if (g.Get().resource() != null)
                            g.Get().resource().icon().render(r, X1, X2, Y1, Y2);
                        else
                            SPRITES.icons().m.questionmark.render(r, X1, X2, Y1, Y2);
                    }
                };

                var b = new GButt.ButtPanel(la)
                {
                    ClickA = () => VIEW.inters().popup.show(pop, this, true)
                };
                b.body.setDim(48);

                s.add(new GHeader(Dic.¤¤Exporting));
                s.addRelBody(6, DIR.E, b);

                RENDEROBJ r = null;
                r = new GStat()
                {
                    Update = text => GFORMAT.iofk(text, g.Get().amount, g.Get().crates * ExportInstance.crateMax),
                    HoverInfoGet = b =>
                    {
                        b.textLL(Dic.¤¤Inbound);
                        b.add(GFORMAT.i(b.text(), g.Get().spaceReserved));
                        b.NL();
                        b.textLL(Dic.¤¤Outbound);
                        b.add(GFORMAT.i(b.text(), g.Get().amountReserved));

                        b.sep();
                        if (g.Get().resource() != null)
                        {
                            b.textLL(Dic.¤¤Total);
                            b.NL(8);

                            b.textLL(Dic.¤¤Stored);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), blueprint.tally.amount.get(g.Get().resource())));
                            b.NL();
                            b.textLL(Dic.¤¤Capacity);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), blueprint.tally.capacity.get(g.Get().resource())));
                            b.NL();
                        }
                    }
                }.hv(Dic.¤¤Stored);
                s.addRightC(32, r);

                r = new GStat()
                {
                    Update = text => GFORMAT.i(text, g.Get().amountReserved),
                }.hv(Dic.¤¤Sold);
                s.addRightC(64, r);

                section.addRelBody(2, DIR.S, s);
            }

            {
                var s = new GuiSection();

                {
                    var p = new GButt.ButtPanel(UI.icons().m.wheel)
                    {
                        RenAction = () => selectedSet(g.Get().fetching()),
                        ClickA = () => g.Get().fetchingSet(!g.Get().fetching()),
                        Render = (r, ds, isActive, isSelected, isHovered) =>
                        {
                            base.render(r, ds, isActive, isSelected, isHovered);
                            if (g.Get().fetching() && g.Get().coolFetch > -1)
                            {
                                GCOLOR.fetchProblem.render(r, X1, X2, Y1, Y2);
                            }
                        },
                        HoverInfoGet = b =>
                        {
                            b.textLL(¤¤prio);
                        }
                    };
                    p.body.setDim(48);
                    s.addRightC(0, p);

                    p = new GButt.ButtPanel(UI.icons().m.wheel)
                    {
                        RenAction = () => selectedSet(g.Get().fetching()),
                        ClickA = () => g.Get().fetchingSet(!g.Get().fetching()),
                        Render = (r, ds, isActive, isSelected, isHovered) =>
                        {
                            base.render(r, ds, isActive, isSelected, isHovered);
                            if (g.Get().fetching() && g.Get().coolFetch > -1)
                            {
                                GCOLOR.fetchProblem.render(r, X1, X2, Y1, Y2);
                            }
                        },
                        HoverInfoGet = b =>
                        {
                            b.textLL(¤¤prioProb);
                        }
                    };
                    p.body.setDim(48);
                    s.addRightC(0, p);
                }

                s.addRightC(8, new MoveOrderPullUI(g, g, null, ExportInstance.ORDERS));
                section.addRelBody(2, DIR.S, s);
            }

            {
                var ex = new UIGoodsExport(false);
                var s = new ClickWrap(ex)
                {
                    Pget = () =>
                    {
                        if (g.Get().resource() == null)
                            return null;
                        ex.res.set(g.Get().resource().tr());
                        return ex;
                    }
                };
                section.addRelBody(2, DIR.S, s);
            }
        }

        protected override void appendTableButt(GuiSection s, GETTER<RoomInstance> ins)
        {
            s.add(new SPRITE.Imp(Icon.S)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    RESOURCE ro = ((ExportInstance)ins.Get()).resource();
                    SPRITE s = ro == null ? SPRITES.icons().s.cancel : ro.icon().small;
                    s.render(r, X1, Y1);
                }
            }, 0, s.body().y2());

            s.addRightC(8, new SPRITE.Imp(s.body().width() - 8 - s.getLastX2(), 12)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    ExportInstance in = (ExportInstance)ins.Get();

                    double t = in.crates * ExportInstance.crateMax;
                    double n = in.amount;
                    double i = in.amountReserved;
                    GMeter.renderDelta(r, (n - i) / t, (n) / 2, X1, X2, Y1, Y2);
                }
            });
        }

        protected override void problem(ExportInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            if (i.resource() == null)
            {
                errors.add(¤¤NoResource);
            }
        }

        protected override void hover(GBox box, ExportInstance i)
        {
            if (i.resource() != null)
            {
                box.setResource(i.resource(), i.amount, i.crates * ExportInstance.crateMax);
            }
        }

        protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
        {
            RENDEROBJ r = null;

            r = new GStat()
            {
                Update = text =>
                {
                    double am = 0;
                    double cap = 0;

                    foreach (RESOURCE r in RESOURCES.ALL())
                    {
                        am += blueprint.tally.amount.get(r);
                        cap += blueprint.tally.capacity.get(r);
                    }
                    GFORMAT.percInv(text, am / cap);
                }
            }.hh(Dic.¤¤Capacity);
            text.add(r);

            r = new GStat()
            {
                Update = text =>
                {
                    int am = 0;
                    foreach (RESOURCE r in RESOURCES.ALL())
                    {
                        am += blueprint.tally.amount.get(r);
                    }
                    GFORMAT.i(text, am);
                }
            }.hh(Dic.¤¤Stored);
            text.add(r);

            r = new GStat()
            {
                Update = text =>
                {
                    int am = 0;
                    foreach (RESOURCE r in RESOURCES.ALL())
                    {
                        am += SETT.HALFENTS().caravans.withdrawals(r, null);
                    }
                    GFORMAT.i(text, am);
                }
            }.hh(Dic.¤¤Outbound);
            text.add(r);
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
            var none = "--";
            var s = new GTSort<RoomInstance>(Dic.¤¤Resource)
            {
                Cmp = (current, cmp) => Dictionary.compare(name(current), name(cmp)),
                Format = (h, text) => text.add(name(h)),
                Name = ins =>
                {
                    if (ins != null && ins is ExportInstance)
                    {
                        var i = (ExportInstance)ins;
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
}