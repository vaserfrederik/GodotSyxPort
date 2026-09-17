using System;
using System.IO;
using System.Linq;
using init.constant;
using snake2d.util.bit;
using snake2d.util.file;
using snake2d.util.sprite.text;
using util.text;
using world.entity.army;

namespace world.army
{
    public sealed class ADDivs : SAVABLE
    {
        private readonly long[] divs = new long[Config.battle().DIVISIONS_PER_ARMY];
        public readonly long[] data;
        private int divI;
        public readonly Str name = new Str(24);

        private readonly WArmy aa;
        private static readonly BitsLong BType = new BitsLong(0xFF_00_00_00_00_00_00_00L);

        public ADDivs(WArmy e)
        {
            data = new long[AD.iinit().dataA.longCount()];
            this.aa = e;
            Clear();
        }

        public override void Save(FilePutter file)
        {
            file.Ls(divs);
            file.I(divI);
            AD.iinit().dataA.Saver().Save(aa, file);
            name.Save(file);
        }

        public override void Load(FileGetter file)
        {
            file.Ls(divs);
            divI = file.I();
            AD.iinit().dataA.Loader().Load(aa, file);
            name.Load(file);
        }

        public override void Clear()
        {
            divI = 0;
            name.Clear().Add(Dic.¤¤Army);
            divs.AsSpan().Fill(0);
        }

        public bool CanAdd()
        {
            return divI < divs.Length;
        }

        public int Size()
        {
            return divI;
        }

        public ADDiv Get(int i)
        {
            if (i < 0 || i >= Size())
            {
                return null;
            }

            switch (BType.Get(divs[i]))
            {
                case WDivRegional.type: return AD.regional().Get((int)divs[i] & 0x0FFFFFFFF);
                case WDivStored.type: return AD.cityDivs().Get(divs[i]);
                case WDivMercenary.type: return AD.mercenaries().Get(divs[i]);
                default: throw new InvalidOperationException();
            }
        }

        public void Insert(int after, int insert)
        {
            if (after < 0 || after >= Size() || insert < 0 || insert >= Size())
                throw new InvalidOperationException(after + " " + insert);
            if (after == insert)
                return;
            long di = divs[insert];
            for (int i = insert; i < Size() - 1; i++)
            {
                divs[i] = divs[i + 1];
            }
            if (after > insert)
                after--;

            for (int i = Size() - 1; i > after; i--)
            {
                divs[i] = divs[i - 1];
            }
            divs[after] = di;
        }

        void Add(ADDiv div)
        {
            int i = divI;
            long d = BType.Set(0, div.type());
            d |= div.index;
            divs[i] = d;
            divI++;
        }

        void Remove(ADDiv div)
        {
            for (int di = 0; di < divI; di++)
            {
                if (Get(di) == div)
                {
                    for (int ii = di; ii < divI - 1; ii++)
                        divs[ii] = divs[ii + 1];
                    divI--;
                    return;
                }
            }
            throw new InvalidOperationException();
        }
    }
}