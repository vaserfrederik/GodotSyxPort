using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Law.Stocks
{
    public class RoomStocks : RoomBlueprintIns<Instance>, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE
    {
        public MConstructor Constructor { get; private set; }
        public Tile Tile { get; private set; }
        public RoomServiceNeed Data { get; private set; }
        public int Used { get; private set; }
        public int Total { get; private set; }

        public RoomStocks(RoomInitData init, RoomCategorySub cat) : base(0, init, "_STOCKS", cat)
        {
            Constructor = new MConstructor(this, init);
            Data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) =>
                {
                    Tile t = Tile.Get(tx, ty);
                    if (t != null)
                    {
                        return t.Service;
                    }
                    return null;
                }
            };
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule()
            {
                Hover = (box, i, rx, ry) =>
                {
                    AREA a = SETT.ROOMS().Map.Rooma.Get(rx, ry);
                    int am = 0;
                    int aa = 0;
                    foreach (COORDINATE c in a.Body())
                    {
                        if (a.Is(c) && Tile.Get(c.X, c.Y) != null)
                        {
                            am++;
                            if (Tile.Get(c.X, c.Y).State == STATE.Available)
                                aa++;
                        }
                    }

                    box.TextLL(Dic.¤¤Available);
                    box.Add(GFORMAT.IofK(box.Text, aa, am));

                    base.Hover(box, i, rx, ry);
                }
            });
        }

        protected override void SaveP(FilePutter f)
        {
            f.I(Used);
            f.I(Total);
        }

        protected override void LoadP(FileGetter f) throws IOException
        {
            Used = f.I();
            Total = f.I();
        }

        protected override void ClearP()
        {
            Used = 0;
            Total = 0;
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return Data.Finder;
        }

        public MConstructor Constructor()
        {
            return Constructor;
        }

        private readonly ROOM_SPECTATOR Activity = new ROOM_SPECTATOR()
        {
            ShouldBoo = (sx, sy) =>
            {
                Tile t = Tile.Get(sx, sy);
                if (t != null && t.State == STATE.Used)
                    return true;
                return false;
            },
            IsActive = (sx, sy) =>
            {
                Tile t = Tile.Get(sx, sy);
                if (t != null && t.State == STATE.Used)
                    return true;
                return false;
            },
            ShouldCheer = (sx, sy) =>
            {
                return false;
            },
            Service = () =>
            {
                return Data;
            }
        };

        public ROOM_SPECTATOR Spec()
        {
            return Activity;
        }

        public DIR StockDir(int tx, int ty, DIR d)
        {
            FurnisherItem it = SETT.ROOMS().FData.Item.Get(tx, ty);
            if (it == null)
                return d;
            if ((GUTIL.Ran2().Get(tx, ty) & 1) == 0)
                return DIR.ORTHO.Get(it.Rotation - 1);
            return DIR.ORTHO.Get(it.Rotation + 1);
        }

        public bool StockIsReserved(int tx, int ty)
        {
            Tile t = Tile.Get(tx, ty);
            if (t != null)
                return t.State == STATE.Reserved || t.State == STATE.Used;
            return Constructor.Service(tx, ty);
        }

        private readonly Coo tmp = new Coo();

        public COORDINATE StockReserve()
        {
            if (Used >= Total)
                return null;

            int am = InstancesSize();
            if (am == 0)
                return null;
            int ri = RND.RInt(am);

            for (int i = 0; i < am; i++)
            {
                Instance ins = GetInstance((ri + i) % am);
                if (ins.Available <= 0)
                    continue;

                foreach (COORDINATE c in ins.Body())
                {
                    Tile t = Tile.Get(c.X, c.Y);
                    if (t != null && t.State == STATE.Available)
                    {
                        t.StateSet(STATE.Reserved);
                        tmp.Set(c);
                        return tmp;
                    }
                    LOG.Err("nono");
                    break;
                }
            }

            for (int i = 0; i < am; i++)
            {
                Instance ins = GetInstance(i);

                foreach (COORDINATE c in ins.Body())
                {
                    Tile t = Tile.Get(c.X, c.Y);
                    if (t != null)
                    {
                        t.StateSet(STATE.None);
                    }
                }

                ins.Available = 0;
            }
            Used = 0;
            Total = 0;

            for (int i = 0; i < am; i++)
            {
                Instance ins = GetInstance(i);

                foreach (COORDINATE c in ins.Body())
                {
                    Tile t = Tile.Get(c.X, c.Y);
                    if (t != null)
                    {
                        t.StateSet(STATE.Available);
                    }
                }
            }

            return null;
        }

        public void StockUse(int tx, int ty)
        {
            Tile t = Tile.Get(tx, ty);
            if (t != null && t.State == STATE.Reserved)
            {
                t.StateSet(STATE.Used);
            }
        }

        public void StockCancel(int tx, int ty)
        {
            Tile t = Tile.Get(tx, ty);
            if (t != null)
            {
                t.StateSet(STATE.Available);
            }
        }

        public RoomServiceNeed Service()
        {
            return Data;
        }

        public int PunishTotal()
        {
            return Total;
        }

        public int PunishUsed()
        {
            return Used;
        }
    }
}