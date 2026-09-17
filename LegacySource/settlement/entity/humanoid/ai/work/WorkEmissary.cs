using static Settlement.Main.SETT;
using static Settlement.Main.SETT.ROOMS;

using Game.Time;
using Settlement.Entity.Humanoid;
using Settlement.Entity.Humanoid.AI.Main;
using Settlement.Entity.Humanoid.AI.Main.AI_SUB;
using Settlement.Main;
using Settlement.Stats;

namespace Settlement.Entity.Humanoid.AI.Work
{
    final class WorkEmissary : WorkAbs
    {
        protected WorkEmissary(AIModule_Work module, PlanBlueprint[] map, Works works)
            : base(module, ROOMS().EMBASSY, map, works)
        {
        }

        protected override AISubActivation FinishedWork(Humanoid a, AIManager d)
        {
            if ((TIME.Days().BitsSinceStart() + STATS.RAN().Get(a.Indu(), 0) & 0b01) == 0)
            {
                return base.FinishedWork(a, d);
            }

            if (STATS.WORK().WORK_TIME.Indu().GetD(a.Indu()) > 0.5)
                return base.FinishedWork(a, d);

            if (AIModules.NextPrio(d) > 7)
            {
                return base.FinishedWork(a, d);
            }

            return GoOnMission.Set(a, d);
        }

        final Resumer GoOnMission = new Resumer(SETT.ROOMS().EMBASSY.Employment().Verb)
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                if (PATH().Finders.EntryPoints.Find(a.TC().X(), a.TC().Y(), d.Path, int.MaxValue))
                {
                    return AI.SUBS().WalkTo.PathFull(a, d);
                }
                return null;
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                return BeOnMission.Set(a, d);
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }
        };

        final Resumer BeOnMission = new Resumer(SETT.ROOMS().EMBASSY.Employment().Verb)
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                SETT.ENTITIES().MoveIntoTheTheUnknown(a);
                a.Speed.MagnitudeInit(0);
                return AI.SUBS().STAND.Activate(a, d);
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (STATS.WORK().WORK_TIME.Indu().GetD(a.Indu()) > 0.9)
                {
                    Can(a, d);
                    return null;
                }
                if (AIModules.NextPrio(d) > 7)
                {
                    Can(a, d);
                    return null;
                }
                return AI.SUBS().STAND.Activate(a, d);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                SETT.ENTITIES().ReturnFromTheTheUnknown(a);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }
        };

        protected override bool ShouldContinue(Humanoid a, AIManager d)
        {
            if (!base.ShouldContinue(a, d))
                return false;

            return true;
        }
    }
}