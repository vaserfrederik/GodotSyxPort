using System;
using System.Collections.Generic;
using init.race;
using init.resources;
using init.type;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.sets;

public class RaceResources
{
    public readonly LIST<RaceResource> ALL;
    public readonly RBIT BIT;
    private readonly RaceResource[] map;
    private readonly ArrayList<ArrayListGrower<WearableResource>> perCl;
    private readonly LIST<LIST<RES_AMOUNT>> homeres;
    private readonly LIST<RES_AMOUNT> homeresAll;

    public RaceResources(LIST<Race> races)
    {
        map = new RaceResource[RESOURCES.ALL().size()];

        {
            ArrayList<LIST<RES_AMOUNT>> ress = new ArrayList(HCLASSES.ALL().size());
            RES_AMOUNT.Imp[] all = new RES_AMOUNT.Imp[RESOURCES.ALL().size()];

            foreach (HCLASS c in HCLASSES.ALL())
            {
                ArrayList<RES_AMOUNT> rr = new ArrayList(RESOURCES.ALL().size());
                foreach (RESOURCE res in RESOURCES.ALL())
                {
                    int am = 0;
                    foreach (Race r in RACES.all())
                    {
                        am = Math.Max(am, r.home().clas(c).amount(res));
                    }
                    if (am > 0)
                    {
                        rr.Add(new RES_AMOUNT.Abs(res, am));
                        if (all[res.index()] != null)
                        {
                            all[res.index()].set(Math.Max(all[res.index()].amount(), am));
                        }
                        else
                        {
                            all[res.index()] = new RES_AMOUNT.Imp(res, am);
                        }
                    }
                }
                ress.Add(new ArrayList<RES_AMOUNT>(rr));
            }
            homeres = ress;

            LinkedList<RES_AMOUNT> tm = new LinkedList();
            foreach (RES_AMOUNT a in all)
            {
                if (a != null)
                    tm.Add(a);
            }

            homeresAll = new ArrayList<RES_AMOUNT>(tm);
        }

        ArrayListGrower<RaceResource> all = new ArrayListGrower();
        RBITImp bit = new RBITImp();

        foreach (Race r in races)
        {
            foreach (HCLASS cl in HCLASSES.ALL())
            {
                int i = 0;
                foreach (RES_AMOUNT a in r.home().clas(cl).resources())
                {
                    if (map[a.resource().index()] == null)
                    {
                        RaceResource res = new RaceResource(all, a.resource());
                        map[res.res.index()] = res;
                        bit.or(res.res);
                    }
                    map[a.resource().index()].map.get(cl.get(r).index).add(STATS.HOME().furniture(i));
                    i++;
                }
            }
        }

        foreach (EquipCivic e in STATS.EQUIP().civics())
        {
            if (map[e.resource.index()] == null)
            {
                RaceResource res = new RaceResource(all, e.resource);
                map[res.res.index()] = res;
                bit.or(res.res);
            }
            foreach (Race r in races)
            {
                foreach (HCLASS cl in HCLASSES.ALL())
                {
                    map[e.resource().index()].map.get(cl.get(r).index).add(e);
                }
            }
        }

        {
            while (perCl.hasRoom())
                perCl.add(new ArrayListGrower<WearableResource>());
            foreach (HCLASS_RACE cl in HCLASS_RACE.ALL())
            {
                foreach (RaceResource r in all)
                {
                    perCl.get(cl.index).add(r.map.get(cl.index));
                }
            }
        }

        ALL = all;
        BIT = bit;
    }

    public LIST<RES_AMOUNT> homeResMax(HCLASS c)
    {
        if (c == null)
            return homeresAll;
        return homeres.get(c.index());
    }

    public LIST<WearableResource> get(HCLASS_RACE cl, RESOURCE res)
    {
        return map[res.index()].map.get(cl.index);
    }

    public RaceResource get(RESOURCE res)
    {
        return map[res.index()];
    }

    public LIST<WearableResource> all(HCLASS_RACE cl)
    {
        return perCl.get(cl.index);
    }

    public class RaceResource : INDEXED
    {
        public readonly RESOURCE res;
        private readonly int index;
        private readonly ArrayList<ArrayListGrower<WearableResource>> map = new ArrayList(HCLASS_RACE.ALL().size());

        public RaceResource(LISTE<RaceResource> all, RESOURCE res)
        {
            this.res = res;
            this.index = all.add(this);
            while (map.hasRoom())
                map.add(new ArrayListGrower<WearableResource>());
        }

        public int index()
        {
            return index;
        }
    }
}