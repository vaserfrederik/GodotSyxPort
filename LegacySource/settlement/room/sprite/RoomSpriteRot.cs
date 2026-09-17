using System;
using System.Collections.Generic;

namespace Settlement.Room.Sprite
{
    using Init.Constant;
    using Settlement.Room.Main.Furnisher;
    using Snake2D;
    using Util.Rendering;

    public class RoomSpriteRot : RoomSprite.Imp
    {
        private const int ROT = 4;

        public readonly int TileEnd;
        public readonly int TileStart;
        //private readonly TILE_SHEET sheet;
        private readonly LIST<SPRITE> blue;

        private readonly TILE_SHEET sheet;

        public RoomSpriteRot(TILE_SHEET sheet, int startTile, int variations, LIST<SPRITE> blueprint)
        {
            this.sheet = sheet;
            this.blue = blueprint;
            this.TileEnd = startTile + variations * ROT;
            this.TileStart = startTile;
            shadowDist = 3;
            shadowHeight = 0;
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int x = it.X();
            int y = it.Y();
            return Render(r, s, data, x, y, it.Ran(), it, degrade, isCandle);
        }

        public bool RenderRandom(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade, bool off)
        {
            int x = it.X();
            int y = it.Y();
            int ran = it.Ran() & int.MaxValue;
            if (off)
            {
                x += -C.SCALE * 2 + (ran & int.MaxValue) % (C.SCALE * 4);
                ran = ran >> 4;
                y += -C.SCALE * 2 + (ran & int.MaxValue) % (C.SCALE * 4);
                ran = ran >> 4;
            }

            int data = ran & 0b011;
            ran = ran >> 2;

            return Render(r, s, data, x, y, ran, it, degrade, false);
        }

        protected bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, int x, int y, int ran, RenderIterator it, double degrade, bool isCandle)
        {
            ran &= int.MaxValue;
            int variations = (TileEnd - TileStart) / 4;
            int tile = (ran % variations);
            tile += (data & 0b011) * variations;
            tile += TileStart;
            tile += GetTileOffset(it, data);
            sheet.Render(r, tile, x, y);
            ran = ran >> 4;
            RenderDegrade(sheet, r, tile, it, degrade);

            if (s != null && (shadowHeight > 0 || shadowDist > 0))
            {
                s.SetDistance2Ground(shadowHeight).SetHeight(shadowDist);
                sheet.Render(s, tile, x, y);
            }

            return false;
        }

        protected static int GetRot(int data)
        {
            return (data & 0b011);
        }

        protected int GetTileOffset(RenderData.RenderIterator it, int data)
        {
            return 0;
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            int tile = (data & 0b011);
            blue.Get(tile).Render(r, x, y);
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int r = 0;

            for (int i = 0; i < DIR.ORTHO.Size; i++)
            {
                DIR d = DIR.ORTHO.Get(i);

                int dx = rx + d.X();
                int dy = ry + d.Y();

                if ((dy < 0 || dy >= item.Height()) || (dx < 0 || dx >= item.Width()) && JoinsWith(null, true, item.Rotation, d, rx, ry, item))
                {
                    r = i;
                    break;
                }
                else if (JoinsWith(item.Sprite(dx, dy), false, item.Rotation, d, rx, ry, item))
                {
                    r = i;
                    break;
                }
            }

            return (byte)r;
        }

        protected bool JoinsWith(RoomSprite s, bool outof, int dir, DIR test, int rx, int ry, FurnisherItem item)
        {
            return test == DIR.ORTHO.Get(dir);
        }

        //public static class Random : RoomSprite.Imp
        //{
        //    private readonly RoomSpriteRot[] vars;
        //    private readonly RoomSpriteRot candle = new RoomSpriteRot(SINGLETYPE.TABLE_WOOD);

        //    public Random(params RoomSpriteRot[] vars)
        //    {
        //        this.vars = vars;
        //    }

        //    public Random(params SINGLETYPE[] sprites)
        //    {
        //        vars = new RoomSpriteRot[sprites.Length];
        //        for (int i = 0; i < vars.Length; i++)
        //        {
        //            vars[i] = new RoomSpriteRot(sprites[i])
        //            {
        //                protected override bool JoinsWith(RoomSprite s, bool outof, int dir, DIR test, int rx, int ry, FurnisherItem item)
        //                {
        //                    return Random.this.JoinsWith(s, outof);
        //                }
        //            };
        //        }
        //    }

        //    protected bool JoinsWith(RoomSprite sprite, bool outof)
        //    {
        //        return false;
        //    }

        //    public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        //    {
        //        if (isCandle)
        //        {
        //            return candle.Render(r, s, data, it, degrade, isCandle);
        //        }
        //        else
        //        {
        //            int var = it.Ran() % vars.Length;
        //            return vars[var].Render(r, s, data, it, degrade, isCandle);
        //        }
        //    }

        //    public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, FurnisherItemTile item)
        //    {
        //        vars[0].RenderPlaceholder(r, x, y, data & 0x03, tx, ty, item);
        //    }

        //    public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        //    {
        //        int r = RND.RInt(vars.Length);
        //        return vars[r].GetData(tx, ty, rx, ry, item, itemRan);
        //    }
        //}
    }
}