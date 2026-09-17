using System;
using System.IO;

namespace util.spritecomposer
{
    public class SpriteData
    {
        public readonly int x1, y1, width, height;

        private SpriteData(int x1, int y1, int width, int height)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.width = width;
            this.height = height;
        }

        public static SpriteData Save(int x1, int y1, int width, int height, int ts)
        {
            Resources.P.Mark("sprite");
            Resources.P.WriteInt(x1);
            Resources.P.WriteInt(y1);
            Resources.P.WriteInt(width);
            Resources.P.WriteInt(height);
            Resources.P.WriteInt(ts);
            return new SpriteData(x1, y1, width, height);
        }

        public static SpriteData Read(FileGetter g) throws IOException
        {
            g.Check("sprite");
            int x1 = g.ReadInt();
            int y1 = g.ReadInt();
            int width = g.ReadInt();
            int height = g.ReadInt();
            Optimizer.Tile t = Optimizer.Get(g.ReadInt());
            return new SpriteData(x1, y1 + t.StartY, width, height);
        }
    }
}