using System;

namespace Settlement.Room.Home.House
{
    public class SpriteConfig
    {
        private readonly Sprite[][] spri;
        private readonly Getter[] sprites = new Getter[4];

        public SpriteConfig(Sprite[][] sprites)
        {
            this.spri = sprites;
            this.sprites[0] = new Getter(sprites);
            for (int i = 1; i < 4; i++)
            {
                this.sprites[i] = new Getter(Rotate(this.sprites[i - 1].Sp));
            }
        }

        private Sprite[][] Rotate(Sprite[][] l)
        {
            int M = l.Length;
            int N = l[0].Length;
            Sprite[][] ret = new Sprite[N][M];
            for (int r = 0; r < M; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    ret[c][M - 1 - r] = l[r][c];
                }
            }
            return ret;
        }

        private class Getter : MAP_OBJECT<Sprite>
        {
            private readonly Sprite[][] sp;

            public Getter(Sprite[][] sp)
            {
                this.sp = sp;
            }

            public override Sprite Get(int tile)
            {
                // TODO Auto-generated method stub
                return null;
            }

            public override Sprite Get(int tx, int ty)
            {
                if (tx < 0 || tx >= sp[0].Length)
                    return null;
                if (ty < 0 || ty >= sp.Length)
                    return null;
                return sp[ty][tx];
            }
        }

        public MAP_OBJECT<Sprite> Get(int rot)
        {
            return sprites[rot];
        }
    }
}