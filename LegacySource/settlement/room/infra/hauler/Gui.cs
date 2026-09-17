using settlement.room.infra.hauler;
using settlement.room.infra.logistics;
using init.resources;
using init.sprite.UI;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.common;
using util.gui.misc;
using util.gui.table;
using view.sett.ui.room;
using util.info;
using System;
using System.Collections.Generic;

class Gui : UIRoomModuleImp<HaulerInstance, ROOM_HAULER>
{
    public Gui(ROOM_HAULER s) : base(s)
    {
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GETTER<HaulerInstance> g, int x1, int y1)
    {
        {
            GuiSection s = new GuiSection();
            int i = 0;

            foreach (TallyData d in blueprint.tally.datas)
            {
                s.addGridD(new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.i(text, d.get(g.get()));
                    }
                }.hv(d.name), i++, 2, 160, 32, DIR.N);
            }
            section.addRelBody(8, DIR.S, s);
        }

        {
            GuiSection s = new GuiSection();

            {
                GButt.ButtPanel p = new GButt.ButtPanel(UI.icons().m.wheel)
                {
                    protected override void renAction()
                    {
                        selectedSet(g.get().fetching());
                    }

                    protected override void clickA()
                    {
                        g.get().fetchingSet(!g.get().fetching());
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        base.render(r, ds, isActive, isSelected, isHovered);
                        if (g.get().fetching() && g.get().coolFetch > -1)
                        {
                            GCOLOR.UI().SOSO.hovered.bind();
                            UI.icons().s.alert.render(r, body.x1() + 6, body.y1() + 6);
                            COLOR.unbind();
                        }
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(¤¤fetch);
                        b.text(¤¤fetchD);
                        b.NL();
                        if (g.get().fetching() && g.get().coolFetch > -1)
                        {
                            b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
                        }
                        base.hoverInfoGet(text);
                    }
                };

                p.body.setDim(48);
                s.addRightC(0, p);

                p = new GButt.ButtPanel(UI.icons().m.priority)
                {
                    protected override void renAction()
                    {
                        selectedSet(g.get().prio());
                    }

                    protected override void clickA()
                    {
                        g.get().prioSet();
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        base.render(r, ds, isActive, isSelected, isHovered);
                        if (g.get().prio() && g.get().coolFetch > -1)
                        {
                            GCOLOR.UI().SOSO.hovered.bind();
                            UI.icons().s.alert.render(r, body.x1() + 6, body.y1() + 6);
                            COLOR.unbind();
                        }
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(MoveDic.¤¤prio);
                        b.text(MoveDic.¤¤prioD);
                        b.NL();
                        if (g.get().prio() && g.get().coolFetch > -1)
                        {
                            b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
                        }
                    }
                };

                p.body.setDim(48);
                s.addRightC(0, p);
            }

            MoveOrderPullUI ui = new MoveOrderPullUI(g, g, null, HaulerInstance.ORDERS);
            s.addRightC(8, ui);
            s.addRightC(0, new MoveOrderPullersUI(g));

            {
                GButt.ButtPanel p = new GButt.ButtPanel(UI.icons().m.lock)
                {
                    protected override void renAction()
                    {
                        selectedSet(g.get().storing());
                    }

                    protected override void clickA()
                    {
                        g.get().storingSet(!g.get().storing());
                    }
                };
                p.hoverTitleSet(¤¤storing);
                p.hoverInfoSet(¤¤storingD);
                p.body.setDim(48);
                s.addRightC(8, p);
            }

            section.addRelBody(4, DIR.S, s);
        }

        section.addRelBody(4, DIR.S, new RENDEROBJ.RenderImp(1, 8)
        {
            public override void render(SPRITE_RENDERER r, float ds)
            {
                GCOLOR.UI().border().render(r, section.body().x1() + 8, section.body().x2() - 8, body.y1() + 4, body.y1() + 5);
            }
        });

        section.addRelBody(8, DIR.S, new UIPickerRes(true)
        {
            protected override void select(RESOURCE r, int li)
            {
                g.get().setResource(r);
            }

            protected override RESOURCE getResource()
            {
                return g.get().resource();
            }
        });
    }

    protected override void appendTableButt(GuiSection s, GETTER<RoomInstance> ins)
    {
        s.add(new SPRITE.Imp(Icon.M)
        {
            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                HaulerInstance inInst = (HaulerInstance)ins.get();
                RESOURCE res = inInst.resource();
                SPRITE ico = res == null ? UI.icons().m.cancel : res.icon();
                ico.render(r, X1, X2, Y1, Y2);
            }
        }, 0, s.body().y2());
    }

    protected override void appendMain(GGrid icons, GGrid text, GuiSection sExtra)
    {
        foreach (TallyData d in blueprint.tally.datas)
        {
            text.add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, d.total(null));
                }
            }.hh(d.name));
        }
    }

    protected override void problem(HaulerInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
    {
        if (i.employees().target() == 0)
            return;
        bool ok = false;
        bool has = false;
        foreach (MoveOrderPull o in i.moveOrdersPull())
        {
            if (o != null)
            {
                has = true;
                CharSequence p = o.problem(i);
                if (p != null)
                {
                    errors.add(p);
                    break;
                }
                else
                {
                    ok = true;
                }
            }
        }

        if (i.fetching() && i.employees().target() > 0 && i.coolFetch > -1 && i.coolOrganize > -1 && (has && !ok))
        {
            errors.add(MoveDic.¤¤pullProblem);
        }

        base.problem(i, free, errors, warnings);
    }

    protected override void hover(GBox box, HaulerInstance i)
    {
        base.hover(box, i);
        box.sep();
        if (i.fetching() && i.employees().target() > 0)
        {
            box.textL(¤¤fetching);
            box.NL();
            if (i.coolFetch > -1)
            {
                box.add(box.text().warnify().add(MoveDic.¤¤fetchProblem));
                box.NL();
            }
            foreach (MoveOrderPull o in i.moveOrdersPull())
                if (o != null)
                {
                    CharSequence p = o.warning(i);
                    if (p != null)
                    {
                        box.add(box.text().warnify().add(p));
                        box.NL();
                    }
                }
        }
        if (i.storing())
        {
            box.add(box.text().warnify().add(¤¤storing));
        }
        box.NL(8);

        if (i.resource() != null)
        {
            box.add(i.resource().icon());
            box.NL();
            foreach (TallyData d in blueprint.tally.datas)
            {
                box.textLL(d.name);
                box.tab(7);
                box.add(GFORMAT.i(box.text(), d.get(i)));
                box.NL();
            }
        }
    }

    protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
    {
        base.appendTableFilters(filters, sorts, appliers);

        foreach (RESOURCE res in RESOURCES.ALL())
        {
            filters.add(new GTFilter<RoomInstance>(res.names)
            {
                public override bool passes(RoomInstance h)
                {
                    HaulerInstance i = (HaulerInstance)h;
                    if (i.resource() == res)
                        return true;
                    return false;
                }
            });
        }
    }
}