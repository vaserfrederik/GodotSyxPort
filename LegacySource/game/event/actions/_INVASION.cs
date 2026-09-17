using System;
using System.Collections.Generic;
using game.event.engine;
using game.faction;
using game.raiding;
using init.race;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;
using world.army;
using world.region;

namespace game.event.actions
{
    final class _INVASION : EventActionConstructor
    {
        private static CharSequence ¤¤arrive = "The army of {0} has now arrived.";

        static
        {
            D.ts(typeof(_INVASION));
        }

        _INVASION() : base("INVASION")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public final class Imp : EventAction
        {
            private readonly Race race;
            private readonly double amountFrom;
            private readonly double amountTo;

            Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                race = RACES.map().readTry("RACE", data);
                amountFrom = data.d("AMOUNT_FROM", 0, 1000);
                amountTo = data.d("AMOUNT_TO", amountFrom, 1000);
                data.checkUnused();
            }

            public override void exe(Event event, EContext data)
            {
                if (race == null)
                    return;

                double pow = 0;
                LIST<RaidRegion> vv = GAME.raiders().entry.entryRegions();
                if (vv.size() > 0)
                {
                    pow = double.MaxValue;
                    foreach (RaidRegion reg in vv)
                    {
                        double p = RD.MILITARY().power.getD(reg.r());
                        if (p < pow)
                            pow = p;
                    }
                }
                pow += AD.power().get(FACTIONS.player());
                pow *= (amountFrom + RND.rFloat() * (amountTo - amountFrom));
                Raider rr = new Raider(race, pow);
                rr.text.set(rr, true);

                GAME.raiders().current.raid(rr, "" + Str.TMP.clear().add(¤¤arrive).insert(0, rr.name));
            }
        }
    }
}