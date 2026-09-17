using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Stats.Colls
{
    using static Settlement.Main.SETT.ROOMS;

    using Game;
    using Init.Race;
    using Init.Race;
    using Init.Resources;
    using Init.Resources;
    using Init.Resources;
    using Init.Type;
    using Init.Type;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Main.SETT;
    using Settlement.Room.Service.Food.Canteen;
    using Settlement.Room.Service.Food.Eatery;
    using Settlement.Stats;
    using Settlement.Stats;
    using Settlement.Stats;
    using Settlement.Stats.Stat;
    using Settlement.Stats.Stat;
    using Settlement.Stats.Stat;
    using Settlement.Stats.Stat;
    using Settlement.Stats.Stat;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Util.Info;
    using Util.Race;
    using Util.Text;

    public class StatsFood : StatCollection
    {
        public static readonly int MAX_RATIONS = 4;

        public readonly STAT FOOD_PREFFERENCE;
        public readonly STAT FOOD_DAYS;
        public readonly STAT FOOD;
        public readonly STAT DRINK;
        public readonly STAT DRINK_PREFFERENCE;
        public readonly STAT STARVATION;

        private readonly All[] foodAllowed = new All[HCLASSES.ALL().Size()];
        private readonly LIST<PERMISSION> food;

        private static readonly CharSequence ¤¤name = "Food";
        private static readonly CharSequence ¤¤desc = "Stats related to food and hunger.";

        static StatsFood()
        {
            D.ts(typeof(StatsFood));
        }

        public StatsFood(StatsInit init) : base(init, "FOOD", ¤¤name, ¤¤desc)
        {
            D.gInit(this);

            STARVATION = new STATData("STARVATION", init, init.count.new DataBit("FOOD_STARVE"));
            FOOD_PREFFERENCE = new STATData("FOOD_PREFFERENCE", init, init.count.new DataNibble("FOOD_PREF"));
            init.onArrivalStats.add(FOOD_PREFFERENCE);
            for (int i = 0; i < foodAllowed.Length; i++)
            {
                foodAllowed[i] = new All();
            }

            FOOD_DAYS = new STATFakeRace("FOOD_DAYS", init)
            {
                private double am;
                private int lastT = -1;

                public override int dataDivider()
                {
                    return 24;
                }

                protected override double getDD(Race race)
                {
                    if (GAME.updateI() == lastT)
                        return am;

                    lastT = GAME.updateI();

                    double a = 0;
                    for (int ei = 0; ei < RESOURCES.EDI().all().Size(); ei++)
                    {
                        ResG r = RESOURCES.EDI().all().get(ei);
                        double aa = ROOMS().STOCKPILE.tally().amountTotal(r.resource);
                        a += aa;
                    }

                    for (int ri = 0; ri < SETT.ROOMS().EATERIES.size(); ri++)
                    {
                        ROOM_EATERY e = SETT.ROOMS().EATERIES.get(ri);
                        double aa = e.totalFood();
                        a += aa;
                    }

                    for (int ri = 0; ri < SETT.ROOMS().CANTEENS.size(); ri++)
                    {
                        ROOM_CANTEEN e = SETT.ROOMS().CANTEENS.get(ri);
                        double aa = e.totalFood();
                        a += aa;
                    }

                    double needed = 0;

                    for (int ci = 0; ci < HCLASSES.ALL().Size(); ci++)
                    {
                        HCLASS c = HCLASSES.ALL().get(ci);
                        if (c.player)
                        {
                            for (int ri = 0; ri < RACES.all().Size(); ri++)
                            {
                                Race r = RACES.all().get(ri);
                                needed += NEEDS.TYPES().HUNGER.rate.get(c.get(r)) * POP.physical(c, r) * FOOD.decree().get(c, r);
                            }
                        }
                    }

                    if (needed == 0)
                        am = a > 0 ? 1 : 0;
                    else
                        am = (a / needed);
                    am /= dataDivider();
                    return am;
                }
            };
            FOOD_DAYS.info().setInt();
            FOOD_DAYS.info().setMatters(true, false);

            StatDecree d = new StatDecree("FOOD_RATIONS_DECREE", init, 2, MAX_RATIONS, 2, D.g("RationsT", "Target Food servings."), 1);
            d.setInt();

            FOOD = new STATData("FOOD_RATIONS", init, init.count.new DataNibble("FOOD_RATIONS", MAX_RATIONS - 1));
            FOOD.addDecree(d);
            init.onArrivalStats.add(FOOD);

            d = new StatDecree("DRINK_RATION_DECREE", init, 2, MAX_RATIONS, 2, D.g("DrinkT", "Target Drink servings"), 1);
            d.setInt();
            DRINK = new STATData("DRINK_RATIONS", init, init.count.new DataNibble("DRINK_RATIONS", 5));
            DRINK.addDecree(d);
            DRINK_PREFFERENCE = new STATData("DRINK_PREFFERENCE", init, init.count.new DataNibble("DRINK_PREF"));
            init.onArrivalStats.add(DRINK_PREFFERENCE);

            foreach (All bb in foodAllowed)
                bb.clear();

            init.savers.put("FOOD_ALLOWED", new SAVABLE()
            {
                public void save(FilePutter file)
                {
                    HCLASSES.MAP().saver().save(foodAllowed, file);
                }

                public void load(FileGetter file)
                {
                    HCLASSES.MAP().loader().load(foodAllowed, file);
                }

                public void clear()
                {
                    foreach (All bb in foodAllowed)
                        bb.clear();
                }
            });

            LIST<RESOURCE> perm = RESOURCES.EDI().res().join(RESOURCES.DRINKS().res());
            ArrayList<PERMISSION> foodList = new ArrayList<>(perm.Size());
            foreach (RESOURCE res in perm)
            {
                foodList.add(new PERMISSION()
                {
                    public void set(HCLASS cl, Race race, bool value)
                    {
                        if (race == null)
                        {
                            for (int ri = 0; ri < RACES.all().Size(); ri++)
                            {
                                set(cl, RACES.all().get(ri), value);
                            }
                            return;
                        }
                        if (value)
                            foodAllowed[cl.index()].foodAllowed[race.index].or(res);
                        else
                            foodAllowed[cl.index()].foodAllowed[race.index].clear(res);
                    }

                    public INFO info()
                    {
                        return res;
                    }

                    public bool get(HCLASS cl, Race race)
                    {
                        if (race == null)
                        {
                            for (int ri = 0; ri < RACES.all().Size(); ri++)
                            {
                                if (get(cl, RACES.all().get(ri)))
                                    return true;
                            }
                            return false;
                        }
                        return (foodAllowed[cl.index()].foodAllowed[race.index].has(res));
                    }
                });
            }
            food = foodList;
        }

        public PERMISSION foodAllowed(ResG e)
        {
            return food.get(e.index());
        }

        public PERMISSION drinkAllowed(ResG e)
        {
            return food.get(e.index());
        }

        public PERMISSION allowed(int index)
        {
            return food.get(index);
        }

        public RBIT fetchMask(Humanoid h)
        {
            return foodAllowed[h.indu().hType().parentClass().index()].foodAllowed[h.race().index];
        }

        public void eat(Humanoid a, int level, double preference)
        {
            Induvidual i = a.indu();
            NEEDS.TYPES().HUNGER.stat().fix(a.indu());
            FOOD.indu().set(i, Math.Max(level - 1, 0));
            FOOD_PREFFERENCE.indu().setD(i, preference);
        }

        public void drink(Humanoid a, int level, double preference)
        {
            Induvidual i = a.indu();

            DRINK.indu().set(i, level);
            DRINK_PREFFERENCE.indu().setD(i, preference);
        }

        private class All : SAVABLE
        {
            public readonly RBITImp[] foodAllowed = new RBITImp[RACES.all().Size()];

            public All()
            {
                for (int i = 0; i < foodAllowed.Length; i++)
                    foodAllowed[i] = new RBITImp();
            }

            public void save(FilePutter file)
            {
                RACES.map().saver().save(foodAllowed, file);
            }

            public void load(FileGetter file)
            {
                RACES.map().loader().load(foodAllowed, file);
            }

            public void clear()
            {
                foreach (RBITImp b in foodAllowed)
                    b.setAll();
            }
        }
    }
}