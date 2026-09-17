using System;
using settlement.entity.humanoid.ai.work;
using game.time;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.food.fish;
using snake2d.util;
using snake2d.util.sprite.text;

namespace Settlement.Entity.Humanoid.Ai.Work
{
    internal class WorkFisherman : WorkAbs
    {
        protected WorkFisherman(AIModule_Work module, ROOM_FISHERY blueprint, PlanBlueprint[] map, Works works)
            : base(module, blueprint, map, works)
        {
        }

        protected override AISubActivation work(Humanoid a, AIManager d)
        {
            ROOM_FISHERY f = (ROOM_FISHERY)work(a).blueprintI();

            if (f.launchFishingExpedition(a, d.planTile.x(), d.planTile.y()))
            {
                return goFishing.Set(a, d);
            }

            return base.work(a, d);
        }

        private readonly Resumer goFishing = new Resumer("fishyfish")
        {
            SetAction = (Humanoid a, AIManager d) =>
            {
                SETT.ENTITIES().moveIntoTheTheUnknown(a);
                a.speed.magnitudeInit(0);
                d.planByte2 = (byte)TIME.days().bitsSinceStart();
                d.planByte3 = 0;
                jobGet(a, d).jobStartPerforming();
                return AI.SUBS().STAND.activate(a, d);
            },

            Res = (Humanoid a, AIManager d) =>
            {
                if (d.planByte3 == 1)
                {
                    return init(a, d);
                }

                if (MATH.distanceC(d.planByte2 & 0x0FF, TIME.days().bitsSinceStart() & 0x0FF, 0x0FFF) >= 2)
                {
                    Can(a, d);
                    return null;
                }

                return AI.SUBS().STAND.activate(a, d);
            },

            Can = (Humanoid a, AIManager d) =>
            {
                base.work.Can(a, d);
                SETT.ENTITIES().returnFromTheTheUnknown(a);
            },

            Con = (Humanoid a, AIManager d) => jobGet(a, d) != null,

            Name = (Humanoid a, AIManager d, Str str) =>
            {
                if (jobGet(a, d) != null)
                    str.add(jobGet(a, d).jobName());
                else
                    base.name(a, d, str);
            },

            Event = (Humanoid a, AIManager d, HEventData e) =>
            {
                if (e.@event == HEvent.FISHINGTRIP_OVER)
                {
                    if (work(a) != null && work(a).blueprintI() is ROOM_FISHERY)
                    {
                        ROOM_FISHERY f = (ROOM_FISHERY)work(a).blueprintI();
                        f.performFishingTrip(a, d.planTile.x(), d.planTile.y(), 0);
                        d.planByte3 = 1;
                    }
                    else
                    {
                        Can(a, d);
                        d.planByte3 = 1;
                    }
                }
                return base.event(a, d, e);
            }
        };
    }
}