using System;

namespace Snake2D
{
    public interface SpriteRenderer
    {
        /**
         * @param x1
         * @param x2
         * @param y1
         * @param y2
         * @param tx1
         * @param tx2
         * @param ty1
         * @param ty2
         */
        void RenderSprite(int x1, int x2, int y1, int y2, TextureCoords texture);

        public static readonly SpriteRenderer Dummy = new DummySpriteRenderer();

        private class DummySpriteRenderer : SpriteRenderer
        {
            public void RenderSprite(int x1, int x2, int y1, int y2, TextureCoords texture)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}