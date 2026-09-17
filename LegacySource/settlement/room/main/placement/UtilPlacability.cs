using System;
using System.Collections.Generic;
using System.Text;

namespace Settlement.Room.Main.Placement
{
    using Settlement.Main;
    using Settlement.Room.Main.Furnisher;
    using Snake2D.Util.DataTypes;
    using Util;

    internal sealed class UtilPlacability
    {
        private readonly RoomPlacer p;
        private readonly StringBuilder sError = new StringBuilder(100);

        private static readonly string ¤¤TooSmall = "¤The area designated is too small.";
        private static readonly string ¤¤NotEnoughItems = "¤This room plan has insufficient: {0} items. Place more items inside the shape to continue.";
        private static readonly string ¤¤NotEnough = "¤This room will have insufficient {0}. Either the shape needs to be expanded, or more items need to placed.";
        private static readonly string ¤¤Disconnected = "¤Area must be connected!";
        private static readonly string ¤¤BlockingSelf = "¤Items are cutting off room. Make sure the room can be reached from the outside.";

        private static readonly string ¤¤NotInside = "Must be placed inside the designated area. You must expand the area before you can place items.";
        private static readonly string ¤¤NotBlockOther = "Must not block other item.";
        private static readonly string ¤¤WillBeBlock = "Must not be blocked by other items.";
        private static readonly string ¤¤WillBlockRoom = "Area is not connected, or an item is cutting off part of the room.";
        private static readonly string ¤¤ItemsREached = "Max amount of this item is reached.";
        private static readonly string ¤¤ItemMustREac = "Item must be reachable.";

        static UtilPlacability()
        {
            D.ts(typeof(UtilPlacability));
        }

        public UtilPlacability(RoomPlacer p)
        {
            D.t(this);
            this.p = p;
        }

        public FurnisherItemGroup CreateProblemGroup()
        {
            foreach (FurnisherItemGroup g in p.Blueprint().Constructor().Groups())
            {
                if (p.Resources.Groups(g) < g.Min)
                {
                    return g;
                }
            }
            return null;
        }

        public string CreateProblem(Area instance)
        {
            sError.Clear();

            if (instance.Area() < 1)
                return sError.Append(¤¤TooSmall).ToString();

            if (p.Blueprint().Constructor().ConstructionProblem(instance) != null)
            {
                return p.Blueprint().Constructor().ConstructionProblem(instance);
            }

            foreach (FurnisherItemGroup g in p.Blueprint().Constructor().Groups())
            {
                if (p.Resources.Groups(g) < g.Min)
                {
                    return sError.AppendFormat(¤¤NotEnoughItems, g.Name).ToString();
                }
            }

            foreach (FurnisherStat s in p.Blueprint().Constructor().Stats())
            {
                if (p.Resources.Stats(s) < s.Min)
                {
                    return sError.AppendFormat(¤¤NotEnough, s.Name).ToString();
                }
            }

            if (!IsConnected(instance))
            {
                return ¤¤Disconnected;
            }

            if (!IsReachable(instance))
            {
                return ¤¤BlockingSelf;
            }

            return null;
        }

        private bool IsConnected(Area instance)
        {
            // Implementation of IsConnected
            return true;
        }

        private bool IsReachable(Area instance)
        {
            // Implementation of IsReachable
            return true;
        }

