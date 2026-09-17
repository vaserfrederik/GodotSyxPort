using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.ui.message;

namespace game.raiding
{
    [Serializable]
    final class MessDemand : MessageSection
    {
        private static CharSequence ¤¤title = "Protection?";
        private static CharSequence ¤¤title2 = "Raider Returns";

        private static CharSequence ¤¤pay = "¤Pay Up";
        private static CharSequence ¤¤decline = "¤Decline Offer";

        private static CharSequence ¤¤proInfo = "¤Our scouts report these people have about {0} soldiers. Paying the sum is a safe bet. Declining might lead to trouble. We have 1 day to decide.";

        private static CharSequence ¤¤nolonger = "You do not have enough goods to meet their demands. ({0})";
        private static CharSequence ¤¤nocreds = "You do not have enough denari to meet their demands.";

        static
        {
            D.ts(typeof(MessDemand));
        }

        private static readonly long serialVersionUID = 1L;
        private readonly Demand demand;

        private readonly Raider raider;
        private readonly string[] mess;

        public MessDemand(Raider raider) : base(raider.raids == 1 ? ¤¤title : ¤¤title2)
        {
            this.raider = raider;

            demand = new Demand(raider);

            mess = new string[raider.text.demandBody.Count];
            int mi = 0;
            foreach (string s in raider.text.demandBody)
                mess[mi++] = s;
        }

        protected override void Make(GuiSection section)
        {
            section.AddRelBody(32, DIR.N, new RaiderPortrait(4).Set(raider));

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (string ss in mess)
            {
                foreach (CharSequence s in UI.FONT().M.GetRows(ss, WIDTH))
                {
                    rows.Add(new GTextR(UI.FONT().M, s));
                }
                rows.Add(new RENDEROBJ.RenderDummy(10, 8));
            }

            section.AddRelBody(8, DIR.S, new GScrollRows(rows, C.HEIGHT() - 600).View());

            Str s = Str.TMP.Clear();
            s.Add(¤¤proInfo);
            s.Insert(0, raider.army.men);

            section.AddRelBody(16, DIR.S, new GText(UI.FONT().S, "" + s).LablifySub().SetMaxWidth(WIDTH));

            section.AddRelBody(16, DIR.S, demand.Section(true));
        }

        [Serializable]
        static class Demand : Serializable
        {
            /**
             * 
             */
            private static readonly long serialVersionUID = 1L;
            private int[] resources = Alloc.ii(TR.ALL().Count);
            private int credits = 0;
            private bool payed = false;
            private bool declined = false;
            private readonly int iteration;
            private readonly Raider raider;

            public Demand(Raider raider) : this(raider, Worth(raider))
            {
            }

            private static int Worth(Raider raider)
            {
                int wo = (int)FACTIONS.WORTH().raider();
                wo = (int)Math.Max(raider.worth, wo /= 4);
                wo = (int)Math.Min(wo, FACTIONS.WORTH().raider() / 2);

                return wo;
            }

            private Demand(Raider raider, double cre)
            {
                iteration = GAME.raiders().current.iterration;
                this.raider = raider;

                if (ee == null)
                    ee = new DEntries();

                ee.Set(this, (int)cre);
            }

            public bool CanRespond()
            {
                if (payed)
                    return false;
                if (declined || !GAME.raiders().current.canPay(iteration))
                    return false;
                return true;
            }

            public bool CanPay()
            {
                if (payed)
                    return false;
                if (declined || !GAME.raiders().current.canPay(iteration))
                    return false;
                if (credits > 0 && FACTIONS.player().credits().GetD() < credits)
                    return false;
                foreach (TRADABLE res in TR.ALL())
                {
                    if (resources[res.index()] > 0 && resources[res.index()] > res.ps().playerOwned())
                    {
                        return false;
                    }
                }

                return true;
            }

