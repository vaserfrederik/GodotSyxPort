using game.battle.div;
using game.battle.formation;
using game.battle.thread.general;
using game.battle.thread.general.offence.ContextLines;
using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    class StepLinesBlocker
    {
        private readonly StrategosUtil u;
        private readonly Bitmap2D blob;
        private readonly Bitmap2D block;
        private readonly ContextLines lines;

        public StepLinesBlocker(StrategosUtil u, Context c)
        {
            this.u = u;
            this.blob = c.blob;
            this.lines = c.lines;
            this.block = c.block;
        }

        /**
         * processes the existing lines, and removes the ones blocking each other.
         */
        public void make()
        {
            block.clear();

            Flooder f = u.flooder.getFlooder();
            f.init(this);

            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    f.setValue2(tx, ty, -1);
                }
            }

            for (int li = 0; li < lines.lines(); li++)
            {
                Line l = lines.get(li);
                l.mark = 0;
                int x = l.cx() >> C.T_SCROLL;
                int y = l.cy() >> C.T_SCROLL;
                f.setValue2(x, y, li);
            }

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = u.getArmy().divisions().get(di);
                if (d.active())
                {
                    DivFormation pos = d.position();
                    int x1 = pos.start().x();
                    int y1 = pos.start().x();

                    x1 += pos.dx() * pos.width();
                    y1 += pos.dy() * pos.width();
                    f.pushSloppy(x1 / C.TILE_SIZE, y1 / C.TILE_SIZE, 0);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getValue2() >= 0)
                {
                    Line n = lines.get((int)t.getValue2());
                    if (block(n))
                        n.mark = 1;
                    continue;
                }

                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        f.pushSmaller(t, d, t.getValue() + d.tileDistance() * (blob.is(t) ? 5 : 1));
                    }
                }
            }

            for (int i = 0; i < lines.lines(); i++)
            {
                if (lines.get(i).mark == 0)
                {
                    lines.remove(i);
                    i--;
                }
            }

            f.done();
        }

        private readonly VectorImp vec = new VectorImp();

        private bool block(Line n)
        {
            vec.set(n.dx, n.dy);
            vec.rotate90();
            vec.rotate90();
            vec.rotate90();

            for (int in = -C.TILE_SIZE * 16; in < C.TILE_SIZE * 16; in += C.TILE_SIZE)
            {
                int ra = n.length - C.TILE_SIZE * 2;
                if (in > 0)
                    ra *= 1.0 - in / (double)(C.TILE_SIZE * 16);
                int st = (n.length - ra) / 2;
                ra = n.length - st;

                for (int r = st; r < ra; r += C.TILE_SIZE)
                {
                    int x = (int)(n.sx + vec.nX() * in + r * n.dx);
                    int y = (int)(n.sy + vec.nY() * in + r * n.dy);
                    x /= C.TILE_SIZE;
                    y /= C.TILE_SIZE;

                    if (block.is(x, y))
                        return false;
                }
            }

            for (int in = -C.TILE_SIZE * 16; in < C.TILE_SIZE * 16; in += C.TILE_SIZE)
            {
                int ra = n.length - C.TILE_SIZE * 2;
                if (in > 0)
                    ra *= 1.0 - in / (double)(C.TILE_SIZE * 16);
                int st = (n.length - ra) / 2;
                ra = n.length - st;

                for (int r = st; r < ra; r += C.TILE_SIZE)
                {
                    int x = (int)(n.sx + vec.nX() * in + r * n.dx);
                    int y = (int)(n.sy + vec.nY() * in + r * n.dy);
                    x /= C.TILE_SIZE;
                    y /= C.TILE_SIZE;

                    block.set(x, y, true);
                }
            }
            return true;
        }
    }
}