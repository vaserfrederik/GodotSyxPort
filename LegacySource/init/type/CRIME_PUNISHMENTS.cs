using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Type
{
    public class CRIME_PUNISHMENTS
    {
        private static readonly string ¤¤freedom = "Crime Tyranny Multiplier";
        private static readonly string ¤¤law = "Crime Law Multiplier";

        private readonly PUNISHMENT PARDON;
        private readonly PUNISHMENT STOCKS;
        private readonly PUNISHMENT BANISH;
        private readonly PUNISHMENT PRISON;
        private readonly PUNISHMENT EXECUTE;
        private readonly PUNISHMENT HARVEST;
        private readonly PUNISHMENT ENSLAVE;

        private readonly List<PUNISHMENT> all;
        private readonly List<PUNISHMENT> CITIZENS;
        private readonly List<PUNISHMENT> SLAVES;
        private readonly List<PUNISHMENT> WAR;
        private static CRIME_PUNISHMENTS self;

        public CRIME_PUNISHMENTS(INIT init)
        {
            self = this;
            List<PUNISHMENT> allList = new List<PUNISHMENT>();

            Json json = new Json(PATHS.CONFIG().init.gets("LAW")).json("PUNISHMENTS");
            Json desc = new Json(PATHS.CONFIG().text.gets("LAW")).json("PUNISHMENTS");

            PARDON = new PUNISHMENT(json, desc, allList, "PARDON");
            STOCKS = new PUNISHMENT(json, desc, allList, "NONE");
            BANISH = new PUNISHMENT(json, desc, allList, "BANISH");
            PRISON = new PUNISHMENT(json, desc, allList, "PRISON");
            EXECUTE = new PUNISHMENT(json, desc, allList, "EXECUTE");
            HARVEST = new PUNISHMENT(json, desc, allList, "HARVEST");
            ENSLAVE = new PUNISHMENT(json, desc, allList, "ENSLAVE");
            this.all = allList;
            CITIZENS = new List<PUNISHMENT> { PARDON, STOCKS, BANISH, PRISON, ENSLAVE, EXECUTE, HARVEST };
            SLAVES = new List<PUNISHMENT> { PARDON, STOCKS, BANISH, PRISON, EXECUTE, HARVEST };
            WAR = new List<PUNISHMENT> { PARDON, STOCKS, BANISH, ENSLAVE, EXECUTE, HARVEST };

            foreach (PUNISHMENT c in CITIZENS)
            {
                c.available[HCLASSES.CITIZEN().index()] = true;
            }

            foreach (PUNISHMENT c in SLAVES)
            {
                c.available[HCLASSES.SLAVE().index()] = true;
            }

            foreach (PUNISHMENT c in WAR)
            {
                c.available[HCLASSES.OTHER().index()] = true;
            }
        }

        public static List<PUNISHMENT> get(HCLASS cl)
        {
            if (cl == HCLASSES.CITIZEN())
                return self.CITIZENS;
            if (cl == HCLASSES.SLAVE())
                return self.SLAVES;
            return self.WAR;
        }

        public static PUNISHMENT STOCKS()
        {
            return self.STOCKS;
        }

        public static PUNISHMENT PARDON()
        {
            return self.PARDON;
        }

        public static PUNISHMENT BANISH()
        {
            return self.BANISH;
        }

        public static PUNISHMENT PRISON()
        {
            return self.PRISON;
        }

        public static PUNISHMENT EXECUTE()
        {
            return self.EXECUTE;
        }

        public static PUNISHMENT ENSLAVE()
        {
            return self.ENSLAVE;
        }

        public static PUNISHMENT HARVEST()
        {
            return self.HARVEST;
        }

        public static List<PUNISHMENT> ALL()
        {
            return self.all;
        }

        public class PUNISHMENT : INDEXED
        {
            public string name, names, action, verb, desc;
            private readonly int index;
            public readonly string key;

            public readonly Icon icon;

            private readonly double value;
            private readonly double cruelty;
            private readonly double mercy;

            private readonly bool[] available = new bool[HCLASSES.ALL().size()];

            public PUNISHMENT(Json json, Json text, List<PUNISHMENT> all, string key)
            {
                this.index = all.Count;
                all.Add(this);

                this.key = key;
                Json t = text.json(key);
                name = t.text("NAME");
                names = t.text("NAMES");
                action = t.text("ACTION");
                verb = t.text("VERB");
                desc = t.text("DESC");

                Json j = json.json(key);

                icon = SPRITES.icons().get(j);
                value = j.d("VALUE", 0, 1);
                mercy = j.dTry("MERCY", 0, 1, 0);
                cruelty = j.dTry("CRUELTY", 0, 1, 0);
            }

            public int index()
            {
                return index;
            }

            public double defaultValue()
            {
                return value;
            }

            public double crueltyValue(HCLASS cl, Race race)
            {
                return cruelty;
            }

            public double crueltyPerPerson(HCLASS cl, Race race)
            {
                return 100 * cruelty / (1 + POP.tot());
            }

            public double mercyValue(HCLASS cl, Race race)
            {
                return mercy;
            }

            public double mercyPerPerson(HCLASS cl, Race race)
            {
                return 100 * mercy / POP.tot();
            }

            public double tyranny(HCLASS cl, Race race)
            {
                if (race == null)
                {
                    double pop = 0;
                    double v = 0;
                    for (int ri = 0; ri < RACES.all().size(); ri++)
                    {
                        int p = STATS.POP().POP.data(cl).get(RACES.all().get(ri));
                        pop += p;
                        v += p * RACES.all().get(ri).pref().punishment(this);
                    }
                    if (pop == 0)
                        return value;
                    return v / pop;
                }
                return race.pref().punishment(this);
            }

            public double law(HCLASS cl, Race race)
            {
                return Math.Sqrt(tyranny(cl, race));
            }

            public StatPunishment stat()
            {
                return STATS.LAW().punishments.get(index);
            }

            public bool available(HCLASS cl)
            {
                return available[cl.index()];
            }

            public void hoverInfo(GUI_BOX text, HCLASS cl, Race race)
            {
                GBox b = (GBox)text;
                b.title(name);
                b.text(desc);

                b.NL();

                b.textLL(¤¤freedom);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), tyranny(cl, race)));
                b.NL();

                b.textLL(¤¤law);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), law(cl, race)));
                b.NL();
            }

            public override string ToString()
            {
                return "PUN_" + key;
            }
        }
    }
}