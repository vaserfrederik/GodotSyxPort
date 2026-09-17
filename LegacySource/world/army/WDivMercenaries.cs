using System;
using System.Collections.Generic;
using System.IO;

namespace World.Army
{
    public class WDivMercenaries : IEnumerable<WDivMercenary>
    {
        private List<WDivMercenary> all = new List<WDivMercenary>(40);

        private static readonly string ¤¤mWTitle = "¤Mercenaries Displeased!";
        private static readonly string ¤¤mWBody = "¤We are running low on Denari and can't pay our mercenaries. We need at least {0} additional Denari to ensure their loyalty.";

        private static readonly string ¤¤mTitle = "¤Mercenaries leaving!";
        private static readonly string ¤¤mBody = "¤Since you don't have enough credits to pay them, your hired mercenaries are leaving you.";

        static WDivMercenaries()
        {
            D.ts(typeof(WDivMercenaries));
        }

        private readonly IUpdater updater = new IUpdater(all.Capacity, TIME.SecondsPerDay())
        {
            protected override void Update(int di, double timeSinceLast)
            {
                if (di == 10)
                {
                    int missed = 0;
                    int cc = 0;
                    foreach (WDivMercenary d in all)
                    {
                        if (d.Army == null || d.Army.Faction != FACTIONS.Player())
                            continue;
                        if (d.MissedPayments < 0)
                        {
                            d.MissedPayments = 0;
                            continue;
                        }
                        int cost = (int)(d.CostPerMan() * d.Men * d.Army.SupplyAmount());
                        if (cost > FACTIONS.Player().Credits.Credits)
                        {
                            d.MissedPayments++;
                            if (d.MissedPayments >= 2)
                            {
                                d.Reassign(null);
                            }
                            cc += cost;
                            missed = Math.Max(missed, d.MissedPayments);
                        }
                        else
                        {
                            FACTIONS.Player().Credits.Inc(-cost, CTYPE.MERCINARIES);
                            d.MissedPayments--;
                            if (d.MissedPayments > 0 && cost <= FACTIONS.Player().Credits.Credits)
                            {
                                FACTIONS.Player().Credits.Inc(-cost, CTYPE.MERCINARIES);
                            }
                        }
                    }

                    if (missed == 1)
                    {
                        Str.TMP.Clear().Add(¤¤mWBody);
                        Str.TMP.Insert(0, cc);
                        new MessageText(¤¤mWTitle, Str.TMP).Send();
                    }
                    else if (missed == 2)
                    {
                        new MessageText(¤¤mTitle, ¤¤mBody).Send();
                    }
                }

                WDivMercenary d = all[di];

                if (d.Army != null)
                {
                    if (d.Army.Recruiting())
                    {
                        d.MenSet(Math.Clamp(d.Men + 1, 0, d.MenTarget()));
                    }
                }
                else
                {
                    STATS.POP().Age.DAYS.Inc(d.Cheif(), 1);
                    if (STATS.POP().Age.ShouldDieOfOldAge(d.Cheif()))
                    {
                        d.Randomize();
                    }

                    if (d.DisbandTime > 0)
                    {
                        d.DisbandTime -= timeSinceLast;
                    }
                }
            }
        };

        public WDivMercenaries()
        {
            D.t(this);

            for (int i = 0; i < all.Capacity; i++)
            {
                WDivMercenary d = new WDivMercenary(i);
                all.Add(d);
            }

            IDebugPanelWorld.Add("mercs randomize", new Action
            {
                public void Exe()
                {
                    Randmoize();
                }
            });
        }

        public void Randmoize()
        {
            for (int i = 0; i < all.Capacity; i++)
            {
                WDivMercenary d = all[i];
                d.Randomize();
            }
        }

        public void Save(FilePutter file)
        {
            foreach (WDivMercenary d in all)
                d.Save(file);
            updater.Save(file);
        }

        public void Load(FileGetter file)
        {
            foreach (WDivMercenary d in all)
                d.Load(file);
            updater.Load(file);
        }

        public void Debug()
        {
            debug = true;
            Update(TIME.SecondsPerDay() * all.Count);
        }

        public void Update(double ds)
        {
            updater.Update(ds);
        }

        private bool debug = false;

        public int Max()
        {
            if (debug)
                return Size();
            else
            {
                double d = FACTIONS.Player().Realm.All.Size;
                foreach (Faction f in DIP.VASSAL.All(FACTIONS.Player()))
                {
                    d += f.Realm.Regions / 2.0;
                }
                foreach (Faction f in DIP.ALLY.All(FACTIONS.Player()))
                {
                    d += f.Realm.Regions / 4.0;
                }

                d /= 16.0;
                int m = (int)(d * Size());
                m = Math.Clamp(m, 1, Size());
                return m;
            }
        }

        public int UpkeepCost(int index)
        {
            return all[index].CostPerMan() * all[index].MenTarget();
        }

        public int SigningCost(int index)
        {
            return 4 * all[index].CostPerMan() * all[index].MenTarget();
        }

        public ADDiv Get(long l)
        {
            return all[(int)(l & 0x00000FFFF)];
        }

        public void Hire(WArmy a, WDivMercenary div)
        {
            div.Reassign(a);
        }

        public IEnumerator<WDivMercenary> GetEnumerator()
        {
            return all.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return all.GetEnumerator();
        }

        public WDivMercenary this[int index]
        {
            get { return all[index]; }
        }

        public bool Contains(int i)
        {
            return all.Contains(i);
        }

        public bool Contains(WDivMercenary obj)
        {
            return all.Contains(obj);
        }

        public int Count
        {
            get { return all.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}