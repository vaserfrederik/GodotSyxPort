using System;
using game.faction.npc;
using game.faction.royalty.opinion;
using game.time;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;
using view.ui.diplomacy.UIDipMess;

namespace view.ui.diplomacy
{
    public abstract class UIDipMessAction : MessageSection
    {
        private static readonly long SerialVersionUID = 1L;
        private readonly double time;
        private readonly string desc;
        private readonly string req;
        private bool accepted;
        private byte aa;
        private readonly double declinePenalty;
        private readonly double happiness;
        private readonly MessIntro intro;
        private readonly MessFaction of;

        public UIDipMessAction(ICharSequence title, ICharSequence desc, ICharSequence req, FactionNPC f, FactionNPC o, double happiness, double decline) : base(title)
        {
            time = TIME.CurrentSecond();
            this.desc = desc.ToString();
            this.req = req.ToString();
            of = new MessFaction(o);
            this.declinePenalty = decline;
            this.happiness = happiness;
            f.request.set(decline, title);
            intro = new MessIntro(f);
        }

        protected override void make(GuiSection section)
        {
            paragraph(desc);

            section.addRelBody(8, DIR.S, new GText(UI.FONT().M, req).lablifySub().setMaxWidth(WIDTH));

            section.addRelBody(8, DIR.S, new GText(UI.FONT().S, UIDipMessDeal.¤¤Time).color(COLOR.WHITE85).setMaxWidth(WIDTH));

            GuiSection s = new GuiSection();
            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Accept)
            {
                protected override void renAction()
                {
                    activeSet(pactive());
                }

                protected override void clickA()
                {
                    accepted = true;
                    ROPINION.GIFTS().makeDeal(intro.faction(), happiness);
                    accept(intro.faction(), of.faction());
                    Close();
                    aa = 1;
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.text(UIDipMessDeal.¤¤AcceptD);
                    b.NL();
                    b.add(GFORMAT.f0(b.text(), happiness));
                }
            });

            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Decline)
            {
                protected override void renAction()
                {
                    bool a = true;
                    if (accepted)
                        a = false;
                    if (Math.Abs(TIME.CurrentSecond() - time) > TIME.SecondsPerDay())
                        a = false;
                    if (intro.faction() == null || of.faction() == null)
                        a = false;
                    activeSet(a);
                }

                protected override void clickA()
                {
                    accepted = true;
                    intro.faction().request.expire();
                    Close();
                    aa = -1;
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.text(UIDipMessDeal.¤¤DeclineD);
                    b.NL();
                    b.add(GFORMAT.f0(b.text(), declinePenalty));
                }
            });

            section.addRelBody(8, DIR.S, s);

            section.addRelBody(16, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    if (!pactive())
                    {
                        text.warnify();
                        if (aa == -1)
                            text.add(UIDipMessDeal.¤¤declined);
                        else if (aa == 1)
                            text.add(UIDipMessDeal.¤¤accepted);
                        else
                            text.add(UIDipMessDeal.¤¤noLonger);
                        text.setMaxWidth(WIDTH);
                    }
                }
            }.r(DIR.N));

            section.addRelBody(8, DIR.N, intro.make());
        }

        private bool pactive()
        {
            if (accepted)
                return false;
            if (Math.Abs(TIME.CurrentSecond() - time) > TIME.SecondsPerDay())
                return false;
            if (intro.faction() == null || of.faction() == null)
                return false;
            return valid(intro.faction(), of.faction());
        }

        protected abstract void accept(FactionNPC f, FactionNPC o);
        protected abstract bool valid(FactionNPC f, FactionNPC o);
    }
}