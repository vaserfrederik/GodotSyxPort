using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.race;
using init.type;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using world.region;

namespace util.text
{
    final class InsertFaction : Inserter<Faction>
    {
        public InsertFaction()
        {
            new II("FACTION")
            {
                public override void Set(Faction t, Str str)
                {
                    str.Add(t.name);
                }
            };

            new II("FACTION_RULER")
            {
                public override void Set(Faction t, Str str)
                {
                    if (t == FACTIONS.Player())
                        str.Add(FACTIONS.Player().rulerName());
                    else
                        str.Add(((FactionNPC)t).court().king().name);
                }
            };

            new II("FACTION_RULER_TITLE")
            {
                public override void Set(Faction t, Str str)
                {
                    if (t == FACTIONS.Player())
                        str.Add(FACTIONS.Player().level().current().name());
                    else
                    {
                        double d = t.realm().regions() / 20.0;
                        int i = (int)(d * FACTIONS.Player().level().all().size());
                        i = CLAMP.i(i, 0, FACTIONS.Player().level().all().size() - 1);
                        str.Add(FACTIONS.Player().level().all().get(i).male);
                    }
                }
            };

            new II("FACTION_RULER_INTRO")
            {
                public override void Set(Faction f, Str str)
                {
                    if (f == FACTIONS.Player())
                        str.Add(RD.RACE(f.race()).names.rIntro.get(0));
                    else
                        str.Add(((FactionNPC)f).nameIntro);
                }
            };

            new II("FACTION_RULER_TITLES")
            {
                public override void Set(Faction ff, Str str)
                {
                    if (ff == FACTIONS.Player())
                    {
                        foreach (PTitle t in FACTIONS.Player().titles.all())
                        {
                            if (t.selected())
                            {
                                str.Add(t.name);
                                str.Add(',').s();
                            }
                        }
                    }
                    else
                    {
                        FactionNPC t = (FactionNPC)ff;
                        LIST<TRAIT> tt = t.court().king().roy().traits;
                        for (int i = 0; i < tt.size(); i++)
                        {
                            str.Add(tt.get(i).rTitle);
                            if (i < tt.size() - 1)
                                str.Add(',').s();
                        }
                    }
                }
            };

            Join(new Inserter<Race>(new InsertRace(), "FACTION_"), new GETTER_TRANS<Faction, Race>()
            {
                public Race Get(Faction f)
                {
                    return f.race();
                }
            });
        }
    }
}