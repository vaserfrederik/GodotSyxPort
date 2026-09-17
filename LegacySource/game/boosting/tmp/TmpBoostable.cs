using System.Collections.Generic;
using snake2d.util.sets;
using util.gui.misc;
using util.info;

namespace game.boosting.tmp
{
    public sealed class TmpBoostable<T> where T : INDEXED
    {
        public readonly int startIndex;
        public readonly int max;
        private readonly TmpBoosting daddy;

        public TmpBoostable(ArrayListGrower<TmpBoostable<?>> all, int max, TmpBoosting daddy)
        {
            int i = 0;
            foreach (TmpBoostable<?> t in all)
            {
                i += t.max;
            }
            this.startIndex = i;
            this.max = max;
            this.daddy = daddy;
            all.Add(this);
        }

        public Data Get(T t)
        {
            return daddy.datas[startIndex + t.Index()];
        }

        public void Set(T t, TmpBoostSpec s, bool set)
        {
            Get(t).Set(s, set);
        }

        public void Toggle(T t, TmpBoostSpec s)
        {
            Get(t).Set(s, !Is(t, s));
        }

        public bool Is(T t, TmpBoostSpec s)
        {
            return Get(t).Is(s);
        }

        public double Add(T t, Boostable bo)
        {
            return Get(t).Add(bo);
        }

        public double Mul(T t, Boostable bo)
        {
            return Get(t).Mul(bo);
        }

        public bool Any(T t)
        {
            return Get(t).HasAny();
        }

        public void Clear(T t)
        {
            Get(t).Clear();
        }

        public void Hover(GBox b, T t)
        {
            foreach (TmpBoostSpec s in GAME.BOOST().Specs())
            {
                if (Is(t, s))
                {
                    b.Add(s.icon);
                    b.TextLL(s.name);
                    b.NL();
                    b.Text(s.desc);
                    b.NL(8);

                    foreach (Boostable bo in BOOSTING.ALL())
                    {
                        if (Add(t, bo) != 0)
                        {
                            b.Add(bo.icon);
                            b.TextL(bo.name);
                            b.Tab(6);
                            b.Add(GFORMAT.f0(b.Text(), Add(t, bo)));
                            b.NL();
                        }
                        if (Mul(t, bo) != 1)
                        {
                            b.Add(bo.icon);
                            b.TextL(bo.name);
                            b.Tab(6);
                            GText tt = b.Text();
                            tt.Add('*');
                            b.Add(GFORMAT.f1(tt, Mul(t, bo)));
                            b.NL();
                        }
                    }
                }
            }
        }
    }
}