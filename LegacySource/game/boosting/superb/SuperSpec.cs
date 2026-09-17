using game.boosting;
using game.time;
using init.type;
using snake2d.util.misc;
using util.data.DOUBLE_O;
using System;

namespace game.boosting.superb
{
    public abstract class SuperSpec<T> : BoosterAbs<T> where T : SuperBoostableObj
    {
        private readonly double from;
        private readonly double to;
        public readonly string desc;
        public bool hidden = false;

        public SuperSpec(SuperBoostable<T> self, BSourceInfo info, string desc, double to, bool isMul) : this(self, info, desc, isMul ? 1 : 0, to, isMul) { }

        protected SuperSpec(SuperBoostable<T> self, BSourceInfo info, string desc, double from, double to, bool isMul) : base(info, isMul)
        {
            this.to = to;
            this.from = from;
            this.desc = desc;
            if (isMul)
            {
                from = 1;
            }
            else
            {
                from = 0;
            }
            self.all.Add(this);
        }

        public override double from() { return from; }

        public override double to() { return to; }

        public override double getValue(double input)
        {
            input = CLAMP.d(input, 0, 1);
            return from() + input * (to() - from());
        }

        public abstract double secondsRemaining(T bo);
        public abstract double increase(T bo);

        public double getModifier(T bo) { return 1; }

        public abstract class SuperSpecImp<T> : SuperSpec<T> where T : SuperBoostableObj
        {
            public readonly int index;
            public readonly string key;

            public SuperSpecImp(SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul) : base(self, info, desc, to, isMul)
            {
                while (self.map.ContainsKey(key))
                {
                    key += "0";
                }
                this.key = key;
                self.map.Add(this.key, this);
                index = self.ups.Add(this);
            }

            public readonly DOUBLE_OE<SuperBoostableObj> value = new DOUBLE_OE<SuperBoostableObj>()
            {
                getD = t => t.boostingData().values()[index],
                setD = (t, d) => { t.boostingData().values()[index] = CLAMP.d(d, 0, 1); return this; }
            };

            public readonly DOUBLE_OE<SuperBoostableObj> time = new DOUBLE_OE<SuperBoostableObj>()
            {
                getD = t => t.boostingData().times()[index],
                setD = (t, d) => { t.boostingData().times()[index] = d; return this; }
            };

            public readonly DOUBLE_OE<T> state = new DOUBLE_OE<T>()
            {
                getD = t => t.boostingData().states()[index],
                setD = (t, d) => { t.boostingData().states()[index] = d; return this; }
            };

            public void toggle(T bo) { activate(bo, !activated(bo)); }

            public abstract void update(T bo, double time);
            public abstract void activate(T bo, bool active);
            public abstract bool activated(T bo);
        }

        public class Permanent<T> : SuperSpecImp<T> where T : SuperBoostableObj
        {
            public Permanent(SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul) : base(self, key, info, desc, to, isMul) { }

            public override void update(T bo, double time) { }

            public override void activate(T bo, bool active) { value.setD(bo, active ? 1 : 0); }

            public override bool activated(T bo) { return value.getD(bo) > 0; }

            public override double secondsRemaining(T bo) { return 0; }

            public override double increase(T bo) { return 0; }

            protected override double pget(T bo) { return value.getD(bo); }
        }

        private class Wrap<T> : SuperSpec<T> where T : SuperBoostableObj
        {
            private readonly Booster boo;

            public Wrap(Booster b, SuperBoostable<T> self, string key, BSourceInfo info, string desc) : base(self, info, desc, b.to(), b.isMul)
            {
                this.boo = b;
            }

            public override double get(T o) { return boo.get(HCLASS_RACE.clP()); }

            protected override double pget(T bo) { return 0; }

            public override double secondsRemaining(T bo) { return 0; }

            public override double increase(T bo) { return 0; }
        }

        public class TimeLimit<T> : SuperSpecImp<T> where T : SuperBoostableObj
        {
            private readonly double seconds;

            public TimeLimit(double days, SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul) : base(self, key, info, desc, to, isMul)
            {
                this.seconds = days * TIME.secondsPerDay();
            }

            public override void update(T bo, double time)
            {
                if (this.time.getD(bo) > 0)
                    this.time.incD(bo, -time);
            }

            protected override double pget(T bo) { return time.getD(bo) > 0 ? 1 : 0; }

            public override void activate(T bo, bool active) { time.setD(bo, active ? seconds : 0); }

            public override bool activated(T bo) { return time.getD(bo) > 0; }

            public override double secondsRemaining(T bo) { return time.getD(bo); }

            public override double increase(T bo) { return 0; }
        }

        public class Downer<T> : SuperSpecImp<T> where T : SuperBoostableObj
        {
            private readonly double decreaseTime;
            private readonly double durationDays;

            public Downer(double daysToDecrease, SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul, double durationDays) : base(self, key, info, desc, to, isMul)
            {
                if (daysToDecrease == 0)
                    throw new Exception();
                this.decreaseTime = daysToDecrease * TIME.secondsPerDayI();
                this.durationDays = durationDays;
            }

            public override void update(T bo, double time)
            {
                if (this.time.getD(bo) > 0)
                {
                    this.time.incD(bo, -time);
                    return;
                }
                value.incD(bo, -time * decreaseTime);
            }

            public override void activate(T bo, bool active)
            {
                value.setD(bo, active ? 1.0 : 0);
                time.setD(bo, durationDays * TIME.secondsPerDay());
            }

            public override bool activated(T bo) { return value.getD(bo) > 0; }

            public override double secondsRemaining(T bo) { return time.getD(bo); }

            public override double increase(T bo) { return -decreaseTime * TIME.secondsPerDay(); }

            protected override double pget(T bo) { return value.getD(bo); }
        }

        public class Uper<T> : SuperSpecImp<T> where T : SuperBoostableObj
        {
            private readonly double decreaseTime;
            private readonly double maxTime;

            public Uper(double daysToIncrease, SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul) : this(self, key, info, desc, to, isMul, -1) { }

            public Uper(double daysToDecrease, SuperBoostable<T> self, string key, BSourceInfo info, string desc, double to, bool isMul, double maxDays) : base(self, key, info, desc, to, isMul)
            {
                if (daysToDecrease == 0)
                    throw new Exception();
                this.decreaseTime = daysToDecrease * TIME.secondsPerDayI();
                this.maxTime = maxDays * TIME.secondsPerDay();
            }

            public override void update(T bo, double time)
            {
                if (state.getD(bo) == 1)
                {
                    if (maxTime >= 0)
                    {
                        double t = this.time.incD(bo, -time).getD(bo);
                        if (t <= 0)
                        {
                            activate(bo, false);
                            return;
                        }
                    }

                    value.incD(bo, time * decreaseTime);
                }
            }

            protected override double pget(T bo)
            {
                if (state.getD(bo) == 0)
                    return 0;
                if (maxTime > 0 && time.getD(bo) <= 0)
                    return 0;
                return value.getD(bo);
            }

            public override void activate(T bo, bool active)
            {
                time.setD(bo, maxTime);
                value.setD(bo, 0);
                state.setD(bo, active ? 1 : 0);
            }

            public override bool activated(T bo) { return state.getD(bo) == 1; }

            public override double secondsRemaining(T bo) { return maxTime >= 0 ? time.getD(bo) : 0; }

            public override double increase(T bo) { return decreaseTime * TIME.secondsPerDay(); }
        }
    }
}