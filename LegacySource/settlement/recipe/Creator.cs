using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Recipe
{
    using Game.Battle.Div;
    using Game.Boosting;
    using Game.Boosting.BSourceInfo;
    using Game.Boosting.Boostable;
    using Game.Boosting.BoostableCat;
    using Game.Boosting.BoosterImp;
    using Game.Faction;
    using Game.Faction.NPC;
    using Game.Faction.Player;
    using Game.Time;
    using Init.Race;
    using Init.Race.Race;
    using Init.Resources;
    using Init.Sprite.UI;
    using Init.Trade;
    using Init.Type;
    using Settlement.Main;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Stats;
    using Snake2D;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite;
    using Util.Text;
    using World.Map.Regions;
    using World.Region;

    class Creator
    {
        static Creator()
        {
            D.Ts(typeof(Recipes));
        }

        public static LIST<Recipe> All(BoostableCat boostsSlave, string nname, string ndesc, ArrayListGrower<FBoost> fboosts)
        {
            ArrayListGrower<Recipe> all = new ArrayListGrower<Recipe>();

            for (int bi = 0; bi < SETT.Rooms.All().Size(); bi++)
            {
                RoomBlueprint b = SETT.Rooms.All()[bi];

                KeyMap<Boolean> hasBoosted = new KeyMap<Boolean>();

                if (b is INDUSTRY_HASER)
                {
                    INDUSTRY_HASER hh = (INDUSTRY_HASER)b;
                    for (int ii = 0; ii < hh.Industries().Size(); ii++)
                    {
                        Industry i = hh.Industries()[ii];
                        if (i.IsOnlyRoomDoNotUse)
                            continue;

                        for (int oi = 0; oi < i.Outs().Size(); oi++)
                        {
                            IndustryResource r = i.Outs()[oi];
                            int index = all.Size();
                            TRADABLE outResource = r.Resource.Tr();
                            double rate = r.Rate;
                            double rateAI = r.AIRate;
                            double aiRecovery = r.AIRecovery;
                            Boostable bo = i.Bonus();
                            string name = " " + i.Blue.Info.Name;
                            SPRITE icon = i.Blue.Icon;
                            RESOURCE inn = UniqueResource(i, hh);
                            if (inn != null)
                            {
                                name += "(" + inn.Name + ")";
                                icon = icon.Twin(inn.Icon().Scaled(0.5), DIR.SE, 2);
                            }

                            ArrayListGrower<RecipeInput> inss = new ArrayListGrower<RecipeInput>();

                            foreach (IndustryResource @in in i.Ins())
                            {
                                inss.Add(new RecipeInput(@in.Resource.Tr(), @in.Rate, bo));
                            }

                            Recipe fi = new Recipe(index, ii, outResource, rate, rateAI, aiRecovery, bo, i.Consumption(), name, icon, inss);

                            all.Add(fi);

                            if (!hasBoosted.ContainsKey(i.Bonus().Key))
                            {
                                if (i.Reg() != null)
                                    new RegBoost(i.Bonus(), i.Reg());
                                fboosts.Add(new FBoost(i.Bonus()));
                                hasBoosted.Put(i.Bonus().Key, true);
                            }
                        }
                    }
                }
            }

            foreach (Race r in RACES.All())
            {
                double w = r.Physics.Slaveprice;

                int index = all.Size();
                TRADABLE outResource = TR.Get(r);

                double rate = 1.0 / w;
                double rateAI = 1.0 / w;

                if (r.Physics.SlavePRriceRecovery <= 0)
                {
                    rate = 0;
                    rateAI = 0;
                }
                double aiRecovery = r.Physics.SlavePRriceRecovery;
                SPRITE icon = new SPRITE.Imp(Icon.M)
                {
                    public override void Render(SPRITE_RENDERER re, int X1, int X2, int Y1, int Y2)
                    {
                        r.Appearance().Icon.Render(re, X1, X2, Y1, Y2);
                    }
                };
                Boostable bo = BOOSTING.Push(r.Key, 1, nname + ": " + r.Info.Names, ndesc, icon, boostsSlave);
                string name = "" + r.Info.Names;

                ArrayListGrower<RecipeInput> inss = new ArrayListGrower<RecipeInput>();
                Recipe fi = new Recipe(index, 0, outResource, rate, rateAI, aiRecovery, bo, null, name, icon, inss);

                all.Add(fi);
                new FBoost(bo);
            }

            return all;
        }

        private static RESOURCE UniqueResource(Industry ins, INDUSTRY_HASER hs)
        {
            if (hs.Industries().Size() == 1)
                return null;

            if (ins.Outs().Size() > 0)
            {
                RESOURCE res = ins.Outs()[0].Resource;
                bool unique = true;
                for (int ii = 0; ii < hs.Industries().Size(); ii++)
                {
                    Industry io = hs.Industries()[ii];
                    if (io == ins)
                        continue;
                    if (io.Outs()[0].Resource == res)
                        unique = false;
                }
                if (unique)
                    return res;
            }

            foreach (Industry io in hs.Industries())
            {
                if (io == ins)
                    continue;
                for (int i1 = 0; i1 < ins.Ins().Size(); i1++)
                {
                    IndustryResource @in = ins.Ins()[i1];
                    bool contains = false;
                    for (int i2 = 0; i2 < io.Ins().Size(); i2++)
                    {
                        IndustryResource ino = io.Ins()[i2];
                        if (ino.Resource == @in.Resource)
                            contains = true;
                    }
                    if (!contains)
                        return @in.Resource;
                }
            }

            return null;
        }

        private class RegBoost : BoosterImp
        {
            bool changeAll;
            int[] lastSecond = Alloc.Ii(FACTIONS.MAX());
            double[] cache = new double[FACTIONS.MAX()];
            private readonly IndustryRegion ireg;

            public RegBoost(Boostable bo, IndustryRegion reg) : base(new BSourceInfo(Recipes.¤¤realm, UI.Icons().S.World), 0, 2, true)
            {
                this.ireg = reg;
                Add(bo);
                Array.Fill(lastSecond, int.MinValue);
                new RD.RDOwnerChanger()
                {
                    public override void Change(Region reg, Faction oldOwner, Faction newOwner)
                    {
                        changeAll = true;
                    }
                };
            }

            public override double VGet(Region reg)
            {
                return 1.0;
            }

            public override double VGet(Induvidual indu)
            {
                return 1.0;
            }

            public override double VGet(Div div)
            {
                return 1.0;
            }

            public override double VGet(HCLASS_RACE t)
            {
                return 1.0;
            }

            public override double VGet(Player f)
            {
                return 1.0;
            }

            public override double VGet(FactionNPC f)
            {
                if (changeAll)
                {
                    Array.Fill(lastSecond, int.MinValue);
                    changeAll = false;
                }

                if (TIME.CurrentSecond() - lastSecond[f.Index()] > TIME.SecondsPerDay())
                {
                    lastSecond[f.Index()] = TIME.SecondsPerDay();
                    double b = 0;
                    for (int i = 0; i < f.Realm().Regions(); i++)
                    {
                        Region reg = f.Realm().Region(i);
                        b += RD.PROSPECT().GetAi(ireg, reg);
                    }
                    b /= f.Realm().Regions();

                    cache[f.Index()] = b;
                }

                return cache[f.Index()];
            }

            public override double GetValue(double input)
            {
                return input;
            }
        }
    }
}