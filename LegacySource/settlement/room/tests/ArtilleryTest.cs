using settlement.main;
using settlement.room.main;
using settlement.room.military.artillery;
using snake2d.util.sprite;
using view.tool;

namespace settlement.room.tests
{
    final class ArtilleryTest : PlacableFixed
    {
        private readonly PlacableFixed p;

        public ArtilleryTest(PlacableFixed p)
        {
            this.p = p;
        }

        public override int width()
        {
            return p.width();
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            p.place(tx, ty, rx, ry);
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r is ArtilleryInstance)
            {
                ((ArtilleryInstance)r).setEnemy();
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
            return "enemy artillery " + p.name();
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