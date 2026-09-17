using settlement.path.finders;
using settlement.path.components;
using settlement.path.path;
using snake2d;
using view.sett;
using view.tool;

class TestPath : PlacableSingle
{
    private readonly SFINDER finder;
    static readonly SPath tester = new SPath();

    public TestPath(CharSequence name, SFINDER finder) : base("path test: " + name)
    {
        this.finder = finder;
        IDebugPanelSett.add(this);
    }

    public override CharSequence isPlacable(int tx, int ty)
    {
        SComponent c = PATH().comps.zero.get(tx, ty);
        if (c == null)
        {
            return E;
        }
        return null;
    }

    public override void placeFirst(int tx, int ty)
    {
        place(tx, ty, tester);
        if (!tester.isSuccessful())
            LOG.ln("nay!");
        else
            LOG.ln("yay " + tester.destX() + " " + tester.destY());
    }

    protected void place(int sx, int sy, SPath p)
    {
        tester.request(sx, sy, finder, int.MaxValue);
    }
}