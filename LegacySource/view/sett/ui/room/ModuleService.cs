using System;
using System.Collections.Generic;
using settlement.main;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    internal sealed class ModuleService : ModuleMaker
    {
        private static readonly CharSequence ¤¤NO = "¤No Available Services";
        private static readonly CharSequence ¤¤AVAILABLE = "¤Available Services";
        private static readonly CharSequence ¤¤USED = "¤Currently Used";
        private static readonly CharSequence ¤¤NEEDS = "¤Needs Work";
        private static readonly CharSequence ¤¤TOTAL = "¤Total";
        private static readonly CharSequence ¤¤QUALITY = "¤Quality";
        private static readonly CharSequence ¤¤ACCESS = "¤The overall access of your minions";
        private static readonly CharSequence ¤¤USAGE = "¤Service";
        private static readonly CharSequence ¤¤Load = "¤Load";
        private static readonly CharSequence ¤¤Capacity = "¤Capacity";
        private static readonly CharSequence ¤¤CapacityD = "¤An rough estimate of how many subjects that can be served.";

        private static readonly CharSequence ¤¤USAGE_DESC = "¤The highest load of this service during a day. Once full, it means there aren't enough services to meet your subjects demands. If low, it's an indication you can cut down on this service.";
        private static readonly CharSequence ¤¤RADIUS = "¤Radius";
        private static readonly CharSequence ¤¤RADIUSD = "¤All services operate within a radius. The radius is the max distance a subject is prepared to walk to get to a service.";

        static
        {
            D.ts(typeof(ModuleService));
        }

        public ModuleService(Init init)
        {
        }

        public override void Make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is ROOM_SERVICE_HASER)
                l.Add(new I((ROOM_SERVICE_HASER)p));
        }

        private sealed class I : UIRoomModule
        {
            private readonly ROOM_SERVICE_HASER p;

            I(ROOM_SERVICE_HASER p)
            {
                this.p = p;
            }

            public override void AppendManageScr(GGrid grid, GGrid text, GuiSection sExta)
            {
                if (p is ROOM_SPECTATOR_HASER)
                {
                }
                else
                {
                    SPRITE s = new SPRITE.Imp(58, 14)
                    {
                        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            double d = 1.0 - p.Service().Load();
                            GMeter.Render(r, GMeter.C_REDGREEN, d, X1, X2, Y1, Y2);
                        }
                    };

                    RENDEROBJ h = new GHeader.HeaderHorizontal(SPRITES.icons().s.citizen, s)
                    {
                        public override void HoverInfoGet(GUI_BOX text)
                        {
                            GBox b = (GBox)text;
                            b.Title(¤¤USAGE);

                            b.TextLL(¤¤Load);
                            b.Tab(6);
                            b.Add(GFORMAT.PercInv(b.Text(), p.Service().Load()));
                            b.NL();
                            b.Text(¤¤USAGE_DESC);
                            b.NL(8);

                            b.TextL(¤¤AVAILABLE);
                            b.Add(GFORMAT.I(b.Text(), p.Service().Available()));
                            b.NL();
                            b.TextL(¤¤TOTAL);
                            b.Add(GFORMAT.I(b.Text(), p.Service().Total()));

                            b.NL(8);
                            b.TextLL(¤¤Capacity);
                            b.Tab(6);
                            b.Add(GFORMAT.I(b.Text(), (int)(p.Service().Total() * p.Service().TotalMultiplier())));
                            b.NL();
                            b.Text(¤¤CapacityD);

                            b.NL(8);
                            b.TextLL(¤¤RADIUS);
                            b.Add(GFORMAT.I(b.Text(), p.Service().Radius));
                            b.NL();
                            b.Text(¤¤RADIUSD);
                        }
                    };

                    grid.Add(h);
                }

                if (p is ROOM_SERVICE_ACCESS_HASER)
                {
                    grid.Add(new GStat
                    {
                        public override void Update(GText text)
                        {
                            RoomServiceAccess a = ((ROOM_SERVICE_ACCESS_HASER)p).Service();
                            GFORMAT.Perc(text, a.CityAccess());
                        }
                    }.Hh(SPRITES.icons().s.arrowUp).HoverInfoSet(¤¤ACCESS));
                }
            }

            public override void AppendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
            {
            }

            public override void AppendButt(GuiSection s, GETTER<RoomInstance> ins)
            {
                DOUBLE d = new DOUBLE
                {
                    public override double GetD()
                    {
                        return 1.0 - ((ROOM_SERVICER)ins.Get()).Service().Load();
                    }
                };

                s.AddRelBody(16, DIR.E, SPRITES.icons().s.human);

                s.AddRightC(2, new GMeter.GMeterSprite(GMeter.C_REDGREEN, d, 48, 12));
            }

            public override void Hover(GBox box, Room room, int rx, int ry)
            {
                ROOM_SERVICER i = (ROOM_SERVICER)room;
                box.TextL(¤¤AVAILABLE).Add(GFORMAT.IofkInv(box.Text(), i.Service().Available(), i.Service().Total()));
                box.NL();
                if (p is ROOM_SPECTATOR_HASER)
                    return;
                box.TextL(¤¤Load);
                box.Add(GFORMAT.PercInv(box.Text(), i.Service().Load()));
            }

            public override void Problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry)
            {
                if (((ROOM_SERVICER)room).Service().Available() == 0)
                    errors.Add(¤¤NO);
            }

            public override void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                SPRITE s = new SPRITE.Imp(48, 16)
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        if (get.Get().BlueprintI() is ROOM_SERVICE_HASER)
                        {
                            SETT.OVERLAY().Service((ROOM_SERVICE_HASER)(get.Get().BlueprintI()));
                        }

                        double d = 1.0 - g(get).Service().Load();
                        GMeter.Render(r, GMeter.C_REDGREEN, d, X1, X2, Y1, Y2);
                    }
                };

                RENDEROBJ r = new GHeader.HeaderHorizontal(¤¤USAGE, s)
                {
                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        RoomServiceInstance i = g(get).Service();
                        GBox b = (GBox)text;

                        b.TextLL(¤¤Load);
                        b.Tab(6);
                        b.Add(GFORMAT.Perc(b.Text(), g(get).Service().Load()));
                        b.NL();
                        text.Text(¤¤USAGE_DESC);
                        text.NL(8);

                        b.TextL(¤¤AVAILABLE);
                        b.Tab(6);
                        text.Add(GFORMAT.I(b.Text(), i.Available()));
                        b.NL();
                        b.TextL(¤¤USED);
                        b.Tab(6);
                        text.Add(GFORMAT.I(b.Text(), i.Total() - i.Available()));
                        b.NL();

                        if (get.Get() is RoomInstance && ((RoomInstance)get.Get()).BlueprintI().Employment() != null)
                        {
                            b.TextL(¤¤NEEDS);
                            b.Tab(6);
                            text.Add(GFORMAT.I(b.Text(), i.Total() - (i.Available() - i.Reserved())));
                            b.NL();
                        }

                        b.TextL(¤¤TOTAL);
                        b.Tab(6);
                        text.Add(GFORMAT.I(b.Text(), i.Total()));
                        b.NL(8);

                        b.TextL(¤¤QUALITY);
                        b.Tab(6);
                        text.Add(GFORMAT.Perc(b.Text(), g(get).Quality()));
                        b.NL(8);

                        b.TextLL(¤¤Capacity);
                        b.Tab(6);
                        text.Add(GFORMAT.I(b.Text(), (int)(i.Total() * p.Service().TotalMultiplier())));
                        b.NL();
                        b.Text(¤¤CapacityD);

                        b.NL(8);
                        b.TextLL(¤¤RADIUS);
                        b.Add(GFORMAT.I(b.Text(), p.Service().Radius));
                        b.NL();
                        b.Text(¤¤RADIUSD);
                    }
                };

                section.AddRelBody(8, DIR.S, r);
            }

            private ROOM_SERVICER g(GETTER<RoomInstance> g)
            {
                return (ROOM_SERVICER)g.Get();
            }
        }
    }
}