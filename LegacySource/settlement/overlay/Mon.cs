using settlement.overlay;
using settlement.main;
using settlement.room.infra.monument;
using settlement.room.main;
using settlement.room.main.furnisher;
using snake2d;
using snake2d.util.misc;
using util.rendering;

class Mon : Addable
{
    public ROOM_MONUMENT m;
    public FurnisherItem it;
    public int radius;
    public int x1, y1;

    public Mon() : base(null, null, null, null, true, false)
    {
    }

    public void Set(ROOM_MONUMENT m, FurnisherItem it, int x1, int y1, int radius)
    {
        this.m = m;
        this.it = it;
        this.radius = radius;
        this.x1 = x1;
        this.y1 = y1;
        Add();
    }

    public void Set(ROOM_MONUMENT m)
    {
        this.m = m;
        this.it = null;
    }

    public override void InitBelow(RenderData data)
    {
        if (it != null)
            SETT.ENV().map.MONUMENT.AddExtra(m, it, x1, y1);
        base.InitBelow(data);
    }

    public override void RenderBelow(Renderer r, RenderIterator it)
    {
        int d = 0;
        RoomBlueprint b = SETT.ROOMS().map.blueprint.Get(it.Tile());
        if (b == null || b.RegistersEnvironment())
        {
            d = m.mapData.Get(it.Tx(), it.Ty());
            if (this.it != null)
                d = CLAMP.i(d + SETT.ENV().map.MONUMENT.Extra(x1, y1, it.Tx(), it.Ty()), 0, m.MaxEnv());
        }

        RenderUnder(d / (double)m.MaxEnv(), r, it);
    }

    public override void FinishBelow()
    {
        it = null;
        base.FinishBelow();
    }
}