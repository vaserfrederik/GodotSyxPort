using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.idle;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.main;
using settlement.room.main.throne;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.text;

namespace settlement.entity.humanoid.ai.subject
{
    public sealed class AIModule_Subject : AIModule
    {
        private readonly INT_OE<AIManager> kissed = new INT_O.INTWRAP<AIManager>(0b0000_0001, AIModules.data().byte3);
        private readonly INT_OE<AIManager> activity = new INT_O.INTWRAP<AIManager>(0b0000_0010, AIModules.data().byte3);
        private readonly PlanJoinArmy army = new PlanJoinArmy();
        private readonly PlanEmmigrate emmi = new PlanEmmigrate();
        public readonly PlanBuryCorpse corpse = new PlanBuryCorpse();

        private static readonly CharSequence ¤¤swearing = "Swearing fealty";
        private static readonly CharSequence ¤¤name = "Misc.";

        private static readonly CharSequence ¤¤aexe = "Watching an execution";

        static AIModule_Subject()
        {
            D.ts(typeof(AIModule_Subject));
        }

        private readonly AA[] activities;

        public AIModule_Subject(AIModule_Idle idle)
            : base(UI.icons().s.human, ¤¤name, null)
        {
            activities = new AA[]
            {
                new AA(BOOSTABLES.ACTIVITY().SOCIAL).add(idle.interract()),
                new AA(BOOSTABLES.ACTIVITY().PUNISHMENT).add(new Activity(SETT.ROOMS().EXECUTION, 48, ¤¤aexe)),
                new AA(BOOSTABLES.ACTIVITY().MOURN).add(new ActivityMourn()),
            };
        }

        public int debug(AIManager aiManager)
        {
            return kissed.get(aiManager);
        }

        private readonly AIPLAN immigrate = new AIPLAN.PLANRES("subImmigrate")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                COORDINATE c = SETT.PATH().finders.rndCoo.find(THRONE.coo().x(), THRONE.coo().y(), 8);
                if (c != null)
                {
                    d.planTile.set(c);
                    AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);
                    start.set(a, d);
                    return s;
                }

                kissed.set(d, 1);
                return null;
            }

            private readonly Resumer start = new Resumer(¤¤swearing)
            {
                public override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override AISubActivation res(Humanoid a, AIManager d)
                {
                    a.speed.turn2(THRONE.coo().x() - a.tc().x(), THRONE.coo().y() - a.tc().y());
                    return swear.set(a, d);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }
            };

            private readonly Resumer swear = new Resumer(¤¤swearing)
            {
                public override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().LAY.activateTime(a, d, 4 + RND.rInt(10));
                }

                public override AISubActivation res(Humanoid a, AIManager d)
                {
                    kissed.set(d, 1);
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

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (emmi.shouldEmmigrate(a))
            {
                return emmi.activate(a, d);
            }

            if (corpse.shouldBury(a, d))
            {
                AiPlanActivation p = corpse.activate(a, d);

                if (p != null)
                {
                    return p;
                }
            }

            if (army.getPriority(a) != 0)
            {
                AiPlanActivation s = army.activate(a, d);
                if (s != null)
                {
                    return s;
                }
            }

            if (kissed.get(d) == 0 && a.indu().clas().player)
            {
                return immigrate.activate(a, d);
            }

            if (activity.get(d) == 1)
            {
                activity.set(d, 0);

                double ma = 0;
                foreach (AA aa in activities)
                {
                    ma += aa.bo.get(a.indu());
                }
                ma *= RND.rFloat();
                foreach (AA aa in activities)
                {
                    ma -= aa.bo.get(a.indu());
                    if (ma <= 0)
                    {
                        int ri = RND.rInt(aa.plans.size());
                        for (int i = 0; i < aa.plans.size(); i++)
                        {
                            AiPlanActivation s = aa.plans.get(ri).activate(a, d);
                            if (s != null)
                            {
                                return s;
                            }
                            ri++;
                            if (ri >= aa.plans.size())
                            {
                                ri = 0;
                            }
                        }
                    }
                }

            }

            return null;
        }

        protected override void init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            if (prev == null || !prev.CLASS.player)
            {
                AIModules.data().byte3.set(d, 0);
            }
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            if (newDay && RND.oneIn(4 + STATS.POP().POP.data().get(null) / 1000))
            {
                activity.set(d, 1);
            }
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            if (army.getPriority(a) != 0)
            {
                return 10;
            }

            if (corpse.shouldBury(a, d))
            {
                return 6;
            }

            if (kissed.get(d) == 0 && a.indu().hType() == HTYPES.SUBJECT())
            {
                return 6;
            }
            if (SETT.ROOMS().DUMP.service().finder.has(a.tc()) && GAME.ARMIES().enemy().men() == 0)
            {
                if (SETT.PATH().finders.corpses.has(a.tc()))
                {
                    return 5;
                }
            }

            if (emmi.shouldEmmigrate(a))
            {
                return 4;
            }

            if (activity.get(d) == 1)
            {
                return 2;
            }

            return 0;
        }

        private class AA
        {
            public readonly ArrayListGrower<AIPLAN> plans = new ArrayListGrower<AIPLAN>();
            public readonly Boostable bo;

            public AA(Boostable bo)
            {
                this.bo = bo;
            }

            public AA add(AIPLAN p)
            {
                plans.add(p);
                return this;
            }
        }
    }
}