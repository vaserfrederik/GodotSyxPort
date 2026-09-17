using System;
using System.IO;
using System.Linq;

namespace Settlement.Stats.Stat
{
    public abstract class DataRaces : HISTORY_INT_OBJECT<Race>, SAVABLE
    {
        private int bitSinceStart = -1;
        private readonly int[][] data = Alloc.i2(STATS.DAYS_SAVED + 1, RACES.All().Count());
        private readonly int[] total = Alloc.ii(STATS.DAYS_SAVED + 1);
        private readonly bool clear;

        public DataRaces(string key, StatsInit init, bool clear)
        {
            init.Savers.Put(key, this);
            this.clear = clear;
        }

        void Set(Race r, int a)
        {
            Init();
            if (r == null)
                total[0] = a;
            else
                data[0][r.Index] = a;
        }

        public void IncrFull(Induvidual i, int d)
        {
            Init();
            data[0][i.Race().Index] += d;
            total[0] += d;
        }

        private void PushDay()
        {
            for (int i = STATS.DAYS_SAVED - 1; i > 0; i--)
            {
                for (int ri = 0; ri < RACES.All().Count(); ri++)
                {
                    data[i][ri] = data[i - 1][ri];
                }
                total[i] = total[i - 1];
            }
        }

        private void Init()
        {
            if (bitSinceStart == TIME.Days().BitsSinceStart())
                return;

            int am = Math.Abs(bitSinceStart - TIME.Days().BitsSinceStart());
            if (am > 0)
            {
                for (int i = 0; i < am; i++)
                    PushDay();
            }
            bitSinceStart = TIME.Days().BitsSinceStart();
        }

        public void Save(FilePutter file)
        {
            file.I(bitSinceStart);
            file.IsE(data);
            file.IsE(total);
        }

        public void Load(FileGetter file) throws IOException
        {
            bitSinceStart = file.I();
            file.IsE(data);
            file.IsE(total);
            if (clear)
            {
                total[0] = 0;
                data[0] = data[0].Select(x => 0).ToArray();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < data[0].Length; i++)
                data[0][i] = 0;
            total[0] = 0;
        }

        public int Get(Race group, int daysBack)
        {
            Init();
            if (group == null)
                return total[daysBack];
            return data[daysBack][group.Index];
        }

        public TIMECYCLE Time()
        {
            return TIME.Days();
        }

        public int HistoryRecords()
        {
            return STATS.DAYS_SAVED;
        }
    }
}