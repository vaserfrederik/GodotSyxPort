using System;

namespace Snake2D.Util.Sprite
{
    public class TextureCoords
    {
        public short x1;
        public short x2;
        public short y1;
        public short y2;

        public static readonly TextureCoords Texture = new TextureCoords();
        public static readonly TextureCoords Normal = new TextureCoords();

        public TextureCoords Get(int x1, int y1, int width, int height)
        {
            this.x1 = (short)x1;
            this.x2 = (short)(x1 + width);
            this.y1 = (short)y1;
            this.y2 = (short)(y1 + height);
            return this;
        }

        public TextureCoords Get(TextureCoords other)
        {
            this.x1 = other.x1;
            this.x2 = other.x2;
            this.y1 = other.y1;
            this.y2 = other.y2;
            return this;
        }

        public int Width()
        {
            return x2 - x1;
        }

        public int Height()
        {
            return y2 - y1;
        }

        public TextureCoords(int x1, int x2, int y1, int y2)
        {
            this.x1 = (short)x1;
            this.y1 = (short)y1;
            this.x2 = (short)x2;
            this.y2 = (short)y2;
        }

        public TextureCoords()
        {
        }

//        public class Imp : TextureCoords, IDimension
//        {
//            public TextureCoords Get(int x1, int y1, int width, int height)
//            {
//                this.x1 = (short)x1;
//                this.x2 = (short)(x1 + width);
//                this.y1 = (short)y1;
//                this.y2 = (short)(y1 + height);
//                return this;
//            }
//
//            public TextureCoords Get(TextureCoords other)
//            {
//                this.x1 = other.x1;
//                this.x2 = other.x2;
//                this.y1 = other.y1;
//                this.y2 = other.y2;
//                return this;
//            }
//
//            public short Y2()
//            {
//                return y2;
//            }
//
//            public short Y1()
//            {
//                return y1;
//            }
//
//            public short X2()
//            {
//                return x2;
//            }
//
//            public short X1()
//            {
//                return x1;
//            }
//
//            public int Width()
//            {
//                return x2 - x1;
//            }
//
//            public int Height()
//            {
//                return y2 - y1;
//            }
//        }
    }
}