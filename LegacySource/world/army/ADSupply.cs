using System;
using System.Collections.Generic;
using game.faction;
using game.time;
using init.resources;
using settlement.stats.equip;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world.army.ADInt;
using world.entity.army;

namespace world.army
{
    public abstract class ADSupply : INDEXED
    {
        public const int STOCKPILE_DAYS = 6;

        public readonly string name;
        public readonly RESOURCE res;

        private readonly INT_OE<WArmy> cached;
        private readonly INT_OE<Faction> cachedF;

        protected readonly ADIntImp current;
        protected readonly ADIntImp consumers;
        protected readonly ADIntImp consumersMax;
        protected readonly ADIntImp amountNeeded;
        protected readonly ADIntImp amountNeededMax;

        public readonly double baseMorale;
        public readonly double baseHealth;
        public readonly double consumptionPerItem;
        public readonly double consumptionPerUser;
        private readonly int index;

        protected ADSupply(int index, string KPrefix, ADInit init, RESOURCE res, string prefix, double consumptionPerUser, double consumptionPerItem, double morale, double health)
        {
            this.consumptionPerItem = consumptionPerItem;
            this.consumptionPerUser = consumptionPerUser;
            this.index = index;
            name = prefix + ": " + res.name;
            current = new ADIntImp(init, $"SUPPLY_{KPrefix}_{res.key}", name, res.names)
            {
                set = (t, i) =>
                {
                    AD.power().mor(t);
                    base.set(t, i);
                },
                min = t => 0
            };
            consumers = new ADIntImp(init, $"SUPPLY_NEEDED_{KPrefix}_{res.key}", name, res.names);
            consumersMax = new ADIntImp(init, $"SUPPLY_CONSUMERS_TARGET_{KPrefix}_{res.key}", name, res.names);
            amountNeeded = new ADIntImp(init, $"SUPPLY_TARGET_{KPrefix}_{res.key}", name, res.names);
            amountNeededMax = new ADIntImp(init, $"SUPPLY_TARGET_MAX_{KPrefix}_{res.key}", name, res.names);

            cached = init.dataA.new DataBit($"SUPPLY_CACHE{KPrefix}_{res.key}");
            cachedF = init.dataT.new DataBit($"SUPPLY_CACHE{KPrefix}_{res.key}");

            this.res = res;
            this.baseMorale = morale;
            this.baseHealth = health;
        }

        public void setChanged(WArmy a)
        {
            cached.set(a, 0);
            if (a.faction() != null)
                cachedF.set(a.faction(), 0);
        }

        public override int index()
        {
            return index;
        }

        public ADIntImp current()
        {
            return current;
        }

        public double consumedPerDayCurrent(WArmy a)
        {
            cache(a);
            return consumers.get(a) * consumptionPerUser + amountNeeded.get(a) * consumptionPerItem;
        }

        public double consumedPerDayCurrent(Faction f)
        {
            cache(f);
            return consumers.get(f) * consumptionPerUser + amountNeeded.get(f) * consumptionPerItem;
        }

        public double consumedPerDayMax(WArmy a)
        {
            cache(a);
            return consumersMax.get(a) * consumptionPerUser + amountNeededMax.get(a) * consumptionPerItem;
        }

        public double consumedPerDayMax(Faction f)
        {
            cache(f);
            return consumersMax.get(f) * consumptionPerUser + amountNeededMax.get(f) * consumptionPerItem;
        }

        public int minimumAmount(WArmy a)
        {
            cache(a);
            return amountNeeded.get(a);
        }

        public int minimumAmount(Faction f)
        {
            cache(f);
            return amountNeeded.get(f);
        }

        public int targetAmount(WArmy a)
        {
            cache(a);
            return amountNeededMax.get(a);
        }

        public int targetAmount(Faction f)
        {
            cache(f);
            return amountNeededMax.get(f);
        }

        public double daysStored(WArmy a)
        {
            cache(a);
            return (double)current().get(a) / consumedPerDayCurrent(a);
        }

        public double daysStored(Faction f)
        {
            cache(f);
            return (double)current().get(f) / consumedPerDayCurrent(f);
        }

        public double moraleAdd(WArmy a)
        {
            cache(a);
            return baseMorale * daysStored(a);
        }

        public double healthMul(WArmy a)
        {
            cache(a);
            return baseHealth * daysStored(a);
        }

        public abstract void transfer(WDIV div, WArmy old, WArmy current);

        public abstract void hover(GBox b, WArmy a);

