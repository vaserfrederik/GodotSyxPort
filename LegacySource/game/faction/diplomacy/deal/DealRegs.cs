using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Faction.Diplomacy.Deal
{
    public class DealRegs
    {
        private readonly List<DealReg> tmp = new List<DealReg>(128);
        private readonly DealReg[] all = new DealReg[128];
        private int selfWorth;
        private int offerableWorth;

        private bool dirty = true;

        private Faction giver;
        private Faction reciever;

        private readonly RegData data;
        private readonly Deal deal;
        private readonly Treaty t = new Treaty();

        private readonly WRegSel sel = new WRegSel();

        public DealRegs(Deal deal, RegData data)
        {
            for (int i = 0; i < all.Length; i++)
                all[i] = new DealReg();
            this.data = data;
            this.deal = deal;
        }

        public void Init(Faction giver, Faction reciever, FactionNPC evaluator)
        {
            data.selected.Clear();
            data.canSelect.Clear();

            this.giver = giver;
            this.reciever = reciever;
            selfWorth = 0;
            for (int i = 0; i < giver.Realm.Regions; i++)
            {
                if (giver.Realm.Region(i).Capitol)
                    continue;
                selfWorth += ValueRegion(giver.Realm.Region(i), evaluator, 0);
                data.selected.Set(giver.Realm.Region(i).Index, false);
            }
            tmp.Clear();

            offerableWorth = 0;
            int ri = 0;

            if (reciever.CapitolRegion != null)
            {
                foreach (RegDist d in WORLD.PATH().RegFinder.All(reciever.CapitolRegion, t, sel))
                {
                    if (!tmp.HasRoom())
                        break;

                    if (d.Reg.Capitol)
                        continue;

                    DealReg rr = all[ri++];
                    rr.Reg = d.Reg;
                    if (giver == evaluator)
                        data.values[d.Reg.Index] = (int)valueRegion(d.Reg, evaluator, d.Distance);
                    else
                    {
                        data.values[d.Reg.Index] = (int)((0.5 + 0.5 * CLAMP.D(1 - d.Distance / 255.0, 0, 1)) * valueRegion(d.Reg, evaluator, d.Distance));
                        if (giver == FACTIONS.Player && !DIP.WAR().Is(giver, evaluator))
                            data.values[d.Reg.Index] *= ROPINION.STANCE().TrustWorthyness(evaluator);
                    }

                    offerableWorth += data.values[d.Reg.Index];
                    tmp.Add(rr);
                }
            }

            dirty = true;
        }

        private static bool log = false;

        private static double ValueRegion(Region reg, FactionNPC faction, double dist)
        {
            if (log)
                LOG.Ln(reg.Info.Name);

            double value = 0;

            foreach (RDRace r in RD.RACES().All)
            {
                value += FACTIONS.PRICE().Get(TR.Get(r.Race)) * 0.25;
            }

            if (log)
                LOG.Ln("slaves " + value);
            double ma = 0;

            foreach (RDBuilding bu in RD.BUILDINGS().All)
            {
                foreach (BoostSpec bo in bu.Boosters().All())
                {
                    TRADABLE resource = RD.OUTPUT().FromBoost(bo.Boostable);
                    if (resource != null)
                    {
                        double m = BUtil.Value(bu.BaseFactors, reg);
                        double v = bo.Booster.Max() * m * faction.Res(resource).PriceBase();
                        if (v > ma)
                        {
                            if (log)
                                LOG.Ln(resource + " " + m + " " + bo.Booster.Max() + " " + faction.Res(resource).PriceBase() + " " + bu.Key + " " + bo.Booster.Info.Name + " " + bo.Boostable.Key);
                            ma = v;
                        }
                    }
                }
            }

            if (log)
                LOG.Ln("res " + ma);
            value += 16 * 5 * ma * (0.05 + RD.RACES().PopSizeD(reg));
            if (log)
                LOG.Ln(value);

            if (reg.Faction == faction)
                value *= 2;
            else if (RD.OWNER().PrevOwner(reg) == faction)
                value *= 1.5;
            else
            {
                value *= CLAMP.D(1.0 - dist / 256.0, 0.1, 1);
            }

            return value;
        }

        public static double LootWorth(Region reg)
        {
            double value = 0;

            foreach (RDRace r in RD.RACES().All)
            {
                value += FACTIONS.PRICE().Get(TR.Get(r.Race)) * 0.25;
            }

            double ma = 0;

            foreach (RDBuilding bu in RD.BUILDINGS().All)
            {
                foreach (BoostSpec bo in bu.Boosters().All())
                {
                    TRADABLE resource = RD.OUTPUT().FromBoost(bo.Boostable);
                    if (resource != null)
                    {
                        double m = BUtil.Value(bu.BaseFactors, reg);
                        double v = bo.Booster.Max() * m * FACTIONS.PRICE().Get(resource);
                        if (v > ma)
                        {
                            ma = v;
                        }
                    }
                }
            }
            value += 16 * 5 * ma * (0.05 + RD.RACES().PopSizeD(reg));

            return value;
        }

        private void Init()
        {
            for (int i = 0; i < giver.Realm.Regions; i++)
            {
                data.canSelect.Set(giver.Realm.Region(i).Index, false);
            }

            foreach (RegDist d in WORLD.PATH().RegFinder.All(reciever.CapitolRegion, itreaty, sel))
            {
                data.canSelect.Set(d.Reg.Index, true);
            }
        }

        private readonly Treaty itreaty = new Treaty();

        public class DealReg : BOOLEAN_MUTABLE
        {
            private Region reg;

            public Region Reg()
            {
                return reg;
            }

            public override bool Is()
            {
                return Selected(reg);
            }

            public override BOOLEAN_MUTABLE Set(bool b)
            {
                data.selected.Set(reg.Index, b);
                dirty = true;
                return this;
            }

            public double Value()
            {
                if (reciever is FactionNPC && reg.Faction == FACTIONS.Player && DIP.WAR().Is((FactionNPC)reciever) && !deal.bools.PEACE.Is())
                    return 0;
                return data.values[reg.Index];
            }

            public bool CanSelect()
            {
                if (deal.bools.ABSORB.Is())
                    return false;
                return data.canSelect.Get(reg.Index);
            }
        }

        public int SelfWorth()
        {
            return selfWorth;
        }

        public int OfferableWorth()
        {
            return offerableWorth;
        }

        public IEnumerable<DealReg> All()
        {
            if (dirty)
            {
                Init();
                dirty = false;
            }
            return tmp;
        }

        public void Clear()
        {
            Init();
        }

        public double Worth()
        {
            double v = 0;
            foreach (DealReg r in All())
            {
                if (r.Is())
                    v += r.Value();
            }
            return v;
        }

        public void Add(Region reg)
        {
            data.selected.Set(reg.Index, true);
            dirty = true;
        }

        public void Select(Region reg, bool sel)
        {
            data.selected.Set(reg.Index, sel);
            dirty = true;
        }

        public bool Selected(Region reg)
        {
            if (deal.bools.ABSORB.Is())
                return false;
            return data.selected.Get(reg.Index);
        }

        public bool SelecteCan(Region reg)
        {
            return data.canSelect.Get(reg.Index);
        }

        public int Value(Region reg)
        {
            if (reciever is FactionNPC && reg.Faction == FACTIONS.Player && DIP.WAR().Is((FactionNPC)reciever) && !deal.bools.PEACE.Is())
                return 0;
            return data.values[reg.Index];
        }

        public class RegData
        {
            private readonly Bitmap1D selected = new Bitmap1D(WREGIONS.MAX, false);
            private readonly Bitmap1D canSelect = new Bitmap1D(WREGIONS.MAX, false);
            private readonly int[] values = Alloc.Ii(WREGIONS.MAX);

        }
    }
}