using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.Thread.General.Offence
{
    class StepLinesBacker
    {
        private readonly StrategosUtil u;
        private readonly ContextLines lines;
        private readonly Bitmap2D blob;
        private readonly Bitmap2D block;
        private readonly IntImp lineI;
        private readonly Context c;
        private readonly double[] dists;

        private readonly Rec bounds = new Rec(SETT.TWIDTH - 80, SETT.THEIGHT - 80);
        private readonly VectorImp vec;

        public StepLinesBacker(StrategosUtil u, Context c)
        {
            this.u = u;
            this.lines = c.lines;
            this.blob = c.blob;
            lineI = c.checkI;
            block = c.block;
            this.c = c;
            dists = new double[Config.battle().DIVISIONS_PER_ARMY];
            bounds.CenterIn(SETT.TILE_BOUNDS);
            vec = new VectorImp();
        }

        public void Init()
        {
            lineI.Set(0);
            block.Clear();
            Flooder f = u.flooder.GetFlooder();
            f.Init(this);

            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    f.SetValue2(tx, ty, 0);
                }
            }

            c.map.Clear();

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = u.GetArmy().divisions().Get(di);
                if (d.active())
                {
                    c.map.Add(d);
                }
            }

            for (int l = 0; l < lines.Lines(); l++)
            {
                Line li = lines.Get(l);

                int tx = li.Cx() / C.TILE_SIZE;
                int ty = li.Cy() / C.TILE_SIZE;

                f.PushSloppy(tx, ty, 0);
                f.SetValue2(tx, ty, li.BlobID);
            }

            Array.Fill(dists, double.MaxValue);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                if (t.Parent != null)
                    t.SetValue2(t.Parent.GetValue2());

                if (c.map.Get(t.X(), t.Y()).Count > 0)
                {
                    double v = t.GetValue();
                    int bi = (int)t.GetValue2();
                    if (v < dists[bi])
                    {
                        dists[bi] = v;
                    }
                }

                bool b = blob.Is(t);

                for (int di = 0; di < DIR.ALL.Size(); di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    int dx = t.X() + d.X();
                    int dy = t.Y() + d.Y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;
                    if (!DivPlacability.TileIsOK(dx, dy, u.GetArmy()))
                        continue;
                    double v = d.TileDistance() * (!b && blob.Is(dx, dy) ? 5 : 1);
                    f.PushSmaller(dx, dy, v + t.GetValue(), t);
                }
            }

            f.Done();

            bool shouldShill = ShouldChill();
            for (int i = 0; i < dists.Length; i++)
            {
                double closest = dists[i];
                if (closest == double.MaxValue)
                {
                    dists[i] = 0;
                    continue;
                }

                if (!shouldShill)
                    closest -= 32;

                closest = (int)closest / 48;
                closest *= 48;

                if (closest < 2)
                    closest = 2;
                if (closest > 256)
                    closest = 256;
                closest *= C.TILE_SIZE;
                dists[i] = closest;
            }
            for (int l = 0; l < lines.Lines(); l++)
            {
                Line li = lines.Get(l);
                li.BlobID = (int)dists[li.BlobID];
            }
        }

        private readonly List<ArtilleryInstance> ins = new List<ArtilleryInstance>(256);

        private bool ShouldChill()
        {
            double aa = GAME.ARMIES().factors.casulties(GAME.ARMIES().enemy());
            double ab = GAME.ARMIES().factors.casulties(GAME.ARMIES().player());

            if (aa / (GAME.ARMIES().enemy().Men() + 1.0) > 0.05 && aa > ab * 0.75)
                return false;

            double pow = 0;
            double epow = 0;

            for (int i = 0; i < Config.battle().DIVISIONS_PER_ARMY; i++)
            {
                pow += u.GetArmy().divisions().Get(i).settings().GetPower();
            }
            for (int i = 0; i < Config.battle().DIVISIONS_PER_ARMY; i++)
            {
                epow += u.GetArmy().enemy().divisions().Get(i).settings().GetPower();
            }

            if (pow > epow * 1.5)
                return false;

            int ally = 0;
            int enemy = 0;

            ins.Clear();
            for (int ai = 0; ai < SETT.ROOMS().ARTILLERY.size(); ai++)
            {
                SETT.ROOMS().ARTILLERY.get(ai).threadInstances(ins);
            }
            for (int ii = 0; ii < ins.Count; ii++)
            {
                ArtilleryInstance i = ins[ii];
                if (i.isFiring())
                {
                    if (i.army() == u.GetArmy())
                        ally++;
                    else
                        enemy++;
                }
            }

            if (enemy > ally)
                return false;

            return ally > 0;
        }

        public bool RetreatThroneLine()
        {
            if (lineI.Get() >= lines.Lines())
                return false;

            Line l = lines.Get(lineI.Get());

            double length = GetRetreat(l, l.BlobID);

            if (length < 0)
            {
                lines.Remove(lineI.Get());
                return true;
            }

            Block(l, length);

            lineI.Inc(1);

            vec.Set(l.dx, l.dy);
            vec.Rotate90();

            l.sx += vec.nX() * length;
            l.sy += vec.nY() * length;

            return true;
        }

        private double GetRetreat(Line n, double target)
        {
            vec.Set(n.dx, n.dy);
            vec.Rotate90();

            for (int inVal = C.TILE_SIZE; inVal <= target; inVal += C.TILE_SIZE)
            {
                for (int r = 0; r <= n.length; r += C.TILE_SIZE)
                {
                    int x = (int)(n.sx + vec.nX() * inVal + r * n.dx);
                    int y = (int)(n.sy + vec.nY() * inVal + r * n.dy);
                    x /= C.TILE_SIZE;
                    y /= C.TILE_SIZE;

                    if (!SETT.IN_BOUNDS(x, y))
                        return inVal - C.TILE_SIZE * 8;

                    if (this.block.Is(x, y))
                    {
                        return inVal - C.TILE_SIZE * 8;
                    }
                    if (Solid(x, y))
                        return inVal - C.TILE_SIZE;
                }
            }

            double awayFromBlobDist = target;

            int cx = n.cx();
            int cy = n.cy();
            int death = 0;
            while (true)
            {
                int x = (int)(cx + vec.nX() * awayFromBlobDist) / C.TILE_SIZE;
                int y = (int)(cy + vec.nY() * awayFromBlobDist) / C.TILE_SIZE;
                if (!bounds.HoldsPoint(x, y))
                    return awayFromBlobDist;
                if (!DivPlacability.TileIsOK(x, y, u.GetArmy()))
                    return awayFromBlobDist;
                awayFromBlobDist += C.TILE_SIZE;
            }
        }

        private void Block(Line n, double awayFromBlobDist)
        {
            vec.Set(n.dx, n.dy);
            vec.Rotate90();

            for (int inVal = C.TILE_SIZE; inVal <= awayFromBlobDist; inVal += C.TILE_SIZE)
            {
                for (int r = 0; r <= n.length; r += C.TILE_SIZE)
                {
                    int x = (int)(n.sx + vec.nX() * inVal + r * n.dx);
                    int y = (int)(n.sy + vec.nY() * inVal + r * n.dy);
                    x /= C.TILE_SIZE;
                    y /= C.TILE_SIZE;
                    this.block.Set(x, y, true);
                }
            }
        }

        public bool Solid(double dx, double dy)
        {
            int tx = (int)dx;
            int ty = (int)dy;
            if (!bounds.HoldsPoint(tx, ty))
                return true;
            if (!DivPlacability.TileIsOK(tx, ty, u.GetArmy()))
                return true;
            return false;
        }
    }
}