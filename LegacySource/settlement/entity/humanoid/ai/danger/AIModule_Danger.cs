using System.Collections.Generic;

namespace Settlement.Entity.Humanoid.AI.Danger
{
    public sealed class AIModule_Danger
    {
        public readonly IList<AIModule> All;

        public AIModule_Danger()
        {
            List<AIModule> all = new List<AIModule>();
            all.Add(new AIModule_Exposure());
            all.Add(new AIModule_Health());
            All = all;
        }
    }
}