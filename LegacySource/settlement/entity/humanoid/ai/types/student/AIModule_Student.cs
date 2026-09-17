using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.knowledge.university;
using settlement.room.main;
using settlement.stats;
using util.text;
using init.sprite.UI;
using init.type;

namespace settlement.entity.humanoid.ai.types.student
{
    public sealed class AIModule_Student : AIModule
    {
        private readonly Plan plan = new Plan(this);

        private static readonly CharSequence ¤¤name = "Study";
        private static readonly CharSequence ¤¤desc = "Gain some education or indoctrination in a learning facility";
        static AIModule_Student()
        {
            D.ts(typeof(AIModule_Student));
        }

        public AIModule_Student() : base(UI.icons().s.admin, ¤¤name, ¤¤desc) { }

        public bool TryInit(Humanoid h, AIManager d)
        {
            return GetFirstUni(h, d) != null;
        }

        public static bool ShouldContinue(Humanoid h, AIManager d)
        {
            ROOM_UNIVERSITY uu = Uni(h);
            if (uu == null)
                return false;

            if (!CheckUni(h, d, uu))
                return false;
            if (STATS.WORK().EMPLOYED.Get(h).Employees.IsOverstaffed())
                return false;
            if (uu.Bonus().Get(h.Indu()) <= 0)
                return false;
            return true;
        }

        static ROOM_UNIVERSITY Uni(Humanoid h)
        {
            RoomInstance ii = STATS.WORK().EMPLOYED.Get(h);
            if (ii != null && ii.BlueprintI() is ROOM_UNIVERSITY)
                return (ROOM_UNIVERSITY)ii.BlueprintI();
            return null;
        }

        private ROOM_UNIVERSITY GetFirstUni(Humanoid h, AIManager d)
        {
            ROOM_UNIVERSITY best = null;
            double bv = 0;
            foreach (ROOM_UNIVERSITY u in SETT.ROOMS().UNIVERSITIES)
            {
                if (u.Emp.Employable() > 0 && CheckUni(h, d, u))
                {
                    double r = u.LearningSpeed * u.Bonus().Get(h.Indu());
                    if (r > bv)
                    {
                        bv = r;
                        best = u;
                    }
                }
            }
            return best;
        }

        static bool CheckUni(Humanoid h, AIManager d, ROOM_UNIVERSITY u)
        {
            if (u.Bonus().Get(h.Indu()) <= 0)
                return false;
            return STATS.EDUCATION().Adult.EducateCan(h.Indu());
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            return plan.Activate(a, d);
        }

        protected override void Init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            ROOM_UNIVERSITY u = GetFirstUni(a, d);
            u.Emp.Employ(a);
        }

        private static readonly double lls = 1 / 16.0;

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            RoomInstance inRoom = STATS.WORK().EMPLOYED.Get(a);
            if (inRoom != null && inRoom.BlueprintI() is ROOM_UNIVERSITY)
            {
                ROOM_UNIVERSITY u = (ROOM_UNIVERSITY)inRoom.BlueprintI();
                double ls = u.LearningSpeed(inRoom, a.Indu());
                STATS.EDUCATION().Adult.Educate(a.Indu(), ls * lls);
            }
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            if (!ShouldContinue(a, d))
                return 0;
            return ((ROOM_UNIVERSITY)STATS.WORK().EMPLOYED.Get(a).BlueprintI()).IsTime.Is() ? 5 : 0;
        }
    }
}