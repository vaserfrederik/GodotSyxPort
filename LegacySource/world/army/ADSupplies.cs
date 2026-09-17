using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.time;
using init.resources;
using init.sprite.UI;
using settlement.main;
using settlement.room.military.artillery;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.text;
using view.ui.message;
using world.army.ADInit;
using world.army.ADInt;
using world.army.ADSupply;
using world.entity.army;

namespace world.army
{
    public static class ADSupplies
    {
        public const int ArtilleryMax = 40;

        private readonly ArrayList<ArrayListGrower<ADSupply>> map = new ArrayList<ArrayListGrower<ADSupply>>(RESOURCES.ALL().Size());
        private readonly ArrayListGrower<RESOURCE> resources = new ArrayListGrower<RESOURCE>();
        {
            while (map.HasRoom())
                map.Add(new ArrayListGrower<ADSupply>());

        }
        public readonly ADIntImp creditsNeeded;
        public readonly ADIntImp creditsTarget;
        public readonly LIST<ADSupply> all;
        public readonly LIST<ADSupply> healths;
        public readonly LIST<ADSupply> morales;
        public readonly LIST<ADSupply.ADSupplyRes> food;
        public readonly LIST<ADSupply> equip;

        private readonly Bitmap1D has = new Bitmap1D(RESOURCES.ALL().Size(), false);
        private readonly ArrayListGrower<ADArtillery> arts = new ArrayListGrower<ADArtillery>();
        private readonly INT_OE<WArmy> artilleryTot;

        private static readonly CharSequence ¤¤Starving = "¤Supplies low!";
        private static readonly CharSequence ¤¤StarvingD = "¤Essential supplies have not been delivered to our army, affecting health. Low health will stop training of new recruits and cause deaths and desertion. Fill up our military depots and fortify the army immediately. Affected army: {0}.";

        static ADSupplies()
        {
            D.ts(typeof(ADSupplies));
        }

        public ADSupplies(ADInit init)
        {
            creditsNeeded = new ADIntImp(init, "CREDITS_CURRENT", Dic.¤¤Currs, "");
            creditsTarget = new ADIntImp(init, "CREDITS_TARGET", Dic.¤¤Currs, "");
            ArrayListGrower<ADSupply> all = new ArrayListGrower<ADSupply>();

            ArrayListGrower<ADSupplyRes> misc = new ArrayListGrower<ADSupplyRes>();
            foreach (ResSupply rs in RESOURCES.SUP().ALL)
            {
                ADSupplyRes s = new ADSupply.ADSupplyRes(all.Size(), init, rs);
                all.Add(s);
                misc.Add(s);
            }
            this.food = misc;

            ArrayListGrower<ADSupply> equip = new ArrayListGrower<ADSupply>();
            foreach (EquipBattle a in STATS.EQUIP().BATTLE_ALL())
            {
                ADSupply s = new ADSupply.ADSupplyEquip(all.Size(), init, a);
                all.Add(s);
                equip.Add(s);
            }
            this.equip = equip;

            init.registers.Add(new Register
            {
                public void Register(ADDiv div, int d)
                {
                    if (div.army() != null)
                    {
                        for (int si = 0; si < all.Size(); si++)
                        {
                            all.Get(si).SetChanged(div.army());
                        }
                    }
                    AD.Supplies().creditsNeeded.Inc(div.army(), d * div.costPerMan() * div.men());
                    AD.Supplies().creditsTarget.Inc(div.army(), d * div.costPerMan() * div.menTarget());
                }
            });

            artilleryTot = init.dataA.NewDataInt("ARTILLARY_TARGET_TOT");

            foreach (ROOM_ARTILLERY a in SETT.ROOMS().ARTILLERY)
                arts.Add(new ADArtillery(init, a, all));

            this.all = all;
            {
                ArrayListGrower<ADSupply> mm = new ArrayListGrower<ADSupply>();
                ArrayListGrower<ADSupply> he = new ArrayListGrower<ADSupply>();
                foreach (ADSupply s in all)
                {
                    if (s.baseMorale > 0)
                    {
                        mm.Add(s);
                    }
                    if (s.baseHealth > 0)
                        he.Add(s);
                }
                this.healths = he;
                this.morales = mm;
            }

            AD.MoraleFactors().Add(new BoosterAbs<WArmy>(new BSourceInfo(Dic.¤¤Supplies, UI.Icons().S.storage), false)
            {
                public double To()
                {
                    return 1;
                }

                protected double Pget(WArmy o)
                {
                    return Morale(o);
                }

                public double From()
                {
                    return 0;
                }

                public double GetValue(double input)
                {
                    return input;
                }
            });

            init.updaters.Add(new Updater
            {
                public void Update(Faction f, double timeSinceLast)
                {
                    // TODO Auto-generated method stub

                }

                public void Update(WArmy a, double timeSinceLast)
                {
                    if (a.faction() == FACTIONS.Player())
                    {
                        double he = Health(a);
                        foreach (ADSupply s in all)
                        {
                            double am = s.consumedPerDayCurrent(a) * timeSinceLast * TIME.secondsPerDayI();
                            int tot = (int)am;
                            if (am - tot > RND.rFloat())
                                tot++;
                            s.current().Inc(a, -tot);
                        }

                        if (he >= 1 && AD.Supplies().Health(a) < 1)
                        {
                            Str.TMP.Clear();
                            Str.TMP.Add(¤¤StarvingD).Insert(0, a.name);
                            new MessageText(¤¤Starving, Str.TMP).Send();
                        }
                    }
                    else
                    {
                        foreach (ADSupply s in AD.Supplies().all)
                        {
                            double am = Math.Ceiling(s.targetAmount(a) / 16.0);
                            double tar = s.minimumAmount(a);
                            am += s.current().Get(a);
                            if (am > tar)
                                am = tar;
                            s.current().Set(a, (int)am);

                        }
                    }

                }
            });

            Bitmap1D res = new Bitmap1D(RESOURCES.ALL().Size(), false);
            foreach (ADSupply a in this.all)
            {
                has.Set(a.res.Index(), true);
                map.Get(a.res.Index()).Add(a);
                if (!res.Get(a.res.Index()))
                {
                    resources.Add(a.res);
                    res.Set(a.res.Index(), true);
                }
            }

        }

