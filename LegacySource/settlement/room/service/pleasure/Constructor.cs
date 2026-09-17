using System;
using System.Collections.Generic;
using System.IO;
using Init.Constant;
using Init.Sprite;
using Settlement.Main;
using Settlement.Path;
using Settlement.Room.Main;
using Settlement.Room.Main.Furnisher;
using Settlement.Room.Sprite;
using Settlement.Tilemap.Floor;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Rnd;
using Util.GUtil;
using Util.Rendering;
using View.Main;

namespace Settlement.Room.Service.Pleasure
{
    internal class Constructor : Furnisher
    {
        private readonly ROOM_PLEASURE blue;

        public readonly FurnisherStat beds = new FurnisherStat.FurnisherStatI(this, 1);
        public readonly FurnisherStat coziness = new FurnisherStat.FurnisherStatRelative(this, beds);
        public readonly FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
        private readonly FurnisherItemTile ww;
        private readonly Floor floor2;

        private static readonly int IIN = 1;
        public static readonly int ISERVICE = 2;
        private static readonly int IWALL = 3;
        private readonly RoomSpriteCombo walls;

        FurnisherItemGroup mgroup;

        private readonly COLOR[] pixCols = new COLOR[256];

        protected Constructor(ROOM_PLEASURE blue, RoomInitData init) : base(init, 3, 3, 88, 44)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().Map.Get(init.Data().Value("FLOOR2"), init.Data());

            Json sp = init.Data().Json("SPRITES");

            {
                COLOR pixCol = new ColorImp(init.Data(), "COLOR_PIXEL_BASE");

                for (int i = 0; i < pixCols.Length; i++)
                {
                    int r = pixCol.Red();
                    int g = pixCol.Green();
                    int b = pixCol.Blue();
                    double hue = 0.25 + 0.5 * RND.rFloat();

                    r = (int)(hue * r + RND.rFloat() * 5);
                    g = (int)(hue * g + RND.rFloat() * 5);
                    b = (int)(hue * b + RND.rFloat() * 5);
                    pixCols[i] = new ColorImp(r, g, b);
                }
            }

            RoomSprite sHead = new RoomSprite1x1(sp.Get("BED_HEAD"));
            RoomSprite sTail = new RoomSprite1x1(sp.Get("BED_TAIL"));
            RoomSprite stop = new RoomSprite1x1(sp.Get("STOP"));
            RoomSprite sWall = new RoomSprite1x1(sp.Get("WALL"));
            RoomSprite sCarpet = new RoomSprite1x1(sp.Get("CARPET"));

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail) }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail) }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail) }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail), new FurnisherItemTile(sHead), new FurnisherItemTile(stop), new FurnisherItemTile(sTail) }
            }, 4);

            mgroup = Flush(1, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sCarpet) }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall) }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall) }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall) }
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall) }
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall) }
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall) }
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { new FurnisherItemTile(sWall), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sCarpet), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall), new FurnisherItemTile(sWall) }
            }, 8);

            Flush(3);

            FurnisherItemTools.MakeUnder(this, sp, "CARPET_COMBO");
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public void AboveR(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade)
        {
            if (SETT.ROOMS().fData.tileData.Get(it.Tile()) > 0)
            {
                bool blur = blue.bed.Init(it.Tx(), it.Ty()) != null && blue.bed.ClientUndressed.Get() == 1;
                int m = 0;
                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.ROOMS().fData.tile.Get(it.Tx(), it.Ty(), d) == ww)
                    {
                        if (!d.IsOrtho())
                        {
                            m |= d.Next(-1).Mask();
                            m |= d.Next(1).Mask();
                        }
                        else
                        {
                            m |= d.Mask();
                            m |= d.Next(-2).Mask();
                            m |= d.Next(2).Mask();
                        }
                        if (!blur)
                        {
                            ABed b = blue.bed.Init(it.Tx() + d.X(), it.Ty() + d.Y());
                            if (b != null && b.ClientUndressed.Get() == 1)
                                blur = true;
                        }
                    }
                }

                double rs = VIEW.renderSecond();

                if (blur)
                {
                    long ran = GUTIL.ran2().Get(it.Tile());
                    ran = ran << 32;
                    ran |= GUTIL.ran1().Get(it.Tile());
                    int D = C.TILE_SIZEH / 2;
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            int ci = (int)((ran & 0x0FF) + (rs * 10));
                            pixCols[ci % 0x0FF].Render(r, it.X() + D * x, it.X() + D * x + D,
                                it.Y() + D * y, it.Y() + D * y + D);
                            ran = ran >> 4;
                        }
                    }
                }

                if (SETT.ROOMS().fData.tileData.Get(it.Tile()) > IIN)
                {
                    if (m != 0 && m != 0x0F)
                    {
                        walls.Render(r, s, m, it, degrade, false);
                    }
                }
            }
        }

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            FurnisherItem t = SETT.ROOMS().fData.item.Get(tx, ty);
            if (t != null && t.Group() == mgroup)
                floor2.PlaceFixed(tx, ty);
            else
                base.PutFloor(tx, ty, upgrade, area);
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new PleasureInstance(blue, area, init);
        }

        public override bool IsHeavy()
        {
            return true;
        }
    }
}