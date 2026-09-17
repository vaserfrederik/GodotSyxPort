using System;
using System.Collections.Generic;
using game;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.stats;
using settlement.stats.service;
using settlement.stats.standing;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

namespace settlement.room.service.module
{
    public abstract class RoomServiceAccess : RoomService, INDEXED
    {
        private static readonly ArrayListGrower<RoomServiceAccess> all = new ArrayListGrower<RoomServiceAccess>();
        static RoomServiceAccess()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    all.Clear();
                }
            };
        }

        public readonly StandingDef standingDef;
        public readonly string[] induMore;
        public readonly BoostSpecs boosts;
        private readonly int index;

        public RoomServiceAccess(RoomBlueprintImp b, RoomInitData data, NEED need) : base(b, data, need)
        {
            this.index = all.Add(this);
            induMore = data.Text().Json("SERVICE").Texts("MORE");
            Json json = data.Data().Json("SERVICE");
            standingDef = new StandingDef(json.Json("STANDING"));
            boosts = new BoostSpecs(b.info.name, b.icon, false);
            boosts.Read(json, BValue.VALUE1);
            usage = json.DTry("USAGE", 0, 1, 1);
        }

        public void ReportAccess(Humanoid a, COORDINATE c)
        {
            ReportAccess(a, c.x, c.y);
        }

        public void ReportContent(Humanoid a, ROOM_SERVICER service)
        {
            RoomInstance ins = (RoomInstance)service;
            Stats().SetAccess(a, true, service.Quality(), 1, ins.Upgrade());
        }

        public void ReportAccess(Humanoid a, int tx, int ty)
        {
            ROOM_SERVICER r = (ROOM_SERVICER)room.Get(tx, ty);
            if (r == null)
                return;
            RoomInstance ins = (RoomInstance)r;
            Stats().SetAccess(a, true, r.Quality(), Stats().Proximity(a), ins.Upgrade());
        }

        public void ReportDistance(Humanoid a)
        {
            double p = 1.0 - (finder.GetDistance() - radius / 3) / radius;
            p = CLAMP.d(p, 0, 1);
            p = Math.Sqrt(p);
            Stats().SetProximity(a, p);
        }

        public void ClearAccess(Humanoid a)
        {
            Stats().SetAccess(a, false, 0, 0, 0);
        }

        public StatServiceRoom Stats()
        {
            return STATS.SERVICE().ROOMS.Get(index);
        }

        public interface ROOM_SERVICE_ACCESS_HASER : ROOM_SERVICE_HASER
        {
            new RoomServiceAccess Service();
        }

        public double CityAccess()
        {
            double d = 0;
            double p = 0;
            for (int ci = 0; ci < HCLASSES.ALLP().Size(); ci++)
            {
                HCLASS c = HCLASSES.ALLP().Get(ci);
                for (int ri = 0; ri < RACES.All().Size(); ri++)
                {
                    Race r = RACES.All().Get(ri);
                    if (Stats().Permission().Is(HCLASS_RACE.clP(r, c)))
                    {
                        double pp = STATS.POP().POP.Data(c).Get(r);
                        p += pp;
                        d += pp * Stats().Access().Data(c).GetD(r);
                    }
                }
            }
            if (p == 0)
                return 0;
            return d / p;
        }

        public bool AccessRequest(Humanoid a)
        {
            return Stats().AccessRequest(a);
        }

        public bool IsGoodTime()
        {
            return true;
        }

        public static LIST<RoomServiceAccess> ALL()
        {
            return all;
        }

        public int Index()
        {
            return index;
        }
    }
}