using System;
using System.Collections.Generic;
using init.race;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.main;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    internal sealed class M_PlanEquip : AIPLAN.PLANRES
    {
        private static readonly string ¤¤name = "Getting equipment";

        static M_PlanEquip()
        {
            D.ts(typeof(M_PlanEquip));
        }

        private readonly RBITImp bits = new RBITImp();

        public M_PlanEquip()
            : base("serEquip")
        {
        }

        static M_PlanEquip()
        {
            D.ts(typeof(M_PlanEquip));
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            bits.Clear();
            foreach (WearableResource e in RACES.res().All(a.Indu().PopCL()))
            {
                int needed = e.Needed(a.Indu());
                if (needed > 0)
                {
                    bits.Or(e.Resource(a.Indu()));
                }
            }

            if (!bits.IsClear())
            {
                RESOURCE res = SETT.PATH().Finders.Resource.Find(bits, a.Tc().X(), a.Tc().Y(), d.Path, 100);
                if (res != null)
                {
                    int nn = 0;
                    foreach (WearableResource rr in RACES.res().Get(a.Indu().PopCL(), res))
                    {
                        if (rr.Resource(a.Indu()) == res)
                        {
                            int n = rr.Needed(a.Indu());
                            if (n > 0)
                                nn += n;
                        }
                    }

                    if (nn <= 0)
                    {
                        throw new Exception($"{res} {RACES.res().Get(a.Indu().PopCL(), res).Count}");
                    }
                    AISubActivation s = fetch.ActivateFound(a, d, res, nn, true, true);
                    return s;
                }
            }

            NEEDS.TYPES().SHOPPING.Stat().FixMax(a.Indu());

            foreach (WearableResource e in RACES.res().All(a.Indu().PopCL()))
            {
                e.WearOut(a.Indu());
            }
            return null;
        }

        protected override void Name(Humanoid a, AIManager d, Str stringObj)
        {
            stringObj.Add(¤¤name);
        }

        private readonly AIPlanResourceMany fetch = new AIPlanResourceMany(this, 64)
        {
            Next = (Humanoid a, AIManager d) =>
            {
                RESOURCE res = d.ResourceCarried();
                int am = d.ResourceA();
                Induvidual i = a.Indu();

                if (res == null || am <= 0)
                    return null;

                foreach (WearableResource r in RACES.res().Get(i.PopCL(), res))
                {
                    r.WearOut(i);
                    int dam = CLAMP.I(am, 0, r.Needed(a.Indu()));
                    r.Inc(i, dam);
                    am -= dam;
                    d.ResourceAInc(-dam);
                    if (am <= 0)
                        break;
                }

                if (AIModules.Current(d).ModuleCanContinue(a, d))
                    return Init(a, d);
                return null;
            },
            Cancel = (Humanoid a, AIManager d) =>
            {
                // Implementation for cancel method
            }
        };
    }
}