using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Food.Pasture
{
    public sealed class RoomPasture : RoomBlueprintIns<PastureInstance>, INDUSTRY_HASER, ROOM_EMPLOY_AUTO, ROOM_IRRIGATED
    {
        public static readonly string Type = "PASTURE";
        private readonly Constructor constructor;
        private readonly int jobsPerDay = TIME.GetWorkPerDay(JobManager.WorkTime);
        private readonly double capacityPerDay = 1.0 / 2.0;
        public readonly AnimalSpecies Species;
        private readonly List<Industry> indus;

        private readonly double ANIMALS_PER_TILE;
        private const double WORKERS_PER_TILE = 1.0 / 64;
        private readonly RoomIrrigated irri;
        public readonly bool IsIndoors;

        private readonly RoomResStorage s1 = new RoomResStorage(3000)
        {
            Resource = () =>
            {
                if (Ins is PastureInstance p)
                {
                    if (p.Industry().Outs().Count > 0)
                    {
                        return p.Industry().Outs()[0].Resource;
                    }
                    return p.Industry().Outs()[0].Resource;
                }
                return indus[0].Outs()[0].Resource;
            },
            Is = (tx, ty) => SETT.ROOMS().fData.TileData.Get(tx, ty) == Constructor.STORAGE1
        };

        private readonly RoomResStorage s2 = new RoomResStorage(3000)
        {
            Resource = () =>
            {
                if (Ins is PastureInstance p)
                {
                    if (p.Industry().Outs().Count > 1)
                    {
                        return p.Industry().Outs()[1].Resource;
                    }
                    return p.Industry().Outs()[0].Resource;
                }
                return indus[0].Outs()[0].Resource;
            },
            Is = (tx, ty) => SETT.ROOMS().fData.TileData.Get(tx, ty) == Constructor.STORAGE2
        };

        private readonly RoomResStorage s3 = new RoomResStorage(3000)
        {
            Resource = () =>
            {
                if (Ins is PastureInstance p)
                {
                    if (p.Industry().Outs().Count > 2)
                    {
                        return p.Industry().Outs()[2].Resource;
                    }
                    return p.Industry().Outs()[0].Resource;
                }
                return indus[0].Outs()[0].Resource;
            },
            Is = (tx, ty) => SETT.ROOMS().fData.TileData.Get(tx, ty) == Constructor.STORAGE3
        };

        private readonly RoomResStorage[] st = new RoomResStorage[]
        {
            s1, s2, s3
        };

        public RoomPasture(RoomInitData data, string key, RoomCategorySub cat, int index) : base(index, data, key, cat)
        {
            Species = SETT.ANIMALS().Map.Read(data.Data());

            ANIMALS_PER_TILE = Math.Clamp(2.5 / (Species.Mass() + 10), 0, 1.0 / 9);

            IsIndoors = data.Data().Bool("INDOORS", false);

            this.constructor = IsIndoors ? new ConstructorIndoor(this, data) : new ConstructorOutdoor(this, data);
            PushBo(data.Data(), Type, true);
            var bbs = new List<RoomBoost>();
            bbs.Add(constructor.Efficiency);
            bbs.Add(new RoomBoost
            {
                Info = new INFO(Dic.¤¤Capacity, Dic.¤¤Capacity),
                Get = r => constructor.Ferarea.Get(r) * ROOM_PASTURE.WORKERS_PER_TILE
            });
            bbs.Add(new RoomBoost
            {
                Info = new INFO(Gui.¤¤Skill, Gui.¤¤SkillD),
                Get = r => ((PastureInstance)r).Skill()
            });
            bbs.Add(new RoomBoost
            {
                Info = new INFO(Gui.¤¤Animals, Gui.¤¤Animals),
                Get = r => (double)((PastureInstance)r).AnimalsCurrent / ((PastureInstance)r).AnimalsMax
            });
            bbs.Add(new RoomBoost
            {
                Info = new INFO(Gui.¤¤Adults, Gui.¤¤Adults),
                Get = r =>
                {
                    var p = (PastureInstance)r;
                    if (p.AnimalsCurrent <= 0) return 0;
                    double an = p.AnimalsCurrent - p.AnimalsCubs;
                    an = Math.Clamp(an, 0, p.AnimalsCurrent);
                    an /= p.AnimalsCurrent;
                    return 0.1 + 0.9 * an;
                }
            });
            bbs.Add(new RoomBoost
            {
                Info = new INFO(Gui.¤¤Tending, Gui.¤¤Tending),
                Get = r =>
                {
                    var p = (PastureInstance)r;
                    return (double)(8 * (p.Auto ? 1 : 0.25));
                }
            });

            indus = Industry.CreateIndustries(this, data, bbs, Bonus(), new Func<Region, double>(reg => RegionInfo.vArea.GetAi(reg)));

            foreach (var i in indus)
            {
                if (i.Outs().Count > 3)
                    data.Data().Error("Can't declare more than 3 output in industry!", "");
                if (i.Ins().Count > 0)
                    data.Data().Error("Can't declare inputs to a pasture industry", "");

                new IndustryRegion(i, 1.0)
                {
                    Occurence = reg => RegionInfo.vArea.GetAi(reg)
                };
            }

            new RoomExperienceBonus(this, data.Data(), Bonus());

            irri = new RoomIrrigated(this, Bonus, 0.5, 1.075)
            {
                Needed = area => area.Area(),
                Irrigation = ins => ((PastureInstance)ins).Water
            };
        }

        public double SlaughterAmount(bool cub, Industry ins)
        {
            return 8 * (cub ? 0.25 : 1);
        }

        protected override void Update(double ds)
        {
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FileStream saveFile)
        {
            IndustryUtil.Save(saveFile, indus);
        }

        protected override void LoadP(FileStream saveFile)
        {
            IndustryUtil.Load(saveFile, indus);
        }

        protected override void ClearP()
        {
            IndustryUtil.Clear(indus);
        }

        public override bool Degrades()
        {
            return false;
        }

        public static bool IsGate(int tx, int ty)
        {
            return SETT.ROOMS().Map.Blueprint.Get(tx, ty) is RoomPasture && SETT.ROOMS().fData.Tile.Is(tx, ty);
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public override List<Industry> Industries()
        {
            return indus;
        }

        public override bool IsAvailable(CLIMATE c)
        {
            foreach (var s in c.Boosters.All())
            {
                if (s.Boostable == Bonus() && s.Booster.IsMul && s.Booster.Min() == 0)
                    return false;
            }
            return true;
        }

        public override bool AutoEmploy(Room r)
        {
            return ((PastureInstance)r).Auto;
        }

        public override void AutoEmploy(Room r, bool b)
        {
            ((PastureInstance)r).Auto = b;
        }

        public override double DegradeRate()
        {
            return 0;
        }

        public override double IndustryFormatProductionRate(GText text, IndustryResource i, RoomInstance ins)
        {
            double prod = i.Rate;

            foreach (var bb in indus[0].Boosts())
            {
                prod *= bb.Get(ins);
            }
            text.Add('+');
            GFORMAT.f(text, prod);
            return prod;
        }

        public override void IndustryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins)
        {
            Gui.IndustryHoverProductionRate(b, i, ins);
        }

        public override double IndustryFormatProductionRateEmpl(GText text, IndustryResource i, RoomInstance ins)
        {
            text.Clear();
            double prod = i.Rate;

            foreach (var bb in indus[0].Boosts())
            {
                prod *= bb.Get(ins);
            }
            return prod;
        }

        public override RoomIrrigated Irrigation()
        {
            return irri;
        }
    }
}