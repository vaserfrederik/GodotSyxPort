using System;
using System.Text;

namespace Settlement.Entity.Humanoid.AI.Subject
{
    using Game;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Entity.Humanoid.AI.Main;
    using Settlement.Main;
    using Settlement.Stats;
    using Util.Text;
    using World;

    class PlanEmmigrate : AIPLAN.PLANRES
    {
        private static readonly string ¤¤name = "¤Fed up with this dump and your failing rule.";

        public PlanEmmigrate() : base("SubEmigrate")
        {
        }

        static PlanEmmigrate()
        {
            D.ts(typeof(PlanEmmigrate));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            GAME.events().riot.emigrate(a);
            return walk.set(a, d);
        }

        bool shouldEmmigrate(Humanoid a)
        {
            if (a.indu().clas() != HCLASSES.CITIZEN())
                return false;

            if (SETT.ENTRY().isClosed())
                return false;

            if (WORLD.camps().available(a.race()) && WORLD.camps().current(a.indu().faction(), a.race()) < STATS.POP().POP.data(HCLASSES.CITIZEN()).get(a.race()))
                return true;

            if (!GAME.events().riot.shouldEmigrate(a))
                return false;

            return true;
        }

        private readonly Resumer walk = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (SETT.PATH().finders.entryPoints.find(a.tc().x(), a.tc().y(), d.path, int.MaxValue))
                {
                    STATS.POP().EMMIGRATING.indu().set(a.indu(), 1);
                    STATS.WORK().EMPLOYED.set(a, null);
                    STATS.BATTLE().RECRUIT.set(a, null);
                    STATS.BATTLE().DIV.set(a, null);
                    return AI.SUBS().walkTo.path(a, d);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                HumanoidResource.dead = CAUSE_LEAVES.EMMIGRATED();
                STATS.POP().EMMIGRATING.indu().set(a.indu(), 0);
                return AI.SUBS().STAND.activate(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                STATS.POP().EMMIGRATING.indu().set(a.indu(), 0);
            }
        };
    }
}