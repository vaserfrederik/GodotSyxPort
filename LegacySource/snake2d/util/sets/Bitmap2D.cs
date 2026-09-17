using System;
using System.IO;

namespace Snake2D.Util.Sets
{
    public class Bitmap2D : MAP_BOOLEANE, BODY_HOLDER, SAVABLE
    {
        private readonly Bitmap1D data;
        private readonly bool outof;
        private readonly Rec body;
        private readonly int width;

        public Bitmap2D(int width, int height, bool outof)
        {
            this.body = new Rec(width, height);
            this.data = new Bitmap1D(width * height, outof);
            this.width = width;
            this.outof = outof;
        }

        public Bitmap2D(DIMENSION body, bool outof)
        {
            this.body = new Rec(body.Width(), body.Height());
            this.width = body.Width();
            this.data = new Bitmap1D(body.Width() * body.Height(), outof);
            this.outof = outof;
        }

        public bool Is(int tile)
        {
            return data.Get(tile);
        }

        public bool Is(int tx, int ty)
        {
            if (body.HoldsPoint(tx, ty))
                return data.Get(tx + ty * width);
            return outof;
        }

        public MAP_BOOLEANE Set(int tile, bool value)
        {
            data.Set(tile, value);
            return this;
        }

        public MAP_BOOLEANE Set(int tx, int ty, bool value)
        {
            if (body.HoldsPoint(tx, ty))
                Set(tx + ty * width, value);
            return this;
        }

        public RECTANGLE Body()
        {
            return body;
        }

        public void Save(FilePutter file)
        {
            data.Save(file);
        }

        public void Load(FileGetter file)
        {
            data.Load(file);
        }

        public void Clear()
        {
            data.Clear();
        }
    }
}