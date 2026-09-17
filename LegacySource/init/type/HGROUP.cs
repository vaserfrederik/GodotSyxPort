using System;
using System.Collections.Generic;
using init.race;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;

namespace init.type
{
    public sealed class HGROUP : MAPPED
    {
        private static LIST<HGROUP> ALL;
        private static LIST<HGROUP> CIT;
        private static LIST<HGROUP> SLAVE;
        private static RMAPS<HGROUP> MAP;

        static HGROUP()
        {
            ArrayListGrower<HGROUP> l = new ArrayListGrower<HGROUP>();
            ArrayListGrower<HGROUP> ll = new ArrayListGrower<HGROUP>();
            foreach (Race r in RACES.all())
            {
                HGROUP g = new HGROUP(l.size(), HCLASSES.SLAVE(), r);
                l.add(g);
                ll.add(g);
            }
            SLAVE = ll;
            ll = new ArrayListGrower<HGROUP>();
            foreach (Race r in RACES.all())
            {
                HGROUP g = new HGROUP(l.size(), HCLASSES.CITIZEN(), r);
                l.add(g);
                ll.add(g);
            }
            CIT = ll;
            ALL = l;
            MAP = new RMAPS<HGROUP>("HGROUP", l);
        }

        public readonly HCLASS type;
        public readonly Race race;
        public readonly SPRITE icon;
        public readonly string name;
        public readonly int index;
        public readonly string key;

        public static LIST<HGROUP> all()
        {
            return ALL;
        }

        public static HGROUP get(HCLASS c, Race r)
        {
            if (c == HCLASSES.SLAVE())
                return SLAVE.get(r.index());
            else if (c == HCLASSES.CITIZEN())
            {
                return CIT.get(r.index());
            }
            return null;
        }

        public static HGROUP get(Humanoid h)
        {
            return get(h.indu());
        }

        public static HGROUP get(Induvidual i)
        {
            return get(i.clas(), i.race());
        }

        public static RMAPS<HGROUP> MAP()
        {
            return MAP;
        }

        private HGROUP(int index, HCLASS t, Race r)
        {
            this.type = t;
            this.race = r;
            this.index = index;
            key = t.key + "_" + r.key;
            name = r.info.names + " (" + t.names + ")";
            icon = new SPRITE.Imp(Icon.M + 12, Icon.M)
            {
                public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                {
                    if (race == null || race.appearance() == null || race.appearance().icon == null)
                        return;
                    double scale = (double)(Y2 - Y1) / height();
                    int x2 = (int)(X1 + race.appearance().icon.width() * scale);
                    race.appearance().icon.render(rr, X1, x2, Y1, (int)(Y1 + race.appearance().icon.height() * scale));
                    x2 -= 6 * scale;
                    type.iconSmall().render(rr, x2, (int)(x2 + type.iconSmall().width() * scale), Y1, (int)(Y1 + type.iconSmall().width() * scale));
                }
            };
        }

        public int index()
        {
            return index;
        }

        public override string ToString()
        {
            return race.info.name + " " + type.name;
        }

        public string key()
        {
            return key;
        }

        public interface HTypeBits
        {
            public default bool is(HGROUP type)
            {
                if (type == null)
                    return false;
                return is(type.index);
            }

            public bool is(int index);

            public default bool is(Humanoid h)
            {
                if (h.indu().clas().player)
                    return is(HGROUP.get(h));
                return false;
            }
        }

        [Serializable]
        public class HTypeBitsImp : HTypeBits
        {
            private static HTypeBitsImp[] specific;

            public static HTypeBitsImp specific(HGROUP t)
            {
                if (specific == null)
                {
                    specific = new HTypeBitsImp[HGROUP.all().size()];
                    for (int i = 0; i < HGROUP.all().size(); i++)
                    {
                        specific[i] = new HTypeBitsImp(false);
                        specific[i].set(HGROUP.all().get(i));
                    }
                }
                return specific[t.index];
            }

            private int[] data;

            public HTypeBitsImp(bool everyone)
            {
                if (everyone)
                    setEveryone();
            }

            private int[] bits()
            {
                if (data == null)
                    data = Alloc.ii((int)Math.Ceiling(HGROUP.all().size() / 32.0));
                else if (data.Length != (int)Math.Ceiling(HGROUP.all().size() / 32.0))
                {
                    data = Alloc.ii((int)Math.Ceiling(HGROUP.all().size() / 32.0));
                }
                return data;
            }

            public bool is(int index)
            {
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                return (bits()[ii] & m) != 0;
            }

            public HTypeBitsImp set(HGROUP type)
            {
                int index = type.index;
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                bits()[ii] |= m;

                return this;
            }

            public HTypeBitsImp copy(HTypeBits other)
            {
                clear();
                for (int i = 0; i < HGROUP.all().size(); i++)
                {
                    if (other.is(i))
                        set(HGROUP.all().get(i));
                }
                return this;
            }

            public HTypeBitsImp setEveryone()
            {
                int[] bits = bits();
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = -1;
                }
                return this;
            }

            public HTypeBitsImp clear(HGROUP type)
            {
                int index = type.index;
                int ii = index >> 5;
                int m = 1 << (index & 0b011111);
                bits()[ii] &= ~m;
                return this;
            }

            public HTypeBitsImp clear()
            {
                int[] bits = bits();
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = 0;
                }
                return this;
            }
        }
    }
}