using System;
using settlement.entity.humanoid.ai.work;
using init.constant;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.job;
using settlement.room.law.police;
using settlement.stats;
using util.text;

final class WorkPolice : WorkAbs
{
    private static CharSequence ¤¤finding = "¤finding suspect";
    private static CharSequence ¤¤bringing = "¤bringing suspect back";
    private static CharSequence ¤¤being = "¤being interrogated";
    static
    {
        D.ts(typeof(WorkPolice));
    }

    private readonly ROOM_POLICE b = SETT.ROOMS().POLICE;

    protected WorkPolice(AIModule_Work module, PlanBlueprint[] map, Works works) : base(module, SETT.ROOMS().POLICE, map, works)
    {
    }

    protected override AISubActivation initBegin(Humanoid a, AIManager d, SETT_JOB j, JOB_MANAGER jm)
    {
        d.planTile.set(j.jobCoo());

        if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) > 0.5)
        {
            Humanoid victim = b.work.clientToFetch(j.jobCoo().x(), j.jobCoo().y());

            if (victim != null)
            {
                d.planObject = victim.id();

                AISubActivation s = AI.SUBS().walkTo.follow(a, d, victim, false, (byte)20);
                if (s != null)
                {
                    work(d).jobReserve(null);
                    b.work.deliverClient(d.planTile.x(), d.planTile.y());
                    findingSuspect.set(a, d);
                    return s;
                }
            }
        }

        return base.initBegin(a, d, j, jm);
    }

    private Humanoid victim(AIManager d)
    {
        ENTITY prey = SETT.ENTITIES().getByID(d.planObject);
        if (prey != null && prey is Humanoid)
            return (Humanoid)prey;
        return null;
    }

    private SETT_JOB work(AIManager d)
    {
        return b.work.job(d.planTile.x(), d.planTile.y());
    }

    private bool reserved(AIManager d)
    {
        SETT_JOB j = work(d);
        return j != null && j.jobReservedIs(null);
    }

    private readonly Resumer findingSuspect = new Resumer(¤¤finding)
    {
        protected override AISubActivation setAction(Humanoid a, AIManager d)
        {
            return null;
        }

        protected override AISubActivation res(Humanoid a, AIManager d)
        {
            if (!reserved(d))
            {
                can(a, d);
                return null;
            }

            if (AI.SUBS().walkTo.followSucess(a, d))
            {
                return knockSuspect.set(a, d);
            }
            else if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) < 0.5)
            {
                AISubActivation s = AI.SUBS().walkTo.follow(a, d, victim(d), false, (byte)20);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }

        public override bool con(Humanoid a, AIManager d)
        {
            return true;
        }

        public override void can(Humanoid a, AIManager d)
        {
            SETT_JOB j = work(d);
            if (j != null && j.jobReservedIs(null))
                j.jobReserveCancel(null);
        }

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.MEET_HARMLESS)
            {
                if (e.other == victim(d))
                {
                    d.overwrite(a, knockSuspect.set(a, d));
                    return true;
                }
            }
            return base.event(a, d, e);
        }
    };

    private readonly Resumer knockSuspect = new Resumer(¤¤finding)
    {
        protected override AISubActivation setAction(Humanoid a, AIManager d)
        {
            Humanoid e = victim(d);
            if (e == null)
            {
                findingSuspect.can(a, d);
                return null;
            }
            if (!reserved(d))
            {
                can(a, d);
                return null;
            }
            a.speed.turn2(a.body(), e.body());
            a.speed.magnitudeInit(0);
            return AI.SUBS().single.activate(a, d, AI.STATES().anima.box, AI.STATES().anima.box.time);
        }

        protected override AISubActivation res(Humanoid a, AIManager d)
        {
            return dragBack.set(a, d);
        }

        public override bool con(Humanoid a, AIManager d)
        {
            return true;
        }

        public override void can(Humanoid a, AIManager d)
        {
            SETT_JOB j = work(d);
            if (j != null && j.jobReservedIs(null))
                j.jobReserveCancel(null);
        }
    };

    internal readonly Resumer dragBack = new Resumer(¤¤bringing)
    {
        protected override AISubActivation setAction(Humanoid a, AIManager d)
        {
            Humanoid v = victim(d);
            if (v == null || !reserved(d))
            {
                can(a, d);
                return null;
            }
            victim(d).knockOut();
            AISubActivation s = AI.SUBS().walkTo.drag(a, d, SETT.HUMANOIDS().draggable, v.id(), d.planTile);
            if (s != null)
                return s;
            victim(d).interrupt();
            return null;
        }

        protected override AISubActivation res(Humanoid a, AIManager d)
        {
            SETT_JOB j = work(d);

            if (!reserved(d))
            {
                can(a, d);
                return null;
            }
            j.jobPerform(a, null, 0);
            Humanoid v = victim(d);
            if (v == null)
            {
                can(a, d);
                return null;
            }

            v.physics.body().moveC(j.jobCoo().x() * C.TILE_SIZE + C.TILE_SIZEH, j.jobCoo().y() * C.TILE_SIZE + C.TILE_SIZEH);

            AIManager d2 = (AIManager)v.ai();
            d2.overwrite(v, plan);

            return null;
        }

        public override bool con(Humanoid a, AIManager d)
        {
            return true;
        }

        public override void can(Humanoid a, AIManager d)
        {
            SETT_JOB j = work(d);
            if (j != null && j.jobReservedIs(null))
                j.jobReserveCancel(null);
            if (victim(d) != null)
                victim(d).interrupt();
        }
    };

    private readonly PLANRES plan = new PLANRES("POLICE_IMPRISONED")
    {
        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            d.planTile.set(a.tc());
            STATS.WORK().incap.stat.indu().set(a.indu(), 1);
            STATS.WORK().EMPLOYED.set(a, null);
            return res.set(a, d);
        }

        internal readonly Resumer res = new Resumer(¤¤being)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.set(a.indu(), 1);

                a.speed.setDirCurrent(b.work.victimDir(d.planTile.x(), d.planTile.y()));
                if (b.work.isLay(d.planTile.x(), d.planTile.y()))
                {
                    return AI.SUBS().LAY.activateTime(a, d, 40);
                }
                return AI.SUBS().STAND.activateTime(a, d, 40);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                SETT_JOB j = work(d);

                if (j == null)
                    return null;

                STATS.NEEDS().DIRTINESS.incD(a.indu(), 0.1);

                a.speed.setDirCurrent(b.work.victimDir(d.planTile.x(), d.planTile.y()));
                if (b.work.isLay(d.planTile.x(), d.planTile.y()))
                {
                    return AI.SUBS().LAY.activateTime(a, d, 40);
                }
                return AI.SUBS().STAND.activateTime(a, d, 40);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                return false;
            }

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                return 0;
            }
        };
    };
}