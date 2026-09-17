using System;
using System.IO;
using init.constant;
using snake2d.util.file;

namespace game.battle.thread.position
{
    public sealed class DivCentre : SAVABLE
    {
        int cx, cy;
        int cxSoft, cySoft;
        int squareCX, squareCY;
        short inPosition;

        //dir faceEnemyDirection (the best way to face the enemy
        // faceEnemyWidth

        public DivCentre()
        {
        }

        public override void save(FilePutter file)
        {
            file.s(inPosition);
            file.i(cx).i(cy);
            file.i(cxSoft).i(cySoft);
        }

        public override void load(FileGetter file)
        {
            inPosition = file.s();
            cx = file.i();
            cy = file.i();
            cxSoft = file.i();
            cySoft = file.i();
            squareCX = file.i();
            squareCY = file.i();
        }

        public override void clear()
        {
            cx = -1;
            cy = -1;
            cxSoft = -1;
            cySoft = -1;
            inPosition = 0;
            squareCX = -1;
            squareCY = -1;
        }

        public int cUnitX()
        {
            return cx;
        }

        public int cUnitY()
        {
            return cy;
        }

        /**
         * 
         * @return current positions centre pixel. -1 if invalid
         */
        public int cX()
        {
            return cxSoft;
        }

        /**
         * 
         * @return current positions centre pixel. -1 if invalid
         */
        public int cY()
        {
            return cySoft;
        }

        public int inFormation()
        {
            return inPosition;
        }

        public int squareCX()
        {
            return squareCX;
        }

        public int squareCY()
        {
            return squareCY;
        }

        public int ctX()
        {
            return cxSoft / C.TILE_SIZE;
        }

        /**
         * 
         * @return current positions centre pixel. -1 if invalid
         */
        public int ctY()
        {
            return cySoft / C.TILE_SIZE;
        }
    }
}