using System;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.work;
using snake2d.util.datatypes;

namespace settlement.misc.job
{
    public interface SETT_JOB
    {
        void jobReserve(RESOURCE r);
        bool jobReservedIs(RESOURCE r);
        void jobReserveCancel(RESOURCE r);
        bool jobReserveCanBe();

        RBIT jobResourceBitToFetch();
        int jobResourcesNeeded(Humanoid skill) => AIModule_Work.MAX_FETCH_AMOUNT;
        double jobPerformTime(Humanoid a);
        void jobStartPerforming();
        RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm);

        COORDINATE jobCoo();
        DIR jobStandDir() => null;

        CharSequence jobName();

        bool jobUseTool();
        bool jobUseHands() => true;
        SoundRace jobSound();

        bool longFetch() => false;
    }
}