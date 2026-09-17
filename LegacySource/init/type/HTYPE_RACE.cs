using System;
using System.Collections.Generic;
using util.keymap;

namespace init.type
{
    public class HTYPE_RACE : MAPPED
    {
        public readonly int index;
        public readonly HTYPE cl;
        public readonly Race race;
        private readonly string key;

        public HTYPE_RACE(int index, HTYPE cl, Race race)
        {
            this.index = index;
            this.cl = cl;
            this.race = race;
            key = (cl == null ? "NULL" : cl.key) + "_" + (race == null ? "NULL" : race.key);
        }

        public override int index()
        {
            return index;
        }

        public override string key()
        {
            return key;
        }

        public override string ToString()
        {
            return "HTYPE_RACE : " + cl + " " + race;
        }

        public static HTYPE_RACE get()
        {
            return all.classes[0][0];
        }

        public static HTYPE_RACE get(Induvidual i)
        {
            return get(i.race(), i.hType());
        }

        public static HTYPE_RACE get(Race race)
        {
            int ci = 0;
            int ri = race == null ? 0 : race.index + 1;
            return all.classes[ci][ri];
        }

        public static HTYPE_RACE get(HTYPE clas)
        {
            int ci = clas == null ? 0 : clas.index() + 1;
            int ri = 0;
            return all.classes[ci][ri];
        }

        public static HTYPE_RACE get(Race race, HTYPE clas)
        {
            int ci = clas == null ? 0 : clas.index() + 1;
            int ri = race == null ? 0 : race.index + 1;
            return all.classes[ci][ri];
        }

        public static RMAPS<HTYPE_RACE> MAP()
        {
            return MAP;
        }

        public static LIST<HTYPE_RACE> ALL()
        {
            return all.all;
        }

        public static LIST<HTYPE_RACE> ALL_REAL()
        {
            return all.allReal;
        }

        private static RClasses all;
        private static RMAPS<HTYPE_RACE> MAP;

        static void init(HTYPES cl, RACES races)
        {
            all = new RClasses(RACES.all(), HTYPES.ALL());
            var pps = new ArrayListGrower<HTYPE_RACE>();
            foreach (HTYPE_RACE p in all.all)
                if (p.cl != null && p.race != null)
                    pps.add(p);
            MAP = new RMAPS<HTYPE_RACE>("PHTYPE_RACE", pps);
        }

        private static class RClasses
        {
            private readonly LIST<HTYPE_RACE> all;
            private readonly LIST<HTYPE_RACE> allReal;
            private readonly HTYPE_RACE[][] classes;

            public RClasses(LIST<Race> races, LIST<HTYPE> types)
            {
                classes = new HTYPE_RACE[types.size() + 1][races.size() + 1];
                var all = new ArrayList<HTYPE_RACE>((races.size() + 1) * (types.size() + 1));
                var real = new ArrayList<HTYPE_RACE>(races.size() * types.size());
                foreach (Race r in races)
                {
                    foreach (HTYPE cl in types)
                    {
                        HTYPE_RACE t = new HTYPE_RACE(all.size(), cl, r);
                        all.add(t);
                        real.add(t);
                    }
                }
                foreach (Race r in races)
                {
                    all.add(new HTYPE_RACE(all.size(), null, r));
                }
                foreach (HTYPE cl in types)
                {
                    HTYPE_RACE t = new HTYPE_RACE(all.size(), cl, null);
                    all.add(t);
                }
                HTYPE_RACE t = new HTYPE_RACE(all.size(), null, null);
                all.add(t);

                foreach (HTYPE_RACE tt in all)
                {
                    int ci = tt.cl == null ? 0 : tt.cl.index() + 1;
                    int ri = tt.race == null ? 0 : tt.race.index + 1;
                    classes[ci][ri] = tt;
                }

                this.all = all;
                this.allReal = real;
            }
        }
    }
}