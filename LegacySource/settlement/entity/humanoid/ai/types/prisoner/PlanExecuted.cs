using System;
using settlement.entity.humanoid.ai.types.prisoner;
using game.time;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.execution;
using settlement.stats;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class PlanExecuted : AIPLAN.PLANRES
    {
        public PlanExecuted() : base("prisExe")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            Client s = SETT.ROOMS().EXECUTION.stations.exectuionReserve();
            if (s == null)
                return null;
            d.planTile.set(s.coo());
            return walk.set(a, d);
        }

        private readonly Resumer walk = new Resumer(CRIME_PUNISHMENTS.EXECUTE().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);
                if (s != null)
                    return s;
                cancel(a, d);
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return ready.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                Client s = s(d);
                if (s == null || !s.clientReserved())
                    return false;
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                Client s = s(d);
                if (s != null)
                    s.clientCancel();
            }
        };

        private Client s(AIManager d)
        {
            return SETT.ROOMS().EXECUTION.stations.client(d.planTile.x(), d.planTile.y());
        }

        private ExecutionStation ss()
        {
            return SETT.ROOMS().EXECUTION.stations;
        }

        private readonly Resumer ready = new Resumer(CRIME_PUNISHMENTS.EXECUTE().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Client s = s(d);
                s.clientUse();
                d.planByte1 = (byte)TIME.hours().bitCurrent();
                d.planByte2 = (byte)TIME.days().bitCurrent();
                return res(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte2 != TIME.days().bitCurrent() && TIME.hours().bitCurrent() > d.planByte1)
                {
                    walk.can(a, d);
                    return null;
                }

                if (!walk.con(a, d))
                {
                    walk.can(a, d);
                    return null;
                }

                Client s = s(d);
                a.speed.setDirCurrent(s.clientDir());

                int type = ss().type(d.planTile.x(), d.planTile.y());

                if (type == ExecutionStation.TYPE_CHOP)
                {
                    return AI.SUBS().LAY.activateTime(a, d, 1);
                }

                if (s.clientBeingExecuted())
                {
                    if (type == ExecutionStation.TYPE_HANG)
                    {
                        return strangled.set(a, d);
                    }
                    else if (type == ExecutionStation.TYPE_GIBBET)
                        return gibbited.set(a, d);
                    else if (type == ExecutionStation.TYPE_CROSS)
                        return crucified.set(a, d);
                }
                return AI.SUBS().STAND.activateTime(a, d, 1);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                walk.can(a, d);
            }
        };

        private readonly Resumer strangled = new Resumer(CRIME_PUNISHMENTS.EXECUTE().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.strangle, 10 + RND.rInt(40));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                AIManager.dead = CAUSE_LEAVES.EXECUTED();
                cancel(a, d);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return walk.con(a, d);
            }

            public override void can(Humanoid a, AIManager d)
            {
                walk.can(a, d);
            }
        };

        private readonly Resumer gibbited = new Resumer(CRIME_PUNISHMENTS.EXECUTE().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.set(a.indu(), 1);
                return AI.SUBS().STAND.activateRndDir(a, d, 45);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                NEEDS.TYPES().HUNGER.stat().stat().indu().inc(a.indu(), 1);

                if (STATS.FOOD().STARVATION.indu().get(a.indu()) > 0)
                {
                    AIManager.dead = CAUSE_LEAVES.EXECUTED();
                }

                return AI.SUBS().STAND.activateRndDir(a, d, 45);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return walk.con(a, d);
            }

            public override void can(Humanoid a, AIManager d)
            {
                walk.can(a, d);
            }
        };

        private readonly Resumer crucified = new Resumer(CRIME_PUNISHMENTS.EXECUTE().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.armsOut, 10 + RND.rInt(40));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                STATS.NEEDS().INJURIES.COUNT.indu().inc(a.indu(), 10);
                SETT.THINGS().gore.bleed(a, a.race().appearance().colors.blood);

                if (STATS.NEEDS().INJURIES.COUNT.indu().isMax(a.indu()))
                {
                    AIManager.dead = CAUSE_LEAVES.EXECUTED();
                }
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.armsOut, 10 + RND.rInt(40));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return walk.con(a, d);
            }

            public override void can(Humanoid a, AIManager d)
            {
                walk.can(a, d);
            }
        };
    }
}