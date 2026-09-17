using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using settlement.stats.service;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.service
{
    class PlanSkinny : S_Plan
    {
        private static readonly CharSequence ¤¤verb = "¤Skinny dipping";

        static PlanSkinny()
        {
            D.ts(typeof(PlanSkinny));
        }

        private readonly StatServiceSimple stat = STATS.SERVICE().skinnyDip;

        public PlanSkinny() : base(STATS.SERVICE().skinnyDip, 1.0)
        {
        }

        public override bool hasAccess(Humanoid a, AIManager d)
        {
            return stat.access(a);
        }

        public override bool allowed(Humanoid a, AIManager d)
        {
            return stat.permission().is(a.indu().popCL());
        }

        public override bool goodTime(Humanoid a, AIManager d)
        {
            if (SETT.WEATHER().ice.canBatheOutside())
            {
                return true;
            }
            return false;
        }

        private int dist;

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            dist = 128;
            return skinnydip.activate(a, d);
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d, int dist)
        {
            this.dist = dist;
            return skinnydip.activate(a, d);
        }

        public readonly AIPLAN skinnydip = new AIPLAN.PLANRES("ser" + stat.total().key())
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                if (SETT.WEATHER().ice.canBatheOutside() && PATH().finders.water.has(a.tc()))
                {
                    return walkToWater.set(a, d);
                }
                return null;
            }

            private readonly AISUB sub = new AISUB.Simple("")
            {
                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;

                    if (!a.speed.isZero())
                        a.speed.magnitudeInit(0);

                    if (d.subByte > 1)
                    {
                        STATS.NEEDS().EXPOSURE.fix(a.indu());
                        STATS.NEEDS().DIRTINESS.set(a.indu(), 0);
                    }

                    if (d.subByte > 20)
                        return null;

                    if (RND.oneIn(5))
                    {
                        DIR dir = DIR.ALL.get(RND.rInt(DIR.ALL.size()));

                        for (int i = 0; i < 8; i++)
                        {
                            int x = a.physics.tileC().x() + dir.x();
                            int y = a.physics.tileC().y() + dir.y();
                            if (SETT.PATH().coster.player.getCost(a.tc().x(), a.tc().y(), x, y) > 0 && PATH().finders.water.get(x, y) != null)
                            {
                                return AI.STATES().WALK2.dirTile(a, d, dir);
                            }
                            dir = dir.next(1);
                        }
                    }

                    if (RND.oneIn(3))
                        return AI.STATES().STAND.aDirRND(a, d, 1 + RND.rFloat(2));
                    return AI.STATES().LAY.activate(a, d, 1 + RND.rFloat(5));
                }
            };

            private readonly Resumer walkToWater = new Resumer(¤¤verb)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (PATH().finders.water.reserve(a.physics.tileC(), d.path, dist))
                    {
                        AISubActivation ss = AI.SUBS().walkTo.pathFull(a, d);
                        if (ss != null)
                        {
                            stat.setAccess(a, true);
                            return ss;
                        }
                        can(a, d);
                    }
                    stat.setAccess(a, false);
                    return null;
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    FINDABLE s = PATH().finders.water.get(d.path.destX(), d.path.destY());
                    if (s == null)
                        return null;
                    if (!s.findableReservedIs())
                    {
                        if (!s.findableReservedCanBe())
                            return null;
                        s.findableReserve();
                    }
                    return bathe.set(a, d);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    FINDABLE s = PATH().finders.water.getReserved(d.path.destX(), d.path.destY());
                    if (s != null)
                        s.findableReserveCancel();
                    STATS.POP().NAKED.set(a.indu(), 0);
                }
            };

            private readonly Resumer bathe = new Resumer(¤¤verb)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    STATS.POP().NAKED.set(a.indu(), 1);
                    STATS.NEEDS().EXPOSURE.fix(a.indu());
                    d.planByte1 = (byte)(5 + RND.rInt(10));
                    return sub.activate(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    STATS.NEEDS().EXPOSURE.fix(a.indu());
                    STATS.NEEDS().DIRTINESS.set(a.indu(), 0);

                    if (!conn(a, d))
                    {
                        can(a, d);
                        return null;
                    }

                    if (d.planByte1-- > 0 && AIModules.current(d) != null && AIModules.current(d).moduleCanContinue(a, d) && SETT.WEATHER().ice.canBatheOutside())
                    {
                        return sub.activate(a, d);
                    }
                    can(a, d);
                    return null;
                }

                private bool conn(Humanoid a, AIManager d)
                {
                    FINDABLE s = PATH().finders.water.getReserved(d.path.destX(), d.path.destY());
                    return s != null && s.findableReservedIs();
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    FINDABLE s = PATH().finders.water.getReserved(d.path.destX(), d.path.destY());
                    if (s != null)
                        s.findableReserveCancel();
                    STATS.POP().NAKED.set(a.indu(), 0);
                }
            };
        };
    }
}