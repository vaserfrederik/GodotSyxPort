using System.Collections.Generic;
using init.sprite.UI;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

namespace game.boosting
{
    public static class BHoverer
    {
        private static readonly ArrayListInt sort = new ArrayListInt(1024);

        private BHoverer()
        {
        }

        public static void HoverDetailed(GUI_BOX box, LIST<BoosterAbs<T>> all, T f, CharSequence name, double baseValue, bool keepNops)
        {
            GBox b = (GBox)box;
            if (name != null)
                b.textLL(name);
            b.NL();

            if (baseValue != 0)
                BoosterAbs.hover(box, baseValue, 0, UI.icons().s.dot, false, Dic.¤¤BaseValue);
            box.NL();

            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);

                if (!l.isMul && d != 0)
                {
                    l.hover(box, d);
                    BoosterAbs.hoverSpan(box, l.from(), l.to());
                    box.NL();
                }
            }

            b.NL(4);

            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);
                if (l.isMul && d != 1)
                {
                    l.hover(box, d);
                    BoosterAbs.hoverSpan(box, l.from(), l.to());
                    box.NL();
                }
            }

            Tot(box, all, f, baseValue);

            if (keepNops)
            {
                b.NL(4);

                foreach (BoosterAbs<T> l in all)
                {
                    double d = l.get(f);
                    if (!l.isMul && d == 0)
                    {
                        l.hover(box, d);
                        BoosterAbs.hoverSpan(box, l.from(), l.to());
                        box.NL();
                    }
                }
                foreach (BoosterAbs<T> l in all)
                {
                    double d = l.get(f);
                    if (l.isMul && d == 1)
                    {
                        l.hover(box, d);
                        BoosterAbs.hoverSpan(box, l.from(), l.to());
                        box.NL();
                    }
                }
            }

            b.NL();
        }

        public static void Hover(GUI_BOX box, LIST<BoosterAbs<T>> all, T f, CharSequence name, double baseValue, bool keepNops)
        {
            GBox b = (GBox)box;
            HoverNoTot(box, all, f, name, baseValue, keepNops);

            b.NL(8);

            Tot(box, all, f, baseValue);
            b.NL(8);

            if (keepNops)
            {
                int t = 0;
                foreach (BoosterAbs<T> l in all)
                {
                    if (t > 1)
                    {
                        t = 0;
                        b.NL();
                    }
                    double d = l.get(f);
                    if (l.isMul && d == 1)
                    {
                        Hov(f, b, l, t++);
                    }
                    else if (!l.isMul && d == 0)
                        Hov(f, b, l, t++);
                }
            }

            b.NL();
        }

        public static void HoverNoTot(GUI_BOX box, LIST<BoosterAbs<T>> all, T f, CharSequence name, double baseValue, bool keepNops)
        {
            GBox b = (GBox)box;
            if (name != null)
                b.textLL(name);
            b.NL();

            sort.clear();
            int ii = 0;
            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);
                if (d > 0 && !l.isMul)
                {
                    sort.add(ii);
                }
                ii++;
            }
            int i = 0;
            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);
                if (d < 0 && !l.isMul)
                {
                    if (i < sort.size())
                    {
                        Hov(f, b, all.get(sort.get(i)), 0);
                        i++;
                    }
                    Hov(f, b, l, 1);
                    b.NL();
                }
            }
            for (; i < sort.size(); i++)
            {
                Hov(f, b, all.get(sort.get(i)), 0);
                b.NL();
            }

            b.NL(4);
            sort.clear();
            ii = 0;
            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);
                if (d > 1 && l.isMul)
                {
                    sort.add(ii);
                }
                ii++;
            }
            i = 0;
            foreach (BoosterAbs<T> l in all)
            {
                double d = l.get(f);
                if (d < 1 && l.isMul)
                {
                    if (i < sort.size())
                    {
                        Hov(f, b, all.get(sort.get(i)), 0);
                        i++;
                    }
                    Hov(f, b, l, 1);
                    b.NL();
                }
            }
            for (; i < sort.size(); i++)
            {
                Hov(f, b, all.get(sort.get(i)), 0);
                b.NL();
            }

            b.NL();
        }

        public static void Tot(GUI_BOX box, LIST<BoosterAbs<T>> all, T t, double baseValue)
        {
            double mul = 1;
            double padd = baseValue > 0 ? baseValue : 0;
            double sub = baseValue < 0 ? baseValue : 0;
            foreach (BoosterAbs<T> s in all)
            {
                if (s.isMul)
                    mul *= s.get(t);
                else
                {
                    double a = s.get(t);
                    if (a < 0)
                        sub += a;
                    else
                        padd += a;
                }
            }
            double tot = padd * mul + sub;

            GBox b = (GBox)box;
            b.tab(1);
            b.textL(Dic.¤¤Total);
            b.tab(5);

            b.add(GFORMAT.f0(b.text(), padd));
            b.add(b.text().add('*'));
            b.add(GFORMAT.f1(b.text(), mul));
            if (sub != 0)
                b.add(GFORMAT.f0(b.text(), sub));
            b.add(b.text().add('='));

            b.add(GFORMAT.fRel(b.text(), tot, baseValue));
            b.NL();
        }

        private static void Hov<T>(T f, GBox b, BoosterAbs<T> l, int tab)
        {
            l.hover(b, l.get(f), tab);
        }
    }
}