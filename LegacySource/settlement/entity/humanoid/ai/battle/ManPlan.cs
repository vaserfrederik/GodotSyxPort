using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.path.finders;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.battle
{
    internal sealed class ManPlan : AIPLAN.PLANRES
    {
        public ManPlan(string key) : base(key) { }

        private static readonly CharSequence ¤¤Name = "¤Manning defenses";

        static ManPlan()
        {
            D.ts(typeof(ManPlan));
        }

        public bool ShouldMan(Humanoid a, AIManager d)
        {
            if (a.Division != null && a.Division.Settings.Mustering)
                return false;
            if (d.Plan == this && GetResumer(d) != wait)
                return true;

            if (!SETT.PATH().Finders.Manning(a.Indu.Army).Has(a.Tc()))
                return false;
            return true;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return wait.Set(a, d);
        }

        private Resumer wait = new Resumer(¤¤Name)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().WalkTo.ServiceInclude(a, d, SETT.PATH().Finders.Manning(a.Indu.Army), 100);
                if (s != null)
                {
                    walk.Set(a, d);
                    return s;
                }
                return AI.SUBS().STAND.ActivateTime(a, d, 5);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().WalkTo.ServiceInclude(a, d, SETT.PATH().Finders.Manning(a.Indu.Army), int.MaxValue);
                if (s != null)
                {
                    walk.Set(a, d);
                    return s;
                }
                if (!a.Indu.Player)
                    STATS.BATTLE().ROUTING.Indu.Set(a.Indu, 1);
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

        private Resumer walk = new Resumer(¤¤Name)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f == null)
                    return null;
                a.Speed.SetDirCurrent(f.FaceDIR());
                if (!AI.Modules().Battle.ModuleCanContinue(a, d))
                {
                    Can(a, d);
                    return null;
                }
                return stand.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f != null)
                    f.FindableReserveCancel();
            }
        };

        private Resumer stand = new Resumer(¤¤Name)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.ActivateTime(a, d, (int)(2 + RND.rFloat() * 2));
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!AI.Modules().Battle.ModuleCanContinue(a, d))
                {
                    Can(a, d);
                    return null;
                }
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f == null)
                    return null;
                if (f.NeedsWork())
                    return work.Set(a, d);
                return AI.SUBS().STAND.ActivateTime(a, d, (int)(2 + RND.rFloat() * 2));
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f != null)
                    f.FindableReserveCancel();
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.CHECK_MORALE)
                {
                    if (a.Indu.Army == GAME.ARMIES().Enemy() && GAME.ARMIES().Enemy().Morale < 0.2)
                    {
                        STATS.BATTLE().ROUTING.Indu.Set(a.Indu, 1);
                        d.Overwrite(a, AI.Modules().Battle.Dessert);
                        return false;
                    }
                }
                return base.Event(a, d, e);
            }
        };

        private Resumer work = new Resumer(¤¤Name)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().WORK_HANDS.Activate(a, d, 1);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f == null)
                    return null;
                f.Work(5, a);
                if (!AI.Modules().Battle.ModuleCanContinue(a, d))
                {
                    Can(a, d);
                    return null;
                }
                if (a.Division != null && a.Division.Settings.Mustering)
                {
                    Can(a, d);
                    return null;
                }
                return stand.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FINDABLE_MANNING f = SETT.PATH().Finders.Manning(a.Indu.Army).GetReserved(d.Path.DestX, d.Path.DestY);
                if (f != null)
                    f.FindableReserveCancel();
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.CHECK_MORALE)
                {
                    if (a.Indu.Army == GAME.ARMIES().Enemy() && GAME.ARMIES().Enemy().Morale < 0.2)
                    {
                        STATS.BATTLE().ROUTING.Indu.Set(a.Indu, 1);
                        d.Overwrite(a, AI.Modules().Battle.Dessert);
                        return false;
                    }
                }
                return base.Event(a, d, e);
            }
        };

        protected override AISubActivation Resume(Humanoid a, AIManager d)
        {
            return base.Resume(a, d);
        }

        public override bool NotifyIfSubFails()
        {
            return false;
        }
    }
}