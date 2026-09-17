using System;
using System.Linq;
using game.faction;
using game.faction.diplomacy;
using game.faction.diplomacy.deal;
using game.faction.npc;
using game.faction.trade;
using init.trade;
using snake2d.util.file;
using util.data;
using world;
using world.entity.caravan;

namespace game.faction.diplomacy.deal
{
    public sealed class DealParty
    {
        private double selfWorth;
        private double offerableWorth;
        private Faction f;
        private Faction other;
        private FactionNPC npc;
        private double dist;

        public readonly IntImp credits = new IntImp
        {
            Min = () => 0,
            Max = () =>
            {
                Faction fa = f;
                int cr = 0;
                if (fa is FactionNPC)
                {
                    cr = (int)((FactionNPC)fa).stockpile.credit();
                }
                else
                    cr = (int)f.credits().credits();
                if (cr < 0)
                    return 0;
                return cr;
            }
        };

        public readonly DealRegs regs;
        private readonly int[] res = Alloc.Ii(TR.ALL().Size());
        private readonly int[] resMax = Alloc.Ii(TR.ALL().Size());

        public readonly INT_OE<TRADABLE> resources = new INT_OE<TRADABLE>
        {
            Get = t => res[t.Index()],
            Min = t => 0,
            Max = t =>
            {
                if (f == FACTIONS.player())
                    return resMax[t.Index()];
                return Math.Max(f.res().GetAvailable(t) - 1, 0);
            },
            Set = (t, i) => res[t.Index()] = i
        };

        public DealParty(Deal deal, DealRegs.RegData rdata)
        {
            regs = new DealRegs(deal, rdata);
        }

        public void Clear()
        {
            credits.Set(0);
            res.Fill(0);
            regs.Clear();
        }

        public void Execute()
        {
            other.credits().Inc(credits.Get(), CTYPE.DIPLOMACY);
            f.credits().Inc(-credits.Get(), CTYPE.DIPLOMACY);

            foreach (DealReg reg in regs.All())
            {
                if (reg.Is())
                {
                    RD.SetFaction(reg.Reg(), other, true);
                }
            }

            bool rr = false;
            foreach (TRADABLE r in TR.ALL())
            {
                if (res[r.Index()] > 0)
                {
                    rr = true;
                    break;
                }
            }

            if (!rr)
                return;

            Shipment s = WORLD.ENTITIES().Caravans.Create(f.capitolRegion().Cx(), f.capitolRegion().Cy(),
                other.capitolRegion(), TRADE_TYPE.diplomacy);
            if (s != null)
            {
                foreach (TRADABLE r in TR.ALL())
                {
                    int a = resources.Get(r);

                    if (a > 0)
                    {
                        s.LoadAndReserve(r, a);
                    }
                }
            }
            else
            {
                foreach (TRADABLE r in TR.ALL())
                {
                    int a = resources.Get(r);
                    other.Buyer(r).AddReserveAndDeliver(a, TRADE_TYPE.diplomacy);
                }
            }

            foreach (TRADABLE r in TR.ALL())
            {
                int a = resources.Get(r);
                if (a > 0)
                {
                    f.Seller(r).Remove(a, TRADE_TYPE.diplomacy, 0, other);
                }
            }

            Clear();
        }

        public double Value()
        {
            double value = 0;
            value += credits.Get();

            foreach (TRADABLE r in TR.ALL())
            {
                if (res[r.Index()] > 0)
                    value += ValueResource(r, res[r.Index()]);
            }

            value += regs.Worth();

            return value;
        }

        public void Init(Faction a, Faction b, FactionNPC evaluator)
        {
            f = a;
            other = b;
            npc = evaluator;
            regs.Init(a, b, evaluator);

            credits.Set(0);
            selfWorth = regs.SelfWorth();
            offerableWorth = regs.OfferableWorth();
            selfWorth += credits.Max();
            offerableWorth += credits.Max();

            foreach (TRADABLE r in TR.ALL())
            {
                res[r.Index()] = 0;
                int available = f.res().GetAvailable(r);
                resMax[r.Index()] = available;

                if (f == FACTIONS.player() && available > 0)
                {
                    dist = WORLD.PATH().Distance(a.capitolRegion(), b.capitolRegion());
                    double feePerUnit = TradeManager.TotalFee(FACTIONS.player(), npc, dist, r, 1);
                    int low = 0;
                    int high = available;
                    int best = 0;

                    while (low <= high)
                    {
                        int mid = low + (high - low) / 2;

                        double pricePerUnit = npc.Res(r).PriceAt(mid) * 0.8;

                        double netPrice = pricePerUnit - feePerUnit;

                        if (netPrice > 0)
                        {
                            best = mid; // feasible, try to sell more
                            low = mid + 1;
                        }
                        else
                        {
                            high = mid - 1; // too much, reduce
                        }
                    }

                    resMax[r.Index()] = best;
                }

                double v = ValueResource(r, (int)Math.Ceiling(available * 0.75));
                selfWorth += v;
                offerableWorth += v;
            }

            if (a != FACTIONS.player())
                offerableWorth *= 0.25;
            Clear();
        }

        public int ValueResource(TRADABLE res, int amount)
        {
            if (f == FACTIONS.player())
            {
                double p = npc.Buyer(res).AddPrice(amount);
                p -= TradeManager.TotalFee(FACTIONS.player(), npc, dist, res, amount);
                p *= 0.9;
                return (int)Math.Max(p, 0);
            }
            else
            {
                double p = npc.Seller(res).RemovePrice(amount);
                p += TradeManager.TotalFee(f, other, dist, res, amount);
                p *= 1.1;
                return (int)Math.Max(p, 1);
            }
        }

        public static int ManualPriceSell(FactionNPC f, TRADABLE res, int amount)
        {
            int p = f.Seller(res).RemovePrice(amount);
            p += TradeManager.TotalFee(f, FACTIONS.player(), RD.DIST().Distance(f), res, amount);
            if (!DIP.Get(f).trades)
                p *= 1.5;
            else
                p *= 1.25;
            return Math.Max(p, 1);
        }

        public static int ManualPriceBuy(FactionNPC f, TRADABLE res, int amount)
        {
            int p = f.Buyer(res).AddPrice(amount);
            p -= TradeManager.TotalFee(FACTIONS.player(), f, RD.DIST().Distance(f), res, amount);

            p *= 0.8;
            return (int)Math.Max(p, 0);
        }

        public double SelfWorth()
        {
            return selfWorth;
        }

        public double OfferableWorth()
        {
            return offerableWorth;
        }

        public Faction F()
        {
            return f;
        }

        public FactionNPC Npc()
        {
            return npc;
        }
    }
}