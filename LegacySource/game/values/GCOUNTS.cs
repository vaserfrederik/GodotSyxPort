using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace game.values
{
    public sealed class GCOUNTS : GameResource
    {
        private LinkedList<SAccumilator> all = new LinkedList<SAccumilator>();
        {
            D.gInit(this);
        }

        public readonly SAccumilator ENSLAVED = new SAccumilator(all, "ENSLAVED", true, D.g("ENSLAVED", "Enslaved Population"));
        public readonly SAccumilator FREED_SLAVES = new SAccumilator(all, "FREED_SLAVES", true, D.g("FREED_SLAVES", "Slaves Freed"));
        public readonly SAccumilator TIME_PLAYED = new SAccumilator(all, "TIME_PLAYED", true, D.g("TIME_PLAYED", "Time Played"));
        public readonly SAccumilator TRADE_SALES = new SAccumilator(all, "TRADE_SALES", true, D.g("TRADE_SALES", "Denari from sales"));
        public readonly SAccumilator TRADE_PURCHASES = new SAccumilator(all, "TRADE_PURCHASES", true, D.g("TRADE_PURCHASES", "Denari from purchases"));

        public readonly SAccumilator RIOTS = new SAccumilator(all, "RIOTS", true, D.g("RIOTS", "Riots"));
        public readonly SAccumilator CRAFTED = new SAccumilator(all, "CRAFTED", true, D.g("CRAFTED", "Goods Crafted"));
        public readonly SAccumilator INVASIONS = new SAccumilator(all, "INVASIONS", true, D.g("INVASIONS", "Invasions"));
        public readonly SAccumilator EXECUTIONS = new SAccumilator(all, "EXECUTIONS", true, D.g("EXECUTIONS", "Executions"));
        public readonly SAccumilator TUNNELS = new SAccumilator(all, "TUNNELS", true, D.g("TUNNELS", "Tunnels dug"));
        public readonly SAccumilator ROOMS_BUILT = new SAccumilator(all, "ROOMS_BUILT", false, D.g("ENSLAVED", "Rooms built"));
        public readonly SAccumilator SUBJECTS = new SAccumilator(all, "SUBJECTS", false, D.g("SUBJECTS", "Population"));

        public readonly SAccumilator INVASIONS_WON = new SAccumilator(all, "INVASIONS_WON", false, D.g("INVASIONS_WON", "Invasions Won"));
        public readonly SAccumilator INVASIONS_LOST = new SAccumilator(all, "INVASIONS_LOST", false, D.g("INVASIONS_LOST", "Invasions Lost"));
        public readonly SAccumilator ROYALTIES_KILLED = new SAccumilator(all, "ROYALTIES_KILLED", false, D.g("ROYALTIES_KILLED", "Royalties assassinated"));
        public readonly SAccumilator CURED = new SAccumilator(all, "HOSPITAL_CURED", false, D.g("HOSPITAL_CURED", "Cured"));
        public readonly SAccumilator ACCIDENTS = new SAccumilator(all, "ACCIDENTS", false, D.g("ACCIDENTS", "Accidents"));
        public readonly SAccumilator UNITES = new SAccumilator(all, "UNITES", false, D.g("UNITES", "Kingdoms united"));

        public readonly IList<SAccumilator> ALL = new List<SAccumilator>(all);
        public readonly RMAPS<SAccumilator> MAP;

        private static readonly string filename = "StatsDoNotCheat";

        private static readonly string ¤¤allTime = "¤all time";

        private readonly int[] trashold;
        private readonly int[] hi;
        private readonly int[] value;

        static
        {
            D.ts(GCOUNTS.class);
        }

        public GCOUNTS()
        {
            base("COUNTS", true);
            all = null;

            trashold = Alloc.ii(ALL.Count);
            hi = Alloc.ii(ALL.Count);
            value = Alloc.ii(ALL.Count);

            MAP = new RMAPS<SAccumilator>("STATISTIC", ALL);
            read();

            GVALUES.FACTION.push("WORLD_REGIONS", Dic.¤¤Regions, UI.icons().s.world, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return t.realm().regions();
                }
            }, false);
        }

        private void read()
        {
            try
            {
                value.Fill(0);
                trashold.Fill(0);
                hi.Fill(0);
                var json = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(PATHS.local().PROFILE.gets(filename)));
                foreach (var s in ALL)
                {
                    trashold[s.index] = 0;
                    hi[s.index] = 0;
                    if (json.ContainsKey(s.key))
                    {
                        trashold[s.index] = json[s.key];
                    }
                    if (json.ContainsKey(s.key + "_HIGH"))
                    {
                        hi[s.index] = json[s.key + "_HIGH"];
                    }
                }
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                foreach (var s in ALL)
                {
                    trashold[s.index] = 0;
                    hi[s.index] = 0;
                }
                try
                {
                    var j = new Dictionary<string, int>();
                    foreach (var s in ALL)
                    {
                        j[s.key] = 0;
                    }
                    if (!PATHS.local().PROFILE.exists(filename))
                    {
                        PATHS.local().PROFILE.create(filename);
                    }
                    File.WriteAllText(PATHS.local().PROFILE.get(filename), JsonConvert.SerializeObject(j));
                }
                catch (Exception ee)
                {
                    ee.printStackTrace();
                }
            }
        }

        public void flush()
        {
            if (!GAME.achieving())
                return;

            try
            {
                var j = new Dictionary<string, int>();
                foreach (var s in ALL)
                {
                    j[s.key] = CLAMP.i(s.allTimeHigh(), 0, int.MaxValue);
                    j[s.key + "_HIGH"] = CLAMP.i(s.allTimeHigh(), 0, int.MaxValue);
                }
                if (!PATHS.local().PROFILE.exists(filename))
                {
                    PATHS.local().PROFILE.create(filename);
                }
                File.WriteAllText(PATHS.local().PROFILE.get(filename), JsonConvert.SerializeObject(j));
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
            }
        }

        public override void update(double ds, Profiler prof)
        {
            TIME_PLAYED.set((int)(TIME.playedGame() / 60.0));
        }

        public sealed class SAccumilator : MAPPED
        {
            public readonly string key;
            private readonly bool isBattle;
            private readonly int index;
            public readonly string name;

            public SAccumilator(IList<SAccumilator> all, string key, bool isBattle, string name)
            {
                index = all.Add(this);
                this.key = "COUNT_" + key;
                this.isBattle = isBattle;
                this.name = name;
                GVALUES.FACTION.push(this.key + "_GAME", name, UI.icons().s.pluses, new DOUBLE_O<Faction>()
                {
                    public double getD(Faction t)
                    {
                        return current();
                    }
                }, false);
                GVALUES.FACTION.push(this.key + "_ALL_TIME", name + " (" + ¤¤allTime + ")", UI.icons().s.pluses, new DOUBLE_O<Faction>()
                {
                    public double getD(Faction t)
                    {
                        return allTimeHigh();
                    }
                }, false);
            }

            public void inc(int delta)
            {
                if (isBattle || !VIEW.b().isActive())
                {
                    value[index] += delta;
                    value[index] &= int.MaxValue;
                    hi[index] += delta;
                    hi[index] &= int.MaxValue;
                }
            }

            public void set(int a)
            {
                if (isBattle || !VIEW.b().isActive())
                {
                    value[index] = a;
                    value[index] &= int.MaxValue;
                    if (value[index] > hi[index])
                        hi[index] = value[index];
                }
            }

            public int allTimeHigh()
            {
                return Math.Max(current(), hi());
            }

            public int current()
            {
                return value[index];
            }

            public int hi()
            {
                return hi[index];
            }

            public override int index()
            {
                return index;
            }

            public override string key()
            {
                return key;
            }
        }
    }
}