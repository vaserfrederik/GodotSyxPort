using System;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using snake2d.util.datatypes;

namespace settlement.entity.humanoid.ai.util
{
    public static class AIUtilMoveH
    {
        private static readonly Rec rec = new Rec();
        private static readonly Coo coo = new Coo();

        private AIUtilMoveH()
        {
        }

        public static void Unfuck(Humanoid h)
        {
            ROOMA r = SETT.ROOMS().Map.Rooma.Get(h.Tc());
            if (r != null)
            {
                FurnisherItem it = SETT.ROOMS().FData.Item.Get(h.Tc());
                if (it != null)
                {
                    SETT.ROOMS().FData.ItemX1Y1(h.Tc(), coo);
                    rec.MoveX1Y1(coo.x(), coo.y());
                    rec.SetDim(it.Width(), it.Height());
                    double best = double.MaxValue;
                    int bi = -1;
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        DIR d = DIR.ORTHO.Get(di);
                        if (rec.HoldsPoint(h.Tc(), d))
                        {
                            AVAILABILITY a = SETT.PATH().Availability.Get(h.Tc(), d);
                            if (a.Player > 0)
                            {
                                double v = a.Player + a.From;
                                if (v < best)
                                {
                                    best = v;
                                    bi = di;
                                }
                            }
                        }
                    }
                    if (bi != -1)
                    {
                        Unfuck(h, DIR.ORTHO.Get(bi));
                    }
                }
            }

            double best = double.MaxValue;
            int bi = -1;

            for (int di = 0; di < DIR.ORTHO.Size; di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                AVAILABILITY a = SETT.PATH().Availability.Get(h.Tc(), d);
                if (a != null && a.Player > 0)
                {
                    double v = a.Player + a.From;
                    if (v < best)
                    {
                        best = v;
                        bi = di;
                    }
                }
            }
            if (bi != -1)
            {
                Unfuck(h, DIR.ORTHO.Get(bi));
            }
            for (int di = 0; di < DIR.ORTHO.Size; di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                if (!SETT.PATH().Solidity.Is(h.Tc(), d))
                {
                    Unfuck(h, d);
                    return;
                }
            }
        }

        public static void Unfuck(Humanoid a, DIR dir)
        {
            int x = (a.Tc().x() + dir.x()) * C.TILE_SIZE + C.TILE_SIZEH;
            int y = (a.Tc().y() + dir.y()) * C.TILE_SIZE + C.TILE_SIZEH;
            int dw = (C.TILE_SIZE - a.Physics.Body().Width()) / 2 - 1;
            x += -dir.x() * dw;
            y += -dir.y() * dw;
            a.Physics.Body().MoveC(x, y);
        }

        public static void MoveToTile(Humanoid a, int tx, int ty, DIR dir)
        {
            int x = tx * C.TILE_SIZE + C.TILE_SIZEH;
            int y = ty * C.TILE_SIZE + C.TILE_SIZEH;

            x += dir.x() * (C.TILE_SIZEH - 1);
            y += dir.y() * (C.TILE_SIZEH - 1);

            a.Physics.Body().MoveC(x, y);
            if (dir != DIR.C)
                a.Speed.SetDirCurrent(dir);
        }
    }
}