using System;

namespace Settlement.Entity.Animal
{
    public interface IAnimalRoomRuin
    {
        bool CanBeGraced(int tx, int ty);
        void Grace(int tx, int ty);
    }
}