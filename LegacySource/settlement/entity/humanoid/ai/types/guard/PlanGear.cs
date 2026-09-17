using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.stats;
using settlement.stats.equip;
using util.text;

namespace settlement.entity.humanoid.ai.types.guard
{
    internal sealed class PlanGear : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤name = "Getting Gear";
        static PlanGear()
        {
            D.ts(typeof(PlanGear));
        }

        protected PlanGear() : base("GUARD_GEAR")
        {
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return fetchGear.Set(a, d);
        }

        private readonly Resumer fetchGear = new Resumer(¤¤name)
        {
            bi = new RBITImp()
        };

        protected override AISubActivation SetAction(Humanoid a, AIManager d)
        {
            bi.Clear();
            Div div = STATS.BATTLE().DIV.Get(a);
            if (div == null)
                return null;

            foreach (Equip e in STATS.EQUIP().BATTLE_ALL())
            {
                if (e.Needed(a.indu()) > 0)
                {
                    bi.Or(e.Resource(a.indu()));
                }
            }

            if (bi.IsClear())
                return null;

            AISubActivation s = AI.SUBS().WalkTo.Resource(a, d, bi, int.MaxValue);
            if (s == null)
            {
                foreach (Equip e in STATS.EQUIP().BATTLE_ALL())
                {
                    e.WearOut(a.indu());
                }
            }
            return s;
        }

        protected override AISubActivation Res(Humanoid a, AIManager d)
        {
            RESOURCE r = d.ResourceCarried();
            foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
            {
                if (r == e.Resource && e.Needed(a.indu()) > 0)
                {
                    e.WearOut(a.indu());
                    e.Inc(a.indu(), 1);
                    d.ResourceCarriedSet(null);
                    break;
                }
            }
            AISubActivation s = Set(a, d);
            return s;
        }

        public override bool Con(Humanoid a, AIManager d)
        {
            return STATS.BATTLE().DIV.Get(a) != null;
        }

        public override void Can(Humanoid a, AIManager d)
        {
        }
    }

    public override bool Event(Humanoid a, AIManager d, HEventData e)
    {
        if (e.Event == HEvent.NOTIFY_CRIME)
        {
            if (e.Other is Humanoid)
            {
                d.Overwrite(a, AI.Listeners().CatchCriminal((Humanoid)e.Other));
                return true;
            }
        }
        return base.Event(a, d, e);
    }
}