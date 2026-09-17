using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World.Army
{
    public sealed class WDivRegionalAll : SAVABLE
    {
        public const int Type = 0;
        private readonly ArrayListResize<WDivRegional> all = new ArrayListResize<WDivRegional>(1024 * 4, 1024 * 1024);
        private readonly ArrayListIntegerResize free = new ArrayListIntegerResize(1024 * 4, 1024 * 1024);

        public WDivRegionalAll()
        {
            GAME.Saver().OnAfterLoad(new ACTION_O<Path>
            {
                Exe = t =>
                {
                    if (VERSION.VersionIsBefore(70, 23))
                    {
                        foreach (WDivRegional rr in all)
                        {
                            if (rr.Army != null)
                                rr.MenSet(rr.Men);
                        }
                    }
                }
            });
        }

        private int Create()
        {
            if (free.Count == 0)
            {
                WDivRegional div = new WDivRegional(all.Count);
                all.Add(div);
                return div.Index;
            }
            int i = free[free.Count - 1];
            free.RemoveAt(free.Count - 1);
            return i;
        }

        public WDivRegional Create(Race race, double amount, WArmy a)
        {
            int i = Create();
            Get(i).Init(race, amount, a);
            return Get(i);
        }

        public WDivRegional Get(int index)
        {
            return all[index];
        }

        public void Retire(WDivRegional div)
        {
            free.Add(div.Index);
        }

        public override void Save(FilePutter file)
        {
            file.I(all.Count);
            foreach (WDivRegional r in all)
            {
                r.Save(file);
            }
            free.Save(file);
        }

        public override void Load(FileGetter file)
        {
            int am = file.I();
            all.Clear();
            free.Clear();
            for (int i = 0; i < am; i++)
            {
                int k = Create();
                Get(k).Load(file);
            }
            free.Load(file);
        }

        public override void Clear()
        {
            all.Clear();
            free.Clear();
        }
    }
}