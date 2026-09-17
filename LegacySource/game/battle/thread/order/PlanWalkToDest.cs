using System;
using System.Collections.Generic;

namespace Game.Battle.Thread.Order
{
    final class PlanWalkToDest : PlanWalkAbs
    {
        public PlanWalkToDest(Tools tools, LISTE<Plan> all, Data data) : base(tools, all, data, DIVTASK.MOVE)
        {
        }

        override void Init()
        {
            SetWalkToDest();
        }

        override void Update(int gamemillis)
        {
            state(m).Update(gamemillis);
        }

        override void Finished()
        {
            task.Stop(div);
            order.task.Set(task);
        }

        override bool ContinueWhenFighting()
        {
            return true;
        }
    }
}