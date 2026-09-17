using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Infra.Transport
{
    [Serializable]
    class Cart : ISerializable
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly long serialVersionUID = 1L;

        public const int PREPARATION_TIME = (int)TIME.workSeconds();

        private short delivering;
        private short resource = -1;
        private short loaded;
        private short unloaded;
        private short unloadedSpots;
        private double preparation;

        public Cart()
        {
        }

        public int Stored()
        {
            return loaded;
        }

        public void Store(int am)
        {
            loaded += (short)am;
        }

        void UnloadedInc(int am)
        {
            unloaded += (short)am;
        }

        public bool NeedsPrep()
        {
            return preparation < PREPARATION_TIME;
        }

        public double PrepD()
        {
            return preparation / PREPARATION_TIME;
        }

        public void Prep(double time)
        {
            preparation += time;
        }

        public int Unloaded()
        {
            return unloaded;
        }

        void UnloadedSpotsInc(int am)
        {
            unloadedSpots += (short)am;
        }

        int UnloadedSpots()
        {
            return unloadedSpots;
        }

        public RESOURCE Resource()
        {
            if (resource == -1)
                return null;
            return RESOURCES.ALL().Get(resource);
        }

        protected void LoadFix()
        {
            resource = (short)RESOURCES.Map().Loader().Fix(resource, -1);
        }

        void Empty()
        {
            loaded = 0;
        }

        void Clear()
        {
            Empty();
        }

        void Coco()
        {
            preparation -= PREPARATION_TIME;
            if (preparation < 0)
                preparation = 0;
        }

        void Go()
        {
            delivering += loaded;
            Clear();
            Coco();
        }

        void Deliver(int am)
        {
            delivering -= (short)am;
        }

        public int Delivering()
        {
            return delivering;
        }

        public void DeliverIncrease(int am)
        {
            delivering += (short)am;
        }

        public void ResourceSet(RESOURCE res, TransportInstance ins)
        {
            RESOURCE old = Resource();
            if (old == res)
            {
                return;
            }
            int am = Stored();

            Empty();
            preparation = 0;
            ins.BlueprintI().Job.Remove(ins);
            resource = res == null ? -1 : (short)res.Index();
            ins.BlueprintI().Job.Add(ins);
            foreach (MoveOrderPull o in ins.PullOrders)
            {
                if (o != null)
                    o.Resbits.ClearSet(ins.MoveCapacity());
            }

            if (old != null && am > 0)
            {
                foreach (COORDINATE c in ins.Body())
                {
                    if (ins.Is(c) && SETT.PATH().Availability.Get(c) == AVAILABILITY.ROOM)
                    {
                        SETT.THINGS().Resources.Create(c, old, am);
                        return;
                    }
                }
            }
        }

        public bool CanGo()
        {
            if (Resource() == null)
                return false;
            if (preparation < PREPARATION_TIME)
                return false;
            if (loaded >= ROOM_TRANSPORT.MAX_LOAD)
                return true;
            return false;
        }

        public bool CartVisible()
        {
            return preparation >= PREPARATION_TIME || loaded > 0;
        }

        public bool OxVisible()
        {
            return preparation >= PREPARATION_TIME;
        }

        // Implementing ISerializable interface
        protected Cart(SerializationInfo info, StreamingContext context)
        {
            delivering = info.GetShort("delivering");
            resource = info.GetShort("resource");
            loaded = info.GetShort("loaded");
            unloaded = info.GetShort("unloaded");
            unloadedSpots = info.GetShort("unloadedSpots");
            preparation = info.GetDouble("preparation");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("delivering", delivering);
            info.AddValue("resource", resource);
            info.AddValue("loaded", loaded);
            info.AddValue("unloaded", unloaded);
            info.AddValue("unloadedSpots", unloadedSpots);
            info.AddValue("preparation", preparation);
        }
    }
}