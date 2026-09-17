using System;
using System.Text;

namespace Settlement.Room.Main.Throne
{
    using static Settlement.Main.SETT;
    using static Settlement.Main.SETT.TERRAIN;

    using Init.Sprite;
    using Settlement.Main;
    using Snake2D;
    using Util.Text;
    using View.Tool;

    class Placer : PlacableFixedImp
    {
        private static readonly CharSequence ¤¤name = "Move Throne";
        private static readonly CharSequence ¤¤desc = "Creates a new construction site that will become the new throne room when finished. The throne is the center of your city and subjects that don't have a clear path to it will function poorly.";

        private int px = -1, py = -1;

        static
        {
            D.ts(typeof(Placer));
        }

        Placer(THRONE t) : base(¤¤name, 4, 1, ¤¤desc, t.icon())
        {
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            if (rx == 0 && ry == 0 && px != tx && py != ty)
            {
                px = tx;
                py = ty;
                new InstanceConstruction(tx, ty, rot());
            }
        }

        public override CharSequence placable(int tx, int ty, int rx, int ry)
        {
            if (ROOMS().map.get(tx, ty) is InstanceConstruction)
                return null;

            if (ROOMS().map.is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;

            if (!TERRAIN().get(tx, ty).clearing().can() && !TERRAIN().get(tx, ty).roofIs())
                return PlacableMessages.¤¤TERRAIN_BLOCK;

            return null;
        }

        public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, bool isPlacable, bool areaIsPlacable)
        {
            SPRITES.cons().ICO.arrows.get(rot()).render(r, x, y);
        }

        public override int width()
        {
            return Sprite.width(rot());
        }

        public override int height()
        {
            return Sprite.height(rot());
        }

        public override PLACABLE getUndo()
        {
            return SETT.ROOMS().DELETE;
        }
    };
}