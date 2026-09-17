using System;
using init.resources.RBIT;
using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using settlement.stats.equip;
using util.text;

namespace settlement.entity.humanoid.ai.home
{
    internal sealed class PlanReturn : AIPLAN.PLANRES
    {
        private static readonly string ¤¤sName = "Returning Furniture";

        static PlanReturn()
        {
            D.ts(typeof(PlanReturn));
        }

        public PlanReturn() : base("SerHomeEquip")
        {
        }

        private readonly RBITImp bits = new RBITImp();

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            Induvidual i = a.indu();
            bits.clear();
            foreach (WearableResource e in STATS.HOME().getTmp(i))
            {
                if (e.needed(i) < 0)
                {
                    bits.or(e.resource(i));
                }
            }

            if (bits.isClear())
                return null;

            RESOURCE res = SETT.PATH().finders.storage.reserve(a.tc(), bits, d.path, 200);

            if (res == null)
            {
                dump(a, d);
                return null;
            }
            else
            {
                d.planByte1 = res.bIndex();
                remOne(a, d, res);
                return walk.set(a, d);
            }
        }

        private readonly Resumer walk = new Resumer(¤¤sName)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().walkTo.depositInited(a, d, RESOURCES.ALL().get(d.planByte1));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (AIModules.current(d).moduleCanContinue(a, d))
                {
                    return init(a, d);
                }
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // Dump
            }
        };

        private void remOne(Humanoid a, AIManager d, RESOURCE res)
        {
            foreach (WearableResource e in STATS.HOME().getTmp(a.indu()))
            {
                if (e.needed(a.indu()) < 0 && e.resource(a.indu()) == res)
                {
                    e.inc(a.indu(), -1);
                    return;
                }
            }
        }

        private void dump(Humanoid a, AIManager d)
        {
            STATS.HOME().dump(a);
        }
    }
}