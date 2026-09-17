using System;

namespace snake2d.util.sprite
{
    public interface TILE_SHEET
    {
        void render(SPRITE_RENDERER r, int tile, int x1, int y1, int x2, int y2);
        void renderTextured(TextureCoords texture, int tile, int x1, int y1);
        void renderTextured(TextureCoords texture, int tile, int x1, int x2, int scale);
        TextureCoords getTexture(int tile);
        int size();
        int tiles();

        default void renderC(SPRITE_RENDERER r, int tile, int cx, int cy)
        {
            render(r, tile, cx - size() / 2, cy - size() / 2);
        }

        default void render(SPRITE_RENDERER r, int tile, int x1, int y1)
        {
            render(r, tile, x1, x1 + size(), y1, y1 + size());
        }

        default SPRITE makeSprite(int tile)
        {
            return new SPRITE.SpriteFromSheet(this, tile);
        }

        default TILE_SHEET slice(int from, int to)
        {
            if (from == 0 && to == 1)
                return this;
            return new Slice(this, from, to);
        }

        static readonly TILE_SHEET DUMMY = new TILE_SHEET()
        {
            tiles = () => 16,
            size = () => 0,
            renderTextured = (texture, tile, x1, x2, scale) => { },
            renderTextured = (texture, tile, x1, y1) => { },
            render = (r, tile, x1, x2, y1, y2) => { },
            getTexture = (tile) => null
        };
    }
}