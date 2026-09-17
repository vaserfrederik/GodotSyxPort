using init.sprite;
using snake2d;
using util.gui.misc;

namespace view.tool
{
    public abstract class PlacableSingle : PLACABLE
    {
        private readonly ICharSequence name;
        private readonly ICharSequence desc;
        private readonly PLACABLE undo;
        PLACER_TYPE previous;

        public PlacableSingle(ICharSequence name)
        {
            this(name, null, null);
        }

        public PlacableSingle(ICharSequence name, ICharSequence desc)
        {
            this(name, desc, null);
        }

        public PlacableSingle(ICharSequence name, ICharSequence desc, PLACABLE undo)
        {
            this.name = name;
            this.desc = desc;
            this.undo = undo;
        }

        public override SPRITE getIcon()
        {
            return SPRITES.icons().m.cancel;
        }

        public override ICharSequence name()
        {
            return name;
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }

        public override void hoverDesc(GBox box)
        {
            if (name != null)
                box.title(name);
            if (desc != null)
                box.text(desc);
        }

        public bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            return false;
        }

        public abstract ICharSequence isPlacable(int tx, int ty);
        public abstract void placeFirst(int tx, int ty);
        public virtual void placeExpanded(int tx, int ty)
        {
        }

        public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, bool isPlacable)
        {
            SPRITES.cons().BIG.dashedThick.render(r, mask, x, y);
        }

        public void placeInfo(GBox b, int tiles)
        {
        }

        protected virtual void init(int tx, int ty)
        {
        }
    }
}