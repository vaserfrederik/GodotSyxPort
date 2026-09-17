using System;
using System.Collections.Generic;
using System.Linq;

namespace Init.Type
{
    public interface MAPPED
    {
        int Index { get; }
        string Key { get; }
    }

    public class WGROUP : MAPPED
    {
        private static List<WGROUP> ALL;
        private static Dictionary<string, WGROUP> MAP;
        private static readonly int[] toIndex;

        static WGROUP()
        {
            List<HTYPE> l = new List<HTYPE>();
            foreach (var t in HTYPES.ALL())
            {
                if (t.IsWorks())
                    l.Add(t);
            }
            List<WGROUP> all = new List<WGROUP>(l.Count * RACES.All().Count);
            toIndex = new int[HTYPES.ALL().Count];
            toIndex.Fill(-1);

            for (int ci = 0; ci < l.Count; ci++)
            {
                toIndex[l[ci].Index()] = ci;
                foreach (var r in RACES.All())
                {
                    all.Add(new WGROUP(ci * RACES.All().Count + r.Index, l[ci], r));
                }
            }
            ALL = all;
            MAP = all.ToDictionary(wg => wg.Key, wg => wg);
        }

        public readonly HTYPE Type;
        public readonly Race Race;
        public readonly SPRITE Icon;
        public readonly string Name;
        public readonly int Index;
        public readonly string Key;

        public static List<WGROUP> All()
        {
            return ALL;
        }

        public static WGROUP Get(HTYPE c, Race r)
        {
            int ci = toIndex[c.Index()];
            if (ci < 0)
                return null;
            return ALL[ci * RACES.All().Count + r.Index];
        }

        public static WGROUP Get(Humanoid h)
        {
            return Get(h.Indu());
        }

        public static WGROUP Get(Induvidual i)
        {
            return Get(i.HType(), i.Race());
        }

        public static Dictionary<string, WGROUP> MAP()
        {
            return MAP;
        }

        private WGROUP(int index, HTYPE t, Race r)
        {
            Type = t;
            Race = r;
            Index = index;
            Key = t.Key + "_" + r.Key;
            Name = r.Info.Names + " (" + t.Names + ")";
            Icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
            {
                Render = (rr, X1, X2, Y1, Y2) =>
                {
                    if (Race == null || Race.Appearance() == null || Race.Appearance().Icon == null)
                        return;
                    double scale = (double)(Y2 - Y1) / Height();
                    int x2 = (int)(X1 + Race.Appearance().Icon.Width() * scale);
                    Race.Appearance().Icon.Render(rr, X1, x2, Y1, (int)(Y1 + Race.Appearance().Icon.Height() * scale));
                    x2 -= 6 * scale;
                    Type.CLASS.IconSmall().Render(rr, x2, (int)(x2 + Type.CLASS.IconSmall().Width() * scale), Y1, (int)(Y1 + Type.CLASS.IconSmall().Width() * scale));
                }
            };
        }

        public int Index()
        {
            return Index;
        }

        public override string ToString()
        {
            return Race.Info.Name + " " + Type.Name;
        }

        public string Key()
        {
            return Key;
        }

        public interface HTypeBits
        {
            bool Is(WGROUP type);
            bool Is(int index);
            bool Is(Humanoid h);
        }

        public class HTypeBitsImp : HTypeBits, ISerializable
        {
            private static HTypeBitsImp[] specific;

            public static HTypeBitsImp Specific(WGROUP t)
            {
                if (specific == null)
                {
                    specific = new HTypeBitsImp[WGROUP.All().Count];
                    for (int i = 0; i < WGROUP.All().Count; i++)
                    {
                        specific[i] = new HTypeBitsImp(false);
                        specific[i].Set(WGROUP.All()[i]);
                    }
                }
                return specific[t.Index];
            }

            private int[] data;

            public HTypeBitsImp(bool everyone)
            {
                if (everyone)
                    SetEveryone();
            }

            private int[] Bits()
            {
                if (data == null)
                    data = new int[(int)Math.Ceiling(WGROUP.All().Count / 32.0)];
                else if (data.Length != (int)Math.Ceiling(WGROUP.All().Count / 32.0))
                    data = new int[(int)Math.Ceiling(WGROUP.All().Count / 32.0)];
                return data;
            }

            public bool Is(int index)
            {
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                return (Bits()[ii] & m) != 0;
            }

            public HTypeBitsImp Set(WGROUP type)
            {
                int index = type.Index;
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                Bits()[ii] |= m;

                return this;
            }

            public HTypeBitsImp Copy(HTypeBits other)
            {
                Clear();
                for (int i = 0; i < WGROUP.All().Count; i++)
                {
                    if (other.Is(i))
                        Set(WGROUP.All()[i]);
                }
                return this;
            }

            public HTypeBitsImp SetEveryone()
            {
                int[] bits = Bits();
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = -1;
                }
                return this;
            }

            public HTypeBitsImp Clear(WGROUP type)
            {
                int index = type.Index;
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                Bits()[ii] &= ~m;
                return this;
            }

            public HTypeBitsImp Clear()
            {
                int[] bits = Bits();
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = 0;
                }
                return this;
            }
        }
    }
}