using System;
using System.IO;

namespace Util.Data
{
    public interface INT : DOUBLE
    {
        int Get();
        int Min();
        int Max();
        double GetD()
        {
            if (Get() < 0)
            {
                if (Min() == 0)
                    return 0;
                return Get() / (double)Min();
            }
            else if (Get() > 0)
            {
                if (Max() == 0)
                    return 0;
                return Get() / (double)Max();
            }
            else
                return 0;
        }

        public interface INTE : INT, DOUBLE_MUTABLE
        {
            void Set(int t);
            void Inc(int i)
            {
                Set(CLAMP.i(Get() + i, Min(), Max()));
            }

            INTE IncD(double d)
            {
                int i = (int)(Max() * d);
                if (i == 0)
                {
                    if (d < 0)
                        i = -1;
                    else
                        i = 1;
                }
                Inc(i);
                return this;
            }

            DOUBLE_MUTABLE SetD(double d)
            {
                Set((int)Math.Ceiling(d * Max()));
                return this;
            }
        }

        public class IntImp : INTE, SAVABLE
        {
            public int i;
            public int min, max;

            public IntImp() : this(int.MinValue, int.MaxValue) { }

            public IntImp(int min, int max)
            {
                this.min = min;
                this.max = max;
                i = CLAMP.i(i, min, max);
            }

            public IntImp(int i, int min, int max)
            {
                this.min = min;
                this.max = max;
                i = CLAMP.i(i, min, max);
                this.i = i;
            }

            public int Get()
            {
                return i;
            }

            public int Min()
            {
                return min;
            }

            public int Max()
            {
                return max;
            }

            public void Set(int t)
            {
                i = CLAMP.i(t, Min(), Max());
            }

            public void Save(FilePutter file)
            {
                file.i(i);
            }

            public void Load(FileGetter file)
            {
                i = file.i();
            }

            public void Clear()
            {
                i = 0;
            }

            public bool IsMax()
            {
                return Get() >= max;
            }
        }
    }
}