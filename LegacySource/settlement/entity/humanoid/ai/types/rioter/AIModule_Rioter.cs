using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.battle;
using settlement.entity.humanoid.ai.crime;
using settlement.entity.humanoid;
using settlement.entity;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.rioter
{
    public sealed class AIModule_Rioter : AIModule
    {
        private static readonly CharSequence ¤¤verb = "¤Rioting";
        private static readonly CharSequence ¤¤name = "¤Riot";

        static
        {
            D.ts(AIModule_Rioter.class);
        }

        public AIModule_Rioter() : base(UI.icons().s.degrade, ¤¤name, null)
        {
        }

        private readonly Animation[] anima = new Animation[] {
            AI.STATES().anima.fist,
            AI.STATES().anima.grab,
            AI.STATES().anima.lay,
        };

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            return planRout.activate(a, d);
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
        {
            // TODO Auto-generated method stub
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            return 11;
        }

        { D.gInit(this); }

        private readonly AIPLAN planRout = new AIPLAN.PLANRES("riot")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                d.planByte1 = 0;
                return go.set(a, d);
            }

            private readonly SubFlee sub = new SubFlee();

            private readonly Resumer go = new Resumer(¤¤verb)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    AIModule_Crime.notify(a);
                    SETT.ROOMS().GUARD.reporter.reportCriminal(a);
                    return sub.activate(a, d, 1);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (a.indu().hType() != HTYPES.RIOTER())
                    {
                        return null;
                    }

                    d.planByte1 -= 5;
                    if (d.planByte1 < 0)
                        d.planByte1 = 0;

                    AIModule_Crime.notify(a);
                    if (RND.oneIn(5))
                        SETT.ROOMS().GUARD.reporter.reportCriminal(a);
                    if (RND.rBoolean())
                    {
                        int ri = RND.rInt(DIR.ORTHO.size());
                        for (int i = 0; i < DIR.ORTHO.size(); i++)
                        {
                            DIR dd = DIR.ORTHO.getC(ri + i);
                            if (SETT.IN_BOUNDS(a.tc(), dd) && SETT.PATH().cost.get(a.tc().x(), a.tc().y(), dd) > 0)
                            {
                                a.speed.turn2(dd);
                                return AI.SUBS().single.activate(a, d, anima[RND.rInt(anima.Length)].activate(a, d, 2 + RND.rInt(5)));
                            }
                        }
                    }

                    if (RND.rBoolean())
                        return AI.SUBS().STAND.activateTime(a, d, 2 + RND.rInt(5));

                    return sub.activate(a, d, 1);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    // TODO Auto-generated method stub
                }
            };

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                return InterBattle.pollReady(a, d, e);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.CHECK_MORALE)
                    return false;
                return InterBattle.listener.event(a, d, e);
            }
        };

        private sealed class SubFlee : AISUB.Simple
        {
            public SubFlee() : base("riot_Flee")
            {
            }

            public AISubActivation activate(Humanoid a, AIManager d, ENTITY other)
            {
                a.speed.turn2(other.body(), a.body());
                return activate(a, d);
            }

            public AISubActivation activate(Humanoid a, AIManager d, int iterations)
            {
                d.subPathByte = (byte)(iterations + 1);
                return activate(a, d);
            }

            public override AISubActivation activate(Humanoid a, AIManager d)
            {
                d.subPathByte = (byte)(2 + RND.rInt(5));
                d.subPathByte2 = (byte)(2 + RND.rInt(15));
                return base.activate(a, d);
            }

            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                a.speed.turnWithAngel(RND.rFloat0(90));
                d.subPathByte--;
                if (SETT.TERRAIN().WATER.DEEP.is(a.tc()))
                {
                    d.subPathByte2--;
                    if (d.subPathByte2 <= 0)
                    {
                        HumanoidResource.dead = CAUSE_LEAVES.DROWNED();
                    }
                }

                if (d.subPathByte > 0)
                {
                    if (RND.oneIn(3))
                        return AI.STATES().jogCrazy.activate(a, d, 2f + RND.rFloat() * 3);
                    return AI.STATES().jog.activate(a, d, 2f + RND.rFloat() * 3);
                }

                return null;
            }

            protected override AISTATE resumeInterrupted(Humanoid a, AIManager d, HEvent event)
            {
                return null;
            }

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                return InterBattle.pollReady(a, d, e);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.CHECK_MORALE)
                    return false;
                return InterBattle.listener.event(a, d, e);
            }
        }
    }
}