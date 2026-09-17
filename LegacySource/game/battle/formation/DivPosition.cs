using System;

namespace Game.Battle.Formation
{
    public interface DivPosition
    {
        COORDINATE Tile(int i);
        COORDINATE Pixel(int i);
        int Px(int i);
        int Py(int i);
        int Tx(int i);
        int Ty(int i);
        int Deployed();
    }
}