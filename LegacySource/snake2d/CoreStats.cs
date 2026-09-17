using System;

namespace snake2d
{
    public class CoreStats
    {
        CoreStats() { }

        private static int cycleCount = 0;
        private static long timer = DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
        private const double referenceValue = 1000000000 / 60;

        public static readonly Value FPS = new Value("FPS");
        public static readonly Value coreTotal = new Value("Core");
        public static readonly Value corePoll = new Value("Poll");
        public static readonly Value coreFinish = new Value("Finish");
        public static readonly Value coreFlush = new Value("Flush");
        public static readonly Value coreSleep = new Value("Sleep");
        public static readonly Value coreSound = new Value("Sound");
        public static readonly Value smallUpdates = new Value("SmallUp");
        public static readonly Value updatePercentage = new Value("UpPer");
        public static readonly Value renderPercentage = new Value("Render");
        public static readonly Value updateThreadPercentage = new Value("Update Total");

        public static readonly Value swapPercentage = new Value("Swap");

        public static readonly Value totalPercentage = new Value("Tot");
        public static readonly Value droppedTicks = new Value("TicksDropped");
        public static readonly Value heap = new Value("heap size");
        public static readonly Value usedHeap = new Value("used heap");
        public static readonly Value heapGrowth = new Value("heap growth");

        private static long oldMemory = 0;
        private static long dMemory;
        private static int memoryCount = 0;

        public static void print2StdOut()
        {
            Printer.ln("--INFO--");
            Printer.ln(smallUpdates);
            Printer.ln(updatePercentage);
            Printer.ln(smallUpdates);
            Printer.ln(updatePercentage);
            Printer.ln(corePoll);
            Printer.ln(totalPercentage);
            Printer.fin();
        }

        static void endOfLoopCalc()
        {
            cycleCount++;

            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000 - timer >= 1000000000)
            {
                smallUpdates.calc();
                droppedTicks.calc();
                updatePercentage.calc();
                renderPercentage.calc();
                swapPercentage.calc();
                coreTotal.calc();
                corePoll.calc();
                coreFinish.calc();
                coreFlush.calc();
                coreSleep.calc();
                coreSound.calc();
                totalPercentage.setD(updatePercentage.ave + renderPercentage.ave);
                totalPercentage.calc();
                FPS.setD((double)cycleCount);
                FPS.calc();
                Runtime r = Runtime.getRuntime();
                long kb = 1024;
                long newMemory = (r.totalMemory() - r.freeMemory()) / kb;
                dMemory += newMemory - oldMemory;
                oldMemory = newMemory;
                heap.setD(r.totalMemory() / kb);
                heap.calc();
                usedHeap.setD(oldMemory);
                usedHeap.calc();
                memoryCount++;
                if (memoryCount == 5)
                {
                    memoryCount = 0;
                    heapGrowth.setD(dMemory / 5);
                    heapGrowth.calc();
                    dMemory = 0;
                }

                cycleCount = 0;
                timer += 1000000000;
            }
        }

        public class Value
        {
            public volatile double current = 0;
            public volatile double min = 1000;
            public volatile double max = -1;
            public volatile double ave = 0;
            private volatile double acc = 0;
            private volatile int cCount = 0;
            private readonly string name;

            private Value(string name)
            {
                this.name = name;
            }

            public void set(long ns)
            {
                setD(100.0 * (ns / referenceValue));
            }

            public void setD(double percentage)
            {
                current = percentage;
                if (percentage < min)
                    min = percentage;
                else if (percentage > max)
                    max = percentage;
                acc += percentage;
                cCount++;
            }

            private void calc()
            {
                ave = acc / cCount;
                cCount = 0;
                acc = 0;
            }

            public override string ToString()
            {
                int percent = (int)ave;
                int frac = (int)((ave - percent) * 100);

                return name + ": " + percent + "," + frac;
            }

            public string ToStringLong()
            {
                return name + ":"
                       + "\n   Current: " + current.ToString()
                       + "\n   Min: " + min.ToString()
                       + "\n   Max: " + max.ToString()
                       + "\n   Ave: " + ave.ToString();
            }

            public string getLabel()
            {
                return name;
            }
        }
    }
}