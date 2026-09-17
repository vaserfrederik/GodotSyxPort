using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using view.subview;

namespace view.tool
{
    public abstract class PlacableSimple : PLACABLE
    {
        private readonly string name;
        private readonly string desc;
        private readonly PLACABLE undo;

        public PlacableSimple(string name)
            : this(name, null, null)
        {
        }

        public PlacableSimple(string name, string desc)
            : this(name, desc, null)
        {
        }

        public PlacableSimple(string name, string desc, PLACABLE undo)
        {
            this.name = name;
            this.desc = desc;
            this.undo = undo;
        }

        public override SPRITE getIcon()
        {
            return SPRITES.icons().m.cancel;
        }

        public override string name()
        {
            return name;
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }

        public override void hoverDesc(GBox box)
        {
            box.title(name);
            box.text(desc);
        }

        public abstract string isPlacable(int x, int y);
        public abstract void place(int x, int y);

        public void renderPlaceHolder(SPRITE_RENDERER r, int cx, int cy, bool isPlacable)
        {
            if (!isPlacable)
                GCOLOR.MAP().OK.bind();
            else
                GCOLOR.MAP().BAD.bind();
            SPRITES.cons().BIG.dashedThick.get(0).renderC(r, cx, cy);
            COLOR.unbind();
        }

        public void renderOverlay(int x, int y, SPRITE_RENDERER r, float ds, GameWindow window)
        {
        }

        public void placeInfo(GBox hoverBox, int cx, int cy)
        {
        }

        public void renderAction(int cx, int cy)
        {
        }
    }
}