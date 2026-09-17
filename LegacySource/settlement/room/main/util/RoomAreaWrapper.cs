using settlement.room.main;
using settlement.main;
using snake2d.util.datatypes;
using System;

namespace settlement.room.main.util
{
    public class RoomAreaWrapper
    {
        private Rec body = new Rec();
        private Room room;
        private int mx, my;
        private int area;
        private bool used;

        public RoomAreaWrapper()
        {
        }

        public ROOMA init(Room r, int x, int y)
        {
            use();
            if (r is ROOMA)
                return (ROOMA)r;
            if (room != r || !a.is(x, y) || true)
            {
                room = r;
                mx = r.mX(x, y);
                my = r.mY(x, y);
                body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
                body.setDim(r.width(mx, my), r.height(mx, my));
                area = r.area(mx, my);
            }
            return a;
        }

        private void use()
        {
            if (used)
            {
                throw new RuntimeException();
            }
            used = true;
        }

        public bool changedAndInit(Room r, int x, int y)
        {
            use();
            if (room != r || !a.is(x, y))
            {
                room = r;
                mx = r.mX(x, y);
                my = r.mY(x, y);
                body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
                body.setDim(r.width(mx, my), r.height(mx, my));
                area = r.area(mx, my);
                return true;
            }
            return false;
        }

        public ROOMA init(Room r, COORDINATE c)
        {
            return init(r, c.x(), c.y());
        }

        public ROOMA area()
        {
            return a;
        }

        public void done()
        {
            used = false;
        }

        private ROOMA a = new ROOMA()
        {
            public RECTANGLE body()
            {
                return body;
            }

            public bool is(int tile)
            {
                return is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }

            public bool is(int tx, int ty)
            {
                return room != null && SETT.ROOMS().map.get(tx, ty) == room && room.isSame(mx, my, tx, ty);
            }

            public int area()
            {
                return area;
            }

            public int index()
            {
                return room.index();
            }

            public int mX()
            {
                return mx;
            }

            public int mY()
            {
                return my;
            }
        };

        public void clear()
        {
            room = null;
        }

        public static class RoomWrap : ROOMA
        {
            private Rec body = new Rec();
            private Room room;
            private int mx, my;
            private int area;

            public bool init(Room r, int x, int y)
            {
                if (room != r || !is(x, y))
                {
                    room = r;
                    mx = r.mX(x, y);
                    my = r.mY(x, y);
                    body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
                    body.setDim(r.width(mx, my), r.height(mx, my));
                    area = r.area(mx, my);
                    return true;
                }
                return false;
            }

            public RECTANGLE body()
            {
                return body;
            }

            public bool is(int tile)
            {
                return is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }

            public bool is(int tx, int ty)
            {
                return SETT.ROOMS().map.get(tx, ty) == room && room.isSame(mx, my, tx, ty);
            }

            public int area()
            {
                return area;
            }

            public int index()
            {
                return room.index();
            }

            public int mX()
            {
                return mx;
            }

            public int mY()
            {
                return my;
            }
        }
    }
}