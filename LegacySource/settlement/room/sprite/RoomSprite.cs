using System;
using settlement.room.main.furnisher;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.sprite;
using util.rendering;

namespace settlement.room.sprite
{
    public interface RoomSprite
    {
        bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade, bool isCandle);

        void RenderBroken(SPRITE_RENDERER r, ShadowBatch s, int x, int y, RenderData.RenderIterator it, FurnisherItem item)
        {
            for (int i = 0; i < item.Group().Blueprint.Resources(); i++)
            {
                int a = item.BrokenResourceAmount(i);
                if (a > 0)
                {
                    item.Group().Blueprint.Resource(i).RenderDebris(r, s, x, y, it.Ran() >> i, a);
                }
            }
        }

        void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            SPRITES.cons().BIG.dashed_hollow.Render(r, 0, x, y);
        }

        void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade)
        {
        }

        void RenderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade)
        {
        }

        int Rotation(int data, FurnisherItem item)
        {
            if (item.Group().Rotations() > 2)
                return item.Rotation + 1;
            return 0;
        }

        byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan);

        byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            return 0;
        }

        int SData();

        class Dummy : RoomSprite
        {
            public bool Render(SPRITE_RENDERER r, ShadowBatch shadowBatch, int data, RenderData.RenderIterator it, double degrade, bool isCandle)
            {
                return false;
            }

            public byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
            {
                // TODO Auto-generated method stub
                return 0;
            }

            public int SData()
            {
                return 0;
            }
        }

        public static readonly Dummy DUMMY = new Dummy();

        public abstract class Imp : RoomSprite
        {
            protected int shadowDist = 3, shadowHeight = 0;
            private int sData = 0;

            public Imp()
            {
            }

            public Imp SetShadow(int height, int heightOverGround)
            {
                this.shadowDist = height;
                this.shadowHeight = heightOverGround;
                return this;
            }

            public int SData()
            {
                return sData;
            }

            public Imp SDataSet(int s)
            {
                this.sData = s;
                return this;
            }

            protected int GetData2(RenderData.RenderIterator it)
            {
                return SETT.ROOMS().fData.spriteData2.Get(it.Tile());
            }

            public void RenderDegrade(TILE_SHEET sheet, SPRITE_RENDERER r, int tile, RenderData.RenderIterator it, double degrade)
            {
                if (degrade > 0.05)
                {
                    OPACITY.O99.Bind();
                    sheet.RenderTextured(SETT.ROOMS().util.filth.Texture(degrade, it.Ran()), tile, it.X(), it.Y());
                    OPACITY.Unbind();
                }
            }
        }
    }
}