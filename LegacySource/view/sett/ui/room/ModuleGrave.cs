using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.sett.ui.room;

namespace view.sett.ui.room
{
    public class ModuleGrave : ModuleMaker
    {
        public ModuleGrave(Init init)
        {
        }

        public override void Make(RoomBlueprint p, List<UIRoomModule> l)
        {
            if (p is GRAVE_DATA_HOLDER)
            {
                l.Add(new I(((GRAVE_DATA_HOLDER)p).graveData()));
            }
        }

        private class I : UIRoomModule
        {
            private readonly GraveData g;

            public I(GraveData g)
            {
                this.g = g;
            }

            public override void AppendManageScr(GGrid grid, GGrid text, GuiSection sExta)
            {
                text.Add(new GStat
                {
                    Update = (GText t) =>
                    {
                        GFORMAT.iofkInv(t, g.available.Get(null), g.total.Get(null));
                    }
                }.Hh(g.available.Info()));

                text.Add(new GStat
                {
                    Update = (GText t) =>
                    {
                        GFORMAT.perc(t, g.respect.GetD(null));
                    }
                }.Hh(g.respect.Info()));
            }

            public override void AppendTableFilters(List<GTFilter<RoomInstance>> filters, List<GTSort<RoomInstance>> sorts, List<UIRoomBulkApplier> appliers)
            {
            }

            public override void AppendButt(GuiSection s, GETTER<RoomInstance> ins)
            {
                DOUBLE d = new DOUBLE
                {
                    GetD = () =>
                    {
                        double d = (double)g.available.Get(ins.Get()) / g.total.Get(ins.Get());
                        return d;
                    }
                };

                s.AddRightC(16, SPRITES.icons().s.death);

                s.AddRightC(2, new GMeter.GMeterSprite(GMeter.C_REDGREEN, d, 48, 12));
            }

            public override void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                GuiSection se = new GuiSection();
                se.Add(new GStat
                {
                    Update = (GText t) =>
                    {
                        GFORMAT.iofkInv(t, g.available.Get(get.Get()), g.total.Get(get.Get()));
                    }
                }.Hv(g.available.Info()));

                se.AddRightC(100, new GStat
                {
                    Update = (GText t) =>
                    {
                        GFORMAT.perc(t, g.respect.GetD(get.Get()));
                    }
                }.Hv(g.respect.Info()));

                section.AddRelBody(8, DIR.S, se);

                GTableBuilder builder = new GTableBuilder
                {
                    NrOFEntries = () => g.total.Get(get.Get())
                };

                builder.Column(null, 350, new GRowBuilder
                {
                    Build = (GETTER<int> ier) =>
                    {
                        return new HOVERABLE.HoverableAbs(350, Icon.M)
                        {
                            Text = new GText(UI.FONT().M, 32),

                            Render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                            {
                                GraveInfo info = g.info(get.Get(), ier.Get());
                                if (info != null)
                                {
                                    int x1 = body().x1();
                                    text.setMaxWidth(340);
                                    text.setMultipleLines(false);
                                    text.lablify().Clear().Set(info.name());
                                    text.renderCY(r, x1 + 8, body().cY());
                                }
                            },

                            HoverInfoGet = (GUI_BOX text) =>
                            {
                                GraveInfo info = g.info(get.Get(), ier.Get());
                                if (info != null)
                                {
                                    GBox b = (GBox)text;
                                    b.Title(info.name());

                                    b.Text(info.race().info.namePosessive);
                                    b.Text(info.type().name);
                                    b.NL();

                                    b.TextL(Dic.¤¤Age);
                                    b.Tab(6);
                                    b.Add(GFORMAT.i(b.Text(), info.years()));
                                    b.NL();

                                    b.NL(8);
                                    b.TextL(info.cause().name);
                                    b.NL();
                                    b.Add(b.Text().Add('(').Add(info.cause().desc).Add(')'));
                                }
                            }
                        };
                    }
                });

                int h = ISidePanel.HEIGHT - section.body().height() - 16;

                section.AddRelBody(8, DIR.S, builder.CreateHeight(h, true));
            }

            public override void Hover(GBox box, Room i, int rx, int ry)
            {
                box.TextL(g.available.Info().name);
                box.Add(GFORMAT.iofkInv(box.Text(), g.available.Get(i), g.total.Get(i)));
                box.NL(2);
            }
        }
    }
}