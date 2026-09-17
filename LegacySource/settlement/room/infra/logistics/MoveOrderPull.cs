using System;
using System.IO;
using System.Runtime.Serialization;

namespace Settlement.Room.Infra.Logistics
{
    [Serializable]
    public sealed class MoveOrderPull : ISerializable
    {
        private static readonly string ¤¤RoomInvalid = "The room to pull from is invalid";
        private static readonly string ¤¤noRes = "No resources have been selected.";
        private static readonly string ¤¤noResBoth = "Neither the source or destination room resources match the selected resources.";
        private static readonly string ¤¤noResSource = "The source room does not have any crates available of the selected resources.";
        private static readonly string ¤¤noResDest = "The destination room's resources doesn't match the selected resources.";
        private static readonly string ¤¤noResSourceA = "The source room does not have any resources available to be pulled.";
        private static readonly string ¤¤noResDestA = "The destination room does not have any crates available of the the selected resources.";
        private static readonly string ¤¤noResBothA = "Neither the source nor destination room have crates available.";
        private static readonly string ¤¤cycle = "The order is cyclic. Resources will be moved back and forth.";

        private static readonly RBITImp tmp = new RBITImp();
        private readonly Coo coo = new Coo();
        public readonly RBITImp resbits = new RBITImp();
        public byte cooldown = 0;
        public byte pullLimit;

        private short lsx, lsy, ldx, ldy;

        static MoveOrderPull()
        {
            D.ts(MoveOrderPull);
        }

        private MoveOrderPull(SerializationInfo info, StreamingContext context)
        {
            coo.set(info.GetInt32("x"), info.GetInt32("y"));
            resbits.clearSet((RBIT)info.GetValue("res", typeof(RBIT)));
        }

        public MoveOrderPull(COORDINATE coo, RBIT res)
        {
            this.coo.set(coo.x(), coo.y());
            resbits.clearSet(res);
        }

        public MoveOrderPull(RoomInstance dest, RBIT res)
        {
            coo.set(dest.mX(), dest.mY());
            resbits.clearSet(res);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", coo.x());
            info.AddValue("y", coo.y());
            info.AddValue("res", resbits);
        }

        public string Problem(MoveOrderPullInstance ins)
        {
            return Problem(Source(), ins, resbits);
        }

        public static string Problem(ROOM_MOVE_SOURCE source, MoveOrderPullInstance ins, RBIT bits)
        {
            if (source == null)
                return ¤¤RoomInvalid;

            if (bits.isClear())
                return ¤¤noRes;

            if (!source.MoveCapacity().Has(bits))
            {
                return ¤¤noResSource;
            }
            if (!ins.MoveOrderPullAccepted().Has(bits))
            {
                return ¤¤noResDest;
            }

            tmp.ClearSet(bits).And(source.MoveCapacity()).And(ins.MoveOrderPullAccepted());

            if (tmp.isClear())
                return ¤¤noResBoth;

            return null;
        }

