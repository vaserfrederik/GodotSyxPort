using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Infra.Stockpile
{
    public class Organiser
    {
        private Bitmap1D check;
        private int[] amounts;
        private int[] xs;
        private int[] ys;

        public Organiser(ROOM_STOCKPILE b)
        {
            check = new Bitmap1D(RESOURCES.ALL().Count, false);
            amounts = new int[RESOURCES.ALL().Count];
            xs = new int[RESOURCES.ALL().Count];
            ys = new int[RESOURCES.ALL().Count];
        }

        public MoveJob Organise(StockpileInstance ins, int am)
        {
            check.Clear();

            for (int i = 0; i < ins.crates.Count; i++)
            {
                ins.crates.Set(i);
                TILE_STORAGE s = ins.Crate(ins.crates.Get().X, ins.crates.Get().Y);
                if (s != null && s.Resource != null && s.StorageReservable > 0)
                {
                    int ri = s.Resource.Index;
                    if (!check.Get(ri) || s.StorageReservable < amounts[ri])
                    {
                        check.Set(ri, true);
                        amounts[ri] = s.StorageReservable;
                        xs[ri] = s.X;
                        ys[ri] = s.Y;
                    }
                }
            }

            for (int i = 0; i < ins.crates.Count; i++)
            {
                ins.crates.Set(i);
                StorageCrate s = ins.Crate(ins.crates.Get().X, ins.crates.Get().Y);
                if (s != null && s.Resource != null && check.Get(s.Resource.Index) && s.Reservable > 0)
                {
                    int ri = s.Resource.Index;
                    if (s.X == xs[ri] && s.Y == ys[ri])
                        continue;
                    if (s.StorageReservable < amounts[ri])
                        continue;
                    MoveJob j = MoveJob.TMP;
                    am = Math.Min(am, s.Reservable);
                    am = Math.Min(am, amounts[ri]);
                    j.MaxAm = am;
                    j.Res = s.Resource;
                    j.Stored = true;
                    j.Prio = ins.Prioritizing();
                    j.Source.Set(s);
                    j.Dest.Set(xs[ri], ys[ri]);
                    return j;
                }
            }

            return null;
        }
    }
}