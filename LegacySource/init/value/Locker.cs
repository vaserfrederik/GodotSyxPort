using System;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;

namespace Init.Value
{
    public abstract class Locker<T>
    {
        public readonly ICharSequence Name;
        public readonly SPRITE Icon;

        public Locker(ICharSequence name, SPRITE icon)
        {
            Name = name;
            Icon = icon;
        }

        public abstract bool InUnlocked(T t);

        public double Progress(T t)
        {
            return InUnlocked(t) ? 1 : 0;
        }

        public void Hover(GUI_BOX text, T t)
        {
            GBox b = (GBox)text;
            b.Add(Icon);
            GText te = b.Text();
            te.SetMaxWidth(250);
            te.SetMultipleLines(true);

            if (InUnlocked(t))
            {
                te.Normalify2().Add(Name);
            }
            else
            {
                te.Errorify().Add(Name);
            }
            te.AdjustWidth();
            b.Add(te);
            b.NL();
        }

        private sealed class LockerValue<T> : Locker<T>
        {
            public readonly Value<T> Getter;
            public readonly COMPARATOR Comp;
            public readonly double Value;

            public LockerValue(COMPARATOR comp, Value<T> getter, double value, SPRITE icon) : base(getter.Name, icon)
            {
                Getter = getter;
                Comp = comp;
                Value = value;
            }

            public override bool InUnlocked(T t)
            {
                return Comp.Passes(Getter.D.GetD(t), Value);
            }

            public override void Hover(GUI_BOX text, T t)
            {
                GBox b = (GBox)text;
                GText name = b.Text();
                name.Add(this.Name);
                name.SetMaxWidth(230);
                name.SetMultipleLines(true);
                GText va = b.Text();
                GText cu = b.Text();
                cu.Add('(');

                if (Getter.IsBool)
                {
                    GFORMAT.Bool(va, Value == 1);
                    GFORMAT.Bool(cu, Getter.D.GetD(t) == 1);
                }
                else if (Getter.Percentage)
                {
                    GFORMAT.Perc(va, Value);
                    GFORMAT.Perc(cu, Getter.D.GetD(t));
                }
                else
                {
                    if ((int)Value == Value)
                    {
                        GFORMAT.IBig(va, (int)Value);
                    }
                    else
                    {
                        GFORMAT.F(va, Value);
                    }
                    if ((int)Getter.D.GetD(t) == Getter.D.GetD(t))
                    {
                        GFORMAT.IBig(cu, (int)Getter.D.GetD(t));
                    }
                    else
                    {
                        GFORMAT.F(cu, Getter.D.GetD(t));
                    }
                }

                GText co = b.Text();
                co.Add(Comp.Rep);

                if (InUnlocked(t))
                {
                    name.Normalify2();
                    va.Normalify2();
                    co.Normalify2();
                }
                else
                {
                    name.Errorify();
                    va.Errorify();
                    co.Errorify();
                }

                b.Add(name);
                b.Tab(6);
                b.Add(co);
                b.Tab(7);
                b.Add(va);
                b.Tab(9);
                cu.Add(')');
                cu.Normalify();
                b.Add(cu);
            }

            public override double Progress(T t)
            {
                return Comp.Progress(Getter.D.GetD(t), Value);
            }
        }
    }
}