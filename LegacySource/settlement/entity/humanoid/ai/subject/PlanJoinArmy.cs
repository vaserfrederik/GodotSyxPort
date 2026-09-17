using System;
using settlement.entity.humanoid.ai.subject;
using game.battle.div;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using settlement.stats.equip;
using util.text;
using world.army;
using world.entity.army;

namespace Settlement.Entity.Humanoid.AI.Subject
{
    public class PlanJoinArmy : AIPLAN.PLANRES
    {
        {
            D.t(this);
        }

        public PlanJoinArmy() : base("subJoinArmy")
        {
        }

        public int GetPriority(Humanoid a)
        {
            return SETT.BATTLE().info.ShouldJoinArmy(a) ? 10 : 0;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            //AISubActivation s = fetchGear.Set(a, d);
            //if (s != null)
            //    return s;
            return path.Set(a, d);
        }

        private readonly Resumer path = new Resumer(D.g("Leaving", "Leaving for the Army"))
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                if (PATH().finders.entity.FindExitNoEnemies(a, a.physics.tileC().x(), a.physics.tileC().y(), d.path, int.MaxValue))
                {
                    Dequip(a, d);
                    return AI.SUBS().walkTo.PathFull(a, d);
                }
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (STATS.BATTLE().DIV.Get(a) == null)
                    return null;
                AIManager.dead = CAUSE_LEAVES.ARMY();
                AD.cityDivs().Add(a, STATS.BATTLE().DIV.Get(a));
                return AI.SUBS().STAND.Activate(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return SETT.BATTLE().info.ShouldJoinArmy(a);
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer fetchGear = new Resumer("Getting Battlegear")
        {
            private readonly RBITImp bi = new RBITImp();

            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                bi.Clear();
                Div div = STATS.BATTLE().DIV.Get(a);
                if (div == null)
                    return null;

                foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                {
                    if (AmountNeeded(a, d, e) > 0)
                    {
                        bi.Or(e.resource(a.indu()));
                    }
                }

                if (bi.IsClear())
                    return null;

                return AI.SUBS().walkTo.Resource(a, d, bi, int.MaxValue);
            }

            private int AmountNeeded(Humanoid a, AIManager d, EquipBattle e)
            {
                Div div = STATS.BATTLE().DIV.Get(a);
                if (div == null)
                    return 0;
                if (!SETT.BATTLE().info.ShouldJoinArmy(a))
                    return 0;
                int aa = e.Target(div) - e.stat().indu().Get(a.indu());
                if (aa <= 0)
                    return 0;

                WArmy army = AD.cityDivs().attachedArmy(STATS.BATTLE().DIV.Get(a));
                if (army == null)
                    return 0;
                int am = (int)AD.supplies().equip.Get(e.indexMilitary()).MinimumTarget(army) - AD.supplies().equip.Get(e.indexMilitary()).Current().Get(army);
                return Math.Min(aa, am);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                RESOURCE r = d.resourceCarried();
                Div div = STATS.BATTLE().DIV.Get(a);
                if (div == null)
                {
                    Can(a, d);
                    return null;
                }
                WArmy army = AD.cityDivs().attachedArmy(STATS.BATTLE().DIV.Get(a));
                if (army == null)
                {
                    Can(a, d);
                    return null;
                }
                foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                {
                    if (AmountNeeded(a, d, e) > 0 && e.resource(a.indu()) == r)
                    {
                        e.Inc(a.indu(), 1);
                        d.resourceCarriedSet(null);
                        AD.supplies().equip.Get(e.indexMilitary()).Current().Inc(army, 1);
                        break;
                    }
                }
                d.resourceCarriedSet(null);
                AISubActivation s = Set(a, d);
                if (s == null)
                    return path.Set(a, d);
                return s;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return STATS.BATTLE().DIV.Get(a) != null;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                Dequip(a, d);
            }
        };

        private void Dequip(Humanoid a, AIManager d)
        {
            foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
            {
                int am = e.Get(a.indu());
                e.Set(a.indu(), 0);
                if (am > 0)
                {
                    WArmy army = AD.cityDivs().attachedArmy(STATS.BATTLE().DIV.Get(a));
                    if (army != null)
                    {
                        int need = (int)AD.supplies().equip.Get(e.indexMilitary()).Needed(army);
                        int aa = Math.Min(am, need);
                        am -= aa;
                        AD.supplies().equip.Get(e.indexMilitary()).Current().Inc(army, aa);
                    }
                    if (am > 0)
                        SETT.THINGS().resources.Create(a.tc(), e.resource, am);
                }

                d.resourceCarriedSet(null);
            }
        }
    }
}