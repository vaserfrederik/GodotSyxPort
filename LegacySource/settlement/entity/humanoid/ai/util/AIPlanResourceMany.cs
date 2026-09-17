using settlement.main;
using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using util.text;

namespace settlement.entity.humanoid.ai.util
{
    public abstract class AIPlanResourceMany
    {
        private readonly Resumer get;
        private static readonly CharSequence ¤¤fetch = "Fetching Resources";

        static
        {
            D.ts(typeof(AIPlanResourceMany));
        }

        public AIPlanResourceMany(AIPLAN.PLANRES p, int extraDistance)
        {
            get = p.new Resumer(¤¤fetch)
            {
                @Override
                protected AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return null;
                }

                @Override
                protected AISubActivation res(Humanoid a, AIManager d)
                {
                    int am = PATH().finders.resource.pickup(resource(a, d), d.path.destX(), d.path.destY(), d.planByte2);

                    if (d.resourceCarried() != resource(a, d))
                    {
                        d.resourceDrop(a);
                        d.resourceCarriedSet(resource(a, d));
                        if (am > 0)
                            am--;
                    }
                    d.resourceAInc(am);
                    d.planByte2 = 0;
                    int more = target(a, d) - d.resourceA();

                    if (more == 0)
                    {
                        return next(a, d);
                    }

                    int dist = extraDistance - extraDistance * d.resourceA() / target(a, d);

                    if (PATH().finders.resource.find(resource(a, d).bit, stored(d) ? resource(a, d).bit : RBIT.NONE, prio(d) ? resource(a, d).bit : RBIT.NONE, a.tc(), d.path, dist) == null)
                    {
                        return next(a, d);
                    }
                    d.planByte2 = 1;
                    AISubActivation s = AI.SUBS().walkTo.path(a, d);
                    if (s == null)
                    {
                        PATH().finders.resource.unreserve(resource(a, d), d.path.destX(), d.path.destY(), 1);
                        return next(a, d);
                    }

                    int extra = target(a, d) - d.resourceA() - d.planByte2;
                    extra = PATH().finders.resource.reserveExtra(stored(d), prio(d), resource(a, d), d.path.destX(), d.path.destY(), extra);
                    d.planByte2 += extra;

                    return s;
                }

                @Override
                public bool con(Humanoid a, AIManager d)
                {
                    int am = target(a, d) - d.resourceA() - d.planByte2;
                    if (am > 0)
                    {
                        d.planByte2 += (byte)PATH().finders.resource.reserveExtra(stored(d), prio(d), resource(a, d), d.path.destX(), d.path.destY(), am);
                    }

                    return d.resourceCarried() == resource(a, d) || PATH().finders.resource.isReservedAndAvailable(resource(a, d), d.path.destX(), d.path.destY());
                }

                @Override
                public void can(Humanoid a, AIManager d)
                {
                    PATH().finders.resource.unreserve(resource(a, d), d.path.destX(), d.path.destY(), d.planByte2);
                    d.resourceDrop(a);
                    cancel(a, d);
                }
            };
        }

        public RESOURCE resource(Humanoid a, AIManager d)
        {
            return RESOURCES.ALL().get(d.planByte4);
        }

        public int target(Humanoid a, AIManager d)
        {
            return d.planByte3 & 0b0011_1111;
        }

        private bool stored(AIManager d)
        {
            return (d.planByte3 & 0b01000000) != 0;
        }

        private bool prio(AIManager d)
        {
            return (d.planByte3 & 0b10000000) != 0;
        }

        private void init(AIManager d, int target, bool stored, bool prio)
        {
            d.planByte2 = 0;
            d.planByte3 = (byte)target;
            if (d.planByte3 <= 0 || d.planByte3 > 0b0011_1111)
                throw new RuntimeException("" + target);

            if (stored)
                d.planByte3 |= 0b01000000;

            if (prio)
                d.planByte3 |= 0b10000000;
        }

        public AISubActivation activate(Humanoid a, AIManager d, RBIT res, int target, int distance, bool stored, bool prio)
        {
            RESOURCE r = PATH().finders.resource.find(res, stored ? res : RBIT.NONE, prio ? res : RBIT.NONE, a.tc(), d.path, distance);
            if (r != null)
                return activateFound(a, d, r, target, stored, prio);
            init(d, target, stored, prio);
            d.resourceDrop(a);
            return null;
        }

        public AISubActivation activateFound(Humanoid a, AIManager d, RESOURCE res, int target, bool stored, bool prio)
        {
            init(d, target, stored, prio);
            d.resourceDrop(a);
            d.planByte4 = res.bIndex();
            AISubActivation s = AI.SUBS().walkTo.path(a, d);
            if (s == null)
            {
                PATH().finders.resource.unreserve(res, d.path.destX(), d.path.destY(), 1);
                return null;
            }
            d.planByte2 = 1;
            target--;
            if (target > 0)
            {
                d.planByte2 += PATH().finders.resource.reserveExtra(stored(d), prio(d), res, d.path.destX(), d.path.destY(), target);
            }
            get.set(a, d);

            return s;
        }

        public abstract void cancel(Humanoid a, AIManager d);

        public abstract AISubActivation next(Humanoid a, AIManager d);
    }
}