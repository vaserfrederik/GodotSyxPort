using settlement.entity.humanoid.ai.main;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using snake2d.util.rnd;
using System;

namespace settlement.entity.humanoid.ai.subwalk
{
    sealed class AISub_follow : PathWalker
    {
        public AISub_follow(string key, AISTATES.WALK_DEST state, string name) : base(key, state, name) { }

        public AISubActivation activate(Humanoid a, AIManager d, ENTITY other, byte tries)
        {
            d.planObject = other.id();
            d.subPathByte = tries;
            d.path.request(a.tc(), other.tc());
            if (d.path.isSuccessful())
                return activate(a, d);
            return activate(a, d, meet);
        }

        protected override bool hasFailed(Humanoid a, AIManager d)
        {
            if (SETT.ENTITIES().getByID(d.planObject) == null)
                return true;
            return false;
        }

        protected override void arrive(Humanoid a, AIManager d) { }

        protected override void abort(Humanoid a, AIManager d) { }

        protected override AISTATE setLast(Humanoid a, AIManager d)
        {
            return last.set(a, d);
        }

        public override AISTATE resume(Humanoid a, AIManager d)
        {
            if (RND.oneIn(5))
            {
                ENTITY prey = SETT.ENTITIES().getByID(d.planObject);
                if (prey == null)
                    return meet.set(a, d);
            }
            return base.resume(a, d);
        }

        public bool isSuccess(Humanoid a, AIManager d)
        {
            ENTITY prey = SETT.ENTITIES().getByID(d.planObject);
            if (prey == null)
                return false;

            int dx = prey.physics.tileC().x() - a.physics.tileC().x();
            int dy = prey.physics.tileC().y() - a.physics.tileC().y();
            int dist = Math.Abs(dx) + Math.Abs(dy);

            return dist <= 1;
        }

        private readonly Resumer last = new Resumer()
        {
            protected override AISTATE setAction(Humanoid a, AIManager d)
            {
                return res(a, d);
            }

            protected override AISTATE res(Humanoid a, AIManager d)
            {
                double m = a.speed.magnitude();

                a.speed.magnitudeInit(0);
                ENTITY prey = SETT.ENTITIES().getByID(d.planObject);

                if (prey == null)
                    return meet.set(a, d);

                int dx = prey.physics.tileC().x() - a.physics.tileC().x();
                int dy = prey.physics.tileC().y() - a.physics.tileC().y();
                int dist = Math.Abs(dx) + Math.Abs(dy);

                if (dist == 0)
                    return meet.set(a, d);

                if (dist < 5)
                {
                    AISTATE s = state.free(a, d, prey.body().cX(), prey.body().cY());
                    a.speed.magnitudeInit(m);
                    return s;
                }
                else
                {
                    d.path.request(a.tc(), prey.tc());
                    if (d.path.isSuccessful())
                    {
                        AISTATE s = activate(a, d).state();
                        a.speed.magnitudeInit(m);
                        return s;
                    }
                }
                return meet.set(a, d);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.COLLISION_TILE)
                {
                    ENTITY prey = SETT.ENTITIES().getByID(d.planObject);
                    if (prey == null)
                    {
                        d.overwrite(a, meet.set(a, d));
                        return false;
                    }
                    a.speed.magnitudeInit(0);
                    a.speed.magnitudeTargetSet(0);
                    d.path.request(a.tc(), prey.tc());
                    if (d.path.isSuccessful())
                    {
                        d.overwrite(a, activate(a, d));
                        return true;
                    }
                    d.overwrite(a, meet.set(a, d));
                    return true;
                }
                return base.event(a, d, e);
            }
        };

        private readonly Resumer collide = new Resumer()
        {
            public override AISTATE res(Humanoid a, AIManager d)
            {
                ENTITY prey = SETT.ENTITIES().getByID(d.planObject);
                if (prey == null)
                    return meet.set(a, d);

                d.path.request(a.tc(), prey.tc());
                if (d.path.isSuccessful())
                {
                    AISTATE s = activate(a, d).state();
                    return s;
                }
                return meet.set(a, d);
            }

            public override bool success(Humanoid a, AIManager d)
            {
                return true;
            }

            public override AISTATE setAction(Humanoid a, AIManager d)
            {
                a.speed.magnitudeInit(0);
                return AI.STATES().STAND.activate(a, d, 0.05);
            }

            public override void can(Humanoid a, AIManager d)
            {
                abort(a, d);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event != HEvent.COLLISION_SOFT)
                    base.event(a, d, e);
                return false;
            }
        };

        private readonly Resumer meet = new Resumer()
        {
            public override AISTATE res(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool success(Humanoid a, AIManager d)
            {
                return true;
            }

            public override AISTATE setAction(Humanoid a, AIManager d)
            {
                a.speed.magnitudeInit(0);
                return AI.STATES().STAND.activate(a, d, 0.1);
            }

            public override void can(Humanoid a, AIManager d)
            {
                abort(a, d);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event != HEvent.MEET_HARMLESS)
                    base.event(a, d, e);
                return false;
            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.MEET_HARMLESS)
            {
                ENTITY target = SETT.ENTITIES().getByID(d.planObject);
                if (target != null && target == e.other)
                    d.overwrite(a, meet.set(a, d));
            }
            else if (e.event == HEvent.COLLISION_SOFT)
            {
                d.overwrite(a, collide.set(a, d));
            }
            else
            {
                return base.event(a, d, e);
            }
            return false;
        }
    }
}