using System;
using System.Collections.Generic;
using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.entity.humanoid.ai.work;
using settlement.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    class WorkAbs : PlanBlueprint
    {
        private static CharSequence ¤¤walk = "¤walking to job";
        private static CharSequence ¤¤storing = "¤storing resource";
        private static CharSequence ¤¤working = "¤working";

        static WorkAbs()
        {
            D.ts(typeof(WorkAbs));
        }

        private readonly Works works;

        public WorkAbs(AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map, Works works)
            : base(module, blueprint, map)
        {
            this.works = works;
        }

        public WorkAbs(string key, AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map, Works works)
            : base(key, module, blueprint, map)
        {
            this.works = works;
        }

        public class Works
        {
            public readonly SubWorkTool subTool = new SubWorkTool("WORK_TOOL")
            {
                protected override SETT_JOB getJob(Humanoid a, AIManager d)
                {
                    if (work(a) == null)
                        return null;
                    return ((JOBMANAGER_HASER)work(a)).getWork().getJob(d.planTile);
                }
            };

            public readonly SubWorkHands subHands = new SubWorkHands("WORK_HANDS")
            {
                protected override SETT_JOB getJob(Humanoid a, AIManager d)
                {
                    if (work(a) == null)
                        return null;
                    return ((JOBMANAGER_HASER)work(a)).getWork().getJob(d.planTile);
                }
            };

            public readonly SubWorkThink subThink = new SubWorkThink("WORK_THINK")
            {
                protected override SETT_JOB getJob(Humanoid a, AIManager d)
                {
                    if (work(a) == null)
                        return null;
                    return ((JOBMANAGER_HASER)work(a)).getWork().getJob(d.planTile);
                }
            };
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            if (!module.moduleCanContinue(a, d) || !hasEmployment(a, d))
                return null;
            if (STATS.WORK().WORK_TIME.indu().getD(a.indu) <= 0)
                return null;

            COORDINATE cc = a.tc;
            SETT_JOB job = ((JOBMANAGER_HASER)work(a)).getWork().getJob(cc);

            if (job != null)
            {
                job.jobStartPerforming();
                return works.subTool.activate(a, d, job);
            }

            return null;
        }

        protected override AISubActivation finishedWork(Humanoid a, AIManager d)
        {
            return init(a, d);
        }

        protected override string debug(Humanoid a, AIManager d)
        {
            return base.debug(a, d);
        }
    }
}