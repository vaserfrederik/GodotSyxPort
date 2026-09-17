using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.service;
using snake2d.util;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.data;
using util.text;

namespace game.tourism
{
    public static class Text
    {
        public static readonly Inserter<InsertData> insert = new Inserter<InsertData>();

        static Text()
        {
            insert.Join(INSERT.indu, new GETTER_TRANS<InsertData, Induvidual>()
            {
                public Induvidual Get(InsertData f)
                {
                    return f.i;
                }
            });

            insert.Join(INSERT.faction, new GETTER_TRANS<InsertData, Faction>()
            {
                public Faction Get(InsertData f)
                {
                    return FACTIONS.Player();
                }
            });

            insert.Join(INSERT.player, new GETTER_TRANS<InsertData, int>()
            {
                public int Get(InsertData f)
                {
                    return STATS.RAN().Get(f.i, 0);
                }
            });

            insert.NewII("ATTRACTION", (InsertData t, Str str) =>
            {
                RoomBlueprintImp att = TOURISM.Attraction(t.i);
                str.Add(att.info.name);
            });

            insert.NewII("ATTRACTIONS", (InsertData t, Str str) =>
            {
                RoomBlueprintImp att = TOURISM.Attraction(t.i);
                str.Add(att.info.names);
            });

            insert.NewII("ATTRACTION_EMPLOYEE", (InsertData t, Str str) =>
            {
                RoomBlueprintImp att = TOURISM.Attraction(t.i);
                str.Add(att.employment().title);
            });

            insert.NewII("SERVICE", (InsertData t, Str str) =>
            {
                StatService s = TOURISM.Service(t.i);
                str.Add(s.name);
            });

            insert.NewII("INN_NAME", (InsertData t, Str str) =>
            {
                RoomInstance ins = SETT.ROOMS().INN.getter.Get(t.inn);
                if (ins != null)
                    str.Add(ins.name());
            });

            insert.NewII("INN_HOST", (InsertData t, Str str) =>
            {
                RoomInstance ins = SETT.ROOMS().INN.getter.Get(t.inn);
                if (ins == null)
                    return;

                if (ins.employees().employed() > 0)
                {
                    int e = (int)(ins.employees().employed() * RND.rFloat());
                    foreach (Humanoid a in ins.employees().employees())
                    {
                        if (e-- <= 0)
                        {
                            str.Add(STATS.APPEARANCE().name(a.indu()));
                            return;
                        }
                    }
                }
                str.Add(STATS.APPEARANCE().name(RACES.all().Rnd(), HTYPES.SUBJECT(), 0, RND.rInt(StatsAppearance.NAME_MAX)));
            });
        }

        public class InsertData
        {
            public double rating;
            public Induvidual i;
            public COORDINATE inn;
        }

        public static readonly InsertData dd = new InsertData();

        public readonly Entry rating;
        public readonly Entry attraction;
        public readonly Entry service;
        public readonly Entry inn;

        private static readonly Str str = new Str(128);

        public Text(Json json)
        {
            rating = new Entry(0, json, "RATING");
            attraction = new Entry(1, json, "ATTRACTION");
            service = new Entry(2, json, "SERVICE");
            inn = new Entry(3, json, "INN");
        }

        private class Entry
        {
            private CharSequence[][] chars = new CharSequence[3][];
            private readonly int scroll;

            public Entry(int index, Json json, string key)
            {
                scroll = index * 8;
                if (json != null)
                {
                    json = json.Json(key);
                    chars[0] = insert.Check(json.texts("BAD"));
                    chars[1] = insert.Check(json.texts("OK"));
                    chars[2] = insert.Check(json.texts("GOOD"));
                }
                else
                {
                    chars[0] = new CharSequence[] { "" };
                    chars[1] = new CharSequence[] { "" };
                    chars[2] = new CharSequence[] { "" };
                }
            }

            public Str Get(InsertData data)
            {
                int ri = (int)Math.Round(data.rating * 2 - 0.25);
                ri = CLAMP.i(ri, 0, 2);
                int r = (int)STATS.RAN().Get(data.i, scroll);
                str.Clear().Add(chars[ri][MATH.Mod(r, chars[ri].Length)]);
                insert.Set(str, data);
                return str;
            }
        }
    }
}