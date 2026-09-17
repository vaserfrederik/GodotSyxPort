using settlement.misc.job;

namespace settlement.room.industry.module
{
    public interface IROOM_IDATA_INSTANCE
    {
        long[] ProductionData();
        JOB_MANAGER GetWork();
    }
}