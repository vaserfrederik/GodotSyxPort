using System;
using snake2d;
using util.text;
using view.tool;

namespace view.sett.ui.room.copy
{
    final class First : PlacableMulti
    {
        private readonly Source source;

        public First(Source source) : base(Dic.¤¤Copy, "", SPRITES.icons().m.expand)
        {
            this.source = source;
        }

        public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return E;
            if (Jobs.get(tx, ty) == null && !SETT.ROOMS().copy.copier.canCopy(tx, ty))
                return E;
            return null;
        }

        public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            source.set(tx, ty, true);
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            return ROOMS().copy.copier.canCopy(fromX, fromY) && ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
        }

        public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area, PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
        {
            if (source.is(tx, ty))
                return;
            if (isPlacable)
                base.renderPlaceHolder(r, mask, x, y, tx, ty, area, type, isPlacable, areaIsPlacable);
            else
                SPRITES.cons().BIG.dashed_hollow.render(r, mask, x, y);
        }

        private readonly PLACABLE undo = new PlacableMulti(Dic.¤¤Undo, "", SPRITES.icons().m.cancel)
        {
            public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                source.set(tx, ty, false);
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                if (source.is(tx, ty))
                    return null;
                return E;
            }

            public override bool expandsTo(int fromX, int fromY, int toX, int toY)
            {
                return source.is(fromX, fromY) && ROOMS().copy.copier.canCopy(fromX, fromY) && ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
            }
        };

        public override PLACABLE getUndo()
        {
            return undo;
        }
    }
}