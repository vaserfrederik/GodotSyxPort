using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.execution;
using settlement.room.law.guard;
using settlement.room.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.guard
{
    final class PlanExecute : AIPLAN.PLANRES
    {
        private static CharSequence ¤¤name = "Executing Prisoners";

        static
        {
            D.ts(PlanExecute.class);
        }

        protected PlanExecute() : base("GUARD_EXECUTE")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return walk.set(a, d);
        }

        static GuardInstance work(Humanoid a)
        {
            RoomInstance ins = STATS.WORK().EMPLOYED.get(a);
            if (ins != null && ins is GuardInstance)
                return (GuardInstance)ins;
            return null;
        }

        private Guard s(AIManager d)
        {
            return SETT.ROOMS().EXECUTION.stations.guard(d.planTile.x(), d.planTile.y());
        }

        private ExecutionStation ss()
        {
            return SETT.ROOMS().EXECUTION.stations;
        }

        final Resumer walk = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Guard g = SETT.ROOMS().GUARD.reporter.pollExecution(work(a));
                if (g == null)
                    return null;

                d.planTile.set(g.coo());

                AISubActivation s = AI.SUBS().walkTo.coo(a, d, d.planTile);

                if (s == null)
                {
                    SETT.ROOMS().GUARD.reporter.reportExecution(g.coo().x(), g.coo().y());
                    return null;
                }
                return s;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return wait.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                Guard s = s(d);
                if (s == null || !s.active())
                    return false;
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                Guard s = s(d);
                if (s != null && s.active())
                {
                    SETT.ROOMS().GUARD.reporter.reportExecution(s.coo().x(), s.coo().y());
                }
            }
        };

        private final Resumer wait = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(2 + RND.rInt(5));
                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 5 + RND.rInt(5)));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte1-- <= 0)
                {
                    return execute.set(a, d);
                }
                a.speed.turn2(DIR.get(a.tc(), d.planTile).next(-1 + RND.rInt(3)));
                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 5 + RND.rInt(5)));
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

        private final Resumer execute = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                int code = ss().type(d.planTile.x(), d.planTile.y());

                if (code == ExecutionStation.TYPE_CHOP)
                {
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.work.activate(a, d));
                }
                else
                {
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.grab.activate(a, d));
                }
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                Guard s = s(d);
                if (s == null || !s.active())
                    return after.set(a, d);

                if (s.workExecute())
                {
                    return set(a, d);
                }
                else
                {
                    return after.set(a, d);
                }
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

        private final Resumer after = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(2 + RND.rInt(5));
                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 5 + RND.rInt(5)));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte1-- <= 0)
                    return null;
                a.speed.turn2(DIR.get(a.tc(), d.planTile).next(-1 + RND.rInt(3)));
                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 5 + RND.rInt(5)));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                return;
            }
        };

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
                return 1.0;
            return base.poll(a, d, e);
        }
    }
}