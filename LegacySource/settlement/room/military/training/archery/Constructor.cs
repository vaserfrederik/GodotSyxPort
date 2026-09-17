using System;
using System.IO;
using settlement.path;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.info;

abstract class Constructor : Furnisher
{
    private readonly ROOM_ARCHERY blue;

    public readonly FurnisherItemTile plat;

    public readonly FurnisherStat men = new FurnisherStat(this, 0)
    {
        Get = (AREA area, double fromItems) => fromItems,
        Format = (GText t, double value) => GFORMAT.i(t, (int)value)
    };

    protected Constructor(ROOM_ARCHERY blue, RoomInitData init) : base(init, 1, 1, 88, 44)
    {
        this.blue = blue;

        Json js = init.data().json("SPRITES");

        RoomSprite1x1 sTarget = new RoomSprite1x1(js, "TARGET_1X1");
        RoomSprite1x1 sLane = new RoomSprite1x1(js, "LANE_1X1");
        RoomSprite1x1 sPlat = new RoomSprite1x1(js, "PLATFORM_1X1");
        RoomSprite spriteCand = new RoomSprite1x1(js, "TABLE_1X1");

        RoomSprite spriteFence = new RoomSpriteCombo(js, "FENCE_COMBO");

        FurnisherItemTile ta = new FurnisherItemTile(this, false, sTarget, AVAILABILITY.ROOM_SOLID, false);
        FurnisherItemTile ll = new FurnisherItemTile(this, false, sLane, AVAILABILITY.ROOM_SOLID, false);
        FurnisherItemTile pp = new FurnisherItemTile(this, true, sPlat, AVAILABILITY.ROOM, false);
        FurnisherItemTile ca = new FurnisherItemTile(this, false, spriteCand, AVAILABILITY.ROOM_SOLID, true);
        FurnisherItemTile __ = new FurnisherItemTile(this, false, spriteFence, AVAILABILITY.ROOM_SOLID, false);
        plat = pp;

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __},
            {ca, ta, ca},
            {__, ll, __},
            {__, ll, __},
            {__, ll, __},
            {__, ll, __},
            {__, ll, __},
            {__, ll, __},
            {ca, pp, ca},
        }, 1.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __},
            {ca, ta, ta, ca},
            {__, ll, ll, __},
            {__, ll, ll, __},
            {__, ll, ll, __},
            {__, ll, ll, __},
            {__, ll, ll, __},
            {__, ll, ll, __},
            {ca, pp, pp, ca},
        }, 2.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __},
            {ca, ta, ta, ta, ca},
            {__, ll, ll, ll, __},
            {__, ll, ll, ll, __},
            {__, ll, ll, ll, __},
            {__, ll, ll, ll, __},
            {__, ll, ll, ll, __},
            {__, ll, ll, ll, __},
            {ca, pp, pp, pp, ca},
        }, 3.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, ca},
        }, 4.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, ca},
        }, 5.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, pp, ca},
        }, 6.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, pp, pp, ca},
        }, 7.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, pp, pp, pp, ca},
        }, 8.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, pp, pp, pp, pp, ca},
        }, 9.0);

        new FurnisherItem(new FurnisherItemTile[][]
        {
            {__, __, __, __, __, __, __, __, __, __, __, __},
            {ca, ta, ta, ta, ta, ta, ta, ta, ta, ta, ta, ca},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {__, ll, ll, ll, ll, ll, ll, ll, ll, ll, ll, __},
            {ca, pp, pp, pp, pp, pp, pp, pp, pp, pp, pp, ca},
        }, 10.0);

        Flush();
    }

    public override bool usesArea()
    {
        return true;
    }

    public override bool mustBeIndoors()
    {
        return false;
    }

    public override bool mustBeOutdoors()
    {
        return true;
    }

    public override ROOM_ARCHERY blue()
    {
        return blue;
    }
}