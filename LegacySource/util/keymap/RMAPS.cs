using System;
using System.Collections.Generic;
using System.IO;

namespace util.keymap
{
    public class RMAPS<T> : RMAP<T> where T : MAPPED
    {
        private Saver saver;
        private Loader loader;

        public RMAPS(string key, LIST<T> all) : base(key, all)
        {
            INIT.addSaver(new Savable(key)
            {
                public void save(FilePutter file)
                {
                    saver = new Saver(file);
                }

                public void load(FileGetter file) => loader = new Loader(file);
            });
        }

        public Saver saver() => saver;

        public Loader loader() => loader;

        public sealed class Saver
        {
            private Saver(FilePutter f)
            {
                f.mark(RMAPS<T>.this);
                f.i(all().size());
                foreach (T s in all())
                {
                    f.chars(s.key());
                }
                f.mark(RMAPS<T>.this);
            }

            public void save(T t, FilePutter f)
            {
                if (t == null)
                    f.i(-1);
                else
                    f.i(t.index());
            }

            public void save(int[] amounts, FilePutter f)
            {
                check(amounts.Length);
                f.is(amounts);
            }

            public void save(short[] amounts, FilePutter f)
            {
                check(amounts.Length);
                f.ss(amounts);
            }

            public void save(SAVABLE[] amounts, FilePutter f)
            {
                check(amounts.Length);
                foreach (SAVABLE s in amounts)
                    s.save(f);
            }

            public void save(LIST<? extends SAVABLE> amounts, FilePutter f)
            {
                check(amounts.size());
                foreach (SAVABLE s in amounts)
                    s.save(f);
            }

            public void save(double[] amounts, FilePutter f)
            {
                check(amounts.Length);
                f.ds(amounts);
            }

            public void save(long[] amounts, FilePutter f)
            {
                check(amounts.Length);
                f.ls(amounts);
            }
        }

        private void check(int ams)
        {
            if (ams != all().size())
                throw new RuntimeException(ams + " " + all().size());
        }

        public sealed class Loader
        {
            private readonly bool isSame;
            private readonly int am;
            private readonly int[] order;

            private Loader(FileGetter f)
            {
                f.check(RMAPS<T>.this);
                isSame = true;
                am = f.i();
                if (am != all().size())
                    isSame = false;

                order = Alloc.ii(Math.Max(all().size(), am));
                Array.Fill(order, -1);

                for (int i = 0; i < am; i++)
                {
                    string k = f.chars();
                    if (map.get(k) != null)
                    {
                        order[i] = map.get(k).index();
                        isSame &= i == map.get(k).index();
                    }
                    else
                        isSame = false;
                }

                this.isSame = isSame;
                f.check(RMAPS<T>.this);
            }

            public T loadB(FileGetter f, T pref) => load(f, pref);

            public int loadI(FileGetter f)
            {
                int i = f.i();
                if (i < 0)
                    return -1;
                if (isSame)
                    return all().get(i).index();
                if (order[i] == -1)
                    return -1;
                return all().get(order[i]).index();
            }

            public T get(int index)
            {
                if (index < 0)
                    return null;
                if (isSame)
                    return all().get(index);
                if (order[index] == -1)
                    return null;
                return all().get(order[index]);
            }

            public T load(FileGetter f)
            {
                int i = f.i();
                if (i < 0)
                    return null;
                if (isSame)
                    return all().get(i);
                if (order[i] == -1)
                    return null;
                return all().get(order[i]);
            }

            public byte[] fix(byte[] old, byte defValue)
            {
                if (isSame)
                    return old;
                if (old.Length != am)
                    throw new RuntimeException();

                byte[] amounts = Alloc.bb(all().size());

                Array.Fill(amounts, defValue);
                for (int i = 0; i < old.Length; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts[o] = old[i];
                }
                return old;
            }

            public int fix(int old, int fallback)
            {
                if (isSame)
                    return old;

                if (old < 0 || old >= order.Length)
                    return fallback;
                int o = order[old];
                if (o != -1)
                    return o;
                return fallback;
            }

            public int[] fix(int[] old, int defValue)
            {
                if (isSame)
                    return old;

                int[] nn = Alloc.ii(all().size());
                Array.Fill(nn, defValue);
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        nn[o] = old[i];
                }
                return nn;
            }

            public bool isSame() => isSame;

            public void load(int[] amounts, FileGetter f, int defValue)
            {
                check(amounts.Length);
                if (isSame)
                {
                    f.is(amounts);
                    return;
                }
                int[] old = Alloc.ii(am);
                f.is(old);
                Array.Fill(amounts, defValue);
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts[o] = old[i];
                }
            }

            public void load(long[] amounts, FileGetter f, long defValue)
            {
                check(amounts.Length);
                if (isSame)
                {
                    f.ls(amounts);
                    return;
                }
                long[] old = new long[am];
                f.ls(old);
                Array.Fill(amounts, defValue);
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts[o] = old[i];
                }
            }

            public void load(short[] amounts, FileGetter f, short defValue)
            {
                check(amounts.Length);
                if (isSame)
                {
                    f.ss(amounts);
                    return;
                }
                short[] old = new short[am];
                f.ss(old);
                Array.Fill(amounts, defValue);
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts[o] = old[i];
                }
            }

            public void load(double[] amounts, FileGetter f, double defValue)
            {
                check(amounts.Length);

                if (isSame)
                {
                    f.ds(amounts);
                    return;
                }
                double[] old = new double[am];
                f.ds(old);
                Array.Fill(amounts, defValue);
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts[o] = old[i];
                }
            }

            public void load(SAVABLE[] amounts, FileGetter f)
            {
                check(amounts.Length);
                if (isSame)
                {
                    foreach (SAVABLE s in amounts)
                        s.load(f);
                    return;
                }

                load(new ArrayList<SAVABLE>(amounts), f);
            }

            public void load(LIST<? extends SAVABLE> amounts, FileGetter f)
            {
                check(amounts.size());
                if (isSame)
                {
                    foreach (SAVABLE s in amounts)
                        s.load(f);
                    return;
                }

                foreach (SAVABLE s in amounts)
                    s.clear();

                int matches = 0;
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        matches++;
                }
                for (int i = matches; i < am; i++)
                {
                    amounts.get(0).load(f);
                }
                amounts.get(0).clear();
                for (int i = 0; i < am; i++)
                {
                    int o = order[i];
                    if (o != -1)
                        amounts.get(o).load(f);
                }
            }
        }
    }
}