            public void HoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                if (credits > 0 && FACTIONS.player().credits().GetD() < credits)
                {
                    b.Error(¤¤nocreds);
                }
                foreach (TRADABLE res in TR.ALL())
                {
                    if (resources[res.index()] > 0 && resources[res.index()] > res.ps().playerOwned())
                    {
                        Str.TMP.Clear().Add(¤¤nolonger).Insert(0, res.names);
                        b.Error(Str.TMP);
                        b.NL();
                        break;
                    }
                }
            }

            public void Pay()
            {
                FACTIONS.player().credits().Inc(-credits, CTYPE.TRIBUTE);
                TR_STOCKPILE stock = new TR_STOCKPILE();
                foreach (TRADABLE res in TR.ALL())
                {
                    stock.Set(res, resources[res.index()]);
                    FACTIONS.player().seller(res).Remove(resources[res.index()], TRADE_TYPE.spoils, 0, null);
                }
            }

            public RENDEROBJ Section(bool buttons)
            {
                GuiSection s = new GuiSection();

                if (buttons)
                {
                    GuiSection bb = new GuiSection();
                    bb.Add(new GButt.ButtPanel(¤¤pay)
                    {
                        RenAction = () =>
                        {
                            if (payed)
                                SelectedSet(true);
                            else
                                ActiveSet(!declined && GAME.raiders().current.canPay(iteration));
                        },
                        ClickA = () =>
                        {
                            if (CanPay())
                            {
                                payed = true;
                                Pay();
                                GAME.raiders().current.Clear();
                                VIEW.messages().Hide();

                                new MessDemandTY(raider).Send();
                            }
                        },
                        HoverInfoGet = HoverInfoGet
                    });

                    bb.AddRightC(0, new GButt.ButtPanel(¤¤decline)
                    {
                        RenAction = () =>
                        {
                            if (declined)
                                SelectedSet(true);
                            else
                                ActiveSet(!payed && GAME.raiders().current.canPay(iteration));
                        },
                        ClickA = () =>
                        {
                            declined = true;
                            VIEW.messages().Hide();
                        }
                    });

                    s.AddRelBody(16, DIR.S, bb);
                }

                return s;
            }
        }

        private static DEntries ee;

        private static class DEntries
        {
            private readonly ArrayListGrower<DEntry> all = new ArrayListGrower<DEntry>();
            private readonly Tree<DEntry> rtree;

            public DEntries()
            {
                foreach (TRADABLE res in TR.ALL())
                {
                    all.Add(new DEntry()
                    {
                        Max = () => (int)(res.ps().playerOwned() * 0.95),
                        Add = (Demand d, int am) => d.resources[res.index()] += am,
                        Value = () => FWorth.worthResource(res, 1)
                    });
                }

                rtree = new Tree<DEntry>(all.Count)
                {
                    IsGreaterThan = (current, cmp) => current.value > cmp.value
                };
            }

            public void Set(Demand d, int credits)
            {
                rtree.Clear();
                foreach (DEntry e in all)
                {
                    e.value = (int)(e.Value() * e.Max() * RND.rFloat1(0.5));
                    if (e.value > 0)
                        rtree.Add(e);
                }

                if (FACTIONS.player().credits().GetD() > 0)
                {
                    int cre = (int)(FACTIONS.player().credits().GetD() * RND.rFloat());
                    if (cre > credits)
                        cre = (int)credits;
                    d.credits += cre;
                    credits -= cre;
                }

                while (credits > 0 && rtree.HasMore())
                {
                    DEntry e = rtree.PollGreatest();

                    int am = e.Max();
                    int v = (int)e.Value();
                    int w = (int)(e.Max() * v);
                    if (w > credits)
                        am = (int)Math.Ceiling((double)credits / v);
                    e.Add(d, am);
                    credits -= am * v;
                }

                if (credits > 0)
                {
                    int am = (int)(FACTIONS.player().credits().GetD() - d.credits);
                    if (am > credits)
                        am = credits;
                    if (am > 0)
                        d.credits += am;
                }
            }
        }

        private static abstract class DEntry
        {
            public int value;
            public int current;

            public abstract void Add(Demand d, int am);
            public abstract int Max();
            public abstract double Value();
        }
    }
}