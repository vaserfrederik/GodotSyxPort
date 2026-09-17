using System;
using System.Collections.Generic;

namespace Settlement.Path.Finders
{
    public class RadiusChecker
    {
        public static RadiusChecker self = new RadiusChecker();

        private byte ri = 0;
        private int[] levelStarts;
        private int[] sizes;
        private byte[] ids;

        public void Check(int sx, int sy, int radius)
        {
            if (NeedsSizeFix())
            {
                levelStarts = new int[SETT.PATH().comps.all.Count];
                sizes = new int[SETT.PATH().comps.all.Count];
                int start = 0;
                for (int l = 0; l < levelStarts.Length; l++)
                {
                    int size = SETT.PATH().comps.all[l].componentsMax();
                    sizes[l] = size;

                    levelStarts[l] = start;
                    start += size;
                }

                ids = new byte[start + 64];
                ri = 0;
            }
            else if (ri == -1)
            {
                Array.Fill(ids, (byte)0);
                ri = 0;
            }

            ri++;

            if (SETT.PATH().comps.zero.Get(sx, sy) == null)
            {
                GAME.Notify(sx + " " + sy);
                return;
            }

            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy(sx, sy, 0);

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                Check(t);
                if (t.GetValue() > radius)
                    break;

                SComponent c = SETT.PATH().comps.zero.Get(t);
                SComponentEdge e = c.EdgeFirst();
                while (e != null)
                {
                    GUTIL.Flooder().PushSmaller(e.To().centreX(), e.To().centreY(), t.GetValue() + e.Distance());
                    e = e.Next();
                }
            }
            ri--;
            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                Check(t);
            }
            ri++;
            GUTIL.Flooder().Done();
        }

        private void Check(COORDINATE c)
        {
            SComponent start = SETT.PATH().comps.zero.Get(c);
            int s = 0;
            while (start != null)
            {
                int id = start.Index() + levelStarts[s];
                if (id < ids.Length)
                    ids[id] = ri;
                start = start.SuperComp();
                s++;
            }
        }

        public bool Is(SComponent c)
        {
            int id = c.Index() + levelStarts[c.Level().Level()];
            if (id >= ids.Length)
                return false;
            return ids[id] == ri;
        }

        public bool Is(int tx, int ty)
        {
            SComponent c = SETT.PATH().comps.zero.Get(tx, ty);
            if (c == null)
                return false;
            int id = c.Index() + levelStarts[c.Level().Level()];
            if (id >= ids.Length)
                return false;
            return ids[id] == ri;
        }

        private bool NeedsSizeFix()
        {
            if (levelStarts == null)
                return true;

            int start = 0;

            for (int l = 0; l < sizes.Length; l++)
            {
                start += SETT.PATH().comps.all[l].componentsMax();
            }

            return start >= ids.Length;
        }
    }
}