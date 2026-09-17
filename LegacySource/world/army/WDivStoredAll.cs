using System;
using System.Collections.Generic;
using System.IO;

namespace World.Army
{
    public sealed class WDivStoredAll
    {
        private readonly WDivStored[] divs = new WDivStored[Config.Battle.DIVISIONS_PER_ARMY];
        public int Amount { get; private set; }
        private readonly int[] ramounts = new int[RACES.All().Count];
        private double upD;
        private int upDI;

        public WDivStoredAll()
        {
            for (int i = 0; i < divs.Length; i++)
                divs[i] = new WDivStored(i);
        }

        public void Save(BinaryWriter file)
        {
            foreach (var d in divs)
            {
                d.Save(file);
            }
        }

        public void Load(BinaryReader file)
        {
            Amount = 0;
            Array.Fill(ramounts, 0);
            foreach (var d in divs)
            {
                d.Load(file);
            }
        }

        public WDivStored Get(long data)
        {
            return divs[(int)(data & 0x0FFFF)];
        }

        public WArmy AttachedArmy(Div div)
        {
            if (div.Army == GAME.ARMIES.Enemy())
                return null;
            return divs[div.Index].Army;
        }

        public double DaysToReturn(Div div)
        {
            if (divs[div.Index].Men > 0)
                return Math.Max(0, divs[div.Index].ReturnSecond - TIME.CurrentSecond) * TIME.SecondsPerDayI;
            return -1;
        }

        public WDIV Get(Div div)
        {
            return divs[div.Index];
        }

        public void Attach(WArmy a, Div div)
        {
            if (a != null)
            {
                foreach (var s in RESOURCES.SUP.ALL)
                {
                    int am = s.Amount(div.Info.Race, div.MenNrOf);
                    if (am <= 0)
                        continue;
                    am = Math.Clamp(am, 0, SETT.ROOMS.STOCKPILE.Tally().AmountReservable[s.Resource]);
                    if (am > 0)
                    {
                        s.Resource.Remove(am, RTYPE.ARMY_SUPPLY);
                        AD.Supplies().Get(s).Current.Inc(a, am);
                    }
                }
            }
            divs[div.Index].Reassign(a);
        }

        public void Add(Humanoid i, Div div)
        {
            divs[div.Index].Add(i);
            foreach (var e in STATS.EQUIP.BATTLE_ALL)
            {
                AD.Supplies().Get(e).Current.Inc(AttachedArmy(div), e.Stat.Indu.Get(i.Indu));
            }
        }

        public int Total()
        {
            return Amount;
        }

        public int Total(Race race)
        {
            if (race == null)
                return Amount;
            return ramounts[race.Index];
        }

        public void Update(double ds)
        {
            upD += ds * 32;

            while (upD > 1)
            {
                upD -= 1;

                if (!SETT.ENTRY.Points.HasAny() || SETT.ENTRY.IsClosed())
                    return;

                WDivStored d = divs[upDI];
                upDI++;
                upDI %= divs.Length;

                if (!ShouldReturn(d))
                    continue;

                COORDINATE cret = SETT.ENTRY.Points.RandomReachable(upDI);

                if (cret == null)
                {
                    upD -= (int)upD;
                    continue;
                }

                Humanoid h = d.PopSoldier(cret.X, cret.Y, HTYPES.SUBJECT());

                if (h != null)
                {
                    Div dd = GAME.ARMIES.Player().Divisions.Get(d.Index);
                    if (dd.MenNrOf < Config.Battle.MEN_PER_DIVISION)
                        h.SetDivision(dd);
                }
            }
        }

        private bool ShouldReturn(WDivStored d)
        {
            if (d.Men == 0)
                return false;

            if (d.Army != null)
            {
                if (d.Men > d.MenTarget())
                    return d.ReturnSecond < TIME.CurrentSecond;
                return false;
            }

            return d.ReturnSecond < TIME.CurrentSecond;
        }

        public Induvidual GetSoldier(int id)
        {
            int div = id & 0x0FFFF;
            int ii = (id >> 16) & 0x0FFFF;
            if (div < divs.Length)
            {
                WDivStored w = divs[div];
                if (ii < w.All.Count)
                {
                    return w.All[ii];
                }
            }
            return null;
        }

        public static int GetSoldierId(int index, int div)
        {
            int id = div;
            id |= index << 16;
            return id;
        }

        public Induvidual GetSoldier(int index, int div)
        {
            int ii = index;
            if (div < divs.Length)
            {
                WDivStored w = divs[div];
                if (ii < w.All.Count)
                {
                    return w.All[ii];
                }
            }
            return null;
        }

        public int Soldiers(int div)
        {
            if (div < divs.Length)
            {
                WDivStored w = divs[div];
                return w.All.Count;
            }
            return 0;
        }
    }
}