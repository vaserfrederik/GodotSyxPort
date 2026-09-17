using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    /**
     * A thing that takes all enemy divs and flood fills them in a fancy way to create blobs surrounding them.
     * @author Jake
     *
     */
    class StepBlob
    {
        private readonly StrategosUtil context;
        private readonly double tileRange = 32;
        private readonly Node[] nodes = new Node[Config.battle().DIVISIONS_PER_ARMY];
        private readonly ArrayList<Node> anodes = new ArrayList<Node>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly Node[][] nmap = new Node[(int)Math.Ceiling(SETT.TWIDTH / tileRange), (int)Math.Ceiling(SETT.THEIGHT / tileRange)];
        private readonly Rec rec = new Rec();

        private readonly Coo[] coos = new Coo[Config.battle().DIVISIONS_PER_ARMY];

        public StepBlob(StrategosUtil context)
        {
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new Node();
            this.context = context;
            for (int c = 0; c < coos.Length; c++)
                coos[c] = new Coo(-16, -16);
        }

        void Update(Bitmap2D blob, int range)
        {

            blob.Clear();

            anodes.ClearSloppy();
            rec.SetDim(1, 1).MoveX1Y1(0, 0);

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {


                Div d = context.GetArmy().enemy().divisions().Get(di);
                if (d.Active())
                {

                    int tx = d.Centre().cUnitX() >> C.T_SCROLL;
                    int ty = d.Centre().cUnitY() >> C.T_SCROLL;
                    Coo c = coos[d.IndexArmy()];
                    if (c.TileDistanceTo(tx, ty) > 10)
                    {
                        c.Set(tx, ty);
                    }


                    Node n = nodes[di];
                    n.next = null;
                    n.coo.Set(c.X(), c.Y());
                    anodes.Add(n);
                }
                else
                {
                    coos[d.IndexArmy()].Set(-C.TILE_SIZE * 16, -C.TILE_SIZE * 16);
                }
            }



            if (anodes.Size() > 0)
            {
                Fill(blob, anodes, range);
            }
        }

        public RECTANGLE Area()
        {
            return rec;
        }

        RECedgeIter iter = new RECedgeIter();

        private void Fill(Bitmap2D blob, LIST<Node> nodes, int range)
        {



            if (context.GetArmy().Men() == 0)
                return;




            for (int y = 0; y < nmap.Length; y++)
            {
                for (int x = 0; x < nmap.Length; x++)
                {
                    nmap[y][x] = null;
                }
            }

            Flooder f = context.Flooder.GetFlooder();
            f.Init(this);

            rec.SetDim(1, 1).MoveX1Y1(nodes.Get(0).coo);

            for (Node n : nodes)
            {
                Add(n, f);
            }

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                if (t.GetValue() > range)
                {
                    f.PushSloppy(t.x(), t.y(), t.GetValue());
                    break;
                }
                blob.Set(t, true);
                rec.Unify(t.x(), t.y());

                if (t.GetValue() < 12)
                {
                    for (int i = 0; i < DIR.ALL.Size(); i++)
                    {
                        int dx = t.x() + DIR.ALL.Get(i).x();
                        int dy = t.y() + DIR.ALL.Get(i).y();
                        if (blob.Body().HoldsPoint(dx, dy) && !SETT.PATH().solidity.Is(dx, dy))
                            if (f.PushSmaller(dx, dy, t.GetValue() + 1) != null)
                                f.SetValue2(dx, dy, t.GetValue2());
                    }
                }
                else
                {
                    for (int i = 0; i < DIR.ORTHO.Size(); i++)
                    {
                        int dx = t.x() + DIR.ORTHO.Get(i).x();
                        int dy = t.y() + DIR.ORTHO.Get(i).y();
                        if (blob.Body().HoldsPoint(dx, dy) && !SETT.PATH().solidity.Is(dx, dy))
                            if (f.PushSmaller(dx, dy, t.GetValue() + 1) != null)
                                f.SetValue2(dx, dy, t.GetValue2());
                    }
                }


            }

            iter.Init(SETT.TILE_BOUNDS);

            foreach (COORDINATE c in iter)
            {
                if (!f.HasBeenPushed(c))
                    f.PushSloppy(c, 0);
            }

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                for (int i = 0; i < DIR.ORTHO.Size(); i++)
                {
                    int dx = t.x() + DIR.ORTHO.Get(i).x();
                    int dy = t.y() + DIR.ORTHO.Get(i).y();
                    if (blob.Body().HoldsPoint(dx, dy) && !f.HasBeenPushed(dx, dy))
                        f.PushSmaller(dx, dy, t.GetValue() + 1);
                }
            }

            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    if (!f.HasBeenPushed(tx, ty))
                        blob.Set(tx, ty, true);
                }
            }


            f.Done();
        }

        private void Add(Node n, Flooder f)
        {

            if (!SETT.IN_BOUNDS(n.coo))
                return;

            f.PushSloppy(n.coo, 0);
            for (int di = 0; di < DIR.ALLC.Size(); di++)
            {
                DIR d = DIR.ALLC.Get(di);
                int x = (int)(n.coo.x() + d.xN() * tileRange);
                int y = (int)(n.coo.y() + d.yN() * tileRange);
                if (SETT.IN_BOUNDS(x, y))
                {
                    int nx = (int)(x / tileRange);
                    int ny = (int)(y / tileRange);
                    Node other = nmap[ny][nx];
                    Add(n, other, f);
                }
            }

            int x = (int)(n.coo.x() / tileRange);
            int y = (int)(n.coo.y() / tileRange);

            n.next = nmap[y][x];
            nmap[y][x] = n;




        }

        private readonly double tileRange2 = tileRange * tileRange;

        private void Add(Node n, Node other, Flooder f)
        {

            while (other != null)
            {

                Node o = other;
                other = other.next;
                double dx = o.coo.x() - n.coo.x();
                double dy = o.coo.y() - n.coo.y();
                if (dx * dx + dy * dy > tileRange2)
                {
                    continue;
                }
                double step = Math.Max(Math.Abs(dx), Math.Abs(dy));
                if (step <= 0)
                    continue;

                dx /= step;
                dy /= step;

                if (TestLine(n, step, dx, dy))
                    AddLine(n, f, step, dx, dy);




            }
        }

        private bool TestLine(Node n, double step, double dx, double dy)
        {
            double x = n.coo.x() + 0.5;
            double y = n.coo.y() + 0.5;
            for (double d = 0; d < step; d++)
            {

                int fx = (int)x;
                int fy = (int)y;
                x += dx;
                y += dy;
                int tx = (int)x;
                int ty = (int)y;
                if (fx == tx && fy == ty)
                    continue;
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                if (SETT.PATH().solidity.Is(tx, ty) || SETT.PATH().solidity.Is(fx, ty) || SETT.PATH().solidity.Is(tx, fy))
                    return false;
            }
            return true;
        }

        private void AddLine(Node n, Flooder f, double step, double dx, double dy)
        {
            double x = n.coo.x() + 0.5;
            double y = n.coo.y() + 0.5;

            for (double d = 0; d < step; d++)
            {
                x += dx;
                y += dy;
                int tx = (int)x;
                int ty = (int)y;
                f.PushSloppy(tx, ty, 0);
            }
        }

        private class Node
        {

            public Node next;
            public readonly Coo coo = new Coo();


            public Node()
            {

            }


        }
    }
}