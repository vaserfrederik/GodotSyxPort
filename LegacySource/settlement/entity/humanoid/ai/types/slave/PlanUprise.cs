using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.stats.equip;
using settlement.stats;
using settlement.entity.humanoid.ai.types.slave;
using util.text;
using snake2d.util.datatypes;
using init.resources;

namespace settlement.entity.humanoid.ai.types.slave
{
    internal sealed class PlanUprise : AIPLAN.PLANRES
    {
        public PlanUprise() : base("slaveUprise")
        {
            // TODO Auto-generated constructor stub
        }

        private static readonly CharSequence ¤¤verb = "¤Minding own business!";

        static PlanUprise()
        {
            D.ts(typeof(PlanUprise));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return steal.Set(a, d);
        }

        private readonly Resumer steal = new Resumer(¤¤verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                int i = GAME.events().uprising.spots.SignUpUpriserPositionByte(a);
                if (i < 0)
                    return null;
                d.planByte3 = (byte)i;

                return Res(a, d);
            }

            private readonly RBITImp bits = new RBITImp();

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (d.resourceCarried() != null)
                {
                    foreach (Equip e in STATS.EQUIP().allE())
                    {
                        if (e.resource() == d.resourceCarried())
                        {
                            e.stat().indu().inc(a.indu(), 1);
                        }
                    }
                    d.resourceCarriedSet(null);
                }

                bits.Clear();
                foreach (Equip e in STATS.EQUIP().BATTLE_ALL())
                {
                    if (e.stat().indu().getD(a.indu()) < 0.3)
                        bits.Or(e.resource());
                }

                if (!bits.IsClear())
                {
                    AISubActivation s = AI.SUBS().walkTo.resource(a, d, bits);
                    if (s != null)
                    {
                        return s;
                    }
                }
                return path.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return GAME.events().uprising.spots.confirmUpriser(d.planByte3);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                GAME.events().uprising.spots.cancelUpriser(a, d.planByte3, false);
            }
        };

        private readonly Resumer path = new Resumer(¤¤verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                COORDINATE c = GAME.events().uprising.spots.getUpriserTile(d.planByte3);
                AISubActivation s = AI.SUBS().walkTo.around(a, d, c.x(), c.y(), 0, 20);
                if (s != null)
                    return s;
                GAME.Notify(c.x() + " " + c.y());
                Can(a, d);
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return wait.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return GAME.events().uprising.spots.confirmUpriser(d.planByte3);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                GAME.events().uprising.spots.cancelUpriser(a, d.planByte3, false);
            }
        };

        private readonly Resumer wait = new Resumer(¤¤verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                GAME.events().uprising.spots.reportUpriserInPosition(d.planByte3);
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return GAME.events().uprising.spots.confirmUpriser(d.planByte3);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                GAME.events().uprising.spots.cancelUpriser(a, d.planByte3, true);
            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.type == HPoll.IS_SLAVE_READY_FOR_UPRISING)
                    return d.planByte3;
                return base.Poll(a, d, e);
            }
        };
    }
}