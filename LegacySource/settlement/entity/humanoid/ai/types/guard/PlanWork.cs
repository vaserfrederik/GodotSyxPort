using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.guard;
using settlement.room.main;
using settlement.stats;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.types.guard
{
    final class PlanWork : AIPLAN.PLANRES
    {
        protected PlanWork() : base("GUARD_GUARD")
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

        private final Resumer walk = new Resumer(SETT.ROOMS().GUARD.employment().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                GuardInstance ins = work(a);

                if (!ins.guardSpot(d.planTile, a.tc()))
                {
                    return null;
                }

                if (d.planTile.isSameAs(a.tc()))
                    return guard.set(a, d);

                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);

                if (s == null)
                    ins.guardSpotReturn(d.planTile.x(), d.planTile.y());

                return s;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return guard.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return work(a) != null;
            }

            public override void can(Humanoid a, AIManager d)
            {
                GuardInstance ins = work(a);
                if (ins != null)
                    ins.guardSpotReturn(d.planTile.x(), d.planTile.y());
            }
        };

        private final Resumer guard = new Resumer(SETT.ROOMS().GUARD.employment().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                GuardInstance ins = work(a);
                a.speed.turn2(ins.guardDir(a.tc().x(), a.tc().y()));
                d.planByte1 = (byte)(2 + RND.rInt(5));

                if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) > 0.8)
                {
                    d.planByte2 = 1;
                    can(a, d);
                    d.planByte2 = 0;
                }
                else
                    d.planByte2 = 1;

                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 5 + RND.rInt(5)));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                GuardInstance ins = work(a);

                if (!AIModules.current(d).moduleCanContinue(a, d) || STATS.WORK().WORK_TIME.indu().getD(a.indu()) > 1.0)
                {
                    can(a, d);
                    return null;
                }

                if (d.planByte2 == 0)
                {
                    if (SETT.ENTITIES().getAtTileSingle(a.tc().x(), a.tc().y()) is Humanoid)
                    {
                        return null;
                    }
                }

                Humanoid c = SETT.ROOMS().GUARD.reporter.pollCriminal(ins);

                if (c != null)
                {
                    can(a, d);
                    return d.resumeOtherPlan(a, AI.listeners().catchCriminal(c));
                }

                d.planByte1--;

                if (d.planByte1 <= 0)
                {
                    can(a, d);
                    if (SETT.ROOMS().GUARD.instancesSize() > 1 && RND.oneIn(10) && STATS.WORK().WORK_TIME.indu().get(a.indu()) <= 0.5)
                        return patrol.set(a, d);
                    return null;
                }
                a.speed.turn2(ins.guardDir(a.tc().x(), a.tc().y()));
                return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 15 + RND.rInt(5)));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return work(a) != null;
            }

            public override void can(Humanoid a, AIManager d)
            {
                if (d.planByte2 == 1)
                {
                    GuardInstance ins = work(a);
                    if (ins != null)
                        ins.guardSpotReturn(d.planTile.x(), d.planTile.y());
                }
            }
        };

        private final Resumer patrol = new Resumer(SETT.ROOMS().GUARD.employment().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                int i = RND.rInt(SETT.ROOMS().GUARD.instancesSize());
                if (SETT.ROOMS().GUARD.getInstance(i) == work(a))
                {
                    i++;
                    i %= SETT.ROOMS().GUARD.instancesSize();
                }

                d.planByte1 = (byte)(2 + RND.rInt(5));
                return AI.SUBS().walkTo.room(a, d, SETT.ROOMS().GUARD.getInstance(i));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                d.planByte1--;
                if (d.planByte1 <= 0)
                {
                    can(a, d);
                    return null;
                }
                a.speed.setRaw(a.speed.dir().next(1 * (RND.rBoolean() ? 1 : -1)), 0);
                return AI.SUBS().STAND.activateTime(a, d, 5 + RND.rInt(5));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return work(a) != null;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.NOTIFY_CRIME)
            {
                if (e.other is Humanoid)
                {
                    d.overwrite(a, AI.listeners().catchCriminal((Humanoid)e.other));
                    return true;
                }
            }
            return base.event(a, d, e);
        }

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
                return 1.0;
            return base.poll(a, d, e);
        }
    }
}