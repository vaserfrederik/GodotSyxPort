using System;
using System.IO;
using System.Collections.Generic;

namespace game.faction.trade
{
    public class TradeShipper : SAVABLE
    {
        private readonly Partner[] partners;
        private readonly List<short> neighFactions = new List<short>(FACTIONS.MAX());

        public TradeShipper()
        {
            partners = new Partner[FACTIONS.MAX()];
            for (int i = 0; i < partners.Length; i++)
                partners[i] = new Partner(FACTIONS.getByIndex(i));
        }

        public void Save(FilePutter file)
        {
            neighFactions.Save(file);
            for (int i = 0; i < partners.Length; i++)
            {
                file.D(partners[i].distance);
            }
            for (int i = 0; i < partners.Length; i++)
            {
                TR.MAP().saver().Save(partners[i].traded, file);
            }
        }

        public void Load(FileGetter file)
        {
            neighFactions.Load(file);
            for (int i = 0; i < partners.Length; i++)
            {
                partners[i].distance = file.D();
            }
            for (int i = 0; i < partners.Length; i++)
            {
                TR.MAP().loader().Load(partners[i].traded, file, 0);
            }
        }

        public void Clear()
        {
            neighFactions.Clear();
        }

        public void Init(Faction buyer)
        {
            if (buyer.capitolRegion() == null)
                return;
            neighFactions.Clear();
            foreach (RegDist d in RD.DIST().tradePartners(buyer))
            {
                if (d.reg.faction() == buyer)
                    continue;

                Partner p = partners[d.reg.faction().index()];
                Array.Fill(p.traded, 0);
                p.distance = d.distance;
                neighFactions.Add((short)d.reg.faction().index());
            }
        }

        public Partner PopNextPartner()
        {
            int i = neighFactions[neighFactions.Count - 1];
            neighFactions.RemoveAt(neighFactions.Count - 1);
            return partners[i];
        }

        public bool HasNextPartner()
        {
            return neighFactions.Count > 0;
        }

        public int Partners()
        {
            return neighFactions.Count;
        }

        public Partner Partner(int i)
        {
            return partners[neighFactions[i]];
        }

        public sealed class Partner
        {
            private readonly short faction;
            private double distance;
            private readonly int[] traded;

            public Partner(Faction faction)
            {
                this.faction = (short)faction.index();
                traded = Alloc.Ii(TR.ALL().size());
            }

            public Faction Faction()
            {
                return FACTIONS.getByIndex(faction);
            }

            public double Distance()
            {
                return distance;
            }

            public int Traded(TRADABLE res)
            {
                return traded[res.index()];
            }

            public void Trade(TRADABLE res, int amount)
            {
                traded[res.index()] += amount;
            }
        }
    }
}