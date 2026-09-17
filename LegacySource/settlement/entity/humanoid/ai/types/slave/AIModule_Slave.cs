using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using util.text;

namespace settlement.entity.humanoid.ai.types.slave
{
    public sealed class AIModule_Slave : AIModule
    {
        private PlanUprise uprise = new PlanUprise();
        private static readonly string ¤¤leave = "Leaving city for another master.";
        private static readonly string ¤¤name = "rise up";

        static AIModule_Slave()
        {
            D.ts(typeof(AIModule_Slave));
        }

        public AIModule_Slave() : base(UI.icons().s.slave, ¤¤name, null)
        {
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            if (GAME.events().uprising.spots.ShouldSignUpUpriser(a))
                return uprise.Activate(a, d);
            if (!SETT.ENTRY().IsClosed() && SETT.TRADE().ShouldLeave(a))
                return leave.Activate(a, d);
            return null;
        }

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
        {
            // TODO Auto-generated method stub
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            if (GAME.events().uprising.spots.ShouldSignUpUpriser(a))
                return 8;
            if (!SETT.ENTRY().IsClosed() && SETT.TRADE().ShouldLeave(a))
                return 9;
            return 0;
        }

        public readonly AIPLAN leave = new AIPLAN.PLANRES("SLAVE_SOLD")
        {
            protected override AISubActivation Init(Humanoid a, AIManager d)
            {
                return start.Set(a, d);
            }

            private readonly Resumer start = new Resumer(¤¤leave)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    if (SETT.PATH().finders.entryPoints.Find(a.tc().x(), a.tc().y(), d.path, int.MaxValue))
                    {
                        SETT.TRADE().ReserveLeave(a);
                        return AI.SUBS().walkTo.path(a, d);
                    }
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    return fin.Set(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                    SETT.TRADE().ReserveLeaveCancel(a);
                }
            };

            private readonly Resumer fin = new Resumer(¤¤leave)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    HumanoidResource.dead = CAUSE_LEAVES.SOLD();
                    SETT.TRADE().Leave(a);
                    return AI.SUBS().STAND.Activate(a, d);
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                    // TODO Auto-generated method stub
                }
            };
        };
    }
}