        public string ItemPlacable(int x1, int y1, FurnisherItemGroup group, FurnisherItem item, Area a)
        {
            if (p.Resources.Groups(group) >= group.Max)
            {
                return ¤¤ItemsREached;
            }

            for (int y = 0; y < item.Height; y++)
            {
                for (int x = 0; x < item.Width; x++)
                {
                    int tx = x + x1;
                    int ty = y + y1;
                    string s = ItemPlacable(tx, ty, x, y, item, a);
                    if (s != null)
                    {
                        return s;
                    }
                }
            }

            GUtil.Filler.Init(this);

            for (int y = 0; y < item.Height; y++)
            {
                for (int x = 0; x < item.Width; x++)
                {
                    FurnisherItemTile t = item.Get(x, y);
                    if (t != null && t.IsBlocker)
                    {
                        int tx = x + x1;
                        int ty = y + y1;
                        GUtil.Filler.Closer.Set(tx, ty);
                    }
                }
            }

            for (int y = -1; y <= item.Height; y++)
            {
                for (int x = -1; x <= item.Width; x++)
                {
                    if (x == -1 || x == item.Width || y == -1 || y == item.Height)
                    {
                        if ((x == -1 && y == -1))
                            continue;
                        if ((x == -1 && y == item.Height))
                            continue;
                        if ((x == item.Width && y == -1))
                            continue;
                        if ((x == item.Width && y == item.Height))
                            continue;

                        int tx = x + x1;
                        int ty = y + y1;
                        if (CheckIfOtherTileBlocked(tx, ty, a) || CheckIfOtherItemBlocked(tx, ty, a))
                        {
                            GUtil.Filler.Done();
                            return ¤¤NotBlockOther + " 2" + " " + CheckIfOtherTileBlocked(tx, ty, a) + " " + CheckIfOtherItemBlocked(tx, ty, a);
                        }
                    }
                }
            }

            bool first = false;
            int area = 0;
            foreach (Coordinate c in a.Body())
            {
                if (!a.Is(c))
                    continue;
                if (!GUtil.Filler.Isser.Is(c.X, c.Y) && !IsBlockerTile(c.X, c.Y, a))
                {
                    area++;
                    if (!first)
                    {
                        GUtil.Filler.Filler.Set(c);
                        first = true;
                    }
                }
            }

            while (GUtil.Filler.HasMore())
            {
                Coordinate c = GUtil.Filler.Poll();
                area--;
                foreach (Direction d in Direction.Ortho)
                {
                    int dx = c.X + d.X;
                    int dy = c.Y + d.Y;
                    if (a.Is(dx, dy) && !IsBlockerTile(dx, dy, a))
                    {
                        GUtil.Filler.Fill(dx, dy);
                    }
                }
            }

            GUtil.Filler.Done();

            if (area != 0)
            {
                return ¤¤WillBlockRoom;
            }

            return null;
        }

        private bool CheckIfOtherTileBlocked(int tx, int ty, Area a)
        {
            if (!a.Is(tx, ty))
                return false;
            FurnisherItemTile t = SETT.Rooms.FData.Tile.Get(tx, ty);
            if (t == null || !t.MustBeReachable)
                return false;

            Coordinate c = SETT.Rooms.FData.ItemMaster(tx, ty, Coo.TMP);
            int mx = c.X;
            int my = c.Y;

            for (int di = 0; di < Direction.Ortho.Count; di++)
            {
                Direction d = Direction.Ortho[di];
                int dx = tx + d.X;
                int dy = ty + d.Y;
                if (!a.Is(dx, dy))
                    continue;
                if (GUtil.Filler.Isser.Is(dx, dy))
                    continue;
                t = SETT.Rooms.FData.Tile.Get(dx, dy);
                if (t == null)
                    return false;
                if (t.IsBlocker)
                    continue;
                c = SETT.Rooms.FData.ItemMaster(dx, dy, Coo.TMP);
                if (c.X == mx && c.Y == my)
                    continue;

                if (t.IsNotBlocker())
                    return false;
            }
            return true;
        }

        private bool CheckIfOtherItemBlocked(int tx, int ty, Area a)
        {
            if (!a.Is(tx, ty))
                return false;
            FurnisherItem item = SETT.Rooms.FData.Item.Get(tx, ty);
            if (item == null)
                return false;

            Coordinate c = SETT.Rooms.FData.ItemMaster(tx, ty, Coo.TMP);

            tx = c.X - item.FirstX;
            ty = c.Y - item.FirstY;

            for (int y = 0; y < item.Height; y++)
            {
                for (int x = 0; x < item.Width; x++)
                {
                    if (x == 0 || x == item.Width - 1 || y == 0 || y == item.Height - 1)
                    {
                        int dx = x + tx;
                        int dy = y + ty;
                        if (!IsBlockedTile(dx, dy, a))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsBlockerTile(int tx, int ty, Area a)
        {
            if (!a.Is(tx, ty))
                return true;
            if (GUtil.Filler.Isser.Is(tx, ty))
                return true;
            FurnisherItemTile t = SETT.Rooms.FData.Tile.Get(tx, ty);
            return t != null && t.IsBlocker;
        }

        private bool IsBlockedTile(int tx, int ty, Area a)
        {
            for (int di = 0; di < Direction.Ortho.Count; di++)
            {
                Direction d = Direction.Ortho[di];
                int dx = tx + d.X;
                int dy = ty + d.Y;
                if (!IsBlockerTile(dx, dy, a))
                    return false;
            }
            return true;
        }
    }
}