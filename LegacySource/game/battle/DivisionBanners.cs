using System;
using System.IO;
using System.Collections.Generic;

namespace Game.Battle
{
    public class DivisionBanners : SAVABLE
    {
        private readonly DivisionBanner[] all;

        public DivisionBanners()
        {
            Bitmap2D[] data = BitmapSprite.Read(PATHS.SPRITE_UI().Get("DivisionSymbols"));
            all = new DivisionBanner[data.Length];
            COLOR[] cols = COLOR.GenerateUnique(40, data.Length, true);
            for (int i = 0; i < data.Length; i++)
            {
                DivisionBanner d = new DivisionBanner(new BitmapSprite());
                d.sprite.Paint(data[i]);
                d.col.Set(cols[i]);
                all[i] = d;
            }
        }

        public DivisionBanner Get(int index)
        {
            index = index % all.Length;
            if (index < 0)
                index += all.Length;
            return all[index];
        }

        public override void Save(FilePutter file)
        {
            file.I(all.Length);
            foreach (DivisionBanner d in all)
            {
                d.sprite.Save(file);
                d.col.Save(file);
                d.bg.Save(file);
            }
        }

        public override void Load(FileGetter file)
        {
            int am = file.I();
            for (int i = 0; i < am; i++)
            {
                Get(i).sprite.Load(file);
                Get(i).col.Load(file);
                Get(i).bg.Load(file);
            }
        }

        public override void Clear()
        {
            // TODO Auto-generated method stub
        }

        public class DivisionBanner : SPRITE
        {
            public readonly BitmapSprite sprite;
            public readonly ColorImp col = new ColorImp();
            public readonly ColorImp bg = new ColorImp(20, 20, 20);
            private readonly int m = 2;

            public DivisionBanner(BitmapSprite sprite)
            {
                this.sprite = sprite;
            }

            public override int Width()
            {
                return BitmapSprite.WIDTH * 2 + m * 2;
            }

            public override int Height()
            {
                return BitmapSprite.HEIGHT * 2 + m * 2;
            }

            public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
            }

            public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                int s = (X2 - X1) / Width();
                if (s < 1)
                    s = 1;

                RenderSymbol(r, X1 + m, Y1 + m, s);
            }

            public void RenderSymbol(SPRITE_RENDERER r, int X1, int Y1, int scale)
            {
                for (int y = -1; y <= BitmapSprite.WIDTH * 2 + 2; y++)
                {
                    for (int x = -1; x <= BitmapSprite.HEIGHT * 2 + 2; x++)
                    {
                        int dx = (x - 1) / 2;
                        int dy = (y - 1) / 2;

                        if (sprite.Is(dx, dy))
                        {
                            COLOR c = col;
                            foreach (DIR d in DIR.ALL)
                            {
                                int ddx = (x - 1 + d.X()) / 2;
                                int ddy = (y - 1 + d.Y()) / 2;
                                if (!sprite.Is(ddx, ddy))
                                {
                                    c = ColorImp.TMP.Set(c).ShadeSelf(0.6);
                                    break;
                                }
                            }
                            c.Render(r, X1 + x * scale, X1 + x * scale + scale, Y1 + y * scale, Y1 + y * scale + scale);
                        }
                        else
                        {
                            foreach (DIR d in DIR.ALL)
                            {
                                int ddx = (x - 1 + d.X()) / 2;
                                int ddy = (y - 1 + d.Y()) / 2;
                                if (sprite.Is(ddx, ddy))
                                {
                                    bg.Render(r, X1 + x * scale, X1 + x * scale + scale, Y1 + y * scale, Y1 + y * scale + scale);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public int Size()
        {
            return all.Length;
        }
    }
}