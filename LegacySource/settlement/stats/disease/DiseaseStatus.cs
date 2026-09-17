using System.Collections.Generic;

namespace Settlement.Stats.Disease
{
    public enum DiseaseStatus
    {
        NONE = 0,
        INCUBATING = 1,
        ISICK = 2,
        IIMMUNE = 3
    }

    public static class DiseaseStatusExtensions
    {
        public static readonly List<DiseaseStatus> ALL = new List<DiseaseStatus>
        {
            DiseaseStatus.NONE,
            DiseaseStatus.INCUBATING,
            DiseaseStatus.ISICK,
            DiseaseStatus.IIMMUNE
        };

        public static bool IsActive(this DiseaseStatus status)
        {
            return status == DiseaseStatus.ISICK;
        }
    }
}