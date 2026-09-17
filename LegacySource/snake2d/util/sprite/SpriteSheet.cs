using System;

namespace snake2d.util.sprite
{
    /**
     * A spritesheet
     * @author mail__000
     *
     */
    public abstract class SpriteSheet
    {
        protected int lastX1 = 0;
        protected int lastX2 = 0;
        protected int lastY1 = 0;
        protected int lastY2 = 0;
        protected int scale;

        /**
         * 
         * @param diffusePath the path to the png image
         */
        protected SpriteSheet(int scale)
        {
            this.scale = scale;
        }

        protected SpriteSheet()
        {
            this(1);
        }

        protected void setScale(int scale)
        {
            this.scale = scale;
        }

        /**
         * 
         * @param x1
         * @param width
         * @param y1
         * @param height
         * @return
         */
        protected SPRITE getSprite(int x1, int width, int y1, int height)
        {
            int tx1 = x1;
            int tx2 = tx1 + width;
            int ty1 = y1;
            int ty2 = ty1 + height;

            lastX1 = x1;
            lastX2 = x1 + width;
            lastY1 = y1;
            lastY2 = y1 + height;
            return new SPRITE.SpriteImp(tx1, tx2, ty1, ty2, width * scale, height * scale);
        }

        /**
         * 
         * @param x1
         * @param width
         * @param y1
         * @param height
         * @param size
         * @return
         */
        protected SPRITE[] getVerticalSpriteArray(int x1, int width, int y1, int height, int size)
        {
            SPRITE[] res = new SPRITE[size];

            for (int i = 0; i < size; i++)
            {
                res[i] = getSprite(x1, width, y1 + height * i, height);
            }

            return res;
        }

        /**
         * 
         * @param x1
         * @param width
         * @param y1
         * @param height
         * @param size
         * @return
         */
        protected SPRITE[] getHorizontalSpriteArray(int x1, int width, int y1, int height, int size)
        {
            SPRITE[] res = new SPRITE[size];

            for (int i = 0; i < size; i++)
            {
                res[i] = getSprite(x1 + width * i, width, y1, height);
            }

            return res;
        }

        protected BigSprite getBigSprite(int x1, int width, int y1, int height)
        {
            lastX1 = x1;
            lastY1 = y1;
            lastX2 = x1 + width;
            lastY2 = y1 + height;
            return new BigSprite(scale, width, height)
            {
                protected override int startX()
                {
                    return x1;
                }

                protected override int startY()
                {
                    return y1;
                }
            };
        }
    }
}