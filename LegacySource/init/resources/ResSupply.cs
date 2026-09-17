using System;
using System.Collections.Generic;
using util.gui.misc;
using util.keymap;
using util.text;
using world.army;

namespace init.resources
{
    public class ResSupply : MAPPED
    {
        public readonly string name;
        public readonly RESOURCE resource;
        public readonly double morale;
        public readonly double health;
        public readonly double consumptionPerPersonday;
        public readonly double consumptionPerItemPerDay;
        public readonly int wantedPerPerson;
        private readonly int[] uses;
        private readonly int index;
        readonly string key;

        private static readonly string ¤¤SupplyRace = "Affected Races";
        private static readonly string ¤¤desc = "Supplies are needed by by certain conscripts and city troops to function. They are shipped automatically from your supply depots in your capitol. An army stores up to 6 days of worth of excess supply, in case supply lines become compromised.";
        private static readonly string ¤¤stored = "Each soldier of this supply wants at least {0} items stored per soldier.";
        private static readonly string ¤¤consume = "Each soldier of this supply consumes {0} items per year.";
        private static readonly string ¤¤consumeItem = "Each items stored degrades by -{0} per year.";
        private static readonly string ¤¤morale = "This item increases morale by at most {0}% for the soldiers affected.";
        private static readonly string ¤¤health = "This is an essential supply for the affected soldiers, and low stocks will lead to {0}% poorer health and desertion.";

        static
        {
            D.ts(typeof(ResSupply));
        }

        public ResSupply(string key, Json json, LISTE<ResSupply> all)
        {
            this.index = all.add(this);
            resource = RESOURCES.map().read(json);
            this.key = key;
            morale = json.d("MORALE_ADD", 0, 1);
            health = json.d("HEALTH_EFFECT", 0, 1);
            consumptionPerPersonday = json.d("CONSUMPTION_PER_USER_DAY", 0, 1000);
            consumptionPerItemPerDay = json.d("CONSUMPTION_PER_ITEM_DAY", 0, 1000);
            wantedPerPerson = json.i("AMOUNT_PER_PERSON");
            uses = new int[RACES.all().size()];
            foreach (Race r in RACES.map().readMany("RACES", json))
            {
                setRace(r, 1);
            }
            name = Dic.¤¤Supplies + ": " + resource.name;
        }

        public override int index()
        {
            return index;
        }

        public int consumedMulPerDay(Race race)
        {
            return uses[race.index];
        }

        public override string key()
        {
            return key;
        }

        void setRace(Race race, int amount)
        {
            uses[race.index] = amount;
        }

        public int amount(Race race, int men)
        {
            return (int)(consumptionPerPersonday * consumedMulPerDay(race) * men * ADSupply.STOCKPILE_DAYS + wantedPerPerson * consumedMulPerDay(race) * men);
        }

        public void hover(GBox b)
        {
            b.title(name);
            b.text(¤¤desc);

            b.NL();
            b.textLL(¤¤SupplyRace);
            b.NL();
            foreach (Race r in RACES.all())
            {
                if (consumedMulPerDay(r) > 0)
                {
                    b.add(r.appearance().icon);
                }
            }
            b.NL();

            if (wantedPerPerson > 0)
            {
                GText t = b.text();
                t.add(¤¤stored);
                t.insert(0, wantedPerPerson);
                b.add(t);
                b.NL();
            }

            if (consumptionPerPersonday > 0)
            {
                GText t = b.text();
                t.add(¤¤consume);
                t.insertD(0, consumptionPerPersonday * TIME.years().bitConversion(TIME.days()), 5);
                b.add(t);
                b.NL();
            }

            if (consumptionPerItemPerDay > 0)
            {
                GText t = b.text();
                t.add(¤¤consumeItem);
                t.insertD(0, consumptionPerItemPerDay * TIME.years().bitConversion(TIME.days()), 5);
                b.add(t);
                b.NL();
            }

            if (morale > 0)
            {
                GText t = b.text();
                t.add(¤¤morale);
                t.insert(0, (int)(morale * 100));
                b.add(t);
                b.NL();
            }

            if (health > 0)
            {
                GText t = b.text();
                t.add(¤¤health);
                t.insert(0, (int)(health * 100));
                t.warnify();
                b.add(t);
                b.NL();
            }
        }
    }
}