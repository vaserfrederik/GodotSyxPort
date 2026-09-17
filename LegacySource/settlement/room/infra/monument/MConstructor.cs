using System;
using System.IO;
using settlement.environment;
using settlement.main;
using settlement.overlay;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d;
using snake2d.util.datatypes;

class MConstructor : Furnisher
{
    private readonly ROOM_MONUMENT blue;

    public MConstructor(ROOM_MONUMENT blue, RoomInitData init) : base(init, init.data().jsons("ITEMS").Length, 0)
    {
        this.blue = blue;
    }

    public override bool UsesArea()
    {
        return false;
    }

    public override bool MustBeIndoors()
    {
        return false;
    }

    public override Room Create(TmpArea area, RoomInit init)
    {
        return blue.instance.place(area);
    }

    public override RoomBlueprintImp Blue()
    {
        return blue;
    }

    public override void RenderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item)
    {
        if (rx == 0 && ry == 0)
        {
            SETT.OVERLAY().monument(blue, item, tx, ty, 8);
        }
    }

    public override Addable Overlay()
    {
        return SETT.OVERLAY().monument(blue);
    }

    public override bool EnvValue(SettEnv e, SettEnvValue v, int tx, int ty)
    {
        if (envRadius[e.index()] != 0)
        {
            v.radius = (double)blue.radius(SETT.ROOMS().fData.item.get(tx, ty)) / SettEnvMap.RADIUS;
            v.value = envValue[e.index()];
            return true;
        }
        return false;
    }

    public override void PutFloor(int tx, int ty, int upgrade, AREA area)
    {
        base.PutFloor(tx, ty, upgrade, area);
        // if (tree)
        // {
        //     SETT.FERTILITY().currentSetAbs(tx, ty, 0.7);
        // }
    }
}