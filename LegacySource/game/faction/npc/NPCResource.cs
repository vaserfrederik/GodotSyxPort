using System;
using System.Collections.Generic;

namespace Game.Faction.NPC
{
    public abstract class NPCResource
    {
        protected NPCResource(List<NPCResource> all)
        {
            all.Add(this);
        }

        protected abstract SAVABLE Saver();
        protected abstract void Update(FactionNPC faction, double seconds);
        protected abstract void Generate(RDRace pref, FactionNPC faction, bool fromScratch);
    }
}