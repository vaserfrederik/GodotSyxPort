using System;

namespace snake2d.util.sprite
{
    public interface SPRITE : DIMENSION
    {
        /**
         * 
         * @param r
         * @param dt
         * @param quad
         * @return true if end of animation. Always false for sprites.
         */
        void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2);

        default void Render(SPRITE_RENDERER r, RECTANGLE rec)
        {
            Render(r, rec.x1(), rec.x2(), rec.y1(), rec.y2());
        }

        /**
         * 
         * @param r
         * @param dt
         * @param X1
         * @param Y1
         * @return true if end of animation. Always false for sprites.
         */
        default void Render(SPRITE_RENDERER r, int X1, int Y1)
        {
            Render(r, X1, X1 + Width(), Y1, Y1 + Height());
        }

        default void RenderC(SPRITE_RENDERER r, int cx, int cy)
        {
            Render(r, cx - Width() / 2, cx - Width() / 2 + Width(), cy - Height() / 2, cy - Height() / 2 + Height());
        }

        default void RenderCScaled(SPRITE_RENDERER r, int cx, int cy, int scale)
        {
            Render(r, cx - Width() * scale / 2, cx - Width() * scale / 2 + Width() * scale, cy - Height() * scale / 2, cy - Height() * scale / 2 + Height() * scale);
        }

        default void RenderC(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            int cx = X1 + (X2 - X1) / 2;
            int cy = Y1 + (Y2 - Y1) / 2;
            RenderC(r, cx, cy);
        }

        default void RenderCY(SPRITE_RENDERER r, int x1, int cy)
        {
            Render(r, x1, x1 + Width(), cy - Height() / 2, cy - Height() / 2 + Height());
        }

        default void RenderCX(SPRITE_RENDERER r, int cx, int y1)
        {
            Render(r, cx - Width() / 2, cx - Width() / 2 + Width(), y1, y1 + Height());
        }

        default void RenderCX(SPRITE_RENDERER r, int cx, int y1, int scale)
        {
            Render(r, cx - scale * Width() / 2, cx - scale * Width() / 2 + scale * Width(), y1, y1 + scale * Height());
        }

        default void RenderCXY2(SPRITE_RENDERER r, int cx, int y2)
        {
            Render(r, cx - Width() / 2, cx - Width() / 2 + Width(), y2 - Height(), y2);
        }

        default void RenderC(SPRITE_RENDERER r, RECTANGLE c)
        {
            RenderC(r, c.cX(), c.cY());
        }

        default void RenderC(SPRITE_RENDERER r, COORDINATE c)
        {
            RenderC(r, c.x(), c.y());
        }

        default void RenderTextured(TextureCoords other, int X1, int X2, int Y1, int Y2)
        {
            // TODO: Implement default method
        }

        default TextureCoords Texture()
        {
            return null;
        }

        default SPRITE Twin(SPRITE b, DIR align, int shadow)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Scaled(int w, int h)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Scaled(double scale)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Wrapped(int w, int h, DIR align)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Color(Color c)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Color(Color c, int a)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Color(int r, int g, int b)
        {
            // TODO: Implement default method
            return null;
        }

        default SPRITE Color(int r, int g, int b, int a)
        {
            // TODO: Implement default method
            return null;
        }
    }
}