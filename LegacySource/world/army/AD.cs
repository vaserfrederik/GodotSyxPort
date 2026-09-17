using System;
using System.Collections.Generic;
using System.IO;

namespace World.Army
{
    public sealed class AD : WorldResource
    {
        private static AD self;
        {
            self = this;
        }

        private readonly ArrayListGrower<BoosterAbs<WArmy>> moraleFactors = new ArrayListGrower<BoosterAbs<WArmy>>();
        private readonly ADInit init = new ADInit();
        private readonly ArrayList<ADArmies> fArmies = new ArrayList<ADArmies>(FACTIONS.MAX() + 1);
        private readonly ADConscripts conscripts = new ADConscripts(init);
        private readonly ADSoldiers soldiers = new ADSoldiers(init);
        private readonly ADPower power = new ADPower(init);
        public readonly ADSupplies supplies = new ADSupplies(init);
        private readonly INT_OE<WArmy> data = init.dataA.new DataShort("FACTION");
        private readonly ADStats stats = new ADStats(init);
        private readonly ADUpdater updater = new ADUpdater(init);
        private readonly ADUpdaterDiv updaterDiv = new ADUpdaterDiv(init);
        public readonly WArmyAI AI = new WArmyAI();

        private readonly WDivStoredAll divsCity = new WDivStoredAll();
        private readonly WDivMercenaries divsMerc = new WDivMercenaries();
        private readonly WDivRegionalAll divsReg = new WDivRegionalAll();
        private readonly INT_OE<WArmy> random = init.dataA.new DataInt("Random");

        private readonly WorldResourceManager saver = new WorldResourceManager
        {
            Save = file =>
            {
                foreach (var a in fArmies)
                    a.saver.Save(file);
                updater.Save(file);
                updaterDiv.Save(file);
                AI.saver.Save(file);
                divsCity.Save(file);
                divsMerc.Save(file);
                divsReg.Save(file);
            },

            Load = file =>
            {
                foreach (var a in fArmies)
                    a.saver.Load(file);
                updater.Load(file);
                updaterDiv.Load(file);
                AI.saver.Load(file);
                divsCity.Load(file);
                divsMerc.Load(file);
                divsReg.Load(file);
            },

            Clear = () =>
            {
                foreach (var a in fArmies)
                    a.saver.Clear();
                updater.Clear();
                updaterDiv.Clear();
                AI.saver.Clear();
                divsReg.Clear();
            },

            Generate = loadPrint =>
            {
                foreach (var f in FACTIONS.all())
                {
                    if (f.isActive())
                    {
                        foreach (var a in init.inits)
                        {
                            a.exe(f);
                        }
                        AI.init(f);
                    }
                }
                mercenaries().randmoize();
            }
        };

        public AD(WORLD ww) : base("Armies", "AD")
        {
            fArmies.Add(new ADArmies(-1, 1024));
            while (fArmies.HasRoom())
                fArmies.Add(new ADArmies(fArmies.Size() - 1, 60));
        }

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        protected override void Update(double ds, Profiler prof)
        {
            prof.logStart(this);

            updater.Update(ds);
            updaterDiv.Update(ds);
            divsCity.Update(ds);
            divsMerc.Update(ds);
            AI.Update(ds);
            prof.logEnd(this);
        }

        public static ADInit iinit()
        {
            return self.init;
        }

        public static ADArmies Army(Faction f)
        {
            return self.fArmies.Get(f == null ? 0 : f.index() + 1);
        }

        public static ADConscripts conscripts()
        {
            return self.conscripts;
        }

        public static Faction faction(WArmy a)
        {
            if (self.data.get(a) == 0)
                return null;
            return FACTIONS.getByIndex(self.data.get(a) - 1);
        }

        public static void factionSet(WArmy a, Faction f)
        {
            if (faction(a) == f)
                return;

            removeOnlyTobeCalledFromAnArmy(a);

            addOnlyToBeCalledFromAnArmy(a, f);
        }

        public static void addOnlyToBeCalledFromAnArmy(WArmy a, Faction f)
        {
            self.data.set(a, f == null ? 0 : f.index() + 1);
            ADArmies aa = army(faction(a));
            aa.armies.add(a.armyIndex());
            foreach (ADInit.Countable cc in self.init.countable)
            {
                cc.count(a, 1);
            }
            self.random.set(a, RND.rInt() & int.MaxValue);
        }

        public static void removeOnlyTobeCalledFromAnArmy(WArmy a)
        {
            foreach (ADInit.Countable cc in self.init.countable)
            {
                cc.count(a, -1);
            }
            ADArmies aa = army(faction(a));
            aa.armies.removeShort(a.armyIndex());
        }

        public static ADInt men(Race race)
        {
            return self.soldiers.current(race);
        }

        public static ADInt menTarget(Race race)
        {
            return self.soldiers.target(race);
        }

        public static void updateArmy(WArmy a)
        {
            self.AI.update(a);
        }

        public static void register(ADDiv div, int d)
        {
            if (div.army() == null)
                return;

            foreach (ADInit.Register rr in self.init.registers)
            {
                rr.register(div, d);
            }
        }

        public static ADPower power()
        {
            return self.power;
        }

        public static ADStats stats()
        {
            return self.stats;
        }

        public static ADSupplies supplies()
        {
            return self.supplies;
        }

        public static WDivStoredAll cityDivs()
        {
            return self.divsCity;
        }

        public static WDivRegionalAll regional()
        {
            return self.divsReg;
        }

        public static WDivMercenaries mercenaries()
        {
            return self.divsMerc;
        }

        public static ArrayListGrower<BoosterAbs<WArmy>> moraleFactors()
        {
            return self.moraleFactors;
        }

        public static double morale(WArmy a)
        {
            return BUtil.value(self.moraleFactors, a);
        }

        public static double rnd(WArmy a)
        {
            return self.random.getD(a);
        }
    }
}