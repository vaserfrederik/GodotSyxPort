using System;
using System.IO;
using System.Linq;

namespace util.statistics
{
    public class HistoryInt : HISTORY_INT.HISTORY_INTE, SAVABLE
    {
        private readonly int[] history;
        private int bitSinceStart = -1;
        private readonly TIMECYCLE c;
        private readonly bool keep;
        private INFO info;
        private readonly int max;

        public HistoryInt(int size, TIMECYCLE c, bool keep)
            : this(null, null, size, c, keep, int.MaxValue)
        {
        }

        public HistoryInt(string name, string desc, int size, TIMECYCLE c, bool keep)
            : this(name, desc, size, c, keep, int.MaxValue)
        {
        }

        public HistoryInt(string name, string desc, int size, TIMECYCLE c, bool keep, int max)
        {
            history = new int[size];
            bitSinceStart = c.bitsSinceStart();
            this.c = c;
            this.keep = keep;
            if (name != null)
                info = new INFO(name, desc);
            this.max = max;
        }

        public int get(int fromZero)
        {
            update();
            int i = history.Length - 1 - fromZero;
            i = Math.Clamp(i, 0, history.Length - 1);
            return history[i];
        }

        private void update()
        {
            if (bitSinceStart == c.bitsSinceStart())
                return;

            int d = Math.Abs(c.bitsSinceStart() - bitSinceStart);

            int dd = 0;
            if (keep)
            {
                dd = history[history.Length - 1];
            }

            if (d >= history.Length)
            {
                for (int i = 0; i < history.Length; i++)
                    history[i] = dd;
            }
            else
            {
                for (int i = 0; i + d < history.Length; i++)
                {
                    history[i] = history[i + d];
                }
                for (int i = history.Length - d; i < history.Length; i++)
                    history[i] = dd;
            }
            bitSinceStart = c.bitsSinceStart();
        }

        public TIMECYCLE time()
        {
            return c;
        }

        public void save(FilePutter file)
        {
            update();
            file.is(history);
            file.i(bitSinceStart);
        }

        public void load(FileGetter file)
        {
            file.is(history);
            bitSinceStart = file.i();
        }

        public void clear()
        {
            for (int i = 0; i < history.Length; i++)
            {
                history[i] = 0;
            }
            bitSinceStart = c.bitsSinceStart();
        }

        public void randomize()
        {
            for (int i = 0; i < history.Length; i++)
            {
                history[i] = RND.rInt(50000);
            }
        }

        public void add(HistoryInt other)
        {
            for (int i = 0; i < other.history.Length && i < history.Length; i++)
            {
                history[i] += other.history[i];
            }
        }

        public void set(int amount)
        {
            update();
            int old = history[history.Length - 1];
            history[history.Length - 1] = Math.Clamp(amount, min(), max());
            change(old, history[history.Length - 1]);
        }

        public void fill(int amount)
        {
            history.Fill(amount);
        }

        protected void change(int old, int current)
        {
        }

        public int get()
        {
            return get(0);
        }

        public int min()
        {
            return int.MinValue;
        }

        public int max()
        {
            return max;
        }

        public int historyRecords()
        {
            return history.Length;
        }

        public double getD(int fromZero)
        {
            return get(fromZero) / (double)max();
        }

        public INFO info()
        {
            return info;
        }
    }
}