        protected void cache(WArmy a)
        {
            if (!cached.get(a))
            {
                add(a);
                cached.set(a, 1);
            }
        }

        protected void cache(Faction f)
        {
            if (!cachedF.get(f))
            {
                foreach (WArmy a in f.armies())
                {
                    add(a);
                    cached.set(a, 1);
                }
                cachedF.set(f, 1);
            }
        }

        protected abstract void add(WArmy a);

        public static readonly string ¤¤affected = "Affected";
        public static readonly string ¤¤Minimum = "Minimum";
        public static readonly string ¤¤Max = "Max";
        public static readonly string ¤¤Stored = "Stored";
        public static readonly string ¤¤ConsumtionRate = "Consumption Rate";
        public static readonly string ¤¤days = "Days";
        public static readonly string ¤¤Morale = "Morale";
        public static readonly string ¤¤Health = "Health";
        public static readonly string ¤¤artD = "Artillery Description";

        public static readonly class ADSupplyEquip
        {
            public readonly EquipBattle equip;

            public ADSupplyEquip(int index, ADInit init, EquipBattle rs) : base(index, "EQUIPMENT", init, rs.resource, Dic.¤¤Equipment, 0, rs.wearRate() / 16.0, 0, 0)
            {
                this.equip = rs;
            }

            protected override void add(WArmy a)
            {
                for (int di = 0; di < a.divs().Count; di++)
                {
                    ADDiv div = a.divs()[di];
                    if (div.needSupplies())
                    {
                        amountNeededMax.inc(div.army(), div.menTarget() * div.target().equipI(equip));
                        amountNeeded.inc(div.army(), div.men() * div.target().equipI(equip));
                    }
                }
            }

            public override void transfer(WDIV div, WArmy old, WArmy current)
            {
                double divAmount = div.menTarget() * div.target().equipI(equip);
                double armyAmount = amountNeededMax.get(old);
                int am = (int)(current().get(old) * (divAmount / armyAmount));
                if (am > 0)
                {
                    current().inc(old, -am);
                    current().inc(current, am);
                }
            }

            public override void hover(GBox b, WArmy a)
            {
                b.title(name);
                b.text(equip.resource.desc);

                b.sep();

                b.textL(Dic.¤¤Minimum);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), minimumAmount(a)));
                b.NL();

                b.textL(Dic.¤¤Max);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), targetAmount(a)));
                b.NL();

                b.textL(Dic.¤¤Stored);
                b.tab(6);
                b.add(GFORMAT.iofkInv(b.text(), current().get(a), targetAmount(a)));
                b.NL();

                b.textL(¤¤ConsumtionRate);
                b.tab(6);
                b.add(GFORMAT.f0(b.text(), -consumedPerDayCurrent(a)));
                b.NL();

                b.textL(¤¤days);
                b.tab(6);
                b.add(GFORMAT.f0(b.text(), daysStored(a)));
                b.NL();
            }
        }

        public static readonly class ADSupplyArt
        {
            public readonly ADArtillery art;
            public readonly int ri;

            public ADSupplyArt(int index, ADInit init, ADArtillery art, RESOURCE res, int ri) : base(index, $"ART_{art.art.key}_{res.key}", init, res, art.art.info.name, 0, 0.2 / TIME.years().bitConversion(TIME.days()), 0, 0)
            {
                this.art = art;
                this.ri = ri;
            }

            protected override void add(WArmy a)
            {
                int am = (int)Math.Ceiling(art.target.get(a) * art.art.constructor().item(1).cost2(ri, art.art.upgrades().max()));
                amountNeeded.set(a, am);
                amountNeededMax.set(a, am);
            }

            public override void transfer(WDIV div, WArmy old, WArmy current)
            {
            }

            public override void hover(GBox b, WArmy a)
            {
                b.title(name);
                b.text(¤¤artD);

                b.sep();

                b.textL(Dic.¤¤Minimum);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), minimumAmount(a)));
                b.NL();

                b.textL(Dic.¤¤Max);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), targetAmount(a)));
                b.NL();

                b.textL(Dic.¤¤Stored);
                b.tab(6);
                b.add(GFORMAT.iofkInv(b.text(), current().get(a), targetAmount(a)));
                b.NL();

                b.textL(¤¤ConsumtionRate);
                b.tab(6);
                b.add(GFORMAT.f0(b.text(), -consumedPerDayCurrent(a)));
                b.NL();

                b.textL(¤¤days);
                b.tab(6);
                b.add(GFORMAT.f0(b.text(), daysStored(a)));
                b.NL();
            }
        }
    }
}