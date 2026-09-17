using System;
using System.Collections.Generic;
using System.Linq;

namespace game.faction.player
{
    public abstract class BoostCompound<T> : ACTION
    {
        private readonly BoostSpecs bos;
        private readonly LIST<T> all;
        private readonly List<Boo> boos = new List<Boo>();

        public BoostCompound(BoostSpecs bos, LIST<T> all)
        {
            this.bos = bos;
            this.all = all;

            BOOSTING.connecter(this);
        }

        protected abstract BoostSpecs bos(T t);

        protected abstract double getValue(T t);

        protected double get(Boostable bo, FactionNPC f, bool isMul)
        {
            return ((FactionNPC)f).bonus.getD(bo);
        }

        public void clearChache()
        {
            foreach (var b in boos)
                b.cacheI = GAME.updateI() - 100;
        }

        public void exe()
        {
            Dictionary<string, LinkedList<Value>> map = new Dictionary<string, LinkedList<Value>>();

            foreach (T t in all)
            {
                BoostSpecs bos = bos(t);

                foreach (BoostSpec s in bos.all())
                {
                    string k = s.boostable.key + s.booster.isMul;
                    if (!map.ContainsKey(k))
                    {
                        map[k] = new LinkedList<Value>();
                    }
                    map[k].Add(new Value(t, s));
                }
            }

            foreach (LinkedList<Value> l in map.Values)
            {
                Boo b = new Boo(l);
                bos.push(b, b.bo);
                b.bo.fGlobal.add(b);
            }
        }

        private class Boo : Booster
        {
            public readonly Boostable bo;
            private int cacheI = -1000;
            private double cache;
            private readonly BValue value;
            private readonly double from;
            private readonly double to;
            private readonly double defV;

            public Boo(LIST<Value> all) : base(bos.info, all.get(0).bo.booster.isMul)
            {
                this.bo = all.get(0).bo.boostable;
                defV = all.get(0).bo.booster.isMul ? 1 : 0;
                double from = 1;
                double to = 1;
                if (isMul)
                {
                    foreach (Value v in all)
                    {
                        double d = (v.bo.booster.getValue(1.0));
                        if (d < 1)
                            from *= d;
                        else
                            to *= 1 + d;
                    }
                }
                else
                {
                    from = 0;
                    to = 0;
                    foreach (Value v in all)
                    {
                        double d = (v.bo.booster.getValue(1.0));
                        if (d < 0)
                            from += d;
                        else
                            to += d;
                    }
                }

                this.from = from;
                this.to = to;

                this.value = new BValue.BValueFaction(bo)
                {
                    public double vGet(Player f)
                    {
                        int ci = GAME.updateI();
                        if (ci != cacheI)
                        {
                            cacheI = GAME.updateI();
                            cache = 0;
                            if (isMul)
                            {
                                foreach (Value v in all)
                                {
                                    cache += (v.bo.booster.getValue(BoostCompound.this.getValue(v.t)) - 1);
                                }
                                cache += 1;
                                cache = Math.Max(0, cache);
                            }
                            else
                            {
                                foreach (Value v in all)
                                {
                                    cache += v.bo.booster.getValue(BoostCompound.this.getValue(v.t));
                                }
                            }
                        }
                        return cache;
                    }

                    public double vGet(FactionNPC f)
                    {
                        return defV;
                    }
                };
            }

            public override double getValue(double input)
            {
                return input;
            }

            protected override double pget(BOOSTABLE_O o)
            {
                return o.boostableValue(value);
            }

            public override double from()
            {
                return from;
            }

            public override double to()
            {
                return to;
            }
        }

        private class Value
        {
            public readonly T t;
            public readonly BoostSpec bo;

            public Value(T t, BoostSpec bo)
            {
                this.t = t;
                this.bo = bo;
            }
        }
    }
}