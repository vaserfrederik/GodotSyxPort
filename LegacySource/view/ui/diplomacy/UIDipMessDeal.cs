using System;
using System.Text;
using System.Windows.Forms;
using game.faction;
using game.faction.diplomacy;
using game.faction.diplomacy.deal;
using game.faction.royalty.opinion;
using game.time;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using view.ui.diplomacy;
using view.ui.message;

namespace view.ui.diplomacy
{
    public sealed class UIDipMessDeal : MessageSection
    {
        private static readonly string¤¤noLonger = "¤This offer is no longer valid.";
        private static readonly string¤¤accepted = "¤You have accepted this offer.";
        private static readonly string¤¤declined = "¤You have declined this offer.";
        private static readonly string¤¤Time = "¤Inform us of your decision within a day.";

        private static readonly string¤¤AcceptD = "¤Accepting this offer will change the faction's opinion of you by:";
        private static readonly string¤¤DeclineD = "¤Declining this offer will change the faction's opinion of you by:";

        private static readonly string¤¤Exp = "The agreement has expired.";
        private static readonly string¤¤Power = "Since this agreement was drafted too much has changed.";

        private static readonly string¤¤Inspect = "¤Inspect Faction.";

        private readonly double powerF;
        private readonly double powerP;
        private readonly bool peace;
        private readonly double time;
        private readonly DealSave save;
        private readonly string message;
        private readonly string desc;
        private byte aa = 0;
        private readonly double happiness;
        private readonly double decline;
        private readonly MessIntro intro;

        static UIDipMessDeal()
        {
            D.ts(typeof(UIDipMessDeal));
        }

        public UIDipMessDeal(string title, string message, string desc, Deal deal, double happiness, double declineP) : base(title)
        {
            peace = deal.bools.PEACE.is();
            powerF = deal.npc.npc().offensivePower();
            powerP = FACTIONS.player().offensivePower();
            time = TIME.currentSecond();
            save = new DealSave(deal);
            this.message = message;
            this.desc = desc ?? null;
            this.happiness = happiness;
            this.decline = declineP;
            deal.npc.npc().request.set(declineP, title);
            intro = new MessIntro(deal.npc.npc());
        }

        public UIDipMessDeal(string title, string desc, Deal deal, double happiness, double declineP) : this(title, desc, null, deal, happiness, declineP)
        {
        }

        protected override void make(GuiSection section)
        {
            paragraph(message);

            section.addRelBody(16, DIR.S, new UIDealListSaved(save, 250));

            section.addRelBody(16, DIR.S, new GStat
            {
                Update = text =>
                {
                    string pa = pactive();
                    if (pa != null)
                    {
                        text.warnify();
                        text.add(pa);
                        text.setMaxWidth(WIDTH);
                    }
                    else
                    {
                        text.color(COLOR.WHITE85);
                        text.add(¤¤Time);
                    }
                }
            }.r(DIR.N));

            var s = new GuiSection();
            s.addRightC(0, new GButt.ButtPanel(UI.icons().m.crossair)
            {
                RenAction = () => activeSet(save.f() != null),
                ClickA = () => VIEW.world().UI.factions.open(save.f()),
                HoverTitleSet = ¤¤Inspect
            });

            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Accept)
            {
                RenAction = () => activeSet(pactive() == null),
                ClickA = () =>
                {
                    var d = DIP.TMP();
                    if (pactive() != null)
                        return;

                    d.execute(false);
                    save.f().request.clear();
                    ROPINION.GIFTS().makeDeal(d.npc.npc(), happiness);
                    aa = 1;
                    VIEW.messages().hide();
                },
                HoverInfoGet = text =>
                {
                    var b = (GBox)text;
                    b.text(¤¤AcceptD);
                    b.NL();
                    b.add(GFORMAT.f0(b.text(), happiness));
                }
            });

            s.addRightC(0, new GButt.ButtPanel(Dic.¤¤Decline)
            {
                RenAction = () =>
                {
                    bool a = true;
                    if (aa == 1)
                        a = false;
                    if (aa == -1)
                        a = false;
                    if (Math.Abs(TIME.currentSecond() - time) > TIME.secondsPerDay())
                        a = false;
                    if (save.f() == null)
                        a = false;
                    activeSet(a);
                },
                ClickA = () =>
                {
                    aa = -1;
                    save.f().request.expire();
                    VIEW.messages().hide();
                },
                HoverInfoGet = text =>
                {
                    var b = (GBox)text;
                    b.text(¤¤DeclineD);
                    b.NL();
                    b.add(GFORMAT.f0(b.text(), decline));
                }
            });

            section.addRelBody(8, DIR.S, s);

            section.addRelBody(8, DIR.N, intro.make());

            if (desc != null)
            {
                section.addRelBody(8, DIR.S, new GText(UI.FONT().M, desc).lablifySub().setMaxWidth(WIDTH));
            }
        }

        private string pactive()
        {
            if (aa == 1)
                return ¤¤accepted;
            if (aa == -1)
                return ¤¤declined;
            if (Math.Abs(TIME.currentSecond() - time) > TIME.secondsPerDay())
                return ¤¤Exp;
            string p = save.set(DIP.TMP());
            if (p != null)
                return p;

            if (peace)
            {
                if (Math.Abs((powerF + 10000.0) / (save.f().offensivePower() + 10000) - 1) > 0.25)
                {
                    return ¤¤Power;
                }

                if (Math.Abs((powerP + 10000.0) / (FACTIONS.player().offensivePower() + 10000) - 1) > 0.25)
                {
                    return ¤¤Power;
                }
            }

            return null;
        }
    }
}