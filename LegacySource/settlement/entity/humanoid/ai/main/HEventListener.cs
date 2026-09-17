using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;

namespace Settlement.Entity.Humanoid.AI.Main
{
    public interface HEventListener
    {
        public bool Event(Humanoid a, AIManager d, HEventData e);
        public double Poll(Humanoid a, AIManager d, HPollData e);
    }
}