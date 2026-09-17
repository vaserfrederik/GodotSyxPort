using settlement.room.main.copy;
using view.tool;

public sealed class ROOM_COPY
{
    private Copier copy;
    public readonly CopierMass copier = new CopierMass();
    public readonly SavedPrintsPlacer savedPlacer;
    public readonly SavedPrints prints;

    public ROOM_COPY(ROOMS r)
    {
        BSwap s = new BSwap(r);
        prints = new SavedPrints(r);
        savedPlacer = new SavedPrintsPlacer(s);
        copy = new Copier(s);
    }

    public void Copy(int rx, int ry)
    {
        if (copy.IsPlacable(rx, ry) == null)
            copy.PlaceFirst(rx, ry);
    }

    public PLACABLE Copy()
    {
        return copy;
    }
}