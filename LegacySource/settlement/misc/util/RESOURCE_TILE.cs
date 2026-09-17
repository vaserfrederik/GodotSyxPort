using settlement.main;
using init.resources;
using settlement.room.main;
using settlement.thing;

public interface RESOURCE_TILE : FINDABLE
{
    public RESOURCE resource();
    public void resourcePickup();
    public int reservable();
    public int amount();

    public default bool isFindable()
    {
        return resource() != null && amount() > 0;
    }

    public bool isStorage();
    public bool isPrio();

    public default double spoilRate()
    {
        return 1.0;
    }

    public default bool hasRoom()
    {
        return true;
    }

    public interface RESOURCE_TILE_HASER
    {
        public RESOURCE_TILE resourceTile(int tx, int ty);
        public double degradeRate();
    }

    public static readonly Getter GETTER = new Getter();

    public class Getter
    {
        private readonly RBITImp tmp = new RBITImp();

        private Getter()
        {
        }

        public int reserve(bool stored, bool fetch, RESOURCE r, int tx, int ty, int amount)
        {
            int am = 0;
            while (am < amount)
            {
                RESOURCE_TILE t = reservable(r, stored, fetch, tx, ty);
                if (t == null)
                    return am;
                while (am < amount && t.findableReservedCanBe())
                {
                    t.findableReserve();
                    am++;
                }
            }
            return am;
        }

        public void unreserve(RESOURCE r, int tx, int ty, int amount)
        {
            while (amount > 0)
            {
                RESOURCE_TILE t = RESOURCE_TILE.GETTER.reserved(r, tx, ty);
                if (t == null)
                    return;
                while (amount > 0 && t.findableReservedIs())
                {
                    t.findableReserveCancel();
                    amount--;
                }
            }
        }

        public int pickup(RESOURCE r, int tx, int ty, int amount)
        {
            int am = 0;
            while (am < amount)
            {
                RESOURCE_TILE t = reserved(r, tx, ty);
                if (t == null)
                    return am;
                while (am < amount && t.findableReservedIs())
                {
                    t.resourcePickup();
                    am++;
                }
            }
            return am;
        }

        public RESOURCE_TILE reservable(RBIT scattered, RBIT stored, RBIT fetch, int tx, int ty)
        {
            tmp.clear();
            tmp.or(scattered).or(stored).or(fetch);

            ScatteredResource sc = THINGS().resources.getReservable(tx, ty, tmp);
            if (sc != null && sc.findableReservedCanBe())
            {
                return sc;
            }

            Room room = ROOMS().map.get(tx, ty);
            if (room == null)
                return null;
            RESOURCE_TILE res = room.resourceTile(tx, ty);
            if (res == null)
                return null;
            RESOURCE r = res.resource();
            if (r == null)
                return null;
            if (!res.findableReservedCanBe())
                return null;
            if (res.isPrio())
            {
                if (fetch.has(r))
                    return res;
                return null;
            }
            if (res.isStorage())
            {
                if (stored.has(r))
                    return res;
                return null;
            }
            if (scattered.has(r))
                return res;
            return null;
        }

        public RESOURCE_TILE reservable(RESOURCE r, bool stored, bool fetch, int tx, int ty)
        {
            ScatteredResource sc = THINGS().resources.getReservable(tx, ty, r.bit);
            if (sc != null)
            {
                return sc;
            }

            Room room = ROOMS().map.get(tx, ty);
            if (room == null)
                return null;
            RESOURCE_TILE res = room.resourceTile(tx, ty);
            if (res == null)
                return null;
            if (r != res.resource())
                return null;
            if (!res.findableReservedCanBe())
                return null;
            if (res.isPrio() && !fetch)
            {
                return null;
            }
            else if (res.isStorage() && !stored)
            {
                return null;
            }
            return res;
        }

        public RESOURCE_TILE reserved(RESOURCE resource, int tx, int ty)
        {
            Room room = ROOMS().map.get(tx, ty);
            if (room != null)
            {
                RESOURCE_TILE res = room.resourceTile(tx, ty);
                if (res != null && res.resource() == resource && res.findableReservedIs())
                {
                    return res;
                }
            }
            foreach (Thing t in THINGS().get(tx, ty))
            {
                if (t is ScatteredResource)
                {
                    ScatteredResource sc = ((ScatteredResource)t);
                    if (sc.findableReservedIs() && sc.resource() == resource)
                    {
                        return sc;
                    }
                }
            }

            return null;
        }
    }
}