using System;
using System.IO;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.path.finders
{
    public sealed class SFinderFindableMap
    {
        public const int Quad = 16;
        private const int W = SETT.TWIDTH / Quad;
        private const int H = SETT.THEIGHT / Quad;

        private readonly Bitmap2D day;
        private readonly Bitmap2D tries;
        private readonly Bitmap2D fail;

        public SFinderFindableMap()
        {
            day = new Bitmap2D(W, H, false);
            tries = new Bitmap2D(W, H, false);
            fail = new Bitmap2D(W, H, false);
        }

        public void Report(COORDINATE c, bool success)
        {
            Report(c.x(), c.y(), success);
        }

        public void Report(int tx, int ty, bool success)
        {
            if (!IN_BOUNDS(tx, ty))
                return;

            tx = tx >> 4;
            ty = ty >> 4;

            tries.Set(tx, ty, true);
            day.Set(tx, ty, (TIME.Days().BitsSinceStart() & 1) == 1);
            if (!success)
            {
                fail.Set(tx, ty, true);
            }
        }

        public bool Has(int tx, int ty)
        {
            tx = tx >> 4;
            ty = ty >> 4;
            return tries.Is(tx, ty);
        }

        public bool Fail(int tx, int ty)
        {
            tx = tx >> 4;
            ty = ty >> 4;
            return fail.Is(tx, ty);
        }

        public bool Is(int tx, int ty)
        {
            tx = tx >> 4;
            ty = ty >> 4;
            return !fail.Is(tx, ty);
        }

        private void Update(int i, bool d)
        {
            if (tries.Is(i) && day.Is(i) == d)
            {
                tries.Set(i, false);
                fail.Set(i, false);
            }
        }

        public void Save(FilePutter file)
        {
            day.Save(file);
            fail.Save(file);
            tries.Save(file);
        }

        public void Load(FileGetter file)
        {
            day.Load(file);
            fail.Load(file);
            tries.Load(file);
        }

        public void Clear()
        {
            day.Clear();
            fail.Clear();
            tries.Clear();
        }
    }
}