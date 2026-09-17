using System;
using System.Collections.Generic;
using System.Linq;
using game.faction;
using game.tourism;
using settlement.room.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room;

namespace settlement.room.infra.inn
{
    class Gui : UIRoomModuleImp<InnInstance, ROOM_INN>
    {
        private static readonly string ¤¤Guestbook = "Guestbook";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_INN s) : base(s)
        {
        }

        protected override void AppendPanel(GuiSection section, GGrid grid, GETTER<InnInstance> g, int x1, int y1)
        {
            GuiSection s = new GuiSection();

            s.Add(new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.iIncr(text, g.Get().earnings);
                }

                public override void HoverInfoGet(GBox b)
                {
                    GText t = b.Text();
                    DicTime.setYears(t, -1);
                    b.Add(t);
                    b.Add(GFORMAT.iIncr(b.Text(), g.Get().earningsLast));
                }
            }.Hv(Dic.¤¤Earnings));

            section.AddRelBody(32, DIR.S, s);

            section.AddRelBody(4, DIR.S, new GHeader(¤¤Guestbook));

            GRows gg = new GRows(2);

            for (int i = 0; i < 4; i++)
            {
                final int k = i;

                RENDEROBJ rr = new RENDEROBJ.RenderImp(800, 400)
                {
                    public override void Render(SPRITE_RENDERER r, float ds)
                    {
                        Review rev = g.Get().reviews[k];
                        if (rev != null && rev.Has())
                        {
                            rev.Render(r, body.X1(), body.Y1(), 800);
                        }
                    }
                };

                gg.Add(new GButt.ButtPanel(new SPRITE.Imp(100, 48))
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        Review rev = g.Get().reviews[k];
                        if (rev != null && rev.Has())
                        {
                            rev.RenderScore(r, X1 + (X2 - X1) / 2, Y1 + 8);
                            rev.RenderCred(r, X1 + 24, Y1 + 8 + 20);
                        }
                    }
                })
                {
                    protected override void RenAction()
                    {
                        Review rev = g.Get().reviews[k];
                        activeSet(rev != null && rev.Has());
                    }

                    protected override void ClickA()
                    {
                        Review rev = g.Get().reviews[k];
                        if (rev == null || !rev.Has())
                            return;
                        VIEW.inters().popup.Show(rr, this);
                    }
                });
            }

            foreach (RENDEROBJ o in gg.Rows())
            {
                section.AddRelBody(4, DIR.S, o);
            }
        }

        protected override void AppendMain(GGrid grid, GGrid text, GuiSection sExtra)
        {
            GuiSection s = new GuiSection();

            final int am = FACTIONS.player().Credits().Get(CTYPE.TOURISM).IN.historyRecords();
            GStaples chart = new GStaples(am)
            {
                protected override void Hover(GBox box, int stapleI)
                {
                    int ago = am - 1 - stapleI;
                    GText t = box.Text();
                    DicTime.setAgo(t, ago * FACTIONS.player().Credits().Get(CTYPE.TOURISM).IN.time().BitSeconds());
                    box.TextLL(t);
                    box.NL();
                    box.Add(GFORMAT.iIncr(box.Text(), FACTIONS.player().Credits().Get(CTYPE.TOURISM).IN.Get(ago)));
                }

                protected override double GetValue(int stapleI)
                {
                    return FACTIONS.player().Credits().Get(CTYPE.TOURISM).IN.Get(am - 1 - stapleI);
                }
            };
            chart.Body().SetDim(240, 58);

            s.Add(chart);
            s.AddRelBody(4, DIR.N, new GHeader(Dic.¤¤Earnings));

            s.AddRelBody(4, DIR.S, new GButt.ButtPanel(Dic.¤¤Tourists)
            {
                protected override void ClickA()
                {
                    VIEW.UI().tourists.Activate();
                }
            }.Pad(4, 1));

            text.Add(s);
        }

        protected override void AppendTableButt(GuiSection s, GETTER<RoomInstance> ins)
        {
        }

        protected override void Hover(GBox box, InnInstance i)
        {
            InnInstance ii = i;

            box.NL();
            box.TextLL(Dic.¤¤Earnings);
            box.Add(GFORMAT.iIncr(box.Text(), ii.earnings));
            box.NL();
        }

        protected override void AppendTableFilters(LISTE<GTFilter<RoomInstance>> filters,
            LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }
    }
}