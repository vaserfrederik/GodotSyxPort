using System;
using System.IO;

namespace game.battle.formation
{
    public class DivPositionImp : DivPosition, SAVABLE
    {
        private int deployed = 0;
        private readonly int half;
        private readonly int[] coos;
        private readonly Coo coo = new Coo();

        public DivPositionImp(int maxMen)
        {
            coos = new int[maxMen * 2];
            half = maxMen;
        }

        public COORDINATE tile(int i)
        {
            if (i >= deployed)
                return null;
            coo.set(coos[i] >> C.T_SCROLL, coos[i + half] >> C.T_SCROLL);
            return coo;
        }

        public COORDINATE pixel(int i)
        {
            if (i >= deployed)
                return null;
            coo.set(coos[i], coos[i + half]);
            return coo;
        }

        public int px(int i)
        {
            return coos[i];
        }

        public int py(int i)
        {
            return coos[i + half];
        }

        public int tx(int i)
        {
            return coos[i] >> C.T_SCROLL;
        }

        public int ty(int i)
        {
            return coos[i + half] >> C.T_SCROLL;
        }

        public void init(int men)
        {
            if (men != this.deployed)
            {
                this.deployed = men;
            }
        }

        public void set(int i, int x, int y)
        {
            coos[i] = x;
            coos[i + half] = y;
        }

        public void save(FilePutter file)
        {
            file.i(deployed);
            file.is(coos);
        }

        public void load(FileGetter file)
        {
            deployed = file.i();
            file.is(coos);
        }

        public void clear()
        {
            deployed = 0;
        }

        public int deployed()
        {
            return deployed;
        }

        public void copyposition(DivPosition pos)
        {
            this.deployed = pos.deployed();
            for (int i = 0; i < deployed; i++)
            {
                coos[i] = pos.px(i);
                coos[i + half] = pos.py(i);
            }
        }
    }
}