using System;
using System.Linq;
using snake2d;
using util.colors;
using util.data;
using util.gui.misc;
using util.info;
using view.main;
using view.ui.diplomacy;

namespace view.world.ui.faction
{
    final class UIDiplomacy : GuiSection
    {
        private static readonly CharSequence ¤¤What = "What do you wish to offer us?";
        private static readonly CharSequence ¤¤Barter = "Barter";
        private static readonly CharSequence ¤¤BarterD = "Allow the Faction to compose a deal that they feel comfortable with based on your demands.";
        private static readonly CharSequence ¤¤desc = "The value of a deal is weighed by the faction's perception of the value of its components. A deal needs to have a possible value in order to go through. A high positive value indicate generosity on your part, and will increase the faction's opinion of you.";
        private static readonly CharSequence ¤¤Accept = "The deal will be accepted";
        private static readonly CharSequence ¤¤AcceptNo = "The deal will not be accepted";
        private static readonly CharSequence ¤¤OpinionD = "The change of opinion of the faction's ruler if this deal is accepted.";
        private static readonly CharSequence ¤¤No = "You have nothing of worth to offer the faction.";

        static UIDiplomacy()
        {
            D.ts(typeof(UIDiplomacy));
        }

        private readonly Deal deal;
        public readonly GuiSection section = new GuiSection();
        private double timer = 0;

        public UIDiplomacy(GETTER<FactionNPC> g, Deal deal, int height)
        {
            this.deal = new Deal();

            addRelBody(0, DIR.S, new GText(UI.FONT().M, ¤¤What).lablifySub());

            GuiSection op = new GuiSection();

            {
                op.addRightC(0, new GStat
                {
                    public void update(GText text)
                    {
                        GFORMAT.iIncr(text, (long)(deal.valueCredits()));
                    }
                }.hh(Dic.¤¤Value).hoverInfoSet(¤¤desc));

                op.addRightC(100, new GStat
                {
                    public void update(GText text)
                    {
                        GFORMAT.f0(text, deal.opinionChange());
                    }
                }.hh(ROPINION.¤¤name).hoverInfoSet(¤¤OpinionD));

                op.addRightC(100, new GStat
                {
                    public void update(GText text)
                    {
                        GFORMAT.f0(text, -deal.betrayal());
                    }

                    public void hoverInfoGet(GBox b)
                    {
                        deal.hoverBetrayal(b);
                    }
                }.hh(OpsStance.¤¤betrayal));

                op.addRightC(100, new GButt.ButtPanel(Dic.¤¤Accept)
                {
                    protected override void renAction()
                    {
                        activeSet(deal.canBeAccepted() || S.get().developer);
                    }

                    protected override void clickA()
                    {
                        if (deal.canBeAccepted() || S.get().developer)
                            deal.execute(true);

                        base.clickA();
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.text(¤¤desc);

                        {
                            b.NL(8);
                            b.textLL(Dic.¤¤Value);
                            b.tab(6);
                            b.add(UI.icons().s.money);
                            b.add(GFORMAT.i(b.text(), (long)(deal.valueCredits())));
                        }

                        {
                            b.NL();
                            b.textLL(ROPINION.¤¤name);
                            b.tab(6);

                            b.add(GFORMAT.f0(b.text(), deal.opinionChange()));
                            GText t = b.text();
                            t.add('(');
                            GFORMAT.f(t, ROPINION.get(deal.npc.npc()));
                            t.add(')');
                            b.add(t);
                        }

                        b.NL(8);
                        if (deal.canBeAccepted())
                            b.textL(¤¤Accept);
                        else
                            b.error(¤¤AcceptNo);
                    }
                }.hoverInfoSet(¤¤desc));

                op.addRightC(16, new GButt.ButtPanel(¤¤Barter)
                {
                    private GTextR t = new GText(UI.FONT().M, ¤¤No).warnify().r(DIR.N);

                    protected override void clickA()
                    {
                        DealDrawfter.draft(deal, true, true);
                        if (deal.hasDeal() && !deal.canBeAccepted())
                        {
                            timer = 5;
                            VIEW.inters().popup.show(t, this);
                        }
                        base.clickA();
                    }

                    protected override void renAction()
                    {
                        activeSet(deal.hasDeal());
                    }
                }.hoverInfoSet(¤¤BarterD));

                if (S.get().developer)
                {
                    op.addRightC(16, new GButt.ButtPanel(UI.icons().s.cog)
                    {
                        protected override void clickA()
                        {
                            GuiSection s = new Debugger.DebuggerSection(700)
                            {
                                protected override void fill(Debugger d)
                                {
                                    deal.setFactionAndClear(deal.npc.npc(), false, d);
                                }
                            };
                            VIEW.inters().popup.show(s, this);
                        }

                        protected override void renAction()
                        {
                            activeSet(deal.hasDeal());
                        }
                    }.hoverInfoSet(¤¤BarterD));
                }
            }

            int h = height - body().height() - op.body().height() - 16;

            GuiSection s = new GuiSection();

            s.add(new UIDealConfig(deal, h));
            s.addRelBody(16, DIR.E, new SPRITE.Imp(1, h)
            {
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    GCOLOR.UI().border().render(r, X1, X2, Y1, Y2);
                }
            });
            s.addRelBody(16, DIR.E, new UIDealList(deal, h));

            addRelBody(8, DIR.S, s);

            addRelBody(8, DIR.S, op);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            GAME.SPEED.tmpPause();
            if (timer > 0)
            {
                timer -= ds;
                if (timer <= 0)
                    VIEW.inters().popup.close();
            }
            base.render(r, ds);
        }

        public void openPeace(FactionNPC other)
        {
            GAME.SPEED.tmpPause();
            deal.setFactionAndClear(other, true);
            DealDrawfter.draftPeace(deal, other, true);
        }
    }
}