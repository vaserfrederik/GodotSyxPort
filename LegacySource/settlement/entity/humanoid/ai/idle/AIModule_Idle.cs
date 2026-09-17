using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.entity.humanoid.ai.main.AIPLAN;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.idle
{
    public sealed class AIModule_Idle : AIModule
    {
        private static readonly CharSequence ¤¤name = "Biding Time";
        private static readonly CharSequence ¤¤name2 = "Bide Time";

        static AIModule_Idle()
        {
            D.ts(typeof(AIModule_Idle));
        }

        public AIModule_Idle() : base(UI.icons().s.cancel, ¤¤name2, ¤¤name)
        {
        }

        private readonly PlanInterract inter = new PlanInterract();

        public AIPLAN interract()
        {
            return inter.interract;
        }

        private readonly AIPLAN plan = new AIPLAN.PLANRES("idlePlan")
        {
            private readonly SubStand sub = new SubStand(this, "idlestand");
            private readonly SubMove walk = new SubMove("idlewalk");

            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                return start.set(a, d);
            }

            private readonly Resumer start = new Resumer(¤¤name)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (PATH().finders.getOutofWay.shouldFind(a))
                    {
                        PATH().finders.getOutofWay.request(a, d.path);
                        return walking.set(a, d);
                    }

                    if (RND.oneIn(10))
                        return walk.activate(a, d);
                    return sub.activate(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (moduleCanContinue(a, d))
                    {
                        if (RND.oneIn(15))
                            return walk.activate(a, d);
                        return sub.activate(a, d);
                    }
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }

                public override double poll(Humanoid a, AIManager d, HPollData e)
                {
                    if (e.type == HPoll.CAN_INTERRACT && a.speed.isZero())
                        return 1.0;
                    return base.poll(a, d, e);
                }
            };

            private readonly Resumer walking = new Resumer(¤¤name)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activateRndDir(a, d, 1 + RND.rInt(4));
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (moduleCanContinue(a, d))
                    {
                        if (PATH().finders.getOutofWay.checkAndSetRequest(a.tc().x(), a.tc().y(), d.path))
                        {
                            return exit.set(a, d);
                        }
                        return AI.SUBS().STAND.activateRndDir(a, d, 1 + RND.rInt(4));
                    }
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }
            };

            private readonly Resumer exit = new Resumer()
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (d.path.isSuccessful())
                    {
                        return AI.SUBS().walkTo.pathFull(a, d);
                    }
                    else
                    {
                        if (RND.oneIn(15))
                            return walk.activate(a, d);
                        return sub.activate(a, d);
                    }
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }
            };
        };

        private readonly AIPLAN bench = new AIPLAN.PLANRES("idleBench")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                int r = (int)(STATS.RAN().get(a.indu(), 39) + TIME.hours().bitsSinceStart());
                r &= 0x0FF;

                if (r > (0xFF >> 1))
                {
                    FINDABLE ff = SETT.ROOMS().BENCH.finder.getReservable(a.tc().x(), a.tc().y());
                    if (ff != null)
                    {
                        d.planTile.set(ff);
                        ff.findableReserve();
                        walk.set(a, d);
                        return AI.SUBS().STAND.activateTime(a, d, 1);
                    }
                    AISubActivation s = AI.SUBS().walkTo.serviceInclude(a, d, SETT.ROOMS().BENCH.finder, SETT.ROOMS().BENCH.radius());
                    if (s != null)
                    {
                        d.planTile.set(d.path.destX(), d.path.destY());
                        walk.set(a, d);
                        return s;
                    }
                    if (!SETT.ROOMS().map.is(a.tc()) && SETT.FLOOR().getter.is(a.tc()))
                    {
                        STATS.SERVICE().bench.clearAccess(a.indu());
                    }
                }
                return null;
            }

            private readonly Resumer walk = new Resumer(¤¤name)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = 16;
                    return null;
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (SETT.ROOMS().BENCH.finder.getReserved(d.planTile.x(), d.planTile.y()) == null)
                    {
                        return null;
                    }
                    STATS.SERVICE().bench.access.indu().set(a.indu(), 1);
                    Room bb = (SETT.ROOMS().BENCH.get(d.planTile.x(), d.planTile.y()));
                    STATS.SERVICE().bench.setAccess(a.indu(), true, 1.0 - bb.getDegrade(d.planTile.x(), d.planTile.y()), bb.upgrade(d.planTile.x(), d.planTile.y()));
                    if (d.planByte1-- < 0 || !moduleCanContinue(a, d))
                    {
                        can(a, d);
                        return null;
                    }

                    DIR dir = SETT.ROOMS().BENCH.benchDir(d.planTile.x(), d.planTile.y(), a.speed.dir());
                    if (RND.oneIn(4))
                        dir = dir.next((int)RND.rSign());
                    a.speed.setDirCurrent(dir);
                    return AI.SUBS().STAND.activateTime(a, d, 1 + RND.rInt(10));
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    FINDABLE s = SETT.ROOMS().BENCH.finder.getReserved(d.planTile.x(), d.planTile.y());
                    if (s != null)
                        s.findableReserveCancel();
                }

                public override double poll(Humanoid a, AIManager d, HPollData e)
                {
                    if (e.type == HPoll.CAN_INTERRACT && a.speed.isZero())
                        return 1.0;
                    return base.poll(a, d, e);
                }
            };
        };

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            AiPlanActivation p = bench.activate(a, d);
            if (p != null)
                return p;

            if (PATH().finders.getOutofWay.shouldFind(a))
                return plan.activate(a, d);
            double b = BOOSTABLES.ACTIVITY().SOCIAL.get(a.indu());
            if (b > 0 && RND.oneIn(5 / b))
            {
                p = inter.lookForFriend.activate(a, d);
                if (p != null)
                    return p;
            }
            return plan.activate(a, d);
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            return 0;
        }
    }
}