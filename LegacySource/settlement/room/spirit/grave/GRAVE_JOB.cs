using Init.Resources;
using Settlement.Entity.Humanoid;
using Settlement.Misc.Job;
using Settlement.Thing.ThingsCorpses;

namespace Settlement.Room.Spirit.Grave
{
    public interface IGraveJob : ISettJob
    {
        new RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm);
        void BuryAndPerform(Corpse c);
    }
}