using System.Collections.Generic;
using init.race;
using snake2d.util.sets;
using util.text;
using world.army.ADInit;
using world.entity.army;

namespace world.army
{
    internal sealed class ADSoldiers
    {
        private readonly ArrayList<ADIntImp> current = new ArrayList<ADIntImp>(RACES.all().Count);
        private readonly ArrayList<ADIntImp> target = new ArrayList<ADIntImp>(RACES.all().Count);
        private readonly ADIntImp currentTot;
        private readonly ADIntImp targetTot;

        public ADSoldiers(ADInit init)
        {
            currentTot = new ADIntImp(init, "SOLDIERS", Dic.¤¤Soldiers, "");
            targetTot = new ADIntImp(init, "SOLDIERS_TARGET", Dic.¤¤SoldiersTarget, "");

            foreach (Race r in RACES.all())
            {
                ADIntImp ii = new ADIntImp(init, "SOLDIERS_" + r.key, Dic.¤¤Soldiers + ": " + r.info.names, "")
                {
                    public override void count(WArmy t, int delta)
                    {
                        currentTot.inc(t, get(t) * delta);
                        base.count(t, delta);
                    }
                };
                current.add(ii);
                ii = new ADIntImp(init, "SOLDIERS_TAR_" + r.key, Dic.¤¤SoldiersTarget + ": " + r.info.names, "")
                {
                    public override void count(WArmy t, int delta)
                    {
                        targetTot.inc(t, get(t) * delta);
                        base.count(t, delta);
                    }
                };
                target.add(ii);
            }

            init.registers.add(new Register()
            {
                public override void register(ADDiv div, int d)
                {
                    current.get(div.race().index()).inc(div.army(), d * div.men());
                    target.get(div.race().index()).inc(div.army(), d * div.menTarget());
                }
            });
        }

        public ADInt current(Race race)
        {
            if (race == null)
                return currentTot;
            return current.get(race.index);
        }

        public ADInt target(Race race)
        {
            if (race == null)
                return targetTot;
            return target.get(race.index);
        }
    }
}