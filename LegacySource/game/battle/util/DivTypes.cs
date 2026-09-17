using System;
using System.Collections.Generic;
using game.battle.util;
using game;
using game.faction;
using init.paths;
using init.race;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;

public class DivTypes
{
    private readonly ArrayListGrower<DivType> types = new ArrayListGrower<DivType>();
    private readonly double[] occMaxs = new double[RACES.all().Size()];
    private DivType tmp = new DivType();

    public DivTypes()
    {
        PATH p = PATHS.INIT().getFolder("battle").getFolder("divType");

        foreach (string f in p.getFiles())
        {
            Json j = new Json(p.gets(f));
            Json[] mm = j.jsons("TYPES");

            foreach (Json jj in mm)
            {
                double occ = jj.d("OCCURENCE");
                LIST<StatTraining> tr = STATS.BATTLE().TRAINING_MAP.readMany(jj);
                Json eqs = jj.json("EQUIPMENT");
                LIST<string> ekeys = eqs.keys();
                occ /= ekeys.size();
                foreach (string k in ekeys)
                {
                    LIST<EquipBattle> eqps = STATS.EQUIP().militaryColl.readMany(k, eqs);
                    types.add(new DivType(occ, tr, eqps));
                }
            }
        }

        GAME.addOnInit(new AA());
    }

    public DivType rnd(Race race, Faction f, double ran)
    {
        ran -= (int)ran;
        ran *= occMaxs[race.index];

        for (int i = 0; i < types.size(); i++)
        {
            if (!types.get(i).valid(race))
                continue;
            ran -= types.get(i).roccurence[race.index()];
            if (ran <= 0)
                return types.get(i);
        }

        DivType r = types.rnd();

        foreach (EquipBattle b in STATS.EQUIP().BATTLE_ALL())
        {
            if (b.allowed(race))
                tmp.equip[b.indexMilitary()] = r.equip(b);
            else
                tmp.equip[b.indexMilitary()] = 0;
        }

        for (int i = 0; i < tmp.training.Length; i++)
            tmp.training[i] = r.training[i];

        return tmp;
    }

    void debug()
    {
        foreach (DivType t in types)
        {
            LOG.ln(t.occurence);
            foreach (StatTraining tr in STATS.BATTLE().TRAINING_ALL)
                LOG.ln(tr.stat.stats.info().name + " " + t.training(tr));
            foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                LOG.ln(e.resource.name + " " + t.equip(e));

            foreach (Race r in RACES.all())
            {
                LOG.ln(r.key + " " + t.roccurence[r.index()] / occMaxs[r.index()]);
            }
            LOG.ln();
        }
    }

    public LIST<DivType> ALL()
    {
        return types;
    }

    private class AA : ACTION
    {
        private Race race;
        private DivType type;

        private DIV_SPEC stats = new DIV_SPEC()
        {
            public double training(StatTraining tr)
            {
                return type.training(tr);
            }

            public double equip(EquipBattle e)
            {
                return type.equip(e);
            }

            public Race race()
            {
                return race;
            }

            public int men()
            {
                return 10;
            }

            public Faction faction()
            {
                return null;
            }

            public double experience()
            {
                return 0.2;
            }

            public CharSequence name()
            {
                return Dic.empty;
            }

            public int bannerI()
            {
                return 0;
            };
        };

        public void exe()
        {
            foreach (DivType t in types)
            {
                type = t;
                for (int ri = 0; ri < RACES.all().Size(); ri++)
                {
                    race = RACES.all().get(ri);
                    if (!t.valid(race))
                        type.roccurence[ri] = 0;
                    else
                        type.roccurence[ri] = type.occurence * GAME.battle().power.get(stats);

                    occMaxs[ri] += type.roccurence[ri];
                }
            }
            //debug();
            //debug();
        }
    }
}