        public LIST<ADArtillery> Arts()
        {
            return arts;
        }

        public LIST<RESOURCE> Resses()
        {
            return resources;
        }

        public LIST<ADSupply> Get(RESOURCE res)
        {
            return map.Get(res.Index());
        }

        public ADSupply Get(ResSupply res)
        {
            return all.Get(res.Index());
        }

        public ADSupply Get(EquipBattle a)
        {
            return all.Get(RESOURCES.SUP().ALL.Size() + a.indexMilitary());
        }

        public void FillAll(WArmy a)
        {
            foreach (ADSupply s in all)
                s.current().Set(a, s.targetAmount(a));
        }

        public ADInt Credits()
        {
            return creditsNeeded;
        }

        public double Morale(WArmy a)
        {
            double m = 0;
            foreach (ADSupply s in morales)
            {
                m += s.moraleAdd(a);
            }
            return m;
        }

        public double Health(WArmy a)
        {
            double m = 1;
            foreach (ADSupply s in healths)
            {
                m *= s.healthMul(a);
            }
            if (a.state() != WArmyState.Fortified)
                m *= 0.5;
            return m;
        }

        public void Transfer(WArmy from, WArmy to, double amount)
        {
            foreach (ADSupply s in all)
            {
                s.current().Transfer(from, to, amount);
            }
        }

        public void Set(WArmy army, double amount)
        {
            foreach (ADSupply s in all)
            {
                s.current().Set(army, amount);
            }
        }

        public void Inc(WArmy army, double amount)
        {
            foreach (ADSupply s in all)
            {
                s.current().Inc(army, amount);
            }
        }

        public void Dec(WArmy army, double amount)
        {
            foreach (ADSupply s in all)
            {
                s.current().Dec(army, amount);
            }
        }

        public class ADArtillery : INDEX
        {
            public readonly ROOM_ARTILLERY art;
            public readonly DATA<byte> target;
            private readonly ArrayListGrower<ADSupply> supplies;

            public ADArtillery(ADInit init, ROOM_ARTILLERY art, LISTE<ADSupply> sups)
            {
                this.art = art;
                target = init.dataA.NewDataByte("ART_TARGET_" + art.Key)
                {
                    public void Set(WArmy t, int s)
                    {
                        artilleryTot.Inc(t, -Get(t));
                        base.Set(t, CLAMP.i(s, 0, max(t)));
                        artilleryTot.Inc(t, Get(t));
                        foreach (ADSupply ss in supplies)
                        {
                            ss.SetChanged(t);
                        }
                    }

                    public int max(WArmy t)
                    {
                        return Get(t) + ArtilleryMax - artilleryTot.Get(t);
                    }

                };

                for (int i = 0; i < art.Constructor().resources(); i++)
                {
                    RESOURCE res = art.Constructor().resource(i);
                    ADSupply.ADSupplyArt sup = new ADSupplyArt(sups.Size(), init, this, res, i);
                    supplies.Add(sup);
                    sups.Add(sup);
                }

            }

            public int Current(WArmy a)
            {
                double d = 1.0;
                foreach (ADSupply ss in supplies)
                {
                    d = Math.Min(d, ss.amountValue(a));
                }
                if (a.state() != WArmyState.Fortified)
                    d *= 0.5;
                return (int)(target.Get(a) * d);
            }

            public override int Index()
            {
                return art.typeIndex();
            }

            public LIST<ADSupply> Sups()
            {
                return supplies;
            }

        }

    }
}