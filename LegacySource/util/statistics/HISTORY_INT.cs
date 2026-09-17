using System;

namespace Util.Statistics
{
    public interface HistoryInt : Int, History
    {
        int Get(int fromZero);

        int Get() => Get(0);

        double GetD() => GetD(0);

        int GetPeriod(int from, int to)
        {
            double am = 0;
            final int k = from - to;
            for (int i = 0; i < k; i++)
            {
                am += (i + 1) * Get(to + i);
            }
            double tot = k * (k + 1) * 0.5;
            am /= tot;
            return (int)Math.Ceiling(am);
        }

        int GetPeriodSum(int from, int to)
        {
            int am = 0;
            from = Util.Misc.Clamp.I(from, -HistoryRecords(), 0);
            to = Util.Misc.Clamp.I(to, from, 0);
            from++;

            while (from <= to)
            {
                am += Get(-from);

                from++;
            }
            return am;
        }

        public interface HistoryIntE : HistoryInt, HistoryE, IntE
        {
        }

        public interface HistoryIntObject<T> : IntO<T>, HistoryObject<T>
        {
            int Get(T t);

            double GetD(T t) => GetD(t, 0);

            int Get(T t, int fromZero);

            int GetPeriod(T t, int from, int to)
            {
                double am = 0;
                final int k = from - to;
                for (int i = 0; i < k; i++)
                {
                    am += (i + 1) * Get(t, to + i);
                }
                double tot = k * (k + 1) * 0.5;
                am /= tot;
                return (int)Math.Ceiling(am);
            }
        }

        public interface HistoryIntObjectE<T> : IntOE<T>, HistoryObjectE<T>
        {
            int Get(T t, int fromZero);
        }

        public class HistoryIntObjectWrapper<T> : HistoryInt
        {
            private readonly HistoryIntObject<T> _t;
            private readonly Util.Data.Getter<T> _g;

            public HistoryIntObjectWrapper(HistoryIntObject<T> t, Util.Data.Getter<T> g)
            {
                _t = t;
                _g = g;
            }

            public int Get() => _t.Get(_g.Get());

            public int Min() => _t.Min(_g.Get());

            public int Max() => _t.Max(_g.Get());

            public Game.Time.TimeCycle Time() => _t.Time();

            public int HistoryRecords() => _t.HistoryRecords();

            public int Get(int fromZero) => (int)_t.Get(_g.Get(), fromZero);

            public Util.Info.Info Info() => _t.Info();

            public double GetD(int fromZero) => _t.GetD(_g.Get(), fromZero);
        }

        public class HistoryIntWrapper<T>
        {
            private HistoryIntObject<T> _t;
            private T _g;

            public HistoryInt Wrap(HistoryIntObject<T> t, T g)
            {
                _t = t;
                _g = g;
                return _i;
            }

            private readonly HistoryInt _i = new HistoryInt()
            {
                Get = () => _t.Get(_g),
                Min = () => _t.Min(_g),
                Max = () => _t.Max(_g),
                Time = () => _t.Time(),
                HistoryRecords = () => _t.HistoryRecords(),
                Get = fromZero => (int)_t.Get(_g, fromZero),
                Info = () => _t.Info(),
                GetD = fromZero => _t.GetD(_g, fromZero)
            };
        }
    }
}