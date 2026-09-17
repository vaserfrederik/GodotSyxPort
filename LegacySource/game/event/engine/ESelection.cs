using System;
using System.Collections.Generic;
using game.event.actions;
using game.faction;
using game.faction.royalty;
using init.value;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.sets;
using world.map.regions;

namespace game.event.engine
{
    public sealed class ESelection
    {
        public LIST<EventAction> onFail = EActions.actions();
        public ESelectionType<Induvidual> indu = new ESelectionType<Induvidual>();
        public ESelectionType<Region> reg = new ESelectionType<Region>();
        public ESelectionType<Faction> faction = new ESelectionType<Faction>();
        public ESelectionType<Royalty> royalty = new ESelectionType<Royalty>();

        public ESelection(Event e, EventActions act, Json data)
        {
            if (data.has("SELECTION"))
            {
                data = data.json("SELECTION");
                onFail = EActions.actions("ON_FAIL", e, act, data);

                indu.read(data, "SUBJECTS", GVALUES.INDU);
                reg.read(data, "REGIONS", GVALUES.REGION);
                faction.read(data, "FACTIONS", GVALUES.FACTION);
                royalty.read(data, "ROYALTIES", GVALUES.ROYALTY);
                data.checkUnused();
            }
        }

        public sealed class ESelectionType<T>
        {
            public ArrayListGrower<Lockable<T>> filters = new ArrayListGrower<Lockable<T>>();
            public EAmount min;
            public EAmount max;
            public bool useAsIcon = false;
            public ESelectionMark mark = new ESelectionMark();

            public ESelectionType()
            {
                min = new EAmount(0);
                max = new EAmount(int.MaxValue);
            }

            public void read(Json data, string key, GValueCat<T> ll)
            {
                if (data.has(key))
                {
                    Json dd = data.json(key);
                    min = am(dd, "MIN_AMOUNT", 0);
                    max = am(dd, "MAX_AMOUNT", int.MaxValue);
                    if (dd.has("FILTERS"))
                    {
                        foreach (Json j in dd.jsons("FILTERS"))
                        {
                            Lockable<T> lockItem = ll.LOCK.push();
                            lockItem.pushPush(j);
                            filters.add(lockItem);
                        }
                    }
                    useAsIcon = dd.bool("USE_AS_ICON", false);
                    mark.read(dd);
                    dd.checkUnused();
                }
            }

            private EAmount am(Json data, string key, int fallback)
            {
                if (data.has(key))
                {
                    Json json = data.json(key);
                    return new EAmount(json, 0);
                }
                else
                {
                    return new EAmount(fallback);
                }
            }
        }

        private sealed class ESelectionMark
        {
            public bool mark = false;
            public string clear = null;
            public string filter = null;

            public void read(Json json)
            {
                if (json.has("MARK"))
                {
                    json = json.json("MARK");
                    mark = json.bool("MARK_WITH_EVENT_KEY", false);
                    clear = json.value("CLEAR_EVENT_KEY", null);
                    filter = json.value("ALLOW_ONLY_MARK", null);
                    json.checkUnused();
                }
            }
        }
    }
}