using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using snake2d.util.rnd;
using util.data;
using util.text;

namespace settlement.entity.humanoid.ai.service
{
    public sealed class AIModule_Service : AIModule
    {
        private readonly INT_OE<AIManager> remaining;
        public readonly S_Plans plans = new S_Plans();

        private static readonly string ¤¤name = "Service";
        private static readonly string ¤¤desc = "Do an activity that is on offer in your city.";

        static AIModule_Service()
        {
            D.ts(typeof(AIModule_Service));
        }

        public AIModule_Service()
            : base(UI.icons().s.trade, ¤¤name, ¤¤desc)
        {
            remaining = AI.data().new DataNibble("SERVICE_REM");
        }

        public AiPlanActivation get(Humanoid a, AIManager d, NEED need, int dist)
        {
            foreach (S_Plan p in plans.needMap[need.index()])
            {
                AiPlanActivation pp = p.getPlan(a, d, dist);
                if (p != null)
                    return pp;
            }

            return null;
        }

        public AiPlanActivation plan(Humanoid a, AIManager d, NEED need, double ran)
        {
            S_Plan s = pservice(a.indu(), need, ran);
            if (s != null)
                return s.getPlan(a, d);
            return null;
        }

        public StatService service(Induvidual a, NEED need, double ran)
        {
            S_Plan s = pservice(a, need, ran);
            if (s != null)
                return s.service;
            return null;
        }

        private S_Plan pservice(Induvidual a, NEED need, double ran)
        {
            if (need == null)
                return null;

            double pm = 0;

            foreach (S_Plan p in plans.needMap[need.index()])
            {
                pm += p.usage;
            }

            pm *= ran;

            foreach (S_Plan p in plans.needMap[need.index()])
            {
                pm -= p.usage;
                if (pm <= 0)
                    return p;
            }

            return null;
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (remaining.get(d) <= 0)
                remaining.set(d, 1);

            while (remaining.get(d) > 0)
            {
                remaining.inc(d, -1);
                AiPlanActivation p = plans.getPlan(a, d);
                if (p != null)
                    return p;
            }

            return null;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
        {
            if (newDay)
            {
                reset(a, d);
            }
        }

        protected override void init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            reset(a, d);
        }

        private void reset(Humanoid a, AIManager d)
        {
            int am = remaining.get(d);
            int n = RND.rInt(TIME.servicePerDay());
            am += n;
            if (am > TIME.servicePerDay() * 2)
                am = TIME.servicePerDay() * 2;
            remaining.set(d, am);
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            if (remaining.get(d) > 0)
                return 3;
            return 0;
        }
    }
}