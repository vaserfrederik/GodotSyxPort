using init.sprite;
using snake2d.util.sprite;
using util.gui.misc;

namespace view.tool
{
    public abstract class PlacableFixedImp : PlacableFixed
    {
        private readonly string name;
        private readonly string desc;
        private readonly SPRITE icon;
        private readonly PLACABLE undo;
        private readonly int rots;
        private readonly int sizes;

        public PlacableFixedImp(string name, int rots, int sizes)
            : this(name, rots, sizes, null, null, null)
        {
        }

        public PlacableFixedImp(string name, int rots, int sizes, string desc, SPRITE icon)
            : this(name, rots, sizes, desc, icon, null)
        {
        }

        public PlacableFixedImp(string name, int rots, int sizes, string desc, SPRITE icon, PLACABLE undo)
        {
            this.name = name;
            this.desc = desc;
            if (icon == null)
                icon = SPRITES.icons().m.cancel;
            this.icon = icon;
            this.undo = undo;
            this.rots = rots;
            this.sizes = sizes;
        }

        public override CharSequence placableWhole(int tx1, int ty1)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public override SPRITE getIcon()
        {
            return icon;
        }

        public override CharSequence name()
        {
            return name;
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }

        public override void hoverDesc(GBox box)
        {
            if (name() != null)
                box.title(name());
            if (desc != null)
                box.text(desc);
        }

        public override int rotations()
        {
            return rots;
        }

        public override int sizes()
        {
            return sizes;
        }
    }
}