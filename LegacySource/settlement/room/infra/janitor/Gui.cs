using System;
using System.Collections.Generic;
using init.resources;
using init.settings;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.infra.janitor
{
    class Gui : UIRoomModuleImp<JanitorInstance, ROOM_JANITOR>
    {
        private static string ¤¤Resources = "Stored Resources";
        private static string ¤¤Global = "Daily global consumption estimate.";
        private static string ¤¤Bad = "This resource can not be reached by this room, and maintenance requiring it can not be performed.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_JANITOR s) : base(s)
        {
        }

        public override void hover(GBox box, JanitorInstance i)
        {
            base.hover(box, i);
        }

        private int maxAm = 0;

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<JanitorInstance> getter, int x1, int y1)
        {
            int rows = 4;

            RESOURCE[] resourceI = new RESOURCE[RESOURCES.ALL().Size()];

            GuiSection s = new GuiSection()
            {
                public void render(SPRITE_RENDERER r, float ds)
                {
                    maxAm = 0;
                    for (RESOURCE res : RESOURCES.ALL())
                    {
                        resourceI[res.index()] = null;
                    }
                    foreach (RESOURCE res in RESOURCES.ALL())
                    {
                        if (SETT.MAINTENANCE().estimateGlobal(res) > 0)
                        {
                            resourceI[maxAm] = res;
                            maxAm++;
                        }
                    }
                    SETT.OVERLAY().MAINTENANCE.add(getter.get());
                    base.render(r, ds);
                }
            };

            GTableBuilder b = new GTableBuilder()
            {
                public int nrOFEntries()
                {
                    return (int)Math.Ceiling((double)maxAm / rows);
                }
            };

            int width = 80;

            for (int off = 0; off < 4; off++)
            {
                int k = off;
                b.column(null, width, new GRowBuilder()
                {
                    public RENDEROBJ build(GETTER<int> ier)
                    {
                        return new Res(width, resourceI, ier, k, getter);
                    }
                });
            }

            s.add(new GHeader(¤¤Resources));
            s.addRelBody(8, DIR.E, ModuleIndustry.makeFetch(getter));
            s.body().incrW(48);

            s.addRelBody(8, DIR.S, b.create(5, false));

            section.addRelBody(8, DIR.S, s);
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }

        private class Res : HoverableAbs
        {
            private readonly RESOURCE[] resourceI;
            private readonly GETTER<int> ier;
            private readonly int off;
            private readonly GETTER<JanitorInstance> getter;

            public Res(int width, RESOURCE[] resourceI, GETTER<int> ier, int off, GETTER<JanitorInstance> getter) : base(width, 32)
            {
                this.resourceI = resourceI;
                this.ier = ier;
                this.off = off;
                this.getter = getter;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                RESOURCE res = resourceI[ier.get() * 4 + off];
                if (res == null)
                    return;

                GButt.ButtPanel.renderBG(r, true, false, isHovered, body);

                res.icon().renderCY(r, body().x1() + 8, body().cY());

                Str.TMP.clear();
                Str.TMP.add(getter.get().bits.resAm(res));
                if (getter.get().bits.resMissing(res))
                    GCOLOR.T().IBAD.bind();

                UI.FONT().S.renderCY(r, body.x1() + 40, body().cY(), Str.TMP);
                COLOR.unbind();

                GButt.ButtPanel.renderFrame(r, body);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                RESOURCE res = resourceI[ier.get() * 4 + off];
                if (res == null)
                    return;
                GBox b = (GBox)text;
                b.title(res.name);
                b.textLL(¤¤Global);
                b.NL();
                b.add(GFORMAT.f0(b.text(), -SETT.MAINTENANCE().estimateGlobal(res)));
                b.NL();
                b.NL(8);

                if (getter.get().bits.resMissing(res))
                    b.add(b.text().warnify().add(¤¤Bad));
                b.NL();

                if (S.get().developer)
                {
                    getter.get().bits.hover(b, res, getter.get());
                    b.NL();
                }

                base.hoverInfoGet(text);
            }
        }
    }
}