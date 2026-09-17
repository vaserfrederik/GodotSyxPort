using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d.util.misc;
using snake2d.util.sets;

namespace game.debug
{
    public sealed class CallCounter
    {
        private readonly int max;
        private int count;
        private readonly KeyMap<EE> map = new KeyMap<EE>();
        private bool running = false;

        public CallCounter(int max)
        {
            this.max = max;
            GAME.Saver().OnAfterLoad(new ACTION_O<Path>(
                t =>
                {
                    running = true;
                }));
        }

        private sealed class EE
        {
            public readonly StackTraceElement[] ee;
            public int count = 0;

            public EE(StackTraceElement[] ee)
            {
                this.ee = ee;
            }
        }

        public void Count()
        {
            if (!running)
                return;

            StackTraceElement[] eee = new StackTrace().GetFrames();
            string ii = "";
            foreach (StackTraceElement e in eee)
            {
                ii += e.ToString();
            }

            if (!map.ContainsKey(ii))
            {
                map.Put(ii, new EE(eee));
            }

            map.Get(ii).count++;
            count++;

            if (count > max)
            {
                List<EE> es = new List<EE>(map.All());
                es.Sort((o1, o2) => o1.count - o2.count);

                foreach (EE e in es)
                {
                    if (e.count > 0)
                    {
                        Console.WriteLine(e.count);
                        for (int ei = 2; ei < e.ee.Length; ei++)
                        {
                            Console.WriteLine(e.ee[ei]);
                        }

                        Console.WriteLine();
                        e.count = 0;
                    }
                }

                count = 0;
            }
        }
    }
}