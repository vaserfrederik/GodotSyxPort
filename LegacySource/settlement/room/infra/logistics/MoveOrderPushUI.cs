using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.text;
using view.main;
using view.tool;

namespace settlement.room.infra.logistics
{
    public class MoveOrderPushUI : GuiSection
    {
        private static readonly CharSequence ¤¤notSet = "not set";
        private static readonly CharSequence ¤¤notSetC = "Click to set a storage site to push to.";
        private static readonly CharSequence ¤¤setC = "Click to go to push destination.";
        private static readonly CharSequence ¤¤issue = "Click to issue a push order, which will deliver resources to another storage site.";
        private static readonly CharSequence ¤¤name = "¤Push Destination";
        private static readonly CharSequence ¤¤Choose = "¤Choose a storage site to push to.";
        private static readonly CharSequence ¤¤NoneSeclected = "¤No push destinations have been set.";
        private static readonly CharSequence ¤¤Limit = "¤Push Limit";
        private static readonly CharSequence ¤¤LimitD = "¤Only push when the destination storage utilization is below this limit.";

        private readonly GETTER<? extends MoveOrderPushInstance> source;
        private readonly GETTER<? extends RoomInstance> room;

        static
        {
            D.ts(typeof(MoveOrderPushUI));
        }

        public MoveOrderPushUI(GETTER<? extends MoveOrderPushInstance> source, GETTER<? extends RoomInstance> room, int orderAm)
        {
            this.source = source;
            this.room = room;
            Placer placer = new Placer();
            Detail popup = new Detail(placer);

            for (int i = 0; i < orderAm; i++)
            {
                int oi = i;

                CLICKABLE c = new CLICKABLE.ClickableAbs(48, 48)
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                        if (source.get().moveOrdersPush()[oi] == null)
                        {
                            UI.icons().m.storage_push.renderC(r, body.cX(), body.cY());
                            GButt.ButtPanel.renderFrame(r, body);
                            return;
                        }

                        MoveOrderPush o = source.get().moveOrdersPush()[oi];

                        if (isHovered && o.destI() != null)
                        {
                            SETT.OVERLAY().add(o.destI().mX(), o.destI().mY());
                        }

                        if (o.problem(source.get()) != null)
                        {
                            GCOLOR.UI().BAD.hovered.bind();
                        }
                        else if (o.warning(source.get()) != null)
                        {
                            GCOLOR.UI().SOSO.hovered.bind();
                        }
                        else
                        {
                            GCOLOR.UI().GOOD.hovered.bind();
                        }
                        UI.icons().s.alert.renderC(r, body.cX(), body.cY());
                        COLOR.unbind();
                        GButt.ButtPanel.renderFrame(r, body);
                    }

                    protected override void clickA()
                    {
                        if (source.get().moveOrdersPush()[oi] == null)
                        {
                            placer.activate(oi);
                        }
                        else
                        {
                            VIEW.inters().popup.show(popup.get(oi), this);
                        }
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(¤¤name);
                        MoveOrderPush o = source.get().moveOrdersPush()[oi];
                        if (o == null)
                        {
                            b.text(¤¤issue);
                            return;
                        }
                        else
                        {
                            b.textLL(¤¤name);
                            b.tab(6);
                            if (o.destI() == null)
                            {
                                b.error(¤¤notSet);
                            }
                            else
                            {
                                b.text(o.destI().name());
                            }
                            b.NL();

                            if (o.problem(source.get()) != null)
                                b.error(o.problem(source.get()));
                            else if (o.warning(source.get()) != null)
                                b.add(b.text().warnify().add(o.warning(source.get())));
                        }
                    }
                };
                addGrid(c, i, 4, 0, 0);
            }
        }

        private class Detail
        {
            private MoveOrderPush o;
            private readonly GuiSection section = new GuiSection();
            private int oi;

            public Detail(Placer placer)
            {
                {
                    section.add(new HOVERABLE.Sprite(placer.getIcon()).hoverInfoSet(placer.name()), 0, section.body().y2() + 4);
                    section.addRightCAbs(48, new CLICKABLE.ClickableAbs(200, 32)
                    {
                        private readonly GText t = new GText(UI.FONT().S, 24);

                        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                        {
                            GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                            t.setMaxWidth(180);
                            t.setMultipleLines(false);
                            t.clear();
                            t.normalify();
                            if (o.dest() == null)
                            {
                                t.add(¤¤notSet);
                                t.errorify();
                            }
                            else
                            {
                                t.add(o.dest().name());
                            }
                            t.render(r, body.x(), body.y());
                            GButt.ButtPanel.renderFrame(r, body);
                        }

                        protected override void clickA()
                        {
                            if (o.destI() != null)
                            {
                                VIEW.s().ui.rooms.open(o.destI());
                            }
                        }

                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            if (o.destI() != null)
                            {
                                GBox b = (GBox)text;
                                VIEW.s().ui.rooms.hover(b, o.destI(), hx, hy);
                            }
                        }
                    });
                }

                {
                    RENDEROBJ ro = new RENDEROBJ(new Renderable.RenderCallback((g, x, y, w, h) =>
                    {
                        if (o.destI() != null)
                        {
                            GBox b = new GBox(g);
                            b.setPosition(x, y);
                            b.setSize(w, h);
                            b.setPadding(5);
                            b.setOutline(new Color(0, 0, 0));
                            b.setBackground(new Color(255, 255, 255, 128));
                            b.addText(o.destI().name(), UI.FONT().M);
                            foreach (KeyValuePair<init, int> kvp in o.destI().storage())
                            {
                                b.addText($"{kvp.Key.name()}: {kvp.Value}", UI.FONT().S);
                            }
                        }
                    }));
                    section.add(ro, 0, section.body().h());
                }

                {
                    INum<int> slider = new INum<int>(0, 100, 50);
                    slider.addChangedCallback((oldValue, newValue) =>
                    {
                        o.limitSet(newValue);
                    });

                    GSlider sl = new GSlider(slider);
                    sl.setLabel(¤¤Limit);
                    section.add(sl, 0, section.body().h() + 5);
                }

                {
                    GButt.ButtPanel removeButton = new GButt.ButtPanel(Dic.¤¤remove);
                    removeButton.addChangedCallback(() =>
                    {
                        source.get().moveOrdersPush()[oi] = null;
                        VIEW.inters().popup.close();
                    });
                    section.add(removeButton, 0, section.body().h() + 10);
                }
            }

            public GuiSection get(int oi)
            {
                this.oi = oi;
                this.o = source.get().moveOrdersPush()[oi];
                return section;
            }
        }

        public class Placer : PlacableSingle
        {
            private Room hov;
            private int hx, hy;

            private int ii;

            public Placer() : base(¤¤name)
            {
            }

            public void activate(int ii)
            {
                this.ii = ii;
                VIEW.s().tools.place(this);
            }

            public override void placeFirst(int tx, int ty)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                VIEW.s().tools.place(null);

                if (source.get().moveOrdersPush()[ii] == null)
                    source.get().moveOrdersPush()[ii] = new MoveOrderPush((RoomInstance)r);
                else
                    source.get().moveOrdersPush()[ii].destSet((RoomInstance)r);
                VIEW.s().ui.rooms.open(room.get());
            }

            public override CharSequence isPlacable(int tx, int ty)
            {
                if (tx == VIEW.s().getWindow().tile().x() && ty == VIEW.s().getWindow().tile().y())
                {
                    SCompPath pp = SETT.PATH().comps.pather.findDest(room.get().mX(), room.get().mY(), tx, ty);
                    if (pp == null)
                        return Dic.¤¤Unreachable;
//                    if (pp.distance() > source.get().moveMaxRadius())
//                        return ¤¤TooFar;
                }

                return pp(tx, ty);
            }

            private CharSequence pp(int tx, int ty)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                hov = null;
                if (r != null && r != source.get() && r is MoveJob.ROOM_MOVE_DEST)
                {
                    hov = r;
                    hx = tx;
                    hy = ty;
                    return null;
                }
                return ¤¤Choose;
            }

            public override void placeInfo(GBox b, int tiles)
            {
                if (hov != null)
                {
                    VIEW.s().ui.rooms.hover(b, hov, hx, hy);
                    hov = null;
                }
            }

            public override SPRITE getIcon()
            {
                return UI.icons().m.crossair;
            }

            public override bool expandsTo(int fromX, int fromY, int toX, int toY)
            {
                if (pp(fromX, fromY) == null && SETT.ROOMS().map.get(fromX, fromY) == SETT.ROOMS().map.get(toX, toY))
                    return true;
                return false;
            }
        }

        public static CharSequence problem(MoveOrderPushInstance i)
        {
            bool isOk = false;
            bool has = false;
            CharSequence prob = null;
            foreach (MoveOrderPush o in i.moveOrdersPush())
                if (o != null)
                {
                    has = true;
                    CharSequence p = o.problem(i);
                    if (p != null)
                    {
                        prob = p;
                    }
                    else
                        isOk = true;
                }

            if (!isOk)
            {
                return prob;
            }
            if (!has)
                return ¤¤NoneSeclected;

            return null;
        }
    }
}