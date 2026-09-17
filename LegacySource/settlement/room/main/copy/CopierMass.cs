using System;
using settlement.room.main.copy;
using game.faction;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using settlement.room.main.placement;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.main.copy
{
    public sealed class CopierMass
    {
        public CopierMass()
        {
        }

        private static readonly RoomAreaWrapper wrap = new RoomAreaWrapper();

        public bool IsPlacable(int sx, int sy, int dx, int dy)
        {
            Room r = SETT.ROOMS().map.Get(sx, sy);
            if (r == null)
                return false;

            if (!CanCopy(sx, sy))
                return false;

            Furnisher c = r.Constructor();

            if (c.Placable(dx, dy, SETT.ROOMS().fData.item.Get(sx, sy), SETT.ROOMS().fData.tile.Get(sx, sy)) != null)
                return false;

            if (PLACEMENT.Placable(dx, dy, c.Blue(), true) != null)
                return false;

            FurnisherItem it = SETT.ROOMS().fData.item.Get(sx, sy);

            if (it == null)
                return true;

            FurnisherItemTile tile = SETT.ROOMS().fData.tile.Get(sx, sy);

            if (tile.MustBeReachable)
            {
                int bi = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.PLACA().solidityWill.Is(dx, dy, d))
                        bi++;
                }
                if (bi == 4)
                    return false;
            }

            return true;
        }

        public bool CanCopy(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.Get(tx, ty);
            if (r == null)
                return false;
            if (r.Blueprint() == SETT.ROOMS().THRONE)
                return false;
            if (r.Constructor() == null)
                return false;
            if (!r.Constructor().Blue().Reqs.Passes(FACTIONS.player()))
                return false;
            return (r.Constructor().CanBeCopied());
        }

        public void Copy(int rx, int ry, int destCX, int destCY, int rot)
        {
            Room room = SETT.ROOMS().map.Get(rx, ry);

            if (room == null || room.Constructor() == null || room.Constructor().Blue() == null)
                return;

            wrap.Done();
            ROOMA r = wrap.Init(room, rx, ry);

            foreach (COORDINATE c in r.Body())
            {
                if (r.Is(c))
                {
                    int dx = c.X() - r.Body().CX();
                    int dy = c.Y() - r.Body().CY();
                    for (int i = 0; i < rot; i++)
                    {
                        int k = dx;
                        dx = -dy;
                        dy = k;
                    }
                    int x = (int)(destCX + dx);
                    int y = (int)(destCY + dy);
                    if (!IsPlacable(c.X(), c.Y(), x, y))
                    {
                        return;
                    }
                }
            }

            TmpArea tmp = SETT.ROOMS().TmpArea(this);

            foreach (COORDINATE c in r.Body())
            {
                if (r.Is(c))
                {
                    int dx = c.X() - r.Body().CX();
                    int dy = c.Y() - r.Body().CY();
                    for (int i = 0; i < rot; i++)
                    {
                        int k = dx;
                        dx = -dy;
                        dy = k;
                    }
                    int x = (int)(destCX + dx);
                    int y = (int)(destCY + dy);
                    tmp.Set(x, y);
                }
            }

            foreach (COORDINATE c in r.Body())
            {
                if (r.Is(c))
                {
                    FurnisherItem it = SETT.ROOMS().fData.item.Get(c);
                    if (it == null)
                        continue;
                    if (!SETT.ROOMS().fData.isMaster.Is(c))
                        continue;

                    COORDINATE ul = SETT.ROOMS().fData.itemX1Y1(c.X(), c.Y(), Coo.TMP);
                    int scx = ul.X() + it.Width() / 2;
                    int scy = ul.Y() + it.Height() / 2;

                    int dx = scx - r.Body().CX();
                    int dy = scy - r.Body().CY();

                    for (int i = 0; i < rot; i++)
                    {
                        int k = dx;
                        dx = -dy;
                        dy = k;
                    }

                    it = it.Group.Item(it.Variation(), (rot + it.Rotation) % it.Group.Rotations());
                    int x = destCX + dx - it.Width() / 2;
                    int y = destCY + dy - it.Height() / 2;

                    SETT.ROOMS().fData.itemSet(x + DeltaX(it, rot), y + DeltaY(it, rot), it, tmp.Room());
                }
            }

            ConstructionInit init = new ConstructionInit(room, r.MX(), r.MY(), false);

            SETT.ROOMS().construction.CreateClean(tmp, init);
        }

        private int DeltaX(FurnisherItem it, int rot)
        {
            if ((it.Width() & 1) == 0)
            {
                if (rot == 1)
                    return 1;
                if (rot == 2)
                    return 1;
            }
            return 0;
        }

        private int DeltaY(FurnisherItem it, int rot)
        {
            if ((it.Height() & 1) == 0)
            {
                if (rot == 2)
                    return 1;
                if (rot == 3)
                    return 1;
            }
            return 0;
        }
    }
}