using System;
using System.Collections.Generic;
using settlement.main;
using game;
using init.constant;
using settlement.path.finders;
using settlement.room.main.employment;
using settlement.thing.pointlight;
using snake2d.util.color;
using snake2d.util.map;
using snake2d.util.sets;
using util.keymap;
using view.sett.ui.room;

public abstract class RoomBlueprint : RoomResource, MAP_OBJECT<Room>, MAPPED
{
    private readonly int index;
    private static readonly ArrayListGrower<RoomBlueprint> ALL = new ArrayListGrower<RoomBlueprint>();
    static RoomBlueprint()
    {
        new GameDisposable
        {
            protected override void Dispose()
            {
                ALL.Clear();
            }
        };
    }

    public readonly string key;

    protected RoomBlueprint(string key)
    {
        index = ALL.Add(this);
        this.key = key;
    }

    public Room Get(int tx, int ty)
    {
        if (IN_BOUNDS(tx, ty))
            return Get(tx + ty * TWIDTH);
        return null;
    }

    public Room Get(int tile)
    {
        Room r = ROOMS().map.Get(tile);
        if (r != null && r.Blueprint() == this)
            return r;
        return null;
    }

    public override int Index()
    {
        return index;
    }

    public abstract SFinderFindable Service(int tx, int ty);

    public abstract COLOR MiniC(int tx, int ty);

    public abstract COLOR MiniCPimped(ColorImp original, int tx, int ty, bool northern, bool southern);

    public bool MakesDudesDirty()
    {
        return false;
    }

    public void AppendView(LISTE<UIRoomModule> mm)
    {
    }

    public double Strength(int tile)
    {
        return 400 * C.TILE_SIZE;
    }

    public LOS LOS(int tx, int ty)
    {
        return SETT.TILE_MAP().LOS(tx, ty);
    }

    public RoomEmploymentSimple Employment()
    {
        return null;
    }

    public override string Key()
    {
        return key;
    }

    public bool RegistersEnvironment()
    {
        return false;
    }
}