        public string Warning(MoveOrderPullInstance ins)
        {
            string p = Problem(ins);
            if (p != null)
                return p;

            ROOM_MOVE_SOURCE source = Source();

            tmp.Clear();
            tmp.Or(source.MoveCapacity());
            tmp.And(resbits);

            if (tmp.isClear())
            {
                return ¤¤noResSource;
            }

            tmp.And(source.SourceAmountMask());

            if (tmp.isClear())
            {
                return ¤¤noResSourceA;
            }

            RBIT bb = ins.MoveOrderPullAvailable();
            if (!bb.Has(resbits))
            {
                return ¤¤noResDestA;
            }

            tmp.And(bb);

            if (tmp.isClear())
            {
                return ¤¤noResBothA;
            }

            RESOURCE_TILE t = source.SourceCrate(tmp, ins.MoveMinAmount(), lsx, lsy, pullLimit / 100.0);
            if (t == null)
            {
                return ¤¤noResSourceA;
            }
            else
            {
                lsx = (short)t.x();
                lsy = (short)t.y();
            }

            RoomInstance isource = (RoomInstance)source;

            if (isource is MoveOrderPullInstance)
            {
                MoveOrderPullInstance oo = (MoveOrderPullInstance)isource;
                foreach (MoveOrderPull o in oo.MoveOrdersPull())
                {
                    if (o != null && o.Source() == ins)
                    {
                        if (ins.MoveOrderPullAccepted().Has(oo.MoveOrderPullAccepted())
                            && resbits.Has(oo.MoveOrderPullAccepted()) && o.resbits.Has(resbits))
                        {
                            return ¤¤cycle;
                        }
                    }
                }
            }

            if (ins is MoveOrderPushInstance)
            {
                MoveOrderPushInstance pi = (MoveOrderPushInstance)ins;
                foreach (MoveOrderPush po in pi.MoveOrdersPush())
                {
                    if (po != null)
                    {
                        foreach (MoveOrderPull puo in ins.MoveOrdersPull())
                        {
                            if (ins.MoveOrderPullAccepted().Has(puo.Source().MoveCapacity()))
                            {
                                return ¤¤cycle;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void DestSet(RoomInstance dest)
        {
            coo.set(dest.mX(), dest.mY());
        }

        public COORDINATE DestCoo()
        {
            return coo;
        }

        public ROOM_MOVE_SOURCE Source()
        {
            Room r = SETT.ROOMS().map.Get(coo);
            if (r != null && r is ROOM_MOVE_SOURCE)
            {
                return (ROOM_MOVE_SOURCE)r;
            }
            return null;
        }

        public RoomInstance SourceI()
        {
            Room r = SETT.ROOMS().map.Get(coo);
            if (r != null && r is ROOM_MOVE_SOURCE)
            {
                return (RoomInstance)r;
            }
            return null;
        }

        public MoveJob Job(ROOM_MOVE_DEST dest, int carryMin, int carryMax)
        {
            if (carryMin <= 0)
                throw new RuntimeException();

            if (carryMax <= 0)
                throw new RuntimeException();

            carryMin = Math.Min(carryMin, carryMax);

            ROOM_MOVE_SOURCE source = Source();
            if (source == null)
                return null;

            RBIT fetchBits = dest.DestSpaceMask();
            if (!source.SourceAmountMask().Has(fetchBits))
                return null;

            tmp.Clear();
            tmp.Or(fetchBits);
            tmp.And(resbits);

            if (tmp.isClear())
                return null;

            RESOURCE_TILE t = source.SourceCrate(tmp, carryMin, lsx, lsy, pullLimit / 100.0);

            if (t == null)
                return null;
            if (t.resource() == null)
                throw new RuntimeException();
            if (!t.resource().bit.Has(tmp))
                throw new RuntimeException();

            int am = Math.Min(carryMax, t.reservable());
            if (am <= 0)
                throw new RuntimeException();
            lsx = (short)t.x();
            lsy = (short)t.y();
            MoveJob j = MoveJob.TMP;
            j.res = t.resource();

            j.source.set(t);
            j.stored = t.isStorage();
            j.prio = t.isPrio();
            TILE_STORAGE st = dest.DestCrate(j.res.bit, carryMin, ldx, ldy);
            if (st == null)
                return null;
            am = Math.Min(am, st.storageReservable());

            if (am <= 0)
            {
                throw new RuntimeException(st.storageReservable() + " " + dest);
            }
            j.maxAm = am;
            j.dest.set(st.x(), st.y());

            st = ((RoomInstance)dest).storage(j.dest.x(), j.dest.y());

            if (st.resource() != j.res)
            {
                throw new RuntimeException(st.resource() + " " + j.res);
            }

            return j;
        }

        public interface MoveOrderPullInstance
        {
            public MoveOrderPull[] MoveOrdersPull();
            public RBIT MoveOrderPullAccepted();
            public RBIT MoveOrderPullAvailable();
            public int MoveMinAmount();
            public int MoveMaxRadius();
            public void CopyFrom(MoveOrderPullInstance same);
        }
    }
}