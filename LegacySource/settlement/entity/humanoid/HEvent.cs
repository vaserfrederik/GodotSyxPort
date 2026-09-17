using System;
using System.Collections.Generic;

namespace Settlement.Entity.Humanoid
{
    public enum HEvent
    {
        MEET_HARMLESS,
        MEET_ENEMY,
        COLLISION_HARD,
        COLLISION_SOFT,
        COLLISION_TILE,
        CHECK_MORALE,
        EXHAUST,
        NOTIFY_CRIME,
        ROOM_REMOVED,
        COLLISION_UNREACHABLE,
        INTERRACT,
        FISHINGTRIP_OVER,
        ALERT_DANGER
    }

    public static class HEventExtensions
    {
        public static readonly List<HEvent> All = new List<HEvent>(Enum.GetValues(typeof(HEvent)));
    }

    public class HEventData
    {
        public HEvent Event;
        public double NorX, NorY, Momentum;
        public ENTITY Other;
        public RoomInstance Room;
        public double FacingDot;
        public bool Broken;
        public int Tx, Ty;
        public bool SpeedHasChanged;

        private HEventData()
        {
        }
    }

    public static class Handler
    {
        private static bool debug = false;
        private static readonly HEventData eventData = new HEventData();

        public static void Collide(Humanoid a, AIManager ai, ECollision coll)
        {
            int time = (int)TIME.CurrentSecond;

            bool hostile = coll.DamageTileStrength > 0 || (coll.Other != null && coll.Other is Humanoid && HPoll.Handler.IsEnemy(a, (Humanoid)coll.Other));

            if (hostile)
                ai.LastCollision = time;
            else if (coll.Other is Humanoid)
                ai.LastCollision = Math.Max(ai.LastCollision, ((Humanoid)coll.Other).Ai.LastCollision);

            double mom = coll.TileMomentum * a.Physics.GetMassI() * EPHYSICS.MomThresholdI;

            CAUSE_LEAVE l = coll.Leave;
            if (l == null)
                l = coll.Other is Animal ? CAUSE_LEAVES.ANIMAL() : CAUSE_LEAVES.SLAYED();

            if (mom > 4)
            {
                a.InflictDamage(2, l);
                if (a.IsRemoved())
                {
                    if (coll.Other is Animal)
                    {
                        Animal an = (Animal)coll.Other;
                        SETT.ANIMALS().Spawn.ReportKillRevenge(an.Species());
                    }
                }
                return;
            }

            if (coll.DamageTileStrength > 0)
            {
                double dam = GAME.Battle().Fight.GetDamageDone(coll, a);

                if (dam > 0 && !a.InflictDamage(dam, l))
                {
                    if (a.IsRemoved())
                    {
                        if (coll.Other is Animal)
                        {
                            Animal an = (Animal)coll.Other;
                            SETT.ANIMALS().Spawn.ReportKillRevenge(an.Species());
                        }
                    }
                    return;
                }
            }

            eventData.Momentum = mom;
            eventData.NorX = coll.NorX;
            eventData.NorY = coll.NorY;
            eventData.SpeedHasChanged = coll.SpeedHasChanged;

            if (mom > 1.0)
            {
                eventData.Event = HEvent.COLLISION_HARD;
                ai.Event(a, eventData);
                return;
            }
            eventData.FacingDot = coll.DirDot;
            eventData.Other = coll.Other;

            if (coll.Other is Humanoid)
            {
                if (hostile)
                    eventData.Event = HEvent.MEET_ENEMY;
                else
                {
                    if (mom > 0)
                        eventData.Event = HEvent.COLLISION_SOFT;
                    else
                        eventData.Event = HEvent.MEET_HARMLESS;
                }
                ai.Event(a, eventData);
            }
            else
            {
                if (mom > 0 || coll.Other == null)
                    eventData.Event = HEvent.COLLISION_SOFT;
                else
                    eventData.Event = HEvent.MEET_HARMLESS;
                ai.Event(a, eventData);
            }
        }

        public static void Meet(Humanoid a, AIManager ai, ENTITY other)
        {
            eventData.Event = HEvent.MEET_HARMLESS;
            eventData.Other = other;
            if (other is Animal && (STATS.RAN().Get(a.Indu(), 0) & 0x01FF) == 0)
                STATS.POP().FRIEND.Set(a.Indu(), other);
            ai.Event(a, eventData);
        }

        public static void AlertDanger(Humanoid a)
        {
            eventData.Event = HEvent.ALERT_DANGER;
            a.Ai.Event(a, eventData);
        }

        public static void NotifyCrime(Humanoid a, ENTITY criminal)
        {
            eventData.Event = HEvent.NOTIFY_CRIME;
            eventData.Other = criminal;
            a.Ai.Event(a, eventData);
        }

        public static void Exhaust(Humanoid a)
        {
            eventData.Event = HEvent.EXHAUST;
            a.Ai.Event(a, eventData);
        }

        public static void CheckMorale(Humanoid a)
        {
            eventData.Event = HEvent.CHECK_MORALE;
            a.Ai.Event(a, eventData);
        }

        public static void RemoveRoom(Humanoid a, RoomInstance room)
        {
            AI.Modules().EvictFromRoom(a, a.Ai, room);
            eventData.Event = HEvent.ROOM_REMOVED;
            eventData.Room = room;
            a.Ai.Event(a, eventData);
        }

        public static void CollisionUnreachable(Humanoid a)
        {
            eventData.Event = HEvent.COLLISION_UNREACHABLE;
            a.Ai.Event(a, eventData);
        }

        public static void FishingTripOver(Humanoid a, double time)
        {
            eventData.Event = HEvent.FISHINGTRIP_OVER;
            eventData.Momentum = time;
            a.Ai.Event(a, eventData);
        }

        public static bool Interact(Humanoid a, Humanoid friend)
        {
            eventData.Event = HEvent.INTERRACT;
            eventData.Other = friend;
            return a.Ai.Event(a, eventData);
        }

        public static bool CollideTile(Humanoid a, AIManager ai, double norX, double norY, double momentum, bool broken, int tx, int ty)
        {
            momentum *= EPHYSICS.MomThresholdI;
            eventData.Momentum = momentum;
            eventData.NorX = norX;
            eventData.NorY = norY;
            eventData.SpeedHasChanged = true;
            if (momentum >= 1)
            {
                eventData.Event = HEvent.COLLISION_HARD;
                ai.Event(a, eventData);
                return true;
            }
            else
            {
                eventData.Event = HEvent.COLLISION_TILE;
                eventData.Tx = tx;
                eventData.Ty = ty;
                eventData.Broken = broken;
                return ai.Event(a, eventData);
            }
        }
    }
}