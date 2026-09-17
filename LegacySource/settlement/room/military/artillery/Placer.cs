using settlement.main;
using settlement.room.main;
using snake2d.util.sprite;
using view.tool;

namespace settlement.room.military.artillery
{
    public sealed class Placer : PlacableFixed
    {
        private readonly ROOM_ARTILLERY blue;
        private readonly PlacableFixed p;

        public Placer(ROOM_ARTILLERY blue, ROOMS r)
        {
            this.blue = blue;
            p = r.placement.placer.CreateItemPlacer(blue, 0);
        }

        public override int width()
        {
            return p.width();
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            p.place(tx, ty, rx, ry);
            SETT.ROOMS().construction.construct(tx, ty);
            ArtilleryInstance ins = blue.Get(tx, ty);
            if (ins != null)
            {
                ins.muster(true);
                ins.fireAtWill(true);
            }
        }

        public override CharSequence placable(int tx, int ty, int rx, int ry)
        {
            return p.placable(tx, ty, rx, ry);
        }

        public override int height()
        {
            return p.height();
        }

        public override SPRITE getIcon()
        {
            return p.getIcon();
        }

        public override CharSequence name()
        {
            return blue.info.name;
        }

        public override PLACABLE getUndo()
        {
            return null;
        }

        public override int rotations()
        {
            return p.rotations();
        }

        public override int sizes()
        {
            return p.sizes();
        }

        public override CharSequence placableWhole(int tx1, int ty1)
        {
            return p.placableWhole(tx1, ty1);
        }

        public override void rotSet(int rot)
        {
            p.rotSet(rot);
        }

        public override int rot()
        {
            return p.rot();
        }
    }
}