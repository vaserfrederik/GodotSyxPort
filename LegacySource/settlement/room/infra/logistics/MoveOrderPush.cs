using System;
using System.Collections.Generic;
using settlement.main;
using settlement.room.main;
using snake2d.util.datatypes;
using util.text;

namespace settlement.room.infra.logistics
{
    [Serializable]
    public class MoveOrderPush : Serializable
    {
        private static readonly string ¤¤RoomInvalid = "The destination room is invalid.";
        private static readonly string ¤¤ResNone = "No resource have been specified.";
        private static readonly string ¤¤ResBad = "The resources specified can not be accepted by the destination room.";
        private static readonly string ¤¤ResANone = "There are not enough resources stocked to deliver.";
        private static readonly string ¤¤ResANoneDest = "The destination does not have the capacity to accept a delivery.";
        private static readonly string ¤¤limit = "The destination room's current storage exceeds the current push limit";

        static MoveOrderPush()
        {
            D.ts(typeof(MoveOrderPush));
        }

        private readonly Coo coo = new Coo();
        public byte cooldown = 0;
        private short ox, oy;
        public byte limit = 80;
        private static readonly long serialVersionUID = 1L;

        public MoveOrderPush(RoomInstance dest)
        {
            coo.set(dest.mX(), dest.mY());
        }

        public string problem(MoveOrderPushInstance ins)
        {
            ROOM_MOVE_DEST dest = dest();
            return problem(dest, ins);
        }

        public static string problem(ROOM_MOVE_DEST dest, MoveOrderPushInstance ins)
        {
            if (dest == null)
                return ¤¤RoomInvalid;
            if (ins.moveOrderPushCapacity().isClear())
                return ¤¤ResNone;
            if (!ins.moveOrderPushCapacity().has(dest.destSpaceMask()))
                return ¤¤ResBad;
            return null;
        }

        public string warning(MoveOrderPushInstance ins)
        {
            string p = problem(ins);
            if (p != null)
                return p;
            ROOM_MOVE_DEST dest = dest();

            RBIT acc = ins.moveOrderPushAvailable();
            if (!acc.has(dest.destSpaceMask()))
            {
                return ¤¤ResANone;
            }

            bool h = false;
            foreach (RESOURCE r in RESOURCES.ALL())
            {
                if (acc.has(r) && limit > dest.storedD(r) * 100)
                {
                    h = true;
                    break;
                }
            }

            if (!h)
            {
                return ¤¤limit;
            }

            TILE_STORAGE t = dest.destCrate(acc, ins.moveMinAmount(), ox, oy);
            if (t == null)
            {
                return ¤¤ResANoneDest;
            }
            else
            {
                ox = (short)t.x();
                oy = (short)t.y();
            }

            return null;
        }

        public void destSet(RoomInstance dest)
        {
            coo.set(dest.mX(), dest.mY());
        }

        public ROOM_MOVE_DEST dest()
        {
            Room r = SETT.ROOMS().map.get(coo);
            if (r != null && r is MoveJob.ROOM_MOVE_DEST)
            {
                return (ROOM_MOVE_DEST)r;
            }
            return null;
        }

        public RoomInstance destI()
        {
            Room r = SETT.ROOMS().map.get(coo);
            if (r != null && r is MoveJob.ROOM_MOVE_DEST)
            {
                return (RoomInstance)r;
            }
            return null;
        }

        public interface MoveOrderPushInstance
        {
            public MoveOrderPush[] moveOrdersPush();
            public RBIT moveOrderPushCapacity();
            public RBIT moveOrderPushAvailable();
            public int moveMinAmount();
            public int moveMaxRadius();
        }
    }
}