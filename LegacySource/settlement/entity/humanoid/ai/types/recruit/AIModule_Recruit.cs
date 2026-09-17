using System;
using System.Collections.Generic;
using System.Text;

namespace Settlement.Entity.Humanoid.AI.Types.Recruit
{
    using Game.Battle.Div;
    using Init.Sprite.UI;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Entity.Humanoid.AI.Main;
    using Settlement.Entity.Humanoid.AI.Main.AiPlan;
    using Settlement.Main;
    using Settlement.Room.Main;
    using Settlement.Room.Military.Training;
    using Settlement.Room.Military.Training.Archery;
    using Settlement.Room.Military.Training.Barracks;
    using Settlement.Stats;
    using Util.Text;

    public sealed class AIModuleRecruit : AIModule
    {
        private readonly PlanBarracks plan = new PlanBarracks(this);
        private readonly PlanRange range = new PlanRange(this);

        private static readonly CharSequence ¤¤name = "Training";
        private static readonly CharSequence ¤¤desc = "Spend time in a training facility to hone skills";
        static
        {
            D.ts(AIModuleRecruit.class);
        }

        public AIModuleRecruit()
            : base(UI.icons().s.shield, ¤¤name, ¤¤desc)
        {
        }

        public bool CanBecome(Humanoid h, AIManager d)
        {
            return SETT.BATTLE().info.updateAndGetEmployment(h, null) != null;
        }

        public void DebugBecome(Humanoid a, AIManager d)
        {
        }

        public void DebugRemove(Humanoid a, AIManager d)
        {
        }

        public bool SetEmploy(Humanoid a, AIManager d)
        {
            ROOM_M_TRAINER current = Current(a);

            ROOM_M_TRAINER tar = SETT.BATTLE().info.updateAndGetEmployment(a, current);

            if (tar == null)
            {
                STATS.WORK().EMPLOYED.set(a, null);
                return false;
            }

            if (current != tar)
            {
                tar.emp.employ(a);
                return true;
            }
            return true;
        }

        public void UpdateNonRecruit(Humanoid a, AIManager d)
        {
            Div div = STATS.BATTLE().RECRUIT.get(a);
            if (div != null)
            {
                if (div.info.men() < div.men())
                    STATS.BATTLE().RECRUIT.set(a, null);
            }
            else
            {
                div = a.division();
                if (div != null && div.info.men() < div.men())
                    STATS.BATTLE().DIV.set(a, null);
            }
        }

        public bool ShouldRemain(Humanoid a, AIManager d)
        {
            ROOM_M_TRAINER current = Current(a);

            ROOM_M_TRAINER tar = SETT.BATTLE().info.updateAndGetEmployment(a, current);

            if (tar == null || tar != current)
                return false;

            return true;
        }

        public ROOM_M_TRAINER Current(Humanoid a)
        {
            RoomInstance ins = STATS.WORK().EMPLOYED.get(a);
            if (ins != null && ins.blueprintI() is ROOM_M_TRAINER)
                return (ROOM_M_TRAINER)ins.blueprintI();
            return null;
        }

        bool PlanShouldContinue(Humanoid a, AIManager d)
        {
            return ShouldRemain(a, d) && ModuleCanContinue(a, d) && !STATS.WORK().EMPLOYED.get(a).employees().isOverstaffed();
        }

        private bool Reinit(Humanoid a, AIManager d)
        {
            STATS.WORK().EMPLOYED.set(a, null);
            SetEmploy(a, d);
            return STATS.WORK().EMPLOYED.get(a) != null;
        }

        protected override void Init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            if (!SetEmploy(a, d))
                throw new RuntimeException();
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            if (!PlanShouldContinue(a, d))
                Reinit(a, d);

            AI.modules().work.swapInstance(a);

            RoomInstance w = STATS.WORK().EMPLOYED.get(a);

            if (w == null)
                return null;

            if (w.blueprintI() is ROOM_BARRACKS)
                return plan.activate(a, d);
            else if (w.blueprintI() is ROOM_ARCHERY)
                return range.activate(a, d);
            else
                throw new RuntimeException("No logic for: " + w.blueprintI());
        }

        private readonly double trainingD = 1.0 / HumanoidResource.updatesPerDay;

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            RoomInstance r = STATS.WORK().EMPLOYED.get(a);

            if (r != null && r.blueprint() is ROOM_M_TRAINER)
                ((ROOM_M_TRAINER)r.blueprint()).train(a, r, trainingD);
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            return STATS.WORK().WORK_TIME.indu().getD(a.indu()) < 1 ? 4 : 0;
        }
    }
}