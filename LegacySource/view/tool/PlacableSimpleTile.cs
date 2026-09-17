using System;
using util.sprite;
using util.color;
using util.gui.misc;
using view.subview;

namespace view.tool
{
    public abstract class PlacableSimpleTile : PLACABLE
    {
        private readonly string name;
        private readonly string desc;
        private readonly PLACABLE undo;

        public PlacableSimpleTile(string name)
            : this(name, null, null)
        {
        }

        public PlacableSimpleTile(string name, string desc)
            : this(name, desc, null)
        {
        }

        public PlacableSimpleTile(string name, string desc, PLACABLE undo)
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

        public abstract string isPlacable(int tx, int ty);
        public abstract void place(int tx, int ty);

        public void renderPlaceHolder(SPRITE_RENDERER r, int tx, int ty, int cx, int cy, bool isPlacable)
        {
            if (!isPlacable)
                GCOLOR.MAP().OK.bind();
            else
                GCOLOR.MAP().BAD.bind();
            SPRITES.cons().BIG.dashedThick.get(0).renderC(r, cx, cy);
            COLOR.unbind();
        }

        public void renderOverlay(GameWindow window)
        {
        }

        public void renderExtra(SPRITE_RENDERER r)
        {
        }

        public void hoverInfo(int tx, int ty, GBox hoverBox)
        {
            // TODO Auto-generated method stub
        }
    }
}