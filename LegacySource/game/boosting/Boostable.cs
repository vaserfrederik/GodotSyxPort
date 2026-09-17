using System;
using System.Collections.Generic;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.info;
using util.keymap;

namespace game.boosting
{
    public sealed class Boostable : INFO, MAPPED
    {
        private readonly ArrayListGrower<Booster> all = new ArrayListGrower<Booster>();
        public readonly ArrayListGrower<Booster> fGlobal = new ArrayListGrower<Booster>();

        private readonly int index;
        private byte deadlockCheck;

        public readonly double baseValue;

        public readonly string key;
        public readonly Icon icon;
        public readonly SPRITE nativeIcon;
        public readonly BoostableCat cat;
        public readonly double minValue;

        public Boostable(int index, string key, double baseValue, CharSequence name, CharSequence desc, SPRITE icon, BoostableCat category, double minValue)
            : base(name, desc)
        {
            this.index = index;
            this.baseValue = baseValue;
            this.key = key;
            this.icon = icon != null ? new Icon(Icon.S, icon) : UI.icons().s.DUMMY;
            nativeIcon = icon != null ? icon : UI.icons().s.DUMMY;
            this.cat = category;
            this.minValue = minValue;
            cat.all.add(this);
        }

        public Boostable Copy()
        {
            Boostable b = new Boostable(index, key, baseValue, name, desc, icon, cat, minValue);
            b.all.add(all);
            b.all.add(all);
            return b;
        }

        public LIST<Booster> All()
        {
            return all;
        }

        public double Min(Class<? extends BOOSTABLE_O> b)
        {
            return BUtil.Min(all, b, baseValue);
        }

        public double Max(Class<? extends BOOSTABLE_O> b)
        {
            return BUtil.Max(all, b, baseValue);
        }

        public double Added(BOOSTABLE_O t)
        {
            double padd = baseValue;
            double mul = 1;
            foreach (Booster s in all)
            {
                if (s.isMul && s.Get(t) > 1)
                    mul *= s.Get(t);
                else
                {
                    double a = s.Get(t);
                    if (a > 0)
                        padd += a;
                }
            }

            return CLAMP.d(padd * mul, minValue, double.MaxValue);
        }

        public double Progress(BOOSTABLE_O b)
        {
            double min = Min(b.GetType());
            double max = Max(b.GetType());

            double delta = max - min;
            return CLAMP.d(Get(b) / delta, 0, 1);
        }

        public double Get(BOOSTABLE_O t)
        {
            if (deadlockCheck > 1)
            {
                throw new RuntimeException(
                    "boostable " + key + "seems to be deadlocked. Make sure it's not a factor in its own factors");
            }

            deadlockCheck++;
            double res = BUtil.Value(all, t, baseValue, 1, minValue);
            deadlockCheck--;
            return res;
        }

        public void AddFactor(BoostSpec f)
        {
            all.add(f.booster);
        }

        public void RemoveFactor(BoostSpec f)
        {
            all.remove(f.booster);
        }

        public void Hover(GUI_BOX box, BOOSTABLE_O f, bool keepNops)
        {
            Hover(box, f, name, keepNops);
        }

        static readonly int htab = 7;

        public void HoverDetailed(GUI_BOX box, BOOSTABLE_O f, CharSequence name, bool keepNops)
        {
            BHoverer.HoverDetailed(box, all, f, name, baseValue, keepNops);
        }

        public void Hover(GUI_BOX box, BOOSTABLE_O f, CharSequence name, bool keepNops)
        {
            BHoverer.Hover(box, all, f, name, baseValue, keepNops);
        }

        public int Index()
        {
            return index;
        }

        public string Key()
        {
            return key;
        }

        public bool Contains(Booster booster)
        {
            string k = booster.isMul + " " + booster.info.name;
            for (int bi = 0; bi < all.size(); bi++)
            {
                Booster b2 = all.get(bi);
                string k2 = b2.isMul + " " + b2.info.name;
                if (k.Equals(k2))
                    return true;
            }
            return false;
        }

        public void Debug(BOOSTABLE_O indu)
        {
            double add = baseValue;
            double sub = 0;
            double mul = 1;
            LOG.ln(name + " " + baseValue);
            foreach (Booster f in all)
            {
                double v = f.Get(indu);
                if (f.isMul)
                    mul *= v;
                else if (v < 0)
                    sub -= v;
                else
                    add += v;
                LOG.ln((add * mul - sub) + " " + f.info.name + " " + (f.isMul ? "*" : "+") + " " + v);
            }
            LOG.ln((add * mul - sub));
        }

        public void Debug()
        {
            LOG.ln(name + " " + baseValue);
            foreach (Booster f in all)
            {
                LOG.ln(f.info.name + " " + (f.isMul ? "*" : "+") + " " + f.From() + " " + f.To());
            }
        }
    }
}