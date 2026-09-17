using init.resources;
using settlement.main;

public abstract class ROOM_DEGRADER
{
    /**
     * three bytes
     * @return
     */
    public abstract int getData();

    /**
     * three bytes
     * @param v
     * @return
     */
    protected abstract void setData(int v, bool realDegraded);

    public double get()
    {
        return get(getData());
    }

    public static double get(int data)
    {
        return MRoom.degrade(data);
    }

    public bool isRealDegraded()
    {
        return MRoom.degradeReal(getData());
    }

    public abstract int resSize();
    public abstract int resAmount(int i);
    public abstract RESOURCE res(int i);
    public abstract int roomArea();
    public abstract double base();
    public abstract double expenseRate();
    public abstract double degRate();

    public static double rate(double boost, double base, double isolation, double resAm, int area)
    {
        boost *= (1.0 + (1.0 - isolation) * 2);
        double v = boost * SETT.MAINTENANCE().tilesPerDay * area;
        v += boost * SETT.MAINTENANCE().resRate * resAm;
        return base * v;
    }

    public static double rateResource(double boost, double base, double isolation, double resAm)
    {
        boost *= (1.0 + (1.0 - isolation) * 2);
        return base * boost * SETT.MAINTENANCE().resRate * resAm;
    }

    public abstract double rate(double boost);

    public int jobs()
    {
        return MRoom.jobs(getData(), roomArea());
    }

    public double getSecret()
    {
        return MRoom.secretDegrade(getData());
    }
}