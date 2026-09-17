using System;
using System.IO;
using System.Linq;

namespace game.boosting.tmp
{
    class Data : SAVABLE
    {
        private readonly Bitmap1D specActive;
        private readonly float[] add;
        private readonly float[] mul;

        public Data(int specs)
        {
            specActive = new Bitmap1D(specs + 2, false);
            add = new float[BOOSTING.ALL().Count];
            mul = new float[BOOSTING.ALL().Count];
            mul.Fill(1f);
        }

        private void Cache()
        {
            if (specActive.Get(specActive.Size - 2))
            {
                add.Fill(0);
                mul.Fill(1);
                bool any = false;
                foreach (TmpBoostSpec s in GAME.BOOST().Specs())
                {
                    if (specActive.Get(s.Index))
                    {
                        any = true;
                        foreach (BoostSpec ss in s.Spec.All())
                        {
                            if (ss.Booster.IsMul)
                            {
                                mul[ss.Boostable.Index()] *= (float)ss.Booster.To();
                            }
                            else
                            {
                                add[ss.Boostable.Index()] += (float)ss.Booster.To();
                            }
                        }
                    }
                }
                specActive.Set(specActive.Size - 1, any);
                specActive.Set(specActive.Size - 2, false);
            }
        }

        public double Add(Boostable bo)
        {
            Cache();
            return add[bo.Index()];
        }

        public double Mul(Boostable bo)
        {
            Cache();
            return mul[bo.Index()];
        }

        public void Set(TmpBoostSpec s, bool set)
        {
            if (set == specActive.Get(s.Index))
                return;
            specActive.Set(s.Index, set);
            SetDirty();
        }

        public void SetDirty()
        {
            specActive.Set(specActive.Size - 2, true);
        }

        public bool Is(TmpBoostSpec spec)
        {
            Cache();
            return specActive.Get(spec.Index);
        }

        public bool HasAny()
        {
            Cache();
            return specActive.Get(specActive.Size - 1);
        }

        public override void Clear()
        {
            specActive.Clear();
            SetDirty();
        }

        public override void Save(FilePutter file)
        {
            specActive.Save(file);
        }

        public override void Load(FileGetter file)
        {
            specActive.Load(file);
            SetDirty();
        }

        public void Load(FileGetter file, int[] oldOrder)
        {
            specActive.Clear();
            Bitmap1D old = new Bitmap1D(specActive.Size, false);
            old.Load(file);

            for (int i = 0; i < old.Size; i++)
            {
                if (old.Get(i) && i < oldOrder.Length && oldOrder[i] != -1)
                {
                    specActive.Set(oldOrder[i], true);
                }
            }
            SetDirty();
        }
    }
}