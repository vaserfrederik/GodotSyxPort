using System;
using game.faction;
using snake2d.util.sprite.text;
using util.data;
using world.map.regions;

namespace util.text
{
    final class InsertRegion : Inserter<Region>
    {
        public InsertRegion()
        {
            new II("NAME")
            {
                public void set(Region t, Str str)
                {
                    if (t == null)
                        return;
                    str.add(t.info.name());
                }
            };

            join(new Inserter<Faction>(new InsertFaction(), "FACTION_"), new GETTER_TRANS<Region, Faction>()
            {
                public Faction get(Region f)
                {
                    if (f.faction() == null)
                        return FACTIONS.player();
                    return f.faction();
                }
            });
        }
    }
}