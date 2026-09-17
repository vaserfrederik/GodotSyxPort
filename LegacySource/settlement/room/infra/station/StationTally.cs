using init.resources;
using settlement.main;
using settlement.room.infra.transport;

namespace settlement.room.infra.station
{
    public class StationTally
    {
        private byte crates = 0;
        private int stored = 0;
        private int reserved = 0;

        public StationTally()
        {
        }

        public void Remove(RESOURCE res, Crate crate, StationInstance ins)
        {
            SETT.ROOMS().STATION.Tally(crate.Resource()).Remove(this, ins);
            crates--;
            stored -= crate.Stored;
            reserved -= crate.Reserved;
            ins.BAmount.Set(res, stored - reserved >= 0);
            ins.BCapacity.Set(res, crates >= 0);
        }

        public void Add(RESOURCE res, Crate crate, StationInstance ins)
        {
            crates++;
            stored += crate.Stored;
            reserved += crate.Reserved;
            ins.BAmount.Set(res, stored - reserved >= 0);
            ins.BCapacity.Set(res, crates >= 0);
            SETT.ROOMS().STATION.Tally(crate.Resource()).Add(this, ins);
        }

        public int Stored()
        {
            return stored;
        }

        public int Reserved()
        {
            return reserved;
        }

        public int Crates()
        {
            return crates;
        }

        public int Space()
        {
            return crates * SETT.ROOMS().STATION.Crate.MAX_AM;
        }

        public int SpaceAvailable()
        {
            return crates * SETT.ROOMS().STATION.Crate.MAX_AM - stored;
        }

        public void Clear()
        {
            crates = 0;
            stored = 0;
            reserved = 0;
        }

        public static class Total
        {
            private byte crates = 0;
            private int stored = 0;
            private int reserved = 0;
            private int incoming = 0;
            private int available = 0;
            public readonly RESOURCE Res;

            public Total(RESOURCE res)
            {
                this.Res = res;
            }

            public void Remove(StationTally crate, StationInstance ins)
            {
                if (ins.Accepting(Res))
                    available--;
                crates -= crate.Crates;
                stored -= crate.Stored;
                reserved -= crate.Reserved;
                incoming -= ins.Incoming(Res);
            }

            public void Add(StationTally crate, StationInstance ins)
            {
                crates += crate.Crates;
                stored += crate.Stored;
                reserved += crate.Reserved;
                incoming += ins.Incoming(Res);
                if (ins.Accepting(Res))
                    available++;
            }

            public void Debug()
            {
                int am = 0;
                for (int i = 0; i < SETT.ROOMS().STATION.InstancesSize(); i++)
                {
                    StationInstance ins = SETT.ROOMS().STATION.GetInstance(i);
                    if (ins.Tally == null)
                        return;
                    if (ins.Accepting(Res))
                        am++;
                }
                if (am != available)
                    throw new RuntimeException(Res + " " + am + " " + available);
            }

            public int Stored()
            {
                return stored;
            }

            public int Reserved()
            {
                return reserved;
            }

            public int Incoming()
            {
                return incoming;
            }

            public int Crates()
            {
                return crates;
            }

            public int Space()
            {
                return crates * ROOM_TRANSPORT.MAX_LOAD;
            }

            public int Accepting()
            {
                return available;
            }

            public void Clear()
            {
                crates = 0;
                stored = 0;
                reserved = 0;
                available = 0;
                incoming = 0;
            }
        }
    }
}