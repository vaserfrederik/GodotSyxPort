using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    internal class F_PlanEat : AIPLAN.PLANRES
    {
        private readonly AISUB sub;
        private static readonly CharSequence ¤¤name = "Finding food";

        static F_PlanEat()
        {
            D.ts(typeof(F_PlanEat));
        }

        public F_PlanEat(AISUB sub) : base("SerEat")
        {
            this.sub = sub;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return fetchRaw.set(a, d);
        }

        private readonly Resumer fetchRaw = new Resumer(¤¤name)
        {
            bits = new RBITImp(),

            setAction = (a, d) =>
            {
                bits.clearSet(RESOURCES.EDI().mask);
                if (STATS.FOOD().STARVATION.indu().get(a.indu()) <= 0)
                {
                    bits.and(STATS.FOOD().fetchMask(a));
                }
                return AI.SUBS().walkTo.resource(a, d, bits, int.MaxValue);
            },

            res = (a, d) =>
            {
                STATS.FOOD().eat(a, 1, 0);
                return eat.set(a, d);
            },

            con = (a, d) => true,

            can = (a, d) => d.resourceCarriedSet(null)
        };

        private readonly Resumer eat = new Resumer(¤¤name)
        {
            res = (a, d) =>
            {
                if (d.resourceCarried() != null && RESOURCES.EDI().is(d.resourceCarried()))
                {
                    FACTIONS.player().res().inc(d.resourceCarried(), RTYPE.CONSUMED, -1);
                }
                d.resourceCarriedSet(null);
                return null;
            },

            con = (a, d) => true,

            can = (a, d) => d.resourceCarriedSet(null),

            setAction = (a, d) => sub.activate(a, d)
        };
    }
}