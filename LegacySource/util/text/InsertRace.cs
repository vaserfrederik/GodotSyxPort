using System;
using System.Collections.Generic;

namespace util.text
{
    public class InsertRace : Inserter<Race>
    {
        public InsertRace()
        {
            new II("RACE") {
                public override void Set(Race t, Str str)
                {
                    str.Add(t.Info.Name);
                }
            };

            new II("RACES") {
                public override void Set(Race t, Str str)
                {
                    str.Add(t.Info.Names);
                }
            };

            new II("RACIAN") {
                public override void Set(Race t, Str str)
                {
                    str.Add(t.Info.NamePosessive);
                }
            };

            new II("RACIANS") {
                public override void Set(Race t, Str str)
                {
                    str.Add(t.Info.NamePosessives);
                }
            };

            new II("RACE_HELLO") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SHello));
                }
            };

            new II("RACE_GOODBYE") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SGoodbye));
                }
            };

            new II("RACE_CURSE") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SCurse));
                }
            };

            new II("RACE_INSULT") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SInsult));
                }
            };

            new II("RACE_INSULTING") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SInsulting));
                }
            };

            new II("RACE_LORD") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SLord));
                }
            };

            new II("RACE_CITY") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SCity));
                }
            };

            new II("RACE_OTHERS") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SOthers));
                }
            };

            new II("RACE_SELVES") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SSelves));
                }
            };

            new II("RACE_SELF") {
                public override void Set(Race a, Str str)
                {
                    str.Add(S(a.Info.SSelf));
                }
            };

            new II("RACE_NAME_RND") {
                public override void Set(Race a, Str str)
                {
                    int g = Ran();
                    str.Add(a.Appearance.Types.GetC(g).Names.FirstNames.GetC(Ran()));
                    str.S();
                    str.Add(a.Appearance.Types.GetC(g).Names.LastNames.GetC(Ran()));
                }
            };

            new II("RACE_LIKED_BUILDING") {
                public override void Set(Race a, Str str)
                {
                    BUILDING_PREF p = null;
                    foreach (BUILDING_PREF pp in BUILDING_PREFS.ALL())
                    {
                        if (p == null || a.Pref.Structure(pp) > a.Pref.Structure(p))
                            p = pp;
                    }
                    str.Add(p.Name);
                }
            };

            new II("RACE_LIKED_ROAD") {
                public override void Set(Race a, Str str)
                {
                    Floor p = null;
                    foreach (Floor pp in SETT.FLOOR.Roads)
                    {
                        if (p == null || pp.Pref(a) > p.Pref(a))
                            p = pp;
                    }
                    str.Add(p.Name);
                }
            };

            new II("RACE_LIKED_FOOD") {
                public override void Set(Race a, Str str)
                {
                    RESOURCE r = a.Pref.Food.GetC(Ran()).Resource;
                    str.Add(r.Name);
                }
            };

            new II("RACE_LIKED_DRINK") {
                public override void Set(Race a, Str str)
                {
                    RESOURCE r = a.Pref.Drink.GetC(Ran()).Resource;
                    str.Add(r.Name);
                }
            };

            new II("RACE_LIKED_FOODS") {
                public override void Set(Race a, Str str)
                {
                    for (int i = a.Pref.Food.Size - 1; i > 0; i--)
                    {
                        str.Add(a.Pref.Food.Get(i).Resource.Names).Add(',').S();
                    }
                    if (a.Pref.Food.Size > 1)
                        str.Add(Dic.¤¤and).S();
                    str.Add(a.Pref.Food.Get(0).Resource.Names);
                }
            };

            new II("HATED_RACE") {
                public override void Set(Race a, Str str)
                {
                    Race hh = RACES.All.Get(0);
                    foreach (Race r in RACES.All)
                    {
                        if (a.Pref.Race(r) < a.Pref.Race(hh))
                            hh = r;
                    }

                    str.Add(hh.Info.Name);
                }
            };
        }

        private CharSequence S(CharSequence[] ss)
        {
            return ss[Ran() % ss.Length];
        }

        private int Ran()
        {
            // Implement your random number generation logic here
            return 0; // Placeholder
        }
    }
}