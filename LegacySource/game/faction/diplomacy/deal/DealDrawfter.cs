using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data.INT;

namespace game.faction.diplomacy.deal
{
    public static class DealDraftor
    {
        public static void DraftPeace(Deal deal, FactionNPC enemy, bool playerIsMakingDeal)
        {
            deal.SetFactionAndClear(enemy, playerIsMakingDeal);
            deal.bools.PEACE.Set(true);
            Draft(deal, 0, true, true);
        }

        public static void Draft(Deal deal, bool bools, bool regs)
        {
            Draft(deal, 0, bools, regs);
        }

        public static void Draft(Deal deal, double dcreds, bool bools, bool regs)
        {
            int v = (int)(deal.ValueCredits() + dcreds);
            if (v < 0)
            {
                Give(deal, deal.player, -v, bools, regs);
            }
            else
            {
                Give(deal, deal.npc, v, bools, regs);
            }
        }

        private static int[] rr;
        private static ArrayListGrower<Gift> giftable = new ArrayListGrower<Gift>();
        private static ArrayList<DealBool> tbools = new ArrayList<DealBool>(16);
        private static bool log = false;

        private static void Init(DealParty g, Deal deal)
        {
            if (rr == null)
            {
                foreach (TRADABLE res in TR.ALL())
                {
                    giftable.Add(new Gift
                    {
                        Max = () => gg.resources.max(res),
                        Get = () => gg.resources.get(res),
                        Set = (t) => gg.resources.set(res, t),
                        Value = (am) => gg.valueResource(res, am),
                        CanGift = () => oo.resources.get(res) <= 0
                    });
                }

                rr = Alloc.ii(giftable.Size());
            }

            Gift.gg = g;
            Gift.oo = deal.npc == g ? deal.player : deal.npc;
            for (int i = 0; i < rr.Length; i++)
                rr[i] = i;
            for (int i = 0; i < rr.Length; i++)
            {
                int o = rr[i];
                int ii = RND.rInt(rr.Length);
                rr[i] = rr[ii];
                rr[ii] = o;
            }
        }

        private static void Give(Deal deal, DealParty g, int value, bool bools, bool regs)
        {
            int creds = (int)(g.credits.max() * 0.9 - g.credits.get());
            creds = Math.Max(creds, 0);

            if (log)
            {
                LOG.ln(value + " " + bools + " " + regs + " " + creds);
            }

            if (bools)
            {
                if (creds >= value * 2)
                {
                    g.credits.inc(value);
                    return;
                }

                if (DIP.WAR().is(deal.player.f(), deal.npc.f()))
                {
                    tbools.clearSloppy();
                    tbools.add(deal.bools.OVERLORD);
                    tbools.add(deal.bools.VASSAL);
                    tbools.add(deal.bools.PEACE);
                    bool wasPeace = deal.bools.PEACE.is();
                    bool hasSet = false;
                    foreach (DealBool b in tbools)
                    {
                        b.set(false);
                    }
                    foreach (DealBool b in tbools)
                    {
                        if (b.problem() == null)
                        {
                            double v = b.value() * (deal.player == g ? 1 : -0);
                            if (v > 0 && value - v >= 0)
                            {
                                if (log)
                                {
                                    LOG.ln("wbool" + " " + b.info.name + " " + v + " " + value);
                                }
                                value -= v;
                                b.set(true);
                                hasSet = true;
                                break;
                            }
                        }
                    }
                    if (!hasSet && wasPeace)
                    {
                        deal.bools.PEACE.set(true);
                    }
                }
                else
                {
                    bool hasOne = false;
                    foreach (DealBool b in deal.bools.all())
                    {
                        if (b.is())
                        {
                            hasOne = true;
                            break;
                        }
                    }

                    if (!hasOne)
                    {
                        foreach (DealBool b in deal.bools.all())
                        {
                            if (b.problem() == null && !b.is())
                            {
                                double v = b.value() * (deal.player == g ? 1 : -0);
                                if (v > 0 && value - v >= 0)
                                {
                                    value -= v;
                                    b.set(true);
                                    if (log)
                                    {
                                        LOG.ln("bool" + " " + b.info.name + " " + v + " " + value);
                                    }
                                }
                            }
                        }
                    }
                }

                if (creds >= value * 2)
                {
                    g.credits.inc(value);
                    return;
                }
            }

            regs &= !deal.bools.ABSORB.is();

            if (regs)
            {
                if (creds >= value * 2)
                {
                    g.credits.inc(value);
                    return;
                }

                bool hasreg = true;
                while (hasreg)
                {
                    hasreg = false;
                    foreach (DealReg r in g.regs.all())
                    {
                        if (r.value() > 0 && r.canSelect() && !r.is() && r.value() < value)
                        {
                            if (log)
                            {
                                LOG.ln("reg" + " " + r.value() + " " + value);
                            }
                            hasreg = true;
                            r.set(true);
                            value -= r.value();
                        }
                    }
                }

                if (creds >= value * 2)
                {
                    g.credits.inc(value);
                    return;
                }
            }

            Init(g, deal);

            foreach (int ri in rr)
            {
                Gift oo = giftable.get(ri);
                if (oo.value(1) <= 0)
                    continue;

                int am = oo.max() - oo.get();
                int oldValue = oo.value(oo.get());
                am = CLAMP.i(oo.max() / 5, 0, am);
                if (am <= 0)
                    continue;
                if (!oo.canGift())
                    continue;
                if (oo.value(oo.get() + 1) - oldValue > value)
                    continue;

                int cv = oo.value(oo.get() + am) - oldValue;
                if (cv == 0)
                    continue;
                while (cv > value && am > 0)
                {
                    double dec = value;
                    dec /= cv;
                    dec = 1.0 - dec;

                    am -= Math.Ceiling(dec * am * 0.5);
                    cv = oo.value(oo.get() + am) - oldValue;
                }

                if (am > 0)
                {
                    value -= cv;
                    oo.inc(am);
                    if (log)
                    {
                        LOG.ln(RESOURCES.ALL().get(ri) + " " + am + " " + oo.value(am) + " " + oldValue);
                    }
                    if (value <= 0)
                        break;
                }

                if (creds >= value)
                {
                    g.credits.inc(value);
                    return;
                }
            }

            if (creds >= value)
            {
                g.credits.inc(value);
                return;
            }

            for (int nopI = 0; nopI < rr.Length && value > 0; nopI++)
            {
                Gift petit = null;
                int MV = int.MaxValue;

                foreach (int ri in rr)
                {
                    Gift oo = giftable.get(ri);
                    int am = oo.max() - oo.get();
                    if (am > 0 && oo.canGift())
                    {
                        int v = oo.value(oo.get() + 1) - oo.value(oo.get());
                        if (v > 0 && v < MV)
                        {
                            petit = oo;
                            MV = v;
                        }
                    }
                }

                if (petit != null)
                {
                    int max = petit.max();
                    int base = petit.value(petit.get());
                    while (petit.get() + 1 < max - 1)
                    {
                        int v = petit.value(petit.get() + 1) - base;
                        petit.inc(1);
                        if (v >= value)
                        {
                            value -= v;
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (value > 0 && creds >= value)
            {
                g.credits.inc(value);
                return;
            }
        }

        private abstract class Gift : INTE
        {
            public static DealParty gg;
            public static DealParty oo;

            public override int min()
            {
                return 0;
            }

            public abstract bool canGift();

            public abstract int value(int am);
        }
    }
}