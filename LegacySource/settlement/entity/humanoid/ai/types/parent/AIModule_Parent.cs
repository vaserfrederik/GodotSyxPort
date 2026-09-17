using System;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using snake2d.util.rnd;
using util.data;
using util.text;

namespace settlement.entity.humanoid.ai.types.parent
{
    public sealed class AIModule_Parent : AIModule
    {
        private static readonly INT_OE<AIManager> nurishBit = new INT_O.INTWRAP<AIManager>(0b0000_0010, AIModules.Data().Byte2);
        private static readonly INT_OE<AIManager> babyAge = AIModules.Data().Byte1;

        private static readonly CharSequence ¤¤name = "Parenting";
        private static readonly CharSequence ¤¤desc = "Need to nurture an infant";
        private static readonly CharSequence ¤¤nurishing = "Nourishing Child";

        static AIModule_Parent()
        {
            D.ts(typeof(AIModule_Parent));
        }

        public AIModule_Parent() : base(UI.icons().s.human, ¤¤name, ¤¤desc)
        {
        }

        private readonly AIPLAN nurish = new AIPLAN.PLANRES("subParenting")
        {
            protected override AISubActivation Init(Humanoid a, AIManager d)
            {
                if (PATH().finders.getOutofWay.ShouldFind(a))
                {
                    PATH().finders.getOutofWay.Request(a, d.path);
                    return walking.Set(a, d);
                }
                d.planByte1 = (byte)(3 + RND.rInt(5));
                return start.Set(a, d);
            }

            private readonly Resumer start = new Resumer(¤¤nurishing)
            {
                public override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    return res(a, d);
                }

                public override AISubActivation Res(Humanoid a, AIManager d)
                {
                    if (d.planByte1-- < 0)
                        return null;

                    return AI.SUBS().single.Activate(a, d, AI.STATES().STAND_BABY.aDirRND(a, d, 1 + RND.rFloat() * 5));
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };

            private readonly Resumer walking = new Resumer(¤¤nurishing)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.ActivateRndDir(a, d, 1 + RND.rInt(4));
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    if (moduleCanContinue(a, d))
                    {
                        if (PATH().finders.getOutofWay.CheckAndSetRequest(a.tc().x(), a.tc().y(), d.path))
                        {
                            return exit.Set(a, d);
                        }
                        return AI.SUBS().STAND.ActivateRndDir(a, d, 1 + RND.rInt(4));
                    }
                    return null;
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };

            private readonly Resumer exit = new Resumer(¤¤nurishing)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    if (d.path.isSuccessful())
                    {
                        return AI.SUBS().walkTo.PathFull(a, d);
                    }
                    else
                    {
                        return start.Set(a, d);
                    }
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    return start.Set(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };
        };

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            nurishBit.Set(d, 0);
            return nurish.Activate(a, d);
        }

        public bool GrowChildUp(Humanoid a, AIManager d)
        {
            if (babyAge.Get(d) >= a.race().physics.babyDays)
            {
                GrowUp(a);
                return true;
            }
            return false;
        }

        public static void GrowUp(Humanoid a)
        {
            HTYPE hh = a.indu().clas() == HCLASSES.SLAVE() ? HTYPES.CHILD_SLAVE() : HTYPES.CHILD();

            Humanoid child = SETT.HUMANOIDS().create(a.race(), a.tc().x(), a.tc().y(), hh, CAUSE_ARRIVES.BORN());

            if (child != null)
            {
                STATS.POP().age.DAYS.Set(child.indu(), a.race().physics.babyDays);
                STATS.POP().TYPE.NATIVE.Set(child.indu());
                STATS.REL().SetParent(child.indu(), a.indu());
                AIModule_Child.SetGrowth(a);
            }
        }

        protected override void Init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            nurishBit.Set(d, 0);
            babyAge.Set(d, 0);
        }

        public static int DaysOld(Humanoid a)
        {
            AIManager d = (AIManager)a.ai();
            return babyAge.Get(d);
        }

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            if (RND.oneIn(4))
            {
                nurishBit.Set(d, 1);
            }
            if (newDay)
            {
                babyAge.Inc(d, 1);
            }
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            return nurishBit.Get(d);
        }
    }
}