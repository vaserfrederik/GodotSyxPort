using System;
using System.Collections.Generic;

namespace Game.Debug
{
    public interface Profiler
    {
        void LogStart(object o);
        void LogStart(Type cl);
        void LogEnd(Type cl);
        void LogEnd(object o);
        void Log();

        static readonly Profiler Dummy = new ProfilerImpl
        {
            LogStart = cl => { },
            LogEnd = cl => { },
            Log = () => { }
        };

        static readonly Profiler Live = new ProfilerImpl
        {
            Entries = new List<Prof> { new Prof(), new Prof() },
            Map = new Dictionary<Type, Prof>(),
            SS = -1,
            Tab = 0,
            CPU = false,
            Mem = true,
            KeepNops = false,
            Outliners = false,
            OMap = new Dictionary<Type, Out>()
        };

        private class ProfilerImpl : Profiler
        {
            public List<Prof> Entries { get; private set; }
            public Dictionary<Type, Prof> Map { get; private set; }
            public long SS { get; private set; }
            public int Tab { get; private set; }
            public bool CPU { get; private set; }
            public bool Mem { get; private set; }
            public bool KeepNops { get; private set; }
            public bool Outliners { get; private set; }
            public Dictionary<Type, Out> OMap { get; private set; }

            public void LogStart(object o)
            {
                LogStart(o.GetType());
            }

            public void LogStart(Type cl)
            {
                if (SS == -1)
                    SS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (!Map.ContainsKey(cl))
                {
                    Prof e = Entries[Entries.Count - 1];
                    e.Acc = 0;
                    e.Tab = Tab;
                    e.MemAcc = 0;
                    Map.Add(cl, e);
                    Entries.RemoveAt(Entries.Count - 1);
                }

                Map[cl].Mem = GC.GetTotalMemory(false);
                Map[cl].Start = DateTime.Now.Ticks;
                Map[cl].Tab = Tab;
                Tab++;
            }

            public void LogEnd(Type cl)
            {
                Prof e = Map[cl];
                long l = DateTime.Now.Ticks - e.Start;
                if (l > 16 * 1000000)
                {
                    Console.WriteLine(cl);
                }

                Map[cl].MemAcc += GC.GetTotalMemory(false) - Map[cl].Mem;
                e.Acc += l;
                Tab--;
            }

            public void Log()
            {
                if (SS == -1)
                    return;

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - SS < 2000)
                    return;

                long tot = 0;
                foreach (var k in Map.Values)
                {
                    tot += k.Acc;
                }

                Console.WriteLine("CPU");
                foreach (var k in Map)
                {
                    Entries.Add(k.Value);
                    int v = (int)(1000.0 * k.Value.Acc / tot);

                    if (Outliners)
                    {
                        if (!OMap.ContainsKey(k.Key))
                        {
                            OMap.Add(k.Key, new Out { Old = v });
                        }
                        else
                        {
                            OMap[k.Key].NN = v;
                        }
                    }

                    if (CPU)
                        if (KeepNops || v > 0)
                            Console.WriteLine(new string(' ', k.Value.Tab * 2) + " " + v + " " + k.Key + " " + k.Value.Acc);
                }

                if (Mem)
                {
                    Console.WriteLine();
                    Console.WriteLine("MEM");
                    foreach (var k in Map)
                    {
                        if (k.Value.MemAcc > 0)
                            Console.WriteLine(new string(' ', k.Value.Tab * 2) + " " + k.Value.MemAcc + " " + k.Key);
                    }
                }

                if (Outliners)
                {
                    Console.WriteLine();
                    Console.WriteLine("CHANGE");
                    foreach (var k in OMap)
                    {
                        double v = k.Value.NN / k.Value.Old;

                        if (v > 1.5)
                        {
                            Console.WriteLine(k.Value.NN + " <- " + k.Value.Old + " " + k.Key);
                        }

                        k.Value.Old = k.Value.NN;
                    }
                }

                Map.Clear();
                Tab = 0;
                SS = -1;
                Console.WriteLine();
            }

            private class Prof
            {
                public long Start;
                public int Tab;
                public long Acc;
                public long Mem;
                public long MemAcc;
            }

            private class Out
            {
                public double Old;
                public double NN;
            }
        }
    }
}