using System;
using System.Collections.Generic;
using System.Linq;
using util.data;
using util.gui.table;
using view.interrupter;

namespace view.sett.ui.standing
{
    final class Cats
    {
        public readonly List<Cat> all = new List<Cat>(STATS.COLLECTIONS().Count);

        private ISidePanel[] panels = new ISidePanel[STATS.COLLECTIONS().Count];
        public readonly Cat access;

        private readonly HCLASS cl;
        private readonly GETTER<Race> race;
        Cats(HCLASS cl, GETTER<Race> race)
        {
            this.race = race;
            this.cl = cl;
            access = new CatAccess(cl, race);
            Add(new CatPopulation(cl, race));
            Add(access);
            Add(new CatServices(cl, race));
            Add(new CatEnv(cl, race));
            Add(new CatReligion(cl, race));
            Add(new CatOccupation(cl, race));
            Add(new CatGovern(cl, race));
            foreach (StatCollection c in STATS.COLLECTIONS())
            {
                if (panels[c.index()] == null && HasStanding(c))
                    Add(new CatDummy(cl, race, c));
            }
        }

        private void Add(Cat p)
        {
            all.Add(p);
            foreach (StatCollection c in p.cs)
                panels[c.index()] = p;
        }

        private int updateI = -1;
        private double biggest = 0;

        double GetBiggest()
        {
            if (updateI == GAME.updateI())
                return biggest;

            updateI = GAME.updateI();
            biggest = 0;

            foreach (Cat ca in all)
            {
                double m = 0;
                foreach (StatCollection c in ca.cs)
                {
                    foreach (STAT s in c.all())
                    {
                        m += s.standing().max(cl, race.get());
                    }
                }
                if (m > biggest)
                    biggest = m;
            }
            return biggest;
        }

        private bool HasStanding(StatCollection c)
        {
            foreach (STAT s in c.all())
            {
                foreach (HCLASS cl in HCLASSES.ALL())
                {
                    foreach (Race r in RACES.all())
                    {
                        if (s.standing().max(cl, r) > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static class Cat : ISidePanel
        {
            public readonly StatCollection[] cs;
            public CharSequence name;
            public CharSequence desc;

            Cat(StatCollection[] cs)
            {
                this.cs = cs;
                titleSet(cs[0].info.name);
                name = cs[0].info.name;
                desc = cs[0].info.name;
            }
        }

        private static class CatDummy : Cat
        {
            CatDummy(HCLASS cl, GETTER<Race> race, params StatCollection[] cs) : base(cs)
            {
                titleSet(cs[0].info.name);

                LinkedList<RENDEROBJ> rens = new LinkedList<RENDEROBJ>();

                foreach (StatCollection c in cs)
                {
                    rens.Add(new StatRow.Title(c.info));
                    foreach (STAT s in c.all())
                    {
                        rens.Add(new StatRow(s, cl, race));
                    }
                }

                section.add(new GScrollRows(rens, HEIGHT, 0).view());
            }
        }
    }
}