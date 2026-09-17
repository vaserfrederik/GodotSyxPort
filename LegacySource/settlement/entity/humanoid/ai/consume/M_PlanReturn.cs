using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using settlement.stats.equip;
using util.text;
using init.resources;
using game.faction;

namespace settlement.entity.humanoid.ai.consume
{
    internal class M_PlanReturn : AIPLAN.PLANRES
    {
        private static readonly string ¤¤sName = "Returning equipment";

        static M_PlanReturn()
        {
            D.ts(typeof(M_PlanReturn));
        }

        public M_PlanReturn() : base("SerEquip")
        {
            // TODO Auto-generated constructor stub
        }

        private readonly RBITImp bits = new RBITImp();

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            Induvidual i = a.indu();
            bits.clear();
            foreach (WearableResource e in STATS.EQUIP().allE())
            {
                if (e.needed(a.indu()) < 0)
                {
                    bits.or(e.resource(i));
                }
            }

            if (bits.isClear())
                return null;

            RESOURCE res = SETT.PATH().finders.storage.reserve(a.tc(), bits, d.path, 512);

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
                FACTIONS.player().res().inc(RESOURCES.ALL().get(d.planByte1), RTYPE.EQUIPPED, 1);
                SETT.THINGS().resources.create(a.physics.tileC(), RESOURCES.ALL().get(d.planByte1), 1);
            }
        };

        private void remOne(Humanoid a, AIManager d, RESOURCE res)
        {
            foreach (WearableResource e in STATS.EQUIP().allE())
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
            Induvidual i = a.indu();

            foreach (WearableResource e in STATS.EQUIP().allE())
            {
                int toDump = -e.needed(a.indu());
                if (toDump > 0)
                {
                    e.inc(i, -toDump);
                    FACTIONS.player().res().inc(e.resource(i), RTYPE.EQUIPPED, toDump);
                    SETT.THINGS().resources.create(a.physics.tileC(), e.resource(i), toDump);
                }
            }
        }
    }
}