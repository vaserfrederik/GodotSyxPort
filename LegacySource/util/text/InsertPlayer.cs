using System;
using System.Collections.Generic;
using game.faction;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d.util;
using snake2d.util.sprite.text;
using world;
using world.map.regions;
using world.region;

namespace util.text
{
    final class InsertPlayer : Inserter<int>
    {
        public InsertPlayer()
        {
            new II("PLAYER_RND_REGION_CLOSE")
            {
                public override void Set(int t, Str str)
                {
                    Region res = FACTIONS.player().capitolRegion();
                    int ri = t;
                    for (int i = 0; i < WREGIONS.MAX; i++)
                    {
                        Region reg = WORLD.REGIONS().all().getC(ri + i);
                        if (reg.active() && reg != FACTIONS.player().capitolRegion() && RD.DIST().distance().get(reg) < 200)
                        {
                            res = reg;
                            break;
                        }
                    }

                    if (res != null)
                        str.add(res.info.name());
                }
            };

            new II("PLAYER_RND_REGION_FAR")
            {
                public override void Set(int t, Str str)
                {
                    Region res = FACTIONS.player().capitolRegion();
                    int ri = t + 10;
                    for (int i = 0; i < WREGIONS.MAX; i++)
                    {
                        Region reg = WORLD.REGIONS().all().getC(ri + i);
                        if (reg.active() && reg != FACTIONS.player().capitolRegion() && RD.DIST().distance().get(reg) > 200)
                        {
                            res = reg;
                            break;
                        }
                    }

                    if (res != null)
                        str.add(res.info.name());
                }
            };

            new II("PLAYER_RND_FACTION_CLOSE")
            {
                public override void Set(int t, Str str)
                {
                    Faction res = null;
                    int ri = t;
                    for (int i = 0; i < FACTIONS.MAX(); i++)
                    {
                        Faction f = FACTIONS.getByIndex(MATH.mod(ri + i, WREGIONS.MAX));
                        if (f != null && f.isActive() && f != FACTIONS.player() && RD.DIST().distance().get(f.capitolRegion()) < 200)
                        {
                            res = f;
                            break;
                        }
                    }

                    if (res != null)
                        str.add(res.name);
                    else
                        str.add("Empire of Sand");
                }
            };

            new II("PLAYER_RND_FACTION_FAR")
            {
                public override void Set(int t, Str str)
                {
                    Faction res = null;
                    int ri = t + 10;
                    for (int i = 0; i < FACTIONS.MAX(); i++)
                    {
                        Faction f = FACTIONS.getByIndex(MATH.mod(ri + i, WREGIONS.MAX));
                        if (f != null && f.isActive() && f != FACTIONS.player() && RD.DIST().distance().get(f.capitolRegion()) > 200)
                        {
                            res = f;
                            break;
                        }
                    }

                    if (res != null)
                        str.add(res.name);
                    else
                        str.add("Empire of Sand");
                }
            };

            for (int i = 0; i < 2; i++)
            {
                int kk = i;
                new II("PLAYER_CITY_RND_NAME_RACE_" + (i + 1))
                {
                    public override void Set(int t, Str str)
                    {
                        str.add("Bob");
                        int ri = t & int.MaxValue;
                        int skip = kk;
                        ENTITY[] ee = SETT.ENTITIES().getAllEnts();
                        int f = ri % ee.Length;
                        for (int k = 0; k < ee.Length; k++)
                        {
                            f++;
                            if (f >= ee.Length)
                                f = 0;
                            if (ee[f] is Humanoid)
                            {
                                Humanoid a = (Humanoid)ee[f];
                                if (a.indu().player() && a.race() == FACTIONS.player().race())
                                {
                                    skip--;
                                    if (skip < 0)
                                    {
                                        str.clear().add(STATS.APPEARANCE().name(a.indu()));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                };
            }

            new II("PLAYER_CITY_ROOM_RND")
            {
                public override void Set(int t, Str str)
                {
                    int tot = 0;
                    foreach (RoomBlueprintIns<?> in in SETT.ROOMS().ins())
                    {
                        if (in is RoomBlueprintIns<?>)
                            tot += in.instancesSize();
                    }

                    tot &= t & int.MaxValue;

                    foreach (RoomBlueprintIns<?> in in SETT.ROOMS().ins())
                    {
                        if (in is RoomBlueprintIns<?>)
                        {
                            if (tot >= in.instancesSize())
                                tot -= in.instancesSize();
                            else
                            {
                                str.add((in.getInstance(tot).name()));
                                return;
                            }
                        }
                        if (tot <= 0)
                        {

                        }
                    }
                    str.add("no rooms");
                }
            };
        }
    }
}