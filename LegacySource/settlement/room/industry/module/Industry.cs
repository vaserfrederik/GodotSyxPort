using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Snake2D;
using Util.Data;
using Util.Info;
using Util.Text;
using World.Map.Regions;

namespace Settlement.Room.Industry.Module
{
    public class Industry : RoomConsumptionAbs, ISavable, IndustryRate, IIndexed
    {
        private static List<Industry> all = new List<Industry>();

        static Industry()
        {
            GameDisposable.Add(() =>
            {
                all.Clear();
            });
        }

        public static IEnumerable<Industry> All()
        {
            return all;
        }

        private readonly List<IndustryResource> outs = new List<IndustryResource>();

        protected IndustryResourceOut[] outMap = new IndustryResourceOut[RESOURCES.ALL().Count];

        private readonly int index;
        public bool IsOnlyRoomDoNotUse { get; set; } = false;
        IndustryRegion reg = null;

        Lockable<Faction> lockable = GVALUES.FACTION.LOCK.Empty;

        private static string ¤¤input = "Input";
        static Industry()
        {
            D.ts(typeof(Industry));
        }

        public readonly SPRITE icon;

        public Industry(RoomBlueprintImp blue, RESOURCE outResource, double outRate, Boostable bonus) : base(blue, bonus)
        {
            index = all.Add(this);
            new IndustryResourceOut(data, outResource, outRate, outRate, outRate);
            icon = outResource.icon();
        }

        public Industry(RoomBlueprintImp blue, RESOURCE[] outs, double[] outRates, Boostable bonus) : base(blue, bonus)
        {
            index = all.Add(this);
            for (int i = 0; i < outs.Length; i++)
            {
                RESOURCE outResource = outs[i];
                double outRate = outRates[i];
                new IndustryResourceIn(data, outResource, outRate, outRate, outRate);
            }
            icon = new SPRITE.Imp(Icon.L)
            {
                render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                {
                    int w = X2 - X1;
                    double scale = (double)w / Icon.L;
                    int dim = (int)(Icon.M * scale);
                    int d = (w - dim) / outs.Length;

                    int y1 = Y1 + (w - dim) / 2;
                    for (int i = 0; i < outs.Length; i++)
                    {
                        int x1 = X1 + d * i;
                        outs[i].resource.icon().render(r, x1, x1 + dim, y1, y1 + dim);
                    }
                }
            };
        }

        public Industry(RoomBlueprintImp blue, Json json, Boostable bonus) : base(blue, bonus)
        {
            index = all.Add(this);
            json = json.Json("INDUSTRY");
            if (json.Has("IN"))
            {
                Json j = json.Json("IN");
                foreach (string k in j.Keys())
                {
                    RESOURCE res = RESOURCES.Map().Get(k, j);
                    double rate = j.D(k, 0, 10000);
                    new IndustryResourceIn(data, res, rate, rate, rate);
                }
            }
            if (json.Has("OUT"))
            {
                Json j = json.Json("OUT");
                foreach (string k in j.Keys())
                {
                    RESOURCE res = RESOURCES.Map().Get(k, j);

                    if (j.JsonIs(k))
                    {
                        Json jj = j.Json(k);
                        double rate = jj.D("PLAYER", 0, 100000);
                        double AIRate = jj.TryD("AI_RATE", 0, 100000, rate);
                        double AIRecovery = jj.TryD("AI_RECOVERY", 0, 100000, 1);
                        new IndustryResourceOut(data, res, rate, AIRate, AIRecovery);
                    }
                    else
                    {
                        double rate = j.D(k, 0, 10000);
                        new IndustryResourceOut(data, res, rate, rate, 1.0);
                    }
                }
            }

            if (json.Has("ICON"))
                icon = SPRITES.icons().Get(json);
            else
            {
                icon = new SPRITE.Imp(Icon.L)
                {
                    render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                    {
                        int w = X2 - X1;
                        double scale = (double)w / Icon.L;
                        int dim = (int)(Icon.M * scale);
                        int d = (w - dim) / outs().Count;

                        int y1 = Y1 + (w - dim) / 2;
                        for (int i = 0; i < outs().Count; i++)
                        {
                            int x1 = X1 + d * i;
                            outs.Get(i).resource.icon().render(r, x1, x1 + dim, y1, y1 + dim);
                        }
                    }
                };
            }
        }

        public Lockable<Faction> Lockable()
        {
            return lockable;
        }

        public Boostable Consumption()
        {
            return conBonus;
        }

        public IndustryResource Out(RESOURCE res)
        {
            return outMap[res.Index()];
        }

        public IEnumerable<IndustryResource> Outs()
        {
            return outs;
        }

        private class IndustryResourceOut : IndustryResource
        {
            IndustryResourceOut(DataOSimple<ROOM_IDATA_INSTANCE> data, RESOURCE res, double rate, double AIRate, double AIRecovery) : base(data, outs.Count, res, rate, AIRate, AIRecovery)
            {
                outs.Add(this);
                outMap[resource.Index()] = this;
                allRes.Add(this);
            }

            public override int Inc(ROOM_IDATA_INSTANCE r, double amount, bool record)
            {
                if (!double.IsFinite(amount))
                {
                    GAME.Warn(amount.ToString());
                    return 0;
                }
                if (!double.IsFinite(day.GetD(r)))
                {
                    day.SetD(r, 0);
                }

                int old = (int)day.GetD(r);
                day.IncD(r, amount);
                int now = (int)day.GetD(r);
                int d = now - old;
                if (d != 0)
                {
                    if (record)
                        GAME.player().res().Inc(resource, RTYPE.PRODUCED, d);
                    year.Inc(r, d);
                    history.Inc(d);
                    GAME.count().CRAFTED.Inc(1);
                }
                return d;
            }

            protected override double GetEffort(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds)
            {
                return IndustryUtil.CalcProductionRate(rateSeconds * workSeconds, skill, Industry.this, (RoomInstance)r);
            }
        }

        public IndustryRegion Reg()
        {
            return reg;
        }

        public static IEnumerable<Industry> CreateIndustries(RoomBlueprintImp blue, RoomInitData init, RoomBoost[] boosts, Boostable bonus, DOUBLE_O<Region> rr) throw IOException
        {
            List<Industry> res = new List<Industry>();

            // Implementation of the method logic
            // ...

            return res;
        }

        public static IEnumerable<Industry> CreateIndustries(RoomBlueprintImp blue, RoomInitData init, RoomBoost[] boosts, Boostable bonus) throw IOException
        {
            DOUBLE_O<Region> rr = new DOUBLE_O<Region>()
            {
                GetD = (Region t) => 1.0
            };
            return CreateIndustries(blue, init, boosts, bonus, rr);
        }

        private static RESOURCE Unique(Industry ins, IEnumerable<Industry> others)
        {
            RESOURCE res = ins.Outs().First().resource;
            bool unique = true;
            foreach (Industry i in others)
            {
                if (i != ins && i.Outs().First().resource == res)
                    unique = false;
            }
            if (unique)
                return res;
            foreach (IndustryResource r in ins.ins())
            {
                unique = true;
                foreach (Industry i in others)
                {
                    if (i != ins)
                    {
                        foreach (IndustryResource or in i.ins())
                        {
                            if (or.resource == r.resource)
                            {
                                unique = false;
                            }
                        }
                    }
                }
                if (unique)
                    return r.resource;
            }
            return res;
        }
    }
}