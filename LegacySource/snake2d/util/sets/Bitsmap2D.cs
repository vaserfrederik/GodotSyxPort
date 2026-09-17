using System;
using System.IO;

namespace snake2d.util.sets
{
    public class Bitsmap2D : MAP_INTE, BODY_HOLDER, SAVABLE
    {
        private readonly Bitsmap1D map;
        private readonly int width;
        private readonly Rec body;

        public Bitsmap2D(int outof, int bits, int width, int height)
        {
            body = new Rec(width, height);
            map = new Bitsmap1D(outof, bits, width * height);
            this.width = width;
        }

        public Bitsmap2D(int outof, int bits, DIMENSION body)
        {
            this.body = new Rec(body.width(), body.height());
            this.width = body.width();
            map = new Bitsmap1D(outof, bits, body.width() * body.height());
        }

        public int get(int tile)
        {
            return map.get(tile);
        }

        public MAP_INTE set(int tile, int value)
        {
            map.set(tile, value);
            return this;
        }

        public MAP_INTE set(int tx, int ty, int value)
        {
            if (body.holdsPoint(tx, ty))
                set(tx + ty * width, value);
            return this;
        }

        public int get(int tx, int ty)
        {
            if (!body.holdsPoint(tx, ty))
                return map.outof;
            return get(tx + ty * width);
        }

        public int max()
        {
            return map.maxValue();
        }

        public void save(FilePutter file)
        {
            map.save(file);
        }

        public void load(FileGetter file)
        {
            map.load(file);
        }

        public void clear()
        {
            map.clear();
        }

        public RECTANGLE body()
        {
            return body;
        }
    }
}